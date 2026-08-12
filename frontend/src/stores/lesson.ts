import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import * as lessonsApi from '@/api/lessons';
import type { LessonDto, LessonSummary, Topic } from '@/api/lessons';

/** Store lesson theo SDD §3.2 */
export const useLessonStore = defineStore('lesson', () => {
  const topics = ref<Topic[]>([]);
  const lessonsByTopic = ref<Record<number, LessonSummary[]>>({});
  const currentLesson = ref<LessonDto | null>(null);
  const loading = ref(false);

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
    // TODO: gọi lessonsApi.fetchTopics() khi backend sẵn sàng
    loading.value = true;
    try {
      // topics.value = await lessonsApi.fetchTopics();
    } finally {
      loading.value = false;
    }
  }

  async function fetchLessons(topicId: number): Promise<void> {
    // TODO: gọi lessonsApi.fetchLessons({ topicId })
    loading.value = true;
    try {
      // lessonsByTopic.value[topicId] = (await lessonsApi.fetchLessons({ topicId })).items;
    } finally {
      loading.value = false;
    }
  }

  async function fetchLesson(id: number): Promise<void> {
    // TODO: currentLesson.value = await lessonsApi.fetchLesson(id)
    loading.value = true;
    try {
      // currentLesson.value = await lessonsApi.fetchLesson(id);
    } finally {
      loading.value = false;
    }
  }

  async function markViewed(id: number): Promise<void> {
    // TODO: await lessonsApi.markViewed(id); cập nhật progress cục bộ
    return Promise.reject(new Error('TODO: lessonStore.markViewed chưa triển khai'));
  }

  return {
    topics,
    lessonsByTopic,
    currentLesson,
    loading,
    progressByTopic,
    fetchTopics,
    fetchLessons,
    fetchLesson,
    markViewed,
  };
});
