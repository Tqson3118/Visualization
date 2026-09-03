// engines/generators/sort/merge.ts — Merge Sort (SDD §4.7.4)
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { arrayStructure, buildGenerator, parseArrayParams, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure mergeSort(a, left, right)',
  '  if left ≥ right then return',
  '  mid ← (left + right) / 2',
  '  mergeSort(a, left, mid)',
  '  mergeSort(a, mid+1, right)',
  '  merge(a, left, mid, right)',
  'procedure merge(a, left, mid, right)',
  '  t ← mảng tạm, k ← left, i ← left, j ← mid+1',
  '  while i ≤ mid and j ≤ right do',
  '    if a[i] ≤ a[j] then t[k] ← a[i], i++',
  '    else t[k] ← a[j], j++',
  '    k++',
  '  sao chép phần còn lại',
  '  ghi t về a[left..right]',
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

export function createMergeGenerator(): SimulationGenerator {
  return buildGenerator('sort.merge', SCHEMA, PSEUDOCODE, {
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

      // Bản đồ trạng thái cho đoạn [lo..hi]:
      // - Nếu phần tử đã done toàn cục: giữ 'done'
      // - Phần tử nằm ngoài đoạn [lo..hi]: gán 'muted' (màu xám mờ để thể hiện chia để trị)
      // - Phần tử nằm trong đoạn [lo..hi]: gán 'active'
      const segMap = (lo: number, hi: number, extra: StatusMap = {}): StatusMap => {
        const m: StatusMap = {};
        for (let k = 0; k < n; k++) {
          if (doneMap[k] === 'done') {
            m[k] = 'done';
          } else if (k >= lo && k <= hi) {
            m[k] = 'active';
          } else {
            m[k] = 'muted';
          }
        }
        for (const [k, v] of Object.entries(extra)) {
          m[Number(k)] = v as any;
        }
        return m;
      };

      trace.vars.left = 0;
      trace.vars.right = n - 1;
      trace.vars.mid = null;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}] được khởi tạo.`,
        structure: arrayStructure(a, segMap(0, n - 1)),
        annotations: [`left=0, right=${n - 1}`],
      });

      const merge = (lo: number, mid: number, hi: number): void => {
        let i = lo;
        let j = mid + 1;
        let k = lo;
        const t: number[] = [];
        trace.vars.i = i;
        trace.vars.j = j;
        trace.vars.k = k;
        trace.push({
          line: 8,
          explanation: `Trộn: t ← mảng tạm, k=${lo}, i=${lo}, j=${mid + 1}.`,
          structure: arrayStructure(a, segMap(lo, hi)),
          annotations: [`k=${lo}, i=${lo}, j=${mid + 1}`],
        });

        while (i <= mid && j <= hi) {
          trace.vars.i = i;
          trace.vars.j = j;
          trace.push({
            line: 9,
            explanation: `Vòng lặp trộn: i=${i} ≤ mid=${mid} và j=${j} ≤ right=${hi}.`,
            structure: arrayStructure(a, segMap(lo, hi, { [i]: 'active', [j]: 'active' })),
            annotations: [`i=${i}, j=${j}`],
          });

          trace.stats.comparisons++;
          trace.push({
            line: 10,
            explanation: `So sánh a[${i}]=${a[i]} và a[${j}]=${a[j]}.`,
            structure: arrayStructure(a, segMap(lo, hi, { [i]: 'active', [j]: 'active' })),
            annotations: [`a[${i}]=${a[i]} ≤ a[${j}]=${a[j]}?`],
          });

          if (a[i] <= a[j]) {
            t[k - lo] = a[i];
            trace.stats.writes++;
            trace.push({
              line: 10,
              explanation: `a[${i}]=${a[i]} ≤ a[${j}]=${a[j]} → đúng, ghi t[${k - lo}] (vị trí ${k}) ← a[${i}]=${a[i]}, i++.`,
              structure: arrayStructure(a, segMap(lo, hi, { [i]: 'highlight' })),
              annotations: [`t[${k - lo}]=${a[i]}`],
            });
            i++;
          } else {
            t[k - lo] = a[j];
            trace.stats.writes++;
            trace.push({
              line: 11,
              explanation: `a[${i}]=${a[i]} ≤ a[${j}]=${a[j]} → sai, ghi t[${k - lo}] (vị trí ${k}) ← a[${j}]=${a[j]}, j++.`,
              structure: arrayStructure(a, segMap(lo, hi, { [j]: 'highlight' })),
              annotations: [`t[${k - lo}]=${a[j]}`],
            });
            j++;
          }
          k++;
          trace.vars.k = k;
          trace.push({
            line: 12,
            explanation: `k++ → ${k}.`,
            structure: arrayStructure(a, segMap(lo, hi)),
            annotations: [`k=${k}`],
          });
        }

        while (i <= mid) {
          t[k - lo] = a[i];
          trace.stats.writes++;
          trace.push({
            line: 13,
            explanation: `Sao chép phần còn lại nửa trái: t[${k - lo}] (vị trí ${k}) ← a[${i}]=${a[i]}, i++.`,
            structure: arrayStructure(a, segMap(lo, hi, { [i]: 'highlight' })),
            annotations: [`t[${k - lo}]=${a[i]}`],
          });
          i++;
          k++;
        }
        while (j <= hi) {
          t[k - lo] = a[j];
          trace.stats.writes++;
          trace.push({
            line: 13,
            explanation: `Sao chép phần còn lại nửa phải: t[${k - lo}] (vị trí ${k}) ← a[${j}]=${a[j]}, j++.`,
            structure: arrayStructure(a, segMap(lo, hi, { [j]: 'highlight' })),
            annotations: [`t[${k - lo}]=${a[j]}`],
          });
          j++;
          k++;
        }

        const isGlobalDone = lo === 0 && hi === n - 1;
        for (let p = lo; p <= hi; p++) {
          a[p] = t[p - lo];
          if (isGlobalDone) {
            doneMap[p] = 'done';
          }
          trace.stats.writes++;
          trace.push({
            line: 14,
            explanation: isGlobalDone
              ? `Ghi t về mảng: a[${p}] ← t[${p - lo}] = ${a[p]} (hoàn tất sắp xếp toàn bộ mảng!).`
              : `Ghi t về mảng: a[${p}] ← t[${p - lo}] = ${a[p]} (đoạn con [${lo}..${hi}] đã hợp nhất tạm thời).`,
            structure: arrayStructure(a, segMap(lo, hi, isGlobalDone ? { [p]: 'done' } : { [p]: 'highlight' })),
            annotations: [`a[${p}]=${a[p]}`, `t=[${t.join(', ')}]`],
          });
        }
        trace.push({
          line: 14,
          explanation: isGlobalDone
            ? `Hoàn tất thuật toán Merge Sort: mảng đã được sắp xếp tăng dần hoàn chỉnh [${a.join(', ')}].`
            : `Hoàn tất trộn đoạn con a[${lo}..${hi}] = [${a.slice(lo, hi + 1).join(', ')}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
          annotations: [isGlobalDone ? 'Toàn bộ mảng đã sắp xếp xong' : `Đoạn con [${lo}..${hi}] đã có thứ tự`],
        });
      };

      const mergeSort = (lo: number, hi: number): void => {
        trace.push({
          line: 1,
          explanation: `mergeSort(a, ${lo}, ${hi}) — sắp xếp đoạn a[${lo}..${hi}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
          annotations: [`đoạn [${lo}..${hi}]`],
        });
        if (lo >= hi) {
          trace.push({
            line: 2,
            explanation: `left=${lo} ≥ right=${hi} → đoạn có ≤ 1 phần tử, quay về.`,
            structure: arrayStructure(a, segMap(lo, hi)),
          });
          return;
        }
        const mid = Math.floor((lo + hi) / 2);
        trace.vars.mid = mid;
        trace.push({
          line: 3,
          explanation: `mid = (${lo} + ${hi}) / 2 = ${mid}.`,
          structure: arrayStructure(a, segMap(lo, hi)),
          annotations: [`mid=${mid}`],
        });
        trace.push({
          line: 4,
          explanation: `Đệ quy sắp xếp nửa trái a[${lo}..${mid}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
        });
        mergeSort(lo, mid);
        trace.push({
          line: 5,
          explanation: `Đệ quy sắp xếp nửa phải a[${mid + 1}..${hi}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
        });
        mergeSort(mid + 1, hi);
        trace.push({
          line: 6,
          explanation: `Trộn hai nửa đã sắp xếp a[${lo}..${mid}] và a[${mid + 1}..${hi}].`,
          structure: arrayStructure(a, segMap(lo, hi)),
          annotations: [`merge(a, ${lo}, ${mid}, ${hi})`],
        });
        merge(lo, mid, hi);
      };

      mergeSort(0, n - 1);

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
