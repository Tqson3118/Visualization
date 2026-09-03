<script setup lang="ts">
// PremiumView — Màn 25: 3 gói + so sánh quyền lợi + checkout QR chuyển khoản MB Bank 2 bước (GP-T7)
// Bước 1: chọn gói → Bước 2: QR VietQR EMVCo (qrcode) + nội dung CK DSV{userId}T{months} + đếm ngược 60s
// → "Tôi đã chuyển khoản" → upgradePremium + mockPayPremium kích hoạt ngay (demo, không xác minh ngân hàng).
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 + strip mono dữ liệu PLAN 1M·3M·12M,
// highlight plan = border+tint success (pattern quests__card--ready, KHÔNG gradient/shadow),
// bảng so sánh Check/X lucide (resolved/quaternary — ngôn ngữ trạng thái thuật toán),
// success = BlockToken resolved (khoảnh khắc đầu tư duy nhất + confetti). Logic QR/countdown GIỮ NGUYÊN.
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import QRCode from 'qrcode';
import { ArrowLeft, ArrowRight, Check, CheckCircle2, Copy, Crown, Info, QrCode, X } from 'lucide-vue-next';

import * as gamificationApi from '@/api/gamification';
import { fireConfetti } from '@/composables/useConfetti';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import { messages } from '@/i18n/vi';

import { buildVietQrPayload, getVietQrImageUrl } from '@/lib/vietqr';


// ── TK nhận tiền (pm-decision-log-gp.md) ──
const MB_BENEFICIARY = {
  bankBin: (import.meta.env.VITE_BANK_BIN as string) || '970422', // MB Bank
  bankNumber: (import.meta.env.VITE_BANK_NUMBER as string) || '83863112088386',
  name: (import.meta.env.VITE_BANK_NAME as string) || 'NGUYEN THI NHU HOA',
} as const;
const ACCOUNT_DISPLAY = (import.meta.env.VITE_BANK_NUMBER_DISPLAY as string) || '8386 3112 0883 86';


// ── Chống bấm nhầm: nút xác nhận chỉ khả dụng sau 60s đếm ngược (DEV/Demo = 0s) ──
const COUNTDOWN_SECONDS = import.meta.env.DEV ? 0 : Number(import.meta.env.VITE_PAYMENT_COUNTDOWN_SECONDS ?? 60);

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

// So sánh quyền lợi — free/premium = null → icon X/Check (lucide, ngữ nghĩa quaternary/resolved)
const BENEFITS = messages.premium.compareRows;

// Strip mono hero: dữ liệu tuần tự (plan id) — signature "dữ liệu luôn được đánh số"
const planStrip = computed(() => PLANS.map((p) => p.id.toUpperCase()).join(' · '));

const checkoutPlan = ref<(typeof PLANS)[number] | null>(null);
const step = ref<1 | 2>(1);
const paying = ref(false);
const success = ref(false);

const qrCanvas = ref<HTMLCanvasElement | null>(null);
const qrError = ref(false);
const countdown = ref(COUNTDOWN_SECONDS);
let countdownTimer: ReturnType<typeof setInterval> | null = null;
let pollTimer: ReturnType<typeof setInterval> | null = null;
let redirectTimer: ReturnType<typeof setTimeout> | null = null;

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

const vietQrImageUrl = computed(() => {
  if (!checkoutPlan.value || !transferContent.value) return '';
  return getVietQrImageUrl(MB_BENEFICIARY, checkoutPlan.value.amount, transferContent.value);
});


const countdownText = computed(() => {
  const s = Math.max(0, countdown.value);
  return `00:${String(s).padStart(2, '0')}`;
});
const confirmEnabled = computed(() => countdown.value <= 0 && !paying.value);

function devSkipCountdown(): void {
  countdown.value = 0;
  stopCountdown();
}

function startPollingPayment(): void {
  stopPollingPayment();
  pollTimer = setInterval(async () => {
    try {
      await gamification.fetchPremium();
      if (gamification.isPremium) {
        stopPollingPayment();
        stopCountdown();
        ui.showToast('Thanh toán thành công! Gói Premium đã được kích hoạt.', 'success');
        fireConfetti('success');
        success.value = true;
        redirectTimer = setTimeout(() => void router.replace({ name: 'home' }), 3000);
      }
    } catch {
      // Bỏ qua lỗi polling mạng tạm thời
    }
  }, 3000);
}

function stopPollingPayment(): void {
  if (pollTimer) {
    clearInterval(pollTimer);
    pollTimer = null;
  }
}

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

onBeforeUnmount(() => {
  stopCountdown();
  stopPollingPayment();
  if (redirectTimer) clearTimeout(redirectTimer);
});

// Vào bước 2 → render QR + chạy đếm ngược + bắt đầu polling thanh toán tự động; rời bước 2 / đóng modal → dừng
watch([step, checkoutPlan, transferContent], () => {
  if (step.value === 2 && checkoutPlan.value && transferContent.value) {
    startCountdown();
    startPollingPayment();
    void nextTick(renderQr);
  } else {
    stopCountdown();
    stopPollingPayment();
  }
});

const isPremiumActive = computed(() => gamification.isPremium);

const activePlanDisplayName = computed(() => {
  if (!gamification.premium) return 'Gói Pro';
  const p = (gamification.premium.plan || gamification.premium.planId || '').toLowerCase();
  if (p.includes('12m') || p.includes('yearly') || p.includes('12')) return 'Gói 12 tháng (1 năm)';
  if (p.includes('3m') || p.includes('quarterly') || p.includes('3')) return 'Gói 3 tháng';
  if (p.includes('1m') || p.includes('monthly') || p.includes('1')) return 'Gói 1 tháng';
  return gamification.premium.plan || 'Gói Pro';
});

function isCurrentPlan(plan: (typeof PLANS)[number]): boolean {
  if (!isPremiumActive.value) return false;
  const cur = (gamification.premium?.planId || gamification.premium?.plan || '').toLowerCase();
  if (plan.id.toLowerCase() === cur) return true;
  if (plan.months === 12 && (cur.includes('12') || cur.includes('year'))) return true;
  if (plan.months === 3 && (cur.includes('3') || cur.includes('quarter'))) return true;
  if (plan.months === 1 && (cur.includes('1') || cur.includes('month'))) return true;
  return false;
}

function openCheckout(plan: (typeof PLANS)[number]): void {
  if (userId.value === null) {
    ui.showToast(messages.premium.needLogin, 'error');
    return;
  }
  if (isCurrentPlan(plan)) {
    ui.showToast('Bạn hiện đang sử dụng gói này. Vui lòng chọn gói có kỳ hạn khác nếu muốn chuyển đổi!', 'warning');
    return;
  }
  // Gia hạn cộng dồn thời hạn tự động trên hệ thống — không cần confirm thay thế
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

async function copyText(text: string, successMsg: string): Promise<void> {
  if (!text) return;
  try {
    await navigator.clipboard.writeText(text);
    ui.showToast(successMsg, 'success');
  } catch {
    ui.showToast('Không thể sao chép vào bộ nhớ tạm', 'error');
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

watch(checkoutPlan, (val) => {
  if (!val) {
    if (redirectTimer) {
      clearTimeout(redirectTimer);
      redirectTimer = null;
    }
    stopCountdown();
    success.value = false;
    step.value = 1;
  }
});

async function confirmPaid(): Promise<void> {

  if (!checkoutPlan.value || !confirmEnabled.value) return;
  paying.value = true;
  try {
    // Bước 1: tạo đơn checkout (OrderRef = DSV{userId}T{months})
    const order = await gamificationApi.upgradePremium(checkoutPlan.value.id);
    
    // Bước 2: thử kích hoạt mock-pay nếu môi trường cho phép
    try {
      await gamificationApi.mockPayPremium(order.orderId);
      await gamification.fetchPremium();
      ui.showToast(messages.premium.upgraded, 'success');
      fireConfetti('success');
      success.value = true;
      redirectTimer = setTimeout(() => void router.replace({ name: 'home' }), 3000);
    } catch {
      // Khi server tắt MockPay (production), hệ thống ghi nhận đơn và đóng modal
      ui.showToast('Đã ghi nhận thông tin chuyển khoản (Mã đơn: ' + order.contentRef + '). Gói Premium sẽ được kích hoạt sau khi hệ thống đối soát.', 'info');
      checkoutPlan.value = null;
    }
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Tạo yêu cầu chuyển khoản thất bại.', 'error');
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
    <!-- Hero — surface band level-2 (không gradient, không blob) + strip mono dữ liệu -->
    <header class="premium__hero">
      <div class="premium__hero-body">
        <span class="premium__hero-icon" aria-hidden="true"><Crown :size="20" /></span>
        <div class="premium__hero-title-wrap">
          <h1 class="premium__title">{{ messages.premium.title }}</h1>
          <p class="premium__sub">{{ messages.premium.subtitle }}</p>
        </div>
        <span class="premium__hero-strip" aria-hidden="true">
          <span class="premium__strip-block" /> PLAN · {{ planStrip }}
        </span>
      </div>
    </header>

    <!-- Banner hiển thị thông tin gói Premium đang hoạt động -->
    <div
      v-if="isPremiumActive && gamification.premium"
      class="card flex items-center gap-3 p-4 border border-amber-500/30 bg-amber-500/10 mb-6 rounded-2xl shadow-lg"
    >
      <div class="p-2 rounded-xl bg-amber-500/20 text-amber-400 border border-amber-500/30 shrink-0">
        <Crown :size="20" />
      </div>
      <div class="flex-1 min-w-0">
        <p class="text-sm font-bold text-white flex items-center gap-2">
          Gói đang dùng: <span class="text-amber-400 font-extrabold">{{ activePlanDisplayName }}</span>
        </p>
        <p class="text-xs text-vdsa-muted mt-0.5">
          Hạn sử dụng: {{ gamification.premium.expiresAt
            ? new Date(gamification.premium.expiresAt).toLocaleDateString('vi-VN')
            : 'Vĩnh viễn' }}
        </p>
      </div>
      <Badge variant="warning" class="shrink-0">Đang hoạt động</Badge>
    </div>

    <div class="premium__plans">
      <article
        v-for="plan in PLANS"
        :key="plan.id"
        class="premium__plan card"
        :class="{ 'premium__plan--highlight': plan.highlight }"
      >
        <div class="premium__plan-head">
          <h3 class="premium__plan-name">{{ plan.name }}</h3>
          <Badge v-if="plan.badge" variant="warning">{{ plan.badge }}</Badge>
          <Badge
            v-if="isCurrentPlan(plan)"
            variant="success"
          >
            Đang dùng
          </Badge>
        </div>
        <p class="premium__plan-price">{{ plan.price }}</p>
        <p class="premium__plan-per">{{ messages.premium.perMonth(planPerMonth(plan)) }}</p>
        <p class="premium__plan-sub">{{ messages.premium.daysLabel(plan.months * 30) }}</p>
        <Button
          :variant="isCurrentPlan(plan) ? 'secondary' : (plan.highlight ? 'primary' : 'secondary')"
          :disabled="isCurrentPlan(plan)"
          block
          @click="openCheckout(plan)"
        >
          <template v-if="isCurrentPlan(plan)">✓ Đang sử dụng (Không thể mua trùng)</template>
          <template v-else-if="isPremiumActive">Gia hạn / Đổi sang gói này</template>
          <template v-else>{{ messages.premium.choose }}</template>
        </Button>
      </article>
    </div>

    <!-- Bảng so sánh Free vs Premium — Check/X lucide (resolved/quaternary), mobile → card-stack -->
    <section class="premium__compare card">
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
              <td :data-label="messages.premium.colFree" class="premium__table-free">
                <X v-if="benefit.free === null" :size="14" class="premium__cell-x" aria-hidden="true" />
                <template v-else>{{ benefit.free }}</template>
              </td>
              <td :data-label="messages.premium.colPremium" class="premium__table-premium">
                <Check v-if="benefit.premium === null" :size="14" class="premium__cell-check" aria-hidden="true" />
                <template v-else>{{ benefit.premium }}</template>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- Checkout modal 2 bước (Màn 26) — logic QR/countdown GIỮ NGUYÊN -->
    <Modal
      :open="checkoutPlan !== null"
      :title="success ? '' : messages.premium.checkoutTitle"
      :closable="!paying"
      width="460px"
      @close="checkoutPlan = null"
    >
      <div v-if="success" class="premium__success" role="status">
        <div class="premium__success-token">
          <BlockToken v-if="checkoutPlan" tone="resolved" label="PREMIUM" :value="checkoutPlan.name" :index="messages.premium.successTokenIndex" />
        </div>
        <h2 class="premium__success-title">{{ messages.premium.successTitle }}</h2>
        <p class="premium__success-desc">{{ messages.premium.successDesc }}</p>
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
              <CheckCircle2 :size="16" class="premium__benefit-icon" aria-hidden="true" />
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
            <div class="premium__qr-frame bg-white flex flex-col items-center justify-center p-2 rounded-xl border border-slate-200">
              <img
                v-if="vietQrImageUrl"
                :src="vietQrImageUrl"
                :alt="messages.premium.qrAria"
                class="w-full max-w-[260px] h-auto rounded-lg shadow-sm"
              />
              <canvas
                v-else
                ref="qrCanvas"
                class="premium__qr-canvas"
                role="img"
                :aria-label="messages.premium.qrAria"
              ></canvas>
            </div>
            <p v-if="qrError" class="premium__qr-error">{{ messages.premium.qrError }}</p>
            <p class="premium__qr-caption">
              <QrCode :size="14" aria-hidden="true" /> {{ checkoutPlan.name }} · {{ checkoutPlan.price }}
            </p>
          </div>

          <div class="premium__qr-info">
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.bankLabel }}</span>
              <span class="premium__qr-value">MB Bank (Ngân hàng Quân Đội)</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.ownerLabel }}</span>
              <span class="premium__qr-value font-medium">{{ MB_BENEFICIARY.name }}</span>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.accountLabel }}</span>
              <div class="flex items-center gap-2">
                <span class="premium__qr-value premium__qr-value--mono">{{ ACCOUNT_DISPLAY }}</span>
                <button type="button" class="text-xs text-primary-400 hover:text-primary-300 underline shrink-0" @click="copyText(MB_BENEFICIARY.bankNumber, 'Đã sao chép số tài khoản')">Sao chép</button>
              </div>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.amountLabel }}</span>
              <div class="flex items-center gap-2">
                <span class="premium__qr-value premium__qr-value--mono font-semibold">{{ checkoutPlan.price }}</span>
                <button type="button" class="text-xs text-primary-400 hover:text-primary-300 underline shrink-0" @click="copyText(String(checkoutPlan.amount), 'Đã sao chép số tiền')">Sao chép</button>
              </div>
            </div>
            <div class="premium__qr-row">
              <span class="premium__qr-label">{{ messages.premium.contentLabel }}</span>
              <div class="flex items-center gap-2">
                <span class="premium__qr-value premium__qr-value--mono text-amber-400 font-bold tracking-wider">{{ transferContent }}</span>
                <button type="button" class="text-xs text-primary-400 hover:text-primary-300 underline shrink-0" @click="copyContent">Sao chép</button>
              </div>
            </div>
          </div>

          <div class="premium__qr-actions">
            <Button variant="secondary" :disabled="paying || !transferContent" @click="copyContent">
              <Copy :size="14" aria-hidden="true" /> {{ messages.premium.copyContent }}
            </Button>
          </div>


          <p class="premium__checkout-note">
            <Info :size="16" class="premium__note-icon" aria-hidden="true" />
            <span>{{ messages.premium.note }}</span>
          </p>

          <p v-if="!confirmEnabled" class="premium__countdown cursor-pointer select-none" role="status" title="Bấm để bỏ qua chờ" @click="devSkipCountdown">
            {{ messages.premium.countdownLabel }} <strong class="premium__countdown-value">{{ countdownText }}</strong>
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

/* Card dùng class global .card (global.css có shadow-md) — §6 cấm shadow card → override */
.premium .card {
  box-shadow: none;
}

/* ── Hero — surface band level-2 (DESIGN.md §6) ── */
.premium__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-xl);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
}

.premium__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.premium__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.premium__hero-title-wrap { display: flex; flex-direction: column; gap: var(--space-xs); }

.premium__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.premium__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

/* Strip mono dữ liệu — dãy plan id (dữ liệu tuần tự, index mono) */
.premium__hero-strip {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-data-core) 25%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-xs) var(--space-sm);
  white-space: nowrap;
}

.premium__strip-block {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
}

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
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.premium__plan:hover { border-color: var(--color-border-strong); }

/* Gói nổi bật — phân cấp bằng border + tint success (KHÔNG gradient/shadow/scale) */
.premium__plan--highlight {
  border-color: color-mix(in srgb, var(--color-success) 45%, var(--color-border));
  background: color-mix(in srgb, var(--color-success) 5%, var(--color-card));
}

.premium__plan--highlight:hover {
  border-color: color-mix(in srgb, var(--color-success) 65%, var(--color-border));
}

.premium__plan-head { display: flex; align-items: flex-start; justify-content: space-between; gap: var(--space-sm); }

.premium__plan-name { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.01em; margin: 0; }

.premium__plan-price {
  font-family: var(--font-mono);
  font-size: var(--text-2xl);
  font-weight: 600;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  line-height: 1.2;
}

.premium__plan-per { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-text-tertiary); }

.premium__plan-sub { font-size: var(--text-xs); color: var(--color-text-muted); margin-bottom: var(--space-xs); }

.premium__plan :deep(button) { margin-top: auto; }

/* ── Compare table (DESIGN.md §4.6) ── */
.premium__compare { display: flex; flex-direction: column; gap: var(--space-md); min-width: 0; transition: none; }

.premium__compare-title { font-size: var(--text-xl); font-weight: 600; letter-spacing: -0.015em; margin: 0; }

.premium__table-wrap { overflow-x: auto; }

.premium__table { width: 100%; border-collapse: collapse; min-width: 420px; }

.premium__table th,
.premium__table td {
  text-align: left;
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
}

.premium__table th {
  height: 40px;
  font-weight: 500;
  color: var(--color-text-tertiary);
  border-bottom: 1px solid var(--color-border);
}

.premium__table td {
  border-bottom: 1px solid var(--color-border);
  color: var(--color-foreground-secondary);
}

.premium__table tbody tr { transition: background-color 150ms cubic-bezier(0.16, 1, 0.3, 1); }
.premium__table tbody tr:hover { background: var(--color-surface-hover); }
.premium__table tbody tr:last-child td { border-bottom: none; }

.premium__table-free { color: var(--color-text-tertiary); }
.premium__table-premium { color: var(--color-foreground); font-weight: 500; }

.premium__cell-check { color: var(--color-success); }
.premium__cell-x { color: var(--color-text-quaternary); }

/* ── Checkout modal ── */
.premium__success {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
  text-align: center;
  padding: var(--space-md) 0;
}

/* Khoảnh khắc đầu tư duy nhất: success block-token resolved vào nhẹ (easing chuẩn) */
.premium__success-token {
  animation: premium-success-enter 300ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes premium-success-enter {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

.premium__success-title { font-size: var(--text-xl); font-weight: 600; letter-spacing: -0.015em; }

.premium__success-desc { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 34ch; }

.premium__checkout-name { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.01em; }

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
  color: var(--color-foreground-secondary);
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

/* Nền trắng là yêu cầu CHỨC NĂNG của QR (ISO/IEC 18004) — ngoại lệ ghi pm-decision-log-viewquality */
.premium__qr-frame {
  padding: var(--space-sm);
  border-radius: var(--radius-lg);
  border: 1px solid var(--color-border-subtle);
}

.premium__qr-canvas {
  width: 208px;
  height: 208px;
  display: block;
}

.premium__qr-error { color: var(--color-destructive); font-size: var(--text-xs); }

.premium__qr-caption {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-text-tertiary);
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

.premium__qr-label { color: var(--color-text-tertiary); font-size: var(--text-xs); flex-shrink: 0; }

.premium__qr-value { font-weight: 500; text-align: right; word-break: break-all; }

.premium__qr-value--mono { font-family: var(--font-mono); font-weight: 600; font-variant-numeric: tabular-nums; }

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

.premium__countdown-value {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
}

.premium__checkout-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  margin-top: var(--space-md);
}

/* Mobile: bảng so sánh → card-stack (thead ẩn, label qua data-label) — §8 cấm scroll ngang bảng chính */
@media (max-width: 640px) {
  .premium__table { min-width: 0; }
  .premium__table thead { display: none; }
  .premium__table,
  .premium__table tbody,
  .premium__table tr,
  .premium__table td { display: block; width: 100%; }
  .premium__table tbody { display: flex; flex-direction: column; gap: var(--space-md); }
  .premium__table tr {
    border: 1px solid var(--color-border);
    border-radius: var(--radius-md);
    padding: var(--space-sm) var(--space-md);
  }
  .premium__table td {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-md);
    border-bottom: 1px solid var(--color-border);
    padding: var(--space-xs) 0;
  }
  .premium__table td:last-child { border-bottom: none; }
  .premium__table td:not(:first-child)::before {
    content: attr(data-label);
    font-size: var(--text-xs);
    font-weight: 500;
    color: var(--color-text-tertiary);
    flex-shrink: 0;
  }
}

@media (prefers-reduced-motion: reduce) {
  .premium__success-token { animation: none; }
}
</style>
