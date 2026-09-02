<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { BookOpen, ChevronsDownUp, ChevronsUpDown, Code, Folder, HelpCircle, Layers, Network, Plus, Search, X } from 'lucide-vue-next';
import { type PathItemDto, type PathItemType, normalizeItemType } from '@/api/pathItems';
import { normalizeVi } from '@/utils/searchNormalize';
import OutlineNode from './OutlineNode.vue';
import {
  provideOutlineTreeContext,
  type OutlineDropZone,
  type OutlineFolderOption,
  type OutlineMoveTarget,
  type OutlineTreeContext,
} from './outlineTreeContext';

/**
 * OutlineTree — cây nội dung dạng outline (plan §5.1).
 * - Đệ quy không giới hạn tầng (OutlineNode tự gọi lại chính nó).
 * - Kéo–thả sắp thứ tự → phát 'moveItem' kèm { parentId, sortOrder } cho view cha gọi API move.
 * - Nhớ trạng thái mở/đóng folder vào localStorage.
 */
const props = withDefaults(
  defineProps<{
    items: PathItemDto[];
    selectedItemId?: number | null;
    readonly?: boolean;
  }>(),
  {
    selectedItemId: null,
    readonly: false,
  },
);

const emit = defineEmits<{
  (e: 'select', item: PathItemDto): void;
  (e: 'add', type: PathItemType, parentId: number | null): void;
  (e: 'rename', item: PathItemDto, title: string): void;
  (e: 'moveItem', item: PathItemDto, target: OutlineMoveTarget): void;
  (e: 'delete', item: PathItemDto): void;
}>();

// ── Duyệt cây phẳng ──
interface FlatEntry {
  item: PathItemDto;
  depth: number;
}

function flattenTree(list: PathItemDto[], depth = 0, out: FlatEntry[] = []): FlatEntry[] {
  for (const item of list) {
    out.push({ item, depth });
    if (item.children?.length) flattenTree(item.children, depth + 1, out);
  }
  return out;
}

function findItem(list: PathItemDto[], id: number): PathItemDto | null {
  for (const item of list) {
    if (item.id === id) return item;
    if (item.children?.length) {
      const found = findItem(item.children, id);
      if (found) return found;
    }
  }
  return null;
}

/** id có nằm trong cây con gốc item không? */
function contains(item: PathItemDto, id: number): boolean {
  return (item.children ?? []).some((c) => c.id === id || contains(c, id));
}

const flat = computed(() => flattenTree(props.items));
const totalCount = computed(() => flat.value.length);

// ── Tìm kiếm / lọc bài học trên cây ──
const searchQuery = ref('');

const filteredItems = computed(() => {
  const q = normalizeVi(searchQuery.value);
  if (!q) return props.items;

  function filterNode(node: PathItemDto): PathItemDto | null {
    const matchSelf = normalizeVi(node.title || '').includes(q);
    const children = (node.children || []).map(filterNode).filter(Boolean) as PathItemDto[];
    if (matchSelf || children.length > 0) {
      return {
        ...node,
        children,
      };
    }
    return null;
  }

  return props.items.map(filterNode).filter(Boolean) as PathItemDto[];
});

watch(searchQuery, (val) => {
  if (val.trim()) {
    expandAll();
  }
});

// ── Đánh số thứ tự theo cây (pre-order): "1", "1.1", "1.2", "2"… ──
const numbering = computed(() => {
  const map = new Map<number, string>();
  const walk = (list: PathItemDto[], prefix: string): void => {
    list.forEach((item, i) => {
      const p = prefix ? prefix + '.' + String(i + 1) : String(i + 1);
      map.set(item.id, p);
      if (normalizeItemType(item.itemType) === 'folder' && item.children?.length) {
        walk(item.children, p);
      }
    });
  };
  walk(props.items, '');
  return map;
});

function prefixOf(id: number): string {
  return numbering.value.get(id) ?? '';
}

// ── Trạng thái mở/đóng — nhớ localStorage ──
const STORAGE_KEY = 'metqua.studio.outline.open.v1';
const openState = ref<Record<number, boolean>>({});

onMounted(() => {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (raw) Object.assign(openState.value, JSON.parse(raw) as Record<number, boolean>);
  } catch {
    // JSON hỏng — giữ state mặc định.
  }
});

watch(
  openState,
  (value) => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(value));
    } catch {
      // private-mode / storage đầy — bỏ qua.
    }
  },
  { deep: true },
);

function toggleFolder(id: number): void {
  openState.value[id] = !(openState.value[id] ?? true);
}

// Mutate in-place (không thay object) để context cho OutlineNode giữ được reactivity.
function expandAll(): void {
  for (const key of Object.keys(openState.value)) delete openState.value[Number(key)];
}

function collapseAll(): void {
  for (const key of Object.keys(openState.value)) delete openState.value[Number(key)];
  for (const { item } of flat.value) {
    if (normalizeItemType(item.itemType) === 'folder') openState.value[item.id] = false;
  }
}

// ── Menu ngữ cảnh (1 node mở tại 1 thời điểm) ──
const openMenuId = ref<number | null>(null);
const menuMode = ref<'actions' | 'move' | 'add'>('actions');

function openMenu(id: number, mode: 'actions' | 'move' | 'add' = 'actions'): void {
  openMenuId.value = id;
  menuMode.value = mode;
}

function closeMenu(): void {
  openMenuId.value = null;
}

// ── Kéo–thả ──
const draggingId = ref<number | null>(null);
const dropZone = ref<OutlineDropZone | null>(null);

function canDrop(target: PathItemDto): boolean {
  const dragId = draggingId.value;
  if (dragId == null || props.readonly) return false;
  if (dragId === target.id) return false;
  const dragged = findItem(props.items, dragId);
  // Không cho thả vào chính cây con của node đang kéo (tránh cycle).
  if (dragged && contains(dragged, target.id)) return false;
  return true;
}

function dropOn(target: PathItemDto, zone: OutlineDropZone): void {
  const dragId = draggingId.value;
  // Chốt kết quả cho phép TRƯỚC khi xóa trạng thái kéo (canDrop đọc draggingId).
  const allowed = dragId != null && canDrop(target);
  draggingId.value = null;
  dropZone.value = null;
  if (!allowed) return;
  const dragged = findItem(props.items, dragId);
  if (!dragged) return;

  if (zone === 'into') {
    if (normalizeItemType(target.itemType) !== 'folder') return;
    emit('moveItem', dragged, { parentId: target.id, sortOrder: target.children?.length ?? 0 });
    return;
  }
  const parentId = target.parentId ?? null;
  const base = target.sortOrder ?? 1;
  emit('moveItem', dragged, { parentId, sortOrder: zone === 'before' ? base : base + 1 });
}

// ── Đích "Di chuyển đến…" ──
function moveTargets(item: PathItemDto): OutlineFolderOption[] {
  const folders: OutlineFolderOption[] = [{ id: null, title: 'Cấp gốc' }];
  for (const { item: candidate } of flat.value) {
    if (candidate.id === item.id) continue;
    if (normalizeItemType(candidate.itemType) !== 'folder') continue;
    if (contains(item, candidate.id)) continue;
    folders.push({ id: candidate.id, title: candidate.title || 'Chương chưa đặt tên' });
  }
  return folders;
}

// ── Handlers phát lên view cha ──
function handleSelect(item: PathItemDto): void {
  emit('select', item);
}

function handleAdd(type: PathItemType, parentId: number | null): void {
  if (parentId !== null) {
    openState.value[parentId] = true;
  }
  emit('add', type, parentId);
}

function handleRename(item: PathItemDto, title: string): void {
  emit('rename', item, title);
}

function handleMoveItem(item: PathItemDto, parentId: number | null, sortOrder?: number): void {
  const target: OutlineMoveTarget = {
    parentId,
    sortOrder: sortOrder ?? countChildren(parentId),
  };
  emit('moveItem', item, target);
}

function countChildren(parentId: number | null): number {
  if (parentId === null) return props.items.length;
  const parent = findItem(props.items, parentId);
  return parent?.children?.length ?? 0;
}

function handleDelete(item: PathItemDto): void {
  emit('delete', item);
}

// ── Popover "Thêm mục" ở thanh công cụ ──
const showAddMenu = ref(false);
const addMenuContainerRef = ref<HTMLElement | null>(null);

function quickAdd(type: PathItemType): void {
  showAddMenu.value = false;
  emit('add', type, null);
}

function handleDocumentClick(e: MouseEvent): void {
  const target = e.target as Node | null;
  if (showAddMenu.value && addMenuContainerRef.value && !addMenuContainerRef.value.contains(target)) {
    showAddMenu.value = false;
  }
  if (openMenuId.value !== null) {
    const isInsideNodeMenu = target && (target as HTMLElement).closest?.('[role="menu"], [data-testid^="node-menu-"], [data-testid^="node-add-trigger-"]');
    if (!isInsideNodeMenu) {
      closeMenu();
    }
  }
}

onMounted(() => {
  document.addEventListener('click', handleDocumentClick);
});

onUnmounted(() => {
  document.removeEventListener('click', handleDocumentClick);
});

const toolbarAddOptions: { type: PathItemType; label: string; testid: string; color: string }[] = [
  { type: 'folder', label: 'Chương (Module)', testid: 'add-item-folder', color: 'text-amber-300' },
  { type: 'theory', label: 'Bài lý thuyết', testid: 'add-item-theory', color: 'text-sky-300' },
  { type: 'quiz', label: 'Quiz trắc nghiệm', testid: 'add-item-quiz', color: 'text-orange-300' },
  { type: 'lab', label: 'Lab thực hành', testid: 'add-item-lab', color: 'text-emerald-300' },
];

// ── Context cho OutlineNode ──
const selectedIdRef = computed<number | null>(() => props.selectedItemId ?? null);
const context: OutlineTreeContext = {
  readonly: props.readonly,
  selectedId: selectedIdRef,
  openState: openState.value,
  toggleFolder,
  expandAll,
  collapseAll,
  openMenuId,
  menuMode,
  openMenu,
  closeMenu,
  draggingId,
  dropZone,
  canDrop,
  prefixOf,
  moveTargets,
  select: handleSelect,
  add: handleAdd,
  rename: handleRename,
  moveItem: handleMoveItem,
  deleteItem: handleDelete,
  dropOn,
};

provideOutlineTreeContext(context);
</script>

<template>
  <div
    data-testid="outline-tree"
    class="flex flex-col bg-transparent border-0 shadow-none w-full flex-1 min-h-0"
  >
    <!-- Thanh công cụ -->
    <div class="flex items-center justify-between gap-2 px-1 py-2 border-b border-[#262438]/60 bg-transparent">
      <div class="flex items-center gap-2 min-w-0">
        <Layers class="w-4 h-4 text-purple-400 shrink-0" />
        <span class="text-xs font-black uppercase tracking-wider text-slate-300 truncate">Cây nội dung</span>
        <span class="text-[10px] font-bold text-slate-500 shrink-0">{{ totalCount }} mục</span>
      </div>

      <div class="flex items-center gap-1.5 shrink-0">
        <button
          type="button"
          class="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-[#2e2c44] transition-colors cursor-pointer"
          :title="totalCount === 0 || Object.keys(openState).length === 0 ? 'Thu gọn hết' : 'Mở rộng hết'"
          :aria-label="totalCount === 0 || Object.keys(openState).length === 0 ? 'Thu gọn hết' : 'Mở rộng hết'"
          @click="totalCount === 0 || Object.keys(openState).length === 0 ? collapseAll() : expandAll()"
        >
          <ChevronsDownUp v-if="Object.keys(openState).length === 0" class="w-4 h-4" />
          <ChevronsUpDown v-else class="w-4 h-4" />
        </button>

        <div v-if="!readonly" ref="addMenuContainerRef" class="relative">
          <button
            type="button"
            data-testid="add-item-trigger"
            class="flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold transition-colors cursor-pointer"
            aria-haspopup="menu"
            :aria-expanded="showAddMenu"
            @click.stop="showAddMenu = !showAddMenu"
          >
            <Plus class="w-3.5 h-3.5" /> Thêm mục
          </button>

          <div
            v-if="showAddMenu"
            class="absolute right-0 top-full mt-1.5 w-48 bg-[#1e1d2c] border border-[#36344d] rounded-xl shadow-2xl p-1.5 space-y-1 z-50"
            role="menu"
            aria-label="Thêm mục mới vào cấp gốc"
          >
            <button
              v-for="opt in toolbarAddOptions"
              :key="opt.type"
              type="button"
              role="menuitem"
              class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold hover:bg-white/5 text-left cursor-pointer"
              :class="opt.color"
              :data-testid="opt.testid"
              @click="quickAdd(opt.type)"
            >
              <Folder v-if="opt.type === 'folder'" class="w-3.5 h-3.5" />
              <BookOpen v-else-if="opt.type === 'theory'" class="w-3.5 h-3.5" />
              <HelpCircle v-else-if="opt.type === 'quiz'" class="w-3.5 h-3.5" />
              <Code v-else class="w-3.5 h-3.5" />
              {{ opt.label }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Thanh tìm kiếm nhanh -->
    <div v-if="items.length > 0" class="px-1 py-1.5 border-b border-[#262438]/60 bg-transparent">
      <div class="relative">
        <Search class="w-3.5 h-3.5 text-slate-500 absolute left-2.5 top-1/2 -translate-y-1/2 pointer-events-none" />
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm bài học, quiz, lab..."
          class="w-full pl-8 pr-7 py-1 text-xs bg-[#0e0d16]/80 border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-500 focus:outline-none focus:border-purple-500 transition-colors"
        />
        <button
          v-if="searchQuery"
          type="button"
          class="absolute right-2 top-1/2 -translate-y-1/2 text-slate-400 hover:text-white p-0.5 cursor-pointer"
          title="Xóa tìm kiếm"
          @click="searchQuery = ''"
        >
          <X class="w-3 h-3" />
        </button>
      </div>
    </div>

    <!-- Cây phẳng trực tiếp trên nền (cuộn mượt mà trọn vẹn, không bị cắt cụt) -->
    <div class="py-2 pb-16 space-y-1 overflow-y-auto overflow-x-auto min-w-0 flex-1 min-h-0 bg-transparent pr-1" role="tree" aria-label="Cây nội dung lộ trình">
      <OutlineNode v-for="rootItem in filteredItems" :key="rootItem.id" :item="rootItem" :depth="0" />

      <div v-if="items.length > 0 && filteredItems.length === 0" class="py-6 px-3 text-center space-y-1">
        <p class="text-xs font-bold text-slate-400">Không tìm thấy bài học phù hợp</p>
        <p class="text-[11px] text-slate-500">Thử tìm với từ khóa khác</p>
      </div>

      <div v-if="items.length === 0" class="py-8 px-4 text-center space-y-3">
        <Network class="w-8 h-8 text-slate-600 mx-auto" aria-hidden="true" />
        <p class="text-sm font-bold text-slate-400">Lộ trình đang trống</p>
        <template v-if="!readonly">
          <ol class="text-xs text-slate-400 space-y-1 text-left max-w-xs mx-auto list-none">
            <li class="flex gap-2"><span class="font-black text-emerald-400">✓</span> Đặt tên lộ trình (đã xong)</li>
            <li class="flex gap-2"><span class="font-black text-purple-400">②</span> Thêm <b class="text-slate-200">Chương</b> — ví dụ: "Cấu trúc dữ liệu tuyến tính"</li>
            <li class="flex gap-2"><span class="font-black text-sky-400">③</span> Thêm <b class="text-slate-200">bài học / quiz / lab</b> vào từng chương (bấm + trên dòng chương)</li>
          </ol>
          <button
            type="button"
            data-testid="add-first-folder"
            class="inline-flex items-center gap-1.5 px-3.5 py-2 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-black transition-colors cursor-pointer"
            @click="quickAdd('folder')"
          >
            <Folder class="w-3.5 h-3.5" aria-hidden="true" /> Thêm chương đầu tiên
          </button>
        </template>
        <p v-else class="text-xs text-slate-500">Chưa có nội dung nào.</p>
      </div>
    </div>
  </div>
</template>
