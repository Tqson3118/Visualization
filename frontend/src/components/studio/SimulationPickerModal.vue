<template>
  <div v-if="isOpen" class="fixed inset-0 z-[100] flex items-center justify-center p-3 sm:p-6 bg-black/80 backdrop-blur-sm animate-fade-in">
    <div class="relative w-full max-w-5xl h-[90vh] max-h-[850px] bg-[#10121d] border border-purple-500/40 rounded-2xl shadow-2xl flex flex-col overflow-hidden text-slate-200">
      <!-- Modal Header -->
      <div class="px-5 py-4 bg-[#17192a] border-b border-purple-500/20 flex items-center justify-between gap-4">
        <div class="flex items-center gap-3">
          <span class="p-2.5 rounded-xl bg-purple-600/25 text-purple-300">
            <Sparkles class="w-5 h-5 text-purple-400" />
          </span>
          <div>
            <h2 class="text-base sm:text-lg font-black text-white tracking-tight">Thư viện Mô phỏng Thuật toán & CTDL</h2>
            <p class="text-xs text-slate-400">Xem trước trực quan 44 thuật toán và chọn nhúng vào bài giảng của bạn</p>
          </div>
        </div>

        <button
          type="button"
          class="p-2 rounded-xl bg-slate-800/80 hover:bg-slate-700 text-slate-400 hover:text-white transition-colors cursor-pointer"
          title="Đóng modal"
          @click="emit('close')"
        >
          <X class="w-5 h-5" />
        </button>
      </div>

      <!-- Filter & Search Toolbar -->
      <div class="px-5 py-3 bg-[#131524] border-b border-slate-800/80 flex flex-col sm:flex-row items-stretch sm:items-center justify-between gap-3">
        <!-- Search bar -->
        <div class="relative flex-1 max-w-md">
          <Search class="w-4 h-4 text-slate-400 absolute left-3 top-1/2 -translate-y-1/2" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm theo tên (bubble sort, cây bst, bfs, hash...)"
            class="w-full pl-9 pr-3 py-1.5 text-xs bg-[#0c0d16] border border-slate-700/80 rounded-xl text-white placeholder:text-slate-500 focus:outline-none focus:border-purple-500"
          />
        </div>

        <!-- Category scrollable tabs -->
        <div class="flex items-center gap-1.5 overflow-x-auto pb-1 sm:pb-0 scrollbar-none text-xs">
          <button
            v-for="cat in CATEGORIES"
            :key="cat.id"
            type="button"
            class="px-2.5 py-1 rounded-lg font-medium whitespace-nowrap transition-colors cursor-pointer text-[11px]"
            :class="selectedCategory === cat.id
              ? 'bg-purple-600 text-white font-bold shadow-md shadow-purple-600/30'
              : 'bg-[#1a1c2e] text-slate-400 hover:text-slate-200 hover:bg-[#23263e]'"
            @click="selectedCategory = cat.id"
          >
            {{ cat.name }}
          </button>
        </div>
      </div>

      <!-- Modal Body (2 Columns: List & Live Preview) -->
      <div class="flex-1 min-h-0 grid grid-cols-1 md:grid-cols-12 divide-y md:divide-y-0 md:divide-x divide-slate-800/80 overflow-hidden">
        <!-- Left: Algorithm list (5/12 cols) -->
        <div class="md:col-span-5 h-full overflow-y-auto p-3 space-y-2 bg-[#0c0e18]">
          <div v-if="filteredSimulations.length === 0" class="p-8 text-center text-xs text-slate-500 italic">
            Không tìm thấy thuật toán nào khớp với từ khóa "{{ searchQuery }}".
          </div>

          <div
            v-for="sim in filteredSimulations"
            :key="sim.key"
            class="p-3 rounded-xl border transition-all cursor-pointer select-none flex flex-col gap-1.5"
            :class="activeSimKey === sim.key
              ? 'bg-purple-950/40 border-purple-500 shadow-md shadow-purple-500/10'
              : 'bg-[#141626] border-slate-800/80 hover:border-slate-700 hover:bg-[#1a1c30]'"
            @click="activeSimKey = sim.key"
          >
            <div class="flex items-start justify-between gap-2">
              <h4 class="text-xs font-bold text-white tracking-tight leading-snug">
                {{ sim.title }}
              </h4>
              <span
                v-if="activeSimKey === sim.key"
                class="px-1.5 py-0.5 rounded bg-purple-500 text-white text-[10px] font-extrabold uppercase shrink-0"
              >
                Đang xem
              </span>
            </div>

            <div class="flex items-center gap-1.5 flex-wrap text-[10px]">
              <span class="px-2 py-0.5 rounded bg-purple-950/80 border border-purple-500/30 text-purple-300 font-mono">
                ✨ {{ sim.key }}
              </span>
              <span class="px-1.5 py-0.5 rounded bg-slate-800 text-slate-300 font-mono">
                {{ sim.complexity?.average || 'O(N)' }}
              </span>
              <span class="text-slate-500">{{ sim.dataStructure }}</span>
            </div>
          </div>
        </div>

        <!-- Right: Live Interactive Simulation Player (7/12 cols) -->
        <div class="md:col-span-7 h-full overflow-y-auto p-4 bg-[#10121f] flex flex-col justify-between">
          <div class="space-y-3">
            <div class="flex items-center justify-between">
              <span class="text-xs font-extrabold text-purple-300 uppercase tracking-wider flex items-center gap-1.5">
                <Play class="w-3.5 h-3.5 text-purple-400" />
                <span>Trực quan hóa & Chạy thử thời gian thực</span>
              </span>
              <span v-if="activeMeta" class="text-[11px] text-slate-400">
                Độ phức tạp: <strong class="text-purple-300">{{ activeMeta.complexity.average }}</strong>
              </span>
            </div>

            <!-- Live Player -->
            <div class="rounded-xl overflow-hidden border border-purple-500/30">
              <InlineSimulationPlayer :key="activeSimKey" :sim-key="activeSimKey" class="!my-0 !border-0 !rounded-none" />
            </div>

            <!-- Algorithm Details Card -->
            <div v-if="activeMeta" class="p-3.5 rounded-xl bg-[#15172b] border border-slate-800 text-xs space-y-2">
              <div class="grid grid-cols-2 sm:grid-cols-4 gap-2 text-[11px]">
                <div class="p-2 rounded-lg bg-[#0e101f]">
                  <span class="text-slate-500 block text-[10px]">Tốt nhất (Best)</span>
                  <span class="font-mono font-bold text-emerald-400">{{ activeMeta.complexity.best }}</span>
                </div>
                <div class="p-2 rounded-lg bg-[#0e101f]">
                  <span class="text-slate-500 block text-[10px]">Trung bình (Avg)</span>
                  <span class="font-mono font-bold text-amber-400">{{ activeMeta.complexity.average }}</span>
                </div>
                <div class="p-2 rounded-lg bg-[#0e101f]">
                  <span class="text-slate-500 block text-[10px]">Xấu nhất (Worst)</span>
                  <span class="font-mono font-bold text-rose-400">{{ activeMeta.complexity.worst }}</span>
                </div>
                <div class="p-2 rounded-lg bg-[#0e101f]">
                  <span class="text-slate-500 block text-[10px]">Bộ nhớ (Space)</span>
                  <span class="font-mono font-bold text-sky-400">{{ activeMeta.complexity.space }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Bottom Action Buttons -->
          <div class="pt-4 border-t border-slate-800/80 flex flex-wrap items-center justify-end gap-2.5 mt-4">
            <button
              type="button"
              class="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white text-xs font-semibold transition-colors cursor-pointer"
              @click="emit('close')"
            >
              Đóng
            </button>
            <button
              type="button"
              class="px-4 py-2 rounded-xl bg-[#231e38] hover:bg-purple-950 border border-purple-500/40 text-purple-200 text-xs font-bold transition-all cursor-pointer flex items-center gap-1.5 shadow-sm"
              @click="insertIntoMarkdown(activeSimKey)"
            >
              <FileCode class="w-3.5 h-3.5 text-purple-400" />
              <span>Chèn vào nội dung bài học [Mô phỏng]</span>
            </button>
            <button
              type="button"
              class="px-4 py-2 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold transition-all cursor-pointer flex items-center gap-1.5 shadow-lg shadow-purple-600/30"
              @click="attachSimulation(activeSimKey)"
            >
              <Plus class="w-3.5 h-3.5" />
              <span>Đính kèm vào bài học</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { Sparkles, X, Search, Play, FileCode, Plus } from 'lucide-vue-next';
import InlineSimulationPlayer from '@/components/simulator/InlineSimulationPlayer.vue';
import { CATALOG, type CatalogMeta } from '@/engines/catalog';

const props = defineProps<{
  isOpen: boolean;
  initialKey?: string;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'attach', key: string): void;
  (e: 'insert', key: string): void;
}>();

const activeSimKey = ref<string>(props.initialKey || 'sort.bubble');
const searchQuery = ref('');
const selectedCategory = ref('all');

const CATEGORIES = [
  { id: 'all', name: 'Tất cả (44)' },
  { id: 'sort', name: 'Sắp xếp' },
  { id: 'search', name: 'Tìm kiếm' },
  { id: 'linear', name: 'Ngăn xếp / Hàng đợi' },
  { id: 'list', name: 'Danh sách liên kết' },
  { id: 'tree', name: 'Cây & AVL' },
  { id: 'heap', name: 'Đống (Heap)' },
  { id: 'hash', name: 'Bảng băm' },
  { id: 'graph', name: 'Đồ thị' },
  { id: 'structure', name: 'Cấu trúc dữ liệu' },
];

const activeMeta = computed<CatalogMeta | undefined>(() => {
  return CATALOG.find((c) => c.key === activeSimKey.value);
});

const filteredSimulations = computed<CatalogMeta[]>(() => {
  return CATALOG.filter((sim) => {
    // Category match
    if (selectedCategory.value !== 'all') {
      if (selectedCategory.value === 'sort' && !sim.key.startsWith('sort.')) return false;
      if (selectedCategory.value === 'search' && !sim.key.startsWith('search.')) return false;
      if (selectedCategory.value === 'linear' && !sim.key.startsWith('stack.') && !sim.key.startsWith('queue.')) return false;
      if (selectedCategory.value === 'list' && !sim.key.startsWith('list.') && sim.key !== 'structure.linkedlist') return false;
      if (selectedCategory.value === 'tree' && !sim.key.startsWith('tree.') && sim.key !== 'structure.bst' && sim.key !== 'structure.avl' && sim.key !== 'structure.binarytree') return false;
      if (selectedCategory.value === 'heap' && !sim.key.startsWith('heap.') && sim.key !== 'structure.heap') return false;
      if (selectedCategory.value === 'hash' && !sim.key.startsWith('hash.') && sim.key !== 'structure.hashtable') return false;
      if (selectedCategory.value === 'graph' && !sim.key.startsWith('graph.') && sim.key !== 'structure.graph') return false;
      if (selectedCategory.value === 'structure' && !sim.key.startsWith('structure.')) return false;
    }

    // Search query match
    if (searchQuery.value.trim()) {
      const q = searchQuery.value.toLowerCase().trim();
      const matchTitle = sim.title.toLowerCase().includes(q);
      const matchKey = sim.key.toLowerCase().includes(q);
      const matchDs = sim.dataStructure.toLowerCase().includes(q);
      const matchTags = sim.tags.some((t) => t.toLowerCase().includes(q));
      return matchTitle || matchKey || matchDs || matchTags;
    }

    return true;
  });
});

watch(() => props.initialKey, (newKey) => {
  if (newKey) {
    activeSimKey.value = newKey;
  }
});

function attachSimulation(key: string): void {
  emit('attach', key);
  emit('close');
}

function insertIntoMarkdown(key: string): void {
  emit('insert', key);
  emit('close');
}
</script>
