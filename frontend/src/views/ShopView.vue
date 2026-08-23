<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import type { Component } from 'vue';
import {
  Frame,
  Gem,
  Image,
  Lightbulb,
  Package,
  Palette,
  Plus,
  Snowflake,
  ShoppingBag,
  Zap,
  Check,
  Sparkles,
  ArrowRight,
  Shield,
  HelpCircle,
} from 'lucide-vue-next';

import * as gamificationApi from '@/api/gamification';
import type { ShopItemDto, InventoryItemDto } from '@/api/gamification';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import Tabs from '@/components/ui/Tabs.vue';
import { formatNumber } from '@/utils/format';
import { messages } from '@/i18n/vi';

const gamification = useGamificationStore();
const ui = useUiStore();

const items = ref<ShopItemDto[]>([]);
const loading = ref(true);
const buyingId = ref<number | null>(null);
const equippingId = ref<number | null>(null);
const activeCategory = ref<'all' | 'support' | 'avatar' | 'frame' | 'boost'>('all');

onMounted(async () => {
  try {
    const [fetchedItems] = await Promise.all([
      gamificationApi.fetchShopItems(),
      gamification.fetchAll(),
      gamification.fetchInventory(),
    ]);
    items.value = fetchedItems;
  } catch {
    ui.showToast(messages.shop.loadError, 'error');
    items.value = [];
  } finally {
    loading.value = false;
  }
});

const canAfford = computed(() => (price: number) => gamification.gems >= price);

// Trạng thái sở hữu & trang bị trong kho đồ
function getInventoryItem(item: ShopItemDto): InventoryItemDto | undefined {
  return gamification.inventory.find((i) => i.itemId === item.id);
}

function isItemOwned(item: ShopItemDto): boolean {
  const inv = getInventoryItem(item);
  return Boolean(inv && inv.quantity > 0);
}

function isItemEquipped(item: ShopItemDto): boolean {
  const inv = getInventoryItem(item);
  return Boolean(inv?.isEquipped);
}

function ownedQuantity(item: ShopItemDto): number {
  const inv = getInventoryItem(item);
  return inv ? inv.quantity : 0;
}

function isCosmeticItem(item: ShopItemDto): boolean {
  return item.slot === 'avatar' || item.slot === 'frame';
}

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

async function toggleEquipFromShop(item: ShopItemDto): Promise<void> {
  const inv = getInventoryItem(item);
  if (!inv) return;
  equippingId.value = item.id;
  try {
    await gamification.equipItem(item.id, !inv.isEquipped);
    ui.showToast(inv.isEquipped ? 'Đã gỡ trang bị.' : `Đã trang bị "${item.name}".`, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể trang bị vật phẩm.', 'error');
  } finally {
    equippingId.value = null;
  }
}

// Ô trám cho lưới 3 cột (3×3) khi tổng item không chia hết cho 3.
const fillCount = computed(() => {
  const len = items.value.length;
  if (len === 0) return 0;
  return (3 - (len % 3)) % 3;
});

// Icon lucide theo slot
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

        <RouterLink :to="{ path: '/profile', query: { tab: 'inventory' } }" class="shop__backpack-btn">
          <Package :size="16" />
          <span>Túi đồ của tôi ({{ gamification.inventory.length }})</span>
          <ArrowRight :size="14" />
        </RouterLink>
      </div>
    </header>

    <div v-if="loading" class="shop__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="150px" />
    </div>

    <EmptyState
      v-else-if="items.length === 0"
      icon="package"
      title="Cửa hàng đang chuẩn bị vật phẩm mới"
      description="Hãy quay lại sau để mua sắm vật phẩm nhé!"
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

                <!-- Ngôi sao lấp lánh trên nền vũ trụ -->
                <g class="shop__stars" fill="currentColor">
                  <path class="shop__star--a" d="M 24 38 l 1.8 3.6 3.6 1.8 -3.6 1.8 -1.8 3.6 -1.8 -3.6 -3.6 -1.8 3.6 -1.8 z" />
                  <path class="shop__star--b" d="M 150 64 l 1.6 3.2 3.2 1.6 -3.2 1.6 -1.6 3.2 -1.6 -3.2 -3.2 -1.6 3.2 -1.6 z" />
                  <circle class="shop__star--c" cx="34" cy="86" r="1.9" />
                  <circle class="shop__star--d" cx="140" cy="28" r="1.6" />
                  <circle class="shop__star--e" cx="48" cy="16" r="1.3" />
                  <circle class="shop__star--f" cx="156" cy="100" r="1.7" />
                </g>

                <!-- Phi hành gia -->
                <g class="shop__astronaut-figure">
                  <!-- balo -->
                  <rect x="64" y="82" width="52" height="66" rx="16" fill="var(--shop-suit)" opacity="0.32" stroke="var(--shop-suit-line)" stroke-width="1.5" />
                  <line x1="90" y1="92" x2="90" y2="138" stroke="var(--shop-suit-line)" stroke-width="1.5" opacity="0.6" />

                  <!-- túi shopping -->
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

            <!-- Mini Tip kiếm gems -->
            <div class="shop__tip-card">
              <h4 class="shop__tip-title flex items-center gap-1.5 text-xs font-bold text-vdsa-purple">
                <Sparkles :size="13" /> Cách kiếm thêm Gems?
              </h4>
              <p class="text-[11px] text-vdsa-muted mt-1 leading-relaxed">
                Hoàn thành Nhiệm vụ hàng ngày, làm bài tập đạt 100% điểm hoặc tích lũy chuỗi streak để nhận Gems miễn phí!
              </p>
              <RouterLink to="/quests" class="shop__tip-link">
                Đến trang Nhiệm Vụ <ArrowRight :size="12" />
              </RouterLink>
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
                :class="{
                  'shop__card--unaffordable': !isItemOwned(item) && !canAfford(item.priceGems),
                  'shop__card--equipped': isItemEquipped(item),
                  'shop__card--owned': isItemOwned(item) && isCosmeticItem(item),
                }"
              >
                <div class="shop__top">
                  <span class="shop__icon" aria-hidden="true">
                    <component :is="itemIcon(item.slot)" :size="20" />
                  </span>
                  <div class="flex items-center gap-1.5">
                    <Badge v-if="isItemEquipped(item)" variant="success" class="text-[10px]">✨ Đang dùng</Badge>
                    <Badge v-else-if="isItemOwned(item) && isCosmeticItem(item)" variant="primary" class="text-[10px]">✓ Đã sở hữu</Badge>
                    <Badge v-if="slotLabel(item.slot)" variant="muted">{{ slotLabel(item.slot) }}</Badge>
                  </div>
                </div>

                <h2 class="shop__name">{{ item.name }}</h2>
                <p class="shop__desc">{{ item.description }}</p>

                <!-- Số lượng trong túi nếu là vật phẩm tiêu hao -->
                <div v-if="!isCosmeticItem(item) && ownedQuantity(item) > 0" class="shop__owned-qty">
                  <span>Đang có trong túi: <strong>x{{ ownedQuantity(item) }}</strong></span>
                </div>

                <footer class="shop__foot">
                  <!-- Case 1: Cosmetic item already owned -> Quick Equip / Unequip -->
                  <template v-if="isCosmeticItem(item) && isItemOwned(item)">
                    <Button
                      size="sm"
                      :variant="isItemEquipped(item) ? 'secondary' : 'primary'"
                      :loading="equippingId === item.id"
                      class="w-full"
                      @click="toggleEquipFromShop(item)"
                    >
                      <Check v-if="isItemEquipped(item)" :size="14" class="mr-1" />
                      {{ isItemEquipped(item) ? 'Gỡ trang bị' : 'Trang bị ngay' }}
                    </Button>
                  </template>

                  <!-- Case 2: Standard Buy Action -->
                  <template v-else>
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
                  </template>
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

.shop .card {
  box-shadow: none;
}

/* ── Hero — surface band level-2 ── */
.shop__hero {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
  overflow: hidden;
}

.shop__hero-body {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  position: relative;
}

.shop__hero-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, #8b5cf6 15%, transparent);
  color: #a78bfa;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.shop__hero-title-wrap {
  display: flex;
  flex-direction: column;
  gap: var(--space-2xs);
}

.shop__title {
  font-size: var(--text-xl);
  font-weight: 700;
  margin: 0;
  color: var(--color-text-primary);
}

.shop__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  margin: 0;
}

.shop__stats {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.shop__stat-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--space-xs) var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
}

.shop__stat-label {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  letter-spacing: 0.05em;
}

.shop__stat-line {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.shop__stat-value {
  font-size: var(--text-lg);
  font-weight: 700;
  color: var(--color-text-primary);
  font-family: var(--font-mono);
}

.shop__stat-unit {
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-tertiary);
}

.shop__backpack-btn {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: color-mix(in srgb, #8b5cf6 15%, transparent);
  border: 1px solid color-mix(in srgb, #8b5cf6 30%, transparent);
  border-radius: var(--radius-md);
  color: #c4b5fd;
  font-size: var(--text-xs);
  font-weight: 600;
  text-decoration: none;
  transition: all 180ms ease;
}

.shop__backpack-btn:hover {
  background: color-mix(in srgb, #8b5cf6 25%, transparent);
  border-color: #a78bfa;
  color: #ffffff;
}

/* ── Store Layout ── */
.shop__store {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.shop__sign {
  position: relative;
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
  text-align: center;
  overflow: hidden;
}

.shop__sign-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  position: relative;
}

.shop__sign-board {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.shop__sign-stars {
  color: #fbbf24;
  font-size: 12px;
  letter-spacing: 4px;
}

.shop__sign-title {
  font-size: var(--text-2xl);
  font-weight: 800;
  color: var(--color-text-primary);
  margin: 0;
  letter-spacing: 0.05em;
}

.shop__sign-sub {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  margin: 0;
}

.shop__decor-gem {
  width: 36px;
  height: 36px;
}

.shop__sale-tag {
  background: #ef4444;
  color: #ffffff;
  font-size: 10px;
  font-weight: 800;
  padding: 4px 8px;
  border-radius: 4px;
  letter-spacing: 0.1em;
}

.shop__layout {
  display: grid;
  grid-template-columns: 260px 1fr;
  gap: var(--space-lg);
  align-items: start;
}

@media (max-width: 860px) {
  .shop__layout {
    grid-template-columns: 1fr;
  }
}

/* ── Mascot Stage ── */
.shop__mascot {
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.shop__mascot-kicker {
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  color: #a78bfa;
  letter-spacing: 0.05em;
  margin: 0;
}

.shop__mascot-stage {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding-block: var(--space-sm);
}

.shop__mascot-bubble {
  position: relative;
  background: var(--color-surface, #1e1b4b);
  border: 1px solid color-mix(in srgb, #8b5cf6 30%, transparent);
  border-radius: 12px;
  padding: 8px 12px;
  margin-bottom: 12px;
  text-align: center;
  max-width: 200px;
}

.shop__mascot-bubble-text {
  font-size: 12px;
  color: #e0e7ff;
  margin: 0;
  line-height: 1.4;
}

.shop__mascot-bubble-tail {
  position: absolute;
  bottom: -6px;
  left: 50%;
  transform: translateX(-50%);
  width: 10px;
  height: 6px;
  background: var(--color-surface, #1e1b4b);
  clip-path: polygon(0 0, 100% 0, 50% 100%);
}

.shop__astronaut {
  width: 140px;
  height: auto;
  --shop-suit: #e2e8f0;
  --shop-suit-line: #94a3b8;
  --shop-suit-dark: #64748b;
  --shop-teal: #8b5cf6;
  --shop-teal-dark: #6d28d9;
  --shop-visor-a: #c4b5fd;
  --shop-visor-b: #6d28d9;
}

.shop__open-sign {
  position: absolute;
  top: 0;
  right: 0;
  font-size: 10px;
  font-weight: 700;
  color: #10b981;
  background: rgba(16, 185, 129, 0.12);
  padding: 2px 8px;
  border-radius: 999px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.shop__open-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: #10b981;
}

.shop__float-gem {
  position: absolute;
  color: #a78bfa;
}
.shop__float-gem--1 { top: 30px; left: 10px; animation: float 3s ease-in-out infinite; }
.shop__float-gem--2 { top: 60px; right: 15px; animation: float 4s ease-in-out infinite reverse; }

@keyframes float {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-6px); }
}

.shop__tip-card {
  margin-top: var(--space-xs);
  padding: var(--space-sm);
  background: color-mix(in srgb, #8b5cf6 8%, transparent);
  border: 1px dashed color-mix(in srgb, #8b5cf6 30%, transparent);
  border-radius: 10px;
}

.shop__tip-link {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  font-weight: 700;
  color: #a78bfa;
  margin-top: 8px;
  text-decoration: none;
}
.shop__tip-link:hover { text-decoration: underline; color: #ffffff; }

/* ── Showcase Items Grid ── */
.shop__showcase {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.shop__showcase-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.shop__showcase-kicker {
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-secondary);
  letter-spacing: 0.05em;
}

.shop__showcase-count {
  font-size: 11px;
  font-family: var(--font-mono);
  color: var(--color-text-tertiary);
}

.shop__grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-md);
}

@media (max-width: 1080px) {
  .shop__grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
@media (max-width: 600px) {
  .shop__grid {
    grid-template-columns: 1fr;
  }
}

.shop__card {
  background: var(--color-card);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  position: relative;
  transition: all 180ms ease;
}

.shop__card:hover {
  border-color: color-mix(in srgb, #8b5cf6 40%, var(--color-border-subtle));
}

.shop__card--equipped {
  border-color: #10b981;
  background: color-mix(in srgb, #10b981 4%, var(--color-card));
}

.shop__card--owned {
  border-color: color-mix(in srgb, #8b5cf6 50%, var(--color-border-subtle));
}

.shop__card--unaffordable {
  opacity: 0.85;
}

.shop__top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 4px;
}

.shop__icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, #8b5cf6 15%, transparent);
  color: #a78bfa;
  display: flex;
  align-items: center;
  justify-content: center;
}

.shop__name {
  font-size: var(--text-base);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0;
}

.shop__desc {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  line-height: 1.5;
  margin: 0;
  flex: 1;
}

.shop__owned-qty {
  font-size: 11px;
  color: var(--color-text-tertiary);
  padding-top: 4px;
}

.shop__foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: var(--space-sm);
  padding-top: var(--space-xs);
  border-top: 1px solid var(--color-border-subtle);
  gap: 8px;
}

.shop__price {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: var(--text-sm);
  font-weight: 700;
  color: #c4b5fd;
  font-family: var(--font-mono);
}

.shop__card--soon {
  border-style: dashed;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-direction: column;
  gap: 8px;
  min-height: 160px;
  color: var(--color-text-tertiary);
}

.shop__soon-icon {
  opacity: 0.4;
}

.shop__soon-text {
  font-size: var(--text-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.shop__footer {
  text-align: center;
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  padding-block: var(--space-md);
}
</style>
