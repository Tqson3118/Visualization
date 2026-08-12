<script setup lang="ts">
// ProfileView — Màn 32: 4 tab (Tổng quan / Tiến độ / Thành tích / Cài đặt) + thẻ tắt nhanh
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import * as progressApi from '@/api/progress';
import * as authApi from '@/api/auth';
import { useUiStore } from '@/stores/ui';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Input from '@/components/ui/Input.vue';
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

function changeTab(next: typeof tab.value): void {
  tab.value = next;
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
    <header class="profile__header card">
      <span class="profile__avatar">{{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}</span>
      <div class="profile__identity">
        <h1 class="profile__name">{{ auth.user?.displayName ?? 'Người dùng' }}</h1>
        <p class="text-muted profile__email">{{ auth.user?.email }}</p>
      </div>
      <div class="profile__stats">
        <span class="profile__stat"><strong>Level {{ level }}</strong> · {{ xp.toLocaleString('vi-VN') }} XP</span>
        <span class="profile__stat">🔥 {{ gamification.streakDays }}</span>
        <span class="profile__stat">❤ {{ gamification.hearts }}/{{ gamification.heartsMax }}</span>
        <span class="profile__stat">💎 {{ gamification.gems }}</span>
        <Badge v-if="gamification.isPremium" variant="warning">Premium</Badge>
      </div>
    </header>

    <div class="profile__tabs">
      <button
        v-for="t in ([
          { key: 'overview', label: 'Tổng quan' },
          { key: 'progress', label: 'Tiến độ' },
          { key: 'achievements', label: 'Thành tích' },
          { key: 'settings', label: 'Cài đặt' },
        ] as const)"
        :key="t.key"
        type="button"
        class="profile__tab"
        :class="{ 'profile__tab--active': tab === t.key }"
        @click="changeTab(t.key)"
      >
        {{ t.label }}
      </button>
    </div>

    <div v-if="loading" class="profile__loading">
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
        </div>
        <div class="card profile__overview-card">
          <h2 class="profile__panel-title">Điểm đến nhanh</h2>
          <div class="profile__quick">
            <RouterLink v-for="link in quickLinks" :key="link.to" class="profile__quick-link" :to="{ name: link.to }">
              {{ link.label }}
            </RouterLink>
          </div>
        </div>
      </div>
    </section>

    <!-- Tab Tiến độ -->
    <section v-else-if="tab === 'progress'" class="profile__panel">
      <div class="profile__progress-actions">
        <Button variant="secondary" size="sm" @click="reloadProgress">Làm mới</Button>
        <Button variant="ghost" size="sm" @click="csvExport">Xuất CSV</Button>
      </div>
      <EmptyState
        v-if="!overview || overview.topics.length === 0"
        icon="chart"
        title="Chưa có dữ liệu tiến độ"
        description="Học vài bài học đầu tiên để thấy tiến độ ở đây."
      />
      <div v-else class="profile__topics">
        <article v-for="topic in overview.topics" :key="topic.id" class="card profile__topic">
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
          class="profile__achievement card"
          :class="{ 'profile__achievement--locked': !ach.unlocked }"
        >
          <span class="profile__achievement-icon">{{ ach.unlocked ? '🏅' : '🔒' }}</span>
          <p class="profile__achievement-label">{{ ach.label }}</p>
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
  max-width: 900px;
}

.profile__header {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.profile__avatar {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: var(--color-primary);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-2xl);
  font-weight: 800;
  flex-shrink: 0;
}

.profile__name { font-size: var(--text-xl); }
.profile__email { font-size: var(--text-sm); }

.profile__stats {
  margin-left: auto;
  display: flex;
  gap: var(--space-sm);
  align-items: center;
  flex-wrap: wrap;
}

.profile__stat { font-size: var(--text-sm); color: var(--color-text-muted); }

.profile__tabs {
  display: flex;
  gap: var(--space-xs);
  border-bottom: 2px solid var(--color-border);
  overflow-x: auto;
}

.profile__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-md);
  font-weight: 700;
  color: var(--color-text-muted);
  cursor: pointer;
  white-space: nowrap;
  margin-bottom: -2px;
}

.profile__tab--active { color: var(--color-primary); border-bottom-color: var(--color-primary); }

.profile__panel-title { font-size: var(--text-md); margin-bottom: var(--space-sm); }

.profile__overview-grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

.profile__overview-card { display: flex; flex-direction: column; gap: var(--space-xs); }

.profile__quick { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__quick-link { font-weight: 600; font-size: var(--text-sm); }

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
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: var(--space-md);
}

.profile__achievement {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm);
  text-align: center;
  padding: var(--space-md);
}

.profile__achievement--locked { opacity: 0.55; }

.profile__achievement-icon { font-size: 1.75rem; }
.profile__achievement-label { font-size: var(--text-xs); font-weight: 600; }

.profile__settings { display: flex; flex-direction: column; gap: var(--space-md); max-width: 420px; }

.profile__password { display: flex; flex-direction: column; gap: var(--space-sm); }

.profile__password-error { color: var(--color-destructive); font-size: var(--text-sm); }

@media (max-width: 768px) {
  .profile__overview-grid { grid-template-columns: 1fr; }
}
</style>
