// features/code-to-visual/dsl/types.ts — Code-to-Visual DSL
// DSL giới hạn (KHÔNG chạy arbitrary code): array/stack/queue operations
// parser -> trace events -> sim steps (data-driven playback, interpreter in-process).

export type DslStructureKind = 'array' | 'stack' | 'queue';

export type DslOperation =
  | 'create'
  | 'push'
  | 'pop'
  | 'peek'
  | 'set'
  | 'swap'
  | 'enqueue'
  | 'dequeue'
  | 'front';

/** Một lệnh DSL đã parse (line 1-based trong source). */
export interface ParsedOp {
  target: DslStructureKind;
  op: 'push' | 'pop' | 'peek' | 'set' | 'swap' | 'enqueue' | 'dequeue' | 'front';
  line: number;
  value?: number;
  index?: number;
  i?: number;
  j?: number;
}

/** Trace event — snapshot sau mỗi thao tác (data-driven playback). */
export interface TraceEvent {
  step: number;
  line: number; // dòng DSL 1-based (carry-through lúc parse)
  structure: DslStructureKind;
  operation: DslOperation;
  state: number[]; // snapshot mảng giá trị của structure đó
  highlightedIndices?: number[];
  explanation: string;
}

/** Lỗi parser/interpreter — kèm line number để highlight. */
export interface DslError {
  line: number;
  message: string;
}

/** Console log entry (editor panel). */
export type ConsoleLogType = 'info' | 'success' | 'error' | 'warn';

export interface ConsoleLogEntry {
  text: string;
  type: ConsoleLogType;
  timestamp: string;
}
