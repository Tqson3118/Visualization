// engines/generators/search/binary.ts — Binary Search (SDD §4.7.8, §4.6.2, §4.9B)
// Dữ liệu chưa sắp xếp → TỰ SẮP XẾP kèm annotation banner (không lỗi — SDD §4.14).
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { arrayStructure, buildGenerator, intField, intArrayField, parseArrayParams, strField, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure binarySearch(a[0..n-1], target)   // a đã sắp xếp',
  '  low ← 0, high ← n-1',
  '  while low ≤ high do',
  '    mid ← (low + high) / 2',
  '    if a[mid] = target then return mid',
  '    if a[mid] < target then low ← mid+1',
  '    else high ← mid-1',
  '  return -1',
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
    { name: 'values', type: 'int[]', label: 'Dãy số', min: -999, max: 999, default: [2, 5, 8, 12, 19, 23], description: 'Dãy số khi chọn Tự nhập (phân cách dấu phẩy)' },
    { name: 'size', type: 'int', label: 'Số lượng phần tử', min: 2, max: 100, default: 15, description: 'Số phần tử khi dùng ngẫu nhiên' },
    { name: 'minValue', type: 'int', label: 'Giá trị tối thiểu', min: -999, max: 999, default: 0, description: 'Cận dưới phạm vi giá trị ngẫu nhiên' },
    { name: 'maxValue', type: 'int', label: 'Giá trị tối đa', min: -999, max: 999, default: 99, description: 'Cận trên phạm vi giá trị ngẫu nhiên' },
  ],
};

/** Bản đồ trạng thái: ngoài đoạn [low..high] → muted; mid → highlight (hoặc active khi so sánh); found → done. */
function binaryStatuses(
  n: number,
  low: number,
  high: number,
  mid: number | null,
  statuses: StatusMap,
  foundIdx: number | null,
  midActive = false,
): StatusMap {
  const m: StatusMap = { ...statuses };
  for (let k = 0; k < n; k++) {
    if (foundIdx !== null && k === foundIdx) m[k] = 'done';
    else if (mid !== null && k === mid) m[k] = midActive ? 'active' : 'highlight';
    else if (k < low || k > high) m[k] = 'muted';
    else if (m[k] !== 'done') m[k] = 'default';
  }
  return m;
}

export function createBinaryGenerator(): SimulationGenerator {
  return buildGenerator('search.binary', SCHEMA, PSEUDOCODE, {
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
      const manual = source === 'manual' || (Array.isArray(rec.values) && rec.values.length > 0);
      const params = parseArrayParams(input.data);
      const values = manual ? intArrayField(rec, 'values', [2, 5, 8, 12, 19, 23]) : params.values;
      const a = values.slice();
      const n = a.length;
      const trace = new Trace();
      const statuses: StatusMap = {};

      trace.vars.low = 0;
      trace.vars.high = n - 1;
      trace.vars.mid = null;
      trace.vars.target = target;
      trace.vars.found = false;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}], tìm target=${target}.`,
        structure: arrayStructure(a, statuses),
        annotations: [`target=${target}, n=${n}`],
      });

      // Tự sắp xếp nếu dữ liệu chưa tăng dần (SDD §4.14) — kèm banner, không phải lỗi.
      let sorted = true;
      for (let k = 0; k + 1 < n; k++) {
        if (a[k] > a[k + 1]) { sorted = false; break; }
      }
      if (!sorted) {
        a.sort((x, y) => x - y);
        trace.push({
          line: 1,
          explanation: `Dữ liệu chưa sắp xếp → tự sắp xếp thành [${a.join(', ')}] trước khi tìm kiếm.`,
          structure: arrayStructure(a, statuses),
          annotations: ['Dữ liệu chưa sắp xếp → tự sắp xếp'],
        });
      }

      let low = 0;
      let high = n - 1;
      trace.vars.low = low;
      trace.vars.high = high;
      trace.push({
        line: 2,
        explanation: `Khởi tạo low = 0, high = ${n - 1}.`,
        structure: arrayStructure(a, binaryStatuses(n, low, high, null, statuses, null)),
        annotations: [`low=${low}, high=${high}`],
      });

      let foundIdx: number | null = null;
      let guard = 0;
      while (low <= high && guard < n + 2) {
        guard++;
        trace.vars.low = low;
        trace.vars.high = high;
        trace.push({
          line: 3,
          explanation: `Kiểm tra low=${low} ≤ high=${high} → đúng, tiếp tục tìm kiếm.`,
          structure: arrayStructure(a, binaryStatuses(n, low, high, null, statuses, null)),
          annotations: [`low=${low} ≤ high=${high}`],
        });

        const mid = Math.floor((low + high) / 2);
        trace.vars.mid = mid;
        trace.push({
          line: 4,
          explanation: `mid = (${low} + ${high}) / 2 = ${mid} (làm tròn xuống).`,
          structure: arrayStructure(a, binaryStatuses(n, low, high, mid, statuses, null)),
          annotations: [`mid=${mid}, a[${mid}]=${a[mid]}`],
        });

        trace.stats.comparisons++;
        trace.push({
          line: 5,
          explanation: `So sánh a[${mid}]=${a[mid]} và target=${target}.`,
          structure: arrayStructure(a, binaryStatuses(n, low, high, mid, statuses, null, true)),
          annotations: [`a[${mid}]=${a[mid]} = target=${target}?`],
        });
        if (a[mid] === target) {
          foundIdx = mid;
          statuses[mid] = 'done';
          trace.vars.found = true;
          trace.push({
            line: 5,
            explanation: `a[${mid}]=${a[mid]} = target=${target} → Tìm thấy tại vị trí ${mid}.`,
            structure: arrayStructure(a, binaryStatuses(n, low, high, mid, statuses, foundIdx)),
            annotations: [`Tìm thấy target=${target} tại vị trí ${mid}`],
          });
          break;
        }
        trace.push({
          line: 5,
          explanation: `a[${mid}]=${a[mid]} = target=${target} → sai, thu hẹp phạm vi.`,
          structure: arrayStructure(a, binaryStatuses(n, low, high, mid, statuses, null, true)),
        });

        trace.stats.comparisons++;
        trace.push({
          line: 6,
          explanation: `So sánh a[${mid}]=${a[mid]} và target=${target}.`,
          structure: arrayStructure(a, binaryStatuses(n, low, high, mid, statuses, null, true)),
          annotations: [`a[${mid}]=${a[mid]} < target=${target}?`],
        });
        if (a[mid] < target) {
          low = mid + 1;
          trace.vars.low = low;
          trace.push({
            line: 6,
            explanation: `a[${mid}]=${a[mid]} < target=${target} → đúng, tìm nửa phải: low = ${low}.`,
            structure: arrayStructure(a, binaryStatuses(n, low, high, null, statuses, null)),
            annotations: [`low=${low}`],
          });
        } else {
          high = mid - 1;
          trace.vars.high = high;
          trace.push({
            line: 7,
            explanation: `a[${mid}]=${a[mid]} < target=${target} → sai, tìm nửa trái: high = ${high}.`,
            structure: arrayStructure(a, binaryStatuses(n, low, high, null, statuses, null)),
            annotations: [`high=${high}`],
          });
        }
      }

      if (foundIdx === null) {
        for (let k = 0; k < n; k++) statuses[k] = 'muted';
        trace.push({
          line: 8,
          explanation: `Kết thúc: không tìm thấy target=${target} trong mảng (return -1).`,
          structure: arrayStructure(a, statuses),
          annotations: ['Không tìm thấy'],
        });
      } else {
        trace.push({
          line: 9,
          explanation: `Kết thúc: tìm thấy target=${target} tại vị trí ${foundIdx}.`,
          structure: arrayStructure(a, binaryStatuses(n, low, high, null, statuses, foundIdx)),
          annotations: [`Tìm thấy target=${target} tại vị trí ${foundIdx}`],
        });
      }
      return trace.steps;
    },
  });
}
