// engines/generators/structure/structures.ts — 10 CTDL tĩnh (structure.*) theo SDD §4.2
// Mỗi structure key: ≥ 5 bước giới thiệu cấu trúc (khởi tạo mẫu, highlight từng phần,
// thao tác minh họa O(1) insert/delete/search theo loại, tổng kết độ phức tạp, kết thúc)
// kèm pseudocode mô tả thao tác chính.
import type { Element, ElementStatus, InputConfig, InputSchema, Link, SimulationGenerator, Structure } from '../../core/types';
import { buildGenerator, hashIndex, hashStructure, heapStructure, intArrayField, intField, Trace } from '../helpers';

// ── Các schema nhỏ cho từng structure ────────────────────────────────────────

function intArraySchema(label: string, dflt: number[], desc: string, kind: string): InputSchema {
  return {
    kind,
    fields: [
      { name: 'values', type: 'int[]', label, min: -999, max: 999, default: dflt, description: desc },
    ],
  };
}

const VALUES_SCHEMA = (kind: string, label: string, dflt: number[]): InputSchema =>
  intArraySchema(label, dflt, 'Dãy giá trị khởi tạo cấu trúc mẫu', kind);

const KEY_SCHEMA = (kind: string, label: string, dflt: number[]): InputSchema =>
  intArraySchema(label, dflt, 'Các khóa dựng cấu trúc mẫu', kind);

const GRAPH_SCHEMA: InputSchema = {
  kind: 'graph',
  fields: [
    { name: 'preset', type: 'select', label: 'Mẫu đồ thị', options: [
      { label: 'Đường đi (path)', value: 'path' },
      { label: 'Chu trình (cycle)', value: 'cycle' },
      { label: 'Lưới (grid)', value: 'grid' },
      { label: 'Tự sinh (custom)', value: 'custom' },
    ], default: 'path', description: 'Mẫu đồ thị' },
    { name: 'vertices', type: 'int', label: 'Số đỉnh', min: 2, max: 50, default: 6, description: 'Số đỉnh của đồ thị' },
    { name: 'directed', type: 'bool', label: 'Có hướng', default: true, description: 'Đồ thị có hướng hay vô hướng' },
    { name: 'weighted', type: 'bool', label: 'Có trọng số', default: true, description: 'Các cạnh có trọng số hay không' },
  ],
};

const HASH_SCHEMA: InputSchema = {
  kind: 'hashtable',
  fields: [
    { name: 'keys', type: 'int[]', label: 'Dãy khóa', min: -999, max: 999, default: [12, 25, 37, 41, 58], description: 'Các khóa chèn vào bảng băm' },
    { name: 'tableSize', type: 'int', label: 'Kích thước bảng', min: 5, max: 31, default: 11, description: 'Số bucket' },
  ],
};

// ── Các mô hình cấu trúc (dùng chung) ───────────────────────────────────────

interface SNode {
  key: number;
  left: SNode | null;
  right: SNode | null;
}

function buildBst(keys: number[]): SNode | null {
  let root: SNode | null = null;
  for (const k of keys) {
    const insert = (node: SNode | null): SNode => {
      if (node === null) return { key: k, left: null, right: null };
      if (k < node.key) node.left = insert(node.left);
      else if (k > node.key) node.right = insert(node.right);
      return node;
    };
    root = insert(root);
  }
  return root;
}

function treeOf(root: SNode | null): Structure {
  const elements: Element[] = [];
  const links: Link[] = [];
  const queue: Array<SNode | null> = [root];
  while (queue.length > 0) {
    const node = queue.shift() ?? null;
    if (node === null) continue;
    const id = `node:${node.key}`;
    elements.push({ id, label: String(node.key), status: 'default', group: 'tree' });
    if (node.left) links.push({ from: id, to: `node:${node.left.key}`, label: 'L' });
    if (node.right) links.push({ from: id, to: `node:${node.right.key}`, label: 'R' });
    queue.push(node.left);
    queue.push(node.right);
  }
  return { kind: 'tree', elements, links };
}

/** Đánh dấu toàn bộ phần tử theo danh sách id. */
function markAll(structure: Structure, ids: string[], status: ElementStatus): Structure {
  return {
    kind: structure.kind,
    elements: structure.elements.map((el) => ({ ...el, status: ids.includes(el.id) ? status : el.status })),
    links: structure.links,
  };
}

function setStatuses(structure: Structure, statusMap: Record<string, ElementStatus>): Structure {
  return {
    kind: structure.kind,
    elements: structure.elements.map((el) => ({ ...el, status: statusMap[el.id] ?? el.status })),
    links: structure.links,
  };
}

function arrayStruct(values: number[], statuses: Record<number, ElementStatus> = {}): Structure {
  return {
    kind: 'array',
    elements: values.map((v, i) => ({ id: `cell:${i}`, label: String(v), status: statuses[i] ?? 'default' })),
    links: [],
  };
}

function listStruct(values: number[], statuses: Record<string, ElementStatus> = {}): Structure {
  const elements: Element[] = values.map((v, i) => ({
    id: `node:${i}`, label: String(v), status: statuses[`node:${i}`] ?? 'default', group: 'linkedlist', meta: { index: i },
  }));
  const links: Link[] = [];
  for (let i = 0; i + 1 < values.length; i++) links.push({ from: `node:${i}`, to: `node:${i + 1}`, label: 'next' });
  return { kind: 'linkedlist', elements, links };
}

function stackStruct(values: number[], capacity: number, statuses: Record<number, string> = {}): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < capacity; i++) {
    const filled = i < values.length;
    elements.push({
      id: `cell:${i}`,
      label: filled ? String(values[i]) : '—',
      status: (statuses[i] as ElementStatus) ?? (filled ? 'default' : 'muted'),
      group: 'stack',
      meta: { empty: !filled },
    });
  }
  return { kind: 'stack', elements, links: [] };
}

function queueStruct(values: number[], capacity: number, statuses: Record<number, string> = {}): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < capacity; i++) {
    const filled = i < values.length;
    elements.push({
      id: `cell:${i}`,
      label: filled ? String(values[i]) : '—',
      status: (statuses[i] as ElementStatus) ?? (filled ? 'default' : 'muted'),
      group: 'queue',
      meta: { empty: !filled },
    });
  }
  return { kind: 'queue', elements, links: [] };
}

// ── Các generator structure.* ───────────────────────────────────────────────

export function createStructureArrayGenerator(): SimulationGenerator {
  const pseudo = [
    'Mảng (Array): vùng nhớ liên tục, truy cập trực tiếp qua chỉ số',
    'a[i] ← x: gán O(1)',
    'chèn vào giữa: dịch n phần tử O(n)',
    'xóa tại vị trí: dịch n phần tử O(n)',
    'tìm kiếm tuyến tính: O(n)',
  ];
  return buildGenerator('structure.array', VALUES_SCHEMA('array', 'Dãy số', [10, 20, 30, 40, 50]), pseudo, {
    validate: (input) => wrapValidate(input, 2, 100),
    generate(input) {
      const values = readValues(input, [10, 20, 30, 40, 50]);
      const trace = new Trace();
      const n = values.length;
      trace.push({ line: 1, explanation: `Bắt đầu: mảng [${values.join(', ')}] — vùng nhớ liên tục, mỗi ô 1 chỉ số.`, structure: arrayStruct(values), annotations: [`n=${n}`] });

      trace.vars.target = null;
      trace.push({ line: 1, explanation: 'Truy cập trực tiếp: a[0] và a[2] đọc được ngay bằng chỉ số (không cần duyệt).', structure: setStatuses(arrayStruct(values), { 'cell:0': 'highlight', 'cell:2': 'highlight' }), annotations: ['a[0]=10, a[2]=30 — truy cập O(1)'] });

      trace.stats.writes++;
      trace.push({ line: 2, explanation: `Gán a[2] ← 99: ghi đè trực tiếp tại chỉ số 2 trong O(1).`, structure: setStatuses(arrayStruct([10, 20, 99, 40, 50]), { 'cell:2': 'swap' }), annotations: ['a[2]=99 — gán O(1)'] });

      trace.stats.writes += n - 2;
      trace.push({ line: 3, explanation: `Chèn 15 vào đầu: dịch ${n} phần tử sang phải → O(n)`, structure: setStatuses(arrayStruct([15, 10, 20, 99, 40, 50]), { 'cell:0': 'swap', 'cell:1': 'swap', 'cell:2': 'swap' }), annotations: ['chèn đầu: dịch chuyển toàn bộ'] });

      trace.stats.writes += n - 1;
      trace.push({ line: 4, explanation: `Xóa phần tử đầu: dịch ${n - 1} phần tử sang trái → O(n).`, structure: setStatuses(arrayStruct([10, 20, 99, 40, 50]), { 'cell:0': 'swap', 'cell:1': 'swap', 'cell:2': 'swap' }), annotations: ['xóa đầu: dịch trái'] });

      trace.stats.comparisons += n;
      trace.push({ line: 5, explanation: `Tìm kiếm giá trị 40: duyệt lần lượt ${n} phần tử → O(n) (tìm thấy tại vị trí 3).`, structure: setStatuses(arrayStruct([10, 20, 99, 40, 50]), { 'cell:3': 'done' }), annotations: ['tìm tuyến tính O(n)'] });

      trace.push({ line: 5, explanation: 'Kết thúc: mảng — truy cập O(1), chèn/xóa giữa O(n), tìm kiếm O(n), bộ nhớ O(n).', structure: setStatuses(arrayStruct([10, 20, 99, 40, 50]), { 'cell:0': 'done', 'cell:1': 'done', 'cell:2': 'done', 'cell:3': 'done', 'cell:4': 'done' }), annotations: ['Độ phức tạp: access O(1), insert/delete O(n), search O(n)'] });
      return trace.steps;
    },
  });
}

export function createStructureLinkedListGenerator(): SimulationGenerator {
  const pseudo = [
    'Danh sách liên kết đơn: các nút nối nhau bằng con trỏ next',
    'chèn đầu: newNode.next ← head; head ← newNode → O(1)',
    'chèn cuối: duyệt tới nút cuối → O(n)',
    'xóa nút: cập nhật con trỏ nút trước → O(1) sau khi tìm',
    'tìm kiếm: duyệt từ head → O(n)',
  ];
  return buildGenerator('structure.linkedlist', VALUES_SCHEMA('linear', 'Giá trị ban đầu', [10, 20, 30]), pseudo, {
    validate: (input) => wrapValidate(input, 0, 20),
    generate(input) {
      const values = readValues(input, [10, 20, 30]);
      const trace = new Trace();
      const n = values.length;
      trace.push({ line: 1, explanation: `Bắt đầu: danh sách liên kết đơn [${values.join(' → ')} → null].`, structure: listStruct(values), annotations: [`head=${values.length > 0 ? 0 : 'null'}`] });

      trace.push({ line: 1, explanation: 'Mỗi nút gồm 2 phần: giá trị (data) và con trỏ next trỏ tới nút kế tiếp.', structure: setStatuses(listStruct(values), { 'node:0': 'highlight' }), annotations: ['nút = data + next'] });

      trace.push({ line: 2, explanation: `Chèn 5 vào đầu: tạo nút mới, newNode.next ← head, head ← newNode — chỉ thao tác con trỏ → O(1).`, structure: listStruct([5, ...values], { 'node:0': 'swap', 'node:1': 'default', 'node:2': 'default', 'node:3': 'default' }), annotations: ['insertHead O(1)'] });

      trace.push({ line: 3, explanation: 'Chèn 40 vào cuối: duyệt từ head tới nút cuối rồi nối next → O(n).', structure: listStruct([5, ...values, 40], { 'node:4': 'swap' }), annotations: [`insertTail O(${values.length + 1})`] });

      trace.push({ line: 4, explanation: 'Xóa nút đầu: head ← head.next, không cần dịch chuyển phần tử → O(1).', structure: listStruct([...values, 40], { 'node:0': 'done' }), annotations: ['deleteHead O(1)'] });

      trace.stats.comparisons += n;
      trace.push({ line: 5, explanation: `Tìm kiếm giá trị 20: duyệt từng nút theo next từ head → O(n) (tìm thấy tại nút 1).`, structure: setStatuses(listStruct([...values, 40]), { 'node:0': 'muted', 'node:1': 'done' }), annotations: ['search O(n)'] });

      trace.push({ line: 5, explanation: 'Kết thúc: danh sách liên kết — chèn/xóa đầu O(1), chèn/xóa cuối O(n), tìm kiếm O(n), bộ nhớ O(n).', structure: setStatuses(listStruct([...values, 40]), { 'node:0': 'done', 'node:1': 'done', 'node:2': 'done', 'node:3': 'done' }), annotations: ['không cần vùng nhớ liên tục'] });
      return trace.steps;
    },
  });
}

export function createStructureStackGenerator(): SimulationGenerator {
  const pseudo = [
    'Ngăn xếp (Stack): LIFO — vào sau ra trước',
    'push(x): s[top+1] ← x → O(1)',
    'pop(): x ← s[top]; top-- → O(1)',
    'peek(): xem s[top] → O(1)',
    'ứng dụng: undo, gọi hàm đệ quy, duyệt DFS',
  ];
  return buildGenerator('structure.stack', VALUES_SCHEMA('linear', 'Giá trị ban đầu', [5, 8, 13]), pseudo, {
    validate: (input) => wrapValidate(input, 0, 20),
    generate(input) {
      const values = readValues(input, [5, 8, 13]);
      const trace = new Trace();
      const capacity = 8;
      trace.push({ line: 1, explanation: `Bắt đầu: ngăn xếp [${values.join(', ')}] (top=${values.length - 1}), dung lượng ${capacity}.`, structure: stackStruct(values, capacity), annotations: ['LIFO: vào sau ra trước'] });

      trace.push({ line: 1, explanation: 'Push luôn thêm vào ĐỈNH ngăn xếp — thao tác ở 1 đầu duy nhất.', structure: stackStruct(values, capacity, { [values.length - 1]: 'highlight' }), annotations: [`top=${values.length - 1}`] });

      const pushed = [...values, 21];
      trace.push({ line: 2, explanation: `push(21): ghi s[top+1] = s[${values.length}] ← 21 → O(1).`, structure: stackStruct(pushed, capacity, { [values.length]: 'swap' }), annotations: [`top: ${values.length - 1} → ${values.length}`] });

      const popped = values.slice(0, -1);
      trace.push({ line: 3, explanation: `pop(): lấy s[top] = ${values[values.length - 1]} ra, top-- → O(1).`, structure: stackStruct(popped, capacity, { [values.length - 2]: 'swap', [values.length - 1]: 'muted' }), annotations: [`x=${values[values.length - 1]}`] });

      trace.push({ line: 4, explanation: `peek(): xem s[top] = ${values[values.length - 2]} mà không xóa → O(1).`, structure: stackStruct(popped, capacity, { [values.length - 2]: 'highlight' }), annotations: ['peek không thay đổi stack'] });

      trace.push({ line: 5, explanation: `Kết thúc: ngăn xếp — push/pop/peek đều O(1), bộ nhớ O(n).`, structure: stackStruct(popped, capacity, { [values.length - 2]: 'done' }), annotations: ['toàn bộ thao tác O(1)'] });
      return trace.steps;
    },
  });
}

export function createStructureQueueGenerator(): SimulationGenerator {
  const pseudo = [
    'Hàng đợi (Queue): FIFO — vào trước ra trước',
    'enqueue(x): q[rear+1] ← x → O(1)',
    'dequeue(): x ← q[front]; front++ → O(1)',
    'ứng dụng: hàng đợi in, lập lịch CPU, duyệt BFS',
  ];
  return buildGenerator('structure.queue', VALUES_SCHEMA('linear', 'Giá trị ban đầu', [5, 8, 13]), pseudo, {
    validate: (input) => wrapValidate(input, 0, 20),
    generate(input) {
      const values = readValues(input, [5, 8, 13]);
      const trace = new Trace();
      const capacity = 8;
      trace.push({ line: 1, explanation: `Bắt đầu: hàng đợi [${values.join(', ')}] (front=0, rear=${values.length - 1}).`, structure: queueStruct(values, capacity), annotations: ['FIFO: vào trước ra trước'] });

      trace.push({ line: 1, explanation: 'Enqueue thêm vào CUỐI, dequeue lấy từ ĐẦU — hai đầu khác nhau.', structure: queueStruct(values, capacity, { [0]: 'highlight', [values.length - 1]: 'highlight' }), annotations: ['front=0, rear=2'] });

      const enqueued = [...values, 21];
      trace.push({ line: 2, explanation: `enqueue(21): ghi q[rear+1] = q[${values.length}] ← 21 → O(1).`, structure: queueStruct(enqueued, capacity, { [values.length]: 'swap' }), annotations: [`rear: ${values.length - 1} → ${values.length}`] });

      trace.push({ line: 3, explanation: `dequeue(): lấy q[front] = ${enqueued[0]} ra, front++ → O(1).`, structure: queueStruct(enqueued.slice(1), capacity, { [0]: 'muted', [1]: 'swap' }), annotations: [`x=${enqueued[0]}`] });

      trace.push({ line: 3, explanation: 'Kết thúc: hàng đợi — enqueue/dequeue đều O(1), bộ nhớ O(n).', structure: queueStruct(enqueued.slice(1), capacity, { [0]: 'done' }), annotations: ['toàn bộ thao tác O(1)'] });
      return trace.steps;
    },
  });
}

export function createStructureBinaryTreeGenerator(): SimulationGenerator {
  const pseudo = [
    'Cây nhị phân: mỗi nút có tối đa 2 con (trái, phải)',
    'duyệt theo cấp (level-order): hàng đợi → O(n)',
    'duyệt trước/sau/giữa (pre/in/post-order): đệ quy → O(n)',
    'tìm kiếm tổng quát: O(n) (không có thứ tự)',
    'chiều cao trung bình O(log n), tệ nhất O(n)',
  ];
  return buildGenerator('structure.binarytree', KEY_SCHEMA('tree', 'Dãy khóa', [50, 30, 70, 20, 40, 60, 80]), pseudo, {
    validate: (input) => wrapValidate(input, 1, 31),
    generate(input) {
      const keys = readValues(input, [50, 30, 70, 20, 40, 60, 80]);
      const root = buildBst(keys);
      const trace = new Trace();
      trace.push({ line: 1, explanation: `Bắt đầu: cây nhị phân ${keys.length} nút — mỗi nút có tối đa 2 con.`, structure: treeOf(root), annotations: [`n=${keys.length}`] });

      trace.push({ line: 1, explanation: 'Nút gốc 50 có con trái 30 và con phải 70; nút lá không có con.', structure: markAll(treeOf(root), ['node:50', 'node:30', 'node:70'], 'highlight'), annotations: ['gốc + 2 con'] });

      trace.push({ line: 2, explanation: `Duyệt level-order: [${keys.join(', ')}] — ghé thăm theo từng tầng, dùng hàng đợi → O(n).`, structure: markAll(treeOf(root), ['node:50', 'node:30', 'node:70'], 'done'), annotations: ['level-order O(n)'] });

      trace.stats.comparisons += keys.length;
      trace.push({ line: 4, explanation: `Tìm kiếm giá trị 40: cây nhị phân không có thứ tự sắp xếp → phải duyệt nhánh có thể chứa, trung bình O(n).`, structure: markAll(treeOf(root), ['node:40'], 'done'), annotations: ['search O(n)'] });

      trace.push({ line: 5, explanation: 'Kết thúc: cây nhị phân — duyệt O(n), tìm kiếm O(n), chiều cao trung bình O(log n) nếu cân bằng.', structure: markAll(treeOf(root), [], 'done'), annotations: ['chiều cao quyết định chi phí'] });
      return trace.steps;
    },
  });
}

export function createStructureBstGenerator(): SimulationGenerator {
  const pseudo = [
    'BST: cây nhị phân tìm kiếm — trái < nút < phải',
    'search(x): so sánh từ gốc, rẽ trái/phải → O(log n)',
    'insert(x): chèn vào lá theo quy tắc → O(log n)',
    'delete(x): 0/1/2 con (2 con: thay bằng min cây phải) → O(log n)',
    'duyệt inorder cho dãy tăng dần → O(n)',
  ];
  return buildGenerator('structure.bst', KEY_SCHEMA('tree', 'Dãy khóa', [50, 30, 70, 20, 40, 60, 80]), pseudo, {
    validate: (input) => wrapValidate(input, 1, 31),
    generate(input) {
      const keys = readValues(input, [50, 30, 70, 20, 40, 60, 80]);
      const root = buildBst(keys);
      const trace = new Trace();
      trace.push({ line: 1, explanation: `Bắt đầu: BST từ [${keys.join(', ')}] — mọi nút trái < nút < mọi nút phải.`, structure: treeOf(root), annotations: [`n=${keys.length}`] });

      trace.push({ line: 1, explanation: 'Quy tắc khóa: 20 và 40 nằm phía trái 30; 60 và 80 nằm phía phải 70.', structure: markAll(treeOf(root), ['node:30', 'node:70'], 'highlight'), annotations: ['trái < gốc < phải'] });

      trace.stats.comparisons += 3;
      trace.push({ line: 2, explanation: `Tìm kiếm 60: 60 > 50 → rẽ phải; 60 < 70 → rẽ trái; gặp 60 → tìm thấy sau 3 phép so sánh → O(log n).`, structure: markAll(treeOf(root), ['node:50', 'node:70', 'node:60'], 'done'), annotations: ['search O(log n)'] });

      trace.stats.comparisons += 3;
      trace.push({ line: 3, explanation: 'Chèn 25: 25 < 50 → trái; 25 < 30 → trái; 25 > 20 → phải → chèn làm con phải của 20.', structure: treeOf({ key: 50, left: { key: 30, left: { key: 20, left: null, right: { key: 25, left: null, right: null } }, right: { key: 40, left: null, right: null } }, right: { key: 70, left: { key: 60, left: null, right: null }, right: { key: 80, left: null, right: null } } }), annotations: ['insert O(log n)'] });

      trace.push({ line: 4, explanation: 'Xóa 30 (2 con): thay bằng min cây con phải là 40, rồi xóa nút 40 → O(log n).', structure: markAll(treeOf(root), ['node:30', 'node:40'], 'swap'), annotations: ['delete 2 con: min cây phải'] });

      trace.push({ line: 5, explanation: `Kết thúc: duyệt inorder ra dãy tăng dần [${[...keys].sort((x, y) => x - y).join(', ')}] — O(n).`, structure: markAll(treeOf(root), [], 'done'), annotations: ['inorder = dãy tăng dần'] });
      return trace.steps;
    },
  });
}

export function createStructureAvlGenerator(): SimulationGenerator {
  const pseudo = [
    'AVL: BST tự cân bằng — |bf| = |hL - hR| ≤ 1 tại mọi nút',
    'sau insert/delete: cập nhật chiều cao, tính bf',
    '|bf| > 1 → xoay LL/RR/LR/RL để cân bằng lại',
    'chiều cao luôn O(log n) → mọi thao tác O(log n)',
  ];
  return buildGenerator('structure.avl', KEY_SCHEMA('tree', 'Dãy khóa', [50, 30, 70, 20, 40, 60, 80]), pseudo, {
    validate: (input) => wrapValidate(input, 1, 31),
    generate(input) {
      const keys = readValues(input, [50, 30, 70, 20, 40, 60, 80]);
      const root = buildBst(keys);
      const bfStruct = (): Structure => {
        const s = treeOf(root);
        const heights: Record<string, number> = {};
        const calc = (node: SNode | null): number => {
          if (node === null) return 0;
          const h = 1 + Math.max(calc(node.left), calc(node.right));
          heights[`node:${node.key}`] = h;
          return h;
        };
        calc(root);
        const leftH = (node: SNode | null): number => (node?.left ? (heights[`node:${node.left.key}`] ?? 0) : 0);
        const rightH = (node: SNode | null): number => (node?.right ? (heights[`node:${node.right.key}`] ?? 0) : 0);
        return {
          kind: 'tree',
          elements: s.elements.map((el) => {
            const id = el.id;
            const h = heights[id] ?? 0;
            // Tìm bf qua khóa của nút (id = node:{key}).
            const key = Number(id.slice('node:'.length));
            let bf = 0;
            const findBf = (n: SNode | null): void => {
              if (n === null) return;
              if (n.key === key) bf = leftH(n) - rightH(n);
              findBf(n.left);
              findBf(n.right);
            };
            findBf(root);
            return { ...el, meta: { bf, height: h } };
          }),
          links: s.links,
        };
      };
      const trace = new Trace();
      trace.push({ line: 1, explanation: `Bắt đầu: cây AVL từ [${keys.join(', ')}] — mọi nút có |balance factor| ≤ 1.`, structure: bfStruct(), annotations: ['AVL cân bằng'] });

      trace.push({ line: 1, explanation: 'balance factor bf(nút) = h(trái) − h(phải); nút lá có bf=0.', structure: bfStruct(), annotations: ['bf = hL − hR'] });

      trace.push({ line: 3, explanation: 'Chèn 25 → cây lệch trái tại nút 30 (bf=2) → mất cân bằng.', structure: markAll(bfStruct(), ['node:30'], 'error'), annotations: ['bf=2 → mất cân bằng'] });

      trace.push({ line: 3, explanation: 'Xoay LL: xoay phải quanh nút 30 → cây cân bằng lại, chiều cao giảm từ 4 xuống 3.', structure: markAll(bfStruct(), ['node:30', 'node:20'], 'swap'), annotations: ['xoay LL'] });

      trace.push({ line: 4, explanation: `Kết thúc: AVL giữ chiều cao O(log n) → search/insert/delete luôn O(log n).`, structure: bfStruct(), annotations: ['mọi thao tác O(log n)'] });
      return trace.steps;
    },
  });
}

export function createStructureHeapGenerator(): SimulationGenerator {
  const pseudo = [
    'Đống nhị phân (max-heap): mảng + quan hệ cha-con',
    'a[0] là phần tử lớn nhất; cha ≥ con',
    'insert: thêm cuối mảng, bubble up → O(log n)',
    'extractMax: lấy a[0], đưa phần tử cuối lên, sift down → O(log n)',
    'heapify: xây đống từ mảng trong O(n)',
  ];
  return buildGenerator('structure.heap', KEY_SCHEMA('heap', 'Dãy khóa', [10, 7, 9, 4, 6, 8]), pseudo, {
    validate: (input) => wrapValidate(input, 1, 31),
    generate(input) {
      const keys = readValues(input, [10, 7, 9, 4, 6, 8]);
      const trace = new Trace();
      trace.push({ line: 1, explanation: `Bắt đầu: max-heap [${keys.join(', ')}] — a[0] = ${keys[0]} là phần tử lớn nhất.`, structure: heapStructure(keys), annotations: [`max=${keys[0]}`] });

      trace.push({ line: 2, explanation: 'Con của a[i] là a[2i+1] (trái) và a[2i+2] (phải); cha luôn ≥ con.', structure: heapStructure(keys, { [0]: 'highlight' }), annotations: ['cha ≥ con'] });

      trace.push({ line: 3, explanation: `Chèn 15: thêm vào cuối mảng rồi bubble up (15 > 8 → đổi chỗ, 15 > 10 → đổi chỗ lên gốc).`, structure: heapStructure([15, 10, 9, 4, 6, 8, 7], { [0]: 'swap', [1]: 'swap', [6]: 'swap' }), annotations: ['insert + bubble up O(log n)'] });

      trace.push({ line: 4, explanation: `extractMax: lấy max=${keys[0]}, đưa phần tử cuối lên gốc rồi sift down → O(log n).`, structure: heapStructure([9, 7, 8, 4, 6], { [0]: 'swap' }), annotations: ['extractMax O(log n)'] });

      trace.push({ line: 5, explanation: 'Kết thúc: đống nhị phân — max O(1), insert/extract O(log n), heapify O(n), bộ nhớ O(n).', structure: heapStructure([9, 7, 8, 4, 6], { [0]: 'done', [1]: 'done', [2]: 'done', [3]: 'done', [4]: 'done' }), annotations: ['nền tảng của Heap Sort & Priority Queue'] });
      return trace.steps;
    },
  });
}

export function createStructureHashTableGenerator(): SimulationGenerator {
  const pseudo = [
    'Bảng băm: mảng bucket + hàm băm h(k)',
    'insert: idx = h(k); thêm vào bucket → O(1) trung bình',
    'xung đột: chuỗi nối kết trong bucket → O(n) tệ nhất',
    'search: tính h(k), duyệt bucket → O(1) trung bình',
    'delete: tính h(k), gỡ nút khỏi chuỗi → O(1) trung bình',
  ];
  return buildGenerator('structure.hashtable', HASH_SCHEMA, pseudo, {
    validate: (input) => {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const errors = validateValues(input, 2, 50);
      const size = intField(rec, 'tableSize', 11);
      if (size < 5 || size > 31) errors.push(`tableSize: phải trong khoảng 5–31 (hiện tại ${size})`);
      return { ok: errors.length === 0, errors };
    },
    generate(input) {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const keys = readValues(input, [12, 25, 37, 41, 58]);
      const tableSize = intField(rec, 'tableSize', 11);
      const buckets: number[][] = Array.from({ length: tableSize }, () => []);
      const statuses: Record<string, ElementStatus> = {};
      const bucketStatuses: Record<number, ElementStatus> = {};
      for (const key of keys) buckets[hashIndex(key, tableSize, 'modulo')].push(key);
      const trace = new Trace();

      trace.push({ line: 1, explanation: `Bắt đầu: bảng băm ${tableSize} bucket, chèn [${keys.join(', ')}] bằng hàm băm modulo.`, structure: hashStructure(tableSize, buckets, statuses, bucketStatuses), annotations: ['chuỗi nối kết'] });

      const idx = hashIndex(keys[0], tableSize, 'modulo');
      trace.push({ line: 2, explanation: `h(${keys[0]}) = ${keys[0]} mod ${tableSize} = ${idx} → chèn vào bucket ${idx}.`, structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }), annotations: [`h(${keys[0]}) = ${idx}`] });

      trace.push({ line: 3, explanation: 'Xung đột: hai khóa khác nhau cùng 1 bucket → nối tiếp vào chuỗi nối kết (danh sách liên kết).', structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'highlight' }), annotations: ['collision → chaining'] });

      trace.stats.comparisons += 2;
      trace.push({ line: 4, explanation: `search(${keys[2]}): tính h(${keys[2]}) = ${hashIndex(keys[2], tableSize, 'modulo')}, duyệt bucket → tìm thấy → O(1) trung bình.`, structure: hashStructure(tableSize, buckets, statuses, { [hashIndex(keys[2], tableSize, 'modulo')]: 'done' }), annotations: ['search O(1) trung bình'] });

      trace.push({ line: 5, explanation: 'Kết thúc: bảng băm — insert/search/delete O(1) trung bình, O(n) tệ nhất khi nhiều xung đột, bộ nhớ O(n).', structure: hashStructure(tableSize, buckets, statuses, bucketStatuses), annotations: ['hệ số tải càng cao, xung đột càng nhiều'] });
      return trace.steps;
    },
  });
}

export function createStructureGraphGenerator(): SimulationGenerator {
  const pseudo = [
    'Đồ thị G = (V, E): tập đỉnh và tập cạnh',
    'có hướng / vô hướng; có trọng số / không trọng số',
    'biểu diễn: ma trận kề O(V²) hoặc danh sách kề O(V+E)',
    'duyệt: BFS (hàng đợi) / DFS (ngăn xếp) → O(V+E)',
    'đường đi ngắn nhất: Dijkstra O((V+E) log V)',
  ];
  return buildGenerator('structure.graph', GRAPH_SCHEMA, pseudo, {
    validate: (input) => {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const errors: string[] = [];
      const vertices = intField(rec, 'vertices', 6);
      if (vertices < 2 || vertices > 50) errors.push(`vertices: phải trong khoảng 2–50 (hiện tại ${vertices})`);
      return { ok: errors.length === 0, errors };
    },
    generate(input) {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const vertices = intField(rec, 'vertices', 6);
      const directed = typeof rec.directed === 'boolean' ? rec.directed : true;
      const weighted = typeof rec.weighted === 'boolean' ? rec.weighted : true;
      const nodes: Element[] = Array.from({ length: vertices }, (_, i) => ({
        id: `node:${i}`, label: String(i), status: 'default' as ElementStatus, meta: { d: null },
      }));
      const links: Link[] = [];
      for (let i = 0; i + 1 < vertices; i++) links.push({ from: `node:${i}`, to: `node:${i + 1}`, label: weighted ? `w=${((i * 7) % 9) + 1}` : undefined });
      const graph: Structure = { kind: 'graph', elements: nodes, links };

      const trace = new Trace();
      trace.push({ line: 1, explanation: `Bắt đầu: đồ thị ${vertices} đỉnh, cạnh ${vertices - 1}, ${directed ? 'có hướng' : 'vô hướng'}${weighted ? ', có trọng số' : ''}.`, structure: graph, annotations: [`V=${vertices}, E=${vertices - 1}`] });

      trace.push({ line: 2, explanation: 'Cạnh nối 2 đỉnh; có hướng thì cạnh (u → v) chỉ đi 1 chiều, có trọng số thì mang nhãn w.', structure: setStatuses(graph, { 'node:0': 'highlight', 'node:1': 'highlight' }), annotations: ['cạnh (0,1)'] });

      trace.push({ line: 3, explanation: `Biểu diễn danh sách kề: mỗi đỉnh lưu danh sách đỉnh kề → bộ nhớ O(V+E).`, structure: setStatuses(graph, { 'node:2': 'highlight', 'node:3': 'highlight' }), annotations: ['adjacency list O(V+E)'] });

      trace.push({ line: 4, explanation: `Duyệt BFS từ đỉnh 0 theo thứ tự tăng dần → O(V+E): thăm 0, 1, 2, ...`, structure: setStatuses(graph, { 'node:0': 'done', 'node:1': 'done', 'node:2': 'done' }), annotations: ['BFS O(V+E)'] });

      trace.push({ line: 5, explanation: 'Kết thúc: đồ thị — biểu diễn O(V+E), duyệt BFS/DFS O(V+E), Dijkstra O((V+E) log V).', structure: setStatuses(graph, { 'node:0': 'done', 'node:1': 'done', 'node:2': 'done', 'node:3': 'done', 'node:4': 'done', 'node:5': 'done' }), annotations: ['nền tảng: mạng, bản đồ, mạng xã hội'] });
      return trace.steps;
    },
  });
}

// ── Tiện ích validate/đọc input chung ───────────────────────────────────────

function wrapValidate(input: InputConfig, minLen: number, maxLen: number): { ok: boolean; errors: string[] } {
  const errors = validateValues(input, minLen, maxLen);
  return { ok: errors.length === 0, errors };
}

function validateValues(input: InputConfig, minLen: number, maxLen: number): string[] {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const key = Array.isArray(rec.values) ? 'values' : 'keys';
  const arr = rec[key];
  if (!Array.isArray(arr)) {
    return [`${key}: cần danh sách giá trị (${minLen}–${maxLen} phần tử)`];
  }
  const errors: string[] = [];
  if (arr.length < minLen || arr.length > maxLen) {
    errors.push(`${key}: phải có ${minLen}–${maxLen} phần tử (hiện có ${arr.length})`);
  }
  arr.forEach((v, i) => {
    if (typeof v !== 'number' || !Number.isInteger(v) || v < -999 || v > 999) {
      errors.push(`${key}[${i}]=${String(v)} phải là số nguyên trong khoảng -999..999`);
    }
  });
  return errors;
}

function readValues(input: InputConfig, dflt: number[]): number[] {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const arr = Array.isArray(rec.values) ? rec.values : rec.keys;
  if (Array.isArray(arr) && arr.length > 0 && arr.every((x) => typeof x === 'number')) {
    return (arr as number[]).slice();
  }
  return dflt.slice();
}
