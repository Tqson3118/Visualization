<script setup lang="ts">
// SimulationsView — Màn 33 "Khám phá": 3 tab (Danh mục / So sánh / CheatSheet).
// View-quality (nhóm A): chrome = surface band level-2 + strip block-token/index mono; stat
// Geist 600 text-2xl + label tertiary; BenchmarkPanel/CheatSheetTable → defineAsyncComponent.
// Catalog redesign (3.3): card gom nhóm theo prefix key (sort./search./stack./queue./list./
// tree./heap./hash./graph./structure. — CATALOG chỉ có category algorithm|structure, không đủ
// chi tiết để phân nhóm visual); mỗi nhóm heading + separator, nhóm rỗng sau filter bị ẩn;
// chip Big-O mono màu theo tốc độ (n log n → success, n² → warning, n³+ → danger); hover card
// = scale nhẹ 1.01 + shadow token (tôn trọng prefers-reduced-motion).
// GIỮ NGUYÊN logic lọc/phân trang + aria-label (selector/e2e hook) + tab key + link "Đọc thêm".
import { computed, defineAsyncComponent, ref, watch } from 'vue';
import type { Component } from 'vue';
import { RouterLink, useRouter } from 'vue-router';

import {
  ArrowRight,
  ArrowUpDown,
  BookOpen,
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
import { getReference } from '@/data/referenceLinks';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Tabs, { type TabItem } from '@/components/ui/Tabs.vue';
import Card from '@/components/ui/Card.vue';
import { messages } from '@/i18n/vi';
import { normalizeVi } from '@/utils/searchNormalize';

// Tab content nặng → lazy-load theo tab (trục 10: route-level splitting)
const CheatSheetTable = defineAsyncComponent(() => import('@/components/lesson/CheatSheetTable.vue'));

const router = useRouter();

type ExploreTab = 'catalog' | 'cheatsheet';
const tab = ref<ExploreTab>('catalog');

const TAB_ITEMS: TabItem[] = [
  { key: 'catalog', label: messages.explore.tabCatalog },
  { key: 'cheatsheet', label: messages.explore.tabCheatsheet },
];

function onTabChange(key: string): void {
  tab.value = key as ExploreTab;
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

// ── Lọc danh mục (giữ nguyên logic cũ) ──
const search = ref('');
const structureFilter = ref('');
const levelFilter = ref('');
const tagFilter = ref('');
const page = ref(1);
const PAGE_SIZE = 12;

watch(page, () => {
  window.scrollTo({ top: 0, behavior: 'smooth' });
});

watch([search, structureFilter, levelFilter, tagFilter], () => {
  page.value = 1;
});

const structures = computed(() => ['', ...new Set(CATALOG.map((item) => item.dataStructure))]);
const tags = computed(() => ['', ...new Set(CATALOG.flatMap((item) => item.tags))]);

const filtered = computed(() => {
  let list = CATALOG;
  const q = normalizeVi(search.value);
  if (q) {
    list = list.filter((i) => {
      const normKey = normalizeVi(i.key);
      const normTitle = normalizeVi(i.title);
      const normDataStructure = normalizeVi(i.dataStructure);
      const normTags = (i.tags || []).map((t) => normalizeVi(t)).join(' ');
      return (
        normKey.includes(q) ||
        normTitle.includes(q) ||
        normDataStructure.includes(q) ||
        normTags.includes(q)
      );
    });
  }
  if (structureFilter.value) list = list.filter((i) => i.dataStructure === structureFilter.value);
  if (levelFilter.value) list = list.filter((i) => i.level === levelFilter.value);
  if (tagFilter.value) list = list.filter((i) => i.tags.includes(tagFilter.value));
  return list;
});

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PAGE_SIZE)));
const paged = computed(() => {
  const start = (page.value - 1) * PAGE_SIZE;
  return filtered.value.slice(start, start + PAGE_SIZE);
});

watch([search, structureFilter, levelFilter, tagFilter], () => {
  page.value = 1;
});

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

// ── Nhóm visual: CATALOG chỉ có category 'algorithm' | 'structure' → gom theo prefix key ──
type CatalogGroupKey =
  | 'sort'
  | 'search'
  | 'stack'
  | 'queue'
  | 'list'
  | 'tree'
  | 'heap'
  | 'hash'
  | 'graph'
  | 'structure';

interface CatalogGroup {
  key: CatalogGroupKey;
  label: string;
  icon: Component;
  items: CatalogMeta[];
}

const GROUP_DEFS: ReadonlyArray<{ key: CatalogGroupKey; label: string; icon: Component; prefix: string }> = [
  { key: 'sort', label: 'Sắp xếp (Sorting)', icon: ArrowUpDown, prefix: 'sort.' },
  { key: 'search', label: 'Tìm kiếm (Searching)', icon: Search, prefix: 'search.' },
  { key: 'stack', label: 'Ngăn xếp (Stack)', icon: SquareStack, prefix: 'stack.' },
  { key: 'queue', label: 'Hàng đợi (Queue)', icon: ListOrdered, prefix: 'queue.' },
  { key: 'list', label: 'Danh sách liên kết (Linked List)', icon: List, prefix: 'list.' },
  { key: 'tree', label: 'Cây (Tree)', icon: GitBranch, prefix: 'tree.' },
  { key: 'heap', label: 'Đống nhị phân (Heap)', icon: Layers, prefix: 'heap.' },
  { key: 'hash', label: 'Bảng băm (Hash Table)', icon: Hash, prefix: 'hash.' },
  { key: 'graph', label: 'Đồ thị (Graph)', icon: Network, prefix: 'graph.' },
  { key: 'structure', label: 'Cấu trúc dữ liệu (Data Structures)', icon: Rows3, prefix: 'structure.' },
];

function groupKeyFor(item: CatalogMeta): CatalogGroupKey {
  return GROUP_DEFS.find((g) => item.key.startsWith(g.prefix))?.key ?? 'structure';
}

/** Nhóm danh sách đã lọc — các nhóm luôn toàn vẹn, không bị phân mảnh cắt ngang */
const allGrouped = computed<CatalogGroup[]>(() => {
  const buckets = new Map<CatalogGroupKey, CatalogMeta[]>();
  for (const item of filtered.value) {
    const key = groupKeyFor(item);
    const list = buckets.get(key);
    if (list) list.push(item);
    else buckets.set(key, [item]);
  }
  return GROUP_DEFS.filter((g) => buckets.has(g.key)).map((g) => ({
    key: g.key,
    label: g.label,
    icon: g.icon,
    items: buckets.get(g.key) ?? [],
  }));
});

const grouped = computed<CatalogGroup[]>(() => allGrouped.value);

// ── Chip Big-O màu theo tốc độ (màu khớp giá trị average đang hiển thị) ──
type ComplexityTone = 'success' | 'warning' | 'danger';

function complexityTone(value: string): ComplexityTone {
  const v = value.toLowerCase();
  if (v.includes('³') || v.includes('n^3') || v.includes('2^n')) return 'danger';
  if (v.includes('²') || v.includes('n^2')) return 'warning';
  return 'success';
}

function clearFilters(): void {
  search.value = '';
  structureFilter.value = '';
  levelFilter.value = '';
  tagFilter.value = '';
  page.value = 1;
}

/** Link "Đọc thêm" cho 1 key (wikipedia + geeksforgeeks từ REFERENCE_LINKS object) — không có thì trả mảng rỗng. */
function linksFor(key: string): { label: string; url: string }[] {
  const ref = getReference(key);
  if (!ref) return [];
  const links: { label: string; url: string }[] = [];
  if (ref.wikipedia) links.push({ label: 'Wikipedia', url: ref.wikipedia });
  if (ref.geeksforgeeks) links.push({ label: 'GeeksforGeeks', url: ref.geeksforgeeks });
  return links;
}

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

/** URL tài liệu cho một mô phỏng — ưu tiên Wikipedia, fallback GeeksforGeeks (undefined → ẩn link). */
function referenceUrl(key: string): string | undefined {
  const ref = getReference(key);
  return ref?.wikipedia ?? ref?.geeksforgeeks;
}
</script>

<template>
  <section class="simulations container">
    <!-- Chrome header — surface band level-2 (bỏ gradient mint + shadow, §1/§6) -->
    <div class="simulations__chrome">
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
    </div>

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
          />
          <select
            v-model="structureFilter"
            class="input simulations__select"
            :aria-label="messages.explore.structureAria"
          >
            <option value="">{{ messages.explore.structureAll }}</option>
            <option v-for="s in structures.slice(1)" :key="s" :value="s">{{ s }}</option>
          </select>
          <select
            v-model="levelFilter"
            class="input simulations__select"
            :aria-label="messages.explore.levelAria"
          >
            <option value="">{{ messages.explore.levelAll }}</option>
            <option value="basic">{{ messages.explore.levelBasic }}</option>
            <option value="advanced">{{ messages.explore.levelAdvanced }}</option>
          </select>
          <select
            v-model="tagFilter"
            class="input simulations__select"
            :aria-label="messages.explore.tagAria"
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

        <div v-else class="simulations__groups">
          <section
            v-for="group in grouped"
            :key="group.key"
            class="simulations__group"
            :aria-label="group.label"
          >
            <h2 class="simulations__group-head">
              <span class="simulations__group-icon" aria-hidden="true">
                <component :is="group.icon" :size="16" />
              </span>
              <span class="simulations__group-title">{{ group.label }}</span>
              <span class="simulations__group-count">
                {{ group.items.length }} {{ messages.explore.statSimulations }}
              </span>
            </h2>
            <div class="simulations__grid">
              <Card
                v-for="item in group.items"
                :key="item.key"
                :padded="false"
                class="simulations__card shadow-none"
                role="button"
                tabindex="0"
                :aria-label="messages.explore.openSimulation(item.title)"
                @click="openSimulation(item.key)"
                @keydown.enter="openSimulation(item.key)"
                @keydown.space.prevent="openSimulation(item.key)"
              >
              <div class="flex flex-col gap-y-1.5 p-6">
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
                <h3 class="text-2xl font-semibold leading-none tracking-tight simulations__card-title">{{ item.title }}</h3>
                <p class="text-sm text-muted-foreground simulations__card-key">{{ item.key }}</p>
              </div>
              <div class="p-6 pt-0 simulations__card-content">
                <div class="simulations__card-row">
                  <dl class="simulations__complexity">
                    <dt>{{ messages.explore.complexityLabel }}</dt>
                    <dd class="simulations__complexity-value">
                      <Badge
                        :variant="complexityTone(item.complexity.average)"
                        class="simulations__complexity-badge"
                      >
                        {{ item.complexity.average }}
                      </Badge>
                      <span class="simulations__complexity-sep" aria-hidden="true">·</span>
                      <span class="simulations__complexity-space">{{ item.complexity.space }}</span>
                    </dd>
                  </dl>
                  <span class="simulations__open">
                    {{ messages.explore.open }}
                    <ArrowRight :size="14" aria-hidden="true" />
                  </span>
                </div>
                <div v-if="linksFor(item.key).length > 0" class="simulations__links">
                  <span class="simulations__links-label">📖 Đọc thêm:</span>
                  <a
                    v-for="link in linksFor(item.key)"
                    :key="link.url"
                    :href="link.url"
                    target="_blank"
                    rel="noopener noreferrer"
                    class="simulations__link"
                    @click.stop
                  >
                    {{ link.label }}
                  </a>
                </div>
              </div>
            </Card>
            </div>
          </section>
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

      <!-- Tab 2: CheatSheet -->
      <section v-else-if="tab === 'cheatsheet'">
        <CheatSheetTable @open-simulation="openSimulation" />
      </section>
    </Tabs>
  </section>
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

/* ── Nhóm visual: heading + separator mỏng, nhóm rỗng bị v-if loại ── */
.simulations__groups {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl);
}

.simulations__group {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  border-top: 1px solid var(--color-border-subtle);
  padding-top: var(--space-lg);
}

.simulations__group-head {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--space-sm);
  margin: 0;
  font-size: var(--text-xl);
  font-weight: 600;
  line-height: 1.25;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
}

.simulations__group-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-primary);
  flex-shrink: 0;
}

.simulations__group-count {
  margin-left: auto;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 400;
  color: var(--color-text-tertiary);
  font-variant-numeric: tabular-nums;
}

.simulations__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: var(--space-md);
}

/* Card clickable — hover: border mạnh + scale nhẹ 1.01 + shadow token (180ms, không giật layout) */
.simulations__card {
  display: flex;
  flex-direction: column;
  cursor: pointer;
  transition:
    border-color 180ms cubic-bezier(0.16, 1, 0.3, 1),
    transform 180ms cubic-bezier(0.16, 1, 0.3, 1),
    box-shadow 180ms cubic-bezier(0.16, 1, 0.3, 1);
}

.simulations__card:hover {
  border-color: var(--color-border-strong);
  transform: scale(1.01);
  box-shadow: var(--shadow-md);
}

.simulations__card:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
}

@media (prefers-reduced-motion: reduce) {
  .simulations__card { transition: none; }
  .simulations__card:hover { transform: none; }
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
  flex-direction: column;
  gap: var(--space-sm);
  padding-top: var(--space-md);
}

.simulations__card-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
}

/* "Đọc thêm" — chip link nhỏ, chỉ hiện khi key có REFERENCE_LINKS (ẩn khi không) */
.simulations__links {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: var(--space-xs);
  border-top: 1px solid var(--color-border-subtle);
  padding-top: var(--space-sm);
}

.simulations__links-label {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.simulations__link {
  display: inline-flex;
  align-items: center;
  padding: var(--space-xs) var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: var(--text-xs);
  font-weight: 500;
  line-height: 1.4;
  color: var(--color-primary);
  text-decoration: none;
  transition:
    border-color 150ms cubic-bezier(0.16, 1, 0.3, 1),
    background-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.simulations__link:hover {
  border-color: var(--color-border-strong);
  background: var(--color-muted);
}

.simulations__link:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
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

.simulations__complexity dt { font-weight: 500; color: var(--color-foreground); white-space: nowrap; }
.simulations__complexity dd { font-family: var(--font-mono); }

/* Chip Big-O màu theo tốc độ (complexityTone) + space mono giữ nguyên thông tin cũ */
.simulations__complexity-value {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-xs);
  line-height: 1.4;
}

.simulations__complexity-badge { font-family: var(--font-mono); font-size: var(--text-xs); }

.simulations__complexity-sep { color: var(--color-text-quaternary); }

.simulations__complexity-space { color: var(--color-text-tertiary); }

.simulations__open {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--color-primary);
  font-weight: 500;
  font-size: var(--text-xs);
  white-space: nowrap;
}

/* Nhóm link phải của thẻ: 'Đọc tài liệu' + 'Mở mô phỏng' */
.simulations__card-links {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
  justify-content: flex-end;
}

.simulations__doc-link {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: var(--space-xs) var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  color: var(--color-primary);
  font-weight: 500;
  font-size: var(--text-xs);
  white-space: nowrap;
  text-decoration: none;
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1), background-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.simulations__doc-link:hover {
  border-color: var(--color-primary);
  background: var(--color-surface-hover);
}

.simulations__doc-link:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
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
