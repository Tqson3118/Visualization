<script setup lang="ts">
// CheatSheetTable — bảng Big-O tương tác (Màn 18 — FR-2.10)
// Dữ liệu từ engines/catalog (44 mô phỏng); lọc nhóm; nút "Xem mô phỏng" deep-link.
// View-quality (nhóm A): chip lọc + nút sim qua Button shadcn (0 raw <button>; aria-pressed);
// giá trị Big-O → block-token chip tối canvas-ink + mono (vùng dữ liệu LUÔN tối); mobile
// ≤640px = card-stack (cấm scroll ngang bảng chính §8); i18n thay hardcode.
import { computed, ref } from 'vue';
import { Play } from 'lucide-vue-next';

import { CATALOG, type CatalogMeta } from '@/engines/catalog';
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
</script>

<template>
  <section class="cheatsheet">
    <header class="cheatsheet__header">
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
            <th scope="col" :aria-label="messages.cheatsheet.colAction"></th>
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
            <td :data-label="messages.cheatsheet.colAction">
              <Button
                variant="outline"
                size="sm"
                :aria-label="messages.cheatsheet.openSimulation(item.title)"
                @click="emit('open-simulation', item.key)"
              >
                <Play aria-hidden="true" />
                {{ messages.cheatsheet.simulate }}
              </Button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <footer class="cheatsheet__footer">
      {{ messages.cheatsheet.source(CATALOG.length) }}
    </footer>
  </section>
</template>

<style scoped>
.cheatsheet { display: flex; flex-direction: column; gap: var(--space-md); }

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

.cheatsheet__table tbody tr {
  transition:
    background-color 150ms var(--ease-out-expo),
    transform 150ms var(--ease-out-expo);
}

.cheatsheet__table tbody tr:hover {
  background: color-mix(in srgb, var(--color-muted) 50%, transparent);
}

.cheatsheet__name { font-weight: 600; }

.cheatsheet__meta { display: flex; gap: var(--space-sm); margin-top: var(--space-xs); flex-wrap: wrap; }

/* Big-O chip — block-token tối (vùng dữ liệu LUÔN tối): mono text-sm, min-h 24px (trục 5f)
   UI-PREMIUM 1D: hover glow (data-core) — phản hồi trực quan khi dò bảng */
.cheatsheet__bigo {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  line-height: 1.4;
  white-space: nowrap;
  transition:
    box-shadow 150ms var(--ease-out-expo),
    transform 150ms var(--ease-out-expo);
}

.cheatsheet__table tbody tr:hover .cheatsheet__bigo {
  box-shadow: var(--glow-data-core);
  transform: translateY(-1px);
}

@media (prefers-reduced-motion: reduce) {
  .cheatsheet__table tbody tr,
  .cheatsheet__bigo {
    transition: none;
  }

  .cheatsheet__table tbody tr:hover .cheatsheet__bigo {
    box-shadow: none;
    transform: none;
  }
}

.cheatsheet__footer {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  font-family: var(--font-mono);
}

/* ── Mobile ≤640px: card-stack (1 tr = 1 card, cấm scroll ngang bảng chính — §8) ── */
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

  /* Cột tên (title + badges) — full width, không flex 2 bên */
  .cheatsheet__table td:first-child {
    display: block;
    padding: 0 0 var(--space-sm);
    border-bottom: 1px solid var(--color-border);
  }
}
</style>
