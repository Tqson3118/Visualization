// engines/generators/tree/bst.ts — BST: insert/delete/search + 4 duyệt (SDD §4.7.12, §4.6.4)
import type { Element, ElementStatus, InputConfig, InputSchema, Link, SimulationGenerator, Structure } from '../../core/types';
import { buildGenerator, intField, intArrayField, strField, Trace } from '../helpers';

// ── Mô hình cây ──────────────────────────────────────────────────────────────

interface BNode {
  key: number;
  left: BNode | null;
  right: BNode | null;
}

function buildBst(keys: number[]): BNode | null {
  let root: BNode | null = null;
  for (const k of keys) {
    root = insertRaw(root, k);
  }
  return root;
}

function insertRaw(node: BNode | null, key: number): BNode {
  if (node === null) return { key, left: null, right: null };
  if (key < node.key) node.left = insertRaw(node.left, key);
  else if (key > node.key) node.right = insertRaw(node.right, key);
  return node;
}

function findRaw(node: BNode | null, key: number): BNode | null {
  if (node === null) return null;
  if (key === node.key) return node;
  return key < node.key ? findRaw(node.left, key) : findRaw(node.right, key);
}

function minNode(node: BNode): BNode {
  let cur = node;
  while (cur.left) cur = cur.left;
  return cur;
}

function treeStructure(root: BNode | null, statuses: Record<string, ElementStatus>, metas?: Record<string, Record<string, unknown>>): Structure {
  const elements: Element[] = [];
  const links: Link[] = [];
  const queue: Array<BNode | null> = [root];
  while (queue.length > 0) {
    const node = queue.shift() ?? null;
    if (node === null) continue;
    const id = `node:${node.key}`;
    elements.push({
      id,
      label: String(node.key),
      status: statuses[id] ?? 'default',
      group: 'tree',
      meta: metas && metas[id] ? metas[id] : undefined,
    });
    if (node.left) links.push({ from: id, to: `node:${node.left.key}`, label: 'L' });
    if (node.right) links.push({ from: id, to: `node:${node.right.key}`, label: 'R' });
    queue.push(node.left);
    queue.push(node.right);
  }
  return { kind: 'tree', elements, links };
}

// ── Schema chung ─────────────────────────────────────────────────────────────

const SCHEMA: InputSchema = {
  kind: 'tree',
  fields: [
    { name: 'keys', type: 'int[]', label: 'Dãy khóa', min: -999, max: 999, default: [50, 30, 70, 20, 40, 60, 80], description: 'Các khóa dựng cây (1–31 khóa, không trùng)' },
    { name: 'operation', type: 'select', label: 'Thao tác', options: [
      { label: 'Chèn', value: 'insert' },
      { label: 'Xóa', value: 'delete' },
      { label: 'Tìm kiếm', value: 'search' },
    ], default: 'insert', description: 'Thao tác trên cây' },
    { name: 'value', type: 'int', label: 'Giá trị', min: -999, max: 999, default: 25, description: 'Khóa chèn / xóa / tìm kiếm' },
  ],
};

const PSEUDO_INSERT = [
  'procedure bstInsert(root, x)',
  '  if root = null then return newNode(x)',
  '  if x < root.key then root.left ← bstInsert(root.left, x)',
  '  else if x > root.key then root.right ← bstInsert(root.right, x)',
  '  else return root              // trùng khóa: bỏ qua',
  '  return root',
  '  end procedure',
];

const PSEUDO_SEARCH = [
  'procedure bstSearch(root, x)',
  '  while root ≠ null do',
  '    if x = root.key then return root',
  '    if x < root.key then root ← root.left',
  '    else root ← root.right',
  '  return null                  // không tìm thấy',
  '  end procedure',
];

const PSEUDO_DELETE = [
  'procedure bstDelete(root, x)',
  '  if root = null then return null',
  '  if x < root.key then root.left ← bstDelete(root.left, x)',
  '  else if x > root.key then root.right ← bstDelete(root.right, x)',
  '  else',
  '    if root.left = null then return root.right',
  '    if root.right = null then return root.left',
  '    min ← bstMin(root.right)',
  '    root.key ← min.key',
  '    root.right ← bstDelete(root.right, min.key)',
  '  return root',
  '  end procedure',
];

const PSEUDO_TRAVERSAL = (name: string, visitLine: number): string[] => [
  `procedure ${name}(root)`,
  '  if root = null then return',
  ...(name === 'inorder'
    ? ['  inorder(root.left)', `  thăm root            // dòng ${visitLine}`, '  inorder(root.right)']
    : name === 'postorder'
      ? ['  postorder(root.left)', '  postorder(root.right)', `  thăm root            // dòng ${visitLine}`]
      : [`  thăm root            // dòng ${visitLine}`, '  preorder(root.left)', '  preorder(root.right)']),
  '  end procedure',
];

const PSEUDO_LEVEL = [
  'procedure levelorder(root)',
  '  queue ← [root]',
  '  while queue ≠ rỗng do',
  '    u ← dequeue',
  '    thăm u',
  '    nếu u.left ≠ null thì enqueue(u.left)',
  '    nếu u.right ≠ null thì enqueue(u.right)',
  '  end procedure',
];

const OPERATIONS = ['insert', 'delete', 'search'];

function bstValidate(input: InputConfig): { ok: boolean; errors: string[] } {
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
  const op = strField(rec, 'operation', 'insert');
  if (!OPERATIONS.includes(op)) errors.push(`operation: phải là một trong ${OPERATIONS.join(', ')} (hiện tại '${op}')`);
  const value = intField(rec, 'value', 25);
  if (value < -999 || value > 999) errors.push(`value: phải trong khoảng -999..999 (hiện tại ${value})`);
  return { ok: errors.length === 0, errors };
}

// ── Các generator ────────────────────────────────────────────────────────────

export function createBstInsertGenerator(): SimulationGenerator {
  return buildGenerator('tree.bst-insert', SCHEMA, PSEUDO_INSERT, {
    validate: bstValidate,
    generate: (input) => runBst(input, 'insert'),
  });
}

export function createBstDeleteGenerator(): SimulationGenerator {
  return buildGenerator('tree.bst-delete', SCHEMA, PSEUDO_DELETE, {
    validate: bstValidate,
    generate: (input) => runBst(input, 'delete'),
  });
}

export function createBstSearchGenerator(): SimulationGenerator {
  return buildGenerator('tree.bst-search', SCHEMA, PSEUDO_SEARCH, {
    validate: bstValidate,
    generate: (input) => runBst(input, 'search'),
  });
}

export function createBstPreorderGenerator(): SimulationGenerator {
  return buildGenerator('tree.bst-preorder', SCHEMA, PSEUDO_TRAVERSAL('preorder', 3), {
    validate: bstValidate,
    generate: (input) => runTraversal(input, 'preorder'),
  });
}

export function createBstInorderGenerator(): SimulationGenerator {
  return buildGenerator('tree.bst-inorder', SCHEMA, PSEUDO_TRAVERSAL('inorder', 4), {
    validate: bstValidate,
    generate: (input) => runTraversal(input, 'inorder'),
  });
}

export function createBstPostorderGenerator(): SimulationGenerator {
  return buildGenerator('tree.bst-postorder', SCHEMA, PSEUDO_TRAVERSAL('postorder', 4), {
    validate: bstValidate,
    generate: (input) => runTraversal(input, 'postorder'),
  });
}

export function createBstLevelorderGenerator(): SimulationGenerator {
  return buildGenerator('tree.bst-levelorder', SCHEMA, PSEUDO_LEVEL, {
    validate: bstValidate,
    generate: (input) => runTraversal(input, 'levelorder'),
  });
}

// ── Insert ───────────────────────────────────────────────────────────────────

function runBst(input: InputConfig, op: 'insert' | 'delete' | 'search'): ReturnType<SimulationGenerator['generate']> {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const keys = intArrayField(rec, 'keys', [50, 30, 70, 20, 40, 60, 80]);
  const value = intField(rec, 'value', 25);
  const trace = new Trace();
  const statuses: Record<string, ElementStatus> = {};
  let root = buildBst(keys);
  const n = countNodes(root);

  trace.vars.x = value;
  trace.vars.current = root ? root.key : null;
  trace.vars.n = n;
  trace.push({
    line: 1,
    explanation: `Bắt đầu: cây BST dựng từ các khóa [${keys.join(', ')}], thao tác ${op} ${value}.`,
    structure: treeStructure(root, statuses),
    annotations: [`n=${n} nút`],
  });

  if (op === 'search') {
    let cur = root;
    let found: BNode | null = null;
    while (cur !== null) {
      statuses[`node:${cur.key}`] = 'active';
      trace.stats.comparisons++;
      trace.push({
        line: 3,
        explanation: `So sánh x=${value} và nút ${cur.key}.`,
        structure: treeStructure(root, statuses),
        annotations: [`x=${value} so với nút ${cur.key}`],
      });
      if (value === cur.key) {
        statuses[`node:${cur.key}`] = 'done';
        found = cur;
        trace.push({
          line: 3,
          explanation: `x=${value} = ${cur.key} → Tìm thấy.`,
          structure: treeStructure(root, statuses),
          annotations: [`Tìm thấy ${value}`],
        });
        break;
      }
      const goRight = value > cur.key;
      trace.push({
        line: goRight ? 5 : 4,
        explanation: `x=${value} so với nút ${cur.key}: ${value} ${goRight ? '>' : '<'} ${cur.key} → rẽ ${goRight ? 'phải' : 'trái'}.`,
        structure: treeStructure(root, statuses),
        annotations: [`${value} ${goRight ? '>' : '<'} ${cur.key} → ${goRight ? 'phải' : 'trái'}`],
      });
      statuses[`node:${cur.key}`] = 'muted';
      cur = goRight ? cur.right : cur.left;
      trace.vars.current = cur ? cur.key : null;
    }
    if (found) {
      trace.push({
        line: 7,
        explanation: `Kết thúc: tìm thấy ${value} trong cây.`,
        structure: treeStructure(root, statuses),
        annotations: [`Tìm thấy ${value}`],
      });
    } else {
      trace.push({
        line: 6,
        explanation: `Kết thúc: không tìm thấy ${value} trong cây (return null).`,
        structure: treeStructure(root, statuses),
        annotations: ['Không tìm thấy'],
      });
    }
    return trace.steps;
  }

  if (op === 'insert') {
    const state = { root };
    insertWithTrace(state, root, value, trace, statuses);
    root = state.root;
    trace.push({
      line: 7,
      explanation: `Kết thúc: đã chèn ${value} vào cây BST.`,
      structure: treeStructure(root, statuses),
      annotations: [`chèn ${value} xong`],
    });
    return trace.steps;
  }

  // delete
  if (root === null) {
    trace.push({
      line: 2,
      explanation: 'Kết thúc: cây rỗng, không có gì để xóa.',
      structure: treeStructure(root, statuses),
    });
    return trace.steps;
  }
  if (!findRaw(root, value)) {
    let cur: BNode | null = root;
    while (cur !== null) {
      statuses[`node:${cur.key}`] = 'active';
      trace.stats.comparisons++;
      trace.push({
        line: 3,
        explanation: `So sánh x=${value} và nút ${cur.key}.`,
        structure: treeStructure(root, statuses),
        annotations: [`x=${value} so với nút ${cur.key}`],
      });
      const goRight = value > cur.key;
      trace.push({
        line: goRight ? 4 : 3,
        explanation: `${value} ${goRight ? '>' : '<'} ${cur.key} → rẽ ${goRight ? 'phải' : 'trái'}.`,
        structure: treeStructure(root, statuses),
      });
      statuses[`node:${cur.key}`] = 'muted';
      if (goRight) cur = cur.right;
      else cur = cur.left;
    }
    trace.push({
      line: 2,
      explanation: `Kết thúc: không tìm thấy ${value} trong cây → không xóa gì (cây giữ nguyên).`,
      structure: treeStructure(root, statuses),
      annotations: ['Không tìm thấy, không xóa'],
    });
    return trace.steps;
  }
  root = deleteWithTrace(root, value, trace, statuses);
  trace.push({
    line: 11,
    explanation: `Kết thúc: đã xóa ${value} khỏi cây BST.`,
    structure: treeStructure(root, statuses),
    annotations: [`xóa ${value} xong`],
  });
  return trace.steps;
}

/** Chèn kèm trace: cập nhật state.root, trả về nút con mới. */
function insertWithTrace(
  state: { root: BNode | null },
  node: BNode | null,
  x: number,
  trace: Trace,
  statuses: Record<string, ElementStatus>,
): BNode {
  if (node === null) {
    trace.push({
      line: 2,
      explanation: `Gặp vị trí rỗng → tạo nút mới mang giá trị ${x}.`,
      structure: treeStructure(state.root, statuses),
      annotations: [`newNode(${x})`],
    });
    return { key: x, left: null, right: null };
  }
  statuses[`node:${node.key}`] = 'active';
  trace.stats.comparisons++;
  trace.push({
    line: 3,
    explanation: `So sánh x=${x} và nút ${node.key}.`,
    structure: treeStructure(state.root, statuses),
    annotations: [`x=${x} so với nút ${node.key}`],
  });
  if (x < node.key) {
    trace.push({
      line: 3,
      explanation: `${x} < ${node.key} → đúng, rẽ trái.`,
      structure: treeStructure(state.root, statuses),
      annotations: [`${x} < ${node.key} → trái`],
    });
    statuses[`node:${node.key}`] = 'muted';
    node.left = insertWithTrace(state, node.left, x, trace, statuses);
    statuses[`node:${node.key}`] = 'default';
  } else if (x > node.key) {
    trace.push({
      line: 4,
      explanation: `${x} > ${node.key} → đúng, rẽ phải.`,
      structure: treeStructure(state.root, statuses),
      annotations: [`${x} > ${node.key} → phải`],
    });
    statuses[`node:${node.key}`] = 'muted';
    node.right = insertWithTrace(state, node.right, x, trace, statuses);
    statuses[`node:${node.key}`] = 'default';
  } else {
    statuses[`node:${node.key}`] = 'highlight';
    trace.push({
      line: 5,
      explanation: `x=${x} = nút ${node.key} → khóa đã tồn tại, bỏ qua chèn.`,
      structure: treeStructure(state.root, statuses),
      annotations: ['khóa trùng → bỏ qua'],
    });
    statuses[`node:${node.key}`] = 'default';
  }
  return node;
}

/** Xóa kèm trace: trả về root mới. */
function deleteWithTrace(node: BNode, x: number, trace: Trace, statuses: Record<string, ElementStatus>): BNode | null {
  statuses[`node:${node.key}`] = 'active';
  trace.stats.comparisons++;
  trace.push({
    line: 3,
    explanation: `So sánh x=${x} và nút ${node.key}.`,
    structure: treeStructure(node, statuses),
    annotations: [`x=${x} so với nút ${node.key}`],
  });
  if (x < node.key) {
    trace.push({
      line: 3,
      explanation: `${x} < ${node.key} → rẽ trái.`,
      structure: treeStructure(node, statuses),
    });
    statuses[`node:${node.key}`] = 'muted';
    if (node.left) node.left = deleteWithTrace(node.left, x, trace, statuses);
    return node;
  }
  if (x > node.key) {
    trace.push({
      line: 4,
      explanation: `${x} > ${node.key} → rẽ phải.`,
      structure: treeStructure(node, statuses),
    });
    statuses[`node:${node.key}`] = 'muted';
    if (node.right) node.right = deleteWithTrace(node.right, x, trace, statuses);
    return node;
  }

  // Tìm thấy nút cần xóa
  statuses[`node:${node.key}`] = 'error';
  trace.push({
    line: 5,
    explanation: `Tìm thấy nút ${node.key} cần xóa.`,
    structure: treeStructure(node, statuses),
    annotations: [`xóa nút ${node.key}`],
  });

  if (node.left === null) {
    statuses[`node:${node.key}`] = 'muted';
    trace.push({
      line: 6,
      explanation: `Nút ${node.key} không có con trái → thay bằng con phải.`,
      structure: treeStructure(node, statuses),
    });
    return node.right;
  }
  if (node.right === null) {
    statuses[`node:${node.key}`] = 'muted';
    trace.push({
      line: 7,
      explanation: `Nút ${node.key} không có con phải → thay bằng con trái.`,
      structure: treeStructure(node, statuses),
    });
    return node.left;
  }

  // 2 con: tìm min cây con phải
  let min = node.right;
  while (min.left) min = min.left;
  statuses[`node:${min.key}`] = 'highlight';
  trace.push({
    line: 8,
    explanation: `Nút ${node.key} có 2 con → tìm min cây con phải: nút ${min.key}.`,
    structure: treeStructure(node, statuses),
    annotations: [`min=${min.key}`],
  });
  statuses[`node:${node.key}`] = 'swap';
  trace.stats.writes++;
  trace.push({
    line: 9,
    explanation: `Thay giá trị: nút ${node.key}.key ← ${min.key}.`,
    structure: treeStructure(node, statuses),
    annotations: [`${node.key} ← ${min.key}`],
  });
  const deletedKey = node.key;
  node.key = min.key;
  statuses[`node:${node.key}`] = 'default';
  statuses[`node:${deletedKey}`] = 'muted';
  trace.push({
    line: 10,
    explanation: `Xóa đệ quy nút ${min.key} khỏi cây con phải.`,
    structure: treeStructure(node, statuses),
  });
  node.right = deleteRaw(node.right, min.key);
  return node;
}

function deleteRaw(node: BNode | null, key: number): BNode | null {
  if (node === null) return null;
  if (key < node.key) { node.left = deleteRaw(node.left, key); return node; }
  if (key > node.key) { node.right = deleteRaw(node.right, key); return node; }
  if (node.left === null) return node.right;
  if (node.right === null) return node.left;
  const min = minNode(node.right);
  node.key = min.key;
  node.right = deleteRaw(node.right, min.key);
  return node;
}

function countNodes(root: BNode | null): number {
  if (root === null) return 0;
  return 1 + countNodes(root.left) + countNodes(root.right);
}

// ── Traversals ───────────────────────────────────────────────────────────────

function runTraversal(input: InputConfig, order: 'preorder' | 'inorder' | 'postorder' | 'levelorder'): ReturnType<SimulationGenerator['generate']> {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const keys = intArrayField(rec, 'keys', [50, 30, 70, 20, 40, 60, 80]);
  const root = buildBst(keys);
  const trace = new Trace();
  const statuses: Record<string, ElementStatus> = {};
  const n = countNodes(root);
  const visited: number[] = [];

  trace.vars.visited = 0;
  trace.vars.n = n;
  trace.push({
    line: 1,
    explanation: `Bắt đầu: duyệt ${order} cây BST [${keys.join(', ')}].`,
    structure: treeStructure(root, statuses),
    annotations: [`n=${n} nút`],
  });

  const visit = (nodeKey: number, line: number, nextLine: number): void => {
    statuses[`node:${nodeKey}`] = 'active';
    trace.push({
      line,
      explanation: `Thăm nút ${nodeKey} (lượt ${visited.length + 1}).`,
      structure: treeStructure(root, statuses),
      annotations: [`thứ tự: ${[...visited, nodeKey].join(' → ')}`],
    });
    statuses[`node:${nodeKey}`] = 'done';
    visited.push(nodeKey);
    trace.vars.visited = visited.length;
    trace.push({
      line: nextLine,
      explanation: `Nút ${nodeKey} đã duyệt xong (${visited.length}/${n}).`,
      structure: treeStructure(root, statuses),
      annotations: [`thứ tự: ${visited.join(' → ')}`],
    });
  };

  if (order === 'preorder') {
    const stack: BNode[] = root ? [root] : [];
    while (stack.length > 0) {
      const node = stack.pop() as BNode;
      visit(node.key, 3, 3);
      if (node.right) stack.push(node.right);
      if (node.left) stack.push(node.left);
    }
  } else if (order === 'inorder') {
    const stack: BNode[] = [];
    let cur: BNode | null = root;
    while (cur !== null || stack.length > 0) {
      while (cur !== null) {
        stack.push(cur);
        cur = cur.left;
      }
      const node = stack.pop() as BNode;
      visit(node.key, 4, 4);
      cur = node.right;
    }
  } else if (order === 'postorder') {
    const stack: Array<{ node: BNode; done: boolean }> = root ? [{ node: root, done: false }] : [];
    while (stack.length > 0) {
      const top = stack[stack.length - 1];
      if (top.done) {
        stack.pop();
        visit(top.node.key, 4, 4);
      } else {
        top.done = true;
        if (top.node.right) stack.push({ node: top.node.right, done: false });
        if (top.node.left) stack.push({ node: top.node.left, done: false });
      }
    }
  } else {
    const queue: BNode[] = root ? [root] : [];
    while (queue.length > 0) {
      const node = queue.shift() as BNode;
      visit(node.key, 4, 5);
      if (node.left) queue.push(node.left);
      if (node.right) queue.push(node.right);
      trace.vars.queue = queue.map((q) => q.key).join(',');
    }
  }

  trace.push({
    line: 7,
    explanation: `Kết thúc: duyệt ${order} xong, thứ tự [${visited.join(', ')}].`,
    structure: treeStructure(root, statuses),
    annotations: [`thứ tự: ${visited.join(' → ')}`],
  });
  return trace.steps;
}
