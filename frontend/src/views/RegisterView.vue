<script setup lang="ts">
// RegisterView — Màn 02 đăng ký (H-E1 polish): split layout đồng bộ LoginView
// (brand aside gradient Aurora + form card), validation inline, checklist mật
// khẩu sống, checkbox giảng viên + đồng ý chính sách. GIỮ nguyên logic validate/
// submit + selector e2e (form.register__card, label.register__row, 4 .ui-input).
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

const auth = useAuthStore();
const router = useRouter();

const form = reactive({
  displayName: '',
  email: '',
  password: '',
  confirmPassword: '',
  isTeacher: false,
  agreePolicy: false,
});

const touched = reactive({ displayName: false, email: false, password: false, confirmPassword: false });
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

function validate(): boolean {
  const errors: Record<string, string> = {};
  if (!form.displayName.trim() || form.displayName.trim().length < 2) {
    errors.displayName = messages.register.displayNameError;
  }
  if (!isValidEmail(form.email)) errors.email = messages.auth.invalidEmail;
  const pwd = validatePassword(form.password);
  if (!pwd.ok) errors.password = messages.auth.passwordRequirement;
  if (form.confirmPassword !== form.password) errors.confirmPassword = messages.register.confirmError;
  if (!form.agreePolicy) errors.agreePolicy = messages.register.agreePolicyError;
  Object.assign(fieldErrors, errors);
  return Object.keys(errors).length === 0;
}

function onBlur(field: keyof typeof touched): void {
  touched[field] = true;
  validate();
}

async function onSubmit(): Promise<void> {
  submitError.value = '';
  if (!validate()) return;
  submitting.value = true;
  try {
    await auth.register({
      displayName: form.displayName.trim(),
      email: form.email.trim().toLowerCase(),
      password: form.password,
      isTeacher: form.isTeacher,
    });
    if (form.isTeacher) {
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
              <p class="register__pending-title">{{ messages.register.pendingTitle }}</p>
              <p class="register__pending-desc">{{ messages.register.pendingDesc }}</p>
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

            <label class="register__row">
              <input v-model="form.isTeacher" type="checkbox" />
              <span>{{ messages.register.isTeacher }}</span>
            </label>

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
  background: rgba(255, 255, 255, 0.22);
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
