<script setup lang="ts">
// BadgeGrid — huy hiệu đã mở khóa vs chưa mở khóa (locked/unlocked khác biệt rõ).
import { Lock, Medal } from 'lucide-vue-next';
import Card from '@/components/ui/Card.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { messages } from '@/i18n/vi';
import type { AchievementDto } from '@/api/gamification';

const props = defineProps<{
  badges: AchievementDto[];
  loading?: boolean;
}>();

const unlockedCount = () => props.badges.filter((b) => b.earnedAt !== null).length;
</script>

<template>
  <Card class="badge-grid">
    <div class="badge-grid__head">
      <h2 class="badge-grid__title">
        <span class="badge-grid__icon" aria-hidden="true"><Medal :size="16" /></span>
        {{ messages.gamification.badgesTitle }}
      </h2>
      <span class="badge-grid__count">{{ unlockedCount() }} / {{ badges.length }}</span>
    </div>

    <div v-if="loading" class="badge-grid__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="64px" />
    </div>

    <EmptyState
      v-else-if="badges.length === 0"
      icon="medal"
      :title="messages.gamification.badgesEmptyTitle"
      :description="messages.gamification.badgesEmptyDesc"
    />

    <ul v-else class="badge-grid__list">
      <li
        v-for="badge in badges"
        :key="badge.id"
        class="badge-grid__item"
        :class="{ 'badge-grid__item--locked': badge.earnedAt === null, 'badge-grid__item--unlocked': badge.earnedAt !== null }"
        :title="badge.description ?? ''"
      >
        <span class="badge-grid__item-icon" aria-hidden="true">
          <img v-if="badge.iconUrl" :src="badge.iconUrl" :alt="badge.name" class="badge-grid__item-img" />
          <Medal v-else :size="22" />
        </span>
        <span class="badge-grid__item-name">{{ badge.name }}</span>
        <Lock v-if="badge.earnedAt === null" class="badge-grid__item-lock" :size="13" aria-hidden="true" />
        <span v-else class="badge-grid__item-check" aria-hidden="true">✓</span>
      </li>
    </ul>
  </Card>
</template>

<style scoped>
.badge-grid { display: flex; flex-direction: column; gap: var(--space-md); }
.badge-grid__head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }
.badge-grid__title { display: inline-flex; align-items: center; gap: var(--space-sm); font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.015em; }
.badge-grid__icon { color: var(--foreground-secondary); display: inline-flex; }
.badge-grid__count { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--foreground-secondary); letter-spacing: 0.08em; }
.badge-grid__loading, .badge-grid__list { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: var(--space-sm); margin: 0; padding: 0; list-style: none; }
.badge-grid__item {
  display: flex; flex-direction: column; align-items: center; gap: var(--space-xs);
  padding: var(--space-md); border: 1px solid var(--border); border-radius: var(--radius-md);
  position: relative; text-align: center;
}
.badge-grid__item--unlocked { border-color: color-mix(in srgb, var(--success) 40%, var(--border)); }
.badge-grid__item--locked { opacity: 0.55; filter: grayscale(0.8); }
.badge-grid__item-icon { color: var(--success); display: inline-flex; }
.badge-grid__item--locked .badge-grid__item-icon { color: var(--foreground-tertiary); }
.badge-grid__item-img { width: 28px; height: 28px; object-fit: contain; }
.badge-grid__item-name { font-size: var(--text-xs); font-weight: 500; color: var(--foreground); }
.badge-grid__item-lock { position: absolute; top: var(--space-sm); right: var(--space-sm); color: var(--foreground-tertiary); }
.badge-grid__item-check { position: absolute; top: var(--space-sm); right: var(--space-sm); color: var(--success); font-family: var(--font-mono); font-size: var(--text-sm); }
</style>
