<script setup lang="ts">
// StreakCard — chuỗi ngày học liên tục + đông cứng (dữ liệu thật từ store /me/streak).
import { Flame, Snowflake } from 'lucide-vue-next';
import Card from '@/components/ui/Card.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import { messages } from '@/i18n/vi';

defineProps<{
  streakDays: number;
  freezeAvailable: number;
  loading?: boolean;
}>();
</script>

<template>
  <Card class="streak-card">
    <div v-if="loading" class="streak-card__loading" aria-busy="true">
      <Skeleton height="56px" />
    </div>
    <template v-else>
      <div class="streak-card__days">
        <span class="streak-card__flame" aria-hidden="true"><Flame :size="22" /></span>
        <span class="streak-card__num">{{ streakDays }}</span>
        <span class="streak-card__unit">{{ messages.gamification.streakUnit }}</span>
      </div>
      <div class="streak-card__freeze">
        <Snowflake :size="14" aria-hidden="true" />
        <span>{{ messages.gamification.streakFreeze(freezeAvailable) }}</span>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.streak-card { display: flex; flex-direction: column; gap: var(--space-sm); }
.streak-card__loading { display: flex; flex-direction: column; gap: var(--space-sm); }
.streak-card__days { display: flex; align-items: center; gap: var(--space-sm); }
.streak-card__flame { color: var(--warning); display: inline-flex; }
.streak-card__num { font-family: var(--font-mono); font-size: 32px; line-height: 1; font-weight: 600; color: var(--foreground); }
.streak-card__unit { font-size: var(--text-sm); color: var(--foreground-secondary); }
.streak-card__freeze { display: inline-flex; align-items: center; gap: var(--space-xs); font-size: var(--text-xs); color: var(--foreground-secondary); }
</style>
