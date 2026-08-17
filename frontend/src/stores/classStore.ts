import { ref } from 'vue';
import { defineStore } from 'pinia';

import * as classesApi from '@/api/classes';
import type {
  ClassAssignmentDto,
  ClassCurriculumDto,
  ClassCurriculumReorderItem,
  ClassCurriculumUpsertRequest,
  ClassDetailDto,
  ClassDto,
  ClassMemberDto,
} from '@/api/types';

/** Store classStore theo SDD §3.2 — Module H (lớp học) — triển khai thật với API /classes. */
export const useClassStore = defineStore('classStore', () => {
  const classes = ref<ClassDto[]>([]);
  const currentClass = ref<ClassDetailDto | null>(null);
  const members = ref<ClassMemberDto[]>([]);
  const assignments = ref<ClassAssignmentDto[]>([]);
  const curriculum = ref<ClassCurriculumDto | null>(null);
  const curriculumLoading = ref(false);
  const curriculumError = ref<string | null>(null);
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

  /** Nạp lộ trình học của lớp (học viên: kèm status từ progress thật). */
  async function fetchCurriculum(classId: number): Promise<void> {
    curriculumLoading.value = true;
    curriculumError.value = null;
    try {
      curriculum.value = await classesApi.fetchClassCurriculum(classId);
    } catch (err) {
      curriculumError.value = err instanceof Error ? err.message : 'Không thể tải lộ trình học';
      throw err;
    } finally {
      curriculumLoading.value = false;
    }
  }

  /** Teacher/Admin: cập nhật meta + publish/unpublish lộ trình. */
  async function updateCurriculumMeta(
    classId: number,
    payload: ClassCurriculumUpsertRequest,
  ): Promise<void> {
    const detail = await classesApi.updateClassCurriculum(classId, payload);
    currentClass.value = detail;
    assignments.value = detail.assignments ?? [];
    if (curriculum.value) {
      curriculum.value = {
        ...curriculum.value,
        title: detail.curriculumTitle ?? null,
        description: detail.curriculumDescription ?? null,
        published: detail.curriculumPublished ?? true,
      };
    }
  }

  /** Teacher/Admin: sắp xếp lại items trong lộ trình. */
  async function reorderCurriculum(classId: number, items: ClassCurriculumReorderItem[]): Promise<void> {
    await classesApi.reorderClassCurriculum(classId, { items });
    await Promise.all([reloadAssignments(classId), fetchCurriculum(classId).catch(() => undefined)]);
  }

  return {
    classes,
    currentClass,
    members,
    assignments,
    curriculum,
    curriculumLoading,
    curriculumError,
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
    fetchCurriculum,
    updateCurriculumMeta,
    reorderCurriculum,
  };
});
