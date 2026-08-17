<script setup lang="ts">
// XpProgressCard — Level + XP tiến tới level kế (dữ liệu THẬT từ store/API, không hardcode).
import { computed } from 'vue';
import Card from '@/components/ui/Card.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import { messages } from '@/i18n/vi';

const props = defineProps<{
  level: number;
  xp: number;
  xpIntoLevel: number;
  xpForNextLevel: number;
  levelProgressPct: number;
  loading?: boolean;
}>();

const pct = computed(() => Math.min(100, Math.max(0, props.levelProgressPct)));
</script>

<template>
  <Card class="xp-progress-card">
    <div v-if="loading" class="xp-progress-card__loading" aria-busy="true">
      <Skeleton height="56px" />
      <Skeleton height="12px" />
    </div>
    <template v-else>
      <div class="xp-progress-card__head">
        <div class="xp-progress-card__level">
          <span class="xp-progress-card__level-num">{{ level }}</span>
          <span class="xp-progress-card__level-label">{{ messages.gamification.levelLabel }}</span>
        </div>
        <BlockToken
          size="md"
          tone="resolved"
          :label="messages.gamification.xpLabel"
          :value="String(xp)"
        />
      </div>
      <div class="xp-progress-card__bar">
        <ProgressBar :value="pct" variant="success" size="sm" />
        <span class="xp-progress-card__bar-caption">
          {{ messages.gamification.xpToNext(xpIntoLevel, xpForNextLevel) }}
        </span>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.xp-progress-card { display: flex; flex-direction: column; gap: var(--space-md); }
.xp-progress-card__loading { display: flex; flex-direction: column; gap: var(--space-sm); }
.xp-progress-card__head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); flex-wrap: wrap; }
.xp-progress-card__level { display: inline-flex; align-items: baseline; gap: var(--space-sm); }
.xp-progress-card__level-num { font-family: var(--font-mono); font-size: 40px; line-height: 1; font-weight: 600; letter-spacing: -0.03em; color: var(--foreground); }
.xp-progress-card__level-label { font-size: var(--text-sm); color: var(--foreground-secondary); }
.xp-progress-card__bar { display: flex; flex-direction: column; gap: var(--space-xs); }
.xp-progress-card__bar-caption { font-size: var(--text-xs); color: var(--foreground-secondary); font-family: var(--font-mono); letter-spacing: 0.06em; }
</style>
