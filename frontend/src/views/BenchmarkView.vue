<script setup lang="ts">
// BenchmarkView — Màn 17: route /benchmark/:k1/:k2 (MIỄN PHÍ tim — 20.4)
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import BenchmarkPanel from '@/components/benchmark/BenchmarkPanel.vue';
import Button from '@/components/ui/Button.vue';
import { getCatalogMeta } from '@/engines/catalog';

const route = useRoute();
const router = useRouter();

const defaultKeys = computed(() => {
  const k1 = String(route.params.k1 ?? '');
  const k2 = String(route.params.k2 ?? '');
  return [k1, k2].filter(Boolean);
});

const invalid = computed(() =>
  defaultKeys.value.some((key) => !getCatalogMeta(key)),
);
</script>

<template>
  <main class="benchmark-view container">
    <nav class="benchmark-view__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
      <span aria-hidden="true">/</span>
      <span>Benchmark</span>
    </nav>

    <div v-if="invalid" class="benchmark-view__invalid card">
      <p>Giải thuật không tồn tại trong danh mục.</p>
      <Button variant="secondary" @click="router.push({ name: 'simulations' })">
        Về danh mục mô phỏng
      </Button>
    </div>

    <BenchmarkPanel v-else :default-keys="defaultKeys" />
  </main>
</template>

<style scoped>
.benchmark-view {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.benchmark-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.benchmark-view__invalid {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  align-items: flex-start;
}
</style>
