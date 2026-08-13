<script setup lang="ts">
// RegisterView — Màn 02 đăng ký: split layout đồng bộ LoginView (brand aside + form card),
// validation inline, checklist mật khẩu sống + segmented "Đăng ký với vai trò" + form con
// giảng viên + đồng ý chính sách.
// View-quality (nhóm A): aside tối canvas-ink (bỏ gradient/blob/glassmorphism), icon
// lucide-vue-next, Motion easing chuẩn cubic-bezier, segmented vai trò qua Button.vue
// (giữ selector e2e button.register__role-option), bỏ shadow shell/role-option.
// GIỮ nguyên logic validate/submit + selector e2e.
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { Motion } from 'motion-v';
import {
  BadgeCheck,
  Building2,
  Check,
  CheckCircle2,
  Circle,
  Lock,
  Mail,
  ShieldCheck,
  Sparkles,
  Target,
  User,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { ApiError } from '@/api/client';
import { messages } from '@/i18n/vi';
import { isValidEmail, validatePassword } from '@/utils/validators';
import BlockToken from '@/components/ui/BlockToken.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

type RegisterRole = 'student' | 'teacher';

const TEACHER_BIO_MAX = 500;

const auth = useAuthStore();
const router = useRouter();

const form = reactive({
  displayName: '',
  email: '',
  password: '',
  confirmPassword: '',
  department: '',
  staffCode: '',
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
  teacherBio: false,
});
const fieldErrors = reactive<Record<string, string>>({});
const submitError = ref('');
const submitting = ref(false);
const submitSuccess = ref(false);
const registeredTeacher = ref(false);

/** Block-token nổi trong aside — 1 block mới mỗi 3s, vị trí ngẫu nhiên (decorative, aria-hidden). */
interface FloatToken {
  id: number;
  top: string;
  left: string;
  value: string;
}
const floatTokens = ref<FloatToken[]>([]);
let tokenId = 0;
let tokenTimer: ReturnType<typeof setInterval> | null = null;
let tokenTimeouts: Array<ReturnType<typeof setTimeout>> = [];

const prefersReducedMotion = (): boolean =>
  typeof window !== 'undefined' &&
  typeof window.matchMedia === 'function' &&
  window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function addFloatToken(): void {
  const id = ++tokenId;
  floatTokens.value.push({
    id,
    top: `${8 + Math.random() * 64}%`,
    left: `${6 + Math.random() * 76}%`,
    value: String(1 + Math.floor(Math.random() * 9)),
  });
  tokenTimeouts.push(
    setTimeout(() => {
      floatTokens.value = floatTokens.value.filter((t) => t.id !== id);
    }, 9000),
  );
}

onMounted(() => {
  if (prefersReducedMotion()) return;
  addFloatToken();
  tokenTimer = setInterval(addFloatToken, 3000);
});

onBeforeUnmount(() => {
  if (tokenTimer) clearInterval(tokenTimer);
  for (const t of tokenTimeouts) clearTimeout(t);
  tokenTimeouts = [];
});

const passwordRules = computed(() => [
  { key: 'length', ok: form.password.length >= 8 && form.password.length <= 64, label: messages.register.checklist[0] },
  { key: 'upper', ok: /[A-Z]/.test(form.password), label: messages.register.checklist[1] },
  { key: 'lower', ok: /[a-z]/.test(form.password), label: messages.register.checklist[2] },
  { key: 'digit', ok: /\d/.test(form.password), label: messages.register.checklist[3] },
  { key: 'special', ok: /[^A-Za-z0-9]/.test(form.password), label: messages.register.checklist[4] },
]);

const passwordOkCount = computed(() => passwordRules.value.filter((r) => r.ok).length);

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
    if (!form.department.trim()) errors.department = messages.auth.departmentRequired;
    if (!form.staffCode.trim()) errors.staffCode = messages.auth.staffCodeRequired;
    if (form.teacherBio.length > TEACHER_BIO_MAX) errors.teacherBio = messages.auth.teacherBioMax;
  }
  if (!form.agreePolicy) errors.agreePolicy = messages.register.agreePolicyError;
  // Xóa key cũ không còn trong errors — tránh stale error khi chuyển vai trò / nhập lại.
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

async function onSubmit(): Promise<void> {
  submitError.value = '';
  markAllTouched(); // submit = chạm mọi field → lỗi inline hiện ngay dù chưa blur
  if (!validate()) return;
  submitting.value = true;
  try {
    const isTeacher = role.value === 'teacher';
    await auth.register({
      displayName: form.displayName.trim(),
      email: form.email.trim().toLowerCase(),
      password: form.password,
      isTeacher,
      ...(isTeacher
        ? {
            department: form.department.trim(),
            staffCode: form.staffCode.trim(),
            teacherBio: form.teacherBio.trim(),
          }
        : {}),
    });
    // Thành công → checkmark ngắn trên nút (khoảnh khắc đầu tư), giữ nguyên luồng redirect
    submitSuccess.value = true;
    submitting.value = false;
    if (isTeacher) {
      registeredTeacher.value = true;
    } else {
      await router.replace({ name: 'path' });
    }
  } catch (err) {
    if (err instanceof ApiError) {
      submitError.value = err.message;
    } else {
      submitError.value = messages.auth.loginFailed;
    }
  } finally {
    submitting.value = false;
  }
}

const BRAND_POINTS = [
  { icon: Sparkles, text: messages.auth.brandPoint1 },
  { icon: Target, text: messages.auth.brandPoint2 },
  { icon: CheckCircle2, text: messages.auth.brandPoint3 },
] as const;

/** Strip block-token trang trí (aria-hidden) — dấu vân tay Data Bench. */
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
      <!-- Brand panel: nền tối canvas-ink + block-token (KHÔNG gradient) -->
      <aside class="register__aside" aria-label="Giới thiệu DSA Visual">
        <!-- Block-token nổi: 1 block mới mỗi 3s, vị trí ngẫu nhiên (pulse — signature §1.5) -->
        <div class="register__float-tokens" aria-hidden="true">
          <div
            v-for="t in floatTokens"
            :key="t.id"
            class="register__float-token"
            :style="{ top: t.top, left: t.left }"
          >
            <BlockToken size="sm" :value="t.value" :index="String(t.id).padStart(2, '0')" pulse />
          </div>
        </div>

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

      <!-- Form đăng ký -->
      <div class="register__form-col">
        <form class="register__card" novalidate @submit.prevent="onSubmit">
          <h1 class="register__title">{{ messages.auth.registerTitle }}</h1>
          <p class="register__subtitle">{{ messages.register.subtitle }}</p>

          <div v-if="registeredTeacher" class="register__pending" role="status">
            <ShieldCheck :size="24" class="register__pending-icon" aria-hidden="true" />
            <div class="register__pending-body">
              <p class="register__pending-title">{{ messages.auth.teacherPendingSuccess }}</p>
              <p class="register__pending-desc">{{ messages.auth.teacherPendingNote }}</p>
              <RouterLink class="register__pending-link" :to="{ name: 'login' }">
                {{ messages.register.backToLogin }}
              </RouterLink>
            </div>
          </div>

          <template v-if="!registeredTeacher">
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

            <div class="register__checklist" :aria-label="'Yêu cầu mật khẩu'">
              <span
                v-for="rule in passwordRules"
                :key="rule.key"
                class="register__check"
                :class="{ 'register__check--ok': rule.ok }"
                role="listitem"
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

            <div v-if="role === 'teacher'" class="register__teacher">
              <Input
                v-model="form.department"
                :label="messages.auth.department"
                :icon="Building2"
                :error="touched.department ? fieldErrors.department : ''"
                :placeholder="messages.auth.departmentPlaceholder"
                autocomplete="organization"
                :maxlength="100"
                required
                @blur="onBlur('department')"
              />

              <Input
                v-model="form.staffCode"
                :label="messages.auth.staffCode"
                :icon="BadgeCheck"
                :error="touched.staffCode ? fieldErrors.staffCode : ''"
                :placeholder="messages.auth.staffCodePlaceholder"
                autocomplete="off"
                :maxlength="50"
                required
                @blur="onBlur('staffCode')"
              />

              <div class="register__field">
                <label class="register__field-label" for="register-teacher-bio">{{ messages.auth.teacherBio }}</label>
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
              <span v-if="submitSuccess" class="register__success" aria-hidden="true">
                <Check :size="16" />
              </span>
              <template v-else>{{ messages.register.submit }}</template>
            </Button>

            <p class="register__switch">
              {{ messages.register.hasAccount }}
              <RouterLink :to="{ name: 'login' }">{{ messages.register.toLogin }}</RouterLink>
            </p>
          </template>
        </form>
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

/* Shell — elevation bằng surface + border (KHÔNG shadow, §6) */
.register__shell {
  display: grid;
  grid-template-columns: 1.05fr 1fr;
  width: 100%;
  max-width: 940px;
  border-radius: var(--radius-lg);
  overflow: hidden;
  border: 1px solid var(--color-border);
  background: var(--color-card);
}

/* ── Brand panel — LUÔN tối (quyết định xuyên-nhóm 5) ── */
.register__aside {
  position: relative;
  overflow: hidden;
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
}

/* Block-token nổi — 1 block mới mỗi 3s, vị trí ngẫu nhiên, nhấp nháy chậm (pulse) */
.register__float-tokens {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.register__float-token {
  position: absolute;
  animation: register-token-enter 6s var(--ease-in-out) both;
}

@keyframes register-token-enter {
  0% { opacity: 0; transform: translateY(10px) scale(0.9); }
  12% { opacity: 0.85; transform: translateY(0) scale(1); }
  80% { opacity: 0.85; }
  100% { opacity: 0; transform: translateY(-8px); }
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

/* Block-token strip — signature "dữ liệu được đánh số" */
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

/* Brand points — stagger khi vào màn (easing chuẩn §7.10; test env jsdom không chạy CSS anim) */
.register__point {
  animation: register-point-in 400ms var(--ease-out-expo) both;
}

.register__point:nth-child(1) { animation-delay: 140ms; }
.register__point:nth-child(2) { animation-delay: 220ms; }
.register__point:nth-child(3) { animation-delay: 300ms; }

@keyframes register-point-in {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

.register__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
  color: rgba(255, 255, 255, 0.6);
}

/* ── Form ── */
.register__form-col {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: clamp(1.5rem, 3vw, 2.25rem);
}

.register__card {
  width: 100%;
  max-width: 24rem;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.register__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  margin: 0;
  margin-bottom: var(--space-xs);
}

.register__subtitle {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  margin-bottom: var(--space-sm);
}

/* Checklist mật khẩu — grid 2 cột để gọn chiều cao form */
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

/* ── Segmented chọn vai trò (Button ghost + active bg-card/border, không shadow) ── */
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
    color: var(--color-foreground-secondary);
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

/* ── Form con giảng viên ── */
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
  min-height: 96px;
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

/* Submit thành công — checkmark vào nhẹ (khoảnh khắc đầu tư của màn) */
.register__success {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  animation: register-check-pop 300ms var(--ease-out-expo) both;
}

@keyframes register-check-pop {
  0% { transform: scale(0.4); opacity: 0; }
  60% { transform: scale(1.15); }
  100% { transform: scale(1); opacity: 1; }
}

/* Input focus — border + glow ring chuyển mượt (token motion) */
.register :deep(.ui-input input) {
  transition:
    border-color var(--duration-normal) var(--ease-out-expo),
    box-shadow var(--duration-normal) var(--ease-out-expo);
}

.register :deep(.ui-input input:focus-visible) {
  border-color: var(--color-primary);
  box-shadow: var(--glow-primary);
}

.register__bio:focus-visible {
  border-color: var(--color-primary);
  box-shadow: var(--glow-primary);
}

.register__switch {
    text-align: center;
    margin: 0;
    font-size: var(--text-sm);
    color: var(--color-text-secondary);
  }

  .register__switch a {
    text-decoration: underline;
    text-underline-offset: 2px;
  }

/* ── Pending teacher ── */
.register__pending {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  background: color-mix(in srgb, var(--color-success) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 35%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  font-size: var(--text-sm);
}

.register__pending-icon {
  color: var(--color-success);
  flex-shrink: 0;
  margin-top: 2px;
}

.register__pending-title {
  font-weight: 600;
  margin-bottom: var(--space-xs);
}

.register__pending-desc {
  margin-bottom: var(--space-sm);
}

.register__pending-link {
  font-weight: 500;
}

/* <768px: ẩn aside, form full-width căn giữa (DESIGN.md §8) */
@media (max-width: 767px) {
  .register__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .register__aside { display: none; }
  .register__form-col { padding: var(--space-lg) var(--space-md); }
}

@media (min-width: 768px) and (max-width: 820px) {
  .register__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .register__aside { padding: var(--space-lg); }
}

@media (prefers-reduced-motion: reduce) {
  .register__float-tokens { display: none; }
  .register__point { animation: none; }
  .register__success { animation: none; }
}
</style>
