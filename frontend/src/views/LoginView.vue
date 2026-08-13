<script setup lang="ts">
// LoginView — Màn 02: đăng nhập (SDD Màn 02).
// View-quality (nhóm A): aside gradient aurora/blob/glassmorphism → panel tối
// `canvas-ink` motif Data Bench (badge mono + block-token + index mono, quyết định
// xuyên-nhóm 5: vùng dữ liệu LUÔN tối); icon = lucide-vue-next; bỏ shadow trên
// shell (elevation = surface + border, §6). GIỮ nguyên logic login + redirect.
import { onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { Check, CheckCircle2, Lock, Mail, Sparkles, Target } from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { messages } from '@/i18n/vi';
import { isValidEmail } from '@/utils/validators';
import BlockToken from '@/components/ui/BlockToken.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import RevealSection from '@/components/ui/RevealSection.vue';

const auth = useAuthStore();
const route = useRoute();
const router = useRouter();

const form = reactive({
  email: '',
  password: '',
});

const submitError = ref('');
const submitting = ref(false);
const submitSuccess = ref(false);
let redirectTimer: ReturnType<typeof setTimeout> | null = null;

async function onSubmit(): Promise<void> {
  submitError.value = '';
  submitSuccess.value = false;
  if (!isValidEmail(form.email)) {
    submitError.value = messages.auth.invalidEmail;
    return;
  }
  submitting.value = true;
  try {
    await auth.login(form.email, form.password);
    // Thành công → checkmark 1 nhịp (khoảnh khắc đầu tư của màn) rồi mới điều hướng
    submitSuccess.value = true;
    submitting.value = false;
    redirectTimer = setTimeout(() => {
      const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/path';
      void router.replace(redirect);
    }, 700);
  } catch {
    submitError.value = messages.auth.loginFailed;
    submitting.value = false;
  }
}

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
  if (redirectTimer) clearTimeout(redirectTimer);
});

const BRAND_POINTS = [
  { icon: Sparkles, text: messages.auth.brandPoint1 },
  { icon: Target, text: messages.auth.brandPoint2 },
  { icon: CheckCircle2, text: messages.auth.brandPoint3 },
] as const;

/** Strip block-token trang trí (aria-hidden) — dấu vân tay Data Bench: block + index mono. */
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
  <main class="login">
    <div class="login__shell">
      <!-- Brand panel: nền tối canvas-ink + block-token (KHÔNG gradient) -->
      <aside class="login__aside" aria-label="Giới thiệu DSA Visual">
        <!-- Block-token nổi: 1 block mới mỗi 3s, vị trí ngẫu nhiên (pulse — signature §1.5) -->
        <div class="login__float-tokens" aria-hidden="true">
          <div
            v-for="t in floatTokens"
            :key="t.id"
            class="login__float-token"
            :style="{ top: t.top, left: t.left }"
          >
            <BlockToken size="sm" :value="t.value" :index="String(t.id).padStart(2, '0')" pulse />
          </div>
        </div>

        <div class="login__aside-inner">
          <span class="login__aside-badge">{{ messages.app.name }}</span>
          <h2 class="login__aside-title">{{ messages.app.tagline }}</h2>

          <div class="login__aside-bench" aria-hidden="true">
            <div
              v-for="(b, idx) in BENCH_BLOCKS"
              :key="b.value"
              class="login__aside-block"
              :class="`login__aside-block--${b.state}`"
            >
              <span class="login__aside-block-value">{{ b.value }}</span>
              <span class="login__aside-block-index">{{ String(idx).padStart(2, '0') }}</span>
            </div>
          </div>

          <div class="login__points" role="list">
            <RevealSection
              v-for="(point, idx) in BRAND_POINTS"
              :key="point.text"
              :delay="idx * 90"
            >
              <div class="login__point" role="listitem">
                <component :is="point.icon" :size="16" class="login__point-icon" aria-hidden="true" />
                <span>{{ point.text }}</span>
              </div>
            </RevealSection>
          </div>
        </div>
      </aside>

      <!-- Form đăng nhập -->
      <div class="login__form-col">
        <form class="login__card" novalidate @submit.prevent="onSubmit">
          <h1 class="login__title">{{ messages.auth.loginTitle }}</h1>
          <p class="login__subtitle">{{ messages.auth.loginSubtitle }}</p>

          <Input
            id="email"
            v-model="form.email"
            label="Email"
            :icon="Mail"
            type="email"
            autocomplete="email"
            :placeholder="messages.auth.emailPlaceholder"
            required
          />

          <Input
            id="password"
            v-model="form.password"
            label="Mật khẩu"
            :icon="Lock"
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

          <Button type="submit" size="lg" class="login__submit" :loading="submitting" block>
            <span v-if="submitSuccess" class="login__success" aria-hidden="true">
              <Check :size="16" />
            </span>
            <template v-else>{{ messages.auth.loginSubmit }}</template>
          </Button>

          <p class="login__switch">
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

/* Shell — elevation bằng surface + border (KHÔNG shadow, §6) */
.login__shell {
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
.login__aside {
  position: relative;
  overflow: hidden;
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
}

/* Block-token nổi — 1 block mới mỗi 3s, vị trí ngẫu nhiên, nhấp nháy chậm (pulse) */
.login__float-tokens {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.login__float-token {
  position: absolute;
  animation: login-token-enter 6s var(--ease-in-out) both;
}

@keyframes login-token-enter {
  0% { opacity: 0; transform: translateY(10px) scale(0.9); }
  12% { opacity: 0.85; transform: translateY(0) scale(1); }
  80% { opacity: 0.85; }
  100% { opacity: 0; transform: translateY(-8px); }
}

.login__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.login__aside-badge {
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

.login__aside-title {
  font-size: var(--text-xl);
  font-weight: 600;
  line-height: 1.35;
  letter-spacing: -0.015em;
  margin: 0;
  color: rgba(255, 255, 255, 0.92);
}

/* Block-token strip — signature "dữ liệu được đánh số" */
.login__aside-bench {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  margin-block: var(--space-xs);
}

.login__aside-block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  width: 36px;
  height: 44px;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
}

.login__aside-block--swap { background: var(--color-conflict); }
.login__aside-block--done { background: var(--color-resolved); }
.login__aside-block--active { box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.4); }

.login__aside-block-value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.4;
}

.login__aside-block-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

.login__points {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  margin: 0;
}

.login__point {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.85);
}

.login__point-icon {
  margin-top: 2px;
  flex-shrink: 0;
  color: rgba(255, 255, 255, 0.6);
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
}

.login__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  margin: 0;
}

.login__subtitle {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  margin-bottom: var(--space-sm);
}

.login__links {
  display: flex;
  justify-content: flex-end;
}

.login__forgot {
  font-size: var(--text-sm);
  font-weight: 500;
  padding-block: var(--space-sm);
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

/* Submit thành công — checkmark vào nhẹ (khoảnh khắc đầu tư của màn) */
.login__success {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  animation: login-check-pop 300ms var(--ease-out-expo) both;
}

@keyframes login-check-pop {
  0% { transform: scale(0.4); opacity: 0; }
  60% { transform: scale(1.15); }
  100% { transform: scale(1); opacity: 1; }
}

/* Input focus — border + glow ring chuyển mượt (token motion) */
.login :deep(.ui-input input) {
  transition:
    border-color var(--duration-normal) var(--ease-out-expo),
    box-shadow var(--duration-normal) var(--ease-out-expo);
}

.login :deep(.ui-input input:focus-visible) {
  border-color: var(--color-primary);
  box-shadow: var(--glow-primary);
}

.login__switch {
  text-align: center;
  margin: 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

/* <768px: ẩn aside, form full-width căn giữa (DESIGN.md §8) */
@media (max-width: 767px) {
  .login__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .login__aside { display: none; }
  .login__form-col { padding: var(--space-lg) var(--space-md); }
}

@media (min-width: 768px) and (max-width: 820px) {
  .login__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .login__aside { padding: var(--space-lg); }
}

@media (prefers-reduced-motion: reduce) {
  .login__float-tokens { display: none; }
  .login__success { animation: none; }
}
</style>
