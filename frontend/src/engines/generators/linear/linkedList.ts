// engines/generators/linear/linkedList.ts — Danh sách liên kết đơn (SDD §4.7.11, §4.6.3)
// Thao tác: insertHead / insertTail / insertAt / deleteAt / search / traverse
import type { Element, InputConfig, InputSchema, Link, SimulationGenerator, Structure } from '../../core/types';
import { buildGenerator, intField, intArrayField, strField, Trace } from '../helpers';

const PSEUDOCODE = [
  'insertHead(x):  newNode ← tạo nút(x); newNode.next ← head; head ← newNode',
  'insertTail(x):  duyệt tới nút cuối; nút cuối.next ← newNode',
  'insertAt(k, x): duyệt tới vị trí k-1 (error nếu k ngoài phạm vi); chèn',
  'deleteAt(k):    duyệt tới k-1; xóa nút k; nút k-1.next ← nút k+1 (error nếu rỗng/k ngoài phạm vi)',
  'search(x):      duyệt, so sánh từng nút; trả vị trí/không tìm thấy',
  'traverse():     từ head in từng giá trị tới null',
];

const SCHEMA: InputSchema = {
  kind: 'linear',
  fields: [
    { name: 'initialValues', type: 'int[]', label: 'Giá trị ban đầu', min: -999, max: 999, default: [], description: 'Danh sách nút ban đầu (0–20 nút)' },
    { name: 'operation', type: 'select', label: 'Thao tác', options: [
      { label: 'Chèn đầu (insertHead)', value: 'insertHead' },
      { label: 'Chèn cuối (insertTail)', value: 'insertTail' },
      { label: 'Chèn tại vị trí (insertAt)', value: 'insertAt' },
      { label: 'Xóa tại vị trí (deleteAt)', value: 'deleteAt' },
      { label: 'Tìm kiếm (search)', value: 'search' },
      { label: 'Duyệt (traverse)', value: 'traverse' },
    ], default: 'insertHead', description: 'Thao tác minh họa' },
    { name: 'value', type: 'int', label: 'Giá trị', min: -999, max: 999, default: 7, description: 'Giá trị chèn / tìm kiếm' },
    { name: 'position', type: 'int', label: 'Vị trí', min: 0, max: 20, default: 0, description: 'Vị trí chèn (0..len) hoặc xóa (0..len-1)' },
  ],
};

const OPERATIONS = ['insertHead', 'insertTail', 'insertAt', 'deleteAt', 'search', 'traverse'];

function listStructure(values: number[], statuses: Record<string, Element['status']>, mutedAll = false): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < values.length; i++) {
    elements.push({
      id: `node:${i}`,
      label: String(values[i]),
      status: statuses[`node:${i}`] ?? (mutedAll ? 'muted' : 'default'),
      group: 'linkedlist',
      meta: { index: i },
    });
  }
  const links: Link[] = [];
  for (let i = 0; i + 1 < values.length; i++) {
    links.push({ from: `node:${i}`, to: `node:${i + 1}`, label: 'next' });
  }
  return { kind: 'linkedlist', elements, links };
}

export function createListInsertGenerator(): SimulationGenerator {
  return buildGenerator('list.insert', SCHEMA, PSEUDOCODE, { validate: listValidate, generate: (i) => runListOps(i) });
}

export function createListDeleteGenerator(): SimulationGenerator {
  return buildGenerator('list.delete', SCHEMA, PSEUDOCODE, { validate: listValidate, generate: (i) => runListOps(i) });
}

export function createListSearchGenerator(): SimulationGenerator {
  return buildGenerator('list.search', SCHEMA, PSEUDOCODE, { validate: listValidate, generate: (i) => runListOps(i) });
}

export function createListTraverseGenerator(): SimulationGenerator {
  return buildGenerator('list.traverse', SCHEMA, PSEUDOCODE, { validate: listValidate, generate: (i) => runListOps(i) });
}

function listValidate(input: InputConfig): { ok: boolean; errors: string[] } {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const errors: string[] = [];
  const initial = intArrayField(rec, 'initialValues', []);
  if (initial.length > 20) errors.push(`initialValues: tối đa 20 nút (hiện có ${initial.length})`);
  initial.forEach((v, i) => {
    if (v < -999 || v > 999) errors.push(`initialValues[${i}]=${v} phải trong khoảng -999..999`);
  });
  const op = strField(rec, 'operation', 'insertHead');
  if (!OPERATIONS.includes(op)) errors.push(`operation: phải là một trong ${OPERATIONS.join(', ')} (hiện tại '${op}')`);
  const value = intField(rec, 'value', 7);
  if (value < -999 || value > 999) errors.push(`value: phải trong khoảng -999..999 (hiện tại ${value})`);
  const position = intField(rec, 'position', 0);
  if (position < 0 || position > 20) errors.push(`position: phải trong khoảng 0–20 (hiện tại ${position})`);
  if (op === 'insertAt' && position > initial.length) {
    errors.push(`insertAt: position=${position} vượt quá độ dài hiện tại ${initial.length} (tối đa ${initial.length})`);
  }
  if (op === 'deleteAt') {
    if (initial.length === 0) errors.push('deleteAt: danh sách đang rỗng, không thể xóa');
    else if (position >= initial.length) errors.push(`deleteAt: position=${position} ngoài phạm vi 0..${initial.length - 1}`);
  }
  return { ok: errors.length === 0, errors };
}

function runListOps(input: InputConfig): ReturnType<SimulationGenerator['generate']> {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const values = intArrayField(rec, 'initialValues', []).slice();
  const op = strField(rec, 'operation', 'insertHead');
  const value = intField(rec, 'value', 7);
  const position = intField(rec, 'position', 0);
  const trace = new Trace();
  const statuses: Record<string, Element['status']> = {};

  trace.vars.head = values.length > 0 ? 0 : null;
  trace.vars.current = null;
  trace.vars.n = values.length;
  trace.push({
    line: 1,
    explanation: values.length > 0
      ? `Bắt đầu: danh sách liên kết [${values.join(' → ')} → null], head = nút 0.`
      : 'Bắt đầu: danh sách liên kết rỗng (head = null).',
    structure: listStructure(values, statuses),
    annotations: [`head=${values.length > 0 ? 0 : 'null'}`],
  });

  const lineFor = (o: string): number => {
    switch (o) {
      case 'insertHead': return 1;
      case 'insertTail': return 2;
      case 'insertAt': return 3;
      case 'deleteAt': return 4;
      case 'search': return 5;
      default: return 6;
    }
  };

  // ── traverse ──
  if (op === 'traverse') {
    if (values.length === 0) {
      trace.push({ line: 6, explanation: 'Danh sách rỗng → không có gì để duyệt.', structure: listStructure(values, statuses), annotations: ['head=null'] });
    } else {
      for (let i = 0; i < values.length; i++) {
        statuses[`node:${i}`] = 'active';
        trace.push({
          line: 6,
          explanation: `Duyệt: gặp nút ${i} có giá trị ${values[i]}.`,
          structure: listStructure(values, statuses),
          annotations: [`current=${i}, value=${values[i]}`],
        });
        statuses[`node:${i}`] = 'done';
      }
      trace.push({
        line: 6,
        explanation: `Duyệt xong: [${values.join(' → ')} → null] (${values.length} nút).`,
        structure: listStructure(values, statuses),
        annotations: ['hết danh sách (null)'],
      });
    }
    trace.push({ line: 6, explanation: `Kết thúc: đã duyệt ${values.length} nút.`, structure: listStructure(values, statuses) });
    return trace.steps;
  }

  // ── search ──
  if (op === 'search') {
    let found = -1;
    for (let i = 0; i < values.length; i++) {
      statuses[`node:${i}`] = 'active';
      trace.stats.comparisons++;
      trace.push({
        line: 5,
        explanation: `Tìm ${value}: so sánh nút ${i} (giá trị ${values[i]}) với ${value}.`,
        structure: listStructure(values, statuses),
        annotations: [`values[${i}]=${values[i]} = ${value}?`],
      });
      if (values[i] === value) {
        statuses[`node:${i}`] = 'done';
        found = i;
        trace.push({
          line: 5,
          explanation: `${values[i]} = ${value} → Tìm thấy tại vị trí ${i}.`,
          structure: listStructure(values, statuses),
          annotations: [`Tìm thấy tại vị trí ${i}`],
        });
        break;
      }
      statuses[`node:${i}`] = 'muted';
      trace.push({
        line: 5,
        explanation: `${values[i]} = ${value} → sai, sang nút tiếp theo.`,
        structure: listStructure(values, statuses),
      });
    }
    if (found === -1) {
      trace.push({
        line: 5,
        explanation: `Kết thúc: không tìm thấy ${value} trong danh sách (${values.length} nút đã duyệt).`,
        structure: listStructure(values, statuses, true),
        annotations: ['Không tìm thấy'],
      });
    } else {
      trace.push({
        line: 5,
        explanation: `Kết thúc: tìm thấy ${value} tại vị trí ${found}.`,
        structure: listStructure(values, statuses),
      });
    }
    return trace.steps;
  }

  // ── insert ──
  if (op === 'insertHead' || op === 'insertTail' || op === 'insertAt') {
    // Bước 1: tạo nút mới (chưa nối) — hiển thị như node phụ cuối.
    const newId = `node:${values.length}`;
    statuses[newId] = 'highlight';
    const withNew: Element[] = listStructure(values, statuses).elements.concat([
      { id: newId, label: String(value), status: 'highlight', group: 'linkedlist', meta: { index: values.length } },
    ]);
    trace.push({
      line: lineFor(op),
      explanation: `Tạo nút mới mang giá trị ${value} (chưa nối vào danh sách).`,
      structure: { kind: 'linkedlist', elements: withNew, links: listStructure(values, statuses).links },
      annotations: [`newNode = ${value}`],
    });
    delete statuses[newId];

    if (op === 'insertHead') {
      values.unshift(value);
      const st: Record<string, Element['status']> = { 'node:0': 'swap' };
      for (let i = 1; i < values.length; i++) st[`node:${i}`] = 'default';
      trace.push({
        line: 1,
        explanation: `newNode.next ← head; head ← newNode → ${value} thành nút đầu.`,
        structure: listStructure(values, st),
        annotations: [`head=${value}`],
      });
      st['node:0'] = 'done';
      trace.push({
        line: 1,
        explanation: `Kết thúc: chèn ${value} vào đầu danh sách → [${values.join(' → ')} → null].`,
        structure: listStructure(values, st),
      });
    } else if (op === 'insertTail') {
      if (values.length === 0) {
        values.push(value);
        trace.push({
          line: 2,
          explanation: `Danh sách rỗng → head ← newNode(${value}).`,
          structure: listStructure(values, { 'node:0': 'swap' }),
          annotations: [`head=${value}`],
        });
        trace.push({ line: 2, explanation: `Kết thúc: [${values.join(' → ')} → null].`, structure: listStructure(values, { 'node:0': 'done' }) });
      } else {
        for (let i = 0; i < values.length; i++) statuses[`node:${i}`] = i === values.length - 1 ? 'active' : 'muted';
        trace.push({
          line: 2,
          explanation: `Duyệt tới nút cuối: nút ${values.length - 1} có giá trị ${values[values.length - 1]}.`,
          structure: listStructure(values, statuses),
          annotations: [`current=${values.length - 1}`],
        });
        values.push(value);
        statuses[`node:${values.length - 1}`] = 'swap';
        trace.push({
          line: 2,
          explanation: `Nút cuối.next ← newNode(${value}).`,
          structure: listStructure(values, statuses),
          annotations: [`tail.next = ${value}`],
        });
        for (const k of Object.keys(statuses)) statuses[k] = 'done';
        trace.push({
          line: 2,
          explanation: `Kết thúc: chèn ${value} vào cuối → [${values.join(' → ')} → null].`,
          structure: listStructure(values, statuses),
        });
      }
    } else {
      // insertAt
      const k = Math.min(position, values.length);
      if (k === 0) {
        values.unshift(value);
        trace.push({
          line: 3,
          explanation: `insertAt(0, ${value}): chèn vào đầu (head ← newNode).`,
          structure: listStructure(values, { 'node:0': 'swap' }),
          annotations: [`position=0`],
        });
        trace.push({
          line: 3,
          explanation: `Kết thúc: chèn ${value} tại vị trí 0 → [${values.join(' → ')} → null].`,
          structure: listStructure(values, { 'node:0': 'done' }),
        });
      } else {
        for (let i = 0; i < k; i++) statuses[`node:${i}`] = i === k - 1 ? 'active' : 'muted';
        trace.push({
          line: 3,
          explanation: `Duyệt tới vị trí k-1=${k - 1} (nút ${values[k - 1]}).`,
          structure: listStructure(values, statuses),
          annotations: [`current=${k - 1}`],
        });
        values.splice(k, 0, value);
        const st: Record<string, Element['status']> = {};
        for (let i = 0; i < values.length; i++) st[`node:${i}`] = i === k ? 'swap' : 'done';
        trace.push({
          line: 3,
          explanation: `Chèn newNode(${value}) vào sau nút ${k - 1}: nút ${k - 1}.next ← newNode, newNode.next ← nút ${k + 1}.`,
          structure: listStructure(values, st),
          annotations: [`chèn ${value} tại vị trí ${k}`],
        });
        trace.push({
          line: 3,
          explanation: `Kết thúc: [${values.join(' → ')} → null].`,
          structure: listStructure(values, st),
        });
      }
    }
    trace.push({ line: 6, explanation: `Kết thúc: danh sách [${values.join(' → ')} → null] (${values.length} nút).`, structure: listStructure(values, statuses) });
    return trace.steps;
  }

  // ── deleteAt ──
  const k = position;
  if (values.length === 0) {
    trace.push({ line: 4, explanation: 'LỖI: danh sách rỗng, không thể xóa.', structure: listStructure(values, statuses), annotations: ['LỖI: danh sách rỗng'] });
    trace.push({ line: 4, explanation: 'Kết thúc: không có gì để xóa.', structure: listStructure(values, statuses) });
    return trace.steps;
  }
  if (k >= values.length) {
    trace.push({ line: 4, explanation: `LỖI: position=${k} ngoài phạm vi 0..${values.length - 1}.`, structure: listStructure(values, statuses), annotations: ['LỖI: vị trí ngoài phạm vi'] });
    trace.push({ line: 4, explanation: 'Kết thúc: không xóa được.', structure: listStructure(values, statuses) });
    return trace.steps;
  }
  if (k === 0) {
    statuses['node:0'] = 'error';
    trace.push({
      line: 4,
      explanation: `Xóa nút đầu: head ← nút ${values.length > 1 ? 1 : 'null'}, giải phóng nút 0 (${values[0]}).`,
      structure: listStructure(values, statuses),
      annotations: [`xóa nút 0 (${values[0]})`],
    });
    values.shift();
    statuses['node:0'] = 'muted';
    trace.push({
      line: 4,
      explanation: `Kết thúc: xóa ${values.length > 0 ? `đầu danh sách → [${values.join(' → ')} → null]` : 'hết (danh sách rỗng)'}.`,
      structure: listStructure(values, statuses),
    });
  } else {
    for (let i = 0; i < k; i++) statuses[`node:${i}`] = i === k - 1 ? 'active' : 'muted';
    trace.push({
      line: 4,
      explanation: `Duyệt tới nút k-1=${k - 1} (giá trị ${values[k - 1]}).`,
      structure: listStructure(values, statuses),
      annotations: [`current=${k - 1}`],
    });
    statuses[`node:${k}`] = 'error';
    trace.push({
      line: 4,
      explanation: `Xóa nút ${k} (${values[k]}): nút ${k - 1}.next ← nút ${k + 1}.`,
      structure: listStructure(values, statuses),
      annotations: [`xóa nút ${k} (${values[k]})`],
    });
    values.splice(k, 1);
    statuses[`node:${k}`] = 'muted';
    const st: Record<string, Element['status']> = {};
    for (let i = 0; i < values.length; i++) st[`node:${i}`] = 'done';
    trace.push({
      line: 4,
      explanation: `Kết thúc: đã xóa ${k === 0 ? 'nút đầu' : `nút ${k}`} → [${values.join(' → ')} → null].`,
      structure: listStructure(values, st),
    });
  }
  return trace.steps;
}
