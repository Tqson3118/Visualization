// engines/__tests__/stepExecutor.spec.ts
// Bê từ source/VisualizationDSA3/frontend/src/core/__tests__/CompilerStepExecutor.instrumentation.spec.ts (V3)
// + bổ sung test ADAPTER EDV (SDD §4.0.3): runCode → TraceEvent, runMeasure.

import { describe, expect, it } from 'vitest';

import {
  CompilerStepExecutor,
  runCode,
  runMeasure,
  type CodeSimulation,
} from '../core/stepExecutor';

const compile = (code: string, array?: number[]) =>
  CompilerStepExecutor.compileAlgorithm(code, [], { array, fallbackToRegex: false });

describe('CompilerStepExecutor instrumentation (Babel AST — bê V3)', () => {
  it('single-line infinite loop is caught by the step guard', () => {
    const code = `let i = 0;\nwhile (true) { i++; }`;
    expect(() => compile(code, [1, 2, 3])).toThrow(/giới hạn thực thi/);
  });

  it('empty-body infinite loop is caught by the loop guard', () => {
    const code = `while (true) {}`;
    expect(() => compile(code, [1, 2, 3])).toThrow(/giới hạn lặp/);
  });

  it('single-statement loop body wrapped in a block stays attached', () => {
    const code = `let s = 0;\nfor (let i = 0; i < 3; i++) s = s + i;\nlog("done " + s);`;
    const frames = compile(code, [1, 2, 3]);
    const lastVars = frames[frames.length - 1].canvasStateSnapshot.loopVariables;
    expect(lastVars?.s).toBe(3);
  });

  it('multi-variable declarations track every name', () => {
    const code = `let a = 1, b = 2, c = 3;\ncompare(0, 1);`;
    const frames = compile(code, [2, 1]);
    const names = new Set<string>();
    for (const f of frames) {
      for (const k of Object.keys(f.canvasStateSnapshot.loopVariables ?? {})) names.add(k);
    }
    expect(names.has('a')).toBe(true);
    expect(names.has('b')).toBe(true);
    expect(names.has('c')).toBe(true);
  });

  it('statements before declarations do not crash (TDZ-safe via safeVars)', () => {
    const code = `foo();\nfunction foo() {\n  const x = 1;\n  compare(0, 0);\n}`;
    expect(() => compile(code, [5, 3])).not.toThrow();
  });

  it('recursive functions keep per-call local state (closure intact)', () => {
    const code = `let visits = 0;\nfunction count(n) {\n  visits = visits + 1;\n  const next = n - 1;\n  if (next > 0) {\n    count(next);\n  }\n  log("n=" + n);\n}\ncount(4);`;
    const frames = compile(code, [1, 2, 3]);
    const lastVars = frames[frames.length - 1].canvasStateSnapshot.loopVariables;
    expect(lastVars?.visits).toBe(4);
  });

  it('nested if/else branches preserve semantics', () => {
    const code = `let r = 0;\nconst v = 5;\nif (v > 10) {\n  r = 1;\n} else {\n  r = 2;\n}\nlog("r=" + r);`;
    const frames = compile(code, [1, 2]);
    const lastVars = frames[frames.length - 1].canvasStateSnapshot.loopVariables;
    expect(lastVars?.r).toBe(2);
  });

  it('else-if chains keep correct branching', () => {
    const code = `let r = 0;\nconst v = 5;\nif (v > 10) {\n  r = 1;\n} else if (v > 3) {\n  r = 2;\n} else {\n  r = 3;\n}\nlog("r=" + r);`;
    const frames = compile(code, [1, 2]);
    const lastVars = frames[frames.length - 1].canvasStateSnapshot.loopVariables;
    expect(lastVars?.r).toBe(2);
  });

  it('loop variables are captured per iteration', () => {
    const code = `for (let i = 0; i < 3; i++) {\n  log("it " + i);\n}`;
    const frames = compile(code, [1, 2, 3]);
    const iValues = new Set<number>();
    for (const f of frames) {
      const i = f.canvasStateSnapshot.loopVariables?.i;
      if (typeof i === 'number') iValues.add(i);
    }
    expect(iValues.has(0)).toBe(true);
    expect(iValues.has(1)).toBe(true);
    expect(iValues.has(2)).toBe(true);
  });

  it('try/catch blocks still compile and run', () => {
    const code = `let caught = 0;\ntry {\n  const a = 1;\n  throw new Error("x");\n} catch (err) {\n  caught = 1;\n}\nlog("caught=" + caught);`;
    const frames = compile(code, [1, 2]);
    const lastVars = frames[frames.length - 1].canvasStateSnapshot.loopVariables;
    expect(lastVars?.caught).toBe(1);
  });
});

describe('ADAPTER EDV — runCode (SDD §4.0.3)', () => {
  const SIM: CodeSimulation = {
    code: 'compare(arr[0], arr[1]);\nswap(arr[0], arr[1]);',
    entry: 'main',
    bindings: [{ variable: 'arr', structure: 'array' }],
  };

  it('trace đúng contract: line/vars/highlight/kind/explanation', () => {
    const result = runCode(SIM, [10, 20]);
    expect(result.error).toBeUndefined();
    expect(result.trace.length).toBeGreaterThan(0);

    const swapEvent = result.trace.find((e) => e.kind === 'swap');
    expect(swapEvent).toBeDefined();
    expect(swapEvent?.highlight).toContain('cell:0');
    expect(swapEvent?.highlight).toContain('cell:1');
    expect(swapEvent?.explanation).toContain('Tráo đổi');
    expect(Array.isArray(swapEvent?.vars.array)).toBe(true);
    expect((swapEvent?.vars.array as number[])).toEqual([20, 10]);
  });

  it('stats đếm comparisons/swaps/writes tích lũy', () => {
    const result = runCode(SIM, [10, 20]);
    expect(result.stats.comparisons).toBeGreaterThan(0);
    expect(result.stats.swaps).toBe(1);
    expect(result.stats.durationMs).toBeGreaterThanOrEqual(0);
  });

  it('output = mảng cuối sau khi chạy', () => {
    const result = runCode(SIM, [10, 20]);
    expect(result.output).toEqual([20, 10]);
  });

  it('code lỗi → RunResult.error có line + message, trace rỗng', () => {
    const bad: CodeSimulation = { code: 'let a = ;', entry: 'main', bindings: [] };
    const result = runCode(bad, [1]);
    expect(result.error).toBeDefined();
    expect(result.error?.line).toBeGreaterThanOrEqual(0);
    expect(result.trace).toEqual([]);
  });
});

describe('ADAPTER EDV — runMeasure (SDD §4.0.3 v2.5)', () => {
  it('đo comparisons/swaps/writes không sinh trace', () => {
    const code = 'compare(arr[0], arr[1]);\nswap(arr[0], arr[1]);';
    const result = runMeasure(code, [10, 20]);
    expect(result).not.toBeNull();
    expect(result?.comparisons).toBe(1);
    expect(result?.swaps).toBe(1);
    expect(result?.durationMs).toBeGreaterThanOrEqual(0);
  });

  it('timeout 5 giây → trả null (chế độ đo không giới hạn event)', () => {
    const code = `let i = 0;\nwhile (true) { i++; }`;
    const result = runMeasure(code, [1, 2, 3]);
    expect(result).toBeNull();
  });
});
