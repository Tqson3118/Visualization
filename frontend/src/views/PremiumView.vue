<script setup lang="ts">
// PremiumView — Màn 25: 3 gói + so sánh quyền lợi + checkout QR chuyển khoản MB Bank 2 bước (GP-T7)
// Bước 1: chọn gói → Bước 2: QR VietQR EMVCo (qrcode) + nội dung CK DSV{userId}T{months} + đếm ngược 60s
// → "Tôi đã chuyển khoản" → upgradePremium + mockPayPremium kích hoạt ngay (demo, không xác minh ngân hàng).
// H-D: chỉ polish UI (hero Aurora, plan cards, modal) — KHÔNG đổi logic QR/countdown/kích hoạt.
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import QRCode from 'qrcode';
import { ArrowLeft, ArrowRight, CheckCircle2, Copy, Crown, Info, QrCode, ShieldCheck } from 'lucide-vue-next';

import * as gamificationApi from '@/api/gamification';
import { fireConfetti } from '@/composables/useConfetti';
import { buildVietQrPayload } from '@/lib/vietqr';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';
import { messages } from '@/i18n/vi';

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
    ui.showToast(messages.premium.needLogin, 'error');
    return;
  }
  if (isPremiumActive.value) {
    if (!window.confirm(messages.premium.confirmReplace)) return;
  }
  checkoutPlan.value = plan;
  step.value = 1;
  success.value = false;
}

function goToStep2(): void {
  if (userId.value === null) {
    ui.showToast(messages.premium.needLogin, 'error');
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
    ui.showToast(messages.premium.copied, 'success');
  } catch {
    ui.showToast(messages.premium.copyFailed, 'error');
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
    ui.showToast(messages.premium.upgraded, 'success');
    setTimeout(() => void router.replace({ name: 'home' }), 2500);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xác nhận chuyển khoản thất bại.', 'error');
  } finally {
    paying.value = false;
  }
}

// Giá quy đổi theo tháng (tính từ price/amount đã chốt — hiển thị tham khảo)
const planPerMonth = (plan: (typeof PLANS)[number]): string =>
  Math.round(plan.amount / plan.months).toLocaleString('vi-VN');
</script>

<template>
  <main class="premium container">
    <!-- Hero gradient Aurora (palette gamification) -->
    <header class="premium__hero">
      <div class="premium__hero-body">
        <span class="premium__hero-icon" aria-hidden="true"><Crown :size="24" /></span>
        <div class="premium__hero-title-wrap">
          <h1 class="premium__title">{{ messages.premium.title }}</h1>
          <p class="premium__sub">{{ messages.premium.subtitle }}</p>
        </div>
        <Badge variant="primary" class="premium__hero-badge">
          <ShieldCheck :size="12" aria-hidden="true" /> {{ messages.premium.badge }}
        </Badge>
      </div>
    </header>

    <div class="premium__plans">
      <article
        v-for="plan in PLANS"
        :key="plan.id"
        class="premium__plan card hover-lift"
        :class="{ 'premium__plan--highlight': plan.highlight }"
      >
        <div class="premium__plan-head">
          <h2 class="premium__plan-name">{{ plan.name }}</h2>
          <Badge v-if="plan.badge" variant="warning">{{ plan.badge }}</Badge>
        </div>
        <p class="premium__plan-price">{{ plan.price }}</p>
        <p class="premium__plan-per text-muted">{{ messages.premium.perMonth(planPerMonth(plan)) }}</p>
        <p class="text-muted premium__plan-sub">{{ messages.premium.daysLabel(plan.months * 30) }}</p>
        <Button :variant="plan.highlight ? 'primary' : 'secondary'" block @click="openCheckout(plan)">
          {{ messages.premium.choose }}
        </Button>
      </article>
    </div>

    <div class="premium__compare card">
      <h2 class="premium__compare-title">{{ messages.premium.compareTitle }}</h2>
      <div class="premium__table-wrap">
        <table class="premium__table">
          <thead>
            <tr>
              <th scope="col">{{ messages.premium.colBenefit }}</th>
              <th scope="col">{{ messages.premium.colFree }}</th>
              <th scope="col">{{ messages.premium.colPremium }}</th>
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
    </div>

    <!-- Checkout modal 2 bước (Màn 26) — logic QR/countdown GIỮ NGUYÊN -->
    <Modal
      :open="checkoutPlan !== null"
      :title="success ? '' : messages.premium.checkoutTitle"
      :closable="!paying"
      width="460px"
      @close="checkoutPlan = null"
    >
      <div v-if="success" class="premium__success" role="status">
        <span class="premium__success-icon">✔</span>
        <h2 class="premium__success-title">{{ messages.premium.successTitle }}</h2>
        <p class="text-muted">{{ messages.premium.successDesc }}</p>
        <Button @click="router.replace({ name: 'home' })">{{ messages.premium.successGo }}</Button>
      </div>

      <template v-else-if="checkoutPlan">
        <!-- Bước 1 -->
        <template v-if="step === 1">
          <h3 class="premium__checkout-name">
            {{ messages.premium.checkoutName(checkoutPlan.name, checkoutPlan.price) }}
          </h3>
          <ul class="premium__checkout-benefits">
            <li v-for="benefit in messages.premium.checkoutBenefits" :key="benefit">
              <CheckCircle2 :size="15" class="premium__benefit-icon" aria-hidden="true" />
              {{ benefit }}
            </li>
          </ul>
          <div class="premium__checkout-actions">
            <Button variant="ghost" @click="checkoutPlan = null">{{ messages.premium.back }}</Button>
            <Button @click="goToStep2">
              {{ messages.premium.continue }} <ArrowRight :size="14" aria-hidden="true" />
            </Button>
          </div>
        </template>

        <!-- Bước 2: QR chuyển khoản MB Bank + đếm ngược 60s (GP-T7) -->
        <template v-else>
          <div class="premium__qr">
            <div class="premium__qr-frame">
              <canvas
                ref="qrCanvas"
                class="premium__qr-canvas"
                role="img"
                :aria-label="messages.premium.qrAria"
              ></canvas>
            </div>
            <p v-if="qrError" class="premium__qr-error">{{ messages.premium.qrError }}</p>
            <p class="premium__qr-caption">
              <QrCode :size="13" aria-hidden="true" /> {{ checkoutPlan.name }} · {{ checkoutPlan.price }}
            </p>
          </div>

          <div class="premium__qr-info">
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.bankLabel }}</span>
              <span class="premium__qr-value">MB Bank</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.ownerLabel }}</span>
              <span class="premium__qr-value">{{ MB_BENEFICIARY.name }}</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.accountLabel }}</span>
              <span class="premium__qr-value premium__qr-value--mono">{{ ACCOUNT_DISPLAY }}</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.amountLabel }}</span>
              <span class="premium__qr-value">{{ checkoutPlan.price }}</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.contentLabel }}</span>
              <span class="premium__qr-value premium__qr-value--mono">{{ transferContent }}</span>
            </div>
          </div>

          <div class="premium__qr-actions">
            <Button variant="ghost" size="sm" :disabled="paying || !transferContent" @click="copyContent">
              <Copy :size="14" aria-hidden="true" /> {{ messages.premium.copyContent }}
            </Button>
          </div>

          <p class="premium__checkout-note">
            <Info :size="14" class="premium__note-icon" aria-hidden="true" />
            <span>{{ messages.premium.note }}</span>
          </p>

          <p v-if="!confirmEnabled" class="premium__countdown" role="status">
            {{ messages.premium.countdownLabel }} <strong>{{ countdownText }}</strong>
          </p>

          <div class="premium__checkout-actions">
            <Button variant="ghost" :disabled="paying" @click="backToStep1">
              <ArrowLeft :size="14" aria-hidden="true" /> {{ messages.premium.back }}
            </Button>
            <Button :loading="paying" :disabled="!confirmEnabled" @click="confirmPaid">
              {{ messages.premium.confirmPaid }}
            </Button>
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

/* ── Hero gradient Aurora (palette 1 — gamification) ── */
.premium__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.premium__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.premium__hero::before {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  top: -120px;
  right: -60px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.premium__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.premium__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.premium__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.premium__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.premium__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.premium__hero-badge { margin-left: auto; }

/* ── Plans ── */
.premium__plans {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-md);
}

.premium__plan {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  align-items: stretch;
  position: relative;
}

.premium__plan--highlight {
  border-color: var(--color-primary);
  border-width: 2px;
  box-shadow: var(--shadow-lg);
  background-image: linear-gradient(180deg, var(--aurora-soft), var(--color-card));
  transform: translateY(-6px) scale(1.02);
}

.premium__plan--highlight:hover { transform: translateY(-8px) scale(1.03); }

.premium__plan-head { display: flex; align-items: flex-start; justify-content: space-between; gap: var(--space-sm); }

.premium__plan-name { font-size: var(--text-lg); }

.premium__plan-price {
  font-size: var(--text-2xl);
  font-weight: 800;
  color: var(--color-primary);
  font-variant-numeric: tabular-nums;
  line-height: 1.2;
}

.premium__plan-per { font-size: var(--text-xs); font-weight: 600; }

.premium__plan-sub { font-size: var(--text-xs); margin-bottom: var(--space-xs); }

.premium__plan :deep(button) { margin-top: auto; }

/* ── Compare table ── */
.premium__compare { display: flex; flex-direction: column; gap: var(--space-md); min-width: 0; }

.premium__compare-title { font-size: var(--text-lg); }

.premium__table-wrap { overflow-x: auto; }

.premium__table { width: 100%; border-collapse: collapse; min-width: 420px; }

.premium__table th, .premium__table td {
  text-align: left;
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  font-size: var(--text-sm);
}

.premium__table th {
  color: var(--color-text-muted);
  text-transform: uppercase;
  font-size: var(--text-xs);
  letter-spacing: 0.04em;
}

.premium__table tbody tr { transition: background-color 150ms ease; }
.premium__table tbody tr:hover { background: var(--color-surface-hover); }
.premium__table tbody tr:last-child td { border-bottom: none; }

.premium__premium-col { color: var(--color-primary); font-weight: 700; }

/* ── Checkout modal ── */
.premium__success {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
  text-align: center;
  padding: var(--space-md) 0;
}

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
  box-shadow: var(--shadow-md);
}

.premium__success-title { font-size: var(--text-xl); }

.premium__checkout-name { font-size: var(--text-md); }

.premium__checkout-benefits {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  margin-top: var(--space-sm);
}

.premium__checkout-benefits li {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.premium__benefit-icon { color: var(--color-success); flex-shrink: 0; }

.premium__checkout-note {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  background: color-mix(in srgb, var(--color-info) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-info) 40%, var(--color-border));
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
}

.premium__note-icon { flex-shrink: 0; margin-top: 2px; color: var(--color-info); }

/* GP-T7: QR chuyển khoản MB Bank */
.premium__qr {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  padding: var(--space-sm) 0;
}

.premium__qr-frame {
  padding: 8px;
  border-radius: var(--radius-lg);
  background: #fff;
  border: 1px solid color-mix(in srgb, var(--color-primary) 35%, var(--color-border));
  box-shadow: 0 0 0 4px color-mix(in srgb, var(--color-primary) 12%, transparent), var(--shadow-md);
}

.premium__qr-canvas {
  width: 208px;
  height: 208px;
  display: block;
}

.premium__qr-error { color: var(--color-danger); font-size: var(--text-xs); }

.premium__qr-caption {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-text-muted);
}

.premium__qr-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  background: var(--color-surface-hover);
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
  font-variant-numeric: tabular-nums;
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

@media (max-width: 640px) {
  .premium__hero-badge { margin-left: 0; }
}
</style>
