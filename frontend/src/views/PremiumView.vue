<script setup lang="ts">
// PremiumView — Màn 25: 3 gói + so sánh quyền lợi + checkout QR chuyển khoản MB Bank 2 bước (GP-T7)
// Bước 1: chọn gói → Bước 2: QR VietQR EMVCo (qrcode) + nội dung CK DSV{userId}T{months} + đếm ngược 60s
// → "Tôi đã chuyển khoản" → upgradePremium + mockPayPremium kích hoạt ngay (demo, không xác minh ngân hàng).
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import QRCode from 'qrcode';

import * as gamificationApi from '@/api/gamification';
import { fireConfetti } from '@/composables/useConfetti';
import { buildVietQrPayload } from '@/lib/vietqr';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';

// ── TK nhận tiền (pm-decision-log-gp.md — ĐÃ CHỐT, KHÔNG đổi) ──
const MB_BENEFICIARY = {
  bankBin: '970422', // MB Bank
  bankNumber: '83863112088386',
  name: 'NGUYEN THI NHU HOA',
} as const;
const ACCOUNT_DISPLAY = '8386 3112 0883 86';

// ── Chống bấm nhầm: nút xác nhận chỉ khả dụng sau 60s đếm ngược ──
const COUNTDOWN_SECONDS = 60;

const gamification = useGamificationStore();
const ui = useUiStore();
const auth = useAuthStore();
const route = useRoute();
const router = useRouter();

// id = planId theo contract backend ("1m"|"3m"|"12m" — PremiumDtos.cs / API_REFERENCE §4.14)
const PLANS = [
  { id: '1m', name: '1 tháng', price: '49.000₫', amount: 49000, months: 1, highlight: false },
  { id: '3m', name: '3 tháng', price: '129.000₫', amount: 129000, months: 3, highlight: false },
  { id: '12m', name: '12 tháng', price: '399.000₫', amount: 399000, months: 12, highlight: true, badge: 'Tiết kiệm nhất' },
];

const BENEFITS = [
  { label: 'Max tim', free: '10 ❤', premium: '30 ❤' },
  { label: 'Hồi tim', free: '30 phút', premium: '10 phút' },
  { label: 'Hint token', free: 'Giới hạn', premium: '30 req/ngày + debug/optimize' },
  { label: 'Avatar + khung VIP', free: '✘', premium: '✔' },
  { label: 'CheatSheet PDF', free: '✘', premium: '✔' },
  { label: 'Benchmark nâng cao', free: 'Cơ bản', premium: 'Đầy đủ' },
];

const checkoutPlan = ref<(typeof PLANS)[number] | null>(null);
const step = ref<1 | 2>(1);
const paying = ref(false);
const success = ref(false);

const qrCanvas = ref<HTMLCanvasElement | null>(null);
const qrError = ref(false);
const countdown = ref(COUNTDOWN_SECONDS);
let countdownTimer: ReturnType<typeof setInterval> | null = null;

const userId = computed(() => auth.user?.id ?? null);

// Nội dung CK tự động DSV{userId}T{months} — user KHÔNG tự ghi (pm-decision-log-gp.md)
const transferContent = computed(() => {
  if (!checkoutPlan.value || userId.value === null) return '';
  return `DSV${userId.value}T${checkoutPlan.value.months}`;
});

const qrPayload = computed(() => {
  if (!checkoutPlan.value || !transferContent.value) return '';
  return buildVietQrPayload(MB_BENEFICIARY, checkoutPlan.value.amount, transferContent.value);
});

const countdownText = computed(() => {
  const s = Math.max(0, countdown.value);
  return `00:${String(s).padStart(2, '0')}`;
});
const confirmEnabled = computed(() => countdown.value <= 0 && !paying.value);

onMounted(() => {
  void gamification.fetchPremium();
  const planQuery = route.query.plan;
  if (typeof planQuery === 'string') {
    // Deep link theo SDD Màn 25: ?plan=1 (số tháng) hoặc ?plan=1m (planId contract)
    const found = PLANS.find((p) => p.id === planQuery || String(p.months) === planQuery);
    if (found) {
      checkoutPlan.value = found;
      step.value = 1;
    }
  }
});

onBeforeUnmount(stopCountdown);

// Vào bước 2 → render QR + chạy đếm ngược; rời bước 2 / đóng modal → dừng đếm ngược
watch([step, checkoutPlan, transferContent], () => {
  if (step.value === 2 && checkoutPlan.value && transferContent.value) {
    startCountdown();
    void nextTick(renderQr);
  } else {
    stopCountdown();
  }
});

const isPremiumActive = computed(() => gamification.isPremium);

function openCheckout(plan: (typeof PLANS)[number]): void {
  if (userId.value === null) {
    ui.showToast('Vui lòng đăng nhập để thanh toán Premium', 'error');
    return;
  }
  if (isPremiumActive.value) {
    if (!window.confirm('Gói Premium hiện tại sẽ được thay thế. Tiếp tục?')) return;
  }
  checkoutPlan.value = plan;
  step.value = 1;
  success.value = false;
}

function goToStep2(): void {
  if (userId.value === null) {
    ui.showToast('Vui lòng đăng nhập để thanh toán Premium', 'error');
    return;
  }
  step.value = 2;
}

function backToStep1(): void {
  stopCountdown();
  step.value = 1;
}

function startCountdown(): void {
  stopCountdown();
  countdown.value = COUNTDOWN_SECONDS;
  countdownTimer = setInterval(() => {
    countdown.value -= 1;
    if (countdown.value <= 0) stopCountdown();
  }, 1000);
}

function stopCountdown(): void {
  if (countdownTimer) {
    clearInterval(countdownTimer);
    countdownTimer = null;
  }
}

async function renderQr(): Promise<void> {
  qrError.value = false;
  const canvas = qrCanvas.value;
  if (!canvas || !qrPayload.value) return;
  try {
    await QRCode.toCanvas(canvas, qrPayload.value, {
      width: 208,
      margin: 2,
      errorCorrectionLevel: 'M',
    });
  } catch {
    qrError.value = true;
  }
}

async function copyContent(): Promise<void> {
  const content = transferContent.value;
  if (!content) return;
  try {
    await navigator.clipboard.writeText(content);
    ui.showToast('Đã sao chép nội dung chuyển khoản', 'success');
  } catch {
    ui.showToast('Không thể sao chép — hãy ghi tay nội dung CK', 'error');
  }
}

async function confirmPaid(): Promise<void> {
  if (!checkoutPlan.value || !confirmEnabled.value) return;
  paying.value = true;
  try {
    // Bước 1: tạo đơn checkout (OrderRef = DSV{userId}T{months}) → Bước 2: mock-pay kích hoạt ngay
    const order = await gamificationApi.upgradePremium(checkoutPlan.value.id);
    await gamificationApi.mockPayPremium(order.orderId);
    success.value = true;
    fireConfetti('success');
    await gamification.fetchPremium();
    ui.showToast('🎉 Nâng cấp Premium thành công!', 'success');
    setTimeout(() => void router.replace({ name: 'home' }), 2500);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xác nhận chuyển khoản thất bại.', 'error');
  } finally {
    paying.value = false;
  }
}
</script>

<template>
  <main class="premium container">
    <header class="premium__header">
      <h1 class="premium__title">⭐ Premium</h1>
      <p class="text-muted premium__sub">Mở khóa toàn bộ quyền lợi học tập — thanh toán qua QR chuyển khoản MB Bank (demo).</p>
    </header>

    <div class="premium__plans">
      <article
        v-for="plan in PLANS"
        :key="plan.id"
        class="premium__plan card"
        :class="{ 'premium__plan--highlight': plan.highlight }"
      >
        <Badge v-if="plan.badge" variant="warning">{{ plan.badge }}</Badge>
        <h2 class="premium__plan-name">{{ plan.name }}</h2>
        <p class="premium__plan-price">{{ plan.price }}</p>
        <p class="text-muted premium__plan-sub">{{ plan.months * 30 }} ngày học Premium</p>
        <Button :variant="plan.highlight ? 'primary' : 'secondary'" block @click="openCheckout(plan)">
          Chọn gói
        </Button>
      </article>
    </div>

    <div class="premium__compare card">
      <h2 class="premium__compare-title">Free vs Premium</h2>
      <table class="premium__table">
        <thead>
          <tr>
            <th>Quyền lợi</th>
            <th>Free</th>
            <th>Premium</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="benefit in BENEFITS" :key="benefit.label">
            <td>{{ benefit.label }}</td>
            <td class="text-muted">{{ benefit.free }}</td>
            <td class="premium__premium-col">{{ benefit.premium }}</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Checkout modal 2 bước (Màn 26) -->
    <Modal
      :open="checkoutPlan !== null"
      :title="success ? '' : 'Xác nhận đăng ký'"
      :closable="!paying"
      width="460px"
      @close="checkoutPlan = null"
    >
      <div v-if="success" class="premium__success" role="status">
        <span class="premium__success-icon">✔</span>
        <h2 class="premium__success-title">Nâng cấp thành công!</h2>
        <p class="text-muted">Chào mừng bạn đến với Premium 🎉</p>
        <Button @click="router.replace({ name: 'home' })">Vào học tiếp</Button>
      </div>

      <template v-else-if="checkoutPlan">
        <!-- Bước 1 -->
        <template v-if="step === 1">
          <h3 class="premium__checkout-name">{{ checkoutPlan.name }} — {{ checkoutPlan.price }}</h3>
          <ul class="premium__checkout-benefits">
            <li>Max tim 30 ❤ (hồi 10 phút)</li>
            <li>Hint token 30 req/ngày + debug/optimize</li>
            <li>CheatSheet PDF + khung VIP</li>
            <li>Benchmark nâng cao</li>
          </ul>
          <div class="premium__checkout-actions">
            <Button variant="ghost" @click="checkoutPlan = null">Quay lại</Button>
            <Button @click="goToStep2">Tiếp tục →</Button>
          </div>
        </template>

        <!-- Bước 2: QR chuyển khoản MB Bank + đếm ngược 60s (GP-T7) -->
        <template v-else>
          <div class="premium__qr">
            <canvas
              ref="qrCanvas"
              class="premium__qr-canvas"
              role="img"
              aria-label="Mã QR chuyển khoản MB Bank"
            ></canvas>
            <p v-if="qrError" class="premium__qr-error">Không thể tạo mã QR — vui lòng thử lại.</p>
          </div>

          <div class="premium__qr-info">
            <div class="premium__qr-row">
              <span class="premium__qr-label">Ngân hàng</span>
              <span class="premium__qr-value">MB Bank</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">Chủ tài khoản</span>
              <span class="premium__qr-value">{{ MB_BENEFICIARY.name }}</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">Số tài khoản</span>
              <span class="premium__qr-value premium__qr-value--mono">{{ ACCOUNT_DISPLAY }}</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">Số tiền</span>
              <span class="premium__qr-value">{{ checkoutPlan.price }}</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">Nội dung CK</span>
              <span class="premium__qr-value premium__qr-value--mono">{{ transferContent }}</span>
            </div>
          </div>

          <div class="premium__qr-actions">
            <Button variant="ghost" size="sm" :disabled="paying || !transferContent" @click="copyContent">
              Sao chép nội dung CK
            </Button>
          </div>

          <p class="premium__checkout-note">
            Thanh toán qua QR chuyển khoản — kích hoạt ngay sau khi xác nhận
            (demo, không xác minh ngân hàng thật).
          </p>

          <p v-if="!confirmEnabled" class="premium__countdown" role="status">
            Nút khả dụng sau <strong>{{ countdownText }}</strong>
          </p>

          <div class="premium__checkout-actions">
            <Button variant="ghost" :disabled="paying" @click="backToStep1">← Quay lại</Button>
            <Button :loading="paying" :disabled="!confirmEnabled" @click="confirmPaid">Tôi đã chuyển khoản</Button>
          </div>
        </template>
      </template>
    </Modal>
  </main>
</template>

<style scoped>
.premium {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 960px;
}

.premium__title { font-size: var(--text-2xl); }
.premium__sub { font-size: var(--text-sm); margin-top: 4px; }

.premium__plans {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-md);
}

.premium__plan {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  align-items: flex-start;
  position: relative;
}

.premium__plan--highlight {
  border-color: var(--color-primary);
  box-shadow: var(--shadow-lg);
  transform: translateY(-4px);
}

.premium__plan-name { font-size: var(--text-lg); }
.premium__plan-price { font-size: var(--text-2xl); font-weight: 800; color: var(--color-primary); }
.premium__plan-sub { font-size: var(--text-xs); }

.premium__compare { display: flex; flex-direction: column; gap: var(--space-md); }

.premium__compare-title { font-size: var(--text-lg); }

.premium__table { width: 100%; border-collapse: collapse; }

.premium__table th, .premium__table td { text-align: left; padding: var(--space-sm); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); }

.premium__table th { color: var(--color-text-muted); text-transform: uppercase; font-size: var(--text-xs); }

.premium__premium-col { color: var(--color-primary); font-weight: 700; }

.premium__success { display: flex; flex-direction: column; align-items: center; gap: var(--space-md); text-align: center; padding: var(--space-md) 0; }

.premium__success-icon {
  width: 72px;
  height: 72px;
  border-radius: 50%;
  background: var(--color-success);
  color: var(--color-on-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  font-weight: 800;
}

.premium__success-title { font-size: var(--text-xl); }

.premium__checkout-name { font-size: var(--text-md); }

.premium__checkout-benefits {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  font-size: var(--text-sm);
}

.premium__checkout-benefits li::before { content: '✔ '; color: var(--color-success); }

.premium__checkout-note {
  background: color-mix(in srgb, var(--color-info) 10%, transparent);
  border: 1px solid var(--color-info);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
}

/* GP-T7: QR chuyển khoản MB Bank */
.premium__qr {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  padding: var(--space-sm) 0;
}

.premium__qr-canvas {
  width: 208px;
  height: 208px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: 6px;
  background: #fff;
}

.premium__qr-error { color: var(--color-danger); font-size: var(--text-xs); }

.premium__qr-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
}

.premium__qr-row {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
}

.premium__qr-label { color: var(--color-text-muted); flex-shrink: 0; }

.premium__qr-value { font-weight: 600; text-align: right; word-break: break-all; }

.premium__qr-value--mono { font-family: var(--font-mono, monospace); }

.premium__qr-actions {
  display: flex;
  justify-content: center;
  margin-top: var(--space-xs);
}

.premium__countdown {
  text-align: center;
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  margin-top: var(--space-sm);
}

.premium__countdown strong {
  color: var(--color-primary);
  font-variant-numeric: tabular-nums;
}

.premium__checkout-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  margin-top: var(--space-md);
}
</style>
