// features/code-to-visual/dsl/parser.ts — Parser DSL giới hạn (array/stack/queue)
// - Hỗ trợ: array.push(n) array.set(i,v) array.swap(i,j) array.pop()
//           stack.push(n) stack.pop() stack.peek()
//           queue.enqueue(n) queue.dequeue() queue.front()
// - Từ chối mọi lệnh lạ kèm line number (DslError { line, message }).
// - KHÔNG exec user code: parser chỉ sinh ParsedOp[] (interpreter in-process, không AST, không new Function).
import type { DslStructureKind, DslError, ParsedOp } from './types';

export interface ParseResult {
  ops: ParsedOp[];
  /** Toàn bộ dòng source gốc (giữ nguyên số dòng → activeLine khớp editor). */
  lines: string[];
}

const OPERATION_PATTERNS = [
  { re: /^array\.push\s*\(\s*(-?\d+)\s*\)$/, target: 'array' as DslStructureKind, op: 'push' as const },
  { re: /^array\.set\s*\(\s*(\d+)\s*,\s*(-?\d+)\s*\)$/, target: 'array' as DslStructureKind, op: 'set' as const },
  { re: /^array\.swap\s*\(\s*(\d+)\s*,\s*(\d+)\s*\)$/, target: 'array' as DslStructureKind, op: 'swap' as const },
  { re: /^array\.pop\s*\(\s*\)$/, target: 'array' as DslStructureKind, op: 'pop' as const },
  { re: /^stack\.push\s*\(\s*(-?\d+)\s*\)$/, target: 'stack' as DslStructureKind, op: 'push' as const },
  { re: /^stack\.pop\s*\(\s*\)$/, target: 'stack' as DslStructureKind, op: 'pop' as const },
  { re: /^stack\.peek\s*\(\s*\)$/, target: 'stack' as DslStructureKind, op: 'peek' as const },
  { re: /^queue\.enqueue\s*\(\s*(-?\d+)\s*\)$/, target: 'queue' as DslStructureKind, op: 'enqueue' as const },
  { re: /^queue\.dequeue\s*\(\s*\)$/, target: 'queue' as DslStructureKind, op: 'dequeue' as const },
  { re: /^queue\.front\s*\(\s*\)$/, target: 'queue' as DslStructureKind, op: 'front' as const },
];

function isBlank(line: string): boolean {
  return line.trim().length === 0;
}

/** Dòng comment: bắt đầu bằng //, # hoặc -- */
function isComment(line: string): boolean {
  const trimmed = line.trim();
  return trimmed.startsWith('//') || trimmed.startsWith('#') || trimmed.startsWith('--');
}

export function parseDsl(source: string): ParseResult {
  const rawLines = source.split(/\r?\n/);
  const lines = rawLines.slice();
  const ops: ParsedOp[] = [];

  rawLines.forEach((raw, idx) => {
    const lineNo = idx + 1;
    const line = raw.trim();
    if (isBlank(line) || isComment(line)) return;

    for (const pattern of OPERATION_PATTERNS) {
      const match = pattern.re.exec(line);
      if (!match) continue;
      const op: ParsedOp = { target: pattern.target, op: pattern.op, line: lineNo };
      if (pattern.op === 'set') {
        op.index = Number(match[1]);
        op.value = Number(match[2]);
      } else if (pattern.op === 'swap') {
        op.i = Number(match[1]);
        op.j = Number(match[2]);
      } else if (match[1] !== undefined) {
        op.value = Number(match[1]);
      }
      ops.push(op);
      return;
    }

    const err: DslError = {
      line: lineNo,
      message: 'Lệnh không hợp lệ: ' + line + ' — chỉ hỗ trợ array.push/set/swap/pop, stack.push/pop/peek, queue.enqueue/dequeue/front.',
    };
    throw err;
  });

  return { ops, lines };
}