import { createRouter, createWebHistory } from 'vue-router';

import HomeView from '@/views/HomeView.vue';
import LoginView from '@/views/LoginView.vue';
import { useAuthStore } from '@/stores/auth';

// Lazy-load các trang lớn (SDD §3.9): simulator, exercise, admin/*
// Các view chưa triển khai dùng PlaceholderView — sẽ thay bằng view thật ở task sau.
const PlaceholderView = () => import('@/views/PlaceholderView.vue');

declare module 'vue-router' {
  interface RouteMeta {
    requiresAuth?: boolean;
    /** Trang công khai chỉ cho khách (login/register — SDD §3.3) */
    guestOnly?: boolean;
    /** Vai trò được phép vào route — SDD §3.3 (VD: admin/**) */
    roles?: Array<'STUDENT' | 'TEACHER' | 'TEACHER_PENDING' | 'ADMIN'>;
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { guestOnly: true },
    },
    {
      path: '/register',
      name: 'register',
      component: PlaceholderView,
      meta: { guestOnly: true },
    },
    {
      path: '/path',
      name: 'path',
      component: PlaceholderView,
      meta: { requiresAuth: true },
    },
    {
      path: '/path/:topicId',
      name: 'path-topic',
      component: PlaceholderView,
      meta: { requiresAuth: true },
    },
    {
      path: '/simulations',
      name: 'simulations',
      component: PlaceholderView,
      meta: { requiresAuth: true },
    },
    {
      path: '/simulator/:key',
      name: 'simulator',
      component: () => import('@/views/SimulatorView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/ladder/:nodeId',
      name: 'ladder',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/code/:key',
      name: 'code',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/exercise/:id',
      name: 'exercise',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/profile',
      name: 'profile',
      component: PlaceholderView,
      meta: { requiresAuth: true },
    },
    {
      path: '/admin',
      redirect: '/admin/users',
    },
    {
      path: '/admin/users',
      name: 'admin-users',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/admin/stats',
      name: 'admin-stats',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/admin/settings',
      name: 'admin-settings',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/admin/content',
      name: 'admin-content',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
    {
      path: '/admin/ladder',
      name: 'admin-ladder',
      component: () => import('@/views/PlaceholderView.vue'),
      meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
    },
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

  // Route giới hạn vai trò (SDD §3.3 — admin/**)
  const requiredRoles = to.matched.flatMap((record) => record.meta.roles ?? []);
  if (requiredRoles.length > 0 && (auth.role === null || !requiredRoles.includes(auth.role))) {
    return { name: 'home' };
  }

  // Đã đăng nhập → rời khỏi login/register (SDD §3.3)
  if (to.meta.guestOnly && auth.isAuthenticated) {
    return { name: 'home' };
  }

  return true;
});

export default router;
