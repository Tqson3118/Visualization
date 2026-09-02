const fs = require('fs');

async function main() {
  const BASE_URL = 'http://localhost:5000/api/v1';

  // 1. Đăng nhập Teacher
  console.log('1. Đăng nhập Teacher...');
  const teacherLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'teacher@demo.local', password: 'Teacher@123' })
  }).then(r => r.json());

  const teacherToken = teacherLogin.accessToken || teacherLogin.data?.accessToken;
  console.log('Teacher Token:', teacherToken ? 'OK' : 'FAIL');


  // 2. Cập nhật Bài 1 (Lesson 151) có simulationKeys: ["sort.bubble"]
  console.log('2. Cập nhật Bài 1 (Lesson 151) gắn Visualizer sort.bubble...');
  const lesson1Res = await fetch(`${BASE_URL}/lessons/151`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      topicId: 1,
      title: 'Bài 1: Khái niệm Quy hoạch động & Mô phỏng Sắp xếp Mảng Trạng thái',
      description: 'Lý thuyết Quy hoạch động kết hợp trực quan hóa Simulator.',
      contentHtml: `# Quy hoạch Động (Dynamic Programming)

Quy hoạch động là một phương pháp tối ưu hóa cực kỳ mạnh mẽ trong Khoa học Máy tính.

## 1. Hai tính chất cốt lõi:
- **Bài toán con gối nhau (Overlapping Subproblems)**: Cùng một bài toán con được giải lặp đi lặp lại nhiều lần.
- **Cấu trúc con tối ưu (Optimal Substructure)**: Lời giải tối ưu của bài toán lớn được xây dựng từ lời giải tối ưu của các bài toán con.

## 2. Mô phỏng Sắp xếp & Cập nhật Trạng thái
Dưới đây là trình mô phỏng tương tác. Bạn có thể bấm **Chạy từng bước (Step)**, **Tự động chạy (Play)** hoặc nhập mảng tùy chỉnh để quan sát thuật toán hoạt động!`,
      status: 2, // Published
      sortOrder: 1,
      simulationKeys: ['sort.bubble']
    })
  }).then(r => r.json());
  console.log('Lesson 1 updated:', lesson1Res.data?.title || lesson1Res);

  // 3. Tạo Bài 2: Exercise Quiz MCQ từ các câu hỏi
  console.log('3. Tạo Bài 2 (Quiz MCQ)...');
  const quizExRes = await fetch(`${BASE_URL}/exercises`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      lessonId: 151,
      title: 'Bài 2: Trắc nghiệm Kiến thức Quy hoạch Động (Import từ Excel/CSV)',
      description: '3 câu hỏi trắc nghiệm kiểm tra độ hiểu bài về Overlapping Subproblems, Memoization và Fibonacci DP.',
      type: 0, // MCQ
      maxScore: 10,
      durationMinutes: 15,
      questions: [
        {
          content: 'Hai đặc trưng cốt lõi của bài toán Quy hoạch động là gì?',
          type: 'Single',
          options: [
            { text: 'Bài toán con gối nhau và Cấu trúc con tối ưu', isCorrect: true },
            { text: 'Chia để trị và Tham lam', isCorrect: false },
            { text: 'Đệ quy vô hạn và Quay lui', isCorrect: false },
            { text: 'Tìm kiếm nhị phân và Cây nhị phân', isCorrect: false }
          ],
          explanation: 'Quy hoạch động áp dụng khi có Overlapping Subproblems và Optimal Substructure.',
          points: 3
        },
        {
          content: 'Độ phức tạp thời gian khi tính số Fibonacci thứ n bằng Quy hoạch động (Tabulation) là gì?',
          type: 'Single',
          options: [
            { text: 'O(N)', isCorrect: true },
            { text: 'O(2^N)', isCorrect: false },
            { text: 'O(log N)', isCorrect: false },
            { text: 'O(1)', isCorrect: false }
          ],
          explanation: 'Tabulation tính từ 0 đến N chỉ mất một vòng lặp tuyến tính O(N).',
          points: 3
        },
        {
          content: 'Kỹ thuật Memoization trong Quy hoạch động áp dụng theo hướng tiếp cận nào?',
          type: 'Single',
          options: [
            { text: 'Top-down (Từ trên xuống)', isCorrect: true },
            { text: 'Bottom-up (Từ dưới lên)', isCorrect: false },
            { text: 'Random Access', isCorrect: false },
            { text: 'Greedy Choice', isCorrect: false }
          ],
          explanation: 'Memoization lưu kết quả đệ quy từ trên xuống (Top-down).',
          points: 4
        }
      ]
    })
  }).then(r => r.json());
  const quizId = quizExRes.data?.id;
  console.log('Quiz Exercise created with ID:', quizId);

  // 4. Tạo Bài 3: Exercise Code Lab
  console.log('4. Tạo Bài 3 (Code Lab Challenge)...');
  const codeConfig = {
    title: 'Tính số Fibonacci bằng Quy hoạch Động',
    description: 'Viết hàm fibonacci(n) trả về số Fibonacci thứ n (F(0)=0, F(1)=1, F(2)=1, F(3)=2...) với n <= 30.',
    signature: 'function fibonacci(n)',
    starterCode: `function fibonacci(n) {
  // TODO: Viết thuật toán Quy hoạch động ở đây
  if (n <= 0) return 0;
  if (n === 1) return 1;
  let dp = [0, 1];
  for (let i = 2; i <= n; i++) {
    dp[i] = dp[i - 1] + dp[i - 2];
  }
  return dp[n];
}`,
    language: 'javascript',
    testCases: [
      { name: 'Test 1: F(0) = 0', input: [0], expected: 0 },
      { name: 'Test 2: F(1) = 1', input: [1], expected: 1 },
      { name: 'Test 3: F(6) = 8', input: [6], expected: 8 },
      { name: 'Test 4: F(10) = 55', input: [10], expected: 55 },
      { name: 'Test 5: F(20) = 6765', input: [20], expected: 6765 }
    ]
  };

  const codeExRes = await fetch(`${BASE_URL}/exercises`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      lessonId: 151,
      title: 'Bài 3: Thử thách Lập trình — Tính số Fibonacci tối ưu DP',
      description: 'Lập trình giải bài toán Fibonacci bằng kỹ thuật Quy hoạch động.',
      type: 1, // Code
      maxScore: 100,
      configJson: JSON.stringify(codeConfig)
    })
  }).then(r => r.json());
  const codeExId = codeExRes.data?.id;
  console.log('Code Lab Exercise created with ID:', codeExId);

  // 5. Thêm Node Quiz (Node 2) và Node Lab (Node 3) vào Khóa học 33
  console.log('5. Thêm Node Quiz và Node Code Lab vào Lộ trình #33...');
  const node2Res = await fetch(`${BASE_URL}/concepts/courses/33/nodes`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'Bài 2: Trắc nghiệm Kiến thức Quy hoạch Động (Import Excel/CSV)',
      finalTestId: quizId,
      sortOrder: 2
    })
  }).then(r => r.json());
  console.log('Node 2 created:', node2Res);

  const node3Res = await fetch(`${BASE_URL}/concepts/courses/33/nodes`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'Bài 3: Thử thách Lập trình — Tính số Fibonacci tối ưu DP',
      finalTestId: codeExId,
      sortOrder: 3
    })
  }).then(r => r.json());
  console.log('Node 3 created:', node3Res);

  // 6. Admin duyệt lộ trình #33
  console.log('6. Đăng nhập Admin và duyệt lại lộ trình #33...');
  const adminLogin = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'admin@system.local', password: 'Admin@123' })
  }).then(r => r.json());
  const adminToken = adminLogin.accessToken || adminLogin.data?.accessToken;
  console.log('Admin Token:', adminToken ? 'OK' : 'FAIL');

  // Gửi review nếu cần
  await fetch(`${BASE_URL}/concepts/courses/33/submit-review`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  });

  const approveRes = await fetch(`${BASE_URL}/concepts/courses/33/review`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${adminToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ approve: true, reason: 'Duyệt lộ trình hoàn thiện 3 bài học.' })
  }).then(r => r.json());
  console.log('Admin Approve status:', approveRes);

  console.log('=== LỘ TRÌNH 3 BÀI ĐÃ SẴN SÀNG 100%! ===');
}

main().catch(console.error);
