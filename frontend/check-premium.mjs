const B = 'http://localhost:5000/api/v1';
const login = await fetch(B + '/auth/login', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email: 'student@demo.local', password: 'Student@123' }) });
const { accessToken } = await login.json();
const r = await fetch(B + '/premium/status', { headers: { Authorization: 'Bearer ' + accessToken } });
console.log('GET /premium/status →', r.status, (await r.text()).slice(0, 300));
