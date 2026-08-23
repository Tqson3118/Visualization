import { ref } from 'vue';
import { defineStore } from 'pinia';

import * as progressApi from '@/api/progress';
import type { ProgressOverviewDto, TeacherReportDto } from '@/api/progress';

/** Store progress theo SDD §3.2 — triển khai thật với API /progress. */
export const useProgressStore = defineStore('progress', () => {
  const overview = ref<ProgressOverviewDto | null>(null);
  const lessonProgress = ref<Record<number, { viewed: boolean; bestScore: number | null; completed: boolean }>>({});
  const reportData = ref<TeacherReportDto | null>(null);
  const loading = ref(false);

  async function fetchOverview(): Promise<void> {
    loading.value = true;
    try {
      overview.value = await progressApi.fetchOverview();
    } finally {
      loading.value = false;
    }
  }

  async function fetchLessonProgress(lessonId: number): Promise<void> {
    lessonProgress.value[lessonId] = await progressApi.fetchLessonProgress(lessonId);
  }

  async function fetchReport(lessonId: number): Promise<void> {
    loading.value = true;
    try {
      reportData.value = await progressApi.fetchReport({ lessonId });
    } finally {
      loading.value = false;
    }
  }

  function reset(): void {
    overview.value = null;
    lessonProgress.value = {};
    reportData.value = null;
    loading.value = false;
  }

  return { overview, lessonProgress, reportData, loading, fetchOverview, fetchLessonProgress, fetchReport, reset };
});
