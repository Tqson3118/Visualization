<script setup lang="ts">
import { computed } from 'vue';
import { Gauge, Layers } from 'lucide-vue-next';
import { CATALOG } from '@/engines/catalog';
import { messages } from '@/i18n/vi';
import BlockToken from '@/components/ui/BlockToken.vue';

const stats = computed(() => ({
  visuals: CATALOG.length,
  groups: new Set(CATALOG.map((c) => c.dataStructure)).size,
  levels: new Set(CATALOG.map((c) => c.level)).size,
}));
</script>

<template>
  <section class="home__stats container" aria-label="Thống kê">
    <BlockToken
      class="home__stat-hero"
      :label="messages.home.statsVisuals"
      :value="`${stats.visuals}+`"
      :aria-label="`${stats.visuals}+ ${messages.home.statsVisuals}`"
    />
    <div class="home__stat">
      <Layers class="home__stat-icon" :size="16" aria-hidden="true" />
      <span class="home__stat-value">{{ stats.groups }}</span>
      <span class="home__stat-label">{{ messages.home.statsGroups }}</span>
    </div>
    <div class="home__stat">
      <Gauge class="home__stat-icon" :size="16" aria-hidden="true" />
      <span class="home__stat-value">{{ stats.levels }}</span>
      <span class="home__stat-label">{{ messages.home.statsLevels }}</span>
    </div>
    <div class="home__stat">
      <span class="home__stat-icon home__stat-icon--mono" aria-hidden="true">100%</span>
      <span class="home__stat-value">100%</span>
      <span class="home__stat-label">{{ messages.home.statsViet }}</span>
    </div>
  </section>
</template>

<style scoped>
.home__stats {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-md, 16px);
  background: rgba(13, 12, 20, 0.72);
  -webkit-backdrop-filter: blur(16px) saturate(1.3);
  backdrop-filter: blur(16px) saturate(1.3);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: var(--radius-xl, 16px);
  padding: var(--space-lg, 24px);
}

.home__stat-hero { width: 100%; }

.home__stat {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-xs, 4px);
  padding: var(--space-sm, 8px) var(--space-md, 16px);
}

.home__stat-icon {
  color: #c084fc;
}

.home__stat-icon--mono {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 700;
}

.home__stat-value {
  font-family: var(--font-mono);
  font-size: var(--text-2xl);
  font-weight: 700;
  letter-spacing: -0.015em;
  color: #fff;
}

.home__stat-label {
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.38);
}

@media (max-width: 768px) {
  .home__stats { grid-template-columns: 1fr 1fr; }
}

@media (max-width: 480px) {
  .home__stats { grid-template-columns: 1fr; }
}
</style>
