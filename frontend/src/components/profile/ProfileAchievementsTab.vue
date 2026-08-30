<script setup lang="ts">
import { computed } from 'vue';
import { Lock, Medal } from 'lucide-vue-next';
import { useGamificationStore } from '@/stores/gamification';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const gamification = useGamificationStore();
const achievements = computed(() => gamification.achievements);
</script>

<template>
  <div class="profile__achievements-panel">
    <div v-if="achievements.length > 0" class="profile__achievements">
      <div
        v-for="ach in achievements"
        :key="ach.id"
        class="profile__achievement"
        :class="{ 'profile__achievement--locked': !ach.earnedAt }"
      >
        <span class="profile__achievement-icon" :class="{ 'profile__achievement-icon--open': ach.earnedAt }">
          <img v-if="ach.iconUrl" :src="ach.iconUrl" :alt="ach.name" class="profile__achievement-img" />
          <component :is="ach.earnedAt ? Medal : Lock" v-else :size="20" aria-hidden="true" />
        </span>
        <p class="profile__achievement-label">{{ ach.name }}</p>
        <p v-if="ach.description" class="profile__achievement-desc">{{ ach.description }}</p>
        <Badge :variant="ach.earnedAt ? 'success' : 'muted'">{{ ach.earnedAt ? 'Đã mở' : 'Chưa mở' }}</Badge>
      </div>
    </div>
    <EmptyState
      v-else
      icon="party-popper"
      title="Chưa có huy hiệu"
      description="Bắt đầu học để mở huy hiệu đầu tiên nhé!"
    />
  </div>
</template>

<style scoped>
.profile__achievements {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: var(--space-md, 16px);
}

.profile__achievement {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  padding: var(--space-lg, 24px);
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: var(--space-sm, 8px);
  transition: all 150ms ease;
}

.profile__achievement--locked {
  opacity: 0.55;
  filter: grayscale(0.8);
}

.profile__achievement-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-full, 9999px);
  background: rgba(255, 255, 255, 0.05);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-secondary, #8b949e);
}

.profile__achievement-icon--open {
  background: rgba(245, 158, 11, 0.15);
  color: #f59e0b;
}

.profile__achievement-img {
  width: 32px;
  height: 32px;
  object-fit: contain;
}

.profile__achievement-label {
  font-size: var(--text-sm, 13px);
  font-weight: 700;
  color: #ffffff;
  margin: 0;
}

.profile__achievement-desc {
  font-size: var(--text-xs, 12px);
  color: var(--color-text-secondary, #8b949e);
  margin: 0;
  line-height: 1.4;
}
</style>
