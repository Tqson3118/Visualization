<script setup lang="ts">
// ShopView — Màn 22: lưới vật phẩm + mua (atomic chống double-spend).
// Layout mới: hero (level-2) giữ nguyên → bên dưới chia 2 khu:
//   Trái = linh vật bán hàng (phi hành gia đứng trong cửa hàng + chat bubble).
//   Phải = trưng bày vật phẩm theo lưới 3 cột (3×3, ô thừa = "Sắp có mặt").
// View-quality C (DESIGN.md §1/§6): không gradient/blob trang trí, gems = 1 stat
// hero duy nhất (block-token tối + index mono), icon lucide đồng nhất, không hover-lift.
import { computed, onMounted, ref } from 'vue';
import type { Component } from 'vue';
import { Frame, Gem, Image, Lightbulb, Package, Palette, Plus, Snowflake, ShoppingBag, Zap } from 'lucide-vue-next';

import * as gamificationApi from '@/api/gamification';
import type { ShopItemDto } from '@/api/gamification';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
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
  // H-D P2: nạp toàn bộ gamification (hearts/streak/premium) khi vào thẳng /shop —
  // gems balance phụ thuộc GET gems (P1 contract, SETUP_TODO đợt I/J).
  void gamification.fetchAll();
});

const canAfford = computed(() => (price: number) => gamification.gems >= price);

// Ô trám cho lưới 3 cột (3×3) khi tổng item không chia hết cho 3.
const fillCount = computed(() => (3 - (items.value.length % 3)) % 3);

async function buy(item: ShopItemDto): Promise<void> {
  if (buyingId.value !== null) return;
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
        <span class="shop__hero-spark shop__hero-spark--1" aria-hidden="true">✦</span>
        <span class="shop__hero-spark shop__hero-spark--2" aria-hidden="true">✦</span>
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
        <span class="shop__stats-spark shop__stats-spark--a" aria-hidden="true">✦</span>
        <span class="shop__stats-spark shop__stats-spark--b" aria-hidden="true">✦</span>
      </div>
      <div class="shop__ticker" aria-hidden="true">
        <div class="shop__ticker-track">
          <template v-for="pass in 2" :key="pass">
            <span v-for="(t, i) in messages.shop.ticker" :key="`${pass}-${i}`" class="shop__ticker-item">
              <span class="shop__ticker-star">✦</span>{{ t }}
            </span>
          </template>
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

    <template v-else>
      <section class="shop__store">
        <!-- Bảng hiệu cửa hàng: chữ SHOP + mái hiên sọc + đèn dây -->
        <header class="shop__sign">
          <div class="shop__sign-row">
            <!-- Trang trí trái: gem + lấp lánh -->
            <div class="shop__sign-decor shop__sign-decor--left" aria-hidden="true">
              <svg class="shop__decor-gem" viewBox="0 0 44 44">
                <defs>
                  <linearGradient id="shop-gem-grad" x1="0" y1="0" x2="1" y2="1">
                    <stop offset="0" stop-color="#c4b5fd" />
                    <stop offset="1" stop-color="#6d28d9" />
                  </linearGradient>
                </defs>
                <path d="M22 2 L40 16 L22 42 L4 16 Z" fill="url(#shop-gem-grad)" stroke="#7c3aed" stroke-width="2" />
                <path d="M4 16 L40 16 L22 2 Z" fill="rgba(255,255,255,0.28)" />
                <path d="M22 16 L22 42 M4 16 L22 16 M40 16 L22 16" stroke="#7c3aed" stroke-width="1.5" opacity="0.55" />
              </svg>
              <span class="shop__decor-spark shop__decor-spark--1">✦</span>
              <span class="shop__decor-spark shop__decor-spark--2">✦</span>
            </div>

            <div class="shop__sign-board">
              <span class="shop__sign-stars" aria-hidden="true">✦ ✦</span>
              <h2 class="shop__sign-title">{{ messages.shop.signTitle }}</h2>
              <p class="shop__sign-sub">{{ messages.shop.signSub }}</p>
            </div>

            <!-- Trang trí phải: thẻ SALE treo dây -->
            <div class="shop__sign-decor shop__sign-decor--right" aria-hidden="true">
              <span class="shop__sale-string"></span>
              <span class="shop__sale-tag">{{ messages.shop.sale }}</span>
              <span class="shop__decor-spark shop__decor-spark--3">✦</span>
            </div>
          </div>
          <div class="shop__sign-awning" aria-hidden="true"></div>
          <div class="shop__sign-lights" aria-hidden="true">
            <span v-for="n in 13" :key="n" class="shop__sign-light"></span>
          </div>
        </header>

        <div class="shop__layout">
          <!-- Khu trái: linh vật bán hàng — phi hành gia đứng trong cửa hàng + chat -->
          <aside class="shop__mascot card" aria-label="Linh vật bán hàng">
            <p class="shop__mascot-kicker">{{ messages.shop.mascotKicker }}</p>
            <div class="shop__mascot-stage">
              <span class="shop__open-sign" aria-hidden="true">
                <span class="shop__open-dot"></span>
                {{ messages.shop.openSign }}
              </span>
              <Gem class="shop__float-gem shop__float-gem--1" :size="16" aria-hidden="true" />
              <Gem class="shop__float-gem shop__float-gem--2" :size="11" aria-hidden="true" />
              <div class="shop__mascot-bubble">
              <p class="shop__mascot-bubble-text">{{ messages.shop.mascotGreeting }}</p>
              <span class="shop__mascot-bubble-tail" aria-hidden="true"></span>
            </div>

            <svg class="shop__astronaut" viewBox="0 0 180 230" aria-hidden="true">
              <defs>
                <linearGradient id="shop-visor" x1="0" y1="0" x2="1" y2="1">
                  <stop offset="0" stop-color="var(--shop-visor-a)" />
                  <stop offset="1" stop-color="var(--shop-visor-b)" />
                </linearGradient>
              </defs>

              <!-- Ngôi sao lấp lánh trên nền vũ trụ (không bay cùng phi hành gia) -->
              <g class="shop__stars" fill="currentColor">
                <path class="shop__star--a" d="M 24 38 l 1.8 3.6 3.6 1.8 -3.6 1.8 -1.8 3.6 -1.8 -3.6 -3.6 -1.8 3.6 -1.8 z" />
                <path class="shop__star--b" d="M 150 64 l 1.6 3.2 3.2 1.6 -3.2 1.6 -1.6 3.2 -1.6 -3.2 -3.2 -1.6 3.2 -1.6 z" />
                <circle class="shop__star--c" cx="34" cy="86" r="1.9" />
                <circle class="shop__star--d" cx="140" cy="28" r="1.6" />
                <circle class="shop__star--e" cx="48" cy="16" r="1.3" />
                <circle class="shop__star--f" cx="156" cy="100" r="1.7" />
              </g>

              <!-- Phi hành gia (nhóm này bồng bềnh) -->
              <g class="shop__astronaut-figure">
                <!-- balo -->
                <rect x="64" y="82" width="52" height="66" rx="16" fill="var(--shop-suit)" opacity="0.32" stroke="var(--shop-suit-line)" stroke-width="1.5" />
                <line x1="90" y1="92" x2="90" y2="138" stroke="var(--shop-suit-line)" stroke-width="1.5" opacity="0.6" />

                <!-- túi shopping (tay trái cầm) -->
                <rect x="27" y="146" width="32" height="38" rx="6" fill="var(--shop-teal)" opacity="0.95" stroke="var(--shop-teal-dark)" stroke-width="2" />
                <line x1="27" y1="168" x2="59" y2="168" stroke="rgba(255,255,255,0.5)" stroke-width="2" opacity="0.6" />

                <!-- tay -->
                <rect x="34" y="92" width="18" height="52" rx="9" fill="var(--shop-suit)" opacity="0.9" stroke="var(--shop-suit-line)" stroke-width="1.5" />
                <line x1="34" y1="118" x2="52" y2="118" stroke="var(--shop-suit-line)" stroke-width="1.5" opacity="0.6" />
                <rect x="128" y="92" width="18" height="52" rx="9" fill="var(--shop-suit)" opacity="0.9" stroke="var(--shop-suit-line)" stroke-width="1.5" />
                <line x1="128" y1="118" x2="146" y2="118" stroke="var(--shop-suit-line)" stroke-width="1.5" opacity="0.6" />

                <!-- chân -->
                <rect x="68" y="150" width="20" height="48" rx="10" fill="var(--shop-suit)" opacity="0.85" stroke="var(--shop-suit-line)" stroke-width="1.5" />
                <line x1="68" y1="178" x2="88" y2="178" stroke="var(--shop-suit-line)" stroke-width="1.5" opacity="0.6" />
                <rect x="92" y="150" width="20" height="48" rx="10" fill="var(--shop-suit)" opacity="0.85" stroke="var(--shop-suit-line)" stroke-width="1.5" />
                <line x1="92" y1="178" x2="112" y2="178" stroke="var(--shop-suit-line)" stroke-width="1.5" opacity="0.6" />

                <!-- giày -->
                <rect x="64" y="190" width="28" height="18" rx="9" fill="var(--shop-teal)" opacity="0.92" />
                <rect x="64" y="202" width="28" height="6" rx="3" fill="var(--shop-teal-dark)" opacity="0.9" />
                <rect x="88" y="190" width="28" height="18" rx="9" fill="var(--shop-teal)" opacity="0.92" />
                <rect x="88" y="202" width="28" height="6" rx="3" fill="var(--shop-teal-dark)" opacity="0.9" />

                <!-- thân -->
                <rect x="56" y="96" width="68" height="72" rx="26" fill="var(--shop-suit)" opacity="0.96" stroke="var(--shop-suit-line)" stroke-width="1.5" />

                <!-- vai -->
                <rect x="46" y="90" width="24" height="22" rx="11" fill="var(--shop-suit)" opacity="0.92" stroke="var(--shop-suit-line)" stroke-width="1.5" />
                <rect x="110" y="90" width="24" height="22" rx="11" fill="var(--shop-suit)" opacity="0.92" stroke="var(--shop-suit-line)" stroke-width="1.5" />

                <!-- đai lưng -->
                <rect x="56" y="150" width="68" height="10" rx="5" fill="var(--shop-suit-dark)" opacity="0.9" stroke="var(--shop-suit-line)" stroke-width="1" />
                <rect x="85" y="148" width="10" height="14" rx="2" fill="var(--shop-teal)" opacity="0.95" />

                <!-- bảng điều khiển ngực -->
                <rect x="74" y="112" width="32" height="10" rx="4" fill="#0b0a12" opacity="0.9" />
                <line x1="78" y1="117" x2="92" y2="117" stroke="var(--shop-teal)" stroke-width="2" opacity="0.9" />
                <circle cx="84" cy="132" r="2.5" fill="var(--shop-teal)" />
                <circle cx="95" cy="132" r="2.5" fill="#a78bfa" />

                <!-- găng tay -->
                <circle cx="43" cy="146" r="11" fill="var(--shop-teal)" stroke="var(--shop-teal-dark)" stroke-width="2" />
                <circle cx="137" cy="146" r="11" fill="var(--shop-teal)" stroke="var(--shop-teal-dark)" stroke-width="2" />

                <!-- mũ bảo hiểm -->
                <circle cx="90" cy="56" r="36" fill="var(--shop-suit)" />
                <circle cx="90" cy="56" r="36" fill="none" stroke="var(--shop-suit-line)" stroke-width="2" />
                <path d="M 62 40 a 30 30 0 0 1 22 -17" fill="none" stroke="rgba(255,255,255,0.7)" stroke-width="3" stroke-linecap="round" />

                <!-- vòng cổ -->
                <rect x="68" y="86" width="44" height="12" rx="6" fill="var(--shop-suit-dark)" opacity="0.9" stroke="var(--shop-suit-line)" stroke-width="1.5" />

                <!-- kính -->
                <rect x="64" y="40" width="52" height="34" rx="17" fill="url(#shop-visor)" />
                <path d="M 74 52 a 15 15 0 0 1 24 -5" fill="none" stroke="rgba(255,255,255,0.9)" stroke-width="4" stroke-linecap="round" />
                <circle cx="108" cy="62" r="2" fill="rgba(255,255,255,0.85)" />

                <!-- ăng-ten -->
                <line x1="90" y1="22" x2="90" y2="9" stroke="var(--shop-suit)" stroke-width="3" stroke-linecap="round" />
                <circle cx="90" cy="7" r="5" fill="var(--shop-teal)" stroke="var(--shop-teal-dark)" stroke-width="1.5" />
                <circle cx="90" cy="7" r="1.6" fill="rgba(255,255,255,0.9)" />
              </g>

              <!-- bóng mặt sàn -->
              <ellipse class="shop__mascot-floor" cx="90" cy="214" rx="48" ry="6" fill="var(--shop-teal)" opacity="0.3" />
            </svg>
            <span class="shop__counter" aria-hidden="true"></span>
          </div>
        </aside>

        <!-- Khu phải: trưng bày vật phẩm theo lưới 3×3 -->
        <section class="shop__showcase" aria-label="Trưng bày vật phẩm">
          <header class="shop__showcase-head">
            <span class="shop__showcase-kicker">{{ messages.shop.showcaseKicker }}</span>
            <span class="shop__showcase-count">{{ items.length }} ITEMS</span>
          </header>

          <div class="shop__grid">
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
                  <Gem :size="14" aria-hidden="true" /> {{ formatNumber(item.priceGems) }}
                </span>
                <Button
                  :disabled="!canAfford(item.priceGems) || buyingId !== null"
                  :loading="buyingId === item.id"
                  @click="buy(item)"
                >
                  {{ messages.shop.buy }}
                </Button>
              </footer>
            </article>

            <!-- Ô trám cho đủ lưới 3 cột (3×3) -->
            <div v-for="i in fillCount" :key="`fill-${i}`" class="shop__card shop__card--soon card" aria-hidden="true">
              <span class="shop__soon-icon"><Plus :size="18" /></span>
              <span class="shop__soon-text">{{ messages.shop.soon }}</span>
            </div>
          </div>
          </section>
        </div>
      </section>
    </template>

    <footer class="shop__footer">{{ messages.shop.footer }}</footer>
  </main>
</template>

<style scoped>
.shop {
  /* Chủ đề tím-trắng: ghi đè gradient aurora (teal) thành tím trong phạm vi shop */
  --gradient-aurora: linear-gradient(
    135deg,
    #a78bfa 0%,
    #8b5cf6 45%,
    #6d28d9 100%
  );

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
  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-xl);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
}

.shop__hero-body { position: relative; display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.shop__hero-spark {
  position: absolute;
  color: #c4b5fd;
  font-size: var(--text-sm);
  animation: shop-blink 2.4s ease-in-out infinite;
}

.shop__hero-spark--1 { right: 8%; top: 6px; }
.shop__hero-spark--2 { right: 4%; bottom: 14px; animation-delay: 0.8s; }

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
  position: relative;
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-sm);
  margin-top: var(--space-sm);
  padding: var(--space-md);
  border: 1px solid rgba(167, 139, 250, 0.28);
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, #8b5cf6 6%, var(--color-card-raised));
}

.shop__stats-spark {
  position: absolute;
  color: #c4b5fd;
  font-size: var(--text-xs);
  animation: shop-blink 2.6s ease-in-out infinite;
}

.shop__stats-spark--a { right: 10px; top: 8px; }
.shop__stats-spark--b { left: 10px; bottom: 8px; animation-delay: 1.1s; }

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

/* ── Banner chữ chạy (ticker) — thông báo cửa hàng ── */
.shop__ticker {
  margin-top: var(--space-sm);
  overflow: hidden;
  border-radius: var(--radius-md);
  border: 1px solid rgba(167, 139, 250, 0.3);
  background: linear-gradient(90deg, #4c1d95, #6d28d9 50%, #4c1d95);
}

.shop__ticker-track {
  display: inline-flex;
  align-items: center;
  white-space: nowrap;
  animation: shop-ticker 30s linear infinite;
}

.shop__ticker-item {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 6px var(--space-lg);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.12em;
  text-transform: uppercase;
  color: #ede9fe;
}

.shop__ticker-star { color: #c4b5fd; }

@keyframes shop-ticker {
  from { transform: translateX(0); }
  to { transform: translateX(-50%); }
}

.shop__loading { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: var(--space-md); }

/* ── Cửa hàng: khung storefront ── */
.shop__store {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding: var(--space-lg);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-xl);
  background: var(--color-card-raised);
}

/* ── Bảng hiệu SHOP ── */
.shop__sign {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.shop__sign-row {
  display: flex;
  align-items: flex-end;
  justify-content: center;
  gap: var(--space-xl);
  width: 100%;
}

/* Trang trí 2 bên bảng hiệu */
.shop__sign-decor {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm);
  padding-bottom: var(--space-md);
  min-width: 52px;
}

.shop__decor-gem {
  width: 40px;
  height: 40px;
  filter: drop-shadow(0 0 8px rgba(167, 139, 250, 0.6));
  animation: shop-bob 3s ease-in-out infinite;
}

.shop__decor-spark {
  color: #c4b5fd;
  font-size: var(--text-sm);
  animation: shop-blink 2.2s ease-in-out infinite;
}

.shop__decor-spark--2 { animation-delay: 0.7s; }
.shop__decor-spark--3 { animation-delay: 1.2s; }

.shop__sale-string {
  width: 1px;
  height: 24px;
  background: rgba(167, 139, 250, 0.55);
}

.shop__sale-tag {
  position: relative;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 700;
  letter-spacing: 0.12em;
  color: #fff;
  background: linear-gradient(135deg, #a78bfa, #6d28d9);
  padding: 6px 12px;
  border-radius: var(--radius-md);
  transform: rotate(6deg);
  box-shadow: 0 0 14px rgba(139, 92, 246, 0.5);
  animation: shop-bob 3.4s ease-in-out infinite;
}

.shop__sale-tag::before {
  content: "";
  position: absolute;
  top: -5px;
  left: 50%;
  transform: translateX(-50%);
  width: 10px;
  height: 10px;
  border-radius: 50%;
  background: var(--color-card-raised);
  border: 1px solid rgba(139, 92, 246, 0.5);
}

.shop__sign-board {
  position: relative;
  width: 100%;
  max-width: 520px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
  padding: var(--space-md) var(--space-lg) var(--space-xl);
  border-radius: var(--radius-xl) var(--radius-xl) 0 0;
  border: 1px solid rgba(139, 92, 246, 0.4);
  border-bottom: none;
  background: linear-gradient(180deg, #1d1636 0%, #0b0a12 100%);
  box-shadow: 0 0 28px rgba(139, 92, 246, 0.2);
}

.shop__sign-title {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-3xl);
  font-weight: 700;
  letter-spacing: 0.28em;
  color: transparent;
  background: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  filter: drop-shadow(0 0 16px rgba(139, 92, 246, 0.45));
}

.shop__sign-stars {
  color: #c4b5fd;
  font-size: var(--text-sm);
  opacity: 0.85;
}

.shop__sign-sub {
  position: absolute;
  bottom: 10px;
  left: 0;
  right: 0;
  margin: 0;
  text-align: center;
  font-family: var(--font-mono);
  font-size: 10px;
  letter-spacing: 0.22em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  white-space: nowrap;
}

/* Mái hiên sọc — chạy dài hết chiều ngang cửa hàng (lấp 2 bên trống) */
.shop__sign-awning {
  height: 16px;
  width: 100%;
  border-radius: 0 0 var(--radius-md) var(--radius-md);
  background: repeating-linear-gradient(
    90deg,
    #a78bfa 0 22px,
    #7c3aed 22px 44px
  );
  opacity: 0.95;
}

/* Đèn dây treo — chạy dài hết chiều ngang cửa hàng */
.shop__sign-lights {
  display: flex;
  justify-content: space-evenly;
  gap: var(--space-md);
  margin-top: var(--space-sm);
  border-top: 1px solid rgba(255, 255, 255, 0.12);
  padding-top: 4px;
  width: 100%;
}

.shop__sign-light {
  width: 7px;
  height: 9px;
  border-radius: 50% 50% 55% 55%;
  background: #a78bfa;
  box-shadow: 0 0 10px 2px rgba(167, 139, 250, 0.55);
  animation: shop-blink 2.4s ease-in-out infinite;
}

.shop__sign-light:nth-child(2n) {
  background: #8b5cf6;
  box-shadow: 0 0 10px 2px rgba(139, 92, 246, 0.55);
  animation-delay: 0.4s;
}

.shop__sign-light:nth-child(3n) {
  background: #c4b5fd;
  box-shadow: 0 0 10px 2px rgba(196, 181, 253, 0.55);
  animation-delay: 0.8s;
}

@media (max-width: 640px) {
  .shop__sign-decor { display: none; }
  .shop__sign-row { gap: 0; }
}

/* ── Layout 2 khu: trái = linh vật · phải = trưng bày ── */
.shop__layout {
  display: grid;
  grid-template-columns: minmax(240px, 300px) 1fr;
  gap: var(--space-lg);
  align-items: stretch;
}

@media (max-width: 900px) {
  .shop__layout { grid-template-columns: 1fr; }
}

/* ── Khu trái: linh vật phi hành gia ── */
.shop__mascot {
  --shop-glow: rgba(139, 92, 246, 0.18);
  --shop-suit: #f5f8fc;
  --shop-suit-dark: #e6e0f5;
  --shop-suit-line: rgba(91, 33, 182, 0.28);
  --shop-visor-a: #c4b5fd;
  --shop-visor-b: #6d28d9;
  --shop-teal: #8b5cf6;
  --shop-teal-dark: #6d28d9;

  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  border-color: var(--color-border-subtle);
  background: var(--color-card-raised);
  overflow: hidden;
}

.shop__mascot-kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

/* Sân khấu vũ trụ — nền tối cố định (khớp motif "dữ liệu luôn tối" §6) */
.shop__mascot-stage {
  position: relative;
  flex: 1;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  min-height: 320px;
  padding-bottom: 52px; /* dành chỗ quầy tính tiền */
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border-subtle);
  overflow: hidden;
  background:
    radial-gradient(120% 90% at 50% 0%, var(--shop-glow) 0%, transparent 60%),
    linear-gradient(180deg, #14122b 0%, #0b0a12 100%);
}

.shop__astronaut {
  width: 170px;
  height: auto;
  color: #c4b5fd;
}

/* Bảng neon OPEN */
.shop__open-sign {
  position: absolute;
  top: var(--space-md);
  left: var(--space-md);
  z-index: 2;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 10px;
  border-radius: var(--radius-full);
  background: rgba(139, 92, 246, 0.14);
  border: 1px solid rgba(167, 139, 250, 0.5);
  color: #c4b5fd;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  letter-spacing: 0.14em;
  box-shadow: 0 0 12px rgba(139, 92, 246, 0.35);
}

.shop__open-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #a78bfa;
  box-shadow: 0 0 8px 2px rgba(167, 139, 250, 0.7);
  animation: shop-blink 1.6s ease-in-out infinite;
}

/* Gems lơ lửng quanh phi hành gia */
.shop__float-gem {
  position: absolute;
  z-index: 1;
  color: #a78bfa;
  filter: drop-shadow(0 0 6px rgba(167, 139, 250, 0.7));
  animation: shop-bob 3.2s ease-in-out infinite;
}

.shop__float-gem--1 { left: 20px; bottom: 92px; }
.shop__float-gem--2 { left: 34px; bottom: 150px; animation-delay: 1.1s; color: #c4b5fd; filter: drop-shadow(0 0 6px rgba(196, 181, 253, 0.7)); }

/* Quầy tính tiền */
.shop__counter {
  position: absolute;
  left: 0;
  right: 0;
  bottom: 0;
  height: 52px;
  background: linear-gradient(180deg, #241b45 0%, #150f2b 100%);
  border-top: 2px solid rgba(167, 139, 250, 0.55);
}

.shop__counter::before {
  content: "";
  position: absolute;
  top: 8px;
  left: 50%;
  transform: translateX(-50%);
  width: 48px;
  height: 22px;
  border-radius: 4px;
  background: #0b0a12;
  border: 1px solid rgba(167, 139, 250, 0.4);
  box-shadow: inset 0 0 8px rgba(139, 92, 246, 0.3);
}

.shop__counter::after {
  content: "";
  position: absolute;
  top: 14px;
  left: 50%;
  transform: translateX(-50%);
  width: 22px;
  height: 3px;
  border-radius: 2px;
  background: #a78bfa;
  box-shadow: 0 0 8px rgba(167, 139, 250, 0.8);
}

/* Chat bubble — nổi bên trên phi hành gia */
.shop__mascot-bubble {
  position: absolute;
  top: var(--space-lg);
  right: var(--space-lg);
  z-index: 1;
  max-width: 210px;
  padding: var(--space-sm) var(--space-md);
  background: var(--color-surface);
  color: var(--color-foreground);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
}

.shop__mascot-bubble-text {
  margin: 0;
  font-size: var(--text-sm);
  font-weight: 500;
  line-height: 1.5;
}

.shop__mascot-bubble-tail {
  position: absolute;
  left: 24px;
  bottom: -7px;
  width: 12px;
  height: 12px;
  background: inherit;
  border-right: 1px solid var(--color-border-subtle);
  border-bottom: 1px solid var(--color-border-subtle);
  transform: rotate(45deg);
}

/* ── Khu phải: trưng bày 3×3 ── */
.shop__showcase {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.shop__showcase-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: var(--space-sm);
}

.shop__showcase-kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
}

.shop__showcase-count { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-text-quaternary); }

.shop__grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-md);
}

@media (max-width: 640px) {
  .shop__grid { grid-template-columns: repeat(2, 1fr); }
}

.shop__card {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  align-items: flex-start;
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

/* Viền gian hàng — dải aurora ở mép trên mỗi ô */
.shop__card::before {
  content: "";
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  border-radius: var(--radius-lg) var(--radius-lg) 0 0;
  background: var(--gradient-aurora);
  opacity: 0.9;
}

.shop__card:hover { border-color: var(--color-border-strong); }

.shop__card--unaffordable { opacity: 0.72; }

.shop__card--soon {
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  border-style: dashed;
  border-color: var(--color-border-strong);
  opacity: 1;
}

.shop__soon-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-full);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: var(--color-muted);
  color: var(--color-text-tertiary);
}

.shop__soon-text { font-size: var(--text-xs); color: var(--color-text-tertiary); }

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

.shop__footer { font-size: var(--text-xs); color: var(--color-text-muted); }

/* ── Animation: phi hành gia bồng bềnh + sao lấp lánh ── */
@keyframes shop-bob {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-6px); }
}

@keyframes shop-blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.35; }
}

.shop__astronaut-figure {
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: shop-bob 3.2s ease-in-out infinite;
}

@keyframes shop-twinkle {
  0%, 100% { opacity: 0.25; }
  50% { opacity: 1; }
}

.shop__stars path,
.shop__stars circle {
  transform-box: fill-box;
  transform-origin: center;
  animation: shop-twinkle 3s ease-in-out infinite;
}

.shop__stars .shop__star--b { animation-delay: 0.6s; }
.shop__stars .shop__star--c { animation-delay: 1.1s; }
.shop__stars .shop__star--d { animation-delay: 0.3s; }
.shop__stars .shop__star--e { animation-delay: 1.6s; }
.shop__stars .shop__star--f { animation-delay: 0.9s; }

@media (prefers-reduced-motion: reduce) {
  .shop__astronaut-figure,
  .shop__stars path,
  .shop__stars circle,
  .shop__float-gem,
  .shop__sign-light,
  .shop__open-dot,
  .shop__decor-gem,
  .shop__sale-tag,
  .shop__decor-spark,
  .shop__hero-spark,
  .shop__stats-spark,
  .shop__ticker-track {
    animation: none;
  }
}
</style>
