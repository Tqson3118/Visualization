<script setup lang="ts">
// ResetPasswordView — Màn N-2: đặt lại mật khẩu bằng token 1 lần (FR-1.6).
// H-E1 polish: split layout đồng bộ LoginView (brand aside + form card),
// checklist mật khẩu + toast success. GIỮ nguyên logic validate/submit/redirect.
import { computed, reactive, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { Motion } from 'motion-v';
import { toast } from 'vue-sonner';

import * as authApi from '@/api/auth';
import { validatePassword } from '@/utils/validators';
import { messages } from '@/i18n/vi';
import BaseIcon from '@/components/ui/BaseIcon.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const route = useRoute();
const router = useRouter();

const form = reactive({ password: '', confirm: '' });
const error = ref('');
const success = ref(false);
const submitting = ref(false);

const token = computed(() => String(route.query.token ?? ''));

const passwordRules = computed(() => [
  { ok: form.password.length >= 8 && form.password.length <= 64, label: messages.reset.checklist[0] },
  { ok: /[A-Z]/.test(form.password), label: messages.reset.checklist[1] },
  { ok: /\d/.test(form.password), label: messages.reset.checklist[2] },
  { ok: /[^A-Za-z0-9]/.test(form.password), label: messages.reset.checklist[3] },
]);

async function onSubmit(): Promise<void> {
  error.value = '';
  if (!validatePassword(form.password).ok) {
    error.value = messages.reset.invalid;
    return;
  }
  if (form.confirm !== form.password) {
    error.value = messages.reset.mismatch;
    return;
  }
  if (!token.value) {
    error.value = messages.reset.noToken;
    return;
  }
  submitting.value = true;
  try {
    await authApi.resetPassword({ token: token.value, newPassword: form.password });
    success.value = true;
    toast.success(messages.reset.toastSuccess);
    setTimeout(() => void router.replace({ name: 'login' }), 2000);
  } catch (err) {
    error.value = err instanceof Error ? err.message : messages.reset.failed;
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
  <main class="reset">
    <Motion
      class="reset__shell"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.32, ease: 'easeOut' }"
    >
      <!-- Brand panel: gradient Aurora + feature list (đồng bộ LoginView) -->
      <aside class="reset__aside" aria-label="Giới thiệu DSA Visual">
        <div class="reset__aside-inner">
          <span class="reset__aside-badge">{{ messages.app.name }}</span>
          <h2 class="reset__aside-title">{{ messages.app.tagline }}</h2>
          <ul class="reset__points">
            <li v-for="point in BRAND_POINTS" :key="point.icon" class="reset__point">
              <BaseIcon :name="point.icon" :size="18" class="reset__point-icon" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Form đặt lại mật khẩu -->
      <div class="reset__form-col">
        <div class="reset__card card">
          <h1 class="reset__title text-gradient-aurora">{{ messages.reset.title }}</h1>
          <p class="reset__subtitle text-muted">{{ messages.reset.subtitle }}</p>

          <div v-if="success" class="reset__success" role="status">
            <span class="reset__success-icon" aria-hidden="true">
              <BaseIcon name="check-circle" :size="26" />
            </span>
            <p class="reset__success-title">{{ messages.reset.successTitle }}</p>
            <p class="reset__success-desc text-muted">{{ messages.reset.successDesc }}</p>
          </div>

          <template v-else>
            <form novalidate @submit.prevent="onSubmit">
              <Input
                v-model="form.password"
                label="Mật khẩu mới"
                type="password"
                icon="key"
                autocomplete="new-password"
                :placeholder="messages.reset.passwordPlaceholder"
                required
              />
              <div class="reset__checklist">
                <span
                  v-for="(rule, idx) in passwordRules"
                  :key="idx"
                  class="reset__check"
                  :class="{ 'reset__check--ok': rule.ok }"
                >
                  <span class="reset__check-mark" aria-hidden="true">{{ rule.ok ? '✓' : '○' }}</span>
                  {{ rule.label }}
                </span>
              </div>
              <Input
                v-model="form.confirm"
                label="Xác nhận mật khẩu mới"
                type="password"
                icon="lock"
                autocomplete="new-password"
                :placeholder="messages.reset.confirmPlaceholder"
                required
              />
              <p v-if="error" class="reset__error" role="alert">{{ error }}</p>
              <Button type="submit" class="reset__submit" :loading="submitting" block>
                {{ messages.reset.submit }}
              </Button>
            </form>
            <RouterLink class="reset__back" :to="{ name: 'login' }">
              ← {{ messages.reset.back }}
            </RouterLink>
          </template>
        </div>
      </div>
    </Motion>
  </main>
</template>

<style scoped>
.reset {
  min-height: calc(100dvh - 120px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

.reset__shell {
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
.reset__aside {
  background-image: var(--gradient-aurora);
  color: #fff;
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
  position: relative;
  isolation: isolate;
}

/* GP-T9b (#8): dark mode gradient Aurora sáng → phủ lớp tối để chữ trắng ≥ 4.5:1. */
.dark .reset__aside {
  background-image: linear-gradient(rgba(4, 47, 46, 0.62), rgba(4, 47, 46, 0.62)), var(--gradient-aurora);
}

.dark .reset__aside::before,
.dark .reset__aside::after {
  opacity: 0.12;
}

.reset__aside::before,
.reset__aside::after {
  content: '';
  position: absolute;
  border-radius: 50%;
  filter: blur(52px);
  opacity: 0.45;
  z-index: -1;
}

.reset__aside::before {
  width: 260px;
  height: 260px;
  background: rgba(255, 255, 255, 0.4);
  top: -90px;
  left: -70px;
}

.reset__aside::after {
  width: 220px;
  height: 220px;
  background: rgba(255, 255, 255, 0.3);
  bottom: -80px;
  right: -50px;
}

.reset__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.reset__aside-badge {
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

.reset__aside-title {
  font-size: var(--text-xl);
  line-height: 1.35;
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.reset__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin-top: var(--space-sm);
}

.reset__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  font-weight: 600;
  color: rgba(255, 255, 255, 0.95);
}

.reset__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
}

/* ── Form ── */
.reset__form-col {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: clamp(1.5rem, 4vw, 2.5rem);
}

.reset__card {
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

.reset__title {
  font-size: var(--text-3xl);
  margin: 0;
}

.reset__subtitle {
  font-size: var(--text-sm);
  margin-bottom: var(--space-sm);
}

/* Checklist mật khẩu — grid 2 cột để gọn chiều cao form */
.reset__checklist {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 4px 12px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  padding-inline: var(--space-xs);
  margin-bottom: var(--space-xs);
}

.reset__check {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
}

/* GP-T9b: text ok dùng primary (≥ 4.5:1 cả 2 theme); dấu ✓ nhấn màu success */
.reset__check--ok {
  color: var(--color-primary);
  font-weight: 600;
}

.reset__check--ok .reset__check-mark {
  color: var(--color-success);
}

.reset__error {
  color: var(--color-destructive);
  font-size: var(--text-sm);
  margin-top: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-destructive) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-destructive) 30%, transparent);
}

.reset__submit {
  margin-top: var(--space-md);
}

.reset__back {
  font-size: var(--text-sm);
  font-weight: 600;
  margin-top: var(--space-sm);
}

/* ── State thành công ── */
.reset__success {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.reset__success-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 52px;
  height: 52px;
  border-radius: var(--radius-full);
  color: var(--color-success);
  background: color-mix(in srgb, var(--color-success) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 35%, transparent);
}

.reset__success-title {
  font-size: var(--text-lg);
  font-weight: 700;
  margin-top: var(--space-xs);
}

.reset__success-desc {
  font-size: var(--text-sm);
}

@media (max-width: 820px) {
  .reset__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .reset__aside { padding: var(--space-lg); }
}
</style>
