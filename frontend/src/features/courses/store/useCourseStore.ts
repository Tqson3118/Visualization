import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { courseApi } from '@/services/courseApi';
import * as lessonsApi from '@/api/lessons';
import { normalizeVi } from '@/utils/searchNormalize';
import { useAuthStore } from '@/stores/auth';
import type { Course, CourseProgress } from '../types/course.types';

export const useCourseStore = defineStore('course', () => {

  const courses = ref<Course[]>([]);
  const rawTopics = ref<string[]>([]);
  const isLoading = ref<boolean>(false);
  const error = ref<string>('');
  const selectedTopic = ref<string>('All');
  const selectedCategory = ref<string>('All');
  const selectedDifficulty = ref<string>('All');
  const searchQuery = ref<string>('');
  const enrolledCourseIds = ref<Set<string>>(new Set());

  const filteredCourses = computed(() => {
    let result = courses.value;
    if (selectedTopic.value !== 'All') {
      result = result.filter(c => {
        const raw = c.topicName;
        const name = raw ? raw.replace(/^Module\s*\d+\s*:\s*/i, '').trim() : '';
        return name === selectedTopic.value;
      });
    }
    if (selectedCategory.value !== 'All') {
      result = result.filter(c => c.category === selectedCategory.value);
    }
    if (selectedDifficulty.value !== 'All') {
      const target = selectedDifficulty.value.toLowerCase();
      result = result.filter(c => {
        const diff = (c.difficulty || '').toLowerCase();
        if (target === 'beginner' || target === 'cơ bản' || target === 'dễ' || target === 'easy') {
          return diff.includes('begin') || diff.includes('easy') || diff.includes('cơ bản') || diff.includes('dễ');
        }
        if (target === 'intermediate' || target === 'trung cấp' || target === 'trung bình' || target === 'medium') {
          return diff.includes('inter') || diff.includes('med') || diff.includes('trung');
        }
        if (target === 'advanced' || target === 'nâng cao' || target === 'khó' || target === 'hard') {
          return diff.includes('adv') || diff.includes('hard') || diff.includes('nâng') || diff.includes('khó');
        }
        return diff === target;
      });
    }
    if (searchQuery.value.trim()) {
      const q = normalizeVi(searchQuery.value);
      result = result.filter(c =>
        normalizeVi(c.title).includes(q) ||
        normalizeVi(c.description).includes(q) ||
        normalizeVi(c.category).includes(q) ||
        (c.topicName ? normalizeVi(c.topicName).includes(q) : false)
      );
    }
    return result;
  });

  const topics = computed(() => {
    if (rawTopics.value.length > 0) {
      return ['All', ...rawTopics.value];
    }
    const tSet = new Set<string>();
    for (const c of courses.value) {
      const name = c.topicName ? c.topicName.replace(/^Module\s*\d+\s*:\s*/i, '').trim() : '';
      if (name) {
        tSet.add(name);
      }
    }
    if (tSet.size === 0) {
      return ['All'];
    }
    return ['All', ...Array.from(tSet)];
  });

  const DIFFICULTY_TERMS = new Set([
    'easy', 'medium', 'hard', 'beginner', 'intermediate', 'advanced',
    'cơ bản', 'trung cấp', 'nâng cao', 'dễ', 'khó', 'trung bình'
  ]);

  const categories = computed(() => {
    const cats = new Set<string>();
    for (const c of courses.value) {
      if (c.category && !DIFFICULTY_TERMS.has(c.category.trim().toLowerCase())) {
        cats.add(c.category.trim());
      }
    }
    if (cats.size === 0) {
      return ['All', 'Cấu trúc dữ liệu', 'Giải thuật', 'Sắp xếp & Tìm kiếm', 'Cây & Bảng băm', 'Đồ thị'];
    }
    return ['All', ...Array.from(cats)];
  });

  const difficulties = computed(() => {
    return ['All', 'Beginner', 'Intermediate', 'Advanced'];
  });

  async function loadCourses() {
    isLoading.value = true;
    error.value = '';
    try {
      const [apiCourses, apiTopics] = await Promise.all([
        courseApi.getCourses(),
        lessonsApi.fetchTopics().catch(() => [] as lessonsApi.Topic[]),
      ]);
      const cleanPrefix = (str?: string) => str ? str.replace(/^Module\s*\d+\s*:\s*/i, '').trim() : str;
      if (apiTopics && apiTopics.length > 0) {
        rawTopics.value = apiTopics
          .filter(t => !t.name.startsWith('Module '))
          .map(t => cleanPrefix(t.name) || t.name);
      }
      const mapped = apiCourses.map(c => ({
        ...c,
        title: cleanPrefix(c.title) || c.title,
        category: cleanPrefix(c.category) || c.category,
        coverImage: c.coverImageUrl ?? c.coverImage,
        topicName: cleanPrefix(c.topicName),
        topicId: c.topicId,
      }));
      courses.value = mapped.filter(c => c.isPublished) as Course[];
    } catch (err) {
      console.warn('Không tải được khóa học từ máy chủ, nạp dữ liệu cục bộ:', err);
      error.value = 'Không kết nối được máy chủ (Đang dùng dữ liệu cục bộ).';
      try {
        const { SEED_COURSES } = await import('@/data/courses');
        courses.value = (SEED_COURSES || []).filter(c => c.isPublished !== false) as unknown as Course[];
      } catch {
        courses.value = [];
      }
    } finally {
      isLoading.value = false;
      _loadEnrollments();
    }
  }

  function _getStoragePrefix(): string | null {
    try {
      const auth = useAuthStore();
      const uid = auth.user?.id;
      return uid ? `u${uid}_` : null;
    } catch {
      return null;
    }
  }

  // Tự động quét dọn sạch các key cũ không có prefix user trên máy để tránh rò rỉ giữa các nick
  function _cleanupLegacyStorage() {
    try {
      const toRemove: string[] = [];
      for (let i = 0; i < localStorage.length; i++) {
        const k = localStorage.key(i);
        if (!k) continue;
        if (/^enrolled_\d+$/.test(k) || /^course_progress_\d+$/.test(k) || /^course_completed_\d+$/.test(k) || /^course_done_\d+_\d+$/.test(k)) {
          toRemove.push(k);
        }
      }
      for (const k of toRemove) {
        localStorage.removeItem(k);
      }
    } catch {}
  }

  function _loadEnrollments() {
    _cleanupLegacyStorage();
    const enrolled = new Set<string>();
    const prefix = _getStoragePrefix();
    if (!prefix) {
      enrolledCourseIds.value = enrolled;
      return;
    }
    for (const c of courses.value) {
      if (localStorage.getItem(`enrolled_${prefix}${c.id}`) === 'true') {
        enrolled.add(c.id);
      }
    }
    enrolledCourseIds.value = enrolled;
  }

  function enrollCourse(courseId: string | number) {
    const sId = String(courseId);
    enrolledCourseIds.value.add(sId);
    const prefix = _getStoragePrefix();
    if (prefix) {
      localStorage.setItem(`enrolled_${prefix}${sId}`, 'true');
    }
  }

  function isEnrolled(courseId: string | number): boolean {
    const sId = String(courseId);
    const prefix = _getStoragePrefix();
    if (!prefix) return false;

    if (!enrolledCourseIds.value.has(sId)) {
      if (localStorage.getItem(`enrolled_${prefix}${sId}`) === 'true') {
        enrolledCourseIds.value.add(sId);
        return true;
      }
      const progress = getCourseProgress(sId);
      if (progress.completedLessonIds.length > 0 || progress.progressPercent > 0) {
        enrollCourse(sId);
        return true;
      }
      return false;
    }
    return true;
  }

  function getCourseById(id: string): Course | undefined {
    return courses.value.find(c => c.id === id);
  }

  function updateCourseLessons(courseId: string, lessons: any[]) {
    const existing = courses.value.find(c => String(c.id) === String(courseId));
    if (existing) {
      existing.lessons = lessons as any;
    }
  }

  function getCourseProgress(courseId: string): CourseProgress {
    const course = getCourseById(courseId);
    if (!course) {
      return {
        courseId,
        completedLessonIds: [],
        totalLessons: 0,
        progressPercent: 0,
        xpEarned: 0,
        isCompleted: false,
      };
    }

    const authStore = useAuthStore();
    const uId = authStore.user?.id;
    if (uId == null) {
      // Chưa có user local (guest/đang restore session): không đọc được progress localStorage,
      // nhưng vẫn tôn trọng progressPercent backend trả kèm course (Plan B bug#2 — không ép 0
      // khi danh sách bài học chưa tải).
      const serverPercent =
        typeof (course as any).progressPercent === 'number' && (course as any).progressPercent > 0
          ? (course as any).progressPercent
          : 0;
      return {
        courseId,
        completedLessonIds: [],
        totalLessons: course.totalLessons || 0,
        progressPercent: serverPercent,
        xpEarned: 0,
        isCompleted: serverPercent >= 100,
      };
    }

    const storedCourseProgress = localStorage.getItem(`course_progress_u${uId}_${courseId}`);
    const storedCompleted = localStorage.getItem(`course_completed_u${uId}_${courseId}`);

    const lessons = course.lessons ?? [];
    let completedCount = 0;
    const completedLessonIds: string[] = [];
    let xpEarned = 0;

    if (lessons.length > 0) {
      for (const lesson of lessons) {
        let isDone = (lesson as any).status === 'Completed' ||
          localStorage.getItem(`course_done_u${uId}_${courseId}_${lesson.id}`) === 'true';
        if (isDone) {
          completedCount++;
          completedLessonIds.push(lesson.id);
        }
      }
    } else {
      let localDoneCount = 0;
      for (let i = 0; i < localStorage.length; i++) {
        const k = localStorage.key(i);
        if (k && k.startsWith(`course_done_u${uId}_${courseId}_`) && localStorage.getItem(k) === 'true') {
          localDoneCount++;
          const lessonId = k.replace(`course_done_u${uId}_${courseId}_`, '');
          completedLessonIds.push(lessonId);
        }
      }
      if (localDoneCount > 0) {
        completedCount = Math.max(completedCount, localDoneCount);
      } else if (storedCourseProgress) {
        try {
          const parsed = JSON.parse(storedCourseProgress);
          if (parsed && typeof parsed.progressPercent === 'number' && parsed.progressPercent > 0) {
            completedCount = Math.max(completedCount, parsed.completedLessonIds?.length || 0);
          }
        } catch {}
      }
    }

    const total = course.totalLessons > 0 ? course.totalLessons : (lessons.length > 0 ? lessons.length : 1);
    let progressPercent = 0;
    if (total > 0 && completedCount > 0) {
      progressPercent = Math.min(100, Math.round((completedCount / total) * 100));
    } else if (typeof (course as any).progressPercent === 'number' && (course as any).progressPercent > 0) {
      progressPercent = (course as any).progressPercent;
    }

    if (storedCompleted === 'true' || (total > 0 && completedCount >= total && total > 0)) {
      progressPercent = 100;
    }

    const isCompleted = progressPercent === 100 || storedCompleted === 'true';

    const result: CourseProgress = {
      courseId,
      completedLessonIds,
      totalLessons: total,
      progressPercent,
      xpEarned,
      isCompleted,
    };

    if (completedCount > 0) {
      try {
        localStorage.setItem(`course_progress_${courseId}`, JSON.stringify(result));
      } catch {}
    }

    return result;
  }

  function setTopic(topic: string) {
    selectedTopic.value = topic;
  }

  function setCategory(category: string) {
    selectedCategory.value = category;
  }

  function setDifficulty(difficulty: string) {
    selectedDifficulty.value = difficulty;
  }

  function setSearchQuery(query: string) {
    searchQuery.value = query;
  }

  function resetFilters() {
    selectedTopic.value = 'All';
    selectedCategory.value = 'All';
    selectedDifficulty.value = 'All';
    searchQuery.value = '';
  }

  function getLessonStatus(lessonId: string): 'not-started' | 'in-progress' | 'completed' {
    const key = `lesson_progress_${lessonId}`;
    const saved = localStorage.getItem(key);
    if (!saved) return 'not-started';
    try {
      const data = JSON.parse(saved);
      if (data.completed === true || data.codelabCompleted === true) return 'completed';
      if (data.hasWatchedVisualizer || data.quizScore !== null) return 'in-progress';
      return 'not-started';
    } catch {
      return 'not-started';
    }
  }

  function getLessonQuizScore(lessonId: string): number | null {
    const key = `lesson_progress_${lessonId}`;
    const saved = localStorage.getItem(key);
    if (!saved) return null;
    try {
      const data = JSON.parse(saved);
      return data.quizScore ?? null;
    } catch {
      return null;
    }
  }

  function getLessonXpEarned(lessonId: string): number {
    const key = `lesson_progress_${lessonId}`;
    const saved = localStorage.getItem(key);
    if (!saved) return 0;
    try {
      const data = JSON.parse(saved);
      return data.xpAwarded ?? 0;
    } catch {
      return 0;
    }
  }

  function getFirstUncompletedLesson(courseId: string): string | null {
    const course = getCourseById(courseId);
    const lessons = course?.lessons ?? [];
    if (lessons.length === 0) return null;

    for (const lesson of lessons) {
      const status = getLessonStatus(lesson.id);
      if (status === 'in-progress') return lesson.id;
    }

    for (const lesson of lessons) {
      const status = getLessonStatus(lesson.id);
      if (status === 'not-started') return lesson.id;
    }

    return lessons[0]?.id ?? null;
  }

  return {
    courses,
    isLoading,
    error,
    selectedTopic,
    selectedCategory,
    selectedDifficulty,
    searchQuery,
    filteredCourses,
    topics,
    categories,
    difficulties,
    loadCourses,
    getCourseById,
    getCourseProgress,
    updateCourseLessons,
    setTopic,
    setCategory,
    setDifficulty,
    setSearchQuery,
    resetFilters,
    getLessonStatus,
    getLessonQuizScore,
    getLessonXpEarned,
    getFirstUncompletedLesson,
    enrollCourse,
    isEnrolled,
    enrolledCourseIds,
    reset() {
      courses.value = [];
      isLoading.value = false;
      error.value = '';
      selectedTopic.value = 'All';
      selectedCategory.value = 'All';
      selectedDifficulty.value = 'All';
      searchQuery.value = '';
      enrolledCourseIds.value = new Set();
    },
  };
});
