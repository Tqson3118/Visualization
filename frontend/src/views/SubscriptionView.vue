<script setup lang="ts">
// SubscriptionView — Màn 27: quản lý gói Premium (trạng thái + hủy gia hạn + cảnh báo hậu quả).
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 (không gradient/blob),
// days-left = 1 hero-stat duy nhất BlockToken resolved (vùng dữ liệu LUÔN tối + index mono),
// error state riêng (gọi API trực tiếp — store fetchPremium nuốt lỗi, pattern ShopView),
// ngày hết hạn mono; khoảnh khắc đầu tư duy nhất: hero-stat enter 300ms easing chuẩn.
// Logic cancel GIỮ NGUYÊN (H-D).
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { CalendarDays, CheckCircle2, CreditCard, RefreshCw, XCircle } from 'lucide-vue-next';

import * as gamificationApi from '@/api/gamification';
import type { PremiumStatusDto } from '@/api/gamification';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import { messages } from '@/i18n/vi';

const ui = useUiStore();
const router = useRouter();

const loading = ref(true);
const error = ref(false);
const status = ref<PremiumStatusDto | null>(null);
const confirmOpen = ref(false);
const cancelling = ref(false);

const BENEFITS = messages.subscription.benefits;
const LOSES = messages.subscription.loses;

async function load(): Promise<void> {
  loading.value = true;
  error.value = false;
  try {
    status.value = await gamificationApi.fetchPremiumStatus();
  } catch {
    error.value = true;
  } finally {
    loading.value = false;
  }
}

onMounted(() => void load());

/** Số ngày còn lại tới hết hạn (UI-only — không đổi store). */
const daysLeft = computed<number | null>(() => {
  const exp = status.value?.expiresAt;
  if (!exp) return null;
  const ms = new Date(exp).getTime() - Date.now();
  if (Number.isNaN(ms)) return null;
  return Math.max(0, Math.ceil(ms / 86_400_000));
});

const isPremiumActive = computed<boolean>(() => {
  if (!status.value) return false;
  return status.value.isPremium === true || status.value.status === 'active';
});


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
  <section class="subscription container">
    <!-- Hero — surface band level-2 (không gradient, không blob) + hero-stat days-left -->
    <header class="subscription__hero">
      <div class="subscription__hero-body">
        <span class="subscription__hero-icon" aria-hidden="true"><CreditCard :size="20" /></span>
        <div class="subscription__hero-title-wrap">
          <h1 class="subscription__title">{{ messages.subscription.title }}</h1>
          <p class="subscription__sub">{{ messages.subscription.subtitle }}</p>
        </div>
      </div>
      <div v-if="!loading && !error && daysLeft !== null" class="subscription__hero-stat" role="status">
        <BlockToken tone="resolved" label="CÒN LẠI" :value="daysLeft" index="ngày" />
      </div>
    </header>

    <div v-if="loading" class="subscription__loading" aria-busy="true">
      <Skeleton v-for="i in 4" :key="i" height="64px" />
    </div>

    <EmptyState
      v-else-if="error"
      icon="alert-circle"
      :title="messages.subscription.errorTitle"
      :description="messages.subscription.errorDesc"
      :action-label="messages.subscription.retry"
      @action="load"
    />

    <EmptyState
      v-else-if="!isPremiumActive || !status"
      icon="sparkles"
      :title="messages.subscription.emptyTitle"
      :description="messages.subscription.emptyDesc"
      :action-label="messages.subscription.emptyAction"
      @action="router.push({ name: 'premium' })"
    />

    <template v-else>
      <div class="subscription__status card">
        <div class="subscription__status-head">
          <h2 class="subscription__status-name">{{ status.plan ?? 'Premium' }}</h2>
          <Badge variant="success">{{ messages.subscription.activeBadge }}</Badge>
        </div>

        <dl class="subscription__status-list">
          <div class="subscription__status-row">
            <dt class="subscription__status-label">
              <CalendarDays :size="14" aria-hidden="true" /> {{ messages.subscription.expiresLabel }}
            </dt>
            <dd class="subscription__status-value subscription__status-value--mono">
              {{ status.expiresAt ? formatDate(status.expiresAt) : messages.subscription.expiresNone }}
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
      </div>

      <div class="subscription__benefits card">
        <h3 class="subscription__benefits-title">{{ messages.subscription.benefitsTitle }}</h3>
        <ul class="subscription__benefits-list">
          <li v-for="benefit in BENEFITS" :key="benefit">
            <CheckCircle2 :size="16" class="subscription__benefit-icon" aria-hidden="true" />
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
  </section>
</template>

<style scoped>
.subscription {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 680px;
}

/* Card dùng class global .card (global.css có shadow-md) — §6 cấm shadow card → override */
.subscription .card {
  box-shadow: none;
}

/* ── Hero — surface band level-2 (DESIGN.md §6) ── */
.subscription__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-xl);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
}

.subscription__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.subscription__hero-icon {
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

.subscription__hero-title-wrap { display: flex; flex-direction: column; gap: var(--space-xs); }

.subscription__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.subscription__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

/* Khoảnh khắc đầu tư duy nhất: hero-stat (days-left block-token) vào nhẹ — easing chuẩn */
.subscription__hero-stat {
  align-self: flex-start;
  animation: subscription-hero-enter 300ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes subscription-hero-enter {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}

.subscription__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.subscription__status { display: flex; flex-direction: column; gap: var(--space-md); transition: none; }

.subscription__status-head { display: flex; align-items: center; gap: var(--space-sm); }

.subscription__status-name { font-size: var(--text-xl); font-weight: 600; letter-spacing: -0.015em; margin: 0; }

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
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-tertiary);
  margin: 0;
}

.subscription__status-value {
  display: inline-flex;
  align-items: center;
  font-size: var(--text-sm);
  font-weight: 600;
  margin: 0;
}

.subscription__status-value--mono { font-family: var(--font-mono); font-variant-numeric: tabular-nums; }

.subscription__benefits { display: flex; flex-direction: column; gap: var(--space-sm); transition: none; }

.subscription__benefits-title { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.01em; margin: 0; }

.subscription__benefits-list {
  list-style: none;
  margin: 0;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.subscription__benefits-list li { display: flex; align-items: center; gap: var(--space-sm); color: var(--color-foreground-secondary); }

.subscription__benefit-icon { color: var(--color-success); flex-shrink: 0; }

.subscription__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.subscription__confirm { display: flex; flex-direction: column; gap: var(--space-sm); font-size: var(--text-sm); }

.subscription__confirm-note { line-height: 1.6; color: var(--color-foreground-secondary); margin: 0; }

.subscription__confirm-loss {
  list-style: none;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  color: var(--color-destructive);
}

.subscription__confirm-loss li { display: flex; align-items: center; gap: var(--space-sm); }

.subscription__loss-icon { flex-shrink: 0; }

@media (prefers-reduced-motion: reduce) {
  .subscription__hero-stat { animation: none; }
}
</style>
