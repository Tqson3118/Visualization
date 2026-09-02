// engines/generators/sort/heap.ts — Heap Sort (SDD §4.7.6)
import type { InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { buildGenerator, heapStructure, parseArrayParams, Trace, validateArrayParams } from '../helpers';

const PSEUDOCODE = [
  'procedure heapSort(a[0..n-1])',
  '  buildMaxHeap(a)            // heapify từ n/2-1 về 0',
  '  for i ← n-1 downto 1 do',
  '    swap a[0], a[i]',
  '    a[i] ← done',
  '    siftDown(a, 0, i-1)',
  'procedure siftDown(a, root, end)',
  '  while 2*root+1 ≤ end do',
  '    child ← max(a[2*root+1], a[2*root+2]) (nếu tồn tại)',
  '    if a[root] < a[child] then swap, root ← child',
  '    else break',
  '  end procedures',
];

const SCHEMA: InputSchema = {
  kind: 'heap',
  fields: [
    { name: 'values', type: 'int[]', label: 'Dãy số', min: -999, max: 999, default: [12, 8, 17, 5, 23, 19, 3, 15, 7, 2], description: 'Dãy số (phân cách dấu phẩy); để trống để dùng chế độ ngẫu nhiên' },
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

export function createHeapSortGenerator(): SimulationGenerator {
  return buildGenerator('sort.heap', SCHEMA, PSEUDOCODE, {
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
      trace.vars.root = null;
      trace.vars.child = null;
      trace.vars.n = n;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: mảng [${a.join(', ')}] được khởi tạo thành cây đống.`,
        structure: heapStructure(a, statuses),
        annotations: [`n=${n}`],
      });

      // buildMaxHeap: heapify từ n/2-1 về 0 (siftDown từng nút)
      trace.push({
        line: 2,
        explanation: `buildMaxHeap: vun đống từng nút từ ${Math.floor(n / 2) - 1} về 0.`,
        structure: heapStructure(a, statuses),
        annotations: [`bắt đầu heapify`],
      });
      const siftDown = (root: number, end: number): void => {
        let r = root;
        while (2 * r + 1 <= end) {
          trace.vars.root = r;
          trace.push({
            line: 8,
            explanation: `siftDown(a, ${r}, ${end}): nút ${r} còn con trong phạm vi.`,
            structure: heapStructure(a, { ...statuses, [r]: 'active' }),
            annotations: [`root=${r}, end=${end}`],
          });
          const l = 2 * r + 1;
          const rr = 2 * r + 2;
          let child = l;
          if (rr <= end) {
            trace.stats.comparisons++;
            trace.push({
              line: 9,
              explanation: `So sánh hai con: a[${l}]=${a[l]} và a[${rr}]=${a[rr]}.`,
              structure: heapStructure(a, { ...statuses, [l]: 'active', [rr]: 'active', [r]: 'highlight' }),
              annotations: [`a[${l}]=${a[l]} vs a[${rr}]=${a[rr]}`],
            });
            if (a[rr] > a[l]) child = rr;
            trace.push({
              line: 9,
              explanation: `Con lớn hơn là a[${child}]=${a[child]}.`,
              structure: heapStructure(a, { ...statuses, [child]: 'highlight', [r]: 'highlight' }),
              annotations: [`child=${child}`],
            });
          } else {
            trace.push({
              line: 9,
              explanation: `Chỉ có con trái a[${l}]=${a[l]} → child=${l}.`,
              structure: heapStructure(a, { ...statuses, [l]: 'active', [r]: 'highlight' }),
              annotations: [`child=${l}`],
            });
          }
          trace.vars.child = child;

          trace.stats.comparisons++;
          trace.push({
            line: 10,
            explanation: `So sánh cha a[${r}]=${a[r]} và con lớn nhất a[${child}]=${a[child]}.`,
            structure: heapStructure(a, { ...statuses, [r]: 'active', [child]: 'active' }),
            annotations: [`a[${r}]=${a[r]} < a[${child}]=${a[child]}?`],
          });
          if (a[r] < a[child]) {
            const oldR = a[r];
            const oldChild = a[child];
            a[r] = oldChild;
            a[child] = oldR;
            trace.stats.swaps++;
            trace.push({
              line: 10,
              explanation: `${oldR} < ${oldChild} → đúng, hoán đổi cha a[${r}] (${oldR}) và con a[${child}] (${oldChild}), root ← ${child}.`,
              structure: heapStructure(a, { ...statuses, [r]: 'swap', [child]: 'swap' }),
              annotations: ['hoán đổi cha-con'],
            });
            r = child;
            trace.vars.root = r;
          } else {
            trace.push({
              line: 11,
              explanation: `a[${r}]=${a[r]} ≥ a[${child}]=${a[child]} → đúng vị trí max-heap, dừng chìm.`,
              structure: heapStructure(a, { ...statuses, [r]: 'active', [child]: 'active' }),
            });
            break;
          }
        }
      };

      for (let i = Math.floor(n / 2) - 1; i >= 0; i--) {
        trace.vars.i = i;
        trace.push({
          line: 2,
          explanation: `Heapify nút i=${i} (a[${i}]=${a[i]}).`,
          structure: heapStructure(a, { ...statuses, [i]: 'active' }),
          annotations: [`heapify nút ${i}`],
        });
        siftDown(i, n - 1);
      }

      for (let i = n - 1; i >= 1; i--) {
        trace.vars.i = i;
        trace.push({
          line: 3,
          explanation: `i=${i}: đưa max a[0]=${a[0]} về cuối mảng tại vị trí a[${i}].`,
          structure: heapStructure(a, statuses),
          annotations: [`i=${i}`],
        });
        const old0 = a[0];
        const oldI = a[i];
        a[0] = oldI;
        a[i] = old0;
        trace.stats.swaps++;
        trace.push({
          line: 4,
          explanation: `Hoán đổi a[0] (${old0}) và a[${i}] (${oldI}).`,
          structure: heapStructure(a, { ...statuses, [0]: 'swap', [i]: 'swap' }),
          annotations: ['hoán đổi'],
        });
        statuses[i] = 'done';
        trace.push({
          line: 5,
          explanation: `a[${i}]=${a[i]} đã nằm đúng vị trí cố định cuối mảng.`,
          structure: heapStructure(a, statuses),
          annotations: [`a[${i}] đúng vị trí`],
        });
        trace.push({
          line: 6,
          explanation: `siftDown(a, 0, ${i - 1}) để khôi phục max-heap cho phần còn lại.`,
          structure: heapStructure(a, statuses),
        });
        siftDown(0, i - 1);
      }

      for (let k = 0; k < n; k++) statuses[k] = 'done';
      trace.push({
        line: 12,
        explanation: `Kết thúc: mảng [${a.join(', ')}] đã sắp xếp tăng dần hoàn chỉnh.`,
        structure: heapStructure(a, statuses),
      });
      return trace.steps;
    },
  });
}
