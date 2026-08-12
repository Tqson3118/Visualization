<script setup lang="ts">
// SubscriptionView — Màn 27: quản lý gói Premium (trạng thái + hủy gia hạn + cảnh báo hậu quả)
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

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

async function cancelRenewal(): Promise<void> {
  cancelling.value = true;
  try {
    // Backend chưa có endpoint hủy riêng — ghi chú & đóng modal
    ui.showToast('Hủy gia hạn: endpoint /me/subscription/cancel sẽ được bổ sung ở backend. Gói vẫn dùng tới hết hạn.', 'info');
    confirmOpen.value = false;
  } finally {
    cancelling.value = false;
  }
}
</script>

<template>
  <main class="subscription container">
    <header class="subscription__header">
      <h1 class="subscription__title">💎 Quản lý gói Premium</h1>
    </header>

    <div v-if="loading" class="subscription__loading">
      <Skeleton v-for="i in 4" :key="i" height="64px" />
    </div>

    <EmptyState
      v-else-if="!premium?.isPremium"
      icon="sparkles"
      title="Bạn chưa có gói Premium"
      description="Nâng cấp để mở khóa toàn bộ quyền lợi học tập."
      action-label="Xem bảng giá"
      @action="router.push({ name: 'premium' })"
    />

    <template v-else>
      <div class="subscription__status card">
        <div class="subscription__status-head">
          <h2 class="subscription__status-name">{{ premium.plan ?? 'Premium' }}</h2>
          <Badge variant="warning">Premium</Badge>
        </div>
        <p class="subscription__status-detail">
          Ngày hết hạn: <strong>{{ premium.expiresAt ? formatDate(premium.expiresAt) : '—' }}</strong>
        </p>
        <p class="subscription__status-detail text-muted">Gia hạn tự động: BẬT (mô phỏng)</p>
      </div>

      <div class="subscription__benefits card">
        <h3 class="subscription__benefits-title">Quyền lợi đang kích hoạt</h3>
        <ul class="subscription__benefits-list">
          <li v-for="benefit in BENEFITS" :key="benefit">✔ {{ benefit }}</li>
        </ul>
      </div>

      <div class="subscription__actions">
        <Button variant="secondary" @click="router.push({ name: 'premium' })">Gia hạn / đổi gói</Button>
        <Button variant="danger" @click="confirmOpen = true">Hủy gia hạn</Button>
      </div>
    </template>

    <Modal :open="confirmOpen" title="Hủy gia hạn Premium?" @close="confirmOpen = false">
      <div class="subscription__confirm">
        <p class="subscription__confirm-note">
          Bạn sẽ <strong>GIỮ</strong> gems, avatar, vật phẩm Shop đã mua — nhưng <strong>MẤT</strong>:
        </p>
        <ul class="subscription__confirm-loss">
          <li>Tim 30 ❤ (về 10 ❤, clamp khi hết hạn)</li>
          <li>Hồi tim 10 phút (về 30 phút)</li>
          <li>Hint token 30 req/ngày</li>
          <li>Khung VIP + CheatSheet PDF</li>
        </ul>
      </div>
      <template #footer>
        <Button variant="secondary" @click="confirmOpen = false">Giữ Premium</Button>
        <Button variant="danger" :loading="cancelling" @click="cancelRenewal">Xác nhận hủy</Button>
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
  max-width: 640px;
}

.subscription__title { font-size: var(--text-2xl); }

.subscription__status { display: flex; flex-direction: column; gap: var(--space-sm); }

.subscription__status-head { display: flex; align-items: center; gap: var(--space-sm); }

.subscription__status-name { font-size: var(--text-lg); }

.subscription__status-detail { font-size: var(--text-sm); }

.subscription__benefits-title { font-size: var(--text-md); margin-bottom: var(--space-sm); }

.subscription__benefits-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  font-size: var(--text-sm);
}

.subscription__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.subscription__confirm { display: flex; flex-direction: column; gap: var(--space-sm); font-size: var(--text-sm); }

.subscription__confirm-loss {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 4px;
  color: var(--color-destructive);
}
</style>
