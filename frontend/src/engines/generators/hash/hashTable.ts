// engines/generators/hash/hashTable.ts — Bảng băm chuỗi nối kết (SDD §4.7 bảng băm, §4.6.5)
import type { ElementStatus, InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import { buildGenerator, hashIndex, hashStructure, intArrayField, intField, strField, Trace } from '../helpers';

const PSEUDOCODE = [
  'procedure hashInsert(table, k)',
  '  idx ← h(k)',
  '  bucket ← table[idx]',
  '  while bucket ≠ null và bucket.key ≠ k do bucket ← bucket.next',
  '  if bucket = null then thêm nút mới vào đầu bucket',
  '  else bucket.key ← k          // cập nhật',
  'procedure hashSearch(table, k)',
  '  idx ← h(k); bucket ← table[idx]',
  '  while bucket ≠ null và bucket.key ≠ k do bucket ← bucket.next',
  '  if bucket ≠ null then return bucket',
  '  return null',
  'procedure hashDelete(table, k)',
  '  idx ← h(k); prev ← null; bucket ← table[idx]',
  '  while bucket ≠ null và bucket.key ≠ k do prev ← bucket; bucket ← bucket.next',
  '  if bucket = null then return',
  '  if prev = null then table[idx] ← bucket.next else prev.next ← bucket.next',
];

const SCHEMA: InputSchema = {
  kind: 'hashtable',
  fields: [
    { name: 'keys', type: 'int[]', label: 'Dãy khóa', min: -999, max: 999, default: [12, 25, 37, 41, 58], description: 'Các khóa thao tác trên bảng băm (2–50 khóa)' },
    { name: 'tableSize', type: 'int', label: 'Kích thước bảng', min: 5, max: 31, default: 11, description: 'Số bucket (nguyên tố khuyến nghị)' },
    { name: 'hashMode', type: 'select', label: 'Hàm băm', options: [
      { label: 'Modulo (k mod m)', value: 'modulo' },
      { label: 'Nhân (multiplication)', value: 'multiplication' },
    ], default: 'modulo', description: 'Hàm băm dùng để tính chỉ số bucket' },
    { name: 'operation', type: 'select', label: 'Thao tác', options: [
      { label: 'Chèn', value: 'insert' },
      { label: 'Tìm kiếm', value: 'search' },
      { label: 'Xóa', value: 'delete' },
    ], default: 'insert', description: 'Thao tác trên bảng băm' },
  ],
};

const OPERATIONS = ['insert', 'search', 'delete'];

export function createHashInsertGenerator(): SimulationGenerator {
  return buildGenerator('hash.insert', SCHEMA, PSEUDOCODE, { validate: hashValidate, generate: (i) => runHash(i, 'insert') });
}

export function createHashSearchGenerator(): SimulationGenerator {
  return buildGenerator('hash.search', SCHEMA, PSEUDOCODE, { validate: hashValidate, generate: (i) => runHash(i, 'search') });
}

export function createHashDeleteGenerator(): SimulationGenerator {
  return buildGenerator('hash.delete', SCHEMA, PSEUDOCODE, { validate: hashValidate, generate: (i) => runHash(i, 'delete') });
}

function hashValidate(input: InputConfig): { ok: boolean; errors: string[] } {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const errors: string[] = [];
  const keys = intArrayField(rec, 'keys', [12, 25, 37, 41, 58]);
  if (keys.length < 2 || keys.length > 50) errors.push(`keys: phải có 2–50 khóa (hiện có ${keys.length})`);
  keys.forEach((k, i) => {
    if (k < -999 || k > 999) errors.push(`keys[${i}]=${k} phải trong khoảng -999..999`);
  });
  const tableSize = intField(rec, 'tableSize', 11);
  if (tableSize < 5 || tableSize > 31) errors.push(`tableSize: phải trong khoảng 5–31 (hiện tại ${tableSize})`);
  const mode = strField(rec, 'hashMode', 'modulo');
  if (mode !== 'modulo' && mode !== 'multiplication') errors.push(`hashMode: phải là 'modulo' hoặc 'multiplication' (hiện tại '${mode}')`);
  const op = strField(rec, 'operation', 'insert');
  if (!OPERATIONS.includes(op)) errors.push(`operation: phải là một trong ${OPERATIONS.join(', ')} (hiện tại '${op}')`);
  return { ok: errors.length === 0, errors };
}

function runHash(input: InputConfig, _preferred: 'insert' | 'search' | 'delete'): ReturnType<SimulationGenerator['generate']> {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const keys = intArrayField(rec, 'keys', [12, 25, 37, 41, 58]);
  const tableSize = intField(rec, 'tableSize', 11);
  const mode = strField(rec, 'hashMode', 'modulo');
  const op = strField(rec, 'operation', 'insert');
  const trace = new Trace();
  const buckets: number[][] = Array.from({ length: tableSize }, () => []);
  // insert: bảng bắt đầu rỗng rồi chèn từng khóa.
  // search/delete: khởi tạo bảng sẵn các khóa (trạng thái đầu vào) rồi thao tác.
  if (op !== 'insert') {
    for (const key of keys) buckets[hashIndex(key, tableSize, mode)].push(key);
  }
  const statuses: Record<string, ElementStatus> = {};
  const bucketStatuses: Record<number, ElementStatus> = {};

  trace.vars.tableSize = tableSize;
  trace.vars.mode = mode;
  trace.vars.idx = null;
  trace.push({
    line: 1,
    explanation: `Bắt đầu: bảng băm ${tableSize} bucket, hàm băm ${mode === 'modulo' ? 'modulo' : 'nhân'}, thao tác ${op} [${keys.join(', ')}].`,
    structure: hashStructure(tableSize, buckets, statuses, bucketStatuses),
    annotations: [`m=${tableSize}, mode=${mode}`],
  });

  const hashStep = (key: number): number => {
    const idx = hashIndex(key, tableSize, mode);
    trace.vars.idx = idx;
    const formula = mode === 'modulo'
      ? `h(${key}) = ${key} mod ${tableSize} = ${idx}`
      : `h(${key}) = ⌊${tableSize} × frac(${key} × 0.6180339887)⌋ = ${idx}`;
    trace.push({
      line: op === 'insert' ? 2 : op === 'search' ? 8 : 13,
      explanation: `Tính chỉ số bucket cho khóa ${key}: ${formula}.`,
      structure: hashStructure(tableSize, buckets, statuses, { ...bucketStatuses, [idx]: 'active' }),
      annotations: [formula],
    });
    return idx;
  };

  if (op === 'insert') {
    for (const key of keys) {
      const idx = hashStep(key);
      if (buckets[idx].length === 0) {
        buckets[idx].push(key);
        statuses[`node:${key}`] = 'swap';
        trace.push({
          line: 5,
          explanation: `Bucket ${idx} trống → chèn ${key} vào đầu bucket ${idx}.`,
          structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }),
          annotations: [`bucket[${idx}] ← ${key}`],
        });
        statuses[`node:${key}`] = 'done';
      } else {
        bucketStatuses[idx] = 'highlight';
        let chain = '';
        let found = false;
        for (const existing of buckets[idx]) {
          chain += `${existing} → `;
          statuses[`node:${existing}`] = 'active';
          trace.stats.comparisons++;
          trace.push({
            line: 4,
            explanation: `Xung đột tại bucket ${idx}: duyệt chuỗi nối kết, so sánh ${existing} với ${key}.`,
            structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'highlight' }),
            annotations: [`chuỗi bucket ${idx}: ${chain}null`],
          });
          statuses[`node:${existing}`] = 'default';
          if (existing === key) { found = true; break; }
        }
        if (found) {
          trace.push({
            line: 6,
            explanation: `Khóa ${key} đã tồn tại trong bucket ${idx} → cập nhật (không chèn trùng).`,
            structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'highlight' }),
            annotations: ['khóa trùng → cập nhật'],
          });
        } else {
          buckets[idx].push(key);
          statuses[`node:${key}`] = 'swap';
          trace.push({
            line: 5,
            explanation: `Khóa ${key} chưa có → nối vào cuối chuỗi bucket ${idx}.`,
            structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }),
            annotations: [`bucket[${idx}] += ${key}`],
          });
          statuses[`node:${key}`] = 'done';
        }
        bucketStatuses[idx] = 'default';
      }
    }
    trace.push({
      line: 6,
      explanation: `Kết thúc: đã chèn [${keys.join(', ')}] vào bảng băm.`,
      structure: hashStructure(tableSize, buckets, statuses, bucketStatuses),
      annotations: ['hoàn tất chèn'],
    });
    return trace.steps;
  }

  if (op === 'search') {
    for (const key of keys) {
      const idx = hashStep(key);
      if (buckets[idx].length === 0) {
        bucketStatuses[idx] = 'muted';
        trace.push({
          line: 11,
          explanation: `Bucket ${idx} rỗng → không tìm thấy ${key}.`,
          structure: hashStructure(tableSize, buckets, statuses, bucketStatuses),
          annotations: [`Không tìm thấy ${key}`],
        });
        continue;
      }
      let found = false;
      let chain = '';
      for (const existing of buckets[idx]) {
        chain += `${existing} → `;
        statuses[`node:${existing}`] = 'active';
        trace.stats.comparisons++;
        trace.push({
          line: 10,
          explanation: `Tìm ${key}: duyệt chuỗi bucket ${idx}, so sánh ${existing}.`,
          structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }),
          annotations: [`chuỗi bucket ${idx}: ${chain}null`],
        });
        if (existing === key) {
          statuses[`node:${existing}`] = 'done';
          found = true;
          trace.push({
            line: 10,
            explanation: `${existing} = ${key} → Tìm thấy trong bucket ${idx}.`,
            structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }),
            annotations: [`Tìm thấy ${key} ở bucket ${idx}`],
          });
          break;
        }
        statuses[`node:${existing}`] = 'default';
      }
      if (!found) {
        bucketStatuses[idx] = 'muted';
        trace.push({
          line: 11,
          explanation: `Duyệt hết chuỗi bucket ${idx} → không tìm thấy ${key}.`,
          structure: hashStructure(tableSize, buckets, statuses, bucketStatuses),
          annotations: [`Không tìm thấy ${key}`],
        });
      }
    }
    trace.push({
      line: 11,
      explanation: 'Kết thúc: hoàn tất tìm kiếm các khóa.',
      structure: hashStructure(tableSize, buckets, statuses, bucketStatuses),
    });
    return trace.steps;
  }

  // delete
  for (const key of keys) {
    const idx = hashStep(key);
    const chain = buckets[idx];
    const pos = chain.indexOf(key);
    if (pos === -1) {
      bucketStatuses[idx] = 'muted';
      trace.push({
        line: 15,
        explanation: `Không tìm thấy ${key} trong bucket ${idx} → không xóa được.`,
        structure: hashStructure(tableSize, buckets, statuses, bucketStatuses),
        annotations: [`Không tìm thấy ${key}`],
      });
      continue;
    }
    for (let i = 0; i < pos; i++) {
      statuses[`node:${chain[i]}`] = 'active';
      trace.stats.comparisons++;
      trace.push({
        line: 14,
        explanation: `Tìm ${key}: duyệt chuỗi bucket ${idx}, gặp nút ${chain[i]}.`,
        structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }),
        annotations: [`prev=${i > 0 ? chain[i - 1] : 'null'}`],
      });
      statuses[`node:${chain[i]}`] = 'default';
    }
    statuses[`node:${key}`] = 'error';
    trace.push({
      line: 14,
      explanation: `Tìm thấy ${key} tại vị trí ${pos} trong bucket ${idx}.`,
      structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }),
      annotations: [`xóa ${key}`],
    });
    buckets[idx].splice(pos, 1);
    statuses[`node:${key}`] = 'muted';
    trace.push({
      line: 16,
      explanation: `Xóa ${key}: ${pos === 0 ? 'bucket đầu' : `nút ${chain[pos - 1] ?? ''}.next bỏ qua ${key}`} — nối chuỗi lại.`,
      structure: hashStructure(tableSize, buckets, statuses, { [idx]: 'active' }),
      annotations: [`đã xóa ${key}`],
    });
    delete statuses[`node:${key}`];
  }
  trace.push({
    line: 16,
    explanation: 'Kết thúc: hoàn tất xóa các khóa.',
    structure: hashStructure(tableSize, buckets, statuses, bucketStatuses),
  });
  return trace.steps;
}
