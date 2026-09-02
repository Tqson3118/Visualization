const BASE_URL = 'http://localhost:5000/api/v1';

async function main() {
  const teacherLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'teacher@demo.local', password: 'Teacher@123' })
  }).then(r => r.json());
  const teacherToken = teacherLogin.accessToken || teacherLogin.data?.accessToken;

  const tree = await fetch(`${BASE_URL}/paths/34/items`, {
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  }).then(r => r.json());
  const items = tree.data || tree || [];
  console.log('Current items in Course 34:', items.length);

  for (const item of items) {
    if (item.id !== 188 && item.parentId !== 188) {
      console.log(`Xóa item thừa: #${item.id} - ${item.title}`);
      await fetch(`${BASE_URL}/items/${item.id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${teacherToken}` }
      });
    }
  }

  // Admin duyệt lại khóa 34
  const adminLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'admin@system.local', password: 'Admin@123' })
  }).then(r => r.json());
  const adminToken = adminLogin.accessToken || adminLogin.data?.accessToken;

  await fetch(`${BASE_URL}/concepts/courses/34/submit-review`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  });

  await fetch(`${BASE_URL}/concepts/courses/34/review`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${adminToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ approve: true, reason: 'Duyệt sạch 1 Module 3 bài.' })
  });

  // Kiểm tra chi tiết khóa 34 phía Student
  const studentLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'student@demo.local', password: 'Student@123' })
  }).then(r => r.json());
  const studentToken = studentLogin.accessToken || studentLogin.data?.accessToken;

  const courseRes = await fetch(`${BASE_URL}/concepts/courses/34`, {
    headers: { 'Authorization': `Bearer ${studentToken}` }
  }).then(r => r.json());

  console.log('=== KẾT QUẢ KHÓA HỌC 34 PHÍA STUDENT ===');
  console.log('Course:', courseRes.title, '| Total Lessons:', courseRes.totalLessons);
  for (const l of (courseRes.lessons || [])) {
    console.log(` - [#${l.id}] ${l.title} | Module: "${l.moduleTitle}" | Type: ${l.sandboxType} | Locked: ${l.locked}`);
  }
}

main().catch(console.error);
