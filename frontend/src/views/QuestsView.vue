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

const sortedQuests = computed(() => {
  return [...gamification.quests].sort((a, b) => {
    const scoreA = a.claimed ? 2 : (a.current >= a.target ? 0 : 1);
    const scoreB = b.claimed ? 2 : (b.current >= b.target ? 0 : 1);
    return scoreA - scoreB;
  });
});

const DIFFICULTY: Record<number, { label: string; cls: string }> = {
  0: { label: messages.quests.difficulty[0], cls: 'quests__diff--easy' },
  1: { label: messages.quests.difficulty[1], cls: 'quests__diff--mid' },
  2: { label: messages.quests.difficulty[2], cls: 'quests__diff--hard' },
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
      <span class="quests__hero-spark quests__hero-spark--1" aria-hidden="true">✦</span>
      <span class="quests__hero-spark quests__hero-spark--2" aria-hidden="true">✦</span>
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
          v-for="(quest, idx) in sortedQuests"
          :key="quest.id"
          class="quests__card card"
          :class="{
            'quests__card--claimed': quest.claimed,
            'quests__card--ready': quest.current >= quest.target && !quest.claimed,
          }"
        >
          <div class="quests__head">
            <Badge class="quests__diff" :class="DIFFICULTY[quest.difficulty ?? 0]?.cls ?? 'quests__diff--mid'">
              {{ DIFFICULTY[quest.difficulty ?? 0]?.label ?? messages.quests.difficulty[1] }}
            </Badge>
            <span class="quests__reward-value">
              <Gem :size="14" aria-hidden="true" /> {{ messages.quests.reward(quest.rewardGems) }}
            </span>
          </div>

          <div class="quests__body">
            <p class="quests__title-text">{{ quest.title }}</p>
            <p class="quests__desc">{{ quest.description }}</p>
            <div class="quests__progress" :class="{ 'quests__progress--done': quest.current >= quest.target }">
              <ProgressBar
                :value="quest.target === 0 ? 0 : Math.min(100, Math.round((quest.current / quest.target) * 100))"
                :variant="quest.current >= quest.target ? 'success' : 'default'"
                show-label
              />
            </div>
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
  /* Bộ tím (khớp chủ đề Shop) — accent riêng cho trang thử thách */
  --q-purple: #8b5cf6;
  --q-purple-light: #a78bfa;
  --q-purple-dark: #6d28d9;
  /* Remap token shadcn primary (teal) → tím NGAY TRONG scope trang:
     Button, ProgressBar, Badge, BlockToken đều tự đổi sang tím. */
  --primary: oklch(0.56 0.24 293);
  --primary-foreground: oklch(0.99 0 0);
  --ring: oklch(0.56 0.24 293);
  /* BlockToken streak đang dùng --resolved (xanh lá) → tím nhạt */
  --data-core: var(--q-purple);
  --resolved: var(--q-purple-light);

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

/* ── Hero — surface band level-2 + tint tím + texture chấm nhẹ ── */
.quests__hero {
  position: relative;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-xl);
  border: 1px solid color-mix(in srgb, var(--q-purple) 22%, var(--color-border-subtle));
  border-radius: var(--radius-lg);
  background:
    radial-gradient(120% 120% at 100% 0%, color-mix(in srgb, var(--q-purple) 10%, transparent) 0%, transparent 55%),
    var(--color-card-raised);
}

/* Texture chấm tím rất nhạt — bớt đơn điệu, không màu mè */
.quests__hero::before {
  content: "";
  position: absolute;
  inset: 0;
  pointer-events: none;
  background-image: radial-gradient(color-mix(in srgb, var(--q-purple) 16%, transparent) 1px, transparent 1px);
  background-size: 18px 18px;
  opacity: 0.5;
}

.quests__hero-spark {
  position: absolute;
  color: var(--q-purple-light);
  font-size: var(--text-sm);
  animation: quests-blink 2.4s ease-in-out infinite;
}

.quests__hero-spark--1 { top: 12px; right: 14%; }
.quests__hero-spark--2 { bottom: 14px; right: 4%; animation-delay: 0.9s; }

.quests__hero-body { position: relative; display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.quests__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--q-purple) 14%, var(--color-muted));
  color: var(--q-purple);
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
  border: 1px solid color-mix(in srgb, var(--q-purple-light) 40%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-xs) var(--space-sm);
  white-space: nowrap;
}

.quests__strip-block {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-sm);
  background: var(--q-purple-light);
  box-shadow: 0 0 6px color-mix(in srgb, var(--q-purple) 60%, transparent);
}

.quests__chips { display: flex; align-items: center; gap: var(--space-sm); flex-wrap: wrap; }

.quests__freeze {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--q-purple-dark);
  background: color-mix(in srgb, var(--q-purple) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--q-purple) 40%, var(--color-border));
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
  background: color-mix(in srgb, var(--q-purple) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--q-purple) 45%, var(--color-border));
  border-radius: var(--radius-lg);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--q-purple-dark);
}

.quests__bonus svg { color: var(--q-purple); }

.quests__list { display: flex; flex-direction: column; gap: var(--space-sm); }

.quests__card {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md) var(--space-lg);
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

/* Dải accent tím bên trái mỗi card — bớt đơn điệu */
.quests__card::before {
  content: "";
  position: absolute;
  left: 0;
  top: 14px;
  bottom: 14px;
  width: 3px;
  border-radius: var(--radius-sm);
  background: color-mix(in srgb, var(--q-purple) 32%, transparent);
}

.quests__card:hover { border-color: color-mix(in srgb, var(--q-purple) 45%, var(--color-border-subtle)); }

.quests__card--claimed { opacity: 0.7; }

/* Trạng thái sẵn sàng claim — tint tím + dải tím đậm */
.quests__card--ready {
  border-color: color-mix(in srgb, var(--q-purple) 50%, var(--color-border));
  background: color-mix(in srgb, var(--q-purple) 6%, var(--color-card));
}

.quests__card--ready::before {
  background: var(--q-purple);
  box-shadow: 0 0 8px color-mix(in srgb, var(--q-purple) 50%, transparent);
}

/* Difficulty badge — 3 nấc tím (DỄ/TB/KHÓ) */
.quests__diff {
  border: 1px solid transparent;
}

.quests__diff--easy {
  background: color-mix(in srgb, var(--q-purple) 12%, var(--color-card));
  color: var(--q-purple-dark);
}

.quests__diff--mid {
  background: color-mix(in srgb, var(--q-purple) 22%, var(--color-card));
  color: var(--q-purple-dark);
}

.quests__diff--hard {
  background: var(--q-purple-dark);
  border-color: color-mix(in srgb, var(--q-purple-dark) 40%, transparent);
  color: #fff;
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

.quests__reward-value svg { color: var(--q-purple); }

.quests__body { display: flex; flex-direction: column; gap: var(--space-sm); min-width: 0; }

.quests__title-text { font-weight: 600; font-size: var(--text-md); }

.quests__desc { font-size: var(--text-xs); line-height: 1.55; color: var(--color-text-muted); }

/* Thanh tiến trình tím (chỉ áp dụng cho indicator, giữ nền track xám trong suốt) */
.quests__progress :deep([role="progressbar"]) {
  background: rgba(255, 255, 255, 0.08) !important;
}

.quests__progress :deep([data-reka-progress-indicator]),
.quests__progress :deep([role="progressbar"] > div) {
  background: var(--q-purple) !important;
}

.quests__progress--done :deep([data-reka-progress-indicator]),
.quests__progress--done :deep([role="progressbar"] > div) {
  background: linear-gradient(90deg, var(--q-purple), var(--q-purple-light)) !important;
  box-shadow: 0 0 8px color-mix(in srgb, var(--q-purple) 40%, transparent);
}

.quests__foot { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }

.quests__xp {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--q-purple-dark);
  font-weight: 600;
}

.quests__footer { font-size: var(--text-xs); color: var(--color-text-muted); }

@keyframes quests-blink {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.35; }
}

@media (prefers-reduced-motion: reduce) {
  .quests__hero-spark {
    animation: none;
  }
}
</style>
