<script setup lang="ts">
// CheatSheetTable — bảng Big-O tương tác (Màn 18 — FR-2.10)
// Dữ liệu từ engines/catalog (44 mô phỏng); lọc nhóm; nút "▶ Xem mô phỏng" deep-link.
import { computed, ref } from 'vue';

import { CATALOG, type CatalogMeta } from '@/engines/catalog';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const emit = defineEmits<{
  'open-simulation': [key: string];
}>();

const groups = computed(() => {
  const set = new Set<string>();
  for (const item of CATALOG) {
    set.add(item.dataStructure);
  }
  return ['Tất cả', ...set];
});

const activeGroup = ref('Tất cả');
const filterKey = ref('');

const filtered = computed(() => {
  let list: CatalogMeta[] = CATALOG;
  if (activeGroup.value !== 'Tất cả') {
    list = list.filter((item) => item.dataStructure === activeGroup.value);
  }
  const q = filterKey.value.trim().toLowerCase();
  if (q) {
    list = list.filter((item) => item.key.toLowerCase().includes(q) || item.title.toLowerCase().includes(q));
  }
  return list;
});

function clearFilters(): void {
  activeGroup.value = 'Tất cả';
  filterKey.value = '';
}
</script>

<template>
  <section class="cheatsheet">
    <header class="cheatsheet__header">
      <div class="cheatsheet__filters">
        <button
          v-for="group in groups"
          :key="group"
          type="button"
          class="cheatsheet__chip"
          :class="{ 'cheatsheet__chip--active': activeGroup === group }"
          @click="activeGroup = group"
        >
          {{ group }}
        </button>
      </div>
      <input
        v-model="filterKey"
        class="cheatsheet__search input"
        type="search"
        placeholder="Tìm theo tên hoặc key..."
        :aria-label="'Tìm kiếm'"
      />
    </header>

    <EmptyState
      v-if="filtered.length === 0"
      icon="search"
      title="Không có mô phỏng phù hợp"
      description="Thử xóa bộ lọc hoặc đổi từ khóa tìm kiếm."
      action-label="Xóa bộ lọc"
      @action="clearFilters"
    />

    <div v-else class="cheatsheet__table-wrap">
      <table class="cheatsheet__table">
        <thead>
          <tr>
            <th>Giải thuật / CTDL</th>
            <th>Best</th>
            <th>Average</th>
            <th>Worst</th>
            <th>Không gian</th>
            <th aria-label="Hành động"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filtered" :key="item.key">
            <td>
              <p class="cheatsheet__name">{{ item.title }}</p>
              <div class="cheatsheet__meta">
                <Badge variant="muted">{{ item.dataStructure }}</Badge>
                <Badge :variant="item.level === 'basic' ? 'primary' : 'warning'">
                  {{ item.level === 'basic' ? 'Cơ bản' : 'Nâng cao' }}
                </Badge>
                <Badge v-if="item.demoAllowed" variant="success">Demo</Badge>
              </div>
            </td>
            <td class="cheatsheet__complexity">{{ item.complexity.best }}</td>
            <td class="cheatsheet__complexity">{{ item.complexity.average }}</td>
            <td class="cheatsheet__complexity">{{ item.complexity.worst }}</td>
            <td class="cheatsheet__complexity">{{ item.complexity.space }}</td>
            <td>
              <button
                type="button"
                class="cheatsheet__sim-btn"
                :aria-label="`Mở mô phỏng ${item.title}`"
                @click="emit('open-simulation', item.key)"
              >
                ▶ Xem mô phỏng
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <footer class="cheatsheet__footer text-muted">
      Nguồn dữ liệu: <code>shared/simulation-catalog.json</code> — 44 mô phỏng.
    </footer>
  </section>
</template>

<style scoped>
.cheatsheet { display: flex; flex-direction: column; gap: var(--space-md); }

.cheatsheet__header { display: flex; flex-direction: column; gap: var(--space-sm); }

.cheatsheet__filters { display: flex; gap: var(--space-xs); flex-wrap: wrap; }

.cheatsheet__chip {
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  padding: 4px 12px;
  border-radius: var(--radius-full);
  font-size: var(--text-xs);
  font-weight: 600;
  cursor: pointer;
  color: var(--color-text-muted);
}

.cheatsheet__chip--active { background: var(--color-primary); color: var(--color-on-primary); border-color: var(--color-primary); }

.cheatsheet__search { max-width: 320px; }

.cheatsheet__table-wrap { overflow-x: auto; }

.cheatsheet__table {
  width: 100%;
  border-collapse: collapse;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow: hidden;
  min-width: 640px;
}

.cheatsheet__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 2px solid var(--color-border);
  background: var(--color-muted);
}

.cheatsheet__table td { padding: var(--space-sm) var(--space-md); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); }

.cheatsheet__name { font-weight: 700; }

.cheatsheet__meta { display: flex; gap: 6px; margin-top: 4px; flex-wrap: wrap; }

.cheatsheet__complexity { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-foreground); white-space: nowrap; }

.cheatsheet__sim-btn {
  background: none;
  border: 1px solid var(--color-primary);
  color: var(--color-primary);
  padding: 4px 10px;
  border-radius: var(--radius-md);
  font-size: var(--text-xs);
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
}
.cheatsheet__sim-btn:hover { background: var(--color-surface-hover); }

.cheatsheet__footer { font-size: var(--text-xs); }
.cheatsheet__footer code { font-family: var(--font-mono); }
</style>
