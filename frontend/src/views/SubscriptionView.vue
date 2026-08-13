<script setup lang="ts">
// SubscriptionView — Màn 27: quản lý gói Premium (trạng thái + hủy gia hạn + cảnh báo hậu quả).
// H-D: hero gradient Aurora + status card (days-left chip), benefits grid,
// modal cảnh báo mất quyền lợi, i18n subscription.* — logic cancel GIỮ NGUYÊN.
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { CalendarDays, CheckCircle2, CreditCard, RefreshCw, XCircle } from 'lucide-vue-next';

import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { messages } from '@/i18n/vi';

const gamification = useGamificationStore();
const ui = useUiStore();
const router = useRouter();

const loading = ref(true);
const confirmOpen = ref(false);
const cancelling = ref(false);

const BENEFITS = [
  'Max tim 30 ❤ (hồi 10 phút)',
  'Hint token 30 req/ngày + debug/optimize',
  'Avatar upload + khung VIP',
  'CheatSheet PDF',
  'Benchmark nâng cao',
];

onMounted(async () => {
  try {
    await gamification.fetchPremium();
  } finally {
    loading.value = false;
  }
});

const premium = computed(() => gamification.premium);

/** Số ngày còn lại tới hết hạn (UI-only — không đổi store). */
const daysLeft = computed<number | null>(() => {
  const exp = premium.value?.expiresAt;
  if (!exp) return null;
  const ms = new Date(exp).getTime() - Date.now();
  if (Number.isNaN(ms)) return null;
  return Math.max(0, Math.ceil(ms / 86_400_000));
});

const LOSES = [
  'Tim 30 ❤ (về 10 ❤, clamp khi hết hạn)',
  'Hồi tim 10 phút (về 30 phút)',
  'Hint token 30 req/ngày',
  'Khung VIP + CheatSheet PDF',
];

async function cancelRenewal(): Promise<void> {
  cancelling.value = true;
  try {
    // Backend chưa có endpoint hủy riêng — ghi chú & đóng modal
    ui.showToast(messages.subscription.cancelInfo, 'info');
    confirmOpen.value = false;
  } finally {
    cancelling.value = false;
  }
}
</script>

<template>
  <main class="subscription container">
    <!-- Hero gradient Aurora (palette gamification) -->
    <header class="subscription__hero">
      <div class="subscription__hero-body">
        <span class="subscription__hero-icon" aria-hidden="true"><CreditCard :size="24" /></span>
        <div class="subscription__hero-title-wrap">
          <h1 class="subscription__title">{{ messages.subscription.title }}</h1>
          <p class="subscription__sub">{{ messages.subscription.subtitle }}</p>
        </div>
        <Badge variant="primary" class="subscription__hero-badge">{{ messages.subscription.badge }}</Badge>
      </div>
    </header>

    <div v-if="loading" class="subscription__loading" aria-busy="true">
      <Skeleton v-for="i in 4" :key="i" height="64px" />
    </div>

    <EmptyState
      v-else-if="!premium?.isPremium"
      icon="sparkles"
      :title="messages.subscription.emptyTitle"
      :description="messages.subscription.emptyDesc"
      :action-label="messages.subscription.emptyAction"
      @action="router.push({ name: 'premium' })"
    />

    <template v-else>
      <div class="subscription__status card">
        <div class="subscription__status-head">
          <h2 class="subscription__status-name">{{ premium.plan ?? 'Premium' }}</h2>
          <Badge variant="warning">{{ messages.subscription.badge }}</Badge>
        </div>

        <dl class="subscription__status-list">
          <div class="subscription__status-row">
            <dt class="subscription__status-label">
              <CalendarDays :size="14" aria-hidden="true" /> {{ messages.subscription.expiresLabel }}
            </dt>
            <dd class="subscription__status-value">
              {{ premium.expiresAt ? formatDate(premium.expiresAt) : messages.subscription.expiresNone }}
            </dd>
          </div>
          <div class="subscription__status-row">
            <dt class="subscription__status-label">
              <RefreshCw :size="14" aria-hidden="true" /> {{ messages.subscription.renewLabel }}
            </dt>
            <dd class="subscription__status-value">
              <Badge variant="success">{{ messages.subscription.renewOn }}</Badge>
            </dd>
          </div>
        </dl>

        <p v-if="daysLeft !== null" class="subscription__days-left" role="status">
          {{ messages.subscription.daysLeft(daysLeft) }}
        </p>
      </div>

      <div class="subscription__benefits card">
        <h3 class="subscription__benefits-title">{{ messages.subscription.benefitsTitle }}</h3>
        <ul class="subscription__benefits-list">
          <li v-for="benefit in BENEFITS" :key="benefit">
            <CheckCircle2 :size="15" class="subscription__benefit-icon" aria-hidden="true" />
            {{ benefit }}
          </li>
        </ul>
      </div>

      <div class="subscription__actions">
        <Button variant="secondary" @click="router.push({ name: 'premium' })">
          {{ messages.subscription.renewBtn }}
        </Button>
        <Button variant="danger" @click="confirmOpen = true">
          {{ messages.subscription.cancelBtn }}
        </Button>
      </div>
    </template>

    <Modal :open="confirmOpen" :title="messages.subscription.cancelTitle" @close="confirmOpen = false">
      <div class="subscription__confirm">
        <p class="subscription__confirm-note">{{ messages.subscription.cancelNote }}</p>
        <ul class="subscription__confirm-loss">
          <li v-for="loss in LOSES" :key="loss">
            <XCircle :size="14" class="subscription__loss-icon" aria-hidden="true" />
            {{ loss }}
          </li>
        </ul>
      </div>
      <template #footer>
        <Button variant="secondary" @click="confirmOpen = false">{{ messages.subscription.keepBtn }}</Button>
        <Button variant="danger" :loading="cancelling" @click="cancelRenewal">
          {{ messages.subscription.confirmCancel }}
        </Button>
      </template>
    </Modal>
  </main>
</template>

<style scoped>
.subscription {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 680px;
}

/* ── Hero gradient Aurora (palette 1 — gamification) ── */
.subscription__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.subscription__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.subscription__hero::before {
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

.subscription__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.subscription__hero-icon {
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

.subscription__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.subscription__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.subscription__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.subscription__hero-badge { margin-left: auto; }

.subscription__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.subscription__status { display: flex; flex-direction: column; gap: var(--space-md); }

.subscription__status-head { display: flex; align-items: center; gap: var(--space-sm); }

.subscription__status-name { font-size: var(--text-lg); }

.subscription__status-list {
  list-style: none;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.subscription__status-row { display: flex; align-items: center; justify-content: space-between; gap: var(--space-md); }

.subscription__status-label {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.subscription__status-value { font-size: var(--text-sm); font-weight: 700; }

.subscription__days-left {
  display: inline-flex;
  align-self: flex-start;
  align-items: center;
  font-size: var(--text-sm);
  font-weight: 800;
  color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-primary) 40%, var(--color-border));
  border-radius: var(--radius-full);
  padding: 4px 14px;
  font-variant-numeric: tabular-nums;
}

.subscription__benefits-title { font-size: var(--text-md); margin-bottom: var(--space-sm); }

.subscription__benefits-list {
  list-style: none;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.subscription__benefits-list li { display: flex; align-items: center; gap: var(--space-sm); }

.subscription__benefit-icon { color: var(--color-success); flex-shrink: 0; }

.subscription__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.subscription__confirm { display: flex; flex-direction: column; gap: var(--space-sm); font-size: var(--text-sm); }

.subscription__confirm-note { line-height: 1.6; }

.subscription__confirm-loss {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 6px;
  color: var(--color-destructive);
}

.subscription__confirm-loss li { display: flex; align-items: center; gap: var(--space-sm); }

.subscription__loss-icon { flex-shrink: 0; }

@media (max-width: 640px) {
  .subscription__hero-badge { margin-left: 0; }
}
</style>
