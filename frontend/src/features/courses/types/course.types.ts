export interface LessonReference {
  id: string;
  title: string;
  order: number;
}

export interface Course {
  id: string;
  title: string;
  description: string;
  category: string;
  difficulty: string;
  xpReward: number;
  /** Backend trả về dưới dạng coverImageUrl; UI ưu tiên field này rồi mới fallback coverImage */
  coverImageUrl?: string;
  coverImage?: string;
  isPremium: boolean;
  lessons?: LessonReference[];
  totalLessons: number;
  isPublished: boolean;
  topicId?: number;
  topicName?: string;
}

export interface CourseProgress {
  courseId: string;
  completedLessonIds: string[];
  totalLessons: number;
  progressPercent: number;
  xpEarned: number;
  isCompleted: boolean;
}
