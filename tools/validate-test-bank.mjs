#!/usr/bin/env node
/** Validate the test-bank contract used by CI. */
import fs from 'node:fs';
import path from 'node:path';

const root = import.meta.dirname ? path.join(import.meta.dirname, '..') : process.cwd();
const examPath = path.join(root, 'docs/test-bank/DE_THI_KIEN_THUC_CODEBASE_DSA.md');
const keyPath = path.join(root, 'docs/test-bank/DAP_AN_VA_GIAI_THICH_CHI_TIET.md');
const exam = fs.readFileSync(examPath, 'utf8');
const key = fs.readFileSync(keyPath, 'utf8');
const failures = [];
const assert = (condition, message) => { if (!condition) failures.push(message); };

const mcq = [...key.matchAll(/^#### \*\*Câu ((?:F\.\d+|\d+\.\d+)) — Đáp án: ([A-D])\*\*$/gm)];
assert(mcq.length === 105, 'Expected 105 MCQ answer headings, found ' + mcq.length);
assert(exam.includes('# 📚 KIẾN THỨC NỀN & BẢNG THUẬT NGỮ'), 'Missing beginner glossary section in exam');
assert(exam.includes('## 🌐 B. Thuật ngữ Web & HTTP'), 'Missing HTTP glossary section');
assert(exam.includes('## 🖼️ C. Thuật ngữ Frontend'), 'Missing frontend glossary section');
assert(exam.includes('## ⚙️ D. Thuật ngữ Backend'), 'Missing backend glossary section');

for (const match of mcq) {
  if (!/^(?:[1-9]|1[0-3])\./.test(match[1])) continue;
  const start = match.index + match[0].length;
  const next = key.slice(start).search(/^#### \*\*Câu |^# 📘 ĐÁP ÁN ĐỀ /m);
  const block = key.slice(start, next < 0 ? key.length : start + next);
  for (const marker of ['① Khái niệm', '② Vì sao', '③ Áp dụng sâu', '④ Tại sao A/B/C/D sai', '⑤ Code thật']) {
    assert(block.includes(marker), 'Câu ' + match[1] + ' missing layer: ' + marker);
  }
}

if (failures.length) {
  console.error(failures.map((f) => '✗ ' + f).join('\n'));
  process.exit(1);
}
console.log('Test-bank validation: PASS (' + mcq.length + ' MCQ; glossary; five-layer explanations 01-13)');
