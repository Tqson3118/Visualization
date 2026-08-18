// features/visual-shell/types/sharedVisualFrame.ts
// Contract chung (B2) cho MỌI visual: legacy engine Step[] · sandbox SortFrame[]
// · Code-to-Visual DSL (TraceEvent → Step → adapter). Đây là ngôn ngữ trung gian
// giữa các nguồn dữ liệu khác nhau và SharedVisualizerShell.vue.

export type SharedVisualStatus =
  | 'idle'
  | 'running'
  | 'comparing'
  | 'swapping'
  | 'visited'
  | 'done'
  | 'error';

export interface SharedVisualFrame {
  /** Key catalog chuẩn, VD: 'sort.bubble' (D6) — dùng để map renderer + title. */
  algorithmKey: string;
  /** Vị trí bước hiện tại (0-based) trong tổng số bước. */
  stepIndex: number;
  /** Tổng số bước của toàn bộ mô phỏng. */
  totalSteps: number;
  /** Mô tả bước bằng tiếng Việt (từ explanation/description). */
  description: string;
  /** Danh sách id phần tử đang được tô sáng (VD: 'cell:2', 'node:5'). */
  highlights?: string[];
  /** Dữ liệu khung hình tùy loại renderer: arrayState | structure | ... */
  data: unknown;
  /** Dòng pseudocode 1-based đang active (dùng trong code view). */
  pseudocodeLine?: number;
  /** Trạng thái tổng thể của bước (dùng cho HUD/legend). */
  status?: SharedVisualStatus;
}
