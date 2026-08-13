<script setup lang="ts">
// ShopView — Màn 22: lưới vật phẩm + mua (atomic chống double-spend).
// H-D: hero gradient Aurora + gem counter, card shadcn + hover-lift,
// icon lucide theo slot (tint aurora/mint/sunset), i18n shop.*.
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
import { formatNumber } from '@/utils/format';
import { messages } from '@/i18n/vi';

const gamification = useGamificationStore();
const ui = useUiStore();

const items = ref<ShopItemDto[]>([]);
const loading = ref(true);
const buyingId = ref<number | null>(null);

onMounted(async () => {
  try {
    items.value = await gamificationApi.fetchShopItems();
  } catch {
    ui.showToast(messages.shop.loadError, 'error');
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
    ui.showToast(messages.shop.bought(item.name), 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không đủ gems hoặc đã đạt tối đa.', 'error');
  } finally {
    buyingId.value = null;
  }
}

// Icon lucide + tint gradient theo slot vật phẩm (fallback Package/mint)
const SLOT_ICON: Record<string, Component> = {
  hint: Lightbulb,
  freeze: Snowflake,
  avatar: Image,
  theme: Palette,
  frame: Frame,
  boost: Zap,
};
const SLOT_TINT: Record<string, 'aurora' | 'mint' | 'sunset'> = {
  hint: 'mint',
  freeze: 'aurora',
  avatar: 'sunset',
  theme: 'aurora',
  frame: 'sunset',
  boost: 'sunset',
};
const itemIcon = (slot: string | null): Component => SLOT_ICON[slot ?? ''] ?? Package;
const itemTint = (slot: string | null): 'aurora' | 'mint' | 'sunset' => SLOT_TINT[slot ?? ''] ?? 'mint';
const slotLabel = (slot: string | null): string => (slot ? (messages.shop.slot[slot as keyof typeof messages.shop.slot] ?? '') : '');
</script>

<template>
  <main class="shop container">
    <!-- Hero gradient Aurora (palette gamification) -->
    <header class="shop__hero">
      <div class="shop__hero-body">
        <span class="shop__hero-icon" aria-hidden="true"><ShoppingBag :size="24" /></span>
        <div class="shop__hero-title-wrap">
          <h1 class="shop__title">{{ messages.shop.title }}</h1>
          <p class="shop__sub">{{ messages.shop.subtitle }}</p>
        </div>
        <Badge variant="primary" class="shop__hero-badge">{{ messages.shop.badge }}</Badge>
      </div>
      <div class="shop__stats">
        <div class="shop__stat-block">
          <span class="shop__stat-label">{{ messages.shop.gemsLabel }}</span>
          <span class="shop__stat-value">
            <Gem :size="16" aria-hidden="true" /> {{ formatNumber(gamification.gems) }}
          </span>
        </div>
        <div class="shop__stat-block">
          <span class="shop__stat-label">Vật phẩm</span>
          <span class="shop__stat-value shop__stat-value--sm">{{ items.length }}</span>
        </div>      </div>
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
        class="shop__card card hover-lift"
        :class="{ 'shop__card--unaffordable': !canAfford(item.priceGems) }"
      >
        <div class="shop__top">
          <span class="shop__icon" :class="`shop__icon--${itemTint(item.slot)}`" aria-hidden="true">
            <component :is="itemIcon(item.slot)" :size="20" />
          </span>
          <Badge v-if="slotLabel(item.slot)" variant="muted">{{ slotLabel(item.slot) }}</Badge>
        </div>
        <h2 class="shop__name">{{ item.name }}</h2>
        <p class="shop__desc text-muted">{{ item.description }}</p>
        <footer class="shop__foot">
          <span class="shop__price text-amber-700 dark:text-amber-400" aria-label="Giá">
            <Gem :size="14" aria-hidden="true" /> {{ formatNumber(item.priceGems) }}
          </span>
          <Button
            size="sm"
            :disabled="!canAfford(item.priceGems)"
            :loading="buyingId === item.id"
            @click="buy(item)"
          >
            {{ messages.shop.buy }}
          </Button>
        </footer>
      </article>
    </div>

    <footer class="shop__footer text-muted">{{ messages.shop.footer }}</footer>
  </main>
</template>

<style scoped>
.shop {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Hero gradient Aurora (palette 1 — gamification) ── */
.shop__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.shop__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.shop__hero::before {
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

.shop__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.shop__hero-icon {
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

.shop__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.shop__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.shop__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.shop__hero-badge { margin-left: auto; }

.shop__stats {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px dashed var(--color-border);
}

.shop__stat-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-surface) 55%, transparent);
}

.shop__stat-label { font-size: var(--text-xs); color: var(--color-text-muted); }

.shop__stat-value {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-lg);
  font-weight: 800;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
}

.shop__stat-value--sm { font-size: var(--text-xs); font-weight: 600; }

.shop__loading { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: var(--space-md); }

.shop__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: var(--space-md);
}

.shop__card { display: flex; flex-direction: column; gap: var(--space-sm); align-items: flex-start; }

.shop__card--unaffordable { opacity: 0.72; }

.shop__top { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); width: 100%; }

.shop__icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: #fff;
  box-shadow: var(--shadow-sm);
}

.shop__icon--aurora { background-image: var(--gradient-aurora); }
.shop__icon--mint { background-image: var(--gradient-mint); }
.shop__icon--sunset { background-image: var(--gradient-sunset); }

.shop__name { font-size: var(--text-md); }

.shop__desc { font-size: var(--text-xs); flex: 1; line-height: 1.55; }

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
  gap: 4px;
  font-weight: 800;
  font-size: var(--text-sm);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}
.shop__footer { font-size: var(--text-xs); }

@media (max-width: 640px) {
  .shop__hero-badge { margin-left: 0; }
}
</style>
