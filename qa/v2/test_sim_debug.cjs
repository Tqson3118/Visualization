const { chromium } = require('@playwright/test');
const path = require('path');

const TARGET_URL = 'https://frontend-eta-ashen-89.vercel.app';

async function testSimulations() {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1280, height: 800 } });
  
  await page.goto(`${TARGET_URL}/simulations`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(1000);
  
  const cardsCount = await page.locator('.simulations__card').count();
  console.log('Simulations cards count on page 1:', cardsCount);
  
  const pageTitle = await page.locator('.simulations__title').innerText();
  console.log('Simulations page title:', pageTitle);
  
  const firstCardTitle = await page.locator('.simulations__card-title').first().innerText();
  console.log('First simulation card title:', firstCardTitle);
  
  await browser.close();
}

testSimulations().catch(console.error);
