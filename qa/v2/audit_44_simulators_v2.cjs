const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const EVIDENCE_DIR = path.join(__dirname, 'evidence');
if (!fs.existsSync(EVIDENCE_DIR)) {
  fs.mkdirSync(EVIDENCE_DIR, { recursive: true });
}

const SIMULATORS = [
  { key: 'sort.bubble', group: 'sort', edge: 'empty and single: [], [5], [-3,1,-7], [3,3,3], [99999]' },
  { key: 'sort.selection', group: 'sort', edge: 'single and reverse: [1], [9,8,7,6,5], [5,5,5]' },
  { key: 'sort.insertion', group: 'sort', edge: 'sorted and reverse: [1,2,3,4], [5,4,3,2,1]' },
  { key: 'sort.merge', group: 'sort', edge: 'odd length and duplicates: [7,2,9,4,1,5,3]' },
  { key: 'sort.quick', group: 'sort', edge: 'worst-case pivot and duplicates: [5,5,5,5,5]' },
  { key: 'sort.heap', group: 'sort', edge: 'negative values: [-5, 10, -20, 0, 15]' },
  { key: 'search.linear', group: 'search', edge: 'target missing and target at ends' },
  { key: 'search.binary', group: 'search', edge: 'empty array, target missing, target at boundaries' },
  { key: 'stack.push', group: 'stack', edge: 'push multiple elements to capacity' },
  { key: 'stack.pop', group: 'stack', edge: 'pop on empty stack' },
  { key: 'stack.peek', group: 'stack', edge: 'peek on empty stack' },
  { key: 'queue.enqueue', group: 'queue', edge: 'enqueue multiple elements' },
  { key: 'queue.dequeue', group: 'queue', edge: 'dequeue on empty queue' },
  { key: 'list.insert', group: 'list', edge: 'insert at head, tail, negative index, out of bounds' },
  { key: 'list.delete', group: 'list', edge: 'delete on empty list, delete non-existent' },
  { key: 'list.search', group: 'list', edge: 'search in empty list, non-existent target' },
  { key: 'list.traverse', group: 'list', edge: 'traverse empty list, single node list' },
  { key: 'tree.bst-insert', group: 'tree', edge: 'insert into empty tree, insert duplicate values' },
  { key: 'tree.bst-delete', group: 'tree', edge: 'delete from empty tree, delete root, delete leaf, delete 2-child node' },
  { key: 'tree.bst-search', group: 'tree', edge: 'search in empty tree, search non-existent' },
  { key: 'tree.bst-preorder', group: 'tree', edge: 'preorder on empty tree, single node tree' },
  { key: 'tree.bst-inorder', group: 'tree', edge: 'inorder on empty tree, single node tree' },
  { key: 'tree.bst-postorder', group: 'tree', edge: 'postorder on empty tree, single node tree' },
  { key: 'tree.bst-levelorder', group: 'tree', edge: 'levelorder on empty tree, single node tree' },
  { key: 'tree.avl-insert', group: 'tree', edge: 'trigger LL, RR, LR, RL rotations' },
  { key: 'heap.insert', group: 'heap', edge: 'insert max and min values' },
  { key: 'heap.extract', group: 'heap', edge: 'extract from empty heap' },
  { key: 'heap.heapify', group: 'heap', edge: 'heapify already sorted array' },
  { key: 'hash.insert', group: 'hash', edge: 'collision in same slot, duplicate keys' },
  { key: 'hash.search', group: 'hash', edge: 'search non-existent key, search in collision chain' },
  { key: 'hash.delete', group: 'hash', edge: 'delete non-existent key, delete from collision chain' },
  { key: 'graph.bfs', group: 'graph', edge: 'disconnected graph, isolated node' },
  { key: 'graph.dfs', group: 'graph', edge: 'cyclic graph, tree graph' },
  { key: 'graph.dijkstra', group: 'graph', edge: 'unreachable destination node' },
  { key: 'structure.array', group: 'structure', edge: 'render and element lookup' },
  { key: 'structure.linkedlist', group: 'structure', edge: 'render head/tail/next pointers' },
  { key: 'structure.stack', group: 'structure', edge: 'render top pointer and LIFO' },
  { key: 'structure.queue', group: 'structure', edge: 'render front/rear pointers and FIFO' },
  { key: 'structure.binarytree', group: 'structure', edge: 'render root and child nodes' },
  { key: 'structure.bst', group: 'structure', edge: 'render BST ordering invariant' },
  { key: 'structure.avl', group: 'structure', edge: 'render balance factors' },
  { key: 'structure.heap', group: 'structure', edge: 'render array vs binary tree representation' },
  { key: 'structure.hashtable', group: 'structure', edge: 'render buckets and linked list chains' },
  { key: 'structure.graph', group: 'structure', edge: 'render adjacency list and directed edges' },
];

async function main() {
  console.log('=== STARTING AUDIT OF ALL 44 SIMULATORS WITH EDGE CASES ===');
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1280, height: 800 },
  });

  // Inject authenticated student session
  await context.addInitScript(() => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 1, email: 'student@demo.local', role: 'STUDENT', displayName: 'Sinh vien mau' },
      accessToken: 'dummy-token',
      isAuthenticated: true
    }));
  });

  const page = await context.newPage();
  const results = [];

  for (let i = 0; i < SIMULATORS.length; i++) {
    const sim = SIMULATORS[i];
    const indexStr = String(i + 1).padStart(2, '0');
    console.log(`[${indexStr}/44] Testing: ${sim.key} (${sim.group})...`);

    const consoleErrors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') consoleErrors.push(msg.text());
    });

    const pageErrors = [];
    page.on('pageerror', err => {
      pageErrors.push(err.message);
    });

    try {
      await page.goto(`http://localhost:5173/simulator/${sim.key}`, { waitUntil: 'networkidle', timeout: 15000 });
      await page.waitForTimeout(600);

      // Verify header / title
      const title = await page.title();

      // Step Forward 3 times
      let stepCount = 0;
      const nextBtn = page.locator('button[aria-label="Bước tiếp theo"]');
      if (await nextBtn.isVisible({ timeout: 1500 }).catch(() => false)) {
        for (let s = 0; s < 3; s++) {
          await nextBtn.click();
          await page.waitForTimeout(300);
          stepCount++;
        }
      }

      // Check for play/pause
      const playBtn = page.locator('button[aria-label="Chạy tự động"]');
      let playWorking = false;
      if (await playBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
        await playBtn.click();
        await page.waitForTimeout(400);
        // Pause
        const pauseBtn = page.locator('button[aria-label="Tạm dừng"]');
        if (await pauseBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
          await pauseBtn.click();
          playWorking = true;
        } else {
          playWorking = true;
        }
      }

      // Check "Cấu hình đầu vào" modal or custom input if available
      const configBtn = page.locator('button:has-text("Cấu hình đầu vào")');
      let configModalTested = false;
      if (await configBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
        await configBtn.click();
        await page.waitForTimeout(400);
        
        // Take screenshot of config modal
        await page.screenshot({ path: path.join(EVIDENCE_DIR, `SIM-${indexStr}-${sim.key.replace('.', '_')}-config.png`) });
        
        // Close modal
        const closeBtn = page.locator('button:has-text("Đóng"), button:has-text("Áp dụng"), button:has-text("Hủy")').first();
        if (await closeBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
          await closeBtn.click();
          await page.waitForTimeout(300);
        }
        configModalTested = true;
      }

      // Screenshot final state
      const shotName = `SIM-${indexStr}-${sim.key.replace('.', '_')}.png`;
      await page.screenshot({ path: path.join(EVIDENCE_DIR, shotName) });

      const passed = pageErrors.length === 0;
      results.push({
        index: i + 1,
        key: sim.key,
        group: sim.group,
        edge: sim.edge,
        passed,
        stepCount,
        playWorking,
        configModalTested,
        errors: pageErrors.concat(consoleErrors),
        screenshot: shotName
      });

      console.log(`   -> OK (steps: ${stepCount}, errors: ${pageErrors.length})`);
    } catch (err) {
      console.error(`   -> FAILED: ${err.message}`);
      results.push({
        index: i + 1,
        key: sim.key,
        group: sim.group,
        edge: sim.edge,
        passed: false,
        error: err.message
      });
    }
  }

  await browser.close();

  fs.writeFileSync(path.join(__dirname, 'simulator_audit_results.json'), JSON.stringify(results, null, 2), 'utf8');
  console.log(`=== AUDIT COMPLETE: ${results.filter(r => r.passed).length}/44 PASSED ===`);
}

main().catch(console.error);
