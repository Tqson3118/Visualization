<script setup lang="ts">
import { Map, Play, Target } from 'lucide-vue-next';
import { messages } from '@/i18n/vi';
</script>

<template>
  <section class="home__section container">
    <div class="home__section-head">
      <span class="home__kicker home__kicker--center">
        <span class="home__kicker-dot" aria-hidden="true" />
        <span class="font-mono">{{ messages.home.featuresKicker }}</span>
      </span>
      <h2 class="home__section-title">{{ messages.home.ctaExplore }}</h2>
    </div>

    <div class="home__grid home__grid--features">
      <div class="home__feature home__feature--featured home__feature--visual glass-card">
        <div class="home__feature-icon" aria-hidden="true">
          <Play :size="22" />
        </div>
        <h3 class="home__feature-title">{{ messages.home.featureVisual.title }}</h3>
        <p class="home__feature-desc">{{ messages.home.featureVisual.desc }}</p>
      </div>

      <div class="home__feature home__feature--featured home__feature--path glass-card">
        <div class="home__feature-icon" aria-hidden="true">
          <Map :size="22" />
        </div>
        <h3 class="home__feature-title">{{ messages.home.featurePath.title }}</h3>
        <p class="home__feature-desc">{{ messages.home.featurePath.desc }}</p>
      </div>

      <div class="home__feature home__feature--compact glass-card">
        <div class="home__feature-icon" aria-hidden="true">
          <Target :size="20" />
        </div>
        <div>
          <h3 class="home__feature-title">{{ messages.home.featurePractice.title }}</h3>
          <p class="home__feature-desc">{{ messages.home.featurePractice.desc }}</p>
        </div>
      </div>
    </div>
  </section>
</template>

<style scoped>
.home__section {
  position: relative;
  z-index: 1;
  display: flex;
  flex-direction: column;
  gap: var(--space-lg, 24px);
}

.home__section-head {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm, 8px);
}

.home__kicker {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm, 8px);
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.38);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.home__kicker--center { justify-content: center; }

.home__kicker-dot {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-full);
  background: #a855f7;
  box-shadow: 0 0 10px rgba(168, 85, 247, 0.25);
}

.home__section-title {
  margin: 0;
  font-size: var(--text-3xl);
  font-weight: 700;
  letter-spacing: -0.025em;
  color: #fff;
}

.home__grid--features {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: var(--space-lg, 24px);
}

.glass-card {
  background: rgba(13, 12, 20, 0.72);
  -webkit-backdrop-filter: blur(16px) saturate(1.3);
  backdrop-filter: blur(16px) saturate(1.3);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: var(--radius-xl, 16px);
  box-shadow: 0 10px 34px rgba(0, 0, 0, 0.25), inset 0 1px 0 rgba(255, 255, 255, 0.03);
  transition: transform 260ms cubic-bezier(0.34, 1.56, 0.64, 1), border-color 200ms ease, box-shadow 260ms ease;
}

.glass-card:hover {
  transform: translateY(-4px);
  border-color: rgba(168, 85, 247, 0.4);
  box-shadow: 0 16px 44px rgba(0, 0, 0, 0.35), 0 0 24px rgba(168, 85, 247, 0.1);
}

.home__feature {
  display: flex;
  flex-direction: column;
  gap: 10px;
  padding: var(--space-xl, 24px);
}

.home__feature--visual { grid-column: span 8; }
.home__feature--path { grid-column: span 4; }
.home__feature--compact { grid-column: 1 / -1; flex-direction: row; align-items: center; gap: var(--space-lg, 24px); }

.home__feature-icon {
  width: 46px;
  height: 46px;
  border-radius: var(--radius-lg, 12px);
  background: rgba(168, 85, 247, 0.12);
  border: 1px solid rgba(168, 85, 247, 0.3);
  color: #c084fc;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.home__feature-title {
  margin: 0;
  font-size: var(--text-md);
  font-weight: 700;
  color: #fff;
}

.home__feature-desc {
  margin: 0;
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.62);
  line-height: 1.6;
}

@media (max-width: 768px) {
  .home__feature--visual,
  .home__feature--path { grid-column: 1 / -1; }
  .home__feature--compact { flex-direction: column; align-items: flex-start; }
}
</style>
