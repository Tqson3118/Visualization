<script setup lang="ts">
// NotFoundView — 404 (route catch-all).
// View-quality (nhóm A): bỏ vòng gradient aurora + icon compass + shadow-xl + heading gradient;
// thay bằng motif Data Bench "mảng mất index" — panel tối canvas-ink, block 4·0·4 + block 03
// dashed conflict (out of bounds) + index mono, CTA qua buttonVariants, Motion easing chuẩn.
// GIỮ NGUYÊN messages.notFound + nút về trang chủ (RouterLink name=home).
import { RouterLink } from 'vue-router';
import { Motion } from 'motion-v';

import { cn } from '@/lib/utils';
import { buttonVariants } from '@/components/ui/button';
import { messages } from '@/i18n/vi';

/**
 * Strip block-token trang trí (aria-hidden) — giá trị block đánh vần "404", index 03 out of bounds.
 * UI-PREMIUM 1D: mỗi block khởi đầu "scattered" (translate/rotate/scale lệch) rồi
 * reassemble về vị trí thật theo stagger — signature "dữ liệu lên sân khấu tối".
 */
const BENCH_BLOCKS = [
  { value: '4', missing: false, dx: -56, dy: 26, rot: -14 },
  { value: '0', missing: false, dx: 0, dy: -34, rot: 10 },
  { value: '4', missing: false, dx: 58, dy: 30, rot: -8 },
  { value: '?', missing: true, dx: 4, dy: 40, rot: 16 },
] as const;
</script>

<template>
  <main class="nf">
    <Motion
      class="nf__inner"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <!-- Panel dữ liệu — LUÔN tối bất kể theme (quyết định xuyên-nhóm 5) -->
      <div class="nf__panel" aria-hidden="true">
        <div class="nf__blocks">
          <div
            v-for="(b, idx) in BENCH_BLOCKS"
            :key="idx"
            class="nf__block"
            :class="{ 'nf__block--missing': b.missing }"
            :style="{ '--i': idx, '--tx': `${b.dx}px`, '--ty': `${b.dy}px`, '--rot': `${b.rot}deg` }"
          >
            <span class="nf__value">{{ b.value }}</span>
            <span class="nf__index">{{ String(idx).padStart(2, '0') }}</span>
          </div>
        </div>
        <p class="nf__label">404 · index 03 — out of bounds</p>
      </div>

      <h1 class="nf__title">{{ messages.notFound.title }}</h1>
      <p class="nf__desc">{{ messages.notFound.desc }}</p>
      <RouterLink :to="{ name: 'home' }" :class="cn(buttonVariants({ variant: 'default', size: 'lg' }), 'nf__cta')">
        {{ messages.notFound.backHome }}
      </RouterLink>
    </Motion>
  </main>
</template>

<style scoped>
.nf {
  min-height: calc(100dvh - 120px);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

.nf__inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
  text-align: center;
  max-width: 520px;
}

/* ── Panel dữ liệu — motif "mảng mất index" (Data Bench, §1) ── */
.nf__panel {
  width: 100%;
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
}

.nf__blocks {
  display: flex;
  gap: var(--space-sm);
}

.nf__block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  width: 48px;
  padding: var(--space-sm) 0;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
  /* UI-PREMIUM 1D: scattered ban đầu → reassemble về vị trí thật (stagger) */
  opacity: 0;
  transform: translate(var(--tx), var(--ty)) rotate(var(--rot)) scale(0.7);
  animation: nf-reassemble 600ms var(--ease-out-expo) forwards;
  animation-delay: calc(140ms + var(--i) * 130ms);
}

@keyframes nf-reassemble {
  to {
    opacity: 1;
    transform: translate(0, 0) rotate(0) scale(1);
  }
}

.nf__block--missing {
  background: color-mix(in srgb, var(--color-conflict) 10%, transparent);
  border: 1px dashed var(--color-conflict);
  animation-delay: calc(140ms + var(--i) * 130ms + 120ms);
}

@media (prefers-reduced-motion: reduce) {
  .nf__block {
    opacity: 1;
    transform: none;
    animation: none;
  }
}

.nf__value {
  font-family: var(--font-mono);
  font-size: var(--text-lg);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.3;
}

.nf__block--missing .nf__value {
  color: var(--color-conflict);
}

.nf__index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

.nf__label {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-index-muted);
  margin: 0;
}

/* ── Typography (§3): H1 48px/600/-0.03em, không gradient ── */
.nf__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: var(--space-xs) 0 0;
}

.nf__desc {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  max-width: 44ch;
  line-height: 1.7;
  margin: 0;
}

.nf__cta {
  margin-top: var(--space-sm);
}
</style>
