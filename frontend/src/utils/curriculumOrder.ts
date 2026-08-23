import type { ClassAssignmentDto, ClassCurriculumReorderItem } from '@/api/types';

/**
 * Tính payload reorder khi chuyển 1 item từ fromIndex → toIndex trong lộ trình học.
 * Trả danh sách { assignmentId, sortOrder } đánh lại theo thứ tự mới (0-based).
 * Thuần túy — không gọi API (dễ unit-test).
 */
export function buildReorderItems(
  list: ClassAssignmentDto[],
  fromIndex: number,
  toIndex: number,
): ClassCurriculumReorderItem[] {
  if (
    fromIndex < 0 ||
    toIndex < 0 ||
    fromIndex >= list.length ||
    toIndex >= list.length ||
    fromIndex === toIndex
  ) {
    return list.map((a, i) => ({ assignmentId: a.id, sortOrder: i }));
  }

  const next = [...list];
  const [moved] = next.splice(fromIndex, 1);
  next.splice(toIndex, 0, moved!);
  return next.map((a, i) => ({ assignmentId: a.id, sortOrder: i }));
}
