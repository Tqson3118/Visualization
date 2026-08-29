const B = 'http://localhost:5050/api/v1';
const post = async (url, body) => {
  const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  const t = await r.text(); let j = null; try { j = JSON.parse(t); } catch {}
  return { status: r.status, j, t };
};
const email = 'rescue.tester.v3@gmail.com', password = 'Rescue@2026';
const s1 = await post(B + '/auth/register/otp', { email });
console.log('1.otp:', s1.status, JSON.stringify(s1.j ?? s1.t).slice(0, 100));
const s3 = await post(B + '/auth/register/otp/verify', { email, code: '999999' });
console.log('2.verify:', s3.status, JSON.stringify(s3.j ?? s3.t).slice(0, 120));
const otpToken = s3.j?.otpToken ?? s3.j?.data?.otpToken;
const reg = await post(B + '/auth/register', { email, password, fullName: 'Rescue Tester', otpToken });
console.log('3.register:', reg.status, JSON.stringify(reg.j ?? reg.t).slice(0, 200));
const login = await post(B + '/auth/login', { email, password });
console.log('4.login:', login.status, JSON.stringify(login.j ?? login.t).slice(0, 200));
import { writeFileSync } from 'fs';
const tok = login.j?.accessToken ?? login.j?.data?.accessToken ?? login.j?.token;
if (tok) { writeFileSync('test-token.txt', tok); console.log('TOKEN SAVED'); }
