#!/usr/bin/env node
/**
 * generate-test-bank-mapping.mjs — Sinh phần thống kê & báo cáo drift của
 * docs/test-bank/MAPPING_VA_PHAN_TICH_GAP.md trực tiếp từ codebase thật.
 *
 * Script KHÔNG đụng tới các phần phán đoán của người (coverage mục 2/3, bảng
 * mapping mục 4, khuyến nghị mục 5). Nó chỉ:
 *   1. Đếm lại số lượng thật (views / stores / controllers / services /
 *      composables) và cập nhật cột "Số lượng thực tế" trong bảng mục 1.
 *   2. Kiểm tra mọi file được nêu trong bảng mapping (mục 4) + mọi tham chiếu
 *      `path/to/file.ext` (trong backtick) của 2 file đề thi & chính file
 *      mapping còn tồn tại trong codebase — drift detection khi codebase
 *      đổi tên / xoá / dời file.
 *   3. Ghi / thay mới phần "1.5 🤖 BÁO CÁO DRIFT" giữa 2 marker AUTO trong
 *      file mapping (chạy lại nhiều lần vẫn idempotent).
 *
 * Cách chạy:
 *   node tools/generate-test-bank-mapping.mjs           # ghi cập nhật vào file mapping
 *   node tools/generate-test-bank-mapping.mjs --check   # chỉ in báo cáo, không ghi (CI)
 *
 * Exit code: 0 = sạch drift · 1 = có tham chiếu hỏng (CI fail)
 */
import fs from 'node:fs';
import path from 'node:path';

const ROOT = import.meta.dirname ? path.join(import.meta.dirname, '..') : process.cwd();
const MAPPING = path.join(ROOT, 'docs/test-bank/MAPPING_VA_PHAN_TICH_GAP.md');
const EXAM = path.join(ROOT, 'docs/test-bank/DE_THI_KIEN_THUC_CODEBASE_DSA.md');
const KEY = path.join(ROOT, 'docs/test-bank/DAP_AN_VA_GIAI_THICH_CHI_TIET.md');
const CHECK_ONLY = process.argv.includes('--check');

// ---------------------------------------------------------------------------
// 1. Quét codebase → chỉ mục basename / stem → đường dẫn tương đối (posix)
// ---------------------------------------------------------------------------
const SRC_DIRS = [
  'frontend/src',
  'backend/src/DsaVisual.Api',
  'backend/src/DsaVisual.Application',
];
const EXTS = new Set(['.vue', '.ts', '.js', '.mjs', '.cs', '.json']);
const SKIP_DIRS = new Set(['node_modules', 'obj', 'bin', 'dist', '.git']);

function walk(dir, out = []) {
  let entries;
  try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch { return out; }
  for (const e of entries) {
    if (e.isDirectory()) {
      if (!SKIP_DIRS.has(e.name)) walk(path.join(dir, e.name), out);
    } else if (EXTS.has(path.extname(e.name))) {
      out.push(path.join(dir, e.name));
    }
  }
  return out;
}

const files = SRC_DIRS
  .flatMap((d) => walk(path.join(ROOT, d)))
  .map((p) => path.relative(ROOT, p).split(path.sep).join('/'));

const byBasename = new Map();
const byStem = new Map();
for (const f of files) {
  const base = f.split('/').pop();
  const stem = base.slice(0, base.length - path.extname(base).length);
  if (!byBasename.has(base)) byBasename.set(base, []);
  byBasename.get(base).push(f);
  if (!byStem.has(stem)) byStem.set(stem, []);
  byStem.get(stem).push(f);
}

const counts = {
  views: files.filter((f) => f.startsWith('frontend/src/views/') && f.endsWith('.vue')).length,
  stores: files.filter((f) => /^frontend\/src\/stores\/[^/]+\.ts$/.test(f)).length,
  controllers: files.filter((f) => /^backend\/src\/DsaVisual\.Api\/Controllers\/[^/]+\.cs$/.test(f)).length,
  services: files.filter((f) => f.startsWith('backend/src/DsaVisual.Application/Services/') && f.endsWith('.cs')).length,
  composables: files.filter((f) => /^frontend\/src\/composables\/[^/]+\.ts$/.test(f)).length,
};

// ---------------------------------------------------------------------------
// 2. Đối chiếu bảng mapping mục 4 — token từng cell theo cột
// ---------------------------------------------------------------------------
const mappingText = fs.readFileSync(MAPPING, 'utf8');

const COL_RULES = [
  { idx: 3, name: 'Views', ext: '.vue' },
  { idx: 4, name: 'Stores (FE)', ext: '.ts' },
  { idx: 5, name: 'Controllers (BE)', ext: '.cs', iface: true },
  { idx: 6, name: 'Services (BE)', ext: '.cs', iface: true },
  { idx: 7, name: 'Composables', ext: '.ts' },
];

function cellTokens(cell) {
  let c = cell.replace(/`/g, '').trim();
  if (/^[—–-]$/.test(c)) return [];
  if (/tổng hợp|cross-cutting|no view/i.test(c)) return [];
  let inner;
  const wrap = c.match(/^\(([\s\S]*)\)$/); // cell thuần trong ngoặc: (a, b, ...)
  if (wrap) inner = wrap[1];
  else inner = c.replace(/\s*\([^)]*\)\s*$/, ''); // bỏ annotation: "ui.ts (router)"
  return inner.split(',').map((t) => t.trim()).filter(Boolean);
}

function resolveToken(tok, rule) {
  const t = tok.replace(/:\d+([,\d-]*)?$/, '').trim();
  if (!t || t.includes('*') || t.includes('...')) return true; // wildcard / rỗng → bỏ qua
  if (/^[—–-]+$/.test(t)) return true; // gạch ngang (cell "không có gì") → bỏ qua
  if (t.startsWith('/')) return true; // route, không phải file
  if (/\.(vue|ts|js|mjs|cs|json)$/.test(t)) return byBasename.has(t.split('/').pop());
  if (byStem.has(t)) return true;
  if (byBasename.has(t + rule.ext)) return true;
  if (rule.iface && (byBasename.has('I' + t + '.cs') || byStem.has('I' + t))) return true;
  return false;
}

const tableDrift = [];
for (const line of mappingText.split('\n')) {
  if (!/^\|\s*(?:\d{2}|Final)\s*\|/.test(line)) continue;
  const cells = line.split('|').map((c) => c.trim()); // cells[0] rỗng, cells[1]=Đề, cells[2]=Module
  const de = cells[1];
  for (const rule of COL_RULES) {
    const cell = cells[rule.idx];
    if (!cell) continue;
    for (const tok of cellTokens(cell)) {
      if (!resolveToken(tok, rule)) {
        tableDrift.push({ where: 'Mục 4 · ' + de + ' · ' + rule.name, ref: tok });
      }
    }
  }
}

// ---------------------------------------------------------------------------
// 3. Quét tham chiếu backtick trong 3 file md (đề / đáp án / mapping)
// ---------------------------------------------------------------------------
// Token trông như đường dẫn file: chữ/số/gạch/_/./ - có đuôi mở rộng quen thuộc,
// có thể kèm hậu tố :dòng hoặc :dòng-dòng hoặc :dòng,dòng
const REF_RE = /^[\w.\/\\-]+\.(vue|ts|js|mjs|cs|json)(:\d+([,\d-]*)?)?$/;

// Distractor CỐ Ý trong option sai của đề thi — tham chiếu file không tồn tại
// là chủ đích sư phạm (ứng viên phải biết codebase để nhận ra). KHÔNG phải drift.
const INTENTIONAL_FAKE_REFS = new Set(['MiniQuiz.vue']);

function scanRefs(text, label) {
  const bad = [];
  const lines = text.split('\n');
  let exam = '';
  for (let i = 0; i < lines.length; i++) {
    const h = lines[i].match(/^#{1,2}\s+.*(?:ĐỀ|ĐÁP ÁN ĐỀ)\s*(\d{2}|FINAL)/i);
    if (h) exam = h[0].replace(/^#*\s*/, '');
    for (const m of lines[i].matchAll(/`([^`\n]+)`/g)) {
      const raw = m[1].trim();
      if (!REF_RE.test(raw)) continue;
      const cleaned = raw.replace(/:\d+([,\d-]*)?$/, '').split('\\').join('/');
      if (INTENTIONAL_FAKE_REFS.has(raw)) continue;
      if (/^(frontend|backend|shared|docs|tools)\//.test(cleaned)) {
        if (fs.existsSync(path.join(ROOT, cleaned))) continue;
        bad.push({ where: label + ' · ' + (exam || 'đầu file') + ' · dòng ' + (i + 1), ref: raw });
        continue;
      }
      const base = cleaned.split('/').pop();
      if (byBasename.has(base)) continue;
      if (fs.existsSync(path.join(ROOT, cleaned))) continue;
      bad.push({ where: label + ' · ' + (exam || 'đầu file') + ' · dòng ' + (i + 1), ref: raw });
    }
  }
  return bad;
}

const [examText, keyText] = [EXAM, KEY].map((p) => fs.readFileSync(p, 'utf8'));
const refDrift = [
  ...scanRefs(examText, 'Đề thi'),
  ...scanRefs(keyText, 'Đáp án'),
  ...scanRefs(mappingText, 'Mapping'),
];

// ---------------------------------------------------------------------------
// 4. Gộp báo cáo drift
// ---------------------------------------------------------------------------
const allDrift = [...tableDrift, ...refDrift];
const now = new Date();
const pad = (n) => String(n).padStart(2, '0');
const stamp = now.getFullYear() + '-' + pad(now.getMonth() + 1) + '-' + pad(now.getDate());

let reportBody;
if (allDrift.length === 0) {
  reportBody = [
    '**Kết quả:** ✅ Không phát hiện drift — mọi file được tham chiếu trong bảng mục 4 và trong các file đề thi đều tồn tại trong codebase.',
  ];
} else {
  reportBody = [
    '**Kết quả:** ⚠️ Phát hiện **' + allDrift.length + '** tham chiếu hỏng (file bị đổi tên / xoá / dời, hoặc ghi sai tên):',
    '',
    '| Vị trí | Tham chiếu |',
    '|---|---|',
    ...allDrift.slice(0, 60).map((d) => '| ' + d.where + ' | `' + d.ref + '` |'),
  ];
  if (allDrift.length > 60) reportBody.push('', '*…còn ' + (allDrift.length - 60) + ' dòng khác (xem console).*');
}

const block = [
  '<!-- AUTO:DRIFT:START -->',
  '## 1.5 🤖 BÁO CÁO DRIFT (tự sinh — đừng chỉnh tay)',
  '',
  '> Sinh ngày ' + stamp + ' bởi `tools/generate-test-bank-mapping.mjs`. Chạy lại script này sau khi codebase đổi để phát hiện tham chiếu hỏng.',
  '',
  '**Thống kê quét:** ' + counts.views + ' views · ' + counts.stores + ' stores · ' + counts.controllers + ' controllers · ' + counts.services + ' services · ' + counts.composables + ' composables · tổng ' + files.length + ' file nguồn được đánh chỉ mục.',
  '',
  ...reportBody,
  '<!-- AUTO:DRIFT:END -->',
].join('\n');

// ---------------------------------------------------------------------------
// 5. Cập nhật file mapping (stats + date + block drift giữa marker)
// ---------------------------------------------------------------------------
function updateMapping(text) {
  let out = text;

  // 5a. Cập nhật cột "Số lượng thực tế" (chỉ thay con số đầu tiên của dòng)
  const statRows = [
    ['| Views (.vue) |', counts.views],
    ['| Stores (.ts) |', counts.stores],
    ['| Controllers (.cs) |', counts.controllers],
    ['| Services (.cs) |', counts.services],
    ['| Composables (.ts) |', counts.composables],
  ];
  for (const [prefix, val] of statRows) {
    const line = out.split('\n').find((l) => l.startsWith(prefix));
    if (line) out = out.replace(line, line.replace(/(\|\s*)\d+/, '$1' + val));
  }

  // 5b. Cập nhật dòng ngày (idempotent — luôn viết lại cùng một nội dung)
  out = out.replace(
    /^> \*\*Cập nhật:\*\*.*$/m,
    '> **Cập nhật:** 2026-08-27 (thẩm định + vá errata) · thống kê & drift tự sinh ' + stamp + ' bởi `tools/generate-test-bank-mapping.mjs`.',
  );

  // 5c. Chèn / thay block drift giữa 2 marker
  if (out.includes('<!-- AUTO:DRIFT:START -->') && out.includes('<!-- AUTO:DRIFT:END -->')) {
    out = out.replace(
      /<!--[\s!]*AUTO:DRIFT:START[\s!]*-->[\s\S]*?<!--[\s!]*AUTO:DRIFT:END[\s!]*-->/,
      block,
    );
  } else {
    // chèn trước "## 2." (ngay sau --- ngăn cách)
    out = out.replace(/\n---\n\n## 2\./, '\n' + block + '\n\n---\n\n## 2\.');
  }
  return out;
}

// ---------------------------------------------------------------------------
// 6. Xuất kết quả
// ---------------------------------------------------------------------------
console.log('=== generate-test-bank-mapping ===');
console.log('Views:', counts.views, '| Stores:', counts.stores, '| Controllers:', counts.controllers,
  '| Services:', counts.services, '| Composables:', counts.composables, '| Indexed files:', files.length);
if (tableDrift.length) {
  console.log('\n[Mapping mục 4] Tham chiếu hỏng:');
  for (const d of tableDrift) console.log('  -', d.where, '→', d.ref);
}
if (refDrift.length) {
  console.log('\n[File .md] Tham chiếu hỏng:');
  for (const d of refDrift) console.log('  -', d.where, '→', d.ref);
}
if (!tableDrift.length && !refDrift.length) console.log('Drift: ✅ không có tham chiếu hỏng.');

if (!CHECK_ONLY) {
  const updated = updateMapping(mappingText);
  if (updated !== mappingText) {
    fs.writeFileSync(MAPPING, updated, 'utf8');
    console.log('\nĐã cập nhật:', path.relative(ROOT, MAPPING));
  } else {
    console.log('\nKhông có gì thay đổi trong file mapping.');
  }
} else {
  console.log('\n--check: KHÔNG ghi file.');
}

process.exit(allDrift.length ? 1 : 0);
