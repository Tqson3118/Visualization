<script setup lang="ts">
// LoginView — Màn 02: đăng nhập (SDD Màn 02)
// G-F2b: split layout (trái = brand + gradient + feature list; phải = form).
// GIỮ nguyên logic login + route/redirect — chỉ visual (Input shadcn label/error/icon,
// Button loading, link quên mật khẩu/đăng ký).
import { reactive, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';

import { useAuthStore } from '@/stores/auth';
import { messages } from '@/i18n/vi';
import { isValidEmail } from '@/utils/validators';
import BaseIcon from '@/components/ui/BaseIcon.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const auth = useAuthStore();
const route = useRoute();
const router = useRouter();

const form = reactive({
  email: '',
  password: '',
});

const submitError = ref('');
const submitting = ref(false);

async function onSubmit(): Promise<void> {
  submitError.value = '';
  if (!isValidEmail(form.email)) {
    submitError.value = messages.auth.invalidEmail;
    return;
  }
  submitting.value = true;
  try {
    await auth.login(form.email, form.password);
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/path';
    await router.replace(redirect);
  } catch {
    submitError.value = messages.auth.loginFailed;
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
  <main class="login">
    <div class="login__shell">
      <!-- Brand panel: gradient Aurora + feature list -->
      <aside class="login__aside" aria-label="Giới thiệu DSA Visual">
        <div class="login__aside-inner">
          <span class="login__aside-badge">{{ messages.app.name }}</span>
          <h2 class="login__aside-title">{{ messages.app.tagline }}</h2>
          <ul class="login__points">
            <li v-for="point in BRAND_POINTS" :key="point.icon" class="login__point">
              <BaseIcon :name="point.icon" :size="18" class="login__point-icon" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Form đăng nhập -->
      <div class="login__form-col">
        <form class="login__card card" novalidate @submit.prevent="onSubmit">
          <h1 class="login__title text-gradient-aurora">{{ messages.auth.loginTitle }}</h1>
          <p class="login__subtitle text-muted">{{ messages.auth.loginSubtitle }}</p>

          <Input
            id="email"
            v-model="form.email"
            label="Email"
            icon="mail"
            type="email"
            autocomplete="email"
            :placeholder="messages.auth.emailPlaceholder"
            required
          />

          <Input
            id="password"
            v-model="form.password"
            label="Mật khẩu"
            icon="lock"
            type="password"
            autocomplete="current-password"
            :placeholder="messages.auth.passwordPlaceholder"
            required
          />

          <div class="login__links">
            <RouterLink class="login__forgot" :to="{ name: 'forgot-password' }">
              {{ messages.auth.forgotPassword }}
            </RouterLink>
          </div>

          <p v-if="submitError" class="login__error" role="alert">{{ submitError }}</p>

          <Button type="submit" class="login__submit" :loading="submitting" block>
            {{ messages.auth.loginSubmit }}
          </Button>

          <p class="text-muted login__switch">
            {{ messages.auth.noAccount }}
            <RouterLink :to="{ name: 'register' }">{{ messages.auth.toRegister }}</RouterLink>
          </p>
        </form>
      </div>
    </div>
  </main>
</template>

<style scoped>
.login {
  min-height: calc(100dvh - 120px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

.login__shell {
  display: grid;
  grid-template-columns: 1.05fr 1fr;
  width: 100%;
  max-width: 920px;
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: var(--shadow-xl);
  border: 1px solid var(--color-border);
  background: var(--color-surface);
}

/* ── Brand panel ── */
.login__aside {
  background-image: var(--gradient-aurora);
  color: #fff;
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
  position: relative;
  isolation: isolate;
}

/* GP-T9b (#8): dark mode gradient Aurora sáng → phủ lớp tối để chữ trắng ≥ 4.5:1. */
.dark .login__aside {
  background-image: linear-gradient(rgba(4, 47, 46, 0.62), rgba(4, 47, 46, 0.62)), var(--gradient-aurora);
}

.dark .login__aside::before,
.dark .login__aside::after {
  opacity: 0.12;
}

.login__aside::before,
.login__aside::after {
  content: '';
  position: absolute;
  border-radius: 50%;
  filter: blur(52px);
  opacity: 0.45;
  z-index: -1;
}

.login__aside::before {
  width: 260px;
  height: 260px;
  background: rgba(255, 255, 255, 0.4);
  top: -90px;
  left: -70px;
}

.login__aside::after {
  width: 220px;
  height: 220px;
  background: rgba(255, 255, 255, 0.3);
  bottom: -80px;
  right: -50px;
}

.login__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.login__aside-badge {
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

.login__aside-title {
  font-size: var(--text-xl);
  line-height: 1.35;
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.login__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin-top: var(--space-sm);
}

.login__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  font-weight: 600;
  color: rgba(255, 255, 255, 0.95);
}

.login__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
}

/* ── Form ── */
.login__form-col {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: clamp(1.5rem, 4vw, 2.5rem);
}

.login__card {
  width: 100%;
  max-width: 24rem;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  box-shadow: none;
  border: none;
  background: transparent;
  padding: 0;
}

.login__title {
  font-size: var(--text-3xl);
  margin: 0;
}

.login__subtitle {
  font-size: var(--text-sm);
  margin-bottom: var(--space-sm);
}

.login__links {
  display: flex;
  justify-content: flex-end;
  margin-top: 2px;
}

.login__forgot {
  font-size: var(--text-xs);
  font-weight: 600;
}

.login__error {
  color: var(--color-destructive);
  font-size: var(--text-sm);
  margin-top: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-destructive) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-destructive) 30%, transparent);
}

.login__submit {
  margin-top: var(--space-md);
}

.login__switch {
  text-align: center;
  margin-top: var(--space-md);
  font-size: var(--text-sm);
}

@media (max-width: 820px) {
  .login__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .login__aside { padding: var(--space-lg); }
}
</style>
