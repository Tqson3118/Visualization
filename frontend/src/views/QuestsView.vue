<script setup lang="ts">
// QuestsView — Màn 23: 5 quest hằng ngày + streak + claim (atomic).
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 (không gradient/blob),
// streak = block-token resolved (dữ liệu tuần tự — luôn tối), reward/XP mono,
// khoảnh khắc đầu tư duy nhất: confetti khi hoàn thành 5/5 (disableForReducedMotion).
import { computed, onMounted, ref } from 'vue';
import { Check, Flame, Gem, Snowflake, Sparkles, Target } from 'lucide-vue-next';
import confetti from 'canvas-confetti';

import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import type { QuestDto } from '@/api/gamification';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
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

/** Khoảnh khắc đầu tư duy nhất của màn: ăn mừng khi claim quest cuối cùng trong ngày. */
function celebrate(): void {
  confetti({ particleCount: 90, spread: 70, origin: { y: 0.7 }, disableForReducedMotion: true });
}

async function claim(quest: QuestDto): Promise<void> {
  claimingId.value = quest.id;
  try {
    await gamification.claimQuest(quest.id);
    ui.showToast(messages.quests.claimedToast(quest.rewardGems, quest.rewardXp), 'success');
    if (gamification.quests.every((q) => q.claimed)) celebrate();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Phần thưởng đã được nhận hoặc chưa hoàn thành.', 'error');
  } finally {
    claimingId.value = null;
  }
}
</script>

<template>
  <main class="quests container">
    <!-- Hero — surface band level-2 (không gradient, không blob) + strip mono -->
    <header class="quests__hero">
      <div class="quests__hero-body">
        <span class="quests__hero-icon" aria-hidden="true"><Target :size="20" /></span>
        <div class="quests__hero-title-wrap">
          <h1 class="quests__title">{{ messages.quests.title }}</h1>
          <p class="quests__sub">{{ messages.quests.subtitle(doneCount, gamification.quests.length) }}</p>
        </div>
        <span class="quests__hero-strip" aria-hidden="true">
          <span class="quests__strip-block" />
          {{ doneCount }}/{{ gamification.quests.length }} DONE
        </span>
      </div>
      <div class="quests__chips">
        <BlockToken size="sm" tone="resolved" label="Streak" :value="gamification.streakDays" index="ngày" />
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
            <span class="quests__reward-value">
              <Gem :size="14" aria-hidden="true" /> {{ messages.quests.reward(quest.rewardGems) }}
            </span>
          </div>

          <div class="quests__body">
            <p class="quests__title-text">{{ quest.title }}</p>
            <p class="quests__desc">{{ quest.description }}</p>
            <ProgressBar
              :value="quest.target === 0 ? 0 : Math.min(100, Math.round((quest.current / quest.target) * 100))"
              :variant="quest.current >= quest.target ? 'success' : 'default'"
              show-label
            />
          </div>

          <div class="quests__foot">
            <span class="quests__xp">{{ messages.quests.rewardXp(quest.rewardXp) }}</span>
            <Button
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

      <footer class="quests__footer">{{ messages.quests.footer }}</footer>
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

/* Card dùng class global .card (global.css có shadow-md) — §6 cấm shadow card → override */
.quests .card {
  box-shadow: none;
}

/* ── Hero — surface band level-2 (DESIGN.md §6) ── */
.quests__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-xl);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
}

.quests__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.quests__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.quests__hero-title-wrap { display: flex; flex-direction: column; gap: var(--space-xs); }

.quests__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.quests__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

/* Strip mono dữ liệu — block-token nhỏ (resolved = quest đã hoàn thành) */
.quests__hero-strip {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-resolved) 30%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-xs) var(--space-sm);
  white-space: nowrap;
}

.quests__strip-block {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-sm);
  background: var(--color-resolved);
}

.quests__chips { display: flex; align-items: center; gap: var(--space-sm); flex-wrap: wrap; }

.quests__freeze {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-info);
  background: color-mix(in srgb, var(--color-info) 12%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-info) 45%, var(--color-border));
  padding: var(--space-xs) 12px;
  border-radius: var(--radius-full);
  font-variant-numeric: tabular-nums;
  min-height: 24px;
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

/* Trạng thái sẵn sàng claim — chỉ border + tint (KHÔNG gradient trang trí) */
.quests__card--ready {
  border-color: color-mix(in srgb, var(--color-success) 45%, var(--color-border));
  background: color-mix(in srgb, var(--color-success) 5%, var(--color-card));
}

.quests__head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }

.quests__reward-value {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-weight: 600;
  font-size: var(--text-sm);
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

.quests__body { display: flex; flex-direction: column; gap: var(--space-sm); min-width: 0; }

.quests__title-text { font-weight: 600; font-size: var(--text-md); }

.quests__desc { font-size: var(--text-xs); line-height: 1.55; color: var(--color-text-muted); }

.quests__foot { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }

.quests__xp {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  font-weight: 600;
}

.quests__footer { font-size: var(--text-xs); color: var(--color-text-muted); }
</style>
