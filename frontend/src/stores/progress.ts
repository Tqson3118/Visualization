import { ref } from 'vue';
import { defineStore } from 'pinia';

import type { ProgressOverviewDto, TeacherReportDto } from '@/api/progress';

/** Store progress theo SDD §3.2 */
export const useProgressStore = defineStore('progress', () => {
  const overview = ref<ProgressOverviewDto | null>(null);
  const lessonProgress = ref<Record<number, { viewed: boolean; bestScore: number | null; completed: boolean }>>({});
  const reportData = ref<TeacherReportDto | null>(null);

  async function fetchOverview(): Promise<void> {
    // TODO: gọi progressApi.fetchOverview()
    return Promise.reject(new Error('TODO: progressStore.fetchOverview chưa triển khai'));
  }

  async function fetchLessonProgress(lessonId: number): Promise<void> {
    // TODO: lessonProgress.value[lessonId] = await progressApi.fetchLessonProgress(lessonId)
    void lessonId;
    return Promise.reject(new Error('TODO: progressStore.fetchLessonProgress chưa triển khai'));
  }

  async function fetchReport(lessonId: number): Promise<void> {
    // TODO: reportData.value = await progressApi.fetchReport({ lessonId })
    void lessonId;
    return Promise.reject(new Error('TODO: progressStore.fetchReport chưa triển khai'));
  }

  return { overview, lessonProgress, reportData, fetchOverview, fetchLessonProgress, fetchReport };
});
