import { getData } from '@/api/client';

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
  xpReward: number;
  totalLessons: number;
  completedLessons: number;
  progressPercent: number;
  lessons?: CourseLessonDto[];
}

export const courseApi = {
  getCourses: () => getData<CourseListDto[]>({ method: 'GET', url: '/concepts/courses' }),
  getCourseById: (id: string) => getData<CourseDetailDto>({ method: 'GET', url: `/concepts/courses/${encodeURIComponent(id)}` }),

  submitCourseFeedback: (payload: CourseFeedbackPayload) =>
    getData<CourseFeedbackDto>({ method: 'POST', url: '/courses/feedback', data: payload }),

  getMyCourseFeedback: (courseId: number) =>
    getData<CourseFeedbackDto[]>({ method: 'GET', url: '/courses/feedback/mine', params: { courseId } }),

  getCourseFeedbackAll: (courseId: number, status?: string) =>
    getData<CourseFeedbackDto[]>({ method: 'GET', url: '/courses/feedback/all', params: { courseId, ...(status ? { status } : {}) } }),

  replyCourseFeedback: (id: number, payload: CourseFeedbackReplyPayload) =>
    getData<CourseFeedbackDto>({ method: 'PUT', url: `/courses/feedback/${id}`, data: payload }),
};
