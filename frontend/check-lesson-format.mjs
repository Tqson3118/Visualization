const B = 'http://localhost:5000/api/v1';
const login = await fetch(B + '/auth/login', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ email: 'teacher@demo.local', password: 'Teacher@123' }) });
const { accessToken } = await login.json();
const r = await fetch(B + '/lessons?page=1&pageSize=5', { headers: { Authorization: 'Bearer ' + accessToken } });
const page = await r.json();
console.log('tổng bài:', page.totalCount ?? '?');
for (const l of (page.items ?? []).slice(0, 5)) {
  const d = await (await fetch(B + '/lessons/' + l.id + '?includeContent=true', { headers: { Authorization: 'Bearer ' + accessToken } })).json();
  const c = (d.contentHtml ?? d.contentMd ?? '').slice(0, 120).replace(/\n/g, ' ');
  console.log('#' + l.id, l.title.slice(0, 40), '| bắt đầu bằng:', JSON.stringify(c));
}
