<script setup lang="ts">
// SimulationsView — Màn 33 "Khám phá": 3 tab (Danh mục / So sánh / CheatSheet)
// Lưới 44 mô phỏng từ engines/catalog + lọc CTDL/tag/mức độ + tìm kiếm + phân trang.
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';

import { CATALOG } from '@/engines/catalog';
import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
import BenchmarkPanel from '@/components/benchmark/BenchmarkPanel.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Button from '@/components/ui/Button.vue';

const router = useRouter();

const tab = ref<'catalog' | 'compare' | 'cheatsheet'>('catalog');

// ── Lọc danh mục ──
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
    <header class="simulations__header">
      <div>
        <h1 class="simulations__title">🔬 Khám phá</h1>
        <p class="text-muted simulations__sub">
          {{ CATALOG.length }} mô phỏng — xem tự do kiểu VisuAlgo. Mở mô phỏng cụ thể trừ 1 tim (3 demo công khai miễn phí).
        </p>
      </div>
    </header>

    <div class="simulations__tabs">
      <button
        type="button"
        class="simulations__tab"
        :class="{ 'simulations__tab--active': tab === 'catalog' }"
        @click="tab = 'catalog'"
      >
        Danh mục
      </button>
      <button
        type="button"
        class="simulations__tab"
        :class="{ 'simulations__tab--active': tab === 'compare' }"
        @click="tab = 'compare'"
      >
        So sánh (Benchmark)
      </button>
      <button
        type="button"
        class="simulations__tab"
        :class="{ 'simulations__tab--active': tab === 'cheatsheet' }"
        @click="tab = 'cheatsheet'"
      >
        CheatSheet (Big-O)
      </button>
    </div>

    <!-- Tab 1: Danh mục -->
    <section v-if="tab === 'catalog'" class="simulations__catalog">
      <div class="simulations__filters card">
        <input v-model="search" class="input simulations__search" type="search" placeholder="Tìm theo tên hoặc key..." aria-label="Tìm kiếm mô phỏng" @input="page = 1" />
        <select v-model="structureFilter" class="input simulations__select" aria-label="Lọc theo cấu trúc" @change="page = 1">
          <option value="">CTDL: Tất cả</option>
          <option v-for="s in structures.slice(1)" :key="s" :value="s">{{ s }}</option>
        </select>
        <select v-model="levelFilter" class="input simulations__select" aria-label="Lọc theo mức độ" @change="page = 1">
          <option value="">Mức độ: Tất cả</option>
          <option value="basic">Cơ bản</option>
          <option value="advanced">Nâng cao</option>
        </select>
        <select v-model="tagFilter" class="input simulations__select" aria-label="Lọc theo tag" @change="page = 1">
          <option value="">Tag: Tất cả</option>
          <option v-for="t in tags.slice(1)" :key="t" :value="t">{{ t }}</option>
        </select>
        <Button v-if="search || structureFilter || levelFilter || tagFilter" variant="ghost" size="sm" @click="clearFilters">
          Xóa bộ lọc
        </Button>
      </div>

      <EmptyState
        v-if="filtered.length === 0"
        icon="search"
        title="Không có mô phỏng phù hợp"
        description="Thử xóa bộ lọc hoặc đổi từ khóa."
        action-label="Xóa bộ lọc"
        @action="clearFilters"
      />

      <div v-else class="simulations__grid">
        <article
          v-for="item in paged"
          :key="item.key"
          class="simulations__card card card--interactive"
          role="button"
          tabindex="0"
          :aria-label="`Mở mô phỏng ${item.title}`"
          @click="openSimulation(item.key)"
          @keydown.enter="openSimulation(item.key)"
        >
          <header class="simulations__card-head">
            <Badge :variant="item.demoAllowed ? 'success' : 'muted'">
              {{ item.demoAllowed ? 'Demo' : item.category === 'algorithm' ? 'Thuật toán' : 'CTDL' }}
            </Badge>
            <Badge :variant="item.level === 'basic' ? 'primary' : 'warning'">
              {{ item.level === 'basic' ? 'Cơ bản' : 'Nâng cao' }}
            </Badge>
          </header>
          <h3 class="simulations__card-title">{{ item.title }}</h3>
          <p class="simulations__card-key text-muted">{{ item.key }}</p>
          <footer class="simulations__card-foot">
            <span class="simulations__complexity">
              {{ item.complexity.average }} · {{ item.complexity.space }}
            </span>
            <span class="simulations__open">Mở →</span>
          </footer>
        </article>
      </div>

      <nav v-if="totalPages > 1" class="simulations__pagination" aria-label="Phân trang">
        <Button variant="ghost" size="sm" :disabled="page <= 1" @click="page -= 1">Trang trước</Button>
        <span class="simulations__page-info">{{ page }}/{{ totalPages }}</span>
        <Button variant="ghost" size="sm" :disabled="page >= totalPages" @click="page += 1">Trang sau</Button>
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
  </main>
</template>

<style scoped>
.simulations {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.simulations__title { font-size: var(--text-2xl); }
.simulations__sub { font-size: var(--text-sm); margin-top: 4px; }

.simulations__tabs {
  display: flex;
  gap: var(--space-xs);
  border-bottom: 2px solid var(--color-border);
  overflow-x: auto;
}

.simulations__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-md);
  font-weight: 700;
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  cursor: pointer;
  white-space: nowrap;
  margin-bottom: -2px;
}

.simulations__tab--active { color: var(--color-primary); border-bottom-color: var(--color-primary); }

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
  gap: var(--space-sm);
  cursor: pointer;
}

.simulations__card-head { display: flex; gap: 6px; }

.simulations__card-title { font-size: var(--text-md); }

.simulations__card-key { font-size: var(--text-xs); font-family: var(--font-mono); }

.simulations__card-foot {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

.simulations__open { color: var(--color-primary); font-weight: 700; }

.simulations__pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
}

.simulations__page-info { font-size: var(--text-sm); color: var(--color-text-muted); }
</style>
