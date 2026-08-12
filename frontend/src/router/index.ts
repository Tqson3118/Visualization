import { createRouter, createWebHistory } from 'vue-router';

import HomeView from '@/views/HomeView.vue';
import LoginView from '@/views/LoginView.vue';
import { useAuthStore } from '@/stores/auth';

// Lazy-load các trang lớn (SDD §3.9): simulator, exercise, admin/*, code, benchmark, path
const RegisterView = () => import('@/views/RegisterView.vue');
const ForgotPasswordView = () => import('@/views/ForgotPasswordView.vue');
const ResetPasswordView = () => import('@/views/ResetPasswordView.vue');

const PathRedirectView = () => import('@/views/PathRedirectView.vue');
const PathView = () => import('@/views/PathView.vue');
const NodeHubView = () => import('@/views/NodeHubView.vue');
const FinalTestView = () => import('@/views/FinalTestView.vue');

const SimulationsView = () => import('@/views/SimulationsView.vue');
const SimulatorView = () => import('@/views/SimulatorView.vue');
const LessonView = () => import('@/views/LessonView.vue');
const ExerciseView = () => import('@/views/ExerciseView.vue');
const LadderView = () => import('@/views/LadderView.vue');
const LabView = () => import('@/views/LabView.vue');
const CodeRunnerView = () => import('@/views/CodeRunnerView.vue');
const BenchmarkView = () => import('@/views/BenchmarkView.vue');
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
const AdminLadderView = () => import('@/views/AdminLadderView.vue');

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
    // Màn 03 — /learn (redirect /path — 20.5.6)
    {
      path: '/learn',
      redirect: '/path',
    },
    // Màn 04 — Chi tiết bài học
    {
      path: '/learn/:lessonId',
      name: 'lesson',
      component: LessonView,
      meta: { requiresAuth: true },
    },
    // Màn 13 — Learning Path
    {
      path: '/path',
      name: 'path',
      component: PathRedirectView,
      meta: { requiresAuth: true },
    },
    {
      path: '/path/:topicId',
      name: 'path-topic',
      component: PathView,
      meta: { requiresAuth: true },
    },
    // Màn 31 — Node Hub
    {
      path: '/path/:topicId/node/:nodeId',
      name: 'node-hub',
      component: NodeHubView,
      meta: { requiresAuth: true },
    },
    // Màn 30 — Kiểm tra cuối lộ trình
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
      meta: { requiresAuth: true },
    },
    // Màn 05 — Mô phỏng (đã đăng nhập hoặc key demo công khai — guard trong view)
    {
      path: '/simulator/:key',
      name: 'simulator',
      component: SimulatorView,
    },
    // Màn 14 — Practice Ladder
    {
      path: '/ladder/:nodeId',
      name: 'ladder',
      component: LadderView,
      meta: { requiresAuth: true },
    },
    // Màn 15 — Interactive Lab (Bậc 2)
    {
      path: '/ladder/:nodeId/lab',
      name: 'lab',
      component: LabView,
      meta: { requiresAuth: true },
    },
    // Màn 16 — Code Runner
    {
      path: '/code/:key',
      name: 'code',
      component: CodeRunnerView,
      meta: { requiresAuth: true },
    },
    // Màn 17 — Benchmark Lab (MIỄN PHÍ tim — 20.4)
    {
      path: '/benchmark/:k1/:k2',
      name: 'benchmark',
      component: BenchmarkView,
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
    // Màn 09/10/11 + N-5/N-6 — Admin
    {
      path: '/admin',
      redirect: '/admin/users',
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
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/admin/settings',
      name: 'admin-settings',
      component: AdminSettingsView,
      meta: { requiresAuth: true, roles: ['ADMIN'] },
    },
    {
      path: '/admin/content',
      name: 'admin-content',
      component: AdminContentView,
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/admin/ladder',
      name: 'admin-ladder',
      component: AdminLadderView,
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
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
