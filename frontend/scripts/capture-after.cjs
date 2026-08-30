const { createServer } = require('vite');
const { chromium } = require('@playwright/test');
const path = require('path');
const fs = require('fs');

async function captureAfter() {
  const screenshotsDir = path.resolve(__dirname, '../public/screenshots');
  if (!fs.existsSync(screenshotsDir)) {
    fs.mkdirSync(screenshotsDir, { recursive: true });
  }

  console.log('Starting Vite server for After screenshots...');
  const server = await createServer({
    configFile: path.resolve(__dirname, '../vite.config.ts'),
    server: { port: 5175 },
  });
  await server.listen();
  const address = server.httpServer.address();
  const baseUrl = `http://localhost:${address.port}`;
  console.log(`Vite server running at ${baseUrl}`);

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1280, height: 800 },
    deviceScaleFactor: 2,
  });

  const page = await context.newPage();

  // Mock API endpoints
  await page.route('**/api/v1/auth/refresh', route => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        accessToken: 'mock-access-token',
        user: { id: 'mock-user-1', email: 'test@example.com', name: 'Lead WebGL Engineer', role: 'STUDENT' }
      })
    });
  });
  await page.route('**/api/v1/me', route => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: 'mock-user-1', email: 'test@example.com', name: 'Lead WebGL Engineer', role: 'STUDENT'
      })
    });
  });
  await page.route('**/api/v1/favorites', route => {
    route.fulfill({ status: 200, contentType: 'application/json', body: '[]' });
  });
  await page.route('**/api/v1/me/inventory', route => {
    route.fulfill({ status: 200, contentType: 'application/json', body: '[]' });
  });
  await page.route('**/api/v1/me/hearts', route => {
    route.fulfill({ status: 200, contentType: 'application/json', body: '{"hearts": 5, "gems": 100}' });
  });

  // 1. HomeView Algorithmic Stage
  console.log('Capturing HomeView Algorithmic Stage (After)...');
  await page.goto(`${baseUrl}/`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1500);
  const benchElement = await page.$('.home__bench');
  if (benchElement) {
    await benchElement.screenshot({ path: path.join(screenshotsDir, 'after_home_stage.png') });
  }

  // Function to capture simulator
  async function captureSimulator(key, filename) {
    console.log(`Capturing ${key} (After)...`);
    await page.goto(`${baseUrl}/simulator/${key}`, { waitUntil: 'networkidle' });
    await page.waitForSelector('.simulator__canvas-wrap, .canvas-area, .data-stage', { timeout: 5000 }).catch(() => {});
    await page.waitForTimeout(2000);

    const canvasWrap = await page.$('.simulator__canvas-wrap') || await page.$('.canvas-area') || await page.$('.data-stage');
    if (canvasWrap) {
      await canvasWrap.screenshot({ path: path.join(screenshotsDir, `${filename}.png`) });
      await canvasWrap.screenshot({ path: path.join(screenshotsDir, `after_${key.replace('.', '_')}.png`) });
    }
    await page.screenshot({ path: path.join(screenshotsDir, `${filename}_full.png`) });
  }

  await captureSimulator('sort.quick', 'after_simulator_quick_sort');
  await captureSimulator('graph.bfs', 'after_simulator_graph_bfs');
  await captureSimulator('tree.bst-insert', 'after_simulator_tree_bst');

  // Also duplicate to after_tree_bst.png
  const treeBstSrc = path.join(screenshotsDir, 'after_simulator_tree_bst.png');
  if (fs.existsSync(treeBstSrc)) {
    fs.copyFileSync(treeBstSrc, path.join(screenshotsDir, 'after_tree_bst.png'));
  }

  await browser.close();
  await server.close();
  console.log('Done capturing After screenshots!');
}

captureAfter().catch(err => {
  console.error(err);
  process.exit(1);
});
