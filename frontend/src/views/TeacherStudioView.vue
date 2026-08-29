<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import {
  BookOpen,
  CheckCircle2,
  ClipboardList,
  ExternalLink,
  GraduationCap,
  Layers3,
  Network,
  Plus,
  RefreshCw,
  ShieldAlert,
  Sparkles,
  Users,
  ArrowRight,
  Eye,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useClassStore } from '@/stores/classStore';
import { useUiStore } from '@/stores/ui';
import * as lessonsApi from '@/api/lessons';
import { courseApi, type CourseListDto } from '@/services/courseApi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const auth = useAuthStore();
const classStore = useClassStore();
const ui = useUiStore();
const router = useRouter();

const loading = ref(true);
const totalLessons = ref(0);
const totalTopics = ref(0);
const totalCourses = ref(0);
const myLessonsCount = ref(0);
const myTopicsCount = ref(0);
const myCoursesCount = ref(0);
const recentLessons = ref<lessonsApi.LessonSummary[]>([]);

// 4 Hubs cốt lõi sau khi gộp gọn (S5 & S9)
const sections = [
  {
    title: 'Lộ trình & Bài giảng',
    description: 'Thiết kế lộ trình học, tổ chức cây Chương - Bài giảng, biên soạn Markdown GFM và gắn mô phỏng trực quan.',
    icon: Network,
    to: { path: '/studio', query: { tab: 'curriculum' } },
    badge: 'Cốt lõi',
    actionLabel: 'Mở Studio biên soạn',
    accentColor: 'text-vdsa-purple-light bg-vdsa-purple/10 border-vdsa-purple/20',
  },
  {
    title: 'Ngân hàng Quiz & Codelab',
    description: 'Tạo câu hỏi trắc nghiệm 4 đáp án, tải file mẫu CSV, bài tập chấm code tự động và lab thực hành.',
    icon: ClipboardList,
    to: { path: '/studio', query: { tab: 'exercises' } },
    actionLabel: 'Ngân hàng Bài tập',
    accentColor: 'text-vdsa-yellow bg-vdsa-yellow/10 border-vdsa-yellow/20',
  },
  {
    title: 'Quản lý Lớp học & Thành viên',
    description: 'Tạo lớp học, cấp mã mời, quản lý danh sách học viên và phân phối lộ trình học nội bộ cho từng lớp.',
    icon: Users,
    to: { name: 'classes' },
    target: '_blank',
    actionLabel: 'Quản lý Lớp học',
    accentColor: 'text-vdsa-red bg-vdsa-red/10 border-vdsa-red/20',
  },
  {
    title: 'Trung tâm Báo cáo & Phản hồi',
    description: 'Theo dõi các thắc mắc bài học, phản hồi góp ý trực tiếp và xử lý báo cáo từ học viên.',
    icon: ShieldAlert,
    to: { path: '/studio', query: { tab: 'feedback' } },
    actionLabel: 'Xử lý Báo cáo',
    accentColor: 'text-vdsa-purple-light bg-vdsa-accent/10 border-vdsa-accent/20',
  },
];

// Quy trình 5 bước kèm action link (S6)
const workflowSteps = [
  {
    step: '01',
    title: 'Chủ đề',
    desc: 'Tạo chuyên đề kiến thức (Sorting, Trees, Graphs...)',
    to: { path: '/studio', query: { tab: 'curriculum' } },
    btnText: 'Tạo chủ đề',
    colorClass: 'text-vdsa-purple-light',
  },
  {
    step: '02',
    title: 'Soạn bài',
    desc: 'Soạn lý thuyết GFM Markdown + định dạng trực quan',
    to: { name: 'studio-lesson-new' },
    btnText: 'Soạn bài mới',
    colorClass: 'text-vdsa-cyan',
  },
  {
    step: '03',
    title: 'Gắn mô phỏng',
    desc: 'Đính kèm visualizer tương tác cho thuật toán',
    to: { name: 'simulations' },
    btnText: 'Thư viện',
    colorClass: 'text-vdsa-green',
  },
  {
    step: '04',
    title: 'Quiz & Lab',
    desc: 'Tải CSV trắc nghiệm hoặc tạo bài code chấm điểm',
    to: { path: '/studio', query: { tab: 'exercises' } },
    btnText: 'Tạo quiz',
    colorClass: 'text-vdsa-yellow',
  },
  {
    step: '05',
    title: 'Giao cho lớp',
    desc: 'Nhập lộ trình học vào lớp — học sinh tham gia học',
    to: { name: 'classes' },
    btnText: 'Mở lớp học',
    colorClass: 'text-vdsa-red',
  },
];

const statusText = computed(() =>
  auth.role === 'ADMIN'
    ? 'Quản trị viên hệ thống (Toàn quyền quản trị nội dung)'
    : 'Giảng viên chính thức (Quyền biên soạn, xuất bản & quản lý lớp)'
);

async function loadStudioData(): Promise<void> {
  loading.value = true;
  try {
    const [lessonData, topicTree, courseList] = await Promise.all([
      lessonsApi.fetchLessons({ page: 1, pageSize: 1000 }),
      lessonsApi.fetchTopics().catch(() => [] as lessonsApi.Topic[]),
      courseApi.getCourses().catch(() => [] as CourseListDto[]),
      classStore.fetchClasses().catch(() => {}),
    ]);

    totalLessons.value = lessonData.total || lessonData.items.length;
    totalTopics.value = topicTree.length;
    totalCourses.value = courseList.length;

    const currentUserId = auth.user?.id;
    if (currentUserId) {
      myLessonsCount.value = lessonData.items.filter((l) => (l as any).createdBy === currentUserId).length;
      myTopicsCount.value = topicTree.filter((t) => (t as any).createdBy === currentUserId).length;
      myCoursesCount.value = courseList.filter(
        (c) => (c as any).createdBy === currentUserId || (c as any).authorId === currentUserId
      ).length;
    }

    if (auth.role === 'TEACHER' && currentUserId) {
      const myLessons = lessonData.items.filter((l) => (l as any).createdBy === currentUserId);
      recentLessons.value = myLessons.length > 0 ? myLessons.slice(0, 5) : lessonData.items.slice(0, 5);
    } else {
      recentLessons.value = lessonData.items.slice(0, 5);
    }
  } catch (err) {
    ui.showToast('Không thể tải một số dữ liệu studio. Vui lòng kiểm tra lại kết nối.', 'warning');
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  void loadStudioData();
});
</script>

<template>
  <main class="teacher-studio container space-y-8 pb-16 pt-4">
    <!-- ═══ 1. HERO BANNER CHÀO MỪNG ═══ -->
    <header class="p-6 md:p-8 rounded-3xl bg-vdsa-surface border border-vdsa-border relative overflow-hidden">
      <!-- Glow decoration -->
      <div class="absolute -right-20 -top-20 w-80 h-80 bg-vdsa-accent/10 rounded-full blur-3xl pointer-events-none"></div>

      <div class="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-6 relative z-10">
        <div class="space-y-2">
          <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-vdsa-accent/15 border border-vdsa-accent/30 text-vdsa-purple-light text-xs font-bold font-mono uppercase tracking-wider">
            <GraduationCap :size="14" /> Teacher Studio
          </div>
          <h1 class="text-2xl sm:text-3xl font-black text-white tracking-tight flex items-center gap-2 flex-wrap">
            Xin chào, {{ auth.user?.displayName || 'Thầy/Cô' }}!
          </h1>
          <p class="text-sm text-vdsa-muted max-w-2xl leading-relaxed">
            Trung tâm điều hành giảng dạy DSA: Biên soạn bài giảng trực quan, đính kèm thuật toán mô phỏng, thiết kế lộ trình và phân phối bài học cho lớp học.
          </p>

          <div class="flex items-center gap-2 pt-2 text-xs text-vdsa-green font-medium">
            <CheckCircle2 :size="14" />
            <span>{{ statusText }}</span>
          </div>
        </div>

        <!-- Quick actions buttons -->
        <div class="flex flex-wrap items-center gap-2.5 shrink-0">
          <Button variant="secondary" size="sm" :disabled="loading" @click="loadStudioData" title="Làm mới dữ liệu">
            <RefreshCw :size="14" :class="{ 'animate-spin': loading }" />
          </Button>

          <RouterLink :to="{ name: 'classes' }" target="_blank" rel="noopener">
            <Button variant="secondary" size="sm">
              <Users :size="14" /> Lớp của tôi
            </Button>
          </RouterLink>

          <RouterLink :to="{ name: 'studio-lesson-new' }">
            <Button variant="primary" size="sm">
              <Plus :size="14" /> Soạn bài giảng mới
            </Button>
          </RouterLink>
        </div>
      </div>
    </header>

    <!-- ═══ 2. KPI METRICS STRIP (S7) ═══ -->
    <section class="grid grid-cols-2 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">Lớp phụ trách</p>
          <h3 class="text-2xl font-black text-white mt-1">
            <span v-if="!loading">{{ classStore.classes.length }}</span>
            <span v-else class="text-lg text-vdsa-disabled">...</span>
          </h3>
        </div>
        <div class="w-12 h-12 rounded-xl bg-vdsa-red/10 text-vdsa-red flex items-center justify-center">
          <Users :size="22" />
        </div>
      </div>

      <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">
            {{ auth.role === 'ADMIN' ? 'Lộ trình hệ thống' : 'Lộ trình của tôi' }}
          </p>
          <h3 class="text-2xl font-black text-white mt-1">
            <span v-if="!loading">{{ auth.role === 'ADMIN' ? totalCourses : myCoursesCount }}</span>
            <span v-else class="text-lg text-vdsa-disabled">...</span>
          </h3>
        </div>
        <div class="w-12 h-12 rounded-xl bg-vdsa-purple/10 text-vdsa-purple-light flex items-center justify-center">
          <Network :size="22" />
        </div>
      </div>

      <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">
            {{ auth.role === 'ADMIN' ? 'Chủ đề hệ thống' : 'Chủ đề của tôi' }}
          </p>
          <h3 class="text-2xl font-black text-white mt-1">
            <span v-if="!loading">{{ auth.role === 'ADMIN' ? totalTopics : myTopicsCount }}</span>
            <span v-else class="text-lg text-vdsa-disabled">...</span>
          </h3>
        </div>
        <div class="w-12 h-12 rounded-xl bg-vdsa-cyan/10 text-vdsa-cyan flex items-center justify-center">
          <Layers3 :size="22" />
        </div>
      </div>

      <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">
            {{ auth.role === 'ADMIN' ? 'Bài giảng hệ thống' : 'Bài giảng của tôi' }}
          </p>
          <h3 class="text-2xl font-black text-white mt-1">
            <span v-if="!loading">{{ auth.role === 'ADMIN' ? totalLessons : myLessonsCount }}</span>
            <span v-else class="text-lg text-vdsa-disabled">...</span>
          </h3>
        </div>
        <div class="w-12 h-12 rounded-xl bg-vdsa-green/10 text-vdsa-green flex items-center justify-center">
          <BookOpen :size="22" />
        </div>
      </div>
    </section>

    <!-- ═══ 3. WORKFLOW QUY TRÌNH GIẢNG DẠY ACTIONABLE (S6) ═══ -->
    <section class="p-5 rounded-2xl bg-vdsa-surface/70 border border-vdsa-border/80">
      <div class="flex items-center justify-between gap-4 mb-3">
        <h3 class="text-xs font-extrabold text-white uppercase tracking-wider flex items-center gap-2">
          <Sparkles class="w-4 h-4 text-vdsa-accent" />
          Quy trình đóng gói bài giảng chuẩn DSA Visual
        </h3>
        <span class="text-xs text-vdsa-muted hidden sm:inline">5 bước tuần tự • Bấm để thao tác ngay</span>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-5 gap-3 text-xs">
        <RouterLink
          v-for="step in workflowSteps"
          :key="step.step"
          :to="step.to"
          class="p-3.5 rounded-xl bg-vdsa-bg border border-vdsa-border/70 hover:border-vdsa-accent/60 hover:bg-vdsa-surface transition-all flex flex-col justify-between space-y-3 group cursor-pointer"
        >
          <div class="space-y-1">
            <div class="font-mono font-bold text-xs" :class="step.colorClass">
              {{ step.step }}. {{ step.title }}
            </div>
            <p class="text-vdsa-secondary text-[11px] leading-relaxed">{{ step.desc }}</p>
          </div>

          <div class="pt-1 flex items-center justify-between text-[11px] font-semibold text-white/80 group-hover:text-vdsa-accent">
            <span>{{ step.btnText }}</span>
            <ArrowRight :size="12" class="group-hover:translate-x-1 transition-transform" />
          </div>
        </RouterLink>
      </div>
    </section>

    <!-- ═══ 4. CORE STUDIO HUBS GRID (S5 & S9) ═══ -->
    <section class="space-y-4">
      <div class="flex items-center justify-between">
        <h2 class="text-lg font-black text-white tracking-tight">Phân hệ Quản lý &amp; Biên soạn</h2>
        <span class="text-xs text-vdsa-muted">4 không gian làm việc chính</span>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <article
          v-for="section in sections"
          :key="section.title"
          class="p-6 rounded-2xl bg-vdsa-surface border border-vdsa-border hover:border-vdsa-border-strong hover:bg-vdsa-surface/90 transition-all flex flex-col justify-between group space-y-4"
        >
          <div class="space-y-3">
            <div class="flex items-center justify-between">
              <div :class="['w-10 h-10 rounded-xl flex items-center justify-center border', section.accentColor]">
                <component :is="section.icon" :size="20" />
              </div>
              <Badge v-if="section.badge" variant="primary" size="sm">{{ section.badge }}</Badge>
            </div>

            <div>
              <h3 class="text-base font-bold text-white group-hover:text-vdsa-accent transition-colors">
                {{ section.title }}
              </h3>
              <p class="text-xs text-vdsa-muted mt-1.5 leading-relaxed">
                {{ section.description }}
              </p>
            </div>
          </div>

          <RouterLink
            :to="typeof section.to === 'string' ? { name: section.to } : section.to"
            :target="section.target || undefined"
            :rel="section.target ? 'noopener' : undefined"
            class="pt-2"
          >
            <Button variant="secondary" size="sm" class="w-full justify-between group-hover:border-vdsa-accent">
              <span>{{ section.actionLabel }}</span>
              <ArrowRight :size="14" class="group-hover:translate-x-1 transition-transform" />
            </Button>
          </RouterLink>
        </article>
      </div>
    </section>

    <!-- ═══ 5. ONBOARDING EMPTY STATE CHO GIẢNG VIÊN MỚI (S10) ═══ -->
    <section
      v-if="!loading && auth.role === 'TEACHER' && myLessonsCount === 0 && myCoursesCount === 0"
      class="p-8 rounded-3xl bg-gradient-to-r from-purple-950/40 via-vdsa-surface to-purple-950/40 border border-purple-500/30 text-center space-y-4 shadow-xl"
    >
      <div class="w-12 h-12 rounded-2xl bg-vdsa-accent/20 text-vdsa-accent flex items-center justify-center mx-auto border border-vdsa-accent/30">
        <Sparkles :size="24" />
      </div>
      <div class="space-y-1.5 max-w-lg mx-auto">
        <h3 class="text-base font-bold text-white">Chào mừng Thầy/Cô đến với Teacher Studio! 🎓</h3>
        <p class="text-xs text-vdsa-muted leading-relaxed">
          Hiện tại Thầy/Cô chưa có lộ trình hoặc bài giảng riêng nào. Hãy bắt đầu tạo lộ trình đầu tiên hoặc kết nối lớp học:
        </p>
      </div>

      <div class="flex flex-wrap items-center justify-center gap-3 pt-2">
        <RouterLink :to="{ path: '/studio', query: { tab: 'curriculum' } }">
          <Button variant="primary" size="sm">
            <Plus :size="14" /> 1. Tạo Lộ trình &amp; Bài giảng mới
          </Button>
        </RouterLink>
        <RouterLink :to="{ name: 'classes' }">
          <Button variant="secondary" size="sm">
            <Users :size="14" /> 2. Mở Lớp học &amp; Gán bài có sẵn
          </Button>
        </RouterLink>
      </div>
    </section>

    <!-- ═══ 6. BÀI HỌC CẬP NHẬT GẦN ĐÂY (S12) ═══ -->
    <section class="p-6 rounded-3xl bg-vdsa-surface border border-vdsa-border space-y-4">
      <div class="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h2 class="text-base font-bold text-white">
            {{ auth.role === 'TEACHER' && myLessonsCount > 0 ? 'Bài học của tôi cập nhật gần đây' : 'Bài học cập nhật gần đây' }}
          </h2>
          <p class="text-xs text-vdsa-muted mt-0.5">Danh sách các bài giảng trong không gian làm việc</p>
        </div>

        <div class="flex items-center gap-2">
          <RouterLink :to="{ name: 'studio-lesson-new' }">
            <Button variant="primary" size="sm">
              <Plus :size="14" /> Soạn bài mới
            </Button>
          </RouterLink>

          <RouterLink :to="{ path: '/studio', query: { tab: 'curriculum' } }">
            <Button variant="ghost" size="sm">
              Xem toàn bộ <ArrowRight :size="13" class="ml-1" />
            </Button>
          </RouterLink>
        </div>
      </div>

      <div v-if="loading" class="space-y-2">
        <Skeleton v-for="i in 3" :key="i" height="52px" />
      </div>

      <div v-else-if="recentLessons.length === 0" class="text-center py-8 text-xs text-vdsa-muted">
        Chưa có bài học nào được tạo. Hãy bấm "Soạn bài mới" để bắt đầu!
      </div>

      <div v-else class="divide-y divide-vdsa-border/60">
        <div
          v-for="l in recentLessons"
          :key="l.id"
          class="py-3 flex items-center justify-between gap-4 hover:bg-vdsa-bg/50 px-2 rounded-xl transition-colors"
        >
          <div class="flex items-center gap-3 min-w-0">
            <span class="w-7 h-7 rounded-lg bg-vdsa-bg border border-vdsa-border font-mono text-xs font-bold text-vdsa-purple-light flex items-center justify-center shrink-0">
              #{{ l.id }}
            </span>
            <div class="min-w-0">
              <div class="flex items-center gap-2">
                <h4 class="text-xs font-bold text-white truncate">{{ l.title }}</h4>
                <Badge v-if="(l as any).isClassOnly" variant="secondary" size="sm" class="text-[10px] py-0">
                  Lớp riêng
                </Badge>
              </div>
              <p class="text-[11px] text-vdsa-muted truncate">{{ l.description || 'Bài học lý thuyết & mô phỏng' }}</p>
            </div>
          </div>

          <div class="flex items-center gap-2 shrink-0">
            <Badge
              :variant="l.status === 'active' ? 'success' : (l.status === 'pendingreview' ? 'warning' : 'muted')"
              size="sm"
            >
              {{ l.status === 'active' ? 'Công khai' : (l.status === 'pendingreview' ? 'Chờ duyệt' : 'Bản nháp') }}
            </Badge>

            <RouterLink
              v-if="(l as any).createdBy === auth.user?.id || auth.role === 'ADMIN'"
              :to="{ path: `/studio/lessons/${l.id}/edit` }"
            >
              <Button variant="ghost" size="sm" title="Chỉnh sửa bài giảng">
                Sửa
              </Button>
            </RouterLink>

            <RouterLink :to="{ name: 'lesson-study', params: { id: l.id } }" target="_blank" rel="noopener">
              <Button variant="ghost" size="sm" title="Xem trước bài học">
                <Eye :size="13" />
              </Button>
            </RouterLink>
          </div>
        </div>
      </div>
    </section>
  </main>
</template>

<style scoped>
.teacher-studio {
  min-height: 100vh;
}
</style>
