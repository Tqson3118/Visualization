<script setup lang="ts">
// ForgotPasswordView — Màn N-2: gửi link khôi phục mật khẩu (FR-1.6).
// View-quality (nhóm A): aside tối canvas-ink (bỏ gradient/blob/glassmorphism), icon
// lucide-vue-next, Motion easing chuẩn, bỏ shadow shell, nút back bằng icon lucide
// (bỏ ký tự '←'). GIỮ nguyên logic gọi API + state sent.
import { ref } from 'vue';
import { RouterLink } from 'vue-router';
import { Motion } from 'motion-v';
import { toast } from 'vue-sonner';
import { ArrowLeft, CheckCircle2, Mail, Sparkles, Target } from 'lucide-vue-next';

import * as authApi from '@/api/auth';
import { isValidEmail } from '@/utils/validators';
import { messages } from '@/i18n/vi';
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
  <main class="forgot">
    <Motion
      class="forgot__shell"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <!-- Brand panel: nền tối canvas-ink + block-token (KHÔNG gradient) -->
      <aside class="forgot__aside" aria-label="Giới thiệu DSA Visual">
        <div class="forgot__aside-inner">
          <span class="forgot__aside-badge">{{ messages.app.name }}</span>
          <h2 class="forgot__aside-title">{{ messages.app.tagline }}</h2>

          <div class="forgot__aside-bench" aria-hidden="true">
            <div
              v-for="(b, idx) in BENCH_BLOCKS"
              :key="b.value"
              class="forgot__aside-block"
              :class="`forgot__aside-block--${b.state}`"
            >
              <span class="forgot__aside-block-value">{{ b.value }}</span>
              <span class="forgot__aside-block-index">{{ String(idx).padStart(2, '0') }}</span>
            </div>
          </div>

          <ul class="forgot__points">
            <li v-for="point in BRAND_POINTS" :key="point.text" class="forgot__point">
              <component :is="point.icon" :size="16" class="forgot__point-icon" aria-hidden="true" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Form gửi link khôi phục -->
      <div class="forgot__form-col">
        <div class="forgot__card">
          <h1 class="forgot__title">{{ messages.forgot.title }}</h1>

          <div v-if="sent" class="forgot__sent" role="status">
            <span class="forgot__sent-icon" aria-hidden="true">
              <Mail :size="26" />
            </span>
            <p class="forgot__sent-title">{{ messages.forgot.sentTitle }}</p>
            <p class="forgot__sent-desc">{{ messages.forgot.sentDesc(email) }}</p>
            <p class="forgot__sent-hint">{{ messages.forgot.sentHint }}</p>
            <RouterLink class="forgot__sent-link" :to="{ name: 'login' }">
              {{ messages.forgot.back }}
            </RouterLink>
          </div>

          <template v-else>
            <p class="forgot__desc">{{ messages.forgot.desc }}</p>
            <form novalidate @submit.prevent="onSubmit">
              <Input
                v-model="email"
                label="Email"
                type="email"
                :icon="Mail"
                :error="error"
                :placeholder="messages.forgot.emailPlaceholder"
                autocomplete="email"
                required
              />
              <Button type="submit" size="lg" class="forgot__submit" :loading="submitting" block>
                {{ messages.forgot.submit }}
              </Button>
            </form>
            <RouterLink class="forgot__back" :to="{ name: 'login' }">
              <ArrowLeft :size="16" aria-hidden="true" />
              {{ messages.forgot.back }}
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

/* Shell — elevation bằng surface + border (KHÔNG shadow, §6) */
.forgot__shell {
  display: grid;
  grid-template-columns: 1.05fr 1fr;
  width: 100%;
  max-width: 920px;
  border-radius: var(--radius-lg);
  overflow: hidden;
  border: 1px solid var(--color-border);
  background: var(--color-card);
}

/* ── Brand panel — LUÔN tối (quyết định xuyên-nhóm 5) ── */
.forgot__aside {
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
}

.forgot__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.forgot__aside-badge {
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

.forgot__aside-title {
  font-size: var(--text-xl);
  font-weight: 600;
  line-height: 1.35;
  letter-spacing: -0.015em;
  margin: 0;
  color: rgba(255, 255, 255, 0.92);
}

/* Block-token strip — signature "dữ liệu được đánh số" */
.forgot__aside-bench {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  margin-block: var(--space-xs);
}

.forgot__aside-block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  width: 36px;
  height: 44px;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
}

.forgot__aside-block--swap { background: var(--color-conflict); }
.forgot__aside-block--done { background: var(--color-resolved); }
.forgot__aside-block--active { box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.4); }

.forgot__aside-block-value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.4;
}

.forgot__aside-block-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

.forgot__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin: 0;
}

.forgot__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.85);
}

.forgot__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
  color: rgba(255, 255, 255, 0.6);
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
}

.forgot__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  margin: 0;
}

.forgot__desc {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  margin-bottom: var(--space-sm);
}

.forgot__submit {
  margin-top: var(--space-md);
}

.forgot__back {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  font-weight: 500;
  padding-block: var(--space-sm);
  width: fit-content;
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
  border-radius: var(--radius-md);
  color: var(--color-success);
  background: color-mix(in srgb, var(--color-success) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 35%, transparent);
}

.forgot__sent-title {
  font-size: var(--text-lg);
  font-weight: 600;
  margin-top: var(--space-xs);
}

.forgot__sent-desc {
  line-height: 1.55;
}

.forgot__sent-hint {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.forgot__sent-link {
  font-weight: 500;
  margin-top: var(--space-sm);
  padding-block: var(--space-xs);
}

@media (max-width: 768px) {
  .forgot__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .forgot__aside { padding: var(--space-lg); }
}
</style>
