<script setup lang="ts">
import { computed, ref } from 'vue';
import { ExternalLink, Layers, Plus, Search, Sparkles, X } from 'lucide-vue-next';
import type { SimulationMetaDto } from '@/api/simulations';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';

const props = defineProps<{
  modelValue: string[];
  simulations: SimulationMetaDto[];
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', val: string[]): void;
  (e: 'insertAnchor', key: string): void;
}>();

const simSearch = ref('');
const simCategoryFilter = ref<string>('all');

const simulationCategories = [
  { key: 'all', label: 'Tất cả (40+)' },
  { key: 'sort', label: 'Sắp xếp (Sorting)' },
  { key: 'search', label: 'Tìm kiếm (Search)' },
  { key: 'linear', label: 'CTDL Tuyến tính' },
  { key: 'tree', label: 'Cây & BST' },
  { key: 'graph', label: 'Đồ thị (Graph)' },
  { key: 'hash', label: 'Bảng băm (Hash)' },
];

const filteredSimulations = computed(() => {
  let list = props.simulations;

  if (simCategoryFilter.value === 'sort') {
    list = list.filter((s) => s.key.startsWith('sort.') || s.tags?.some((t) => t.includes('sort')));
  } else if (simCategoryFilter.value === 'search') {
    list = list.filter((s) => s.key.startsWith('search.') || s.tags?.some((t) => t.includes('search')));
  } else if (simCategoryFilter.value === 'linear') {
    list = list.filter((s) => s.key.startsWith('list.') || s.key.startsWith('stack.') || s.key.startsWith('queue.'));
  } else if (simCategoryFilter.value === 'tree') {
    list = list.filter((s) => s.key.startsWith('tree.') || s.key.startsWith('bst.'));
  } else if (simCategoryFilter.value === 'graph') {
    list = list.filter((s) => s.key.startsWith('graph.'));
  } else if (simCategoryFilter.value === 'hash') {
    list = list.filter((s) => s.key.startsWith('hash.') || s.key.startsWith('set.'));
  }

  if (simSearch.value.trim()) {
    const q = simSearch.value.trim().toLowerCase();
    list = list.filter(
      (s) =>
        s.title.toLowerCase().includes(q) ||
        s.key.toLowerCase().includes(q) ||
        s.dataStructure?.toLowerCase().includes(q) ||
        s.tags?.some((t) => t.toLowerCase().includes(q)),
    );
  }

  return list;
});

function toggleSimulation(key: string): void {
  const current = [...props.modelValue];
  const idx = current.indexOf(key);
  if (idx >= 0) {
    current.splice(idx, 1);
  } else {
    current.push(key);
  }
  emit('update:modelValue', current);
}

function removeSimulation(key: string): void {
  const current = props.modelValue.filter((k) => k !== key);
  emit('update:modelValue', current);
}

function insertToTheory(key: string): void {
  if (!props.modelValue.includes(key)) {
    toggleSimulation(key);
  }
  emit('insertAnchor', key);
}
</script>

<template>
  <div class="simulation-tab flex flex-col h-full p-6 overflow-y-auto max-w-5xl mx-auto space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between border-b border-vdsa-border pb-4">
      <div>
        <h2 class="text-lg font-black text-white flex items-center gap-2">
          <Layers class="text-vdsa-purple" :size="20" />
          Kho Mô phỏng Trực quan Tương tác (40+ Thuật toán)
        </h2>
        <p class="text-xs text-vdsa-muted mt-1">
          Chọn các thuật toán trực quan để đính kèm vào bài giảng hoặc chèn trực tiếp thẻ hoạt ảnh vào nội dung lý thuyết.
        </p>
      </div>
      <Badge variant="primary" class="font-mono text-xs">
        Đã gắn: {{ modelValue.length }} mô phỏng
      </Badge>
    </div>

    <!-- Selected chips bar -->
    <div v-if="modelValue.length > 0" class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-2">
      <div class="flex items-center justify-between text-xs text-vdsa-muted">
        <span class="font-bold text-white flex items-center gap-1.5">
          <Sparkles :size="13" class="text-amber-400" /> Mô phỏng đang đính kèm vào bài học:
        </span>
        <span>Bấm dấu X để gỡ</span>
      </div>
      <div class="flex flex-wrap gap-2">
        <span
          v-for="simKey in modelValue"
          :key="simKey"
          class="inline-flex items-center gap-1.5 px-3 py-1 rounded-lg bg-purple-500/15 border border-purple-500/30 text-purple-200 text-xs font-mono"
        >
          <span>{{ simKey }}</span>
          <button
            type="button"
            class="text-slate-400 hover:text-red-400 transition-colors cursor-pointer"
            title="Gỡ mô phỏng"
            @click="removeSimulation(simKey)"
          >
            <X :size="13" />
          </button>
        </span>
      </div>
    </div>

    <!-- Search and Categories Filter -->
    <div class="space-y-3">
      <div class="flex items-center gap-3">
        <div class="flex-1 flex items-center gap-2 px-3 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl">
          <Search :size="15" class="text-vdsa-muted" />
          <input
            v-model="simSearch"
            type="text"
            placeholder="Tìm kiếm theo tên hoặc mã thuật toán (vd: quick, tree, bst, dijkstra...)..."
            class="flex-1 bg-transparent border-none outline-none text-xs text-white placeholder-slate-500"
          />
          <button v-if="simSearch" type="button" class="text-slate-400 hover:text-white" @click="simSearch = ''">
            <X :size="14" />
          </button>
        </div>
      </div>

      <!-- Categories Pills -->
      <div class="flex gap-2 overflow-x-auto pb-1">
        <button
          v-for="cat in simulationCategories"
          :key="cat.key"
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-bold whitespace-nowrap transition-colors border cursor-pointer"
          :class="
            simCategoryFilter === cat.key
              ? 'bg-vdsa-accent text-white border-vdsa-accent shadow'
              : 'bg-vdsa-surface text-vdsa-secondary border-vdsa-border hover:text-white'
          "
          @click="simCategoryFilter = cat.key"
        >
          {{ cat.label }}
        </button>
      </div>
    </div>

    <!-- Simulation Cards Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-3.5">
      <div
        v-for="sim in filteredSimulations"
        :key="sim.key"
        class="p-4 rounded-xl bg-vdsa-surface border transition-all flex flex-col justify-between gap-3 cursor-pointer"
        :class="
          modelValue.includes(sim.key)
            ? 'border-vdsa-accent bg-vdsa-accent/5 shadow-md shadow-purple-950/20'
            : 'border-vdsa-border hover:border-slate-600'
        "
        @click="toggleSimulation(sim.key)"
      >
        <div class="flex items-start justify-between gap-2">
          <div class="flex items-start gap-3">
            <input
              type="checkbox"
              :checked="modelValue.includes(sim.key)"
              class="mt-1 rounded text-vdsa-purple focus:ring-0 cursor-pointer"
              @click.stop="toggleSimulation(sim.key)"
            />
            <div>
              <h4 class="text-sm font-bold text-white leading-snug">{{ sim.title }}</h4>
              <p class="text-[11px] font-mono text-purple-300 mt-0.5">{{ sim.key }}</p>
              <p v-if="sim.dataStructure" class="text-xs text-slate-400 mt-1 line-clamp-1">
                {{ sim.dataStructure }}
              </p>
            </div>
          </div>
          <Badge variant="muted" class="text-[10px] shrink-0 uppercase tracking-wider font-mono">
            {{ sim.category }}
          </Badge>
        </div>

        <div class="flex items-center justify-between pt-2 border-t border-vdsa-border/60">
          <button
            type="button"
            class="text-xs font-bold text-vdsa-purple-light hover:text-white flex items-center gap-1 cursor-pointer transition-colors"
            title="Chèn thẻ [Mô phỏng: key] vào bài giảng"
            @click.stop="insertToTheory(sim.key)"
          >
            <Plus :size="13" /> Chèn vào bài giảng
          </button>
          <a
            :href="`/simulator/${sim.key}`"
            target="_blank"
            class="text-xs text-slate-400 hover:text-white flex items-center gap-1 transition-colors"
            @click.stop
          >
            Xem thử <ExternalLink :size="12" />
          </a>
        </div>
      </div>

      <div v-if="filteredSimulations.length === 0" class="col-span-full py-12 text-center text-slate-500 text-xs">
        Không tìm thấy mô phỏng phù hợp với từ khóa "{{ simSearch }}".
      </div>
    </div>
  </div>
</template>
