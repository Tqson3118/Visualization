<script setup lang="ts">
// PremiumView — Màn 25: 3 gói + so sánh quyền lợi + checkout mô phỏng 2 bước (Màn 26)
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import * as gamificationApi from '@/api/gamification';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';

const gamification = useGamificationStore();
const ui = useUiStore();
const route = useRoute();
const router = useRouter();

const PLANS = [
  { id: 'monthly', name: '1 tháng', price: '49.000₫', months: 1, highlight: false },
  { id: 'quarterly', name: '3 tháng', price: '129.000₫', months: 3, highlight: false },
  { id: 'yearly', name: '12 tháng', price: '399.000₫', months: 12, highlight: true, badge: 'Tiết kiệm nhất' },
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

onMounted(() => {
  void gamification.fetchPremium();
  const planQuery = route.query.plan;
  if (typeof planQuery === 'string') {
    const found = PLANS.find((p) => p.id === planQuery);
    if (found) {
      checkoutPlan.value = found;
      step.value = 1;
    }
  }
});

const isPremiumActive = computed(() => gamification.isPremium);

function openCheckout(plan: (typeof PLANS)[number]): void {
  if (isPremiumActive.value) {
    if (!window.confirm('Gói Premium hiện tại sẽ được thay thế. Tiếp tục?')) return;
  }
  checkoutPlan.value = plan;
  step.value = 1;
  success.value = false;
}

async function pay(): Promise<void> {
  if (!checkoutPlan.value) return;
  paying.value = true;
  try {
    // Bước 1: tạo đơn checkout mô phỏng → Bước 2: mock-pay kích hoạt
    const order = await gamificationApi.upgradePremium(checkoutPlan.value.id);
    await gamificationApi.mockPayPremium(order.orderId);
    success.value = true;
    await gamification.fetchPremium();
    ui.showToast('🎉 Nâng cấp Premium thành công!', 'success');
    setTimeout(() => void router.replace({ name: 'home' }), 2500);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Thanh toán mô phỏng thất bại.', 'error');
  } finally {
    paying.value = false;
  }
}
</script>

<template>
  <main class="premium container">
    <header class="premium__header">
      <h1 class="premium__title">⭐ Premium</h1>
      <p class="text-muted premium__sub">Mở khóa toàn bộ quyền lợi học tập — thanh toán MÔ PHỎNG, không trừ tiền thật.</p>
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
            <Button @click="step = 2">Tiếp tục →</Button>
          </div>
        </template>

        <!-- Bước 2 -->
        <template v-else>
          <p class="premium__checkout-note">
            Thanh toán MÔ PHỎNG — <strong>không trừ tiền thật</strong>. Bấm nút bên dưới để kích hoạt Premium.
          </p>
          <div class="premium__checkout-actions">
            <Button variant="ghost" :disabled="paying" @click="step = 1">← Quay lại</Button>
            <Button :loading="paying" @click="pay">Thanh toán mô phỏng</Button>
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

.premium__checkout-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  margin-top: var(--space-md);
}
</style>
