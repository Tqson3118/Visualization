import { createPinia } from 'pinia';
import { createApp } from 'vue';

import App from './App.vue';
import router from './router';
import { useAuthStore } from '@/stores/auth';
import './styles/tokens.css';
import './styles/global.css';

/**
 * Boot app (ADR-004, bug P1 #1 — SETUP_TODO §6.1):
 * Token chỉ nằm trong memory Pinia nên F5/reload mất phiên. Gọi auth.refresh()
 * (cookie HttpOnly) TRƯỚC khi router guard chạy để khôi phục session.
 * Nếu không có refresh token hợp lệ → status 'error', không chặn user tới trang công khai.
 */
async function bootstrap(): Promise<void> {
  const app = createApp(App);
  const pinia = createPinia();
  app.use(pinia);

  const auth = useAuthStore(pinia);
  try {
    const token = await auth.refresh();
    if (token) {
      // Refresh thành công → lấy user để role guard (admin/**) và header hiển thị đúng
      await auth.fetchMe();
    }
  } catch {
    // refresh/fetchMe lỗi (mạng, cookie hết hạn) → giữ nguyên trạng thái 'error'; guard lo phần còn lại
  }

  app.use(router);
  app.mount('#app');
}

void bootstrap();
