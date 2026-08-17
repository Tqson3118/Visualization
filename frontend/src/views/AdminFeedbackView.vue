<script setup lang="ts">
// AdminFeedbackView — Màn 10b: GV xem + trả lời ý kiến học viên (tương tác 2 chiều).
// HV gửi ở trang chi tiết khóa học → GV lọc theo khóa/trạng thái, trả lời, đánh dấu xử lý.
// Dữ liệu thật qua /courses/feedback/all (TEACHER,ADMIN).
import { onMounted, ref, computed } from 'vue';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import AdminNav from '@/components/admin/AdminNav.vue';
import { courseApi, type CourseFeedbackDto } from '@/services/courseApi';

const loading = ref(true);
const error = ref('');
const courses = ref<Array<{ id: string; title: string }>>([]);
const courseId = ref<number | null>(null);
const statusFilter = ref('');
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
  if (!statusFilter.value) return items.value;
  return items.value.filter((i) => i.status === statusFilter.value);
});

const newCount = computed(() => items.value.filter((i) => i.status === 'New').length);

async function loadCourses() {
  try {
    const list = (await courseApi.getCourses()) as Array<{ id: string; title: string }>;
    courses.value = list;
    if (list.length > 0 && courseId.value === null) {
      courseId.value = Number(list[0].id);
      await loadFeedback();
    }
  } catch {
    error.value = 'Không tải được danh sách khóa học.';
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
    setTimeout(() => (draft.saved = false), 2000);
  } catch {
    error.value = 'Lưu phản hồi thất bại — vui lòng thử lại.';
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
  <div class="mx-auto max-w-5xl px-4 py-6 space-y-6">
    <AdminNav active="feedback" />

    <Card>
      <CardHeader>
        <CardTitle>Ý kiến học viên</CardTitle>
        <CardDescription>
          Học viên gửi đóng góp ý kiến từ trang khóa học — giảng viên đọc, trả lời và đánh dấu xử lý.
        </CardDescription>
      </CardHeader>
      <CardContent class="space-y-5">
        <!-- Filters -->
        <div class="flex flex-col md:flex-row md:items-center gap-4">
          <label class="flex items-center gap-3 text-sm">
            <span class="font-medium text-muted-foreground shrink-0">Khóa học</span>
            <select
              v-model.number="courseId"
              @change="onCourseChange"
              class="h-9 rounded-md border border-input bg-background px-3 text-sm outline-none focus:border-primary"
            >
              <option v-for="c in courses" :key="c.id" :value="Number(c.id)">{{ c.title }}</option>
            </select>
          </label>
          <div class="flex items-center gap-2 flex-wrap">
            <Button
              v-for="f in STAT_FILTERS"
              :key="f.value"
              :variant="statusFilter === f.value ? 'primary' : 'secondary'"
              size="sm"
              @click="statusFilter = f.value"
            >
              {{ f.label }}
              <Badge v-if="f.value === 'New' && newCount > 0" variant="danger" class="ml-1">{{ newCount }}</Badge>
            </Button>
          </div>
        </div>

        <p v-if="error" class="text-sm text-destructive">{{ error }}</p>

        <!-- Loading -->
        <div v-if="loading" class="space-y-3">
          <Skeleton v-for="i in 3" :key="i" class="h-28 w-full" />
        </div>

        <!-- Empty -->
        <EmptyState
          v-else-if="filteredItems.length === 0"
          icon="message"
          title="Chưa có ý kiến nào"
          description="Khi học viên gửi ý kiến từ trang khóa học, chúng sẽ hiển thị tại đây."
        />

        <!-- Feedback list -->
        <div v-else class="space-y-4">
          <div v-for="item in filteredItems" :key="item.id" class="rounded-xl border bg-card p-5">
            <div class="flex items-center gap-3 flex-wrap mb-3">
              <span class="font-semibold text-sm">{{ item.userName }}</span>
              <Badge :variant="typeVariant(item.type)">{{ typeLabel(item.type) }}</Badge>
              <Badge :variant="statusVariant(item.status)">{{ statusLabel(item.status) }}</Badge>
              <span class="text-xs text-muted-foreground ml-auto">{{ formatDate(item.createdAt) }}</span>
            </div>
            <p class="text-sm leading-relaxed text-foreground/90">{{ item.content }}</p>

            <div class="mt-4 space-y-3">
              <label class="block text-xs font-medium text-muted-foreground">Trả lời học viên</label>
              <textarea
                v-model="draftFor(item).text"
                rows="2"
                maxlength="2000"
                class="w-full rounded-md border border-input bg-background px-3 py-2 text-sm outline-none focus:border-primary resize-none"
                placeholder="Nhập câu trả lời (sẽ hiển thị cho học viên khi họ quay lại khóa học)..."
              ></textarea>
              <div class="flex items-center gap-3 justify-between flex-wrap">
                <select
                  v-model="draftFor(item).status"
                  class="h-9 rounded-md border border-input bg-background px-3 text-sm outline-none focus:border-primary"
                >
                  <option value="New">Mới</option>
                  <option value="Read">Đã đọc</option>
                  <option value="Resolved">Đã xử lý</option>
                </select>
                <div class="flex items-center gap-2">
                  <span v-if="draftFor(item).saved" class="text-xs font-semibold text-emerald-500">Đã lưu ✓</span>
                  <Button
                    size="sm"
                    :disabled="draftFor(item).saving"
                    @click="saveReply(item)"
                  >
                    {{ draftFor(item).saving ? 'Đang lưu...' : 'Lưu phản hồi' }}
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
