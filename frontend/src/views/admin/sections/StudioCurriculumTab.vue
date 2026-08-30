<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  Check,
  FolderPlus,
  Layers,
  Lock,
  MoreVertical,
  Network,
  Pencil,
  School,
  Send,
  Trash2,
} from 'lucide-vue-next';
import { courseApi, type CourseListDto } from '@/services/courseApi';
import {
  createPathItem,
  deletePathItem,
  fetchPathTree,
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

// ── Danh sách lộ trình của tôi ──
const paths = ref<CourseListDto[]>([]);
const loadingPaths = ref(false);

async function loadPaths(): Promise<void> {
  loadingPaths.value = true;
  try {
    const all = await courseApi.getCourses();
    const myId = auth.user?.id;
    paths.value = all.filter(
      (p) => auth.role === 'ADMIN' || p.createdBy == null || myId == null || p.createdBy === myId,
    );
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tải danh sách lộ trình.', 'error');
  } finally {
    loadingPaths.value = false;
  }
}

// ── Lộ trình đang chọn (đồng bộ query ?courseId=) ──
const selectedPathId = ref<number | null>(null);
const selectedPath = computed(
  () => paths.value.find((p) => Number(p.id) === selectedPathId.value) ?? null,
);

function syncFromRoute(): void {
  const q = route.query.courseId;
  const id = Array.isArray(q) ? Number(q[0]) : Number(q);
  selectedPathId.value = Number.isFinite(id) && id > 0 ? id : null;
}

function selectPath(id: number | null): void {
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
};

const selectedPathStatus = computed(() => {
  const status = selectedPath.value?.status ?? 'draft';
  return { key: status, label: pathStatusLabel[status] ?? 'Nháp' };
});

// ── Cây nội dung ──
const tree = ref<PathItemDto[]>([]);
const loadingTree = ref(false);

async function loadTree(): Promise<void> {
  if (selectedPathId.value == null) {
    tree.value = [];
    return;
  }
  loadingTree.value = true;
  try {
    tree.value = await fetchPathTree(selectedPathId.value);
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
  busy.value = true;
  try {
    const created = await createPathItem(selectedPathId.value, {
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
async function handleItemSaved(): Promise<void> {
  await loadTree();
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

// Đổi tên lộ trình
const renameModalOpen = ref(false);
const renamePathTitle = ref('');
const renamingPath = ref(false);

function openRenamePath(): void {
  if (!selectedPath.value) return;
  pathMenuOpen.value = false;
  renamePathTitle.value = selectedPath.value.title;
  renameModalOpen.value = true;
}

async function handleRenamePath(): Promise<void> {
  if (!selectedPath.value) return;
  const next = renamePathTitle.value.trim();
  if (!next || next === selectedPath.value.title) {
    renameModalOpen.value = false;
    return;
  }
  renamingPath.value = true;
  try {
    await courseApi.updateCourse(selectedPath.value.id, { title: next });
    await loadPaths();
    renameModalOpen.value = false;
    ui.showToast('Đã đổi tên lộ trình.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể đổi tên lộ trình.', 'error');
  } finally {
    renamingPath.value = false;
  }
}

// Chế độ hiển thị (D6)
const changingVisibility = ref(false);

async function handleSetVisibility(scope: 'draft' | 'class' | 'public'): Promise<void> {
  const path = selectedPath.value;
  if (!path || changingVisibility.value) return;
  pathMenuOpen.value = false;
  changingVisibility.value = true;
  try {
    if (scope === 'public') {
      await courseApi.submitCourseForReview(path.id);
      ui.showToast('Đã gửi lộ trình để admin duyệt công khai.', 'success');
    } else {
      await courseApi.updateCourse(path.id, { title: path.title, scope });
      ui.showToast(scope === 'draft' ? 'Lộ trình đã về chế độ Nháp.' : 'Lộ trình đã mở cho lớp học.', 'success');
    }
    await loadPaths();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể đổi chế độ hiển thị.', 'error');
  } finally {
    changingVisibility.value = false;
  }
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

// ── Khởi động ──
onMounted(async () => {
  document.addEventListener('click', handleDocClick);
  syncFromRoute();
  await loadPaths();
  // Query trỏ tới lộ trình không còn trong danh sách → bỏ chọn.
  if (selectedPathId.value != null && !selectedPath.value) selectPath(null);
  if (selectedPathId.value != null) await loadTree();
});

onUnmounted(() => {
  document.removeEventListener('click', handleDocClick);
});

watch(
  () => route.query.courseId,
  () => {
    syncFromRoute();
    if (selectedPathId.value != null && !tree.value.length) void loadTree();
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
          :value="selectedPathId ?? ''"
          :disabled="loadingPaths"
          class="min-w-0 max-w-full flex-1 sm:flex-none sm:w-72 px-2.5 py-1.5 text-xs font-bold bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500 disabled:opacity-50 cursor-pointer"
          aria-label="Chọn lộ trình để chỉnh sửa"
          @change="selectPath(($event.target as HTMLSelectElement).value ? Number(($event.target as HTMLSelectElement).value) : null)"
        >
          <option value="" disabled>— Chọn lộ trình —</option>
          <option v-for="p in paths" :key="p.id" :value="Number(p.id)">{{ p.title }}</option>
        </select>

        <span
          v-if="selectedPath"
          class="text-[10px] font-extrabold px-1.5 py-0.5 rounded border shrink-0"
          :class="selectedPathStatus.key === 'active' ? 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' : selectedPathStatus.key === 'pending_review' ? 'bg-amber-500/20 text-amber-300 border-amber-500/30' : 'bg-slate-500/20 text-slate-300 border-slate-500/30'"
        >
          {{ selectedPathStatus.label }}
        </span>
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <Button
          size="sm"
          variant="primary"
          data-testid="create-path"
          :loading="creatingPath"
          @click="handleCreatePath"
        >
          <FolderPlus :size="14" aria-hidden="true" /> Tạo lộ trình
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
              type="button"
              role="menuitem"
              data-testid="path-visibility-draft"
              class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-slate-300 hover:bg-white/5 text-left cursor-pointer"
              @click="handleSetVisibility('draft')"
            >
              <Lock :size="13" /> Nháp — chỉ tôi thấy
            </button>
            <button
              type="button"
              role="menuitem"
              data-testid="path-visibility-class"
              class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-slate-300 hover:bg-white/5 text-left cursor-pointer"
              @click="handleSetVisibility('class')"
            >
              <School :size="13" /> Mở cho lớp học
            </button>
            <button
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

    <!-- Cây + panel soạn (D7 + D8) -->
    <div v-else class="grid gap-4" :class="editorOpen ? 'lg:grid-cols-[minmax(0,1fr)_460px]' : 'grid-cols-1'">
      <div class="relative min-w-0">
        <div
          v-if="loadingTree"
          class="absolute inset-0 z-20 flex items-center justify-center rounded-2xl bg-[#12111a]/70"
          role="status"
          aria-label="Đang tải cây nội dung"
        >
          <span class="text-xs font-bold text-slate-400">Đang tải cây nội dung…</span>
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

      <ItemEditorSlideOver
        v-if="editorOpen"
        :open="editorOpen"
        :item="editedItem"
        :path-id="selectedPathId ?? 0"
        @close="closeEditor"
        @saved="handleItemSaved"
      />
    </div>

    <!-- Modal đổi tên lộ trình -->
    <Modal :open="renameModalOpen" title="Đổi tên lộ trình" @close="renameModalOpen = false">
      <div class="space-y-3 pt-1">
        <div>
          <label for="rename-path-input" class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
            Tên lộ trình <span class="text-rose-400">*</span>
          </label>
          <input
            id="rename-path-input"
            v-model="renamePathTitle"
            type="text"
            data-testid="rename-path-input"
            placeholder="Ví dụ: Cấu trúc dữ liệu & Thuật toán — Nhập môn"
            class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            @keydown.enter="handleRenamePath"
          />
        </div>
      </div>
      <template #footer>
        <Button variant="ghost" size="sm" @click="renameModalOpen = false">Hủy</Button>
        <Button
          variant="primary"
          size="sm"
          data-testid="rename-path-confirm"
          :loading="renamingPath"
          :disabled="!renamePathTitle.trim()"
          @click="handleRenamePath"
        >
          <Check :size="13" aria-hidden="true" /> Lưu tên mới
        </Button>
      </template>
    </Modal>
  </div>
</template>
