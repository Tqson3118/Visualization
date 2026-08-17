<script setup lang="ts">
// ProfileView — Màn 32: 5 tab (Tổng quan / Tiến độ / Túi đồ / Thành tích / Cài đặt) + thẻ tắt nhanh.
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 (bỏ gradient/blob/shadow),
// 1 stat hero duy nhất (XP — block-token tối canvas-ink + index mono), stat phụ level-1,
// radar + vùng dữ liệu LUÔN tối, icon lucide-vue-next (cấm emoji), không hover-lift/shadow card.
import { computed, onMounted, ref } from 'vue';
import type { Component } from 'vue';
import {
  Check,
  Circle,
  Clock,
  Download,
  Flame,
  Frame,
  Image as ImageIcon,
  Lock,
  Medal,
  Package,
  Pencil,
  RefreshCw,
  ShoppingBag,
  Target,
  Trophy,
  Users,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import * as progressApi from '@/api/progress';
import * as authApi from '@/api/auth';
import type { InventoryItemDto } from '@/api/gamification';
import { avatarVariant, equipGroup, equippedItem, frameVariant } from '@/utils/equipment';
import { useUiStore } from '@/stores/ui';
import Tabs from '@/components/ui/Tabs.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import Input from '@/components/ui/Input.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';
import { messages } from '@/i18n/vi';

const auth = useAuthStore();
const gamification = useGamificationStore();
const progressStore = useProgressStore();
const ui = useUiStore();

const tab = ref<'overview' | 'progress' | 'achievements' | 'inventory' | 'settings'>('overview');
const loading = ref(true);
const loadError = ref('');

const passwordForm = ref({ current: '', next: '' });
const passwordError = ref('');
const passwordBusy = ref(false);

const level = computed(() => gamification.level);
const xp = computed(() => gamification.xp);

const isTeacherPending = computed(() => auth.role === 'TEACHER_PENDING');

onMounted(async () => {
  loading.value = true;
  await Promise.allSettled([
    gamification.fetchAll(),
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

async function retryOverview(): Promise<void> {
  loadError.value = '';
  loading.value = true;
  try {
    await progressStore.fetchOverview();
  } catch {
    loadError.value = messages.profile.progressLoadError;
  }
  loading.value = false;
}

function changeTab(next: string): void {
  tab.value = next as typeof tab.value;
}

function goSettings(): void {
  tab.value = 'settings';
}

async function onChangePassword(): Promise<void> {
  passwordError.value = '';
  if (passwordForm.value.next.length < 8) {
    passwordError.value = messages.profile.passwordTooShort;
    return;
  }
  passwordBusy.value = true;
  try {
    await authApi.changePassword({ currentPassword: passwordForm.value.current, newPassword: passwordForm.value.next });
    ui.showToast(messages.profile.passwordChanged, 'success');
    passwordForm.value = { current: '', next: '' };
  } catch (err) {
    passwordError.value = err instanceof Error ? err.message : messages.profile.passwordTooShort;
  } finally {
    passwordBusy.value = false;
  }
}

const overview = computed(() => progressStore.overview);

// Tiến độ lộ trình tổng (data thật từ /progress/me)
const levelProgressPct = computed(() => {
  const o = overview.value;
  if (!o || o.lessonsTotal === 0) return 0;
  return Math.min(100, Math.round((o.lessonsViewed / o.lessonsTotal) * 100));
});

const quickLinks: Array<{ label: string; to: string; icon: Component }> = [
  { label: 'Thử thách hằng ngày', to: 'quests', icon: Target },
  { label: 'Bảng xếp hạng', to: 'leaderboard', icon: Trophy },
  { label: 'Cửa hàng', to: 'shop', icon: ShoppingBag },
  { label: 'Lớp học', to: 'classes', icon: Users },
];

const achievements = computed(() => gamification.achievements);

// ── Kho đồ (Màn N-8): avatar + khung viền từ /me/inventory (data thật) ──
const equippingId = ref<number | null>(null);

const avatarItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === 'avatar'));
const frameItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === 'frame'));
const consumableItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === null));

const invGroups = computed(() =>
  [
    { key: 'avatar', label: 'Avatar', icon: ImageIcon, items: avatarItems.value },
    { key: 'frame', label: 'Khung viền', icon: Frame, items: frameItems.value },
  ].filter((group) => group.items.length > 0),
);

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

async function toggleEquip(item: InventoryItemDto): Promise<void> {
  equippingId.value = item.itemId;
  try {
    await gamification.equipItem(item.itemId, !item.isEquipped);
    ui.showToast(item.isEquipped ? 'Đã gỡ trang bị.' : `Đã trang bị "${item.name}".`, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể trang bị vật phẩm.', 'error');
  } finally {
    equippingId.value = null;
  }
}

// ── Skill radar (vue-echarts — G-F2d) ──
// 5-6 kỹ năng = chủ đề (topics) từ /progress/me. Giá trị = progressPct thật — KHÔNG bịa.
// Rỗng → EmptyState. Nền chart LUÔN tối (canvas-ink — vùng dữ liệu, quyết định xuyên-nhóm #5).

const skillData = computed(() =>
  (overview.value?.topics ?? []).map((topic) => ({
    name: topic.name.length > 18 ? `${topic.name.slice(0, 18)}…` : topic.name,
    value: topic.progressPct,
  })),
);

/** Đọc CSS variable thành màu cụ thể (ECharts canvas không hiểu var()). */
function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

const radarOption = computed(() => {
  // Phụ thuộc theme (ui.theme) → recompute option khi toggle sáng/tối
  void ui.theme;
  const indexMuted = cssVar('--color-index-muted', '#6B7385');
  const dataCore = cssVar('--vdsa-purple', '#8b5cf6');
  const ink = cssVar('--color-canvas-ink', '#0D1020');

  return {
    tooltip: {
      trigger: 'item' as const,
      backgroundColor: ink,
      borderColor: indexMuted,
      textStyle: { color: indexMuted, fontSize: 12 },
    },
    radar: {
      indicator: skillData.value.map((skill) => ({ name: skill.name, max: 100 })),
      radius: '68%',
      splitNumber: 4,
      axisName: { color: indexMuted, fontSize: 11 },
      splitLine: { lineStyle: { color: indexMuted, opacity: 0.35 } },
      splitArea: {
        areaStyle: {
          color: ['rgba(139,92,246,0.06)', 'rgba(139,92,246,0.12)', 'rgba(139,92,246,0.18)', 'rgba(139,92,246,0.24)', 'rgba(139,92,246,0.3)'],
        },
      },
      axisLine: { lineStyle: { color: indexMuted } },
    },
    series: [
      {
        type: 'radar' as const,
        data: [
          {
            value: skillData.value.map((skill) => skill.value),
            name: 'Độ phủ kỹ năng',
          },
        ],
        areaStyle: { color: 'rgba(139,92,246,0.25)' },
        lineStyle: { color: dataCore, width: 2 },
        symbol: 'circle' as const,
        symbolSize: 5,
        itemStyle: { color: dataCore },
      },
    ],
  };
});

async function reloadProgress(): Promise<void> {
  try {
    await progressStore.fetchOverview();
    ui.showToast('Đã tải lại tiến độ.', 'success');
  } catch {
    ui.showToast(messages.profile.progressLoadError, 'error');
  }
}

function csvExport(): void {
  void progressApi.fetchOverview().then(async (data) => {
    const rows = [
      ['lessonsViewed', data.lessonsViewed],
      ['lessonsTotal', data.lessonsTotal],
      ['exercisesCompleted', data.exercisesCompleted],
      ['exercisesTotal', data.exercisesTotal],
      ['avgScore', data.avgScore ?? ''],
    ];
    const csv = '\uFEFF' + rows.map((r) => r.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'progress.csv';
    link.click();
    URL.revokeObjectURL(url);
  });
}
</script>

<template>
  <main class="profile container">
    <!-- Banner hồ sơ giảng viên chờ duyệt (TEACHER_PENDING) -->
    <div v-if="isTeacherPending" class="profile__banner" role="status">
      <Clock :size="16" aria-hidden="true" />
      <span>Hồ sơ giảng viên của bạn đang chờ xét duyệt — tính năng giảng dạy sẽ mở khóa khi Admin duyệt.</span>
    </div>

    <!-- Hero profile card — surface band level-2 (không gradient, không blob, không shadow) -->
    <header class="profile__hero">
      <div class="profile__hero-main">
        <span class="profile__avatar-frame" :class="frameThemeClass">
          <span class="profile__avatar" :class="avatarThemeClass" aria-hidden="true">
            {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
          </span>
        </span>
        <div class="profile__identity">
          <h1 class="profile__name">{{ auth.user?.displayName ?? 'Người dùng' }}</h1>
          <p class="profile__email">{{ auth.user?.email }}</p>
          <div class="profile__chips">
            <Badge variant="primary">Lv {{ level }}</Badge>
            <Badge variant="success" class="profile__streak-chip"><Flame :size="12" class="profile__flame" /> {{ gamification.streakDays }} ngày streak</Badge>
            <Badge v-if="gamification.isPremium" variant="warning">Premium</Badge>
          </div>
        </div>
        <div class="profile__actions">
          <Button variant="secondary" size="sm" @click="goSettings">
            <Pencil :size="14" /> Chỉnh sửa
          </Button>
        </div>
      </div>

      <!-- Stat hierarchy (DESIGN.md §6): 1 hero duy nhất = XP (block-token tối + index mono),
           còn lại stat phụ level-1. Streak = block-token resolved (dữ liệu tuần tự). -->
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

    <!-- Tabs shadcn: Tổng quan / Tiến độ / Túi đồ / Thành tích / Cài đặt -->
    <Tabs
      :tabs="[
        { key: 'overview', label: 'Tổng quan' },
        { key: 'progress', label: 'Tiến độ' },
        { key: 'inventory', label: 'Túi đồ' },
        { key: 'achievements', label: 'Thành tích' },
        { key: 'settings', label: 'Cài đặt' },
      ]"
      :model-value="tab"
      @change="changeTab"
    />

    <div v-if="loading" class="profile__loading" aria-busy="true">
      <Skeleton v-for="i in 4" :key="i" height="48px" />
    </div>

    <!-- Tab Tổng quan -->
    <section v-else-if="tab === 'overview'" class="profile__panel">
      <EmptyState
        v-if="loadError"
        icon="alert-circle"
        title="Không tải được tiến độ"
        :description="loadError"
        :action-label="messages.common.retry"
        @action="retryOverview"
      />
      <template v-else>
        <div class="profile__overview-grid">
          <div class="card profile__overview-card">
            <h2 class="profile__panel-title">Tiến độ tổng</h2>
            <p class="text-muted">Bài học đã xem: <span class="profile__mono">{{ overview?.lessonsViewed ?? 0 }}/{{ overview?.lessonsTotal ?? 0 }}</span></p>
            <p class="text-muted">Bài tập đã làm: <span class="profile__mono">{{ overview?.exercisesCompleted ?? 0 }}/{{ overview?.exercisesTotal ?? 0 }}</span></p>
            <p class="text-muted">Điểm trung bình: <span class="profile__mono">{{ overview?.avgScore ?? '—' }}</span></p>
            <div class="profile__overview-progress">
              <span class="text-muted">Hoàn thành bài học</span>
              <ProgressBar :value="levelProgressPct" :variant="levelProgressPct >= 100 ? 'success' : 'default'" />
            </div>
          </div>
          <div class="card profile__overview-card">
            <h2 class="profile__panel-title">Điểm đến nhanh</h2>
            <div class="profile__quick">
              <RouterLink v-for="(link, i) in quickLinks" :key="link.to" class="profile__quick-link" :to="{ name: link.to }">
                <span class="profile__quick-idx">{{ String(i + 1).padStart(2, '0') }}</span>
                <component :is="link.icon" :size="16" aria-hidden="true" />
                {{ link.label }}
              </RouterLink>
            </div>
          </div>
        </div>

        <!-- Skill radar (vue-echarts lazy) — data thật từ /progress/me, nền LUÔN tối -->
        <div class="card profile__radar-card">
          <div class="profile__radar-head">
            <h2 class="profile__panel-title">Skill radar</h2>
            <span class="text-muted">Độ phủ kỹ năng theo chủ đề</span>
          </div>
          <div v-if="skillData.length > 1" class="profile__radar-canvas">
            <VChartLazy :option="radarOption" height="300px" />
          </div>
          <EmptyState
            v-else
            icon="target"
            title="Chưa có dữ liệu kỹ năng"
            description="Hoàn thành bài học/bài tập trong từng chủ đề để vẽ radar kỹ năng của bạn."
          />
          <p class="profile__radar-note">
            Điểm mỗi trục = phần trăm hoàn thành chủ đề tương ứng (bài học + bài tập), tính từ tiến độ thực tế.
          </p>
        </div>
      </template>
    </section>

    <!-- Tab Tiến độ -->
    <section v-else-if="tab === 'progress'" class="profile__panel">
      <div class="profile__progress-actions">
        <Button variant="secondary" size="sm" @click="reloadProgress">
          <RefreshCw :size="14" /> Làm mới
        </Button>
        <Button variant="ghost" size="sm" @click="csvExport">
          <Download :size="14" /> Xuất CSV
        </Button>
      </div>
      <EmptyState
        v-if="loadError"
        icon="alert-circle"
        title="Không tải được tiến độ"
        :description="loadError"
        :action-label="messages.common.retry"
        @action="retryOverview"
      />
      <EmptyState
        v-else-if="!overview || overview.topics.length === 0"
        icon="target"
        title="Chưa có dữ liệu tiến độ"
        description="Học vài bài học đầu tiên để thấy tiến độ ở đây."
      />
      <div v-else class="profile__topics">
        <article v-for="topic in overview.topics" :key="topic.id" class="card profile__topic">
          <div class="profile__topic-head">
            <h3 class="profile__topic-name">{{ topic.name }}</h3>
            <span class="profile__topic-pct">{{ topic.progressPct }}%</span>
          </div>
          <ProgressBar :value="topic.progressPct" :variant="topic.progressPct >= 100 ? 'success' : 'default'" />
          <ul class="profile__topic-lessons">
            <li v-for="lesson in topic.lessons" :key="lesson.id" class="profile__topic-lesson">
              <span :class="lesson.completed ? 'profile__done' : 'profile__todo'">
                <Check v-if="lesson.completed" :size="14" aria-hidden="true" />
                <Circle v-else :size="10" aria-hidden="true" />
                {{ lesson.title }}
              </span>
              <Badge v-if="lesson.bestScore !== null" variant="primary">{{ lesson.bestScore }} điểm</Badge>
            </li>
          </ul>
        </article>
      </div>
    </section>

    <!-- Tab Túi đồ (Kho đồ — Màn N-8): avatar + khung viền từ /me/inventory -->
    <section v-else-if="tab === 'inventory'" class="profile__panel">
      <div class="profile__inv-groups">
        <section v-for="group in invGroups" :key="group.key" class="profile__inv-group">
          <h2 class="profile__panel-title profile__inv-title">{{ group.label }}</h2>
          <div class="profile__inv-grid">
            <article v-for="item in group.items" :key="item.id" class="card profile__inv-card">
              <span class="profile__inv-icon" aria-hidden="true">
                <component :is="group.icon" :size="20" />
              </span>
              <div class="profile__inv-body">
                <p class="profile__inv-name">{{ item.name }}</p>
                <Badge variant="muted">x{{ item.quantity }}</Badge>
              </div>
              <Button
                size="sm"
                :variant="item.isEquipped ? 'secondary' : 'primary'"
                :loading="equippingId === item.itemId"
                :disabled="equippingId !== null && equippingId !== item.itemId"
                @click="toggleEquip(item)"
              >
                {{ item.isEquipped ? 'Đang trang bị' : 'Trang bị' }}
              </Button>
            </article>
          </div>
        </section>

        <section v-if="consumableItems.length > 0" class="profile__inv-group">
          <h2 class="profile__panel-title profile__inv-title">Vật phẩm khác</h2>
          <ul class="card profile__inv-other">
            <li v-for="item in consumableItems" :key="item.id" class="profile__inv-other-row">
              <Package :size="16" aria-hidden="true" />
              <span class="profile__inv-name">{{ item.name }}</span>
              <Badge variant="muted">x{{ item.quantity }}</Badge>
            </li>
          </ul>
        </section>
      </div>

      <EmptyState
        v-if="gamification.inventory.length === 0"
        icon="package"
        title="Túi đồ trống"
        description="Mua avatar và khung viền tại Cửa hàng — trang bị ngay tại đây."
      />
    </section>

    <!-- Tab Thành tích (data thật từ /achievements) -->
    <section v-else-if="tab === 'achievements'" class="profile__panel">
      <div class="profile__achievements">
        <div
          v-for="ach in achievements"
          :key="ach.id"
          class="profile__achievement"
          :class="{ 'profile__achievement--locked': !ach.earnedAt }"
        >
          <span class="profile__achievement-icon" :class="{ 'profile__achievement-icon--open': ach.earnedAt }">
            <img v-if="ach.iconUrl" :src="ach.iconUrl" :alt="ach.name" class="profile__achievement-img" />
            <component :is="ach.earnedAt ? Medal : Lock" v-else :size="20" aria-hidden="true" />
          </span>
          <p class="profile__achievement-label">{{ ach.name }}</p>
          <p v-if="ach.description" class="profile__achievement-desc">{{ ach.description }}</p>
          <Badge :variant="ach.earnedAt ? 'success' : 'muted'">{{ ach.earnedAt ? 'Đã mở' : 'Chưa mở' }}</Badge>
        </div>
      </div>
      <EmptyState
        v-if="achievements.every((a) => !a.earnedAt)"
        icon="party-popper"
        title="Chưa có huy hiệu"
        description="Bắt đầu học để mở huy hiệu đầu tiên nhé!"
      />
    </section>

    <!-- Tab Cài đặt -->
    <section v-else class="profile__panel">
      <div class="card profile__settings">
        <h2 class="profile__panel-title">Đổi mật khẩu</h2>
        <form class="profile__password" novalidate @submit.prevent="onChangePassword">
          <Input v-model="passwordForm.current" label="Mật khẩu hiện tại" type="password" autocomplete="current-password" required />
          <Input v-model="passwordForm.next" label="Mật khẩu mới" type="password" autocomplete="new-password" required />
          <p v-if="passwordError" class="profile__password-error" role="alert">{{ passwordError }}</p>
          <Button type="submit" :loading="passwordBusy">{{ messages.profile.savePassword }}</Button>
        </form>
      </div>
    </section>
  </main>
</template>

<style scoped>
.profile {
  /* Bộ tím (khớp --vdsa-purple / tab Sorting Sandbox) — accent riêng cho trang hồ sơ */
  --p-purple: #8b5cf6;
  --p-purple-light: #a78bfa;
  --p-purple-dark: #7c3aed;
  /* Remap token shadcn primary (teal) → tím NGAY TRONG scope trang:
     ProgressBar, Badge primary, Button primary, Tabs active đều tự đổi sang tím. */
  --primary: oklch(0.56 0.24 293);
  --primary-foreground: oklch(0.99 0 0);
  --ring: oklch(0.56 0.24 293);
  /* BlockToken XP hero đang dùng --data-core (xanh #4255FF) → tím */
  --data-core: var(--p-purple);
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 920px;
}

/* Card — kính mờ (glass) đồng bộ phong cách hiện tại: nền bán trong suốt + blur +
   viền sáng nhẹ + glow tông primary. Thay cho card phẳng shadow-none cũ. */
.profile .card {
  background: color-mix(in srgb, var(--color-card) 72%, transparent);
  backdrop-filter: blur(10px);
  border-color: color-mix(in srgb, var(--p-purple) 22%, var(--color-border-subtle));
  box-shadow:
    inset 0 1px 0 color-mix(in srgb, #fff 6%, transparent),
    0 8px 28px color-mix(in srgb, var(--p-purple) 7%, transparent);
  transition: border-color 180ms cubic-bezier(0.16, 1, 0.3, 1), box-shadow 180ms cubic-bezier(0.16, 1, 0.3, 1);
}
.profile .card:hover {
  border-color: color-mix(in srgb, var(--p-purple) 45%, var(--color-border-subtle));
  box-shadow:
    inset 0 1px 0 color-mix(in srgb, #fff 8%, transparent),
    0 10px 34px color-mix(in srgb, var(--p-purple) 12%, transparent);
}

/* ── Hero profile — glass band + dải accent gradient trên đỉnh ── */
.profile__hero {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg);
  border: 1px solid color-mix(in srgb, var(--p-purple) 30%, var(--color-border-subtle));
  border-radius: var(--radius-lg);
  background:
    linear-gradient(160deg, color-mix(in srgb, var(--p-purple) 7%, transparent) 0%, transparent 42%),
    color-mix(in srgb, var(--color-card-raised) 78%, transparent);
  backdrop-filter: blur(14px);
  box-shadow:
    inset 0 1px 0 color-mix(in srgb, #fff 7%, transparent),
    0 10px 36px color-mix(in srgb, var(--p-purple) 10%, transparent);
  overflow: hidden;
}

/* Dải accent gradient (primary → data-core) trên đỉnh hero */
.profile__hero::before {
  content: '';
  position: absolute;
  inset: 0 0 auto 0;
  height: 3px;
  background: linear-gradient(90deg, var(--p-purple), var(--p-purple-dark), transparent);
  opacity: 0.85;
}

.profile__hero-main {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

/* Banner hồ sơ giảng viên chờ duyệt — warning band kính mờ */
.profile__banner {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid color-mix(in srgb, var(--color-warning) 40%, transparent);
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--color-warning) 10%, transparent);
  backdrop-filter: blur(10px);
  color: var(--color-foreground);
  font-size: var(--text-sm);
  font-weight: 500;
}

.profile__banner svg { color: var(--color-warning); flex-shrink: 0; }

/* Khung viền avatar — gradient theo itemKey (frameVariant), fallback primary */
.profile__avatar-frame {
  border-radius: var(--radius-full);
  padding: 3px;
  display: inline-flex;
  flex-shrink: 0;
}

.profile__avatar-frame--neon {
  background: linear-gradient(135deg, #ec4899, #22d3ee);
  box-shadow: 0 0 16px rgba(236, 72, 153, 0.45);
}

.profile__avatar-frame--gold {
  background: linear-gradient(135deg, #f59e0b, #fde68a, #f59e0b);
  box-shadow: 0 0 18px rgba(250, 204, 21, 0.5);
}

.profile__avatar-frame--cyber {
  background: linear-gradient(135deg, #22d3ee, #6366f1);
  box-shadow: 0 0 16px rgba(34, 211, 238, 0.45);
}

.profile__avatar-frame--fire {
  background: linear-gradient(135deg, #ef4444, #f97316);
  box-shadow: 0 0 16px rgba(239, 68, 68, 0.5);
}

.profile__avatar-frame--ice {
  background: linear-gradient(135deg, #7dd3fc, #93c5fd);
  box-shadow: 0 0 14px rgba(125, 211, 252, 0.5);
}

.profile__avatar-frame--default {
  background: linear-gradient(135deg, var(--p-purple), var(--p-purple-dark));
  box-shadow: 0 0 14px color-mix(in srgb, var(--p-purple) 45%, transparent);
}

.profile__avatar {
  width: 60px;
  height: 60px;
  border-radius: var(--radius-full);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-mono);
  font-size: var(--text-xl);
  font-weight: 700;
  flex-shrink: 0;
}

/* Avatar theme theo itemKey đang trang bị — gradient tối + chữ sáng (đọc được cả 2 theme) */
.profile__avatar--cyber { background: linear-gradient(135deg, #0e7490, #155e75); color: #a5f3fc; }
.profile__avatar--gold { background: linear-gradient(135deg, #b45309, #92400e); color: #fef3c7; }
.profile__avatar--neon { background: linear-gradient(135deg, #be185d, #6b21a8); color: #fbcfe8; }
.profile__avatar--wizard { background: linear-gradient(135deg, #6d28d9, #4c1d95); color: #ddd6fe; }
.profile__avatar--bot { background: linear-gradient(135deg, #0f766e, #134e4a); color: #99f6e4; }

.profile__identity { display: flex; flex-direction: column; gap: var(--space-xs); min-width: 0; }

.profile__name {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
  margin: 0;
}

.profile__email { font-size: var(--text-sm); color: var(--color-text-muted); }

.profile__chips { display: flex; gap: var(--space-xs); flex-wrap: wrap; }

.profile__actions { margin-left: auto; }

/* ── Stat hierarchy: 1 hero (XP) + stat phụ level-1 — compact 1 hàng ── */
.profile__stats-row {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-sm);
  padding-top: var(--space-sm);
  border-top: 1px solid var(--color-border-subtle);
}

/* Mobile: XP hero full width, stat phụ 2 cột — không chiếm cả màn hình */
.profile__stats-hero { grid-column: 1 / -1; }

@media (min-width: 640px) {
  .profile__stats-row { grid-template-columns: repeat(4, 1fr); }
  .profile__stats-hero { grid-column: span 2; }
}

@media (min-width: 1024px) {
  .profile__stats-row { grid-template-columns: repeat(6, 1fr); }
  .profile__stats-hero { grid-column: span 2; }
}

/* Khoảnh khắc đầu tư duy nhất của màn: hero-stat vào nhẹ (transform+opacity, easing chuẩn) */
@keyframes profile-hero-enter {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}

.profile__stats-hero {
  animation: profile-hero-enter 300ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

/* Stat phụ — glass + viền trên màu riêng từng loại (Level=cyan, Gems=xanh lá, Tim=đỏ) */
.profile__stat-block {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  padding: var(--space-sm);
  border: 1px solid color-mix(in srgb, var(--color-border-strong) 55%, transparent);
  border-top: 2px solid var(--p-purple);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-card) 75%, transparent);
  backdrop-filter: blur(8px);
  min-width: 0;
  transition: transform 180ms cubic-bezier(0.16, 1, 0.3, 1), box-shadow 180ms cubic-bezier(0.16, 1, 0.3, 1);
}
.profile__stat-block:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 22px color-mix(in srgb, var(--p-purple) 12%, transparent);
}
.profile__stat-block--level { border-top-color: #a78bfa; }
.profile__stat-block--gems { border-top-color: #8b5cf6; }
.profile__stat-block--hearts { border-top-color: #7c3aed; }
.profile__stat-block--level .profile__stat-value { color: color-mix(in srgb, #a78bfa 62%, var(--color-foreground)); }
.profile__stat-block--gems .profile__stat-value { color: color-mix(in srgb, #8b5cf6 62%, var(--color-foreground)); }
.profile__stat-block--hearts .profile__stat-value { color: color-mix(in srgb, #7c3aed 62%, var(--color-foreground)); }

.profile__stat-label {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  font-weight: 500;
}

.profile__stat-line { display: flex; align-items: baseline; gap: var(--space-xs); }

.profile__stat-value {
  font-size: var(--text-xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.profile__stat-unit {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.profile__level-progress { display: flex; flex-direction: column; gap: var(--space-xs); }
.profile__level-progress-head { display: flex; justify-content: space-between; align-items: center; gap: var(--space-sm); }
.profile__level-progress-label { font-size: var(--text-sm); font-weight: 600; }
.profile__level-progress-note { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-text-tertiary); }

.profile__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__panel { display: flex; flex-direction: column; gap: var(--space-lg); }

.profile__panel-title {
  font-size: var(--text-xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  margin-bottom: var(--space-sm);
  padding-left: var(--space-sm);
  border-left: 3px solid var(--p-purple);
}

.profile__overview-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

.profile__overview-card { display: flex; flex-direction: column; gap: var(--space-xs); }
.profile__overview-progress { display: flex; flex-direction: column; gap: var(--space-xs); margin-top: var(--space-sm); }

.profile__mono { font-family: var(--font-mono); font-size: var(--text-sm); }

.profile__quick { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__quick-link {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid color-mix(in srgb, var(--p-purple) 20%, var(--color-border-subtle));
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-card) 70%, transparent);
  backdrop-filter: blur(8px);
  color: var(--color-foreground);
  font-weight: 500;
  font-size: var(--text-sm);
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1), transform 150ms cubic-bezier(0.16, 1, 0.3, 1), box-shadow 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.profile__quick-link:hover {
  border-color: color-mix(in srgb, var(--p-purple) 55%, var(--color-border-subtle));
  transform: translateX(3px);
  box-shadow: 0 6px 18px color-mix(in srgb, var(--p-purple) 12%, transparent);
  text-decoration: none;
}

.profile__quick-link svg { color: var(--p-purple); }

.profile__quick-idx {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

/* ── Skill radar card — vùng dữ liệu LUÔN tối ── */
.profile__radar-card { display: flex; flex-direction: column; gap: var(--space-xs); }
.profile__radar-head { display: flex; justify-content: space-between; align-items: baseline; gap: var(--space-sm); flex-wrap: wrap; }
.profile__radar-note { font-size: var(--text-xs); color: var(--color-text-tertiary); margin-top: var(--space-xs); }

.profile__radar-canvas {
  margin-top: var(--space-xs);
  border: 1px solid color-mix(in srgb, var(--p-purple-dark) 20%, transparent);
  border-radius: var(--radius-lg);
  background: var(--color-canvas-ink);
  padding: var(--space-sm);
  width: 100%;
  min-width: 0;
  overflow: hidden;
}

.profile__progress-actions { display: flex; gap: var(--space-sm); justify-content: flex-end; }

.profile__topics { display: flex; flex-direction: column; gap: var(--space-lg); }

.profile__topic { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__topic-head { display: flex; justify-content: space-between; align-items: center; gap: var(--space-sm); }
.profile__topic-name { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.01em; }
.profile__topic-pct { font-family: var(--font-mono); font-size: var(--text-sm); color: var(--color-text-tertiary); }

.profile__topic-lessons { list-style: none; display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__topic-lesson {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.profile__done {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--color-success);
  font-weight: 500;
}

.profile__todo {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--color-text-tertiary);
}

.profile__achievements {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
  gap: var(--space-md);
}

.profile__achievement {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm);
  text-align: center;
  padding: var(--space-lg) var(--space-md);
  border: 1px solid color-mix(in srgb, var(--p-purple) 18%, var(--color-border-subtle));
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--color-card) 72%, transparent);
  backdrop-filter: blur(8px);
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1), box-shadow 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.profile__achievement:hover { border-color: color-mix(in srgb, var(--p-purple) 45%, var(--color-border-subtle)); }

.profile__achievement--locked { opacity: 0.55; }

.profile__achievement-icon { color: var(--color-text-quaternary); }
.profile__achievement-icon--open {
  color: var(--color-success);
  filter: drop-shadow(0 0 8px color-mix(in srgb, var(--color-success) 60%, transparent));
}
.profile__achievement-img { width: 24px; height: 24px; object-fit: contain; border-radius: var(--radius-sm); }
.profile__achievement-label { font-size: var(--text-xs); font-weight: 600; color: var(--color-foreground); }
.profile__achievement-desc { font-size: var(--text-xs); line-height: 1.5; color: var(--color-text-muted); }

/* ── Kho đồ — nhóm avatar/khung + vật phẩm khác ── */
.profile__inv-groups { display: flex; flex-direction: column; gap: var(--space-lg); }

.profile__inv-group { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__inv-title { margin-bottom: 0; }

.profile__inv-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: var(--space-md);
}

.profile__inv-card {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-md);
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1), box-shadow 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.profile__inv-card:hover {
  border-color: color-mix(in srgb, var(--p-purple) 50%, var(--color-border-subtle));
  box-shadow: 0 8px 22px color-mix(in srgb, var(--p-purple) 12%, transparent);
}

.profile__inv-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: linear-gradient(135deg, color-mix(in srgb, var(--p-purple) 16%, var(--color-muted)), var(--color-muted));
  color: var(--p-purple);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.profile__inv-body {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-xs);
  min-width: 0;
  flex: 1;
}

.profile__inv-name {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-foreground);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 100%;
}

.profile__inv-other {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  padding: var(--space-md);
}

.profile__inv-other-row {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.profile__inv-other-row svg { color: var(--color-text-tertiary); flex-shrink: 0; }
.profile__inv-other-row .profile__inv-name { flex: 1; }

.profile__settings { display: flex; flex-direction: column; gap: var(--space-md); max-width: 440px; }

.profile__password { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__password-error { color: var(--color-destructive); font-size: var(--text-sm); }

@media (prefers-reduced-motion: reduce) {
  .profile__stats-hero { animation: none; }
}

/* ── Streak: ngọn lửa bập bùng ── */
.profile__streak-chip { position: relative; }

/* Halo cam phía sau badge streak */
.profile__streak-chip::after {
  content: '';
  position: absolute;
  inset: -3px -6px;
  border-radius: 999px;
  background: radial-gradient(circle, rgba(251, 146, 60, 0.4) 0%, transparent 70%);
  filter: blur(7px);
  z-index: -1;
  animation: flame-glow 1.6s ease-in-out infinite;
}

/* Ngọn lửa icon: bập bùng nghiêng + phóng to nhỏ */
.profile__flame {
  color: #fb923c;
  transform-origin: center bottom;
  animation: flame-flicker 1.5s ease-in-out infinite;
  filter: drop-shadow(0 0 3px rgba(251, 146, 60, 0.8));
}

/* Streak token (BlockToken) — hơi thở phát sáng cam */
.profile__streak-token {
  animation: streak-breathe 2.4s ease-in-out infinite;
}

@keyframes flame-flicker {
  0%, 100% { transform: scale(1) rotate(-2deg); }
  20%      { transform: scale(1.14) rotate(2deg) translateY(-1px); }
  45%      { transform: scale(0.94) rotate(-1.5deg); }
  70%      { transform: scale(1.08) rotate(1deg) translateY(-0.5px); }
}

@keyframes flame-glow {
  0%, 100% { opacity: 0.55; transform: scale(1); }
  50%      { opacity: 1; transform: scale(1.15); }
}

@keyframes streak-breathe {
  0%, 100% { box-shadow: 0 0 0 rgba(251, 146, 60, 0); }
  50%      { box-shadow: 0 0 20px rgba(251, 146, 60, 0.4); }
}

@media (prefers-reduced-motion: reduce) {
  .profile__flame,
  .profile__streak-chip::after,
  .profile__streak-token { animation: none; }
}

@media (max-width: 768px) {
  .profile__overview-grid { grid-template-columns: 1fr; }
  .profile__actions { margin-left: 0; }
}
</style>
