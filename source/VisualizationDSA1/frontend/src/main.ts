import { createApp, nextTick } from 'vue'
import { createPinia } from 'pinia'
import { MotionPlugin } from '@vueuse/motion'
import router from './router'
import './style.css'
import App from './App.vue'
import AOS from 'aos'
import 'aos/dist/aos.css'
import Particles from '@tsparticles/vue3'
import { loadSlim } from '@tsparticles/slim'
import { useAuthStore } from './features/auth/store/useAuthStore'
import { useUserProgressStore } from './features/user-progress/store/useUserProgressStore'

import BaseIcon from './shared/components/BaseIcon.vue'

const app  = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(MotionPlugin)
app.use(Particles, {
  init: async engine => {
    await loadSlim(engine)
  },
})
app.component('BaseIcon', BaseIcon)


const originalFetch = window.fetch;
window.fetch = async function (input: RequestInfo | URL, init?: RequestInit): Promise<Response> {
  const authStore = useAuthStore();
  const url = typeof input === 'string' ? input : (input instanceof URL ? input.toString() : input.url);

  
  const isApiRequest = url.includes('/api/v1/') || url.includes('/api/v1/concepts/');
  const isRefreshRequest = url.includes('/auth/refresh') || url.includes('/concepts/auth/refresh');

  let headers = new Headers(init?.headers);

  
  if (isApiRequest && !isRefreshRequest) {
    const token = authStore.getAccessToken();
    if (token) {
      headers.set('Authorization', `Bearer ${token}`);
    }
  }

  
  const newInit = {
    ...init,
    headers,
  };

  let response = await originalFetch(input, newInit);

  
  
  
  if (response.status === 401 && isApiRequest && !isRefreshRequest) {
    console.warn(`[Fetch Interceptor] 401 Unauthorized detected for ${url}. Attempting token refresh...`);
    try {
      const newToken = await authStore.refreshAccessToken();
      if (newToken) {
        headers.set('Authorization', `Bearer ${newToken}`);
        console.log(`[Fetch Interceptor] Token refreshed successfully. Retrying ${url}...`);
        response = await originalFetch(input, {
          ...init,
          headers,
        });
      }
    } catch (refreshErr) {
      console.error('[Fetch Interceptor] Token refresh failed:', refreshErr);
    }
  } else if (response.status === 403 && isApiRequest) {
    console.warn(`[Fetch Interceptor] 403 Forbidden for ${url} — permission denied, NOT clearing auth state.`);
  }

  return response;
};



const authStore     = useAuthStore()
const progressStore = useUserProgressStore()

// HOTFIX-4 — mount NGAY, init stores song song: không bao giờ để trang trắng
// dù backend chậm/offline (authStore.init trước đây chặn app.mount vĩnh viễn).
app.use(router)
router.isReady().then(() => {
  app.mount('#app')
  AOS.init({
    duration: 700,
    easing: 'ease-out-cubic',
    once: true,
    offset: 80,
  })

  // ── HOTFIX: AOS 2.3.4 detect scroll bằng `window.pageYOffset`, nhưng app cuộn
  // nội dung trong scroll container lồng (.app-view/.app-main) → window.pageYOffset
  // luôn = 0 → phần tử dưới fold không bao giờ nhận `aos-animate` (trang nhìn "trống").
  // Giải pháp: tự reveal bằng IntersectionObserver (hoạt động đúng với mọi scroll
  // container), quét lại khi DOM thay đổi (SPA chuyển view).
  let aosIo: IntersectionObserver | null = null;
  let aosMutObserver: MutationObserver | null = null;
  let aosScanScheduled = false;

  const aosReveal = (entries: IntersectionObserverEntry[]): void => {
    for (const entry of entries) {
      if (entry.isIntersecting) {
        entry.target.classList.add('aos-animate');
        aosIo?.unobserve(entry.target);
      }
    }
  };

  const aosScan = (): void => {
    aosScanScheduled = false;
    if (!aosIo) return;
    document.querySelectorAll('[data-aos]:not(.aos-animate)').forEach(el => aosIo?.observe(el));
  };

  const aosScheduleScan = (): void => {
    if (aosScanScheduled) return;
    aosScanScheduled = true;
    requestAnimationFrame(aosScan);
  };

  aosIo = new IntersectionObserver(aosReveal, { root: null, threshold: 0, rootMargin: '0px 0px -40px 0px' });
  aosMutObserver = new MutationObserver(() => aosScheduleScan());
  aosMutObserver.observe(document.documentElement, { childList: true, subtree: true });
  aosScan();

  // SPA: view mới mount sau afterEach → quét lại sau nextTick
  router.afterEach(() => {
    nextTick(() => aosScheduleScan());
  });
})

// Init song song — UI tự cập nhật khi store load xong, không chặn render
void authStore.init().then(() => progressStore.initFromServer())
