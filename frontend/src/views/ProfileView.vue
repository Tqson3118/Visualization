<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Flame, Pencil } from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import { avatarImageUrl, avatarVariant, equippedItem, frameVariant } from '@/utils/equipment';
import { messages } from '@/i18n/vi';
import Tabs from '@/components/ui/Tabs.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import BlockToken from '@/components/ui/BlockToken.vue';

// Subcomponents
import ProfileOverviewTab from '@/components/profile/ProfileOverviewTab.vue';
import ProfileProgressTab from '@/components/profile/ProfileProgressTab.vue';
import ProfileInventoryTab from '@/components/profile/ProfileInventoryTab.vue';
import ProfileAchievementsTab from '@/components/profile/ProfileAchievementsTab.vue';
import ProfileFeedbackTab from '@/components/profile/ProfileFeedbackTab.vue';
import ProfileSettingsTab from '@/components/profile/ProfileSettingsTab.vue';

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const gamification = useGamificationStore();
const progressStore = useProgressStore();

const tab = ref<'overview' | 'progress' | 'achievements' | 'inventory' | 'feedback' | 'settings'>('overview');
const loading = ref(true);
const loadError = ref('');
const avatarImgFailed = ref(false);

watch(
  () => auth.user?.avatarUrl,
  () => {
    avatarImgFailed.value = false;
  },
);

const level = computed(() => gamification.level);
const xp = computed(() => gamification.xp);

const equippedAvatar = computed(() => equippedItem(gamification.inventory, 'avatar'));
const equippedFrame = computed(() => equippedItem(gamification.inventory, 'frame'));

const avatarThemeClass = computed(() => {
  const key = equippedAvatar.value?.itemKey;
  return key ? `profile__avatar--${avatarVariant(key)}` : '';
});
const frameThemeClass = computed(() => {
  const key = equippedFrame.value?.itemKey;
  return key ? `profile__avatar-frame--${frameVariant(key)}` : '';
});

const overview = computed(() => progressStore.overview);
const levelProgressPct = computed(() => {
  const o = overview.value;
  if (!o || o.lessonsTotal === 0) return 0;
  return Math.min(100, Math.round((o.lessonsViewed / o.lessonsTotal) * 100));
});

function changeTab(next: string): void {
  tab.value = next as typeof tab.value;
  void router.replace({ query: { ...route.query, tab: next } });
}

function goSettings(): void {
  changeTab('settings');
}

async function retryOverview(): Promise<void> {
  loadError.value = '';
  try {
    await progressStore.fetchOverview();
  } catch {
    loadError.value = messages.profile.progressLoadError;
  }
}

onMounted(async () => {
  loading.value = true;
  const qTab = route.query.tab as string;
  if (qTab && ['overview', 'progress', 'achievements', 'inventory', 'feedback', 'settings'].includes(qTab)) {
    tab.value = qTab as typeof tab.value;
  }

  await Promise.allSettled([
    gamification.fetchAll(),
    gamification.fetchQuests(),
    gamification.fetchInventory(),
    gamification.fetchAchievements(),
    auth.fetchMe().catch(() => undefined),
  ]);

  try {
    await progressStore.fetchOverview();
  } catch {
    loadError.value = messages.profile.progressLoadError;
  }
  loading.value = false;
});
</script>

<template>
  <main class="profile container">
    <!-- Header: Identity + Stat hierarchy -->
    <header class="card profile__header">
      <div class="profile__user">
        <span class="profile__avatar-frame" :class="frameThemeClass">
          <img
            v-if="auth.user?.avatarUrl && !avatarImgFailed"
            :src="auth.user.avatarUrl"
            :alt="auth.user?.displayName ?? 'Avatar'"
            class="profile__avatar profile__avatar-image"
            @error="avatarImgFailed = true"
          />
          <img
            v-else-if="equippedAvatar && (equippedAvatar.imageUrl || avatarImageUrl(equippedAvatar.itemKey))"
            :src="equippedAvatar.imageUrl || avatarImageUrl(equippedAvatar.itemKey)"
            :alt="auth.user?.displayName ?? 'Avatar'"
            class="profile__avatar profile__avatar-image"
          />
          <span v-else class="profile__avatar" :class="avatarThemeClass" aria-hidden="true">
            {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
          </span>

          <img
            v-if="equippedFrame && (equippedFrame.imageUrl || avatarImageUrl(equippedFrame.itemKey))"
            :src="equippedFrame.imageUrl || avatarImageUrl(equippedFrame.itemKey)"
            class="profile__avatar-frame-overlay"
            alt=""
            aria-hidden="true"
          />
        </span>
        <div class="profile__identity">
          <h1 class="profile__name">{{ auth.user?.displayName ?? 'Người dùng' }}</h1>
          <p class="profile__email">{{ auth.user?.email }}</p>
          <div class="profile__chips">
            <Badge variant="primary">Lv {{ level }}</Badge>
            <Badge variant="success" class="profile__streak-chip"><Flame :size="12" class="profile__flame" /> {{ gamification.streakDays }} ngày streak</Badge>
            <Badge v-if="gamification.isPremium" variant="warning">Premium</Badge>
            <Badge v-if="auth.role === 'TEACHER'" variant="secondary">Giảng viên</Badge>
            <Badge v-else-if="auth.role === 'ADMIN'" variant="success">Quản trị viên</Badge>
          </div>
        </div>
        <div class="profile__actions">
          <Button variant="secondary" size="sm" @click="goSettings">
            <Pencil :size="14" /> Chỉnh sửa
          </Button>
        </div>
      </div>

      <!-- Stat hierarchy -->
      <div class="profile__stats-row">
        <BlockToken
          label="XP"
          :value="xp.toLocaleString('vi-VN')"
          index="01 · tích lũy"
          class="profile__stats-hero"
        />
        <div class="profile__stat-block profile__stat-block--level">
          <span class="profile__stat-label">Level</span>
          <div class="profile__stat-line">
            <span class="profile__stat-value">{{ level }}</span>
            <span class="profile__stat-unit">CẤP</span>
          </div>
        </div>
        <BlockToken size="sm" tone="resolved" label="Streak" :value="gamification.streakDays" index="ngày" class="profile__streak-token" />
        <div class="profile__stat-block profile__stat-block--gems">
          <span class="profile__stat-label">Gems</span>
          <div class="profile__stat-line">
            <span class="profile__stat-value">{{ gamification.gems }}</span>
            <span class="profile__stat-unit">GEMS</span>
          </div>
        </div>
        <div class="profile__stat-block profile__stat-block--hearts">
          <span class="profile__stat-label">Tim</span>
          <div class="profile__stat-line">
            <span class="profile__stat-value">{{ gamification.hearts }}/{{ gamification.heartsMax }}</span>
            <span class="profile__stat-unit">TIM</span>
          </div>
        </div>
      </div>

      <div class="profile__level-progress">
        <div class="profile__level-progress-head">
          <span class="profile__level-progress-label">Tiến độ lộ trình</span>
          <span class="profile__level-progress-note">
            {{ overview?.lessonsViewed ?? 0 }}/{{ overview?.lessonsTotal ?? 0 }} bài học
          </span>
        </div>
        <ProgressBar
          :value="levelProgressPct"
          show-label
          :variant="levelProgressPct >= 100 ? 'success' : 'default'"
        />
      </div>
    </header>

    <!-- Tabs: Tổng quan / Tiến độ / Túi đồ / Thành tích / Phản hồi / Cài đặt -->
    <Tabs
      :tabs="[
        { key: 'overview', label: 'Tổng quan' },
        { key: 'progress', label: 'Tiến độ' },
        { key: 'inventory', label: 'Túi đồ', badge: gamification.inventory.length > 0 ? gamification.inventory.length : undefined },
        { key: 'achievements', label: 'Thành tích' },
        { key: 'feedback', label: 'Ý kiến & Phản hồi' },
        { key: 'settings', label: 'Cài đặt' },
      ]"
      v-model="tab"
      @change="changeTab"
    />

    <div v-if="loading" class="profile__loading" aria-busy="true">
      <Skeleton v-for="i in 4" :key="i" height="48px" />
    </div>

    <!-- Tab 1: Tổng quan -->
    <section v-else-if="tab === 'overview'" class="profile__panel">
      <ProfileOverviewTab :load-error="loadError" @retry="retryOverview" />
    </section>

    <!-- Tab 2: Tiến độ -->
    <section v-else-if="tab === 'progress'" class="profile__panel">
      <ProfileProgressTab :load-error="loadError" @retry="retryOverview" />
    </section>

    <!-- Tab 3: Túi đồ -->
    <section v-else-if="tab === 'inventory'" class="profile__panel">
      <ProfileInventoryTab />
    </section>

    <!-- Tab 4: Thành tích -->
    <section v-else-if="tab === 'achievements'" class="profile__panel">
      <ProfileAchievementsTab />
    </section>

    <!-- Tab 5: Ý kiến & Phản hồi của tôi -->
    <section v-else-if="tab === 'feedback'" class="profile__panel">
      <ProfileFeedbackTab />
    </section>

    <!-- Tab 6: Cài đặt -->
    <section v-else-if="tab === 'settings'" class="profile__panel">
      <ProfileSettingsTab />
    </section>
  </main>
</template>

<style scoped>
.profile {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl, 24px);
  padding-block: var(--space-xl, 24px) var(--space-3xl, 48px);
}

.profile__header {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-xl, 16px);
  padding: var(--space-xl, 24px);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg, 24px);
}

.profile__user {
  display: flex;
  align-items: center;
  gap: var(--space-lg, 24px);
}

.profile__avatar-frame {
  position: relative;
  width: 72px;
  height: 72px;
  border-radius: var(--radius-full, 9999px);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: all 0.3s ease;
}

.profile__avatar-frame--neon    { padding: 3px; background: linear-gradient(135deg, #ec4899, #22d3ee); box-shadow: 0 0 16px rgba(236, 72, 153, 0.45); }
.profile__avatar-frame--gold    { padding: 3px; background: linear-gradient(135deg, #f59e0b, #fde68a, #f59e0b); box-shadow: 0 0 18px rgba(250, 204, 21, 0.5); }
.profile__avatar-frame--cyber   { padding: 3px; background: linear-gradient(135deg, #22d3ee, #6366f1); box-shadow: 0 0 16px rgba(34, 211, 238, 0.45); }
.profile__avatar-frame--fire    { padding: 3px; background: linear-gradient(135deg, #ef4444, #f97316); box-shadow: 0 0 16px rgba(239, 68, 68, 0.5); }
.profile__avatar-frame--ice     { padding: 3px; background: linear-gradient(135deg, #7dd3fc, #93c5fd); box-shadow: 0 0 14px rgba(125, 211, 252, 0.5); }
.profile__avatar-frame--default { padding: 3px; background: linear-gradient(135deg, #a855f7, #6366f1); box-shadow: 0 0 14px rgba(168, 85, 247, 0.4); }

.profile__avatar-frame-overlay {
  position: absolute;
  top: -8px;
  left: -8px;
  width: calc(100% + 16px);
  height: calc(100% + 16px);
  pointer-events: none;
  object-fit: contain;
  z-index: 2;
}

.profile__avatar {
  width: 100%;
  height: 100%;
  border-radius: var(--radius-full, 9999px);
  background: linear-gradient(135deg, var(--color-primary, #7c3aed), #a855f7);
  color: #ffffff;
  font-size: var(--text-xl, 20px);
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile__avatar-image {
  object-fit: cover;
}

.profile__identity {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.profile__name {
  font-size: var(--text-2xl, 24px);
  font-weight: 800;
  color: #ffffff;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.profile__email {
  font-size: var(--text-xs, 12px);
  color: var(--color-text-secondary, #8b949e);
  margin: 0;
}

.profile__chips {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 4px;
}

.profile__streak-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.profile__flame {
  color: #f97316;
}

.profile__stats-row {
  display: grid;
  grid-template-columns: 1.5fr repeat(4, 1fr);
  gap: var(--space-md, 16px);
  align-items: stretch;
}

.profile__stats-hero {
  grid-column: span 1;
}

.profile__stat-block {
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  padding: var(--space-md, 16px);
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.profile__stat-label {
  font-size: var(--text-xs, 12px);
  color: var(--color-text-secondary, #8b949e);
  text-transform: uppercase;
  font-weight: 600;
  letter-spacing: 0.05em;
}

.profile__stat-line {
  display: flex;
  align-items: baseline;
  gap: 6px;
}

.profile__stat-value {
  font-family: var(--font-mono, monospace);
  font-size: var(--text-2xl, 24px);
  font-weight: 800;
  color: #ffffff;
}

.profile__stat-unit {
  font-family: var(--font-mono, monospace);
  font-size: 10px;
  color: var(--color-text-secondary, #8b949e);
}

.profile__level-progress {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.profile__level-progress-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.profile__level-progress-label {
  font-size: var(--text-xs, 12px);
  font-weight: 600;
  color: #ffffff;
}

.profile__level-progress-note {
  font-family: var(--font-mono, monospace);
  font-size: var(--text-xs, 12px);
  color: var(--color-text-secondary, #8b949e);
}

.profile__loading {
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 16px);
}

.profile__panel {
  animation: panel-in 200ms ease both;
}

@keyframes panel-in {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (max-width: 768px) {
  .profile__user {
    flex-direction: column;
    align-items: flex-start;
  }

  .profile__actions {
    align-self: flex-start;
  }

  .profile__stats-row {
    grid-template-columns: repeat(2, 1fr);
  }

  .profile__stats-hero {
    grid-column: span 2;
  }
}
</style>