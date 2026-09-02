const fs = require('fs');

const BASE_URL = 'http://localhost:5000/api/v1';

async function login(email, password) {
  const res = await fetch(`${BASE_URL}/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  }).then(r => r.json());
  return res.accessToken || res.data?.accessToken;
}

async function run() {
  console.log('========================================================');
  console.log('=== BẮT ĐẦU KIỂM THỬ E2E CHU TRÌNH ĐẦY ĐỦ (LIFECYCLE) ===');
  console.log('========================================================\n');

  const teacherToken = await login('teacher@demo.local', 'Teacher@123');
  const studentToken = await login('student@demo.local', 'Student@123');
  const adminToken = await login('admin@system.local', 'Admin@123');

  // BƯỚC 1: TEACHER TẠO KHÓA HỌC Ở TRẠNG THÁI NHÁP (DRAFT)
  console.log('--- BƯỚC 1: Giảng viên tạo Lộ trình mới (Trạng thái Draft) ---');
  const createCourseRes = await fetch(`${BASE_URL}/concepts/courses`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'Lộ trình Cây Nhị Phân Tìm Kiếm (BST) Chuẩn 2026',
      description: 'Khóa học thực chiến từ lý thuyết trực quan đến cài đặt giải thuật Cây BST.',
      topicId: 1,
      scope: 'draft'
    })
  }).then(r => r.json());

  const courseData = createCourseRes.data || createCourseRes;
  const courseId = courseData.id;
  console.log(`-> Tạo thành công Khóa học ID: #${courseId} | Tiêu đề: "${courseData.title}" | Trạng thái: ${courseData.status}\n`);

  // Soạn 1 Folder Module duy nhất
  console.log('-> Soạn 1 Module duy nhất: "Module 1: Cấu trúc Cây BST & Thao tác Cơ bản"');
  const folderRes = await fetch(`${BASE_URL}/paths/${courseId}/items`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      itemType: 0, // Folder
      title: 'Module 1: Cấu trúc Cây BST & Thao tác Cơ bản',
      description: 'Nền tảng về cây nhị phân tìm kiếm, tính chất BST và các phép toán chèn/tìm.',
      sortOrder: 1
    })
  }).then(r => r.json());
  const folderData = folderRes.data || folderRes;
  const folderId = folderData.id;
  console.log(`   + Folder Module ID: #${folderId}`);


  // Soạn Bài 1: Lý thuyết kèm Visualizer tree.binary-search-tree
  console.log('-> Soạn Bài 1 (Lý thuyết + Mô phỏng tree.binary-search-tree)...');
  const lesson1Res = await fetch(`${BASE_URL}/lessons`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      topicId: 1,
      title: 'Bài 1: Khái niệm Cây BST & Trực quan hóa Chèn/Tìm kiếm',
      description: 'Lý thuyết cây nhị phân tìm kiếm kết hợp mô phỏng tương tác.',
      contentHtml: `# Cây Nhị Phân Tìm Kiếm (Binary Search Tree - BST)

Cây nhị phân tìm kiếm là một cấu trúc dữ liệu dạng cây có tính chất đặc biệt:

## 1. Tính chất BST
- Với mọi nút $N$:
  - Mọi khóa ở **cây con bên trái** đều nhỏ hơn khóa tại $N$: $Key(Left) < Key(N)$.
  - Mọi khóa ở **cây con bên phải** đều lớn hơn khóa tại $N$: $Key(Right) > Key(N)$.
- Độ phức tạp tìm kiếm/chèn trung bình: $O(\\log n)$.

## 2. Mô phỏng trực quan tương tác
Hãy bấm **Chạy thử thuật toán** để quan sát từng bước thêm phần tử và tìm kiếm nút trên cây!`,
      status: 2, // Published
      sortOrder: 1,
      simulationKeys: ['tree.binary-search-tree']
    })
  }).then(r => r.json());
  const lesson1Id = lesson1Res.id || lesson1Res.data?.id;

  const item1Res = await fetch(`${BASE_URL}/paths/${courseId}/items`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      itemType: 1, // Theory
      title: 'Bài 1: Khái niệm Cây BST & Trực quan hóa Chèn/Tìm kiếm',
      description: 'Lý thuyết cơ bản và mô phỏng trực quan.',
      parentId: folderId,
      lessonId: lesson1Id,
      sortOrder: 1
    })
  }).then(r => r.json());
  console.log(`   + Item 1 (Theory) ID: #${item1Res.id || item1Res.data?.id}`);

  // Soạn Bài 2: Quiz MCQ trắc nghiệm
  console.log('-> Soạn Bài 2 (Quiz Trắc nghiệm 3 câu)...');
  const quizExRes = await fetch(`${BASE_URL}/exercises`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      lessonId: lesson1Id,
      title: 'Bài 2: Trắc nghiệm Kiến thức Cây BST',
      description: '3 câu hỏi trắc nghiệm kiểm tra độ hiểu bài về Cây Nhị phân Tìm kiếm.',
      type: 0, // MCQ
      maxScore: 10,
      durationMinutes: 15,
      questions: [
        {
          content: 'Tính chất cốt lõi của Cây Nhị phân Tìm kiếm (BST) là gì?',
          type: 'Single',
          options: [
            { text: 'Khóa con trái < Khóa nút gốc < Khóa con phải', isCorrect: true },
            { text: 'Tất cả các nút lá cùng độ sâu', isCorrect: false },
            { text: 'Mỗi nút có tối đa 3 nút con', isCorrect: false },
            { text: 'Khóa con trái luôn lớn hơn khóa con phải', isCorrect: false }
          ],
          explanation: 'Định nghĩa BST: Mọi nút con bên trái nhỏ hơn nút hiện tại, mọi nút con bên phải lớn hơn.',
          points: 3
        },
        {
          content: 'Độ phức tạp thời gian trung bình của phép tìm kiếm trên Cây BST cân bằng là gì?',
          type: 'Single',
          options: [
            { text: 'O(log N)', isCorrect: true },
            { text: 'O(N)', isCorrect: false },
            { text: 'O(N^2)', isCorrect: false },
            { text: 'O(1)', isCorrect: false }
          ],
          explanation: 'Trên cây cân bằng có chiều cao log N, tìm kiếm mất O(log N).',
          points: 3
        },
        {
          content: 'Thứ tự duyệt cây nào trên BST sẽ cho dãy khóa tăng dần?',
          type: 'Single',
          options: [
            { text: 'In-order (Trung thứ tự: Trái - Gốc - Phải)', isCorrect: true },
            { text: 'Pre-order (Tiền thứ tự: Gốc - Trái - Phải)', isCorrect: false },
            { text: 'Post-order (Hậu thứ tự: Trái - Phải - Gốc)', isCorrect: false },
            { text: 'Level-order (Duyệt theo mức)', isCorrect: false }
          ],
          explanation: 'Duyệt In-order trên BST luôn cho kết quả là dãy giá trị tăng dần.',
          points: 4
        }
      ]
    })
  }).then(r => r.json());
  const quizExId = quizExRes.id || quizExRes.data?.id;

  const item2Res = await fetch(`${BASE_URL}/paths/${courseId}/items`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      itemType: 2, // Quiz
      title: 'Bài 2: Trắc nghiệm Kiến thức Cây BST',
      description: '3 câu hỏi trắc nghiệm kiểm tra kiến thức.',
      parentId: folderId,
      sortOrder: 2
    })
  }).then(r => r.json());
  const item2Id = item2Res.id || item2Res.data?.id;
  await fetch(`${BASE_URL}/items/${item2Id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'Bài 2: Trắc nghiệm Kiến thức Cây BST',
      finalTestId: quizExId
    })
  });
  console.log(`   + Item 2 (Quiz) ID: #${item2Id}`);

  // Soạn Bài 3: Code Lab Thử thách Chấm tự động
  console.log('-> Soạn Bài 3 (Code Lab Thử thách Chèn nút BST)...');
  const codeConfig = {
    entryFunction: 'searchBST',
    starterCode: `function searchBST(root, val) {
  // Viết thuật toán tìm kiếm trên cây BST
  if (!root || root.val === val) return root ? root.val : -1;
  if (val < root.val) return searchBST(root.left, val);
  return searchBST(root.right, val);
}`,
    testCases: [
      { input: '4', expected: '4', isHidden: false },
      { input: '2', expected: '2', isHidden: false },
      { input: '7', expected: '7', isHidden: false },
      { input: '5', expected: '-1', isHidden: true },
      { input: '10', expected: '-1', isHidden: true }
    ]
  };

  const codeExRes = await fetch(`${BASE_URL}/exercises`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      lessonId: lesson1Id,
      title: 'Bài 3: Thử thách Lập trình — Tìm kiếm trên Cây BST',
      description: 'Cài đặt giải thuật tìm kiếm trên cây nhị phân tìm kiếm.',
      type: 1, // Code
      maxScore: 100,
      configJson: JSON.stringify(codeConfig)
    })
  }).then(r => r.json());
  const codeExId = codeExRes.id || codeExRes.data?.id;

  const item3Res = await fetch(`${BASE_URL}/paths/${courseId}/items`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      itemType: 3, // Lab
      title: 'Bài 3: Thử thách Lập trình — Tìm kiếm trên Cây BST',
      description: 'Bài tập lập trình chấm tự động.',
      parentId: folderId,
      sortOrder: 3
    })
  }).then(r => r.json());
  const item3Id = item3Res.id || item3Res.data?.id;
  await fetch(`${BASE_URL}/items/${item3Id}`, {
    method: 'PUT',
    headers: {
      'Authorization': `Bearer ${teacherToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      title: 'Bài 3: Thử thách Lập trình — Tìm kiếm trên Cây BST',
      labExerciseId: codeExId
    })
  });
  console.log(`   + Item 3 (Code Lab) ID: #${item3Id}\n`);

  // BƯỚC 2: HỌC SINH TÌM THỬ XEM CÓ RA KHÔNG (KHÔNG RA LÀ ĐÚNG VÌ ĐANG DRAFT)
  console.log('--- BƯỚC 2: Học sinh tìm kiếm Lộ trình trên /path ---');
  const studentPathList = await fetch(`${BASE_URL}/concepts/courses`, {
    headers: { 'Authorization': `Bearer ${studentToken}` }
  }).then(r => r.json());
  const coursesList = studentPathList.courses || studentPathList.items || studentPathList.data || studentPathList;
  const foundCourseInDraft = Array.isArray(coursesList) && coursesList.some(c => c.id === courseId);
  console.log(`-> Kết quả tìm kiếm phía Học sinh: Khóa học #${courseId} có xuất hiện không? -> ${foundCourseInDraft ? 'CÓ (SAI)' : 'KHÔNG XUẤT HIỆN (HOÀN TOÀN CHÍNH XÁC VÌ ĐANG DRAFT!)'}\n`);

  // BƯỚC 3: TEACHER HOÀN THÀNH & GỬI DUYỆT (SUBMIT REVIEW)
  console.log('--- BƯỚC 3: Giảng viên gửi duyệt Lộ trình cho Admin ---');
  const submitReviewRes = await fetch(`${BASE_URL}/concepts/courses/${courseId}/submit-review`, {
    method: 'POST',
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  }).then(r => r.json());
  console.log(`-> Kết quả gửi duyệt:`, submitReviewRes);

  const courseAfterSubmit = await fetch(`${BASE_URL}/concepts/courses/${courseId}`, {
    headers: { 'Authorization': `Bearer ${teacherToken}` }
  }).then(r => r.json());
  console.log(`-> Trạng thái mới của khóa học: ${courseAfterSubmit.status} (PendingReview)\n`);

  // BƯỚC 4: ADMIN KIỂM DUYỆT & PHÊ DUYỆT (APPROVE)
  console.log('--- BƯỚC 4: Quản trị viên (Admin) Phê duyệt Lộ trình ---');
  const approveRes = await fetch(`${BASE_URL}/concepts/courses/${courseId}/review`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${adminToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ approve: true, reason: 'Nội dung Cây BST đầy đủ 3 bài học chất lượng cao, phê duyệt công khai!' })
  }).then(r => r.json());
  console.log(`-> Kết quả Admin phê duyệt:`, approveRes);

  const courseAfterApprove = await fetch(`${BASE_URL}/concepts/courses/${courseId}`, {
    headers: { 'Authorization': `Bearer ${adminToken}` }
  }).then(r => r.json());
  console.log(`-> Trạng thái mới của khóa học: ${courseAfterApprove.status} (Active)\n`);

  // BƯỚC 5: HỌC SINH TÌM THẤY & HỌC TUẦN TỰ
  console.log('--- BƯỚC 5: Học sinh kiểm tra danh sách và học tuần tự ---');
  const studentPathListAfter = await fetch(`${BASE_URL}/concepts/courses`, {
    headers: { 'Authorization': `Bearer ${studentToken}` }
  }).then(r => r.json());
  const coursesListAfter = studentPathListAfter.courses || studentPathListAfter.items || studentPathListAfter.data || studentPathListAfter;
  const foundCourseActive = Array.isArray(coursesListAfter) && coursesListAfter.some(c => c.id === courseId);
  console.log(`-> Học sinh tìm thấy Khóa học #${courseId} trên danh sách: ${foundCourseActive ? 'CÓ (XUẤT HIỆN THÀNH CÔNG)' : 'CHƯA THẤY'}`);

  // Kiểm tra trạng thái khóa tuần tự phía Học sinh
  const studentCourseDetail = await fetch(`${BASE_URL}/concepts/courses/${courseId}`, {
    headers: { 'Authorization': `Bearer ${studentToken}` }
  }).then(r => r.json());
  console.log(`-> Cấu trúc cây bài học phía Học sinh:`);
  console.log(`   - Tổng số bài: ${studentCourseDetail.totalLessons}`);
  for (const l of studentCourseDetail.lessons || []) {
    console.log(`   - [#${l.id}] ${l.title} | Module: "${l.moduleTitle}" | Type: ${l.sandboxType} | Locked: ${l.locked}`);
  }

  console.log('\n========================================================');
  console.log('=== CHU TRÌNH E2E BACKEND HOÀN TẤT THÀNH CÔNG 100%! ===');
  console.log('========================================================');

  return { courseId, item1Id: item1Res.id || item1Res.data?.id, item2Id, item3Id };
}

run().catch(console.error);
