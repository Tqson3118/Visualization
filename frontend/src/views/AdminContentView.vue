<script setup lang="ts">
// AdminContentView (Unified Teacher & Admin Studio Orchestrator Shell)
// Tách 3 tab độc lập: Overview / Curriculum (Outline Tree & Studio) / Feedback
import { ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import StudioShell from '@/components/studio/StudioShell.vue';
import StudioOverviewTab from './admin/sections/StudioOverviewTab.vue';
import StudioCurriculumTab from './admin/sections/StudioCurriculumTab.vue';
import StudioFeedbackTab from './admin/sections/StudioFeedbackTab.vue';

type StudioTab = 'overview' | 'curriculum' | 'feedback';
/** Tab do StudioShell/StudioOverviewTab phát ra — 'exercises' là alias cũ của 'curriculum' */
type StudioTabInput = StudioTab | 'exercises';

const route = useRoute();
const router = useRouter();
const activeTab = ref<StudioTab>('overview');

watch(
  () => route.query.tab,
  (queryTab) => {
    if (queryTab === 'exercises') {
      activeTab.value = 'curriculum';
      void router.replace({ query: { ...route.query, tab: 'curriculum' } }).catch(() => {});
    } else if (
      queryTab === 'curriculum' ||
      queryTab === 'feedback' ||
      queryTab === 'overview'
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
  activeTab.value = tab;
  if (route.query.tab !== tab) {
    const query: Record<string, any> = { ...route.query, tab };
    void router.replace({ query }).catch(() => {});
  }
}
</script>

<template>
  <StudioShell :active-tab="activeTab" @update:active-tab="switchTab">
    <StudioOverviewTab v-if="activeTab === 'overview'" @switch-tab="switchTab" />
    <StudioCurriculumTab v-else-if="activeTab === 'curriculum'" />
    <StudioFeedbackTab v-else-if="activeTab === 'feedback'" />
  </StudioShell>
</template>

