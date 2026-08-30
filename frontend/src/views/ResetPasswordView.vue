<script setup lang="ts">
// ResetPasswordView — Màn N-2: đặt lại mật khẩu bằng token 1 lần (FR-1.6).
// View-quality (nhóm A): aside tối canvas-ink (bỏ gradient/blob/glassmorphism), icon
// lucide-vue-next, Motion easing chuẩn, bỏ shadow shell, checklist dùng icon lucide,
// timer redirect có cleanup khi unmount. GIỮ nguyên logic validate/submit/redirect.
import { computed, onUnmounted, reactive, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { Motion } from 'motion-v';
import { toast } from 'vue-sonner';
import { ArrowLeft, Check, CheckCircle2, Circle, KeyRound, Lock, Sparkles, Target } from 'lucide-vue-next';

import * as authApi from '@/api/auth';
import { validatePassword } from '@/utils/validators';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const route = useRoute();
const router = useRouter();

const form = reactive({ password: '', confirm: '' });
const error = ref('');
const success = ref(false);
const submitting = ref(false);
let redirectTimer: ReturnType<typeof setTimeout> | null = null;

const token = computed(() => String(route.query.token ?? ''));

const passwordRules = computed(() => [
  { key: 'length', ok: form.password.length >= 8 && form.password.length <= 64, label: messages.reset.checklist[0] },
  { key: 'upper', ok: /[A-Z]/.test(form.password), label: messages.reset.checklist[1] },
  { key: 'digit', ok: /\d/.test(form.password), label: messages.reset.checklist[2] },
  { key: 'special', ok: /[^A-Za-z0-9]/.test(form.password), label: messages.reset.checklist[3] },
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
    redirectTimer = setTimeout(() => void router.replace({ name: 'login' }), 2000);
  } catch (err) {
    error.value = err instanceof Error ? err.message : messages.reset.failed;
  } finally {
    submitting.value = false;
  }
}

onUnmounted(() => {
  if (redirectTimer) clearTimeout(redirectTimer);
});

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
  <main class="reset">
    <Motion
      class="reset__shell"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <!-- Brand panel: nền tối canvas-ink + block-token (KHÔNG gradient) -->
      <aside class="reset__aside" aria-label="Giới thiệu DSA Visual">
        <div class="reset__aside-inner">
          <span class="reset__aside-badge">{{ messages.app.name }}</span>
          <h2 class="reset__aside-title">{{ messages.app.tagline }}</h2>

          <div class="reset__aside-bench" aria-hidden="true">
            <div
              v-for="(b, idx) in BENCH_BLOCKS"
              :key="b.value"
              class="reset__aside-block"
              :class="`reset__aside-block--${b.state}`"
            >
              <span class="reset__aside-block-value">{{ b.value }}</span>
              <span class="reset__aside-block-index">{{ String(idx).padStart(2, '0') }}</span>
            </div>
          </div>

          <ul class="reset__points">
            <li v-for="point in BRAND_POINTS" :key="point.text" class="reset__point">
              <component :is="point.icon" :size="16" class="reset__point-icon" aria-hidden="true" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Form đặt lại mật khẩu -->
      <div class="reset__form-col">
        <div class="reset__card">
          <h1 class="reset__title">{{ messages.reset.title }}</h1>
          <p class="reset__subtitle">{{ messages.reset.subtitle }}</p>

          <div v-if="success" class="reset__success" role="status">
            <span class="reset__success-icon" aria-hidden="true">
              <CheckCircle2 :size="26" />
            </span>
            <p class="reset__success-title">{{ messages.reset.successTitle }}</p>
            <p class="reset__success-desc">{{ messages.reset.successDesc }}</p>
          </div>

          <template v-else>
            <form novalidate @submit.prevent="onSubmit">
              <Input
                v-model="form.password"
                label="Mật khẩu mới"
                type="password"
                :icon="KeyRound"
                autocomplete="new-password"
                :placeholder="messages.reset.passwordPlaceholder"
                required
              />
              <div class="reset__checklist">
                <span
                  v-for="rule in passwordRules"
                  :key="rule.key"
                  class="reset__check"
                  :class="{ 'reset__check--ok': rule.ok }"
                >
                  <Check v-if="rule.ok" :size="14" class="reset__check-mark" aria-hidden="true" />
                  <Circle v-else :size="14" class="reset__check-mark" aria-hidden="true" />
                  {{ rule.label }}
                </span>
              </div>
              <Input
                v-model="form.confirm"
                label="Xác nhận mật khẩu mới"
                type="password"
                :icon="Lock"
                autocomplete="new-password"
                :placeholder="messages.reset.confirmPlaceholder"
                required
              />
              <p v-if="error" class="reset__error" role="alert">{{ error }}</p>
              <Button type="submit" size="lg" class="reset__submit" :loading="submitting" block>
                {{ messages.reset.submit }}
              </Button>
            </form>
            <RouterLink class="reset__back" :to="{ name: 'login' }">
              <ArrowLeft :size="16" aria-hidden="true" />
              {{ messages.reset.back }}
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

/* Shell — elevation bằng surface + border (KHÔNG shadow, §6) */
.reset__shell {
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
.reset__aside {
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
}

.reset__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.reset__aside-badge {
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

.reset__aside-title {
  font-size: var(--text-xl);
  font-weight: 600;
  line-height: 1.35;
  letter-spacing: -0.015em;
  margin: 0;
  color: rgba(255, 255, 255, 0.92);
}

/* Block-token strip — signature "dữ liệu được đánh số" */
.reset__aside-bench {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  margin-block: var(--space-xs);
}

.reset__aside-block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  width: 36px;
  height: 44px;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
}

.reset__aside-block--swap { background: var(--color-conflict); }
.reset__aside-block--done { background: var(--color-resolved); }
.reset__aside-block--active { box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.4); }

.reset__aside-block-value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.4;
}

.reset__aside-block-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

.reset__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin: 0;
}

.reset__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.85);
}

.reset__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
  color: rgba(255, 255, 255, 0.6);
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
}

.reset__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  margin: 0;
}

.reset__subtitle {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  margin-bottom: var(--space-sm);
}

/* Checklist mật khẩu — grid 2 cột để gọn chiều cao form */
.reset__checklist {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: var(--space-xs) var(--space-md);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  padding-inline: var(--space-xs);
  margin-bottom: var(--space-xs);
}

.reset__check {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  white-space: nowrap;
}

.reset__check-mark {
  color: var(--color-text-quaternary);
  flex-shrink: 0;
}

.reset__check--ok {
  color: var(--color-text-secondary);
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
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  font-weight: 500;
  padding-block: var(--space-sm);
  width: fit-content;
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
  border-radius: var(--radius-md);
  color: var(--color-success);
  background: color-mix(in srgb, var(--color-success) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 35%, transparent);
}

.reset__success-title {
  font-size: var(--text-lg);
  font-weight: 600;
  margin-top: var(--space-xs);
}

.reset__success-desc {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

@media (max-width: 768px) {
  .reset__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .reset__aside { padding: var(--space-lg); }
}
</style>
