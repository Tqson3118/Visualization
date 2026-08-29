<script setup lang="ts">
// RegisterView — Màn 02 đăng ký: Wizard 3 bước (B0, A2, A3).
// Bước 1: Nhập thông tin (Học viên / Giảng viên - Khoa tùy chọn, Mã GV bắt buộc).
// Bước 2: Nhập mã OTP 6 chữ số gửi qua email (đếm ngược 5 phút, cooldown 60s, dev mode hint).
// Bước 3: Hoàn tất (Sinh viên tự động đăng nhập & chuyển sang courses, Giảng viên chờ duyệt).
import { computed, onBeforeUnmount, reactive, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { Motion } from 'motion-v';
import {
  ArrowLeft,
  ArrowRight,
  BadgeCheck,
  Building2,
  Check,
  CheckCircle2,
  Circle,
  Clock,
  KeyRound,
  Link as LinkIcon,
  Lock,
  Mail,
  RefreshCw,
  ShieldCheck,
  Sparkles,
  Target,
  User,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import * as authApi from '@/api/auth';
import { ApiError } from '@/api/client';
import { messages } from '@/i18n/vi';
import { isValidEmail, isValidUrl, validatePassword } from '@/utils/validators';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

type RegisterRole = 'student' | 'teacher';

const TEACHER_BIO_MAX = 500;
const isDevMode = import.meta.env.DEV;

const auth = useAuthStore();
const router = useRouter();

// ── Step management (1: Info, 2: OTP verify, 3: Completed) ──
const currentStep = ref<1 | 2 | 3>(1);

const form = reactive({
  displayName: '',
  email: '',
  password: '',
  confirmPassword: '',
  department: '',
  staffCode: '',
  academicDegree: '',
  profileLink: '',
  teacherBio: '',
  agreePolicy: false,
});

const role = ref<RegisterRole>('student');

const touched = reactive({
  displayName: false,
  email: false,
  password: false,
  confirmPassword: false,
  department: false,
  staffCode: false,
  academicDegree: false,
  profileLink: false,
  teacherBio: false,
});
const fieldErrors = reactive<Record<string, string>>({});
const submitError = ref('');
const submitting = ref(false);
const registeredTeacher = ref(false);

// ── OTP State (Step 2) ──
const otpDigits = reactive<string[]>(['', '', '', '', '', '']);
const otpCode = computed(() => otpDigits.join(''));
const otpError = ref('');
const otpExpiresSeconds = ref(300);
const resendCooldownSeconds = ref(0);
const resendingOtp = ref(false);

let otpTimerInterval: number | null = null;
let resendTimerInterval: number | null = null;

const otpTimeFormatted = computed(() => {
  const m = Math.floor(otpExpiresSeconds.value / 60);
  const s = otpExpiresSeconds.value % 60;
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
});

function startOtpCountdown(seconds = 300): void {
  if (otpTimerInterval) clearInterval(otpTimerInterval);
  otpExpiresSeconds.value = seconds;
  otpTimerInterval = window.setInterval(() => {
    if (otpExpiresSeconds.value > 0) {
      otpExpiresSeconds.value--;
    } else {
      if (otpTimerInterval) clearInterval(otpTimerInterval);
    }
  }, 1000);
}

function startResendCooldown(seconds = 60): void {
  if (resendTimerInterval) clearInterval(resendTimerInterval);
  resendCooldownSeconds.value = seconds;
  resendTimerInterval = window.setInterval(() => {
    if (resendCooldownSeconds.value > 0) {
      resendCooldownSeconds.value--;
    } else {
      if (resendTimerInterval) clearInterval(resendTimerInterval);
    }
  }, 1000);
}

onBeforeUnmount(() => {
  if (otpTimerInterval) clearInterval(otpTimerInterval);
  if (resendTimerInterval) clearInterval(resendTimerInterval);
});

function onOtpInput(index: number, event: Event): void {
  const target = event.target as HTMLInputElement;
  const val = target.value.replace(/\D/g, '');

  if (!val) {
    otpDigits[index] = '';
    return;
  }

  // Handle paste or multi-char input
  if (val.length > 1) {
    const chars = val.slice(0, 6).split('');
    chars.forEach((c, idx) => {
      if (index + idx < 6) {
        otpDigits[index + idx] = c;
      }
    });
    const nextIdx = Math.min(index + chars.length, 5);
    focusOtpInput(nextIdx);
    return;
  }

  otpDigits[index] = val;
  if (index < 5) {
    focusOtpInput(index + 1);
  }
}

function onOtpKeyDown(index: number, event: KeyboardEvent): void {
  if (event.key === 'Backspace') {
    if (!otpDigits[index] && index > 0) {
      otpDigits[index - 1] = '';
      focusOtpInput(index - 1);
    } else {
      otpDigits[index] = '';
    }
  } else if (event.key === 'ArrowLeft' && index > 0) {
    focusOtpInput(index - 1);
  } else if (event.key === 'ArrowRight' && index < 5) {
    focusOtpInput(index + 1);
  }
}

function onOtpPaste(event: ClipboardEvent): void {
  event.preventDefault();
  const pasted = event.clipboardData?.getData('text') ?? '';
  const digitsOnly = pasted.replace(/\D/g, '').slice(0, 6);
  if (digitsOnly) {
    digitsOnly.split('').forEach((char, idx) => {
      if (idx < 6) otpDigits[idx] = char;
    });
    const nextIdx = Math.min(digitsOnly.length, 5);
    focusOtpInput(nextIdx);
  }
}

function focusOtpInput(index: number): void {
  const el = document.getElementById(`otp-digit-${index}`) as HTMLInputElement | null;
  el?.focus();
  el?.select();
}

// ── Validation Rules ──
const passwordRules = computed(() => [
  { key: 'length', ok: form.password.length >= 8 && form.password.length <= 64, label: messages.register.checklist[0] },
  { key: 'upper', ok: /[A-Z]/.test(form.password), label: messages.register.checklist[1] },
  { key: 'lower', ok: /[a-z]/.test(form.password), label: messages.register.checklist[2] },
  { key: 'digit', ok: /\d/.test(form.password), label: messages.register.checklist[3] },
  { key: 'special', ok: /[^A-Za-z0-9]/.test(form.password), label: messages.register.checklist[4] },
]);

const roleOptions = computed<{ value: RegisterRole; label: string }[]>(() => [
  { value: 'student', label: messages.auth.roleStudent },
  { value: 'teacher', label: messages.auth.roleTeacher },
]);

function selectRole(next: RegisterRole): void {
  role.value = next;
  validate();
}

function validate(): boolean {
  const errors: Record<string, string> = {};
  if (!form.displayName.trim() || form.displayName.trim().length < 2) {
    errors.displayName = messages.register.displayNameError;
  }
  if (!isValidEmail(form.email)) errors.email = messages.auth.invalidEmail;
  const pwd = validatePassword(form.password);
  if (!pwd.ok) errors.password = messages.auth.passwordRequirement;
  if (form.confirmPassword !== form.password) errors.confirmPassword = messages.register.confirmError;

  if (role.value === 'teacher') {
    // A2: Khoa/Bộ môn là TÙY CHỌN (bỏ bắt buộc)
    if (form.department.trim().length > 100) {
      errors.department = 'Khoa/bộ môn không được vượt quá 100 ký tự';
    }
    // A3: Mã giảng viên là BẮT BUỘC
    if (!form.staffCode.trim()) {
      errors.staffCode = messages.auth.staffCodeRequired;
    } else if (form.staffCode.trim().length > 50) {
      errors.staffCode = 'Mã giảng viên không được vượt quá 50 ký tự';
    }

    if (form.profileLink.trim() && !isValidUrl(form.profileLink.trim())) {
      errors.profileLink = messages.auth.profileLinkInvalid;
    }
    if (form.teacherBio.length > TEACHER_BIO_MAX) {
      errors.teacherBio = messages.auth.teacherBioMax;
    }
  }

  if (!form.agreePolicy) errors.agreePolicy = messages.register.agreePolicyError;

  // Xóa key cũ không còn trong errors
  for (const key of Object.keys(fieldErrors)) {
    if (!(key in errors)) delete fieldErrors[key];
  }
  Object.assign(fieldErrors, errors);
  return Object.keys(errors).length === 0;
}

function onBlur(field: keyof typeof touched): void {
  touched[field] = true;
  validate();
}

function markAllTouched(): void {
  for (const key of Object.keys(touched)) {
    touched[key as keyof typeof touched] = true;
  }
}

// ── Step 1 Submit: Validate & Send OTP ──
async function handleStep1Submit(): Promise<void> {
  submitError.value = '';
  markAllTouched();
  if (!validate()) return;

  submitting.value = true;
  try {
    const res = await authApi.sendRegisterOtp(form.email.trim().toLowerCase());
    startOtpCountdown(res.expiresInSeconds || 300);
    startResendCooldown(60);
    currentStep.value = 2;
    otpError.value = '';
    // Reset OTP digits
    otpDigits.splice(0, 6, '', '', '', '', '', '');
    setTimeout(() => focusOtpInput(0), 150);
  } catch (err) {
    if (err instanceof ApiError) {
      if (err.code === 'EMAIL_EXISTS' || err.status === 409) {
        touched.email = true;
        fieldErrors.email = err.message || 'Email này đã được sử dụng để đăng ký.';
      } else if (err.code === 'INVALID_EMAIL' || err.status === 400) {
        touched.email = true;
        fieldErrors.email = err.message || messages.auth.invalidEmail;
      } else if (err.code === 'DOMAIN_NOT_ALLOWED') {
        touched.email = true;
        fieldErrors.email = err.message || 'Tên miền email không được phép đăng ký.';
      } else {
        submitError.value = err.message;
      }
    } else {
      submitError.value = 'Không thể gửi mã OTP xác thực. Vui lòng thử lại.';
    }
  } finally {
    submitting.value = false;
  }
}

// ── Step 2 Resend OTP ──
async function handleResendOtp(): Promise<void> {
  if (resendCooldownSeconds.value > 0 || resendingOtp.value) return;

  resendingOtp.value = true;
  otpError.value = '';
  try {
    const res = await authApi.sendRegisterOtp(form.email.trim().toLowerCase());
    startOtpCountdown(res.expiresInSeconds || 300);
    startResendCooldown(60);
    otpDigits.splice(0, 6, '', '', '', '', '', '');
    setTimeout(() => focusOtpInput(0), 100);
  } catch (err) {
    if (err instanceof ApiError) {
      otpError.value = err.message;
    } else {
      otpError.value = 'Không thể gửi lại mã OTP. Vui lòng thử lại sau.';
    }
  } finally {
    resendingOtp.value = false;
  }
}

// ── Step 2 Verify OTP & Finish Registration ──
async function handleVerifyAndRegister(): Promise<void> {
  otpError.value = '';
  const code = otpCode.value.trim();
  if (code.length !== 6) {
    otpError.value = 'Vui lòng nhập đủ 6 chữ số mã OTP.';
    return;
  }

  submitting.value = true;
  try {
    // 1. Verify OTP to obtain otpToken
    const verifyRes = await authApi.verifyRegisterOtp(form.email.trim().toLowerCase(), code);
    const otpToken = verifyRes.otpToken;

    // 2. Complete registration
    const isTeacher = role.value === 'teacher';
    await auth.register({
      displayName: form.displayName.trim(),
      email: form.email.trim().toLowerCase(),
      password: form.password,
      isTeacher,
      otpToken,
      ...(isTeacher
        ? {
            department: form.department.trim() || undefined,
            staffCode: form.staffCode.trim(),
            ...(form.academicDegree.trim() ? { academicDegree: form.academicDegree.trim() } : {}),
            ...(form.profileLink.trim() ? { profileLink: form.profileLink.trim() } : {}),
            teacherBio: form.teacherBio.trim() || undefined,
          }
        : {}),
    });

    if (isTeacher) {
      registeredTeacher.value = true;
      currentStep.value = 3;
    } else {
      await router.replace({ name: 'courses' });
    }
  } catch (err) {
    if (err instanceof ApiError) {
      if (err.code === 'CONFLICT') {
        // A3: StaffCode conflict handling -> Quay lại Bước 1 và highlight lỗi
        currentStep.value = 1;
        touched.staffCode = true;
        fieldErrors.staffCode = err.message || 'Mã giảng viên đã được sử dụng — vui lòng kiểm tra lại.';
      } else if (['OTP_INVALID', 'OTP_EXPIRED', 'OTP_USED', 'ACCOUNT_LOCKED'].includes(err.code)) {
        otpError.value = err.message;
      } else {
        otpError.value = err.message;
      }
    } else {
      otpError.value = 'Đăng ký thất bại. Vui lòng thử lại.';
    }
  } finally {
    submitting.value = false;
  }
}

function backToStep1(): void {
  currentStep.value = 1;
  submitError.value = '';
}

const BRAND_POINTS = [
  { icon: Sparkles, text: messages.auth.brandPoint1 },
  { icon: Target, text: messages.auth.brandPoint2 },
  { icon: CheckCircle2, text: messages.auth.brandPoint3 },
] as const;

const BENCH_BLOCKS = [
  { value: '7', state: 'done' },
  { value: '3', state: 'swap' },
  { value: '8', state: 'active' },
  { value: '1', state: 'default' },
  { value: '9', state: 'default' },
  { value: '2', state: 'default' },
] as const;
</script>

<template>
  <main class="register">
    <Motion
      class="register__shell"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <!-- Brand panel: nền tối canvas-ink + block-token -->
      <aside class="register__aside" aria-label="Giới thiệu DSA Visual">
        <div class="register__aside-inner">
          <span class="register__aside-badge">{{ messages.app.name }}</span>
          <h2 class="register__aside-title">{{ messages.app.tagline }}</h2>

          <div class="register__aside-bench" aria-hidden="true">
            <div
              v-for="(b, idx) in BENCH_BLOCKS"
              :key="b.value"
              class="register__aside-block"
              :class="`register__aside-block--${b.state}`"
            >
              <span class="register__aside-block-value">{{ b.value }}</span>
              <span class="register__aside-block-index">{{ String(idx).padStart(2, '0') }}</span>
            </div>
          </div>

          <ul class="register__points">
            <li v-for="point in BRAND_POINTS" :key="point.text" class="register__point">
              <component :is="point.icon" :size="16" class="register__point-icon" aria-hidden="true" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Wizard Flow Container -->
      <div class="register__form-col">
        <div class="register__card">
          <!-- Wizard Step Indicator -->
          <div class="register__steps" aria-label="Các bước đăng ký">
            <div class="register__step-item" :class="{ 'register__step-item--active': currentStep === 1, 'register__step-item--done': currentStep > 1 }">
              <span class="register__step-num">1</span>
              <span class="register__step-text">Thông tin</span>
            </div>
            <div class="register__step-divider" :class="{ 'register__step-divider--active': currentStep > 1 }"></div>
            <div class="register__step-item" :class="{ 'register__step-item--active': currentStep === 2, 'register__step-item--done': currentStep > 2 }">
              <span class="register__step-num">2</span>
              <span class="register__step-text">Xác thực OTP</span>
            </div>
            <div class="register__step-divider" :class="{ 'register__step-divider--active': currentStep === 3 }"></div>
            <div class="register__step-item" :class="{ 'register__step-item--active': currentStep === 3 }">
              <span class="register__step-num">3</span>
              <span class="register__step-text">Hoàn tất</span>
            </div>
          </div>

          <!-- ══════ BƯỚC 1: NHẬP THÔNG TIN ══════ -->
          <form v-if="currentStep === 1" class="space-y-3.5" novalidate @submit.prevent="handleStep1Submit">
            <div>
              <h1 class="register__title">{{ messages.auth.registerTitle }}</h1>
              <p class="register__subtitle">{{ messages.register.subtitle }}</p>
            </div>

            <Input
              v-model="form.displayName"
              label="Họ tên"
              :icon="User"
              :error="touched.displayName ? fieldErrors.displayName : ''"
              :placeholder="messages.register.displayNamePlaceholder"
              autocomplete="name"
              required
              @blur="onBlur('displayName')"
            />

            <Input
              v-model="form.email"
              label="Email"
              type="email"
              :icon="Mail"
              :error="touched.email ? fieldErrors.email : ''"
              :placeholder="messages.register.emailPlaceholder"
              autocomplete="email"
              required
              @blur="onBlur('email')"
            />

            <Input
              v-model="form.password"
              label="Mật khẩu"
              type="password"
              :icon="Lock"
              :error="touched.password ? fieldErrors.password : ''"
              :placeholder="messages.register.passwordPlaceholder"
              autocomplete="new-password"
              required
              @blur="onBlur('password')"
            />

            <div class="register__checklist" role="list" :aria-label="'Yêu cầu mật khẩu'">
              <span
                v-for="rule in passwordRules"
                :key="rule.key"
                class="register__check"
                :class="{ 'register__check--ok': rule.ok }"
              >
                <Check v-if="rule.ok" :size="14" class="register__check-mark" aria-hidden="true" />
                <Circle v-else :size="14" class="register__check-mark" aria-hidden="true" />
                {{ rule.label }}
              </span>
            </div>

            <Input
              v-model="form.confirmPassword"
              label="Xác nhận mật khẩu"
              type="password"
              :icon="Lock"
              :error="touched.confirmPassword ? fieldErrors.confirmPassword : ''"
              :placeholder="messages.register.confirmPlaceholder"
              autocomplete="new-password"
              required
              @blur="onBlur('confirmPassword')"
            />

            <fieldset class="register__role">
              <legend class="register__role-label">{{ messages.auth.roleLabel }}</legend>
              <div class="register__role-group">
                <Button
                  v-for="opt in roleOptions"
                  :key="opt.value"
                  type="button"
                  variant="ghost"
                  size="sm"
                  class="register__role-option"
                  :class="{ 'register__role-option--active': role === opt.value }"
                  :aria-pressed="role === opt.value"
                  @click="selectRole(opt.value)"
                >
                  {{ opt.label }}
                </Button>
              </div>
            </fieldset>

            <!-- Form con giảng viên -->
            <div v-if="role === 'teacher'" class="register__teacher">
              <!-- A2: Khoa/Bộ môn là TÙY CHỌN -->
              <div class="register__field">
                <div class="flex items-center justify-between">
                  <label class="register__field-label">{{ messages.auth.department }}</label>
                  <span class="text-[11px] text-vdsa-muted bg-vdsa-surface px-1.5 py-0.5 rounded border border-vdsa-border/60">Tùy chọn</span>
                </div>
                <Input
                  v-model="form.department"
                  :icon="Building2"
                  :error="touched.department ? fieldErrors.department : ''"
                  :placeholder="messages.auth.departmentPlaceholder"
                  autocomplete="organization"
                  :maxlength="100"
                  @blur="onBlur('department')"
                />
              </div>

              <!-- A3: Mã giảng viên là BẮT BUỘC + DUY NHẤT -->
              <div class="register__field">
                <label class="register__field-label">
                  {{ messages.auth.staffCode }} <span class="text-rose-500">*</span>
                </label>
                <Input
                  v-model="form.staffCode"
                  :icon="BadgeCheck"
                  :error="touched.staffCode ? fieldErrors.staffCode : ''"
                  :placeholder="messages.auth.staffCodePlaceholder"
                  autocomplete="off"
                  :maxlength="50"
                  required
                  @blur="onBlur('staffCode')"
                />
                <p v-if="fieldErrors.staffCode" class="text-xs text-rose-500 mt-1" role="alert">
                  {{ fieldErrors.staffCode }}
                </p>
              </div>

              <div class="register__field">
                <div class="flex items-center justify-between">
                  <label class="register__field-label" for="register-academic-degree">{{ messages.auth.academicDegree }}</label>
                  <span class="text-[11px] text-vdsa-muted bg-vdsa-surface px-1.5 py-0.5 rounded border border-vdsa-border/60">Tùy chọn</span>
                </div>
                <select
                  id="register-academic-degree"
                  v-model="form.academicDegree"
                  class="register__select"
                  @blur="onBlur('academicDegree')"
                >
                  <option value="">{{ messages.auth.academicDegreePlaceholder }}</option>
                  <option v-for="opt in messages.auth.academicDegreeOptions" :key="opt" :value="opt">
                    {{ opt }}
                  </option>
                </select>
              </div>

              <div class="register__field">
                <div class="flex items-center justify-between">
                  <label class="register__field-label">{{ messages.auth.profileLink }}</label>
                  <span class="text-[11px] text-vdsa-muted bg-vdsa-surface px-1.5 py-0.5 rounded border border-vdsa-border/60">Tùy chọn</span>
                </div>
                <Input
                  v-model="form.profileLink"
                  type="url"
                  :icon="LinkIcon"
                  :error="touched.profileLink ? fieldErrors.profileLink : ''"
                  :placeholder="messages.auth.profileLinkPlaceholder"
                  autocomplete="off"
                  :maxlength="2048"
                  @blur="onBlur('profileLink')"
                />
              </div>

              <div class="register__field">
                <div class="flex items-center justify-between">
                  <label class="register__field-label" for="register-teacher-bio">{{ messages.auth.teacherBio }}</label>
                  <span class="text-[11px] text-vdsa-muted bg-vdsa-surface px-1.5 py-0.5 rounded border border-vdsa-border/60">Tùy chọn</span>
                </div>
                <textarea
                  id="register-teacher-bio"
                  v-model="form.teacherBio"
                  class="register__bio"
                  :placeholder="messages.auth.teacherBioPlaceholder"
                  :maxlength="TEACHER_BIO_MAX"
                  :aria-invalid="Boolean(touched.teacherBio && fieldErrors.teacherBio)"
                  @blur="onBlur('teacherBio')"
                ></textarea>
                <div class="register__bio-meta">
                  <p
                    v-if="touched.teacherBio && fieldErrors.teacherBio"
                    class="register__error"
                    role="alert"
                  >
                    {{ fieldErrors.teacherBio }}
                  </p>
                  <span class="register__bio-count">{{ form.teacherBio.length }}/{{ TEACHER_BIO_MAX }}</span>
                </div>
              </div>

              <p class="register__note">{{ messages.auth.teacherPendingNote }}</p>
            </div>

            <label class="register__row">
              <input v-model="form.agreePolicy" type="checkbox" />
              <span>
                {{ messages.register.agreePolicy }}
                <RouterLink :to="{ name: 'privacy' }">chính sách bảo mật</RouterLink>
              </span>
            </label>
            <p v-if="fieldErrors.agreePolicy" class="register__error" role="alert">
              {{ fieldErrors.agreePolicy }}
            </p>

            <p v-if="submitError" class="register__error register__error--boxed" role="alert">
              {{ submitError }}
            </p>

            <Button type="submit" size="lg" class="register__submit" :loading="submitting" block>
              Tiếp tục <ArrowRight :size="16" class="ml-1" />
            </Button>

            <p class="register__switch">
              {{ messages.register.hasAccount }}
              <RouterLink :to="{ name: 'login' }">{{ messages.register.toLogin }}</RouterLink>
            </p>
          </form>

          <!-- ══════ BƯỚC 2: NHẬP MÃ OTP EMAIL ══════ -->
          <form v-else-if="currentStep === 2" class="space-y-5" novalidate @submit.prevent="handleVerifyAndRegister">
            <div>
              <h1 class="register__title">Xác thực Email</h1>
              <p class="register__subtitle">
                Mã xác thực gồm 6 chữ số đã được gửi tới địa chỉ <strong class="text-white">{{ form.email }}</strong>.
              </p>
            </div>

            <!-- Gợi ý Dev Mode chỉ hiện khi chạy local dev -->
            <div v-if="isDevMode" class="p-3 rounded-xl bg-indigo-500/10 border border-indigo-500/30 text-xs text-indigo-300 flex items-center gap-2">
              <KeyRound :size="15" class="text-indigo-400 shrink-0" />
              <span><strong>Dev mode:</strong> mã OTP mặc định <code class="bg-indigo-950/80 px-1.5 py-0.5 rounded text-white font-mono font-bold">123456</code></span>
            </div>

            <!-- 6 ô nhập mã OTP -->
            <div class="space-y-2">
              <label class="block text-xs font-bold text-vdsa-secondary uppercase text-center">Nhập mã OTP 6 chữ số</label>
              <div class="register__otp-boxes" @paste="onOtpPaste">
                <input
                  v-for="(_, index) in otpDigits"
                  :id="`otp-digit-${index}`"
                  :key="index"
                  v-model="otpDigits[index]"
                  type="text"
                  inputmode="numeric"
                  maxlength="1"
                  autocomplete="one-time-code"
                  class="register__otp-input"
                  :class="{ 'register__otp-input--error': otpError }"
                  @input="onOtpInput(index, $event)"
                  @keydown="onOtpKeyDown(index, $event)"
                />
              </div>
            </div>

            <!-- Đếm ngược thời gian & Gửi lại mã -->
            <div class="flex items-center justify-between text-xs px-1">
              <div class="flex items-center gap-1.5 text-vdsa-muted">
                <Clock :size="14" :class="otpExpiresSeconds < 60 ? 'text-rose-400' : 'text-vdsa-muted'" />
                <span>Hiệu lực: <strong :class="otpExpiresSeconds < 60 ? 'text-rose-400' : 'text-white'" class="font-mono">{{ otpTimeFormatted }}</strong></span>
              </div>

              <button
                type="button"
                class="text-vdsa-accent hover:underline font-semibold disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1"
                :disabled="resendCooldownSeconds > 0 || resendingOtp"
                @click="handleResendOtp"
              >
                <RefreshCw v-if="resendingOtp" :size="12" class="animate-spin" />
                <span v-if="resendCooldownSeconds > 0">Gửi lại mã ({{ resendCooldownSeconds }}s)</span>
                <span v-else>Gửi lại mã OTP</span>
              </button>
            </div>

            <p v-if="otpError" class="register__error register__error--boxed text-center" role="alert">
              {{ otpError }}
            </p>

            <div class="space-y-2.5 pt-2">
              <Button
                type="submit"
                size="lg"
                variant="primary"
                block
                :loading="submitting"
                :disabled="otpCode.length !== 6"
              >
                Xác nhận & Hoàn tất
              </Button>

              <Button
                type="button"
                size="sm"
                variant="ghost"
                block
                :disabled="submitting"
                @click="backToStep1"
              >
                <ArrowLeft :size="15" class="mr-1" /> Quay lại sửa thông tin
              </Button>
            </div>
          </form>

          <!-- ══════ BƯỚC 3: HOÀN TẤT (GIẢNG VIÊN CHỜ DUYỆT) ══════ -->
          <div v-else-if="currentStep === 3 || registeredTeacher" class="register__pending" role="status">
            <ShieldCheck :size="32" class="register__pending-icon" aria-hidden="true" />
            <div class="register__pending-body space-y-2">
              <p class="register__pending-title">{{ messages.auth.teacherPendingSuccess }}</p>
              <p class="register__pending-desc">{{ messages.auth.teacherPendingNote }}</p>
              <div class="pt-2">
                <RouterLink class="register__pending-link inline-flex items-center gap-1 text-sm font-semibold text-emerald-400 hover:underline" :to="{ name: 'login' }">
                  {{ messages.register.backToLogin }} <ArrowRight :size="14" />
                </RouterLink>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Motion>
  </main>
</template>

<style scoped>
.register {
  min-height: calc(100dvh - 120px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

/* Shell */
.register__shell {
  display: grid;
  grid-template-columns: 1.05fr 1fr;
  width: 100%;
  max-width: 960px;
  border-radius: var(--radius-lg);
  overflow: hidden;
  border: 1px solid var(--color-border);
  background: var(--color-card);
}

/* Brand panel */
.register__aside {
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
}

.register__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.register__aside-badge {
  display: inline-flex;
  width: fit-content;
  padding: var(--space-xs) var(--space-md);
  border-radius: var(--radius-md);
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.12);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-index-muted);
}

.register__aside-title {
  font-size: var(--text-xl);
  font-weight: 600;
  line-height: 1.35;
  letter-spacing: -0.015em;
  margin: 0;
  color: rgba(255, 255, 255, 0.92);
}

.register__aside-bench {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  margin-block: var(--space-xs);
}

.register__aside-block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  width: 36px;
  height: 44px;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
}

.register__aside-block--swap { background: var(--color-conflict); }
.register__aside-block--done { background: var(--color-resolved); }
.register__aside-block--active { box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.4); }

.register__aside-block-value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.4;
}

.register__aside-block-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

.register__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin: 0;
}

.register__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.85);
}

.register__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
  color: rgba(255, 255, 255, 0.6);
}

/* Form Container */
.register__form-col {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: clamp(1.5rem, 3vw, 2.25rem);
}

.register__card {
  width: 100%;
  max-width: 25rem;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* Wizard Step Progress Indicator */
.register__steps {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-bottom: var(--space-sm);
  border-bottom: 1px solid var(--color-border);
}

.register__step-item {
  display: flex;
  align-items: center;
  gap: 6px;
  opacity: 0.4;
  transition: opacity 200ms ease;
}

.register__step-item--active {
  opacity: 1;
}

.register__step-item--done {
  opacity: 0.8;
}

.register__step-num {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  background: var(--color-muted);
  border: 1px solid var(--color-border);
  font-size: 11px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-primary);
}

.register__step-item--active .register__step-num {
  background: var(--color-accent, #6366f1);
  border-color: var(--color-accent, #6366f1);
  color: #ffffff;
}

.register__step-item--done .register__step-num {
  background: var(--color-success, #10b981);
  border-color: var(--color-success, #10b981);
  color: #ffffff;
}

.register__step-text {
  font-size: 11px;
  font-weight: 600;
  color: var(--color-text-secondary);
}

.register__step-divider {
  flex: 1;
  height: 1px;
  background: var(--color-border);
  margin-inline: 8px;
}

.register__step-divider--active {
  background: var(--color-success, #10b981);
}

/* Typography */
.register__title {
  font-size: var(--text-3xl);
  font-weight: 600;
  line-height: 1.15;
  letter-spacing: -0.025em;
  margin: 0;
  margin-bottom: var(--space-xs);
}

.register__subtitle {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  margin-bottom: 0;
  line-height: 1.4;
}

/* Checklist */
.register__checklist {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--space-xs) var(--space-md);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  padding-inline: var(--space-xs);
}

.register__check {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
}

.register__check-mark {
  color: var(--color-text-quaternary);
  flex-shrink: 0;
}

.register__check--ok {
  color: var(--color-text-secondary);
}

.register__check--ok .register__check-mark {
  color: var(--color-success);
}

/* Segmented vai trò */
.register__role {
  border: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.register__role-label {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-secondary);
  margin-bottom: var(--space-xs);
}

.register__role-group {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-sm);
  padding: var(--space-xs);
  background: var(--color-muted);
  border-radius: var(--radius-md);
}

.register__role-option {
  color: var(--color-text-tertiary);
}

.register__role-option:hover {
  background: var(--color-surface-hover);
  color: var(--color-foreground);
}

.register__role-option--active {
  background: var(--color-card);
  border: 1px solid var(--color-border);
  color: var(--color-foreground);
}

/* Form con giảng viên */
.register__teacher {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-muted);
}

.register__field { display: flex; flex-direction: column; gap: var(--space-xs); }

.register__field-label {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-secondary);
}

.register__bio {
  min-height: 84px;
  resize: vertical;
  line-height: 1.5;
  width: 100%;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-family: inherit;
  background: var(--color-card);
}

.register__select {
  width: 100%;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-family: inherit;
  background: var(--color-card);
  color: var(--color-text-secondary);
}

.register__bio-meta {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-sm);
}

.register__bio-count {
  margin-left: auto;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.register__note { font-size: var(--text-xs); color: var(--color-text-tertiary); margin: 0; }

.register__row {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  cursor: pointer;
  padding-block: var(--space-xs);
}

.register__row input[type='checkbox'] {
  width: 1.125rem;
  height: 1.125rem;
  accent-color: var(--color-primary);
  cursor: pointer;
  flex-shrink: 0;
}

.register__row a {
  font-weight: 500;
}

.register__error {
  color: var(--color-destructive);
  font-size: var(--text-sm);
}

.register__error--boxed {
  margin-top: var(--space-xs);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-destructive) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-destructive) 30%, transparent);
}

.register__submit {
  margin-top: var(--space-md);
}

.register__switch {
  text-align: center;
  margin: 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

/* OTP 6-Box Styling */
.register__otp-boxes {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.register__otp-input {
  width: 44px;
  height: 52px;
  text-align: center;
  font-size: 22px;
  font-weight: 700;
  font-family: var(--font-mono, monospace);
  background: var(--color-surface, #161b22);
  border: 1.5px solid var(--color-border, #30363d);
  border-radius: 10px;
  color: #ffffff;
  transition: all 150ms ease;
}

.register__otp-input:focus {
  outline: none;
  border-color: var(--color-accent, #6366f1);
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.25);
  background: var(--color-card, #0d1117);
}

.register__otp-input--error {
  border-color: var(--color-destructive, #ef4444);
}

/* Pending teacher */
.register__pending {
  display: flex;
  align-items: flex-start;
  gap: var(--space-md);
  background: color-mix(in srgb, var(--color-success) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 35%, transparent);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
  font-size: var(--text-sm);
}

.register__pending-icon {
  color: var(--color-success);
  flex-shrink: 0;
  margin-top: 2px;
}

.register__pending-title {
  font-size: var(--text-base);
  font-weight: 700;
  margin-bottom: var(--space-xs);
  color: #ffffff;
}

.register__pending-desc {
  color: var(--color-text-secondary);
  line-height: 1.5;
}

@media (max-width: 820px) {
  .register__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .register__aside { padding: var(--space-lg); }
  .register__otp-input { width: 38px; height: 46px; font-size: 18px; }
}
</style>
