<script setup lang="ts">
// SimulationsView — Màn 33 "Khám phá": 3 tab (Danh mục / So sánh / CheatSheet).
// View-quality (nhóm A): bỏ chrome gradient mint + shadow → surface band level-2; thêm strip
// block-token + index mono trong banner (dữ liệu tuần tự → quyết định 4); stat bỏ gradient/800
// → Geist 600 text-2xl + label tertiary; card bỏ hover-lift (shadow+ease mặc định) → hover đổi
// border + Space key; BenchmarkPanel/CheatSheetTable → defineAsyncComponent (lazy theo tab).
// GIỮ NGUYÊN logic lọc/phân trang + aria-label (selector/e2e hook) + tab key.
import { computed, defineAsyncComponent, ref } from 'vue';
import type { Component } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { Motion } from 'motion-v';
import {
  ArrowRight,
  ArrowUpDown,
  GitBranch,
  Hash,
  Layers,
  List,
  ListOrdered,
  Network,
  Play,
  Rows3,
  Search,
  SquareStack,
  FlaskConical,
} from 'lucide-vue-next';

import { CATALOG, type CatalogMeta } from '@/engines/catalog';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Tabs, { type TabItem } from '@/components/ui/Tabs.vue';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { messages } from '@/i18n/vi';

// Tab content nặng → lazy-load theo tab (trục 10: route-level splitting)
const BenchmarkPanel = defineAsyncComponent(() => import('@/components/benchmark/BenchmarkPanel.vue'));
const CheatSheetTable = defineAsyncComponent(() => import('@/components/lesson/CheatSheetTable.vue'));

const router = useRouter();

type ExploreTab = 'catalog' | 'compare' | 'cheatsheet';
const tab = ref<ExploreTab>('catalog');

const TAB_ITEMS: TabItem[] = [
  { key: 'catalog', label: messages.explore.tabCatalog },
  { key: 'compare', label: messages.explore.tabCompare },
  { key: 'cheatsheet', label: messages.explore.tabCheatsheet },
];

function onTabChange(key: string): void {
  tab.value = key as ExploreTab;
}

// ── Lọc danh mục (giữ nguyên logic cũ) ──
const search = ref('');
const structureFilter = ref('');
const levelFilter = ref('');
const tagFilter = ref('');
const page = ref(1);
const PAGE_SIZE = 12;

const structures = computed(() => ['', ...new Set(CATALOG.map((item) => item.dataStructure))]);
const tags = computed(() => ['', ...new Set(CATALOG.flatMap((item) => item.tags))]);

const filtered = computed(() => {
  let list = CATALOG;
  const q = search.value.trim().toLowerCase();
  if (q) list = list.filter((i) => i.key.toLowerCase().includes(q) || i.title.toLowerCase().includes(q));
  if (structureFilter.value) list = list.filter((i) => i.dataStructure === structureFilter.value);
  if (levelFilter.value) list = list.filter((i) => i.level === levelFilter.value);
  if (tagFilter.value) list = list.filter((i) => i.tags.includes(tagFilter.value));
  return list;
});

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PAGE_SIZE)));
const paged = computed(() => filtered.value.slice((page.value - 1) * PAGE_SIZE, page.value * PAGE_SIZE));

// Số liệu — nguồn CATALOG (stat phụ level-1, §6: tối đa 1 hero/màn — strip block là hero motif)
const stats = computed(() => ({
  simulations: CATALOG.length,
  groups: new Set(CATALOG.map((c) => c.dataStructure)).size,
  levels: new Set(CATALOG.map((c) => c.level)).size,
}));

/** Strip block-token trang trí (aria-hidden) — "tìm kiếm 01 tại index 02" ngôn ngữ dữ liệu. */
const BENCH_BLOCKS = [
  { value: '3', found: false },
  { value: '7', found: false },
  { value: '1', found: true },
  { value: '8', found: false },
  { value: '4', found: false },
] as const;

/** Icon theo loại mô phỏng (algorithm → prefix key; structure → full key). */
const STRUCTURE_ICONS: Record<string, Component> = {
  'structure.array': Rows3,
  'structure.linkedlist': List,
  'structure.stack': SquareStack,
  'structure.queue': ListOrdered,
  'structure.binarytree': GitBranch,
  'structure.bst': GitBranch,
  'structure.avl': GitBranch,
  'structure.heap': Layers,
  'structure.hashtable': Hash,
  'structure.graph': Network,
};

const ALGO_PREFIX_ICONS: Array<[string, Component]> = [
  ['sort.', ArrowUpDown],
  ['search.', Search],
  ['stack.', SquareStack],
  ['queue.', ListOrdered],
  ['list.', List],
  ['tree.', GitBranch],
  ['heap.', Layers],
  ['hash.', Hash],
  ['graph.', Network],
];

function iconFor(item: CatalogMeta): Component {
  return (
    STRUCTURE_ICONS[item.key] ??
    ALGO_PREFIX_ICONS.find(([prefix]) => item.key.startsWith(prefix))?.[1] ??
    Play
  );
}

function clearFilters(): void {
  search.value = '';
  structureFilter.value = '';
  levelFilter.value = '';
  tagFilter.value = '';
  page.value = 1;
}

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}
</script>

<template>
  <main class="simulations container">
    <!-- Chrome header — surface band level-2 (bỏ gradient mint + shadow, §1/§6) -->
    <Motion
      class="simulations__chrome"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <nav class="simulations__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'home' }">{{ messages.explore.breadcrumbHome }}</RouterLink>
        <span aria-hidden="true">/</span>
        <span aria-current="page">{{ messages.explore.title }}</span>
      </nav>
      <div class="simulations__hero">
        <span class="simulations__icon" aria-hidden="true">
          <FlaskConical :size="20" />
        </span>
        <div>
          <h1 class="simulations__title">{{ messages.explore.title }}</h1>
          <p class="simulations__sub">{{ messages.explore.sub(CATALOG.length) }}</p>
        </div>
        <Badge variant="success" class="simulations__badge">{{ messages.explore.demoBadge }}</Badge>
      </div>

      <div class="simulations__meta">
        <dl class="simulations__stats" aria-label="Thống kê">
          <div class="simulations__stat">
            <dt>{{ messages.explore.statSimulations }}</dt>
            <dd>{{ stats.simulations }}</dd>
          </div>
          <div class="simulations__stat">
            <dt>{{ messages.explore.statGroups }}</dt>
            <dd>{{ stats.groups }}</dd>
          </div>
          <div class="simulations__stat">
            <dt>{{ messages.explore.statLevels }}</dt>
            <dd>{{ stats.levels }}</dd>
          </div>
        </dl>

        <!-- Strip block-token + index mono — dấu vân tay Data Bench (decorative) -->
        <div class="simulations__bench" aria-hidden="true">
          <p class="simulations__bench-label">INDEX 00–04 · CATALOG {{ CATALOG.length }}</p>
          <div class="simulations__bench-blocks">
            <div
              v-for="(b, idx) in BENCH_BLOCKS"
              :key="idx"
              class="simulations__bench-block"
              :class="{ 'simulations__bench-block--found': b.found }"
            >
              <span class="simulations__bench-value">{{ b.value }}</span>
              <span class="simulations__bench-index">{{ String(idx).padStart(2, '0') }}</span>
            </div>
          </div>
        </div>
      </div>
    </Motion>

    <!-- Tabs shadcn (giữ key catalog/compare/cheatsheet) -->
    <Tabs :tabs="TAB_ITEMS" :model-value="tab" @change="onTabChange">
      <!-- Tab 1: Danh mục -->
      <section v-if="tab === 'catalog'" class="simulations__catalog">
        <div class="simulations__filters">
          <input
            v-model="search"
            class="input simulations__search"
            type="search"
            :placeholder="messages.explore.searchPlaceholder"
            :aria-label="messages.explore.searchAria"
            @input="page = 1"
          />
          <select
            v-model="structureFilter"
            class="input simulations__select"
            :aria-label="messages.explore.structureAria"
            @change="page = 1"
          >
            <option value="">{{ messages.explore.structureAll }}</option>
            <option v-for="s in structures.slice(1)" :key="s" :value="s">{{ s }}</option>
          </select>
          <select
            v-model="levelFilter"
            class="input simulations__select"
            :aria-label="messages.explore.levelAria"
            @change="page = 1"
          >
            <option value="">{{ messages.explore.levelAll }}</option>
            <option value="basic">{{ messages.explore.levelBasic }}</option>
            <option value="advanced">{{ messages.explore.levelAdvanced }}</option>
          </select>
          <select
            v-model="tagFilter"
            class="input simulations__select"
            :aria-label="messages.explore.tagAria"
            @change="page = 1"
          >
            <option value="">{{ messages.explore.tagAll }}</option>
            <option v-for="t in tags.slice(1)" :key="t" :value="t">{{ t }}</option>
          </select>
          <Button
            v-if="search || structureFilter || levelFilter || tagFilter"
            variant="ghost"
            size="sm"
            @click="clearFilters"
          >
            {{ messages.explore.clearFilters }}
          </Button>
        </div>

        <EmptyState
          v-if="filtered.length === 0"
          icon="search"
          :title="messages.explore.emptyTitle"
          :description="messages.explore.emptyDesc"
          :action-label="messages.explore.clearFilters"
          @action="clearFilters"
        />

        <div v-else class="simulations__grid">
          <Card
            v-for="item in paged"
            :key="item.key"
            class="simulations__card shadow-none"
            role="button"
            tabindex="0"
            :aria-label="messages.explore.openSimulation(item.title)"
            @click="openSimulation(item.key)"
            @keydown.enter="openSimulation(item.key)"
            @keydown.space.prevent="openSimulation(item.key)"
          >
            <CardHeader>
              <div class="simulations__card-head">
                <span class="simulations__card-icon" aria-hidden="true">
                  <component :is="iconFor(item)" :size="20" />
                </span>
                <span class="simulations__card-badges">
                  <Badge :variant="item.demoAllowed ? 'success' : 'muted'">
                    {{
                      item.demoAllowed
                        ? messages.explore.badgeDemo
                        : item.category === 'algorithm'
                          ? messages.explore.badgeAlgorithm
                          : messages.explore.badgeStructure
                    }}
                  </Badge>
                  <Badge :variant="item.level === 'basic' ? 'primary' : 'warning'">
                    {{
                      item.level === 'basic'
                        ? messages.explore.levelBasic
                        : messages.explore.levelAdvanced
                    }}
                  </Badge>
                </span>
              </div>
              <CardTitle class="simulations__card-title">{{ item.title }}</CardTitle>
              <CardDescription class="simulations__card-key">{{ item.key }}</CardDescription>
            </CardHeader>
            <CardContent class="simulations__card-content">
              <dl class="simulations__complexity">
                <dt>{{ messages.explore.complexityLabel }}</dt>
                <dd>{{ item.complexity.average }} · {{ item.complexity.space }}</dd>
              </dl>
              <span class="simulations__open">
                {{ messages.explore.open }}
                <ArrowRight :size="14" aria-hidden="true" />
              </span>
            </CardContent>
          </Card>
        </div>

        <nav
          v-if="totalPages > 1"
          class="simulations__pagination"
          :aria-label="messages.explore.paginationAria"
        >
          <Button variant="ghost" size="sm" :disabled="page <= 1" @click="page -= 1">
            {{ messages.explore.prevPage }}
          </Button>
          <span class="simulations__page-info">{{ messages.explore.pageOf(page, totalPages) }}</span>
          <Button variant="ghost" size="sm" :disabled="page >= totalPages" @click="page += 1">
            {{ messages.explore.nextPage }}
          </Button>
        </nav>
      </section>

      <!-- Tab 2: So sánh -->
      <section v-else-if="tab === 'compare'">
        <BenchmarkPanel />
      </section>

      <!-- Tab 3: CheatSheet -->
      <section v-else>
        <CheatSheetTable @open-simulation="openSimulation" />
      </section>
    </Tabs>
  </main>
</template>

<style scoped>
.simulations {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Chrome header — surface band level-2 (§6): card-raised + border-subtle, KHÔNG shadow ── */
.simulations__chrome {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.simulations__breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.simulations__breadcrumb a {
  color: var(--color-primary);
  font-weight: 600;
  padding-block: var(--space-xs);
}

.simulations__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.simulations__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.simulations__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.simulations__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  max-width: 64ch;
  margin-top: var(--space-xs);
}

.simulations__badge { margin-left: auto; }

/* ── Meta row: stat phụ level-1 + strip block-token (hero motif duy nhất) ── */
.simulations__meta {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
  border-top: 1px solid var(--color-border-subtle);
  padding-top: var(--space-md);
}

.simulations__stats {
  display: flex;
  gap: var(--space-xl);
  flex-wrap: wrap;
  margin: 0;
  padding: 0;
}

.simulations__stat {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.simulations__stat dt {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  font-weight: 500;
}

.simulations__stat dd {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
}

.simulations__bench {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  gap: var(--space-xs);
}

.simulations__bench-label {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

.simulations__bench-blocks {
  display: flex;
  gap: var(--space-sm);
}

.simulations__bench-block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  width: 32px;
  padding: var(--space-xs) 0;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
}

.simulations__bench-block--found { background: var(--color-resolved); }

.simulations__bench-value {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.4;
}

.simulations__bench-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

/* ── Danh mục ── */
.simulations__catalog { display: flex; flex-direction: column; gap: var(--space-lg); }

.simulations__filters {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
  padding: var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.simulations__search { flex: 1; min-width: 200px; }
.simulations__select { width: auto; min-width: 160px; }

.simulations__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: var(--space-md);
}

/* Card clickable — hover chỉ đổi border (§4.2), không shadow/scale; focus-visible ring */
.simulations__card {
  display: flex;
  flex-direction: column;
  cursor: pointer;
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.simulations__card:hover { border-color: var(--color-border-strong); }

.simulations__card:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
}

.simulations__card-head {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-bottom: var(--space-sm);
}

.simulations__card-icon {
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.simulations__card-badges { display: flex; gap: var(--space-xs); flex-wrap: wrap; }

.simulations__card-title {
  font-size: var(--text-lg);
  line-height: 1.3;
  letter-spacing: -0.015em;
}

.simulations__card-key {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  margin-top: var(--space-xs);
  color: var(--color-text-tertiary);
}

.simulations__card-content {
  margin-top: auto;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  padding-top: var(--space-md);
}

.simulations__complexity {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  margin: 0;
  padding: 0;
}

.simulations__complexity dt { font-weight: 500; color: var(--color-foreground); }
.simulations__complexity dd { font-family: var(--font-mono); }

.simulations__open {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--color-primary);
  font-weight: 500;
  font-size: var(--text-xs);
  white-space: nowrap;
}

.simulations__pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
}

.simulations__page-info {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  font-variant-numeric: tabular-nums;
}

@media (max-width: 640px) {
  .simulations__chrome { padding: var(--space-md); }
  .simulations__badge { margin-left: 0; }
  .simulations__hero { align-items: flex-start; }
  .simulations__bench { align-items: flex-start; }
}
</style>
