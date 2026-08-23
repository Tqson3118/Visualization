<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import type { Component } from 'vue';
import { useRoute, useRouter } from 'vue-router';
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
  User,
  Mail,
  ShieldCheck,
  Calendar,
  Sparkles,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import * as progressApi from '@/api/progress';
import * as authApi from '@/api/auth';
import type { InventoryItemDto } from '@/api/gamification';
import { avatarImageUrl, avatarVariant, equipGroup, equippedItem, frameVariant } from '@/utils/equipment';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
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

// Gamification dashboard
import XpProgressCard from '@/components/gamification/XpProgressCard.vue';
import StreakCard from '@/components/gamification/StreakCard.vue';
import QuestProgressCard from '@/components/gamification/QuestProgressCard.vue';
import BadgeGrid from '@/components/gamification/BadgeGrid.vue';

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const gamification = useGamificationStore();
const progressStore = useProgressStore();
const ui = useUiStore();

const tab = ref<'overview' | 'progress' | 'achievements' | 'inventory' | 'settings'>('overview');
const loading = ref(true);
const loadError = ref('');

// Profile Name Form
const displayNameInput = ref('');
const nameUpdating = ref(false);

// Password Form
const passwordForm = ref({ current: '', next: '' });
const passwordError = ref('');
const passwordBusy = ref(false);

// Avatar upload
const avatarFile = ref<File | null>(null);
const avatarPreview = ref<string | null>(null);
const avatarError = ref('');
const avatarUploading = ref(false);

function onAvatarSelected(event: Event): void {
  const input = event.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const file = input.files[0];
  if (file.size > 2 * 1024 * 1024) {
    avatarError.value = 'Ảnh không được vượt quá 2MB.';
    return;
  }
  avatarError.value = '';
  avatarFile.value = file;
  const reader = new FileReader();
  reader.onload = (e) => {
    avatarPreview.value = e.target?.result as string;
  };
  reader.readAsDataURL(file);
}

async function uploadAvatar(): Promise<void> {
  if (!avatarFile.value) return;
  avatarUploading.value = true;
  avatarError.value = '';
  try {
    const base64 = await new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = reject;
      reader.readAsDataURL(avatarFile.value!);
    });
    await authApi.updateProfile({ avatarUrl: base64 });
    await auth.fetchMe();
    ui.showToast('Cập nhật ảnh đại diện thành công!', 'success');
    avatarFile.value = null;
    avatarPreview.value = null;
  } catch (err) {
    avatarError.value = err instanceof Error ? err.message : 'Không thể upload ảnh.';
  } finally {
    avatarUploading.value = false;
  }
}

async function removeAvatar(): Promise<void> {
  avatarUploading.value = true;
  try {
    await authApi.updateProfile({ avatarUrl: null });
    await auth.fetchMe();
    ui.showToast('Đã xóa ảnh đại diện.', 'success');
  } catch (err) {
    avatarError.value = err instanceof Error ? err.message : 'Không thể xóa ảnh.';
  } finally {
    avatarUploading.value = false;
  }
}

async function onUpdateDisplayName(): Promise<void> {
  if (displayNameInput.value.trim().length < 2) {
    ui.showToast('Họ và tên phải từ 2 ký tự trở lên.', 'warning');
    return;
  }
  nameUpdating.value = true;
  try {
    await authApi.updateProfile({ displayName: displayNameInput.value.trim() });
    await auth.fetchMe();
    ui.showToast('Cập nhật họ tên thành công!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Cập nhật thất bại.', 'error');
  } finally {
    nameUpdating.value = false;
  }
}

const level = computed(() => gamification.level);
const xp = computed(() => gamification.xp);
const isTeacherPending = computed(() => auth.role === 'TEACHER_PENDING');

onMounted(async () => {
  loading.value = true;
  // Initialize tab from route query if present
  const qTab = route.query.tab as string;
  if (qTab && ['overview', 'progress', 'achievements', 'inventory', 'settings'].includes(qTab)) {
    tab.value = qTab as typeof tab.value;
  }

  await Promise.allSettled([
    gamification.fetchAll(),
    gamification.fetchQuests(),
    gamification.fetchInventory(),
    gamification.fetchAchievements(),
    auth.fetchMe().catch(() => undefined),
  ]);

  if (auth.user) {
    displayNameInput.value = auth.user.displayName || '';
  }

  try {
    await progressStore.fetchOverview();
  } catch {
    loadError.value = messages.profile.progressLoadError;
  }
  loading.value = false;
});

watch(
  () => route.query.tab,
  (newTab) => {
    if (newTab && typeof newTab === 'string' && ['overview', 'progress', 'achievements', 'inventory', 'settings'].includes(newTab)) {
      tab.value = newTab as typeof tab.value;
    }
  },
);

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
  void router.replace({ query: { ...route.query, tab: next } });
}

function goSettings(): void {
  changeTab('settings');
}

async function onChangePassword(): Promise<void> {
  passwordError.value = '';
  if (passwordForm.value.next.length < 6) {
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
    { key: 'avatar', label: 'Avatar tùy biến', icon: ImageIcon, items: avatarItems.value },
    { key: 'frame', label: 'Khung viền hồ sơ', icon: Frame, items: frameItems.value },
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

// ── Skill radar (vue-echarts) ──
const skillData = computed(() =>
  (overview.value?.topics ?? []).map((topic) => ({
    name: topic.name.length > 18 ? `${topic.name.slice(0, 18)}…` : topic.name,
    value: topic.progressPct,
  })),
);

function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

const radarOption = computed(() => {
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

    <!-- Hero profile card — surface band level-2 -->
    <header class="profile__hero">
      <div class="profile__hero-main">
        <span class="profile__avatar-frame" :class="frameThemeClass">
          <img
            v-if="equippedAvatar && avatarImageUrl(equippedAvatar.itemKey)"
            :src="avatarImageUrl(equippedAvatar.itemKey)"
            :alt="equippedAvatar.name"
            class="profile__avatar profile__avatar-image"
            :class="avatarThemeClass"
          />
          <img
            v-else-if="auth.user?.avatarUrl"
            :src="auth.user.avatarUrl"
            :alt="auth.user?.displayName ?? 'Avatar'"
            class="profile__avatar profile__avatar-image"
          />
          <span v-else class="profile__avatar" :class="avatarThemeClass" aria-hidden="true">
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

      <!-- Stat hierarchy: 1 hero duy nhất = XP -->
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

    <!-- Tabs: Tổng quan / Tiến độ / Túi đồ / Thành tích / Cài đặt -->
    <Tabs
      :tabs="[
        { key: 'overview', label: 'Tổng quan' },
        { key: 'progress', label: 'Tiến độ' },
        { key: 'inventory', label: 'Túi đồ', badge: gamification.inventory.length > 0 ? gamification.inventory.length : undefined },
        { key: 'achievements', label: 'Thành tích' },
        { key: 'settings', label: 'Cài đặt' },
      ]"
      :model-value="tab"
      @change="changeTab"
    />

    <div v-if="loading" class="profile__loading" aria-busy="true">
      <Skeleton v-for="i in 4" :key="i" height="48px" />
    </div>

    <!-- ═══ TAB 1: TỔNG QUAN ═══ -->
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

        <!-- Skill radar -->
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

        <!-- Gamification dashboard -->
        <div class="profile__gamification">
          <div class="profile__gamification-row">
            <XpProgressCard
              :level="gamification.level"
              :xp="gamification.xp"
              :xp-into-level="gamification.xpIntoLevel"
              :xp-for-next-level="gamification.xpForNextLevel"
              :level-progress-pct="gamification.levelProgressPct"
              :loading="gamification.loading"
            />
            <StreakCard
              :streak-days="gamification.streakDays"
              :freeze-available="gamification.freezeAvailable"
              :loading="gamification.loading"
            />
          </div>
          <QuestProgressCard :quests="gamification.quests" :loading="gamification.loading" />
          <BadgeGrid :badges="gamification.achievements" :loading="gamification.loading" />
        </div>
      </template>
    </section>

    <!-- ═══ TAB 2: TIẾN ĐỘ ═══ -->
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

    <!-- ═══ TAB 3: TÚI ĐỒ ═══ -->
    <section v-else-if="tab === 'inventory'" class="profile__panel">
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
                  v-if="avatarImageUrl(item.itemKey)"
                  :src="avatarImageUrl(item.itemKey)"
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
    </section>

    <!-- ═══ TAB 4: THÀNH TÍCH ═══ -->
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

    <!-- ═══ TAB 5: CÀI ĐẶT ═══ -->
    <section v-else class="profile__panel">
      <div class="card profile__settings space-y-6">
        <!-- 1. Thông tin cá nhân -->
        <section>
          <h2 class="profile__panel-title mb-3">Thông tin tài khoản</h2>
          <form class="space-y-4 max-w-lg" @submit.prevent="onUpdateDisplayName">
            <Input v-model="displayNameInput" label="Họ và tên hiển thị" placeholder="Nhập họ và tên..." required />

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Địa chỉ Email</label>
                <div class="px-3 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl text-xs text-vdsa-muted flex items-center justify-between">
                  <span>{{ auth.user?.email }}</span>
                  <Badge variant="success" class="text-[10px]">Đã xác thực</Badge>
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Vai trò hệ thống</label>
                <div class="px-3 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl text-xs text-vdsa-muted flex items-center gap-1.5">
                  <ShieldCheck :size="14" class="text-vdsa-purple" />
                  <span class="font-bold text-white">{{ auth.role }}</span>
                </div>
              </div>
            </div>

            <Button type="submit" size="sm" variant="primary" :loading="nameUpdating">
              Lưu thay đổi họ tên
            </Button>
          </form>
        </section>

        <hr class="profile__divider" />

        <!-- 2. Ảnh đại diện -->
        <section>
          <h2 class="profile__panel-title mb-3">Ảnh đại diện</h2>
          <div class="profile__avatar-upload">
            <div class="profile__avatar-preview">
              <img
                v-if="auth.user?.avatarUrl || avatarPreview"
                :src="(avatarPreview ?? auth.user?.avatarUrl) || ''"
                alt="Avatar"
                class="profile__avatar-preview-img"
              />
              <span v-else class="profile__avatar-preview-placeholder">📷</span>
            </div>
            <div class="profile__avatar-actions">
              <input
                type="file"
                accept="image/*"
                class="profile__avatar-input"
                @change="onAvatarSelected"
              />
              <Button
                v-if="auth.user?.avatarUrl"
                variant="secondary"
                size="sm"
                :loading="avatarUploading"
                @click="removeAvatar"
              >
                Xóa ảnh
              </Button>
              <Button
                v-if="avatarFile"
                variant="primary"
                size="sm"
                :loading="avatarUploading"
                @click="uploadAvatar"
              >
                Cập nhật
              </Button>
            </div>
            <p v-if="avatarError" class="profile__avatar-error" role="alert">{{ avatarError }}</p>
          </div>
        </section>

        <hr class="profile__divider" />

        <!-- 3. Đổi mật khẩu -->
        <section>
          <h2 class="profile__panel-title mb-3">Bảo mật & Đổi mật khẩu</h2>
          <form class="profile__password max-w-lg" novalidate @submit.prevent="onChangePassword">
            <Input v-model="passwordForm.current" label="Mật khẩu hiện tại" type="password" autocomplete="current-password" required />
            <Input v-model="passwordForm.next" label="Mật khẩu mới (tối thiểu 6 ký tự)" type="password" autocomplete="new-password" required />
            <p v-if="passwordError" class="profile__password-error" role="alert">{{ passwordError }}</p>
            <Button type="submit" size="sm" :loading="passwordBusy">{{ messages.profile.savePassword }}</Button>
          </form>
        </section>
      </div>
    </section>
  </main>
</template>

<style scoped>
.profile {
  --p-purple: #8b5cf6;
  --p-purple-light: #a78bfa;
  --p-purple-dark: #7c3aed;
  --primary: oklch(0.56 0.24 293);
  --primary-foreground: oklch(0.99 0 0);
  --ring: oklch(0.56 0.24 293);
  --data-core: var(--p-purple);
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 960px;
}

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

/* ── Hero profile ── */
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

.profile__hero::before {
  content: '';
  position: absolute;
  inset: 0 0 auto 0;
  height: 3px;
  background: linear-gradient(90deg, var(--p-purple-light), var(--p-purple), transparent 80%);
}

.profile__banner {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, #eab308 14%, transparent);
  border: 1px solid color-mix(in srgb, #eab308 40%, transparent);
  color: #fde047;
  font-size: var(--text-sm);
}

.profile__hero-main {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.profile__avatar-frame {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 5px;
  border-radius: 50%;
  transition: all 250ms ease;
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
  background: linear-gradient(135deg, var(--p-purple-light), var(--p-purple-dark));
  box-shadow: 0 0 14px rgba(168, 85, 247, 0.4);
}

.profile__avatar {
  width: 68px;
  height: 68px;
  border-radius: 50%;
  background: var(--p-purple);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-2xl);
  font-weight: 800;
  object-fit: cover;
  transition: all 250ms ease;
}

.profile__avatar--cyber { background: linear-gradient(135deg, #0e7490, #155e75); color: #a5f3fc; }
.profile__avatar--gold { background: linear-gradient(135deg, #b45309, #92400e); color: #fef3c7; }
.profile__avatar--neon { background: linear-gradient(135deg, #be185d, #6b21a8); color: #fbcfe8; }
.profile__avatar--wizard { background: linear-gradient(135deg, #6d28d9, #4c1d95); color: #ddd6fe; }
.profile__avatar--bot { background: linear-gradient(135deg, #0f766e, #134e4a); color: #99f6e4; }

.profile__identity {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
  min-width: 200px;
}

.profile__name {
  font-size: var(--text-2xl);
  font-weight: 800;
  margin: 0;
  color: var(--color-text-primary);
}

.profile__email {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  margin: 0;
}

.profile__chips {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 4px;
  flex-wrap: wrap;
}

.profile__flame { color: #f97316; }

.profile__actions {
  margin-left: auto;
}

.profile__stats-row {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.profile__stat-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--space-xs) var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
}

.profile__stat-label {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  letter-spacing: 0.05em;
}

.profile__stat-line {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.profile__stat-value {
  font-size: var(--text-base);
  font-weight: 700;
  color: var(--color-text-primary);
  font-family: var(--font-mono);
}

.profile__stat-unit {
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-tertiary);
}

.profile__level-progress {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.profile__level-progress-head {
  display: flex;
  justify-content: space-between;
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
}

/* ── Panel & Tabs ── */
.profile__panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.profile__overview-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-md);
}

@media (max-width: 720px) {
  .profile__overview-grid {
    grid-template-columns: 1fr;
  }
}

.profile__overview-card {
  padding: var(--space-lg);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.profile__panel-title {
  font-size: var(--text-base);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0 0 var(--space-xs) 0;
}

.profile__mono {
  font-family: var(--font-mono);
  font-weight: 700;
  color: var(--color-text-primary);
}

.profile__overview-progress {
  margin-top: var(--space-sm);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.profile__quick {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-xs);
  margin-top: var(--space-xs);
}

.profile__quick-link {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: var(--radius-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  color: var(--color-text-primary);
  font-size: var(--text-xs);
  text-decoration: none;
  font-weight: 600;
  transition: all 180ms ease;
}

.profile__quick-link:hover {
  background: color-mix(in srgb, var(--p-purple) 15%, var(--color-surface));
  border-color: var(--p-purple-light);
}

.profile__quick-idx {
  font-family: var(--font-mono);
  font-size: 10px;
  color: var(--color-text-tertiary);
}

/* ── Radar ── */
.profile__radar-card {
  padding: var(--space-lg);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.profile__radar-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.profile__radar-note {
  font-size: 11px;
  color: var(--color-text-tertiary);
  margin: 0;
}

/* ── Gamification ── */
.profile__gamification {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.profile__gamification-row {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-md);
}

@media (max-width: 720px) {
  .profile__gamification-row {
    grid-template-columns: 1fr;
  }
}

/* ── Progress Tab ── */
.profile__progress-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-xs);
}

.profile__topics {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.profile__topic {
  padding: var(--space-md) var(--space-lg);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.profile__topic-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.profile__topic-name {
  font-size: var(--text-sm);
  font-weight: 700;
  margin: 0;
}

.profile__topic-pct {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 700;
  color: var(--p-purple-light);
}

.profile__topic-lessons {
  list-style: none;
  padding: 0;
  margin: var(--space-xs) 0 0 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.profile__topic-lesson {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--text-xs);
}

.profile__done {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--color-text-primary);
}

.profile__todo {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--color-text-tertiary);
}

/* ── Inventory Tab ── */
.profile__inv-groups {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.profile__inv-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.profile__inv-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-md);
}

@media (max-width: 800px) {
  .profile__inv-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
@media (max-width: 500px) {
  .profile__inv-grid {
    grid-template-columns: 1fr;
  }
}

.profile__inv-card {
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  position: relative;
}

.profile__inv-card--equipped {
  border-color: #10b981;
  background: color-mix(in srgb, #10b981 6%, var(--color-card));
}

.profile__inv-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--p-purple) 15%, transparent);
  color: var(--p-purple-light);
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile__inv-name {
  font-size: var(--text-sm);
  font-weight: 700;
  margin: 0;
}

.profile__inv-other {
  list-style: none;
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  margin: 0;
}

.profile__inv-other-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  border-radius: var(--radius-md);
  background: var(--color-surface);
}

/* ── Achievements Tab ── */
.profile__achievements {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: var(--space-md);
}

.profile__achievement {
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  background: var(--color-card);
  border: 1px solid var(--color-border-subtle);
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 6px;
}

.profile__achievement--locked {
  opacity: 0.55;
  filter: grayscale(0.8);
}

.profile__achievement-icon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--color-surface);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-tertiary);
}

.profile__achievement-icon--open {
  background: color-mix(in srgb, #eab308 20%, transparent);
  color: #facc15;
}

.profile__achievement-label {
  font-size: var(--text-sm);
  font-weight: 700;
  margin: 0;
}

.profile__achievement-desc {
  font-size: 11px;
  color: var(--color-text-tertiary);
  margin: 0;
}

/* ── Settings Tab ── */
.profile__settings {
  padding: var(--space-xl);
  border-radius: var(--radius-lg);
}

.profile__divider {
  border: none;
  border-top: 1px solid var(--color-border-subtle);
  margin-block: var(--space-md);
}

.profile__avatar-upload {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.profile__avatar-preview {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: var(--color-surface);
  border: 2px dashed var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.profile__avatar-preview-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile__avatar-preview-placeholder {
  font-size: 24px;
}

.profile__avatar-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.profile__avatar-input {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
}

.profile__avatar-error,
.profile__password-error {
  font-size: 12px;
  color: #ef4444;
  margin: 0;
}

.profile__password {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}
</style>