<script setup lang="ts">
// ForgotPasswordView — Màn N-2: gửi link khôi phục mật khẩu (FR-1.6).
// H-E1 polish: split layout đồng bộ LoginView (brand aside + form card),
// toast success sau khi gửi. GIỮ nguyên logic gọi API + state sent.
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import { Motion } from 'motion-v';
import { toast } from 'vue-sonner';

import * as authApi from '@/api/auth';
import { isValidEmail } from '@/utils/validators';
import { messages } from '@/i18n/vi';
import BaseIcon from '@/components/ui/BaseIcon.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const email = ref('');
const error = ref('');
const sent = ref(false);
const submitting = ref(false);

async function onSubmit(): Promise<void> {
  error.value = '';
  if (!isValidEmail(email.value)) {
    error.value = messages.forgot.invalidEmail;
    return;
  }
  submitting.value = true;
  try {
    await authApi.forgotPassword(email.value.trim());
    sent.value = true;
    toast.success(messages.forgot.toastSent);
  } catch (err) {
    error.value = err instanceof Error ? err.message : messages.forgot.failed;
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
  <main class="forgot">
    <Motion
      class="forgot__shell"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.32, ease: 'easeOut' }"
    >
      <!-- Brand panel: gradient Aurora + feature list (đồng bộ LoginView) -->
      <aside class="forgot__aside" aria-label="Giới thiệu DSA Visual">
        <div class="forgot__aside-inner">
          <span class="forgot__aside-badge">{{ messages.app.name }}</span>
          <h2 class="forgot__aside-title">{{ messages.app.tagline }}</h2>
          <ul class="forgot__points">
            <li v-for="point in BRAND_POINTS" :key="point.icon" class="forgot__point">
              <BaseIcon :name="point.icon" :size="18" class="forgot__point-icon" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Form gửi link khôi phục -->
      <div class="forgot__form-col">
        <div class="forgot__card card">
          <h1 class="forgot__title text-gradient-aurora">{{ messages.forgot.title }}</h1>

          <div v-if="sent" class="forgot__sent" role="status">
            <span class="forgot__sent-icon" aria-hidden="true">
              <BaseIcon name="mail" :size="26" />
            </span>
            <p class="forgot__sent-title">{{ messages.forgot.sentTitle }}</p>
            <p class="forgot__sent-desc">{{ messages.forgot.sentDesc(email) }}</p>
            <p class="forgot__sent-hint text-muted">{{ messages.forgot.sentHint }}</p>
            <RouterLink class="forgot__sent-link" :to="{ name: 'login' }">
              {{ messages.forgot.back }}
            </RouterLink>
          </div>

          <template v-else>
            <p class="forgot__desc text-muted">{{ messages.forgot.desc }}</p>
            <form novalidate @submit.prevent="onSubmit">
              <Input
                v-model="email"
                label="Email"
                type="email"
                icon="mail"
                :error="error"
                :placeholder="messages.forgot.emailPlaceholder"
                autocomplete="email"
                required
              />
              <Button type="submit" class="forgot__submit" :loading="submitting" block>
                {{ messages.forgot.submit }}
              </Button>
            </form>
            <RouterLink class="forgot__back" :to="{ name: 'login' }">
              ← {{ messages.forgot.back }}
            </RouterLink>
          </template>
        </div>
      </div>
    </Motion>
  </main>
</template>

<style scoped>
.forgot {
  min-height: calc(100dvh - 120px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

.forgot__shell {
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
.forgot__aside {
  background-image: var(--gradient-aurora);
  color: #fff;
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
  position: relative;
  isolation: isolate;
}

/* GP-T9b (#8): dark mode gradient Aurora sáng → phủ lớp tối để chữ trắng ≥ 4.5:1. */
.dark .forgot__aside {
  background-image: linear-gradient(rgba(4, 47, 46, 0.62), rgba(4, 47, 46, 0.62)), var(--gradient-aurora);
}

.dark .forgot__aside::before,
.dark .forgot__aside::after {
  opacity: 0.12;
}

.forgot__aside::before,
.forgot__aside::after {
  content: '';
  position: absolute;
  border-radius: 50%;
  filter: blur(52px);
  opacity: 0.45;
  z-index: -1;
}

.forgot__aside::before {
  width: 260px;
  height: 260px;
  background: rgba(255, 255, 255, 0.4);
  top: -90px;
  left: -70px;
}

.forgot__aside::after {
  width: 220px;
  height: 220px;
  background: rgba(255, 255, 255, 0.3);
  bottom: -80px;
  right: -50px;
}

.forgot__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.forgot__aside-badge {
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

.forgot__aside-title {
  font-size: var(--text-xl);
  line-height: 1.35;
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.forgot__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin-top: var(--space-sm);
}

.forgot__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  font-weight: 600;
  color: rgba(255, 255, 255, 0.95);
}

.forgot__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
}

/* ── Form ── */
.forgot__form-col {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: clamp(1.5rem, 4vw, 2.5rem);
}

.forgot__card {
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

.forgot__title {
  font-size: var(--text-3xl);
  margin: 0;
}

.forgot__desc {
  font-size: var(--text-sm);
  margin-bottom: var(--space-sm);
}

.forgot__submit {
  margin-top: var(--space-md);
}

.forgot__back {
  font-size: var(--text-sm);
  font-weight: 600;
  margin-top: var(--space-sm);
}

/* ── State đã gửi ── */
.forgot__sent {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.forgot__sent-icon {
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

.forgot__sent-title {
  font-size: var(--text-lg);
  font-weight: 700;
  margin-top: var(--space-xs);
}

.forgot__sent-desc {
  line-height: 1.55;
}

.forgot__sent-hint {
  font-size: var(--text-sm);
}

.forgot__sent-link {
  font-weight: 600;
  margin-top: var(--space-sm);
}

@media (max-width: 820px) {
  .forgot__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .forgot__aside { padding: var(--space-lg); }
}
</style>
