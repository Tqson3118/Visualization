<script setup lang="ts">
// AdminContentView — Màn 09: quản lý lessons/topics CRUD cơ bản + gắn mô phỏng
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';

import * as lessonsApi from '@/api/lessons';
import type { LessonSummary, Topic } from '@/api/lessons';
import { useUiStore } from '@/stores/ui';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import { CATALOG } from '@/engines/catalog';
import { formatDate } from '@/utils/format';

const ui = useUiStore();
const router = useRouter();

const tab = ref<'lessons' | 'topics'>('lessons');
const lessons = ref<LessonSummary[]>([]);
const topics = ref<Topic[]>([]);
const loading = ref(true);

// Form bài học
const formOpen = ref(false);
const editingId = ref<number | null>(null);
const saving = ref(false);
const form = reactive({
  title: '',
  description: '',
  topicId: 1,
  status: 'draft' as 'draft' | 'active' | 'hidden',
  contentHtml: '',
  simulationKey: '',
  sortOrder: 1,
});

// Form topic
const topicFormOpen = ref(false);
const topicForm = reactive({ name: '', description: '', sortOrder: 0 });

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    const [lessonPage, topicTree] = await Promise.all([
      lessonsApi.fetchLessons({}),
      lessonsApi.fetchTopics().catch(() => [] as Topic[]),
    ]);
    lessons.value = lessonPage.items;
    topics.value = topicTree;
    if (topicTree.length > 0 && form.topicId === 1 && !topicTree.some((t) => t.id === 1)) {
      form.topicId = topicTree[0].id;
    }
  } catch {
    ui.showToast('Không thể tải nội dung (backend chưa khả dụng).', 'error');
  } finally {
    loading.value = false;
  }
}

const topicName = computed(() => (id: number) => topics.value.find((t) => t.id === id)?.name ?? `#${id}`);

function openCreate(): void {
  editingId.value = null;
  Object.assign(form, {
    title: '',
    description: '',
    topicId: topics.value[0]?.id ?? 1,
    status: 'draft',
    contentHtml: '',
    simulationKey: '',
    sortOrder: lessons.value.length + 1,
  });
  formOpen.value = true;
}

function openEdit(lesson: LessonSummary): void {
  editingId.value = lesson.id;
  Object.assign(form, {
    title: lesson.title,
    description: lesson.description,
    topicId: lesson.topicId,
    status: lesson.status,
    contentHtml: '',
    simulationKey: '',
    sortOrder: lesson.sortOrder,
  });
  formOpen.value = true;
}

async function saveLesson(): Promise<void> {
  if (form.title.trim().length < 3) {
    ui.showToast('Tiêu đề phải từ 3 ký tự.', 'warning');
    return;
  }
  saving.value = true;
  try {
    const payload = {
      topicId: form.topicId,
      title: form.title.trim(),
      description: form.description,
      contentHtml: form.contentHtml || '<p>Đang biên soạn...</p>',
      status: form.status,
      sortOrder: form.sortOrder,
    };
    if (editingId.value === null) {
      const created = await lessonsApi.createLesson(payload);
      if (form.simulationKey) {
        await lessonsApi.attachSimulation(created.id, { simulationKey: form.simulationKey }).catch(() => undefined);
      }
    } else {
      await lessonsApi.updateLesson(editingId.value, payload);
    }
    ui.showToast('Đã lưu bài học.', 'success');
    formOpen.value = false;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Lưu thất bại.', 'error');
  } finally {
    saving.value = false;
  }
}

async function deleteLesson(lesson: LessonSummary): Promise<void> {
  if (!window.confirm(`Xóa bài học "${lesson.title}"? (xóa mềm — ẩn khỏi người học)`)) return;
  try {
    await lessonsApi.deleteLesson(lesson.id);
    ui.showToast('Đã xóa bài học.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xóa thất bại.', 'error');
  }
}

async function saveTopic(): Promise<void> {
  try {
    await lessonsApi.createTopic(topicForm);
    ui.showToast('Đã tạo chủ đề.', 'success');
    topicFormOpen.value = false;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Tạo chủ đề thất bại.', 'error');
  }
}
</script>

<template>
  <main class="admin-content container">
    <h1 class="admin-content__title">📚 Quản trị nội dung</h1>

    <AdminNav active="content" />

    <div class="admin-content__tabs">
      <button type="button" class="admin-content__tab" :class="{ 'admin-content__tab--active': tab === 'lessons' }" @click="tab = 'lessons'">
        Bài học ({{ lessons.length }})
      </button>
      <button type="button" class="admin-content__tab" :class="{ 'admin-content__tab--active': tab === 'topics' }" @click="tab = 'topics'">
        Chủ đề ({{ topics.length }})
      </button>
    </div>

    <div v-if="loading" class="admin-content__loading">
      <Skeleton v-for="i in 5" :key="i" height="44px" />
    </div>

    <!-- Danh sách bài học -->
    <template v-else-if="tab === 'lessons'">
      <div class="admin-content__toolbar">
        <Button size="sm" @click="openCreate">+ Thêm bài học</Button>
        <Button variant="ghost" size="sm" @click="router.push({ name: 'admin-ladder' })">
          Soạn bài tập ở tab Ladder →
        </Button>
      </div>

      <EmptyState
        v-if="lessons.length === 0"
        icon="book"
        title="Chưa có bài học"
        description="Tạo bài học đầu tiên."
        action-label="Tạo bài học"
        @action="openCreate"
      />

      <div v-else class="admin-content__table card">
        <table>
          <thead>
            <tr>
              <th>Tiêu đề</th>
              <th>Chủ đề</th>
              <th>Trạng thái</th>
              <th>Mô phỏng</th>
              <th>Ngày tạo</th>
              <th>Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="lesson in lessons" :key="lesson.id">
              <td class="admin-content__title-cell">{{ lesson.title }}</td>
              <td>{{ topicName(lesson.topicId) }}</td>
              <td><Badge :variant="lesson.status === 'active' ? 'success' : lesson.status === 'draft' ? 'warning' : 'muted'">{{ lesson.status }}</Badge></td>
              <td>{{ lesson.simulationCount }}</td>
              <td class="text-muted">{{ formatDate(new Date()) }}</td>
              <td>
                <div class="admin-content__actions">
                  <Button size="sm" variant="ghost" @click="openEdit(lesson)">Sửa</Button>
                  <Button size="sm" variant="danger" @click="deleteLesson(lesson)">Xóa</Button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>

    <!-- Danh sách chủ đề -->
    <template v-else>
      <div class="admin-content__toolbar">
        <Button size="sm" @click="topicFormOpen = true">+ Thêm chủ đề</Button>
      </div>
      <div class="admin-content__topics">
        <div v-for="topic in topics" :key="topic.id" class="admin-content__topic card">
          <h3 class="admin-content__topic-name">{{ topic.name }}</h3>
          <p class="text-muted admin-content__topic-desc">{{ topic.description }}</p>
        </div>
      </div>
    </template>

    <!-- Modal bài học -->
    <Modal :open="formOpen" :title="editingId === null ? 'Thêm bài học' : 'Sửa bài học'" @close="formOpen = false">
      <form class="admin-content__form" novalidate @submit.prevent="saveLesson">
        <Input v-model="form.title" label="Tiêu đề *" required />
        <Input v-model="form.description" label="Mô tả ngắn" />
        <div class="admin-content__row">
          <label class="label">Chủ đề</label>
          <select v-model="form.topicId" class="input">
            <option v-for="topic in topics" :key="topic.id" :value="topic.id">{{ topic.name }}</option>
          </select>
        </div>
        <div class="admin-content__row">
          <label class="label">Trạng thái</label>
          <select v-model="form.status" class="input">
            <option value="draft">Bản nháp</option>
            <option value="active">Kích hoạt</option>
            <option value="hidden">Ẩn</option>
          </select>
        </div>
        <div class="admin-content__row">
          <label class="label">Gắn mô phỏng (tùy chọn)</label>
          <select v-model="form.simulationKey" class="input">
            <option value="">— Không gắn —</option>
            <option v-for="sim in CATALOG" :key="sim.key" :value="sim.key">{{ sim.title }}</option>
          </select>
        </div>
        <div class="admin-content__row">
          <label class="label">Nội dung (HTML đơn giản)</label>
          <textarea v-model="form.contentHtml" class="input admin-content__html" rows="5" placeholder="<h2>Ý tưởng</h2><p>...</p>" />
        </div>
        <div class="admin-content__actions">
          <Button variant="ghost" @click="formOpen = false">Hủy</Button>
          <Button type="submit" :loading="saving">Lưu</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal chủ đề -->
    <Modal :open="topicFormOpen" title="Thêm chủ đề" @close="topicFormOpen = false">
      <form class="admin-content__form" novalidate @submit.prevent="saveTopic">
        <Input v-model="topicForm.name" label="Tên chủ đề *" required />
        <Input v-model="topicForm.description" label="Mô tả" />
        <div class="admin-content__actions">
          <Button variant="ghost" @click="topicFormOpen = false">Hủy</Button>
          <Button type="submit">Tạo</Button>
        </div>
      </form>
    </Modal>
  </main>
</template>

<style scoped>
.admin-content {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.admin-content__title { font-size: var(--text-2xl); }

.admin-content__tabs { display: flex; gap: var(--space-xs); border-bottom: 2px solid var(--color-border); }

.admin-content__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-md);
  font-weight: 700;
  color: var(--color-text-muted);
  cursor: pointer;
  margin-bottom: -2px;
}

.admin-content__tab--active { color: var(--color-primary); border-bottom-color: var(--color-primary); }

.admin-content__toolbar { display: flex; gap: var(--space-sm); justify-content: flex-end; flex-wrap: wrap; }

.admin-content__table { padding: 0; overflow-x: auto; }

.admin-content__table table { width: 100%; border-collapse: collapse; min-width: 640px; }

.admin-content__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 2px solid var(--color-border);
  background: var(--color-muted);
}

.admin-content__table td { padding: var(--space-sm) var(--space-md); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); }

.admin-content__title-cell { font-weight: 700; }

.admin-content__actions { display: flex; gap: var(--space-xs); flex-wrap: wrap; }

.admin-content__topics { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-content__topic-name { font-size: var(--text-md); }
.admin-content__topic-desc { font-size: var(--text-sm); }

.admin-content__form { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-content__row { display: flex; flex-direction: column; gap: var(--space-xs); }

.admin-content__html { font-family: var(--font-mono); font-size: var(--text-xs); resize: vertical; }

.admin-content__actions { display: flex; justify-content: flex-end; gap: var(--space-sm); }
</style>
