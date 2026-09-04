import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import type { Lesson, QuizQuestion, CodeLabTask } from '../types/lesson.types';
import { fetchLessonProgress, saveLessonProgress, awardXp, fetchLessonDetail, getLessonAuthToken, type LessonDetailResponse } from '../services/lessonApi';
import { statelessQuizApi } from '../../quiz-system/service/statelessQuizApi';
import { parseSandboxDemo, parseSandboxSimulationKey } from '../utils/sandboxConfig';
import { CODELAB_TASK_REGISTRY } from '../utils/codelabTaskRegistry';
import { useAuthStore } from '@/stores/auth';
import { useCourseStore } from '@/features/courses/store/useCourseStore';
import { courseApi, type CourseDetailDto } from '@/services/courseApi';

/** Thông tin bổ sung từ backend (không nằm trong Lesson local). */
export interface LessonMeta {
  courseId: string;
  courseTitle: string;
  quizId: string | null;
  /** Id exercise CODE của node (bài ASM / kiểm tra cuối) — nộp lên máy chủ chấm. */
  exerciseId: string | null;
  sandboxType: string;
  sandboxConfig: string;
  orderIndex: number;
  lastSubmittedCode?: string | null;
  lastQuizSubmission?: {
    score: number;
    maxScore: number;
    passed: boolean;
    answersJson?: string | null;
    resultJson?: string | null;
    submittedAt?: string | null;
  } | null;
}

export function applySubmissionResultsToQuestions(questions: QuizQuestion[], resultJson?: string | null | any[]): void {
  if (!resultJson || !questions || questions.length === 0) return;
  try {
    const parsed = typeof resultJson === 'string' ? JSON.parse(resultJson) : resultJson;
    if (!Array.isArray(parsed)) return;
    for (let i = 0; i < questions.length; i++) {
      const q = questions[i];
      const res = parsed.find((r: any) => String(r.questionId ?? r.QuestionId ?? '') === String(q.id)) ?? parsed[i];
      if (!res) continue;
      const rawIndices = res.correctIndices ?? res.CorrectIndices ?? res.correctAnswer ?? res.CorrectAnswer;
      const rawIndex = res.correctIndex ?? res.CorrectIndex;
      if (Array.isArray(rawIndices) && rawIndices.length > 0) {
        q.correctIndices = rawIndices.map(Number).filter(n => !isNaN(n));
        q.correctIndex = q.correctIndices[0];
      } else if (typeof rawIndex === 'number' && !isNaN(rawIndex)) {
        q.correctIndex = rawIndex;
        q.correctIndices = [rawIndex];
      }
      const exp = res.explanation ?? res.Explanation;
      if (typeof exp === 'string') {
        q.explanation = exp;
      }
    }
  } catch (e) {
    console.warn('Error applying submission results to questions:', e);
  }
}

function mapBackendQuizQuestions(questions: Array<{ id: string; text: string; type?: string; options: string[]; correctIndex?: number | null; correctIndices?: number[] | null; explanation?: string }>): QuizQuestion[] {
  return questions.map(q => {
    const validIndices = Array.isArray(q.correctIndices)
      ? q.correctIndices.filter(x => typeof x === 'number' && !isNaN(x))
      : (typeof q.correctIndex === 'number' && !isNaN(q.correctIndex) ? [q.correctIndex] : []);
    return {
      id: q.id,
      questionText: q.text,
      type: q.type || 'SINGLE',
      options: q.options,
      correctIndex: typeof q.correctIndex === 'number' ? q.correctIndex : (validIndices.length > 0 ? validIndices[0] : undefined),
      correctIndices: validIndices,
      explanation: q.explanation ?? '',
    };
  });
}

export function getLessonProgress(lessonId: string): {
  hasWatchedVisualizer: boolean;
  quizScore: number | null;
  codelabCompleted: boolean;
  xpAwarded: number;
} | null {
  const key = `lesson_progress_${lessonId}`;
  const saved = localStorage.getItem(key);
  if (!saved) return null;
  try {
    return JSON.parse(saved);
  } catch {
    return null;
  }
}

export const useLessonStore = defineStore('lessonStudy', () => {
  // ── State ──
  const currentLesson = ref<Lesson | null>(null);
  const lessonMeta = ref<LessonMeta | null>(null);
  const activeStep = ref<number>(1);
  const isLoading = ref<boolean>(false);
  const error = ref<string | null>(null);
  const isOfflineFallback = ref<boolean>(false);

  // ── Progress ──
  const hasWatchedVisualizer = ref<boolean>(false);
  const quizScore = ref<number | null>(null);
  const bestScore = ref<number>(0);
  const codelabCompleted = ref<boolean>(false);
  const xpAwarded = ref<number>(0);

  // ── Sync ──
  const isSyncing = ref<boolean>(false);
  const isOnline = ref<boolean>(navigator.onLine);

  // ── Bài đã hoàn thành (tick ở mini-map sidebar) ──
  // Lưu local theo lesson id — đánh dấu khi bấm "Hoàn thành bài học" (theory/quiz/codelab).
  const completedLessonIds = ref<string[]>(loadCompletedLessonIds());

  // Cờ "đã bấm Hoàn thành bài học" — gửi lên backend (SaveProgress → node pass → mở khoá node sau).
  const lessonFinished = ref(false);

  function getCompletedStorageKey(): string {
    try {
      const authStore = useAuthStore();
      const uId = authStore.user?.id;
      return uId ? `dsa.completedLessons_u${uId}` : 'dsa.completedLessons';
    } catch {
      return 'dsa.completedLessons';
    }
  }

  function loadCompletedLessonIds(): string[] {
    try {
      const key = getCompletedStorageKey();
      const raw = localStorage.getItem(key) ?? (key !== 'dsa.completedLessons' ? localStorage.getItem('dsa.completedLessons') : null);
      return JSON.parse(raw ?? '[]') as string[];
    } catch {
      return [];
    }
  }

  async function markLessonCompleted(id: string) {
    if (!completedLessonIds.value.includes(id)) {
      completedLessonIds.value.push(id);
      localStorage.setItem(getCompletedStorageKey(), JSON.stringify(completedLessonIds.value));
    }
    if (lessonMeta.value?.courseId) {
      try {
        localStorage.setItem(`course_done_${lessonMeta.value.courseId}_${id}`, 'true');
        const courseStore = useCourseStore();
        courseStore.enrollCourse(String(lessonMeta.value.courseId));
      } catch {}
    }
    lessonFinished.value = true;

    // Chặn nhận XP free nếu bài học thuộc lộ trình mà user chưa tham gia lộ trình đó
    if (lessonMeta.value?.courseId) {
      try {
        const courseStore = useCourseStore();
        if (!courseStore.isEnrolled(String(lessonMeta.value.courseId))) {
          return;
        }
      } catch {
        // ignore
      }
    }

    if (currentLesson.value) {
      const totalXp = currentLesson.value.xpReward ?? 0;
      if (xpAwarded.value < totalXp) {
        const diff = totalXp - xpAwarded.value;
        xpAwarded.value += diff;
        saveToLocalStorage();
        try {
          const authStore = useAuthStore();
          if (authStore.user) {
            authStore.user.xp = (authStore.user.xp ?? 0) + diff;
          }
        } catch {
          // Pinia store update
        }
      }
    }
  }

  // ── Computed ──

  const attachedSimulationKeys = ref<string[]>([]);

  /** SimulationKey của bài học (node LAB): đọc từ sandboxConfig (json simulationKey) hoặc demo cũ. */
  const simulationKey = computed<string | null>(() => {
    const cfg = lessonMeta.value?.sandboxConfig ?? '';
    return parseSandboxSimulationKey(cfg) ?? parseSandboxDemo(cfg) ?? (attachedSimulationKeys.value[0] || null);
  });

  /** Tất cả simulation keys đính kèm bài học */
  const simulationKeys = computed<string[]>(() => {
    const list: string[] = [];
    const cfg = lessonMeta.value?.sandboxConfig ?? '';
    const single = parseSandboxSimulationKey(cfg) ?? parseSandboxDemo(cfg);
    if (single && !list.includes(single)) list.push(single);
    if (cfg) {
      try {
        const parsed = JSON.parse(cfg);
        if (Array.isArray(parsed.simulationKeys)) {
          for (const k of parsed.simulationKeys) {
            if (typeof k === 'string' && !list.includes(k)) list.push(k);
          }
        }
      } catch {}
    }
    for (const k of attachedSimulationKeys.value) {
      if (!list.includes(k)) list.push(k);
    }
    return list;
  });

  const quizPassed = computed(() => {
    const questions = currentLesson.value?.quizQuestions;
    if (!questions || questions.length === 0 || quizScore.value === null) return false;
    const requiredScore = Math.ceil(questions.length * 0.7);
    return quizScore.value >= requiredScore;
  });

  const totalXpEarned = computed(() => xpAwarded.value);
  const isLessonComplete = computed(() =>
    codelabCompleted.value || (!currentLesson.value?.codelabTask && quizPassed.value)
  );

  const getStorageKey = (lessonId: string) => `lesson_progress_${lessonId}`;

  window.addEventListener('online', () => {
    isOnline.value = true;
    syncToServer().catch(() => {});
  });
  window.addEventListener('offline', () => {
    isOnline.value = false;
  });
  window.addEventListener('storage', (e) => {
    if ((e.key === 'dsa.completedLessons' || e.key === getCompletedStorageKey()) && e.newValue) {
      try {
        completedLessonIds.value = JSON.parse(e.newValue);
      } catch {}
    }
  });

  // ── Local storage ──
  function loadFromLocalStorage(lessonId: string) {
    const data = localStorage.getItem(getStorageKey(lessonId));
    if (data) {
      try {
        const parsed = JSON.parse(data);
        hasWatchedVisualizer.value = !!parsed.hasWatchedVisualizer;
        quizScore.value = parsed.quizScore ?? null;
        bestScore.value = parsed.bestScore ?? 0;
        codelabCompleted.value = !!parsed.codelabCompleted;
        xpAwarded.value = parsed.xpAwarded ?? 0;
      } catch (e) {
        console.warn('Lỗi khi khôi phục tiến độ từ local:', e);
      }
    }
  }

  function saveToLocalStorage() {
    if (!currentLesson.value) return;
    const data = {
      hasWatchedVisualizer: hasWatchedVisualizer.value,
      quizScore: quizScore.value,
      bestScore: bestScore.value,
      codelabCompleted: codelabCompleted.value,
      xpAwarded: xpAwarded.value,
      completed: isLessonComplete.value || lessonFinished.value,
    };
    localStorage.setItem(getStorageKey(currentLesson.value.id), JSON.stringify(data));
  }

  async function syncToServer(force = false) {
    if (!currentLesson.value) return;

    const token = getLessonAuthToken();
    if (!token || !isOnline.value) {
      saveToLocalStorage();
      return;
    }

    if (isSyncing.value && !force) return;

    isSyncing.value = true;
    try {
      const payload = {
        lessonId: currentLesson.value.id,
        hasWatchedVisualizer: hasWatchedVisualizer.value,
        quizScore: quizScore.value,
        bestScore: bestScore.value,
        codelabCompleted: codelabCompleted.value,
        xpAwarded: xpAwarded.value,
        completed: lessonFinished.value,
      };

      await saveLessonProgress(payload);
      saveToLocalStorage();
    } catch (err) {
      console.warn('Đồng bộ thất bại, sẽ thử lại sau', err);
      saveToLocalStorage();

      setTimeout(() => {
        if (isOnline.value) syncToServer(true);
      }, 10000);
    } finally {
      isSyncing.value = false;
    }
  }

  let syncTimeout: ReturnType<typeof setTimeout> | null = null;
  function debouncedSync() {
    if (syncTimeout) clearTimeout(syncTimeout);
    syncTimeout = setTimeout(() => {
      syncToServer().catch(() => {});
      syncTimeout = null;
    }, 3000);
  }

  /** Build Lesson từ API detail + quiz backend + codelab từ sandboxConfig. */
  async function buildLessonFromApi(detail: LessonDetailResponse): Promise<Lesson> {
    let codelabTask: CodeLabTask | CodeLabTask[] | undefined;

    if (detail.sandboxType === 'codelab' && detail.sandboxConfig) {
      try {
        const config = JSON.parse(detail.sandboxConfig);
        if (Array.isArray(config)) {
          codelabTask = config as CodeLabTask[];
        } else if (config.tasks && Array.isArray(config.tasks)) {
          codelabTask = config.tasks as CodeLabTask[];
        } else if (config.id && config.testCases) {
          codelabTask = config as CodeLabTask;
        } else if (config.testCases && Array.isArray(config.testCases) && (config.description || config.starterCode || config.entryFunction)) {
          codelabTask = {
            id: config.id || 'default',
            title: config.title || detail.title,
            description: config.description || detail.contentMd || detail.title,
            initialCode: config.starterCode || config.initialCode || 'function solve(input) {\n  return null;\n}',
            solution: config.solution || '',
            entryFunction: config.entryFunction || 'solve',
            testCases: (config.testCases ?? []).map((tc: any) => ({
              input: tc.input ?? '',
              expectedOutput: tc.expectedOutput ?? tc.expected ?? '',
              isHidden: tc.isHidden ?? false,
            })),
          };
        } else if (config.signature && config.testCases) {
          // Check if registry has better matching data (e.g. for bubble-sort, binary-search)
          const lowerTitle = detail.title.toLowerCase();
          let registryTask: CodeLabTask | undefined;
          if (lowerTitle.includes('bubble') || lowerTitle.includes('nổi bọt')) registryTask = CODELAB_TASK_REGISTRY['bubble-sort'];
          else if (lowerTitle.includes('binary search') || lowerTitle.includes('nhị phân')) registryTask = CODELAB_TASK_REGISTRY['binary-search'];
          else if (lowerTitle.includes('stack') || lowerTitle.includes('ngăn xếp')) registryTask = CODELAB_TASK_REGISTRY['stack'];
          else if (lowerTitle.includes('tree') || lowerTitle.includes('cây') || lowerTitle.includes('đệ quy')) registryTask = CODELAB_TASK_REGISTRY['tree-traversal'];
          else if (lowerTitle.includes('two pointers') || lowerTitle.includes('hai con trỏ')) registryTask = CODELAB_TASK_REGISTRY['two-pointers'];

          if (registryTask) {
            codelabTask = registryTask;
          } else {
            // Parse entry function and params from signature e.g. "function bubbleSort(arr)"
            const match = /function\s+([a-zA-Z0-9_$]+)\s*\(([^)]*)\)/.exec(config.signature);
            const fnName = match ? match[1] : (config.testCases?.[0]?.entryFunction ?? 'solve');
            const fnParams = match ? match[2] : 'arr';

            const validTestCases = (config.testCases ?? []).filter((tc: any) => tc.input || tc.expectedOutput).map((tc: any) => ({
              input: tc.input ?? '',
              expectedOutput: tc.expectedOutput ?? '',
              isHidden: tc.isHidden ?? false,
            }));

            codelabTask = {
              description: config.signature ?? detail.title,
              initialCode: `function ${fnName}(${fnParams}) {\n  // Hướng dẫn: Cài đặt giải thuật tại đây\n  \n  return null;\n}`,
              solution: '',
              entryFunction: fnName,
              testCases: validTestCases.length > 0 ? validTestCases : [
                { input: '[[5, 2, 8, 1, 9]]', expectedOutput: '[1, 2, 5, 8, 9]' },
                { input: '[[3, 1, 2]]', expectedOutput: '[1, 2, 3]' },
                { input: '[[]]', expectedOutput: '[]', isHidden: true },
              ],
            };
          }
        }
      } catch (e) {
        console.warn('Cannot parse sandboxConfig for codelabTask');
      }
    }

    // Chuẩn hóa: Nếu bài học có tiêu đề lý thuyết ("Học: ...", "Bài X: ...") mà không chứa từ khóa quiz/lab,
    // đảm bảo sandboxType là 'dsa' để hiển thị giao diện lý thuyết + visualizer thay vì bị quiz chiếm quyền.
    const titleLower = (detail.title || '').toLowerCase();
    const isQuizNaming = titleLower.includes('quiz') || titleLower.includes('quizz') || titleLower.includes('trắc nghiệm') || titleLower.includes('kiểm tra');
    const isLabNaming = titleLower.includes('assignment') || titleLower.includes('lab') || titleLower.includes('thực hành') || titleLower.includes('bài tập');
    const isTheoryNaming = titleLower.startsWith('học:') || titleLower.startsWith('bài ');
    if (isTheoryNaming && !isQuizNaming && !isLabNaming && detail.sandboxType === 'quiz') {
      detail.sandboxType = 'dsa';
    }

    let quizQuestions: QuizQuestion[] = [];

    let targetQuizId = detail.quizId;
    if (!targetQuizId && detail.sandboxType === 'quiz' && detail.sandboxConfig) {
      try {
        const config = JSON.parse(detail.sandboxConfig);
        if (config.quizId) targetQuizId = String(config.quizId);
      } catch (e) {
        console.warn('Cannot parse sandboxConfig for quizId');
      }
    }

    if (targetQuizId) {
      try {
        const quiz = await statelessQuizApi.getQuizById(targetQuizId);
        if (quiz?.questions && quiz.questions.length > 0) {
          quizQuestions = mapBackendQuizQuestions(quiz.questions);
          // Tự động khôi phục đáp án đúng và giải thích nếu bài học đã có bài nộp trước đó
          if (detail.lastQuizSubmission?.resultJson) {
            applySubmissionResultsToQuestions(quizQuestions, detail.lastQuizSubmission.resultJson);
          }
        }
      } catch (e) {
        console.warn('Không tải được quiz backend, giữ quiz trống:', e);
      }
    }

    if (detail.sandboxType === 'codelab' && !codelabTask) {
      const taskTitle = detail.title || 'Thực hành lập trình';
      const taskDesc = detail.contentMd || 'Cài đặt và hoàn thành giải thuật theo yêu cầu của bài tập.';
      codelabTask = {
        description: `${taskTitle}\n\n${taskDesc}`,
        initialCode: `function solution() {\n  // TODO: Viết mã giải thuật tại đây\n  \n  return null;\n}`,
        solution: '',
        entryFunction: 'solution',
        testCases: [
          { input: '[]', expectedOutput: 'true' }
        ],
      };
    }

    if (detail.lastSubmittedCode && codelabTask) {
      if (Array.isArray(codelabTask)) {
        if (codelabTask.length > 0) {
          codelabTask[0].initialCode = detail.lastSubmittedCode;
        }
      } else {
        codelabTask.initialCode = detail.lastSubmittedCode;
      }
    }

    return {
      id: detail.id,
      title: detail.title,
      algorithmId: '',
      xpReward: detail.xpReward,
      theoryContent: detail.contentMd || '',
      quizQuestions,
      codelabTask,
      lastQuizSubmission: detail.lastQuizSubmission,
      lastSubmittedCode: detail.lastSubmittedCode,
    };
  }

  // ── Load lesson ──
  let lessonLoadRequestId = 0;

  async function loadLesson(lessonId: string) {
    const requestId = ++lessonLoadRequestId;
    isLoading.value = true;
    error.value = null;
    isOfflineFallback.value = false;

    activeStep.value = 1;
    hasWatchedVisualizer.value = false;
    quizScore.value = null;
    bestScore.value = 0;
    codelabCompleted.value = false;
    xpAwarded.value = 0;
    lessonFinished.value = false;
    lessonMeta.value = null;
    completedLessonIds.value = loadCompletedLessonIds();
    // KHÔNG null currentLesson ngay — giữ bài cũ hiển thị trong lúc fetch bài mới
    // (chuyển bài mượt, không nháy spinner; spinner chỉ hiện khi chưa có bài nào).

    const token = getLessonAuthToken();
    if (token && isOnline.value) {
      try {
        const urlParams = new URLSearchParams(window.location.search);
        const courseIdParam = urlParams.get('courseId');
        const detail = await fetchLessonDetail(lessonId, courseIdParam);
        if (requestId !== lessonLoadRequestId) return;
        const lesson = await buildLessonFromApi(detail);
        if (requestId !== lessonLoadRequestId) return;
        currentLesson.value = lesson;
        if ((detail as any).simulations && Array.isArray((detail as any).simulations)) {
          attachedSimulationKeys.value = (detail as any).simulations.map((s: any) => s.simulationKey || s);
        } else {
          attachedSimulationKeys.value = [];
        }
        lessonMeta.value = {
          courseId: detail.courseId,
          courseTitle: detail.courseTitle,
          quizId: detail.quizId,
          exerciseId: detail.exerciseId,
          sandboxType: detail.sandboxType,
          sandboxConfig: detail.sandboxConfig,
          orderIndex: detail.orderIndex,
          lastSubmittedCode: detail.lastSubmittedCode,
          lastQuizSubmission: detail.lastQuizSubmission,
        };

        if (detail.sandboxType === 'quiz') {
          activeStep.value = 3;
        }
      } catch (e: any) {
        const httpStatus = e?.response?.status || e?.status;
        if (httpStatus === 403) {
          currentLesson.value = null;
          error.value = 'Bài học này chưa được mở khóa. Bạn cần hoàn thành các bài học trước đó trong lộ trình để tiếp tục!';
          isLoading.value = false;
          return;
        }
        if (httpStatus === 401) {
          currentLesson.value = null;
          error.value = 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại để tiếp tục học.';
          isLoading.value = false;
          return;
        }
        if (httpStatus === 404) {
          currentLesson.value = null;
          error.value = 'Không tìm thấy bài học này trong hệ thống.';
          isLoading.value = false;
          return;
        }

        try {
          const urlParams = new URLSearchParams(window.location.search);
          const courseIdParam = urlParams.get('courseId');
          if (courseIdParam) {
            const courseData = await courseApi.getCourseById(courseIdParam) as unknown as CourseDetailDto;
            const found = courseData.lessons?.find(l => String(l.id) === String(lessonId));
            if (found && requestId === lessonLoadRequestId) {
            const detail: LessonDetailResponse = {
              id: String(found.id),
              courseId: String(courseData.id),
              courseTitle: courseData.title,
              title: found.title,
              contentMd: found.contentMd || '',
              sandboxType: found.sandboxType || 'dsa',
              sandboxConfig: found.sandboxConfig || '',
              quizId: found.quizId || null,
              exerciseId: null,
              xpReward: found.xpReward || 100,
              orderIndex: found.orderIndex || 0,
              status: found.status || 'NotStarted',
              lastActiveFrameIndex: 0,
              lastScrollPercent: 0,
            };
            const lesson = await buildLessonFromApi(detail);
            if (requestId !== lessonLoadRequestId) return;
            currentLesson.value = lesson;

            const simKeys: string[] = [];
            if (Array.isArray((found as any).simulations)) {
              simKeys.push(...(found as any).simulations.map((s: any) => s.simulationKey || s));
            } else if (Array.isArray((found as any).simulationKeys)) {
              simKeys.push(...(found as any).simulationKeys);
            }
            if (found.contentMd) {
              const matches = found.contentMd.matchAll(/\[(?:Mô phỏng|Simulation|mo phong):\s*([a-zA-Z0-9._-]+)\]/gi);
              for (const m of matches) {
                if (m[1] && !simKeys.includes(m[1])) simKeys.push(m[1]);
              }
            }
            attachedSimulationKeys.value = simKeys;

            lessonMeta.value = {
              courseId: detail.courseId,
              courseTitle: detail.courseTitle,
              quizId: detail.quizId,
              exerciseId: detail.exerciseId,
              sandboxType: detail.sandboxType,
              sandboxConfig: detail.sandboxConfig,
              orderIndex: detail.orderIndex,
            };
            error.value = null;
          } else {
            currentLesson.value = null;
            error.value = e instanceof Error ? e.message : 'Không tìm thấy bài học';
          }
        } else {
          currentLesson.value = null;
          error.value = e instanceof Error ? e.message : 'Không tìm thấy bài học';
        }
        } catch {
          currentLesson.value = null;
          error.value = e instanceof Error ? e.message : 'Không tìm thấy bài học';
        }
      }
    } else if (!currentLesson.value) {
      error.value = 'Không tìm thấy bài học';
    }

    // Khôi phục tiến độ (local trước, server sau).
    if (currentLesson.value) {
      loadFromLocalStorage(lessonId);

      if (token && isOnline.value) {
        try {
          const serverData = await fetchLessonProgress(lessonId);
          if (requestId !== lessonLoadRequestId) return;
          if (serverData && Object.keys(serverData).length > 0) {
            hasWatchedVisualizer.value = !!serverData.hasWatchedVisualizer || hasWatchedVisualizer.value;
            if (serverData.quizScore !== undefined) quizScore.value = serverData.quizScore;
            if (serverData.bestScore !== undefined && serverData.bestScore > bestScore.value) bestScore.value = serverData.bestScore;
            codelabCompleted.value = !!serverData.codelabCompleted || codelabCompleted.value;
            if (serverData.xpAwarded !== undefined && serverData.xpAwarded > xpAwarded.value) xpAwarded.value = serverData.xpAwarded;

            saveToLocalStorage();
          }
        } catch (e) {
          console.warn('Không thể fetch progress từ server, dùng local', e);
        }
      }

      if (codelabCompleted.value) {
        activeStep.value = 4;
      } else if (quizPassed.value) {
        activeStep.value = 3;
      } else if (hasWatchedVisualizer.value) {
        activeStep.value = 2;
      }
    }

    if (requestId === lessonLoadRequestId) {
      isLoading.value = false;
    }
  }

  function markVisualizerWatched() {
    if (!hasWatchedVisualizer.value) {
      hasWatchedVisualizer.value = true;
      saveToLocalStorage();
      debouncedSync();
    }
  }

  async function submitQuiz(answers: Record<string, number | number[]>) {
    if (!currentLesson.value) return;

    const questions = currentLesson.value.quizQuestions ?? [];
    let correct = 0;

    let attemptResult: any = null;

    // Gửi lịch sử làm bài lên server
    try {
      const quizId = lessonMeta.value?.quizId;
      if (quizId) {
        const token = getLessonAuthToken();
        const answersArray = questions.map(q => {
          const raw = answers[q.id];
          return raw !== undefined ? raw : -1;
        });
        attemptResult = await statelessQuizApi.submitAttempt(quizId, answersArray, token, true);
        if (attemptResult) {
          correct = attemptResult.score;
          // Gắn lại giải thích và đáp án đúng trả về từ server vào question
          if (attemptResult.questionResults && attemptResult.questionResults.length > 0) {
            applySubmissionResultsToQuestions(questions, attemptResult.questionResults);
          }
        }
      }
    } catch (e) {
      console.error('Lỗi khi lưu lịch sử Quiz:', e);
      // Fallback chấm điểm cục bộ nếu mất mạng
      for (const q of questions) {
        const userAns = answers[q.id];
        const userArr = Array.isArray(userAns) ? userAns : (userAns !== undefined ? [userAns] : []);
        const correctArr = q.correctIndices && q.correctIndices.length > 0 ? q.correctIndices : (q.correctIndex !== undefined ? [q.correctIndex] : []);
        const isMatch = userArr.length > 0 && correctArr.length > 0 && userArr.length === correctArr.length && userArr.every(x => correctArr.includes(x));
        if (isMatch) correct++;
      }
    }

    quizScore.value = correct;
    if (correct > bestScore.value) {
      bestScore.value = correct;
    }

    const subObj = {
      score: correct,
      maxScore: questions.length,
      passed: quizPassed.value,
      answersJson: JSON.stringify(answers),
      resultJson: JSON.stringify(attemptResult?.questionResults ?? []),
      submittedAt: new Date().toISOString(),
    };
    if (currentLesson.value) {
      currentLesson.value.lastQuizSubmission = subObj;
    }
    if (lessonMeta.value) {
      lessonMeta.value.lastQuizSubmission = subObj;
    }

    saveToLocalStorage();
    await syncToServer(true);

    if (quizPassed.value) {
      const hasCodelab = !!currentLesson.value.codelabTask;
      const quizXpCap = hasCodelab
        ? Math.floor(currentLesson.value.xpReward * 0.5)
        : currentLesson.value.xpReward;
      if (xpAwarded.value < quizXpCap) {
        const diff = quizXpCap - xpAwarded.value;
        xpAwarded.value += diff;
        saveToLocalStorage();
        await syncToServer(true);
        try {
          const authStore = useAuthStore();
          if (authStore.user) authStore.user.xp = (authStore.user.xp ?? 0) + diff;
        } catch {
          // Pinia store update
        }
      }
    }
  }

  function resetQuiz() {
    quizScore.value = null;
  }

  async function completeCodelab() {
    if (!currentLesson.value) return;

    if (!codelabCompleted.value) {
      codelabCompleted.value = true;

      saveToLocalStorage();
      await syncToServer(true);

      const totalXp = currentLesson.value.xpReward;
      if (xpAwarded.value < totalXp) {
        const diff = totalXp - xpAwarded.value;
        xpAwarded.value += diff;
        saveToLocalStorage();
        await syncToServer(true);
        try {
          const authStore = useAuthStore();
          if (authStore.user) authStore.user.xp = (authStore.user.xp ?? 0) + diff;
        } catch {
          // Pinia store update
        }
      }
    }
  }

  function goToStep(stepNumber: number) {
    if (stepNumber === 3 && !hasWatchedVisualizer.value) return;
    if (stepNumber === 4 && !quizPassed.value) return;
    if (stepNumber === 4 && !currentLesson.value?.codelabTask) return;
    activeStep.value = stepNumber;
  }

  return {
    currentLesson,
    lessonMeta,
    simulationKey,
    simulationKeys,
    activeStep,
    isLoading,
    error,
    isOfflineFallback,
    hasWatchedVisualizer,
    quizScore,
    bestScore,
    quizPassed,
    codelabCompleted,
    xpAwarded,

    isSyncing,
    isOnline,
    totalXpEarned,
    isLessonComplete,
    completedLessonIds,
    lessonFinished,
    markLessonCompleted,

    loadLesson,
    markVisualizerWatched,
    submitQuiz,
    resetQuiz,
    completeCodelab,
    goToStep,
    syncToServer,
    reset() {
      currentLesson.value = null;
      lessonMeta.value = null;
      activeStep.value = 1;
      isLoading.value = false;
      error.value = null;
      isOfflineFallback.value = false;
      hasWatchedVisualizer.value = false;
      quizScore.value = null;
      bestScore.value = 0;
      codelabCompleted.value = false;
      xpAwarded.value = 0;
      lessonFinished.value = false;
      completedLessonIds.value = [];
    },
  };
});
