<script setup lang="ts">
// BenchmarkView — Màn 17: route /benchmark/:k1/:k2 (KHÔNG tốn tim — SDD 20.4)
// P1-B3: chrome surface band level-2 (bỏ gradient-mint/shadow/::after + text-gradient),
// kicker mono + H1 text-3xl; badge "Không tốn tim" (bỏ số mục SDD 20.4 lộ UI);
// invalid state bỏ class .card (shadow-md) → panel level-1.
// Giữ logic/route + BenchmarkPanel (đã chuẩn hoá trong task này).
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

const keysLabel = computed(() => defaultKeys.value.join(' vs '));

const invalid = computed(() =>
  defaultKeys.value.some((key) => !getCatalogMeta(key)),
);

const algoNames = computed(() =>
  defaultKeys.value.map((key) => getCatalogMeta(key)?.title ?? key).join(' · '),
);
</script>

<template>
  <main class="benchmark-view container">
    <!-- Chrome header — surface band level-2 + kicker mono (DESIGN.md §1, không gradient) -->
    <header class="benchmark-view__chrome">
      <nav class="benchmark-view__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
        <span aria-hidden="true">/</span>
        <span>Benchmark</span>
      </nav>
      <p class="benchmark-view__kicker">
        Benchmark · <span class="font-mono">{{ keysLabel || 'so sánh' }}</span>
      </p>
      <div class="benchmark-view__hero">
        <h1 class="benchmark-view__title">
          <FlaskConical :size="28" aria-hidden="true" class="benchmark-view__title-icon" />
          Benchmark Lab
        </h1>
        <p class="benchmark-view__sub">
          So sánh thời gian thực tế của {{ algoNames || 'các giải thuật' }} theo độ lớn n — đo trong Web Worker, không tốn tim.
        </p>
        <Badge variant="success" class="benchmark-view__badge">Không tốn tim</Badge>
      </div>
    </header>

    <div v-if="invalid" class="benchmark-view__invalid">
      <p class="benchmark-view__invalid-text">
        Giải thuật không tồn tại trong danh mục — kiểm tra lại đường dẫn hoặc chọn từ danh mục mô phỏng.
      </p>
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

/* Chrome header — surface band level-2 (DESIGN.md §1 + §6, không gradient/shadow) */
.benchmark-view__chrome {
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.benchmark-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.benchmark-view__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.benchmark-view__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
}

.benchmark-view__hero {
  display: flex;
  align-items: flex-start;
  flex-direction: column;
  gap: var(--space-xs);
}

.benchmark-view__title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.02em;
  line-height: 1.15;
  margin: 0;
}

.benchmark-view__title-icon { color: var(--color-text-tertiary); flex-shrink: 0; }

.benchmark-view__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  max-width: 64ch;
  margin: 0;
}

.benchmark-view__badge { align-self: flex-start; }

.benchmark-view__invalid {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  align-items: flex-start;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
}

.benchmark-view__invalid-text { font-size: var(--text-sm); color: var(--color-text-secondary); margin: 0; }
</style>
