<template>
  <div class="algo-library-view container mx-auto px-4 py-8 max-w-7xl animate-fade-in">
    <header class="mb-8 text-left flex flex-col md:flex-row md:items-end justify-between gap-4">
      <div class="flex-1">
        <h1 class="text-4xl font-extrabold text-text-primary m-0 tracking-tight">Thư viện thuật toán</h1>
        <p class="text-text-secondary mt-2 text-lg">
          Trực quan hóa từng bước — chọn nhóm thuật toán hoặc cấu trúc dữ liệu để bắt đầu.
        </p>
      </div>
      <div class="search-box relative w-full md:w-80">
        <svg class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-text-muted" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
          <circle cx="11" cy="11" r="8"></circle>
          <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
        </svg>
        <input
          v-model="searchQuery"
          type="text"
          placeholder="Tìm kiếm thuật toán..."
          class="w-full bg-bg-surface border border-border-default rounded-xl pl-10 pr-3 py-2.5 text-sm text-text-primary placeholder:text-text-muted focus:border-accent focus:outline-none transition-colors"
        />
      </div>
    </header>

    <div class="flex flex-col lg:flex-row gap-6">
      <!-- Sidebar filter -->
      <aside class="lg:w-60 shrink-0 space-y-6">
        <div class="glass-panel p-4 rounded-xl">
          <h3 class="text-xs font-bold uppercase tracking-wider text-text-muted mb-3">Nhóm</h3>
          <div class="space-y-1">
            <button
              v-for="group in groups"
              :key="group.value"
              @click="selectedGroup = group.value"
              class="w-full flex items-center justify-between px-3 py-2 rounded-lg text-sm font-medium transition-colors"
              :class="selectedGroup === group.value
                ? 'bg-accent/15 text-accent border border-accent/25'
                : 'text-text-secondary hover:bg-bg-hover border border-transparent'"
            >
              <span>{{ group.label }}</span>
              <span class="text-xs text-text-muted">{{ groupCount(group.value) }}</span>
            </button>
          </div>
        </div>

        <div class="glass-panel p-4 rounded-xl">
          <h3 class="text-xs font-bold uppercase tracking-wider text-text-muted mb-3">Độ khó</h3>
          <div class="space-y-1">
            <button
              v-for="d in difficultyOptions"
              :key="d.value"
              @click="selectedDifficulty = d.value"
              class="w-full flex items-center justify-between px-3 py-2 rounded-lg text-sm font-medium transition-colors"
              :class="selectedDifficulty === d.value
                ? 'bg-accent-green/15 text-accent-green border border-accent-green/25'
                : 'text-text-secondary hover:bg-bg-hover border border-transparent'"
            >
              <span>{{ d.label }}</span>
            </button>
          </div>
        </div>

        <button
          v-if="hasActiveFilter"
          @click="clearFilters"
          class="w-full px-3 py-2 rounded-lg text-sm font-semibold text-accent border border-border-accent hover:bg-accent/10 transition-colors"
        >
          Xóa bộ lọc
        </button>
      </aside>

      <!-- Grid -->
      <div class="flex-1">
        <div v-if="loading" class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
          <div v-for="i in 6" :key="i" class="card-skeleton rounded-xl h-48 bg-bg-surface border border-border-default animate-pulse"></div>
        </div>

        <div v-else-if="filteredAlgorithms.length === 0" class="empty-state text-center py-20 bg-bg-surface rounded-2xl border border-border-default">
          <BaseIcon name="search" class="w-16 h-16 text-text-muted mx-auto mb-4" />
          <h3 class="text-xl font-bold text-text-primary">Không tìm thấy thuật toán phù hợp</h3>
          <p class="text-text-secondary mt-2 mb-6">Vui lòng thay đổi bộ lọc hoặc từ khóa tìm kiếm.</p>
          <button @click="clearFilters" class="btn-primary px-6 py-2.5 rounded-xl">Xóa bộ lọc</button>
        </div>

        <div v-else class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-4">
          <router-link
            v-for="algo in filteredAlgorithms"
            :key="algo.id"
            :to="algoRoute(algo)"
            class="algo-card glass-panel spring-hover flex flex-col rounded-2xl p-5 cursor-pointer relative overflow-hidden"
          >
            <div class="flex items-start justify-between mb-3">
              <div class="algo-card-icon" :class="iconClass(algo.category)">
                <BaseIcon :name="iconName(algo.category)" class="w-5 h-5" />
              </div>
              <div class="flex gap-1.5">
                <span v-if="algo.difficulty === 'Hard'" class="px-2 py-0.5 rounded text-[10px] font-bold bg-accent-red/20 text-accent-red border border-accent-red/30">{{ difficultyLabel(algo.difficulty) }}</span>
                <span v-else-if="algo.difficulty === 'Medium'" class="px-2 py-0.5 rounded text-[10px] font-bold bg-accent-warm/20 text-accent-warm border border-accent-warm/30">{{ difficultyLabel(algo.difficulty) }}</span>
                <span v-else class="px-2 py-0.5 rounded text-[10px] font-bold bg-accent-green/20 text-accent-green border border-accent-green/30">{{ difficultyLabel(algo.difficulty) }}</span>
                <span v-if="isPremiumAlgo(algo)" class="px-2 py-0.5 rounded text-[10px] font-bold uppercase bg-accent-yellow text-black">Premium</span>
              </div>
            </div>

            <h3 class="text-base font-bold text-text-primary leading-snug mb-1">{{ algo.name }}</h3>
            <span class="text-[11px] font-semibold uppercase tracking-wider text-text-muted mb-3">{{ categoryLabel(algo.category) }}</span>

            <div class="flex items-center gap-3 text-[11px] text-text-secondary font-mono mb-4">
              <span class="flex items-center gap-1"><span class="text-accent">⏱</span>{{ algo.timeComplexity }}</span>
              <span class="flex items-center gap-1"><span class="text-accent-green">💾</span>{{ algo.spaceComplexity }}</span>
            </div>

            <div class="flex-1"></div>

            <div class="flex items-center justify-between border-t border-border-subtle pt-3">
              <span class="text-xs font-bold text-accent">▶ Xem ngay</span>
              <span v-if="isPremiumAlgo(algo)" class="text-xs text-text-muted flex items-center gap-1">
                <BaseIcon name="lock" class="w-3 h-3" /> Nâng cấp
              </span>
            </div>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import { ALGORITHM_CATALOG } from '@/features/dsa-modules/services/algorithmCatalog';
import { useAuthStore } from '@/features/auth/store/useAuthStore';

const authStore = useAuthStore();

const searchQuery = ref('');
const selectedGroup = ref('all');
const selectedDifficulty = ref('all');

const groups = [
  { label: 'Tất cả', value: 'all' },
  { label: 'Sorting', value: 'Sorting' },
  { label: 'Graph', value: 'Graph' },
  { label: 'Searching', value: 'Searching' },
  { label: 'Stack & Queue', value: 'Stack-Queue' },
  { label: 'Tree', value: 'Tree' },
];

const difficultyOptions = [
  { label: 'Tất cả độ khó', value: 'all' },
  { label: 'Dễ', value: 'Easy' },
  { label: 'Trung bình', value: 'Medium' },
  { label: 'Khó', value: 'Hard' },
];

const PREMIUM_IDS = new Set(['dijkstra', 'bellman-ford', 'kruskal', 'prim', 'tarjan', 'a-star', 'bst', 'monotonic-stack']);

const hasActiveFilter = computed(() =>
  selectedGroup.value !== 'all' || selectedDifficulty.value !== 'all' || searchQuery.value.trim() !== ''
);

const filteredAlgorithms = computed(() => {
  const q = searchQuery.value.trim().toLowerCase();
  return ALGORITHM_CATALOG.filter(a => {
    const matchGroup = selectedGroup.value === 'all' || a.category === selectedGroup.value;
    const matchDiff = selectedDifficulty.value === 'all' || a.difficulty === selectedDifficulty.value;
    const matchSearch = !q || a.name.toLowerCase().includes(q) || a.id.toLowerCase().includes(q);
    return matchGroup && matchDiff && matchSearch;
  });
});

function groupCount(groupValue: string): number {
  if (groupValue === 'all') return ALGORITHM_CATALOG.length;
  return ALGORITHM_CATALOG.filter(a => a.category === groupValue).length;
}

function clearFilters(): void {
  searchQuery.value = '';
  selectedGroup.value = 'all';
  selectedDifficulty.value = 'all';
}

function categoryLabel(category: string): string {
  const map: Record<string, string> = {
    Sorting: 'Sắp xếp',
    Graph: 'Đồ thị',
    Searching: 'Tìm kiếm',
    'Stack-Queue': 'Stack & Queue',
    Tree: 'Cây',
  };
  return map[category] || category;
}

function difficultyLabel(d: string): string {
  const map: Record<string, string> = { Easy: 'Dễ', Medium: 'TB', Hard: 'Khó' };
  return map[d] || d;
}

function iconName(category: string): string {
  const map: Record<string, string> = {
    Sorting: 'sorting',
    Graph: 'graph',
    Searching: 'search',
    'Stack-Queue': 'code-ide',
    Tree: 'graph',
  };
  return map[category] || 'dsa';
}

function iconClass(category: string): string {
  const map: Record<string, string> = {
    Sorting: 'algo-icon--sorting',
    Graph: 'algo-icon--graph',
    Searching: 'algo-icon--search',
    'Stack-Queue': 'algo-icon--sq',
    Tree: 'algo-icon--tree',
  };
  return map[category] || 'algo-icon--default';
}

function isPremiumAlgo(algo: { id: string }): boolean {
  return PREMIUM_IDS.has(algo.id);
}

function algoRoute(algo: { id: string; category: string }): string {
  // Premium gating thật: user không premium → Checkout thay vì mở visualizer
  if (isPremiumAlgo(algo) && !authStore.isPremium) return '/checkout';
  if (algo.category === 'Sorting') return '/sorting';
  if (algo.category === 'Graph') return '/graph';
  // Searching / Stack-Queue / Tree chưa có màn visualizer riêng → quay về thư viện
  return '/algorithms';
}

const loading = ref(false);
</script>

<style scoped>
.algo-library-view {
  min-height: 0;
}

.algo-card-icon {
  width: 42px;
  height: 42px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.algo-icon--sorting { background: var(--color-accent-primary-dim); color: var(--color-accent-primary); }
.algo-icon--graph { background: var(--color-accent-warm-glow); color: var(--color-accent-warm); }
.algo-icon--search { background: var(--color-accent-cyan-dim); color: var(--color-accent-cyan); }
.algo-icon--sq { background: var(--color-accent-purple-dim); color: var(--color-accent-purple); }
.algo-icon--tree { background: var(--color-accent-green-dim); color: var(--color-accent-green); }
.algo-icon--default { background: var(--color-bg-hover); color: var(--color-text-secondary); }

.animate-fade-in {
  animation: fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}
</style>
