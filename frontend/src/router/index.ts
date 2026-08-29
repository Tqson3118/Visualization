import { createRouter, createWebHistory } from 'vue-router';

import HomeView from '@/views/HomeView.vue';
import LoginView from '@/views/LoginView.vue';
import { useAuthStore } from '@/stores/auth';

// Lazy-load các trang lớn (SDD §3.9): simulator, exercise, admin/*, code, benchmark, path
const RegisterView = () => import('@/views/RegisterView.vue');
const ForgotPasswordView = () => import('@/views/ForgotPasswordView.vue');
const ResetPasswordView = () => import('@/views/ResetPasswordView.vue');

// PathRedirectView/PathView/NodeHubView — legacy flow không còn route dùng (D7: giữ file, bỏ import).
const FinalTestView = () => import('@/views/FinalTestView.vue');

const SimulationsView = () => import('@/views/SimulationsView.vue');
const SimulatorView = () => import('@/views/SimulatorView.vue');
const ExerciseView = () => import('@/views/ExerciseView.vue');
const CodeRunnerView = () => import('@/views/CodeRunnerView.vue');
const CheatSheetView = () => import('@/views/CheatSheetView.vue');
const LeaderboardView = () => import('@/views/LeaderboardView.vue');
const ProfileView = () => import('@/views/ProfileView.vue');
const ClassesView = () => import('@/views/ClassesView.vue');
const ClassDetailView = () => import('@/views/ClassDetailView.vue');
const ClassReportView = () => import('@/views/ClassReportView.vue');
const ShopView = () => import('@/views/ShopView.vue');
const QuestsView = () => import('@/views/QuestsView.vue');
const PremiumView = () => import('@/views/PremiumView.vue');
const SubscriptionView = () => import('@/views/SubscriptionView.vue');
const HelpView = () => import('@/views/HelpView.vue');
const PrivacyView = () => import('@/views/PrivacyView.vue');

const AdminUsersView = () => import('@/views/AdminUsersView.vue');
const AdminStatsView = () => import('@/views/AdminStatsView.vue');
const AdminSettingsView = () => import('@/views/AdminSettingsView.vue');
const AdminContentView = () => import('@/views/AdminContentView.vue');

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean;
    /** Trang công khai chỉ cho khách (login/register — SDD §3.3) */
    guestOnly?: boolean;
    /** Vai trò được phép vào route — SDD §3.3 (VD: admin/**) */
    roles?: Array<'STUDENT' | 'TEACHER' | 'TEACHER_PENDING' | 'ADMIN'>;
  }
}

/**
 * Route theo SCREEN_MAP §10A (32 màn + N-1..N-16 đã gộp) — 0 route trỏ PlaceholderView.
 * Redirect bắt buộc (20.5.6): /learn → /path; /dashboard → /profile.
 */
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  // E1: restore vị trí khi back/forward (savedPosition), chuyển trang mới scroll lên đầu.
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) return savedPosition;
    return { top: 0 };
  },
  routes: [
    // Màn 01 — Trang chủ công khai
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    // Màn 02 — Auth
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: RegisterView,
      meta: { guestOnly: true },
    },
    // Màn N-2 — Khôi phục mật khẩu (công khai)
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: ForgotPasswordView,
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: ResetPasswordView,
    },
    // Màn 03 — /learn (alias → /path — D2)
    {
      path: '/learn',
      redirect: '/path',
    },
    // VDSA — Lộ trình (canonical: /path).
    {
      path: '/path',
      name: 'path-list',
      alias: '/courses',
      component: () => import('@/views/courses/CoursesListView.vue'),
      meta: { public: true },
    },
    {
      path: '/path/:id',
      name: 'path-detail',
      component: () => import('@/views/courses/CourseDetailView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/lessons/:id',
      name: 'lesson-study',
      component: () => import('@/views/lesson/LessonStudyView.vue'),
      meta: { requiresAuth: true },
    },
    // Alias cũ (D2 — không mất link cũ): /courses* → /path*
    {
      path: '/courses',
      redirect: '/path',
    },
    {
      path: '/courses/:id',
      name: 'course-detail',
      redirect: (to) => ({ name: 'path-detail', params: { id: to.params.id } }),
    },
    // Alias cũ: /learn/:lessonId & /courses/:courseId/lessons/:lessonId → /lessons/:id
    {
      path: '/learn/:lessonId',
      redirect: (to) => ({ name: 'lesson-study', params: { id: to.params.lessonId } }),
    },
    {
      path: '/courses/:courseId/lessons/:lessonId',
      redirect: (to) => ({ name: 'lesson-study', params: { id: to.params.lessonId }, query: { courseId: to.params.courseId } }),
    },
    // Sandbox legacy redirects → /simulations
    {
      path: '/sorting-sandbox',
      redirect: '/simulations',
    },
    {
      path: '/searching-sandbox',
      redirect: '/simulations',
    },
    {
      path: '/stack-queue-sandbox',
      redirect: '/simulations',
    },
    // Legacy deep link roadmap — topicId cũ ≠ pathId mới: không mapping an toàn → /path (D2)
    {
      path: '/path/:pathId/node/:nodeId',
      name: 'node-hub',
      redirect: '/path',
    },
    // Alias tên cũ 'path-topic' (LadderView/FinalTestView/NodeHubView vẫn push the name) →
    // path-detail (D2). Khai báo SAU path-detail để URL /path/:id match component trực tiếp.
    {
      path: '/path/:topicId',
      name: 'path-topic',
      redirect: (to) => ({ name: 'path-detail', params: { id: to.params.topicId } }),
    },
    // Màn 30 — Kiểm tra cuối lộ trình (legacy compat — giữ live, D2)
    {
      path: '/path/:topicId/final-test',
      name: 'final-test',
      component: FinalTestView,
      meta: { requiresAuth: true },
    },
    // Màn 33 — Khám phá (danh mục + Benchmark + CheatSheet)
    {
      path: '/simulations',
      name: 'simulations',
      component: SimulationsView,
      meta: { public: true },
    },
    // Màn 05 — Mô phỏng (đã đăng nhập hoặc key demo công khai — guard trong view)
    {
      path: '/simulator/:key',
      name: 'simulator',
      component: SimulatorView,
    },
    // Alias cũ: /ladder/:nodeId & /ladder/:nodeId/lab → /path (D2)
    {
      path: '/ladder/:nodeId',
      redirect: '/path',
    },
    {
      path: '/ladder/:nodeId/lab',
      redirect: '/path',
    },
    // Màn 16 — Code Runner
    {
      path: '/code/:key',
      name: 'code',
      component: CodeRunnerView,
      meta: { requiresAuth: true },
    },
    // Màn 18 — CheatSheet
    {
      path: '/cheatsheet',
      name: 'cheatsheet',
      component: CheatSheetView,
      meta: { requiresAuth: true },
    },
    // Màn 06 — Bài tập trắc nghiệm (Bậc 1)
    {
      path: '/exercise/:id',
      name: 'exercise',
      component: ExerciseView,
      meta: { requiresAuth: true },
    },
    // Màn 08 — /dashboard (redirect /profile — 20.5.6)
    {
      path: '/dashboard',
      redirect: '/profile',
    },
    // Màn 32 — Hồ sơ
    {
      path: '/profile',
      name: 'profile',
      component: ProfileView,
      meta: { requiresAuth: true },
    },
    // Màn 24 — Bảng xếp hạng
    {
      path: '/leaderboard',
      name: 'leaderboard',
      component: LeaderboardView,
      meta: { requiresAuth: true },
    },
    // Màn 23 — Daily Quest
    {
      path: '/quests',
      name: 'quests',
      component: QuestsView,
      meta: { requiresAuth: true },
    },
    // Màn 22 — Gems Shop
    {
      path: '/shop',
      name: 'shop',
      component: ShopView,
      meta: { requiresAuth: true },
    },
    // Màn 25 — Premium (26 gộp — checkout modal)
    {
      path: '/premium',
      name: 'premium',
      component: PremiumView,
      meta: { requiresAuth: true },
    },
    // Màn 27 — Quản lý gói Premium
    {
      path: '/account/subscription',
      name: 'subscription',
      component: SubscriptionView,
      meta: { requiresAuth: true },
    },
    // Màn 19/20/21 — Lớp học
    {
      path: '/classes',
      name: 'classes',
      component: ClassesView,
      meta: { requiresAuth: true },
    },
    {
      path: '/classes/:id',
      name: 'class-detail',
      component: ClassDetailView,
      meta: { requiresAuth: true },
    },
    {
      path: '/classes/:id/report',
      name: 'class-report',
      component: ClassReportView,
      meta: { requiresAuth: true },
    },
    // Teacher Studio — entry riêng cho giảng viên, Admin vẫn vào Admin Console
    // Studio Lộ trình & Soạn bài — entry thống nhất cho Teacher và Admin
    {
      path: '/studio',
      name: 'curriculum-studio',
      component: AdminContentView,
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/studio/lessons/new',
      name: 'studio-lesson-new',
      component: () => import('@/views/admin/AdminLessonEditorView.vue'),
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/studio/lessons/:id/edit',
      name: 'studio-lesson-edit',
      component: () => import('@/views/admin/AdminLessonEditorView.vue'),
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    // Teacher Studio — chuyển hướng thống nhất về Studio Overview
    {
      path: '/teacher',
      redirect: { path: '/studio', query: { tab: 'overview' } },
    },
    {
      path: '/pending-teacher',
      name: 'pending-teacher',
      component: () => import('@/views/PendingTeacherView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/admin/content',
      redirect: '/studio',
    },
    {
      path: '/admin/content/lessons/new',
      redirect: '/studio/lessons/new',
    },
    {
      path: '/admin/content/lessons/:id/edit',
      redirect: (to) => `/studio/lessons/${to.params.id}/edit`,
    },
    // Màn 09/10/11 + N-5/N-6 — Admin
    {
      path: '/admin',
      redirect: (to) => ({ name: 'admin-users', query: to.query }),
    },
    {
      path: '/admin/users',
      name: 'admin-users',
      component: AdminUsersView,
      meta: { requiresAuth: true, roles: ['ADMIN'] },
    },
    {
      path: '/admin/stats',
      name: 'admin-stats',
      component: AdminStatsView,
      meta: { requiresAuth: true, roles: ['ADMIN'] },
    },
    {
      path: '/admin/settings',
      name: 'admin-settings',
      component: AdminSettingsView,
      meta: { requiresAuth: true, roles: ['ADMIN'] },
    },
    {
      path: '/admin/ladder',
      redirect: { path: '/studio', query: { tab: 'exercises' } },
    },
    {
      path: '/admin/feedback',
      redirect: { path: '/studio', query: { tab: 'feedback' } },
    },
    // Màn 12 — Trang phụ trợ (công khai)
    {
      path: '/help',
      name: 'help',
      component: HelpView,
    },
    {
      path: '/privacy',
      name: 'privacy',
      component: PrivacyView,
    },
    // 404
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue'),
    },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();

  // Route cần đăng nhập → chuyển /login kèm redirect (SDD §3.4)
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } };
  }

  // TEACHER_PENDING → chuyển hướng trang hướng dẫn chờ duyệt nếu cố vào studio/admin
  if (auth.role === ('TEACHER_PENDING' as any) && to.name !== 'pending-teacher' && to.name !== 'login' && to.name !== 'home' && to.name !== 'privacy' && to.name !== 'help') {
    if (to.path.startsWith('/studio') || to.path.startsWith('/teacher') || to.path.startsWith('/admin')) {
      return { name: 'pending-teacher' };
    }
  }

  // Route giới hạn vai trò (SDD §3.3 — admin/**). Backend ADMIN-only cho /admin/users + /admin/settings (SETUP_TODO §8.5)
  // → TEACHER bị chặn FE: chuyển /profile nếu đã đăng nhập, ngược lại /login.
  const requiredRoles = to.matched.flatMap((record) => record.meta.roles ?? []);
  if (requiredRoles.length > 0 && (auth.role === null || !requiredRoles.includes(auth.role))) {
    return auth.isAuthenticated ? { name: 'profile' } : { name: 'login' };
  }

  // Đã đăng nhập → rời khỏi login/register (SDD §3.3)
  if (to.meta.guestOnly && auth.isAuthenticated) {
    return { name: 'home' };
  }

  return true;
});

export default router;