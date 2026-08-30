const post = async (url, body) => {
  const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  return { status: r.status, body: (await r.text()).slice(0, 400) };
};
const B = 'http://localhost:5000/api/v1';
const s = await post(B + '/auth/login', { email: 'student@demo.local', password: 'Student@123' });
console.log('STUDENT login:', s.status, s.body);
const t = await post(B + '/auth/login', { email: 'teacher@demo.local', password: 'Teacher@123' });
console.log('TEACHER login:', t.status, t.body.slice(0, 200));
const a = await post(B + '/auth/login', { email: 'admin@system.local', password: 'Admin@123' });
console.log('ADMIN login:', a.status, a.body.slice(0, 200));
