const BASE_URL = 'http://localhost:5000/api/v1';

async function main() {
  // 1. Đăng nhập Teacher
  console.log('1. Đăng nhập Teacher để dọn dẹp cây giáo trình...');
  const teacherLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'teacher@demo.local', password: 'Teacher@123' })
  }).then(r => r.json());
  const teacherToken = teacherLogin.accessToken || teacherLogin.data?.accessToken;

  // Lấy toàn bộ items của khóa 33
  const tree = await fetch(`${BASE_URL}/paths/33/items`, {
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  }).then(r => r.json());
  const items = tree.data || tree || [];
  console.log('Current items in Course 33:', items.length);

  // Xóa toàn bộ các item cũ không thuộc Folder 168
  for (const item of items) {
    if (item.id !== 168 && item.parentId !== 168) {
      console.log(`Xóa item thừa: #${item.id} - ${item.title}`);
      await fetch(`${BASE_URL}/items/${item.id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${teacherToken}` }
      });
    }
  }

  // Đổi tên Folder 168 thành "Module 1: Quy hoạch Động Thực chiến 2026"
  await fetch(`${BASE_URL}/items/168`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'Module 1: Quy hoạch Động Thực chiến 2026',
      description: 'Lộ trình bài bản 3 bài: Lý thuyết + Visual, Quiz trắc nghiệm, Code Lab thử thách.'
    })
  });

  // Admin duyệt lại khóa học 33
  console.log('2. Admin duyệt lại khóa học 33...');
  const adminLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'admin@system.local', password: 'Admin@123' })
  }).then(r => r.json());
  const adminToken = adminLogin.accessToken || adminLogin.data?.accessToken;

  await fetch(`${BASE_URL}/concepts/courses/33/submit-review`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  });

  await fetch(`${BASE_URL}/concepts/courses/33/review`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${adminToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ approve: true, reason: 'Duyệt cây chuẩn 1 Module 3 bài.' })
  });

  // 3. Đăng nhập Student và kiểm tra trạng thái khóa tuần tự
  console.log('3. Đăng nhập Student và kiểm tra API khóa tuần tự...');
  const studentLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'student@demo.local', password: 'Student@123' })
  }).then(r => r.json());
  const studentToken = studentLogin.accessToken || studentLogin.data?.accessToken;

  // Lấy chi tiết khóa học phía Student
  const courseRes = await fetch(`${BASE_URL}/concepts/courses/33`, {
    headers: { 'Authorization': `Bearer ${studentToken}` }
  }).then(r => r.json());

  console.log('=== DANH SÁCH BÀI HỌC VÀ TRẠNG THÁI KHÓA PHÍA STUDENT ===');
  console.log('Course:', courseRes.title, '| Total Lessons:', courseRes.totalLessons);
  for (const l of (courseRes.lessons || [])) {
    console.log(` - [#${l.id}] ${l.title}`);
    console.log(`   + Module: ${l.moduleTitle} | Type: ${l.sandboxType} | Locked: ${l.locked} | Status: ${l.status}`);
  }
}

main().catch(console.error);
