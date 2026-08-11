<template>
  <section class="flex-1 min-h-0 flex flex-col">
    <!-- Guest banner: visible leaderboard but personal stats hidden -->
    <div v-if="!authStore.isAuthenticated" class="flex-shrink-0 mx-4 mt-4 px-4 py-3 rounded-xl border border-border-default bg-bg-surface flex items-center justify-between gap-4">
      <div class="flex items-center gap-3">
        <BaseIcon name="gamification" class="w-5 h-5 text-accent flex-shrink-0" />
        <span class="text-sm text-text-secondary">Đăng nhập để xem <span class="text-accent font-semibold">XP · Streak · Badges</span> cá nhân của bạn</span>
      </div>
      <router-link to="/login" class="flex-shrink-0 px-4 py-1.5 bg-accent text-text-inverse text-xs font-bold rounded-lg hover:bg-accent/90 transition-colors">
        Đăng nhập
      </router-link>
    </div>
    <GamificationWorkspace />
  </section>
</template>

<script setup lang="ts">
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import BaseIcon from '../../shared/components/BaseIcon.vue';
import { GamificationWorkspace } from '../../features/gamification-engine';

const authStore = useAuthStore();
</script>
