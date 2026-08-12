// engines/generators/tree/avl.ts — AVL insert với 4 kiểu xoay LL/RR/LR/RL (SDD §4.7.13, §4.6.4)
import type { Element, ElementStatus, InputConfig, InputSchema, Link, SimulationGenerator, Structure } from '../../core/types';
import { buildGenerator, intField, intArrayField, strField, Trace } from '../helpers';

interface AvlNode {
  key: number;
  left: AvlNode | null;
  right: AvlNode | null;
  height: number;
}

const heightOf = (n: AvlNode | null): number => (n ? n.height : 0);
const bfOf = (n: AvlNode): number => heightOf(n.left) - heightOf(n.right);
const updateHeight = (n: AvlNode): void => {
  n.height = 1 + Math.max(heightOf(n.left), heightOf(n.right));
};

function rotateRight(y: AvlNode): AvlNode {
  const x = y.left as AvlNode;
  y.left = x.right;
  x.right = y;
  updateHeight(y);
  updateHeight(x);
  return x;
}

function rotateLeft(x: AvlNode): AvlNode {
  const y = x.right as AvlNode;
  x.right = y.left;
  y.left = x;
  updateHeight(x);
  updateHeight(y);
  return y;
}

function buildAvl(keys: number[]): AvlNode | null {
  let root: AvlNode | null = null;
  for (const k of keys) root = insertPlain(root, k);
  return root;
}

function insertPlain(node: AvlNode | null, key: number): AvlNode {
  if (node === null) return { key, left: null, right: null, height: 1 };
  if (key < node.key) node.left = insertPlain(node.left, key);
  else if (key > node.key) node.right = insertPlain(node.right, key);
  else return node;
  updateHeight(node);
  const balance = bfOf(node);
  if (balance > 1 && key < (node.left as AvlNode).key) return rotateRight(node);
  if (balance < -1 && key > (node.right as AvlNode).key) return rotateLeft(node);
  if (balance > 1) {
    node.left = rotateLeft(node.left as AvlNode);
    return rotateRight(node);
  }
  if (balance < -1) {
    node.right = rotateRight(node.right as AvlNode);
    return rotateLeft(node);
  }
  return node;
}

function avlStructure(root: AvlNode | null, statuses: Record<string, ElementStatus>): Structure {
  const elements: Element[] = [];
  const links: Link[] = [];
  const queue: Array<AvlNode | null> = [root];
  while (queue.length > 0) {
    const node = queue.shift() ?? null;
    if (node === null) continue;
    const id = `node:${node.key}`;
    elements.push({
      id,
      label: String(node.key),
      status: statuses[id] ?? 'default',
      group: 'tree',
      meta: { bf: bfOf(node), height: node.height },
    });
    if (node.left) links.push({ from: id, to: `node:${node.left.key}`, label: 'L' });
    if (node.right) links.push({ from: id, to: `node:${node.right.key}`, label: 'R' });
    queue.push(node.left);
    queue.push(node.right);
  }
  return { kind: 'tree', elements, links };
}

const PSEUDOCODE = [
  'procedure avlInsert(root, x)',
  '  if root = null then return newNode(x)',
  '  if x < root.key then root.left ← avlInsert(root.left, x)',
  '  else if x > root.key then root.right ← avlInsert(root.right, x)',
  '  else return root',
  '  root.height ← 1 + max(h(left), h(right))',
  '  balance ← h(left) - h(right)',
  '  if |balance| > 1 then',
  '    if balance > 1 và x < left.key then return rotateRight(root)      // LL',
  '    if balance < -1 và x > right.key then return rotateLeft(root)     // RR',
  '    if balance > 1 và x > left.key then root.left ← rotateLeft(left); return rotateRight(root)  // LR',
  '    if balance < -1 và x < right.key thì root.right ← rotateRight(right); return rotateLeft(root)  // RL',
  '  return root',
  '  end procedure',
];

const SCHEMA: InputSchema = {
  kind: 'tree',
  fields: [
    { name: 'keys', type: 'int[]', label: 'Dãy khóa', min: -999, max: 999, default: [50, 30, 70, 20, 40, 60, 80], description: 'Các khóa dựng cây AVL ban đầu (1–31 khóa, không trùng)' },
    { name: 'operation', type: 'select', label: 'Thao tác', options: [
      { label: 'Chèn', value: 'insert' },
    ], default: 'insert', description: 'AVL chỉ minh họa chèn kèm xoay cân bằng' },
    { name: 'value', type: 'int', label: 'Giá trị', min: -999, max: 999, default: 25, description: 'Khóa chèn vào cây' },
  ],
};

export function createAvlInsertGenerator(): SimulationGenerator {
  return buildGenerator('tree.avl-insert', SCHEMA, PSEUDOCODE, {
    validate(input: InputConfig) {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const errors: string[] = [];
      const keys = intArrayField(rec, 'keys', [50, 30, 70, 20, 40, 60, 80]);
      if (keys.length < 1 || keys.length > 31) errors.push(`keys: phải có 1–31 khóa (hiện có ${keys.length})`);
      const seen = new Set<number>();
      keys.forEach((k, i) => {
        if (k < -999 || k > 999) errors.push(`keys[${i}]=${k} phải trong khoảng -999..999`);
        if (seen.has(k)) errors.push(`keys[${i}]=${k} trùng khóa — keys không được trùng`);
        seen.add(k);
      });
      const value = intField(rec, 'value', 25);
      if (value < -999 || value > 999) errors.push(`value: phải trong khoảng -999..999 (hiện tại ${value})`);
      return { ok: errors.length === 0, errors };
    },

    generate(input: InputConfig) {
      const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
      const keys = intArrayField(rec, 'keys', [50, 30, 70, 20, 40, 60, 80]);
      const value = intField(rec, 'value', 25);
      const trace = new Trace();
      const statuses: Record<string, ElementStatus> = {};
      const state = { root: buildAvl(keys) };

      trace.vars.x = value;
      trace.vars.height = null;
      trace.vars.balance = null;
      trace.push({
        line: 1,
        explanation: `Bắt đầu: cây AVL từ các khóa [${keys.join(', ')}], chèn ${value}.`,
        structure: avlStructure(state.root, statuses),
        annotations: [`x=${value}`],
      });

      const insertAvl = (node: AvlNode | null, x: number): AvlNode => {
        if (node === null) {
          trace.push({
            line: 2,
            explanation: `Gặp vị trí rỗng → tạo nút mới ${x} (height=1).`,
            structure: avlStructure(state.root, statuses),
            annotations: [`newNode(${x}), h=1`],
          });
          const created: AvlNode = { key: x, left: null, right: null, height: 1 };
          statuses[`node:${x}`] = 'highlight';
          trace.push({
            line: 2,
            explanation: `Nút ${x} được thêm vào cây.`,
            structure: avlStructure(state.root, statuses),
            annotations: [`chèn ${x}`],
          });
          statuses[`node:${x}`] = 'done';
          return created;
        }

        statuses[`node:${node.key}`] = 'active';
        trace.stats.comparisons++;
        trace.push({
          line: 3,
          explanation: `So sánh x=${x} và nút ${node.key}.`,
          structure: avlStructure(state.root, statuses),
          annotations: [`x=${x} so với nút ${node.key}`],
        });
        if (x < node.key) {
          trace.push({
            line: 3,
            explanation: `${x} < ${node.key} → rẽ trái.`,
            structure: avlStructure(state.root, statuses),
            annotations: [`${x} < ${node.key} → trái`],
          });
          statuses[`node:${node.key}`] = 'muted';
          node.left = insertAvl(node.left, x);
          statuses[`node:${node.key}`] = 'default';
        } else if (x > node.key) {
          trace.push({
            line: 4,
            explanation: `${x} > ${node.key} → rẽ phải.`,
            structure: avlStructure(state.root, statuses),
            annotations: [`${x} > ${node.key} → phải`],
          });
          statuses[`node:${node.key}`] = 'muted';
          node.right = insertAvl(node.right, x);
          statuses[`node:${node.key}`] = 'default';
        } else {
          statuses[`node:${node.key}`] = 'highlight';
          trace.push({
            line: 5,
            explanation: `x=${x} = nút ${node.key} → khóa đã tồn tại, bỏ qua.`,
            structure: avlStructure(state.root, statuses),
            annotations: ['khóa trùng → bỏ qua'],
          });
          statuses[`node:${node.key}`] = 'default';
          return node;
        }

        const oldHeight = node.height;
        updateHeight(node);
        trace.vars.height = node.height;
        trace.push({
          line: 6,
          explanation: `Cập nhật chiều cao nút ${node.key}: ${oldHeight} → ${node.height}.`,
          structure: avlStructure(state.root, statuses),
          annotations: [`h(${node.key})=${node.height}`],
        });

        const balance = bfOf(node);
        trace.vars.balance = balance;
        trace.push({
          line: 7,
          explanation: `balance(${node.key}) = h(${node.left ? node.left.key : '∅'}) - h(${node.right ? node.right.key : '∅'}) = ${balance}.`,
          structure: avlStructure(state.root, statuses),
          annotations: [`bf(${node.key})=${balance}`],
        });

        if (balance > 1 || balance < -1) {
          statuses[`node:${node.key}`] = 'error';
          trace.push({
            line: 8,
            explanation: `|balance| = |${balance}| > 1 → nút ${node.key} mất cân bằng, cần xoay.`,
            structure: avlStructure(state.root, statuses),
            annotations: [`bf=${balance} → mất cân bằng`],
          });
          statuses[`node:${node.key}`] = 'default';

          const left = node.left as AvlNode;
          const right = node.right as AvlNode;
          let rotation = '';

          if (balance > 1 && x < left.key) {
            rotation = 'LL';
            statuses[`node:${node.key}`] = 'swap';
            statuses[`node:${left.key}`] = 'swap';
            trace.push({
              line: 9,
              explanation: `Trường hợp LL: xoay phải quanh nút ${node.key}.`,
              structure: avlStructure(state.root, statuses),
              annotations: ['xoay LL (right rotation)'],
            });
            const newRoot = rotateRight(node);
            state.root = state.root === node ? newRoot : state.root;
            node = newRoot;
          } else if (balance < -1 && x > right.key) {
            rotation = 'RR';
            statuses[`node:${node.key}`] = 'swap';
            statuses[`node:${right.key}`] = 'swap';
            trace.push({
              line: 10,
              explanation: `Trường hợp RR: xoay trái quanh nút ${node.key}.`,
              structure: avlStructure(state.root, statuses),
              annotations: ['xoay RR (left rotation)'],
            });
            const newRoot = rotateLeft(node);
            state.root = state.root === node ? newRoot : state.root;
            node = newRoot;
          } else if (balance > 1 && x > left.key) {
            rotation = 'LR';
            statuses[`node:${left.key}`] = 'swap';
            trace.push({
              line: 11,
              explanation: `Trường hợp LR: xoay trái quanh con trái ${left.key}, rồi xoay phải quanh nút ${node.key}.`,
              structure: avlStructure(state.root, statuses),
              annotations: ['xoay LR'],
            });
            node.left = rotateLeft(left);
            const newRoot = rotateRight(node);
            state.root = state.root === node ? newRoot : state.root;
            node = newRoot;
          } else {
            rotation = 'RL';
            statuses[`node:${right.key}`] = 'swap';
            trace.push({
              line: 12,
              explanation: `Trường hợp RL: xoay phải quanh con phải ${right.key}, rồi xoay trái quanh nút ${node.key}.`,
              structure: avlStructure(state.root, statuses),
              annotations: ['xoay RL'],
            });
            node.right = rotateRight(right);
            const newRoot = rotateLeft(node);
            state.root = state.root === node ? newRoot : state.root;
            node = newRoot;
          }

          updateHeight(node);
          for (const key of Object.keys(statuses)) statuses[key] = 'done';
          trace.push({
            line: rotation === 'LL' ? 9 : rotation === 'RR' ? 10 : rotation === 'LR' ? 11 : 12,
            explanation: `Sau xoay ${rotation}, nút ${node.key} cân bằng: h=${node.height}, bf=${bfOf(node)}.`,
            structure: avlStructure(state.root, statuses),
            annotations: [`xoay ${rotation} xong`],
          });
          for (const key of Object.keys(statuses)) statuses[key] = 'default';
        }
        return node;
      };

      state.root = insertAvl(state.root, value);

      for (const key of Object.keys(statuses)) statuses[key] = 'default';
      trace.push({
        line: 13,
        explanation: `Kết thúc: đã chèn ${value} vào cây AVL (cân bằng, mọi nút có |bf| ≤ 1).`,
        structure: avlStructure(state.root, statuses),
        annotations: ['AVL cân bằng'],
      });
      return trace.steps;
    },
  });
}
