const http = require('http');
const fs = require('fs');
const path = require('path');
const os = require('os');

const PORT = process.env.PORT || 3333;
const STUDY_DIR = path.resolve(__dirname, '..');
const WEB_DIR = __dirname;

const MIME_TYPES = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.js': 'application/javascript; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.md': 'text/markdown; charset=utf-8',
  '.png': 'image/png',
  '.jpg': 'image/jpeg',
  '.svg': 'image/svg+xml',
  '.ico': 'image/x-icon'
};

const DOCS_METADATA = [
  {
    id: '01',
    file: '01_kien_truc_tong_the_va_ha_tang.md',
    title: 'Chặng 1 — Kiến trúc tổng thể & Hạ tầng',
    icon: 'fa-server',
    badge: 'Nền tảng',
    duration: '45 phút',
    desc: 'Clean Architecture .NET 8, Vue 3 SPA, Bootstrap F5, In-memory JWT, Cookie HttpOnly, Pipeline Middleware, Rate Limiting.'
  },
  {
    id: '02',
    file: '02_trai_tim_engine_mo_phong_thuat_toan.md',
    title: 'Chặng 2 — Trái tim Engine mô phỏng',
    icon: 'fa-microchip',
    badge: 'Core Engine',
    duration: '60 phút',
    desc: 'Catalog/Registry, 44 Generators, Pinia VCR Store, 6 Canvas Renderers, Web Worker compile, Sampling 3000 frames.'
  },
  {
    id: '03',
    file: '03_khoa_hoc_bai_hoc_va_teacher_studio.md',
    title: 'Chặng 3 — Khóa học, Bài học & Lớp học',
    icon: 'fa-graduation-cap',
    badge: 'LMS & Studio',
    duration: '45 phút',
    desc: 'Curriculum lộ trình, Vòng đời Lesson (Draft→Active), 3 Sandbox Types (Theory/Quiz/Codelab), Quản lý lớp, Export CSV.'
  },
  {
    id: '04',
    file: '04_code_runner_sandbox_va_benchmark.md',
    title: 'Chặng 4 — Code Runner & Benchmark',
    icon: 'fa-terminal',
    badge: 'Sandbox & Đo đếm',
    duration: '45 phút',
    desc: 'Web Worker client runner, Babel AST instrumentation, Execution Guards (10k/1M/5s), Đo đếm Benchmark Big-O thực nghiệm.'
  },
  {
    id: '05',
    file: '05_gamification_shop_va_kinh_te_ao.md',
    title: 'Chặng 5 — Gamification, Shop & Kinh tế ảo',
    icon: 'fa-trophy',
    badge: 'Gamification',
    duration: '45 phút',
    desc: 'EXP/Level, Daily Quests, Streak, Sổ cái Gems Ledger (Earn - Spend), Shop/Inventory, Sinh mã VietQR EMVCo offline.'
  },
  {
    id: '06',
    file: '06_quan_tri_admin_va_bao_mat.md',
    title: 'Chặng 6 — Quản trị Admin & Bảo mật',
    icon: 'fa-shield-halved',
    badge: 'Security & Admin',
    duration: '40 phút',
    desc: 'Quản trị Users/Content/Stats/Settings, Phòng thủ 4 lớp (JWT, RateLimit, FluentValidation, Ganss.Xss Whitelist 13 tags).'
  },
  {
    id: '07',
    file: '07_so_tay_cau_hoi_van_dap_bao_ve_do_an.md',
    title: 'Chặng 7 — Sổ tay 60+ Câu hỏi Vấn đáp',
    icon: 'fa-book-bookmark',
    badge: 'Vấn đáp & Matrix',
    duration: '90 phút',
    desc: 'Ma trận ánh xạ 24+ luồng dữ liệu FE↔BE↔DB, Bộ 60+ câu hỏi phản biện chuyên sâu kèm đáp án và phân tích Gap học thuật.'
  }
];

function getLocalIPs() {
  const interfaces = os.networkInterfaces();
  const ips = [];
  for (const name of Object.keys(interfaces)) {
    for (const iface of interfaces[name]) {
      if (iface.family === 'IPv4' && !iface.internal) {
        if (!iface.address.startsWith('169.254')) {
          ips.push(iface.address);
        }
      }
    }
  }
  return ips;
}

const server = http.createServer((req, res) => {
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type');

  if (req.method === 'OPTIONS') {
    res.writeHead(204);
    res.end();
    return;
  }

  const parsedUrl = new URL(req.url, `http://${req.headers.host || 'localhost'}`);
  const pathname = decodeURIComponent(parsedUrl.pathname);

  // API danh sách docs & server info
  if (pathname === '/api/docs') {
    res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
    res.end(JSON.stringify({
      docs: DOCS_METADATA,
      serverIps: getLocalIPs(),
      port: PORT
    }));
    return;
  }

  // API đọc nội dung 1 doc
  if (pathname.startsWith('/api/doc/')) {
    const docId = pathname.replace('/api/doc/', '').trim();
    const docMeta = DOCS_METADATA.find(d => d.id === docId || d.file === docId);
    if (!docMeta) {
      res.writeHead(404, { 'Content-Type': 'application/json; charset=utf-8' });
      res.end(JSON.stringify({ error: 'Not found' }));
      return;
    }

    const filePath = path.join(STUDY_DIR, docMeta.file);
    fs.readFile(filePath, 'utf8', (err, content) => {
      if (err) {
        res.writeHead(500, { 'Content-Type': 'application/json; charset=utf-8' });
        res.end(JSON.stringify({ error: 'Cannot read file' }));
        return;
      }
      res.writeHead(200, { 'Content-Type': 'application/json; charset=utf-8' });
      res.end(JSON.stringify({ ...docMeta, content }));
    });
    return;
  }

  // Static files serving
  let staticPath = path.join(WEB_DIR, pathname === '/' ? 'index.html' : pathname);
  if (!fs.existsSync(staticPath) || !fs.statSync(staticPath).isFile()) {
    staticPath = path.join(WEB_DIR, 'index.html');
  }

  const ext = path.extname(staticPath);
  const contentType = MIME_TYPES[ext] || 'text/plain';
  fs.readFile(staticPath, (err, data) => {
    if (err) {
      res.writeHead(404, { 'Content-Type': 'text/plain' });
      res.end('404 Not Found');
      return;
    }
    res.writeHead(200, { 'Content-Type': contentType });
    res.end(data);
  });
});

server.listen(PORT, '0.0.0.0', () => {
  const ips = getLocalIPs();
  console.log('\n======================================================');
  console.log('🚀 VISUALIZATION DSA — STUDY WEB APP ĐANG CHẠY!');
  console.log('======================================================');
  console.log(`💻 Mở trên máy tính:   http://localhost:${PORT}`);
  if (ips.length > 0) {
    ips.forEach((ip, idx) => {
      console.log(`📱 Mở trên điện thoại (${idx + 1}): http://${ip}:${PORT}`);
    });
    console.log(`   (Đảm bảo điện thoại và máy tính kết nối chung mạng Wi-Fi)`);
  }
  console.log('======================================================\n');
});
