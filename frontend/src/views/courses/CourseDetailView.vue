<template>
  <div class="course-detail-view w-full animate-fade-in bg-vdsa-bg min-h-[calc(100vh-var(--app-header-h,68px))]" v-bind="$attrs">

    <div v-if="loading" class="text-center py-32">
      <div class="inline-block w-8 h-8 border-4 border-accent/20 border-t-indigo-500 rounded-full animate-spin"></div>
      <p class="text-vdsa-muted mt-4">Đang tải thông tin lộ trình...</p>
    </div>

    <div v-else-if="error" class="text-center py-32 container mx-auto max-w-2xl">
      <div class="text-5xl mb-4"><BaseIcon name="warning" class="w-14 h-14 text-vdsa-red mx-auto" /></div>
      <h3 class="text-xl font-bold text-vdsa-secondary">{{ error }}</h3>
      <p class="text-vdsa-muted mt-2">Vui lòng thử lại sau hoặc liên hệ hỗ trợ.</p>
      <router-link :to="{ name: 'path-list' }" class="mt-6 inline-block px-6 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl text-vdsa-secondary hover:text-white transition">Quay lại</router-link>
    </div>

    <div v-else-if="course">
      <!-- ── HEADER (kiểu Educative: breadcrumb + title/stats/buttons căn giữa) ── -->
      <div class="container mx-auto max-w-5xl px-4 pt-10 pb-12">
        <!-- Breadcrumb: Lớp học hoặc Lộ trình / <tên khóa> -->
        <nav class="flex items-center gap-2 text-sm text-vdsa-muted mb-6 justify-center" aria-label="Breadcrumb">
          <router-link v-if="fromClassId" :to="`/classes/${fromClassId}`" class="font-semibold hover:text-white transition-colors">Lớp học</router-link>
          <router-link v-else :to="{ name: 'path-list' }" class="font-semibold hover:text-white transition-colors">Lộ trình</router-link>
          <BaseIcon name="chevron-right" class="w-3.5 h-3.5 text-vdsa-disabled" />
          <span class="text-vdsa-secondary font-medium truncate">{{ course.title }}</span>
        </nav>

        <div class="flex flex-col items-center text-center">
          <h1 class="text-3xl md:text-5xl font-extrabold text-white leading-tight tracking-tight max-w-4xl">
            {{ course.title }}
          </h1>

          <p class="text-base md:text-lg text-vdsa-secondary leading-relaxed mt-5 max-w-3xl">
            {{ course.description }}
          </p>

          <!-- Stats: lessons/quiz/lab/XP thật -->
          <div class="flex flex-wrap items-center justify-center gap-x-6 gap-y-2 mt-6 text-sm text-vdsa-muted font-medium">
            <span class="flex items-center gap-1.5 whitespace-nowrap shrink-0"><BaseIcon name="book-open" class="w-4 h-4 text-vdsa-purple-light" /> {{ course.lessons.length }} Bài học</span>
            <span v-if="quizCount > 0" class="flex items-center gap-1.5 whitespace-nowrap shrink-0"><BaseIcon name="quiz" class="w-4 h-4 text-vdsa-purple-light" /> {{ quizCount }} Quiz</span>
            <span v-if="labCount > 0" class="flex items-center gap-1.5 whitespace-nowrap shrink-0"><BaseIcon name="code-ide" class="w-4 h-4 text-vdsa-purple-light" /> {{ labCount }} Bài tập</span>
            <span class="flex items-center gap-1.5 whitespace-nowrap shrink-0"><BaseIcon name="zap" class="w-4 h-4 text-vdsa-purple-light" /> {{ xpTotal }} XP</span>
          </div>

          <!-- Actions: Start Learning / Course Content cạnh nhau, căn giữa -->
          <div class="flex flex-wrap items-center justify-center gap-3 mt-9">
            <button
              v-if="course?.lessons?.length && !courseStore.isEnrolled(course.id)"
              @click="showRegisterModal = true"
              class="px-8 py-3.5 bg-vdsa-accent hover:bg-vdsa-accent-light text-white font-bold rounded-xl transition-all duration-200 shadow-lg shadow-vdsa-accent hover:shadow-[0_0_32px_rgba(168,85,247,0.6)] hover:scale-[1.04] hover:ring-2 hover:ring-vdsa-accent-light/60 flex items-center justify-center gap-2 text-base cursor-pointer whitespace-nowrap shrink-0"
            >
              <span>{{ messages.courses.enrollWithHeart(1) }}</span> <BaseIcon name="plus" class="w-4 h-4" />
            </button>

            <button
              v-if="course?.lessons?.length && courseStore.isEnrolled(course.id)"
              @click="startLesson(nextStudyLesson!)"
              class="px-8 py-3.5 bg-vdsa-accent hover:bg-vdsa-accent-light text-white font-bold rounded-xl transition-all duration-200 shadow-lg shadow-vdsa-accent hover:shadow-[0_0_32px_rgba(168,85,247,0.6)] hover:scale-[1.04] hover:ring-2 hover:ring-vdsa-accent-light/60 flex items-center justify-center gap-2 text-base cursor-pointer whitespace-nowrap shrink-0 max-w-full"
            >
              <span class="truncate">{{ nextStudyLessonLabel }}</span> <BaseIcon name="play" class="w-4 h-4 shrink-0" />
            </button>
            <button @click="scrollToLessons" class="px-8 py-3.5 bg-vdsa-surface hover:bg-vdsa-accent/10 hover:text-vdsa-accent-light hover:border-vdsa-accent/50 text-white font-bold rounded-xl border border-vdsa-border transition-all duration-200 flex items-center justify-center gap-2 text-base cursor-pointer whitespace-nowrap shrink-0">
              Nội dung lộ trình <BaseIcon name="chevron-down" class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>

      <!-- ── MAIN CONTENT ────────────────────────────────────── -->
      <div class="container mx-auto max-w-5xl px-4 pb-20">
        <div class="w-full h-px bg-gradient-to-r from-transparent via-vdsa-border to-transparent mb-12"></div>

        <div class="flex flex-col gap-14 w-full">

          <!-- LEARNING OBJECTIVES (kiểu Educative: checklist) -->
          <section v-if="courseObjectives.length">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider mb-6">Mục tiêu học tập</h2>
            <div class="p-8 rounded-3xl border border-vdsa-border bg-vdsa-surface">
              <ul class="space-y-4">
                <li v-for="(obj, i) in courseObjectives" :key="i" class="flex items-start gap-4">
                  <div class="mt-1 w-6 h-6 rounded-full bg-vdsa-green/10 flex items-center justify-center shrink-0">
                    <BaseIcon name="check" class="w-3.5 h-3.5 text-vdsa-green" />
                  </div>
                  <p class="text-vdsa-secondary leading-relaxed text-sm md:text-base">{{ obj }}</p>
                </li>
              </ul>
            </div>
          </section>

          <!-- KEY OUTCOMES (cards) -->
          <section v-if="courseOutcomes.length">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider mb-6">Kết quả đạt được</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div v-for="(outcome, i) in courseOutcomes" :key="i" class="p-6 rounded-2xl border border-vdsa-border bg-vdsa-surface hover:bg-vdsa-hover transition-colors">
                <div class="w-10 h-10 rounded-xl bg-vdsa-purple/10 flex items-center justify-center mb-4">
                  <BaseIcon name="trophy" class="w-5 h-5 text-vdsa-purple-light" />
                </div>
                <h3 class="text-white font-bold mb-2">{{ outcome.title }}</h3>
                <p class="text-vdsa-muted text-sm leading-relaxed">{{ outcome.desc }}</p>
              </div>
            </div>
          </section>

          <!-- WHY CHOOSE THIS COURSE (timeline giữa + scroll reveal 2 chiều — giống Educative) -->
          <section v-if="(course.highlights ?? []).length" class="why-choose">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider mb-2">Tại sao chọn lộ trình này?</h2>
            <p class="text-vdsa-muted text-sm mb-12">Kỹ năng Cấu trúc Dữ liệu không thể thiếu đối với mọi lập trình viên — từ phỏng vấn đến công việc thực tế.</p>

            <div class="relative">
              <!-- Thanh timeline: nét đứt nhạt + tím đổ xuống nối thành đường đậm sáng -->
              <div
                ref="whyTrackRef"
                class="absolute -top-4 -bottom-4 w-[3px] left-[15px] md:left-1/2 md:-translate-x-1/2"
              >
                <!-- Nét đứt nhạt ban đầu -->
                <div class="absolute inset-0 border-l-2 border-dashed border-white/30"></div>
                <!-- Tím đổ xuống theo scroll (nối các nét đứt thành đường liền đậm sáng) -->
                <div
                  ref="whyFillRef"
                  class="absolute top-0 left-0 w-full rounded-t-full bg-gradient-to-b from-vdsa-accent-light via-vdsa-accent to-vdsa-accent-dark shadow-[0_0_14px_rgba(168,85,247,0.7)]"
                  :style="{ height: fillPercent + '%' }"
                ></div>
              </div>

              <div
                v-for="(h, i) in (course.highlights ?? [])"
                :key="i"
                v-reveal="i"
                class="relative pb-28 last:pb-0"
              >
                <!-- Dấu chấm trên timeline (mỗi chấm một ý) -->
                <span class="absolute top-1 w-5 h-5 rounded-full bg-vdsa-accent shadow-[0_0_14px_rgba(168,85,247,0.9)] ring-4 ring-vdsa-accent/15 z-10 flex items-center justify-center left-[15px] md:left-1/2 md:-translate-x-1/2">
                  <span class="w-2 h-2 rounded-full bg-white"></span>
                </span>

                <!-- Nội dung so le trái/phải (mobile: đều đổ sang phải thanh) -->
                <div
                  :class="i % 2 === 0
                    ? 'ml-auto md:w-1/2 md:pr-16 md:text-right pl-10 pr-2'
                    : 'md:w-1/2 md:pl-16 pl-10 pr-2'"
                  class="min-h-[116px]"
                >
                  <h3 class="text-lg md:text-xl font-bold text-white leading-snug">{{ h.title }}</h3>
                  <p class="text-vdsa-muted text-sm md:text-base leading-relaxed mt-2">{{ h.description }}</p>
                </div>
              </div>
            </div>
          </section>

          <!-- COURSE CONTENT (curriculum kiểu Educative: đánh số + lesson count) -->
          <section id="course-lessons" class="scroll-mt-8">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider">Nội dung lộ trình</h2>
            <p class="text-sm text-vdsa-muted mb-6">{{ course.lessons.length }} bài học · {{ modules.length }} chủ đề</p>

            <PathModuleList
              :modules="modules"
              @select-lesson="onModuleLessonSelect"
            />
          </section>

          <!-- TESTIMONIALS (từ backend — tùy biến theo khóa) -->
          <section v-if="(course.testimonials ?? []).length">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider mb-6">Học viên nói gì về lộ trình</h2>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
              <figure v-for="(t, i) in (course.testimonials ?? [])" :key="i" class="p-6 rounded-2xl border border-vdsa-border bg-vdsa-surface flex flex-col">
                <BaseIcon name="quote" class="w-6 h-6 text-vdsa-accent mb-3" />
                <blockquote class="text-vdsa-secondary text-sm leading-relaxed flex-1">"{{ t.quote }}"</blockquote>
                <figcaption class="mt-4 flex items-center gap-3">
                  <span class="w-9 h-9 rounded-full bg-vdsa-accent/10 text-vdsa-accent flex items-center justify-center font-bold text-sm shrink-0">
                    {{ initial(t.name) }}
                  </span>
                  <span>
                    <span class="block text-sm font-bold text-white">{{ t.name }}</span>
                    <span class="block text-xs text-vdsa-muted">{{ t.role }}</span>
                  </span>
                </figcaption>
              </figure>
            </div>
          </section>

          <!-- ABOUT THE AUTHOR (từ backend — AuthorId trỏ giảng viên thật) -->
          <section v-if="course.author">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider mb-6">Về giảng viên</h2>
            <div class="p-6 md:p-8 rounded-2xl border border-vdsa-border bg-vdsa-surface flex flex-col md:flex-row gap-6">
              <div class="w-20 h-20 rounded-2xl bg-vdsa-accent/10 text-vdsa-accent flex items-center justify-center text-3xl font-black shrink-0">
                {{ initial(course.author.name) }}
              </div>
              <div>
                <h3 class="text-lg font-bold text-white">{{ course.author.name }}</h3>
                <p v-if="course.author.academicDegree" class="text-xs font-semibold text-vdsa-accent mt-1 uppercase tracking-wider">{{ course.author.academicDegree }}</p>
                <p v-if="course.author.bio" class="text-vdsa-secondary text-sm leading-relaxed mt-3">{{ course.author.bio }}</p>
                <a
                  v-if="course.author.profileLink"
                  :href="course.author.profileLink"
                  target="_blank"
                  rel="noopener noreferrer"
                  class="inline-flex items-center gap-2 text-sm font-semibold text-vdsa-accent hover:text-vdsa-accent-light mt-4"
                >
                  <BaseIcon name="link" class="w-4 h-4" /> Tìm hiểu thêm về giảng viên
                </a>
              </div>
            </div>
          </section>

          <!-- GÓP Ý CHO GIẢNG VIÊN (tương tác 2 chiều) -->
          <section id="course-feedback" class="scroll-mt-8">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider mb-2">Góp ý cho giảng viên</h2>
            <p class="text-vdsa-muted text-sm mb-6">
              Bạn có thắc mắc, đóng góp ý kiến, báo lỗi hoặc đề xuất nội dung? Hãy gửi tại đây — giảng viên sẽ đọc và phản hồi bạn.
            </p>

            <form v-if="auth.isAuthenticated" @submit.prevent="submitFeedback" class="p-6 rounded-2xl border border-vdsa-border bg-vdsa-surface">
              <div class="flex flex-col md:flex-row gap-4 mb-4">
                <label class="flex flex-col gap-1.5 md:w-56">
                  <span class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">Loại ý kiến</span>
                  <select v-model="feedbackType" class="px-3 py-2.5 rounded-xl bg-vdsa-bg border border-vdsa-border text-sm text-white focus:border-vdsa-accent outline-none">
                    <option value="Suggestion">Góp ý</option>
                    <option value="Bug">Báo lỗi</option>
                    <option value="Request">Đề xuất nội dung</option>
                  </select>
                </label>
                <div class="flex-1">
                  <textarea
                    v-model="feedbackContent"
                    rows="3"
                    maxlength="1000"
                    required
                    placeholder="Nhập nội dung ý kiến của bạn (tối đa 1000 ký tự)..."
                    class="w-full px-4 py-3 rounded-xl bg-vdsa-bg border border-vdsa-border text-sm text-white placeholder:text-vdsa-disabled focus:border-vdsa-accent outline-none resize-none"
                  ></textarea>
                  <div class="flex items-center justify-between mt-2">
                    <span v-if="feedbackError" class="text-xs font-semibold text-vdsa-red">{{ feedbackError }}</span>
                    <span v-else></span>
                    <span class="text-xs text-vdsa-muted">{{ feedbackContent.length }}/1000</span>
                  </div>
                </div>
              </div>
              <div class="flex justify-end">
                <button
                  type="submit"
                  :disabled="feedbackSending"
                  class="px-6 py-2.5 rounded-xl font-bold bg-vdsa-accent hover:bg-vdsa-accent-light text-white transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <BaseIcon name="send" class="w-4 h-4" /> {{ feedbackSending ? 'Đang gửi...' : 'Gửi ý kiến' }}
                </button>
              </div>
            </form>

            <div v-else class="p-6 rounded-2xl border border-vdsa-border bg-vdsa-surface flex items-center gap-3 text-sm text-vdsa-secondary">
              <BaseIcon name="message" class="w-5 h-5 text-vdsa-accent" />
              <span>
                Đăng nhập để gửi ý kiến cho giảng viên —
                <router-link :to="{ name: 'login', query: { redirect: $route.fullPath } }" class="text-vdsa-accent font-semibold hover:text-vdsa-accent-light">đăng nhập ngay</router-link>
              </span>
            </div>

            <div v-if="myFeedback.length" class="mt-8">
              <h3 class="text-sm font-bold text-vdsa-muted uppercase tracking-wider mb-4">Ý kiến của tôi ({{ myFeedback.length }})</h3>
              <div class="flex flex-col gap-4">
                <div v-for="item in myFeedback" :key="item.id" class="p-5 rounded-2xl border border-vdsa-border bg-vdsa-surface">
                  <div class="flex items-center gap-3 mb-3 flex-wrap">
                    <span class="text-xs font-bold uppercase tracking-wider px-2 py-0.5 rounded"
                      :class="typeBadgeClass(item.type)">{{ typeLabel(item.type) }}</span>
                    <span class="text-xs px-2 py-0.5 rounded font-bold uppercase tracking-wider"
                      :class="statusBadgeClass(item.status)">{{ statusLabel(item.status) }}</span>
                    <span class="text-xs text-vdsa-muted">{{ formatDate(item.createdAt) }}</span>
                  </div>
                  <p class="text-vdsa-secondary text-sm leading-relaxed">{{ item.content }}</p>
                  <div v-if="item.replyText" class="mt-4 p-4 rounded-xl bg-vdsa-bg border-l-2 border-vdsa-accent">
                    <p class="text-xs font-bold text-vdsa-accent mb-1 flex items-center gap-1.5">
                      <BaseIcon name="user-round" class="w-3.5 h-3.5" /> {{ item.repliedByName || 'Giảng viên' }} đã phản hồi
                    </p>
                    <p class="text-sm text-vdsa-secondary leading-relaxed">{{ item.replyText }}</p>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <!-- RELATED COURSES (tự ẩn khi trống) -->
          <section v-if="relatedCourses.length">
            <h2 class="text-xl font-bold text-white uppercase tracking-wider mb-6">Lộ trình liên quan</h2>
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
              <router-link
                v-for="rc in relatedCourses"
                :key="rc.id"
                :to="{ name: 'path-detail', params: { id: rc.id } }"
                class="p-5 rounded-2xl border border-vdsa-border bg-vdsa-surface hover:bg-vdsa-hover hover:border-vdsa-accent/40 transition-all group"
              >
                <h3 class="text-white font-bold leading-snug group-hover:text-vdsa-accent-light transition-colors">{{ rc.title }}</h3>
                <p class="text-xs text-vdsa-muted mt-2 line-clamp-2">{{ rc.description }}</p>
                <p class="text-xs font-semibold text-vdsa-yellow mt-3 flex items-center gap-1">
                  <BaseIcon name="zap" class="w-3 h-3" /> {{ rc.xpReward }} XP
                </p>
              </router-link>
            </div>
          </section>

        </div>
      </div>
    </div>

    <!-- Custom Registration Modal — Teleport NẰM TRONG root div (Teleport đứng cạnh div gốc
         vẫn là fragment root → <Transition mode="out-in"> bị kẹt, không rời trang được) -->
    <Teleport to="body">
    <div v-if="showRegisterModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm transition-opacity">
      <div class="bg-vdsa-surface border border-vdsa-border rounded-2xl p-6 max-w-sm w-full shadow-2xl animate-fade-in-up">
        <div class="flex items-center gap-3 mb-4 text-vdsa-yellow">
          <BaseIcon name="alert-circle" class="w-6 h-6" />
          <h3 class="text-xl font-bold text-white">Mở khóa lộ trình</h3>
        </div>
        <p class="text-vdsa-secondary mb-6 leading-relaxed">
          Bạn có chắc muốn đăng ký lộ trình <strong class="text-white">{{ course?.title }}</strong> này không?
          <br /><span class="text-xs text-purple-300 font-medium inline-block mt-2">Đăng ký lộ trình với 1 🤍. Bài 1 miễn phí, các bài tiếp theo tốn 1 🤍 khi tham gia lần đầu (học lại miễn phí sau khi pass).</span>
        </p>
        <div class="flex gap-3 justify-end">
          <button
            @click="showRegisterModal = false"
            class="px-5 py-2.5 rounded-xl font-semibold text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          >
            Hủy bỏ
          </button>
          <button
            @click="confirmRegistration"
            class="px-5 py-2.5 rounded-xl font-semibold bg-vdsa-accent hover:bg-vdsa-accent-light text-white shadow-lg shadow-vdsa-accent transition-all flex items-center gap-2 cursor-pointer"
          >
            Mở khóa ngay (1 🤍)
            <BaseIcon name="check" class="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
defineOptions({ inheritAttrs: false });

import { ref, computed, onMounted, onUnmounted, onActivated, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useCourseStore } from '@/features/courses/store/useCourseStore';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useHeartSystem } from '@/composables/useHeartSystem';
import { useUiStore } from '@/stores/ui';
import { courseApi, type CourseDetailDto, type CourseFeedbackDto, type CourseLessonDto } from '@/services/courseApi';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import PathModuleList, { type PathModuleGroup, type PathModuleLesson } from '@/components/path/PathModuleList.vue';
import { messages } from '@/i18n/vi';

const courseStore = useCourseStore();
const auth = useAuthStore();
const gamification = useGamificationStore();
const heartSystem = useHeartSystem();
const ui = useUiStore();
const route = useRoute();
const router = useRouter();
const showRegisterModal = ref(false);
const targetLessonToStart = ref<CourseLessonDto | null>(null);

const loading = ref(true);
const error = ref<string | null>(null);
const course = ref<CourseDetailDto | null>(null);

// ── v-reveal: hiện dần khi lướt vào viewport (2 chiều — lặp lại mỗi lần vào/ra) ──
// Dùng làm binding: v-reveal="index" → stagger delay theo thứ tự item.
const vReveal = {
  mounted(el: HTMLElement & { _observer?: IntersectionObserver }, binding: { value?: number }) {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
      return;
    }
    el.classList.add('reveal-item');
    el.style.transitionDelay = `${(binding.value ?? 0) * 140}ms`;
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            el.classList.add('is-visible');
          } else {
            el.classList.remove('is-visible');
          }
        });
      },
      { threshold: 0.2 },
    );
    observer.observe(el);
    el._observer = observer;
  },
  unmounted(el: HTMLElement & { _observer?: IntersectionObserver }) {
    el._observer?.disconnect();
    delete el._observer;
  },
};


// ── Timeline tô tím theo scroll (Why choose) ──
const whyTrackRef = ref<HTMLElement | null>(null);
const fillPercent = ref(0);

function updateTimelineFill() {
  const track = whyTrackRef.value;
  if (!track) return;
  const rect = track.getBoundingClientRect();
  const viewport = window.innerHeight;
  // Fill TRỄ hơn vùng hiển thị (chia thêm viewport*0.5) → khi lướt xuống,
  // nét đứt nhạt hiện trước ở phía dưới, tím đổ xuống nối dần phía trên.
  const progress = (viewport - rect.top) / (rect.height + viewport * 0.5);
  fillPercent.value = Math.min(1, Math.max(0, progress)) * 100;
}

onMounted(() => {
  window.addEventListener('scroll', updateTimelineFill, { passive: true });
  window.addEventListener('resize', updateTimelineFill);
});

onUnmounted(() => {
  window.removeEventListener('scroll', updateTimelineFill);
  window.removeEventListener('resize', updateTimelineFill);
});

const modules = computed<PathModuleGroup[]>(() => {
  if (!course.value?.lessons) return [];
  const map = new Map<string, PathModuleLesson[]>();
  course.value.lessons.forEach(l => {
    const mTitle = l.moduleTitle || 'Nội dung bài học';
    if (!map.has(mTitle)) {
      map.set(mTitle, []);
    }
    if (l.sandboxType !== 'folder') {
      map.get(mTitle)!.push({
        id: l.id,
        nodeId: typeof l.nodeId === 'number' ? l.nodeId : Number(l.id),
        lessonId: l.lessonId ? Number(l.lessonId) : undefined,
        title: l.title,
        sandboxType: l.sandboxType,
        quizId: l.quizId ? Number(l.quizId) : undefined,
        status: l.status,
        isCompleted: l.status === 'Completed',
        locked: l.status === 'Completed' ? false : !!l.locked,
        xpReward: l.xpReward,
      });
    }
  });
  return Array.from(map.entries())
    .filter(([, lessons]) => lessons.length > 0)
    .map(([title, lessons]) => ({ title, lessons }));
});

/** Adapter: PathModuleLesson → CourseLessonDto gốc rồi gọi startLesson. */
function onModuleLessonSelect(lesson: PathModuleLesson): void {
  const target = (course.value?.lessons ?? []).find(
    (l) => String(l.id) === String(lesson.nodeId ?? lesson.id),
  );
  if (target) void startLesson(target);
}

/** C3: nút "Bắt đầu học" đưa học viên vào bài đang học dở — bài chưa hoàn thành đầu tiên. */
const nextStudyLesson = computed<CourseLessonDto | null>(() => {
  const lessons = (course.value?.lessons ?? []).filter((l) => l.sandboxType !== 'folder');
  if (lessons.length === 0) return null;
  return lessons.find((l) => l.status !== 'Completed' && !l.locked) ?? lessons.find((l) => l.status !== 'Completed') ?? lessons[0];
});

const nextStudyLessonLabel = computed(() => {
  const lessons = (course.value?.lessons ?? []).filter((l) => l.sandboxType !== 'folder');
  if (lessons.length === 0) return 'Bắt đầu học';
  const isCompletedAll = lessons.length > 0 && lessons.every((l) => l.status === 'Completed');
  if (isCompletedAll) return 'Ôn tập lộ trình';
  const hasProgress = lessons.some((l) => l.status === 'Completed');
  const target = nextStudyLesson.value;
  if (!target) return 'Bắt đầu học';
  return hasProgress ? 'Học tiếp: ' + target.title : 'Bắt đầu học';
});

const quizCount = computed(() => course.value?.lessons.filter(l => l.sandboxType === 'quiz' || l.quizId).length ?? 0);
const labCount = computed(() => course.value?.lessons.filter(l => l.sandboxType === 'codelab').length ?? 0);
const xpTotal = computed(() => course.value?.xpReward ?? 0);

const relatedCourses = ref<Array<{ id: string; title: string; description: string; xpReward: number }>>([]);

const OUTCOME_MILESTONES = [
  'Làm chủ Cấu trúc dữ liệu',
  'Chinh phục Bài toán phỏng vấn',
  'Thực hành & Chấm điểm tự động',
  'Tối ưu độ phức tạp thuật toán',
  'Tự tin phỏng vấn Tech',
];

function cleanTitle(rawTitle: string): string {
  if (!rawTitle) return '';
  return rawTitle.replace(/Mini-Quizz/gi, 'Mini-Quiz');
}

const courseObjectives = computed(() => course.value?.learningObjectives ?? []);
const courseOutcomes = computed(() => (course.value?.keyOutcomes ?? []).map((text, idx) => {
  const sep = text.includes(':') ? ':' : (text.includes(' — ') ? ' — ' : (text.includes(' - ') ? ' - ' : ''));
  if (sep) {
    const [t, ...rest] = text.split(sep);
    return { title: t.trim(), desc: rest.join(sep).trim() };
  }
  return {
    title: OUTCOME_MILESTONES[idx % OUTCOME_MILESTONES.length] || `Kỹ năng đạt được #${idx + 1}`,
    desc: text,
  };
}));

function initial(name: string) { return (name || '?').trim().charAt(0).toUpperCase(); }

function scrollToLessons() {
  document.getElementById('course-lessons')?.scrollIntoView({ behavior: 'smooth' });
}

const fromClassId = computed(() => (route.query.classId ? Number(route.query.classId) : null));

const isOwnCourse = computed(() => {
  if (!auth.isAuthenticated || !course.value) return false;
  if (auth.user?.role === 'ADMIN') return true;
  const uId = auth.user?.id;
  return (
    auth.user?.role === 'TEACHER' &&
    uId != null &&
    (course.value.authorId === uId || (course.value as any).createdBy === uId)
  );
});

async function autoEnrollFromClass(): Promise<void> {
  if (!auth.isAuthenticated || !course.value) return;
  if (isOwnCourse.value) return;
  if (courseStore.isEnrolled(course.value.id)) return;

  // Lớp học: tự động cấp quyền học cho học viên trong lớp (miễn phí)
  courseStore.enrollCourse(course.value.id);
}

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
    if (course.value.lessons) {
      courseStore.updateCourseLessons(courseId, course.value.lessons);
    }
    updateTimelineFill();

    if (auth.isAuthenticated) {
      loadMyFeedback();
    } else {
      myFeedback.value = [];
    }

    if (fromClassId.value && auth.isAuthenticated && course.value && !courseStore.isEnrolled(course.value.id)) {
      await autoEnrollFromClass();
    }

    // Related courses: ưu tiên cùng topicId -> cùng category -> trùng từ khóa tiêu đề
    try {
      const all = await courseApi.getCourses() as Array<{ id: string; title: string; description: string; xpReward: number; topicId?: number; category?: string }>;
      const currentTopicId = (course.value as any)?.topicId;
      const currentCategory = course.value?.category?.toLowerCase() || '';
      const titleWords = (course.value?.title || '').toLowerCase().split(/\s+/).filter(w => w.length > 2);

      const others = (all || []).filter(c => String(c.id) !== courseId);
      const scored = others.map(c => {
        let score = 0;
        if (currentTopicId && c.topicId === currentTopicId) score += 10;
        if (currentCategory && c.category?.toLowerCase() === currentCategory) score += 5;
        const cTitleLower = (c.title || '').toLowerCase();
        for (const word of titleWords) {
          if (cTitleLower.includes(word)) score += 2;
        }
        return { course: c, score };
      });

      const relevant = scored.filter(s => s.score > 0);
      relevant.sort((a, b) => b.score - a.score);
      relatedCourses.value = relevant.slice(0, 3).map(s => s.course);
    } catch {
      relatedCourses.value = [];
    }
  } catch (err) {
    console.error('Failed to load course detail:', err);
    error.value = 'Không tìm thấy lộ trình này (Lỗi kết nối máy chủ).';
  } finally {
    loading.value = false;
  }
}

async function confirmRegistration() {
  if (!auth.isAuthenticated) {
    router.push({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }
  const ok = await heartSystem.spendHeartSafely('Mở khóa lộ trình', isOwnCourse.value);
  if (!ok) return;

  showRegisterModal.value = false;
  if (course.value) {
    courseStore.enrollCourse(course.value.id);
    ui.showToast('Mở khóa lộ trình thành công! (-1 🤍)', 'success');
    if (targetLessonToStart.value) {
      const nextTarget = targetLessonToStart.value;
      targetLessonToStart.value = null;
      // Bỏ qua trừ tim vì học viên vừa dùng 1 tim để mở khóa toàn bộ lộ trình
      await startLesson(nextTarget, true);
    }
  }
}

async function startLesson(lesson: CourseLessonDto, skipHeartCharge = false) {
  if (!isOwnCourse.value && lesson.locked) return; // node chưa mở khoá — bấm bị chặn (backend cũng 403)

  if (!auth.isAuthenticated) {
    router.push({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }

  // Chặn khi chưa enroll lộ trình: hiện modal xác nhận tham gia lộ trình (học viên hoặc giáo viên học lộ trình người khác)
  if (!isOwnCourse.value && course.value && !courseStore.isEnrolled(course.value.id)) {
    targetLessonToStart.value = lesson;
    showRegisterModal.value = true;
    return;
  }

  const courseId = Number(course.value?.id);
  const nodeId = typeof lesson.nodeId === 'number' ? lesson.nodeId : Number(lesson.id);

  // Quy tắc tim: Đăng ký tốn 1 tim, node 1 miễn phí (0 tim), các node mới sau tốn 1 tim khi vào lần đầu
  const playableLessons = (course.value?.lessons ?? []).filter(l => l.sandboxType !== 'folder');
  const firstLesson = playableLessons[0];
  const isFirstNode = firstLesson && (String(firstLesson.id) === String(lesson.id) || (firstLesson.nodeId && firstLesson.nodeId === lesson.nodeId));
  const isAlreadyDone = lesson.status === 'Completed';
  const shouldChargeHeart = false; // Option B: chỉ trừ 1 tim khi đăng ký lộ trình, không trừ khi vào từng bài

  if (courseId && nodeId && shouldChargeHeart) {
    const ok = await heartSystem.enterLessonNode(courseId, nodeId, false, isOwnCourse.value);
    if (!ok) return;
  }

  if (course.value?.id) {
    courseStore.enrollCourse(course.value.id);
  }

  const queryObj: Record<string, string> = { courseId: String(course.value?.id) };
  if (fromClassId.value) {
    queryObj.classId = String(fromClassId.value);
  }

  router.push({ name: 'lesson-study', params: { id: lesson.id }, query: queryObj });
}

// ── Góp ý cho giảng viên (2 chiều) ──────────────────────────

const feedbackType = ref<'Suggestion' | 'Bug' | 'Request'>('Suggestion');
const feedbackContent = ref('');
const feedbackSending = ref(false);
const feedbackError = ref('');
const myFeedback = ref<CourseFeedbackDto[]>([]);

async function loadMyFeedback() {
  if (!course.value || !auth.isAuthenticated) return;
  try {
    myFeedback.value = await courseApi.getMyCourseFeedback(Number(course.value.id));
  } catch {
    myFeedback.value = [];
  }
}

async function submitFeedback() {
  if (!course.value) return;
  feedbackError.value = '';
  feedbackSending.value = true;
  try {
    await courseApi.submitCourseFeedback({
      courseId: Number(course.value.id),
      type: feedbackType.value,
      content: feedbackContent.value,
    });
    feedbackContent.value = '';
    await loadMyFeedback();
  } catch (err) {
    console.error('Failed to submit feedback:', err);
    feedbackError.value = 'Không gửi được ý kiến — vui lòng thử lại.';
  } finally {
    feedbackSending.value = false;
  }
}

function typeLabel(t: string) {
  return t === 'Bug' ? 'Báo lỗi' : t === 'Request' ? 'Đề xuất nội dung' : 'Góp ý';
}

function statusLabel(s: string) {
  return s === 'Read' ? 'Đã đọc' : s === 'Resolved' ? 'Đã xử lý' : 'Mới';
}

function typeBadgeClass(t: string) {
  return t === 'Bug'
    ? 'bg-vdsa-red/10 text-vdsa-red-light'
    : t === 'Request'
      ? 'bg-vdsa-cyan/10 text-vdsa-cyan-light'
      : 'bg-vdsa-accent/10 text-vdsa-accent';
}

function statusBadgeClass(s: string) {
  return s === 'Resolved'
    ? 'bg-vdsa-green/10 text-vdsa-green'
    : 'bg-vdsa-yellow/10 text-vdsa-yellow';
}

function formatDate(iso: string) {
  try {
    const trimmed = (iso || '').trim();
    const dateStr = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}/.test(trimmed) && !trimmed.endsWith('Z') && !/[+-]\d{2}:\d{2}$/.test(trimmed)
      ? trimmed + 'Z'
      : trimmed;
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      timeZone: 'Asia/Ho_Chi_Minh',
    });
  } catch {
    return iso;
  }
}

onMounted(() => {
  loadCourseDetail();
  window.addEventListener('focus', loadCourseDetail);
});

onUnmounted(() => {
  window.removeEventListener('focus', loadCourseDetail);
});

onActivated(() => {
  loadCourseDetail();
});

watch(
  () => route.params.id,
  () => loadCourseDetail(),
);
</script>

<style scoped>
@import "./CourseDetailView.css";
</style>
