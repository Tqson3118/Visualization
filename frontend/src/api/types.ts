/** DTO dùng chung (API_REFERENCE §1.3, §3.11) */

/** Phản hồi phân trang chuẩn: { items, page, pageSize, total, totalPages } */
export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}

// ── Classes (Module H — API_REFERENCE §4.11) ──

export interface ClassDto {
  id: number;
  name: string;
  description: string | null;
  inviteCode: string;
  ownerId: number;
  memberCount: number;
  createdAt: string;
  /** Vai trò của người gọi trong lớp (quyết định tab/điều khiển hiển thị) */
  role: 'OWNER' | 'TEACHER' | 'STUDENT';
}

/** Thành viên lớp — ClassMemberDto (API_REFERENCE §4.11): chỉ có userId, KHÔNG có id. */
export interface ClassMemberDto {
  userId: number;
  displayName: string;
  email: string;
  joinedAt: string;
}

export interface ClassAssignmentDto {
  id: number;
  lessonId: number | null;
  exerciseId: number | null;
  title: string | null;
  dueAt: string | null;
  allowLateSubmission: boolean;
  /** Thứ tự trong lộ trình học của lớp (sort curriculum). */
  sortOrder: number;
  createdAt: string;
}

/** Chi tiết lớp — GET /classes/{id} + POST /classes/join-by-code (API_REFERENCE §4.11). */
export type ClassDetailDto = ClassDto & {
  members?: ClassMemberDto[];
  assignments?: ClassAssignmentDto[];
  /** Lộ trình học (curriculum) per-class. */
  curriculumTitle?: string | null;
  curriculumDescription?: string | null;
  curriculumPublished?: boolean;
};

// ── Learning Path / Curriculum per-class (feature port — teacher creates path) ──

/** Body PUT /classes/{id}/curriculum — meta + publish/unpublish. */
export interface ClassCurriculumUpsertRequest {
  title?: string | null;
  description?: string | null;
  published?: boolean | null;
}

export interface ClassCurriculumReorderItem {
  assignmentId: number;
  sortOrder: number;
}

export interface ClassCurriculumReorderRequest {
  items: ClassCurriculumReorderItem[];
}

/** Một item trong lộ trình — status: not_started | in_progress | completed. */
export interface ClassCurriculumItemDto {
  assignmentId: number;
  lessonId: number | null;
  exerciseId: number | null;
  title: string;
  itemType: 'lesson' | 'exercise';
  sortOrder: number;
  dueAt: string | null;
  status: 'not_started' | 'in_progress' | 'completed';
  bestScore: number | null;
}

/** Lộ trình học của lớp cho học viên — GET /classes/{id}/curriculum. */
export interface ClassCurriculumDto {
  classId: number;
  title: string | null;
  description: string | null;
  published: boolean;
  progressPct: number;
  items: ClassCurriculumItemDto[];
}

/** Thống kê 1 bài gán trong báo cáo lớp — khớp backend ClassReportAssignmentDto. */
export interface ClassReportAssignmentDto {
  assignmentId: number;
  title: string;
  dueAt: string | null;
  onTime: number;
  late: number;
  notSubmitted: number;
  avgScore: number;
}

/** Học viên chậm tiến độ (thiếu ≥ 2 bài gán) — khớp backend LaggingLearnerDto. */
export interface LaggingLearnerDto {
  userId: number;
  displayName: string;
  missingCount: number;
}

/** Báo cáo lớp — khớp backend ClassReportDto (GET /classes/{id}/report). */
export interface ClassReportDto {
  classId: number;
  className: string;
  totalMembers: number;
  assignments: ClassReportAssignmentDto[];
  laggingLearners: LaggingLearnerDto[];
}

// ── Benchmark (API_REFERENCE §4.14 — POST /benchmarks/run) ──

/** 1 điểm đo tại n — client gửi kèm (server không tự đo — ADR-012, SETUP_TODO §6.8) */
export interface BenchmarkMeasurementDto {
  n: number;
  durationMs: number | null;   // null = N/A (timeout)
  comparisons: number | null;
  swaps: number | null;
}

/** Kết quả đo 1 thuật toán — results[] bắt buộc trong BenchmarkRequest */
export interface BenchmarkResultDto {
  key: string;
  measurements: BenchmarkMeasurementDto[];
}

export interface BenchmarkRequest {
  keys: string[];
  sizes: number[];
  language?: string;
  /** Kết quả đo phía client (server không tự đo được — ADR-012) */
  results?: BenchmarkResultDto[];
}

export interface BenchmarkCellDto {
  key: string;
  durationMs: number | null;   // null = N/A (timeout)
  comparisons: number | null;
  swaps: number | null;
  writes: number | null;
}

export interface BenchmarkRowDto {
  size: number;
  results: BenchmarkCellDto[];
}

export interface BenchmarkRunDto {
  keys: string[];
  sizes: number[];
  rows: BenchmarkRowDto[];
  conclusion: string | null;
  measuredAt: string;
}

// ── Favorites (API_REFERENCE §4.9) ──

export interface FavoriteDto {
  id: number;
  simKey: string;
  title: string;
  input: unknown;
  createdAt: string;
}

// ── Admin settings (API_REFERENCE §4.10) ──

export interface SystemSettingsDto {
  siteName: string;
  allowedDomains: string[];      // domain email được phép đăng ký
  passwordPolicy: { minLength: number; requireUppercase: boolean; requireDigit: boolean; requireSpecial: boolean };
  uploadMaxMb: number;
  sandboxSeconds: number;
  sandboxMemoryMb: number;
}
