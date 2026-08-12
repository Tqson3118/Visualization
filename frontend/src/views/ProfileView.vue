<script setup lang="ts">
// ProfileView — Màn 32: 4 tab (Tổng quan / Tiến độ / Thành tích / Cài đặt) + thẻ tắt nhanh.
// G-F2d: hero profile card (avatar lớn, level + XP progress, badge streak, nút chỉnh sửa),
// skill radar vue-echarts (lazy — VChartLazy) từ overview.topics (data thật — KHÔNG bịa),
// Tabs shadcn + thẻ thành tích Badge + hover-lift.
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { Download, Flame, Pencil, RefreshCw } from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import * as progressApi from '@/api/progress';
import * as authApi from '@/api/auth';
import { useUiStore } from '@/stores/ui';
import Tabs from '@/components/ui/Tabs.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Input from '@/components/ui/Input.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';
import { messages } from '@/i18n/vi';

const auth = useAuthStore();
const gamification = useGamificationStore();
const progressStore = useProgressStore();
const ui = useUiStore();
const router = useRouter();

const tab = ref<'overview' | 'progress' | 'achievements' | 'settings'>('overview');
const loading = ref(true);

const passwordForm = ref({ current: '', next: '' });
const passwordError = ref('');
const passwordBusy = ref(false);

const level = computed(() => gamification.level);
const xp = computed(() => gamification.xp);

onMounted(async () => {
  loading.value = true;
  await Promise.allSettled([gamification.fetchAll(), progressStore.fetchOverview(), auth.fetchMe().catch(() => undefined)]);
  loading.value = false;
});

function changeTab(next: string): void {
  tab.value = next as typeof tab.value;
}

function goSettings(): void {
  tab.value = 'settings';
}

async function onChangePassword(): Promise<void> {
  passwordError.value = '';
  if (passwordForm.value.next.length < 8) {
    passwordError.value = 'Mật khẩu mới phải từ 8 ký tự';
    return;
  }
  passwordBusy.value = true;
  try {
    await authApi.changePassword({ currentPassword: passwordForm.value.current, newPassword: passwordForm.value.next });
    ui.showToast('Đổi mật khẩu thành công!', 'success');
    passwordForm.value = { current: '', next: '' };
  } catch (err) {
    passwordError.value = err instanceof Error ? err.message : 'Đổi mật khẩu thất bại.';
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

const quickLinks = [
  { label: '🏆 Thử thách hằng ngày', to: 'quests' },
  { label: '🏆 Bảng xếp hạng', to: 'leaderboard' },
  { label: '🛒 Cửa hàng', to: 'shop' },
  { label: '👥 Lớp học', to: 'classes' },
] as const;

const achievements = [
  { id: 'first-sim', label: 'Mô phỏng đầu tiên', unlocked: true },
  { id: 'first-quiz', label: 'Bài quiz đầu tiên', unlocked: true },
  { id: 'streak-7', label: '7 ngày liên tiếp', unlocked: gamification.streakDays >= 7 },
  { id: 'pass-node', label: 'Hoàn thành node đầu tiên', unlocked: false },
  { id: 'final-test', label: 'Kiểm tra cuối lộ trình', unlocked: false },
  { id: 'top10', label: 'Top 10 bảng xếp hạng', unlocked: false },
] as const;

// ── Skill radar (vue-echarts — G-F2d) ──
// 5-6 kỹ năng = chủ đề (topics) từ /progress/me: Sắp xếp & TM / CTDL tuyến tính /
// Cây / Bảng băm / Đồ thị. Giá trị = progressPct thật — KHÔNG bịa. Rỗng → EmptyState.

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
  const textColor = cssVar('--color-text-muted', '#5E7A77');
  const axisColor = cssVar('--color-border', '#cbd5e1');
  const foreground = cssVar('--color-foreground', '#134e4a');
  const cardColor = cssVar('--color-card', '#ffffff');
  const teal = '#14b8a6';

  return {
    tooltip: {
      trigger: 'item' as const,
      backgroundColor: cardColor,
      borderColor: axisColor,
      textStyle: { color: foreground, fontSize: 12 },
    },
    radar: {
      indicator: skillData.value.map((skill) => ({ name: skill.name, max: 100 })),
      radius: '68%',
      splitNumber: 4,
      axisName: { color: textColor, fontSize: 11 },
      splitLine: { lineStyle: { color: axisColor } },
      splitArea: {
        areaStyle: {
          color: ['rgba(20,184,166,0.02)', 'rgba(20,184,166,0.05)', 'rgba(20,184,166,0.08)', 'rgba(20,184,166,0.12)', 'rgba(20,184,166,0.16)'],
        },
      },
      axisLine: { lineStyle: { color: axisColor } },
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
        areaStyle: { color: 'rgba(20,184,166,0.18)' },
        lineStyle: { color: teal, width: 2 },
        symbol: 'circle' as const,
        symbolSize: 5,
        itemStyle: { color: teal },
      },
    ],
  };
});

async function reloadProgress(): Promise<void> {
  try {
    await progressStore.fetchOverview();
    ui.showToast('Đã tải lại tiến độ.', 'success');
  } catch {
    ui.showToast('Không thể tải tiến độ.', 'error');
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
    <!-- Hero profile card (gamification Aurora + sunset streak) -->
    <header class="profile__hero">
      <div class="profile__hero-main">
        <span class="profile__avatar" aria-hidden="true">
          {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
        </span>
        <div class="profile__identity">
          <h1 class="profile__name">{{ auth.user?.displayName ?? 'Người dùng' }}</h1>
          <p class="profile__email">{{ auth.user?.email }}</p>
          <div class="profile__chips">
            <Badge variant="primary">Lv {{ level }}</Badge>
            <Badge variant="success" class="profile__streak-chip"><Flame :size="11" /> {{ gamification.streakDays }} ngày streak</Badge>
            <Badge v-if="gamification.isPremium" variant="warning">Premium</Badge>
          </div>
        </div>
        <div class="profile__actions">
          <Button variant="secondary" size="sm" class="hover-lift" @click="goSettings">
            <Pencil :size="14" /> Chỉnh sửa
          </Button>
        </div>
      </div>

      <div class="profile__stats-row">
        <div class="profile__stat-block">
          <span class="profile__stat-label">Level</span>
          <span class="profile__stat-value">{{ level }}</span>
        </div>
        <div class="profile__stat-block">
          <span class="profile__stat-label">XP</span>
          <span class="profile__stat-value">{{ xp.toLocaleString('vi-VN') }}</span>
        </div>
        <div class="profile__stat-block">
          <span class="profile__stat-label">🔥 Streak</span>
          <span class="profile__stat-value">{{ gamification.streakDays }}</span>
        </div>
        <div class="profile__stat-block">
          <span class="profile__stat-label">💎 Gems</span>
          <span class="profile__stat-value">{{ gamification.gems }}</span>
        </div>
        <div class="profile__stat-block">
          <span class="profile__stat-label">❤️ Tim</span>
          <span class="profile__stat-value">{{ gamification.hearts }}/{{ gamification.heartsMax }}</span>
        </div>
      </div>

      <div class="profile__level-progress">
        <div class="profile__level-progress-head">
          <span class="profile__level-progress-label">Tiến độ lộ trình</span>
          <span class="profile__level-progress-note text-muted">
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

    <!-- Tabs shadcn: Tổng quan / Tiến độ / Thành tích / Cài đặt -->
    <Tabs
      :tabs="[
        { key: 'overview', label: 'Tổng quan' },
        { key: 'progress', label: 'Tiến độ' },
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
      <div class="profile__overview-grid">
        <div class="card profile__overview-card">
          <h2 class="profile__panel-title">Tiến độ tổng</h2>
          <p class="text-muted">Bài học đã xem: {{ overview?.lessonsViewed ?? 0 }}/{{ overview?.lessonsTotal ?? 0 }}</p>
          <p class="text-muted">Bài tập đã làm: {{ overview?.exercisesCompleted ?? 0 }}/{{ overview?.exercisesTotal ?? 0 }}</p>
          <p class="text-muted">Điểm trung bình: {{ overview?.avgScore ?? '—' }}</p>
          <div class="profile__overview-progress">
            <span class="text-muted">Hoàn thành bài học</span>
            <ProgressBar :value="levelProgressPct" :variant="levelProgressPct >= 100 ? 'success' : 'default'" />
          </div>
        </div>
        <div class="card profile__overview-card">
          <h2 class="profile__panel-title">Điểm đến nhanh</h2>
          <div class="profile__quick">
            <RouterLink v-for="link in quickLinks" :key="link.to" class="profile__quick-link hover-lift" :to="{ name: link.to }">
              {{ link.label }}
            </RouterLink>
          </div>
        </div>
      </div>

      <!-- Skill radar (vue-echarts lazy) — data thật từ /progress/me -->
      <div class="card profile__radar-card">
        <div class="profile__radar-head">
          <h2 class="profile__panel-title">🧭 Skill radar</h2>
          <span class="text-muted">Độ phủ kỹ năng theo chủ đề</span>
        </div>
        <VChartLazy v-if="skillData.length > 1" :option="radarOption" height="300px" />
        <EmptyState
          v-else
          icon="target"
          title="Chưa có dữ liệu kỹ năng"
          description="Hoàn thành bài học/bài tập trong từng chủ đề để vẽ radar kỹ năng của bạn."
        />
        <p class="profile__radar-note text-muted">
          Điểm mỗi trục = phần trăm hoàn thành chủ đề tương ứng (bài học + bài tập), tính từ tiến độ thực tế.
        </p>
      </div>
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
        v-if="!overview || overview.topics.length === 0"
        icon="target"
        title="Chưa có dữ liệu tiến độ"
        description="Học vài bài học đầu tiên để thấy tiến độ ở đây."
      />
      <div v-else class="profile__topics">
        <article v-for="topic in overview.topics" :key="topic.id" class="card profile__topic hover-lift">
          <div class="profile__topic-head">
            <h3 class="profile__topic-name">{{ topic.name }}</h3>
            <span class="text-muted profile__topic-pct">{{ topic.progressPct }}%</span>
          </div>
          <ProgressBar :value="topic.progressPct" :variant="topic.progressPct >= 100 ? 'success' : 'default'" />
          <ul class="profile__topic-lessons">
            <li v-for="lesson in topic.lessons" :key="lesson.id" class="profile__topic-lesson">
              <span :class="lesson.completed ? 'profile__done' : 'text-muted'">
                {{ lesson.completed ? '✓' : '○' }} {{ lesson.title }}
              </span>
              <Badge v-if="lesson.bestScore !== null" variant="primary">{{ lesson.bestScore }} điểm</Badge>
            </li>
          </ul>
        </article>
      </div>
    </section>

    <!-- Tab Thành tích -->
    <section v-else-if="tab === 'achievements'" class="profile__panel">
      <div class="profile__achievements">
        <div
          v-for="ach in achievements"
          :key="ach.id"
          class="profile__achievement card hover-lift"
          :class="{ 'profile__achievement--locked': !ach.unlocked }"
        >
          <span class="profile__achievement-icon">{{ ach.unlocked ? '🏅' : '🔒' }}</span>
          <p class="profile__achievement-label">{{ ach.label }}</p>
          <Badge :variant="ach.unlocked ? 'success' : 'muted'">{{ ach.unlocked ? 'Đã mở' : 'Chưa mở' }}</Badge>
        </div>
      </div>
      <EmptyState
        v-if="achievements.every((a) => !a.unlocked)"
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
          <Button type="submit" size="sm" :loading="passwordBusy">{{ messages.common.save }}</Button>
        </form>
      </div>
    </section>
  </main>
</template>

<style scoped>
.profile {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 920px;
}

/* ── Hero profile card (Aurora gamification) ── */
.profile__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-xl);
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.profile__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 66%, transparent);
}

.profile__hero::before {
  content: '';
  position: absolute;
  width: 280px;
  height: 280px;
  border-radius: 50%;
  top: -130px;
  right: -70px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.profile__hero-main {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.profile__avatar {
  width: 84px;
  height: 84px;
  border-radius: 50%;
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-3xl);
  font-weight: 800;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
  border: 3px solid color-mix(in srgb, var(--color-on-primary) 70%, transparent);
}

.profile__identity { display: flex; flex-direction: column; gap: 4px; min-width: 0; }

.profile__name {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
  margin: 0;
}

.profile__email { font-size: var(--text-sm); color: var(--color-text-muted); }

.profile__chips { display: flex; gap: var(--space-xs); flex-wrap: wrap; margin-top: 4px; }
.profile__streak-chip { background-image: var(--gradient-sunset); color: var(--color-on-primary); }

.profile__actions { margin-left: auto; }

.profile__stats-row {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px dashed var(--color-border);
}

.profile__stat-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
  text-align: center;
  padding: var(--space-sm);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-surface) 55%, transparent);
}

.profile__stat-label { font-size: var(--text-xs); color: var(--color-text-muted); }
.profile__stat-value { font-size: var(--text-md); font-weight: 800; color: var(--color-foreground); }

.profile__level-progress { display: flex; flex-direction: column; gap: 6px; }
.profile__level-progress-head { display: flex; justify-content: space-between; align-items: center; gap: var(--space-sm); }
.profile__level-progress-label { font-size: var(--text-sm); font-weight: 700; }
.profile__level-progress-note { font-size: var(--text-xs); }

.profile__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__panel { display: flex; flex-direction: column; gap: var(--space-md); }

.profile__panel-title { font-size: var(--text-md); margin-bottom: var(--space-sm); }

.profile__overview-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

.profile__overview-card { display: flex; flex-direction: column; gap: var(--space-xs); }
.profile__overview-progress { display: flex; flex-direction: column; gap: 4px; margin-top: var(--space-sm); }

.profile__quick { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__quick-link {
  font-weight: 600;
  font-size: var(--text-sm);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: var(--color-surface-hover);
  transition: background-color 180ms ease, color 180ms ease;
}
.profile__quick-link:hover { background: color-mix(in srgb, var(--color-primary) 14%, var(--color-surface)); }

/* ── Skill radar card ── */
.profile__radar-card { display: flex; flex-direction: column; gap: var(--space-xs); }
.profile__radar-head { display: flex; justify-content: space-between; align-items: baseline; gap: var(--space-sm); flex-wrap: wrap; }
.profile__radar-note { font-size: var(--text-xs); margin-top: 4px; }

.profile__progress-actions { display: flex; gap: var(--space-sm); justify-content: flex-end; }

.profile__topics { display: flex; flex-direction: column; gap: var(--space-md); }

.profile__topic { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__topic-head { display: flex; justify-content: space-between; align-items: center; }
.profile__topic-name { font-size: var(--text-md); }
.profile__topic-pct { font-size: var(--text-sm); }

.profile__topic-lessons { list-style: none; display: flex; flex-direction: column; gap: 6px; }

.profile__topic-lesson {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.profile__done { color: var(--color-success); font-weight: 600; }

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
  border: 1px solid var(--color-border);
}

.profile__achievement:not(.profile__achievement--locked) {
  border-color: color-mix(in srgb, var(--color-success) 40%, var(--color-border));
  background-image: linear-gradient(180deg, color-mix(in srgb, var(--color-success) 6%, var(--color-card)), var(--color-card));
}

.profile__achievement--locked { opacity: 0.6; }

.profile__achievement-icon { font-size: 1.75rem; }
.profile__achievement-label { font-size: var(--text-xs); font-weight: 600; }

.profile__settings { display: flex; flex-direction: column; gap: var(--space-md); max-width: 440px; }

.profile__password { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__password-error { color: var(--color-destructive); font-size: var(--text-sm); }

@media (max-width: 768px) {
  .profile__overview-grid { grid-template-columns: 1fr; }
  .profile__stats-row { grid-template-columns: repeat(2, 1fr); }
  .profile__actions { margin-left: 0; }
}
</style>
