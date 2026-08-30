import { computed, isRef, type MaybeRef, unref } from 'vue';
import type { LessonSummary, Topic } from '@/api/lessons';
import type { ExerciseSummaryDto } from '@/api/exercises';
import { normalizeVi } from '@/utils/searchNormalize';

export interface GroupedTopicContent {
  id: number;
  name: string;
  description?: string;
  lessons: LessonSummary[];
  exercises: ExerciseSummaryDto[];
  totalCount: number;
}

export function useCurriculumSearch(options: {
  lessons: MaybeRef<LessonSummary[]>;
  exercises: MaybeRef<ExerciseSummaryDto[]>;
  topics?: MaybeRef<Topic[]>;
  query?: MaybeRef<string>;
  topicFilter?: MaybeRef<number | string | null | undefined>;
}) {
  const query = options.query ? (isRef(options.query) ? options.query : computed(() => unref(options.query) ?? '')) : computed(() => '');
  const topicFilter = options.topicFilter ? (isRef(options.topicFilter) ? options.topicFilter : computed(() => unref(options.topicFilter))) : computed(() => null);

  const cleanTitle = (rawTitle?: string): string => {
    if (!rawTitle) return '';
    return rawTitle.replace(/Mini-Quizz/gi, 'Mini-Quiz');
  };

  const filteredLessons = computed(() => {
    const rawLessons = unref(options.lessons) || [];
    const q = normalizeVi((unref(query) || '').trim());
    const tf = unref(topicFilter);

    return rawLessons.filter((l) => {
      if (tf && tf !== 'all' && l.topicId !== Number(tf)) {
        return false;
      }
      if (!q) return true;
      const title = normalizeVi(l.title || '');
      const desc = normalizeVi(l.description || '');
      return title.includes(q) || desc.includes(q);
    });
  });

  const filteredExercises = computed(() => {
    const rawExercises = unref(options.exercises) || [];
    const q = normalizeVi((unref(query) || '').trim());
    const tf = unref(topicFilter);

    return rawExercises.filter((e) => {
      if (tf && tf !== 'all' && e.lessonId) {
        const rawLessons = unref(options.lessons) || [];
        const parent = rawLessons.find((l) => l.id === e.lessonId);
        if (parent && parent.topicId !== Number(tf)) {
          return false;
        }
      }
      if (!q) return true;
      const title = normalizeVi(e.title || '');
      const desc = normalizeVi(e.description || '');
      return title.includes(q) || desc.includes(q);
    });
  });

  const groupedByTopic = computed<GroupedTopicContent[]>(() => {
    const rawTopics = unref(options.topics) || [];
    const lessonsList = filteredLessons.value;
    const exercisesList = filteredExercises.value;

    const topicMap = new Map<number, GroupedTopicContent>();

    // Initialize map from topics
    for (const t of rawTopics) {
      topicMap.set(t.id, {
        id: t.id,
        name: t.name,
        description: t.description,
        lessons: [],
        exercises: [],
        totalCount: 0,
      });
    }

    // Unassigned container
    const unassigned: GroupedTopicContent = {
      id: 0,
      name: 'Chưa phân chương',
      description: 'Các bài học chưa được gán vào chương cụ thể',
      lessons: [],
      exercises: [],
      totalCount: 0,
    };

    for (const l of lessonsList) {
      if (l.topicId && topicMap.has(l.topicId)) {
        topicMap.get(l.topicId)!.lessons.push(l);
      } else {
        unassigned.lessons.push(l);
      }
    }

    for (const e of exercisesList) {
      if (e.lessonId) {
        const parentLesson = (unref(options.lessons) || []).find((l) => l.id === e.lessonId);
        if (parentLesson?.topicId && topicMap.has(parentLesson.topicId)) {
          topicMap.get(parentLesson.topicId)!.exercises.push(e);
          continue;
        }
      }
      unassigned.exercises.push(e);
    }

    const groups: GroupedTopicContent[] = [];
    for (const group of topicMap.values()) {
      group.totalCount = group.lessons.length + group.exercises.length;
      if (group.totalCount > 0 || !unref(query)?.trim()) {
        groups.push(group);
      }
    }

    unassigned.totalCount = unassigned.lessons.length + unassigned.exercises.length;
    if (unassigned.totalCount > 0) {
      groups.push(unassigned);
    }

    return groups;
  });

  return {
    query,
    topicFilter,
    filteredLessons,
    filteredExercises,
    groupedByTopic,
    cleanTitle,
  };
}
