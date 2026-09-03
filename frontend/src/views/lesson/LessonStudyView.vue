<template>
  <div class="lesson-study-view flex h-[calc(100vh-var(--app-header-h,68px))] w-full overflow-hidden bg-vdsa-bg-secondary font-sans relative">

    <!-- Floating Sidebar Open Button (when sidebar is collapsed) -->
    <button
      v-if="isSidebarCollapsed"
      @click="isSidebarCollapsed = false"
      type="button"
      class="absolute top-4 left-4 z-40 p-2.5 rounded-xl bg-[#1a182c] hover:bg-[#25223e] border border-purple-500/30 text-purple-300 shadow-xl shadow-purple-900/20 transition-all flex items-center gap-1.5 text-xs font-bold cursor-pointer"
      title="Mở danh sách bài học"
    >
      <BaseIcon name="list" class="w-4 h-4" />
      <span class="hidden sm:inline">Mục lục</span>
    </button>

    <!-- LEFT SIDEBAR: Course Mini Map -->
    <aside
      class="shrink-0 bg-vdsa-surface border-r border-vdsa-border flex flex-col h-full overflow-hidden shadow-xl z-30 transition-all duration-300"
      :class="isSidebarCollapsed ? '-ml-72 lg:-ml-80 w-72 lg:w-80 pointer-events-none' : 'w-72 lg:w-80'"
    >
       <!-- Course Title, Progress & Search -->
       <div class="p-4 lg:p-5 border-b border-vdsa-border shrink-0">
         <div class="flex items-center justify-between gap-2 mb-3">
           <h2 class="text-xs font-extrabold text-white line-clamp-1 uppercase tracking-wider">{{ course?.title || 'Lộ trình DSA' }}</h2>
           <button
             type="button"
             @click="isSidebarCollapsed = true"
             class="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors cursor-pointer"
             title="Thu gọn mục lục"
           >
             <BaseIcon name="chevron-left" class="w-4 h-4" />
           </button>
         </div>

         <!-- Course Overall Progress Bar -->
         <div class="mb-4 bg-[#141320] p-2.5 rounded-xl border border-vdsa-border">
           <div class="flex justify-between items-center text-[11px] mb-1.5">
             <span class="text-slate-400 font-semibold">Tiến độ khóa</span>
             <span class="text-purple-300 font-bold font-mono">{{ completedLessonsCount }}/{{ totalLessonsCount }} ({{ courseProgressPercent }}%)</span>
           </div>
           <div class="w-full h-1.5 bg-black/40 rounded-full overflow-hidden">
             <div
               class="h-full rounded-full bg-gradient-to-r from-purple-500 to-emerald-400 transition-all duration-500"
               :style="{ width: `${courseProgressPercent}%` }"
             ></div>
           </div>
         </div>

         <!-- Search Box -->
         <div class="relative">
           <BaseIcon name="search" class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-vdsa-muted" />
           <input v-model="searchQuery" type="text" placeholder="Tìm bài học..." class="w-full bg-vdsa-bg-secondary border border-vdsa-border rounded-xl pl-9 pr-4 py-2 text-xs font-semibold text-white focus:outline-none focus:border-vdsa-accent transition-colors placeholder:text-vdsa-disabled">
         </div>
       </div>

       <!-- Modules List -->
       <div class="flex-1 overflow-y-auto p-3 lg:p-4 space-y-1 custom-scrollbar">
         <div v-for="(module, mIdx) in groupedModules" :key="mIdx" class="module-group mb-2">
            <button @click="toggleModule(mIdx)" class="w-full flex items-center justify-between text-left py-2 px-2 rounded-lg group hover:bg-vdsa-hover transition-colors">
               <span class="text-xs font-bold text-vdsa-secondary group-hover:text-white transition-colors uppercase tracking-wider line-clamp-2 pr-2">{{ mIdx + 1 }}. {{ module.title }}</span>
               <BaseIcon :name="expandedModules.includes(mIdx) ? 'chevron-up' : 'chevron-down'" class="w-4 h-4 text-vdsa-muted transition-transform duration-300 shrink-0" />
            </button>

            <div v-if="expandedModules.includes(mIdx)" class="mt-1 ml-[11px] pl-4 border-l-2 border-vdsa-border space-y-1 py-1">
               <button v-for="lesson in module.lessons" :key="lesson.id"
                  @click="goToLesson(lesson.id)"
                  :disabled="isLessonLocked(lesson)"
                   class="w-full text-left py-2.5 px-3 rounded-xl transition-all relative flex items-center gap-3 group"
                   :class="lesson.id === lessonId ? 'bg-vdsa-accent/10 text-vdsa-accent font-bold ring-1 ring-vdsa-accent/40' : (isLessonLocked(lesson) ? 'text-vdsa-disabled cursor-not-allowed opacity-60' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover font-semibold')">

                   <!-- Timeline dot -->
                   <div class="w-[9px] h-[9px] rounded-full absolute -left-[21px] border-2 transition-colors z-10"
                        :class="lesson.id === lessonId ? 'bg-vdsa-accent border-vdsa-accent shadow-[0_0_8px_rgba(168,85,247,0.9)]' : 'bg-vdsa-surface border-vdsa-border group-hover:border-text-muted'"></div>

                   <span class="text-xs line-clamp-2 leading-snug">{{ cleanTitle(lesson.title) }}</span>
                   <BaseIcon
                     v-if="isLessonCompleted(lesson)"
                     name="check-circle"
                     class="w-4 h-4 text-vdsa-green ml-auto shrink-0"
                   />
                   <BaseIcon
                     v-else-if="isLessonLocked(lesson)"
                     name="lock"
                     class="w-4 h-4 text-vdsa-disabled ml-auto shrink-0"
                   />
                </button>
            </div>
         </div>
         <div v-if="groupedModules.length === 0 && !lessonStore.isLoading" class="text-center text-xs text-vdsa-muted mt-4">
           Không tìm thấy bài học nào.
         </div>
       </div>
    </aside>

    <!-- RIGHT CONTENT: Lesson Content -->
    <div class="flex-1 flex flex-col h-full min-w-0">
      <header class="px-4 lg:px-6 py-2.5 border-b border-vdsa-border bg-vdsa-bg-secondary backdrop-blur-md flex items-center justify-between shrink-0 shadow-lg z-20 flex-wrap gap-2">
        <div class="flex items-center gap-3 min-w-0 flex-1">
          <router-link :to="route.query.classId ? `/classes/${route.query.classId}` : (courseId ? `/path/${courseId}` : '/path')" class="text-xs font-semibold text-vdsa-muted hover:text-white transition-colors flex items-center gap-1 shrink-0 whitespace-nowrap">
            <BaseIcon name="arrow-left" class="w-3.5 h-3.5" /> Thoát
          </router-link>
          <span class="text-vdsa-disabled shrink-0">|</span>
          <h2 class="text-xs sm:text-sm font-extrabold text-white truncate" v-if="lessonStore.currentLesson">
            {{ cleanTitle(lessonStore.currentLesson.title) }}
          </h2>
          <h2 class="text-xs sm:text-sm font-extrabold text-vdsa-muted truncate" v-else-if="lessonStore.isLoading">Đang tải bài học...</h2>
        </div>

        <div class="flex items-center gap-2 shrink-0">
          <!-- Previous / Next Navigation in Header -->
          <div class="flex items-center gap-1 mr-1">
            <button
              v-if="prevLessonId"
              @click="goToLesson(prevLessonId)"
              class="px-2.5 py-1 rounded-lg bg-vdsa-surface hover:bg-vdsa-hover border border-vdsa-border text-slate-300 hover:text-white text-xs font-semibold flex items-center gap-1 transition-colors cursor-pointer"
              title="Bài trước"
            >
              <BaseIcon name="chevron-left" class="w-3.5 h-3.5" />
              <span class="hidden md:inline">Trước</span>
            </button>
            <button
              v-if="nextLessonIdNav"
              @click="goToLesson(nextLessonIdNav)"
              class="px-2.5 py-1 rounded-lg bg-vdsa-surface hover:bg-vdsa-hover border border-vdsa-border text-slate-300 hover:text-white text-xs font-semibold flex items-center gap-1 transition-colors cursor-pointer"
              title="Bài tiếp theo"
            >
              <span class="hidden md:inline">Sau</span>
              <BaseIcon name="chevron-right" class="w-3.5 h-3.5" />
            </button>
          </div>

          <span class="px-2.5 py-1 rounded-lg bg-vdsa-yellow/20 text-vdsa-yellow border border-vdsa-yellow/30 font-bold font-mono text-xs flex items-center gap-1.5 whitespace-nowrap shrink-0">
            <svg class="w-3.5 h-3.5 text-vdsa-yellow" fill="currentColor" viewBox="0 0 24 24">
              <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" />
            </svg>
            <span>+{{ lessonStore.currentLesson?.xpReward ?? 0 }} XP</span>
          </span>
        </div>
      </header>

      <div v-if="lessonStore.isOfflineFallback" class="px-6 py-2 bg-vdsa-yellow/10 border-b border-vdsa-yellow/30 text-xs text-vdsa-yellow flex items-center gap-2" role="alert">
        <BaseIcon name="warning" class="w-3.5 h-3.5 flex-shrink-0" />
        <span>Không kết nối được máy chủ — đang hiển thị nội dung bài học cục bộ.</span>
      </div>

      <main class="flex-1 relative w-full h-full overflow-hidden">
        <!-- Loading -->
        <div v-if="lessonStore.isLoading && !lessonStore.currentLesson" class="w-full h-full flex flex-col items-center justify-center text-center">
          <div class="inline-block w-8 h-8 border-4 border-vdsa-accent/20 border-t-vdsa-purple-light rounded-full animate-spin"></div>
          <p class="text-vdsa-muted mt-4">Đang tải bài học...</p>
        </div>

        <!-- Error -->
        <div v-else-if="lessonStore.error && !lessonStore.currentLesson" class="w-full h-full flex flex-col items-center justify-center text-center px-6">
          <div class="text-5xl mb-4"><BaseIcon name="warning" class="w-14 h-14 text-vdsa-red mx-auto" /></div>
          <h3 class="text-xl font-bold text-vdsa-secondary">{{ lessonStore.error }}</h3>
          <p class="text-vdsa-muted mt-2">Vui lòng quay lại lộ trình và thử lại.</p>
          <router-link :to="courseId ? `/path/${courseId}` : '/path'" class="mt-6 px-6 py-2.5 bg-vdsa-accent text-white font-semibold rounded-xl hover:bg-vdsa-accent-dark transition-colors">
            Quay lại lộ trình
          </router-link>
        </div>

        <!-- Blocked / Not Enrolled State (Chặn XP Bypass) -->
        <div v-else-if="!isEnrolled && courseId" class="w-full h-full flex flex-col items-center justify-center text-center px-6 py-12">
          <div class="relative w-20 h-20 mb-6 flex items-center justify-center rounded-3xl bg-vdsa-accent/10 text-vdsa-accent border border-vdsa-accent/30 shadow-[0_0_30px_rgba(168,85,247,0.25)]">
            <BaseIcon name="lock" class="w-10 h-10" />
          </div>
          <h3 class="text-2xl font-black text-white">Bạn chưa tham gia lộ trình này</h3>
          <p class="text-vdsa-secondary mt-2 max-w-md text-sm leading-relaxed">
            Bạn cần tham gia lộ trình <strong class="text-white">{{ course?.title || 'này' }}</strong> để mở khóa bài học và nhận điểm XP.
          </p>
          <div class="flex items-center gap-3 mt-8">
            <router-link :to="courseId ? `/path/${courseId}` : '/path'" class="px-6 py-2.5 rounded-xl font-bold bg-vdsa-surface hover:bg-vdsa-hover text-vdsa-secondary hover:text-white border border-vdsa-border transition-all">
              Quay lại lộ trình
            </router-link>
            <button
              @click="showEnrollModal = true"
              class="px-6 py-2.5 rounded-xl font-bold bg-vdsa-accent hover:bg-vdsa-accent-light text-white shadow-lg shadow-vdsa-accent transition-all flex items-center gap-2 cursor-pointer"
            >
              Tham gia lộ trình (1 🤍)
              <BaseIcon name="plus" class="w-4 h-4" />
            </button>
          </div>
        </div>

        <!-- Content -->
        <template v-else-if="lessonStore.currentLesson">
          <LessonStepTheory
            v-if="!lessonStore.lessonMeta?.sandboxType || lessonStore.lessonMeta?.sandboxType === 'dsa'"
            :title="lessonStore.currentLesson.title"
            :content="lessonStore.currentLesson.theoryContent"
            :is-completed="isCurrentLessonCompleted"
            :simulation-key="lessonStore.simulationKey"
            :simulation-keys="lessonStore.simulationKeys"
            @completeStep="onQuizComplete"
          />

          <LessonStepQuiz
            v-else-if="lessonStore.lessonMeta?.sandboxType === 'quiz'"
            :questions="lessonStore.currentLesson.quizQuestions ?? []"
            :initial-submission="lessonStore.currentLesson.lastQuizSubmission ?? lessonStore.lessonMeta?.lastQuizSubmission ?? null"
            :exercise-id="lessonStore.lessonMeta?.quizId ?? lessonStore.lessonMeta?.exerciseId ?? null"
            @submit="onQuizSubmit"
            @completeStep="onQuizComplete"
          />

          <LessonStepCodeLab
            v-else-if="lessonStore.lessonMeta?.sandboxType === 'codelab'"
            :problem-title="`Thực hành: ${lessonStore.currentLesson.title}`"
            :codelab-task="lessonStore.currentLesson.codelabTask"
            :exercise-id="lessonStore.lessonMeta?.exerciseId ?? null"
            @completeLesson="onLessonComplete"
          />
        </template>
      </main>
    </div>

    <!-- Modal Xác nhận tham gia lộ trình -->
    <Teleport to="body">
      <div v-if="showEnrollModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
        <div class="bg-vdsa-surface border border-vdsa-border rounded-2xl p-6 max-w-sm w-full shadow-2xl animate-fade-in-up">
          <div class="flex items-center gap-3 mb-4 text-vdsa-yellow">
            <BaseIcon name="alert-circle" class="w-6 h-6" />
            <h3 class="text-xl font-bold text-white">Mở khóa lộ trình</h3>
          </div>
          <p class="text-vdsa-secondary mb-6 leading-relaxed">
            Bạn có muốn mở khóa lộ trình <strong class="text-white">{{ course?.title || 'này' }}</strong> để bắt đầu học bài học này không?
            <br /><span class="text-xs text-purple-300 font-medium inline-block mt-2">Đăng ký lộ trình với 1 🤍. Bài 1 miễn phí, các bài tiếp theo tốn 1 🤍 khi tham gia lần đầu (học lại miễn phí sau khi pass).</span>
          </p>
          <div class="flex gap-3 justify-end">
            <button
              @click="showEnrollModal = false"
              class="px-5 py-2.5 rounded-xl font-semibold text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
            >
              Hủy bỏ
            </button>
            <button
              @click="confirmLessonEnroll"
              class="px-5 py-2.5 rounded-xl font-semibold bg-vdsa-accent hover:bg-vdsa-accent-light text-white shadow-lg shadow-vdsa-accent transition-all flex items-center gap-2 cursor-pointer"
            >
              Mở khóa ngay (1 🤍)
              <BaseIcon name="check" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </Teleport>

    <LessonCompletionModal
      :show="showCompletionModal"
      :xp-reward="lessonStore.currentLesson?.xpReward ?? 0"
      :quiz-id="lessonStore.lessonMeta?.quizId"
      :next-lesson-id="nextLessonId"
      @go-next="goToNextLesson"
      @close="goBackToCourse"
    />

    <!-- Modal Vinh danh Tốt nghiệp / Hoàn thành Lộ trình -->
    <CourseCompletionModal
      :show="showCourseCompletedModal"
      :course-title="course?.title || 'Lộ trình DSA'"
      :total-xp="totalRoadmapXp"
      :lesson-xp="lessonStore.currentLesson?.xpReward ?? 50"
      @close="goBackToCourse"
      @explore-more="onExploreMoreCourses"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LessonStepTheory from './components/LessonStepTheory.vue';
import LessonStepQuiz from './components/LessonStepQuiz.vue';
import LessonStepCodeLab from './components/LessonStepCodeLab.vue';
import LessonCompletionModal from './LessonCompletionModal.vue';
import CourseCompletionModal from './CourseCompletionModal.vue';
import { useLessonStore } from '@/features/lesson/store/useLessonStore';
import { useCourseStore } from '@/features/courses/store/useCourseStore';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useHeartSystem } from '@/composables/useHeartSystem';
import { useUiStore } from '@/stores/ui';
import { fireConfetti } from '@/composables/useConfetti';
import { courseApi } from '@/services/courseApi';
import { normalizeVi } from '@/utils/searchNormalize';
import BaseIcon from '@/shared/components/BaseIcon.vue';

interface LessonDto {
  id: string;
  title: string;
  moduleTitle?: string;
  sandboxType?: string;
  status?: string;
  locked?: boolean;
}

interface CourseDetailDto {
  id: string;
  title: string;
  xpReward?: number;
  lessons: LessonDto[];
}

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();
const courseStore = useCourseStore();
const authStore = useAuthStore();
const gamificationStore = useGamificationStore();
const heartSystem = useHeartSystem();
const uiStore = useUiStore();

const isSidebarCollapsed = ref(false);
const showCompletionModal = ref(false);
const showCourseCompletedModal = ref(false);
const showEnrollModal = ref(false);
const nextLessonId = ref<string | null>(null);

function cleanTitle(rawTitle?: string): string {
  if (!rawTitle) return '';
  return rawTitle.replace(/Mini-Quizz/gi, 'Mini-Quiz');
}

const lessonId = computed(() => route.params.id as string);
const courseId = computed(() => {
  const fromQuery = route.query.courseId;
  if (typeof fromQuery === 'string' && fromQuery.length > 0) return fromQuery;
  return lessonStore.lessonMeta?.courseId ?? null;
});

const isOwnCourse = computed(() => {
  if (!authStore.isAuthenticated) return false;
  if (authStore.user?.role === 'ADMIN') return true;
  const uId = authStore.user?.id;
  const authorId = (course.value as any)?.authorId ?? (course.value as any)?.author?.id;
  const createdBy = (course.value as any)?.createdBy;
  return (
    authStore.user?.role === 'TEACHER' &&
    uId != null &&
    (authorId === uId || createdBy === uId)
  );
});

const isEnrolled = computed(() => {
  if (isOwnCourse.value) return true;
  const cId = courseId.value;
  if (!cId) return true; // Standalone lesson hoặc không gắn roadmap
  if (completedLessonsCount.value > 0) {
    courseStore.enrollCourse(String(cId));
    return true;
  }
  return courseStore.isEnrolled(String(cId));
});

async function confirmLessonEnroll() {
  if (!authStore.isAuthenticated) {
    router.push({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }
  const cId = courseId.value;
  if (!cId) return;
  const ok = await heartSystem.spendHeartSafely('Mở khóa lộ trình', isOwnCourse.value);
  if (!ok) return;

  courseStore.enrollCourse(String(cId));
  showEnrollModal.value = false;
  uiStore.showToast('Mở khóa lộ trình thành công! (-1 🤍)', 'success');
}

const course = ref<CourseDetailDto | null>(null);
const searchQuery = ref('');
const expandedModules = ref<number[]>([]);

const groupedModules = computed(() => {
  if (!course.value?.lessons) return [];

  const q = normalizeVi(searchQuery.value);
  const map = new Map<string, { title: string, lessons: LessonDto[] }>();

  course.value.lessons.forEach(l => {
    if (l.sandboxType === 'folder') return;
    const normTitle = normalizeVi(l.title);
    const normModuleTitle = normalizeVi(l.moduleTitle || '');
    if (q && !normTitle.includes(q) && !normModuleTitle.includes(q)) return;

    const mTitle = l.moduleTitle || 'General';
    if (!map.has(mTitle)) {
      map.set(mTitle, { title: mTitle, lessons: [] });
    }
    map.get(mTitle)!.lessons.push(l);
  });

  return Array.from(map.values());
});

const toggleModule = (mIdx: number) => {
  if (expandedModules.value.includes(mIdx)) {
    expandedModules.value = expandedModules.value.filter(i => i !== mIdx);
  } else {
    expandedModules.value.push(mIdx);
  }
};

const completedLessonIds = ref<Set<string>>(new Set());

/** Bài đã hoàn thành: ưu tiên reactive set + status Completed từ backend/db + local storage theo course */
function isLessonCompleted(lesson: LessonDto): boolean {
  if (!lesson) return false;
  const key = String((lesson as any).nodeId ?? lesson.id ?? '');
  if (!key) return false;

  if (completedLessonIds.value.has(key)) return true;
  if (lesson.status === 'Completed') return true;
  const cId = courseId.value;
  const uId = authStore.user?.id;
  if (cId && uId && localStorage.getItem(`course_done_u${uId}_${cId}_${key}`) === 'true') {
    completedLessonIds.value.add(key);
    return true;
  }
  return false;
}

const isCurrentLessonCompleted = computed(() => {
  const curId = String(lessonId.value);
  const currentL = course.value?.lessons?.find(l => String(l.id) === curId || String((l as any).nodeId) === curId);
  if (currentL && isLessonCompleted(currentL)) return true;
  return isLessonCompleted({ id: curId } as any);
});


// Thống kê tiến độ toàn khóa học (tự động cập nhật real-time)
const totalLessonsCount = computed(() => {
  return (course.value?.lessons ?? []).filter(l => l.sandboxType !== 'folder').length;
});

const completedLessonsCount = computed(() => {
  return (course.value?.lessons ?? []).filter(l => l.sandboxType !== 'folder' && isLessonCompleted(l)).length;
});

const courseProgressPercent = computed(() => {
  if (totalLessonsCount.value === 0) return 0;
  return Math.round((completedLessonsCount.value / totalLessonsCount.value) * 100);
});

const totalRoadmapXp = computed(() => {
  return course.value?.xpReward || (totalLessonsCount.value * 100);
});

function isLessonLocked(lesson: LessonDto): boolean {
  if (isOwnCourse.value) return false;
  if (isLessonCompleted(lesson)) return false; // Bài đã hoàn thành luôn mở
  if (!course.value?.lessons || course.value.lessons.length === 0) return lesson.locked ?? false;
  const lessons = course.value.lessons;
  const idx = lessons.findIndex(l => String(l.id) === String(lesson.id));
  if (idx === 0) return false; // Bài đầu tiên luôn mở
  if (idx < 0) return lesson.locked ?? true;
  // Mở khóa nếu bài liền trước đã hoàn thành
  const prevLesson = lessons[idx - 1];
  if (prevLesson && isLessonCompleted(prevLesson)) return false;
  return lesson.locked ?? true;
}



// Điều hướng Previous / Next trên Header
const currentLessonIndex = computed(() => {
  const lessons = course.value?.lessons ?? [];
  return lessons.findIndex(l => String(l.id) === String(lessonId.value));
});

const prevLessonId = computed(() => {
  const lessons = course.value?.lessons ?? [];
  const idx = currentLessonIndex.value;
  if (idx > 0) return String(lessons[idx - 1].id);
  return null;
});

const nextLessonIdNav = computed(() => {
  const lessons = course.value?.lessons ?? [];
  const idx = currentLessonIndex.value;
  if (idx >= 0 && idx < lessons.length - 1) {
    const next = lessons[idx + 1];
    if (!isLessonLocked(next)) return String(next.id);
  }
  return null;
});

async function enterLessonNode(cId: string | number, lId: string | number): Promise<boolean> {
  const pathId = Number(cId);
  const nodeId = Number(lId);
  if (!pathId || !nodeId) return true;

  if (isOwnCourse.value) return true;

  // Node 1 (bài học đầu tiên) luôn Miễn phí (0 tim vì đã trả lúc đăng ký lộ trình)
  const playableLessons = (course.value?.lessons ?? []).filter(l => l.sandboxType !== 'folder');
  const firstLesson = playableLessons[0];
  const isFirstNode = firstLesson && (String(firstLesson.id) === String(lId) || ((firstLesson as any).nodeId && (firstLesson as any).nodeId === nodeId));
  if (isFirstNode) return true;

  // Bài đã hoàn thành -> Xem lại / học lại Miễn phí (0 tim)
  const target = course.value?.lessons?.find(l => String(l.id) === String(lId) || String((l as any).nodeId) === String(nodeId) || String((l as any).lessonId) === String(lId));
  if (target && isLessonCompleted(target)) return true;

  // Các bài học mới tiếp theo -> tốn 1 tim khi vào lần đầu
  return heartSystem.enterLessonNode(pathId, nodeId, false, isOwnCourse.value);
}

async function goToLesson(id: string) {
  const target = course.value?.lessons.find(l => l.id === id);
  if (!isOwnCourse.value && target && isLessonLocked(target)) return; // node chưa mở khoá — không cho vào
  if (!isEnrolled.value) {
    showEnrollModal.value = true;
    return;
  }
  if (courseId.value) {
    const ok = await enterLessonNode(courseId.value, id);
    if (!ok) return;
  }
  router.push({
    name: 'lesson-study',
    params: { id },
    query: {
      ...route.query,
      ...(courseId.value ? { courseId: courseId.value } : {}),
    },
  });
}

watch(courseId, async (id) => {
  if (id) {
    courseStore.enrollCourse(String(id));
  }
  if (id && !course.value) {
    try {
      course.value = await courseApi.getCourseById(id) as unknown as CourseDetailDto;
      if (course.value.lessons) {
        courseStore.updateCourseLessons(String(id), course.value.lessons);
        const prog = courseStore.getCourseProgress(String(id));
        const c = courseStore.getCourseById(String(id));
        if (c) {
          (c as any).completedLessons = prog.completedLessonIds.length;
          (c as any).progressPercent = prog.progressPercent;
        }

        // Đồng bộ các bài đã hoàn thành vào reactive set
        const uId = authStore.user?.id;
        course.value.lessons.forEach(l => {
          if (l.status === 'Completed') {
            completedLessonIds.value.add(String(l.id));
          }
          if (uId && localStorage.getItem(`course_done_u${uId}_${id}_${l.id}`) === 'true') {
            completedLessonIds.value.add(String(l.id));
          }
        });
        completedLessonIds.value = new Set(completedLessonIds.value);

        // Tự động mở module chứa bài học hiện tại
        const currentMTitle = course.value.lessons.find(l => l.id === lessonId.value)?.moduleTitle || 'General';
        const map = new Map<string, boolean>();
        course.value.lessons.forEach(l => map.set(l.moduleTitle || 'General', true));
        const uniqueModules = Array.from(map.keys());
        const moduleIdx = uniqueModules.indexOf(currentMTitle);
        if (moduleIdx !== -1 && !expandedModules.value.includes(moduleIdx)) {
          expandedModules.value.push(moduleIdx);
        }
      }
    } catch (e) {
      console.warn('Không tải được thông tin khóa học cho sidebar', e);
    }
  }
}, { immediate: true });

const FULL_STEPS = [
  { number: 1, label: 'Lý Thuyết' },
  { number: 2, label: 'Quiz' },
  { number: 3, label: 'Code Lab' },
];

const steps = computed(() => {
  if (lessonStore.lessonMeta?.sandboxType === 'quiz') {
    return [FULL_STEPS[1]]; // Chỉ hiện tab Quiz
  }
  const hasCodelab = !!lessonStore.currentLesson?.codelabTask;
  return hasCodelab ? FULL_STEPS : FULL_STEPS.slice(0, 2);
});

async function onQuizSubmit(answers: Record<string, number | number[]>): Promise<void> {
  await lessonStore.submitQuiz(answers);
}

function onQuizComplete(): void {
  if (lessonStore.currentLesson?.codelabTask) {
    lessonStore.goToStep(3);
  } else {
    void finishLesson();
  }
}

async function onLessonComplete(): Promise<void> {
  await lessonStore.completeCodelab();
  void finishLesson();
}

function resolveNextLessonId(): string | null {
  const currentId = String(lessonId.value);
  const lessons = (course.value?.lessons ?? []).filter(l => l.sandboxType !== 'folder');
  const currentIdx = lessons.findIndex(l =>
    String(l.id) === currentId ||
    String((l as any).lessonId) === currentId ||
    String((l as any).nodeId) === currentId
  );
  if (currentIdx === -1 || currentIdx >= lessons.length - 1) return null;
  const next = lessons[currentIdx + 1];
  return String(next.id || (next as any).lessonId || (next as any).nodeId);
}

async function finishLesson(): Promise<void> {
  if (lessonStore.lessonMeta?.sandboxType === 'quiz') {
    const sub = lessonStore.currentLesson?.lastQuizSubmission ?? lessonStore.lessonMeta?.lastQuizSubmission;
    if (sub && !sub.passed) {
      uiStore.showToast('Bạn cần đạt tối thiểu 70% điểm để hoàn thành bài trắc nghiệm này.', 'warning');
      return;
    }
  }

  const rewardXp = lessonStore.currentLesson?.xpReward ?? 0;
  const strCurId = String(lessonId.value);
  await lessonStore.markLessonCompleted(lessonId.value);
  try {
    await lessonStore.syncToServer(true);
  } catch (err) {
    console.warn('Sync lesson progress error', err);
  }

  // Cập nhật đúng bài đang học
  const currentL = course.value?.lessons?.find(l => 
    String(l.id) === strCurId || String((l as any).nodeId) === strCurId
  );
  const targetKey = currentL ? String((currentL as any).nodeId ?? currentL.id) : strCurId;

  completedLessonIds.value.add(targetKey);
  completedLessonIds.value.add(strCurId);
  if (currentL) {
    currentL.status = 'Completed';
  }
  const uId = authStore.user?.id;
  if (courseId.value && uId) {
    localStorage.setItem(`course_done_u${uId}_${courseId.value}_${targetKey}`, 'true');
  }
  completedLessonIds.value = new Set(completedLessonIds.value);

  if (courseId.value) {
    if (course.value?.lessons) {
      courseStore.updateCourseLessons(String(courseId.value), course.value.lessons);
    }
    const prog = courseStore.getCourseProgress(String(courseId.value));
    const c = courseStore.getCourseById(String(courseId.value));
    if (c) {
      (c as any).completedLessons = completedLessonsCount.value || prog.completedLessonIds.length;
      (c as any).progressPercent = courseProgressPercent.value || prog.progressPercent;
    }
  }

  const nextId = resolveNextLessonId();
  nextLessonId.value = nextId;

  const isAllCourseFinished = totalLessonsCount.value > 0 && completedLessonsCount.value >= totalLessonsCount.value;

  if (nextId) {
    fireConfetti('node-pass');
    showCompletionModal.value = true;
  } else if (isAllCourseFinished && totalLessonsCount.value > 1) {
    fireConfetti('levelup');
    showCourseCompletedModal.value = true;
  } else {
    fireConfetti('node-pass');
    showCompletionModal.value = true;
  }
}

async function goToNextLesson(nextId: string): Promise<void> {
  if (courseId.value) {
    const ok = await enterLessonNode(courseId.value, nextId);
    if (!ok) return;
  }
  showCompletionModal.value = false;
  const q: Record<string, string> = {};
  if (courseId.value) q.courseId = String(courseId.value);
  if (route.query.classId) q.classId = String(route.query.classId);
  void router.push({ name: 'lesson-study', params: { id: nextId }, query: q });
}

function goBackToCourse(): void {
  showCompletionModal.value = false;
  showCourseCompletedModal.value = false;
  const fromClassId = route.query.classId;
  if (fromClassId) {
    void router.push(`/classes/${fromClassId}`);
    return;
  }
  void router.push(courseId.value ? `/path/${courseId.value}` : '/path');
}

function onExploreMoreCourses(): void {
  showCourseCompletedModal.value = false;
  const fromClassId = route.query.classId;
  if (fromClassId) {
    void router.push(`/path`);
    return;
  }
  void router.push('/path');
}

function autoExpandCurrentModule(currLessonId: string): void {
  if (!course.value?.lessons) return;
  const currentMTitle = course.value.lessons.find(l => String(l.id) === String(currLessonId))?.moduleTitle || 'General';
  const map = new Map<string, boolean>();
  course.value.lessons.forEach(l => map.set(l.moduleTitle || 'General', true));
  const uniqueModules = Array.from(map.keys());
  const moduleIdx = uniqueModules.indexOf(currentMTitle);
  if (moduleIdx !== -1 && !expandedModules.value.includes(moduleIdx)) {
    expandedModules.value.push(moduleIdx);
  }
}

function scrollToTopContainers() {
  window.scrollTo({ top: 0, left: 0, behavior: 'instant' });
  if (document.documentElement) document.documentElement.scrollTop = 0;
  if (document.body) document.body.scrollTop = 0;
  const scrollSelectors = [
    'main',
    '.lesson-step-theory',
    '.lesson-step-quiz',
    '.lesson-step-codelab',
    '.theory-content-container',
    '.custom-scrollbar',
    '.lesson-study__body',
  ];
  scrollSelectors.forEach(sel => {
    document.querySelectorAll(sel).forEach(el => {
      el.scrollTo({ top: 0, left: 0, behavior: 'instant' });
    });
  });
}

watch(lessonId, async (id) => {
  if (id) {
    autoExpandCurrentModule(id);
    scrollToTopContainers();
    await lessonStore.loadLesson(id);
    await nextTick();
    scrollToTopContainers();
  }
}, { immediate: true });

watch(() => lessonStore.activeStep, () => {
  void nextTick(() => {
    scrollToTopContainers();
  });
});
</script>
