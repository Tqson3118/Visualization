<template>
  <div class="lesson-study-view flex h-[calc(100vh-64px)] w-full overflow-hidden bg-vdsa-bg-secondary font-sans">
    
    <!-- LEFT SIDEBAR: Course Mini Map -->
    <aside class="w-72 lg:w-80 shrink-0 bg-vdsa-surface border-r border-vdsa-border flex flex-col h-full overflow-hidden shadow-xl z-30">
       <!-- Course Title & Search -->
       <div class="p-4 lg:p-5 border-b border-vdsa-border shrink-0">
         <h2 class="text-sm font-extrabold text-white mb-4 line-clamp-2">{{ course?.title || 'Đang tải khóa học...' }}</h2>
         <!-- Search Box -->
         <div class="relative">
           <BaseIcon name="search" class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-vdsa-muted" />
           <input v-model="searchQuery" type="text" placeholder="Tìm bài học..." class="w-full bg-vdsa-bg-secondary border border-vdsa-border rounded-xl pl-9 pr-4 py-2 text-xs font-semibold text-white focus:outline-none focus:border-accent transition-colors placeholder:text-vdsa-disabled">
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
                  :disabled="lesson.locked"
                   class="w-full text-left py-2.5 px-3 rounded-xl transition-all relative flex items-center gap-3 group"
                   :class="lesson.id === lessonId ? 'bg-vdsa-accent/10 text-vdsa-accent font-bold ring-1 ring-vdsa-accent/40' : (lesson.locked ? 'text-vdsa-disabled cursor-not-allowed opacity-60' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover font-semibold')">
                   
                   <!-- Timeline dot -->
                   <div class="w-[9px] h-[9px] rounded-full absolute -left-[21px] border-2 transition-colors z-10"
                        :class="lesson.id === lessonId ? 'bg-vdsa-accent border-vdsa-accent shadow-[0_0_8px_rgba(168,85,247,0.9)]' : 'bg-vdsa-surface border-vdsa-border group-hover:border-text-muted'"></div>
                   
                   <span class="text-xs line-clamp-2 leading-snug">{{ lesson.title }}</span>
                   <BaseIcon
                     v-if="isLessonCompleted(lesson)"
                     name="check-circle"
                     class="w-4 h-4 text-vdsa-green ml-auto shrink-0"
                   />
                   <BaseIcon
                     v-else-if="lesson.locked"
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
      <header class="px-6 py-3 border-b border-vdsa-border bg-vdsa-bg-secondary backdrop-blur-md flex items-center justify-between shrink-0 shadow-lg z-20 flex-wrap gap-2">
        <div class="flex items-center gap-3 min-w-0">
          <router-link :to="courseId ? `/courses/${courseId}` : '/courses'" class="text-xs font-semibold text-vdsa-muted hover:text-white transition-colors flex items-center gap-1 shrink-0">
            <BaseIcon name="arrow-left" class="w-3.5 h-3.5" /> Thoát khóa học
          </router-link>
          <span class="text-vdsa-disabled">|</span>
          <h2 class="text-sm font-extrabold text-white line-clamp-1" v-if="lessonStore.currentLesson">
            {{ lessonStore.currentLesson.title }}
          </h2>
          <h2 class="text-sm font-extrabold text-vdsa-muted line-clamp-1" v-else-if="lessonStore.isLoading">Đang tải bài học...</h2>
        </div>




        <div class="flex items-center gap-2 font-mono text-xs">
          <span class="px-2.5 py-1 rounded-lg bg-vdsa-yellow/50 text-vdsa-yellow border border-vdsa-yellow/30 font-bold flex items-center gap-1.5">
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
          <div class="inline-block w-8 h-8 border-4 border-accent/20 border-t-indigo-500 rounded-full animate-spin"></div>
          <p class="text-vdsa-muted mt-4">Đang tải bài học...</p>
        </div>

        <!-- Error -->
        <div v-else-if="lessonStore.error && !lessonStore.currentLesson" class="w-full h-full flex flex-col items-center justify-center text-center px-6">
          <div class="text-5xl mb-4"><BaseIcon name="warning" class="w-14 h-14 text-vdsa-red mx-auto" /></div>
          <h3 class="text-xl font-bold text-vdsa-secondary">{{ lessonStore.error }}</h3>
          <p class="text-vdsa-muted mt-2">Vui lòng quay lại khóa học và thử lại.</p>
          <router-link :to="courseId ? `/courses/${courseId}` : '/courses'" class="mt-6 px-6 py-2.5 bg-accent text-white font-semibold rounded-xl hover:bg-vdsa-accent-dark transition-colors">
            Quay lại khóa học
          </router-link>
        </div>

        <!-- Content -->
        <template v-else-if="lessonStore.currentLesson">
          <LessonStepTheory
            v-if="!lessonStore.lessonMeta?.sandboxType || lessonStore.lessonMeta?.sandboxType === 'dsa'"
            :title="lessonStore.currentLesson.title"
            :content="lessonStore.currentLesson.theoryContent"
            @completeStep="onQuizComplete"
          />

          <LessonStepQuiz
            v-else-if="lessonStore.lessonMeta?.sandboxType === 'quiz'"
            :questions="lessonStore.currentLesson.quizQuestions ?? []"
            @submit="onQuizSubmit"
            @completeStep="onQuizComplete"
          />

          <LessonStepCodeLab
            v-else-if="lessonStore.lessonMeta?.sandboxType === 'codelab' && lessonStore.currentLesson.codelabTask"
            :problem-title="`Thực hành: ${lessonStore.currentLesson.title}`"
            :codelab-task="lessonStore.currentLesson.codelabTask"
            :exercise-id="lessonStore.lessonMeta?.exerciseId ?? null"
            @completeLesson="onLessonComplete"
          />
        </template>
      </main>
    </div>

    <LessonCompletionModal
      :show="showCompletionModal"
      :xp-reward="lessonStore.currentLesson?.xpReward ?? 0"
      :quiz-id="lessonStore.lessonMeta?.quizId"
      :next-lesson-id="nextLessonId"
      @go-quiz="goToQuiz"
      @go-next="goToNextLesson"
      @close="goBackToCourse"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LessonStepTheory from './components/LessonStepTheory.vue';
import LessonStepQuiz from './components/LessonStepQuiz.vue';
import LessonStepCodeLab from './components/LessonStepCodeLab.vue';
import LessonCompletionModal from './LessonCompletionModal.vue';
import { useLessonStore } from '@/features/lesson/store/useLessonStore';
import { courseApi } from '@/services/courseApi';
import BaseIcon from '@/shared/components/BaseIcon.vue';

interface LessonDto {
  id: string;
  title: string;
  moduleTitle?: string;
  status?: string;
  locked?: boolean;
}

interface CourseDetailDto {
  id: string;
  title: string;
  lessons: LessonDto[];
}

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();

const showCompletionModal = ref(false);
const nextLessonId = ref<string | null>(null);

const lessonId = computed(() => route.params.id as string);
const courseId = computed(() => {
  const fromQuery = route.query.courseId;
  if (typeof fromQuery === 'string' && fromQuery.length > 0) return fromQuery;
  return lessonStore.lessonMeta?.courseId ?? null;
});

const course = ref<CourseDetailDto | null>(null);
const searchQuery = ref('');
const expandedModules = ref<number[]>([]);

const groupedModules = computed(() => {
  if (!course.value?.lessons) return [];
  
  const q = searchQuery.value.toLowerCase().trim();
  const map = new Map<string, { title: string, lessons: LessonDto[] }>();
  
  course.value.lessons.forEach(l => {
    if (q && !l.title.toLowerCase().includes(q) && !(l.moduleTitle || '').toLowerCase().includes(q)) return;
    
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

function goToLesson(id: string) {
  const target = course.value?.lessons.find(l => l.id === id);
  if (target?.locked) return; // node chưa mở khoá — không cho vào (backend cũng chặn 403)
  router.push({ name: 'lesson-study', params: { id }, query: courseId.value ? { courseId: courseId.value } : {} });
}

/** Bài đã hoàn thành: đánh dấu local khi bấm "Hoàn thành bài học" (tick tức thì) +
 *  status Completed từ backend cho các bài khác. */
function isLessonCompleted(lesson: LessonDto): boolean {
  return lessonStore.completedLessonIds.includes(lesson.id) || lesson.status === 'Completed';
}

watch(courseId, async (id) => {
  if (id && !course.value) {
    try {
      course.value = await courseApi.getCourseById(id) as unknown as CourseDetailDto;
      if (course.value.lessons) {
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

async function onQuizSubmit(answers: Record<string, number>): Promise<void> {
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

async function finishLesson(): Promise<void> {
  lessonStore.markLessonCompleted(lessonId.value);
  // Đồng bộ "đã hoàn thành" lên backend NGAY (node pass → mở khoá bài tiếp theo — nghiệp vụ
  // lộ trình tuần tự); quiz/codelab đã sync riêng, cờ Completed bổ sung cho bài lý thuyết.
  void lessonStore.syncToServer(true);
  nextLessonId.value = await resolveNextLessonId();
  showCompletionModal.value = true;
}

/** Tìm bài kế tiếp trong cùng roadmap. API đã trả lessons theo đúng thứ tự
 *  (module → item) nên KHÔNG sort lại — sort theo orderIndex sẽ trộn giữa các chặng. */
async function resolveNextLessonId(): Promise<string | null> {
  const courseIdValue = courseId.value;
  const currentId = lessonId.value;
  if (!courseIdValue || !currentId) return null;
  try {
    const data = await courseApi.getCourseById(courseIdValue) as unknown as {
      lessons?: Array<{ id: string }>;
    };
    const lessons = data.lessons ?? [];
    const currentIdx = lessons.findIndex(l => l.id === currentId);
    if (currentIdx === -1 || currentIdx >= lessons.length - 1) return null;
    return lessons[currentIdx + 1].id;
  } catch (err) {
    console.warn('Không tải được danh sách bài kế tiếp:', err);
    return null;
  }
}

function goToQuiz(quizId: string): void {
  showCompletionModal.value = false;
  // Quiz được nhúng trong flow bài học (Theory → Quiz → CodeLab) — không có route quiz riêng.
  void quizId;
}

function goToNextLesson(nextId: string): void {
  showCompletionModal.value = false;
  router.push({ name: 'lesson-study', params: { id: nextId }, query: courseId.value ? { courseId: courseId.value } : {} });
}

function goBackToCourse(): void {
  showCompletionModal.value = false;
  router.push(courseId.value ? `/courses/${courseId.value}` : '/courses');
}

watch(lessonId, (id) => {
  if (id) void lessonStore.loadLesson(id);
}, { immediate: true });
</script>
