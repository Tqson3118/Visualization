<script setup lang="ts">
import { CheckCircle2, Sparkles, Target } from 'lucide-vue-next';
import { messages } from '@/i18n/vi';

const BRAND_POINTS = [
  { icon: Sparkles, text: messages.auth.brandPoint1 },
  { icon: Target, text: messages.auth.brandPoint2 },
  { icon: CheckCircle2, text: messages.auth.brandPoint3 },
] as const;

const BENCH_BLOCKS = [
  { value: '7', state: 'done' },
  { value: '3', state: 'swap' },
  { value: '8', state: 'active' },
  { value: '1', state: 'default' },
  { value: '9', state: 'default' },
  { value: '2', state: 'default' },
] as const;
</script>

<template>
  <aside class="register__aside" aria-label="Giới thiệu DSA Visual">
    <div class="register__aside-inner">
      <span class="register__aside-badge">{{ messages.app.name }}</span>
      <h2 class="register__aside-title">{{ messages.app.tagline }}</h2>

      <div class="register__aside-bench" aria-hidden="true">
        <div
          v-for="(b, idx) in BENCH_BLOCKS"
          :key="b.value"
          class="register__aside-block"
          :class="`register__aside-block--${b.state}`"
        >
          <span class="register__aside-block-value">{{ b.value }}</span>
          <span class="register__aside-block-index">{{ String(idx).padStart(2, '0') }}</span>
        </div>
      </div>

      <ul class="register__points">
        <li v-for="point in BRAND_POINTS" :key="point.text" class="register__point">
          <component :is="point.icon" :size="16" class="register__point-icon" aria-hidden="true" />
          <span>{{ point.text }}</span>
        </li>
      </ul>
    </div>
  </aside>
</template>

<style scoped>
.register__aside {
  background: var(--color-canvas-ink, #0d1020);
  border-radius: var(--radius-xl, 16px) 0 0 var(--radius-xl, 16px);
  padding: var(--space-2xl, 32px);
  display: flex;
  flex-direction: column;
  justify-content: center;
  border-right: 1px solid rgba(255, 255, 255, 0.08);
}

.register__aside-inner {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg, 24px);
}

.register__aside-badge {
  display: inline-block;
  align-self: flex-start;
  font-family: var(--font-mono, monospace);
  font-size: var(--text-xs, 12px);
  color: var(--color-primary-bright, #a855f7);
  background: rgba(168, 85, 247, 0.12);
  border: 1px solid rgba(168, 85, 247, 0.3);
  padding: 4px 10px;
  border-radius: var(--radius-full, 9999px);
  font-weight: 600;
}

.register__aside-title {
  margin: 0;
  font-size: var(--text-2xl, 24px);
  font-weight: 800;
  line-height: 1.3;
  color: #ffffff;
}

.register__aside-bench {
  display: flex;
  gap: 6px;
  padding: 12px;
  background: rgba(0, 0, 0, 0.35);
  border-radius: var(--radius-lg, 12px);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.register__aside-block {
  flex: 1;
  height: 48px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-md, 8px);
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.register__aside-block-value {
  font-family: var(--font-mono, monospace);
  font-size: var(--text-xs, 12px);
  font-weight: 700;
  color: #ffffff;
}

.register__aside-block-index {
  font-family: var(--font-mono, monospace);
  font-size: 9px;
  color: rgba(255, 255, 255, 0.35);
}

.register__aside-block--default { background: #181225; border-color: rgba(168, 85, 247, 0.35); }
.register__aside-block--active { background: #7c3aed; }
.register__aside-block--swap { background: #ef4444; }
.register__aside-block--done { background: #10b981; }

.register__points {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 16px);
}

.register__point {
  display: flex;
  align-items: center;
  gap: var(--space-sm, 8px);
  font-size: var(--text-sm, 13px);
  color: rgba(255, 255, 255, 0.7);
}

.register__point-icon {
  color: var(--color-primary-bright, #a855f7);
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .register__aside {
    border-radius: 0 0 var(--radius-xl, 16px) var(--radius-xl, 16px);
    border-right: none;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
  }
}
</style>
