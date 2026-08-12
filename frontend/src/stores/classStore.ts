import { ref } from 'vue';
import { defineStore } from 'pinia';

/** Store classStore theo SDD §3.2 — Module H (lớp học) */
export interface ClassSummary {
  id: number;
  name: string;
  inviteCode: string;
  memberCount: number;
  createdAt: string;
}

export interface ClassMember {
  id: number;
  displayName: string;
  email: string;
  role: 'STUDENT' | 'TEACHER';
  joinedAt: string;
}

export interface ClassAssignment {
  id: number;
  lessonId: number | null;
  exerciseId: number | null;
  dueAt: string | null;
  status: 'open' | 'closed';
}

export const useClassStore = defineStore('classStore', () => {
  const classes = ref<ClassSummary[]>([]);
  const currentClass = ref<ClassSummary | null>(null);
  const members = ref<ClassMember[]>([]);
  const assignments = ref<ClassAssignment[]>([]);

  async function fetchClasses(): Promise<void> {
    // TODO: gọi API /classes
    return Promise.reject(new Error('TODO: classStore.fetchClasses chưa triển khai'));
  }

  async function joinClass(code: string): Promise<void> {
    // TODO: gọi POST /classes/{id}/join { inviteCode: code }
    void code;
    return Promise.reject(new Error('TODO: classStore.joinClass chưa triển khai'));
  }

  async function fetchClass(id: number): Promise<void> {
    // TODO: gọi GET /classes/{id} → currentClass + members + assignments
    void id;
    return Promise.reject(new Error('TODO: classStore.fetchClass chưa triển khai'));
  }

  async function assignContent(payload: { classId: number; lessonId?: number | null; exerciseId?: number | null; dueAt?: string | null }): Promise<void> {
    // TODO: gọi POST /classes/{id}/assignments
    void payload;
    return Promise.reject(new Error('TODO: classStore.assignContent chưa triển khai'));
  }

  return {
    classes,
    currentClass,
    members,
    assignments,
    fetchClasses,
    joinClass,
    fetchClass,
    assignContent,
  };
});
