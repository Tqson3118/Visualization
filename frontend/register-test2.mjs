const B = 'http://localhost:5050/api/v1';
const post = async (url, body) => {
  const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  const t = await r.text(); let j = null; try { j = JSON.parse(t); } catch {}
  return { status: r.status, j, t: t.slice(0, 200) };
};
import { readFileSync, existsSync } from 'fs';
const email = 'rescue.tester.2026@gmail.com', password = 'Rescue@2026';
const s1 = await post(B + '/auth/register/otp', { email });
console.log('otp:', s1.status, JSON.stringify(s1.j ?? s1.t).slice(0, 150));
let code = null;
for (let i = 0; i < 10; i++) {
  await new Promise(r => setTimeout(r, 1500));
  if (existsSync('smtp-mails.log')) {
    const mails = readFileSync('smtp-mails.log', 'utf8').trim().split('\n').filter(Boolean).map(l => { try { return JSON.parse(l); } catch { return null; } }).filter(Boolean);
    const last = mails[mails.length - 1];
    if (last && last.to.some(t => t.includes(email))) {
      code = (last.data.match(/\b(\d{6})\b/) ?? [])[1];
      console.log('mail found! code =', code);
      break;
    }
  }
}
if (!code) { console.log('NO MAIL CAPTURED'); process.exit(1); }
const s3 = await post(B + '/auth/register/otp/verify', { email, code });
console.log('verify:', s3.status, JSON.stringify(s3.j ?? s3.t).slice(0, 150));
const otpToken = s3.j?.otpToken ?? s3.j?.data?.otpToken;
const reg = await post(B + '/auth/register', { email, password, fullName: 'Rescue Tester', otpToken });
console.log('register:', reg.status, JSON.stringify(reg.j ?? reg.t).slice(0, 200));
const login = await post(B + '/auth/login', { email, password });
console.log('login:', login.status, JSON.stringify(login.j ?? login.t).slice(0, 300));
