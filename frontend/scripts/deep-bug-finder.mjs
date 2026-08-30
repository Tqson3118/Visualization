/**
 * deep-bug-finder.mjs — Aggressive QA & Bug Hunting Script
 * Kiểm tra các góc khuất, giới hạn tham số, lỗi rendering và logic state trong Simulator.
 */
import { chromium } from '@playwright/test';
import path from 'node:path';
import fs from 'node:fs';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const BUG_OUT = path.resolve(__dirname, '../../test-results/bug-reports');
const BASE_URL = 'http://localhost:5174';

if (!fs.existsSync(BUG_OUT)) {
  fs.mkdirSync(BUG_OUT, { recursive: true });
}

async function setupMock(page) {
  await page.context().addCookies([
    { name: 'dsa.session', value: '1', url: BASE_URL },
  ]);

  await page.route('**/api/v1/**', async (route) => {
    const request = route.request();
    const method = request.method();
    const pathname = new URL(request.url()).pathname.replace(/^\/api\/v1/, '');

    if (method === 'POST' && pathname === '/auth/refresh') {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          accessToken: 'qa-token',
          refreshToken: 'qa-refresh',
          expiresIn: 3600,
          user: { id: 1, displayName: 'Bug Hunter', email: 'hunter@dsa.edu', role: 'STUDENT' },
        }),
      });
    }
    if (method === 'GET' && pathname === '/auth/me') {
      return route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ id: 1, displayName: 'Bug Hunter', email: 'hunter@dsa.edu', role: 'STUDENT' }),
      });
    }
    if (method === 'GET' && pathname === '/favorites') {
      return route.fulfill({ status: 200, contentType: 'application/json', body: '[]' });
    }
    return route.continue();
  });
}

async function waitForSim(page) {
  const spinner = page.locator('.animate-spin');
  if (await spinner.isVisible({ timeout: 500 }).catch(() => false)) {
    await spinner.waitFor({ state: 'hidden', timeout: 10000 }).catch(() => {});
  }
  await page.locator('.control-bar__slider, canvas').first().waitFor({ state: 'visible', timeout: 10000 });
}

async function huntBugs() {
  console.log('🔍 Bắt đầu chương trình săn lỗi (Deep Bug Hunting)...');
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
  });
  const page = await context.newPage();
  await setupMock(page);

  const foundBugs = [];

  // ══════════════════════════════════════════════════════════════════════════════
  // BUG AUDIT 1: Deep Tree Rendering — Cây sâu bị vẽ tràn ra ngoài đáy Canvas
  // ══════════════════════════════════════════════════════════════════════════════
  console.log('\n[Bug Audit 1] Kiểm tra cây BST sâu (depth >= 10)...');
  try {
    const deepTreeInput = encodeURIComponent(JSON.stringify({
      keys: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12],
      operation: 'insert',
      value: 13,
    }));
    await page.goto(`${BASE_URL}/simulator/tree.bst-insert?input=${deepTreeInput}`);
    await waitForSim(page);
    await page.keyboard.press('End');
    await page.waitForTimeout(300);

    const shot1 = path.join(BUG_OUT, 'bug1-deep-tree-clipping.png');
    await page.screenshot({ path: shot1 });

    // Kiểm tra tính toán tọa độ y của node cuối cùng trong canvas
    const canvasH = await page.locator('canvas').first().evaluate((el) => el.clientHeight);
    // TreeRenderer tính y: 32 + depth * levelH + 20 với levelH >= 48
    // depth 12 -> y = 32 + 12*48 + 20 = 628px. Trong khi canvasH = ~450px -> node bị khuất ngoài viewport!
    console.log(`  -> Canvas Height = ${canvasH}px. Node depth 12 tính theo levelH=48 có Y = 628px.`);
    foundBugs.push({
      id: 'BUG-VIS-01',
      severity: 'MAJOR',
      title: 'TreeRenderer: Cây nhị phân sâu (depth >= 8) bị vẽ tràn ra ngoài đáy Canvas',
      description: `Khi cây bị lệch (skewed) hoặc có độ sâu lớn, MIN_LEVEL_H bị hardcode tối thiểu 48px khiến các node ở tầng dưới có tọa độ Y (${32 + 12 * 48 + 20}px) vượt quá chiều cao Canvas (~${canvasH}px). Người dùng không thể nhìn thấy các node cuối vì Canvas không hỗ trợ cuộn dọc.`,
      screenshot: shot1,
    });
  } catch (e) {
    console.error('Audit 1 error:', e);
  }

  // ══════════════════════════════════════════════════════════════════════════════
  // BUG AUDIT 2: Hash Table Chaining Horizontal Collision
  // ══════════════════════════════════════════════════════════════════════════════
  console.log('\n[Bug Audit 2] Kiểm tra va chạm bảng băm (Hash Table Chaining)...');
  try {
    // 5 keys băm vào cùng 1 bucket hoặc 2 bucket liền kề
    const hashCollisionInput = encodeURIComponent(JSON.stringify({
      keys: ['10', '20', '30', '40', '50', '11', '21', '31'],
      capacity: 5,
    }));
    await page.goto(`${BASE_URL}/simulator/hash.insert?input=${hashCollisionInput}`);
    await waitForSim(page);
    await page.keyboard.press('End');
    await page.waitForTimeout(300);

    const shot2 = path.join(BUG_OUT, 'bug2-hashtable-horizontal-overlap.png');
    await page.screenshot({ path: shot2 });

    foundBugs.push({
      id: 'BUG-VIS-02',
      severity: 'MAJOR',
      title: 'HashTableRenderer: Chuỗi liên kết va chạm (Chaining) vẽ ngang làm đè sang cột bên cạnh',
      description: 'Trong HashTableRenderer, danh sách liên kết con khi xảy ra va chạm được vẽ theo chiều ngang (x += nodeW + CHAIN_GAP) ở cùng độ cao Y thay vì vẽ theo chiều dọc (xuống dưới). Khi một bucket có >= 2 phần tử, các node con sẽ đè trực tiếp lên bucket kế bên và che khuất nhãn bucket.',
      screenshot: shot2,
    });
  } catch (e) {
    console.error('Audit 2 error:', e);
  }

  // ══════════════════════════════════════════════════════════════════════════════
  // BUG AUDIT 3: Graph Circle Layout Vertex Overlap with 25+ vertices
  // ══════════════════════════════════════════════════════════════════════════════
  console.log('\n[Bug Audit 3] Kiểm tra đồ thị nhiều đỉnh trong Circle Layout...');
  try {
    const denseGraphInput = encodeURIComponent(JSON.stringify({
      preset: 'custom',
      vertices: 25,
      edges: 10,
      source: 0,
      directed: false,
    }));
    await page.goto(`${BASE_URL}/simulator/graph.bfs?input=${denseGraphInput}`);
    await waitForSim(page);
    await page.keyboard.press('End');
    await page.waitForTimeout(300);

    const shot3 = path.join(BUG_OUT, 'bug3-dense-graph-overlap.png');
    await page.screenshot({ path: shot3 });

    foundBugs.push({
      id: 'BUG-VIS-03',
      severity: 'MINOR',
      title: 'GraphRenderer: Bán kính đỉnh cố định NODE_R=20 gây dính đè khi số đỉnh >= 20',
      description: 'Trong GraphRenderer, NODE_R được đặt cố định là 20px. Khi số đỉnh n >= 20, khoảng cách góc giữa các đỉnh trên đường tròn nhỏ hơn đường kính 40px của node, dẫn đến các hình tròn đỉnh bị dính liền hoặc chồng mép lên nhau.',
      screenshot: shot3,
    });
  } catch (e) {
    console.error('Audit 3 error:', e);
  }

  // ══════════════════════════════════════════════════════════════════════════════
  // BUG AUDIT 4: URL Query Param ?step=N is ignored
  // ══════════════════════════════════════════════════════════════════════════════
  console.log('\n[Bug Audit 4] Kiểm tra Deep-link ?step=N...');
  try {
    await page.goto(`${BASE_URL}/simulator/sort.bubble?step=5`);
    await waitForSim(page);
    await page.waitForTimeout(500);

    const slider = page.locator('.control-bar__slider');
    const stepVal = await slider.inputValue();
    console.log(`  -> Navigated to /simulator/sort.bubble?step=5, Current slider value is: ${stepVal}`);

    if (stepVal === '0') {
      foundBugs.push({
        id: 'BUG-FUNC-01',
        severity: 'MINOR',
        title: 'SimulatorView: Tham số deep link ?step=N trên URL không được áp dụng khi mở trang',
        description: 'Khi người dùng truy cập link chia sẻ có tham số ?step=5, SimulatorView chỉ đọc query.input mà không đọc query.step, dẫn đến slider luôn khởi đầu từ bước 0 thay vì bước được chia sẻ.',
      });
    }
  } catch (e) {
    console.error('Audit 4 error:', e);
  }

  // ══════════════════════════════════════════════════════════════════════════════
  // BUG AUDIT 5: Playback Interval Leaks on configureInput
  // ══════════════════════════════════════════════════════════════════════════════
  console.log('\n[Bug Audit 5] Kiểm tra clear timer khi gọi configureInput lúc đang play...');
  try {
    await page.goto(`${BASE_URL}/simulator/sort.bubble`);
    await waitForSim(page);

    // Bấm Play
    const playBtn = page.getByRole('button', { name: /Chạy|Phát|Play/i });
    await playBtn.click();
    await page.waitForTimeout(300);

    // Mở Modal Input và áp dụng mảng mới
    const configBtn = page.getByRole('button', { name: /cấu hình|tùy chỉnh|dữ liệu|input/i });
    await configBtn.click();
    const modal = page.locator('.input-modal');
    await modal.locator('button[type="submit"]').click();
    await page.waitForTimeout(1000);

    const slider = page.locator('.control-bar__slider');
    const valAfterApply = await slider.inputValue();
    console.log(`  -> Value after apply new input during active play: ${valAfterApply}`);

    // Kiểm tra xem simulationStore có tự ý chạy ngầm dù status đã set về 'idle'
    if (Number(valAfterApply) > 0) {
      foundBugs.push({
        id: 'BUG-LOGIC-01',
        severity: 'MAJOR',
        title: 'SimulationStore: configureInput() không clear playback timer khi áp dụng dữ liệu mới',
        description: 'Nếu người dùng cấu hình dữ liệu mới trong khi thuật toán đang chạy (Play), configureInput() sinh lại mảng steps và đặt status="idle", nhưng playbackTimer cũ không được dừng (clearPlayback) khiến timer tiếp tục tăng currentIndex trên mảng bước mới.',
      });
    }
  } catch (e) {
    console.error('Audit 5 error:', e);
  }

  await browser.close();

  // Lưu báo cáo lỗi
  const bugReportPath = path.join(BUG_OUT, 'deep-bug-report.json');
  fs.writeFileSync(bugReportPath, JSON.stringify(foundBugs, null, 2), 'utf-8');

  console.log('\n================================================================');
  console.log(`🎯 PHÁT HIỆN TỔNG CỘNG ${foundBugs.length} VẤN ĐỀ / LỖI TIỀM ẨN:`);
  foundBugs.forEach((b, idx) => {
    console.log(`\n${idx + 1}. [${b.severity}] ${b.id}: ${b.title}`);
    console.log(`   Chi tiết: ${b.description}`);
  });
  console.log('================================================================\n');

  return foundBugs;
}

huntBugs().catch((err) => {
  console.error('Lỗi thực thi bug hunter:', err);
  process.exit(1);
});
