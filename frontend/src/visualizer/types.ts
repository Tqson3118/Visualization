// visualizer/types.ts — HỢP ĐỒNG CHUNG (Workstream B/B2)
// Các nguồn dữ liệu khác nhau — legacy engine Step[] (engines/core/types), DSL trace
// (features/code-to-visual/dsl), SortFrame/VCR của algorithm sandbox (reference) — được
// chuyển về SharedVisualFrame để một SharedVisualizerShell render chung được.

/** Trạng thái hiển thị dùng chung — tương ứng channel highlight cũ + BarStatus của sandbox mới. */
export type VisualizerStatus =
  | 'idle'
  | 'running'
  | 'comparing'
  | 'swapping'
  | 'visited'
  | 'done'
  | 'error';

/** Frame trung gian ổn định giữa engine và renderer. */
export interface SharedVisualFrame {
  algorithmKey: string;
  stepIndex: number;
  totalSteps: number;
  description: string;
  /** Danh sách id element được highlight (channel highlight). */
  highlights?: string[];
  /** Dữ liệu render — renderer adapter quyết định cách vẽ (mảng / graph / stack / tree / fallback). */
  data: unknown;
  /** Dòng pseudocode 1-based (0/undefined = chưa ánh xạ). */
  pseudocodeLine?: number;
  status?: VisualizerStatus;
  annotations?: string[];
  variables?: Record<string, string | number | boolean | null>;
}

/** Renderer adapter — contract cho registry renderer (array/sorting, searching, graph, stack/queue, tree, legacy). */
export interface VisualRendererAdapter<T = unknown> {
  readonly kind: string;
  supports(frame: SharedVisualFrame): boolean;
  render(frame: SharedVisualFrame): T;
}
