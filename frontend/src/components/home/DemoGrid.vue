<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowRight, ArrowUpDown, Network, Play, Search } from 'lucide-vue-next';
import type { Component } from 'vue';
import { CATALOG } from '@/engines/catalog';
import { messages } from '@/i18n/vi';

const router = useRouter();

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

function openDemo(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}
</script>

<template>
  <section class="home__section container">
    <div class="home__section-head">
      <span class="home__kicker home__kicker--center">
        <span class="home__kicker-dot" aria-hidden="true" />
        <span class="font-mono">{{ messages.home.demoBadge }}</span>
      </span>
      <h2 class="home__section-title">{{ messages.home.demoTabTitle }}</h2>
      <p class="home__section-desc">{{ messages.home.demoTabDesc }}</p>
    </div>

    <div class="home__grid">
      <div v-for="demo in demos" :key="demo.key" class="home__demo glass-card">
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
</template>

<style scoped>
.home__section {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  gap: var(--space-lg, 24px);
}

.home__section-head {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm, 8px);
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

.home__kicker--center { justify-content: center; }

.home__kicker-dot {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-full);
  background: #a855f7;
  box-shadow: 0 0 10px rgba(168, 85, 247, 0.25);
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
  color: rgba(255, 255, 255, 0.62);
  font-size: var(--text-sm);
}

.home__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: var(--space-lg, 24px);
}

.glass-card {
  background: rgba(13, 12, 20, 0.72);
  -webkit-backdrop-filter: blur(16px) saturate(1.3);
  backdrop-filter: blur(16px) saturate(1.3);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: var(--radius-xl, 16px);
  box-shadow: 0 10px 34px rgba(0, 0, 0, 0.25), inset 0 1px 0 rgba(255, 255, 255, 0.03);
  transition: transform 260ms cubic-bezier(0.34, 1.56, 0.64, 1), border-color 200ms ease, box-shadow 260ms ease;
}

.glass-card:hover {
  transform: translateY(-4px);
  border-color: rgba(168, 85, 247, 0.4);
  box-shadow: 0 16px 44px rgba(0, 0, 0, 0.35), 0 0 24px rgba(168, 85, 247, 0.1);
}

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
  border-bottom: 1px solid rgba(255, 255, 255, 0.07);
  display: flex;
  align-items: center;
  justify-content: center;
}

.home__thumb-bars { display: flex; align-items: flex-end; gap: 4px; height: 48px; }

.home__thumb-bar {
  width: 14px;
  border-radius: var(--radius-sm, 4px) var(--radius-sm, 4px) 2px 2px;
  background: #181225;
  border: 1px solid rgba(168, 85, 247, 0.4);
}

.home__thumb-bar:nth-child(1) { height: 40%; }
.home__thumb-bar:nth-child(2) { height: 65%; }
.home__thumb-bar:nth-child(3) { height: 50%; }
.home__thumb-bar:nth-child(4) { height: 80%; }
.home__thumb-bar:nth-child(5) { height: 100%; }

.home__thumb-bar--done { background: #a855f7; border-color: #a855f7; }

.home__thumb-row { display: flex; align-items: center; gap: 4px; }

.home__thumb-block {
  width: 18px;
  height: 24px;
  border-radius: var(--radius-sm, 4px);
  background: #181225;
  border: 1px solid rgba(168, 85, 247, 0.4);
}

.home__thumb-block--found {
  background: #a855f7;
  border-color: #a855f7;
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

.home__thumb-node--visited { background: #a855f7; border-color: #a855f7; }

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
  padding: var(--space-lg, 24px);
  flex: 1;
}

.home__demo-title {
  display: flex;
  align-items: center;
  gap: var(--space-sm, 8px);
  margin: 0;
  font-size: var(--text-md);
  font-weight: 700;
  color: #fff;
}

.home__demo-title-icon { color: #c084fc; }

.home__demo-meta {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.38);
}

.home__demo-complexity {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: var(--space-sm, 8px);
  font-size: var(--text-xs);
  border-top: 1px solid rgba(255, 255, 255, 0.07);
  padding-top: var(--space-sm, 8px);
  margin: 4px 0 0;
}

.home__demo-complexity dt { color: rgba(255, 255, 255, 0.38); }
.home__demo-complexity dd { font-family: var(--font-mono); color: #c084fc; }

.home__demo-run {
  margin-top: auto;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0.6rem 1rem;
  border-radius: var(--radius-lg);
  background: linear-gradient(135deg, #a855f7, #7c3aed);
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
</style>
