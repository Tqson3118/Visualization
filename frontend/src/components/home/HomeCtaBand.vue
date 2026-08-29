<script setup lang="ts">
import { getActivePinia } from 'pinia';
import { RouterLink } from 'vue-router';
import { AlertCircle, ArrowRight } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth';
import { messages } from '@/i18n/vi';

const auth = getActivePinia()
  ? useAuthStore()
  : ({ role: null, isAuthenticated: false } as unknown as ReturnType<typeof useAuthStore>);
</script>

<template>
  <section class="home__cta-band">
    <div class="home__cta-band-glow" aria-hidden="true" />
    <div class="home__cta-band-inner">
      <h2 class="home__cta-band-title">{{ messages.home.ctaBandTitle }}</h2>
      <p class="home__cta-band-desc">{{ messages.home.ctaBandDesc }}</p>

      <div v-if="auth.role === 'TEACHER_PENDING'" class="home__pending-banner" role="status">
        <AlertCircle class="home__pending-icon" :size="18" aria-hidden="true" />
        <span>Tài khoản chờ Admin phê duyệt</span>
      </div>
      <div v-else-if="auth.role === 'TEACHER'" class="home__cta">
        <RouterLink :to="{ path: '/studio' }" class="home__cta-primary">
          Vào Studio Soạn bài
          <ArrowRight class="home__cta-arrow" aria-hidden="true" />
        </RouterLink>
        <RouterLink :to="{ name: 'classes' }" class="home__cta-ghost">
          Quản lý lớp học
        </RouterLink>
      </div>
      <div v-else-if="auth.role === 'ADMIN'" class="home__cta">
        <RouterLink :to="{ name: 'admin-users' }" class="home__cta-primary">
          Admin Console
          <ArrowRight class="home__cta-arrow" aria-hidden="true" />
        </RouterLink>
        <RouterLink :to="{ path: '/studio' }" class="home__cta-ghost">
          Studio Lộ trình
        </RouterLink>
      </div>
      <div v-else-if="auth.isAuthenticated" class="home__cta">
        <RouterLink :to="{ name: 'path-list' }" class="home__cta-primary">
          {{ messages.home.ctaGoCourses }}
          <ArrowRight class="home__cta-arrow" aria-hidden="true" />
        </RouterLink>
        <RouterLink :to="{ name: 'simulations' }" class="home__cta-ghost">
          {{ messages.home.ctaGoSims }}
        </RouterLink>
      </div>
      <div v-else class="home__cta">
        <RouterLink :to="{ name: 'path-list' }" class="home__cta-primary">
          {{ messages.home.ctaGoCourses }}
          <ArrowRight class="home__cta-arrow" aria-hidden="true" />
        </RouterLink>
        <RouterLink :to="{ name: 'register' }" class="home__cta-ghost">
          Đăng ký học ngay
        </RouterLink>
      </div>
    </div>
  </section>
</template>

<style scoped>
.home__cta-band {
  position: relative;
  z-index: 1;
  margin: var(--space-sm, 8px) 0 0;
  padding: var(--space-2xl, 32px) var(--space-lg, 24px);
  text-align: center;
  overflow: hidden;
  border-block: 1px solid rgba(255, 255, 255, 0.07);
  background: rgba(168, 85, 247, 0.04);
}

.home__cta-band-glow {
  position: absolute;
  top: -60%;
  left: 50%;
  width: 700px;
  height: 360px;
  transform: translateX(-50%);
  background: radial-gradient(ellipse, rgba(168, 85, 247, 0.14) 0%, transparent 70%);
  pointer-events: none;
  animation: cos-nebula 10s ease-in-out infinite alternate;
}

@keyframes cos-nebula {
  from { opacity: 0.7; transform: translate(-50%, 0) scale(1); }
  to { opacity: 1; transform: translate(-50%, 12px) scale(1.06); }
}

.home__cta-band-inner {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md, 16px);
}

.home__cta-band-title {
  margin: 0;
  font-size: var(--text-3xl);
  font-weight: 700;
  letter-spacing: -0.025em;
  color: #fff;
}

.home__cta-band-desc {
  margin: 0;
  max-width: 60ch;
  color: rgba(255, 255, 255, 0.62);
  font-size: var(--text-sm);
}

.home__cta {
  display: flex;
  gap: var(--space-md, 16px);
  flex-wrap: wrap;
  justify-content: center;
}

.home__cta-primary,
.home__cta-ghost {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 0.7rem 1.5rem;
  border-radius: var(--radius-lg);
  font-size: var(--text-sm);
  font-weight: 700;
  text-decoration: none;
  transition: transform 200ms cubic-bezier(0.34, 1.56, 0.64, 1), box-shadow 200ms ease, border-color 200ms ease;
}

.home__cta-primary {
  background: linear-gradient(135deg, #a855f7, #7c3aed);
  color: #fff;
  box-shadow: 0 0 22px rgba(168, 85, 247, 0.35);
}

.home__cta-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 0 34px rgba(168, 85, 247, 0.5);
}

.home__cta-arrow { width: 15px; height: 15px; }

.home__cta-ghost {
  background: rgba(13, 12, 20, 0.72);
  -webkit-backdrop-filter: blur(12px) saturate(1.3);
  backdrop-filter: blur(12px) saturate(1.3);
  color: rgba(255, 255, 255, 0.9);
  border: 1px solid rgba(255, 255, 255, 0.14);
}

.home__cta-ghost:hover {
  border-color: #a855f7;
  color: #c084fc;
  transform: translateY(-2px);
}

.home__pending-banner {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm, 8px);
  padding: 0.75rem 1.25rem;
  border-radius: var(--radius-lg);
  background: rgba(245, 158, 11, 0.12);
  border: 1px solid rgba(245, 158, 11, 0.35);
  color: #fbbf24;
  font-size: var(--text-sm);
  font-weight: 600;
}

.home__pending-icon {
  color: #f59e0b;
  flex-shrink: 0;
}

@media (max-width: 640px) {
  .home__cta-band-title { font-size: var(--text-2xl); }
}

@media (prefers-reduced-motion: reduce) {
  .home__cta-band-glow {
    animation: none !important;
  }
}
</style>
