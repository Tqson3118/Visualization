<script setup lang="ts">
// SimulationsView — Màn 33 "Khám phá": 3 tab (Danh mục / So sánh / CheatSheet)
// H-E2: chrome hero Cyber Mint (đồng bộ BenchmarkView) + shadcn Tabs/Card + icon mapping.
// GIỮ NGUYÊN logic lọc/phân trang + aria-label (selector/e2e hook).
import { computed, ref } from 'vue';
import type { Component } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
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
import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
import BenchmarkPanel from '@/components/benchmark/BenchmarkPanel.vue';
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

// Số liệu hero (H-E2) — nguồn CATALOG như HomeView stats
const stats = computed(() => ({
  simulations: CATALOG.length,
  groups: new Set(CATALOG.map((c) => c.dataStructure)).size,
  levels: new Set(CATALOG.map((c) => c.level)).size,
}));

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
    <!-- Chrome header — Cyber Mint (palette 3, đồng bộ BenchmarkView) -->
    <header class="simulations__chrome">
      <nav class="simulations__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'home' }">{{ messages.explore.breadcrumbHome }}</RouterLink>
        <span aria-hidden="true">/</span>
        <span>{{ messages.explore.title }}</span>
      </nav>
      <div class="simulations__hero">
        <span class="simulations__icon" aria-hidden="true">
          <FlaskConical :size="22" />
        </span>
        <div>
          <h1 class="simulations__title">{{ messages.explore.title }}</h1>
          <p class="simulations__sub">{{ messages.explore.sub(CATALOG.length) }}</p>
        </div>
        <Badge variant="success" class="simulations__badge">{{ messages.explore.demoBadge }}</Badge>
      </div>
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
    </header>

    <!-- Tabs shadcn (giữ key catalog/compare/cheatsheet) -->
    <Tabs :tabs="TAB_ITEMS" :model-value="tab" @change="onTabChange">
      <!-- Tab 1: Danh mục -->
      <section v-if="tab === 'catalog'" class="simulations__catalog">
        <div class="simulations__filters card">
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
            class="hover-lift simulations__card"
            role="button"
            tabindex="0"
            :aria-label="messages.explore.openSimulation(item.title)"
            @click="openSimulation(item.key)"
            @keydown.enter="openSimulation(item.key)"
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

/* ── Chrome header — Cyber Mint (đồng bộ BenchmarkView) ── */
.simulations__chrome {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 28%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-mint);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.simulations__chrome::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  /* H-E2: 68% (thay 62% của BenchmarkView) để text-muted trên chrome ≥ 4.5:1 cả 2 theme */
  background: color-mix(in srgb, var(--color-background) 68%, transparent);
}

.simulations__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.simulations__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.simulations__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.simulations__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.simulations__title {
  font-size: clamp(var(--text-2xl), 4vw, var(--text-3xl));
  background-image: var(--gradient-mint);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.simulations__sub {
  font-size: var(--text-sm);
  /* H-E2: foreground 92% — text-muted chỉ 3.47:1 trên chrome light (fail AA) */
  color: color-mix(in srgb, var(--color-foreground) 92%, transparent);
  max-width: 64ch;
  margin-top: 2px;
}

.simulations__badge { margin-left: auto; }

.simulations__stats {
  display: flex;
  gap: var(--space-md);
  flex-wrap: wrap;
  border-top: 1px solid color-mix(in srgb, var(--color-border) 55%, transparent);
  padding-top: var(--space-md);
}

.simulations__stat {
  display: flex;
  align-items: baseline;
  gap: 6px;
  font-size: var(--text-xs);
}

.simulations__stat dt {
  color: color-mix(in srgb, var(--color-foreground) 92%, transparent);
  font-weight: 600;
  order: 2;
}
.simulations__stat dd {
  font-size: var(--text-lg);
  font-weight: 800;
  background-image: var(--gradient-mint);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
  font-variant-numeric: tabular-nums;
}

/* ── Danh mục ── */
.simulations__catalog { display: flex; flex-direction: column; gap: var(--space-lg); }

.simulations__filters {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
  padding: var(--space-md);
}

.simulations__search { flex: 1; min-width: 200px; }
.simulations__select { width: auto; min-width: 160px; }

.simulations__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: var(--space-md);
}

.simulations__card {
  display: flex;
  flex-direction: column;
  cursor: pointer;
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

.simulations__card-badges { display: flex; gap: 6px; flex-wrap: wrap; }

.simulations__card-title { font-size: var(--text-md); line-height: 1.3; }

.simulations__card-key {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  margin-top: 4px;
  /* H-E2: --muted-foreground shadcn dark chỉ 3.34:1 trên --card → dùng legacy token 5.3+ */
  color: var(--color-text-muted);
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
  gap: 2px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

.simulations__complexity dt { font-weight: 700; color: var(--color-foreground); }
.simulations__complexity dd { font-family: var(--font-mono); }

.simulations__open {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  color: var(--color-primary);
  font-weight: 700;
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
  color: var(--color-text-muted);
  font-variant-numeric: tabular-nums;
}

@media (max-width: 640px) {
  .simulations__chrome { padding: var(--space-md); }
  .simulations__badge { margin-left: 0; }
  .simulations__hero { align-items: flex-start; }
}
</style>
