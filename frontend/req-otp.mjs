const email = process.argv[2] ?? 'rescue.tester.v3@gmail.com';
const r = await fetch('http://localhost:5050/api/v1/auth/register/otp', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email }) });
console.log(r.status, (await r.text()).slice(0, 120));
