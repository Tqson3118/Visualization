const fs = require('fs');
const path = require('path');

const STUDY_DIR = path.resolve(__dirname, '..');
const WEB_DIR = __dirname;

const DOCS_METADATA = [
  {
    id: '01',
    file: '01_kien_truc_tong_the_va_ha_tang.md',
    title: 'Chặng 1 — Kiến trúc tổng thể & Hạ tầng',
    icon: 'fa-server',
    badge: 'Nền tảng .NET 10 & Vue 3',
    color: 'from-blue-500 to-cyan-500',
    duration: '45 phút',
    desc: 'Clean Architecture .NET 10, Vue 3 SPA, Bootstrap F5, In-memory JWT, Cookie HttpOnly, Pipeline Middleware, Rate Limiting.'
  },
  {
    id: '02',
    file: '02_trai_tim_engine_mo_phong_thuat_toan.md',
    title: 'Chặng 2 — Trái tim Engine mô phỏng',
    icon: 'fa-microchip',
    badge: 'Core Engine (44 Generators)',
    color: 'from-emerald-500 to-teal-500',
    duration: '60 phút',
    desc: 'Catalog/Registry, 44 Generators, Pinia VCR Store, 6 Canvas Renderers, Web Worker compile, Sampling 3000 frames.'
  },
  {
    id: '03',
    file: '03_khoa_hoc_bai_hoc_va_teacher_studio.md',
    title: 'Chặng 3 — Khóa học, Bài học & Lớp học',
    icon: 'fa-graduation-cap',
    badge: 'LMS & Codelab Sandbox',
    color: 'from-violet-500 to-purple-500',
    duration: '50 phút',
    desc: 'Curriculum lộ trình, Vòng đời Lesson (Draft→Active), 3 Sandbox Types (Theory/Quiz/Codelab), Quản lý lớp, Export CSV.'
  },
  {
    id: '04',
    file: '04_code_runner_sandbox_va_benchmark.md',
    title: 'Chặng 4 — Code Runner & Benchmark',
    icon: 'fa-terminal',
    badge: 'Sandbox & Đo đếm Big-O',
    color: 'from-amber-500 to-orange-500',
    duration: '45 phút',
    desc: 'Web Worker client runner, Babel AST instrumentation, Execution Guards (10k/1M/5s), Đo đếm Benchmark Big-O thực nghiệm.'
  },
  {
    id: '05',
    file: '05_gamification_shop_va_kinh_te_ao.md',
    title: 'Chặng 5 — Gamification, Shop & Kinh tế ảo',
    icon: 'fa-trophy',
    badge: 'Gamification & VietQR',
    color: 'from-pink-500 to-rose-500',
    duration: '50 phút',
    desc: 'EXP/Level, Daily Quests, Streak, Sổ cái Gems Ledger (Earn - Spend), Shop/Inventory, Sinh mã VietQR EMVCo offline.'
  },
  {
    id: '06',
    file: '06_quan_tri_admin_va_bao_mat.md',
    title: 'Chặng 6 — Quản trị Admin & Bảo mật',
    icon: 'fa-shield-halved',
    badge: 'Security & 4-Layer Defense',
    color: 'from-red-500 to-amber-500',
    duration: '40 phút',
    desc: 'Quản trị Users/Content/Stats/Settings, Phòng thủ 4 lớp (JWT, RateLimit, FluentValidation, Ganss.Xss Whitelist 13 tags).'
  },
  {
    id: '07',
    file: '07_so_tay_cau_hoi_van_dap_bao_ve_do_an.md',
    title: 'Chặng 7 — Sổ tay 80+ Câu hỏi Vấn đáp',
    icon: 'fa-book-bookmark',
    badge: 'Vấn đáp & 34 Luồng Trace',
    color: 'from-teal-500 to-emerald-500',
    duration: '90 phút',
    desc: 'Ma trận ánh xạ 34+ luồng dữ liệu FE↔BE↔DB, Bộ 80+ câu hỏi phản biện chuyên sâu kèm đáp án và phân tích Gap học thuật.'
  }
];

function extractTOC(content) {
  const lines = content.split('\n');
  const toc = [];
  let inCode = false;

  for (let line of lines) {
    if (line.startsWith('```')) {
      inCode = !inCode;
      continue;
    }
    if (inCode) continue;

    const h2Match = line.match(/^##\s+(.+)$/);
    const h3Match = line.match(/^###\s+(.+)$/);

    if (h2Match) {
      const title = h2Match[1].trim();
      const slug = title.toLowerCase().replace(/[^\w\u00C0-\u1EF9]+/g, '-').replace(/^-+|-+$/g, '');
      toc.push({ level: 2, title, slug });
    } else if (h3Match) {
      const title = h3Match[1].trim();
      const slug = title.toLowerCase().replace(/[^\w\u00C0-\u1EF9]+/g, '-').replace(/^-+|-+$/g, '');
      toc.push({ level: 3, title, slug });
    }
  }
  return toc;
}

function extractQAs(content, docId, docTitle) {
  const qas = [];

  // Match Pattern 1: **A1. Question?** Answer... or **B2. Question?** Answer...
  const groupQRegex = /\*\*([A-G]\d+)\.\s*([^\*]+?)\*\*\s*([\s\S]+?)(?=(\n\s*\*\*[A-G]\d+\.|\n\s*##|\n\s*---|$))/g;
  let match;
  while ((match = groupQRegex.exec(content)) !== null) {
    const code = match[1].trim();
    const question = match[2].trim();
    const answer = match[3].trim();
    qas.push({
      id: `${docId}-${code}`,
      docId,
      docTitle,
      code,
      q: question,
      a: answer,
      category: getCategoryFromCode(code)
    });
  }

  // If none found with group pattern, try Numbered Pattern: 1. **Question?** Answer...
  if (qas.length === 0) {
    const numQRegex = /(?:^|\n)(\d+)\.\s+\*\*([^\*]+?)\*\*\s+([\s\S]+?)(?=(\n\s*\d+\.\s+\*\*|\n\s*##|\n\s*---|$))/g;
    while ((match = numQRegex.exec(content)) !== null) {
      const num = match[1].trim();
      const question = match[2].trim();
      const answer = match[3].trim();
      qas.push({
        id: `${docId}-Q${num}`,
        docId,
        docTitle,
        code: `Q${num}`,
        q: question,
        a: answer,
        category: docTitle.split('—')[1]?.trim() || docTitle
      });
    }
  }

  return qas;
}

function getCategoryFromCode(code) {
  const prefix = code.charAt(0).toUpperCase();
  const map = {
    'A': 'Kiến trúc & Hạ tầng',
    'B': 'Engine & Mô phỏng',
    'C': 'Khóa học & Studio',
    'D': 'Code Runner & Benchmark',
    'E': 'Gamification & VietQR',
    'F': 'Admin & Bảo mật',
    'G': 'Vận hành & Trade-off'
  };
  return map[prefix] || 'Tổng hợp';
}

const docsBundle = [];
const allQAs = [];

for (const meta of DOCS_METADATA) {
  const filePath = path.join(STUDY_DIR, meta.file);
  let content = '';
  try {
    content = fs.readFileSync(filePath, 'utf8');
  } catch (err) {
    console.error(`Warning: cannot read ${meta.file}:`, err.message);
  }

  const toc = extractTOC(content);
  const qas = extractQAs(content, meta.id, meta.title);
  allQAs.push(...qas);

  docsBundle.push({
    ...meta,
    content,
    toc,
    qas,
    qaCount: qas.length
  });
}

// Write Universal Module Definition (UMD) output
const bundleData = {
  version: '2.0.0',
  buildDate: new Date().toISOString(),
  docs: docsBundle,
  allQAs: allQAs
};

const jsOutput = `// Auto-generated Study Docs Data Bundle - Universal Module
(function(root) {
  var bundle = ${JSON.stringify(bundleData, null, 2)};
  if (typeof module !== 'undefined' && module.exports) {
    module.exports = bundle;
  }
  if (typeof window !== 'undefined') {
    window.STUDY_DOCS_BUNDLE = bundle;
  }
  if (typeof root !== 'undefined') {
    root.STUDY_DOCS_BUNDLE = bundle;
  }
})(typeof globalThis !== 'undefined' ? globalThis : this);
`;

fs.writeFileSync(path.join(WEB_DIR, 'docs-data.js'), jsOutput, 'utf8');
console.log('✅ Generated study/web/docs-data.js successfully!');
console.log(`📊 Total Docs: ${docsBundle.length}, Total Extracted Q&A: ${allQAs.length}`);
