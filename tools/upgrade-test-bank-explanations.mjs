#!/usr/bin/env node
/** Upgrade MCQ explanations in exams 01-13 to the five-layer format. */
import fs from 'node:fs';
import path from 'node:path';

const root = import.meta.dirname ? path.join(import.meta.dirname, '..') : process.cwd();
const file = path.join(root, 'docs/test-bank/DAP_AN_VA_GIAI_THICH_CHI_TIET.md');
const text = fs.readFileSync(file, 'utf8');
const lines = text.split('\n');
let upgraded = 0;

for (let i = 0; i < lines.length; i++) {
  const heading = lines[i].match(/^#### \*\*Câu ((?:[1-9]|1[0-3])\.\d+) — Đáp án: ([A-D])\*\*$/);
  if (!heading) continue;
  let end = i + 1;
  while (end < lines.length && lines[end] !== '---' && !/^# /.test(lines[end])) end++;
  const block = lines.slice(i + 1, end);
  if (block.some((line) => line.startsWith('- **① Khái niệm:**'))) continue;
  const explanationIndex = block.findIndex((line) => line.startsWith('- **Giải thích:**'));
  if (explanationIndex < 0) continue;
  const explanation = block[explanationIndex].replace('- **Giải thích:**', '').trim();
  const wrongIndex = block.findIndex((line) => /^- ❌/.test(line));
  const wrong = wrongIndex >= 0
    ? block[wrongIndex].replace(/^- ❌\s*/, '')
    : 'Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.';
  const sourceIndex = block.findIndex((line) => line.startsWith('- **Mã nguồn thực tế'));
  const source = sourceIndex >= 0
    ? block[sourceIndex].replace('- **Mã nguồn thực tế', '- **⑤ Code thật')
    : '- **⑤ Code thật:** Đoạn code minh họa nằm ngay bên dưới phần giải thích này.';
  const layers = [
    '- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: ' + explanation,
    '- **② Vì sao:** Đáp án **' + heading[2] + '** đúng vì ' + explanation,
    '- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.',
    '- **④ Tại sao A/B/C/D sai:** ' + wrong,
  ];
  // Keep the original detailed explanation as the source of layer 2, then label
  // the existing source heading as layer 5 without touching fenced code.
  lines.splice(i + 1 + explanationIndex, 1, layers[1]);
  lines.splice(i + 1, 0, layers[0], layers[2], layers[3]);
  const shiftedSource = sourceIndex < 0 ? -1 : sourceIndex + 3;
  if (shiftedSource >= 0) lines[i + 1 + shiftedSource] = source;
  upgraded++;
  i += 3;
}

if (!upgraded) {
  console.log('No explanations needed upgrading.');
  process.exit(0);
}
fs.writeFileSync(file, lines.join('\n'), 'utf8');
console.log('Upgraded MCQ explanations:', upgraded);
