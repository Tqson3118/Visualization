<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { Clock, KeyRound, RefreshCw } from 'lucide-vue-next';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import * as authApi from '@/api/auth';
import { ApiError } from '@/api/client';

const props = defineProps<{
  open: boolean;
}>();

const emit = defineEmits<{
  close: [];
  success: [];
}>();

const auth = useAuthStore();
const ui = useUiStore();
const isDevMode = import.meta.env.DEV;

const twoFactorSending = ref(false);
const twoFactorBusy = ref(false);
const twoFactorError = ref('');
const twoFaDigits = reactive<string[]>(['', '', '', '', '', '']);
const twoFaCode = computed(() => twoFaDigits.join(''));
const twoFaExpiresSeconds = ref(300);
const twoFaResendCooldownSeconds = ref(0);

let twoFaTimerInterval: number | null = null;
let twoFaResendTimerInterval: number | null = null;

const twoFaTimeFormatted = computed(() => {
  const m = Math.floor(twoFaExpiresSeconds.value / 60);
  const s = twoFaExpiresSeconds.value % 60;
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
});

function start2FaCountdown(seconds = 300): void {
  if (twoFaTimerInterval) clearInterval(twoFaTimerInterval);
  twoFaExpiresSeconds.value = seconds;
  twoFaTimerInterval = window.setInterval(() => {
    if (twoFaExpiresSeconds.value > 0) {
      twoFaExpiresSeconds.value--;
    } else if (twoFaTimerInterval) {
      clearInterval(twoFaTimerInterval);
    }
  }, 1000);
}

function start2FaResendCooldown(seconds = 60): void {
  if (twoFaResendTimerInterval) clearInterval(twoFaResendTimerInterval);
  twoFaResendCooldownSeconds.value = seconds;
  twoFaResendTimerInterval = window.setInterval(() => {
    if (twoFaResendCooldownSeconds.value > 0) {
      twoFaResendCooldownSeconds.value--;
    } else if (twoFaResendTimerInterval) {
      clearInterval(twoFaResendTimerInterval);
    }
  }, 1000);
}

function focus2FaDigit(index: number): void {
  const input = document.getElementById(`twofa-digit-${index}`) as HTMLInputElement | null;
  input?.focus();
}

function onTwoFaDigitInput(index: number, event: Event): void {
  const target = event.target as HTMLInputElement;
  const val = target.value.replace(/\D/g, '');
  if (!val) {
    twoFaDigits[index] = '';
    return;
  }
  if (val.length > 1) {
    const chars = val.slice(0, 6).split('');
    chars.forEach((c, idx) => {
      if (index + idx < 6) twoFaDigits[index + idx] = c;
    });
    const nextIdx = Math.min(index + chars.length, 5);
    focus2FaDigit(nextIdx);
    return;
  }
  twoFaDigits[index] = val;
  if (index < 5) focus2FaDigit(index + 1);
}

function onTwoFaDigitKeyDown(index: number, event: KeyboardEvent): void {
  if (event.key === 'Backspace') {
    if (!twoFaDigits[index] && index > 0) {
      twoFaDigits[index - 1] = '';
      focus2FaDigit(index - 1);
    } else {
      twoFaDigits[index] = '';
    }
  }
}

function onTwoFaDigitPaste(event: ClipboardEvent): void {
  event.preventDefault();
  const pasted = event.clipboardData?.getData('text') || '';
  const digits = pasted.replace(/\D/g, '').slice(0, 6).split('');
  digits.forEach((d, i) => {
    twoFaDigits[i] = d;
  });
  focus2FaDigit(Math.min(digits.length, 5));
}

async function handleResend2FaOtp(): Promise<void> {
  if (twoFaResendCooldownSeconds.value > 0 || twoFactorSending.value) return;
  twoFactorSending.value = true;
  twoFactorError.value = '';
  try {
    const res = await authApi.send2FaCode();
    start2FaCountdown(res.expiresInSeconds || 300);
    start2FaResendCooldown(60);
    twoFaDigits.splice(0, 6, '', '', '', '', '', '');
    setTimeout(() => focus2FaDigit(0), 100);
  } catch (err) {
    twoFactorError.value = err instanceof Error ? err.message : 'Không thể gửi lại mã OTP.';
  } finally {
    twoFactorSending.value = false;
  }
}

async function handleVerify2Fa(): Promise<void> {
  twoFactorError.value = '';
  const code = twoFaCode.value.trim();
  if (code.length !== 6) {
    twoFactorError.value = 'Vui lòng nhập đủ 6 chữ số mã OTP.';
    return;
  }
  twoFactorBusy.value = true;
  try {
    await authApi.verify2FaCode(code);
    await auth.fetchMe();
    ui.showToast('Đã kích hoạt xác thực 2FA thành công!', 'success');
    emit('success');
    emit('close');
  } catch (err) {
    if (err instanceof ApiError) {
      twoFactorError.value = err.message;
    } else {
      twoFactorError.value = 'Mã OTP không chính xác hoặc đã hết hạn.';
    }
  } finally {
    twoFactorBusy.value = false;
  }
}
</script>

<template>
  <Modal
    :open="open"
    title="Bật xác thực hai lớp (2FA)"
    class="max-w-md"
    @close="emit('close')"
  >
    <div class="space-y-4">
      <p class="text-xs text-vdsa-secondary leading-relaxed">
        Mã xác thực gồm 6 chữ số đã được gửi tới email <strong class="text-white">{{ auth.user?.email }}</strong>. Vui lòng nhập mã để hoàn tất kích hoạt.
      </p>

      <!-- Dev Mode hint -->
      <div v-if="isDevMode" class="p-2.5 rounded-xl bg-indigo-500/10 border border-indigo-500/30 text-xs text-indigo-300 flex items-center gap-2">
        <KeyRound :size="14" class="text-indigo-400 shrink-0" />
        <span><strong>Dev mode:</strong> mã OTP mặc định <code class="bg-indigo-950/80 px-1 py-0.5 rounded text-white font-mono font-bold">123456</code></span>
      </div>

      <!-- 6 ô nhập mã OTP -->
      <div class="space-y-2">
        <label class="block text-xs font-bold text-vdsa-secondary uppercase text-center">Nhập mã OTP 6 chữ số</label>
        <div class="profile__twofa-otp-boxes" @paste="onTwoFaDigitPaste">
          <input
            v-for="(_, index) in twoFaDigits"
            :id="`twofa-digit-${index}`"
            :key="index"
            v-model="twoFaDigits[index]"
            type="text"
            inputmode="numeric"
            maxlength="1"
            autocomplete="one-time-code"
            class="profile__twofa-otp-input"
            :class="{ 'profile__twofa-otp-input--error': twoFactorError }"
            @input="onTwoFaDigitInput(index, $event)"
            @keydown="onTwoFaDigitKeyDown(index, $event)"
          />
        </div>
      </div>

      <!-- Countdown & Gửi lại mã -->
      <div class="flex items-center justify-between text-xs px-1">
        <div class="flex items-center gap-1.5 text-vdsa-muted">
          <Clock :size="14" :class="twoFaExpiresSeconds < 60 ? 'text-rose-400' : 'text-vdsa-muted'" />
          <span>Hiệu lực: <strong :class="twoFaExpiresSeconds < 60 ? 'text-rose-400' : 'text-white'" class="font-mono">{{ twoFaTimeFormatted }}</strong></span>
        </div>

        <button
          type="button"
          class="text-vdsa-accent hover:underline font-semibold disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1"
          :disabled="twoFaResendCooldownSeconds > 0 || twoFactorSending"
          @click="handleResend2FaOtp"
        >
          <RefreshCw v-if="twoFactorSending" :size="12" class="animate-spin" />
          <span v-if="twoFaResendCooldownSeconds > 0">Gửi lại mã ({{ twoFaResendCooldownSeconds }}s)</span>
          <span v-else>Gửi lại mã OTP</span>
        </button>
      </div>

      <p v-if="twoFactorError" class="text-xs text-rose-500 text-center font-medium" role="alert">
        {{ twoFactorError }}
      </p>

      <div class="flex items-center justify-end gap-2 pt-3 border-t border-vdsa-border">
        <Button variant="ghost" size="sm" :disabled="twoFactorBusy" @click="emit('close')">
          Hủy
        </Button>
        <Button
          variant="primary"
          size="sm"
          :loading="twoFactorBusy"
          :disabled="twoFaCode.length !== 6"
          @click="handleVerify2Fa"
        >
          Xác nhận & Bật 2FA
        </Button>
      </div>
    </div>
  </Modal>
</template>

<style scoped>
.profile__twofa-otp-boxes {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: clamp(4px, 1.5vw, 8px);
  margin-block: 4px;
  width: 100%;
}

.profile__twofa-otp-input {
  width: clamp(34px, 11vw, 44px);
  height: clamp(42px, 13vw, 52px);
  text-align: center;
  font-size: clamp(16px, 5vw, 22px);
  font-weight: 700;
  font-family: var(--font-mono, monospace);
  background: var(--color-surface, #161b22);
  border: 1.5px solid var(--color-border, #30363d);
  border-radius: 10px;
  color: #ffffff;
  transition: all 150ms ease;
  flex-shrink: 1;
}

.profile__twofa-otp-input:focus {
  outline: none;
  border-color: var(--color-accent, #6366f1);
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.25);
  background: var(--color-card, #0d1117);
}

.profile__twofa-otp-input--error {
  border-color: var(--color-destructive, #ef4444);
}
</style>
