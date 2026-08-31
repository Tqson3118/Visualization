import { getData } from '@/api/client';
import type { ExerciseDto } from '@/api/exercises';
import type { LessonDto } from '@/api/lessons';

export type PathItemType = 'folder' | 'theory' | 'quiz' | 'lab';

export interface PathItemDto {
  id: number;
  pathId: number;
  parentId?: number | null;
  itemType: PathItemType | number;
  title: string;
  description?: string | null;
  lessonId?: number | null;
  finalTestId?: number | null;
  labExerciseId?: number | null;
  exerciseId?: number | null;
  sortOrder: number;
  children: PathItemDto[];
  exercise?: ExerciseDto | null;
  lesson?: LessonDto | null;
}

export interface PathItemCreateRequest {
  itemType: PathItemType | number;
  title: string;
  description?: string;
  parentId?: number | null;
  lessonId?: number | null;
  sortOrder?: number;
}

export interface PathItemUpdateRequest {
  title: string;
  description?: string;
  lessonId?: number | null;
  finalTestId?: number | null;
  labExerciseId?: number | null;
}

export interface PathItemMoveRequest {
  parentId?: number | null;
  sortOrder: number;
}

export interface UpdateAssignmentDeadlineRequest {
  pathItemId: number;
  dueAt?: string | null;
  allowLateSubmission?: boolean;
}

export function normalizeItemType(type: PathItemType | number | string): PathItemType {
  if (type === 0 || type === 'folder' || type === 'Folder') return 'folder';
  if (type === 1 || type === 'theory' || type === 'Theory') return 'theory';
  if (type === 2 || type === 'quiz' || type === 'Quiz') return 'quiz';
  if (type === 3 || type === 'lab' || type === 'Lab') return 'lab';
  return 'theory';
}

export async function fetchPathTree(pathId: number): Promise<PathItemDto[]> {
  return getData<PathItemDto[]>({
    method: 'GET',
    url: `/paths/${pathId}/items`,
  });
}

export async function findPathItemByLesson(lessonId: number): Promise<PathItemDto> {
  return getData<PathItemDto>({
    method: 'GET',
    url: `/paths/find-by-lesson/${lessonId}`,
  });
}

export async function fetchItemDetail(itemId: number): Promise<PathItemDto> {
  return getData<PathItemDto>({
    method: 'GET',
    url: `/items/${itemId}`,
  });
}

export async function createPathItem(pathId: number, data: PathItemCreateRequest): Promise<PathItemDto> {
  return getData<PathItemDto>({
    method: 'POST',
    url: `/paths/${pathId}/items`,
    data,
  });
}

export async function updatePathItem(itemId: number, data: PathItemUpdateRequest): Promise<PathItemDto> {
  return getData<PathItemDto>({
    method: 'PUT',
    url: `/items/${itemId}`,
    data,
  });
}

export async function movePathItem(itemId: number, data: PathItemMoveRequest): Promise<PathItemDto> {
  return getData<PathItemDto>({
    method: 'POST',
    url: `/items/${itemId}/move`,
    data,
  });
}

export async function deletePathItem(itemId: number): Promise<void> {
  return getData<void>({
    method: 'DELETE',
    url: `/items/${itemId}`,
  });
}

export async function updateClassDeadline(classId: number, data: UpdateAssignmentDeadlineRequest): Promise<void> {
  return getData<void>({
    method: 'PUT',
    url: `/classes/${classId}/assignments/deadline`,
    data,
  });
}

export async function assignCourseToClasses(courseId: number, classIds: number[]): Promise<void> {
  return getData<void>({
    method: 'POST',
    url: `/courses/${courseId}/assign-classes`,
    data: { classIds },
  });
}
