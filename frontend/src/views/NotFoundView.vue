<script setup lang="ts">
import { RouterLink } from 'vue-router';

import { messages } from '@/i18n/vi';
</script>

<template>
  <main class="not-found">
    <p class="not-found__code" aria-hidden="true">404</p>
    <h1>{{ messages.notFound.title }}</h1>
    <p class="text-muted">{{ messages.notFound.desc }}</p>
    <RouterLink :to="{ name: 'home' }" class="btn btn-primary mt-lg">
      {{ messages.notFound.backHome }}
    </RouterLink>
  </main>
</template>

<style scoped>
.not-found {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  text-align: center;
  padding: var(--space-lg);
}

.not-found__code {
  font-size: var(--text-4xl);
  font-weight: 700;
  color: var(--color-primary);
}
</style>
