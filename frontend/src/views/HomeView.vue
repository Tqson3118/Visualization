<script setup lang="ts">
import { RouterLink } from 'vue-router';

import { messages } from '@/i18n/vi';
</script>

<template>
  <main class="home">
    <header class="home__nav container">
      <span class="home__brand">{{ messages.app.name }}</span>
      <nav class="home__links">
        <RouterLink :to="{ name: 'login' }">{{ messages.nav.login }}</RouterLink>
        <RouterLink :to="{ name: 'register' }" class="btn btn-primary btn--sm">
          {{ messages.nav.register }}
        </RouterLink>
      </nav>
    </header>

    <section class="home__hero container">
      <h1 class="home__title">{{ messages.home.heroTitle }}</h1>
      <p class="home__subtitle">{{ messages.home.heroSubtitle }}</p>
      <div class="home__cta">
        <RouterLink :to="{ name: 'simulations' }" class="btn btn-primary">
          {{ messages.home.ctaExplore }}
        </RouterLink>
        <RouterLink :to="{ name: 'register' }" class="btn btn-secondary">
          {{ messages.home.ctaStart }}
        </RouterLink>
      </div>
    </section>

    <section class="home__features container">
      <article class="card">
        <h2>{{ messages.home.featureVisual.title }}</h2>
        <p class="text-muted">{{ messages.home.featureVisual.desc }}</p>
      </article>
      <article class="card">
        <h2>{{ messages.home.featurePath.title }}</h2>
        <p class="text-muted">{{ messages.home.featurePath.desc }}</p>
      </article>
      <article class="card">
        <h2>{{ messages.home.featurePractice.title }}</h2>
        <p class="text-muted">{{ messages.home.featurePractice.desc }}</p>
      </article>
    </section>
  </main>
</template>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: var(--space-3xl);
  padding-bottom: var(--space-3xl);
}

.home__nav {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-block: var(--space-lg);
}

.home__brand {
  font-weight: 700;
  font-size: var(--text-lg);
  color: var(--color-primary);
}

.home__links {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.btn--sm {
  padding: 0.5rem var(--space-md);
  font-size: var(--text-sm);
}

.home__hero {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-lg);
  padding-block: var(--space-2xl);
}

.home__title {
  max-width: 20ch;
  font-size: var(--text-3xl);
}

.home__subtitle {
  max-width: 52ch;
  color: var(--color-text-muted);
  font-size: var(--text-md);
}

.home__cta {
  display: flex;
  gap: var(--space-md);
  flex-wrap: wrap;
  justify-content: center;
}

.home__features {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: var(--space-lg);
}
</style>
