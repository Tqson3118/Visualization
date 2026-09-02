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

  // Lấy cây outline hiện tại của lộ trình #33
  const tree = await fetch(`${BASE_URL}/paths/33/items`, {
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  }).then(r => r.json());

  console.log('Current Tree items count:', tree.data?.length || tree.length || 0);

  // Tìm folder module ID
  let folderId = null;
  const items = tree.data || tree;
  if (Array.isArray(items) && items.length > 0) {
    const folder = items.find(it => it.itemType === 0 || it.itemType === 'Folder');
    if (folder) folderId = folder.id;
  }

  if (!folderId) {
    console.log('Tạo Folder Module...');
    const createFolder = await fetch(`${BASE_URL}/paths/33/items`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${teacherToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        itemType: 0, // Folder
        title: 'Module 1: Nguyên lý Quy hoạch Động & Thực hành Trực quan',
        description: 'Chương học cơ bản về Quy hoạch động từ lý thuyết đến thực hành.',
        sortOrder: 1
      })
    }).then(r => r.json());
    folderId = createFolder.data?.id || createFolder.id;
    console.log('Folder Module created with ID:', folderId);
  } else {
    console.log('Using existing Folder Module ID:', folderId);
  }

  // 2. Cập nhật / Tạo Bài 1: Lý thuyết kèm Visualizer sort.bubble
  console.log('2. Tạo Bài 1 (Theory + Visualizer)...');
  const lesson1 = await fetch(`${BASE_URL}/lessons/151`, {
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

Quy hoạch động là một kỹ thuật thiết kế thuật toán tối ưu hóa dựa trên hai nguyên lý cốt lõi:

## 1. Hai nguyên lý cơ bản
- **Bài toán con gối nhau (Overlapping Subproblems)**: Cùng một bài toán con được giải lặp đi lặp lại nhiều lần.
- **Cấu trúc con tối ưu (Optimal Substructure)**: Lời giải tối ưu của bài toán lớn được xây dựng từ lời giải tối ưu của các bài toán con.

## 2. Mô phỏng Sắp xếp & Cập nhật Trạng thái
Dưới đây là trình mô phỏng trực quan tương tác. Bạn có thể bấm **Chạy từng bước (Step)**, **Tự động chạy (Play)** hoặc nhập mảng tùy chỉnh để quan sát các phần tử đổi chỗ và cập nhật trạng thái thuật toán!`,
      status: 2, // Published
      sortOrder: 1,
      simulationKeys: ['sort.bubble']
    })
  }).then(r => r.json());

  // Tạo / Cập nhật Item 1 trong Folder
  const item1 = await fetch(`${BASE_URL}/paths/33/items`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      itemType: 1, // Theory
      title: 'Bài 1: Khái niệm Quy hoạch động & Mô phỏng Sắp xếp Mảng Trạng thái',
      description: 'Lý thuyết cơ bản và mô phỏng trực quan.',
      parentId: folderId,
      lessonId: 151,
      sortOrder: 1
    })
  }).then(r => r.json());
  console.log('Item 1 (Theory):', item1.data?.id || item1.id);

  // 3. Tạo Bài 2: Quiz Trắc nghiệm (Exercise MCQ từ CSV)
  console.log('3. Tạo Bài 2 (Quiz MCQ)...');
  const quizEx = await fetch(`${BASE_URL}/exercises`, {
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
  const quizExId = quizEx.data?.id || quizEx.id || 1;

  const item2 = await fetch(`${BASE_URL}/paths/33/items`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      itemType: 2, // Quiz
      title: 'Bài 2: Trắc nghiệm Kiến thức Quy hoạch Động (Import Excel/CSV)',
      description: '3 câu hỏi trắc nghiệm kiểm tra kiến thức.',
      parentId: folderId,
      sortOrder: 2
    })
  }).then(r => r.json());
  const item2Id = item2.data?.id || item2.id;
  if (item2Id && quizExId) {
    await fetch(`${BASE_URL}/items/${item2Id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${teacherToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        title: 'Bài 2: Trắc nghiệm Kiến thức Quy hoạch Động (Import Excel/CSV)',
        finalTestId: quizExId
      })
    });
  }
  console.log('Item 2 (Quiz):', item2Id, 'QuizExId:', quizExId);

  // 4. Tạo Bài 3: Code Lab Thử thách Code Chấm tự động
  console.log('4. Tạo Bài 3 (Code Lab Challenge)...');
  const codeConfig = {
    entryFunction: 'fibonacci',
    starterCode: `function fibonacci(n) {\n  // Viết thuật toán Quy hoạch động ở đây\n  if (n <= 0) return 0;\n  if (n === 1) return 1;\n  let dp = [0, 1];\n  for (let i = 2; i <= n; i++) {\n    dp[i] = dp[i - 1] + dp[i - 2];\n  }\n  return dp[n];\n}`,
    testCases: [
      { input: '0', expected: '0', isHidden: false },
      { input: '1', expected: '1', isHidden: false },
      { input: '6', expected: '8', isHidden: false },
      { input: '10', expected: '55', isHidden: true },
      { input: '20', expected: '6765', isHidden: true }
    ]
  };

  const codeEx = await fetch(`${BASE_URL}/exercises`, {
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
  const codeExId = codeEx.data?.id || codeEx.id || 2;

  const item3 = await fetch(`${BASE_URL}/paths/33/items`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      itemType: 3, // Lab
      title: 'Bài 3: Thử thách Lập trình — Tính số Fibonacci tối ưu DP',
      description: 'Bài tập lập trình chấm tự động.',
      parentId: folderId,
      sortOrder: 3
    })
  }).then(r => r.json());
  const item3Id = item3.data?.id || item3.id;
  if (item3Id && codeExId) {
    await fetch(`${BASE_URL}/items/${item3Id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${teacherToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        title: 'Bài 3: Thử thách Lập trình — Tính số Fibonacci tối ưu DP',
        labExerciseId: codeExId
      })
    });
  }
  console.log('Item 3 (Lab):', item3Id, 'CodeExId:', codeExId);

  // 5. Admin duyệt lại lộ trình #33
  console.log('5. Admin duyệt lộ trình #33...');
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

  const approveRes = await fetch(`${BASE_URL}/concepts/courses/33/review`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${adminToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ approve: true, reason: 'Duyệt hoàn thiện 3 bài học.' })
  }).then(r => r.json());
  console.log('Approve result:', approveRes);

  console.log('=== SETUP HOÀN TẤT TRỌN VẸN 100%! ===');
}

main().catch(console.error);
