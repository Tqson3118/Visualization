const post = async (url, body) => {
  try {
    const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
    const text = await r.text();
    return r.status + ' ' + text.slice(0, 250);
  } catch (e) { return 'FETCH_ERR ' + e.message; }
};
const B = 'http://localhost:5000/api/v1';
console.log('register:', await post(B + '/auth/register', { email: 'rescue2@dsa.local', password: 'Rescue@12345', fullName: 'Rescue Tester' }));
console.log('login admin:', await post(B + '/auth/login', { email: 'admin@dsa-visual.local', password: 'ChangeMe@123' }));
console.log('login rescue2:', await post(B + '/auth/login', { email: 'rescue2@dsa.local', password: 'Rescue@12345' }));
