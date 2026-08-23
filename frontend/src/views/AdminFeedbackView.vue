<script setup lang="ts">
import { onMounted, ref, computed } from 'vue';
import {
  MessageSquare,
  CheckCircle2,
  AlertCircle,
  Clock,
  Sparkles,
  Search,
  Filter,
  Send,
  Check,
  RefreshCw,
  User,
  Inbox,
} from 'lucide-vue-next';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import AdminNav from '@/components/admin/AdminNav.vue';
import { courseApi, type CourseFeedbackDto } from '@/services/courseApi';
import { useUiStore } from '@/stores/ui';

const ui = useUiStore();

const loading = ref(true);
const error = ref('');
const courses = ref<Array<{ id: string; title: string }>>([]);
const courseId = ref<number | null>(null);
const statusFilter = ref('');
const searchQuery = ref('');
const items = ref<CourseFeedbackDto[]>([]);

// Reply editor state
const replyDraft = ref<Record<number, { text: string; status: string; saving: boolean; saved: boolean }>>({});

const STAT_FILTERS = [
  { value: '', label: 'Tất cả' },
  { value: 'New', label: 'Mới' },
  { value: 'Read', label: 'Đã đọc' },
  { value: 'Resolved', label: 'Đã xử lý' },
];

const typeLabel = (t: string) => (t === 'Bug' ? 'Báo lỗi' : t === 'Request' ? 'Đề xuất nội dung' : 'Góp ý');
const statusLabel = (s: string) => (s === 'Read' ? 'Đã đọc' : s === 'Resolved' ? 'Đã xử lý' : 'Mới');

const typeVariant = (t: string) => (t === 'Bug' ? 'danger' : t === 'Request' ? 'primary' : 'secondary') as 'danger' | 'primary' | 'secondary';
const statusVariant = (s: string) => (s === 'Resolved' ? 'success' : s === 'Read' ? 'secondary' : 'warning') as 'success' | 'secondary' | 'warning';

const filteredItems = computed(() => {
  let list = items.value;
  if (statusFilter.value) {
    list = list.filter((i) => i.status === statusFilter.value);
  }
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim();
    list = list.filter((i) => i.content.toLowerCase().includes(q) || (i.userName && i.userName.toLowerCase().includes(q)));
  }
  return list;
});

const totalCount = computed(() => items.value.length);
const newCount = computed(() => items.value.filter((i) => i.status === 'New').length);
const resolvedCount = computed(() => items.value.filter((i) => i.status === 'Resolved').length);

async function loadCourses() {
  try {
    const list = (await courseApi.getCourses()) as Array<{ id: string; title: string }>;
    courses.value = list;
    if (list.length > 0 && courseId.value === null) {
      courseId.value = Number(list[0].id);
      await loadFeedback();
    }
  } catch {
    error.value = 'Không tải được danh sách lộ trình.';
  }
}

async function loadFeedback() {
  if (courseId.value === null) return;
  loading.value = true;
  error.value = '';
  try {
    items.value = await courseApi.getCourseFeedbackAll(courseId.value);
  } catch {
    error.value = 'Không tải được ý kiến học viên.';
  } finally {
    loading.value = false;
  }
}

function onCourseChange() {
  statusFilter.value = '';
  searchQuery.value = '';
  loadFeedback();
}

function draftFor(item: CourseFeedbackDto) {
  if (!replyDraft.value[item.id]) {
    replyDraft.value[item.id] = { text: item.replyText ?? '', status: item.status, saving: false, saved: false };
  }
  return replyDraft.value[item.id];
}

async function saveReply(item: CourseFeedbackDto) {
  const draft = draftFor(item);
  draft.saving = true;
  draft.saved = false;
  try {
    const updated = await courseApi.replyCourseFeedback(item.id, {
      replyText: draft.text,
      status: draft.status,
    });
    const idx = items.value.findIndex((i) => i.id === item.id);
    if (idx >= 0) items.value[idx] = updated;
    draft.saved = true;
    ui.showToast('Đã lưu câu trả lời cho học viên!', 'success');
    setTimeout(() => (draft.saved = false), 2000);
  } catch {
    ui.showToast('Lưu phản hồi thất bại — vui lòng thử lại.', 'error');
  } finally {
    draft.saving = false;
  }
}

function formatDate(iso: string) {
  try {
    return new Date(iso).toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
  } catch {
    return iso;
  }
}

onMounted(loadCourses);
</script>

<template>
  <main class="admin-feedback container">
    <!-- Banner: surface band level-2 (DESIGN §1/#1 — KHÔNG gradient, KHÔNG shadow) -->
    <header class="admin-feedback__hero">
      <div class="admin-feedback__hero-inner">
        <div class="admin-feedback__hero-main">
          <div class="admin-feedback__hero-badges">
            <Badge variant="primary">
              <MessageSquare :size="12" /> Quản trị phản hồi
            </Badge>
          </div>
          <h1 class="admin-feedback__title">Ý kiến & Đóng góp của Học viên</h1>
          <p class="admin-feedback__sub">Lắng nghe ý kiến từ các lộ trình học, giải đáp thắc mắc và đánh dấu xử lý đóng góp của học viên.</p>
        </div>

        <!-- Mono strip: thống kê ý kiến mới cần xử lý -->
        <div class="admin-feedback__hero-strip" aria-hidden="true">
          <div class="admin-feedback__strip-panel">
            <div class="admin-feedback__strip-stat">
              <span class="admin-feedback__strip-num">{{ newCount }}</span>
              <span class="admin-feedback__strip-text">Ý kiến mới</span>
            </div>
          </div>
        </div>
      </div>
    </header>

    <AdminNav active="feedback" />

    <!-- Summary KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs text-vdsa-muted font-semibold uppercase tracking-wider">Tổng số ý kiến</p>
          <p class="text-2xl font-extrabold text-white mt-1">{{ totalCount }}</p>
        </div>
        <div class="w-10 h-10 rounded-lg bg-vdsa-accent/15 text-vdsa-accent flex items-center justify-center">
          <Inbox :size="20" />
        </div>
      </div>

      <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs text-vdsa-muted font-semibold uppercase tracking-wider">Ý kiến mới cần xử lý</p>
          <p class="text-2xl font-extrabold text-vdsa-yellow mt-1">{{ newCount }}</p>
        </div>
        <div class="w-10 h-10 rounded-lg bg-vdsa-yellow/15 text-vdsa-yellow flex items-center justify-center">
          <AlertCircle :size="20" />
        </div>
      </div>

      <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between">
        <div>
          <p class="text-xs text-vdsa-muted font-semibold uppercase tracking-wider">Đã giải quyết xong</p>
          <p class="text-2xl font-extrabold text-vdsa-green mt-1">{{ resolvedCount }}</p>
        </div>
        <div class="w-10 h-10 rounded-lg bg-vdsa-green/15 text-vdsa-green flex items-center justify-center">
          <CheckCircle2 :size="20" />
        </div>
      </div>
    </div>

    <!-- Filter & Toolbar -->
    <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex flex-col md:flex-row items-stretch md:items-center justify-between gap-4">
      <!-- Course Selector -->
      <div class="flex items-center gap-3">
        <label class="text-xs font-bold text-vdsa-secondary uppercase shrink-0">Lộ trình:</label>
        <select
          v-model.number="courseId"
          class="bg-vdsa-bg border border-vdsa-border rounded-xl px-3 py-2 text-sm text-white focus:outline-none focus:border-accent min-w-[240px]"
          @change="onCourseChange"
        >
          <option v-for="c in courses" :key="c.id" :value="Number(c.id)">{{ c.title }}</option>
        </select>
      </div>

      <!-- Status Filter Tabs -->
      <div class="flex items-center gap-2 flex-wrap">
        <Button
          v-for="f in STAT_FILTERS"
          :key="f.value"
          :variant="statusFilter === f.value ? 'primary' : 'secondary'"
          size="sm"
          @click="statusFilter = f.value"
        >
          {{ f.label }}
          <Badge v-if="f.value === 'New' && newCount > 0" variant="danger" class="ml-1.5">{{ newCount }}</Badge>
        </Button>
      </div>
    </div>

    <!-- Feedback List State -->
    <div v-if="loading" class="space-y-3" aria-busy="true">
      <Skeleton v-for="i in 3" :key="i" height="120px" />
    </div>

    <EmptyState
      v-else-if="filteredItems.length === 0"
      icon="message"
      title="Chưa có ý kiến nào"
      description="Khi học viên gửi đóng góp ý kiến từ trang lộ trình, chúng sẽ hiển thị tại đây."
    />

    <!-- Feedback Cards -->
    <div v-else class="space-y-4">
      <div
        v-for="item in filteredItems"
        :key="item.id"
        class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border hover:border-vdsa-accent/40 transition-colors space-y-4"
      >
        <!-- Header -->
        <div class="flex items-center justify-between flex-wrap gap-2">
          <div class="flex items-center gap-2.5">
            <div class="w-8 h-8 rounded-full bg-vdsa-bg-secondary border border-vdsa-border text-vdsa-purple-light flex items-center justify-center font-bold text-xs">
              <User :size="15" />
            </div>
            <div>
              <span class="font-bold text-sm text-white">{{ item.userName || 'Học viên ẩn danh' }}</span>
              <span class="text-xs text-vdsa-muted ml-2">{{ formatDate(item.createdAt) }}</span>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <Badge :variant="typeVariant(item.type)">{{ typeLabel(item.type) }}</Badge>
            <Badge :variant="statusVariant(item.status)">{{ statusLabel(item.status) }}</Badge>
          </div>
        </div>

        <!-- Student Content -->
        <div class="p-3.5 rounded-xl bg-vdsa-bg border border-vdsa-border/70 text-sm text-vdsa-secondary leading-relaxed">
          {{ item.content }}
        </div>

        <!-- Teacher Reply Box -->
        <div class="pt-2 border-t border-vdsa-border/60 space-y-2.5">
          <div class="flex items-center justify-between">
            <label class="text-xs font-bold text-vdsa-secondary uppercase flex items-center gap-1.5">
              <Send :size="13" class="text-vdsa-accent" /> Phản hồi từ Giảng viên
            </label>
            <span v-if="draftFor(item).saved" class="text-xs font-bold text-vdsa-green flex items-center gap-1">
              <Check :size="13" /> Đã lưu thành công
            </span>
          </div>

          <textarea
            v-model="draftFor(item).text"
            rows="2"
            maxlength="2000"
            class="w-full rounded-xl border border-vdsa-border bg-vdsa-bg px-3 py-2 text-sm text-white placeholder:text-vdsa-disabled outline-none focus:border-accent resize-y"
            placeholder="Nhập câu trả lời (sẽ hiển thị cho học viên khi họ quay lại khóa học)..."
          ></textarea>

          <div class="flex items-center justify-between flex-wrap gap-2">
            <div class="flex items-center gap-2">
              <label class="text-xs text-vdsa-muted font-semibold">Cập nhật trạng thái:</label>
              <select
                v-model="draftFor(item).status"
                class="rounded-lg border border-vdsa-border bg-vdsa-bg px-2.5 py-1.5 text-xs text-white outline-none focus:border-accent"
              >
                <option value="New">Mới</option>
                <option value="Read">Đã đọc</option>
                <option value="Resolved">Đã xử lý</option>
              </select>
            </div>

            <Button
              size="sm"
              variant="primary"
              :disabled="draftFor(item).saving"
              @click="saveReply(item)"
            >
              <span v-if="draftFor(item).saving" class="inline-block w-3.5 h-3.5 border-2 border-white/30 border-t-white rounded-full animate-spin mr-1.5"></span>
              {{ draftFor(item).saving ? 'Đang lưu...' : 'Lưu phản hồi' }}
            </Button>
          </div>
        </div>
      </div>
    </div>
  </main>
</template>

<style scoped>
.admin-feedback {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 1100px;
}

/* ── Banner: surface band level-2 (DESIGN §6) — không gradient, không shadow ── */
.admin-feedback__hero {
  border-bottom: 1px solid var(--border-subtle);
  background: var(--card-raised);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.admin-feedback__hero-inner {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.admin-feedback__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.admin-feedback__hero-badges { display: flex; gap: var(--space-sm); }
.admin-feedback__title { font-size: var(--text-4xl); font-weight: 600; margin: 0; color: var(--foreground); }
.admin-feedback__sub { color: var(--foreground-secondary); font-size: var(--text-sm); margin: 0; }

.admin-feedback__strip-panel {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}

.admin-feedback__strip-stat {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
}

.admin-feedback__strip-num {
  font-family: var(--font-mono);
  font-size: var(--text-2xl);
  font-weight: 700;
  color: var(--color-warning);
  line-height: 1;
}

.admin-feedback__strip-text {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  margin-top: 2px;
}
</style>
