import { createRouter, createWebHashHistory } from 'vue-router';
import { routes } from './routes';
import { useAuthStore } from '../features/auth/store/useAuthStore';









const router = createRouter({
  history: createWebHashHistory(),
  routes,
});








router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();

  
  
  if (from.path.startsWith('/admin') && !to.path.startsWith('/admin') && authStore.isImpersonating) {
    authStore.stopImpersonating();
  }

  // Đã gỡ bỏ logic tự động chuyển hướng về dashboard khi đang ở trang chủ để User có thể xem được Landing Page

  
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next({ name: 'landing' });
  }

  
  if (to.meta.requiresRole) {
    const requiredRole = to.meta.requiresRole as string;
    const userRole = authStore.userRole;

    const hasAccess = userRole === requiredRole;

    if (!hasAccess) {
      return next({ name: 'dashboard' });
    }
  }

  next();
});

export default router;
