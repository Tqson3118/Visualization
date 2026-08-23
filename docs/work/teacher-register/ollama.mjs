// Ollama vision 7 tiêu chí — docs/work/teacher-register (Task L)
// Chạy: node docs/work/teacher-register/ollama.mjs
import fs from 'node:fs';
import path from 'node:path';

const OUT = path.resolve('docs/work/teacher-register');
const OLLAMA = 'http://localhost:11434/api/generate';
const MODEL = 'qwen2.5vl:3b';

const CRITERIA_PROMPT = `Bạn là chuyên gia đánh giá UI/UX. Hãy:
1) Mô tả ngắn bố cục trang, các phần tử chính (tiêu đề, form, nút, thông báo), màu sắc.
2) Phát hiện lỗi hiển thị: layout lệch, cắt chữ, chồng lấn, contrast kém, chữ nhỏ khó đọc, spacing bất thường.
3) Chấm 7 tiêu chí, mỗi tiêu chí 1-5 điểm (5= xuất sắc, 1= kém):
   UI-1 Thẩm mỹ (màu sắc hài hòa, không chói/mờ, không nhàm)
   UI-2 Nhất quán (spacing/bo góc/typography/màu đồng bộ)
   UI-3 Rõ ràng (hierarchy tiêu đề/body/nút, không chồng lấn, không cắt chữ)
   UI-4 Phản hồi trực quan (hover/focus/loading/empty state, thông báo lỗi rõ)
   UX-5 Luồng thao tác (thao tác chính dễ tìm, ít bước, không bế tắc)
   UX-6 Khả năng tiếp cận (contrast text, kích thước chạm, label rõ, focus ring)
   UX-7 Độ thỏa mãn (cảm giác muốn dùng tiếp, điểm wow)
Mỗi tiêu chí: điểm + 1 dòng nhận xét cụ thể + 1 câu gợi ý sửa. Tiêu chí <=3 điểm PHẢI nêu rõ chỗ chưa đạt. Trả lời tiếng Việt, format từng dòng:
UI-1 Thẩm mỹ: X/5 - nhận xét | gợi ý
UI-2 Nhất quán: X/5 - nhận xét | gợi ý
UI-3 Rõ ràng: X/5 - nhận xét | gợi ý
UI-4 Phản hồi: X/5 - nhận xét | gợi ý
UX-5 Luồng: X/5 - nhận xét | gợi ý
UX-6 Tiếp cận: X/5 - nhận xét | gợi ý
UX-7 Thỏa mãn: X/5 - nhận xét | gợi ý`;


const SCREENS = [
  ['smoke-01-register-student.png', 'Trang Đăng ký tài khoản chế độ SINH VIÊN: form gồm Họ tên, Email, Mật khẩu + checklist mật khẩu, Nhập lại mật khẩu, segmented chọn vai trò Sinh viên/Giảng viên, checkbox đồng ý chính sách, nút Đăng ký.'],
  ['smoke-02-register-teacher.png', 'Trang Đăng ký chế độ GIẢNG VIÊN: segmented đã chọn Giảng viên, form con viền dashed gồm Khoa/Bộ môn, Mã giảng viên, Kinh nghiệm giảng dạy (textarea có đếm ký tự) + ghi chú chờ duyệt.'],
  ['smoke-03-register-pending.png', 'Trang Đăng ký sau khi submit GIẢNG VIÊN: khối thông báo xanh "Đăng ký thành công! Tài khoản giảng viên đang chờ duyệt" + link "Về đăng nhập".'],
  ['smoke-04-register-missing-department.png', 'Trang Đăng ký chế độ GIẢNG VIÊN có lỗi validation: Khoa/Bộ môn và Mã giảng viên bỏ trống, lỗi đỏ hiện dưới field.'],
  ['smoke-05-admin-review-modal.png', 'Màn Admin Quản lý người dùng: tab "Chờ duyệt Teacher", modal duyệt giảng viên hiển thị Thông tin giảng viên (Khoa/Bộ môn, Mã giảng viên, Kinh nghiệm giảng dạy), nút Xác nhận duyệt.'],
  ['smoke-06-teacher-logged-in.png', 'Trang sau khi giảng viên đăng nhập thành công: header có avatar tên "GV Smoke Test", menu điều hướng (Lộ trình, Khám phá, Lớp học, Thử thách, Cửa hàng, Quản trị), nội dung trang Lộ trình học.'],
];

function callOllama(imgB64, prompt) {
  const body = JSON.stringify({ model: MODEL, prompt, images: [imgB64], stream: false, options: { temperature: 0.2 } });
  return fetch(OLLAMA, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body,
    signal: AbortSignal.timeout(180000),
  }).then((r) => r.json());
}

const summary = [];
for (const [file, ctx] of SCREENS) {
  const p = path.join(OUT, file);
  if (!fs.existsSync(p)) { console.log(`SKIP (thiếu ảnh): ${file}`); continue; }
  const imgB64 = fs.readFileSync(p).toString('base64');
  const prompt = `${CRITERIA_PROMPT}\n\nBối cảnh ảnh: ${ctx}\n\nẢnh chụp màn hình:`;
  console.log(`Đang chấm ${file}...`);
  try {
    const res = await callOllama(imgB64, prompt);
    const text = (res.response ?? JSON.stringify(res)).trim();
    fs.writeFileSync(path.join(OUT, `vision-${file.replace('.png', '')}.txt`), text, 'utf8');
    const scores = [...text.matchAll(/(UI-[1-4]|UX-[5-7])[^:]*:\s*(\d)\/5/g)].map((m) => `${m[1]}=${m[2]}`);
    summary.push(`${file}: ${scores.join(' ')}`);
    console.log(`OK ${file} → ${scores.join(' ')}`);
  } catch (e) {
    summary.push(`${file}: LỖI ${e.message}`);
    console.log(`ERR ${file}: ${e.message}`);
  }
}
fs.writeFileSync(path.join(OUT, 'ollama-summary.txt'), summary.join('\n'), 'utf8');
console.log('\n=== TỔNG KẾT ===');
console.log(summary.join('\n'));
