<script setup lang="ts">
// HomeView — Màn 01: trang chủ công khai (SDD Màn 01).
// Tân trang 15/08: chủ đề "vũ trụ toán học" — sao/nebula/lưới tọa độ/glyph công thức
// nổi (O(log n), Σ, √, λ...) + panel demo chạy engine THẬT kiểu terminal glass.
// Palette "terminal dark" bê từ VisualizationDSA-main (nền #0d0c14, accent #a855f7/#c084fc).
// GIỮ NGUYÊN logic demo: 3 demo công khai catalog.ts chạy engine thật, playback theo bước.
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import {
  AlertCircle,
  ArrowRight,
  ArrowUpDown,
  Gauge,
  Layers,
  Map,
  Network,
  Play,
  Search,
  Target,
  Users,
} from 'lucide-vue-next';

import type { Component, Directive } from 'vue';
import type { InputConfig, Step } from '@/engines/core/types';
import { CATALOG } from '@/engines/catalog';
import { getSimulation } from '@/engines/registry';
import { messages } from '@/i18n/vi';
import { getActivePinia } from 'pinia';
import { useAuthStore } from '@/stores/auth';
import Button from '@/components/ui/Button.vue';
import BlockToken from '@/components/ui/BlockToken.vue';

const router = useRouter();
const auth = getActivePinia() ? useAuthStore() : ({ role: null, isAuthenticated: false } as unknown as ReturnType<typeof useAuthStore>);

/* ── v-reveal: hiện dần khi lướt xuống (IntersectionObserver — fade + slide lên).
   `v-reveal` không delay, `v-reveal="120"` → delay 120ms (stagger cho card). ── */
const vReveal: Directive<HTMLElement, number> = {
  mounted(el, binding) {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    // jsdom/test không có IntersectionObserver → bỏ qua (hiện luôn)
    if (typeof IntersectionObserver === 'undefined') return;
    el.classList.add('home-reveal');
    const delay = binding.value ?? 0;
    if (delay > 0) el.style.transitionDelay = `${delay}ms`;
    const io = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            el.classList.add('home-reveal--in');
            io.disconnect();
          }
        }
      },
      { threshold: 0.12 },
    );
    io.observe(el);
    el.dataset.revealIo = '1';
    (el as HTMLElement & { __revealIo?: IntersectionObserver }).__revealIo = io;
  },
  unmounted(el) {
    (el as HTMLElement & { __revealIo?: IntersectionObserver }).__revealIo?.disconnect();
  },
};

/** Icon cho 3 demo công khai (FR-7.6: sort.bubble / search.binary / graph.bfs) */
const DEMO_ICONS: Record<string, Component> = {
  'sort.bubble': ArrowUpDown,
  'search.binary': Search,
  'graph.bfs': Network,
};

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

/* ── Hero mini-sim: step THẬT từ engine (catalog.ts + registry) ── */
const DEMO_INPUTS: Record<string, InputConfig> = {
  'sort.bubble': { kind: 'array', data: { values: [5, 3, 8, 1, 9, 2] } },
  'search.binary': { kind: 'array', data: { target: 19, inputSource: 'manual', values: [2, 5, 8, 12, 19, 23] } },
  'graph.bfs': { kind: 'graph', data: { preset: 'path', directed: false, weighted: false, vertices: 6, edges: 5, source: 0 } },
};

const DEMO_KEYS = ['sort.bubble', 'search.binary', 'graph.bfs'] as const;

const activeKey = ref<(typeof DEMO_KEYS)[number]>('sort.bubble');
const steps = shallowRef<Step[]>([]);
const stepIndex = ref(0);
const stepTimer = ref<ReturnType<typeof setInterval> | null>(null);
const restartTimer = ref<ReturnType<typeof setTimeout> | null>(null);

const activeMeta = computed(() => CATALOG.find((c) => c.key === activeKey.value));
const currentStep = computed(() => steps.value[stepIndex.value] ?? null);
const frameElements = computed(() => currentStep.value?.structure.elements ?? []);
const stepLabel = computed(() => messages.home.simStepOf(stepIndex.value + 1, steps.value.length));

const demoOptions = computed(() =>
  DEMO_KEYS.map((key) => ({
    key,
    label: key === 'sort.bubble'
      ? messages.home.demoBubble
      : key === 'search.binary'
        ? messages.home.demoBinary
        : messages.home.demoBfs,
  })),
);

const prefersReducedMotion = (): boolean =>
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function stopPlayback(): void {
  if (stepTimer.value) clearInterval(stepTimer.value);
  if (restartTimer.value) clearTimeout(restartTimer.value);
  stepTimer.value = null;
  restartTimer.value = null;
}

function startPlayback(): void {
  stopPlayback();
  // Tôn trọng prefers-reduced-motion: không autoplay, giữ frame đầu tĩnh.
  if (prefersReducedMotion()) return;
  stepTimer.value = setInterval(() => {
    if (stepIndex.value < steps.value.length - 1) {
      stepIndex.value++;
    } else {
      // Hết dãy → dừng 1 nhịp rồi lặp nhẹ (ambient, không giật vòng lại ngay).
      stopPlayback();
      restartTimer.value = setTimeout(() => {
        stepIndex.value = 0;
        startPlayback();
      }, 1400);
    }
  }, 380);
}

function loadDemo(key: (typeof DEMO_KEYS)[number]): void {
  const gen = getSimulation(key);
  if (!gen) return;
  steps.value = gen.generate(DEMO_INPUTS[key]);
  stepIndex.value = 0;
}

function selectDemo(key: (typeof DEMO_KEYS)[number]): void {
  if (key === activeKey.value) return;
  activeKey.value = key;
  loadDemo(key);
  startPlayback();
}

onMounted(() => {
  loadDemo(activeKey.value);
  startPlayback();
  // Hero mờ + trượt nhẹ khi cuộn xuống (parallax cảm giác)
  if (!prefersReducedMotion()) {
    window.addEventListener('scroll', onHeroScroll, { passive: true });
  }
});

function onHeroScroll(): void {
  const hero = document.querySelector<HTMLElement>('.home__hero');
  if (!hero) return;
  const y = window.scrollY;
  const p = Math.min(y / 420, 1);
  const ease = 1 - Math.pow(1 - p, 2);
  // Fade dần toàn bộ hero (nền vũ trụ + nội dung) khi lướt xuống
  hero.style.opacity = String(1 - ease * 0.92);
  // Khối chữ trượt xuống chậm hơn — cảm giác chiều sâu
  const copy = document.querySelector('.home__hero-copy');
  if (copy) copy.setAttribute('style', `transform: translateY(${y * 0.14}px);`);
}

onUnmounted(() => {
  stopPlayback();
  window.removeEventListener('scroll', onHeroScroll);
});

/** Block status (engine ElementStatus) → class màu block-token (DESIGN §2.1). */
function blockStatusClass(status: string): string {
  switch (status) {
    case 'swap':
      return 'home__block--swap';
    case 'done':
      return 'home__block--done';
    case 'muted':
      return 'home__block--muted';
    case 'active':
    case 'highlight':
      return 'home__block--active';
    default:
      return 'home__block--default';
  }
}
</script>

<template>
  <main class="home">
    <!-- ══ HERO — vũ trụ toán học: sao + nebula + lưới tọa độ + glyph công thức + quỹ đạo ══ -->
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
            <RouterLink :to="{ name: 'courses' }" class="home__cta-primary">
              Vào học Lộ trình
              <ArrowRight class="home__cta-arrow" aria-hidden="true" />
            </RouterLink>
            <RouterLink :to="{ name: 'simulations' }" class="home__cta-ghost">
              <Play class="home__cta-ghost-icon" :size="15" aria-hidden="true" />
              Mô phỏng thuật toán
            </RouterLink>
          </div>
          <div v-else class="home__cta">
            <RouterLink :to="{ name: 'courses' }" class="home__cta-primary">
              {{ messages.home.ctaExplore }}
              <ArrowRight class="home__cta-arrow" aria-hidden="true" />
            </RouterLink>
            <RouterLink :to="{ name: 'register' }" class="home__cta-ghost">
              <Play class="home__cta-ghost-icon" :size="15" aria-hidden="true" />
              {{ messages.home.ctaStart }}
            </RouterLink>
          </div>
        </div>

        <!-- Panel demo tối — terminal glass chạy engine THẬT từng bước -->
        <div class="home__bench" aria-label="Mô phỏng trực quan đang chạy — mở mô phỏng để tương tác từng bước">
          <div class="home__bench-bar">
            <span class="home__bench-dot home__bench-dot--red" />
            <span class="home__bench-dot home__bench-dot--yellow" />
            <span class="home__bench-dot home__bench-dot--green" />
            <span class="home__bench-title">{{ messages.home.benchTitle }}</span>
            <span class="home__bench-live">
              <span class="home__bench-live-dot" aria-hidden="true" />
              {{ messages.home.simLive }}
            </span>
          </div>

          <div class="home__bench-head">
            <span class="home__bench-key">{{ activeMeta?.key }}</span>
            <span class="home__bench-step">{{ stepLabel }}</span>
          </div>

          <div
            :key="activeKey"
            class="home__bench-stage"
            role="img"
            :aria-label="`${activeMeta?.title ?? ''} — ${stepLabel}`"
          >
            <div
              v-for="(el, idx) in frameElements"
              :key="`${el.id}::${stepIndex}`"
              class="home__block"
              :class="blockStatusClass(el.status)"
            >
              <span class="home__block-value">{{ el.label }}</span>
              <span class="home__block-index">{{ String(idx).padStart(2, '0') }}</span>
            </div>
          </div>

          <p class="home__bench-explain">
            {{ currentStep?.explanation ?? messages.home.benchReady }}
          </p>

          <div class="home__bench-tabs" role="group" aria-label="Chọn mô phỏng demo">
            <Button
              v-for="opt in demoOptions"
              :key="opt.key"
              type="button"
              variant="ghost"
              size="sm"
              class="home__bench-tab"
              :class="{ 'home__bench-tab--active': opt.key === activeKey }"
              :aria-pressed="opt.key === activeKey"
              @click="selectDemo(opt.key)"
            >
              {{ opt.label }}
            </Button>
          </div>
        </div>
      </div>
    </section>

    <!-- ══ STATS — dải glass số liệu hệ sinh thái ══ -->
    <section class="home__stats container" aria-label="Thống kê" v-reveal>
      <BlockToken
        class="home__stat-hero"
        :label="messages.home.statsVisuals"
        :value="`${stats.visuals}+`"
        :aria-label="`${stats.visuals}+ ${messages.home.statsVisuals}`"
      />
      <div class="home__stat">
        <Layers class="home__stat-icon" :size="16" aria-hidden="true" />
        <span class="home__stat-value">{{ stats.groups }}</span>
        <span class="home__stat-label">{{ messages.home.statsGroups }}</span>
      </div>
      <div class="home__stat">
        <Gauge class="home__stat-icon" :size="16" aria-hidden="true" />
        <span class="home__stat-value">{{ stats.levels }}</span>
        <span class="home__stat-label">{{ messages.home.statsLevels }}</span>
      </div>
      <div class="home__stat">
        <span class="home__stat-icon home__stat-icon--mono" aria-hidden="true">100%</span>
        <span class="home__stat-value">100%</span>
        <span class="home__stat-label">{{ messages.home.statsViet }}</span>
      </div>
    </section>

    <!-- ══ DEMO — 3 mô phỏng công khai (FR-7.6) ══ -->
    <section class="home__section container" v-reveal>
      <div class="home__section-head" v-reveal>
        <span class="home__kicker home__kicker--center">
          <span class="home__kicker-dot" aria-hidden="true" />
          <span class="font-mono">{{ messages.home.demoBadge }}</span>
        </span>
        <h2 class="home__section-title">{{ messages.home.demoTabTitle }}</h2>
        <p class="home__section-desc">{{ messages.home.demoTabDesc }}</p>
      </div>

      <div class="home__grid">
        <div v-for="(demo, di) in demos" :key="demo.key" class="home__demo glass-card" v-reveal="di * 100">
          <div class="home__demo-thumb" aria-hidden="true">
            <div v-if="demo.key === 'sort.bubble'" class="home__thumb-bars">
              <span class="home__thumb-bar" />
              <span class="home__thumb-bar" />
              <span class="home__thumb-bar" />
              <span class="home__thumb-bar home__thumb-bar--done" />
              <span class="home__thumb-bar home__thumb-bar--done" />
            </div>
            <div v-else-if="demo.key === 'search.binary'" class="home__thumb-row">
              <span class="home__thumb-block" />
              <span class="home__thumb-block" />
              <span class="home__thumb-block home__thumb-block--found" />
              <span class="home__thumb-block" />
              <span class="home__thumb-block" />
            </div>
            <div v-else class="home__thumb-graph">
              <span class="home__thumb-node" />
              <span class="home__thumb-edge" />
              <span class="home__thumb-node home__thumb-node--visited" />
              <span class="home__thumb-edge" />
              <span class="home__thumb-node" />
              <span class="home__thumb-edge" />
              <span class="home__thumb-node" />
            </div>
          </div>

          <div class="home__demo-body">
            <h3 class="home__demo-title">
              <component :is="demo.icon" :size="16" class="home__demo-title-icon" aria-hidden="true" />
              {{ demo.title }}
            </h3>
            <p class="home__demo-meta">
              {{ demo.dataStructure }} · Cấp độ {{ demo.level }}
            </p>
            <dl class="home__demo-complexity">
              <dt>{{ messages.home.demoComplexity }}</dt>
              <dd>TB {{ demo.complexity.average }} · {{ demo.complexity.space }}</dd>
            </dl>
            <button
              type="button"
              class="home__demo-run"
              :aria-label="`${messages.home.demoOpen} ${demo.title}`"
              @click="openDemo(demo.key)"
            >
              <Play class="size-4" aria-hidden="true" />
              {{ messages.home.demoRun }}
              <ArrowRight class="size-4" aria-hidden="true" />
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- ══ FEATURES — ba chìa khóa làm chủ thuật toán ══ -->
    <section class="home__section container" v-reveal>
      <div class="home__section-head" v-reveal>
        <span class="home__kicker home__kicker--center">
          <span class="home__kicker-dot" aria-hidden="true" />
          <span class="font-mono">{{ messages.home.featuresKicker }}</span>
        </span>
        <h2 class="home__section-title">{{ messages.home.ctaExplore }}</h2>
      </div>

      <div class="home__grid home__grid--features">
        <div class="home__feature home__feature--featured home__feature--visual glass-card" v-reveal>
          <div class="home__feature-icon" aria-hidden="true">
            <Play :size="22" />
          </div>
          <h3 class="home__feature-title">{{ messages.home.featureVisual.title }}</h3>
          <p class="home__feature-desc">{{ messages.home.featureVisual.desc }}</p>
        </div>

        <div class="home__feature home__feature--featured home__feature--path glass-card" v-reveal="120">
          <div class="home__feature-icon" aria-hidden="true">
            <Map :size="22" />
          </div>
          <h3 class="home__feature-title">{{ messages.home.featurePath.title }}</h3>
          <p class="home__feature-desc">{{ messages.home.featurePath.desc }}</p>
        </div>

        <div class="home__feature home__feature--compact glass-card" v-reveal="200">
          <div class="home__feature-icon" aria-hidden="true">
            <Target :size="20" />
          </div>
          <div>
            <h3 class="home__feature-title">{{ messages.home.featurePractice.title }}</h3>
            <p class="home__feature-desc">{{ messages.home.featurePractice.desc }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ══ CTA BAND — kết trang vũ trụ ══ -->
    <section class="home__cta-band">
      <div class="home__cta-band-glow" aria-hidden="true" />
      <div class="home__cta-band-inner" v-reveal>
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
          <RouterLink :to="{ name: 'courses' }" class="home__cta-primary">
            {{ messages.home.ctaGoCourses }}
            <ArrowRight class="home__cta-arrow" aria-hidden="true" />
          </RouterLink>
          <RouterLink :to="{ name: 'simulations' }" class="home__cta-ghost">
            {{ messages.home.ctaGoSims }}
          </RouterLink>
        </div>
        <div v-else class="home__cta">
          <RouterLink :to="{ name: 'courses' }" class="home__cta-primary">
            {{ messages.home.ctaGoCourses }}
            <ArrowRight class="home__cta-arrow" aria-hidden="true" />
          </RouterLink>
          <RouterLink :to="{ name: 'register' }" class="home__cta-ghost">
            Đăng ký học ngay
          </RouterLink>
        </div>
      </div>
    </section>
  </main>
</template>

<style scoped>
/* ════════════════════════════════════════════════════════════════
   VŨ TRỤ TOÁN HỌC — palette "terminal dark" bê từ VisualizationDSA-main
   (nền #0d0c14/#181c19, accent #a855f7/#c084fc, glass blur + saturate)
   ════════════════════════════════════════════════════════════════ */
.home {
  --cos-bg: #0b0a12;
  --cos-bg-deep: #0d0c14;
  --cos-bg-panel: rgba(13, 12, 20, 0.72);
  --cos-bg-terminal: rgba(10, 9, 16, 0.82);
  --cos-border: rgba(255, 255, 255, 0.07);
  --cos-border-strong: rgba(255, 255, 255, 0.14);
  --cos-purple: #a855f7;
  --cos-purple-light: #c084fc;
  --cos-purple-dark: #7c3aed;
  --cos-purple-dim: rgba(168, 85, 247, 0.12);
  --cos-purple-glow: rgba(168, 85, 247, 0.25);
  --cos-red: #b85c5c;
  --cos-yellow: #c9a227;
  --cos-text: rgba(255, 255, 255, 0.9);
  --cos-text-2: rgba(255, 255, 255, 0.62);
  --cos-text-3: rgba(255, 255, 255, 0.38);

  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-2xl);
  padding-bottom: var(--space-3xl);
  /* Trang chủ tràn lên tận đỉnh: bù padding --app-header-h của app-shell để hero
     (sao/glow tím) phủ kín 0–112px — không còn dải nền phẳng "đen đen" sau cụm nav nổi.
     Nền trong suốt để chòm sao tương tác (canvas fixed) hiện xuyên qua;
     nền nền tối là body (#0B0A12). */
  margin-top: calc(-1 * var(--app-header-h, 112px));
  background: radial-gradient(ellipse 80% 50% at 50% -10%, rgba(168, 85, 247, 0.08), transparent 70%);
  color: var(--cos-text);
  overflow: hidden;
}

/* ── Reveal khi lướt xuống (v-reveal — fade + slide lên) ── */
.home-reveal {
  opacity: 0;
  transform: translateY(26px);
  transition:
    opacity 650ms cubic-bezier(0.16, 1, 0.3, 1),
    transform 650ms cubic-bezier(0.16, 1, 0.3, 1);
  will-change: opacity, transform;
}

.home-reveal--in {
  opacity: 1;
  transform: none;
}

@media (prefers-reduced-motion: reduce) {
  .home-reveal {
    opacity: 1;
    transform: none;
    transition: none;
  }
}

/* ── HERO ── */
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

/* Sao — 2 lớp radial-gradient lặp, nhấp nháy nhẹ */
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

/* Lưới tọa độ toán học — mờ dần ra rìa */
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

/* Nebula tím — thở rất chậm (tối giản) */
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

/* Glyph công thức — TĨNH (bỏ animation trôi) */
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

/* Quỹ đạo — hành tinh dữ liệu (xoay chậm, tối giản) */
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
  background: radial-gradient(circle at 32% 30%, var(--cos-purple-light), var(--cos-purple-dark));
  box-shadow: 0 0 26px var(--cos-purple-glow), 0 0 60px rgba(168, 85, 247, 0.18);
}

.home__orbit-dot {
  position: absolute;
  width: 9px;
  height: 9px;
  border-radius: 50%;
  box-shadow: 0 0 10px currentColor;
}

.home__orbit-dot--1 { top: 4%; left: 50%; color: var(--cos-purple-light); animation: cos-spin 16s linear infinite; transform-origin: 0 96px; }
.home__orbit-dot--2 { top: 50%; left: 92%; color: var(--cos-purple-light); animation: cos-spin 20s linear infinite reverse; transform-origin: -100px 0; }
.home__orbit-dot--3 { top: 82%; left: 8%; color: var(--cos-yellow); animation: cos-spin 24s linear infinite; transform-origin: 92px -64px; }

@keyframes cos-spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}

/* ── Hero nội dung ── */
.home__hero-grid {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-xl);
  padding-block: var(--space-2xl) var(--space-xl);
}

@media (min-width: 960px) {
  .home__hero-grid {
    grid-template-columns: repeat(12, minmax(0, 1fr));
    align-items: center;
    gap: var(--space-xl);
    padding-block: var(--space-3xl);
  }

  .home__hero-copy { grid-column: span 7; }
  .home__bench { grid-column: span 5; }
}

.home__hero-copy {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-md);
}

.home__kicker {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--cos-text-3);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.home__kicker--center { justify-content: center; }

.home__kicker-dot {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-full);
  background: var(--cos-purple);
  box-shadow: 0 0 10px var(--cos-purple-glow);
}

.home__badge {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  margin: 0;
  padding: 4px 12px;
  border-radius: var(--radius-full);
  background: var(--cos-purple-dim);
  border: 1px solid rgba(168, 85, 247, 0.3);
  color: var(--cos-purple-light);
  font-size: var(--text-xs);
  font-weight: 600;
  letter-spacing: 0.02em;
}

.home__badge-star {
  color: var(--cos-yellow);
  font-size: 11px;
}

.home__title {
  margin: 0;
  max-width: 20ch;
  font-size: var(--text-4xl);
  font-weight: 700;
  line-height: 1.12;
  letter-spacing: -0.03em;
  background: linear-gradient(120deg, #ffffff 20%, var(--cos-purple-light) 90%);
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
  color: var(--cos-text-2);
}

.home__cta {
  display: flex;
  gap: var(--space-md);
  flex-wrap: wrap;
  margin-top: var(--space-sm);
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
  background: linear-gradient(135deg, var(--cos-purple), var(--cos-purple-dark));
  color: #fff;
  box-shadow: 0 0 22px rgba(168, 85, 247, 0.35);
}

.home__cta-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 0 34px rgba(168, 85, 247, 0.5);
}

.home__cta-arrow { width: 15px; height: 15px; }

.home__cta-ghost {
  background: var(--cos-bg-panel);
  -webkit-backdrop-filter: blur(12px) saturate(1.3);
  backdrop-filter: blur(12px) saturate(1.3);
  color: var(--cos-text);
  border: 1px solid var(--cos-border-strong);
}

.home__pending-banner {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
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

.home__cta-ghost:hover {
  border-color: var(--cos-purple);
  color: var(--cos-purple-light);
  transform: translateY(-2px);
}

.home__cta-ghost-icon { color: var(--cos-purple-light); }

/* ── Bench — terminal glass chạy engine thật ── */
.home__bench {
  position: relative;
  background: var(--cos-bg-terminal);
  -webkit-backdrop-filter: blur(18px) saturate(1.4);
  backdrop-filter: blur(18px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-xl);
  padding: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 24px 60px rgba(0, 0, 0, 0.45), inset 0 1px 0 rgba(255, 255, 255, 0.04);
  animation: bench-in 600ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes bench-in {
  from { opacity: 0; transform: translateY(18px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.home__bench-bar {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 10px 14px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  background: rgba(255, 255, 255, 0.02);
}

.home__bench-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

.home__bench-dot--red { background: var(--cos-red); }
.home__bench-dot--yellow { background: var(--cos-yellow); }
.home__bench-dot--green { background: var(--cos-purple); }

.home__bench-title {
  margin-left: 6px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--cos-text-3);
}

.home__bench-live {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-mono);
  font-size: 10px;
  letter-spacing: 0.1em;
  color: var(--cos-purple-light);
}

.home__bench-live-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--cos-purple-light);
  box-shadow: 0 0 8px var(--cos-purple);
  animation: bench-live 2s cubic-bezier(0.16, 1, 0.3, 1) infinite;
}

@keyframes bench-live {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.2; }
}

.home__bench-head {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: 14px 18px 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
}

.home__bench-key {
  color: var(--cos-purple-light);
}

.home__bench-key::before {
  content: '>';
  margin-right: 6px;
  color: var(--cos-text-3);
}

.home__bench-step {
  margin-left: auto;
  color: var(--cos-text-3);
  white-space: nowrap;
}

.home__bench-stage {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: var(--space-sm);
  min-height: 88px;
  padding: 14px 18px 6px;
  animation: stage-in 250ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes stage-in {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}

.home__block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  width: 44px;
  height: 58px;
  border-radius: var(--radius-md);
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.home__block-value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  color: rgba(255, 255, 255, 0.92);
}

.home__block-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.35);
}

.home__block--default { background: #181225; border-color: rgba(168, 85, 247, 0.35); }
.home__block--active { background: var(--cos-purple-dark); box-shadow: 0 0 14px rgba(168, 85, 247, 0.4); }
.home__block--swap { background: var(--cos-red); animation: bench-pop 240ms cubic-bezier(0.16, 1, 0.3, 1) both; }
.home__block--done { background: var(--cos-purple); box-shadow: 0 0 10px rgba(168, 85, 247, 0.35); }
.home__block--muted { background: #181225; opacity: 0.4; }

@keyframes bench-pop {
  0% { transform: scale(1); }
  50% { transform: scale(1.12); }
  100% { transform: scale(1); }
}

.home__bench-explain {
  margin: 0;
  min-height: 38px;
  padding: 6px 18px 4px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  line-height: 1.55;
  color: rgba(255, 255, 255, 0.55);
}

.home__bench-tabs {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  padding: 8px 14px 14px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  margin-top: 2px;
}

.home__bench-tab {
  color: var(--cos-text-3);
  border-radius: var(--radius-md);
}

.home__bench-tab:hover {
  color: rgba(255, 255, 255, 0.9);
  background: rgba(255, 255, 255, 0.07);
}

.home__bench-tab--active {
  background: var(--cos-purple-dim);
  color: var(--cos-purple-light);
  box-shadow: inset 0 0 0 1px rgba(168, 85, 247, 0.35);
}

/* ── Stats — dải glass ── */
.home__stats {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-md);
  background: var(--cos-bg-panel);
  -webkit-backdrop-filter: blur(16px) saturate(1.3);
  backdrop-filter: blur(16px) saturate(1.3);
  border: 1px solid var(--cos-border);
  border-radius: var(--radius-xl);
  padding: var(--space-lg);
}

.home__stat-hero { width: 100%; }

.home__stat {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-xs);
  padding: var(--space-sm) var(--space-md);
}

.home__stat-icon {
  color: var(--cos-purple-light);
}

.home__stat-icon--mono {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 700;
}

.home__stat-value {
  font-family: var(--font-mono);
  font-size: var(--text-2xl);
  font-weight: 700;
  letter-spacing: -0.015em;
  color: #fff;
}

.home__stat-label {
  font-size: var(--text-xs);
  color: var(--cos-text-3);
}

/* ── Sections chung ── */
.home__section {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.home__section-head {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm);
}

.home__section-title {
  margin: 0;
  font-size: var(--text-3xl);
  font-weight: 700;
  letter-spacing: -0.025em;
  color: #fff;
}

.home__section-desc {
  margin: 0;
  max-width: 56ch;
  color: var(--cos-text-2);
  font-size: var(--text-sm);
}

.home__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: var(--space-lg);
}

.home__grid--features {
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: var(--space-lg);
}

/* Glass card chung */
.glass-card {
  background: var(--cos-bg-panel);
  -webkit-backdrop-filter: blur(16px) saturate(1.3);
  backdrop-filter: blur(16px) saturate(1.3);
  border: 1px solid var(--cos-border);
  border-radius: var(--radius-xl);
  box-shadow: 0 10px 34px rgba(0, 0, 0, 0.25), inset 0 1px 0 rgba(255, 255, 255, 0.03);
  transition: transform 260ms cubic-bezier(0.34, 1.56, 0.64, 1), border-color 200ms ease, box-shadow 260ms ease;
}

.glass-card:hover {
  transform: translateY(-4px);
  border-color: rgba(168, 85, 247, 0.4);
  box-shadow: 0 16px 44px rgba(0, 0, 0, 0.35), 0 0 24px rgba(168, 85, 247, 0.1);
}

/* ── Demo cards ── */
.home__demo {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.home__demo-thumb {
  height: 96px;
  margin: 0;
  background:
    radial-gradient(ellipse 70% 100% at 50% 0%, rgba(168, 85, 247, 0.08), transparent 70%),
    #0d0c14;
  border-bottom: 1px solid var(--cos-border);
  display: flex;
  align-items: center;
  justify-content: center;
}

.home__thumb-bars { display: flex; align-items: flex-end; gap: 4px; height: 48px; }

.home__thumb-bar {
  width: 14px;
  border-radius: var(--radius-sm) var(--radius-sm) 2px 2px;
  background: #181225;
  border: 1px solid rgba(168, 85, 247, 0.4);
}

.home__thumb-bar:nth-child(1) { height: 40%; }
.home__thumb-bar:nth-child(2) { height: 65%; }
.home__thumb-bar:nth-child(3) { height: 50%; }
.home__thumb-bar:nth-child(4) { height: 80%; }
.home__thumb-bar:nth-child(5) { height: 100%; }

.home__thumb-bar--done { background: var(--cos-purple); border-color: var(--cos-purple); }

.home__thumb-row { display: flex; align-items: center; gap: 4px; }

.home__thumb-block {
  width: 18px;
  height: 24px;
  border-radius: var(--radius-sm);
  background: #181225;
  border: 1px solid rgba(168, 85, 247, 0.4);
}

.home__thumb-block--found {
  background: var(--cos-purple);
  border-color: var(--cos-purple);
  box-shadow: 0 0 12px rgba(168, 85, 247, 0.45);
}

.home__thumb-graph { display: flex; align-items: center; }

.home__thumb-node {
  width: 14px;
  height: 14px;
  border-radius: var(--radius-full);
  background: #181225;
  border: 1px solid rgba(168, 85, 247, 0.5);
}

.home__thumb-node--visited { background: var(--cos-purple); border-color: var(--cos-purple); }

.home__thumb-edge {
  width: 18px;
  height: 2px;
  border-radius: 1px;
  background: rgba(168, 85, 247, 0.3);
}

.home__demo-body {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: var(--space-lg);
  flex: 1;
}

.home__demo-title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin: 0;
  font-size: var(--text-md);
  font-weight: 700;
  color: #fff;
}

.home__demo-title-icon { color: var(--cos-purple-light); }

.home__demo-meta {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--cos-text-3);
}

.home__demo-complexity {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  border-top: 1px solid var(--cos-border);
  padding-top: var(--space-sm);
  margin: 4px 0 0;
}

.home__demo-complexity dt { color: var(--cos-text-3); }
.home__demo-complexity dd { font-family: var(--font-mono); color: var(--cos-purple-light); }

.home__demo-run {
  margin-top: auto;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0.6rem 1rem;
  border-radius: var(--radius-lg);
  background: linear-gradient(135deg, var(--cos-purple), var(--cos-purple-dark));
  border: none;
  color: #fff;
  font-size: var(--text-sm);
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 0 16px rgba(168, 85, 247, 0.25);
  transition: box-shadow 200ms ease, transform 200ms ease;
}

.home__demo-run:hover {
  box-shadow: 0 0 28px rgba(168, 85, 247, 0.45);
  transform: translateY(-1px);
}

/* ── Features ── */
.home__feature {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: var(--space-xl);
}

.home__feature--visual { grid-column: span 8; }
.home__feature--path { grid-column: span 4; }
.home__feature--compact { grid-column: 1 / -1; flex-direction: row; align-items: center; gap: var(--space-lg); }

.home__feature-icon {
  width: 46px;
  height: 46px;
  border-radius: var(--radius-lg);
  background: var(--cos-purple-dim);
  border: 1px solid rgba(168, 85, 247, 0.3);
  color: var(--cos-purple-light);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.home__feature-title {
  margin: 0;
  font-size: var(--text-md);
  font-weight: 700;
  color: #fff;
}

.home__feature-desc {
  margin: 0;
  font-size: var(--text-sm);
  color: var(--cos-text-2);
  line-height: 1.6;
}

/* ── CTA band ── */
.home__cta-band {
  position: relative;
  z-index: 1;
  margin: var(--space-sm) 0 0;
  padding: var(--space-2xl) var(--space-lg);
  text-align: center;
  overflow: hidden;
  border-block: 1px solid var(--cos-border);
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

.home__cta-band-inner {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
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
  color: var(--cos-text-2);
  font-size: var(--text-sm);
}

/* ── Responsive ── */
@media (max-width: 900px) {
  .home__stats { grid-template-columns: 1fr 1fr; }
  .home__feature--visual,
  .home__feature--path { grid-column: 1 / -1; }
  .home__feature--compact { flex-direction: column; align-items: flex-start; }
  .home__orbit { display: none; }
}

@media (max-width: 640px) {
  .home { gap: var(--space-xl); padding-bottom: var(--space-2xl); }
  .home__stats { grid-template-columns: 1fr; }
  .home__title { font-size: var(--text-3xl); }
  .home__cta-band-title { font-size: var(--text-2xl); }
  .home__glyph { display: none; }
}

/* Tôn trọng prefers-reduced-motion — chặn mọi animation vũ trụ */
@media (prefers-reduced-motion: reduce) {
  .home__nebula,
  .home__orbit-ring,
  .home__orbit-dot,
  .home__cta-band-glow {
    animation: none !important;
  }
}
</style>
