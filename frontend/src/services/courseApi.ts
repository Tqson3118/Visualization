import { client, getData } from '@/api/client';

export interface CourseHighlightDto {
  title: string;
  description: string;
}

export interface CourseTestimonialDto {
  name: string;
  role: string;
  quote: string;
}

export interface CourseAuthorDto {
  name: string;
  academicDegree: string | null;
  bio: string | null;
  profileLink: string | null;
  avatarUrl: string | null;
}

export interface CourseLessonDto {
  id: string;
  nodeId?: number;
  lessonId?: number;
  title: string;
  moduleTitle?: string;
  moduleDescription?: string;
  contentMd: string;
  sandboxType: string;
  sandboxConfig: string;
  quizId: string | null;
  xpReward: number;
  orderIndex: number;
  status: string;
  locked?: boolean;
}

export interface CourseDetailDto {
  id: string;
  title: string;
  description: string;
  category: string;
  difficulty: string;
  isPremium: boolean;
  coverImageUrl?: string;
  coverImage?: string;
  isPublished: boolean;
  status?: 'draft' | 'pending_review' | 'active' | 'rejected' | string;
  rejectionReason?: string | null;
  reviewedBy?: number | null;
  reviewedAt?: string | null;
  submittedAt?: string | null;
  authorName?: string | null;
  createdBy?: number;
  authorId?: number | null;
  progressPercent: number;
  xpReward: number;
  learningObjectives: string[];
  keyOutcomes: string[];
  rating: number;
  ratingCount: number;
  highlights: CourseHighlightDto[];
  testimonials: CourseTestimonialDto[];
  author: CourseAuthorDto | null;
  lessons: CourseLessonDto[];
}

export interface CourseFeedbackDto {
  id: number;
  courseId: number;
  courseTitle: string;
  userId: number;
  userName: string;
  type: 'Suggestion' | 'Bug' | 'Request';
  content: string;
  status: 'New' | 'Read' | 'Resolved';
  replyText: string | null;
  repliedByName: string | null;
  repliedAt: string | null;
  createdAt: string;
}

export interface CourseFeedbackPayload {
  courseId: number;
  type: string;
  content: string;
}

export interface CourseFeedbackReplyPayload {
  status?: string;
  replyText?: string;
}

export interface CourseListDto {
  id: string;
  title: string;
  description: string;
  category: string;
  difficulty: string;
  isPremium: boolean;
  coverImageUrl?: string;
  coverImage?: string;
  isPublished: boolean;
  status?: 'draft' | 'pending_review' | 'active' | 'rejected' | string;
  rejectionReason?: string | null;
  reviewedBy?: number | null;
  reviewedAt?: string | null;
  submittedAt?: string | null;
  authorName?: string | null;
  createdBy?: number;
  authorId?: number | null;
  xpReward: number;
  totalLessons: number;
  completedLessons: number;
  progressPercent: number;
  lessons?: CourseLessonDto[];
}

export interface CourseUpsertPayload {
  title: string;
  description?: string;
  category?: string;
  difficulty?: string;
  topicId?: number;
  sortOrder?: number;
  isActive?: boolean;
  scope?: 'draft' | 'class' | 'public';
  status?: string;
  learningObjectives?: string[];
  keyOutcomes?: string[];
  highlights?: CourseHighlightDto[];
}

export interface CourseNodePayload {
  title: string;
  lessonId?: number;
  sortOrder?: number;
  finalTestId?: number;
}

export const courseApi = {
  getCourses: () => getData<CourseListDto[]>({ method: 'GET', url: '/concepts/courses' }),
  getPendingCourses: () => getData<CourseListDto[]>({ method: 'GET', url: '/concepts/courses/pending' }),
  getCourseById: (id: string | number) => getData<CourseDetailDto>({ method: 'GET', url: `/concepts/courses/${encodeURIComponent(String(id))}` }),
  createCourse: (payload: CourseUpsertPayload) =>
    getData<CourseDetailDto>({ method: 'POST', url: '/concepts/courses', data: payload }),
  updateCourse: (id: string | number, payload: CourseUpsertPayload) =>
    getData<CourseDetailDto>({ method: 'PUT', url: `/concepts/courses/${encodeURIComponent(String(id))}`, data: payload }),
  deleteCourse: (id: string | number) =>
    client.delete(`/concepts/courses/${encodeURIComponent(String(id))}`),
  assignToClasses: (courseId: string | number, classIds: number[]) =>
    getData<{ message: string }>({ method: 'POST', url: `/concepts/courses/${encodeURIComponent(String(courseId))}/assign-classes`, data: { classIds } }),
  addCourseNode: (courseId: string | number, payload: CourseNodePayload) =>
    getData<unknown>({ method: 'POST', url: `/concepts/courses/${encodeURIComponent(String(courseId))}/nodes`, data: payload }),
  deleteCourseNode: (courseId: string | number, nodeId: number) =>
    client.delete(`/concepts/courses/${encodeURIComponent(String(courseId))}/nodes/${nodeId}`),
  reorderCourseNodes: (courseId: string | number, nodeIds: number[]) =>
    client.put(`/concepts/courses/${encodeURIComponent(String(courseId))}/reorder`, { nodeIds }),

  submitCourseForReview: (courseId: string | number) =>
    getData<{ message: string }>({ method: 'POST', url: `/concepts/courses/${encodeURIComponent(String(courseId))}/submit-review` }),

  reviewCourse: (courseId: string | number, payload: { approve: boolean; reason?: string }) =>
    getData<{ message: string }>({ method: 'POST', url: `/concepts/courses/${encodeURIComponent(String(courseId))}/review`, data: payload }),

  submitCourseFeedback: (payload: CourseFeedbackPayload) =>
    getData<CourseFeedbackDto>({ method: 'POST', url: '/courses/feedback', data: payload }),

  getMyCourseFeedback: (courseId: number) =>
    getData<CourseFeedbackDto[]>({ method: 'GET', url: '/courses/feedback/mine', params: { courseId } }),

  getCourseFeedbackAll: (courseId: number, status?: string) =>
    getData<CourseFeedbackDto[]>({ method: 'GET', url: '/courses/feedback/all', params: { courseId, ...(status ? { status } : {}) } }),

  getTeacherFeedback: (params?: { courseId?: number; status?: string; type?: string }) =>
    getData<CourseFeedbackDto[]>({ method: 'GET', url: '/courses/feedback/for-teacher', params }),

  replyCourseFeedback: (id: number, payload: CourseFeedbackReplyPayload) =>
    getData<CourseFeedbackDto>({ method: 'PUT', url: `/courses/feedback/${id}`, data: payload }),
};
