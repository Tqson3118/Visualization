<script setup lang="ts">
// QuestsView — Màn 23: 5 quest hằng ngày + streak + claim (atomic)
import { computed, onMounted, ref } from 'vue';

import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import type { QuestDto } from '@/api/gamification';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const gamification = useGamificationStore();
const ui = useUiStore();

const loading = ref(true);
const claimingId = ref<number | null>(null);

onMounted(async () => {
  try {
    await gamification.fetchQuests();
  } catch {
    ui.showToast('Không thể tải quest hằng ngày.', 'error');
  } finally {
    loading.value = false;
  }
  void gamification.fetchHearts();
  void gamification.fetchStreak();
});

const doneCount = computed(() => gamification.quests.filter((q) => q.current >= q.target).length);
const allDone = computed(() => gamification.quests.length > 0 && doneCount.value === gamification.quests.length);

const DIFFICULTY: Record<number, { label: string; variant: 'success' | 'warning' | 'danger' }> = {
  0: { label: 'DỄ', variant: 'success' },
  1: { label: 'TB', variant: 'warning' },
  2: { label: 'KHÓ', variant: 'danger' },
};

async function claim(quest: QuestDto): Promise<void> {
  claimingId.value = quest.id;
  try {
    await gamification.claimQuest(quest.id);
    ui.showToast(`+${quest.rewardGems} 💎, +${quest.rewardXp} XP`, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Phần thưởng đã được nhận hoặc chưa hoàn thành.', 'error');
  } finally {
    claimingId.value = null;
  }
}
</script>

<template>
  <main class="quests container">
    <header class="quests__header">
      <div>
        <h1 class="quests__title">🏆 Thử thách hằng ngày</h1>
        <p class="text-muted quests__sub">
          Đã hoàn thành {{ doneCount }}/{{ gamification.quests.length }} · Reset 00:00 (UTC+7)
        </p>
      </div>
      <div class="quests__streak">
        🔥 {{ gamification.streakDays }} ngày
        <span v-if="gamification.freezeAvailable > 0" class="quests__freeze">❄️ {{ gamification.freezeAvailable }}</span>
      </div>
    </header>

    <div v-if="loading" class="quests__loading">
      <Skeleton v-for="i in 5" :key="i" height="72px" />
    </div>

    <EmptyState
      v-else-if="gamification.quests.length === 0"
      icon="target"
      title="Hôm nay chưa có thử thách"
      description="Quay lại sau — quest mới sẽ được tạo mỗi ngày."
    />

    <template v-else>
      <div v-if="allDone" class="quests__bonus" role="status">
        ⭐ Hoàn thành 5/5 — nhận thêm <strong>+10 gems</strong> khi claim quest cuối!
      </div>

      <div class="quests__list">
        <article
          v-for="(quest, idx) in gamification.quests"
          :key="quest.id"
          class="quests__card card"
          :class="{ 'quests__card--claimed': quest.claimed }"
        >
          <Badge :variant="DIFFICULTY[idx % 3]?.variant ?? 'muted'">
            {{ DIFFICULTY[idx % 3]?.label ?? 'TB' }}
          </Badge>
          <div class="quests__body">
            <p class="quests__title-text">{{ quest.title }}</p>
            <p class="text-muted quests__desc">{{ quest.description }}</p>
            <ProgressBar
              :value="quest.target === 0 ? 0 : Math.min(100, Math.round((quest.current / quest.target) * 100))"
              :variant="quest.current >= quest.target ? 'success' : 'default'"
              show-label
            />
          </div>
          <div class="quests__reward">
            <span class="quests__reward-value">+{{ quest.rewardGems }} 💎</span>
            <Button
              size="sm"
              :disabled="quest.current < quest.target || quest.claimed"
              :loading="claimingId === quest.id"
              @click="claim(quest)"
            >
              {{ quest.claimed ? '✅ Đã nhận' : quest.current >= quest.target ? '▶ Nhận thưởng' : 'Đang chạy...' }}
            </Button>
          </div>
        </article>
      </div>

      <footer class="quests__footer text-muted">
        Quy tắc: 5 quest/ngày (2 DỄ + 2 TB + 1 KHÓ) · claim atomic chống double-spend ·
        thưởng tim khi tim đầy tự đổi +5 gems.
      </footer>
    </template>
  </main>
</template>

<style scoped>
.quests {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 720px;
}

.quests__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.quests__title { font-size: var(--text-2xl); }
.quests__sub { font-size: var(--text-sm); margin-top: 4px; }

.quests__streak {
  font-size: var(--text-md);
  font-weight: 800;
  background: color-mix(in srgb, var(--color-warning) 14%, transparent);
  border: 1px solid var(--color-warning);
  padding: 6px 14px;
  border-radius: var(--radius-full);
}

.quests__freeze { font-size: var(--text-sm); color: var(--color-info); }

.quests__bonus {
  background: color-mix(in srgb, var(--color-warning) 14%, transparent);
  border: 1px solid var(--color-warning);
  border-radius: var(--radius-lg);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  font-weight: 600;
}

.quests__list { display: flex; flex-direction: column; gap: var(--space-sm); }

.quests__card {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md);
}

.quests__card--claimed { opacity: 0.75; }

.quests__body { flex: 1; display: flex; flex-direction: column; gap: 6px; min-width: 0; }

.quests__title-text { font-weight: 700; }

.quests__desc { font-size: var(--text-xs); }

.quests__reward { display: flex; flex-direction: column; align-items: flex-end; gap: var(--space-sm); }

.quests__reward-value { font-weight: 800; color: var(--color-warning); font-size: var(--text-sm); white-space: nowrap; }

.quests__footer { font-size: var(--text-xs); }

@media (max-width: 640px) {
  .quests__card { flex-direction: column; align-items: stretch; }
  .quests__reward { flex-direction: row; justify-content: space-between; align-items: center; }
}
</style>
