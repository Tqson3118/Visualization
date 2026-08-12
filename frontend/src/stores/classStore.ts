import { ref } from 'vue';
import { defineStore } from 'pinia';

import * as classesApi from '@/api/classes';
import type { ClassAssignmentDto, ClassDto, ClassMemberDto } from '@/api/types';

/** Store classStore theo SDD §3.2 — Module H (lớp học) — triển khai thật với API /classes. */
export const useClassStore = defineStore('classStore', () => {
  const classes = ref<ClassDto[]>([]);
  const currentClass = ref<ClassDto | null>(null);
  const members = ref<ClassMemberDto[]>([]);
  const assignments = ref<ClassAssignmentDto[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchClasses(): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      classes.value = await classesApi.fetchClasses();
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không thể tải danh sách lớp';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function createClass(name: string, description?: string): Promise<ClassDto> {
    const created = await classesApi.createClass({ name, description });
    classes.value = [created, ...classes.value];
    return created;
  }

  async function joinClass(code: string): Promise<ClassDto> {
    // Cần id lớp — join theo mã mời: endpoint POST /classes/{id}/join cần id.
    // Khi chưa có id, tìm lớp khớp mã trong danh sách đã tải (hoặc ném lỗi).
    const target = classes.value.find((c) => c.inviteCode.toUpperCase() === code.toUpperCase());
    if (!target) {
      // Server hỗ trợ join theo mã: thử id 0? Không — ném lỗi rõ ràng để view hướng dẫn.
      throw new Error('Không tìm thấy lớp với mã mời này — hãy kiểm tra lại mã hoặc liên hệ giảng viên.');
    }
    const joined = await classesApi.joinClass(target.id, code);
    classes.value = [joined, ...classes.value.filter((c) => c.id !== joined.id)];
    return joined;
  }

  async function fetchClass(id: number): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      const detail = await classesApi.fetchClass(id);
      currentClass.value = detail;
      members.value = detail.members ?? [];
      assignments.value = detail.assignments ?? [];
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không thể tải chi tiết lớp';
      throw err;
    } finally {
      loading.value = false;
    }
  }

  async function reloadMembers(id: number): Promise<void> {
    members.value = await classesApi.fetchClassMembers(id);
  }

  async function reloadAssignments(id: number): Promise<void> {
    assignments.value = await classesApi.fetchClassAssignments(id);
  }

  async function assignContent(payload: { classId: number; lessonId?: number | null; exerciseId?: number | null; dueAt?: string | null }): Promise<void> {
    const created = await classesApi.createClassAssignment(payload.classId, {
      lessonId: payload.lessonId ?? null,
      exerciseId: payload.exerciseId ?? null,
      dueAt: payload.dueAt ?? null,
    });
    assignments.value = [...assignments.value, created];
  }

  async function removeMember(classId: number, userId: number): Promise<void> {
    await classesApi.removeClassMember(classId, userId);
    members.value = members.value.filter((m) => m.id !== userId);
  }

  async function removeClass(classId: number): Promise<void> {
    await classesApi.deleteClass(classId);
    classes.value = classes.value.filter((c) => c.id !== classId);
    currentClass.value = null;
  }

  return {
    classes,
    currentClass,
    members,
    assignments,
    loading,
    error,
    fetchClasses,
    createClass,
    joinClass,
    fetchClass,
    reloadMembers,
    reloadAssignments,
    assignContent,
    removeMember,
    removeClass,
  };
});
