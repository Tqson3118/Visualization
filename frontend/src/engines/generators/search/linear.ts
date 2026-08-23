// engines/generators/search/linear.ts — Linear Search (SDD §4.7.7, §4.6.2)
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { arrayStructure, buildGenerator, intField, intArrayField, parseArrayParams, strField, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure linearSearch(a[0..n-1], target)',
  '  for i ← 0 to n-1 do',
  '    if a[i] = target then',
  '      return i             // tìm thấy',
  '  return -1                // không thấy',
  '  end procedure',
];

const SCHEMA: InputSchema = {
  kind: 'array',
  fields: [
    { name: 'target', type: 'int', label: 'Giá trị cần tìm', min: -999, max: 999, default: 42, description: 'Giá trị cần tìm trong mảng' },
    { name: 'inputSource', type: 'select', label: 'Nguồn dữ liệu', options: [
      { label: 'Ngẫu nhiên', value: 'random' },
      { label: 'Tự nhập', value: 'manual' },
    ], default: 'random', description: 'Chọn nguồn dữ liệu đầu vào' },
    { name: 'values', type: 'int[]', label: 'Dãy số', min: -999, max: 999, default: [5, 3, 8, 1, 9, 2], description: 'Dãy số khi chọn Tự nhập (phân cách dấu phẩy)' },
    { name: 'size', type: 'int', label: 'Số lượng phần tử', min: 2, max: 100, default: 15, description: 'Số phần tử khi dùng ngẫu nhiên' },
    { name: 'minValue', type: 'int', label: 'Giá trị tối thiểu', min: -999, max: 999, default: 0, description: 'Cận dưới phạm vi giá trị ngẫu nhiên' },
    { name: 'maxValue', type: 'int', label: 'Giá trị tối đa', min: -999, max: 999, default: 99, description: 'Cận trên phạm vi giá trị ngẫu nhiên' },
  ],
};

export function createLinearGenerator(): SimulationGenerator {
  return buildGenerator('search.linear', SCHEMA, PSEUDOCODE, {
    validate(input: InputConfig) {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const errors = validateArrayParams(input.data);
      const target = intField(rec, 'target', 42);
      if (target < -999 || target > 999) errors.push(`target: phải trong khoảng -999..999 (hiện tại ${target})`);
      const source = strField(rec, 'inputSource', 'random');
      if (source !== 'random' && source !== 'manual') errors.push(`inputSource: phải là 'random' hoặc 'manual' (hiện tại '${source}')`);
      if (source === 'manual' && !(Array.isArray(rec.values) && rec.values.length > 0)) {
        errors.push('inputSource=manual yêu cầu nhập values (2–100 phần tử)');
      }
      return { ok: errors.length === 0, errors };
    },

    generate(input: InputConfig) {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const target = intField(rec, 'target', 42);
      const source = strField(rec, 'inputSource', 'random');
      // inputSource=manual ưu tiên values nhập tay (nếu có), ngược lại sinh ngẫu nhiên.
      const manual = source === 'manual' || (Array.isArray(rec.values) && rec.values.length > 0);
      const params = parseArrayParams(input.data);
      const values = manual ? intArrayField(rec, 'values', [5, 3, 8, 1, 9, 2]) : params.values;
      const a = values.slice();
      const n = a.length;
      const trace = new Trace();
      const statuses: StatusMap = {};

      trace.vars.i = 0;
      trace.vars.target = target;
      trace.vars.found = false;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}], tìm target=${target}.`,
        structure: arrayStructure(a, statuses),
        annotations: [`target=${target}, n=${n}`],
      });

      let foundAt = -1;
      for (let i = 0; i < n; i++) {
        trace.vars.i = i;
        trace.push({
          line: 2,
          explanation: `Xét a[${i}]=${a[i]}.`,
          structure: arrayStructure(a, { ...statuses, [i]: 'active' }),
          annotations: [`i=${i}`],
        });
        trace.stats.comparisons++;
        trace.push({
          line: 3,
          explanation: `So sánh a[${i}]=${a[i]} và target=${target}.`,
          structure: arrayStructure(a, { ...statuses, [i]: 'active' }),
          annotations: [`a[${i}]=${a[i]} so với target=${target}?`],
        });
        if (a[i] === target) {
          statuses[i] = 'done';
          foundAt = i;
          trace.vars.found = true;
          trace.push({
            line: 4,
            explanation: `a[${i}]=${a[i]} = target=${target} → Tìm thấy tại vị trí ${i}.`,
            structure: arrayStructure(a, statuses),
            annotations: [`Tìm thấy tại vị trí ${i}`],
          });
          break;
        }
        trace.push({
          line: 3,
          explanation: `a[${i}]=${a[i]} = target=${target} → sai, tiếp tục duyệt.`,
          structure: arrayStructure(a, { ...statuses, [i]: 'muted' }),
        });
        statuses[i] = 'muted';
      }

      if (foundAt === -1) {
        for (let k = 0; k < n; k++) statuses[k] = 'muted';
        trace.push({
          line: 5,
          explanation: `Kết thúc: không tìm thấy target=${target} trong mảng (return -1).`,
          structure: arrayStructure(a, statuses),
          annotations: ['Không tìm thấy'],
        });
      } else {
        trace.push({
          line: 6,
          explanation: `Kết thúc: tìm thấy target=${target} tại vị trí ${foundAt}.`,
          structure: arrayStructure(a, statuses),
          annotations: [`Tìm thấy tại vị trí ${foundAt}`],
        });
      }
      return trace.steps;
    },
  });
}
