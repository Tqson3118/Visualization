// DOM probe — verify vision-model claims (cắt chữ, chồng lấn, overflow) bằng DOM thật
import { chromium } from 'playwright';
const BASE = 'http://localhost:5180';
const browser = await chromium.launch();
const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });

const probe = () => {
  const issues = [];
  const els = [...document.querySelectorAll('input, textarea, button, label, p, h1, h2, h3, span, a, select')];
  for (const el of els) {
    const r = el.getBoundingClientRect();
    if (r.width === 0 || r.height === 0) continue;
    // Tràn ngang viewport
    if (r.right > window.innerWidth + 1 || r.left < -1) {
      issues.push(`tràn ngang: <${el.tagName.toLowerCase()} class="${el.className.toString().slice(0, 60)}"> left=${Math.round(r.left)} right=${Math.round(r.right)}`);
    }
    // Cắt chữ: nội dung dài hơn khung
    if (el.scrollWidth > el.clientWidth + 2 && el.clientWidth > 0) {
      issues.push(`cắt chữ ngang: <${el.tagName.toLowerCase()} class="${el.className.toString().slice(0, 60)}"> scrollW=${el.scrollWidth} clientW=${el.clientWidth} text="${(el.textContent || '').slice(0, 50)}"`);
    }
  }
  // Chồng lấn label+input (kiểm tra cặp gần nhau khó chính xác — bỏ qua, chỉ ghi note)
  const ov = { sw: document.body.scrollWidth, cw: document.body.clientWidth };
  return { issues, overflow: ov.sw > ov.cw ? `OVERFLOW ${ov.sw}>${ov.cw}` : 'no-overflow' };
};

async function run(label, url, setup) {
  await page.goto(`${BASE}${url}`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(800);
  if (setup) await setup(page);
  await page.waitForTimeout(400);
  const res = await page.evaluate(probe);
  console.log(`\n===== ${label} =====`);
  console.log(res.overflow);
  if (res.issues.length === 0) console.log('Không phát hiện tràn ngang / cắt chữ theo DOM');
  else console.log(res.issues.slice(0, 12).join('\n'));
}

// 1. Register student
await run('register-student', '/register');
// 2. Register teacher
await run('register-teacher', '/register', async (p) => {
  await p.getByRole('button', { name: 'Giảng viên', exact: true }).click();
});
// 3. Pending (cần login admin? — pending state cần user mới; bỏ qua UI, dùng nhanh: register lại sẽ báo email tồn tại → không được.
//    Thay vào đó: kiểm tra layout của thông báo qua bước register trong context mới là phức tạp — skip, đã có ảnh + không overflow khi chụp)
await browser.close();
console.log('\nDONE');
