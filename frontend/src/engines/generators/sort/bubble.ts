// engines/generators/sort/bubble.ts — Bubble Sort (SDD §4.7.1, §4.9/4.9A)
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { arrayStructure, buildGenerator, parseArrayParams, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure bubbleSort(a[0..n-1])',
  '  for i ← 0 to n-2 do',
  '    swapped ← false',
  '    for j ← 0 to n-2-i do',
  '      if a[j] > a[j+1] then',
  '        swap a[j], a[j+1]',
  '        swapped ← true',
  '    if swapped = false then',
  '      return          // mảng đã sắp xếp',
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

export function createBubbleGenerator(): SimulationGenerator {
  return buildGenerator('sort.bubble', SCHEMA, PSEUDOCODE, {
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
      trace.vars.swapped = false;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}] được khởi tạo.`,
        structure: arrayStructure(a, statuses),
        annotations: ['i=0, j=0'],
      });
      if (n >= 90) {
        trace.push({
          line: 1,
          explanation: `Dữ liệu lớn (${n} phần tử), mô phỏng có thể chậm.`,
          structure: arrayStructure(a, statuses),
          annotations: ['Dữ liệu lớn, mô phỏng có thể chậm'],
        });
      }

      for (let i = 0; i <= n - 2; i++) {
        trace.vars.i = i;
        // Trace chuẩn (§4.9A): pass đầu tiên không có bước dòng 2/3 (đã gộp vào bước khởi tạo).
        if (i > 0) {
          trace.push({
            line: 2,
            explanation: `i=${i} (vòng ngoài lần ${i + 1}).`,
            structure: arrayStructure(a, statuses),
            annotations: [`i=${i}`],
          });
          trace.vars.swapped = false;
          trace.push({
            line: 3,
            explanation: 'Đặt swapped = false cho vòng lặp mới.',
            structure: arrayStructure(a, statuses),
            annotations: ['swapped=false'],
          });
        }

        // Trace chuẩn (§4.9A) dùng cận vòng trong j ≤ n-2 (không trừ i) → [3,1,2] đúng 21 bước,
        // 4 phép so sánh; phần tử a[n-1-i] vẫn đứng đúng vị trí sau mỗi pass.
        for (let j = 0; j <= n - 2; j++) {
          trace.vars.j = j;
          if (statuses[j] !== 'done') statuses[j] = 'default';
          if (statuses[j + 1] !== 'done') statuses[j + 1] = 'default';
          trace.push({
            line: 4,
            explanation: `Bắt đầu vòng lặp trong với j=${j}.`,
            structure: arrayStructure(a, statuses),
            annotations: [`j=${j}`],
          });

          const x = a[j];
          const y = a[j + 1];
          if (statuses[j] !== 'done') statuses[j] = 'active';
          if (statuses[j + 1] !== 'done') statuses[j + 1] = 'active';
          trace.stats.comparisons++;
          trace.push({
            line: 5,
            explanation: `So sánh a[${j}]=${x} và a[${j + 1}]=${y}.`,
            structure: arrayStructure(a, statuses),
            annotations: [`so sánh a[${j}]=${x} > a[${j + 1}]=${y}?`],
          });

          if (x > y) {
            a[j] = y;
            a[j + 1] = x;
            statuses[j] = 'swap';
            statuses[j + 1] = 'swap';
            trace.stats.swaps++;
            trace.push({
              line: 6,
              explanation: `${x} > ${y} → đúng, hoán đổi a[${j}] và a[${j + 1}].`,
              structure: arrayStructure(a, statuses),
              annotations: ['hoán đổi'],
            });
            trace.vars.swapped = true;
            trace.push({
              line: 7,
              explanation: 'Đánh dấu swapped = true (vòng trong có hoán đổi).',
              structure: arrayStructure(a, statuses),
              annotations: ['swapped=true'],
            });
          } else {
            trace.push({
              line: 5,
              explanation: `${x} > ${y} → sai, không hoán đổi.`,
              structure: arrayStructure(a, statuses),
            });
          }
          if (statuses[j] !== 'done') statuses[j] = 'default';
          if (statuses[j + 1] !== 'done') statuses[j + 1] = 'default';
        }

        statuses[n - 1 - i] = 'done';
        trace.push({
          line: 8,
          explanation:
            trace.vars.swapped === true
              ? 'Kết thúc vòng trong; swapped=true → tiếp tục vòng ngoài.'
              : 'Kết thúc vòng trong; swapped=false → mảng đã sắp xếp, thoát sớm.',
          structure: arrayStructure(a, statuses),
          annotations: [`a[${n - 1 - i}] đã nằm đúng vị trí`],
        });

        if (trace.vars.swapped === false) {
          for (let k = 0; k < n; k++) statuses[k] = 'done';
          trace.push({
            line: 9,
            explanation: 'swapped = false → mảng đã sắp xếp, kết thúc sớm (return).',
            structure: arrayStructure(a, statuses),
          });
          break;
        }
      }

      if (trace.steps.length === 0 || trace.steps[trace.steps.length - 1].pseudocodeLine !== 10) {
        for (let k = 0; k < n; k++) statuses[k] = 'done';
        trace.push({
          line: 10,
          explanation: `Kết thúc: mảng [${a.join(', ')}] đã sắp xếp.`,
          structure: arrayStructure(a, statuses),
        });
      }
      return trace.steps;
    },
  });
}
