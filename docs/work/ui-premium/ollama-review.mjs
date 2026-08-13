// Dev-E2E (UI-PREMIUM Phase 2) — gửi 14 ảnh final tới Ollama qwen2.5vl:3b
// chấm 7 tiêu chí (thẩm mỹ/nhất quán/rõ ràng/phản hồi trực quan/luồng thao tác/
// tiếp cận/thỏa mãn) mỗi trục 1-5. Chuẩn: ≥4/5 mỗi trục, ≤3 trục phải sửa.
import { readFileSync, writeFileSync } from 'fs';

const OLLAMA = 'http://localhost:11434/api/generate';
const MODEL = 'qwen2.5vl:3b';
const OUT = 'D:/FPT/neww/trees/ui-premium/docs/work/ui-premium';

const SCREENS = [
  { file: 'final-01-home.png', ten: 'Trang chủ (hero + stats count-up + demo cards + CTA band)' },
  { file: 'final-02-login.png', ten: 'Đăng nhập (aside canvas-ink + form)' },
  { file: 'final-03-register.png', ten: 'Đăng ký (aside + form vai trò)' },
  { file: 'final-04-premium.png', ten: 'Premium (pricing cards glow + bảng so sánh)' },
  { file: 'final-05-profile-fixed.png', ten: 'Hồ sơ (ProgressRing avatar + stats + timeline block-token)' },
  { file: 'final-06-simulator.png', ten: 'Simulator (canvas tối + mã giả + điều khiển)' },
  { file: 'final-07-404.png', ten: 'Trang 404 (block-token scatter)' },
  { file: 'final-08-coderunner.png', ten: 'Code Runner (editor + chạy)' },
  { file: 'final-09-leaderboard.png', ten: 'Bảng xếp hạng (podium top 3 + rank rows)' },
  { file: 'final-10-quests.png', ten: 'Thử thách (quest cards + progress)' },
  { file: 'final-11-shop.png', ten: 'Cửa hàng (item cards + gems)' },
  { file: 'final-12-benchmark.png', ten: 'Benchmark (hero-stat + lựa chọn thuật toán)' },
  { file: 'final-13-admin-stats.png', ten: 'Admin thống kê (KPI + biểu đồ)' },
  { file: 'final-14-home-mobile.png', ten: 'Trang chủ mobile (360px)' },
];

const CRITERIA = [
  'thẩm mỹ (thiết kế đẹp, cao cấp, không thô sơ)',
  'nhất quán (màu sắc, typography, spacing đồng bộ toàn màn hình)',
  'rõ ràng (thông tin dễ đọc, phân cấp visual hợp lý)',
  'phản hồi trực quan (hover/state/loading có tín hiệu rõ)',
  'luồng thao tác (điều hướng, nút bấm dễ hiểu)',
  'tiếp cận (contrast, kích thước nhấp, a11y)',
  'thỏa mãn (cảm giác chuyên nghiệp, wow factor)',
];

function buildPrompt(ten) {
  return [
    `Đây là ảnh chụp màn hình (1366x768 hoặc mobile) giao diện web ứng dụng học thuật toán DSA Visual — màn hình: ${ten}.`,
    'Bạn là chuyên gia UI/UX review khắt khe. Chấm điểm 7 tiêu chí dưới đây, mỗi tiêu chí 1-5 (1 = rất kém, 5 = xuất sắc):',
    ...CRITERIA.map((c, i) => `${i + 1}. ${c}`),
    'Với MỖI tiêu chí trả lời đúng định dạng: "Tiêu chí X — điểm: Y/5 — lý do 1 câu ngắn".',
    'Sau đó 1 dòng tổng kết: "TỔNG: điểm trung bình Z/5 — mục cần sửa: ..." (nếu trung bình >= 4/5 ghi "KHÔNG CẦN SỬA").',
    'CẤM trả lời chung chung; nêu vị trí cụ thể khi chê. Trả lời tiếng Việt.',
  ].join('\n');
}

const results = [];
for (const s of SCREENS) {
  const imgFile = `${OUT}/${s.file}`;
  const outFile = `${OUT}/ollama-${s.file.replace('.png', '.txt')}`;
  const b64 = readFileSync(imgFile).toString('base64');
  const prompt = buildPrompt(s.ten);

  let response = null;
  for (let attempt = 1; attempt <= 2; attempt++) {
    try {
      const t0 = Date.now();
      const res = await fetch(OLLAMA, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ model: MODEL, prompt, images: [b64], stream: false }),
      });
      const j = await res.json();
      response = j.response;
      const sec = ((Date.now() - t0) / 1000).toFixed(0);
      writeFileSync(outFile, `# Ollama 7 tiêu chí — ${s.ten}\n# Ảnh: ${imgFile}\n# Model: ${MODEL} | ${sec}s\n\n${response ?? '(trống)'}\n`, 'utf8');
      console.log(`OK ${s.file} (${sec}s)`);
      break;
    } catch (e) {
      console.log(`RETRY ${s.file} lần ${attempt}: ${e.message}`);
      if (attempt === 2) {
        writeFileSync(outFile, `# LỖI: ${e.message}`, 'utf8');
        response = `ERROR: ${e.message}`;
      }
      await new Promise((r) => setTimeout(r, 3000));
    }
  }
  results.push({ file: s.file, ten: s.ten, response });
}
writeFileSync(`${OUT}/ollama-summary.json`, JSON.stringify(results, null, 2), 'utf8');
console.log(`\n✅ Xong ${results.length} lượt → ${OUT}/ollama-*.txt`);
