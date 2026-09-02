<script setup lang="ts">
// LoginView — Màn 02: đăng nhập (SDD Màn 02).
// View-quality (nhóm A): aside gradient aurora/blob/glassmorphism → panel tối
// `canvas-ink` motif Data Bench (badge mono + block-token + index mono, quyết định
// xuyên-nhóm 5: vùng dữ liệu LUÔN tối); icon = lucide-vue-next; bỏ shadow trên
// shell (elevation = surface + border, §6). GIỮ nguyên logic login + redirect.
import { onBeforeUnmount, reactive, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { ArrowLeft, CheckCircle2, KeyRound, Lock, Mail, RefreshCw, Shield, Sparkles, Target } from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import * as authApi from '@/api/auth';
import { ApiError } from '@/api/client';
import { messages } from '@/i18n/vi';
import { isValidEmail } from '@/utils/validators';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const auth = useAuthStore();
const route = useRoute();
const router = useRouter();

const form = reactive({
  email: '',
  password: '',
});

const isTwoFactorStep = ref(false);
const twoFactorToken = ref('');
const twoFactorMessage = ref('');
const otpCode = ref('');
const resending = ref(false);
const resendCountdown = ref(60);
let resendTimer: ReturnType<typeof setInterval> | null = null;

function startResendCountdown(): void {
  stopResendCountdown();
  resendCountdown.value = 60;
  resendTimer = setInterval(() => {
    resendCountdown.value -= 1;
    if (resendCountdown.value <= 0) {
      stopResendCountdown();
    }
  }, 1000);
}

function stopResendCountdown(): void {
  if (resendTimer) {
    clearInterval(resendTimer);
    resendTimer = null;
  }
}

onBeforeUnmount(() => {
  stopResendCountdown();
});

const submitError = ref('');
const submitting = ref(false);

async function finishRedirect(): Promise<void> {
  if (auth.user?.role === 'TEACHER_PENDING') {
    await router.replace('/pending-teacher');
    return;
  }
  let redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/path';
  if (!redirect || redirect.startsWith('/login') || redirect.startsWith('/register')) {
    redirect = '/path';
  }
  await router.replace(redirect);
}

async function onSubmit(): Promise<void> {
  submitError.value = '';
  if (!isValidEmail(form.email)) {
    submitError.value = messages.auth.invalidEmail;
    return;
  }
  submitting.value = true;
  try {
    const res = await auth.login(form.email, form.password);
    if (res.requiresTwoFactor && res.twoFactorToken) {
      twoFactorToken.value = res.twoFactorToken;
      twoFactorMessage.value = res.message || 'Mã xác thực 2FA đã được gửi đến email của bạn.';
      isTwoFactorStep.value = true;
      otpCode.value = '';
      startResendCountdown();
      return;
    }
    await finishRedirect();
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

async function onVerify2Fa(): Promise<void> {
  submitError.value = '';
  const code = otpCode.value.trim();
  if (code.length !== 6 || !/^\d{6}$/.test(code)) {
    submitError.value = 'Vui lòng nhập đầy đủ mã OTP 6 chữ số';
    return;
  }
  submitting.value = true;
  try {
    await auth.login2Fa(twoFactorToken.value, code);
    await finishRedirect();
  } catch (err) {
    if (err instanceof ApiError) {
      submitError.value = err.message;
    } else {
      submitError.value = 'Mã xác thực không đúng hoặc đã hết hạn';
    }
  } finally {
    submitting.value = false;
  }
}

async function onResend2Fa(): Promise<void> {
  if (resendCountdown.value > 0 || resending.value) return;
  resending.value = true;
  submitError.value = '';
  try {
    const res = await authApi.resendLogin2Fa({ twoFactorToken: twoFactorToken.value });
    twoFactorMessage.value = res.message || 'Mã xác thực mới đã được gửi qua email.';
    startResendCountdown();
  } catch (err) {
    if (err instanceof ApiError) {
      submitError.value = err.message;
    } else {
      submitError.value = 'Không thể gửi lại mã xác thực. Vui lòng thử lại sau.';
    }
  } finally {
    resending.value = false;
  }
}

function backToLogin(): void {
  isTwoFactorStep.value = false;
  submitError.value = '';
  otpCode.value = '';
  stopResendCountdown();
}

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

          <ul class="login__points">
            <li v-for="point in BRAND_POINTS" :key="point.text" class="login__point">
              <component :is="point.icon" :size="16" class="login__point-icon" aria-hidden="true" />
              <span>{{ point.text }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- Form đăng nhập / Form 2FA -->
      <div class="login__form-col">
        <!-- Form 2FA OTP -->
        <form v-if="isTwoFactorStep" class="login__card" novalidate @submit.prevent="onVerify2Fa">
          <div class="flex items-center gap-2 mb-1">
            <span class="p-2 rounded-lg bg-primary/10 text-primary inline-flex">
              <Shield :size="22" />
            </span>
            <div>
              <h1 class="login__title text-2xl font-bold">Xác thực hai lớp</h1>
              <p class="text-xs text-muted-foreground">Bảo vệ tài khoản với mã OTP 6 số</p>
            </div>
          </div>

          <p class="login__subtitle text-xs text-muted-foreground mt-2">
            {{ twoFactorMessage }}
          </p>

          <Input
            id="otpCode"
            v-model="otpCode"
            label="Mã xác thực OTP (6 chữ số)"
            :icon="KeyRound"
            type="text"
            inputmode="numeric"
            :maxlength="6"
            placeholder="VD: 123456"
            required
            autocomplete="one-time-code"
          />

          <p v-if="submitError" class="login__error" role="alert">{{ submitError }}</p>

          <Button type="submit" size="lg" class="login__submit" :loading="submitting" block>
            Xác nhận đăng nhập
          </Button>

          <div class="flex items-center justify-between mt-2 pt-2 border-t border-border/40 text-xs">
            <Button
              type="button"
              variant="ghost"
              size="sm"
              :disabled="resendCountdown > 0 || resending"
              @click="onResend2Fa"
            >
              <RefreshCw :size="13" class="mr-1" :class="{ 'animate-spin': resending }" />
              {{ resendCountdown > 0 ? `Gửi lại mã (${resendCountdown}s)` : 'Gửi lại mã' }}
            </Button>

            <button
              type="button"
              class="text-xs text-muted-foreground hover:text-foreground inline-flex items-center gap-1 cursor-pointer bg-transparent border-0"
              @click="backToLogin"
            >
              <ArrowLeft :size="13" /> Quay lại đăng nhập
            </button>
          </div>
        </form>

        <!-- Form Đăng nhập thường -->
        <form v-else class="login__card" novalidate @submit.prevent="onSubmit">
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
            {{ messages.auth.loginSubmit }}
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
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  padding: clamp(1.5rem, 4vw, 2.5rem);
  display: flex;
  align-items: center;
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

.login__switch {
  text-align: center;
  margin: 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

@media (max-width: 768px) {
  .login__shell { grid-template-columns: 1fr; max-width: 28rem; }
  .login__aside { padding: var(--space-lg); }
}
</style>
