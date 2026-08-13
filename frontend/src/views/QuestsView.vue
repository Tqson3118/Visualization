<script setup lang="ts">
// QuestsView — Màn 23: 5 quest hằng ngày + streak + claim (atomic).
// H-D: hero gradient Aurora + streak/freeze chip, quest card shadcn
// (difficulty Badge + reward chip + ProgressBar + CTA), i18n quests.*.
import { computed, onMounted, ref } from 'vue';
import { Check, Flame, Gem, Snowflake, Sparkles, Target } from 'lucide-vue-next';

import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import type { QuestDto } from '@/api/gamification';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { messages } from '@/i18n/vi';

const gamification = useGamificationStore();
const ui = useUiStore();

const loading = ref(true);
const claimingId = ref<number | null>(null);

onMounted(async () => {
  try {
    await gamification.fetchQuests();
  } catch {
    ui.showToast(messages.quests.loadError, 'error');
  } finally {
    loading.value = false;
  }
  void gamification.fetchHearts();
  void gamification.fetchStreak();
});

const doneCount = computed(() => gamification.quests.filter((q) => q.current >= q.target).length);
const allDone = computed(() => gamification.quests.length > 0 && doneCount.value === gamification.quests.length);

const DIFFICULTY: Record<number, { label: string; variant: 'success' | 'warning' | 'danger' }> = {
  0: { label: messages.quests.difficulty[0], variant: 'success' },
  1: { label: messages.quests.difficulty[1], variant: 'warning' },
  2: { label: messages.quests.difficulty[2], variant: 'danger' },
};

async function claim(quest: QuestDto): Promise<void> {
  claimingId.value = quest.id;
  try {
    await gamification.claimQuest(quest.id);
    ui.showToast(messages.quests.claimedToast(quest.rewardGems, quest.rewardXp), 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Phần thưởng đã được nhận hoặc chưa hoàn thành.', 'error');
  } finally {
    claimingId.value = null;
  }
}
</script>

<template>
  <main class="quests container">
    <!-- Hero gradient Aurora (palette gamification) -->
    <header class="quests__hero">
      <div class="quests__hero-body">
        <span class="quests__hero-icon" aria-hidden="true"><Target :size="24" /></span>
        <div class="quests__hero-title-wrap">
          <h1 class="quests__title">{{ messages.quests.title }}</h1>
          <p class="quests__sub">{{ messages.quests.subtitle(doneCount, gamification.quests.length) }}</p>
        </div>
        <Badge variant="primary" class="quests__hero-badge">{{ messages.quests.badge }}</Badge>
      </div>
      <div class="quests__chips">
        <span class="quests__streak">
          <Flame :size="15" aria-hidden="true" />
          {{ gamification.streakDays }}
          <span class="quests__streak-label">{{ messages.quests.streakDays }}</span>
        </span>
        <span v-if="gamification.freezeAvailable > 0" class="quests__freeze" :title="messages.quests.freezeAvailable">
          <Snowflake :size="14" aria-hidden="true" /> {{ gamification.freezeAvailable }}
        </span>
      </div>
    </header>

    <div v-if="loading" class="quests__loading" aria-busy="true">
      <Skeleton v-for="i in 5" :key="i" height="110px" />
    </div>

    <EmptyState
      v-else-if="gamification.quests.length === 0"
      icon="target"
      :title="messages.quests.emptyTitle"
      :description="messages.quests.emptyDesc"
    />

    <template v-else>
      <div v-if="allDone" class="quests__bonus" role="status">
        <Sparkles :size="16" aria-hidden="true" />
        <span>{{ messages.quests.bonus }}</span>
      </div>

      <div class="quests__list">
        <article
          v-for="(quest, idx) in gamification.quests"
          :key="quest.id"
          class="quests__card card"
          :class="{
            'quests__card--claimed': quest.claimed,
            'quests__card--ready': quest.current >= quest.target && !quest.claimed,
          }"
        >
          <div class="quests__head">
            <Badge :variant="DIFFICULTY[idx % 3]?.variant ?? 'muted'">
              {{ DIFFICULTY[idx % 3]?.label ?? messages.quests.difficulty[1] }}
            </Badge>
            <span class="quests__reward-value text-amber-700 dark:text-amber-400">
              <Gem :size="14" aria-hidden="true" /> {{ messages.quests.reward(quest.rewardGems) }}
            </span>
          </div>

          <div class="quests__body">
            <p class="quests__title-text">{{ quest.title }}</p>
            <p class="quests__desc text-muted">{{ quest.description }}</p>
            <ProgressBar
              :value="quest.target === 0 ? 0 : Math.min(100, Math.round((quest.current / quest.target) * 100))"
              :variant="quest.current >= quest.target ? 'success' : 'default'"
              show-label
            />
          </div>

          <div class="quests__foot">
            <span class="quests__xp text-muted">{{ messages.quests.rewardXp(quest.rewardXp) }}</span>
            <Button
              size="sm"
              :disabled="quest.current < quest.target || quest.claimed"
              :loading="claimingId === quest.id"
              @click="claim(quest)"
            >
              <Check v-if="quest.claimed" :size="14" aria-hidden="true" />
              {{ quest.claimed ? messages.quests.claimed : quest.current >= quest.target ? messages.quests.claim : messages.quests.inProgress }}
            </Button>
          </div>
        </article>
      </div>

      <footer class="quests__footer text-muted">{{ messages.quests.footer }}</footer>
    </template>
  </main>
</template>

<style scoped>
.quests {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 760px;
}

/* ── Hero gradient Aurora (palette 1 — gamification) ── */
.quests__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.quests__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.quests__hero::before {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  top: -120px;
  right: -60px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.quests__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.quests__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.quests__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.quests__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.quests__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.quests__hero-badge { margin-left: auto; }

.quests__chips { display: flex; align-items: center; gap: var(--space-sm); flex-wrap: wrap; }

.quests__streak {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-sm);
  font-weight: 800;
  font-variant-numeric: tabular-nums;
  background-image: var(--gradient-sunset);
  color: var(--color-on-primary);
  padding: 5px 14px;
  border-radius: var(--radius-full);
  box-shadow: var(--shadow-sm);
}

.quests__streak-label { font-weight: 600; font-size: var(--text-xs); opacity: 0.92; }

.quests__freeze {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: var(--text-sm);
  font-weight: 700;
  color: var(--color-info);
  background: color-mix(in srgb, var(--color-info) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-info) 45%, var(--color-border));
  padding: 4px 12px;
  border-radius: var(--radius-full);
  font-variant-numeric: tabular-nums;
}

.quests__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.quests__bonus {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  background: color-mix(in srgb, var(--color-success) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 45%, var(--color-border));
  border-radius: var(--radius-lg);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-success);
}

.quests__list { display: flex; flex-direction: column; gap: var(--space-sm); }

.quests__card { display: flex; flex-direction: column; gap: var(--space-sm); padding: var(--space-md) var(--space-lg); }

.quests__card--claimed { opacity: 0.7; }

.quests__card--ready {
  border-color: color-mix(in srgb, var(--color-success) 45%, var(--color-border));
  background: linear-gradient(180deg, color-mix(in srgb, var(--color-success) 5%, var(--color-card)), var(--color-card));
}

.quests__head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }

.quests__reward-value {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-weight: 800;
  font-size: var(--text-sm);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.quests__body { display: flex; flex-direction: column; gap: 6px; min-width: 0; }

.quests__title-text { font-weight: 700; font-size: var(--text-md); }

.quests__desc { font-size: var(--text-xs); line-height: 1.55; }

.quests__foot { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }

.quests__xp { font-size: var(--text-xs); font-weight: 600; }

.quests__footer { font-size: var(--text-xs); }

@media (max-width: 640px) {
  .quests__hero-badge { margin-left: 0; }
}
</style>
