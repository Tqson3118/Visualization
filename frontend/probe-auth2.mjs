const B = 'http://localhost:5000/api/v1';
const post = async (url, body) => {
  const r = await fetch(url, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
  return { status: r.status, text: (await r.text()).slice(0, 300) };
};
// 1. request OTP
console.log('otp-req:', JSON.stringify(await post(B + '/auth/register/otp', { email: 'rescue3@dsa.local' })));
// 2. read MailHog
try {
  const mh = await fetch('http://localhost:8025/api/v2/messages?limit=5');
  console.log('mailhog status:', mh.status);
  if (mh.ok) {
    const j = await mh.json();
    for (const m of (j.messages ?? [])) {
      console.log('MAIL:', (m.Content?.Headers?.Subject?.[0] ?? '(no subject)'), '->', (m.To ?? [{}])[0]?.Address ?? m.To);
    }
  }
} catch (e) { console.log('mailhog unreachable:', e.message); }
