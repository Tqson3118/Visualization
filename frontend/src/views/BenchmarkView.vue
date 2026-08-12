<script setup lang="ts">
// BenchmarkView — Màn 17: route /benchmark/:k1/:k2 (MIỄN PHÍ tim — 20.4)
// G-F2c: chrome header Cyber Mint nhẹ + breadcrumb; giữ logic/route + BenchmarkPanel.
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { FlaskConical } from 'lucide-vue-next';

import BenchmarkPanel from '@/components/benchmark/BenchmarkPanel.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
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

const algoNames = computed(() =>
  defaultKeys.value.map((key) => getCatalogMeta(key)?.title ?? key).join(' · '),
);
</script>

<template>
  <main class="benchmark-view container">
    <header class="benchmark-view__chrome">
      <nav class="benchmark-view__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
        <span aria-hidden="true">/</span>
        <span>Benchmark</span>
      </nav>
      <div class="benchmark-view__hero">
        <span class="benchmark-view__icon" aria-hidden="true">
          <FlaskConical :size="22" />
        </span>
        <div>
          <h1 class="benchmark-view__title">Benchmark Lab</h1>
          <p class="benchmark-view__sub">
            So sánh thời gian thực tế của {{ algoNames || 'các giải thuật' }} theo độ lớn n — đo trong Web Worker, miễn phí tim.
          </p>
        </div>
        <Badge variant="success" class="benchmark-view__badge">Miễn phí tim</Badge>
      </div>
    </header>

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

/* Chrome header — Cyber Mint nhẹ (palette 3) */
.benchmark-view__chrome {
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
  gap: var(--space-sm);
}

.benchmark-view__chrome::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 62%, transparent);
}

.benchmark-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.benchmark-view__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.benchmark-view__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.benchmark-view__icon {
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

.benchmark-view__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-mint);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.benchmark-view__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 64ch; margin-top: 2px; }

.benchmark-view__badge { margin-left: auto; }

.benchmark-view__invalid {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  align-items: flex-start;
}
</style>
