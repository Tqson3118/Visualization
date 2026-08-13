<script setup lang="ts">
// NotFoundView — 404 (route catch-all). H-E2: 404 gradient Aurora + Motion fade-up + decorative ring.
// GIỮ NGUYÊN messages.notFound + nút về trang chủ (RouterLink name=home).
import { RouterLink } from 'vue-router';
import { Motion } from 'motion-v';
import { Compass } from 'lucide-vue-next';

import { messages } from '@/i18n/vi';
</script>

<template>
  <main class="not-found">
    <Motion
      class="not-found__inner"
      :initial="{ opacity: 0, y: 16 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.4, ease: 'easeOut' }"
    >
      <div class="not-found__ring" aria-hidden="true">
        <span class="not-found__code">404</span>
      </div>
      <span class="not-found__icon" aria-hidden="true">
        <Compass :size="26" />
      </span>
      <h1 class="not-found__heading">{{ messages.notFound.title }}</h1>
      <p class="not-found__desc text-muted">{{ messages.notFound.desc }}</p>
      <RouterLink :to="{ name: 'home' }" class="btn btn-primary hover-glow mt-lg">
        {{ messages.notFound.backHome }}
      </RouterLink>
    </Motion>
  </main>
</template>

<style scoped>
.not-found {
  min-height: calc(100vh - 160px);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  padding: var(--space-2xl) var(--space-lg);
}

.not-found__inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm);
  max-width: 480px;
}

/* Vòng trang trí 404 — gradient Aurora, không chặn tương tác */
.not-found__ring {
  position: relative;
  width: 168px;
  height: 168px;
  border-radius: 50%;
  background-image: var(--gradient-aurora);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: var(--shadow-xl);
  margin-bottom: var(--space-sm);
}

.not-found__ring::before {
  content: '';
  position: absolute;
  inset: 10px;
  border-radius: 50%;
  border: 2px dashed rgba(255, 255, 255, 0.5);
}

.not-found__code {
  font-size: clamp(3.5rem, 10vw, 5rem);
  font-weight: 800;
  color: #fff;
  line-height: 1;
  letter-spacing: 0.04em;
  font-variant-numeric: tabular-nums;
}

.not-found__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background: var(--color-muted);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-top: var(--space-sm);
}

.not-found__heading {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.not-found__desc {
  font-size: var(--text-sm);
  max-width: 44ch;
  line-height: 1.7;
}
</style>
