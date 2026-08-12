<script setup lang="ts">
// HomeView — Màn 01: trang chủ công khai (SDD Màn 01)
// G-F2b: hero gradient Aurora + stats + 3 demo công khai (FR-7.6) + feature highlight.
// KHÔNG đổi route/logic — chỉ visual (Card shadcn + hover-lift + gradient + icon).
import { computed } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { ArrowUpDown, ArrowRight, Map, Network, Play, Search, Sparkles, Target } from 'lucide-vue-next';

import type { Component } from 'vue';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { CATALOG } from '@/engines/catalog';
import { messages } from '@/i18n/vi';

const router = useRouter();

/** Icon cho 3 demo công khai (FR-7.6: sort.bubble / search.binary / graph.bfs) */
const DEMO_ICONS: Record<string, Component> = {
  'sort.bubble': ArrowUpDown,
  'search.binary': Search,
  'graph.bfs': Network,
};

const FEATURE_ICONS = [Sparkles, Map, Target];

const demos = computed(() =>
  CATALOG.filter((c) => c.demoAllowed).map((c) => ({
    key: c.key,
    title: c.title,
    dataStructure: c.dataStructure,
    level: c.level,
    complexity: c.complexity,
    icon: DEMO_ICONS[c.key] ?? Play,
  })),
);

/** Số liệu tĩnh từ danh mục nội dung (44 mô phỏng — SDD §19.6A) */
const stats = computed(() => ({
  visuals: CATALOG.length,
  groups: new Set(CATALOG.map((c) => c.dataStructure)).size,
  levels: new Set(CATALOG.map((c) => c.level)).size,
}));

function openDemo(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}
</script>

<template>
  <main class="home">
    <header class="home__nav container">
      <span class="home__brand">{{ messages.app.name }}</span>
      <nav class="home__links" aria-label="Điều hướng công khai">
        <RouterLink :to="{ name: 'login' }">{{ messages.nav.login }}</RouterLink>
        <RouterLink :to="{ name: 'register' }" class="btn btn-primary btn--sm hover-lift">
          {{ messages.nav.register }}
        </RouterLink>
      </nav>
    </header>

    <!-- Hero — gradient Aurora (G-F2a palette) -->
    <section class="home__hero container">
      <div class="home__hero-inner bg-aurora-gradient">
        <p class="home__hero-badge">DSA Visual</p>
        <h1 class="home__title">{{ messages.home.heroTitle }}</h1>
        <p class="home__subtitle">{{ messages.home.heroSubtitle }}</p>
        <div class="home__cta">
          <RouterLink :to="{ name: 'simulations' }" class="btn btn--light hover-glow">
            {{ messages.home.ctaExplore }}
          </RouterLink>
          <RouterLink :to="{ name: 'register' }" class="btn btn--outline-light hover-lift">
            {{ messages.home.ctaStart }}
          </RouterLink>
        </div>
      </div>
    </section>

    <!-- Số liệu (SDD Màn 01 — nguồn danh mục nội dung) -->
    <section class="home__stats container" aria-label="Thống kê">
      <div class="home__stat">
        <span class="home__stat-value">{{ stats.visuals }}+</span>
        <span class="home__stat-label">{{ messages.home.statsVisuals }}</span>
      </div>
      <div class="home__stat">
        <span class="home__stat-value">{{ stats.groups }}</span>
        <span class="home__stat-label">{{ messages.home.statsGroups }}</span>
      </div>
      <div class="home__stat">
        <span class="home__stat-value">{{ stats.levels }}</span>
        <span class="home__stat-label">{{ messages.home.statsLevels }}</span>
      </div>
      <p class="home__stats-note">{{ messages.home.statsNote }}</p>
    </section>

    <!-- 3 demo công khai (FR-7.6) — Card shadcn + hover-lift -->
    <section class="home__section container">
      <div class="home__section-head">
        <span class="home__kicker">{{ messages.home.demoBadge }}</span>
        <h2 class="home__section-title text-gradient-aurora">{{ messages.home.demoTabTitle }}</h2>
        <p class="home__section-desc">{{ messages.home.demoTabDesc }}</p>
      </div>

      <div class="home__grid">
        <Card
          v-for="demo in demos"
          :key="demo.key"
          class="hover-lift home__demo"
        >
          <CardHeader>
            <div class="home__demo-icon" aria-hidden="true">
              <component :is="demo.icon" :size="22" />
            </div>
            <CardTitle class="home__demo-title">{{ demo.title }}</CardTitle>
            <CardDescription class="home__demo-meta">
              {{ demo.dataStructure }} · Cấp độ {{ demo.level }}
            </CardDescription>
          </CardHeader>
          <CardContent class="home__demo-content">
            <dl class="home__demo-complexity">
              <dt>{{ messages.home.demoComplexity }}</dt>
              <dd>TB {{ demo.complexity.average }} · {{ demo.complexity.space }}</dd>
            </dl>
            <button
              type="button"
              class="home__demo-btn"
              :aria-label="`${messages.home.demoOpen} ${demo.title}`"
              @click="openDemo(demo.key)"
            >
              <Play :size="16" aria-hidden="true" />
              {{ messages.home.demoRun }}
              <ArrowRight :size="16" aria-hidden="true" />
            </button>
          </CardContent>
        </Card>
      </div>
    </section>

    <!-- Feature highlight (SDD Màn 01 — 3 tính năng chính) -->
    <section class="home__section container">
      <div class="home__grid home__grid--features">
        <Card
          v-for="(feature, idx) in [
            { title: messages.home.featureVisual.title, desc: messages.home.featureVisual.desc },
            { title: messages.home.featurePath.title, desc: messages.home.featurePath.desc },
            { title: messages.home.featurePractice.title, desc: messages.home.featurePractice.desc },
          ]"
          :key="feature.title"
          class="hover-lift"
        >
          <CardHeader>
            <div class="home__demo-icon home__demo-icon--accent" aria-hidden="true">
              <component :is="FEATURE_ICONS[idx]" :size="22" />
            </div>
            <CardTitle class="home__feature-title">{{ feature.title }}</CardTitle>
            <CardDescription>{{ feature.desc }}</CardDescription>
          </CardHeader>
        </Card>
      </div>
    </section>
  </main>
</template>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: var(--space-2xl);
  padding-bottom: var(--space-3xl);
}

.home__nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-block: var(--space-lg);
}

.home__brand {
  font-weight: 800;
  font-size: var(--text-lg);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.home__links {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.btn--sm {
  padding: 0.5rem var(--space-md);
  font-size: var(--text-sm);
}

/* ── Hero ── */
.home__hero-inner {
  position: relative;
  overflow: hidden;
  border-radius: var(--radius-xl);
  padding: clamp(2rem, 6vw, 4.5rem) clamp(1.5rem, 5vw, 3.5rem);
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-lg);
  box-shadow: var(--shadow-lg);
  isolation: isolate;
}

/* Đốm sáng trang trí — không chặn tương tác.
   GP-T9b: giảm opacity → giảm "vùng chói" trên gradient (#4). */
.home__hero-inner::before,
.home__hero-inner::after {
  content: '';
  position: absolute;
  border-radius: 50%;
  filter: blur(48px);
  opacity: 0.28;
  z-index: -1;
}

.home__hero-inner::before {
  width: 320px;
  height: 320px;
  background: rgba(255, 255, 255, 0.35);
  top: -120px;
  left: -80px;
}

.home__hero-inner::after {
  width: 260px;
  height: 260px;
  background: rgba(255, 255, 255, 0.28);
  bottom: -100px;
  right: -60px;
}

.home__hero-badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 14px;
  border-radius: var(--radius-full);
  background: rgba(255, 255, 255, 0.22);
  border: 1px solid rgba(255, 255, 255, 0.45);
  color: #fff;
  font-size: var(--text-xs);
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  backdrop-filter: blur(4px);
}

/* GP-T9b (#6): dark mode gradient sáng (0.8-0.86) làm chữ trắng chỉ ~1.8:1
   → phủ lớp tối để chữ trắng ≥ 4.5:1. Light đã có gradient tối (0.52-0.55) nên không cần. */
.dark .home__hero-inner {
  background-image: linear-gradient(rgba(4, 47, 46, 0.62), rgba(4, 47, 46, 0.62)), var(--gradient-aurora);
}

.dark .home__hero-inner::before,
.dark .home__hero-inner::after {
  opacity: 0.12;
}

.home__title {
  max-width: 20ch;
  font-size: clamp(2rem, 5vw, 3.5rem);
  color: #fff;
  text-shadow: 0 2px 12px rgba(0, 0, 0, 0.18);
}

.home__subtitle {
  max-width: 52ch;
  color: rgba(255, 255, 255, 0.92);
  font-size: var(--text-md);
}

.home__cta {
  display: flex;
  gap: var(--space-lg);
  flex-wrap: wrap;
  justify-content: center;
  margin-top: var(--space-sm);
}

.btn--light {
  background: #fff;
  color: var(--color-primary);
}

.btn--light:hover {
  background: rgba(255, 255, 255, 0.92);
}

.btn--outline-light {
  background: transparent;
  color: #fff;
  border: 2px solid rgba(255, 255, 255, 0.7);
}

.btn--outline-light:hover {
  background: rgba(255, 255, 255, 0.15);
  border-color: #fff;
}

/* ── Stats ── */
.home__stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  padding: var(--space-lg);
  box-shadow: var(--shadow-md);
}

.home__stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  text-align: center;
}

.home__stat-value {
  font-size: var(--text-2xl);
  font-weight: 800;
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.home__stat-label {
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  font-weight: 600;
}

.home__stats-note {
  grid-column: 1 / -1;
  text-align: center;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

/* ── Sections chung ── */
.home__section {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.home__section-head {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
}

.home__kicker {
  font-size: var(--text-xs);
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--color-primary);
}

.home__section-title {
  font-size: var(--text-2xl);
  margin: 0;
}

.home__section-desc {
  max-width: 56ch;
  color: var(--color-text-muted);
  font-size: var(--text-sm);
}

.home__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: var(--space-lg);
}

.home__grid--features {
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
}

.home__demo {
  display: flex;
  flex-direction: column;
}

.home__demo-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: #fff;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-sm);
  box-shadow: var(--shadow-md);
}

.home__demo-icon--accent {
  background-image: var(--gradient-sunset);
}

.home__demo-title {
  font-size: var(--text-md);
  line-height: 1.3;
}

.home__demo-meta {
  margin-top: 4px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
}

.home__demo-content {
  margin-top: auto;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.home__demo-complexity {
  display: flex;
  justify-content: space-between;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  border-top: 1px solid var(--color-border);
  padding-top: var(--space-sm);
}

.home__demo-complexity dt { font-weight: 700; color: var(--color-foreground); }
.home__demo-complexity dd { font-family: var(--font-mono); }

.home__demo-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  width: 100%;
  padding: 0.6rem 1rem;
  border-radius: var(--radius-md);
  background: var(--color-primary);
  color: var(--color-on-primary);
  font-weight: 700;
  font-size: var(--text-sm);
  border: none;
  cursor: pointer;
  transition: transform 180ms ease, box-shadow 180ms ease, filter 180ms ease;
}

.home__demo-btn:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-md);
  filter: brightness(1.05);
}

.home__feature-title {
  font-size: var(--text-md);
}

@media (max-width: 640px) {
  .home__stats { grid-template-columns: 1fr; gap: var(--space-md); }
  .home__stats-note { grid-column: 1; }
}
</style>
