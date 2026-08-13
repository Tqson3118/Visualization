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

export interface ClassMemberDto {
  id: number;
  displayName: string;
  email: string;
  role: 'STUDENT' | 'TEACHER';
  joinedAt: string;
}

export interface ClassAssignmentDto {
  id: number;
  lessonId: number | null;
  exerciseId: number | null;
  dueAt: string | null;
  status: 'open' | 'closed';
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
