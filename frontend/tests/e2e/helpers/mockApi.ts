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
  lessonsViewed: 0,
  lessonsTotal: 0,
  exercisesCompleted: 0,
  exercisesTotal: 0,
  avgScore: null,
  topics: [],
};

/** GET /lessons/{id} — NodeHubView (lessonStore.fetchLesson) */
const MOCK_LESSON: LessonDto = {
  id: 1,
  title: 'Sắp xếp nổi bọt (Bubble Sort)',
  description: 'Thuật toán sắp xếp cơ bản',
  topicId: 1,
  sortOrder: 1,
  status: 'active',
  simulationCount: 1,
  exerciseCount: 0,
  progress: null,
  contentHtml: '<p>Bubble Sort — so sánh và hoán đổi liên tiếp.</p>',
};

export interface MockApiOptions {
  /** Số tim ban đầu (GET /me/hearts) — mỗi POST enter trừ 1 (FR-10.1) */
  initialHearts?: number;
  heartsMax?: number;
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

  await page.route('**/api/v1/**', async (route) => {
    const request = route.request();
    const method = request.method();
    const url = new URL(request.url());
    const path = url.pathname.replace(/^\/api\/v1/, '');

    // ── Auth (API_REFERENCE §4.1) ──
    if (method === 'POST' && path === '/auth/login') {
      // LoginResponse: { accessToken, expiresIn, user }
      await json(route, 200, {
        accessToken: MOCK_ACCESS_TOKEN,
        expiresIn: 3600,
        user: MOCK_USER,
      });
      return;
    }
    if (method === 'POST' && path === '/auth/register') {
      // RegisterResponse = LoginResponse (student — RegisterView redirect /path)
      await json(route, 201, {
        accessToken: MOCK_ACCESS_TOKEN,
        expiresIn: 3600,
        user: MOCK_USER,
      });
      return;
    }
    if (method === 'POST' && path === '/auth/refresh') {
      await json(route, 200, { accessToken: MOCK_ACCESS_TOKEN, expiresIn: 3600 });
      return;
    }
    if (method === 'POST' && path === '/auth/logout') {
      await empty(route, 204);
      return;
    }
    if (method === 'GET' && path === '/auth/me') {
      await json(route, 200, MOCK_USER);
      return;
    }

    // ── Lessons / Topics (API_REFERENCE §4.3-4.4) ──
    if (method === 'GET' && path === '/topics') {
      await json(route, 200, MOCK_TOPICS);
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

    // ── Progress (API_REFERENCE §4.7) ──
    if (method === 'GET' && path === '/progress/me') {
      await json(route, 200, MOCK_PROGRESS_OVERVIEW);
      return;
    }

    // ── Gamification (API_REFERENCE §4.14) ──
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

    // Fallback an toàn: endpoint chưa mock → 404 chuẩn ApiErrorBody
    await apiError(route, 404, 'NOT_FOUND', `E2E mock: endpoint ${method} ${path} chưa được mock`);
  });
}
