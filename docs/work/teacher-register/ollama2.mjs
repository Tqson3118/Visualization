// Ollama vision v2 — prompt nghiêm ngặt để lấy đủ 7 tiêu chí dạng cố định
import fs from 'node:fs';
import path from 'node:path';

const OUT = path.resolve('docs/work/teacher-register');
const OLLAMA = 'http://localhost:11434/api/generate';
const MODEL = 'qwen2.5vl:3b';

const STRICT = `Chấm điểm UI/UX cho ảnh màn hình web này. Phát hiện lỗi hiển thị thực sự: cắt chữ, chồng lấn, layout lệch, contrast kém, chữ quá nhỏ khó đọc.
Trả lời ĐÚNG format sau, 8 dòng, KHÔNG thêm dòng nào khác:
TÓM TẮT: <1-2 câu mô tả ngắn trang và lỗi hiển thị nếu có, nếu không có lỗi ghi "không phát hiện lỗi hiển thị rõ ràng">
UI-1 Thẩm mỹ: <1-5>/5 - <nhận xét 1 câu> | <gợi ý 1 câu>
UI-2 Nhất quán: <1-5>/5 - <nhận xét 1 câu> | <gợi ý 1 câu>
UI-3 Rõ ràng: <1-5>/5 - <nhận xét 1 câu> | <gợi ý 1 câu>
UI-4 Phản hồi trực quan: <1-5>/5 - <nhận xét 1 câu> | <gợi ý 1 câu>
UX-5 Luồng thao tác: <1-5>/5 - <nhận xét 1 câu> | <gợi ý 1 câu>
UX-6 Tiếp cận: <1-5>/5 - <nhận xét 1 câu> | <gợi ý 1 câu>
UX-7 Thỏa mãn: <1-5>/5 - <nhận xét 1 câu> | <gợi ý 1 câu>
Không viết gì khác ngoài 8 dòng trên.`;

const SCREENS = [
  ['smoke-01-register-student.png', 'Trang Đăng ký tài khoản chế độ SINH VIÊN (DSA-Visual): tiêu đề, form Họ tên/Email/Mật khẩu/checklist mật khẩu/Nhập lại mật khẩu, segmented chọn vai trò Sinh viên/Giảng viên, checkbox đồng ý chính sách, nút Đăng ký.'],
  ['smoke-02-register-teacher.png', 'Trang Đăng ký chế độ GIẢNG VIÊN: segmented đã chọn Giảng viên, form con viền dashed Khoa/Bộ môn + Mã giảng viên + Kinh nghiệm giảng dạy (textarea đếm ký tự) + ghi chú chờ duyệt.'],
  ['smoke-03-register-pending.png', 'Trang Đăng ký sau submit GIẢNG VIÊN: khối thông báo xanh "Đăng ký thành công! Tài khoản giảng viên đang chờ duyệt" + link "Về đăng nhập".'],
  ['smoke-04-register-missing-department.png', 'Trang Đăng ký GIẢNG VIÊN lỗi validation: Khoa/Bộ môn và Mã giảng viên bỏ trống, có lỗi đỏ dưới field.'],
  ['smoke-05-admin-review-modal.png', 'Màn admin Quản lý người dùng: tab "Chờ duyệt Teacher", modal "Duyệt giảng viên" hiện Thông tin giảng viên (Khoa/Bộ môn, Mã giảng viên, Kinh nghiệm giảng dạy), nút Xác nhận duyệt/Từ chối.'],
  ['smoke-06-teacher-logged-in.png', 'Trang sau khi giảng viên đăng nhập: header avatar "GV Smoke Test", menu Lộ trình/Khám phá/Lớp học/Thử thách/Cửa hàng/Quản trị, nội dung trang Lộ trình học.'],
];

function callOllama(imgB64, prompt) {
  const body = JSON.stringify({ model: MODEL, prompt, images: [imgB64], stream: false, options: { temperature: 0.1 } });
  return fetch(OLLAMA, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body, signal: AbortSignal.timeout(180000) }).then((r) => r.json());
}

const summary = [];
for (const [file, ctx] of SCREENS) {
  const p = path.join(OUT, file);
  if (!fs.existsSync(p)) { console.log(`SKIP ${file}`); continue; }
  const imgB64 = fs.readFileSync(p).toString('base64');
  const prompt = `${STRICT}\n\nBối cảnh: ${ctx}\n\nẢnh:`;
  console.log(`Chấm ${file}...`);
  try {
    const res = await callOllama(imgB64, prompt);
    const text = (res.response ?? JSON.stringify(res)).trim();
    fs.writeFileSync(path.join(OUT, `vision2-${file.replace('.png', '')}.txt`), text, 'utf8');
    const scores = [...text.matchAll(/(UI-[1-4]|UX-[5-7])[^:]*:\s*(\d)\/5/g)].map((m) => `${m[1]}=${m[2]}`);
    summary.push(`${file}: ${scores.length === 7 ? scores.join(' ') : 'FORMAT KHÔNG ĐỦ (' + scores.join(' ') + ')'}`);
    console.log(`OK ${file}: ${scores.join(' ') || '(không parse được điểm)'}`);
  } catch (e) {
    summary.push(`${file}: LỖI ${e.message}`);
    console.log(`ERR ${file}: ${e.message}`);
  }
}
fs.writeFileSync(path.join(OUT, 'ollama-summary2.txt'), summary.join('\n'), 'utf8');
console.log('\n=== TỔNG KẾT ===');
console.log(summary.join('\n'));
