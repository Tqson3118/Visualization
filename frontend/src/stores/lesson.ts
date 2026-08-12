import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import * as lessonsApi from '@/api/lessons';
import type { LessonDto, LessonSummary, Topic } from '@/api/lessons';

/** Store lesson theo SDD §3.2 — triển khai thật với API lessons/topics. */
export const useLessonStore = defineStore('lesson', () => {
  const topics = ref<Topic[]>([]);
  const lessonsByTopic = ref<Record<number, LessonSummary[]>>({});
  const currentLesson = ref<LessonDto | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const progressByTopic = computed(() =>
    topics.value.map((topic) => {
      const lessons = lessonsByTopic.value[topic.id] ?? [];
      const done = lessons.filter((lesson) => lesson.progress?.completed).length;
      return {
        topicId: topic.id,
        name: topic.name,
        done,
        total: lessons.length,
        percent: lessons.length === 0 ? 0 : Math.round((done / lessons.length) * 100),
      };
    }),
  );

  async function fetchTopics(): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      topics.value = await lessonsApi.fetchTopics();
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không thể tải danh sách chủ đề';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchLessons(topicId: number): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      const page = await lessonsApi.fetchLessons({ topicId });
      lessonsByTopic.value[topicId] = page.items;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không thể tải bài học';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function fetchLesson(id: number): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      currentLesson.value = await lessonsApi.fetchLesson(id);
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không thể tải bài học';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function markViewed(id: number): Promise<void> {
    await lessonsApi.markViewed(id);
    // Cập nhật progress cục bộ
    if (currentLesson.value) {
      currentLesson.value.progress = {
        viewed: true,
        bestScore: currentLesson.value.progress?.bestScore ?? null,
        completed: currentLesson.value.progress?.completed ?? false,
      };
    }
  }

  return {
    topics,
    lessonsByTopic,
    currentLesson,
    loading,
    error,
    progressByTopic,
    fetchTopics,
    fetchLessons,
    fetchLesson,
    markViewed,
  };
});
