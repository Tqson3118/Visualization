<script setup lang="ts">
// ShopView — Màn 22: lưới vật phẩm + mua (atomic chống double-spend)
import { computed, onMounted, ref } from 'vue';

import * as gamificationApi from '@/api/gamification';
import type { ShopItemDto } from '@/api/gamification';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { formatNumber } from '@/utils/format';

const gamification = useGamificationStore();
const ui = useUiStore();

const items = ref<ShopItemDto[]>([]);
const loading = ref(true);
const buyingId = ref<number | null>(null);

onMounted(async () => {
  try {
    items.value = await gamificationApi.fetchShopItems();
  } catch {
    ui.showToast('Không thể tải cửa hàng (backend chưa khả dụng).', 'error');
    items.value = [];
  } finally {
    loading.value = false;
  }
  void gamification.fetchHearts();
});

const canAfford = computed(() => (price: number) => gamification.gems >= price);

async function buy(item: ShopItemDto): Promise<void> {
  buyingId.value = item.id;
  try {
    await gamification.buyItem(item.id);
    ui.showToast(`Đã mua "${item.name}"!`, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không đủ gems hoặc đã đạt tối đa.', 'error');
  } finally {
    buyingId.value = null;
  }
}

const ICON: Record<string, string> = {
  hint: '💡',
  freeze: '❄️',
  avatar: '🖼️',
  theme: '🎨',
  frame: '🖼️',
  boost: '⚡',
};
</script>

<template>
  <main class="shop container">
    <header class="shop__header">
      <div>
        <h1 class="shop__title">🛒 Cửa hàng</h1>
        <p class="text-muted shop__sub">Giao dịch atomic — 2 tab mua cùng lúc chỉ 1 thành công</p>
      </div>
      <div class="shop__gems">
        💎 <strong>{{ formatNumber(gamification.gems) }}</strong> gems
      </div>
    </header>

    <div v-if="loading" class="shop__loading">
      <Skeleton v-for="i in 6" :key="i" height="120px" />
    </div>

    <EmptyState
      v-else-if="items.length === 0"
      icon="package"
      title="Cửa hàng trống"
      description="Vật phẩm đang được chuẩn bị — quay lại sau nhé."
    />

    <div v-else class="shop__grid">
      <article v-for="item in items" :key="item.id" class="shop__card card">
        <span class="shop__icon" aria-hidden="true">{{ ICON[item.slot ?? ''] ?? '📦' }}</span>
        <h2 class="shop__name">{{ item.name }}</h2>
        <p class="shop__desc text-muted">{{ item.description }}</p>
        <footer class="shop__foot">
          <Badge variant="warning">💎 {{ formatNumber(item.priceGems) }}</Badge>
          <Button
            size="sm"
            :disabled="!canAfford(item.priceGems)"
            :loading="buyingId === item.id"
            @click="buy(item)"
          >
            Mua
          </Button>
        </footer>
      </article>
    </div>

    <footer class="shop__footer text-muted">
      Ghi chú: equip khung/avatar/theme tại Hồ sơ (tính năng kho đồ — backlog giai đoạn sau).
      Kiếm gems bằng cách pass node, nâng sao, hoàn thành quest.
    </footer>
  </main>
</template>

<style scoped>
.shop {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.shop__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.shop__title { font-size: var(--text-2xl); }
.shop__sub { font-size: var(--text-sm); margin-top: 4px; }

.shop__gems {
  font-size: var(--text-md);
  background: color-mix(in srgb, var(--color-warning) 14%, transparent);
  border: 1px solid var(--color-warning);
  padding: 6px 14px;
  border-radius: var(--radius-full);
}

.shop__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: var(--space-md);
}

.shop__card { display: flex; flex-direction: column; gap: var(--space-sm); align-items: flex-start; }

.shop__icon { font-size: 2rem; }

.shop__name { font-size: var(--text-md); }

.shop__desc { font-size: var(--text-xs); flex: 1; }

.shop__foot {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  width: 100%;
  margin-top: auto;
}

.shop__footer { font-size: var(--text-xs); }
</style>
