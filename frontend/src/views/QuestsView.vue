<script setup lang="ts">
// QuestsView — Màn 23: 5 quest hằng ngày + streak + claim (atomic).
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 (không gradient/blob),
// streak = block-token resolved (dữ liệu tuần tự — luôn tối), reward/XP mono,
// khoảnh khắc đầu tư duy nhất: confetti khi hoàn thành 5/5 (disableForReducedMotion).
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
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
import AnimatedNumber from '@/components/ui/AnimatedNumber.vue';
import { messages } from '@/i18n/vi';

const gamification = useGamificationStore();
const ui = useUiStore();

const loading = ref(true);
const claimingId = ref<number | null>(null);
/** Pop +XP vừa claim (count-up qua AnimatedNumber) — tự ẩn sau 2s */
const claimPop = ref<{ questId: number; xp: number; gems: number } | null>(null);
let popTimer: ReturnType<typeof setTimeout> | undefined;

/** % hiển thị của mỗi quest — animate từ 0 → thật khi load (progress bar fill animated) */
const shownPct = reactive<Record<number, number>>({});
let rafIds: number[] = [];
let animTimers: ReturnType<typeof setTimeout>[] = [];

const reducedMotion =
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

const pctOf = (quest: QuestDto): number =>
  quest.target === 0 ? 0 : Math.min(100, Math.round((quest.current / quest.target) * 100));

function animatePct(id: number, to: number): void {
  if (reducedMotion) {
    shownPct[id] = to;
    return;
  }
  const start = performance.now();
  const duration = 650;
  const tick = (now: number): void => {
    const p = Math.min((now - start) / duration, 1);
    const eased = p === 1 ? 1 : 1 - Math.pow(2, -10 * p);
    shownPct[id] = (to - 0) * eased;
    if (p < 1) rafIds.push(requestAnimationFrame(tick));
  };
  rafIds.push(requestAnimationFrame(tick));
}

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

onUnmounted(() => {
  rafIds.forEach((id) => cancelAnimationFrame(id));
  animTimers.forEach((t) => clearTimeout(t));
  if (popTimer) clearTimeout(popTimer);
});

/** Khi quest đã load xong → chạy animated fill từng card (stagger). */
watch(
  () => loading.value,
  (v) => {
    if (!v && gamification.quests.length > 0) {
      animTimers.forEach((t) => clearTimeout(t));
      gamification.quests.forEach((q, i) => {
        animTimers.push(
          setTimeout(() => animatePct(q.id, pctOf(q)), 100 + i * 90),
        );
      });
    }
  },
);

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

/** Burst nhỏ ngay tại nút claim mỗi lần nhận thưởng (disableForReducedMotion bật sẵn). */
function celebrateSmall(): void {
  confetti({
    particleCount: 36,
    spread: 55,
    startVelocity: 26,
    origin: { y: 0.75 },
    disableForReducedMotion: true,
  });
}

async function claim(quest: QuestDto): Promise<void> {
  claimingId.value = quest.id;
  try {
    await gamification.claimQuest(quest.id);
    ui.showToast(messages.quests.claimedToast(quest.rewardGems, quest.rewardXp), 'success');
    // Count-up XP + burst confetti ngay tại quest vừa claim
    claimPop.value = { questId: quest.id, xp: quest.rewardXp, gems: quest.rewardGems };
    if (popTimer) clearTimeout(popTimer);
    popTimer = setTimeout(() => {
      claimPop.value = null;
    }, 2000);
    celebrateSmall();
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

    <!-- Empty state: giữ nguyên copy, thêm icon animation nhẹ (glow thở phía sau) -->
    <div v-else-if="gamification.quests.length === 0" class="quests__empty">
      <span class="quests__empty-glow" aria-hidden="true" />
      <EmptyState
        icon="target"
        :title="messages.quests.emptyTitle"
        :description="messages.quests.emptyDesc"
      />
    </div>

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
            <!-- Reward badge — glow khi quest ≥ 80% (glow-resolved ngữ nghĩa thành công) -->
            <span
              class="quests__reward-value"
              :class="{ 'quests__reward-value--hot': pctOf(quest) >= 80 }"
            >
              <Gem :size="14" aria-hidden="true" /> {{ messages.quests.reward(quest.rewardGems) }}
            </span>
          </div>

          <div class="quests__body">
            <p class="quests__title-text">{{ quest.title }}</p>
            <p class="quests__desc">{{ quest.description }}</p>
            <ProgressBar
              :value="shownPct[quest.id] ?? 0"
              :variant="quest.current >= quest.target ? 'success' : 'default'"
              show-label
            />
          </div>

          <div class="quests__foot">
            <span class="quests__foot-left">
              <span class="quests__xp">{{ messages.quests.rewardXp(quest.rewardXp) }}</span>
              <!-- Count-up XP sau khi claim (AnimatedNumber + pop) -->
              <Transition name="quest-pop">
                <span v-if="claimPop?.questId === quest.id" class="quests__pop" aria-hidden="true">
                  <AnimatedNumber :value="claimPop.xp" prefix="+" suffix=" XP" :duration="700" immediate />
                  <template v-if="claimPop.gems"> · +{{ claimPop.gems }} <Gem :size="11" /></template>
                </span>
              </Transition>
            </span>
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

/* Empty state — icon animation nhẹ: glow "thở" phía sau (giữ nguyên EmptyState component) */
.quests__empty { position: relative; }

.quests__empty-glow {
  position: absolute;
  top: 18px;
  left: 50%;
  transform: translateX(-50%);
  width: 168px;
  height: 168px;
  border-radius: 50%;
  background: radial-gradient(
    circle,
    color-mix(in srgb, var(--color-data-core) 20%, transparent),
    transparent 70%
  );
  pointer-events: none;
  z-index: 0;
  animation: quests-glow 2.6s ease-in-out infinite;
}

.quests__empty :deep(.empty-state) { position: relative; z-index: 1; }

@keyframes quests-glow {
  0%, 100% { opacity: 0.5; transform: translateX(-50%) scale(0.92); }
  50% { opacity: 1; transform: translateX(-50%) scale(1.06); }
}

/* Reward badge glow khi ≥ 80% (ngữ nghĩa gần hoàn thành — resolved) */
.quests__reward-value--hot {
  color: var(--color-warning);
  background: color-mix(in srgb, var(--color-warning) 12%, transparent);
  border-radius: var(--radius-full);
  padding: 2px 10px;
  box-shadow: 0 0 10px color-mix(in srgb, var(--color-warning) 32%, transparent);
}

/* Pop +XP sau claim — count-up + slide lên nhẹ */
.quests__foot-left {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  min-width: 0;
}

.quests__pop {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-resolved);
  background: color-mix(in srgb, var(--color-resolved) 12%, var(--color-surface));
  border: 1px solid color-mix(in srgb, var(--color-resolved) 40%, transparent);
  border-radius: var(--radius-full);
  padding: 2px 10px;
  white-space: nowrap;
  font-variant-numeric: tabular-nums;
}

.quest-pop-enter-active {
  transition: opacity 260ms cubic-bezier(0.16, 1, 0.3, 1), transform 260ms cubic-bezier(0.16, 1, 0.3, 1);
}

.quest-pop-leave-active {
  transition: opacity 180ms cubic-bezier(0.7, 0, 0.84, 0), transform 180ms cubic-bezier(0.7, 0, 0.84, 0);
}

.quest-pop-enter-from, .quest-pop-leave-to { opacity: 0; transform: translateY(8px) scale(0.94); }

@media (prefers-reduced-motion: reduce) {
  .quests__empty-glow { animation: none; opacity: 0.4; }
  .quest-pop-enter-active, .quest-pop-leave-active { transition: none; }
}

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
