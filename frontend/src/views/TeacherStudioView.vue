<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import {
  BookOpen,
  CheckCircle2,
  ClipboardList,
  ExternalLink,
  FlaskConical,
  GraduationCap,
  Layers3,
  MessageSquare,
  Network,
  Plus,
  RefreshCw,
  ShieldAlert,
  Sparkles,
  Users,
  FileCode,
  ArrowRight,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useClassStore } from '@/stores/classStore';
import * as lessonsApi from '@/api/lessons';
import { courseApi, type CourseListDto } from '@/services/courseApi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const auth = useAuthStore();
const classStore = useClassStore();
const router = useRouter();

const loading = ref(true);
const totalLessons = ref(0);
const totalTopics = ref(0);
const totalCourses = ref(0);
const recentLessons = ref<lessonsApi.LessonSummary[]>([]);

const sections = [
  {
    title: 'Lộ trình học (Course Builder)',
    description: 'Thiết kế sơ đồ cây lộ trình DSA: mục tiêu, kết quả đầu ra và thứ tự mở khóa bài học.',
    icon: Network,
    to: 'admin-content',
    badge: 'Cốt lõi',
    actionLabel: 'Mở Course Builder',
    accentColor: 'text-vdsa-purple-light bg-vdsa-purple/10 border-vdsa-purple/20',
  },
  {
    title: 'Chủ đề & Module kiến thức',
    description: 'Tổ chức các nhóm kiến thức: Sắp xếp, Cây, Đồ thị, Quy hoạch động, Stack/Queue...',
    icon: Layers3,
    to: 'admin-content',
    actionLabel: 'Quản lý Chủ đề',
    accentColor: 'text-vdsa-cyan bg-vdsa-cyan/10 border-vdsa-cyan/20',
  },
  {
    title: 'Bài giảng & Mô phỏng tương tác',
    description: 'Soạn thảo giáo án Markdown GFM, hỗ trợ AI DeepSeek định dạng và đính kèm animation trực quan.',
    icon: BookOpen,
    to: 'admin-content',
    badge: 'AI Powered',
    actionLabel: 'Soạn Bài giảng',
    accentColor: 'text-vdsa-green bg-vdsa-green/10 border-vdsa-green/20',
  },
  {
    title: 'Ngân hàng Quiz & Codelab',
    description: 'Tạo trắc nghiệm 4 đáp án, tải file mẫu CSV, bài tập chấm code tự động và lab thực hành.',
    icon: ClipboardList,
    to: 'admin-ladder',
    actionLabel: 'Ngân hàng Bài tập',
    accentColor: 'text-vdsa-yellow bg-vdsa-yellow/10 border-vdsa-yellow/20',
  },
  {
    title: 'Quản lý Lớp học & Thành viên',
    description: 'Tạo lớp học, cấp mã mời học viên, theo dõi bảng xếp hạng và phân phối bài học nội bộ.',
    icon: Users,
    to: 'classes',
    actionLabel: 'Quản lý Lớp học',
    accentColor: 'text-vdsa-red bg-vdsa-red/10 border-vdsa-red/20',
  },
  {
    title: 'Trung tâm Báo cáo & Phản hồi',
    description: 'Theo dõi các thắc mắc bài học, phản hồi góp ý và báo cáo vi phạm từ học viên.',
    icon: ShieldAlert,
    to: 'admin-settings',
    actionLabel: 'Xử lý Báo cáo',
    accentColor: 'text-vdsa-purple-light bg-vdsa-accent/10 border-vdsa-accent/20',
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
      lessonsApi.fetchLessons({ page: 1, pageSize: 8 }),
      lessonsApi.fetchTopics().catch(() => []),
      courseApi.getCourses().catch(() => [] as CourseListDto[]),
      classStore.fetchClasses().catch(() => {}),
    ]);

    totalLessons.value = lessonData.total || lessonData.items.length;
    totalTopics.value = topicTree.length;
    totalCourses.value = courseList.length;
    recentLessons.value = lessonData.items.slice(0, 5);
  } catch (err) {
    // Graceful fallback
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
          <Button variant="secondary" size="sm" :disabled="loading" @click="loadStudioData">
            <RefreshCw :size="14" :class="{ 'animate-spin': loading }" />
          </Button>

          <RouterLink :to="{ name: 'classes' }">
            <Button variant="secondary" size="sm">
              <Users :size="14" /> Lớp của tôi
            </Button>
          </RouterLink>

          <RouterLink :to="{ name: 'admin-content' }">
            <Button variant="primary" size="sm">
              <Plus :size="14" /> Soạn bài giảng mới
            </Button>
          </RouterLink>
        </div>
      </div>
    </header>

    <!-- ═══ 2. KPI METRICS STRIP ═══ -->
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
          <p class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">Lộ trình học</p>
          <h3 class="text-2xl font-black text-white mt-1">
            <span v-if="!loading">{{ totalCourses }}</span>
            <span v-else class="text-lg text-vdsa-disabled">...</span>
          </h3>
        </div>
        <div class="w-12 h-12 rounded-xl bg-vdsa-purple/10 text-vdsa-purple-light flex items-center justify-center">
          <Network :size="22" />
        </div>
      </div>

      <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">Chủ đề kiến thức</p>
          <h3 class="text-2xl font-black text-white mt-1">
            <span v-if="!loading">{{ totalTopics }}</span>
            <span v-else class="text-lg text-vdsa-disabled">...</span>
          </h3>
        </div>
        <div class="w-12 h-12 rounded-xl bg-vdsa-cyan/10 text-vdsa-cyan flex items-center justify-center">
          <Layers3 :size="22" />
        </div>
      </div>

      <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs font-bold text-vdsa-muted uppercase tracking-wider">Bài giảng hệ thống</p>
          <h3 class="text-2xl font-black text-white mt-1">
            <span v-if="!loading">{{ totalLessons }}</span>
            <span v-else class="text-lg text-vdsa-disabled">...</span>
          </h3>
        </div>
        <div class="w-12 h-12 rounded-xl bg-vdsa-green/10 text-vdsa-green flex items-center justify-center">
          <BookOpen :size="22" />
        </div>
      </div>
    </section>

    <!-- ═══ 3. WORKFLOW QUY TRÌNH GIẢNG DẠY ═══ -->
    <section class="p-5 rounded-2xl bg-vdsa-surface/70 border border-vdsa-border/80">
      <div class="flex items-center justify-between gap-4 mb-3">
        <h3 class="text-xs font-extrabold text-white uppercase tracking-wider flex items-center gap-2">
          <Sparkles class="w-4 h-4 text-vdsa-accent" />
          Quy trình đóng gói bài giảng chuẩn DSA Visual
        </h3>
        <span class="text-xs text-vdsa-muted hidden sm:inline">5 bước tuần tự</span>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-5 gap-3 text-xs">
        <div class="p-3 rounded-xl bg-vdsa-bg border border-vdsa-border/70 flex flex-col justify-between space-y-2">
          <div class="font-mono text-vdsa-purple-light font-bold">01. Chủ đề</div>
          <p class="text-vdsa-secondary">Tạo chuyên đề kiến thức (Sorting, Trees, Graphs...)</p>
        </div>

        <div class="p-3 rounded-xl bg-vdsa-bg border border-vdsa-border/70 flex flex-col justify-between space-y-2">
          <div class="font-mono text-vdsa-cyan font-bold">02. Soạn bài &amp; AI</div>
          <p class="text-vdsa-secondary">Soạn lý thuyết GFM Markdown + Tối ưu hóa bằng DeepSeek AI</p>
        </div>

        <div class="p-3 rounded-xl bg-vdsa-bg border border-vdsa-border/70 flex flex-col justify-between space-y-2">
          <div class="font-mono text-vdsa-green font-bold">03. Gắn mô phỏng</div>
          <p class="text-vdsa-secondary">Đính kèm animation visualizer tương tác cho thuật toán</p>
        </div>

        <div class="p-3 rounded-xl bg-vdsa-bg border border-vdsa-border/70 flex flex-col justify-between space-y-2">
          <div class="font-mono text-vdsa-yellow font-bold">04. Quiz &amp; Lab</div>
          <p class="text-vdsa-secondary">Tải CSV ngân hàng câu hỏi hoặc tạo bài thực hành code</p>
        </div>

        <div class="p-3 rounded-xl bg-vdsa-bg border border-vdsa-border/70 flex flex-col justify-between space-y-2">
          <div class="font-mono text-vdsa-red font-bold">05. Giao cho lớp</div>
          <p class="text-vdsa-secondary">Phân phối bài học hoặc ghim vào lộ trình lớp học</p>
        </div>
      </div>
    </section>

    <!-- ═══ 4. CORE STUDIO HUBS GRID ═══ -->
    <section class="space-y-4">
      <div class="flex items-center justify-between">
        <h2 class="text-lg font-black text-white tracking-tight">Phân hệ Quản lý &amp; Biên soạn</h2>
        <span class="text-xs text-vdsa-muted">Chọn phân hệ để làm việc</span>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
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

          <RouterLink :to="{ name: section.to }" class="pt-2">
            <Button variant="secondary" size="sm" class="w-full justify-between group-hover:border-vdsa-accent">
              <span>{{ section.actionLabel }}</span>
              <ArrowRight :size="14" class="group-hover:translate-x-1 transition-transform" />
            </Button>
          </RouterLink>
        </article>
      </div>
    </section>

    <!-- ═══ 5. BÀI HỌC GẦN ĐÂY ═══ -->
    <section class="p-6 rounded-3xl bg-vdsa-surface border border-vdsa-border space-y-4">
      <div class="flex items-center justify-between">
        <div>
          <h2 class="text-base font-bold text-white">Bài học cập nhật gần đây</h2>
          <p class="text-xs text-vdsa-muted mt-0.5">Danh sách các bài học mới nhất trong hệ thống giáo trình</p>
        </div>

        <RouterLink :to="{ name: 'admin-content' }">
          <Button variant="ghost" size="sm">
            Xem toàn bộ <ArrowRight :size="13" class="ml-1" />
          </Button>
        </RouterLink>
      </div>

      <div v-if="loading" class="space-y-2">
        <Skeleton v-for="i in 3" :key="i" height="52px" />
      </div>

      <div v-else-if="recentLessons.length === 0" class="text-center py-6 text-xs text-vdsa-muted">
        Chưa có bài học nào được tạo. Hãy bấm "Soạn bài giảng mới" để bắt đầu!
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
              <h4 class="text-xs font-bold text-white truncate">{{ l.title }}</h4>
              <p class="text-[11px] text-vdsa-muted truncate">{{ l.description || 'Bài học lý thuyết & mô phỏng' }}</p>
            </div>
          </div>

          <div class="flex items-center gap-2 shrink-0">
            <Badge :variant="l.status === 'active' ? 'success' : 'muted'" size="sm">
              {{ l.status === 'active' ? 'Công khai' : 'Bản nháp' }}
            </Badge>
            <RouterLink :to="{ name: 'lesson-study', params: { lessonId: l.id } }" target="_blank">
              <Button variant="ghost" size="sm" title="Xem trước bài học">
                <ExternalLink :size="13" />
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
