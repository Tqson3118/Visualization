<script setup lang="ts">
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  BookOpen,
  Check,
  Code,
  Edit3,
  Folder,
  FolderPlus,
  HelpCircle,
  Layers,
  Lock,
  MoreVertical,
  Network,
  Pencil,
  School,
  Send,
  Settings,
  Save,
  Trash2,
  PanelLeftClose,
  PanelLeftOpen,
  Tags,
  Plus,
  AlertCircle,
} from 'lucide-vue-next';
import { courseApi, type CourseListDto } from '@/services/courseApi';
import {
  createPathItem,
  deletePathItem,
  fetchPathTree,
  findPathItemByLesson,
  movePathItem,
  updatePathItem,
  type PathItemDto,
  type PathItemType,
} from '@/api/pathItems';
import OutlineTree from '@/components/studio/OutlineTree.vue';
import ItemEditorSlideOver from '@/components/studio/ItemEditorSlideOver.vue';
import type { OutlineMoveTarget } from '@/components/studio/outlineTreeContext';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { useConfirm } from '@/composables/useConfirm';
import { fetchTopics, createTopic, updateTopic, deleteTopic, type Topic } from '@/api/lessons';
import { formatDate } from '@/utils/format';

/**
 * Studio > Lộ trình — màn curriculum tối giản theo plan §5.1 / D7 / D8:
 * chỉ còn dropdown lộ trình + 2 nút chính (Tạo lộ trình, menu ⋯) + cây outline
 * + panel soạn trượt bên phải. Không còn banner, toggle chế độ xem hay list tổng hợp.
 */
const route = useRoute();
const router = useRouter();
const ui = useUiStore();
const auth = useAuthStore();
const { confirm } = useConfirm();

const emit = defineEmits<{
  (e: 'dirtyChange', val: boolean): void;
}>();

const isEditorDirty = ref(false);
function handleDirtyChange(val: boolean): void {
  isEditorDirty.value = val;
  emit('dirtyChange', val);
}

// ── Danh sách lộ trình của tôi ──
const paths = ref<CourseListDto[]>([]);
const loadingPaths = ref(false);

async function loadPaths(): Promise<void> {
  loadingPaths.value = true;
  try {
    const all = await courseApi.getCourses();
    const myId = auth.user?.id;
    paths.value = all.filter(
      (p) => auth.role === 'ADMIN' || p.createdBy == null || p.createdBy <= 1 || myId == null || p.createdBy === myId || p.authorId === myId,
    );
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tải danh sách lộ trình.', 'error');
  } finally {
    loadingPaths.value = false;
  }
}

// ── Lộ trình đang chọn (đồng bộ query ?courseId=) ──
const selectedPathId = ref<string | number | null>(null);
const selectedPath = computed(
  () => paths.value.find((p) => String(p.id) === String(selectedPathId.value ?? '')) ?? null,
);

function syncFromRoute(): void {
  const q = route.query.courseId;
  const id = Array.isArray(q) ? q[0] : q;
  selectedPathId.value = id != null && String(id).length > 0 ? (Number.isNaN(Number(id)) ? String(id) : Number(id)) : null;
}

function selectPath(id: string | number | null): void {
  selectedPathId.value = id;
  void router.replace({ query: { ...route.query, courseId: id != null ? String(id) : undefined } });
  if (id != null) void loadTree();
  else {
    tree.value = [];
    closeEditor();
  }
}

const pathStatusLabel: Record<string, string> = {
  draft: 'Nháp',
  pending_review: 'Chờ duyệt',
  active: 'Công khai',
  rejected: 'Bị từ chối',
  class: 'Lớp học',
  classonly: 'Lớp học',
};

const selectedPathStatus = computed(() => {
  // pendingScope = lựa chọn mới từ menu (⋯) chưa bấm "Lưu lộ trình" — hiển thị kèm "(chưa lưu)".
  const status = pendingScope.value
    ? (pendingScope.value === 'class' ? 'classonly' : pendingScope.value)
    : (selectedPath.value?.status ?? 'draft');
  const label = pathStatusLabel[status] ?? 'Nháp';
  return { key: status, label: pendingScope.value ? label + ' (chưa lưu)' : label };
});

// ── Cây nội dung ──
const tree = ref<PathItemDto[]>([]);
const loadingTree = ref(false);

async function loadTree(): Promise<void> {
  if (selectedPathId.value == null) {
    tree.value = [];
    return;
  }
  const numericId = Number(selectedPathId.value);
  if (!Number.isFinite(numericId)) {
    tree.value = [];
    return;
  }
  loadingTree.value = true;
  try {
    tree.value = await fetchPathTree(numericId);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể nạp cây nội dung.', 'error');
    tree.value = [];
  } finally {
    loadingTree.value = false;
  }
}

function findTreeItem(list: PathItemDto[], id: number): PathItemDto | null {
  for (const item of list) {
    if (item.id === id) return item;
    if (item.children?.length) {
      const found = findTreeItem(item.children, id);
      if (found) return found;
    }
  }
  return null;
}

// ── Panel soạn (D8) ──
const treeCollapsed = ref(false);
const selectedItemId = ref<number | null>(null);
const editorOpen = ref(false);
const editedItem = computed<PathItemDto | null>(() =>
  selectedItemId.value != null ? findTreeItem(tree.value, selectedItemId.value) : null,
);

function openEditor(item: PathItemDto): void {
  selectedItemId.value = item.id;
  editorOpen.value = true;
}

function closeEditor(): void {
  editorOpen.value = false;
  selectedItemId.value = null;
}

const busy = ref(false);

// ── Tạo lộ trình ──
const creatingPath = ref(false);

async function handleCreatePath(): Promise<void> {
  creatingPath.value = true;
  try {
    const created = await courseApi.createCourse({ title: 'Lộ trình mới', scope: 'draft' });
    await loadPaths();
    selectPath(Number(created.id));
    ui.showToast('Đã tạo lộ trình. Bước tiếp: bấm Thêm mục → Chương (Module) → rồi thêm bài học vào từng chương.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tạo lộ trình.', 'error');
  } finally {
    creatingPath.value = false;
  }
}

// ── Thêm mục ──
const DEFAULT_TITLES: Record<PathItemType, string> = {
  folder: 'Module mới',
  theory: 'Bài lý thuyết mới',
  quiz: 'Quiz trắc nghiệm mới',
  lab: 'Lab thực hành mới',
};

async function handleAddItem(type: PathItemType, parentId: number | null): Promise<void> {
  if (selectedPathId.value == null || busy.value) return;
  if (parentId == null && type !== 'folder') {
    ui.showToast('Bắt buộc phải tạo hoặc chọn một Chương (Module) trước khi thêm bài học!', 'warning');
    return;
  }
  busy.value = true;
  try {
    const created = await createPathItem(Number(selectedPathId.value), {
      itemType: type,
      title: DEFAULT_TITLES[type],
      parentId,
    });
    await loadTree();
    openEditor(created);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể thêm mục mới.', 'error');
  } finally {
    busy.value = false;
  }
}

// ── Đổi tên mục ──
async function handleRenameItem(item: PathItemDto, title: string): Promise<void> {
  if (busy.value) return;
  busy.value = true;
  try {
    await updatePathItem(item.id, { title });
    await loadTree();
    ui.showToast('Đã đổi tên mục.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể đổi tên mục.', 'error');
  } finally {
    busy.value = false;
  }
}

// ── Xóa mục ──
async function handleDeleteItem(item: PathItemDto): Promise<void> {
  const ok = await confirm({
    title: 'Xóa mục này?',
    message: 'Xóa "' + (item.title || 'mục chưa đặt tên') + '" khỏi lộ trình? Các mục con (nếu có) cũng sẽ bị xóa.',
    confirmLabel: 'Xóa',
    variant: 'danger',
  });
  if (!ok || busy.value) return;
  busy.value = true;
  try {
    await deletePathItem(item.id);
    if (selectedItemId.value === item.id) closeEditor();
    await loadTree();
    ui.showToast('Đã xóa mục.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể xóa mục.', 'error');
  } finally {
    busy.value = false;
  }
}

// ── Kéo–thả / di chuyển (POST /items/{id}/move) ──
async function handleMoveItem(item: PathItemDto, target: OutlineMoveTarget): Promise<void> {
  if (busy.value) return;
  busy.value = true;
  try {
    await movePathItem(item.id, { parentId: target.parentId, sortOrder: target.sortOrder });
    await loadTree();
    ui.showToast('Đã di chuyển mục.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể di chuyển mục.', 'error');
    await loadTree();
  } finally {
    busy.value = false;
  }
}

// ── Lưu từ panel soạn ──
async function handleItemSaved(savedItem?: PathItemDto): Promise<void> {
  await loadTree();
  if (savedItem?.id) {
    const curId = savedItem.id;
    selectedItemId.value = null;
    await nextTick();
    selectedItemId.value = curId;
  }
}

// ── Menu lộ trình (⋯) ──
const pathMenuOpen = ref(false);
const pathMenuRef = ref<HTMLElement | null>(null);

function togglePathMenu(): void {
  pathMenuOpen.value = !pathMenuOpen.value;
}

function handleDocClick(e: MouseEvent): void {
  if (pathMenuOpen.value && pathMenuRef.value && !pathMenuRef.value.contains(e.target as Node)) {
    pathMenuOpen.value = false;
  }
}

// Cài đặt / Sửa thông tin lộ trình
const availableTopics = ref<Topic[]>([]);
const knowledgeTopics = computed(() => {
  return availableTopics.value.filter((t) => !t.name.startsWith('Module '));
});
const editPathModalOpen = ref(false);
const savingPath = ref(false);
const editPathForm = ref({
  title: '',
  description: '',
  difficulty: 'Beginner',
  topicId: null as number | null,
  scope: 'Draft' as 'Draft' | 'ClassOnly' | 'Public',
  learningObjectives: [] as string[],
  keyOutcomes: [] as string[],
});

watch(knowledgeTopics, (topics) => {
  if (topics.length > 0 && !editPathForm.value.topicId) {
    const dTopic = topics.find((t) => t.name.toLowerCase().includes('cấu trúc dữ liệu') || t.name.toLowerCase().includes('dsa')) || topics[0];
    if (dTopic) editPathForm.value.topicId = dTopic.id;
  }
}, { immediate: true });

// ── Quản lý Danh mục Chủ đề (Topic CRUD) ──
const manageTopicsModalOpen = ref(false);
const editingTopicId = ref<number | null>(null);
const topicForm = ref({
  name: '',
  description: '',
  sortOrder: 1,
});
const savingTopic = ref(false);
const deletingTopicId = ref<number | null>(null);
const topicError = ref('');

function openManageTopicsModal(): void {
  manageTopicsModalOpen.value = true;
  resetTopicForm();
}

function resetTopicForm(): void {
  editingTopicId.value = null;
  topicForm.value = { name: '', description: '', sortOrder: knowledgeTopics.value.length + 1 };
  topicError.value = '';
}

function startEditTopic(t: Topic): void {
  editingTopicId.value = t.id;
  topicForm.value = {
    name: t.name,
    description: t.description || '',
    sortOrder: (t as any).sortOrder || 1,
  };
  topicError.value = '';
}

async function handleSaveTopic(): Promise<void> {
  if (!topicForm.value.name.trim()) {
    topicError.value = 'Tên chủ đề không được để trống.';
    return;
  }
  savingTopic.value = true;
  topicError.value = '';
  try {
    if (editingTopicId.value) {
      await updateTopic(editingTopicId.value, {
        name: topicForm.value.name.trim(),
        description: topicForm.value.description.trim() || undefined,
        sortOrder: topicForm.value.sortOrder,
      });
      ui.showToast('Cập nhật chủ đề thành công!', 'success');
    } else {
      await createTopic({
        name: topicForm.value.name.trim(),
        description: topicForm.value.description.trim() || undefined,
        sortOrder: topicForm.value.sortOrder,
      });
      ui.showToast('Tạo chủ đề mới thành công!', 'success');
    }
    availableTopics.value = await fetchTopics();
    resetTopicForm();
  } catch (err) {
    topicError.value = err instanceof Error ? err.message : 'Không thể lưu chủ đề.';
  } finally {
    savingTopic.value = false;
  }
}

async function handleDeleteTopic(t: Topic): Promise<void> {
  const confirmed = window.confirm(`Bạn có chắc chắn muốn xóa chủ đề "${t.name}"?`);
  if (!confirmed) return;
  deletingTopicId.value = t.id;
  try {
    await deleteTopic(t.id);
    ui.showToast(`Đã xóa chủ đề "${t.name}" thành công!`, 'success');
    availableTopics.value = await fetchTopics();
    if (editPathForm.value.topicId === t.id) {
      editPathForm.value.topicId = knowledgeTopics.value[0]?.id || null;
    }
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể xóa chủ đề.', 'error');
  } finally {
    deletingTopicId.value = null;
  }
}

watch(
  () => route.query.openTopics,
  (val) => {
    if (val === '1' || val === 'true') {
      openManageTopicsModal();
    }
  },
  { immediate: true },
);

const isPublicLocked = computed(() => {
  if (!selectedPath.value) return false;
  const s = String(selectedPath.value.status || '').toLowerCase();
  const isPub = Boolean((selectedPath.value as any).isPublished);
  return s === 'active' || s === 'pending_review' || isPub;
});

const isDraftLocked = computed(() => {
  if (!selectedPath.value) return false;
  const s = String(selectedPath.value.status || '').toLowerCase();
  const isPub = Boolean((selectedPath.value as any).isPublished);
  const vis = String((selectedPath.value as any).visibility || '').toLowerCase();
  return s === 'active' || s === 'pending_review' || s === 'class' || s === 'classonly' || vis === 'classonly' || isPub;
});

function openEditPathModal(): void {
  if (!selectedPath.value) return;
  pathMenuOpen.value = false;
  pendingScope.value = null;
  const currStatus = String(selectedPath.value.status || '').toLowerCase();
  const isPub = Boolean((selectedPath.value as any).isPublished);
  const isClass = currStatus === 'class' || currStatus === 'classonly' || String((selectedPath.value as any).visibility || '').toLowerCase() === 'classonly';
  const currScope: 'Draft' | 'ClassOnly' | 'Public' =
    currStatus === 'active' || currStatus === 'pending_review' || isPub
      ? 'Public'
      : isClass
        ? 'ClassOnly'
        : 'Draft';

  const defaultTopic = knowledgeTopics.value.find((t) => t.name.toLowerCase().includes('cấu trúc dữ liệu') || t.name.toLowerCase().includes('dsa')) || knowledgeTopics.value[0];
  const currentTopicId = (selectedPath.value as any).topicId || defaultTopic?.id || null;

  editPathForm.value = {
    title: selectedPath.value.title || '',
    description: selectedPath.value.description || '',
    difficulty: (selectedPath.value as any).difficulty || 'Beginner',
    topicId: currentTopicId,
    scope: currScope,
    learningObjectives: [...((selectedPath.value as any).learningObjectives || [])],
    keyOutcomes: [...((selectedPath.value as any).keyOutcomes || [])],
  };
  editPathModalOpen.value = true;
}

async function handleSavePath(): Promise<void> {
  const path = selectedPath.value;
  if (!path) return;
  const modalOpen = editPathModalOpen.value;
  const src = modalOpen
    ? editPathForm.value
    : {
        title: path.title || '',
        description: path.description || '',
        difficulty: (path as any).difficulty || 'Beginner',
        topicId: (path as any).topicId ?? null,
        scope: pendingScope.value || undefined,
        learningObjectives: ((path as any).learningObjectives || []) as string[],
        keyOutcomes: ((path as any).keyOutcomes || []) as string[],
      };
  if (!src.title.trim()) {
    ui.showToast('Tên lộ trình không được để trống.', 'warning');
    return;
  }
  savingPath.value = true;
  try {
    const chosenScope = modalOpen
      ? editPathForm.value.scope
      : (pendingScope.value
          ? (pendingScope.value === 'class' ? 'ClassOnly' : 'Draft')
          : ((src as any).scope || undefined));

    await courseApi.updateCourse(path.id, {
      title: src.title.trim(),
      description: src.description.trim(),
      difficulty: src.difficulty,
      topicId: src.topicId ?? undefined,
      scope: chosenScope,
      learningObjectives: src.learningObjectives,
      keyOutcomes: src.keyOutcomes,
    });
    pendingScope.value = null;
    await loadPaths();
    editPathModalOpen.value = false;
    const currentSaved = paths.value.find(p => p.id === path.id);
    const savedStatus = String((currentSaved as any)?.status || '').toLowerCase();
    const scopeLabel = savedStatus === 'classonly' || savedStatus === '2' || chosenScope === 'ClassOnly'
      ? 'Chế độ: Dành cho lớp học'
      : (savedStatus === 'draft' || savedStatus === '0' || chosenScope === 'Draft'
          ? 'Chế độ: Bản nháp'
          : 'Chế độ: Công khai');
    ui.showToast(`Đã lưu lộ trình (${scopeLabel}) thành công!`, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể lưu lộ trình.', 'error');
  } finally {
    savingPath.value = false;
  }
}

// Đổi tên lộ trình
const renameModalOpen = ref(false);
const renamePathTitle = ref('');
const renamingPath = ref(false);

function openRenamePath(): void {
  openEditPathModal();
}

async function handleRenamePath(): Promise<void> {
  await handleSavePath();
}

// Chế độ hiển thị (D6) — chọn Nháp/Lớp học chỉ đánh dấu pending,
// ghi thật khi bấm "Lưu lộ trình" (fix bug tự lưu không qua nút Save).
const changingVisibility = ref(false);
const pendingScope = ref<'draft' | 'class' | null>(null);

async function handleSetVisibility(scope: 'draft' | 'class' | 'public'): Promise<void> {
  const path = selectedPath.value;
  if (!path || changingVisibility.value) return;
  pathMenuOpen.value = false;

  if (scope === 'draft' && isDraftLocked.value) {
    ui.showToast('Lộ trình đã được phát hành, không thể chuyển ngược về Bản nháp.', 'warning');
    return;
  }
  if (scope === 'class' && isPublicLocked.value) {
    ui.showToast('Lộ trình đã xuất bản công khai toàn hệ thống, không thể chuyển về phạm vi lớp riêng.', 'warning');
    return;
  }

  if (scope === 'public') {
    if (!(path as any).topicId && !editPathForm.value.topicId) {
      ui.showToast('Vui lòng chọn Chủ đề cho lộ trình trước khi gửi duyệt công khai.', 'warning');
      openEditPathModal();
      return;
    }
    // Gửi duyệt công khai là hành động tường minh, giữ nguyên gọi ngay.
    changingVisibility.value = true;
    try {
      await courseApi.submitCourseForReview(path.id);
      pendingScope.value = null;
      ui.showToast('Đã gửi lộ trình để admin duyệt công khai.', 'success');
      await loadPaths();
    } catch (err) {
      ui.showToast(err instanceof Error ? err.message : 'Không thể gửi duyệt.', 'error');
    } finally {
      changingVisibility.value = false;
    }
    return;
  }
  pendingScope.value = scope;
  ui.showToast(
    scope === 'draft'
      ? 'Đã chọn chế độ Nháp — bấm "Lưu lộ trình" để áp dụng.'
      : 'Đã chọn chế độ Lớp học — bấm "Lưu lộ trình" để áp dụng.',
    'info',
  );
}

// Xóa lộ trình
async function handleDeletePath(): Promise<void> {
  const path = selectedPath.value;
  if (!path) return;
  pathMenuOpen.value = false;
  const ok = await confirm({
    title: 'Xóa lộ trình?',
    message: 'Xóa vĩnh viễn lộ trình "' + path.title + '" cùng toàn bộ cây nội dung? Hành động này không thể hoàn tác.',
    confirmLabel: 'Xóa lộ trình',
    variant: 'danger',
  });
  if (!ok) return;
  try {
    await courseApi.deleteCourse(path.id);
    selectPath(null);
    await loadPaths();
    ui.showToast('Đã xóa lộ trình.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể xóa lộ trình.', 'error');
  }
}

async function handleLessonIdQuery(): Promise<void> {
  const qLessonId = route.query.lessonId;
  if (!qLessonId) return;
  const numLessonId = Number(Array.isArray(qLessonId) ? qLessonId[0] : qLessonId);
  if (!numLessonId) return;

  try {
    const item = await findPathItemByLesson(numLessonId);
    if (item && item.pathId) {
      if (String(selectedPathId.value) !== String(item.pathId)) {
        selectedPathId.value = item.pathId;
        await loadTree();
      }
      const found = findTreeItem(tree.value, item.id);
      if (found) {
        openEditor(found);
      } else {
        openEditor(item);
      }
    }
  } catch (err) {
    console.warn('Không tìm thấy mục lộ trình theo lessonId:', err);
  }
}

// ── Khởi động ──
onMounted(async () => {
  document.addEventListener('click', handleDocClick);
  await loadPaths();
  syncFromRoute();
  try {
    availableTopics.value = await fetchTopics();
  } catch (e) {
    console.warn('Không thể nạp danh sách chủ đề:', e);
  }
  // Query trỏ tới lộ trình không còn trong danh sách → bỏ chọn.
  if (selectedPathId.value != null && !selectedPath.value) {
    selectPath(null);
  }
  if (selectedPathId.value != null) {
    await loadTree();
  }
  await handleLessonIdQuery();
});

onUnmounted(() => {
  document.removeEventListener('click', handleDocClick);
});

watch(
  () => [route.query.courseId, route.query.lessonId],
  async () => {
    syncFromRoute();
    if (selectedPathId.value != null) await loadTree();
    await handleLessonIdQuery();
  },
);
</script>

<template>
  <div class="space-y-4">
    <!-- Thanh công cụ: dropdown lộ trình + 2 nút chính (plan §5.1) -->
    <div class="flex flex-wrap items-center gap-2.5 rounded-2xl border border-[#262438] bg-[#12111a] px-3.5 py-3">
      <div class="flex items-center gap-2 min-w-0 flex-1">
        <Network class="w-4 h-4 text-purple-400 shrink-0" aria-hidden="true" />
        <label for="studio-path-select" class="text-[11px] font-black uppercase tracking-wider text-slate-400 shrink-0">
          Lộ trình của tôi
        </label>
        <select
          id="studio-path-select"
          data-testid="path-select"
          :value="selectedPathId != null ? String(selectedPathId) : ''"
          :disabled="loadingPaths"
          class="min-w-0 max-w-full flex-1 sm:flex-none sm:w-72 px-2.5 py-1.5 text-xs font-bold bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500 disabled:opacity-50 cursor-pointer"
          aria-label="Chọn lộ trình để chỉnh sửa"
          @change="selectPath(($event.target as HTMLSelectElement).value || null)"
        >
          <option value="" disabled>— Chọn lộ trình —</option>
          <option v-for="p in paths" :key="p.id" :value="String(p.id)">{{ p.title }}</option>
        </select>

        <span
          v-if="selectedPath"
          class="text-[10px] font-extrabold px-1.5 py-0.5 rounded border shrink-0"
          :class="selectedPathStatus.key === 'active' ? 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' : selectedPathStatus.key === 'pending_review' ? 'bg-amber-500/20 text-amber-300 border-amber-500/30' : selectedPathStatus.key === 'class' || selectedPathStatus.key === 'classonly' ? 'bg-sky-500/20 text-sky-300 border-sky-500/30' : 'bg-slate-500/20 text-slate-300 border-slate-500/30'"
        >
          {{ selectedPathStatus.label }}
        </span>
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <Button
          v-if="selectedPath && !selectedItemId"
          size="sm"
          variant="primary"
          data-testid="save-path-btn"
          :loading="savingPath"
          @click="handleSavePath"
        >
          <Save :size="13" aria-hidden="true" /> Lưu lộ trình
        </Button>

        <Button
          v-if="selectedPath"
          size="sm"
          variant="secondary"
          data-testid="edit-path-btn"
          @click="openEditPathModal"
        >
          <Settings :size="13" aria-hidden="true" /> Cài đặt lộ trình
        </Button>

        <Button
          size="sm"
          variant="secondary"
          class="gap-1.5 text-xs font-bold text-purple-300 hover:text-white border-purple-500/30 hover:border-purple-500/60"
          data-testid="manage-topics-btn"
          @click="openManageTopicsModal"
        >
          <Tags :size="14" aria-hidden="true" /> Quản lý chủ đề
        </Button>

        <Button
          size="sm"
          variant="secondary"
          data-testid="create-path"
          :loading="creatingPath"
          @click="handleCreatePath"
        >
          <FolderPlus :size="14" aria-hidden="true" /> Tạo lộ trình mới
        </Button>

        <div v-if="selectedPath" ref="pathMenuRef" class="relative">
          <button
            type="button"
            data-testid="path-menu"
            aria-haspopup="menu"
            :aria-expanded="pathMenuOpen"
            aria-label="Tuỳ chọn lộ trình"
            class="p-2 rounded-lg bg-[#1e1d2c] border border-[#2e2c44] text-slate-300 hover:text-white hover:bg-[#2e2c44] transition-colors cursor-pointer"
            @click.stop="togglePathMenu"
          >
            <MoreVertical :size="14" />
          </button>

          <div
            v-if="pathMenuOpen"
            class="absolute right-0 top-full mt-1.5 w-60 bg-[#1e1d2c] border border-[#36344d] rounded-xl shadow-2xl p-1.5 space-y-1 z-50"
            role="menu"
            aria-label="Tuỳ chọn lộ trình"
          >
            <button
              type="button"
              role="menuitem"
              data-testid="path-menu-rename"
              class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-sky-300 hover:bg-sky-500/10 text-left cursor-pointer"
              @click="openRenamePath"
            >
              <Pencil :size="13" /> Đổi tên lộ trình
            </button>
            <button
              v-if="!isDraftLocked"
              type="button"
              role="menuitem"
              data-testid="path-visibility-draft"
              class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-slate-300 hover:bg-white/5 text-left cursor-pointer"
              @click="handleSetVisibility('draft')"
            >
              <Lock :size="13" /> Nháp — chỉ tôi thấy
            </button>
            <button
              v-if="!isPublicLocked"
              type="button"
              role="menuitem"
              data-testid="path-visibility-public"
              class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-amber-300 hover:bg-amber-500/10 text-left cursor-pointer"
              @click="handleSetVisibility('public')"
            >
              <Send :size="13" /> Công khai — gửi duyệt
            </button>
            <div class="border-t border-[#2e2c44] my-1" />
            <button
              type="button"
              role="menuitem"
              data-testid="path-menu-delete"
              class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-rose-300 hover:bg-rose-500/10 text-left cursor-pointer"
              @click="handleDeletePath"
            >
              <Trash2 :size="13" /> Xóa lộ trình
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Chưa có lộ trình nào -->
    <div
      v-if="!loadingPaths && paths.length === 0"
      class="rounded-2xl border border-dashed border-[#2e2c44] bg-[#12111a] py-14 text-center space-y-3"
    >
      <Layers class="w-10 h-10 text-slate-600 mx-auto" aria-hidden="true" />
      <h2 class="text-base font-black text-slate-200">Chưa có lộ trình nào</h2>
      <p class="text-xs text-slate-500 max-w-md mx-auto leading-relaxed">
        Lộ trình gồm 3 tầng: Tên lộ trình → Chương (Module) → Bài học/Quiz/Lab. Tạo lộ trình đầu tiên để bắt đầu.
      </p>
      <Button size="md" variant="primary" data-testid="create-path-empty" :loading="creatingPath" @click="handleCreatePath">
        <FolderPlus :size="15" aria-hidden="true" /> Tạo lộ trình đầu tiên
      </Button>
    </div>

    <!-- Chưa chọn lộ trình -->
    <div
      v-else-if="!selectedPath"
      class="rounded-2xl border border-dashed border-[#2e2c44] bg-[#12111a] py-14 text-center space-y-3"
    >
      <Network class="w-10 h-10 text-slate-600 mx-auto" aria-hidden="true" />
      <h2 class="text-base font-black text-slate-200">Chọn một lộ trình để chỉnh sửa</h2>
      <p class="text-xs text-slate-500">Chọn lộ trình ở dropdown phía trên, hoặc tạo lộ trình mới.</p>
    </div>

    <!-- Teacher Workbench: Cây (fixed width / collapsible) + Editor (flex-1) -->
    <div v-else class="flex flex-col lg:flex-row gap-3.5 items-start min-h-[calc(100vh-210px)]">
      <!-- Cột cây thu gọn (Focus Mode) -->
      <div
        v-if="treeCollapsed"
        class="hidden lg:flex flex-col items-center py-3 px-1.5 rounded-2xl border border-[#262438] bg-[#12111a] shrink-0 h-[calc(100vh-var(--app-header-h,68px)-32px)] w-12 sticky top-[calc(var(--app-header-h,68px)+16px)] justify-between transition-all"
      >
        <button
          type="button"
          class="p-2 rounded-xl bg-purple-600/20 hover:bg-purple-600 text-purple-300 hover:text-white transition-colors cursor-pointer"
          title="Mở rộng Cây nội dung"
          aria-label="Mở rộng Cây nội dung"
          @click="treeCollapsed = false"
        >
          <PanelLeftOpen class="w-4 h-4" />
        </button>

        <span class="text-[11px] font-black text-slate-500 uppercase tracking-widest [writing-mode:vertical-lr] rotate-180 select-none py-4">
          Cây nội dung ({{ tree.length }})
        </span>

        <Layers class="w-4 h-4 text-slate-600 mb-1" />
      </div>

      <!-- Cột cây nội dung đầy đủ (420px - 520px linh hoạt) -->
      <div
        v-else
        class="w-full lg:w-[420px] xl:w-[480px] 2xl:w-[520px] shrink-0 lg:sticky lg:top-[calc(var(--app-header-h,68px)+16px)] flex flex-col h-auto lg:h-[calc(100vh-var(--app-header-h,68px)-32px)] min-h-0 transition-all"
      >
        <div class="relative min-w-0 flex-1 flex flex-col min-h-0">
          <div
            v-if="loadingTree"
            class="absolute inset-0 z-20 flex items-center justify-center rounded-2xl bg-[#12111a]/70"
            role="status"
            aria-label="Đang tải cây nội dung"
          >
            <span class="text-xs font-bold text-slate-400">Đang tải cây nội dung…</span>
          </div>

          <!-- Nút thu gọn thanh cây nhanh (Desktop Focus Mode) -->
          <div class="hidden lg:flex items-center justify-end pb-1.5">
            <button
              type="button"
              class="text-[10px] font-bold text-slate-400 hover:text-purple-300 flex items-center gap-1 px-2 py-0.5 rounded-lg hover:bg-white/5 transition-colors cursor-pointer"
              title="Thu gọn cây để mở rộng tối đa khu vực soạn thảo"
              @click="treeCollapsed = true"
            >
              <PanelLeftClose class="w-3 h-3" />
              <span>Chế độ tập trung (Ẩn cây)</span>
            </button>
          </div>

          <OutlineTree
            :items="tree"
            :selected-item-id="selectedItemId"
            @select="openEditor"
            @add="handleAddItem"
            @rename="handleRenameItem"
            @move-item="handleMoveItem"
            @delete="handleDeleteItem"
          />
        </div>
      </div>

      <!-- Cột Editor / Workbench Right Side -->
      <div class="flex-1 min-w-0 w-full flex flex-col h-auto lg:h-[calc(100vh-200px)] min-h-[480px]">
        <ItemEditorSlideOver
          v-if="editorOpen && editedItem"
          :open="editorOpen"
          :item="editedItem"
          :path-id="Number(selectedPathId) || 0"
          @close="closeEditor"
          @saved="handleItemSaved"
          @add-child="handleAddItem"
          @select-item="openEditor"
          @dirty-change="handleDirtyChange"
        />

        <!-- Empty State khi chưa chọn item nào -->
        <div
          v-else
          class="h-full rounded-2xl border border-dashed border-[#262438] bg-[#12111a]/70 p-6 md:p-8 flex flex-col justify-center items-center text-center space-y-5"
          data-testid="workbench-empty-guide"
        >
          <div class="w-16 h-16 rounded-3xl bg-gradient-to-br from-purple-600/20 to-sky-600/20 border border-purple-500/30 flex items-center justify-center shadow-lg shadow-purple-950/40">
            <Edit3 class="w-8 h-8 text-purple-400" />
          </div>

          <div class="space-y-1.5 max-w-md">
            <h3 class="text-sm md:text-base font-black text-white uppercase tracking-wider">
              Bàn làm việc Giảng viên
            </h3>
            <p class="text-xs text-slate-400 leading-relaxed">
              Chọn một mục trên <strong class="text-slate-200">Cây nội dung</strong> bên trái để bắt đầu chỉnh sửa, hoặc tạo bài học mới ngay bên dưới.
            </p>
          </div>

          <div class="w-full max-w-xl space-y-3 pt-2">
            <!-- Primary Action: Chương (Module) cho cấp gốc -->
            <button
              type="button"
              data-testid="workbench-add-folder-hero"
              class="w-full flex items-center justify-between p-3.5 rounded-xl bg-gradient-to-r from-amber-500/10 via-purple-500/10 to-[#171624] hover:from-amber-500/20 hover:via-purple-500/20 hover:to-[#201e33] border border-amber-500/30 hover:border-amber-500/60 text-white text-xs font-bold transition-all cursor-pointer group shadow-sm hover:scale-[1.01]"
              @click="handleAddItem('folder', null)"
            >
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-lg bg-amber-500/20 border border-amber-500/30 flex items-center justify-center text-amber-400 group-hover:scale-110 transition-transform shrink-0">
                  <Folder class="w-5 h-5" />
                </div>
                <div class="text-left">
                  <div class="flex items-center gap-2">
                    <span class="text-sm font-bold text-white">+ Thêm Chương (Module mới)</span>
                    <span class="text-[9px] uppercase font-black tracking-wider px-1.5 py-0.5 rounded bg-amber-500/20 text-amber-300 border border-amber-500/30">Cấp 1</span>
                  </div>
                  <p class="text-[11px] text-slate-400 font-normal mt-0.5">Tạo các Chương ở cấp gốc trước (Chương 1, Chương 2...), sau đó thêm bài học vào trong chương.</p>
                </div>
              </div>
              <span class="text-xs font-black text-amber-400 shrink-0 pr-1">Tạo chương →</span>
            </button>

            <!-- Hướng dẫn quy trình soạn thảo chuẩn -->
            <div class="p-3 bg-[#171624]/90 border border-[#27253b] rounded-xl text-left space-y-1.5">
              <span class="text-[11px] font-bold text-amber-300 flex items-center gap-1.5">
                <FolderPlus class="w-3.5 h-3.5 text-amber-400" /> Quy tắc soạn thảo giáo trình:
              </span>
              <ul class="text-xs text-slate-400 space-y-1 list-disc list-inside leading-relaxed">
                <li><strong class="text-slate-200">Cấp 1:</strong> Bắt buộc là <strong class="text-amber-300">Chương (Module)</strong> để quản lý cấu trúc bài giảng rõ ràng.</li>
                <li><strong class="text-slate-200">Trong Chương:</strong> Bấm vào Chương trên cây hoặc biểu tượng <strong class="text-purple-300">+</strong> để chọn thêm <strong class="text-sky-300">Lý thuyết</strong>, <strong class="text-orange-300">Quiz trắc nghiệm</strong>, hoặc <strong class="text-emerald-300">Codelab</strong>.</li>
              </ul>
            </div>
          </div>

          <div class="flex items-center gap-4 text-[11px] text-slate-500 pt-2 border-t border-[#262438]/80">
            <span class="flex items-center gap-1.5">
              <span class="w-1.5 h-1.5 rounded-full bg-emerald-400" /> Kéo thả sắp xếp
            </span>
            <span class="flex items-center gap-1.5">
              <span class="w-1.5 h-1.5 rounded-full bg-purple-400" /> Tự động lưu bản nháp
            </span>
            <span class="flex items-center gap-1.5">
              <span class="w-1.5 h-1.5 rounded-full bg-sky-400" /> Nhúng 44 mô phỏng
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Cài đặt & Lưu Lộ trình -->
    <Modal
      :open="editPathModalOpen"
      title="Cài đặt & Lưu thông tin Lộ trình"
      :is-dirty="Boolean(editPathForm.title || editPathForm.description)"
      @close="editPathModalOpen = false"
    >
      <div class="space-y-4 pt-1 max-h-[70vh] overflow-y-auto pr-1">
        <!-- Trạng thái kiểm duyệt hiện tại -->
        <div v-if="selectedPath" class="p-3 rounded-xl bg-[#141224] border border-[#2e2c44] flex items-center justify-between text-xs">
          <div class="flex items-center gap-2">
            <span class="text-slate-400 font-medium">Trạng thái:</span>
            <span
              class="px-2 py-0.5 rounded-md text-[11px] font-black border"
              :class="selectedPath.status === 'active' ? 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' : selectedPath.status === 'pending_review' ? 'bg-amber-500/20 text-amber-300 border-amber-500/30' : selectedPath.status === 'rejected' ? 'bg-rose-500/20 text-rose-300 border-rose-500/30' : 'bg-slate-500/20 text-slate-300 border-slate-500/30'"
            >
              {{ selectedPath.status === 'active' ? 'Đã xuất bản (Công khai)' : selectedPath.status === 'pending_review' ? 'Đang chờ Admin duyệt' : selectedPath.status === 'rejected' ? 'Bị từ chối' : (selectedPath.status === 'class' || selectedPath.status === 'classonly' ? 'Dành cho lớp học' : 'Bản nháp') }}
            </span>
          </div>
          <span v-if="selectedPath.submittedAt && selectedPath.status === 'pending_review'" class="text-[11px] text-slate-400 font-medium">
            Đã gửi: {{ formatDate(selectedPath.submittedAt) }}
          </span>
        </div>
        <div v-if="selectedPath?.rejectionReason && selectedPath.status === 'rejected'" class="p-2.5 rounded-xl bg-rose-500/10 border border-rose-500/30 text-rose-300 text-xs">
          <strong>Lý do từ chối:</strong> {{ selectedPath.rejectionReason }}
        </div>

        <div>
          <label for="edit-path-title" class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
            Tên lộ trình <span class="text-rose-400">*</span>
          </label>
          <input
            id="edit-path-title"
            v-model="editPathForm.title"
            type="text"
            placeholder="Ví dụ: Cấu trúc dữ liệu & Thuật toán — Nhập môn"
            class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
          />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <div class="flex items-center justify-between mb-1">
              <label for="edit-path-topic" class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider">
                Chủ đề kiến thức ({{ knowledgeTopics.length }} Chủ đề)
              </label>
              <button
                type="button"
                class="text-[11px] text-purple-400 hover:text-purple-300 font-bold hover:underline cursor-pointer flex items-center gap-1"
                @click="openManageTopicsModal"
              >
                <Tags :size="12" /> Quản lý chủ đề
              </button>
            </div>
            <select
              id="edit-path-topic"
              v-model="editPathForm.topicId"
              class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500 cursor-pointer"
            >
              <option v-if="knowledgeTopics.length === 0" :value="null">Cấu trúc dữ liệu &amp; Giải thuật</option>
              <option v-for="t in knowledgeTopics" :key="t.id" :value="t.id">{{ t.name }}</option>
            </select>
          </div>

          <div>
            <label for="edit-path-diff" class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
              Độ khó / Cấp độ
            </label>
            <select
              id="edit-path-diff"
              v-model="editPathForm.difficulty"
              class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500 cursor-pointer"
            >
              <option value="Beginner">Cơ bản (Beginner / Dễ)</option>
              <option value="Intermediate">Trung cấp (Intermediate / TB)</option>
              <option value="Advanced">Nâng cao (Advanced / Khó)</option>
            </select>
          </div>
        </div>

        <div>
          <label for="edit-path-scope" class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
            Phạm vi hiển thị & Mục đích sử dụng
          </label>
          <select
            id="edit-path-scope"
            v-model="editPathForm.scope"
            class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500 cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed"
            :disabled="isPublicLocked"
          >
            <option value="ClassOnly" :disabled="isPublicLocked">
              Dành cho lớp học (ClassOnly — Chỉ hiển thị trong lớp riêng)
            </option>
            <option value="Public">
              {{ auth.role === 'ADMIN' ? 'Công khai (Public — Xuất bản toàn hệ thống ngay)' : 'Công khai (Public — Gửi Admin kiểm duyệt)' }}
            </option>
            <option value="Draft" :disabled="isDraftLocked">
              Bản nháp (Draft — Chỉ tác giả thấy{{ isDraftLocked ? ' — Đã khóa' : '' }})
            </option>
          </select>

          <!-- Ghi chú giải thích khóa trạng thái -->
          <p v-if="isPublicLocked" class="text-[11px] text-purple-300 mt-1.5 flex items-start gap-1.5 font-medium leading-normal bg-purple-500/10 p-2 rounded-lg border border-purple-500/20">
            <Lock :size="13" class="shrink-0 mt-0.5 text-purple-400" />
            <span>Lộ trình đã xuất bản công khai trên toàn hệ thống. Mọi sửa đổi nội dung giảng dạy được cập nhật trực tiếp tại Studio.</span>
          </p>
          <p v-else-if="isDraftLocked" class="text-[11px] text-amber-400 mt-1.5 flex items-start gap-1.5 font-medium leading-normal bg-amber-500/10 p-2 rounded-lg border border-amber-500/20">
            <Lock :size="13" class="shrink-0 mt-0.5 text-amber-400" />
            <span>Lộ trình đang phục vụ lớp học, tùy chọn chuyển về Bản nháp đã được khóa để bảo vệ tiến độ và dữ liệu học tập của học viên.</span>
          </p>
        </div>
      </div>

      <template #footer>
        <Button variant="ghost" size="sm" @click="editPathModalOpen = false">Hủy</Button>
        <Button
          variant="primary"
          size="sm"
          data-testid="edit-path-confirm"
          :loading="savingPath"
          :disabled="!editPathForm.title.trim()"
          @click="handleSavePath"
        >
          <Check :size="13" aria-hidden="true" /> Lưu lộ trình
        </Button>
      </template>
    </Modal>

    <!-- Modal Quản lý Danh mục Chủ đề Kiến thức (CRUD) -->
    <Modal
      :open="manageTopicsModalOpen"
      title="Quản lý Danh mục Chủ đề Kiến thức"
      @close="manageTopicsModalOpen = false"
    >
      <div class="space-y-4 pt-1 max-h-[75vh] overflow-y-auto pr-1">
        <p class="text-xs text-slate-400 leading-relaxed">
          Tạo mới và quản lý các chuyên đề kiến thức DSA. Toàn bộ giảng viên, quản trị viên và học sinh đều được đồng bộ theo danh mục chủ đề này.
        </p>

        <!-- Form Thêm / Sửa Chủ đề -->
        <div class="p-4 rounded-xl bg-[#141224] border border-[#2e2c44] space-y-3">
          <div class="flex items-center justify-between">
            <span class="text-xs font-bold text-white flex items-center gap-1.5">
              <Tags :size="14" class="text-purple-400" />
              {{ editingTopicId ? 'Chỉnh sửa chủ đề' : 'Thêm chủ đề mới' }}
            </span>
            <button
              v-if="editingTopicId"
              type="button"
              class="text-[11px] text-slate-400 hover:text-white cursor-pointer"
              @click="resetTopicForm"
            >
              Hủy sửa
            </button>
          </div>

          <div v-if="topicError" class="p-2 rounded-lg bg-rose-500/10 border border-rose-500/30 text-rose-300 text-xs flex items-center gap-1.5">
            <AlertCircle :size="14" />
            <span>{{ topicError }}</span>
          </div>

          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
              Tên chủ đề <span class="text-rose-400">*</span>
            </label>
            <input
              v-model="topicForm.name"
              type="text"
              placeholder="Ví dụ: Quy hoạch động, Đồ thị nâng cao, Cây Trie..."
              class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            />
          </div>

          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
              Mô tả tóm tắt
            </label>
            <textarea
              v-model="topicForm.description"
              rows="2"
              placeholder="Mô tả phạm vi kiến thức và các thuật toán trong chủ đề..."
              class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500 resize-none"
            ></textarea>
          </div>

          <div class="flex justify-end gap-2 pt-1">
            <Button
              v-if="editingTopicId"
              variant="ghost"
              size="sm"
              class="text-xs"
              @click="resetTopicForm"
            >
              Hủy
            </Button>
            <Button
              variant="primary"
              size="sm"
              class="text-xs font-bold"
              :loading="savingTopic"
              :disabled="!topicForm.name.trim()"
              @click="handleSaveTopic"
            >
              <Check :size="13" /> {{ editingTopicId ? 'Cập nhật' : 'Tạo chủ đề' }}
            </Button>
          </div>
        </div>

        <!-- Danh sách Chủ đề hiện có -->
        <div>
          <div class="flex items-center justify-between mb-2">
            <span class="text-[11px] font-bold text-slate-300 uppercase tracking-wider">
              Danh sách chủ đề hiện tại ({{ knowledgeTopics.length }})
            </span>
          </div>

          <div class="space-y-1.5">
            <div
              v-for="t in knowledgeTopics"
              :key="t.id"
              class="p-2.5 rounded-xl bg-[#12111d] border border-[#252338] hover:border-[#383556] flex items-center justify-between gap-3 transition-colors"
            >
              <div class="min-w-0 flex-1">
                <div class="flex items-center gap-2">
                  <span class="text-xs font-bold text-white">{{ t.name }}</span>
                  <span class="text-[10px] px-1.5 py-0.2 rounded bg-purple-500/10 text-purple-300 border border-purple-500/20 font-mono">
                    ID: {{ t.id }}
                  </span>
                </div>
                <p v-if="t.description" class="text-[11px] text-slate-400 truncate mt-0.5">
                  {{ t.description }}
                </p>
              </div>

              <div class="flex items-center gap-1 shrink-0">
                <button
                  type="button"
                  class="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-[#252338] transition-colors cursor-pointer"
                  title="Chỉnh sửa chủ đề"
                  @click="startEditTopic(t)"
                >
                  <Pencil :size="13" />
                </button>
                <button
                  type="button"
                  class="p-1.5 rounded-lg text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 transition-colors cursor-pointer"
                  title="Xóa chủ đề"
                  :disabled="deletingTopicId === t.id"
                  @click="handleDeleteTopic(t)"
                >
                  <Trash2 :size="13" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <template #footer>
        <Button variant="secondary" size="sm" @click="manageTopicsModalOpen = false">
          Đóng
        </Button>
      </template>
    </Modal>
  </div>
</template>
