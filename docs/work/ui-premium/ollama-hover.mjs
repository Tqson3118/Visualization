// Ollama hover-state review — chấm lại trục tương tác bằng ảnh CÓ hover
// (trước đó model chê "không có hover" vì ảnh tĩnh không thể hiện).
import { readFileSync, writeFileSync } from 'fs';

const OLLAMA = 'http://localhost:11434/api/generate';
const MODEL = 'qwen2.5vl:3b';
const OUT = 'D:/FPT/neww/trees/ui-premium/docs/work/ui-premium';

const SCREENS = [
  { file: 'hover-01-home-card.png', ten: 'Trang chủ — card demo ĐANG HOVER (nổi lên 2px, border đậm hơn, glow icon)' },
  { file: 'hover-02-home-cta.png', ten: 'Trang chủ — CTA "Bắt đầu ngay" ĐANG HOVER (press scale + glow ring)' },
];

const prompt = [
  'Đây là ảnh chụp màn hình có TRẠNG THÁI HOVER đang được thể hiện (card nổi lên, glow, press).',
  'Bạn là chuyên gia UI/UX review. Chấm 3 tiêu chí mỗi cái 1-5 (1 rất kém, 5 xuất sắc):',
  '1. phản hồi trực quan (hover/state có tín hiệu rõ không)',
  '2. thẩm mỹ (thiết kế cao cấp)',
  '3. thỏa mãn (wow factor, chuyên nghiệp)',
  'Mỗi tiêu chí đúng 1 dòng: "Tiêu chí X — điểm: Y/5 — lý do ngắn cụ thể".',
  'Dòng cuối: "TỔNG: trung bình Z/5".',
  'Trả lời tiếng Việt, ngắn gọn.',
].join('\n');

const results = [];
for (const s of SCREENS) {
  const b64 = readFileSync(`${OUT}/${s.file}`).toString('base64');
  const res = await fetch(OLLAMA, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ model: MODEL, prompt: `${prompt}\nMàn hình: ${s.ten}`, images: [b64], stream: false }),
  });
  const j = await res.json();
  const text = `# Ollama hover review — ${s.ten}\n# Ảnh: ${s.file}\n# Model: ${MODEL}\n\n${j.response ?? '(trống)'}\n`;
  writeFileSync(`${OUT}/ollama-hover-${s.file.replace('.png', '.txt')}`, text, 'utf8');
  console.log(`OK ${s.file}: ${(j.response || '').split('\n').pop()}`);
  results.push({ file: s.file, response: j.response });
}
writeFileSync(`${OUT}/ollama-hover-summary.json`, JSON.stringify(results, null, 2), 'utf8');
