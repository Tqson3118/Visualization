#!/usr/bin/env node
/**
 * render-diagrams.mjs
 * Render 6 SVG diagram files (1920×1080) sang PNG cùng thư mục diagrams\
 * bang Playwright CLI (chromium). Khong require('playwright') truc tiep.
 *
 * Cach chay (tu repo root D:\FPT\neww):
 *   node tailieu\render-diagrams.mjs
 *
 * Exit code: 0 = tat ca thanh cong, 1 = co loi.
 *
 * GHI CHU: KHONG dung --full-page -- playwright CLI 1.62.1 tren Windows bi treo
 * (process khong thoat, file khong duoc ghi) khi chup full-page mot trang
 * file:// SVG. Vi cac SVG nay co viewBox dung 1920x1080 nen screenshot theo
 * viewport (--viewport-size=1920,1080) da chup tron ven noi dung.
 */
import { execSync } from 'node:child_process';
import { existsSync, statSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = path.resolve(__dirname, '..');
const DIAGRAMS_DIR = path.join(__dirname, 'diagrams');

const FILES = [
  '01-usecase-tong-quan.svg',
  '02-usecase-hoc-vien.svg',
  '03-usecase-giang-vien.svg',
  '04-usecase-admin.svg',
  '05-erd-tong-quan.svg',
  '06-erd-chi-tiet.svg',
];

const VIEWPORT = '1920,1080';
const WAIT_MS = 500;
const TIMEOUT_MS = 120000;

/** Chuyen duong dan Windows sang file:// URL an toan (khong co spaces trong repo nay). */
function toFileUrl(filePath) {
  return 'file:///' + filePath.replace(/\\/g, '/');
}

function main() {
  const errors = [];
  let okCount = 0;

  console.log(`[START] Render ${FILES.length} SVG -> PNG (viewport ${VIEWPORT}, wait ${WAIT_MS}ms)`);
  console.log(`[DIR]   ${DIAGRAMS_DIR}`);

  for (const file of FILES) {
    const svgPath = path.join(DIAGRAMS_DIR, file);
    if (!existsSync(svgPath)) {
      errors.push(`${file}: SVG khong ton tai (${svgPath})`);
      console.error(`[SKIP] ${file} -- khong tim thay SVG`);
      continue;
    }

    const pngName = file.replace(/\.svg$/i, '.png');
    const pngPath = path.join(DIAGRAMS_DIR, pngName);
    const url = toFileUrl(svgPath);

    const cmd = [
      'npx',
      '--no-install',
      'playwright',
      'screenshot',
      url,
      `"${pngPath}"`,
      `--viewport-size=${VIEWPORT}`,
      `--wait-for-timeout=${WAIT_MS}`,
    ].join(' ');

    console.log(`[RENDER] ${file} -> ${pngName}`);
    console.log(`  ${cmd}`);

    try {
      execSync(cmd, {
        cwd: REPO_ROOT,
        stdio: 'inherit',
        timeout: TIMEOUT_MS,
      });
      const size = statSync(pngPath).size;
      okCount += 1;
      console.log(`[OK] ${pngName} (${(size / 1024).toFixed(1)} KB)`);
    } catch (err) {
      errors.push(`${file}: ${err.message}`);
      console.error(`[FAIL] ${file} -- render loi: ${err.message}`);
    }
  }

  if (errors.length > 0 || okCount !== FILES.length) {
    console.error(`[DONE] ${okCount}/${FILES.length} thanh cong, co loi xay ra.`);
    process.exit(1);
  }
  console.log(`[DONE] Render hoan tat: ${okCount}/${FILES.length} PNG.`);
}

main();
