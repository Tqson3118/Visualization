const { chromium } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

async function run() {
  const browser = await chromium.launch();
  const context = await browser.newContext({
    viewport: { width: 1280, height: 720 },
  });
  
  // Set auth local storage to bypass guest check
  await context.addInitScript(() => {
    window.localStorage.setItem('auth', JSON.stringify({
      user: { id: 1, email: 'student@demo.local', role: 'STUDENT' },
      accessToken: 'fake-token',
      isAuthenticated: true
    }));
  });

  const page = await context.newPage();
  const evidenceDir = path.join(__dirname, 'evidence');
  if (!fs.existsSync(evidenceDir)) {
    fs.mkdirSync(evidenceDir, { recursive: true });
  }

  const targets = [
    { key: 'sort.bubble', name: 'SIM-bubble' },
    { key: 'queue.dequeue', name: 'SIM-queue-dequeue' },
    { key: 'tree.bst-delete', name: 'SIM-bst-delete' },
    { key: 'hash.search', name: 'SIM-hash-search' },
    { key: 'graph.dijkstra', name: 'SIM-dijkstra' },
    { key: 'structure.hashtable', name: 'SIM-structure-hashtable' }
  ];

  for (const t of targets) {
    try {
      console.log(`Navigating to ${t.key}...`);
      await page.goto(`http://localhost:5173/simulator/${t.key}`);
      await page.waitForTimeout(1000); // Wait for render
      
      // Click next step a few times if possible
      for(let i=0; i<3; i++) {
        const nextBtn = page.locator('button[aria-label="Bước tiếp theo"]');
        if (await nextBtn.isVisible()) {
          await nextBtn.click();
          await page.waitForTimeout(500);
        }
      }

      await page.screenshot({ path: path.join(evidenceDir, `${t.name}.png`) });
      console.log(`Saved screenshot for ${t.key}`);
    } catch (e) {
      console.error(`Failed to snapshot ${t.key}:`, e);
    }
  }

  await browser.close();
}

run();
