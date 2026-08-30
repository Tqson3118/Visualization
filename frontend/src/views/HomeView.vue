<script setup lang="ts">
// HomeView — Màn 01: trang chủ công khai (SDD Màn 01).
// Refactored to modular architecture: HeroSection, StatsBar, DemoGrid, FeatureGrid, HomeCtaBand.
import HeroSection from '@/components/home/HeroSection.vue';
import StatsBar from '@/components/home/StatsBar.vue';
import DemoGrid from '@/components/home/DemoGrid.vue';
import FeatureGrid from '@/components/home/FeatureGrid.vue';
import HomeCtaBand from '@/components/home/HomeCtaBand.vue';
</script>

<template>
  <section class="home">
    <HeroSection />
    <StatsBar />
    <DemoGrid />
    <FeatureGrid />
    <HomeCtaBand />
  </section>
</template>

<style scoped>
.home {
  --cos-bg: #0b0a12;
  --cos-bg-deep: #0d0c14;
  --cos-text: rgba(255, 255, 255, 0.9);

  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-2xl, 32px);
  padding-bottom: var(--space-3xl, 48px);
  margin-top: calc(-1 * var(--app-header-h, 68px));
  background: radial-gradient(ellipse 80% 50% at 50% -10%, rgba(168, 85, 247, 0.08), transparent 70%);
  color: var(--cos-text);
  overflow: hidden;
}

@media (max-width: 640px) {
  .home {
    gap: var(--space-xl, 24px);
    padding-bottom: var(--space-2xl, 32px);
  }
}
</style>
