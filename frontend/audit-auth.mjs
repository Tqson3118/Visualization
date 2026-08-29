import { chromium } from 'playwright';
import { writeFileSync } from 'fs';
const BASE = 'http://localhost:5174';
const EMAIL = 'rescue.tester.v3@gmail.com', PASS = 'Rescue@2026';
const ROUTES = ['/path','/dashboard','/profile','/simulations','/simulator/sort.bubble','/simulator/graph.bfs','/cheatsheet','/leaderboard','/quests','/shop','/premium','/account/subscription','/classes','/exercise/1','/lessons/1','/courses','/learn/1','/help'];

const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: 1366, height: 900 } });
const page = await ctx.newPage();
// login via UI form
await page.goto(BASE + '/login', { waitUntil: 'domcontentloaded' });
await page.waitForTimeout(1500);
await page.evaluate(([e, p]) => {
  const set = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;
  const em = document.querySelector('input[type=email], input[name=email]');
  const pw = document.querySelector('input[type=password]');
  set.call(em, e); em.dispatchEvent(new Event('input', { bubbles: true }));
  set.call(pw, p); pw.dispatchEvent(new Event('input', { bubbles: true }));
}, [EMAIL, PASS]);
await page.evaluate(() => { const b = Array.from(document.querySelectorAll('button')).find(x => /đăng nhập/i.test(x.textContent || '')); b?.click(); });
await page.waitForTimeout(3500);
console.log('after login:', page.url());

const results = [];
for (const route of ROUTES) {
  const errs = [], pageErrs = [], badReq = [];
  const onC = m => { if (m.type() === 'error') errs.push(m.text().slice(0, 160)); };
  const onP = e => pageErrs.push(String(e.message).slice(0, 160));
  const onR = r => { if (r.status() >= 400) badReq.push(r.status() + ' ' + r.url().split('/api').pop().slice(0, 60)); };
  page.on('console', onC); page.on('pageerror', onP); page.on('response', onR);
  let url = '', len = 0;
  try {
    await page.goto(BASE + route, { waitUntil: 'domcontentloaded', timeout: 25000 });
    await page.waitForTimeout(2200);
    url = page.url().replace(BASE, '');
    len = await page.evaluate(() => document.body.innerText.length);
    // mobile overflow check
    await page.setViewportSize({ width: 390, height: 844 });
    await page.waitForTimeout(700);
    const mob = await page.evaluate(() => document.documentElement.scrollWidth - document.documentElement.clientWidth);
    await page.setViewportSize({ width: 1366, height: 900 });
    results.push({ route, url, len, mobOverflowPx: mob, pageErrs, consoleErrs: errs.filter(e => !e.includes('auth/refresh')), badReq });
  } catch (e) { results.push({ route, url, len, error: String(e.message).slice(0, 100), pageErrs, consoleErrs: errs, badReq }); }
  page.off('console', onC); page.off('pageerror', onP); page.off('response', onR);
  console.log(route.padEnd(26), 'len=' + String(len).padEnd(6), 'mobOv=' + String(results[results.length - 1].mobOverflowPx ?? '?').padEnd(4), 'jsErr=' + pageErrs.length, 'badApi=' + badReq.length);
}
writeFileSync('route-audit-auth.json', JSON.stringify(results, null, 1));
await browser.close();
console.log('DONE');
