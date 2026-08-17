<script setup lang="ts">
// QuestProgressCard — daily quests với 3 trạng thái: todo / in progress / completed.
import { computed } from 'vue';
import { Check, Circle, Loader2, Target } from 'lucide-vue-next';
import Card from '@/components/ui/Card.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { messages } from '@/i18n/vi';
import type { QuestDto } from '@/api/gamification';

const props = defineProps<{
  quests: QuestDto[];
  loading?: boolean;
  error?: string | null;
}>();

const group = (q: QuestDto): 'done' | 'progress' | 'todo' =>
  q.current >= q.target ? 'done' : q.current > 0 ? 'progress' : 'todo';

const done = computed(() => props.quests.filter((q) => group(q) === 'done').length);
const inProgress = computed(() => props.quests.filter((q) => group(q) === 'progress').length);
const todo = computed(() => props.quests.filter((q) => group(q) === 'todo').length);

const pct = (q: QuestDto): number =>
  q.target === 0 ? 0 : Math.min(100, Math.round((q.current / q.target) * 100));
</script>

<template>
  <Card class="quest-progress-card">
    <div class="quest-progress-card__head">
      <h2 class="quest-progress-card__title">
        <span class="quest-progress-card__icon" aria-hidden="true"><Target :size="16" /></span>
        {{ messages.gamification.questsTitle }}
      </h2>
      <span class="quest-progress-card__count">{{ done }} / {{ quests.length }}</span>
    </div>

    <div v-if="loading" class="quest-progress-card__list" aria-busy="true">
      <Skeleton v-for="i in 3" :key="i" height="48px" />
    </div>

    <EmptyState
      v-else-if="quests.length === 0 && !error"
      icon="target"
      :title="messages.gamification.questsEmptyTitle"
      :description="messages.gamification.questsEmptyDesc"
    />

    <p v-else-if="error" class="quest-progress-card__error" role="alert">{{ error }}</p>

    <ul v-else class="quest-progress-card__list">
      <li
        v-for="quest in quests"
        :key="quest.id"
        class="quest-progress-card__row"
        :class="`quest-progress-card__row--${group(quest)}`"
      >
        <span class="quest-progress-card__row-status" aria-hidden="true">
          <Check v-if="group(quest) === 'done'" :size="15" />
          <Loader2 v-else-if="group(quest) === 'progress'" :size="15" />
          <Circle v-else :size="11" />
        </span>
        <div class="quest-progress-card__row-main">
          <div class="quest-progress-card__row-title-line">
            <p class="quest-progress-card__row-title">{{ quest.title }}</p>
            <Badge v-if="quest.claimed" variant="success">{{ messages.gamification.questClaimed }}</Badge>
          </div>
          <ProgressBar
            :value="pct(quest)"
            :variant="group(quest) === 'done' ? 'success' : group(quest) === 'progress' ? 'warning' : 'default'"
            size="sm"
          />
          <span class="quest-progress-card__row-meta">
            {{ quest.current }} / {{ quest.target }} · +{{ quest.rewardXp }} XP · +{{ quest.rewardGems }} gems
            <template v-if="todo > 0">· {{ messages.gamification.questTodo }}</template>
          </span>
        </div>
      </li>
    </ul>
    <p v-if="quests.length > 0 && todo > 0" class="quest-progress-card__legend">
      <span class="quest-progress-card__legend-dot" aria-hidden="true" />
      {{ messages.gamification.questTodoNote }}
    </p>
  </Card>
</template>

<style scoped>
.quest-progress-card { display: flex; flex-direction: column; gap: var(--space-md); }
.quest-progress-card__head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }
.quest-progress-card__title { display: inline-flex; align-items: center; gap: var(--space-sm); font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.015em; }
.quest-progress-card__icon { color: var(--foreground-secondary); display: inline-flex; }
.quest-progress-card__count { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--foreground-secondary); letter-spacing: 0.08em; }
.quest-progress-card__list { display: flex; flex-direction: column; gap: var(--space-sm); margin: 0; padding: 0; list-style: none; }
.quest-progress-card__row { display: flex; gap: var(--space-sm); align-items: flex-start; padding: var(--space-sm); border: 1px solid var(--border); border-radius: var(--radius-md); }
.quest-progress-card__row--done { border-color: color-mix(in srgb, var(--success) 35%, var(--border)); }
.quest-progress-card__row--progress { border-color: color-mix(in srgb, var(--warning) 35%, var(--border)); }
.quest-progress-card__row-status { color: var(--foreground-secondary); display: inline-flex; margin-top: 2px; }
.quest-progress-card__row--done .quest-progress-card__row-status { color: var(--success); }
.quest-progress-card__row--progress .quest-progress-card__row-status { color: var(--warning); }
.quest-progress-card__row-main { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--space-xs); }
.quest-progress-card__row-title-line { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }
.quest-progress-card__row-title { font-size: var(--text-sm); font-weight: 500; }
.quest-progress-card__row-meta { font-size: var(--text-xs); color: var(--foreground-secondary); font-family: var(--font-mono); }
.quest-progress-card__error { font-size: var(--text-sm); color: var(--destructive); }
.quest-progress-card__legend { font-size: var(--text-xs); color: var(--foreground-secondary); display: inline-flex; align-items: center; gap: var(--space-xs); }
.quest-progress-card__legend-dot { width: 8px; height: 8px; border-radius: 50%; background: var(--foreground-tertiary); }
</style>
