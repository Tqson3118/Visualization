// Audit tự động toàn bộ route — DSA Visual rescue sweep
import { chromium } from 'playwright';
import { writeFileSync } from 'fs';

const BASE = process.env.BASE_URL || 'http://localhost:5174';
const ROUTES = ["/","/login","/register","/forgot-password","/learn","/path","/simulations","/simulator/sort.bubble","/simulator/search.binary","/simulator/graph.bfs","/sorting-sandbox","/searching-sandbox","/stack-queue-sandbox","/cheatsheet","/dashboard","/profile","/leaderboard","/quests","/shop","/premium","/account/subscription","/classes","/studio","/teacher","/admin/content","/admin","/admin/users","/admin/stats","/admin/settings","/admin/ladder","/admin/feedback","/help","/privacy","/nonexistent-page-xyz"];

const browser = await chromium.launch();
const ctx = await browser.newContext({ viewport: { width: 1366, height: 900 } });
const page = await ctx.newPage();
const results = [];

for (const route of ROUTES) {
  const errs = [], pageErrs = [], badReq = [];
  const onConsole = m => { if (m.type() === 'error') errs.push(m.text().slice(0, 200)); };
  const onPage = e => pageErrs.push(String(e.message).slice(0, 200));
  const onResp = r => { if (r.status() >= 400) badReq.push(r.status() + ' ' + r.url().slice(0, 120)); };
  page.on('console', onConsole); page.on('pageerror', onPage); page.on('response', onResp);
  let url = '', len = 0, overX = false;
  try {
    await page.goto(BASE + route, { waitUntil: 'domcontentloaded', timeout: 20000 });
    await page.waitForTimeout(1800);
    url = page.url();
    len = (await page.evaluate(() => document.body.innerText.length)).valueOf();
    overX = await page.evaluate(() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 2);
  } catch (e) { pageErrs.push('NAV: ' + String(e.message).slice(0, 120)); }
  page.off('console', onConsole); page.off('pageerror', onPage); page.off('response', onResp);
  const strip = s => s.length > 1 && s.endsWith('/') ? s.slice(0, -1) : s;
  const redirected = !strip(url).endsWith(strip(route));
  results.push({ route, finalUrl: url.replace(BASE, ''), redirected, bodyLen: len, overX,
    jsErrors: pageErrs, consoleErrors: errs.filter(e => !e.includes('auth/refresh')), badRequests: badReq.filter(r => !r.includes('auth/refresh')) });
  console.log(route.padEnd(28), redirected ? '-> ' + url.replace(BASE, '') : 'ok', 'len=' + len, 'err=' + (pageErrs.length + errs.length));
}
writeFileSync('route-audit.json', JSON.stringify(results, null, 1));
await browser.close();
console.log('DONE. Wrote route-audit.json');
