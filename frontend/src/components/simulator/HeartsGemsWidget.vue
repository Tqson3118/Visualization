<script setup lang="ts">
// HeartsGemsWidget — hiển thị tim/gems/streak ở header (SDD §8.7, FR-10.1/10.4)
// Dữ liệu từ gamificationStore (fetchAll). Click tim < max → popover hồi tim + link Premium.
import { computed, onMounted, ref } from 'vue';

import { useGamificationStore } from '@/stores/gamification';
import BaseIcon from '@/components/ui/BaseIcon.vue';
import Tooltip from '@/components/ui/Tooltip.vue';
import { formatDuration } from '@/utils/format';

const gamification = useGamificationStore();
const popoverOpen = ref(false);

onMounted(() => {
  if (gamification.heartsMax === 5 && gamification.hearts === 0) {
    void gamification.fetchHearts();
  }
});

const heartsLabel = computed(() => `${gamification.hearts}/${gamification.heartsMax}`);

/** Đếm ngược tới tim kế tiếp (từ lastHeartAt + 30 phút) */
const nextHeartIn = computed(() => {
  if (!gamification.lastHeartAt) return null;
  const base = new Date(gamification.lastHeartAt).getTime();
  const next = base + 30 * 60 * 1000;
  const diff = Math.max(0, next - Date.now());
  return diff <= 0 ? null : formatDuration(Math.ceil(diff / 1000));
});
</script>

<template>
  <div class="hearts-gems">
    <Tooltip :text="nextHeartIn ? `Tim hồi sau: ${nextHeartIn}` : 'Tim đầy'">
      <button
        type="button"
        class="hearts-gems__chip"
        :class="{ 'hearts-gems__chip--empty': gamification.hearts === 0 }"
        :aria-label="`Tim: ${heartsLabel}`"
        @click="popoverOpen = !popoverOpen"
      >
        <BaseIcon name="flame" :size="14" />
        {{ heartsLabel }}
      </button>
    </Tooltip>
    <span class="hearts-gems__chip" title="Gems" aria-label="Gems">
      <BaseIcon name="sparkles" :size="14" />
      {{ gamification.gems }}
    </span>
    <span v-if="gamification.streakDays > 0" class="hearts-gems__chip" title="Streak" aria-label="Streak">
      🔥 {{ gamification.streakDays }}
    </span>

    <Teleport to="body">
      <Transition name="hearts-pop">
        <div v-if="popoverOpen" class="hearts-gems__pop card">
          <p class="hearts-gems__pop-title">Tim của bạn</p>
          <p v-if="nextHeartIn" class="hearts-gems__pop-desc">
            Tim tiếp theo sau <strong>{{ nextHeartIn }}</strong> (30 phút/tim — bản Free)
          </p>
          <RouterLink class="hearts-gems__pop-link" to="/premium">
            Nâng cấp Premium — hồi tim chỉ 10 phút
          </RouterLink>
          <button type="button" class="hearts-gems__pop-close" @click="popoverOpen = false">
            Đóng
          </button>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<style scoped>
.hearts-gems {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
}

.hearts-gems__chip {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 4px 10px;
  border-radius: var(--radius-full);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  font-size: var(--text-xs);
  font-weight: 700;
  color: var(--color-foreground);
}

.hearts-gems__chip--empty { border-color: var(--color-destructive); color: var(--color-destructive); }

button.hearts-gems__chip { cursor: pointer; }

.hearts-gems__pop {
  position: fixed;
  top: 56px;
  right: var(--space-md);
  z-index: var(--z-raised);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 260px;
  box-shadow: var(--shadow-lg);
}

.hearts-gems__pop-title { font-weight: 700; font-size: var(--text-sm); }

.hearts-gems__pop-desc { font-size: var(--text-sm); color: var(--color-text-muted); }

.hearts-gems__pop-link { font-size: var(--text-sm); font-weight: 600; }

.hearts-gems__pop-close {
  align-self: flex-start;
  background: none;
  border: none;
  color: var(--color-text-muted);
  font-size: var(--text-xs);
  cursor: pointer;
  padding: 0;
}

.hearts-pop-enter-active,
.hearts-pop-leave-active { transition: opacity 150ms ease, transform 150ms ease; }
.hearts-pop-enter-from,
.hearts-pop-leave-to { opacity: 0; transform: translateY(-4px); }
</style>
