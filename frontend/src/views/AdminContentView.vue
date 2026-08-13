<script setup lang="ts">
// AdminContentView — Màn 09: quản lý lessons/topics CRUD cơ bản + gắn mô phỏng
// View-quality 14/08 (Nhóm D): banner surface band + mono strip block-token
// (số bài học/chủ đề — dữ liệu thật); bảng §4.6 + mobile card-stack; SỬA BUG
// cột "Ngày tạo" hiển thị formatDate(new Date()) (LessonSummary không có
// createdAt) → cột index mono #01; topic card bỏ gradient/hover-lift;
// error state + retry.
// Ghi chú Phase 2: nội dung rich-text (contentHtml) của CMS — KHÔNG sửa.
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  ArrowDown,
  ArrowRight,
  ArrowUp,
  Layers,
  Network,
  Pencil,
  Plus,
  RefreshCw,
  Trash2,
} from 'lucide-vue-next';

import * as lessonsApi from '@/api/lessons';
import type { LessonSummary, Topic } from '@/api/lessons';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Tabs from '@/components/ui/Tabs.vue';
import { CATALOG } from '@/engines/catalog';

const ui = useUiStore();
const router = useRouter();

const tab = ref<'lessons' | 'topics'>('lessons');
const lessons = ref<LessonSummary[]>([]);
const topics = ref<Topic[]>([]);
const loading = ref(true);
const loadError = ref(false);

// ── UI-PREMIUM 1D: sắp xếp cột (client-side, presentation only) + indicator animation ──
type LessonSortKey = 'title' | 'topic' | 'status' | 'sim';
const sortKey = ref<LessonSortKey | null>(null);
const sortDir = ref<'asc' | 'desc'>('asc');

const sortedLessons = computed(() => {
  const list = [...lessons.value];
  if (sortKey.value === null) return list;
  const dir = sortDir.value === 'asc' ? 1 : -1;
  list.sort((a, b) => {
    switch (sortKey.value) {
      case 'title':
        return a.title.localeCompare(b.title, 'vi') * dir;
      case 'topic':
        return topicName.value(a.topicId).localeCompare(topicName.value(b.topicId), 'vi') * dir;
      case 'status':
        return a.status.localeCompare(b.status) * dir;
      case 'sim':
        return (a.simulationCount - b.simulationCount) * dir;
      default:
        return 0;
    }
  });
  return list;
});

function toggleSort(key: LessonSortKey): void {
  if (sortKey.value === key) {
    sortDir.value = sortDir.value === 'asc' ? 'desc' : 'asc';
  } else {
    sortKey.value = key;
    sortDir.value = 'asc';
  }
}

function sortAria(key: LessonSortKey): 'ascending' | 'descending' | 'none' {
  if (sortKey.value !== key) return 'none';
  return sortDir.value === 'asc' ? 'ascending' : 'descending';
}

// ── Xóa bài học qua Modal xác nhận (UI-PREMIUM 1D: bỏ window.confirm) ──
const deleteTarget = ref<LessonSummary | null>(null);
const deleting = ref(false);

function askDelete(lesson: LessonSummary): void {
  deleteTarget.value = lesson;
}

async function confirmDelete(): Promise<void> {
  if (!deleteTarget.value) return;
  deleting.value = true;
  try {
    await lessonsApi.deleteLesson(deleteTarget.value.id);
    ui.showToast('Đã xóa bài học.', 'success');
    deleteTarget.value = null;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xóa thất bại.', 'error');
  } finally {
    deleting.value = false;
  }
}

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
  loadError.value = false;
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
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

const topicName = computed(() => (id: number) => topics.value.find((t) => t.id === id)?.name ?? `#${id}`);

/** Số bài học mỗi chủ đề (tính từ danh sách lessons đã tải — presentation only). */
const topicLessonCount = computed(() => {
  const map = new Map<number, number>();
  for (const lesson of lessons.value) {
    map.set(lesson.topicId, (map.get(lesson.topicId) ?? 0) + 1);
  }
  return map;
});

/** Strip banner: block-token dữ liệu thật — số bài học + số chủ đề. */
const stripBlocks = computed<boolean[]>(() => {
  const count = Math.min(Math.max(lessons.value.length, topics.value.length), 5);
  const size = Math.max(count, 1);
  return Array.from({ length: size }, (_, i) => i < count);
});

const pad = (n: number): string => String(n).padStart(2, '0');

const contentTabs = computed(() => [
  { key: 'lessons', label: messages.admin.content.tabLessons, badge: lessons.value.length > 0 ? lessons.value.length : undefined },
  { key: 'topics', label: messages.admin.content.tabTopics, badge: topics.value.length > 0 ? topics.value.length : undefined },
]);

const statusLabel: Record<string, string> = {
  draft: messages.admin.content.statusDraft,
  active: messages.admin.content.statusActive,
  hidden: messages.admin.content.statusHidden,
};

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
    <!-- Banner: surface band level-2 (DESIGN §1/#1 — KHÔNG gradient, KHÔNG shadow) -->
    <header class="admin-content__hero">
      <div class="admin-content__hero-inner">
        <div class="admin-content__hero-main">
          <div class="admin-content__hero-badges">
            <Badge variant="primary">{{ messages.admin.badge }}</Badge>
          </div>
          <h1 class="admin-content__title">{{ messages.admin.content.title }}</h1>
          <p class="admin-content__sub">{{ messages.admin.content.subtitle }}</p>
        </div>

        <!-- Mono strip: block-token dữ liệu thật (bài học/chủ đề) + index mono -->
        <div class="admin-content__hero-strip" aria-hidden="true">
          <div class="admin-content__strip-panel">
            <div class="admin-content__strip-blocks">
              <span
                v-for="(filled, i) in stripBlocks"
                :key="i"
                class="admin-content__strip-block"
                :class="{ 'admin-content__strip-block--empty': !filled }"
                :style="{ '--i': i }"
              />
            </div>
            <div class="admin-content__strip-index">
              <span v-for="(_, i) in stripBlocks" :key="i">{{ String(i).padStart(2, '0') }}</span>
            </div>
          </div>
          <p class="admin-content__strip-caption">{{ messages.admin.content.stripLabel(lessons.length, topics.length) }}</p>
        </div>
      </div>
    </header>

    <AdminNav active="content" />

    <Tabs :tabs="contentTabs" :model-value="tab" @change="(key: string) => (tab = key as 'lessons' | 'topics')" />

    <div v-if="loading" class="admin-content__loading" aria-busy="true">
      <Skeleton v-for="i in 5" :key="i" height="56px" />
    </div>

    <div v-else-if="loadError" class="admin-content__error" role="alert">
      <p class="admin-content__error-text">Không thể tải nội dung (backend chưa khả dụng).</p>
      <Button size="sm" variant="secondary" @click="load">
        <RefreshCw :size="14" /> {{ messages.admin.content.retry }}
      </Button>
    </div>

    <!-- Danh sách bài học -->
    <template v-else-if="tab === 'lessons'">
      <div class="admin-content__toolbar">
        <Button size="md" @click="openCreate"><Plus :size="16" /> {{ messages.admin.content.addLesson }}</Button>
        <Button size="sm" variant="ghost" @click="router.push({ name: 'admin-ladder' })">
          {{ messages.admin.content.ladderHint }} <ArrowRight :size="16" />
        </Button>
      </div>

      <EmptyState
        v-if="lessons.length === 0"
        icon="book"
        :title="messages.admin.content.emptyLessons"
        :description="messages.admin.content.emptyLessonsDesc"
        :action-label="messages.admin.content.addLesson"
        @action="openCreate"
      />

      <div v-else class="admin-content__table">
        <div class="admin-content__table-scroll">
          <table class="table-stack">
            <thead>
              <tr>
                <th scope="col">{{ messages.admin.content.colIndex }}</th>
                <th scope="col" :aria-sort="sortAria('title')">
                  <button
                    type="button"
                    class="admin-content__sort"
                    :class="{ 'admin-content__sort--active': sortKey === 'title' }"
                    @click="toggleSort('title')"
                  >
                    <span>{{ messages.admin.content.colTitle }}</span>
                    <span class="admin-content__sort-arrows" aria-hidden="true">
                      <ArrowUp :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'title' && sortDir === 'asc' }" />
                      <ArrowDown :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'title' && sortDir === 'desc' }" />
                    </span>
                  </button>
                </th>
                <th scope="col" :aria-sort="sortAria('topic')">
                  <button
                    type="button"
                    class="admin-content__sort"
                    :class="{ 'admin-content__sort--active': sortKey === 'topic' }"
                    @click="toggleSort('topic')"
                  >
                    <span>{{ messages.admin.content.colTopic }}</span>
                    <span class="admin-content__sort-arrows" aria-hidden="true">
                      <ArrowUp :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'topic' && sortDir === 'asc' }" />
                      <ArrowDown :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'topic' && sortDir === 'desc' }" />
                    </span>
                  </button>
                </th>
                <th scope="col" :aria-sort="sortAria('status')">
                  <button
                    type="button"
                    class="admin-content__sort"
                    :class="{ 'admin-content__sort--active': sortKey === 'status' }"
                    @click="toggleSort('status')"
                  >
                    <span>{{ messages.admin.content.colStatus }}</span>
                    <span class="admin-content__sort-arrows" aria-hidden="true">
                      <ArrowUp :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'status' && sortDir === 'asc' }" />
                      <ArrowDown :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'status' && sortDir === 'desc' }" />
                    </span>
                  </button>
                </th>
                <th scope="col" :aria-sort="sortAria('sim')">
                  <button
                    type="button"
                    class="admin-content__sort"
                    :class="{ 'admin-content__sort--active': sortKey === 'sim' }"
                    @click="toggleSort('sim')"
                  >
                    <span>{{ messages.admin.content.colSim }}</span>
                    <span class="admin-content__sort-arrows" aria-hidden="true">
                      <ArrowUp :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'sim' && sortDir === 'asc' }" />
                      <ArrowDown :size="12" class="admin-content__sort-arrow" :class="{ 'admin-content__sort-arrow--on': sortKey === 'sim' && sortDir === 'desc' }" />
                    </span>
                  </button>
                </th>
                <th scope="col" class="admin-content__actions-col">{{ messages.admin.content.colActions }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(lesson, idx) in sortedLessons" :key="lesson.id">
                <td :data-label="messages.admin.content.colIndex" class="admin-content__idx">{{ pad(idx + 1) }}</td>
                <td :data-label="messages.admin.content.colTitle" class="admin-content__title-cell">
                  <p class="admin-content__title-text">{{ lesson.title }}</p>
                  <p v-if="lesson.description" class="admin-content__title-desc">{{ lesson.description }}</p>
                </td>
                <td :data-label="messages.admin.content.colTopic"><Badge variant="secondary">{{ topicName(lesson.topicId) }}</Badge></td>
                <td :data-label="messages.admin.content.colStatus">
                  <Badge :variant="lesson.status === 'active' ? 'success' : lesson.status === 'draft' ? 'warning' : 'muted'">
                    {{ statusLabel[lesson.status] ?? lesson.status }}
                  </Badge>
                </td>
                <td :data-label="messages.admin.content.colSim">
                  <span class="admin-content__sim-count" :class="{ 'admin-content__sim-count--zero': lesson.simulationCount === 0 }">
                    <Network :size="13" /> {{ lesson.simulationCount }}
                  </span>
                </td>
                <td :data-label="messages.admin.content.colActions">
                  <div class="admin-content__actions">
                    <Button size="sm" variant="ghost" @click="openEdit(lesson)">
                      <Pencil :size="16" /> {{ messages.admin.content.edit }}
                    </Button>
                    <Button size="sm" variant="danger" @click="askDelete(lesson)">
                      <Trash2 :size="16" /> {{ messages.admin.content.delete }}
                    </Button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- Danh sách chủ đề -->
    <template v-else>
      <div class="admin-content__toolbar">
        <Button size="md" @click="topicFormOpen = true"><Plus :size="16" /> {{ messages.admin.content.addTopic }}</Button>
      </div>

      <EmptyState
        v-if="topics.length === 0"
        icon="book"
        :title="messages.admin.content.emptyTopics"
        :description="messages.admin.content.emptyTopicsDesc"
        :action-label="messages.admin.content.addTopic"
        @action="topicFormOpen = true"
      />

      <div v-else class="admin-content__topics">
        <Card v-for="topic in topics" :key="topic.id" class="admin-content__topic">
          <CardHeader class="admin-content__topic-head">
            <span class="admin-content__topic-icon" aria-hidden="true"><Layers :size="16" /></span>
            <div class="admin-content__topic-meta">
              <CardTitle class="admin-content__topic-name">{{ topic.name }}</CardTitle>
              <CardDescription class="admin-content__topic-desc">
                {{ topic.description || '—' }}
              </CardDescription>
            </div>
            <Badge variant="secondary" class="admin-content__topic-count">
              {{ topicLessonCount.get(topic.id) ?? 0 }} {{ messages.admin.content.lessonsCount }}
            </Badge>
          </CardHeader>
        </Card>
      </div>
    </template>

    <!-- Modal bài học -->
    <Modal :open="formOpen" :title="editingId === null ? messages.admin.content.createLessonTitle : messages.admin.content.editLessonTitle" @close="formOpen = false">
      <form class="admin-content__form" novalidate @submit.prevent="saveLesson">
        <Input v-model="form.title" :label="messages.admin.content.lessonTitle" required />
        <Input v-model="form.description" :label="messages.admin.content.lessonDesc" />
        <div class="admin-content__row">
          <label class="label" for="lesson-topic">{{ messages.admin.content.lessonTopic }}</label>
          <select id="lesson-topic" v-model="form.topicId" class="input">
            <option v-for="topic in topics" :key="topic.id" :value="topic.id">{{ topic.name }}</option>
          </select>
        </div>
        <div class="admin-content__row">
          <label class="label" for="lesson-status">{{ messages.admin.content.lessonStatus }}</label>
          <select id="lesson-status" v-model="form.status" class="input">
            <option value="draft">{{ messages.admin.content.statusDraft }}</option>
            <option value="active">{{ messages.admin.content.statusActive }}</option>
            <option value="hidden">{{ messages.admin.content.statusHidden }}</option>
          </select>
        </div>
        <div class="admin-content__row">
          <label class="label" for="lesson-sim">{{ messages.admin.content.simAttach }}</label>
          <select id="lesson-sim" v-model="form.simulationKey" class="input">
            <option value="">{{ messages.admin.content.simNone }}</option>
            <option v-for="sim in CATALOG" :key="sim.key" :value="sim.key">{{ sim.title }}</option>
          </select>
        </div>
        <div class="admin-content__row">
          <label class="label" for="lesson-html">{{ messages.admin.content.contentHtml }}</label>
          <textarea id="lesson-html" v-model="form.contentHtml" class="input admin-content__html" rows="5" :placeholder="messages.admin.content.htmlPlaceholder" />
        </div>
        <div class="admin-content__actions">
          <Button variant="ghost" @click="formOpen = false">{{ messages.admin.content.cancel }}</Button>
          <Button type="submit" :loading="saving">{{ messages.admin.content.save }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal chủ đề -->
    <Modal :open="topicFormOpen" :title="messages.admin.content.createTopicTitle" @close="topicFormOpen = false">
      <form class="admin-content__form" novalidate @submit.prevent="saveTopic">
        <Input v-model="topicForm.name" :label="messages.admin.content.topicName" required />
        <Input v-model="topicForm.description" :label="messages.admin.content.topicDesc" />
        <div class="admin-content__actions">
          <Button variant="ghost" @click="topicFormOpen = false">{{ messages.admin.content.cancel }}</Button>
          <Button type="submit">{{ messages.admin.content.create }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Xác nhận xóa bài học (UI-PREMIUM 1D: bỏ window.confirm → Modal + toast cùng động từ) -->
    <Modal
      :open="deleteTarget !== null"
      title="Xóa bài học"
      @close="deleteTarget = null"
    >
      <div class="admin-content__confirm">
        <p class="admin-content__confirm-text">
          Bạn sắp xóa bài học <strong>{{ deleteTarget?.title }}</strong> — đây là xóa mềm, bài học sẽ bị
          ẩn khỏi người học.
        </p>
      </div>
      <template #footer>
        <Button variant="ghost" :disabled="deleting" @click="deleteTarget = null">
          {{ messages.admin.content.cancel }}
        </Button>
        <Button variant="danger" :loading="deleting" @click="confirmDelete">
          <Trash2 :size="16" /> {{ messages.admin.content.delete }}
        </Button>
      </template>
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

/* ── Banner: surface band level-2 (DESIGN §6) — không gradient, không shadow ── */
.admin-content__hero {
  border-bottom: 1px solid var(--border-subtle);
  background: var(--card-raised);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.admin-content__hero-inner {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.admin-content__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
  flex: 1 1 320px;
}

.admin-content__hero-badges { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.admin-content__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  margin: 0;
  color: var(--foreground);
}

.admin-content__sub {
  color: var(--foreground-secondary);
  font-size: var(--text-sm);
  max-width: 60ch;
  margin: 0;
}

/* ── Mono strip: block-token dữ liệu thật (khoảnh khắc đầu tư duy nhất) ── */
.admin-content__hero-strip { flex: 0 1 260px; display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-content__strip-panel {
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.25);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.admin-content__strip-blocks {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.admin-content__strip-block {
  height: 28px;
  border-radius: var(--radius-sm);
  background: var(--data-core);
  opacity: 0;
  transform: translateY(6px);
  animation: admin-strip-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
  animation-delay: calc(var(--i) * 45ms + 60ms);
}

.admin-content__strip-block--empty {
  background: transparent;
  border: 1px dashed var(--data-core);
  opacity: 1;
  transform: none;
  animation: none;
}

.admin-content__strip-index {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.admin-content__strip-index span {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  text-align: center;
}

.admin-content__strip-caption {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  letter-spacing: 0.08em;
  text-align: right;
}

@keyframes admin-strip-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .admin-content__strip-block {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

/* ── Loading / Error ── */
.admin-content__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-content__error {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
  padding: var(--space-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border-radius: var(--radius-md);
}

.admin-content__error-text { margin: 0; font-size: var(--text-sm); color: var(--destructive); }

.admin-content__toolbar { display: flex; gap: var(--space-sm); justify-content: flex-end; flex-wrap: wrap; }

/* ── Table bài học (DESIGN §4.6) ── */
.admin-content__table {
  padding: 0;
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.admin-content__table-scroll { overflow-x: auto; border-radius: inherit; }

.admin-content__table table { width: 100%; border-collapse: collapse; }

.admin-content__table th {
  text-align: left;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-tertiary);
  padding: 0 var(--space-md);
  height: 40px;
  border-bottom: 1px solid var(--border);
  background: var(--muted);
  white-space: nowrap;
}

.admin-content__table td {
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.admin-content__table tbody tr { transition: background-color 150ms var(--ease-out-expo); }

.admin-content__table tbody tr:hover { background: color-mix(in srgb, var(--muted) 50%, transparent); }

.admin-content__table tbody tr:last-child td { border-bottom: none; }

/* ── Sort header (UI-PREMIUM 1D) — indicator animation (ArrowUp/Down) ── */
.admin-content__sort {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: 0;
  border: none;
  background: transparent;
  font: inherit;
  color: inherit;
  cursor: pointer;
}

.admin-content__sort:hover { color: var(--foreground); }

.admin-content__sort-arrows {
  display: inline-flex;
  flex-direction: column;
  gap: 1px;
  color: var(--foreground-tertiary);
}

.admin-content__sort-arrow {
  opacity: 0.35;
  transition:
    opacity 150ms var(--ease-out-expo),
    transform 150ms var(--ease-out-expo),
    color 150ms var(--ease-out-expo);
}

.admin-content__sort-arrow--on {
  opacity: 1;
  color: var(--primary);
  transform: scale(1.2) translateY(0);
}

.admin-content__sort-arrow:not(.admin-content__sort-arrow--on) {
  transform: scale(0.85);
}

/* ── Confirm delete ── */
.admin-content__confirm-text {
  margin: 0;
  font-size: var(--text-sm);
  line-height: 1.7;
  color: var(--foreground-secondary);
}

.admin-content__confirm-text strong { color: var(--foreground); font-weight: 600; }

.admin-content__idx {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  white-space: nowrap;
}

.admin-content__title-cell { min-width: 0; }

.admin-content__title-text { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 280px; }

.admin-content__title-desc { font-size: var(--text-xs); color: var(--foreground-tertiary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 280px; }

.admin-content__sim-count {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  font-variant-numeric: tabular-nums;
}

.admin-content__sim-count--zero { color: var(--foreground-tertiary); }

.admin-content__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

/* ── Topic grid ── */
.admin-content__topics { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: var(--space-md); }

.admin-content__topic { min-width: 0; border-color: var(--border); transition: border-color 150ms; }

.admin-content__topic:hover { border-color: var(--border-strong); }

.admin-content__topic-head { display: flex; flex-direction: row; align-items: flex-start; gap: var(--space-sm); }

.admin-content__topic-icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.admin-content__topic-meta { min-width: 0; flex: 1; }

.admin-content__topic-name {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
}

.admin-content__topic-desc { font-size: var(--text-sm); display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

.admin-content__topic-count { flex-shrink: 0; }

/* ── Modal form ── */
.admin-content__form { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-content__row { display: flex; flex-direction: column; gap: var(--space-xs); }

/* Select/textarea chưa có wrapper shadcn — giữ .input nhưng token + easing chuẩn */
.admin-content__row .input,
.admin-content__html {
  background: var(--card);
  border-color: var(--border);
  color: var(--foreground);
  font-size: var(--text-sm);
  transition: border-color 150ms;
}

.admin-content__html { font-family: var(--font-mono); font-size: var(--text-xs); resize: vertical; }

.admin-content__actions { display: flex; justify-content: flex-end; gap: var(--space-sm); }

@media (max-width: 640px) {
  .admin-content__hero { padding: var(--space-lg); }
  .admin-content__hero-strip { flex-basis: 100%; }
  .admin-content__strip-caption { text-align: left; }
}

/* Bảng → card-stack mobile (responsive.css .table-stack — <768px) + tinh chỉnh riêng */
@media (max-width: 767px) {
  .admin-content__table-scroll { overflow-x: visible; }

  .admin-content__table td:first-child { grid-column: 1 / -1; }
  .admin-content__table td:last-child { align-items: flex-start; }

  .admin-content__title-text,
  .admin-content__title-desc { max-width: 100%; white-space: normal; }
}
</style>
