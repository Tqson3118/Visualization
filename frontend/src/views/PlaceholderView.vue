<script setup lang="ts">
import { RouterLink } from 'vue-router';

import { messages } from '@/i18n/vi';

// View tạm cho các route chưa triển khai (SDD §3.1 — sẽ thay bằng view thật ở task sau)
</script>

<template>
  <main class="placeholder">
    <h1>{{ messages.placeholder.title }}</h1>
    <p class="text-muted">{{ messages.placeholder.desc }}</p>
    <RouterLink :to="{ name: 'home' }" class="btn btn-secondary mt-lg">
      {{ messages.notFound.backHome }}
    </RouterLink>
  </main>
</template>

<style scoped>
.placeholder {
  min-height: 60vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  text-align: center;
  padding: var(--space-lg);
}
</style>
