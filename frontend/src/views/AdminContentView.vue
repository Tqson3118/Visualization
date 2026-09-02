<script setup lang="ts">
// AdminContentView (Unified Teacher & Admin Studio Orchestrator Shell)
// Tách 3 tab độc lập: Overview / Curriculum (Outline Tree & Studio) / Feedback
import { ref, watch } from 'vue';
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router';
import StudioShell from '@/components/studio/StudioShell.vue';
import StudioOverviewTab from './admin/sections/StudioOverviewTab.vue';
import StudioCurriculumTab from './admin/sections/StudioCurriculumTab.vue';
import StudioFeedbackTab from './admin/sections/StudioFeedbackTab.vue';
import StudioModerationTab from './admin/sections/StudioModerationTab.vue';
import AdminGamificationSettingsTab from './admin/sections/AdminGamificationSettingsTab.vue';

type StudioTab = 'overview' | 'curriculum' | 'feedback' | 'moderation' | 'gamification';
/** Tab do StudioShell/StudioOverviewTab phát ra — 'exercises' là alias cũ của 'curriculum' */
type StudioTabInput = StudioTab | 'exercises';

const route = useRoute();
const router = useRouter();
const activeTab = ref<StudioTab>('overview');
const isDirty = ref(false);

watch(
  () => route.query.tab,
  (queryTab) => {
    if (queryTab === 'exercises') {
      activeTab.value = 'curriculum';
      void router.replace({ query: { ...route.query, tab: 'curriculum' } }).catch(() => {});
    } else if (
      queryTab === 'curriculum' ||
      queryTab === 'feedback' ||
      queryTab === 'overview' ||
      queryTab === 'moderation' ||
      queryTab === 'gamification'
    ) {
      activeTab.value = queryTab as StudioTab;
    } else {
      activeTab.value = 'overview';
    }
  },
  { immediate: true },
);

function switchTab(newTab: StudioTabInput): void {
  const tab: StudioTab = newTab === 'exercises' ? 'curriculum' : newTab;
  if (activeTab.value === 'curriculum' && tab !== 'curriculum' && isDirty.value) {
    const ok = window.confirm('Bạn có thay đổi chưa lưu trong bài soạn. Bạn có chắc chắn muốn rời đi và hủy thay đổi?');
    if (!ok) return;
    isDirty.value = false;
  }
  activeTab.value = tab;
  if (route.query.tab !== tab) {
    // Fix S-3: lessonId là ý định điều hướng tạm thời (mở editor từ "Sửa bài") —
    // phải xóa khi đổi tab, nếu không quay lại tab Giáo trình sẽ tự mở lại bài vừa sửa.
    const query: Record<string, any> = { tab };
    if (route.query.courseId) query.courseId = route.query.courseId;
    void router.replace({ query }).catch(() => {});
  }
}

onBeforeRouteLeave((to, from, next) => {
  if (isDirty.value) {
    const ok = window.confirm('Bạn có thay đổi chưa lưu trong bài soạn. Bạn có chắc chắn muốn rời đi và hủy thay đổi?');
    if (!ok) {
      next(false);
      return;
    }
    isDirty.value = false;
  }
  next();
});
</script>

<template>
  <StudioShell :active-tab="activeTab" @update:active-tab="switchTab">
    <StudioOverviewTab v-if="activeTab === 'overview'" @switch-tab="switchTab" />
    <StudioCurriculumTab v-else-if="activeTab === 'curriculum'" @dirty-change="isDirty = $event" />
    <StudioFeedbackTab v-else-if="activeTab === 'feedback'" />
    <StudioModerationTab v-else-if="activeTab === 'moderation'" />
    <AdminGamificationSettingsTab v-else-if="activeTab === 'gamification'" />
  </StudioShell>
</template>

