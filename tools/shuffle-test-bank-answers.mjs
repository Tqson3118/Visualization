#!/usr/bin/env node
/**
 * shuffle-test-bank-answers.mjs — Xáo trộn vị trí đáp án A/B/C/D trong ngân hàng đề thi
 * để phân bố chữ cái cân bằng (~25%/chữ), kèm remap tham chiếu chữ cái trong
 * phần giải thích của file đáp án (bỏ qua nội dung trong code fence).
 *
 * Cách chạy:  node tools/shuffle-test-bank-answers.mjs [seed]
 */
import fs from 'node:fs';
import path from 'node:path';

const ROOT = import.meta.dirname ? path.join(import.meta.dirname, '..') : process.cwd();
// DSH_TESTBANK_DIR: ghi đè thư mục đề thi (dùng để dry-run/test trên bản copy)
const DOCS_DIR = process.env.DSH_TESTBANK_DIR
  ? path.resolve(process.env.DSH_TESTBANK_DIR)
  : path.join(ROOT, 'docs/test-bank');
const EXAM = path.join(DOCS_DIR, 'DE_THI_KIEN_THUC_CODEBASE_DSA.md');
const KEY  = path.join(DOCS_DIR, 'DAP_AN_VA_GIAI_THICH_CHI_TIET.md');
const SEED = Number(process.argv[2] ?? 20260827);
const T = String.fromCharCode(96); const FENCE = T + T + T;

function mulberry32(a) {
  return function () {
    a |= 0; a = (a + 0x6D2B79F5) | 0;
    let t = Math.imul(a ^ (a >>> 15), 1 | a);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}
const rng = mulberry32(SEED);

// Parse đáp án đúng từ heading: #### **Câu X.Y — Đáp án: Z**
function parseKeyAnswers(keyText) {
  const map = new Map();
  const re = /^#### \*\*Câu ((?:F\.\d+|\d+\.\d+)) — Đáp án: ([A-D])\*\*/gm;
  let m;
  while ((m = re.exec(keyText)) !== null) map.set(m[1], m[2]);
  return map;
}

// Tìm khối MCQ 4-option trong đề thi
function findMcqBlocks(examLines) {
  const blocks = [];
  const optRe = /^- \*\*([A-D])\.\*\* (.*)$/;
  for (let i = 0; i < examLines.length; i++) {
    const qm = examLines[i].match(/^\*\*Câu ((?:F\.\d+|\d+\.\d+)) /);
    if (!qm) continue;
    const opts = new Map(); const optLineIdx = new Map();
    let j = i + 1;
    // quét tới 12 dòng tới: bỏ qua dòng stem/trống, dừng khi gặp câu hoặc heading mới
    while (j < examLines.length && j <= i + 12 && opts.size < 4) {
      const line = examLines[j];
      if (/^\*\*Câu /.test(line) || /^#{1,6} /.test(line)) break;
      const om = line.match(optRe);
      if (om) { opts.set(om[1], om[2]); optLineIdx.set(om[1], j); }
      j++;
    }
    const keysOk = [...opts.keys()].sort().join('') === 'ABCD';
    if (opts.size === 4 && keysOk) blocks.push({ id: qm[1], opts, optLineIdx });
  }
  return blocks;
}

// Chỉ remap chữ cái đứng độc lập, không phải ký tự đầu/cuối của một từ.
// Dùng Unicode property escapes để bảo vệ cả chữ tiếng Việt có dấu và các
// chữ cái Unicode khác; cách này không phụ thuộc vào danh sách dấu thủ công.
const STANDALONE_LETTER_RE = /(?<![\p{L}\p{N}])([A-D])(?![\p{L}\p{N}#+-])/gu;

// Remap chữ cái A-D đứng độc lập (ngoài code fence)
function remapLetters(text, map) {
  const parts = text.split(new RegExp('(' + FENCE + '[\\s\\S]*?' + FENCE + ')', 'g'));
  return parts.map((p, idx) => (idx % 2 === 1 ? p : p.replace(
    STANDALONE_LETTER_RE,
    (mm, LL) => map[LL] ?? LL,
  ))).join("");
}

function main() {
// Regression guard: Vietnamese/Unicode words must remain unchanged, while
// standalone answer references are remapped.
if (process.env.SHUFFLE_TEST_REGEX === '1') {
  const sample = 'Các Câu Bài Cờ Bác Cần A, B và D';
  const result = remapLetters(sample, { A: 'D', B: 'A', D: 'C' });
  if (result !== 'Các Câu Bài Cờ Bác Cần D, A và C') {
    throw new Error('STANDALONE_LETTER_RE regression: ' + result);
  }
  console.log('Regex regression: PASS');
  return;
}

  const examText = fs.readFileSync(EXAM, 'utf8');
  const keyText = fs.readFileSync(KEY, 'utf8');
  const answers = parseKeyAnswers(keyText);
  const examLines = examText.split('\n');
  const blocks = findMcqBlocks(examLines);
  const counts = { A: 0, B: 0, C: 0, D: 0 }; const before = { A: 0, B: 0, C: 0, D: 0 };
  const skipped = []; const qMaps = new Map();
  for (const b of blocks) {
    const correct = answers.get(b.id);
    if (!correct) { skipped.push(b.id + '(no-key)'); continue; }
    before[correct]++;
    const letters = ['A', 'B', 'C', 'D'];
    letters.sort((x, y) => counts[x] - counts[y] || rng() - 0.5);
    const target = letters[0]; // chữ ít được dùng nhất
    const others = letters.slice(1).sort(() => rng() - 0.5);
    const map = { [correct]: target };
    let k = 0;
    for (const L of letters) if (L !== correct) map[L] = others[k++];
    for (const L of letters) {
      const newPos = map[L];
      examLines[b.optLineIdx.get(newPos)] = '- **' + newPos + '.** ' + b.opts.get(L);
    }
    counts[target]++; qMaps.set(b.id, map);
  }
  fs.writeFileSync(EXAM, examLines.join('\n'), 'utf8');
  // Đổi chữ đáp án trong heading key
  let newKey = keyText.replace(
    /^(#### \*\*Câu ((?:F\.\d+|\d+\.\d+)) — Đáp án: [A-D]\*\*)$/gm,
    (full, head, id) => {
      const m = qMaps.get(id); const old = answers.get(id);
      return m ? head.replace(/Đáp án: [A-D]/, 'Đáp án: ' + m[old]) : full;
    },
  );
  // Xác định block giải thích: từ heading MCQ tới heading/--- kế tiếp
  const headingRe = /^(#{1,6} .*)$/gm;
  const sepRe = /^---$/gm;
  const stops = [];
  let hm;
  while ((hm = headingRe.exec(newKey)) !== null) stops.push(hm.index);
  while ((hm = sepRe.exec(newKey)) !== null) stops.push(hm.index);
  stops.sort((a, b) => a - b);
  const mcqRe = /^#### \*\*Câu ((?:F\.\d+|\d+\.\d+)) — Đáp án: ([A-D])\*\*/gm;
  const segs = [];
  while ((hm = mcqRe.exec(newKey)) !== null) {
    const start = hm.index + hm[0].length;
    const nextStop = stops.find((s) => s > start);
    segs.push({ id: hm[1], start, end: nextStop ?? newKey.length });
  }
  let offset = 0;
  for (const sg of segs) {
    const m = qMaps.get(sg.id); if (!m) continue;
    const s = sg.start + offset, e = sg.end + offset;
    const seg = newKey.slice(s, e);
    const seg2 = remapLetters(seg, m);
    newKey = newKey.slice(0, s) + seg2 + newKey.slice(e);
    offset += seg2.length - seg.length;
  }
  fs.writeFileSync(KEY, newKey, 'utf8');
  const total = Object.values(counts).reduce((a, b) => a + b, 0);
  console.log('Seed:', SEED, '| shuffled:', total);
  console.log('BEFORE:', JSON.stringify(before));
  console.log('AFTER: ', JSON.stringify(counts),
    Object.fromEntries(Object.entries(counts).map(([k, v]) => [k, total ? (100 * v / total).toFixed(1) + '%' : '0%'])));
  if (skipped.length) console.log('SKIPPED:', skipped.join(', '));
}
main();