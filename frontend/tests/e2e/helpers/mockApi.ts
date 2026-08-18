/**
 * Route-mock backend cho E2E (Playwright).
 *
 * Nguyên tắc (tests/e2e/README.md cũ): CẤM để test E2E phụ thuộc dữ liệu backend thật.
 * Mọi request /api/v1/* bị chặn ở tầng network (page.route — glob chính xác trong code
 * bên dưới) và trả dữ liệu giả ĐÚNG SHAPE theo `src/api/*` + `src/stores/*`
 * (endpoint nào mock — liệt kê trong README).
 *
 * Lưu ý shape:
 * - axios baseURL = '/api/v1' (src/api/client.ts) → URL thật trong browser:
 *   http://localhost:5173/api/v1/... (Vite proxy /api → :5000 nhưng route chặn TRƯỚC proxy).
 * - Auth: token chỉ trong memory Pinia (ADR-004) → spec phải login qua UI trong cùng phiên
 *   (xem helpers/auth.ts) — không có localStorage token để seed.
 * - Lỗi trả về theo ApiErrorBody: { error: { code, message, field, details } } (client.ts).
 */
import type { Page, Route } from '@playwright/test';

import type { UserSummary } from '../../../src/api/auth';
import type { LessonDto, Topic } from '../../../src/api/lessons';
import type { HeartsStatusDto, LearningPathDto } from '../../../src/api/gamification';
import type { ProgressOverviewDto } from '../../../src/api/progress';
import type { ClassDto } from '../../../src/api/types';

export const MOCK_ACCESS_TOKEN = 'e2e-access-token';

export const MOCK_USER: UserSummary = {
  id: 1,
  displayName: 'E2E Student',
  email: 'e2e@test.edu',
  role: 'STUDENT',
  avatarUrl: null,
  createdAt: '2026-08-12T00:00:00.000Z',
};

/** GET /topics — LadderView + PathRedirectView dùng lessonStore.fetchTopics()/lessonsApi.fetchTopics() */
const MOCK_TOPICS: Topic[] = [
  {
    id: 1,
    parentId: null,
    name: 'Sắp xếp & Tìm kiếm',
    description: 'Bubble, Selection, Insertion, Merge, Quick, Heap + tìm kiếm tuyến tính/nhị phân',
    sortOrder: 1,
    children: [],
  },
];

/** GET /classes — LeaderboardView tab Lớp (G-F3E-NEW-2: resolveClassId → classStore.fetchClasses) */
const MOCK_CLASSES: ClassDto[] = [
  {
    id: 7,
    name: 'Lớp DSA 01',
    description: 'Nhóm 2 — CTDL & Giải thuật',
    inviteCode: 'ABC123',
    ownerId: 1,
    memberCount: 3,
    createdAt: '2026-08-01T00:00:00.000Z',
    role: 'STUDENT',
  },
];

/** GET /learning-path/1 — PathView (Màn 13) bản đồ node; node 1 = Bubble Sort active */
const MOCK_LEARNING_PATH: LearningPathDto = {
  id: 1,
  name: 'Sắp xếp & Tìm kiếm',
  description: 'Các thuật toán sắp xếp và tìm kiếm trên mảng',
  progressPct: 0,
  nodes: [
    {
      id: 1,
      title: 'Sắp xếp nổi bọt (Bubble Sort)',
      description: 'Độ phức tạp: O(n²) · Không gian: O(1)',
      sortOrder: 1,
      status: 'active',
      stars: 0,
      bestScore: null,
      lessonId: 1,
      simulationKey: 'sort.bubble',
      exerciseId: null,
      requiredStages: { quiz: true, lab: true, code: false },
    },
    {
      id: 2,
      title: 'Sắp xếp chọn (Selection Sort)',
      description: 'Độ phức tạp: O(n²) · Không gian: O(1)',
      sortOrder: 2,
      status: 'locked',
      stars: 0,
      bestScore: null,
      lessonId: null,
      simulationKey: 'sort.selection',
      exerciseId: null,
      requiredStages: { quiz: true, lab: true, code: false },
    },
  ],
  finalTestUnlocked: false,
};

/** GET /progress/me — PathRedirectView + ProfileView (progressStore.fetchOverview) */
const MOCK_PROGRESS_OVERVIEW: ProgressOverviewDto = {
  // G-F2d: seed vài chủ đề + số liệu để skill radar / level progress render ảnh verify §6.2
  lessonsViewed: 3,
  lessonsTotal: 12,
  exercisesCompleted: 2,
  exercisesTotal: 10,
  avgScore: 7.5,
  topics: [
    {
      id: 1,
      name: 'Sắp xếp & Tìm kiếm',
      progressPct: 45,
      lessons: [
        { id: 1, title: 'Sắp xếp nổi bọt (Bubble Sort)', viewed: true, bestScore: 8, completed: true },
        { id: 2, title: 'Tìm kiếm nhị phân', viewed: false, bestScore: null, completed: false },
      ],
    },
    {
      id: 2,
      name: 'CTDL tuyến tính',
      progressPct: 30,
      lessons: [
        { id: 3, title: 'Ngăn xếp (Stack)', viewed: true, bestScore: 6, completed: false },
        { id: 4, title: 'Hàng đợi (Queue)', viewed: false, bestScore: null, completed: false },
      ],
    },
    {
      id: 3,
      name: 'Cây',
      progressPct: 15,
      lessons: [
        { id: 5, title: 'Cây nhị phân tìm kiếm', viewed: false, bestScore: null, completed: false },
      ],
    },
    {
      id: 4,
      name: 'Bảng băm',
      progressPct: 5,
      lessons: [
        { id: 6, title: 'Bảng băm cơ bản', viewed: false, bestScore: null, completed: false },
      ],
    },
    {
      id: 5,
      name: 'Đồ thị',
      progressPct: 10,
      lessons: [
        { id: 7, title: 'Duyệt BFS/DFS', viewed: false, bestScore: null, completed: false },
      ],
    },
  ],
};

/**
 * GET /leaderboard — LeaderboardView (Màn 24). G-F2d mock: PagedResponse shape
 * (items/page/pageSize/total/totalPages), 14 người, pageSize 10. Thứ tự theo tab
 * KHÁC nhau để ảnh verify chứng minh reorder animation khi đổi tab.
 * userId 1 = "E2E Student" (MOCK_USER) → api tự suy myRank → dòng "Bạn" ghim cuối.
 */
const MOCK_LEADERBOARD_PLAYERS: Array<{
  id: number;
  name: string;
  streak?: number;
  level?: number;
  week: number;
  levelScore: number;
  classScore: number;
}> = [
  { id: 11, name: 'Nguyễn Minh Anh', streak: 12, level: 24, week: 1280, levelScore: 2450, classScore: 980 },
  { id: 12, name: 'Trần Quốc Bảo', streak: 9, level: 21, week: 1150, levelScore: 2100, classScore: 940 },
  { id: 13, name: 'Lê Thị Cẩm Tú', streak: 15, level: 26, week: 1090, levelScore: 2680, classScore: 920 },
  { id: 14, name: 'Phạm Văn Dũng', streak: 6, level: 18, week: 940, levelScore: 1850, classScore: 890 },
  { id: 15, name: 'Hoàng Thu Hà', streak: 11, level: 22, week: 870, levelScore: 1980, classScore: 860 },
  { id: 16, name: 'Võ Ngọc Khánh', streak: 4, level: 15, week: 760, levelScore: 1490, classScore: 830 },
  { id: 17, name: 'Đặng Minh Long', streak: 8, level: 19, week: 690, levelScore: 1720, classScore: 800 },
  { id: 1, name: 'E2E Student', streak: 3, level: 9, week: 620, levelScore: 980, classScore: 770 },
  { id: 18, name: 'Bùi Thanh Mai', streak: 5, level: 16, week: 580, levelScore: 1410, classScore: 750 },
  { id: 19, name: 'Đỗ Xuân Nam', streak: 2, level: 12, week: 510, levelScore: 1120, classScore: 720 },
  { id: 20, name: 'Cao Thị Ngọc', streak: 7, level: 17, week: 470, levelScore: 1550, classScore: 700 },
  { id: 21, name: 'Lâm Vĩnh Phúc', streak: 1, level: 10, week: 410, levelScore: 1020, classScore: 680 },
  { id: 22, name: 'Hồ Nhật Quang', streak: 0, level: 8, week: 360, levelScore: 860, classScore: 650 },
  { id: 23, name: 'Vương Gia Thịnh', streak: 0, level: 7, week: 300, levelScore: 740, classScore: 620 },
];

function mockLeaderboardPage(tab: string, page: number): {
  items: Array<{
    rank: number;
    userId: number;
    displayName: string;
    avatarUrl: null;
    value: number;
    streak?: number;
    level?: number;
  }>;
  totalPages: number;
  total: number;
} {
  // Thứ tự theo tab (khác nhau → reorder animation khi đổi tab):
  // week: xếp theo week desc · level: theo levelScore desc · class: theo classScore desc
  const ordered = [...MOCK_LEADERBOARD_PLAYERS].sort((a, b) => {
    if (tab === 'level') return b.levelScore - a.levelScore;
    if (tab === 'class') return b.classScore - a.classScore;
    return b.week - a.week;
  });
  const pageSize = 10;
  const total = ordered.length;
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const slice = ordered.slice((page - 1) * pageSize, page * pageSize);
  const items = slice.map((player, i) => ({
    rank: (page - 1) * pageSize + i + 1,
    userId: player.id,
    displayName: player.name,
    avatarUrl: null,
    value: tab === 'level' ? player.levelScore : tab === 'class' ? player.classScore : player.week,
    streak: tab === 'week' ? player.streak : undefined,
    level: tab === 'level' ? player.level : undefined,
  }));
  return { items, totalPages, total };
}

/** GET /lessons/{id} — NodeHubView (lessonStore.fetchLesson) */
const MOCK_LESSON: LessonDto = {
  id: 1,
  title: 'Sắp xếp nổi bọt (Bubble Sort)',
  description: 'Thuật toán sắp xếp cơ bản',
  topicId: 1,
  sortOrder: 1,
  status: 'active',
  simulationCount: 1,
  exerciseCount: 1,
  progress: null,
  contentHtml: '<p>Bubble Sort — so sánh và hoán đổi liên tiếp.</p>',
  // G-F2b: thêm data liên kết để Màn 04 render thẻ mô phỏng/bài tập (ảnh verify §6.2)
  simulations: [{ simulationKey: 'sort.bubble', title: 'Sắp xếp nổi bọt (Bubble Sort)' }],
  exercises: [{ id: 1, title: 'Trắc nghiệm Bubble Sort', type: 'MCQ' }],
};

/** GET /exercises/1 — ExerciseView (Màn 06) render quiz thật (ảnh verify §6.2) */
const MOCK_EXERCISE = {
  id: 1,
  title: 'Trắc nghiệm Bubble Sort',
  description: 'Kiểm tra hiểu biết về thuật toán Bubble Sort',
  type: 'MCQ',
  lessonId: 1,
  nodeId: 1,
  stage: 1,
  durationMinutes: 10,
  maxScore: 4,
  status: 'active',
  questions: [
    {
      id: 1,
      content: 'Bubble Sort là thuật toán sắp xếp thuộc nhóm nào?',
      type: 'SINGLE',
      options: ['So sánh & hoán đổi', 'Chèn từng phần tử', 'Chia để trị', 'Đếm phân phối'],
      points: 1,
    },
    {
      id: 2,
      content: 'Độ phức tạp trung bình của Bubble Sort là gì?',
      type: 'SINGLE',
      options: ['O(n)', 'O(n log n)', 'O(n²)', 'O(log n)'],
      points: 1,
    },
    {
      id: 3,
      content: 'Sau lượt duyệt đầu tiên, phần tử lớn nhất nằm ở đâu?',
      type: 'SINGLE',
      options: ['Đầu mảng', 'Cuối mảng', 'Giữa mảng', 'Không xác định'],
      points: 1,
    },
    {
      id: 4,
      content: 'Bubble Sort ổn định (stable) hay không?',
      type: 'SINGLE',
      options: ['Ổn định', 'Không ổn định', 'Tùy dữ liệu', 'Chỉ với mảng chẵn'],
      points: 1,
    },
  ],
};


/** GET /concepts/courses & /concepts/courses/{id} — CoursesListView/CourseDetailView (B4).
 *  Shape khớp CourseListDto/CourseDetailDto (src/services/courseApi.ts). */
const MOCK_COURSE_LIST: Array<Record<string, unknown>> = [
  {
    id: '1',
    title: 'Lộ trình nền tảng DSA',
    description: 'Mảng, danh sách liên kết, ngăn xếp, hàng đợi và các thuật toán sắp xếp cơ bản.',
    category: 'Algorithm',
    difficulty: 'Beginner',
    isPremium: false,
    isPublished: true,
    xpReward: 240,
    totalLessons: 6,
    completedLessons: 1,
    progressPercent: 16,
  },
  {
    id: '2',
    title: 'Lộ trình Cây & Đồ thị',
    description: 'BST, AVL, BFS/DFS, Dijkstra.',
    category: 'Tree/Graph',
    difficulty: 'Intermediate',
    isPremium: true,
    isPublished: true,
    xpReward: 320,
    totalLessons: 8,
    completedLessons: 0,
    progressPercent: 0,
  },
];

const MOCK_COURSE_DETAIL: Record<string, unknown> = {
  id: '1',
  title: 'Lộ trình nền tảng DSA',
  description:
    'Bắt đầu từ Mảng, danh sách liên kết, ngăn xếp, hàng đợi rồi tới các thuật toán sắp xếp — mỗi chặng có lý thuyết, mô phỏng trực quan, quiz và codelab.',
  category: 'Algorithm',
  difficulty: 'Beginner',
  isPremium: false,
  isPublished: true,
  progressPercent: 16,
  xpReward: 240,
  learningObjectives: ['Hiểu mảng và các thao tác cơ bản', 'Nắm vững Bubble/Selection/Insertion Sort'],
  keyOutcomes: ['Giải được bài sắp xếp cơ bản', 'Nhận diện độ phức tạp O(n²)'],
  rating: 4.8,
  ratingCount: 12,
  highlights: [
    { title: 'Lý thuyết tinh gọn', description: 'Mỗi bài có bài giảng ngắn kèm mã giả trực quan.' },
    { title: 'Mô phỏng từng bước', description: 'Chạy thử thuật toán ngay trong bài học.' },
  ],
  testimonials: [
    { name: 'Minh Anh', role: 'Sinh viên', quote: 'Học sắp xếp rất dễ hiểu nhờ mô phỏng từng bước.' },
  ],
  author: {
    name: 'Giảng viên DSA',
    academicDegree: 'ThS. Khoa học Máy tính',
    bio: 'Giảng viên Cấu trúc Dữ liệu & Giải thuật.',
    profileLink: null,
    avatarUrl: null,
  },
  lessons: [
    {
      id: '1',
      title: 'Sắp xếp nổi bọt (Bubble Sort)',
      moduleTitle: 'Chặng 1 — Sắp xếp',
      contentMd: '## Bubble Sort\nSo sánh các cặp liền kề và hoán đổi khi cần.',
      sandboxType: 'dsa',
      sandboxConfig: '',
      quizId: null,
      xpReward: 40,
      orderIndex: 1,
      status: 'Active',
    },
    {
      id: '2',
      title: 'Tìm kiếm nhị phân',
      moduleTitle: 'Chặng 2 — Tìm kiếm',
      contentMd: '## Binary Search\nChia đôi không gian tìm kiếm mỗi bước.',
      sandboxType: 'quiz',
      sandboxConfig: '',
      quizId: 'quiz-binary-1',
      xpReward: 40,
      orderIndex: 2,
      status: 'Active',
    },
  ],
};
export interface MockApiOptions {
  /** Số tim ban đầu (GET /me/hearts) — mỗi POST enter trừ 1 (FR-10.1) */
  initialHearts?: number;
  heartsMax?: number;
  /** Vai trò user trong mock (mặc định STUDENT) — spec teacher/admin dùng TEACHER/ADMIN */
  role?: typeof MOCK_USER.role;
}

function json(route: Route, status: number, body: unknown): Promise<void> {
  return route.fulfill({
    status,
    contentType: 'application/json',
    body: JSON.stringify(body),
  });
}

function empty(route: Route, status: number): Promise<void> {
  return route.fulfill({ status });
}

function apiError(route: Route, status: number, code: string, message: string): Promise<void> {
  return json(route, status, { error: { code, message, field: null, details: [] } });
}

/**
 * Đăng ký mock cho toàn bộ request `/api/v1/**` trên trang.
 * Gọi MỘT lần trước page.goto() ở đầu mỗi test.
 */
export async function mockApi(page: Page, options: MockApiOptions = {}): Promise<void> {
  let hearts = options.initialHearts ?? 10;
  const heartsMax = options.heartsMax ?? 10;
  // User theo vai trò test (mặc định STUDENT — MOCK_USER).
  const mockUser: UserSummary = options.role ? { ...MOCK_USER, role: options.role } : MOCK_USER;
  // Trạng thái phiên auth trong mock: MỞ ĐẦU chưa đăng nhập (mỗi test tạo page mới).
  // Boot app gọi POST /auth/refresh TRƯỚC router guard (G-BF2, src/main.ts) — nếu mock
  // luôn trả 200 thì app boot coi như đã authenticated → guard guestOnly/requiresAuth
  // không redirect /login → spec auth/ladder/code-runner fail. Refresh phải trả 401 khi
  // chưa login (giống backend không có refresh-token cookie), 200 chỉ sau khi đã login.
  let isAuthenticated = false;

  await page.route('**/api/v1/**', async (route) => {
    const request = route.request();
    const method = request.method();
    const url = new URL(request.url());
    const path = url.pathname.replace(/^\/api\/v1/, '');

    // ── Auth (API_REFERENCE §4.1) ──
    if (method === 'POST' && path === '/auth/login') {
      // LoginResponse: { accessToken, expiresIn, user }
      isAuthenticated = true;
      await json(route, 200, {
        accessToken: MOCK_ACCESS_TOKEN,
        expiresIn: 3600,
        user: mockUser,
      });
      return;
    }
    if (method === 'POST' && path === '/auth/register') {
      // RegisterResponse = LoginResponse (student — RegisterView redirect /path)
      isAuthenticated = true;
      await json(route, 201, {
        accessToken: MOCK_ACCESS_TOKEN,
        expiresIn: 3600,
        user: mockUser,
      });
      return;
    }
    if (method === 'POST' && path === '/auth/refresh') {
      // Boot chưa login → 401 (AuthStore.refresh bắt lỗi → status='error', guard lo redirect);
      // đã login → 200 kèm token mới (RefreshResponse — AuthStore chỉ đọc response.accessToken).
      if (!isAuthenticated) {
        await apiError(route, 401, 'UNAUTHORIZED', 'Refresh token không hợp lệ hoặc đã hết hạn');
        return;
      }
      await json(route, 200, {
        accessToken: MOCK_ACCESS_TOKEN,
        refreshToken: 'e2e-refresh-token',
        expiresIn: 3600,
        user: mockUser,
      });
      return;
    }
    if (method === 'POST' && path === '/auth/logout') {
      isAuthenticated = false;
      await empty(route, 204);
      return;
    }
    if (method === 'GET' && path === '/auth/me') {
      await json(route, 200, mockUser);
      return;
    }

    // ── Lessons / Topics (API_REFERENCE §4.3-4.4) ──
    if (method === 'GET' && path === '/topics') {
      await json(route, 200, MOCK_TOPICS);
      return;
    }

    // ── Classes (API_REFERENCE §4.11) — G-F3E-NEW-2: Leaderboard tab Lớp lấy classId ──
    if (method === 'GET' && path === '/classes') {
      await json(route, 200, MOCK_CLASSES);
      return;
    }

    if (method === 'GET' && path === '/lessons') {
      // PagedResponse<LessonSummary> — rỗng, an toàn cho view dùng fallback
      await json(route, 200, { items: [], page: 1, pageSize: 20, total: 0, totalPages: 0 });
      return;
    }
    if (method === 'GET' && /^\/lessons\/\d+$/.test(path)) {
      await json(route, 200, MOCK_LESSON);
      return;
    }

    // ── Exercises (API_REFERENCE §4.6) — G-F2b: GET /exercises/{id} để Màn 06 render quiz thật
    if (method === 'GET' && /^\/exercises\/\d+$/.test(path)) {
      await json(route, 200, MOCK_EXERCISE);
      return;
    }
    // G-F2c: GET /exercises?nodeId&stage — LadderView (Bậc 1 quiz + Bậc 3 code) → trả summary
    if (method === 'GET' && path === '/exercises') {
      const nodeIdParam = url.searchParams.get('nodeId');
      const stageParam = url.searchParams.get('stage');
      const items = [
        { id: 1, title: 'Trắc nghiệm Bubble Sort', type: 'MCQ', lessonId: 1, nodeId: 1, stage: 1, durationMinutes: 10, maxScore: 4, status: 'active' },
        { id: 2, title: 'Code Bubble Sort', type: 'CODE', lessonId: 1, nodeId: 1, stage: 3, durationMinutes: 15, maxScore: 10, status: 'active' },
      ].filter((item) => (!nodeIdParam || String(item.nodeId) === nodeIdParam) && (!stageParam || String(item.stage) === stageParam));
      await json(route, 200, { items, page: 1, pageSize: 20, total: items.length, totalPages: 1 });
      return;
    }

    // ── Progress (API_REFERENCE §4.7) ──
    if (method === 'GET' && path === '/progress/me') {
      await json(route, 200, MOCK_PROGRESS_OVERVIEW);
      return;
    }

    // ── Gamification (API_REFERENCE §4.14) ──
    if (method === 'GET' && path === '/me/inventory') {
      // AppHeader (global) fetch inventory trên mọi trang — mock rỗng (D4: tránh 404 console).
      await json(route, 200, []);
      return;
    }
    if (method === 'GET' && path === '/me/hearts') {
      const body: HeartsStatusDto = { hearts, heartsMax, lastHeartAt: null, nextHeartAt: null };
      await json(route, 200, body);
      return;
    }
    if (method === 'GET' && path === '/me/streak') {
      await json(route, 200, { streakDays: 0, freezeAvailable: 0 });
      return;
    }
    if (method === 'GET' && path === '/premium/status') {
      await json(route, 200, { isPremium: false, plan: null, expiresAt: null });
      return;
    }
    if (method === 'GET' && /^\/learning-path\/\d+$/.test(path)) {
      await json(route, 200, MOCK_LEARNING_PATH);
      return;
    }

    // ── Leaderboard (API_REFERENCE §4.14 — G-F2d: PagedResponse, thứ tự theo tab) ──
    if (method === 'GET' && path === '/leaderboard') {
      const tab = url.searchParams.get('tab') ?? 'week';
      const page = Math.max(1, Number(url.searchParams.get('page')) || 1);
      await json(route, 200, {
        items: mockLeaderboardPage(tab, page).items,
        page,
        pageSize: 10,
        total: mockLeaderboardPage(tab, page).total,
        totalPages: mockLeaderboardPage(tab, page).totalPages,
      });
      return;
    }
    if (method === 'POST' && /^\/learning-path\/\d+\/nodes\/\d+\/enter$/.test(path)) {
      // FR-10.1: trừ 1 tim atomic — heartsLeft giảm mỗi lần gọi (mock stateful)
      hearts = Math.max(0, hearts - 1);
      await json(route, 200, { session: { id: 1, nodeId: 1 }, heartsLeft: hearts });
      return;
    }

    // ── Favorites (API_REFERENCE §4.9 — SimulatorView checkFavorite khi đã đăng nhập) ──
    if (method === 'GET' && path === '/favorites') {
      await json(route, 200, []);
      return;
    }
    if (method === 'POST' && path === '/favorites') {
      await json(route, 201, { id: 1, simKey: 'sort.bubble', title: 'Bubble Sort', input: null, createdAt: '2026-08-12T00:00:00.000Z' });
      return;
    }
    if (method === 'DELETE' && /^\/favorites\/\d+$/.test(path)) {
      await empty(route, 204);
      return;
    }

    // ── Code Runner (API_REFERENCE §4.13 — ADR-012 sandbox client; chỉ lưu vết lên server) ──
    if (method === 'POST' && path === '/code-runs') {
      await json(route, 201, {
        id: 1,
        exerciseId: null,
        status: 'passed',
        passed: 1,
        total: 1,
        createdAt: '2026-08-12T00:00:00.000Z',
      });
      return;
    }
    if (method === 'GET' && /^\/exercises\/\d+\/code-submissions\/me$/.test(path)) {
      await json(route, 200, []);
      return;
    }

    // ── Benchmark (API_REFERENCE §4.14 — G-F2c: POST /benchmarks/run lưu kết quả đo client) ──
    if (method === 'POST' && path === '/benchmarks/run') {
      const body = request.postDataJSON() as { keys?: string[]; sizes?: number[] } | null;
      await json(route, 201, {
        keys: body?.keys ?? [],
        sizes: body?.sizes ?? [],
        rows: [],
        conclusion: null,
        measuredAt: '2026-08-12T00:00:00.000Z',
      });
      return;
    }

    // ── Courses/Lộ trình (courseApi — /concepts/courses*) — B4 ──
    if (method === 'GET' && path === '/concepts/courses') {
      await json(route, 200, MOCK_COURSE_LIST);
      return;
    }
    if (method === 'GET' && /^\/concepts\/courses\/[^/]+$/.test(path)) {
      await json(route, 200, MOCK_COURSE_DETAIL);
      return;
    }
    if (method === 'GET' && path === '/courses/feedback/mine') {
      await json(route, 200, []);
      return;
    }
    if (method === 'POST' && path === '/courses/feedback') {
      await json(route, 201, {
        id: 1,
        courseId: 1,
        courseTitle: 'Lộ trình nền tảng DSA',
        userId: 1,
        userName: 'E2E Student',
        type: 'Suggestion',
        content: 'Góp ý E2E',
        status: 'New',
        replyText: null,
        repliedByName: null,
        repliedAt: null,
        createdAt: '2026-08-12T00:00:00.000Z',
      });
      return;
    }

    // Fallback an toàn: endpoint chưa mock → 404 chuẩn ApiErrorBody
    await apiError(route, 404, 'NOT_FOUND', `E2E mock: endpoint ${method} ${path} chưa được mock`);
  });
}
