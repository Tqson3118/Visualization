// engines/generators/sort/insertion.ts — Insertion Sort (SDD §4.7.3)
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { arrayStructure, buildGenerator, parseArrayParams, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure insertionSort(a[0..n-1])',
  '  for i ← 1 to n-1 do',
  '    key ← a[i]',
  '    j ← i-1',
  '    while j ≥ 0 and a[j] > key do',
  '      a[j+1] ← a[j]',
  '      j ← j-1',
  '    a[j+1] ← key',
  '    i-part → done',
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

export function createInsertionGenerator(): SimulationGenerator {
  return buildGenerator('sort.insertion', SCHEMA, PSEUDOCODE, {
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
      trace.vars.key = null;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}] được khởi tạo.`,
        structure: arrayStructure(a, statuses),
        annotations: ['i=1'],
      });

      for (let i = 1; i <= n - 1; i++) {
        trace.vars.i = i;
        trace.push({
          line: 2,
          explanation: `i=${i} — lấy phần tử a[${i}]=${a[i]} để chèn vào đoạn đã sắp xếp a[0..${i - 1}].`,
          structure: arrayStructure(a, statuses),
          annotations: [`i=${i}`],
        });

        const key = a[i];
        trace.vars.key = key;
        statuses[i] = 'highlight';
        trace.push({
          line: 3,
          explanation: `key ← a[${i}] = ${key}.`,
          structure: arrayStructure(a, statuses),
          annotations: [`key=${key}`],
        });

        let j = i - 1;
        trace.vars.j = j;
        trace.push({
          line: 4,
          explanation: `j ← i-1 = ${j}.`,
          structure: arrayStructure(a, statuses),
          annotations: [`j=${j}`],
        });

        while (j >= 0 && a[j] > key) {
          trace.stats.comparisons++;
          trace.push({
            line: 5,
            explanation: `Kiểm tra: j=${j} ≥ 0 và a[${j}]=${a[j]} > key=${key} → đúng, cần dịch sang phải.`,
            structure: arrayStructure(a, { ...statuses, [j]: 'active' }),
            annotations: [`${a[j]} > ${key} (đúng)`],
          });

          a[j + 1] = a[j];
          statuses[j + 1] = 'swap';
          trace.stats.writes++;
          trace.push({
            line: 6,
            explanation: `a[${j + 1}] ← a[${j}] = ${a[j]} (dịch sang phải).`,
            structure: arrayStructure(a, statuses),
            annotations: [`a[${j + 1}]=${a[j]}`],
          });

          j--;
          trace.vars.j = j;
          trace.push({
            line: 7,
            explanation: `j ← j-1 = ${j}.`,
            structure: arrayStructure(a, statuses),
            annotations: [`j=${j}`],
          });
        }

        if (j >= 0) trace.stats.comparisons++;
        trace.push({
          line: 5,
          explanation: j < 0
            ? `j=${j} < 0 → dừng dịch chuyển (đã hết đoạn đã sắp xếp).`
            : `a[${j}]=${a[j]} > key=${key} → sai, dừng dịch chuyển.`,
          structure: arrayStructure(a, j >= 0 ? { ...statuses, [j]: 'active' } : statuses),
          annotations: [`vị trí chèn: ${j + 1}`],
        });

        a[j + 1] = key;
        statuses[j + 1] = 'done';
        trace.stats.writes++;
        trace.push({
          line: 8,
          explanation: `Chèn key=${key} vào a[${j + 1}].`,
          structure: arrayStructure(a, statuses),
          annotations: [`a[${j + 1}]=${key}`],
        });

        for (let k = 0; k <= i; k++) statuses[k] = 'done';
        trace.push({
          line: 9,
          explanation: `Đoạn a[0..${i}] đã sắp xếp xong (${i + 1}/${n} phần tử).`,
          structure: arrayStructure(a, statuses),
          annotations: [`a[0..${i}] đúng vị trí`],
        });
      }

      trace.push({
        line: 10,
        explanation: `Kết thúc: mảng [${a.join(', ')}] đã sắp xếp.`,
        structure: arrayStructure(a, statuses),
      });
      return trace.steps;
    },
  });
}
