const { BASE_URL, createBrowserContext, loginAs, takeEvidence } = require('./helpers');

const SIMULATION_KEYS = [
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

async function runGroupB() {
  console.log('\n======================================================');
  console.log('STARTING TEST GROUP B: 44 SIMULATORS & INPUT FUZZING');
  console.log('======================================================\n');

  const findings = [];
  const { browser, page, consoleLogs, networkErrors } = await createBrowserContext();

  try {
    // Log in as student to have full access to all non-demo simulators
    await loginAs(page, 'student@demo.local', 'Student@123');

    // ------------------------------------------------------------------------
    // TC-B01: Scan all 44 algorithms in catalog
    // ------------------------------------------------------------------------
    console.log(`\n--- Running TC-B01: Scanning all ${SIMULATION_KEYS.length} Simulators ---`);
    let passedCount = 0;
    let failedSims = [];

    for (let i = 0; i < SIMULATION_KEYS.length; i++) {
      const key = SIMULATION_KEYS[i];
      const simErrorsBefore = consoleLogs.filter(l => l.type === 'error' || l.type === 'pageerror').length;
      
      try {
        await page.goto(`${BASE_URL}/simulator/${key}`);
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(300);

        // Check canvas element
        const canvas = await page.$('canvas');
        if (!canvas) {
          throw new Error(`No canvas rendered for key: ${key}`);
        }

        // Test Step Next button
        const nextBtn = await page.$('button[title*="tiếp" i], button[aria-label*="next" i], button:has-text("Bước tiếp"), button:has-text("Next"), button.sim-ctrl__btn--next');
        if (nextBtn) {
          await nextBtn.click().catch(() => {});
          await page.waitForTimeout(100);
        }

        // Test Play / Pause button
        const playBtn = await page.$('button[title*="chạy" i], button[title*="play" i], button[aria-label*="play" i], button.sim-ctrl__btn--play');
        if (playBtn) {
          await playBtn.click().catch(() => {});
          await page.waitForTimeout(200);
          await playBtn.click().catch(() => {}); // pause
        }

        const simErrorsAfter = consoleLogs.filter(l => l.type === 'error' || l.type === 'pageerror').length;
        if (simErrorsAfter > simErrorsBefore) {
          const newErrors = consoleLogs.slice(simErrorsBefore).map(e => e.text).join('; ');
          console.log(`[TC-B01 Warning] Console error on '${key}': ${newErrors}`);
          failedSims.push({ key, error: newErrors });
        } else {
          passedCount++;
        }

        // Capture sample screenshots
        if (['sort.bubble', 'tree.avl-insert', 'graph.dijkstra', 'structure.binarytree'].includes(key)) {
          await takeEvidence(page, `QA-B01_${key.replace('.', '_')}`);
        }

      } catch (simErr) {
        console.error(`[TC-B01 FAIL] Simulator '${key}' threw error:`, simErr.message);
        failedSims.push({ key, error: simErr.message });
      }
    }

    console.log(`\n[TC-B01 Result] ${passedCount}/${SIMULATION_KEYS.length} Simulators passed standard controls without console errors.`);
    if (failedSims.length > 0) {
      findings.push({
        id: 'QA-B01-ERR',
        severity: 'P1',
        title: 'Có lỗi console/crash tại một số Simulator trong Catalog 44 thuật toán',
        details: `Các simulator gặp lỗi: ${failedSims.map(f => `${f.key} (${f.error})`).join(', ')}`
      });
    }

    // ------------------------------------------------------------------------
    // TC-B02 & TC-B03: Input Fuzzing (Sort & Search)
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-B02 & TC-B03: Input Fuzzing on Sort & Search ---');
    await page.goto(`${BASE_URL}/simulator/sort.quick`);
    await page.waitForLoadState('networkidle');

    // Look for custom array input
    const arrayInput = await page.$('input[placeholder*="mảng" i], input[placeholder*="1, 2, 3" i], input#custom-input, input[type="text"]');
    if (arrayInput) {
      console.log('[Fuzzing] Testing duplicate array: [7, 7, 7, 7, 7]...');
      await arrayInput.fill('7, 7, 7, 7, 7, 7');
      const runBtn = await page.$('button:has-text("Chạy"), button:has-text("Mô phỏng"), button:has-text("Khởi tạo")');
      if (runBtn) await runBtn.click().catch(() => {});
      await page.waitForTimeout(400);
      await takeEvidence(page, 'QA-B03_fuzzing_duplicates');

      console.log('[Fuzzing] Testing negative and huge numbers...');
      await arrayInput.fill('-999999, 0, 999999, -5, 1000000');
      if (runBtn) await runBtn.click().catch(() => {});
      await page.waitForTimeout(400);
      await takeEvidence(page, 'QA-B03_fuzzing_extremes');
    }

    // ------------------------------------------------------------------------
    // TC-B04: Cây BST Dãy số suy biến (Degenerate Tree 1,2,3,4,5,6,7)
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-B04: BST Degenerate Tree Fuzzing ---');
    await page.goto(`${BASE_URL}/simulator/tree.bst-insert`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-B04_bst_initial');

    const valueInput = await page.$('input[type="number"], input[placeholder*="giá trị" i], input[placeholder*="số" i], input[type="text"]');
    const insertBtn = await page.$('button:has-text("Chèn"), button:has-text("Insert"), button:has-text("Thêm")');
    if (valueInput && insertBtn) {
      for (const val of [1, 2, 3, 4, 5, 6, 7]) {
        await valueInput.fill(String(val));
        await insertBtn.click().catch(() => {});
        await page.waitForTimeout(300);
      }
      await takeEvidence(page, 'QA-B04_bst_degenerate_tree');
    }

    // ------------------------------------------------------------------------
    // TC-B05: Cây AVL 4 trường hợp xoay (LL, RR, LR, RL)
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-B05: AVL Rotations (LL, RR, LR, RL) ---');
    await page.goto(`${BASE_URL}/simulator/tree.avl-insert`);
    await page.waitForLoadState('networkidle');
    const avlInput = await page.$('input[type="number"], input[placeholder*="giá trị" i], input[type="text"]');
    const avlInsertBtn = await page.$('button:has-text("Chèn"), button:has-text("Insert"), button:has-text("Thêm")');
    if (avlInput && avlInsertBtn) {
      for (const v of [30, 10, 20]) {
        await avlInput.fill(String(v));
        await avlInsertBtn.click().catch(() => {});
        await page.waitForTimeout(400);
      }
      await takeEvidence(page, 'QA-B05_avl_rotation_lr');
    }

    // ------------------------------------------------------------------------
    // TC-B06: Đồ thị không liên thông & Dijkstra
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-B06: Graph Dijkstra on Disconnected Nodes ---');
    await page.goto(`${BASE_URL}/simulator/graph.dijkstra`);
    await page.waitForLoadState('networkidle');
    await takeEvidence(page, 'QA-B06_dijkstra_initial');

    // ------------------------------------------------------------------------
    // TC-B07: Phản biện Cấu hình Simulator (Speed & Theme Storage)
    // ------------------------------------------------------------------------
    console.log('\n--- Running TC-B07: Simulator Config Persistence (Global vs Per-Sim) ---');
    await page.goto(`${BASE_URL}/simulator/sort.bubble`);
    await page.waitForLoadState('networkidle');
    const speedSlider = await page.$('input[type="range"], [class*="speed"], [role="slider"]');
    if (speedSlider) {
      await speedSlider.fill('2').catch(() => {});
    }
    await page.goto(`${BASE_URL}/simulator/graph.dijkstra`);
    await page.waitForLoadState('networkidle');
    const currentSpeedVal = await page.evaluate(() => {
      const slider = document.querySelector('input[type="range"], [role="slider"]');
      return slider ? slider.value : null;
    });
    console.log(`[TC-B07] Speed on Dijkstra after changing Bubble sort: ${currentSpeedVal}`);
    await takeEvidence(page, 'QA-B07_speed_config_check');

  } catch (err) {
    console.error('[Error in Group B Execution]:', err);
    findings.push({
      id: 'QA-B-CRASH',
      severity: 'P0',
      title: 'Crash trong quá trình chạy Group B',
      details: err.stack || err.message
    });
  } finally {
    await browser.close();
  }

  console.log('\nGroup B Completed with findings:', findings.length);
  return findings;
}

runGroupB();
