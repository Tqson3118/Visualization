<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { Frame, Image as ImageIcon, Package } from 'lucide-vue-next';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import type { InventoryItemDto } from '@/api/gamification';
import { avatarImageUrl, equipGroup } from '@/utils/equipment';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const router = useRouter();
const gamification = useGamificationStore();
const ui = useUiStore();
const auth = useAuthStore();

const equippingId = ref<number | null>(null);

const avatarItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === 'avatar'));
const frameItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === 'frame'));
const consumableItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === null));

const invGroups = computed(() =>
  [
    { key: 'avatar', label: 'Avatar tùy biến', icon: ImageIcon, items: avatarItems.value },
    { key: 'frame', label: 'Khung viền hồ sơ', icon: Frame, items: frameItems.value },
  ].filter((group) => group.items.length > 0),
);

async function toggleEquip(item: InventoryItemDto): Promise<void> {
  equippingId.value = item.itemId;
  try {
    await gamification.equipItem(item.itemId, !item.isEquipped);
    await auth.fetchMe();
    ui.showToast(item.isEquipped ? 'Đã gỡ trang bị.' : `Đã trang bị "${item.name}".`, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể trang bị vật phẩm.', 'error');
  } finally {
    equippingId.value = null;
  }
}
</script>

<template>
  <div class="profile__inv-panel">
    <div v-if="gamification.inventory.length > 0" class="profile__inv-groups">
      <section v-for="group in invGroups" :key="group.key" class="profile__inv-group">
        <h2 class="profile__panel-title profile__inv-title">{{ group.label }}</h2>
        <div class="profile__inv-grid">
          <article
            v-for="item in group.items"
            :key="item.id"
            class="card profile__inv-card"
            :class="{ 'profile__inv-card--equipped': item.isEquipped }"
          >
            <span class="profile__inv-icon" aria-hidden="true">
              <img
                v-if="item.imageUrl || avatarImageUrl(item.itemKey)"
                :src="item.imageUrl || avatarImageUrl(item.itemKey)"
                :alt="item.name"
                class="w-10 h-10 rounded-full object-cover shadow-sm"
              />
              <component :is="group.icon" v-else :size="20" />
            </span>
            <div class="profile__inv-body">
              <p class="profile__inv-name">{{ item.name }}</p>
              <div class="flex items-center gap-1.5 mt-0.5">
                <Badge v-if="item.isEquipped" variant="success" class="text-[10px]">✨ Đang dùng</Badge>
                <Badge v-else variant="muted" class="text-[10px]">x{{ item.quantity }}</Badge>
              </div>
            </div>
            <Button
              size="sm"
              :variant="item.isEquipped ? 'secondary' : 'primary'"
              :loading="equippingId === item.itemId"
              :disabled="equippingId !== null && equippingId !== item.itemId"
              @click="toggleEquip(item)"
            >
              {{ item.isEquipped ? 'Gỡ trang bị' : 'Trang bị' }}
            </Button>
          </article>
        </div>
      </section>

      <section v-if="consumableItems.length > 0" class="profile__inv-group">
        <h2 class="profile__panel-title profile__inv-title">Vật phẩm tiêu hao & hỗ trợ</h2>
        <ul class="card profile__inv-other">
          <li v-for="item in consumableItems" :key="item.id" class="profile__inv-other-row">
            <div class="flex items-center gap-2">
              <Package :size="16" class="text-vdsa-purple" aria-hidden="true" />
              <span class="profile__inv-name">{{ item.name }}</span>
            </div>
            <Badge variant="primary">Số lượng: {{ item.quantity }}</Badge>
          </li>
        </ul>
      </section>
    </div>

    <EmptyState
      v-else
      icon="package"
      title="Túi đồ trống"
      description="Bạn chưa sở hữu vật phẩm nào. Ghé thăm Cửa hàng để trang bị avatar và khung viền độc đáo!"
      action-label="Đến Cửa hàng ngay"
      @action="router.push('/shop')"
    />
  </div>
</template>

<style scoped>
.profile__inv-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg, 24px);
}

.profile__inv-groups {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl, 32px);
}

.profile__panel-title {
  font-size: var(--text-md, 15px);
  font-weight: 700;
  color: #ffffff;
  margin-bottom: var(--space-md, 16px);
}

.profile__inv-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: var(--space-md, 16px);
}

.profile__inv-card {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  padding: var(--space-md, 16px);
  display: flex;
  align-items: center;
  gap: var(--space-md, 16px);
  transition: all 150ms ease;
}

.profile__inv-card--equipped {
  border-color: var(--color-accent, #a855f7);
  background: rgba(168, 85, 247, 0.06);
}

.profile__inv-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md, 8px);
  background: rgba(255, 255, 255, 0.05);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-accent, #a855f7);
  flex-shrink: 0;
}

.profile__inv-body {
  flex: 1;
  min-width: 0;
}

.profile__inv-name {
  font-size: var(--text-sm, 13px);
  font-weight: 600;
  color: #ffffff;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.profile__inv-other {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  padding: var(--space-sm, 8px);
  list-style: none;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.profile__inv-other-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  border-radius: 6px;
}

.profile__inv-other-row:hover {
  background: rgba(255, 255, 255, 0.03);
}
</style>
