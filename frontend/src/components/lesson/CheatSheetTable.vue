<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  BookOpen,
  Play,
  Printer,
  Download,
  FileText,
  HelpCircle,
  ExternalLink,
  Layers,
  Sparkles,
  Search,
  Check,
  TrendingUp,
} from 'lucide-vue-next';

import { CATALOG, type CatalogMeta } from '@/engines/catalog';
import { getReference } from '@/data/referenceLinks';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/button/Button.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { messages } from '@/i18n/vi';

const emit = defineEmits<{
  'open-simulation': [key: string];
}>();

const groups = computed(() => {
  const set = new Set<string>();
  for (const item of CATALOG) {
    set.add(item.dataStructure);
  }
  return [messages.cheatsheet.all, ...set];
});

const activeGroup = ref<string>(messages.cheatsheet.all);
const filterKey = ref('');

const filtered = computed(() => {
  let list: CatalogMeta[] = CATALOG;
  if (activeGroup.value !== messages.cheatsheet.all) {
    list = list.filter((item) => item.dataStructure === activeGroup.value);
  }
  const q = filterKey.value.trim().toLowerCase();
  if (q) {
    list = list.filter((item) => item.key.toLowerCase().includes(q) || item.title.toLowerCase().includes(q));
  }
  return list;
});

function clearFilters(): void {
  activeGroup.value = messages.cheatsheet.all;
  filterKey.value = '';
}

/** URL tài liệu — ưu tiên Wikipedia, fallback GeeksforGeeks (undefined → ẩn link). */
function referenceUrl(key: string): string | undefined {
  const ref = getReference(key);
  return ref?.wikipedia ?? ref?.geeksforgeeks;
}

function printCheatSheet(): void {
  window.print();
}

// ── Bảng tóm tắt Big-O tổng quan ──
const BIG_O_RANKS = [
  { notation: 'O(1)', name: 'Hằng số (Constant)', speed: 'Tuyệt vời', color: 'bg-emerald-500/15 text-emerald-400 border-emerald-500/30', example: 'Truy cập mảng bằng index, Push/Pop stack' },
  { notation: 'O(log N)', name: 'Logarit (Logarithmic)', speed: 'Tốt', color: 'bg-teal-500/15 text-teal-400 border-teal-500/30', example: 'Tìm kiếm nhị phân (Binary Search), Tìm kiếm BST cân bằng' },
  { notation: 'O(N)', name: 'Tuyến tính (Linear)', speed: 'Khá', color: 'bg-blue-500/15 text-blue-400 border-blue-500/30', example: 'Duyệt danh sách liên kết, Tìm kiếm tuần tự' },
  { notation: 'O(N log N)', name: 'Linearithmic', speed: 'Chấp nhận được', color: 'bg-indigo-500/15 text-indigo-400 border-indigo-500/30', example: 'Merge Sort, Quick Sort (TB), Heap Sort' },
  { notation: 'O(N²)', name: 'Bậc hai (Quadratic)', speed: 'Kém', color: 'bg-amber-500/15 text-amber-400 border-amber-500/30', example: 'Bubble Sort, Insertion Sort, 2 vòng lặp lồng nhau' },
  { notation: 'O(2^N)', name: 'Hàm mũ (Exponential)', speed: 'Rất tệ', color: 'bg-rose-500/15 text-rose-400 border-rose-500/30', example: 'Đệ quy Fibonacci ngây thơ, Duyệt tập hợp con (Subsets)' },
  { notation: 'O(N!)', name: 'Giai thừa (Factorial)', speed: 'Thảm họa', color: 'bg-red-600/15 text-red-500 border-red-500/30', example: 'Sinh tất cả hoán vị (Permutations), Bài toán người bán hàng TSP' },
];

// ── Danh mục tài liệu tham khảo PDF chính thống ──
const REFERENCE_DOCS = [
  {
    title: 'DSA Full CheatSheet & Quick Reference Card',
    author: 'Stanford & MIT Course Notes',
    desc: 'Tổng hợp công thức Big-O, cấu trúc dữ liệu, cây và đồ thị chuẩn ôn thi kỹ sư phần mềm.',
    link: 'https://www.bigocheatsheet.com/',
    type: 'Web & PDF Reference',
  },
  {
    title: 'Algorithms 4th Edition - CheatSheet Sheet',
    author: 'Robert Sedgewick (Princeton University)',
    desc: 'Bảng tra cứu độ phức tạp thuật toán và thuộc tính cấu trúc dữ liệu kinh điển của ĐH Princeton.',
    link: 'https://algs4.cs.princeton.edu/cheatsheet/',
    type: 'Princeton Academic Doc',
  },
  {
    title: 'GeeksforGeeks DSA Reference Guide',
    author: 'GeeksforGeeks',
    desc: 'Bộ tài liệu trực quan minh họa từng bước hoạt động của các thuật toán phổ biến.',
    link: 'https://www.geeksforgeeks.org/dsa-tutorial-learn-data-structures-and-algorithms/',
    type: 'Online Tutorials & Code',
  },
];
</script>

<template>
  <section class="cheatsheet">
    <!-- Top Action Bar for PDF Print -->
    <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 print-hide">
      <div>
        <h2 class="text-lg font-extrabold text-white flex items-center gap-2">
          <FileText :size="20" class="text-vdsa-accent" />
          Bảng Tra Cứu Big-O &amp; Tài Liệu Giải Thuật (CheatSheet)
        </h2>
        <p class="text-xs text-vdsa-muted mt-1">
          Tra cứu nhanh độ phức tạp thời gian &amp; không gian của 44 thuật toán và cấu trúc dữ liệu. Hỗ trợ in và xuất file PDF.
        </p>
      </div>

      <div class="flex items-center gap-2">
        <Button variant="outline" size="sm" class="gap-1.5" @click="printCheatSheet">
          <Printer :size="15" /> Xuất File PDF / In CheatSheet (A4)
        </Button>
      </div>
    </div>

    <!-- 1. BẢNG XẾP HẠNG THỨ BẬC BIG-O TRỰC QUAN -->
    <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-4">
      <h3 class="text-sm font-extrabold text-white uppercase tracking-wider flex items-center gap-2">
        <TrendingUp :size="16" class="text-vdsa-yellow" />
        1. Phổ Độ Phức Tạp Big-O &amp; Quy Tắc Nhận Diện
      </h3>

      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
        <div
          v-for="rank in BIG_O_RANKS"
          :key="rank.notation"
          class="p-3.5 rounded-xl border flex flex-col justify-between gap-2"
          :class="rank.color"
        >
          <div>
            <div class="flex items-center justify-between">
              <span class="font-mono font-extrabold text-sm">{{ rank.notation }}</span>
              <span class="text-[10px] uppercase font-bold px-2 py-0.5 rounded-full border bg-black/20">{{ rank.speed }}</span>
            </div>
            <p class="text-xs font-bold text-white mt-1">{{ rank.name }}</p>
          </div>
          <p class="text-[11px] text-white/70 leading-relaxed border-t border-white/10 pt-1.5">
            <strong>Ví dụ:</strong> {{ rank.example }}
          </p>
        </div>
      </div>
    </div>

    <!-- 2. BỘ TÀI LIỆU PDF & THAM KHẢO CHÍNH THỐNG -->
    <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-4 print-hide">
      <h3 class="text-sm font-extrabold text-white uppercase tracking-wider flex items-center gap-2">
        <BookOpen :size="16" class="text-vdsa-purple-light" />
        2. Tài Liệu PDF &amp; Nguồn Tra Cứu Chính Thống
      </h3>

      <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <a
          v-for="doc in REFERENCE_DOCS"
          :key="doc.title"
          :href="doc.link"
          target="_blank"
          rel="noopener noreferrer"
          class="p-4 rounded-xl bg-vdsa-bg border border-vdsa-border hover:border-vdsa-accent hover:bg-vdsa-hover transition-all flex flex-col justify-between group"
        >
          <div>
            <div class="flex items-center justify-between text-xs text-vdsa-purple-light font-bold mb-1">
              <span>{{ doc.type }}</span>
              <ExternalLink :size="13" class="group-hover:translate-x-0.5 group-hover:-translate-y-0.5 transition-transform" />
            </div>
            <h4 class="text-sm font-bold text-white group-hover:text-vdsa-accent transition-colors">{{ doc.title }}</h4>
            <p class="text-[11px] text-vdsa-muted mt-1">{{ doc.desc }}</p>
          </div>
          <span class="text-[10px] text-vdsa-disabled font-semibold mt-3">Nguồn: {{ doc.author }}</span>
        </a>
      </div>
    </div>

    <!-- 3. BẢNG CHI TIẾT ĐỘ PHỨC TẠP 44 MÔ PHỎNG (INTERACTIVE TABLE) -->
    <div class="space-y-4">
      <div class="flex items-center justify-between print-hide">
        <h3 class="text-sm font-extrabold text-white uppercase tracking-wider flex items-center gap-2">
          <Layers :size="16" class="text-vdsa-accent" />
          3. Bảng Chi Tiết Độ Phức Tạp {{ CATALOG.length }} Thuật Toán &amp; Cấu Trúc Dữ Liệu
        </h3>
      </div>

      <header class="cheatsheet__header print-hide">
        <div class="cheatsheet__filters" role="group" :aria-label="messages.cheatsheet.filterGroupAria">
          <Button
            v-for="group in groups"
            :key="group"
            variant="outline"
            size="sm"
            :aria-pressed="activeGroup === group"
            :class="
              activeGroup === group
                ? 'border-primary bg-primary text-primary-foreground hover:bg-primary/90 hover:text-primary-foreground'
                : ''
            "
            @click="activeGroup = group"
          >
            {{ group }}
          </Button>
        </div>
        <input
          v-model="filterKey"
          name="cheatsheet-search"
          class="cheatsheet__search input"
          type="search"
          :placeholder="messages.cheatsheet.searchPlaceholder"
          :aria-label="messages.cheatsheet.searchAria"
        />
      </header>

      <EmptyState
        v-if="filtered.length === 0"
        icon="search"
        :title="messages.cheatsheet.emptyTitle"
        :description="messages.cheatsheet.emptyDesc"
        :action-label="messages.cheatsheet.clearFilters"
        @action="clearFilters"
      />

      <div v-else class="cheatsheet__table-wrap">
        <table class="cheatsheet__table">
          <thead>
            <tr>
              <th scope="col">{{ messages.cheatsheet.colAlgorithm }}</th>
              <th scope="col">{{ messages.cheatsheet.colBest }}</th>
              <th scope="col">{{ messages.cheatsheet.colAverage }}</th>
              <th scope="col">{{ messages.cheatsheet.colWorst }}</th>
              <th scope="col">{{ messages.cheatsheet.colSpace }}</th>
              <th scope="col" class="print-hide" :aria-label="messages.cheatsheet.colAction"></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in filtered" :key="item.key">
              <td>
                <p class="cheatsheet__name">{{ item.title }}</p>
                <div class="cheatsheet__meta">
                  <Badge variant="muted">{{ item.dataStructure }}</Badge>
                  <Badge :variant="item.level === 'basic' ? 'primary' : 'warning'">
                    {{ item.level === 'basic' ? messages.explore.levelBasic : messages.explore.levelAdvanced }}
                  </Badge>
                  <Badge v-if="item.demoAllowed" variant="success">{{ messages.explore.badgeDemo }}</Badge>
                </div>
              </td>
              <td :data-label="messages.cheatsheet.colBest">
                <code class="cheatsheet__bigo">{{ item.complexity.best }}</code>
              </td>
              <td :data-label="messages.cheatsheet.colAverage">
                <code class="cheatsheet__bigo">{{ item.complexity.average }}</code>
              </td>
              <td :data-label="messages.cheatsheet.colWorst">
                <code class="cheatsheet__bigo">{{ item.complexity.worst }}</code>
              </td>
              <td :data-label="messages.cheatsheet.colSpace">
                <code class="cheatsheet__bigo">{{ item.complexity.space }}</code>
              </td>
              <td class="print-hide" :data-label="messages.cheatsheet.colAction">
                <div class="cheatsheet__actions">
                  <a
                    v-if="referenceUrl(item.key)"
                    class="cheatsheet__doc-link"
                    :href="referenceUrl(item.key)"
                    target="_blank"
                    rel="noopener noreferrer"
                    :aria-label="`Đọc tài liệu: ${item.title}`"
                  >
                    <BookOpen :size="14" aria-hidden="true" />
                    Tài liệu
                  </a>
                  <Button
                    variant="outline"
                    size="sm"
                    :aria-label="messages.cheatsheet.openSimulation(item.title)"
                    @click="emit('open-simulation', item.key)"
                  >
                    <Play aria-hidden="true" />
                    {{ messages.cheatsheet.simulate }}
                  </Button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <footer class="cheatsheet__footer print-hide">
        {{ messages.cheatsheet.source(CATALOG.length) }}
      </footer>
    </div>
  </section>
</template>

<style scoped>
.cheatsheet { display: flex; flex-direction: column; gap: var(--space-lg); }

.cheatsheet__header { display: flex; flex-direction: column; gap: var(--space-sm); }

.cheatsheet__filters { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.cheatsheet__search { max-width: 320px; }

/* ── Bảng dữ liệu — level-1 (§4.6): thead h-10 medium tertiary, td 12/16px, hover muted ── */
.cheatsheet__table-wrap { overflow-x: auto; }

.cheatsheet__table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
  min-width: 640px;
}

.cheatsheet__table th {
  text-align: left;
  height: 40px;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--color-text-tertiary);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  background: var(--color-muted);
}

.cheatsheet__table td {
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--color-border);
  font-size: var(--text-sm);
}

.cheatsheet__table tbody tr:last-child td { border-bottom: none; }

.cheatsheet__table tbody tr:hover {
  background: color-mix(in srgb, var(--color-muted) 50%, transparent);
}

.cheatsheet__name { font-weight: 600; }

.cheatsheet__actions {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

/* Link tài liệu */
.cheatsheet__doc-link {
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
  transition:
    border-color 150ms cubic-bezier(0.16, 1, 0.3, 1),
    background-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.cheatsheet__doc-link:hover {
  border-color: var(--color-primary);
  background: var(--color-surface-hover);
}

.cheatsheet__doc-link:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
}

.cheatsheet__meta { display: flex; gap: var(--space-sm); margin-top: var(--space-xs); flex-wrap: wrap; }

/* Big-O chip */
.cheatsheet__bigo {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: 3px 8px;
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  color: #38bdf8;
  font-family: var(--font-mono);
  font-size: 13px;
  font-weight: 500;
  line-height: 1.4;
  white-space: nowrap;
}

.cheatsheet__footer {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  font-family: var(--font-mono);
}

/* ── Mobile ≤640px: card-stack ── */
@media (max-width: 640px) {
  .cheatsheet__table-wrap { overflow: visible; }

  .cheatsheet__table {
    min-width: 0;
    background: transparent;
    border: none;
    border-radius: 0;
    overflow: visible;
  }

  .cheatsheet__table thead { display: none; }

  .cheatsheet__table,
  .cheatsheet__table tbody,
  .cheatsheet__table tr { display: block; }

  .cheatsheet__table tr {
    background: var(--color-card);
    border: 1px solid var(--color-border);
    border-radius: var(--radius-lg);
    padding: var(--space-md);
    margin-bottom: var(--space-md);
  }

  .cheatsheet__table tr:last-child { margin-bottom: 0; }

  .cheatsheet__table tbody tr:hover { background: var(--color-card); }

  .cheatsheet__table td {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-sm);
    padding: var(--space-xs) 0;
    border-bottom: 1px solid var(--color-border);
  }

  .cheatsheet__table td:last-child { border-bottom: none; }

  .cheatsheet__table td[data-label]::before {
    content: attr(data-label);
    font-size: var(--text-xs);
    font-weight: 500;
    color: var(--color-text-tertiary);
  }

  .cheatsheet__table td:first-child {
    display: block;
    padding: 0 0 var(--space-sm);
    border-bottom: 1px solid var(--color-border);
  }
}

/* ── PDF Print Styles: In A4 chuẩn và đẹp ── */
@media print {
  .print-hide { display: none !important; }
  .cheatsheet { padding: 0 !important; gap: 12px !important; }
  .cheatsheet__table {
    background: #ffffff !important;
    color: #000000 !important;
    border: 1px solid #333333 !important;
    font-size: 11px !important;
    min-width: 100% !important;
  }
  .cheatsheet__table th {
    background: #f0f0f0 !important;
    color: #000000 !important;
    border-bottom: 1px solid #333333 !important;
    font-weight: bold !important;
  }
  .cheatsheet__table td {
    color: #000000 !important;
    border-bottom: 1px solid #e0e0e0 !important;
    padding: 6px 8px !important;
  }
  .cheatsheet__bigo {
    background: #f3f4f6 !important;
    color: #111827 !important;
    border: 1px solid #d1d5db !important;
    font-weight: bold !important;
  }
}
</style>
