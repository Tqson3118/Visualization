<script setup lang="ts">
// RegisterView — Màn 02 đăng ký: split layout đồng bộ LoginView (H-E1 polish:
// brand aside gradient Aurora + form card, validation inline, checklist mật khẩu sống)
// + segmented "Đăng ký với vai trò" (Sinh viên/Giảng viên — task L) + form con giảng viên
// (Khoa/Bộ môn, Mã GV, Kinh nghiệm) + đồng ý chính sách. GIỮ nguyên logic validate/submit
// + selector e2e (form.register__card, label.register__row, button.register__role-option).
import { computed, reactive, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { Motion } from 'motion-v';

import { useAuthStore } from '@/stores/auth';
import { ApiError } from '@/api/client';
import { messages } from '@/i18n/vi';
import { isValidEmail, validatePassword } from '@/utils/validators';
import BaseIcon from '@/components/ui/BaseIcon.vue';
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
const registeredTeacher = ref(false);

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
  { icon: 'sparkles', text: messages.auth.brandPoint1 },
  { icon: 'target', text: messages.auth.brandPoint2 },
  { icon: 'check-circle', text: messages.auth.brandPoint3 },
] as const;
</script>

<template>
  <main class="register">
    <Motion
      class="register__shell"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.32, ease: 'easeOut' }"
    >
      <!-- Brand panel: gradient Aurora + feature list (đồng bộ LoginView) -->
      <aside class="register__aside" aria-label="Giới thiệu DSA Visual">
        <div class="register__aside-inner">
          <span class="register__aside-badge">{{ messages.app.name }}</span>
          <h2 class="register__aside-title">{{ messages.app.tagline }}</h2>
          <ul class="register__points">
            <li v-for="point in BRAND_POINTS" :key="point.icon" class="register__point">
              <BaseIcon :name="point.icon" :size="18" class="register__point-icon" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Form đăng ký -->
      <div class="register__form-col">
        <form class="register__card card" novalidate @submit.prevent="onSubmit">
          <h1 class="register__title text-gradient-aurora">{{ messages.auth.registerTitle }}</h1>
          <p class="register__subtitle text-muted">{{ messages.register.subtitle }}</p>

          <div v-if="registeredTeacher" class="register__pending" role="status">
            <BaseIcon name="shield" :size="24" class="register__pending-icon" />
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
              icon="user"
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
              icon="mail"
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
              icon="lock"
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
                <span class="register__check-mark" aria-hidden="true">{{ rule.ok ? '✓' : '○' }}</span>
                {{ rule.label }}
              </span>
            </div>

            <Input
              v-model="form.confirmPassword"
              label="Xác nhận mật khẩu"
              type="password"
              icon="lock"
              :error="touched.confirmPassword ? fieldErrors.confirmPassword : ''"
              :placeholder="messages.register.confirmPlaceholder"
              autocomplete="new-password"
              required
              @blur="onBlur('confirmPassword')"
            />

            <fieldset class="register__role">
              <legend class="label register__role-label">{{ messages.auth.roleLabel }}</legend>
              <div class="register__role-group">
                <button
                  v-for="opt in roleOptions"
                  :key="opt.value"
                  type="button"
                  :aria-pressed="role === opt.value"
                  class="register__role-option"
                  :class="{ 'register__role-option--active': role === opt.value }"
                  @click="selectRole(opt.value)"
                >
                  {{ opt.label }}
                </button>
              </div>
            </fieldset>

            <div v-if="role === 'teacher'" class="register__teacher">
              <Input
                v-model="form.department"
                :label="messages.auth.department"
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
                :error="touched.staffCode ? fieldErrors.staffCode : ''"
                :placeholder="messages.auth.staffCodePlaceholder"
                autocomplete="off"
                :maxlength="50"
                required
                @blur="onBlur('staffCode')"
              />

              <div class="register__field">
                <label class="label" for="register-teacher-bio">{{ messages.auth.teacherBio }}</label>
                <textarea
                  id="register-teacher-bio"
                  v-model="form.teacherBio"
                  class="input register__bio"
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

            <Button type="submit" class="register__submit" :loading="submitting" block>
              {{ messages.register.submit }}
            </Button>

            <p class="text-muted register__switch">
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

.register__shell {
  display: grid;
  grid-template-columns: 1.05fr 1fr;
  width: 100%;
  max-width: 940px;
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: var(--shadow-xl);
  border: 1px solid var(--color-border);
  background: var(--color-surface);
}

/* ── Brand panel ── */
.register__aside {
  background-image: var(--gradient-aurora);
  color: #fff;
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
  position: relative;
  isolation: isolate;
}

/* GP-T9b (#8): dark mode gradient Aurora sáng → phủ lớp tối để chữ trắng ≥ 4.5:1. */
.dark .register__aside {
  background-image: linear-gradient(rgba(4, 47, 46, 0.62), rgba(4, 47, 46, 0.62)), var(--gradient-aurora);
}

.dark .register__aside::before,
.dark .register__aside::after {
  opacity: 0.12;
}

.register__aside::before,
.register__aside::after {
  content: '';
  position: absolute;
  border-radius: 50%;
  filter: blur(52px);
  opacity: 0.45;
  z-index: -1;
}

.register__aside::before {
  width: 260px;
  height: 260px;
  background: rgba(255, 255, 255, 0.4);
  top: -90px;
  left: -70px;
}

.register__aside::after {
  width: 220px;
  height: 220px;
  background: rgba(255, 255, 255, 0.3);
  bottom: -80px;
  right: -50px;
}

.register__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.register__aside-badge {
  display: inline-flex;
  width: fit-content;
  padding: 4px 14px;
  border-radius: var(--radius-full);
  background: rgba(255, 255, 255, 0.35);
  border: 1px solid rgba(255, 255, 255, 0.45);
  font-size: var(--text-xs);
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  backdrop-filter: blur(4px);
}

.register__aside-title {
  font-size: var(--text-xl);
  line-height: 1.35;
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.register__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin-top: var(--space-sm);
}

.register__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  font-weight: 600;
  color: rgba(255, 255, 255, 0.95);
}

.register__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
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
  box-shadow: none;
  border: none;
  background: transparent;
  padding: 0;
}

.register__title {
  font-size: var(--text-3xl);
  margin: 0;
  margin-bottom: 2px;
}

.register__subtitle {
  font-size: var(--text-sm);
  margin-bottom: var(--space-sm);
}

/* Checklist mật khẩu — grid 2 cột để gọn chiều cao form */
.register__checklist {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 4px 12px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  padding-inline: var(--space-xs);
}

.register__check {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
}

/* GP-T9b: text ok dùng primary (≥ 4.5:1 cả 2 theme); dấu ✓ nhấn màu success */
.register__check--ok {
  color: var(--color-primary);
  font-weight: 600;
}

.register__check--ok .register__check-mark {
  color: var(--color-success);
}

/* ── Segmented chọn vai trò ── */
.register__role {
  border: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.register__role-label { margin-bottom: 0; }

.register__role-group {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
  padding: 4px;
  background: var(--color-muted);
  border-radius: var(--radius-md);
}

.register__role-option {
  padding: 0.5rem;
  border: 1px solid transparent;
  border-radius: calc(var(--radius-md) - 4px);
  background: transparent;
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-muted);
  transition: var(--transition-fast);
}

.register__role-option:hover { color: var(--color-primary); }

.register__role-option--active {
  background: var(--color-surface);
  border-color: var(--color-border);
  color: var(--color-primary);
  box-shadow: var(--shadow-sm);
}

/* ── Form con giảng viên ── */
.register__teacher {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 4%, transparent);
}

.register__field { display: flex; flex-direction: column; gap: var(--space-xs); }

.register__bio { min-height: 96px; resize: vertical; line-height: 1.5; }

.register__bio-meta {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-sm);
}

.register__bio-count {
  margin-left: auto;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  font-variant-numeric: tabular-nums;
}

.register__note { font-size: var(--text-xs); color: var(--color-text-muted); }

.register__row {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  cursor: pointer;
}

.register__row input[type='checkbox'] {
  width: 1rem;
  height: 1rem;
  accent-color: var(--color-primary);
  cursor: pointer;
  flex-shrink: 0;
}

.register__row a {
  font-weight: 600;
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
  margin-top: var(--space-md);
  font-size: var(--text-sm);
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
  font-weight: 700;
  margin-bottom: var(--space-xs);
}

.register__pending-desc {
  margin-bottom: var(--space-sm);
}

.register__pending-link {
  font-weight: 600;
}

@media (max-width: 820px) {
  .register__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .register__aside { padding: var(--space-lg); }
}
</style>
