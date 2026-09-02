<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import {
  MessageSquare,
  RefreshCw,
  Search,
  Send,
} from 'lucide-vue-next';

import { courseApi, type CourseListDto, type CourseFeedbackDto } from '@/services/courseApi';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import { normalizeVi } from '@/utils/searchNormalize';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const ui = useUiStore();
const route = useRoute();

const courses = ref<CourseListDto[]>([]);
const feedbackItems = ref<CourseFeedbackDto[]>([]);
const loading = ref(true);

const selectedCourseId = ref<string | number | 'all'>(
  typeof route.query.courseId === 'string' ? route.query.courseId : 'all',
);
const feedbackStatusFilter = ref<string>('');
const feedbackSearchQuery = ref('');
const replyTexts = ref<Record<number, string>>({});
const replySaving = ref<Record<number, boolean>>({});

async function loadCourses(): Promise<void> {
  try {
    courses.value = await courseApi.getCourses().catch(() => [] as CourseListDto[]);
  } catch {
    courses.value = [];
  }
}

async function loadFeedback(): Promise<void> {
  loading.value = true;
  try {
    const cId = selectedCourseId.value !== 'all' ? Number(selectedCourseId.value) : undefined;
    feedbackItems.value = await courseApi.getTeacherFeedback({
      courseId: cId,
      status: feedbackStatusFilter.value || undefined,
    });
  } catch (err) {
    console.error('Không thể tải danh sách ý kiến học viên:', err);
  } finally {
    loading.value = false;
  }
}

watch(
  [selectedCourseId, feedbackStatusFilter],
  () => {
    void loadFeedback();
  },
  { immediate: true },
);

onMounted(() => {
  void loadCourses();
});

const filteredFeedbacks = computed(() => {
  let list = feedbackItems.value;
  if (selectedCourseId.value !== 'all') {
    list = list.filter((i) => i.courseId === Number(selectedCourseId.value));
  }
  if (feedbackStatusFilter.value === 'OPEN') {
    // C5: nhóm "Đang xử lý" gộp New + Read.
    list = list.filter((i) => i.status === 'New' || i.status === 'Read');
  } else if (feedbackStatusFilter.value) {
    list = list.filter((i) => i.status === feedbackStatusFilter.value);
  }
  if (feedbackSearchQuery.value.trim()) {
    const q = normalizeVi(feedbackSearchQuery.value);
    list = list.filter(
      (i) => normalizeVi(i.content).includes(q) || normalizeVi(i.userName).includes(q),
    );
  }
  return list;
});

async function sendFeedbackReply(item: CourseFeedbackDto): Promise<void> {
  const text = (replyTexts.value[item.id] ?? '').trim();
  if (!text) {
    ui.showToast('Vui lòng nhập nội dung trả lời.', 'warning');
    return;
  }
  replySaving.value[item.id] = true;
  try {
    await courseApi.replyCourseFeedback(item.id, {
      replyText: text,
      status: 'Resolved',
    });
    item.replyText = text;
    item.status = 'Resolved';
    replyTexts.value[item.id] = '';
    ui.showToast('Đã gửi phản hồi tới học viên thành công!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Gửi phản hồi thất bại.', 'error');
  } finally {
    replySaving.value[item.id] = false;
  }
}
</script>

<template>
  <div class="space-y-4" data-testid="studio-feedback-tab">
    <!-- Course Selector Bar trong Tab Phản hồi -->
    <div class="course-bar">
      <div class="course-bar__selector-group">
        <label class="course-bar__label">Giáo trình học:</label>
        <select v-model="selectedCourseId" class="course-bar__select">
          <option value="all">🌐 Tất cả Giáo trình (Tổng hợp)</option>
          <option v-for="course in courses" :key="course.id" :value="course.id">
            📖 {{ course.title }}
          </option>
        </select>
      </div>
      <div class="flex items-center gap-2">
        <select v-model="feedbackStatusFilter" class="course-bar__select text-xs py-1.5 w-auto">
          <option value="">Tất cả Trạng thái</option>
          <option value="OPEN">Đang xử lý</option>
          <option value="Resolved">Đã xử lý</option>
        </select>
        <Button size="sm" variant="secondary" @click="loadFeedback">
          <RefreshCw :size="14" /> Làm mới
        </Button>
      </div>
    </div>

    <div class="flex items-center justify-between gap-4 flex-wrap">
      <div class="search-box">
        <Search :size="14" class="search-box__icon" />
        <input
          v-model="feedbackSearchQuery"
          type="text"
          class="search-box__input"
          placeholder="Tìm nội dung góp ý hoặc tên học viên..."
        />
      </div>
    </div>

    <div v-if="loading" class="space-y-3 py-4">
      <Skeleton v-for="i in 3" :key="i" height="100px" />
    </div>

    <EmptyState
      v-else-if="filteredFeedbacks.length === 0"
      icon="user"
      title="Chưa có ý kiến nào từ học viên"
      description="Khi học viên gửi góp ý, báo lỗi hoặc thắc mắc trong khóa học, chúng sẽ hiển thị ở đây."
    />

    <div v-else class="space-y-3">
      <article
        v-for="item in filteredFeedbacks"
        :key="item.id"
        class="p-4 rounded-xl border border-slate-700/70 bg-slate-900/60 space-y-3"
      >
        <div class="flex items-center justify-between gap-2 flex-wrap">
          <div class="flex items-center gap-2.5">
            <span class="w-8 h-8 rounded-full bg-purple-600/30 text-purple-300 flex items-center justify-center font-bold text-xs">
              {{ item.userName ? item.userName.charAt(0).toUpperCase() : 'U' }}
            </span>
            <div>
              <div class="text-sm font-semibold text-slate-100">{{ item.userName }}</div>
              <div class="text-xs text-slate-400">Khóa học: {{ item.courseTitle }} • {{ formatDate(item.createdAt) }}</div>
            </div>
          </div>

          <div class="flex items-center gap-1.5">
            <Badge :variant="item.type === 'Bug' ? 'danger' : 'secondary'">{{ item.type }}</Badge>
            <Badge :variant="item.status === 'Resolved' ? 'success' : 'warning'">{{ item.status === 'Resolved' ? 'Đã xử lý' : 'Đang xử lý' }}</Badge>
          </div>
        </div>

        <p class="text-sm text-slate-200 bg-slate-950/40 p-3 rounded-lg border border-slate-800">
          {{ item.content }}
        </p>

        <!-- Trả lời feedback -->
        <div v-if="item.replyText" class="p-3 bg-purple-950/30 border border-purple-800/40 rounded-lg text-xs text-purple-200">
          <strong>Phản hồi của giảng viên ({{ item.repliedByName || 'Bạn' }}):</strong>
          <p class="mt-1">{{ item.replyText }}</p>
        </div>

        <div v-else-if="item.status !== 'Resolved'" class="flex items-center gap-2 pt-1">
          <input
            v-model="replyTexts[item.id]"
            type="text"
            placeholder="Nhập phản hồi trực tiếp cho học viên..."
            class="flex-1 rounded-md border border-slate-700 bg-slate-950 px-3 py-1.5 text-xs text-slate-100 focus:outline-none focus:ring-1 focus:ring-purple-500"
            @keydown.enter="sendFeedbackReply(item)"
          />
          <Button
            size="sm"
            variant="primary"
            :disabled="replySaving[item.id]"
            @click="sendFeedbackReply(item)"
          >
            <Send :size="13" /> Gửi phản hồi
          </Button>
        </div>
      </article>
    </div>
  </div>
</template>

<style scoped>
.course-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md) var(--space-lg);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.course-bar__selector-group {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.course-bar__label {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--foreground);
  white-space: nowrap;
}

.course-bar__select {
  min-width: 280px;
  max-width: 420px;
  padding: 8px 12px;
  border-radius: var(--radius-md);
  background: var(--muted);
  border: 1px solid var(--border);
  color: var(--foreground);
  font-size: var(--text-sm);
  font-weight: 500;
  outline: none;
  cursor: pointer;
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
}

.search-box__icon {
  position: absolute;
  left: 10px;
  color: var(--foreground-tertiary);
}

.search-box__input {
  width: 280px;
  padding: 7px 12px 7px 32px;
  border-radius: var(--radius-md);
  background: var(--card);
  border: 1px solid var(--border);
  color: var(--foreground);
  font-size: var(--text-xs);
  outline: none;
  transition: border-color 150ms;
}

.search-box__input:focus {
  border-color: var(--primary);
}

@media (max-width: 768px) {
  .course-bar {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>
