const B = 'http://localhost:5050/api/v1';
const post = async (url, body, token) => {
  const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: 'Bearer ' + token } : {}) }, body: JSON.stringify(body) });
  const t = await r.text();
  let j = null; try { j = JSON.parse(t); } catch {}
  return { status: r.status, j, t: t.slice(0, 200) };
};
import { readFileSync } from 'fs';
const email = 'rescue-test@dsa.local', password = 'Rescue@2026';
const step = async (name, p) => { console.log(name, p.status, JSON.stringify(p.j ?? p.t).slice(0, 180)); return p; };

// 1. OTP request
const s1 = await step('otp:', await post(B + '/auth/register/otp', { email }));
if (s1.status !== 200) { console.log('STOP'); process.exit(1); }
// 2. Read caught mail
await new Promise(r => setTimeout(r, 2500));
const mails = readFileSync('smtp-mails.log', 'utf8').trim().split('\n').filter(Boolean).map(l => JSON.parse(l));
const mail = mails[mails.length - 1];
const code = (mail.data.match(/\b(\d{6})\b/) ?? [])[1];
console.log('OTP code from mail:', code, '| mail snippet:', mail.data.replace(/\s+/g, ' ').slice(0, 120));
// 3. Verify OTP
const s3 = await step('verify:', await post(B + '/auth/register/otp/verify', { email, code }));
const otpToken = s3.j?.otpToken ?? s3.j?.data?.otpToken;
// 4. Register
const reg = await step('register:', await post(B + '/auth/register', { email, password, fullName: 'Rescue Tester', otpToken }));
// 5. Login (UI dùng form; API login trả token)
const login = await post(B + '/auth/login', { email, password });
console.log('login:', login.status, JSON.stringify(login.j ?? login.t).slice(0, 250));
if (login.j?.accessToken ?? login.j?.data?.accessToken) {
  const tok = login.j.accessToken ?? login.j.data.accessToken;
  const me = await fetch(B + '/auth/me', { headers: { Authorization: 'Bearer ' + tok } });
  console.log('me:', me.status, (await me.text()).slice(0, 200));
}
