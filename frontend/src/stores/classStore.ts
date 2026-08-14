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

  /** v2.15: tham gia lớp bằng mã mời — gọi API trực tiếp POST /classes/join-by-code (KHÔNG dò trong danh sách đã tham gia). */
  async function joinByCode(code: string): Promise<ClassDto> {
    const joined = await classesApi.joinByCode(code);
    classes.value = [joined, ...classes.value.filter((c) => c.id !== joined.id)];
    return joined;
  }

  async function joinClass(code: string): Promise<ClassDto> {
    return joinByCode(code);
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

  async function assignContent(payload: {
    classId: number;
    lessonId?: number | null;
    exerciseId?: number | null;
    dueAt?: string | null;
    allowLateSubmission?: boolean;
  }): Promise<void> {
    const created = await classesApi.createClassAssignment(payload.classId, {
      lessonId: payload.lessonId ?? null,
      exerciseId: payload.exerciseId ?? null,
      dueAt: payload.dueAt ?? null,
      allowLateSubmission: payload.allowLateSubmission,
    });
    assignments.value = [...assignments.value, created];
  }

  async function updateAssignment(
    classId: number,
    assignId: number,
    payload: { dueAt?: string | null; allowLateSubmission?: boolean },
  ): Promise<void> {
    await classesApi.updateClassAssignment(classId, assignId, payload);
    await reloadAssignments(classId);
  }

  async function removeAssignment(classId: number, assignId: number): Promise<void> {
    await classesApi.deleteClassAssignment(classId, assignId);
    await reloadAssignments(classId);
  }

  async function removeMember(classId: number, userId: number): Promise<void> {
    await classesApi.removeClassMember(classId, userId);
    members.value = members.value.filter((m) => m.userId !== userId);
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
    joinByCode,
    joinClass,
    fetchClass,
    reloadMembers,
    reloadAssignments,
    assignContent,
    updateAssignment,
    removeAssignment,
    removeMember,
    removeClass,
  };
});
