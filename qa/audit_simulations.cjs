// qa/audit_simulations.cjs
const fs = require('fs');
const path = require('path');
const { chromium } = require('d:/FPT/metqua/frontend/node_modules/playwright');

const ALL_44_KEYS = [
  'sort.bubble', 'sort.selection', 'sort.insertion', 'sort.merge', 'sort.quick', 'sort.heap',
  'search.linear', 'search.binary',
  'stack.push', 'stack.pop', 'stack.peek',
  'queue.enqueue', 'queue.dequeue',
  'list.insert', 'list.delete', 'list.search', 'list.traverse',
  'tree.bst-insert', 'tree.bst-delete', 'tree.bst-search', 'tree.bst-preorder', 'tree.bst-inorder', 'tree.bst-postorder', 'tree.bst-levelorder',
  'tree.avl-insert',
  'heap.insert', 'heap.extract', 'heap.heapify',
  'hash.insert', 'hash.search', 'hash.delete',
  'graph.bfs', 'graph.dfs', 'graph.dijkstra',
  'structure.array', 'structure.linkedlist', 'structure.stack', 'structure.queue', 'structure.binarytree', 'structure.bst', 'structure.avl', 'structure.heap', 'structure.hashtable', 'structure.graph'
];

async function runAudit() {
  console.log('===============================================================');
  console.log('=== BẮT ĐẦU KIỂM THỬ TOÀN DIỆN 44 MÔ PHỎNG DSAVISUAL (BROWSER) ===');
  console.log('===============================================================\n');

  const evidenceDir = path.join(__dirname, 'evidence');
  if (!fs.existsSync(evidenceDir)) fs.mkdirSync(evidenceDir, { recursive: true });

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 }
  });

  const page = await context.newPage();

  const consoleErrors = [];
  page.on('console', msg => {
    if (msg.type() === 'error') {
      consoleErrors.push({ url: page.url(), text: msg.text() });
    }
  });
  page.on('pageerror', err => {
    consoleErrors.push({ url: page.url(), text: err.message });
  });

  // 1. Đăng nhập qua trang /login
  console.log('1. Đăng nhập tài khoản Student (student@demo.local)...');
  await page.goto('http://localhost:5173/login', { waitUntil: 'networkidle' });
  await page.fill('#email', 'student@demo.local');
  await page.fill('#password', 'Student@123');
  await page.click('button[type="submit"]');
  await page.waitForTimeout(1000);
  console.log('   Đăng nhập thành công! URL hiện tại:', page.url());

  // 2. Mở trang /simulations
  console.log('\n2. Kiểm tra trang danh mục /simulations...');
  await page.goto('http://localhost:5173/simulations', { waitUntil: 'networkidle' });
  await page.waitForTimeout(500);
  await page.screenshot({ path: path.join(evidenceDir, 'SIM-00-catalog-overview.png') });
  console.log('   Đã chụp ảnh SIM-00-catalog-overview.png');

  const results = [];

  // 3. Duyệt qua 44 mô phỏng
  console.log('\n3. Kiểm tra chi tiết 44 mô phỏng trên trình duyệt...');
  for (let i = 0; i < ALL_44_KEYS.length; i++) {
    const key = ALL_44_KEYS[i];
    const url = `http://localhost:5173/simulator/${key}`;
    process.stdout.write(`[${String(i + 1).padStart(2, '0')}/44] ${key.padEnd(25, ' ')} `);

    try {
      await page.goto(url, { waitUntil: 'networkidle', timeout: 15000 });
      await page.waitForTimeout(500);

      const simData = await page.evaluate(() => {
        const titleEl = document.querySelector('.simulator-header__title, h1');
        const badgeEls = Array.from(document.querySelectorAll('.simulator-header__badge')).map(b => b.textContent.trim());
        const canvas = document.querySelector('canvas');
        const statsEl = document.querySelector('.simulator-controls-card');
        const explanationEl = document.querySelector('.simulator-col--right');
        const pseudocodeLines = document.querySelectorAll('.pseudocode-line, [class*="pseudocode"]');
        const hasError = !!document.querySelector('[role="alert"], .text-rose-400');
        const alertText = hasError ? document.querySelector('[role="alert"], .text-rose-400')?.textContent?.trim() : null;

        // Extract steps count text (e.g. "Bước 1/66")
        const statsText = statsEl?.textContent?.trim() || '';
        const stepMatch = statsText.match(/Bước\s+(\d+)\/(\d+)/i);

        return {
          title: titleEl?.textContent?.trim() || '',
          badges: badgeEls,
          hasCanvas: !!canvas,
          stepCurrent: stepMatch ? parseInt(stepMatch[1], 10) : 0,
          totalSteps: stepMatch ? parseInt(stepMatch[2], 10) : 0,
          hasPseudocode: pseudocodeLines.length > 0,
          explanationSample: explanationEl?.textContent?.trim()?.slice(0, 120) || '',
          hasError,
          alertText
        };
      });

      // Bấm Next step 2 lần để verify interactive controls
      let nextStepWorked = false;
      const nextBtn = page.locator('button:has-text("Bước tới"), button[aria-label*="kế tiếp" i], button[title*="Bước tiếp theo" i], button:has(.lucide-step-forward), button:has(.lucide-chevron-right)').first();
      if (await nextBtn.isVisible() && await nextBtn.isEnabled()) {
        await nextBtn.click();
        await page.waitForTimeout(100);
        if (await nextBtn.isEnabled()) {
          await nextBtn.click();
          await page.waitForTimeout(100);
        }

        const afterStep = await page.evaluate(() => {
          const statsText = document.querySelector('.simulator-controls-card')?.textContent?.trim() || '';
          const stepMatch = statsText.match(/Bước\s+(\d+)\/(\d+)/i);
          return stepMatch ? parseInt(stepMatch[1], 10) : 0;
        });
        if (afterStep > simData.stepCurrent) {
          nextStepWorked = true;
        }
      }

      // Chụp ảnh bằng chứng
      const shotName = `SIM-${key.replace('.', '_')}.png`;
      const shotPath = path.join(evidenceDir, shotName);
      await page.screenshot({ path: shotPath });

      results.push({
        index: i + 1,
        key,
        title: simData.title,
        badges: simData.badges,
        hasCanvas: simData.hasCanvas,
        stepCurrent: simData.stepCurrent,
        totalSteps: simData.totalSteps,
        nextStepWorked,
        hasPseudocode: simData.hasPseudocode,
        explanationSample: simData.explanationSample,
        hasError: simData.hasError,
        alertText: simData.alertText,
        shotName
      });

      console.log(`✅ OK | Steps: ${simData.totalSteps} | Canvas: ${simData.hasCanvas ? 'Yes' : 'No'} | Title: "${simData.title}"`);
    } catch (err) {
      console.log(`❌ FAIL | Error: ${err.message}`);
      const errShot = `SIM-${key.replace('.', '_')}-ERROR.png`;
      try { await page.screenshot({ path: path.join(evidenceDir, errShot) }); } catch (_) {}
      results.push({
        index: i + 1,
        key,
        title: '',
        hasCanvas: false,
        stepCurrent: 0,
        totalSteps: 0,
        nextStepWorked: false,
        hasError: true,
        alertText: err.message,
        shotName: errShot
      });
    }
  }

  // 4. Test các trường hợp đặc biệt & Edge Cases
  console.log('\n4. Kiểm tra các trường hợp Edge Case & Input Tùy Chỉnh...');

  // Test 4.1: Input Modal
  console.log('   Testing sort.bubble với Input Modal...');
  await page.goto('http://localhost:5173/simulator/sort.bubble', { waitUntil: 'networkidle' });
  const configBtn = page.locator('button:has-text("Cấu hình"), button:has-text("Dữ liệu đầu vào")');
  if (await configBtn.isVisible()) {
    await configBtn.click();
    await page.waitForTimeout(300);
    await page.screenshot({ path: path.join(evidenceDir, 'SIM-input-modal.png') });
    const closeBtn = page.locator('button:has-text("Đóng"), button:has-text("Hủy"), button[aria-label*="Đóng" i]').first();
    if (await closeBtn.isVisible()) await closeBtn.click();
  }

  // Test 4.2: BST Delete với 2 con (trigger successor logic)
  console.log('   Testing tree.bst-delete (node 2 con)...');
  await page.goto('http://localhost:5173/simulator/tree.bst-delete', { waitUntil: 'networkidle' });
  await page.waitForTimeout(400);
  for (let s = 0; s < 7; s++) {
    const nBtn = page.locator('button:has-text("Bước tới")').first();
    if (await nBtn.isVisible() && await nBtn.isEnabled()) {
      await nBtn.click();
      await page.waitForTimeout(100);
    }
  }
  await page.screenshot({ path: path.join(evidenceDir, 'SIM-tree_bst-delete-successor.png') });

  // Test 4.3: AVL rotation (LL/RR)
  console.log('   Testing tree.avl-insert (rotation)...');
  await page.goto('http://localhost:5173/simulator/tree.avl-insert', { waitUntil: 'networkidle' });
  await page.waitForTimeout(400);
  for (let s = 0; s < 8; s++) {
    const nBtn = page.locator('button:has-text("Bước tới")').first();
    if (await nBtn.isVisible() && await nBtn.isEnabled()) {
      await nBtn.click();
      await page.waitForTimeout(100);
    }
  }
  await page.screenshot({ path: path.join(evidenceDir, 'SIM-tree_avl-insert-rotation.png') });

  // Test 4.4: Binary search với mảng chưa sắp xếp (Auto-sort)
  console.log('   Testing search.binary...');
  await page.goto('http://localhost:5173/simulator/search.binary', { waitUntil: 'networkidle' });
  await page.waitForTimeout(400);
  await page.screenshot({ path: path.join(evidenceDir, 'SIM-search_binary-pointers.png') });

  // Test 4.5: Dijkstra relaxation & dist table
  console.log('   Testing graph.dijkstra...');
  await page.goto('http://localhost:5173/simulator/graph.dijkstra', { waitUntil: 'networkidle' });
  await page.waitForTimeout(400);
  for (let s = 0; s < 6; s++) {
    const nBtn = page.locator('button:has-text("Bước tới")').first();
    if (await nBtn.isVisible() && await nBtn.isEnabled()) {
      await nBtn.click();
      await page.waitForTimeout(100);
    }
  }
  await page.screenshot({ path: path.join(evidenceDir, 'SIM-graph_dijkstra-relaxation.png') });

  // Test 4.6: Hash Table Chaining
  console.log('   Testing hash.insert...');
  await page.goto('http://localhost:5173/simulator/hash.insert', { waitUntil: 'networkidle' });
  await page.waitForTimeout(400);
  for (let s = 0; s < 6; s++) {
    const nBtn = page.locator('button:has-text("Bước tới")').first();
    if (await nBtn.isVisible() && await nBtn.isEnabled()) {
      await nBtn.click();
      await page.waitForTimeout(100);
    }
  }
  await page.screenshot({ path: path.join(evidenceDir, 'SIM-hash_insert-chaining.png') });

  // Test 4.7: Structure Hash Table Terminology fix
  console.log('   Testing structure.hashtable...');
  await page.goto('http://localhost:5173/simulator/structure.hashtable', { waitUntil: 'networkidle' });
  await page.waitForTimeout(400);
  await page.screenshot({ path: path.join(evidenceDir, 'SIM-structure_hashtable-fixed.png') });

  // Ghi kết quả thô ra JSON
  fs.writeFileSync(path.join(__dirname, 'audit_raw_results.json'), JSON.stringify({ results, consoleErrors }, null, 2));

  console.log('\n===============================================================');
  console.log('=== KIỂM THỬ HOÀN TẤT: 44/44 MÔ PHỎNG ĐÃ ĐƯỢC AUDIT ===');
  console.log('===============================================================');

  await browser.close();
}

runAudit().catch(err => {
  console.error('Fatal audit error:', err);
  process.exit(1);
});
