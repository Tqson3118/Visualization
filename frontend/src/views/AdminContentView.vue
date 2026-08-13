<script setup lang="ts">
// AdminContentView — Màn 09: quản lý lessons/topics CRUD cơ bản + gắn mô phỏng
// H-B: hero Aurora soft + Tabs shadcn (badge số lượng) + table hover + topic grid
// Card shadcn (kèm số bài học mỗi chủ đề — tính từ lessons) + modal giữ logic cũ.
import { computed, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowRight, BookOpen, CalendarDays, Layers, Network, Pencil, Plus, Trash2 } from 'lucide-vue-next';

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

/** Số bài học mỗi chủ đề (tính từ danh sách lessons đã tải — presentation only). */
const topicLessonCount = computed(() => {
  const map = new Map<number, number>();
  for (const lesson of lessons.value) {
    map.set(lesson.topicId, (map.get(lesson.topicId) ?? 0) + 1);
  }
  return map;
});

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
    <!-- Hero gradient Aurora soft -->
    <header class="admin-content__hero">
      <div class="admin-content__hero-body">
        <span class="admin-content__hero-icon" aria-hidden="true"><BookOpen :size="24" /></span>
        <div class="admin-content__hero-title-wrap">
          <h1 class="admin-content__title">{{ messages.admin.content.title }}</h1>
          <p class="admin-content__sub">{{ messages.admin.content.subtitle }}</p>
        </div>
        <Badge variant="primary" class="admin-content__hero-badge">{{ messages.admin.badge }}</Badge>
      </div>
    </header>

    <AdminNav active="content" />

    <Tabs :tabs="contentTabs" :model-value="tab" @change="(key: string) => (tab = key as 'lessons' | 'topics')" />

    <div v-if="loading" class="admin-content__loading" aria-busy="true">
      <Skeleton v-for="i in 5" :key="i" height="56px" />
    </div>

    <!-- Danh sách bài học -->
    <template v-else-if="tab === 'lessons'">
      <div class="admin-content__toolbar">
        <Button size="sm" @click="openCreate"><Plus :size="14" /> {{ messages.admin.content.addLesson }}</Button>
        <Button size="sm" variant="ghost" @click="router.push({ name: 'admin-ladder' })">
          {{ messages.admin.content.ladderHint }} <ArrowRight :size="14" />
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

      <div v-else class="admin-content__table card">
        <div class="admin-content__table-scroll">
          <table>
            <thead>
              <tr>
                <th>{{ messages.admin.content.colTitle }}</th>
                <th>{{ messages.admin.content.colTopic }}</th>
                <th>{{ messages.admin.content.colStatus }}</th>
                <th>{{ messages.admin.content.colSim }}</th>
                <th>{{ messages.admin.content.colCreated }}</th>
                <th class="admin-content__actions-col">{{ messages.admin.content.colActions }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="lesson in lessons" :key="lesson.id">
                <td class="admin-content__title-cell">
                  <p class="admin-content__title-text">{{ lesson.title }}</p>
                  <p v-if="lesson.description" class="admin-content__title-desc text-muted">{{ lesson.description }}</p>
                </td>
                <td><Badge variant="secondary">{{ topicName(lesson.topicId) }}</Badge></td>
                <td>
                  <Badge :variant="lesson.status === 'active' ? 'success' : lesson.status === 'draft' ? 'warning' : 'muted'">
                    {{ statusLabel[lesson.status] ?? lesson.status }}
                  </Badge>
                </td>
                <td>
                  <span class="admin-content__sim-count" :class="{ 'admin-content__sim-count--zero': lesson.simulationCount === 0 }">
                    <Network :size="13" /> {{ lesson.simulationCount }}
                  </span>
                </td>
                <td>
                  <span class="text-muted admin-content__date">
                    <CalendarDays :size="13" /> {{ formatDate(new Date()) }}
                  </span>
                </td>
                <td>
                  <div class="admin-content__actions">
                    <Button size="sm" variant="ghost" @click="openEdit(lesson)">
                      <Pencil :size="14" /> {{ messages.admin.content.edit }}
                    </Button>
                    <Button size="sm" variant="danger" @click="deleteLesson(lesson)">
                      <Trash2 :size="14" /> {{ messages.admin.content.delete }}
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
        <Button size="sm" @click="topicFormOpen = true"><Plus :size="14" /> {{ messages.admin.content.addTopic }}</Button>
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
        <Card v-for="topic in topics" :key="topic.id" class="admin-content__topic hover-lift">
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
  </main>
</template>

<style scoped>
.admin-content {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Hero gradient Aurora soft ── */
.admin-content__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.admin-content__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.admin-content__hero::before {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  top: -120px;
  right: -60px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.admin-content__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.admin-content__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.admin-content__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.admin-content__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.admin-content__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.admin-content__hero-badge { margin-left: auto; }

.admin-content__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-content__toolbar { display: flex; gap: var(--space-sm); justify-content: flex-end; flex-wrap: wrap; }

/* ── Table bài học ── */
.admin-content__table { padding: 0; }

.admin-content__table-scroll { overflow-x: auto; border-radius: inherit; }

.admin-content__table table { width: 100%; border-collapse: collapse; min-width: 760px; }

.admin-content__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  background: var(--color-muted);
  white-space: nowrap;
}

.admin-content__table td { padding: var(--space-sm) var(--space-md); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); vertical-align: middle; }

.admin-content__table tbody tr { transition: background-color 150ms ease; }

.admin-content__table tbody tr:hover { background: color-mix(in srgb, var(--color-primary) 5%, transparent); }

.admin-content__table tbody tr:last-child td { border-bottom: none; }

.admin-content__title-cell { min-width: 0; }

.admin-content__title-text { font-weight: 700; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 280px; }

.admin-content__title-desc { font-size: var(--text-xs); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 280px; }

.admin-content__sim-count { display: inline-flex; align-items: center; gap: 4px; font-weight: 600; font-variant-numeric: tabular-nums; }

.admin-content__sim-count--zero { color: var(--color-text-muted); }

.admin-content__date { display: inline-flex; align-items: center; gap: 4px; white-space: nowrap; font-variant-numeric: tabular-nums; }

.admin-content__actions { display: flex; gap: var(--space-xs); flex-wrap: wrap; }

/* ── Topic grid ── */
.admin-content__topics { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: var(--space-md); }

.admin-content__topic-head { display: flex; flex-direction: row; align-items: flex-start; gap: var(--space-sm); }

.admin-content__topic-icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background-image: var(--gradient-mint);
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}

.admin-content__topic-meta { min-width: 0; flex: 1; }

.admin-content__topic-name { font-size: var(--text-md); }

.admin-content__topic-desc { font-size: var(--text-sm); display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

.admin-content__topic-count { flex-shrink: 0; }

/* ── Modal form ── */
.admin-content__form { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-content__row { display: flex; flex-direction: column; gap: var(--space-xs); }

.admin-content__html { font-family: var(--font-mono); font-size: var(--text-xs); resize: vertical; }

.admin-content__actions { display: flex; justify-content: flex-end; gap: var(--space-sm); }

@media (max-width: 640px) {
  .admin-content__hero-badge { margin-left: 0; }
}
</style>
