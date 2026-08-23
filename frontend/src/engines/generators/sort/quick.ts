// engines/generators/sort/quick.ts — Quick Sort (Lomuto) (SDD §4.7.5)
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { arrayStructure, buildGenerator, parseArrayParams, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure quickSort(a, low, high)',
  '  if low ≥ high then return',
  '  p ← partition(a, low, high)',
  '  quickSort(a, low, p-1)',
  '  quickSort(a, p+1, high)',
  'procedure partition(a, low, high)',
  '  pivot ← a[high]',
  '  i ← low-1',
  '  for j ← low to high-1 do',
  '    if a[j] ≤ pivot then',
  '      i ← i+1',
  '      swap a[i], a[j]',
  '  swap a[i+1], a[high]      // pivot về đúng vị trí',
  '  return i+1',
  '  end procedures',
];

const SCHEMA: InputSchema = {
  kind: 'array',
  fields: [
    { name: 'values', type: 'int[]', label: 'Dãy số', min: -999, max: 999, default: [5, 3, 8, 1, 9, 2], description: 'Dãy số (phân cách dấu phẩy); để trống để dùng chế độ ngẫu nhiên' },
    { name: 'size', type: 'int', label: 'Số lượng phần tử', min: 2, max: 100, default: 15, description: 'Số phần tử khi dùng chế độ ngẫu nhiên' },
    { name: 'minValue', type: 'int', label: 'Giá trị tối thiểu', min: -999, max: 999, default: 0, description: 'Cận dưới phạm vi giá trị ngẫu nhiên' },
    { name: 'maxValue', type: 'int', label: 'Giá trị tối đa', min: -999, max: 999, default: 99, description: 'Cận trên phạm vi giá trị ngẫu nhiên' },
    { name: 'allowDuplicates', type: 'bool', label: 'Cho phép trùng lặp', default: true, description: 'Cho phép các giá trị trùng nhau' },
    { name: 'preset', type: 'select', label: 'Kiểu dữ liệu', options: [
      { label: 'Ngẫu nhiên', value: 'random' },
      { label: 'Tăng dần', value: 'sorted-asc' },
      { label: 'Giảm dần', value: 'sorted-desc' },
      { label: 'Gần như đã sắp xếp', value: 'nearly-sorted' },
      { label: 'Toàn giá trị bằng nhau', value: 'all-equal' },
      { label: 'Tự nhập', value: 'custom' },
    ], default: 'random', description: 'Kiểu dữ liệu khi không nhập values' },
  ],
};

export function createQuickGenerator(): SimulationGenerator {
  return buildGenerator('sort.quick', SCHEMA, PSEUDOCODE, {
    validate(input: InputConfig) {
      const errors = validateArrayParams(input.data);
      return { ok: errors.length === 0, errors };
    },

    generate(input: InputConfig) {
      const { values } = parseArrayParams(input.data);
      const a = values.slice();
      const n = a.length;
      const trace = new Trace();
      const doneMap: StatusMap = {};

      const segMap = (lo: number, hi: number, extra: StatusMap = {}): StatusMap => {
        const m: StatusMap = { ...doneMap, ...extra };
        for (let k = lo; k <= hi; k++) {
          if (doneMap[k] !== 'done' && !(k in extra)) m[k] = 'active';
        }
        return m;
      };

      trace.vars.low = 0;
      trace.vars.high = n - 1;
      trace.vars.i = null;
      trace.vars.j = null;
      trace.vars.pivot = null;
      trace.vars.p = null;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}] được khởi tạo.`,
        structure: arrayStructure(a, segMap(0, n - 1)),
        annotations: [`low=0, high=${n - 1}`],
      });

      const partition = (lo: number, hi: number): number => {
        const pivot = a[hi];
        trace.vars.pivot = pivot;
        trace.push({
          line: 7,
          explanation: `partition(a, ${lo}, ${hi}): pivot ← a[${hi}] = ${pivot}.`,
          structure: arrayStructure(a, segMap(lo, hi, { [hi]: 'highlight' })),
          annotations: [`pivot=a[${hi}]=${pivot}`],
        });
        let i = lo - 1;
        trace.vars.i = i;
        trace.push({
          line: 8,
          explanation: `i ← low-1 = ${lo - 1}.`,
          structure: arrayStructure(a, segMap(lo, hi, { [hi]: 'highlight' })),
          annotations: [`i=${i}`],
        });

        for (let j = lo; j <= hi - 1; j++) {
          trace.vars.j = j;
          trace.push({
            line: 9,
            explanation: `Vòng lặp partition với j=${j}.`,
            structure: arrayStructure(a, segMap(lo, hi, { [j]: 'active', [hi]: 'highlight' })),
            annotations: [`j=${j}`],
          });

          trace.stats.comparisons++;
          trace.push({
            line: 10,
            explanation: `So sánh a[${j}]=${a[j]} và pivot=${pivot}.`,
            structure: arrayStructure(a, segMap(lo, hi, { [j]: 'active', [hi]: 'highlight' })),
            annotations: [`a[${j}]=${a[j]} ≤ pivot=${pivot}?`],
          });

          if (a[j] <= pivot) {
            trace.push({
              line: 10,
              explanation: `a[${j}]=${a[j]} ≤ pivot=${pivot} → đúng.`,
              structure: arrayStructure(a, segMap(lo, hi, { [j]: 'active', [hi]: 'highlight' })),
            });
            i++;
            trace.vars.i = i;
            trace.push({
              line: 11,
              explanation: `i ← i+1 = ${i}.`,
              structure: arrayStructure(a, segMap(lo, hi, { [i]: 'active', [j]: 'active', [hi]: 'highlight' })),
              annotations: [`i=${i}`],
            });
            if (i !== j) {
              const x = a[i];
              a[i] = a[j];
              a[j] = x;
              trace.stats.swaps++;
              trace.push({
                line: 12,
                explanation: `Hoán đổi a[${i}]=${a[i]} và a[${j}]=${a[j]} (đưa phần tử ≤ pivot sang trái).`,
                structure: arrayStructure(a, segMap(lo, hi, { [i]: 'swap', [j]: 'swap', [hi]: 'highlight' })),
                annotations: ['hoán đổi'],
              });
            } else {
              trace.push({
                line: 12,
                explanation: `i=${i} = j=${j} → không cần hoán đổi.`,
                structure: arrayStructure(a, segMap(lo, hi, { [hi]: 'highlight' })),
              });
            }
          } else {
            trace.push({
              line: 10,
              explanation: `a[${j}]=${a[j]} ≤ pivot=${pivot} → sai, bỏ qua.`,
              structure: arrayStructure(a, segMap(lo, hi, { [j]: 'active', [hi]: 'highlight' })),
            });
          }
        }

        const x = a[i + 1];
        a[i + 1] = a[hi];
        a[hi] = x;
        trace.stats.swaps++;
        trace.push({
          line: 13,
          explanation: `Đưa pivot=${pivot} về vị trí: hoán đổi a[${i + 1}] và a[${hi}].`,
          structure: arrayStructure(a, segMap(lo, hi, { [i + 1]: 'swap', [hi]: 'swap' })),
          annotations: [`pivot về vị trí ${i + 1}`],
        });
        trace.vars.p = i + 1;
        trace.push({
          line: 14,
          explanation: `return i+1 = ${i + 1} — vị trí chia đôi của pivot.`,
          structure: arrayStructure(a, segMap(lo, hi, { [i + 1]: 'done' })),
          annotations: [`p=${i + 1}`],
        });
        return i + 1;
      };

      const quickSort = (lo: number, hi: number): void => {
        trace.push({
          line: 1,
          explanation: `quickSort(a, ${lo}, ${hi}) — sắp xếp đoạn a[${lo}..${hi}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
          annotations: [`đoạn [${lo}..${hi}]`],
        });
        if (lo >= hi) {
          trace.push({
            line: 2,
            explanation: `low=${lo} ≥ high=${hi} → đoạn có ≤ 1 phần tử, quay về.`,
            structure: arrayStructure(a, segMap(lo, hi)),
          });
          return;
        }
        trace.push({
          line: 3,
          explanation: `Gọi partition(a, ${lo}, ${hi}) để phân hoạch đoạn.`,
          structure: arrayStructure(a, segMap(lo, hi)),
        });
        const p = partition(lo, hi);
        doneMap[p] = 'done';
        trace.push({
          line: 13,
          explanation: `Pivot a[${p}]=${a[p]} đã nằm đúng vị trí cuối cùng.`,
          structure: arrayStructure(a, segMap(lo, hi, { [p]: 'done' })),
          annotations: [`a[${p}] đúng vị trí`],
        });
        trace.push({
          line: 4,
          explanation: `Đệ quy sắp xếp nửa trái a[${lo}..${p - 1}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
        });
        quickSort(lo, p - 1);
        trace.push({
          line: 5,
          explanation: `Đệ quy sắp xếp nửa phải a[${p + 1}..${hi}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
        });
        quickSort(p + 1, hi);
      };

      quickSort(0, n - 1);

      for (let k = 0; k < n; k++) doneMap[k] = 'done';
      trace.push({
        line: 15,
        explanation: `Kết thúc: mảng [${a.join(', ')}] đã sắp xếp.`,
        structure: arrayStructure(a, segMap(0, n - 1)),
      });
      return trace.steps;
    },
  });
}
