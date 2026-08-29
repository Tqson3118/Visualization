<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import type { Component } from 'vue';
import { ShoppingBag, Target, Trophy, Users } from 'lucide-vue-next';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';
import XpProgressCard from '@/components/gamification/XpProgressCard.vue';
import StreakCard from '@/components/gamification/StreakCard.vue';
import QuestProgressCard from '@/components/gamification/QuestProgressCard.vue';
import BadgeGrid from '@/components/gamification/BadgeGrid.vue';

const props = defineProps<{
  loadError?: string;
}>();

const emit = defineEmits<{
  retry: [];
}>();

const gamification = useGamificationStore();
const progressStore = useProgressStore();
const ui = useUiStore();

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
</script>

<template>
  <div class="profile__overview-panel">
    <EmptyState
      v-if="loadError"
      icon="alert-circle"
      title="Không tải được tiến độ"
      :description="loadError"
      :action-label="messages.common.retry"
      @action="emit('retry')"
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
  </div>
</template>

<style scoped>
.profile__overview-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl, 24px);
}

.profile__overview-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--space-lg, 24px);
}

.profile__overview-card {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  padding: var(--space-lg, 24px);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm, 8px);
}

.profile__panel-title {
  font-size: var(--text-md, 15px);
  font-weight: 700;
  color: #ffffff;
  margin: 0;
}

.profile__mono {
  font-family: var(--font-mono, monospace);
  font-weight: 600;
  color: #ffffff;
}

.profile__overview-progress {
  display: flex;
  flex-direction: column;
  gap: 6px;
  margin-top: 6px;
}

.profile__quick {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-sm, 8px);
  margin-top: 4px;
}

.profile__quick-link {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-md, 8px);
  color: rgba(255, 255, 255, 0.85);
  font-size: var(--text-xs, 12px);
  font-weight: 500;
  text-decoration: none;
  transition: all 150ms ease;
}

.profile__quick-link:hover {
  background: rgba(168, 85, 247, 0.1);
  border-color: var(--color-accent, #a855f7);
  color: #ffffff;
}

.profile__quick-idx {
  font-family: var(--font-mono, monospace);
  font-size: 10px;
  color: rgba(255, 255, 255, 0.4);
}

.profile__radar-card {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  padding: var(--space-lg, 24px);
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 16px);
}

.profile__radar-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.profile__radar-canvas {
  width: 100%;
}

.profile__radar-note {
  font-size: var(--text-xs, 12px);
  color: var(--color-text-secondary, #8b949e);
  margin: 0;
}

.profile__gamification {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg, 24px);
}

.profile__gamification-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--space-lg, 24px);
}
</style>
