<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  AlertCircle,
  BookOpen,
  Check,
  CheckCircle2,
  Clock,
  Eye,
  ExternalLink,
  Layers,
  Network,
  RefreshCw,
  Search,
  ShieldCheck,
  User,
  X,
  XCircle,
} from 'lucide-vue-next';

import { courseApi, type CourseListDto, type CourseDetailDto } from '@/services/courseApi';
import { fetchPathTree, fetchItemDetail, type PathItemDto, normalizeItemType } from '@/api/pathItems';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import { normalizeVi } from '@/utils/searchNormalize';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Modal from '@/components/ui/Modal.vue';
import Drawer from '@/components/ui/Drawer.vue';
import ProseContent from '@/components/ui/ProseContent.vue';

const ui = useUiStore();
const router = useRouter();

function openDirectCurriculum(course: CourseListDto): void {
  void router.push({ path: '/studio', query: { tab: 'curriculum', courseId: course.id } });
}

const pendingCourses = ref<CourseListDto[]>([]);
const loading = ref(true);
const searchQuery = ref('');
const categoryFilter = ref('');

// Phê duyệt / Từ chối
const reviewingId = ref<string | number | null>(null);
const rejectModalOpen = ref(false);
const targetRejectCourse = ref<CourseListDto | null>(null);
const rejectReason = ref('');
const rejectSubmitting = ref(false);

// Xem trước cây giáo trình
const previewDrawerOpen = ref(false);
const previewLoading = ref(false);
const previewCourse = ref<CourseDetailDto | null>(null);
const previewTree = ref<PathItemDto[]>([]);

interface FlattenedNode {
  item: PathItemDto;
  depth: number;
}

function flattenTree(items: PathItemDto[], depth = 0): FlattenedNode[] {
  const res: FlattenedNode[] = [];
  for (const it of items) {
    res.push({ item: it, depth });
    if (it.children && it.children.length > 0) {
      res.push(...flattenTree(it.children, depth + 1));
    }
  }
  return res;
}

const flattenedPreviewTree = computed(() => {
  return flattenTree(previewTree.value);
});

onMounted(loadPendingCourses);

async function loadPendingCourses(): Promise<void> {
  loading.value = true;
  try {
    pendingCourses.value = await courseApi.getPendingCourses();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tải danh sách lộ trình chờ duyệt.', 'error');
    pendingCourses.value = [];
  } finally {
    loading.value = false;
  }
}

const filteredCourses = computed(() => {
  let list = pendingCourses.value;
  if (categoryFilter.value) {
    list = list.filter((c) => c.category === categoryFilter.value);
  }
  if (searchQuery.value.trim()) {
    const q = normalizeVi(searchQuery.value);
    list = list.filter(
      (c) =>
        normalizeVi(c.title || '').includes(q) ||
        normalizeVi(c.authorName || '').includes(q) ||
        normalizeVi(c.description || '').includes(q),
    );
  }
  return list;
});

async function handleApprove(course: CourseListDto): Promise<void> {
  if (reviewingId.value != null) return;
  reviewingId.value = course.id;
  try {
    await courseApi.reviewCourse(course.id, { approve: true });
    ui.showToast(`Đã phê duyệt và xuất bản lộ trình "${course.title}" thành công!`, 'success');
    if (previewDrawerOpen.value && previewCourse.value?.id === course.id) {
      previewDrawerOpen.value = false;
    }
    await loadPendingCourses();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Phê duyệt lộ trình thất bại.', 'error');
  } finally {
    reviewingId.value = null;
  }
}

function openRejectModal(course: CourseListDto): void {
  targetRejectCourse.value = course;
  rejectReason.value = '';
  rejectModalOpen.value = true;
}

async function handleConfirmReject(): Promise<void> {
  if (!targetRejectCourse.value) return;
  if (!rejectReason.value.trim()) {
    ui.showToast('Vui lòng nhập lý do từ chối để giáo viên chỉnh sửa.', 'warning');
    return;
  }

  rejectSubmitting.value = true;
  try {
    await courseApi.reviewCourse(targetRejectCourse.value.id, {
      approve: false,
      reason: rejectReason.value.trim(),
    });
    ui.showToast(`Đã từ chối lộ trình "${targetRejectCourse.value.title}".`, 'info');
    rejectModalOpen.value = false;
    targetRejectCourse.value = null;
    if (previewDrawerOpen.value) {
      previewDrawerOpen.value = false;
    }
    await loadPendingCourses();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Từ chối lộ trình thất bại.', 'error');
  } finally {
    rejectSubmitting.value = false;
  }
}

async function openPreview(course: CourseListDto): Promise<void> {
  previewDrawerOpen.value = true;
  previewLoading.value = true;
  previewCourse.value = null;
  previewTree.value = [];
  try {
    const [detail, tree] = await Promise.all([
      courseApi.getCourseById(course.id),
      fetchPathTree(Number(course.id)).catch(() => [] as PathItemDto[]),
    ]);
    previewCourse.value = detail;
    previewTree.value = tree;
  } catch (err) {
    ui.showToast('Không thể tải chi tiết giáo trình để xem trước.', 'error');
  } finally {
    previewLoading.value = false;
  }
}

// Xem trước chi tiết từng bài học / node
const selectedNodeDetail = ref<PathItemDto | null>(null);
const nodeDetailLoading = ref(false);
const nodeDetailModalOpen = ref(false);

async function openNodeDetail(item: PathItemDto): Promise<void> {
  selectedNodeDetail.value = item;
  nodeDetailModalOpen.value = true;
  nodeDetailLoading.value = true;
  try {
    const full = await fetchItemDetail(item.id);
    selectedNodeDetail.value = full;
  } catch {
    // Giữ thông tin cơ bản hiện có nếu không fetch được thêm
  } finally {
    nodeDetailLoading.value = false;
  }
}
</script>

<template>
  <div class="space-y-6">
    <!-- Hero Header -->
    <header class="p-4 md:p-6 bg-[#131120] border border-[#27253b] rounded-2xl flex flex-col sm:flex-row sm:items-center justify-between gap-4 shadow-xl relative overflow-hidden">
      <div class="absolute -right-10 -bottom-10 w-48 h-48 bg-purple-600/10 rounded-full blur-3xl pointer-events-none" />
      
      <div class="space-y-1 relative z-10">
        <div class="flex items-center gap-2">
          <Badge variant="primary" class="gap-1 bg-purple-500/20 text-purple-300 border-purple-500/30">
            <ShieldCheck :size="13" /> Kiểm duyệt Nội dung
          </Badge>
          <span class="text-xs text-slate-400 font-bold">DSA Content Moderation</span>
        </div>
        <h1 class="text-lg md:text-xl font-black text-white tracking-wide flex items-center gap-2">
          Duyệt Giáo trình & Lộ trình
        </h1>
        <p class="text-xs text-slate-400 max-w-xl">
          Kiểm tra chất lượng bài giảng, mô phỏng và bài tập do Giảng viên biên soạn trước khi xuất bản ra toàn nền tảng.
        </p>
      </div>

      <div class="flex items-center gap-2 shrink-0 relative z-10">
        <Button variant="secondary" size="sm" :loading="loading" class="gap-1.5" @click="loadPendingCourses">
          <RefreshCw :size="14" /> Làm mới
        </Button>
      </div>
    </header>

    <!-- Toolbar Filters -->
    <div class="flex flex-col sm:flex-row gap-3">
      <div class="relative flex-1">
        <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
        <input
          v-model="searchQuery"
          type="search"
          placeholder="Tìm theo tên lộ trình, tác giả hoặc mô tả..."
          class="w-full pl-9 pr-4 py-2 text-xs bg-[#131120] border border-[#27253b] rounded-xl text-white placeholder-slate-500 focus:outline-none focus:border-purple-500 transition-colors"
        />
      </div>

      <select
        v-model="categoryFilter"
        class="px-3 py-2 text-xs font-bold bg-[#131120] border border-[#27253b] rounded-xl text-slate-200 focus:outline-none focus:border-purple-500 cursor-pointer shrink-0"
      >
        <option value="">Tất cả danh mục</option>
        <option value="Cấu trúc dữ liệu">Cấu trúc dữ liệu</option>
        <option value="Giải thuật">Giải thuật</option>
        <option value="Sắp xếp & Tìm kiếm">Sắp xếp & Tìm kiếm</option>
        <option value="Cây & Bảng băm">Cây & Bảng băm</option>
        <option value="Đồ thị">Đồ thị</option>
      </select>
    </div>

    <!-- Loading Skeleton -->
    <div v-if="loading" class="space-y-4">
      <Skeleton v-for="i in 3" :key="i" height="120px" class="rounded-2xl" />
    </div>

    <!-- Empty State -->
    <EmptyState
      v-else-if="filteredCourses.length === 0"
      icon="check-circle"
      title="Không có lộ trình nào đang chờ duyệt"
      description="Tất cả lộ trình từ giảng viên đã được xử lý hoặc chưa có yêu cầu xuất bản mới."
    />

    <!-- Course Moderation Grid / List -->
    <div v-else class="grid grid-cols-1 gap-4">
      <article
        v-for="course in filteredCourses"
        :key="course.id"
        class="p-4 md:p-5 bg-[#131120] border border-[#27253b] hover:border-purple-500/50 rounded-2xl transition-all shadow-md space-y-4"
      >
        <div class="flex flex-col md:flex-row md:items-center justify-between gap-3">
          <div class="space-y-1.5 min-w-0">
            <div class="flex items-center gap-2 flex-wrap">
              <span class="px-2 py-0.5 rounded-full text-[10px] font-black bg-amber-500/20 text-amber-300 border border-amber-500/30">
                ⏳ Chờ duyệt
              </span>
              <span v-if="course.category" class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-purple-500/10 text-purple-300 border border-purple-500/20">
                {{ course.category }}
              </span>
              <span v-if="course.difficulty" class="px-2 py-0.5 rounded-full text-[10px] font-bold bg-slate-800 text-slate-300 border border-slate-700">
                {{ course.difficulty }}
              </span>
            </div>
            
            <h2 class="text-base font-black text-white truncate">
              {{ course.title }}
            </h2>

            <p class="text-xs text-slate-400 line-clamp-2 leading-relaxed">
              {{ course.description || 'Chưa có mô tả tóm tắt cho lộ trình này.' }}
            </p>
          </div>

          <!-- Meta badges & Actions -->
          <div class="flex flex-row md:flex-col items-end justify-between md:justify-center gap-3 shrink-0 pt-2 md:pt-0 border-t md:border-t-0 border-[#222033]">
            <div class="text-right space-y-1">
              <div class="text-xs font-bold text-slate-300 flex items-center gap-1.5 justify-end">
                <User :size="13" class="text-purple-400" />
                <span>{{ course.authorName || 'Giảng viên' }}</span>
              </div>
              <div class="text-[11px] text-slate-500 flex items-center gap-1.5 justify-end">
                <Clock :size="12" />
                <span>{{ course.submittedAt ? formatDate(course.submittedAt) : 'Vừa gửi' }}</span>
              </div>
            </div>

            <div class="flex items-center gap-2">
              <Button
                size="sm"
                variant="secondary"
                class="text-xs font-bold gap-1"
                title="Mở trực tiếp toàn bộ giáo trình trong Studio để duyệt chi tiết"
                @click="openDirectCurriculum(course)"
              >
                <ExternalLink :size="13" /> Xem giáo trình
              </Button>
              
              <Button
                size="sm"
                variant="danger"
                class="text-xs font-bold gap-1"
                @click="openRejectModal(course)"
              >
                <X :size="13" /> Từ chối
              </Button>

              <Button
                size="sm"
                variant="primary"
                class="text-xs font-bold gap-1 bg-emerald-600 hover:bg-emerald-500 text-white border-emerald-500"
                :loading="reviewingId === course.id"
                @click="handleApprove(course)"
              >
                <Check :size="13" /> Phê duyệt
              </Button>
            </div>
          </div>
        </div>

        <!-- Quick Stats Strip -->
        <div class="pt-3 border-t border-[#1f1d2e] flex items-center gap-4 text-xs font-bold text-slate-400 flex-wrap">
          <div class="flex items-center gap-1.5">
            <BookOpen :size="14" class="text-purple-400" />
            <span>{{ course.totalLessons || 0 }} bài học</span>
          </div>
          <div class="flex items-center gap-1.5">
            <span class="text-amber-400">⚡</span>
            <span>+{{ course.xpReward || 0 }} XP</span>
          </div>
        </div>
      </article>
    </div>

    <!-- Reject Reason Modal -->
    <Modal
      :open="rejectModalOpen"
      title="Từ chối xuất bản Lộ trình"
      @close="rejectModalOpen = false"
    >
      <div class="space-y-4">
        <div class="p-3 bg-amber-500/10 border border-amber-500/20 rounded-xl text-xs text-amber-300 flex items-start gap-2">
          <AlertCircle :size="16" class="shrink-0 mt-0.5" />
          <span>
            Vui lòng giải thích rõ lý do từ chối để tác giả <strong>{{ targetRejectCourse?.authorName || 'Giảng viên' }}</strong> biết và bổ sung, cập nhật lại nội dung.
          </span>
        </div>

        <div class="space-y-1.5">
          <label class="text-xs font-bold text-slate-300">Lý do từ chối *</label>
          <textarea
            v-model="rejectReason"
            rows="4"
            placeholder="Ví dụ: Thiếu bài tập thực hành cho Chương 2; Cần đính kèm mô phỏng trực quan cho bài Bubble Sort..."
            class="w-full p-3 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-xl text-white placeholder-slate-500 focus:outline-none focus:border-purple-500"
          />
        </div>

        <div class="flex items-center justify-end gap-2 pt-2">
          <Button variant="secondary" size="sm" @click="rejectModalOpen = false">
            Hủy
          </Button>
          <Button
            variant="danger"
            size="sm"
            :loading="rejectSubmitting"
            @click="handleConfirmReject"
          >
            Xác nhận Từ chối
          </Button>
        </div>
      </div>
    </Modal>

    <!-- Preview Curriculum Drawer -->
    <Drawer
      :open="previewDrawerOpen"
      title="Chi tiết & Cây Giáo trình"
      size="lg"
      @close="previewDrawerOpen = false"
    >
      <div v-if="previewLoading" class="p-6 space-y-4">
        <Skeleton v-for="i in 5" :key="i" height="40px" class="rounded-lg" />
      </div>

      <div v-else-if="previewCourse" class="space-y-6 pb-6">
        <!-- Header info -->
        <div class="p-4 bg-[#171527] border border-[#27253b] rounded-xl space-y-2">
          <h2 class="text-base font-black text-white">{{ previewCourse.title }}</h2>
          <p class="text-xs text-slate-300 leading-relaxed">{{ previewCourse.description }}</p>
          <div class="flex items-center gap-2 pt-2 text-xs font-bold text-slate-400">
            <Badge variant="primary">{{ previewCourse.category }}</Badge>
            <Badge variant="muted">{{ previewCourse.difficulty }}</Badge>
            <span>Tác giả: <strong class="text-white">{{ previewCourse.authorName }}</strong></span>
          </div>
        </div>

        <!-- Curriculum Tree Nodes -->
        <div class="space-y-2">
          <h3 class="text-xs font-bold uppercase tracking-wider text-slate-400 flex items-center gap-1.5">
            <Layers :size="14" class="text-purple-400" />
            <span>Cấu trúc Cây Giáo trình ({{ flattenedPreviewTree.length }} mục)</span>
          </h3>

          <div v-if="flattenedPreviewTree.length === 0" class="p-4 bg-[#12111d] rounded-xl text-xs text-slate-500 text-center italic">
            Chưa có mục nội dung nào trong cây giáo trình.
          </div>

          <div v-else class="space-y-1.5">
            <div
              v-for="{ item, depth } in flattenedPreviewTree"
              :key="item.id"
              class="p-3 bg-[#12111d] hover:bg-[#1a1829] border border-[#222033] hover:border-purple-500/40 rounded-xl flex items-center justify-between gap-3 text-xs transition-colors cursor-pointer group"
              :style="{ marginLeft: `${depth * 18}px` }"
              @click="openNodeDetail(item)"
            >
              <div class="flex items-center gap-2 min-w-0">
                <span
                  class="p-1.5 rounded-lg text-xs"
                  :class="item.itemType === 'folder' ? 'bg-amber-500/20 text-amber-300' : item.itemType === 'quiz' ? 'bg-orange-500/20 text-orange-300' : 'bg-sky-500/20 text-sky-300'"
                >
                  <Network v-if="item.itemType === 'folder'" :size="14" />
                  <BookOpen v-else-if="item.itemType === 'theory'" :size="14" />
                  <Layers v-else :size="14" />
                </span>
                <span class="font-bold text-white group-hover:text-purple-300 transition-colors truncate">
                  <span v-if="depth > 0" class="text-slate-500 font-mono mr-1">└─</span>
                  {{ item.title }}
                </span>
              </div>
              <div class="flex items-center gap-2 shrink-0">
                <span class="text-[10px] font-mono text-slate-500 uppercase">{{ item.itemType }}</span>
                <button
                  type="button"
                  class="px-2 py-0.5 rounded bg-purple-600/20 text-purple-300 hover:bg-purple-600/40 text-[10px] font-bold flex items-center gap-1 cursor-pointer"
                >
                  <Eye :size="11" /> Xem
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Bottom Action Bar -->
        <div class="pt-4 border-t border-[#222033] flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="danger"
            @click="openRejectModal(previewCourse as any)"
          >
            <X :size="13" /> Từ chối
          </Button>
          <Button
            size="sm"
            variant="primary"
            class="bg-emerald-600 hover:bg-emerald-500 text-white border-emerald-500"
            :loading="reviewingId === previewCourse.id"
            @click="handleApprove(previewCourse as any)"
          >
            <Check :size="13" /> Phê duyệt xuất bản
          </Button>
        </div>
      </div>
    </Drawer>

    <!-- Modal Xem trước chi tiết Bài học / Node -->
    <Modal
      :open="nodeDetailModalOpen"
      :title="'Chi tiết: ' + (selectedNodeDetail?.title || 'Bài học')"
      @close="nodeDetailModalOpen = false"
    >
      <div v-if="nodeDetailLoading" class="p-6 space-y-3">
        <Skeleton height="30px" class="rounded-lg" />
        <Skeleton height="150px" class="rounded-lg" />
      </div>

      <div v-else-if="selectedNodeDetail" class="space-y-4 max-h-[70vh] overflow-y-auto pr-1 text-xs">
        <div class="p-3 bg-[#171624] border border-[#27253b] rounded-xl flex items-center justify-between gap-3">
          <div>
            <span class="text-[10px] font-mono text-purple-400 font-bold uppercase">Loại mục: {{ selectedNodeDetail.itemType }}</span>
            <h3 class="text-sm font-black text-white mt-0.5">{{ selectedNodeDetail.title }}</h3>
            <p v-if="selectedNodeDetail.description" class="text-slate-400 text-[11px] mt-1">{{ selectedNodeDetail.description }}</p>
          </div>
          <Badge variant="primary" class="shrink-0 text-[10px]">ID: #{{ selectedNodeDetail.id }}</Badge>
        </div>

        <!-- Theory: Markdown / HTML content -->
        <div v-if="selectedNodeDetail.itemType === 'theory' || selectedNodeDetail.lesson" class="space-y-3">
          <div v-if="selectedNodeDetail.lesson?.simulations?.length" class="p-2.5 bg-sky-500/10 border border-sky-500/20 rounded-xl space-y-1">
            <span class="text-[10px] font-bold text-sky-300 uppercase tracking-wider">Mô phỏng đính kèm:</span>
            <div class="flex flex-wrap gap-1.5 pt-1">
              <span
                v-for="s in selectedNodeDetail.lesson.simulations"
                :key="s.simulationKey"
                class="px-2 py-0.5 rounded bg-sky-500/20 text-sky-200 text-[11px] font-mono"
              >
                🎮 {{ s.title || s.simulationKey }}
              </span>
            </div>
          </div>

          <div class="p-4 bg-[#0e0d16] border border-[#262438] rounded-xl">
            <h4 class="text-[11px] font-bold uppercase tracking-wider text-slate-400 mb-2">Nội dung bài học</h4>
            <ProseContent :content-html="selectedNodeDetail.lesson?.contentHtml || selectedNodeDetail.description || 'Chưa có nội dung chi tiết.'" />
          </div>
        </div>

        <!-- Quiz: Questions -->
        <div v-else-if="selectedNodeDetail.itemType === 'quiz' || selectedNodeDetail.exercise?.type === 'MCQ'" class="space-y-3">
          <div class="flex items-center justify-between text-[11px] font-bold text-slate-400">
            <span>Danh sách câu hỏi trắc nghiệm</span>
            <span>Tổng điểm: {{ selectedNodeDetail.exercise?.maxScore ?? 100 }}</span>
          </div>

          <div v-if="!selectedNodeDetail.exercise?.questions?.length" class="p-4 text-center text-slate-500 bg-[#0e0d16] rounded-xl italic">
            Chưa có câu hỏi trắc nghiệm nào.
          </div>

          <div
            v-for="(q, idx) in (selectedNodeDetail.exercise?.questions || [])"
            :key="idx"
            class="p-3 bg-[#0e0d16] border border-[#262438] rounded-xl space-y-2"
          >
            <div class="flex items-start justify-between gap-2 font-bold text-white text-xs">
              <span>Câu {{ idx + 1 }}: {{ q.content }}</span>
              <span class="text-[10px] text-amber-400 shrink-0 font-mono">{{ q.points || 1 }}đ</span>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-1.5 pt-1">
              <div
                v-for="(opt, optIdx) in q.options"
                :key="optIdx"
                class="p-2 rounded-lg text-[11px] border"
                :class="(q as any).correctIndices?.includes(optIdx) || (q as any).correctIndex === optIdx ? 'bg-emerald-500/15 border-emerald-500/40 text-emerald-300 font-bold' : 'bg-white/5 border-transparent text-slate-400'"
              >
                <span class="font-mono mr-1">[{{ String.fromCharCode(65 + optIdx) }}]</span>
                {{ opt }}
              </div>
            </div>

            <p v-if="(q as any).explanation" class="text-[10px] text-slate-400 bg-white/5 p-2 rounded-lg italic">
              💡 Giải thích: {{ (q as any).explanation }}
            </p>
          </div>
        </div>

        <!-- Lab: Code & Test Cases -->
        <div v-else-if="selectedNodeDetail.itemType === 'lab' || selectedNodeDetail.exercise?.type === 'CODE'" class="space-y-3">
          <div class="p-3 bg-[#0e0d16] border border-[#262438] rounded-xl space-y-1.5">
            <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Mã nguồn khởi tạo (Starter Code):</span>
            <pre class="p-2.5 rounded-lg bg-black/50 text-slate-200 font-mono text-[11px] overflow-x-auto">{{ (selectedNodeDetail.exercise as any)?.configJson ? JSON.parse((selectedNodeDetail.exercise as any).configJson)?.starterCode || '' : '' }}</pre>
          </div>
        </div>

        <!-- Folder -->
        <div v-else-if="selectedNodeDetail.itemType === 'folder'" class="p-4 bg-[#0e0d16] border border-[#262438] rounded-xl text-center text-slate-400">
          📁 Chương (Module) chứa các bài học con.
        </div>
      </div>

      <template #footer>
        <Button variant="secondary" size="sm" @click="nodeDetailModalOpen = false">Đóng</Button>
      </template>
    </Modal>
  </div>
</template>
