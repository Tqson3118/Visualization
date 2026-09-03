<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  BookOpen,
  ChevronDown,
  ChevronRight,
  Code,
  CornerUpLeft,
  Folder,
  FolderOpen,
  FolderInput,
  GripVertical,
  HelpCircle,
  MoreVertical,
  Pencil,
  Plus,
  Trash2,
} from 'lucide-vue-next';
import { type PathItemDto, normalizeItemType } from '@/api/pathItems';
import { useOutlineNodeContext, type OutlineDropZone } from './outlineTreeContext';

/**
 * Node đệ quy của cây outline (D7 — lồng bao nhiêu tầng tùy ý).
 * State dùng chung (menu, kéo–thả, mở/đóng) lấy từ OutlineTree qua context.
 */
const props = defineProps<{ item: PathItemDto; depth: number }>();

const ctx = useOutlineNodeContext();
// Destructure để template tự unwrap các ref.
const { openState, openMenuId, menuMode, draggingId } = ctx;

const itemType = computed(() => normalizeItemType(props.item.itemType));
const isFolder = computed(() => itemType.value === 'folder');
const expanded = computed(() => openState[props.item.id] ?? true);
const isSelected = computed(() => ctx.selectedId.value === props.item.id);
const hasChildren = computed(() => (props.item.children?.length ?? 0) > 0);
const childCount = computed(() => props.item.children?.length ?? 0);

const badge = computed(() => {
  switch (itemType.value) {
    case 'folder':
      return { label: 'Chương', color: 'bg-amber-500/20 text-amber-300 border-amber-500/30' };
    case 'theory':
      return { label: 'Lý thuyết', color: 'bg-sky-500/20 text-sky-300 border-sky-500/30' };
    case 'quiz':
      return { label: 'Trắc nghiệm', color: 'bg-orange-500/20 text-orange-300 border-orange-500/30' };
    default:
      return { label: 'Codelab', color: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30' };
  }
});

const iconClass = computed(() => {
  switch (itemType.value) {
    case 'folder':
      return expanded.value ? 'w-4 h-4 text-amber-400 shrink-0' : 'w-4 h-4 text-amber-400/70 shrink-0';
    case 'theory':
      return 'w-4 h-4 text-sky-400 shrink-0';
    case 'quiz':
      return 'w-4 h-4 text-orange-400 shrink-0';
    default:
      return 'w-4 h-4 text-emerald-400 shrink-0';
  }
});

const menuOpen = computed(() => openMenuId.value === props.item.id);
const mode = computed(() => (menuOpen.value ? menuMode.value : 'actions'));
const isDragging = computed(() => draggingId.value === props.item.id);

// ── Đổi tên inline ──
const renaming = ref(false);
const renameValue = ref('');
const renameInput = ref<HTMLInputElement | null>(null);

function startRename(): void {
  ctx.closeMenu();
  renaming.value = true;
  renameValue.value = props.item.title;
  // Đợi input mount xong rồi focus + chọn hết chữ.
  requestAnimationFrame(() => {
    renameInput.value?.focus();
    renameInput.value?.select();
  });
}

function commitRename(): void {
  if (!renaming.value) return;
  renaming.value = false;
  const next = renameValue.value.trim();
  if (next && next !== props.item.title) ctx.rename(props.item, next);
}

function cancelRename(): void {
  renaming.value = false;
}

function focusRenameInput(el: unknown): void {
  if (el instanceof HTMLInputElement) renameInput.value = el;
}

// ── Kéo–thả sắp thứ tự ──
const localZone = ref<OutlineDropZone | null>(null);

function onDragStart(e: DragEvent): void {
  if (ctx.readonly) return;
  ctx.draggingId.value = props.item.id;
  localZone.value = null;
  e.dataTransfer?.setData('text/plain', String(props.item.id));
  if (e.dataTransfer) {
    e.dataTransfer.effectAllowed = 'move';
  }
}

function onDragOver(e: DragEvent): void {
  if (!ctx.canDrop(props.item)) return;
  e.preventDefault();
  if (e.dataTransfer) e.dataTransfer.dropEffect = 'move';
  const rect = (e.currentTarget as HTMLElement | null)?.getBoundingClientRect();
  const height = rect && rect.height > 0 ? rect.height : 0;
  // jsdom (test) trả rect 0×0 → mặc định vùng giữa.
  const ratio = rect && height > 0 ? (e.clientY - rect.top) / height : 0.5;
  let zone: OutlineDropZone;
  if (isFolder.value && ratio > 0.25 && ratio < 0.75) {
    zone = 'into';
  } else {
    zone = ratio < 0.5 ? 'before' : 'after';
  }
  localZone.value = zone;
}

function onDragLeave(): void {
  localZone.value = null;
}

function onDrop(e: DragEvent): void {
  e.preventDefault();
  const zone = localZone.value ?? (isFolder.value ? 'into' : 'after');
  localZone.value = null;
  if (draggingId.value != null) ctx.dropOn(props.item, zone);
}

function onDragEnd(): void {
  ctx.draggingId.value = null;
  localZone.value = null;
}

const zoneClass = computed(() => {
  if (!localZone.value) return '';
  if (localZone.value === 'into') return 'ring-2 ring-purple-500/80 bg-purple-500/10';
  return 'ring-2 ring-sky-500/70';
});

// ── Menu ngữ cảnh ──
function openActionsMenu(): void {
  if (menuOpen.value && mode.value === 'actions') {
    ctx.closeMenu();
  } else {
    ctx.openMenu(props.item.id, 'actions');
  }
}

function openAddMenu(): void {
  if (menuOpen.value && mode.value === 'add') {
    ctx.closeMenu();
  } else {
    ctx.openMenu(props.item.id, 'add');
  }
}

function openMoveMenu(): void {
  ctx.openMenu(props.item.id, 'move');
}

function moveTo(parentId: number | null): void {
  ctx.closeMenu();
  ctx.moveItem(props.item, parentId);
}

function quickAdd(type: 'folder' | 'theory' | 'quiz' | 'lab'): void {
  ctx.closeMenu();
  ctx.add(type, props.item.id);
}

const moveOptions = computed(() => (mode.value === 'move' ? ctx.moveTargets(props.item) : []));

// ── Bàn phím (a11y) ──
function onKeydown(e: KeyboardEvent): void {
  if (renaming.value) return;
  if (e.key === 'Enter' || e.key === ' ') {
    e.preventDefault();
    ctx.select(props.item);
  } else if (e.key === 'ArrowRight' && isFolder.value && !expanded.value) {
    e.preventDefault();
    ctx.toggleFolder(props.item.id);
  } else if (e.key === 'ArrowLeft' && isFolder.value && expanded.value) {
    e.preventDefault();
    ctx.toggleFolder(props.item.id);
  }
}

function onRowClick(): void {
  if (renaming.value) return;
  ctx.select(props.item);
}
</script>

<template>
  <div class="tree-node-wrapper" :style="{ paddingLeft: Math.min(depth * 8, 24) + 'px' }">
    <div
      class="group relative flex items-center gap-1.5 rounded-lg pl-1.5 pr-1.5 py-1.5 cursor-pointer transition-colors border border-transparent select-none min-w-0"
      :class="[
        isSelected ? 'bg-purple-600/25 border-purple-500/50 border-l-4 border-l-purple-500 shadow-sm' : 'hover:bg-[#1e1d2c]',
        zoneClass,
        isDragging ? 'opacity-40' : '',
      ]"
      role="treeitem"
      :aria-level="depth + 1"
      :aria-selected="isSelected"
      :aria-expanded="isFolder ? expanded : undefined"
      :data-testid="'outline-node-' + item.id"
      :draggable="!ctx.readonly"
      tabindex="0"
      @click="onRowClick"
      @keydown="onKeydown"
      @dragstart="onDragStart"
      @dragover="onDragOver"
      @dragleave="onDragLeave"
      @drop="onDrop"
      @dragend="onDragEnd"
    >
      <!-- Drag handle -->
      <span
        v-if="!ctx.readonly"
        class="drag-handle shrink-0 p-0.5 rounded text-slate-500 hover:text-slate-200 cursor-grab active:cursor-grabbing opacity-0 group-hover:opacity-100 transition-opacity"
        :data-testid="'drag-handle-' + item.id"
        :title="'Kéo để di chuyển ' + (item.title || 'mục này')"
        aria-hidden="true"
      >
        <GripVertical class="w-3.5 h-3.5" />
      </span>

      <!-- Folder toggle -->
      <button
        v-if="isFolder"
        type="button"
        class="shrink-0 p-0.5 rounded text-slate-400 hover:text-white cursor-pointer"
        :data-testid="'folder-toggle-' + item.id"
        :aria-expanded="expanded"
        :aria-label="(expanded ? 'Thu gọn ' : 'Mở rộng ') + (item.title || 'thư mục')"
        @click.stop="ctx.toggleFolder(item.id)"
      >
        <ChevronDown v-if="expanded" class="w-3.5 h-3.5" />
        <ChevronRight v-else class="w-3.5 h-3.5" />
      </button>
      <span v-else class="w-[18px] shrink-0" aria-hidden="true" />

      <!-- Đánh số theo cây: 1, 1.1, 1.2… -->
      <span
        v-if="ctx.prefixOf(item.id)"
        class="shrink-0 text-[10px] font-mono font-bold text-slate-500 bg-white/5 px-1.5 py-0.5 rounded"
        aria-hidden="true"
      >{{ ctx.prefixOf(item.id) }}</span>

      <!-- Icon -->
      <FolderOpen v-if="isFolder && expanded" :class="iconClass" />
      <Folder v-else-if="isFolder" :class="iconClass" />
      <BookOpen v-else-if="itemType === 'theory'" :class="iconClass" />
      <HelpCircle v-else-if="itemType === 'quiz'" :class="iconClass" />
      <Code v-else :class="iconClass" />

      <!-- Title / rename input -->
      <input
        v-if="renaming"
        :ref="focusRenameInput"
        v-model="renameValue"
        type="text"
        class="flex-1 min-w-[60px] px-1.5 py-0.5 text-xs font-bold bg-[#0e0d16] border border-purple-500 rounded text-white focus:outline-none"
        :data-testid="'rename-input-' + item.id"
        :aria-label="'Tên mới cho ' + (item.title || 'mục')"
        @click.stop
        @keydown.enter.prevent="commitRename"
        @keydown.esc.prevent="cancelRename"
        @blur="commitRename"
      />
      <span
        v-else
        class="flex-1 min-w-0 text-xs font-bold truncate text-slate-200 group-hover:text-white"
        :title="item.title || 'Mục chưa đặt tên'"
      >
        {{ item.title || 'Mục chưa đặt tên' }}
      </span>

      <span
        v-if="depth === 0 || !isFolder"
        class="text-[9px] font-extrabold px-1.5 py-0.5 rounded border shrink-0"
        :class="badge.color"
      >
        {{ badge.label }}
      </span>

      <span
        v-if="isFolder && hasChildren"
        class="text-[9px] font-bold text-slate-400 bg-white/5 px-1 py-0.5 rounded shrink-0"
      >
        {{ childCount }}
      </span>

      <!-- Right: node actions -->
      <div
        v-if="!ctx.readonly"
        class="flex items-center gap-1 ml-auto transition-opacity shrink-0"
        :class="menuOpen ? 'opacity-100' : 'opacity-0 group-hover:opacity-100 focus-within:opacity-100'"
        @click.stop
      >
        <button
          v-if="isFolder"
          type="button"
          :data-testid="'node-add-trigger-' + item.id"
          title="Thêm mục vào chương này"
          aria-label="Thêm mục vào chương này"
          class="p-1.5 rounded-lg bg-purple-600/20 hover:bg-purple-600/40 text-purple-300 border border-purple-500/30 transition-colors cursor-pointer"
          @click.stop="openAddMenu"
        >
          <Plus class="w-3.5 h-3.5" />
        </button>

        <button
          type="button"
          title="Tuỳ chọn"
          aria-label="Tuỳ chọn của mục"
          class="p-1.5 rounded-lg hover:bg-[#2e2c44] text-slate-400 hover:text-white transition-colors cursor-pointer"
          :data-testid="'node-menu-' + item.id"
          @click="openActionsMenu"
        >
          <MoreVertical class="w-3.5 h-3.5" />
        </button>
      </div>
    </div>

    <!-- Menu: hành động -->
    <div
      v-if="menuOpen && mode === 'actions'"
      class="relative z-[70] ml-auto w-fit"
      role="menu"
      :aria-label="'Hành động cho ' + (item.title || 'mục')"
    >
      <div class="absolute right-2 -top-1 w-44 bg-[#1e1d2c] border border-[#36344d] rounded-xl shadow-2xl p-1.5 space-y-1 z-[70]">
        <button
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-sky-300 hover:bg-sky-500/10 text-left cursor-pointer"
          data-testid="node-menu-rename"
          @click="startRename"
        >
          <Pencil class="w-3.5 h-3.5" /> Đổi tên
        </button>
        <button
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-purple-300 hover:bg-purple-500/10 text-left cursor-pointer"
          data-testid="node-menu-move"
          @click="openMoveMenu"
        >
          <FolderInput class="w-3.5 h-3.5" /> Di chuyển đến…
        </button>
        <button
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-rose-300 hover:bg-rose-500/10 text-left cursor-pointer"
          data-testid="node-menu-delete"
          @click="ctx.deleteItem(item); ctx.closeMenu()"
        >
          <Trash2 class="w-3.5 h-3.5" /> Xóa
        </button>
      </div>
    </div>

    <!-- Menu: chọn đích di chuyển -->
    <div v-else-if="menuOpen && mode === 'move'" class="relative z-[70] ml-auto w-fit" role="menu" aria-label="Chọn thư mục đích">
      <div class="absolute right-2 -top-1 w-52 max-h-56 overflow-y-auto bg-[#1e1d2c] border border-[#36344d] rounded-xl shadow-2xl p-1.5 space-y-1 z-[70]">
        <p class="px-2.5 pt-1 pb-0.5 text-[10px] font-black uppercase tracking-wider text-slate-500">Di chuyển đến…</p>
        <button
          v-if="isFolder"
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-slate-300 hover:bg-white/5 text-left cursor-pointer"
          data-testid="move-target-root"
          @click="moveTo(null)"
        >
          <CornerUpLeft class="w-3.5 h-3.5" /> Cấp gốc
        </button>
        <button
          v-for="opt in moveOptions"
          :key="opt.id ?? 'root'"
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-amber-300 hover:bg-amber-500/10 text-left cursor-pointer"
          :data-testid="'move-target-' + opt.id"
          @click="moveTo(opt.id)"
        >
          <Folder class="w-3.5 h-3.5 shrink-0" />
          <span class="truncate">{{ opt.title }}</span>
        </button>
        <p v-if="moveOptions.length === 0" class="px-2.5 py-1.5 text-[11px] text-slate-500">
          Không có thư mục nào khác để di chuyển đến.
        </p>
      </div>
    </div>

    <!-- Menu: thêm mục con vào chương -->
    <div v-else-if="menuOpen && mode === 'add'" class="relative z-[70] ml-auto w-fit" role="menu" aria-label="Thêm mục con">
      <div class="absolute right-2 -top-1 w-48 bg-[#1e1d2c] border border-[#36344d] rounded-xl shadow-2xl p-1.5 space-y-1 z-[70]">
        <div class="px-2 py-0.5 text-[10px] font-bold text-slate-400 border-b border-[#2e2c44]/80">
          Thêm vào chương:
        </div>
        <button
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-sky-300 hover:bg-sky-500/10 text-left cursor-pointer"
          :data-testid="'node-add-theory-' + item.id"
          @click="quickAdd('theory')"
        >
          <BookOpen class="w-3.5 h-3.5" /> Bài lý thuyết
        </button>
        <button
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-orange-300 hover:bg-orange-500/10 text-left cursor-pointer"
          :data-testid="'node-add-quiz-' + item.id"
          @click="quickAdd('quiz')"
        >
          <HelpCircle class="w-3.5 h-3.5" /> Quiz trắc nghiệm
        </button>
        <button
          type="button"
          role="menuitem"
          class="w-full flex items-center gap-2 px-2.5 py-1.5 rounded-lg text-xs font-bold text-emerald-300 hover:bg-emerald-500/10 text-left cursor-pointer"
          :data-testid="'node-add-lab-' + item.id"
          @click="quickAdd('lab')"
        >
          <Code class="w-3.5 h-3.5" /> Lab thực hành
        </button>
      </div>
    </div>

    <!-- Children (đệ quy — D7) -->
    <div v-if="isFolder && expanded && hasChildren" class="mt-1 space-y-1 border-l border-[#262438] ml-3 pl-0" role="group">
      <OutlineNode v-for="child in item.children" :key="child.id" :item="child" :depth="depth + 1" />
    </div>
  </div>
</template>
