// engines/generators/sort/selection.ts — Selection Sort (SDD §4.7.2)
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { arrayStructure, buildGenerator, parseArrayParams, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure selectionSort(a[0..n-1])',
  '  for i ← 0 to n-2 do',
  '    minIdx ← i',
  '    for j ← i+1 to n-1 do',
  '      if a[j] < a[minIdx] then',
  '        minIdx ← j',
  '    if minIdx ≠ i then',
  '      swap a[i], a[minIdx]',
  '    a[i] ← done',
  '  end procedure',
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

export function createSelectionGenerator(): SimulationGenerator {
  return buildGenerator('sort.selection', SCHEMA, PSEUDOCODE, {
    validate(input: InputConfig) {
      const errors = validateArrayParams(input.data);
      return { ok: errors.length === 0, errors };
    },

    generate(input: InputConfig) {
      const { values } = parseArrayParams(input.data);
      const a = values.slice();
      const n = a.length;
      const trace = new Trace();
      const statuses: StatusMap = {};

      trace.vars.i = 0;
      trace.vars.j = 0;
      trace.vars.minIdx = 0;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}] được khởi tạo.`,
        structure: arrayStructure(a, statuses),
        annotations: ['i=0, minIdx=0'],
      });

      for (let i = 0; i <= n - 2; i++) {
        trace.vars.i = i;
        if (i > 0) {
          trace.push({
            line: 2,
            explanation: `i=${i} (vòng ngoài lần ${i + 1}).`,
            structure: arrayStructure(a, statuses),
            annotations: [`i=${i}`],
          });
        }

        trace.vars.minIdx = i;
        trace.push({
          line: 3,
          explanation: `minIdx = i = ${i} — coi a[${i}]=${a[i]} là phần tử nhỏ nhất hiện tại.`,
          structure: arrayStructure(a, statuses),
          annotations: [`minIdx=${i}`],
        });

        for (let j = i + 1; j <= n - 1; j++) {
          trace.vars.j = j;
          trace.push({
            line: 4,
            explanation: `Xét a[${j}]=${a[j]} trong đoạn tìm min.`,
            structure: arrayStructure(a, { ...statuses, [j]: 'active', [trace.vars.minIdx as number]: 'highlight' }),
            annotations: [`j=${j}, minIdx=${trace.vars.minIdx}`],
          });

          const minIdx = trace.vars.minIdx as number;
          trace.stats.comparisons++;
          trace.push({
            line: 5,
            explanation: `So sánh a[${j}]=${a[j]} và a[${minIdx}]=${a[minIdx]}.`,
            structure: arrayStructure(a, { ...statuses, [j]: 'active', [minIdx]: 'highlight' }),
            annotations: [`a[${j}]=${a[j]} < a[${minIdx}]=${a[minIdx]}?`],
          });
          if (a[j] < a[minIdx]) {
            trace.vars.minIdx = j;
            trace.push({
              line: 6,
              explanation: `${a[j]} < ${a[minIdx]} → đúng, cập nhật minIdx = ${j}.`,
              structure: arrayStructure(a, { ...statuses, [j]: 'highlight' }),
              annotations: [`minIdx=${j}`],
            });
          } else {
            trace.push({
              line: 5,
              explanation: `${a[j]} < ${a[minIdx]} → sai, giữ minIdx = ${minIdx}.`,
              structure: arrayStructure(a, { ...statuses, [j]: 'active', [minIdx]: 'highlight' }),
            });
          }
        }

        const minIdx = trace.vars.minIdx as number;
        if (minIdx !== i) {
          statuses[i] = 'swap';
          statuses[minIdx] = 'swap';
          const oldI = a[i];
          const oldMin = a[minIdx];
          a[i] = oldMin;
          a[minIdx] = oldI;
          trace.stats.swaps++;
          trace.push({
            line: 8,
            explanation: `minIdx=${minIdx} ≠ i=${i} → hoán đổi a[${i}] (${oldI}) và a[${minIdx}] (${oldMin}).`,
            structure: arrayStructure(a, statuses),
            annotations: ['hoán đổi'],
          });
        } else {
          trace.push({
            line: 7,
            explanation: `minIdx=${i} = i → a[${i}]=${a[i]} đã là phần tử nhỏ nhất trong đoạn, không cần hoán đổi.`,
            structure: arrayStructure(a, { ...statuses, [i]: 'highlight' }),
            annotations: [`a[${i}]=${a[i]} đã là min`],
          });
        }

        statuses[i] = 'done';
        trace.push({
          line: 9,
          explanation: `a[${i}]=${a[i]} đã nằm đúng vị trí cuối cùng.`,
          structure: arrayStructure(a, statuses),
          annotations: [`a[${i}] đúng vị trí`],
        });
      }

      for (let k = 0; k < n; k++) statuses[k] = 'done';
      trace.push({
        line: 10,
        explanation: `Kết thúc: mảng [${a.join(', ')}] đã sắp xếp.`,
        structure: arrayStructure(a, statuses),
      });
      return trace.steps;
    },
  });
}
