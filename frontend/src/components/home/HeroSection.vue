<script setup lang="ts">
import { onMounted, onUnmounted } from 'vue';
import { RouterLink } from 'vue-router';
import { AlertCircle, ArrowRight, Layers, Play, Users } from 'lucide-vue-next';
import { getActivePinia } from 'pinia';
import { useAuthStore } from '@/stores/auth';
import { messages } from '@/i18n/vi';
import LiveDemoBench from './LiveDemoBench.vue';

const auth = getActivePinia()
  ? useAuthStore()
  : ({ role: null, isAuthenticated: false } as unknown as ReturnType<typeof useAuthStore>);

const prefersReducedMotion = (): boolean =>
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function onHeroScroll(): void {
  const hero = document.querySelector<HTMLElement>('.home__hero');
  if (!hero) return;
  const y = window.scrollY;
  const p = Math.min(y / 420, 1);
  const ease = 1 - Math.pow(1 - p, 2);
  hero.style.opacity = String(1 - ease * 0.92);
  const copy = document.querySelector('.home__hero-copy');
  if (copy) copy.setAttribute('style', `transform: translateY(${y * 0.14}px);`);
}

onMounted(() => {
  if (!prefersReducedMotion()) {
    window.addEventListener('scroll', onHeroScroll, { passive: true });
  }
});

onUnmounted(() => {
  window.removeEventListener('scroll', onHeroScroll);
});
</script>

<template>
  <section class="home__hero">
    <div class="home__cosmos" aria-hidden="true">
      <div class="home__stars home__stars--a" />
      <div class="home__stars home__stars--b" />
      <div class="home__grid-plane" />
      <div class="home__nebula" />
      <span class="home__glyph home__glyph--1">O(log&nbsp;n)</span>
      <span class="home__glyph home__glyph--2">Σ</span>
      <span class="home__glyph home__glyph--3">√n</span>
      <span class="home__glyph home__glyph--4">λ</span>
      <span class="home__glyph home__glyph--5">π</span>
      <span class="home__glyph home__glyph--6">{&nbsp;&nbsp;}</span>
      <span class="home__glyph home__glyph--7">→</span>
      <span class="home__glyph home__glyph--8">∞</span>
      <div class="home__orbit">
        <span class="home__orbit-ring" />
        <span class="home__orbit-core" />
        <span class="home__orbit-dot home__orbit-dot--1" />
        <span class="home__orbit-dot home__orbit-dot--2" />
        <span class="home__orbit-dot home__orbit-dot--3" />
      </div>
    </div>

    <div class="container home__hero-grid">
      <div class="home__hero-copy">
        <p class="home__kicker">
          <span class="home__kicker-dot" aria-hidden="true" />
          <span class="font-mono">{{ messages.home.heroKicker }}</span>
        </p>
        <p class="home__badge">
          <span class="home__badge-star" aria-hidden="true">✦</span>
          {{ messages.home.heroBadge }}
        </p>
        <h1 class="home__title">
          {{ messages.home.heroTitle }}
        </h1>
        <p class="home__subtitle">{{ messages.home.heroSubtitle }}</p>

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
            <Users class="home__cta-ghost-icon" :size="15" aria-hidden="true" />
            Quản lý lớp học
          </RouterLink>
        </div>
        <div v-else-if="auth.role === 'ADMIN'" class="home__cta">
          <RouterLink :to="{ name: 'admin-users' }" class="home__cta-primary">
            Admin Console
            <ArrowRight class="home__cta-arrow" aria-hidden="true" />
          </RouterLink>
          <RouterLink :to="{ path: '/studio' }" class="home__cta-ghost">
            <Layers class="home__cta-ghost-icon" :size="15" aria-hidden="true" />
            Studio Lộ trình
          </RouterLink>
        </div>
        <div v-else-if="auth.isAuthenticated" class="home__cta">
          <RouterLink :to="{ name: 'path-list' }" class="home__cta-primary">
            Vào học Lộ trình
            <ArrowRight class="home__cta-arrow" aria-hidden="true" />
          </RouterLink>
          <RouterLink :to="{ name: 'simulations' }" class="home__cta-ghost">
            <Play class="home__cta-ghost-icon" :size="15" aria-hidden="true" />
            Mô phỏng thuật toán
          </RouterLink>
        </div>
        <div v-else class="home__cta">
          <RouterLink :to="{ name: 'path-list' }" class="home__cta-primary">
            {{ messages.home.ctaExplore }}
            <ArrowRight class="home__cta-arrow" aria-hidden="true" />
          </RouterLink>
          <RouterLink :to="{ name: 'register' }" class="home__cta-ghost">
            <Play class="home__cta-ghost-icon" :size="15" aria-hidden="true" />
            {{ messages.home.ctaStart }}
          </RouterLink>
        </div>
      </div>

      <LiveDemoBench />
    </div>
  </section>
</template>

<style scoped>
.home__hero {
  position: relative;
  min-height: 92vh;
  display: flex;
  align-items: center;
  overflow: hidden;
}

.home__cosmos {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.home__stars {
  position: absolute;
  inset: 0;
  background-repeat: repeat;
}

.home__stars--a {
  background-image:
    radial-gradient(1.2px 1.2px at 12% 22%, rgba(255, 255, 255, 0.85), transparent 100%),
    radial-gradient(1px 1px at 28% 62%, rgba(255, 255, 255, 0.6), transparent 100%),
    radial-gradient(1.4px 1.4px at 41% 18%, rgba(192, 132, 252, 0.8), transparent 100%),
    radial-gradient(1px 1px at 55% 78%, rgba(255, 255, 255, 0.5), transparent 100%),
    radial-gradient(1.2px 1.2px at 68% 34%, rgba(255, 255, 255, 0.7), transparent 100%),
    radial-gradient(1px 1px at 82% 12%, rgba(192, 132, 252, 0.7), transparent 100%),
    radial-gradient(1.3px 1.3px at 90% 58%, rgba(255, 255, 255, 0.55), transparent 100%),
    radial-gradient(1px 1px at 6% 84%, rgba(255, 255, 255, 0.5), transparent 100%);
  background-size: 460px 460px;
}

.home__stars--b {
  background-image:
    radial-gradient(1px 1px at 18% 42%, rgba(255, 255, 255, 0.45), transparent 100%),
    radial-gradient(1.3px 1.3px at 36% 88%, rgba(255, 255, 255, 0.6), transparent 100%),
    radial-gradient(1px 1px at 52% 8%, rgba(192, 132, 252, 0.6), transparent 100%),
    radial-gradient(1.2px 1.2px at 74% 66%, rgba(255, 255, 255, 0.5), transparent 100%),
    radial-gradient(1px 1px at 88% 26%, rgba(255, 255, 255, 0.45), transparent 100%),
    radial-gradient(1.4px 1.4px at 95% 86%, rgba(192, 132, 252, 0.55), transparent 100%);
  background-size: 340px 340px;
}

.home__grid-plane {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(168, 85, 247, 0.055) 1px, transparent 1px),
    linear-gradient(90deg, rgba(168, 85, 247, 0.055) 1px, transparent 1px);
  background-size: 46px 46px;
  -webkit-mask-image: radial-gradient(ellipse 90% 70% at 50% 40%, black 20%, transparent 75%);
  mask-image: radial-gradient(ellipse 90% 70% at 50% 40%, black 20%, transparent 75%);
}

.home__nebula {
  position: absolute;
  top: -12%;
  right: -6%;
  width: 640px;
  height: 640px;
  background: radial-gradient(circle, rgba(168, 85, 247, 0.16) 0%, rgba(192, 132, 252, 0.05) 45%, transparent 70%);
  filter: blur(10px);
  animation: cos-nebula 24s ease-in-out infinite alternate;
}

@keyframes cos-nebula {
  from { opacity: 0.7; transform: translate(0, 0) scale(1); }
  to { opacity: 1; transform: translate(-20px, 12px) scale(1.06); }
}

.home__glyph {
  position: absolute;
  font-family: var(--font-mono);
  color: rgba(255, 255, 255, 0.08);
  font-weight: 600;
  user-select: none;
}

.home__glyph--1 { top: 16%; left: 6%; font-size: 3rem; }
.home__glyph--2 { top: 68%; left: 12%; font-size: 4.5rem; }
.home__glyph--3 { top: 30%; right: 12%; font-size: 3.4rem; }
.home__glyph--4 { bottom: 22%; left: 4%; font-size: 3rem; }
.home__glyph--5 { top: 10%; right: 30%; font-size: 2.6rem; }
.home__glyph--6 { bottom: 14%; right: 8%; font-size: 2.8rem; }
.home__glyph--7 { top: 55%; left: 3%; font-size: 3.2rem; }
.home__glyph--8 { top: 22%; left: 42%; font-size: 2.4rem; }

.home__orbit {
  position: absolute;
  right: 8%;
  bottom: 6%;
  width: 200px;
  height: 200px;
  opacity: 0.8;
}

.home__orbit-ring {
  position: absolute;
  inset: 0;
  border: 1px dashed rgba(192, 132, 252, 0.28);
  border-radius: 50%;
  animation: cos-spin 60s linear infinite;
}

.home__orbit-core {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 34px;
  height: 34px;
  transform: translate(-50%, -50%);
  border-radius: 50%;
  background: radial-gradient(circle at 32% 30%, #c084fc, #7c3aed);
  box-shadow: 0 0 26px rgba(168, 85, 247, 0.25), 0 0 60px rgba(168, 85, 247, 0.18);
}

.home__orbit-dot {
  position: absolute;
  width: 9px;
  height: 9px;
  border-radius: 50%;
  box-shadow: 0 0 10px currentColor;
}

.home__orbit-dot--1 { top: 4%; left: 50%; color: #c084fc; animation: cos-spin 16s linear infinite; transform-origin: 0 96px; }
.home__orbit-dot--2 { top: 50%; left: 92%; color: #c084fc; animation: cos-spin 20s linear infinite reverse; transform-origin: -100px 0; }
.home__orbit-dot--3 { top: 82%; left: 8%; color: #c9a227; animation: cos-spin 24s linear infinite; transform-origin: 92px -64px; }

@keyframes cos-spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

.home__hero-grid {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-xl, 24px);
  padding-block: var(--space-2xl, 32px) var(--space-xl, 24px);
}

@media (min-width: 960px) {
  .home__hero-grid {
    grid-template-columns: repeat(12, minmax(0, 1fr));
    align-items: center;
    gap: var(--space-xl, 24px);
    padding-block: var(--space-3xl, 48px);
  }

  .home__hero-copy { grid-column: span 7; }
  :deep(.home__bench) { grid-column: span 5; }
}

.home__hero-copy {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-md, 16px);
}

.home__kicker {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm, 8px);
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.38);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.home__kicker-dot {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-full);
  background: #a855f7;
  box-shadow: 0 0 10px rgba(168, 85, 247, 0.25);
}

.home__badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin: 0;
  padding: 4px 12px;
  border-radius: var(--radius-full);
  background: rgba(168, 85, 247, 0.12);
  border: 1px solid rgba(168, 85, 247, 0.3);
  color: #c084fc;
  font-size: var(--text-xs);
  font-weight: 600;
  letter-spacing: 0.02em;
}

.home__badge-star {
  color: #c9a227;
  font-size: 11px;
}

.home__title {
  margin: 0;
  max-width: 20ch;
  font-size: var(--text-4xl);
  font-weight: 700;
  line-height: 1.12;
  letter-spacing: -0.03em;
  background: linear-gradient(120deg, #ffffff 20%, #c084fc 90%);
  -webkit-background-clip: text;
  background-clip: text;
  -webkit-text-fill-color: transparent;
  color: transparent;
}

.home__subtitle {
  margin: 0;
  max-width: 54ch;
  font-size: var(--text-md);
  line-height: 1.65;
  color: rgba(255, 255, 255, 0.62);
}

.home__cta {
  display: flex;
  gap: var(--space-md, 16px);
  flex-wrap: wrap;
  margin-top: var(--space-sm, 8px);
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

.home__cta-ghost-icon { color: #c084fc; }

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

@media (max-width: 768px) {
  .home__orbit { display: none; }
  .home__title { font-size: var(--text-3xl); }
  .home__glyph { display: none; }
}

@media (prefers-reduced-motion: reduce) {
  .home__nebula,
  .home__orbit-ring,
  .home__orbit-dot {
    animation: none !important;
  }
}
</style>
