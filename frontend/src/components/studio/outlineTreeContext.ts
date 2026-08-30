/**
 * Context chia sẻ giữa OutlineTree (cha) và OutlineNode (node đệ quy).
 * Props down / events up vẫn giữ nguyên ở rìa component: OutlineTree nhận
 * sự kiện từ node qua các handler trong context này, rồi phát tiếp lên view cha.
 */
import { inject, provide } from 'vue';
import type { InjectionKey, Ref } from 'vue';
import type { PathItemDto, PathItemType } from '@/api/pathItems';

/** Đích di chuyển: parentId=null là cấp gốc; sortOrder là chỉ số chèn. */
export interface OutlineMoveTarget {
  parentId: number | null;
  sortOrder: number;
}

export type OutlineDropZone = 'into' | 'before' | 'after';

export interface OutlineFolderOption {
  id: number | null;
  title: string;
}

export interface OutlineTreeContext {
  readonly: boolean;
  /** Node đang được chọn trong cây (id). */
  selectedId: Ref<number | null>;
  /** Trạng thái mở/đóng của folder — key thiếu nghĩa là mở (true). */
  openState: Record<number, boolean>;
  toggleFolder(id: number): void;
  expandAll(): void;
  collapseAll(): void;
  /** Menu ngữ cảnh — chỉ 1 node mở menu tại một thời điểm. */
  openMenuId: Ref<number | null>;
  menuMode: Ref<'actions' | 'move' | 'add'>;
  openMenu(id: number, mode?: 'actions' | 'move' | 'add'): void;
  closeMenu(): void;
  /** Kéo–thả: id node đang kéo + vùng đích đang hover. */
  draggingId: Ref<number | null>;
  dropZone: Ref<OutlineDropZone | null>;
  canDrop(target: PathItemDto): boolean;
  /** Số thứ tự hiển thị theo cây: "1", "1.1", "1.2"… (rỗng nếu không tính được). */
  prefixOf(id: number): string;
  /** Danh sách đích hợp lệ cho "Di chuyển đến…" (loại node này + hậu duệ). */
  moveTargets(item: PathItemDto): OutlineFolderOption[];
  /** Handlers phát lên view cha. */
  select(item: PathItemDto): void;
  add(type: PathItemType, parentId: number | null): void;
  rename(item: PathItemDto, title: string): void;
  moveItem(item: PathItemDto, parentId: number | null, sortOrder?: number): void;
  deleteItem(item: PathItemDto): void;
  dropOn(target: PathItemDto, zone: OutlineDropZone): void;
}

const OUTLINE_TREE_KEY: InjectionKey<OutlineTreeContext> = Symbol('outline-tree-context');

export function provideOutlineTreeContext(ctx: OutlineTreeContext): void {
  provide(OUTLINE_TREE_KEY, ctx);
}

export function useOutlineNodeContext(): OutlineTreeContext {
  const ctx = inject(OUTLINE_TREE_KEY);
  if (!ctx) throw new Error('OutlineNode phải được dùng bên trong OutlineTree.');
  return ctx;
}