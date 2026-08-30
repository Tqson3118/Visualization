import { chromium } from 'playwright';
import fs from 'fs';
import path from 'path';

async function main() {
  const outputDir = 'D:/FPT/neww/frontend/public/screenshots/current_review';
  if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
  }

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 1.5,
  });
  const page = await context.newPage();

  // 1. HOME VIEW
  console.log('Visiting HomeView (http://localhost:5173/)...');
  await page.goto('http://localhost:5173/', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await page.screenshot({ path: path.join(outputDir, '01_home_hero_stage.png') });
  console.log('Saved 01_home_hero_stage.png');

  // 2. SIMULATOR - QUICK SORT
  console.log('Visiting Simulator QuickSort (http://localhost:5173/simulator/sort.quick)...');
  await page.goto('http://localhost:5173/simulator/sort.quick', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1500);

  // Play a few steps
  const nextStepBtn = page.locator('button:has-text("Bước tới"), button[aria-label*="bước"], button[title*="bước"]').first();
  if (await nextStepBtn.isVisible()) {
    await nextStepBtn.click();
    await page.waitForTimeout(400);
    await nextStepBtn.click();
    await page.waitForTimeout(400);
  }
  await page.screenshot({ path: path.join(outputDir, '02_simulator_quicksort.png') });
  console.log('Saved 02_simulator_quicksort.png');

  // 3. SIMULATOR - BFS GRAPH
  console.log('Visiting Simulator BFS Graph (http://localhost:5173/simulator/graph.bfs)...');
  await page.goto('http://localhost:5173/simulator/graph.bfs', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1500);
  if (await nextStepBtn.isVisible()) {
    await nextStepBtn.click();
    await page.waitForTimeout(400);
    await nextStepBtn.click();
    await page.waitForTimeout(400);
  }
  await page.screenshot({ path: path.join(outputDir, '03_simulator_graph_bfs.png') });
  console.log('Saved 03_simulator_graph_bfs.png');

  // 4. SIMULATOR - BST TREE
  console.log('Visiting Simulator BST Tree (http://localhost:5173/simulator/tree.bst)...');
  await page.goto('http://localhost:5173/simulator/tree.bst', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1500);
  if (await nextStepBtn.isVisible()) {
    await nextStepBtn.click();
    await page.waitForTimeout(400);
    await nextStepBtn.click();
    await page.waitForTimeout(400);
  }
  await page.screenshot({ path: path.join(outputDir, '04_simulator_tree_bst.png') });
  console.log('Saved 04_simulator_tree_bst.png');

  // 5. SIMULATIONS CATALOG
  console.log('Visiting Simulations Catalog (http://localhost:5173/simulations)...');
  await page.goto('http://localhost:5173/simulations', { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  await page.screenshot({ path: path.join(outputDir, '05_simulations_catalog.png') });
  console.log('Saved 05_simulations_catalog.png');

  await browser.close();
  console.log('ALL CURRENT REVIEW SCREENSHOTS CAPTURED!');
}

main().catch(console.error);
