<script setup lang="ts">
import { computed, onMounted, onActivated, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  ArrowRight,
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
  Eye,
  Edit,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import * as lessonsApi from '@/api/lessons';
import { courseApi, type CourseListDto } from '@/services/courseApi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Card from '@/components/ui/Card.vue';
const emit = defineEmits<{
  (e: 'switchTab', tab: 'curriculum' | 'feedback'): void;
}>();

const auth = useAuthStore();
const ui = useUiStore();
const router = useRouter();

const loading = ref(true);
const totalLessons = ref(0);
const totalTopics = ref(0);
const totalCourses = ref(0);
const myLessonsCount = ref(0);
const myTopicsCount = ref(0);
const myCoursesCount = ref(0);
interface HubSection {
  title: string;
  description: string;
  icon: any;
  tab?: 'curriculum' | 'feedback';
  to?: { name: string } | string;
  badge?: string;
  actionLabel: string;
  accentColor: string;
}

interface WorkflowStepItem {
  step: string;
  title: string;
  desc: string;
  tab?: 'curriculum' | 'feedback';
  to?: { name: string } | string;
  btnText: string;
}

// 3 Hubs (Tạm thời ẩn phần Lớp học)
const sections: HubSection[] = [
  {
    title: 'Giáo trình & Bài giảng',
    description: 'Thiết kế giáo trình học, tổ chức cây Chương - Bài giảng, biên soạn Markdown GFM và gắn mô phỏng trực quan.',
    icon: Network,
    tab: 'curriculum',
    badge: 'Cốt lõi',
    actionLabel: 'Mở Studio biên soạn',
    accentColor: 'text-vdsa-purple-light bg-vdsa-purple/10 border-vdsa-purple/20',
  },
  {
    title: 'Quiz & Codelab trong lộ trình',
    description: 'Thêm Quiz trắc nghiệm và Lab chấm code tự động vào cây lộ trình — mỗi item một mục trên cây.',
    icon: ClipboardList,
    tab: 'curriculum',
    actionLabel: 'Mở cây lộ trình',
    accentColor: 'text-vdsa-yellow bg-vdsa-yellow/10 border-vdsa-yellow/20',
  },
  {
    title: 'Trung tâm Báo cáo & Phản hồi',
    description: 'Theo dõi các thắc mắc bài học, phản hồi góp ý trực tiếp và xử lý báo cáo từ học viên.',
    icon: ShieldAlert,
    tab: 'feedback',
    actionLabel: 'Xử lý Báo cáo',
    accentColor: 'text-vdsa-purple-light bg-vdsa-accent/10 border-vdsa-accent/20',
  },
];

// Workflow steps (4 bước chuẩn)
const workflowSteps: WorkflowStepItem[] = [
  {
    step: '01',
    title: 'Chủ đề',
    desc: 'Tạo chuyên đề kiến thức (Sorting, Trees, Graphs...)',
    tab: 'curriculum',
    btnText: 'Tạo chủ đề',
  },
  {
    step: '02',
    title: 'Soạn bài',
    desc: 'Soạn lý thuyết GFM Markdown + định dạng trực quan ngay trong panel cây lộ trình',
    tab: 'curriculum',
    btnText: 'Mở cây lộ trình',
  },
  {
    step: '03',
    title: 'Gắn mô phỏng',
    desc: 'Đính kèm visualizer tương tác cho thuật toán',
    to: { name: 'simulations' },
    btnText: 'Thư viện',
  },
  {
    step: '04',
    title: 'Quiz & Lab',
    desc: 'Thêm quiz 4 đáp án và lab chấm code tự động vào cây lộ trình',
    tab: 'curriculum',
    btnText: 'Thêm quiz/lab',
  },
];

async function loadStudioData(): Promise<void> {
  loading.value = true;
  try {
    const [lessonData, topicTree, courseList] = await Promise.all([
      lessonsApi.fetchLessons({ page: 1, pageSize: 1000 }),
      lessonsApi.fetchTopics().catch(() => [] as lessonsApi.Topic[]),
      courseApi.getCourses().catch(() => [] as CourseListDto[]),
    ]);

    const currentUserId = auth.user?.id;

    // Lọc giáo trình thuộc quyền của giảng viên (hoặc admin)
    const myCourses = courseList.filter(
      (c) => auth.role === 'ADMIN' || !currentUserId || (c as any).createdBy === currentUserId || (c as any).authorId === currentUserId || (c as any).createdBy == null || (c as any).createdBy <= 1,
    );
    myCoursesCount.value = myCourses.length;

    // Lọc chủ đề kiến thức DSA chính thức (loại bỏ các module con)
    const knowledgeTopics = topicTree.filter((t) => !t.name.startsWith('Module '));
    myTopicsCount.value = knowledgeTopics.length;

    // Lọc bài học thuộc quyền của giảng viên
    const myLessons = lessonData.items.filter(
      (l) => auth.role === 'ADMIN' || !currentUserId || (l as any).createdBy === currentUserId || (l as any).authorId === currentUserId || (l as any).createdBy == null || (l as any).createdBy <= 1,
    );
    myLessonsCount.value = (auth.role === 'ADMIN' && (lessonData.total ?? 0) > 0) ? lessonData.total : myLessons.length;
  } catch {
    ui.showToast('Không thể tải một số dữ liệu tổng quan.', 'warning');
  } finally {
    loading.value = false;
  }
}

function handleStepClick(st: WorkflowStepItem): void {
  if (st.step === '01') {
    void router.push({ path: '/studio', query: { tab: 'curriculum', openTopics: '1' } });
    emit('switchTab', 'curriculum');
    return;
  }
  if (st.tab) {
    emit('switchTab', st.tab);
  } else if (st.to) {
    void router.push(st.to);
  }
}

onMounted(() => {
  void loadStudioData();
  window.addEventListener('focus', loadStudioData);
});

onActivated(() => {
  void loadStudioData();
});
</script>

<template>
  <div class="studio-overview space-y-6">
    <!-- Hero Banner -->
    <header class="p-6 md:p-8 rounded-3xl bg-vdsa-surface border border-vdsa-border relative overflow-hidden">
      <div class="absolute -right-20 -top-20 w-80 h-80 bg-vdsa-accent/10 rounded-full blur-3xl pointer-events-none"></div>

      <div class="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-6 relative z-10">
        <div class="space-y-2">
          <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-vdsa-accent/15 border border-vdsa-accent/30 text-vdsa-purple-light text-xs font-bold font-mono uppercase tracking-wider">
            <GraduationCap :size="14" /> Trung Tâm Điều Hành Studio
          </div>
          <h1 class="text-2xl sm:text-3xl font-black text-white tracking-tight flex items-center gap-2 flex-wrap">
            Xin chào, {{ auth.user?.displayName || 'Thầy/Cô' }}!
          </h1>
          <p class="text-sm text-vdsa-muted max-w-2xl leading-relaxed">
            Biên soạn bài giảng trực quan, đính kèm thuật toán mô phỏng tương tác, tổ chức giáo trình đào tạo và kiểm soát chất lượng giảng dạy DSA.
          </p>
        </div>

        <div class="flex flex-wrap items-center gap-2.5 shrink-0">
          <Button variant="secondary" size="sm" :disabled="loading" @click="loadStudioData" title="Làm mới dữ liệu">
            <RefreshCw :size="14" :class="{ 'animate-spin': loading }" /> Làm mới
          </Button>
          <Button variant="primary" size="sm" class="gap-1.5 font-bold" @click="router.push({ path: '/studio', query: { tab: 'curriculum' } })">
            <Network :size="15" /> Tạo lộ trình mới
          </Button>
        </div>
      </div>
    </header>

    <!-- 3 KPI Cards (Hiển thị tài nguyên của tôi) -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <Card class="p-4 bg-vdsa-surface border border-vdsa-border rounded-2xl flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-purple-500/15 text-purple-300 flex items-center justify-center font-bold">
          <BookOpen :size="22" />
        </div>
        <div>
          <p class="text-[11px] font-bold text-vdsa-muted uppercase">Tổng bài học</p>
          <h3 class="text-xl font-black text-white mt-0.5">{{ myLessonsCount }}</h3>
        </div>
      </Card>

      <Card class="p-4 bg-vdsa-surface border border-vdsa-border rounded-2xl flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-sky-500/15 text-sky-300 flex items-center justify-center font-bold">
          <Network :size="22" />
        </div>
        <div>
          <p class="text-[11px] font-bold text-vdsa-muted uppercase">Chương / Topic</p>
          <h3 class="text-xl font-black text-white mt-0.5">{{ myTopicsCount }}</h3>
        </div>
      </Card>

      <Card class="p-4 bg-vdsa-surface border border-vdsa-border rounded-2xl flex items-center gap-4">
        <div class="w-12 h-12 rounded-xl bg-emerald-500/15 text-emerald-300 flex items-center justify-center font-bold">
          <Layers3 :size="22" />
        </div>
        <div>
          <p class="text-[11px] font-bold text-vdsa-muted uppercase">Giáo trình (Course)</p>
          <h3 class="text-xl font-black text-white mt-0.5">{{ myCoursesCount }}</h3>
        </div>
      </Card>
    </div>

    <!-- 3 Hubs -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <div
        v-for="hub in sections"
        :key="hub.title"
        class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border hover:border-vdsa-accent/50 transition-all flex flex-col justify-between gap-4"
      >
        <div class="space-y-2">
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <span class="p-2.5 rounded-xl border" :class="hub.accentColor">
                <component :is="hub.icon" :size="20" />
              </span>
              <h3 class="text-base font-bold text-white">{{ hub.title }}</h3>
            </div>
            <Badge v-if="hub.badge" variant="primary" class="text-[10px] uppercase font-mono">{{ hub.badge }}</Badge>
          </div>
          <p class="text-xs text-vdsa-muted leading-relaxed">{{ hub.description }}</p>
        </div>

        <div>
          <Button
            v-if="hub.tab"
            variant="secondary"
            size="sm"
            class="text-xs gap-1.5"
            @click="emit('switchTab', hub.tab)"
          >
            {{ hub.actionLabel }} <ArrowRight :size="13" />
          </Button>
          <Button
            v-else-if="hub.to"
            variant="secondary"
            size="sm"
            class="text-xs gap-1.5"
            @click="router.push(hub.to)"
          >
            {{ hub.actionLabel }} <ArrowRight :size="13" />
          </Button>
        </div>
      </div>
    </div>

    <!-- 4-Step Pipeline Workflow with dashed connector -->
    <div class="p-6 rounded-3xl bg-vdsa-surface border border-vdsa-border space-y-4">
      <div class="flex items-center justify-between">
        <h3 class="text-sm font-extrabold uppercase tracking-wider text-white flex items-center gap-2">
          <Sparkles :size="16" class="text-amber-400" /> Quy trình 4 bước biên soạn & giảng dạy
        </h3>
        <span class="text-xs text-vdsa-muted">Chuẩn hóa theo Quyết định D0</span>
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3 relative">
        <div
          v-for="(st, idx) in workflowSteps"
          :key="st.step"
          class="p-4 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border flex flex-col justify-between gap-3 relative"
        >
          <div>
            <span class="text-xs font-black font-mono text-purple-400">#{{ st.step }}</span>
            <h4 class="text-xs font-bold text-white mt-1">{{ st.title }}</h4>
            <p class="text-[11px] text-vdsa-muted mt-1 leading-relaxed">{{ st.desc }}</p>
          </div>
          <Button
            variant="ghost"
            size="sm"
            class="text-xs h-7 w-full justify-center text-purple-300 hover:text-white"
            @click="handleStepClick(st)"
          >
            {{ st.btnText }} →
          </Button>
        </div>
      </div>
    </div>
  </div>
</template>
