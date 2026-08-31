<script setup lang="ts">
// HeartsGemsWidget — hiển thị tim/gems/streak ở header (SDD §8.7, FR-10.1/10.4)
// Dữ liệu từ gamificationStore (fetchHearts → GET /api/v1/me/hearts). Đếm ngược tới tim kế
// theo gói: Free 30 phút/tim (max ≤ 10), Premium 10 phút/tim (max 30) — khớp HeartConfig backend.
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';

import { useGamificationStore } from '@/stores/gamification';
import BaseIcon from '@/components/ui/BaseIcon.vue';
import Tooltip from '@/components/ui/Tooltip.vue';
import { formatDuration } from '@/utils/format';

const gamification = useGamificationStore();
const popoverOpen = ref(false);
let route: ReturnType<typeof useRoute> | null = null;
try {
  route = useRoute();
} catch {
  route = null;
}

function onDocumentClick(e: MouseEvent): void {
  if (!popoverOpen.value) return;
  const target = e.target as HTMLElement | null;
  if (target?.closest('.hearts-gems') || target?.closest('.hearts-gems__pop')) {
    return;
  }
  popoverOpen.value = false;
}

function onKeydown(e: KeyboardEvent): void {
  if (e.key === 'Escape' && popoverOpen.value) {
    popoverOpen.value = false;
  }
}

if (route) {
  watch(
    () => route.fullPath,
    () => {
      popoverOpen.value = false;
    },
  );
}

/** Mốc thời gian hiện tại — tick mỗi giây để đếm ngược chạy (không cần re-render nguồn khác). */
const nowMs = ref(Date.now());

let tickTimer: ReturnType<typeof setInterval> | null = null;
let syncTimer: ReturnType<typeof setTimeout> | null = null;

/** Chu kỳ hồi tim theo gói — Premium 10 phút, Free 30 phút (heartsMax ≥ 30 ⇒ đang Premium). */
const regenMinutes = computed(() =>
  gamification.isPremium || gamification.heartsMax >= 30 ? 10 : 30,
);

onMounted(() => {
  // Đồng bộ hearts/heartsMax/lastHeartAt/streak từ backend (store chỉ fetch theo view cụ thể).
  void gamification.fetchHearts();
  void gamification.fetchStreak();
  document.addEventListener('click', onDocumentClick);
  window.addEventListener('keydown', onKeydown);
  tickTimer = setInterval(() => {
    nowMs.value = Date.now();
  }, 1000);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocumentClick);
  window.removeEventListener('keydown', onKeydown);
  if (tickTimer !== null) clearInterval(tickTimer);
  if (syncTimer !== null) clearTimeout(syncTimer);
});

const heartsLabel = computed(() => `${gamification.hearts}/${gamification.heartsMax}`);

/** Mốc tim kế tiếp = lastHeartAt + (floor(elapsed/interval) + 1) * interval — khớp nextHeartInSeconds backend. */
const nextHeartAtMs = computed(() => {
  if (!gamification.lastHeartAt) return null;
  const intervalMs = regenMinutes.value * 60 * 1000;
  const base = new Date(gamification.lastHeartAt).getTime();
  const elapsed = Math.max(0, nowMs.value - base);
  return base + (Math.floor(elapsed / intervalMs) + 1) * intervalMs;
});

/** Còn bao lâu tới tim kế tiếp — null khi tim đầy hoặc chưa có dữ liệu. */
const nextHeartIn = computed(() => {
  if (gamification.hearts >= gamification.heartsMax) return null;
  const next = nextHeartAtMs.value;
  if (next === null) return null;
  const diff = Math.max(0, next - nowMs.value);
  return diff <= 0 ? null : formatDuration(Math.ceil(diff / 1000));
});

// Vừa hết đếm ngược (tim đã hồi) → fetch lại để đồng bộ hearts với backend.
watch(nextHeartIn, (value, prev) => {
  if (prev !== null && value === null && gamification.hearts < gamification.heartsMax) {
    if (syncTimer !== null) clearTimeout(syncTimer);
    syncTimer = setTimeout(() => {
      void gamification.fetchHearts();
    }, 500);
  }
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
    <span class="hearts-gems__chip" title="Đá quý" aria-label="Đá quý">
      <BaseIcon name="sparkles" :size="14" />
      {{ gamification.gems }}
    </span>
    <span v-if="gamification.streakDays > 0" class="hearts-gems__chip" title="Chuỗi ngày" aria-label="Chuỗi ngày">
      🔥 {{ gamification.streakDays }}
    </span>

    <Teleport to="body">
      <Transition name="hearts-pop">
        <div v-if="popoverOpen" class="hearts-gems__pop card">
          <p class="hearts-gems__pop-title">Tim của bạn</p>
          <p v-if="nextHeartIn" class="hearts-gems__pop-desc">
            Tim tiếp theo sau <strong>{{ nextHeartIn }}</strong> ({{ regenMinutes }} phút/tim — bản
            {{ gamification.isPremium ? 'Cao cấp' : 'Miễn phí' }})
          </p>
          <RouterLink v-if="!gamification.isPremium" class="hearts-gems__pop-link" to="/premium">
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
  white-space: nowrap;
  flex-shrink: 0;
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

@media (max-width: 480px) {
  .hearts-gems {
    gap: 4px;
  }
  .hearts-gems__chip {
    padding: 3px 6px;
    gap: 3px;
    font-size: 11px;
  }
}
</style>
