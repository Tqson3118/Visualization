<script setup lang="ts">
// ShopView — Màn 22: lưới vật phẩm + mua (atomic chống double-spend).
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 (không gradient/blob),
// gems = 1 stat hero duy nhất (block-token tối + index mono), icon lucide đồng nhất
// (bỏ tint gradient theo slot), giá mono, không hover-lift/shadow card.
import { computed, onMounted, ref } from 'vue';
import type { Component } from 'vue';
import { Frame, Gem, Image, Lightbulb, Package, Palette, Snowflake, ShoppingBag, Zap } from 'lucide-vue-next';

import * as gamificationApi from '@/api/gamification';
import type { ShopItemDto } from '@/api/gamification';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import Modal from '@/components/ui/Modal.vue';
import { formatNumber } from '@/utils/format';
import { messages } from '@/i18n/vi';

const gamification = useGamificationStore();
const ui = useUiStore();

const items = ref<ShopItemDto[]>([]);
const loading = ref(true);
const buyingId = ref<number | null>(null);
/** Vật phẩm đang chờ xác nhận mua (confirm modal) */
const confirmItem = ref<ShopItemDto | null>(null);

onMounted(async () => {
  try {
    items.value = await gamificationApi.fetchShopItems();
  } catch {
    ui.showToast(messages.shop.loadError, 'error');
    items.value = [];
  } finally {
    loading.value = false;
  }
  // H-D P2: nạp toàn bộ gamification (hearts/streak/premium) khi vào thẳng /shop —
  // gems balance phụ thuộc GET gems (P1 contract, SETUP_TODO đợt I/J).
  void gamification.fetchAll();
});

const canAfford = computed(() => (price: number) => gamification.gems >= price);

/** Mở confirm modal trước khi mua (chống click nhầm — atomic double-spend vẫn ở backend). */
function openConfirm(item: ShopItemDto): void {
  confirmItem.value = item;
}

async function buy(item: ShopItemDto): Promise<void> {
  buyingId.value = item.id;
  try {
    await gamification.buyItem(item.id);
    ui.showToast(messages.shop.bought(item.name), 'success');
    confirmItem.value = null;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không đủ gems hoặc đã đạt tối đa.', 'error');
  } finally {
    buyingId.value = null;
  }
}

/** Confirm modal → gọi buy thật (giữ nguyên atomic chống double-spend ở store/API). */
function confirmBuy(): void {
  if (confirmItem.value) void buy(confirmItem.value);
}

// Icon lucide theo slot (đồng nhất màu — không tint gradient lung tung)
const SLOT_ICON: Record<string, Component> = {
  hint: Lightbulb,
  freeze: Snowflake,
  avatar: Image,
  theme: Palette,
  frame: Frame,
  boost: Zap,
};
const itemIcon = (slot: string | null): Component => SLOT_ICON[slot ?? ''] ?? Package;
const slotLabel = (slot: string | null): string => (slot ? (messages.shop.slot[slot as keyof typeof messages.shop.slot] ?? '') : '');
</script>

<template>
  <main class="shop container">
    <!-- Hero — surface band level-2 (không gradient, không blob) -->
    <header class="shop__hero">
      <div class="shop__hero-body">
        <span class="shop__hero-icon" aria-hidden="true"><ShoppingBag :size="20" /></span>
        <div class="shop__hero-title-wrap">
          <h1 class="shop__title">{{ messages.shop.title }}</h1>
          <p class="shop__sub">{{ messages.shop.subtitle }}</p>
        </div>
      </div>
      <div class="shop__stats">
        <BlockToken label="GEMS" :value="formatNumber(gamification.gems)" index="01 · ví" />
        <div class="shop__stat-block">
          <span class="shop__stat-label">Vật phẩm</span>
          <div class="shop__stat-line">
            <span class="shop__stat-value">{{ items.length }}</span>
            <span class="shop__stat-unit">ITEMS</span>
          </div>
        </div>
      </div>
    </header>

    <div v-if="loading" class="shop__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="150px" />
    </div>

    <EmptyState
      v-else-if="items.length === 0"
      icon="package"
      :title="messages.shop.emptyTitle"
      :description="messages.shop.emptyDesc"
    />

    <div v-else class="shop__grid">
      <article
        v-for="item in items"
        :key="item.id"
        class="shop__card card"
        :class="{ 'shop__card--unaffordable': !canAfford(item.priceGems) }"
      >
        <div class="shop__top">
          <span class="shop__icon" aria-hidden="true">
            <component :is="itemIcon(item.slot)" :size="20" />
          </span>
          <Badge v-if="slotLabel(item.slot)" variant="muted">{{ slotLabel(item.slot) }}</Badge>
        </div>
        <h2 class="shop__name">{{ item.name }}</h2>
        <p class="shop__desc">{{ item.description }}</p>
        <footer class="shop__foot">
          <span class="shop__price" aria-label="Giá">
            <Gem :size="14" aria-hidden="true" class="shop__price-gem" /> {{ formatNumber(item.priceGems) }}
          </span>
          <Button
            :disabled="!canAfford(item.priceGems)"
            :loading="buyingId === item.id"
            @click="openConfirm(item)"
          >
            {{ messages.shop.buy }}
          </Button>
        </footer>
      </article>
    </div>

    <footer class="shop__footer">{{ messages.shop.footer }}</footer>

    <!-- Confirm mua: dialog đã có shadow (§4.5 hợp lệ) — content slide-up nhẹ -->
    <Modal
      :open="confirmItem !== null"
      :title="messages.shop.title"
      @close="confirmItem = null"
    >
      <div v-if="confirmItem" class="shop__confirm">
        <p class="shop__confirm-name">{{ confirmItem.name }}</p>
        <p class="shop__confirm-desc">{{ confirmItem.description }}</p>
        <p class="shop__confirm-price">
          <Gem :size="16" aria-hidden="true" class="shop__price-gem" />
          {{ formatNumber(confirmItem.priceGems) }}
          <span class="shop__confirm-gems">{{ messages.shop.gemsLabel }}</span>
        </p>
      </div>
      <template #footer>
        <Button variant="ghost" @click="confirmItem = null">{{ messages.classes.cancel }}</Button>
        <Button
          variant="primary"
          :loading="buyingId === confirmItem?.id"
          :disabled="!confirmItem || !canAfford(confirmItem.priceGems)"
          @click="confirmBuy"
        >
          {{ messages.shop.buy }}
        </Button>
      </template>
    </Modal>
  </main>
</template>

<style scoped>
.shop {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* Card dùng class global .card (global.css có shadow-md) — §6 cấm shadow card → override */
.shop .card {
  box-shadow: none;
}

/* ── Hero — surface band level-2 (DESIGN.md §6) ── */
.shop__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-xl);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
}

.shop__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.shop__hero-icon {
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

.shop__hero-title-wrap { display: flex; flex-direction: column; gap: var(--space-xs); }

.shop__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.shop__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

/* ── Stats: 1 hero (gems — block-token tối) + 1 stat phụ level-1 ── */
.shop__stats {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border-subtle);
}

@media (min-width: 640px) {
  .shop__stats { grid-template-columns: repeat(2, 1fr); }
}

.shop__stat-block {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  padding: var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-card);
}

.shop__stat-label { font-size: var(--text-xs); color: var(--color-text-tertiary); font-weight: 500; }

.shop__stat-line { display: flex; align-items: baseline; gap: var(--space-sm); }

.shop__stat-value {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
}

.shop__stat-unit { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-text-tertiary); }

.shop__loading { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: var(--space-md); }

.shop__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: var(--space-md);
}

.shop__card {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  align-items: flex-start;
  transform: perspective(900px);
  transition:
    border-color 150ms cubic-bezier(0.16, 1, 0.3, 1),
    transform 180ms cubic-bezier(0.16, 1, 0.3, 1);
}

/* 3D tilt subtle khi hover — chỉ transform + transition (1–2°, không quá mức) */
.shop__card:hover {
  border-color: var(--color-border-strong);
  transform: perspective(900px) rotateX(1.2deg) rotateY(-1.2deg) translateY(-2px);
}

.shop__card--unaffordable { opacity: 0.72; }

.shop__top { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); width: 100%; }

.shop__icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.shop__name { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.01em; margin: 0; }

.shop__desc { font-size: var(--text-xs); flex: 1; line-height: 1.55; color: var(--color-text-muted); }

.shop__foot {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  width: 100%;
  margin-top: auto;
}

.shop__price {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-weight: 600;
  font-size: var(--text-sm);
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

/* Price tag gems — bounce nhẹ (keyframe ui-bounce-soft có sẵn trong global.css) */
.shop__price-gem {
  color: var(--color-info);
  animation: ui-bounce-soft 2.2s ease-in-out infinite;
}

/* ── Confirm mua: content slide-up + copy rõ ràng ── */
.shop__confirm {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  animation: shop-confirm-up 220ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes shop-confirm-up {
  from { opacity: 0; transform: translateY(14px); }
  to { opacity: 1; transform: translateY(0); }
}

.shop__confirm-name {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  margin: 0;
}

.shop__confirm-desc {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  margin: 0;
}

.shop__confirm-price {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-md);
  font-weight: 600;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  margin: 0;
}

.shop__confirm-gems {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  font-weight: 400;
}

.shop__footer { font-size: var(--text-xs); color: var(--color-text-muted); }

@media (prefers-reduced-motion: reduce) {
  .shop__card,
  .shop__card:hover {
    transform: none;
    transition: none;
  }
  .shop__price-gem { animation: none; }
  .shop__confirm { animation: none; }
}
</style>
