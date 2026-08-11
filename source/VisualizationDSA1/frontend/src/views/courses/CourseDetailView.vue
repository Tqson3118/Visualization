<template>
  <div class="course-detail-view h-full w-full relative animate-fade-in text-text-primary pb-0">
    
    <!-- Hero Banner Background -->
    <div class="absolute top-0 left-0 w-full h-[40vh] z-0 overflow-hidden" :class="getCategoryGradient(course?.category || '')">
      <div class="absolute inset-0 flex items-center justify-center opacity-30">
        <BaseIcon :name="getCategoryIcon(course?.category || '')" class="w-48 h-48" />
      </div>
      <div class="absolute inset-0 bg-gradient-to-b from-transparent via-bg-primary/80 to-bg-primary"></div>
      <!-- Grid pattern -->
      <div class="absolute inset-0" style="background-image: linear-gradient(rgba(255, 255, 255, 0.05) 1px, transparent 1px), linear-gradient(90deg, rgba(255, 255, 255, 0.05) 1px, transparent 1px); background-size: 30px 30px; mask-image: linear-gradient(to bottom, white 30%, transparent 90%);"></div>
    </div>

    <!-- Main Content Container -->
    <div class="container mx-auto px-6 py-6 lg:py-12 max-w-5xl relative z-10 flex flex-col h-full">
      
      <!-- Top Navigation -->
      <router-link to="/courses" class="inline-flex items-center gap-2 text-sm font-bold text-accent hover:text-accent transition-colors mb-8 bg-bg-surface px-4 py-2 rounded-xl backdrop-blur-md border border-border-accent">
        <BaseIcon name="chevron-left" class="w-4 h-4" /> Quay lại bản đồ Lộ trình
      </router-link>

      <!-- Loading State -->
      <div v-if="loading" class="text-center py-32">
        <div class="inline-block w-12 h-12 border-4 border-border-accent border-t-indigo-500 rounded-full animate-spin"></div>
        <p class="text-text-secondary mt-6 font-medium">Đang thiết lập Roadmap...</p>
      </div>
      <div v-else-if="error" class="text-center py-32 glass-panel rounded-3xl">
        <BaseIcon name="warning" class="w-14 h-14 mx-auto mb-6 text-accent-warm" />
        <h3 class="text-2xl font-bold text-text-primary">{{ error }}</h3>
        <p class="text-text-muted mt-3">Vui lòng thử lại sau hoặc liên hệ hỗ trợ.</p>
      </div>

      <!-- Course Content -->
      <div v-else-if="course" class="grid grid-cols-1 lg:grid-cols-12 gap-10 h-full">
        
        <!-- Left Column: Info & Timeline (Takes 8 columns on large screens) -->
        <div class="lg:col-span-8 flex flex-col gap-8 h-full overflow-y-auto custom-scrollbar pr-2 md:pr-4 pb-32">
          
          <!-- Hero Info -->
          <section class="flex flex-col gap-4">
            <div class="flex items-center gap-3">
              <span class="px-3 py-1 rounded-lg text-xs font-black uppercase tracking-wider bg-accent text-text-primary shadow-[0_0_15px_rgba(99,102,241,0.5)] border border-border-accent">
                {{ getCategoryLabel(course.category) }}
              </span>
              <span class="px-3 py-1 rounded-lg text-xs font-black uppercase tracking-wider bg-bg-hover text-text-secondary border border-border-default">
                {{ getDifficultyLabel(course.difficulty) }}
              </span>
            </div>

            <h1 class="text-4xl md:text-5xl font-black text-transparent bg-clip-text bg-gradient-to-r from-accent-light via-bg-surface to-accent-purple mt-2 leading-tight">
              {{ course.title }}
            </h1>
            
            <p class="text-text-secondary text-lg leading-relaxed mt-2 opacity-90">
              {{ course.description }}
            </p>
          </section>

          <!-- Header removed per redesign plan -->
        <section class="p-8 rounded-3xl glass-panel">
          <h2 class="text-2xl font-bold text-text-primary mb-6">Nội dung bài học</h2>

          <div class="lessons-timeline relative flex flex-col gap-6 ml-4 md:ml-6 pl-6 md:pl-8 border-l-2 border-border-default">
            <div
              v-for="(lesson, idx) in course.lessons"
              :key="lesson.id"
              class="lesson-item relative transition-all duration-300 animate-fade-in group w-full"
              :style="{ animationDelay: `${idx * 0.1}s` }"
            >
              <!-- Timeline Dot -->
              <div class="absolute -left-[33px] md:-left-[41px] top-6 w-4 h-4 rounded-full border-4 border-border-default z-20 shadow-[0_0_10px_rgba(0,0,0,0.5)] transition-all duration-300 group-hover:scale-125"
                :class="[
                  getLessonState(lesson, idx, course) === 'completed' ? 'bg-accent-green shadow-emerald-500/50' : '',
                  getLessonState(lesson, idx, course) === 'active' ? 'bg-accent-warm shadow-amber-400/50 animate-pulse' : '',
                  getLessonState(lesson, idx, course) === 'locked_sequence' ? 'bg-bg-hover border-border-default opacity-50' : '',
                  getLessonState(lesson, idx, course) === 'available' ? 'bg-bg-hover' : ''
                ]"
              ></div>

              <!-- Lesson Card -->
              <div 
                class="group relative flex flex-col md:flex-row items-start md:items-center p-6 md:p-8 rounded-[2rem] border transition-all duration-500 cursor-pointer overflow-hidden backdrop-blur-md"
                :class="[
                  getLessonState(lesson, idx, course) === 'completed' ? 'bg-accent-green/10 border-accent-green/20 hover:border-accent-green/40 hover:bg-accent-green/20' : '',
                  getLessonState(lesson, idx, course) === 'active' ? 'bg-accent/10 border-border-accent shadow-[0_0_30px_rgba(99,102,241,0.15)] hover:bg-accent/20' : '',
                  getLessonState(lesson, idx, course) === 'available' ? 'bg-bg-surface border-border-default/50 hover:bg-bg-surface hover:border-border-default' : '',
                  getLessonState(lesson, idx, course) === 'locked_sequence' ? 'bg-bg-secondary/20 border-border-default opacity-60' : ''
                ]"
                @click="getLessonState(lesson, idx, course) !== 'locked_sequence' && startLesson(lesson, idx, course)"
              >
                  <!-- Glassmorphism shine effect -->
                  <div v-if="getLessonState(lesson, idx, course) !== 'locked_sequence'" class="absolute top-0 left-0 w-full h-full bg-gradient-to-b from-bg-surface/5 to-transparent opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none"></div>

                  <div class="flex-1 z-10">
                    <div class="flex items-center gap-3 mb-2">
                      <span class="text-xs font-black text-text-muted tracking-wider">TRẠM {{ idx + 1 }}</span>
                      <span v-if="lesson.sandboxType" class="px-2 py-0.5 rounded text-[9px] font-black uppercase tracking-widest bg-accent/20 text-accent border border-border-accent">
                        {{ lesson.sandboxType }}
                      </span>
                      <span v-if="lesson.quizId" class="px-2 py-0.5 rounded text-[9px] font-black uppercase tracking-widest bg-accent-purple/20 text-accent-purple border border-accent-purple/30">
                        Quiz
                      </span>
                    </div>
                    <h3 class="text-lg font-bold text-text-primary transition-colors"
                        :class="getLessonState(lesson, idx, course) !== 'locked_sequence' ? 'group-hover:text-accent-light' : 'text-text-secondary'">
                      <BaseIcon v-if="getLessonState(lesson, idx, course) === 'locked_sequence'" name="lock" class="w-4 h-4 inline-block mr-2 align-text-bottom text-accent-warm" />
                      {{ lesson.title }}
                    </h3>
                  </div>

                  <div class="flex items-center justify-between md:justify-end gap-4 z-10 w-full md:w-auto mt-2 md:mt-0 pt-4 md:pt-0 border-t md:border-none border-border-default">
                    <div class="flex flex-col md:items-end">
                      <span class="text-[10px] text-text-secondary font-medium uppercase tracking-wider">Phần thưởng</span>
                      <span class="text-accent-green font-bold text-sm"><BaseIcon name="zap" class="w-3.5 h-3.5 inline-block mr-0.5 align-text-bottom" /> +{{ lesson.xpReward }} XP</span>
                    </div>
                    
                    <button
                      class="px-5 py-2.5 rounded-xl text-sm font-bold transition-all duration-300 flex items-center gap-2"
                      :class="[
                        getLessonState(lesson, idx, course) === 'completed' ? 'bg-accent-green/10 text-accent-green border border-accent-green/20 hover:bg-accent-green/20 shadow-lg hover:shadow-emerald-500/20' : '',
                        getLessonState(lesson, idx, course) === 'active' ? 'bg-accent text-text-primary hover:bg-accent shadow-lg shadow-indigo-600/30' : '',
                        getLessonState(lesson, idx, course) === 'available' ? 'bg-bg-hover text-text-secondary hover:bg-bg-hover shadow-lg' : '',
                        getLessonState(lesson, idx, course) === 'locked_sequence' ? 'bg-bg-secondary text-text-muted cursor-not-allowed border border-border-default' : ''
                      ]"
                      :disabled="getLessonState(lesson, idx, course) === 'locked_sequence'"
                      @click.stop="startLesson(lesson, idx, course)"
                    >
                      {{ getLessonState(lesson, idx, course) === 'completed' ? 'Học lại' : 
                         getLessonState(lesson, idx, course) === 'active' ? 'Tiếp tục' : 
                         getLessonState(lesson, idx, course) === 'locked_sequence' ? 'Chưa mở khóa' : 'Bắt đầu' }}
                      
                      <svg v-if="!['completed', 'locked_sequence'].includes(getLessonState(lesson, idx, course))" xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                        <path fill-rule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clip-rule="evenodd" />
                      </svg>
                      <svg v-if="getLessonState(lesson, idx, course) === 'locked_sequence'" xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                      </svg>
                    </button>
                  </div>
              </div>
            </div>
          </div>
        </section>

        </div>

        <!-- Right Column: Sidebar (Takes 4 columns) -->
        <div class="lg:col-span-4 h-full overflow-y-auto custom-scrollbar pb-32 hidden lg:block">
          <!-- Sticky Wrapper -->
          <div class="flex flex-col gap-6 pt-0">
            
        <!-- Stats Card -->
            <div class="p-6 rounded-3xl glass-panel flex flex-col gap-5">
              <h3 class="text-sm font-bold text-text-secondary uppercase tracking-wider mb-2 border-b border-border-default pb-4">
                Tổng quan Lộ trình
              </h3>
              
              <div class="flex items-center gap-4">
                <div class="w-10 h-10 rounded-xl bg-accent/10 border border-border-accent flex items-center justify-center">
                  <BaseIcon name="book" class="w-5 h-5 text-accent" />
                </div>
                <div>
                  <div class="text-[10px] font-bold text-text-muted uppercase tracking-wider">Số chặng (Nodes)</div>
                  <div class="text-lg font-black text-text-primary">{{ course.lessons.length }} Trạm</div>
                </div>
              </div>

              <div class="flex items-center gap-4">
                <div class="w-10 h-10 rounded-xl bg-accent-green/10 border border-accent-green/20 flex items-center justify-center">
                  <BaseIcon name="zap" class="w-5 h-5 text-accent-green" />
                </div>
                <div>
                  <div class="text-[10px] font-bold text-text-muted uppercase tracking-wider">Tổng phần thưởng</div>
                  <div class="text-lg font-black text-accent-green">{{ totalXp }} XP</div>
                </div>
              </div>

              <div class="flex items-center gap-4">
                <div class="w-10 h-10 rounded-xl bg-accent-purple/10 border border-accent-purple/20 flex items-center justify-center">
                  <BaseIcon name="target" class="w-5 h-5 text-accent-purple" />
                </div>
                <div>
                  <div class="text-[10px] font-bold text-text-muted uppercase tracking-wider">Tiến độ của bạn</div>
                  <div class="text-lg font-black text-text-primary">
                    {{ course.lessons.filter(l => l.status === 'Completed').length }} / {{ course.lessons.length }}
                  </div>
                </div>
              </div>

              <!-- G3.2 Enrollment -->
              <div class="mt-2">
                <button
                  v-if="enrollmentStatus === 'not-enrolled'"
                  @click="enroll"
                  class="w-full px-4 py-3 rounded-xl bg-accent text-white font-bold text-sm hover:bg-accent-light transition-all flex items-center justify-center gap-2"
                  :disabled="enrolling"
                >
                  <BaseIcon name="plus" class="w-4 h-4" />
                  {{ enrolling ? 'Đang đăng ký...' : 'Đăng ký Lộ trình' }}
                </button>

                <div v-else-if="enrollmentStatus === 'enrolled'" class="space-y-2">
                  <div class="flex items-center justify-between text-xs">
                    <span class="font-bold text-accent-green flex items-center gap-1">
                      <BaseIcon name="check" class="w-3.5 h-3.5" /> Đang học
                    </span>
                    <span class="text-text-muted font-mono">{{ enrollmentProgress }}%</span>
                  </div>
                  <div class="w-full h-2 bg-bg-hover rounded-full overflow-hidden border border-border-default">
                    <div class="h-full bg-gradient-to-r from-accent to-accent-green rounded-full transition-all" :style="{ width: enrollmentProgress + '%' }"></div>
                  </div>
                  <button
                    @click="dropEnrollment"
                    class="w-full px-4 py-2 rounded-xl text-xs font-semibold text-accent-red border border-accent-red/30 hover:bg-accent-red/10 transition-all"
                  >
                    Rời bỏ Lộ trình
                  </button>
                </div>

                <div v-else-if="enrollmentStatus === 'blocked'" class="text-xs text-accent-red bg-accent-red/10 border border-accent-red/20 rounded-xl p-3">
                  {{ enrollmentError || 'Bạn đã đạt tối đa 3 roadmap đang học.' }}
                </div>
              </div>

              <!-- G3.9 Roadmap Stats + Review (UC-50/51) -->
              <div class="border-t border-border-default pt-4 mt-2 space-y-4">
                <div class="flex items-center gap-4">
                  <div class="w-10 h-10 rounded-xl bg-accent-warm/10 border border-accent-warm/20 flex items-center justify-center">
                    <BaseIcon name="star" class="w-5 h-5 text-accent-warm" />
                  </div>
                  <div class="flex-1">
                    <div class="text-[10px] font-bold text-text-muted uppercase tracking-wider">Đánh giá</div>
                    <StarRatingDisplay
                      v-if="roadmapStats?.avgRating != null"
                      :value="roadmapStats.avgRating"
                      :count="roadmapStats.reviewCount"
                      show-value
                    />
                    <div v-else class="text-xs text-text-muted">Chưa có đánh giá</div>
                  </div>
                </div>

                <div class="flex items-center gap-4">
                  <div class="w-10 h-10 rounded-xl bg-accent-blue/10 border border-accent-blue/20 flex items-center justify-center">
                    <BaseIcon name="users" class="w-5 h-5 text-accent-blue" />
                  </div>
                  <div>
                    <div class="text-[10px] font-bold text-text-muted uppercase tracking-wider">Lượt học</div>
                    <div class="text-lg font-black text-text-primary">
                      {{ roadmapStats?.enrollCount ?? 0 }} học · {{ roadmapStats?.completionCount ?? 0 }} hoàn thành
                    </div>
                  </div>
                </div>

                <div v-if="authStore.isAuthenticated">
                  <div v-if="roadmapStats?.myRating != null" class="text-xs text-accent-green bg-accent-green/10 border border-accent-green/20 rounded-xl p-3 flex items-center gap-2">
                    <BaseIcon name="check-circle" class="w-4 h-4 shrink-0" />
                    <span>Bạn đã đánh giá <StarRatingDisplay :value="roadmapStats.myRating" /></span>
                  </div>
                  <button
                    v-else-if="roadmapStats?.myCanReview"
                    @click="showReviewModal = true"
                    class="w-full px-4 py-3 rounded-xl bg-accent-warm text-white font-bold text-sm hover:bg-accent-warm/90 transition-all flex items-center justify-center gap-2"
                  >
                    <BaseIcon name="star" class="w-4 h-4 fill-current" />
                    Đánh giá roadmap
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <RoadmapReviewModal
    :show="showReviewModal"
    :roadmap-id="course?.id ?? ''"
    @close="showReviewModal = false"
    @submitted="onReviewSubmitted"
  />

</template>
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import { teacherStudioService, type CustomRoadmapDto } from '@/services/TeacherStudioService';
import { sessionApi, OutOfHeartsError } from '@/features/gamification-engine/service/sessionApi';
import { useSessionStore } from '@/features/gamification-engine/store/useSessionStore';
import { roadmapApi, type RoadmapStatsDto } from '@/services/roadmapApi';
import { API_BASE_URL } from '@/services/apiConfig';
import StarRatingDisplay from '@/components/rating/StarRatingDisplay.vue';
import RoadmapReviewModal from '@/components/rating/RoadmapReviewModal.vue';

interface LessonDto {
  id: string;
  title: string;
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
  coverImageUrl: string;
  isPublished: boolean;
  lessons: LessonDto[];
}

const authStore = useAuthStore();
const sessionStore = useSessionStore();
const route = useRoute();
const router = useRouter();
const BASE_URL = API_BASE_URL;

const loading = ref(true);
const error = ref<string | null>(null);
const course = ref<CourseDetailDto | null>(null);

// G3.9 — Roadmap stats + review (UC-50/51)
const roadmapStats = ref<RoadmapStatsDto | null>(null);
const showReviewModal = ref(false);

// G3.2 — Enrollment (UC-07)
const enrollmentStatus = ref<'idle' | 'loading' | 'enrolled' | 'not-enrolled' | 'blocked'>('idle');
const enrollmentProgress = ref(0);
const enrollmentError = ref('');
const enrolling = ref(false);

const totalXp = computed(() => {
  return course.value?.lessons.reduce((acc, l) => acc + l.xpReward, 0) ?? 0;
});

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

function getCategoryGradient(category: string) {
  const map: Record<string, string> = {
    DataStructure: 'bg-gradient-to-br from-accent to-blue-800',
    Algorithm: 'bg-gradient-to-br from-emerald-600 to-teal-800',
    OOP: 'bg-gradient-to-br from-accent-purple to-violet-800',
    SystemDesign: 'bg-gradient-to-br from-accent-warm to-red-800'
  };
  return map[category] || 'bg-gradient-to-br from-slate-600 to-slate-800';
}

function getCategoryIcon(category: string) {
  const map: Record<string, string> = {
    DataStructure: 'link',
    Algorithm: 'zap',
    OOP: 'oop',
    SystemDesign: 'system-architect'
  };
  return map[category] || 'book';
}

function getLessonState(lesson: LessonDto, idx: number, courseRef: CourseDetailDto | null) {
  // NOTE: Không có premium lock — mọi lesson đều miễn phí truy cập.
  // Gate duy nhất là hệ thống Tim (❤️) do server kiểm tra khi bắt đầu bài.
  
  if (lesson.status === 'Completed') return 'completed';
  if (lesson.status === 'InProgress') return 'active';
  
  // Sequential locking: NotStarted lessons are locked unless they are the first or the previous one is completed.
  if (lesson.status === 'NotStarted') {
    if (idx === 0) return 'active';
    const prevLesson = courseRef?.lessons[idx - 1];
    if (prevLesson?.status === 'Completed') return 'active';
    return 'locked_sequence';
  }
  
  return 'available';
}



function nodeXpReward(difficulty: string): number {
  const map: Record<string, number> = { Easy: 30, Medium: 50, Hard: 80 };
  return map[difficulty] || 50;
}

function mapRoadmapToCourseDetail(roadmap: CustomRoadmapDto): CourseDetailDto {
  const visibleNodes = roadmap.nodes.filter(n => !n.isHidden);
  const lessons: LessonDto[] = visibleNodes.map((node, idx) => ({
    id: node.id,
    title: node.name,
    contentMd: node.description,
    sandboxType: '',
    sandboxConfig: node.contentJson ?? '[]',
    quizId: node.quizId ?? null,
    xpReward: nodeXpReward(node.difficulty),
    orderIndex: node.sortOrder ?? idx + 1,
    status: node.isComplete ? 'Completed' : 'NotStarted',
  }));
  return {
    id: roadmap.id,
    title: roadmap.name,
    description: roadmap.description,
    category: inferCategory(roadmap),
    difficulty: inferDifficulty(roadmap.nodes),
    isPremium: false,
    coverImageUrl: roadmap.thumbnailUrl ?? '',
    isPublished: true,
    lessons,
  };
}

function parseTags(tags: string): string[] {
  try {
    const parsed: unknown = JSON.parse(tags || '[]');
    if (Array.isArray(parsed)) return parsed.filter((t): t is string => typeof t === 'string');
    if (typeof parsed === 'string' && parsed) return [parsed];
    return [];
  } catch {
    return tags ? tags.split(',').map(t => t.trim()).filter(Boolean) : [];
  }
}

function inferCategory(roadmap: CustomRoadmapDto): string {
  const tags = parseTags(roadmap.tags);
  const all = [roadmap.name, roadmap.description, ...tags].join(' ').toLowerCase();
  if (all.includes('oop') || all.includes('solid') || all.includes('đối tượng')) return 'OOP';
  if (all.includes('graph') || all.includes('đồ thị') || all.includes('dijkstra') || all.includes('bfs')) return 'DataStructure';
  if (all.includes('system') || all.includes('thiết kế hệ thống')) return 'SystemDesign';
  return 'Algorithm';
}

function inferDifficulty(nodes: CustomRoadmapDto['nodes']): string {
  const weights: Record<string, number> = { Easy: 1, Medium: 2, Hard: 3 };
  if (!nodes.length) return 'Beginner';
  const avg = nodes.reduce((acc, n) => acc + (weights[n.difficulty] ?? 2), 0) / nodes.length;
  if (avg >= 2.4) return 'Advanced';
  if (avg >= 1.6) return 'Intermediate';
  return 'Beginner';
}

async function loadCourseDetail() {
  loading.value = true;
  error.value = null;
  const courseId = route.params.id as string;

  try {
    const roadmap = await teacherStudioService.getRoadmap(courseId);
    course.value = mapRoadmapToCourseDetail(roadmap);
    await loadRoadmapStats();
  } catch (err) {
    console.error('Failed to load roadmap detail:', err);
    error.value = 'Không thể tải thông tin lộ trình.';
  } finally {
    loading.value = false;
  }
}

async function loadRoadmapStats(): Promise<void> {
  try {
    roadmapStats.value = await roadmapApi.getStats(route.params.id as string);
  } catch (err) {
    console.warn('Failed to load roadmap stats:', err);
    roadmapStats.value = null;
  }
}

function onReviewSubmitted(_rating: number): void {
  loadRoadmapStats();
}


async function startLesson(lesson: LessonDto, idx: number, courseRef: CourseDetailDto | null) {
  const state = getLessonState(lesson, idx, courseRef);
  if (state === 'locked_sequence') {
    // Bài bị khóa vì cần hoàn thành bài trước (sequential) — không cho vào
    return;
  }
  if (!authStore.isAuthenticated) {
    router.push('/login');
    return;
  }
  try {
    await sessionApi.enterNode(lesson.id);
    router.push({
      name: 'lesson-study',
      params: { id: lesson.id },
      query: { courseId: route.params.id as string },
    });
  } catch (err) {
    if (err instanceof OutOfHeartsError) {
      sessionStore.showOutOfHearts(err.recoveryInfo);
      return;
    }
    const message = err instanceof Error ? err.message : 'Không thể mở bài học.';
    enrollmentError.value = message === 'OUT_OF_HEARTS' ? 'Bạn đã hết tim. Hãy chờ hồi tim hoặc nâng cấp Premium.' : message;
    enrollmentStatus.value = 'blocked';
  }
}

onMounted(() => {
  loadCourseDetail();
  loadEnrollmentStatus();
});

// ── Enrollment (G3.2.3) ──
async function loadEnrollmentStatus(): Promise<void> {
  if (!authStore.isAuthenticated) {
    enrollmentStatus.value = 'not-enrolled';
    return;
  }
  try {
    const res = await fetch(`${BASE_URL}/api/v1/enrollments/my`, {
      headers: { Authorization: `Bearer ${authStore.getAccessToken()}` },
    });
    if (res.ok) {
      const data = await res.json();
      const courseId = route.params.id as string;
      const match = (data.items ?? []).find((e: any) => e.roadmapId === courseId);
      if (match) {
        enrollmentStatus.value = 'enrolled';
        enrollmentProgress.value = match.progressPercent ?? 0;
      } else {
        enrollmentStatus.value = 'not-enrolled';
      }
    }
  } catch (err) {
    console.warn('Failed to load enrollment status:', err);
  }
}

async function enroll(): Promise<void> {
  if (!authStore.isAuthenticated) {
    router.push('/login');
    return;
  }
  enrolling.value = true;
  enrollmentError.value = '';
  try {
    const res = await fetch(`${BASE_URL}/api/v1/enrollments`, {
      method: 'POST',
      headers: {
        Authorization: `Bearer ${authStore.getAccessToken()}`,
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ roadmapId: route.params.id as string }),
    });
    if (res.ok) {
      enrollmentStatus.value = 'enrolled';
    } else {
      const errData = await res.json();
      enrollmentStatus.value = errData.error === 'MAX_ENROLLMENTS' ? 'blocked' : 'not-enrolled';
      enrollmentError.value = errData.message ?? 'Không thể đăng ký lộ trình.';
    }
  } catch (err) {
    enrollmentStatus.value = 'not-enrolled';
    enrollmentError.value = 'Không thể kết nối máy chủ.';
  } finally {
    enrolling.value = false;
  }
}

async function dropEnrollment(): Promise<void> {
  try {
    const courseId = route.params.id as string;
    const res = await fetch(`${BASE_URL}/api/v1/enrollments/my`, {
      headers: { Authorization: `Bearer ${authStore.getAccessToken()}` },
    });
    if (res.ok) {
      const data = await res.json();
      const match = (data.items ?? []).find((e: any) => e.roadmapId === courseId);
      if (match) {
        await fetch(`${BASE_URL}/api/v1/enrollments/${match.id}`, {
          method: 'DELETE',
          headers: { Authorization: `Bearer ${authStore.getAccessToken()}` },
        });
      }
    }
    enrollmentStatus.value = 'not-enrolled';
    enrollmentProgress.value = 0;
  } catch (err) {
    console.warn('Failed to drop enrollment:', err);
  }
}
</script>

<style scoped>
@import "./CourseDetailView.css";
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.2);
}

.animate-fade-in {
  animation: fadeIn 0.5s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(15px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
