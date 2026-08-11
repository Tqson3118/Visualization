<template>
  <div class="course-detail-view w-full animate-fade-in">
    
    <div v-if="loading" class="text-center py-32">
      <div class="inline-block w-8 h-8 border-4 border-accent/20 border-t-indigo-500 rounded-full animate-spin"></div>
      <p class="text-text-muted mt-4">Đang tải thông tin khóa học...</p>
    </div>

    <div v-else-if="error" class="text-center py-32 container mx-auto max-w-2xl">
      <div class="text-5xl mb-4"><BaseIcon name="warning" class="w-14 h-14 text-accent-red mx-auto" /></div>
      <h3 class="text-xl font-bold text-text-secondary">{{ error }}</h3>
      <p class="text-text-muted mt-2">Vui lòng thử lại sau hoặc liên hệ hỗ trợ.</p>
      <router-link to="/courses" class="mt-6 inline-block px-6 py-2 bg-bg-surface border border-border-subtle rounded-xl text-text-secondary hover:text-white transition">Quay lại</router-link>
    </div>

    <div v-else-if="course">
      <!-- HERO BANNER -->
      <div class="w-full relative overflow-hidden bg-bg-main pt-20 pb-24 px-4 border-b border-border-subtle/50 bg-cover bg-center bg-no-repeat"
           :style="{ backgroundImage: 'linear-gradient(to bottom, rgba(15, 16, 19, 0.6), rgba(15, 16, 19, 0.98)), url(' + (course.coverImage || '/images/data_structure_cover.png') + ')' }">
        <!-- Abstract gradient blobs -->
        <div class="absolute top-0 left-0 w-[500px] h-[500px] bg-indigo-600/10 rounded-full blur-[120px] -translate-x-1/2 -translate-y-1/2 pointer-events-none"></div>
        <div class="absolute bottom-0 right-0 w-[600px] h-[600px] bg-purple-600/10 rounded-full blur-[150px] translate-x-1/3 translate-y-1/3 pointer-events-none"></div>

        <router-link to="/courses" class="absolute top-6 left-6 md:top-8 md:left-8 z-20 text-sm font-semibold text-text-muted hover:text-white transition-colors flex items-center gap-2 bg-bg-surface/50 hover:bg-bg-surface px-4 py-2 rounded-xl backdrop-blur-md border border-border-subtle">
          <BaseIcon name="arrow-left" class="w-4 h-4" /> Về trang chủ
        </router-link>

        <div class="container mx-auto max-w-5xl relative z-10">

          <div class="flex flex-col items-center text-center max-w-3xl mx-auto">
            <div class="flex items-center gap-3 mb-6">
              <span class="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-widest bg-accent/20 text-accent border border-accent/30 flex items-center gap-2">
                <BaseIcon name="cpu" class="w-3 h-3" /> {{ getCategoryLabel(course.category) }}
              </span>
            </div>

            <h1 class="text-4xl md:text-5xl lg:text-6xl font-extrabold text-white leading-tight mb-6 tracking-tight">
              {{ course.title }}
            </h1>
            
            <p class="text-lg md:text-xl text-text-secondary leading-relaxed mb-8 max-w-2xl">
              {{ course.description }}
            </p>

            <div class="flex flex-wrap items-center justify-center gap-6 text-sm text-text-muted font-medium mb-10">
              <div class="flex items-center gap-1.5 text-accent-yellow">
                <BaseIcon name="star" class="w-4 h-4 fill-current" />
                <BaseIcon name="star" class="w-4 h-4 fill-current" />
                <BaseIcon name="star" class="w-4 h-4 fill-current" />
                <BaseIcon name="star" class="w-4 h-4 fill-current" />
                <BaseIcon name="star-half" class="w-4 h-4 fill-current" />
                <span class="text-white ml-1">4.9</span>
              </div>
              <div class="flex items-center gap-2"><BaseIcon name="book-open" class="w-4 h-4" /> {{ course.lessons.length }} Lessons</div>
              <div class="flex items-center gap-2"><BaseIcon name="clock" class="w-4 h-4" /> 25h Total</div>
              <div class="flex items-center gap-2"><BaseIcon name="calendar" class="w-4 h-4" /> Updated Today</div>
            </div>

            <div class="flex items-center gap-4">
              <button 
                v-if="course?.lessons?.length && !courseStore.isEnrolled(course.id)" 
                @click="showRegisterModal = true" 
                class="px-8 py-3.5 bg-accent hover:bg-accent-hover text-white font-bold rounded-2xl transition-all shadow-lg shadow-accent/20 flex items-center gap-2 text-lg"
              >
                Đăng ký khóa học <BaseIcon name="plus" class="w-4 h-4" />
              </button>
              
              <button 
                v-if="course?.lessons?.length && courseStore.isEnrolled(course.id)" 
                @click="startLesson(course.lessons[0])" 
                class="px-8 py-3.5 bg-accent hover:bg-accent-hover text-white font-bold rounded-2xl transition-all shadow-lg shadow-accent/20 flex items-center gap-2 text-lg"
              >
                Start Learning <BaseIcon name="play" class="w-4 h-4" />
              </button>
              <button @click="scrollToLessons" class="px-8 py-3.5 bg-bg-surface hover:bg-bg-hover text-white font-bold rounded-2xl border border-border-subtle transition-all flex items-center gap-2 text-lg">
                Course Content <BaseIcon name="arrow-down" class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- MAIN CONTENT -->
      <div class="container mx-auto px-4 py-12 max-w-4xl">
        
        <!-- Separator -->
        <div class="w-full h-px bg-gradient-to-r from-transparent via-white/20 to-transparent mb-12"></div>

        <div class="flex flex-col gap-12 w-full">
          
          <!-- UNLOCK PREMIUM WARNING -->
          <div
            v-if="course.isPremium && !authStore.currentUser?.isPremium"
            class="p-6 rounded-3xl border border-accent-yellow/30 bg-gradient-to-br from-accent-yellow/10 via-accent-yellow/5 to-transparent backdrop-blur flex flex-col md:flex-row items-center gap-6 justify-between"
          >
            <div class="flex items-start gap-4">
              <div class="text-3xl shrink-0"><BaseIcon name="crown" class="w-10 h-10 text-accent-yellow" /></div>
              <div>
                <h3 class="text-xl font-black text-accent-yellow uppercase tracking-wider mb-2">Unlock Premium</h3>
                <p class="text-text-secondary text-sm leading-relaxed">
                  Get full access to all interactive sandboxes, premium video tutorials, and certification quizzes.
                </p>
              </div>
            </div>
            <router-link
              to="/checkout"
              class="shrink-0 px-8 py-3 bg-gradient-to-r from-accent-yellow to-amber-400 hover:from-amber-400 hover:to-accent-yellow text-slate-950 font-bold rounded-2xl transition-all duration-300 shadow-lg shadow-accent-yellow/20"
            >
              Upgrade Now
            </router-link>
          </div>

          <!-- LEARNING OBJECTIVES -->
          <section class="p-8 rounded-3xl border border-border-subtle bg-bg-secondary/50">
            <h2 class="text-xs font-bold text-text-muted uppercase tracking-widest mb-6">Learning Objectives</h2>
            <ul class="space-y-4">
              <li v-for="(obj, i) in courseObjectives" :key="i" class="flex items-start gap-4">
                <div class="mt-1 w-5 h-5 rounded-full bg-accent-green/20 flex items-center justify-center shrink-0">
                  <BaseIcon name="check" class="w-3 h-3 text-accent-green" />
                </div>
                <p class="text-text-secondary leading-relaxed text-sm md:text-base">{{ obj }}</p>
              </li>
            </ul>
          </section>

          <!-- KEY OUTCOMES -->
          <section class="p-8 rounded-3xl border border-border-subtle bg-bg-secondary/50">
            <h2 class="text-xs font-bold text-text-muted uppercase tracking-widest mb-6">Key Outcomes</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div v-for="(outcome, i) in courseOutcomes" :key="i" class="p-6 rounded-2xl border border-border-subtle bg-bg-surface hover:bg-bg-tertiary transition-colors">
                <div class="w-10 h-10 rounded-xl bg-accent-purple/10 flex items-center justify-center mb-4">
                  <BaseIcon name="award" class="w-5 h-5 text-accent-purple" />
                </div>
                <h3 class="text-white font-bold mb-2">{{ outcome.title }}</h3>
                <p class="text-text-muted text-sm leading-relaxed">{{ outcome.desc }}</p>
              </div>
            </div>
          </section>

          <!-- COURSE CONTENT (LESSONS) -->
          <section id="course-lessons" class="pt-4">
            <h2 class="text-2xl font-bold text-white mb-8">Course Content</h2>
            
            <div class="flex flex-col gap-6">
              <div v-for="(module, mIdx) in groupedModules" :key="mIdx" class="module-group">
                <!-- Module Header -->
                <button 
                  @click="toggleModule(mIdx)"
                  class="w-full flex items-center justify-between p-6 rounded-2xl border border-border-subtle bg-bg-surface hover:bg-bg-secondary transition-colors text-left group"
                >
                  <div class="border-l-4 border-accent-purple pl-4">
                    <h3 class="text-xl font-bold text-white group-hover:text-accent-purple transition-colors">{{ module.title }}</h3>
                    <p v-if="module.description" class="text-sm text-text-muted mt-1">{{ module.description }}</p>
                  </div>
                  <div 
                    class="w-10 h-10 rounded-full bg-bg-main border border-border-subtle flex items-center justify-center shrink-0 transition-transform duration-300"
                    :class="expandedModules.includes(mIdx) ? 'rotate-180' : ''"
                  >
                    <BaseIcon name="chevron-down" class="w-5 h-5 text-text-muted group-hover:text-white transition-colors" />
                  </div>
                </button>
                
                <!-- Module Lessons -->
                <div v-show="expandedModules.includes(mIdx)" class="lessons-timeline flex flex-col gap-4 mt-4 ml-6 pl-6 border-l border-border-subtle">
                  <button
                    v-for="(lesson, idx) in module.lessons"
                    :key="lesson.id"
                    @click="startLesson(lesson)"
                    class="lesson-item w-full text-left p-5 rounded-2xl border transition-all duration-300 flex items-center justify-between gap-4 cursor-pointer group"
                    :class="lesson.status === 'Completed'
                      ? 'border-accent-green/20 bg-accent-green/5 hover:border-accent-green/40 hover:bg-accent-green/10'
                      : 'border-border-subtle bg-bg-secondary hover:border-border-default hover:bg-bg-surface'"
                  >
                    <div class="flex items-center gap-4">
                      <div class="flex-shrink-0 w-12 h-12 rounded-xl bg-bg-surface border border-border-subtle flex items-center justify-center font-bold text-sm shadow-inner transition-colors group-hover:border-accent/30">
                        <span v-if="lesson.status === 'Completed'" class="text-accent-green text-lg"><BaseIcon name="check" class="w-5 h-5" /></span>
                        <span v-else class="text-text-muted group-hover:text-white transition-colors">{{ idx + 1 }}</span>
                      </div>

                      <div>
                        <h3 class="text-base font-bold text-white leading-tight group-hover:text-accent transition-colors">{{ lesson.title }}</h3>
                        <div class="flex items-center gap-3 mt-2 text-xs text-text-muted">
                          <span class="flex items-center gap-1 font-semibold text-accent-yellow"><BaseIcon name="zap" class="w-3 h-3" /> +{{ lesson.xpReward }} XP</span>
                          <span v-if="lesson.sandboxType === 'codelab'" class="text-accent-yellow font-bold uppercase text-[9px] tracking-wider bg-accent-yellow/10 px-2 py-0.5 rounded border border-accent-yellow/20">
                            Assignment
                          </span>
                          <span v-else-if="lesson.sandboxType === 'quiz' || lesson.quizId" class="text-accent-purple font-bold uppercase text-[9px] tracking-wider bg-accent-purple/10 px-2 py-0.5 rounded border border-accent-purple/20">
                            Quiz Included
                          </span>
                          <span v-else-if="lesson.sandboxType" class="text-accent font-bold uppercase text-[9px] tracking-wider bg-accent/10 px-2 py-0.5 rounded border border-accent/20">
                            Interactive Sandbox
                          </span>
                        </div>
                      </div>
                    </div>
                  </button>
                </div>
              </div>
            </div>
          </section>

        </div>
      </div>
    </div>
  </div>

  <!-- Custom Registration Modal -->
  <div v-if="showRegisterModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
    <div class="bg-bg-surface border border-border-subtle rounded-2xl p-6 max-w-sm w-full shadow-2xl animate-fade-in-up">
      <div class="flex items-center gap-3 mb-4 text-accent-yellow">
        <BaseIcon name="alert-circle" class="w-6 h-6" />
        <h3 class="text-xl font-bold text-white">Xác nhận đăng ký</h3>
      </div>
      <p class="text-text-secondary mb-6 leading-relaxed">
        Bạn có chắc muốn đăng ký khóa học <strong class="text-white">{{ course?.title }}</strong> này không?
      </p>
      <div class="flex gap-3 justify-end">
        <button 
          @click="showRegisterModal = false"
          class="px-5 py-2.5 rounded-xl font-semibold text-text-muted hover:text-white hover:bg-bg-hover transition-colors"
        >
          Hủy bỏ
        </button>
        <button 
          @click="confirmRegistration"
          class="px-5 py-2.5 rounded-xl font-semibold bg-accent hover:bg-accent-hover text-white shadow-lg shadow-accent/20 transition-all flex items-center gap-2"
        >
          Đồng ý
          <BaseIcon name="check" class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import { useCourseStore } from '../../features/courses/store/useCourseStore';
import { courseApi } from '../../services/courseApi';
import CourseCover from '../../features/courses/components/CourseCover.vue';
import { COURSES } from '../../data/courses';

interface LessonDto {
  id: string;
  title: string;
  moduleTitle?: string;
  moduleDescription?: string;
  contentMd: string;
  sandboxType: string;
  sandboxConfig: string;
  quizId: string | null;
  xpReward: number;
  orderIndex: number;
  status: 'NotStarted' | 'InProgress' | 'Completed';
}

interface CourseDetailDto {
  id: string;
  title: string;
  description: string;
  category: string;
  difficulty: string;
  isPremium: boolean;
  coverImage: string;
  isPublished: boolean;
  lessons: LessonDto[];
}

const authStore = useAuthStore();
const courseStore = useCourseStore();
const route = useRoute();
const router = useRouter();
const showRegisterModal = ref(false);

const loading = ref(true);
const error = ref<string | null>(null);
const course = ref<CourseDetailDto | null>(null);

const expandedModules = ref<number[]>([]); // Empty by default so all are collapsed

const toggleModule = (mIdx: number) => {
  if (expandedModules.value.includes(mIdx)) {
    expandedModules.value = expandedModules.value.filter(i => i !== mIdx);
  } else {
    expandedModules.value.push(mIdx);
  }
};

const groupedModules = computed(() => {
  if (!course.value?.lessons) return [];
  const map = new Map<string, { title: string, description: string, lessons: LessonDto[] }>();
  
  course.value.lessons.forEach(l => {
    const mTitle = l.moduleTitle || 'General';
    const mDesc = l.moduleDescription || '';
    if (!map.has(mTitle)) {
      map.set(mTitle, { title: mTitle, description: mDesc, lessons: [] });
    }
    map.get(mTitle)!.lessons.push(l);
  });
  
  return Array.from(map.values());
});

const totalXp = computed(() => {
  return course.value?.lessons.reduce((acc, l) => acc + l.xpReward, 0) ?? 0;
});

const courseObjectives = computed(() => {
  if (course.value?.title.includes('Data Structure')) {
    return [
      "Master core data structures: Arrays, Linked Lists, Trees, Graphs",
      "Understand time and space complexity for diverse operations",
      "Build scalable systems using appropriate data structures",
      "Prepare for FAANG-level technical interviews with structured thinking"
    ];
  }
  return [
    "Nắm vững các khái niệm cơ bản và nâng cao của khóa học",
    "Thực hành qua các bài tập và sandbox trực quan sinh động",
    "Tối ưu hóa thuật toán và cấu trúc dữ liệu cho hiệu năng cao",
    "Sẵn sàng vượt qua các buổi phỏng vấn kỹ thuật hóc búa"
  ];
});

const courseOutcomes = computed(() => {
  if (course.value?.title.includes('Data Structure')) {
    return [
      { title: "Ace Technical Interviews", desc: "Navigate complex algorithmic problems with confidence and structured thinking." },
      { title: "Architect Scalable Systems", desc: "Design and implement systems that meet real-world demands using right data structures." }
    ];
  }
  return [
    { title: "Vượt qua phỏng vấn", desc: "Tự tin giải quyết các bài toán hóc búa từ nhà tuyển dụng." },
    { title: "Thiết kế hệ thống", desc: "Áp dụng kiến thức để xây dựng phần mềm thực tế, tối ưu hiệu năng." }
  ];
});

function scrollToLessons() {
  document.getElementById('course-lessons')?.scrollIntoView({ behavior: 'smooth' });
}

const categoryMap: Record<string, string> = {
  DataStructure: 'Cấu trúc dữ liệu',
  Algorithm: 'Giải thuật',
  OOP: 'Hướng đối tượng (OOP)',
  SystemDesign: 'Thiết kế hệ thống'
};

const difficultyMap: Record<string, string> = {
  Beginner: 'Cơ bản',
  Intermediate: 'Trung cấp',
  Advanced: 'Nâng cao'
};

function getCategoryLabel(val: string) { return categoryMap[val] || val; }
function getDifficultyLabel(val: string) { return difficultyMap[val] || val; }

async function loadCourseDetail() {
  loading.value = true;
  error.value = null;
  const courseId = route.params.id as string;

  try {
    const data = await courseApi.getCourseById(courseId);
    course.value = {
      ...data,
      coverImage: data.coverImageUrl ?? data.coverImage,
    } as unknown as CourseDetailDto;
    // Store will handle the enrollment check automatically now
  } catch (err) {
    console.error('Failed to load course detail:', err);
    const localCourse = COURSES.find(c => c.id === courseId);
    if (localCourse) {
      course.value = localCourse as unknown as CourseDetailDto;
    } else {
      error.value = 'Không tìm thấy khóa học này (Lỗi kết nối máy chủ).';
    }
  } finally {
    loading.value = false;
  }
}

function confirmRegistration() {
  showRegisterModal.value = false;
  if (course.value) {
    courseStore.enrollCourse(course.value.id);
  }
}

function startLesson(lesson: LessonDto) {
  // Chặn mọi user chưa đăng nhập hoặc chưa có Premium khi khóa học là premium
  // (trước đây chỉ check role==='Student' → user vô danh đi thẳng vào).
  const hasPremium = authStore.currentUser?.isPremium === true;
  if (course.value?.isPremium && !hasPremium) {
    router.push({ name: 'checkout' });
    return;
  }
  router.push({ name: 'lesson-study', params: { id: lesson.id }, query: { courseId: course.value?.id } });
}

onMounted(() => {
  loadCourseDetail();
});

// Chuyển course A→B (cùng component, chỉ đổi param) phải load lại — trước đây hiển thị dữ liệu cũ.
watch(
  () => route.params.id,
  () => loadCourseDetail(),
);
</script>

<style scoped>
@import "./CourseDetailView.css";
</style>
