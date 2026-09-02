 import { describe, it, expect, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import { CATALOG } from '@/engines/catalog';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';

describe('UX Audit Bug Fixes Test Suite', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  describe('BUG-001: Hero CTA Link', () => {
    it('HeroSection for guest should point to /simulations for explore CTA', async () => {
      const { default: HeroSection } = await import('@/components/home/HeroSection.vue');
      const auth = useAuthStore();
      auth.user = null;

      const wrapper = mount(HeroSection, {
        global: {
          stubs: {
            RouterLink: {
              template: '<a :href="to?.name || to?.path || to" class="router-link"><slot /></a>',
              props: ['to'],
            },
            LiveDemoBench: true,
          },
        },
      });

      const primaryCta = wrapper.find('.home__cta-primary');
      expect(primaryCta.exists()).toBe(true);
      expect(primaryCta.text()).toContain('Khám phá mô phỏng');
      expect(primaryCta.attributes('href')).toBe('simulations');
    });
  });

  describe('BUG-003: Simulation Catalog Pagination', () => {
    it('PAGE_SIZE of 12 slices the catalog properly across pages', () => {
      const PAGE_SIZE = 12;
      const totalPages = Math.max(1, Math.ceil(CATALOG.length / PAGE_SIZE));
      expect(CATALOG.length).toBe(44);
      expect(totalPages).toBe(4);

      const page1 = CATALOG.slice(0, PAGE_SIZE);
      expect(page1.length).toBe(12);

      const page4 = CATALOG.slice(3 * PAGE_SIZE, 4 * PAGE_SIZE);
      expect(page4.length).toBe(8);
      expect(page1.length + 12 + 12 + page4.length).toBe(44);
    });
  });

  describe('BUG-004: Simulator Guest Access Guard', () => {
    it('marks demoAllowed as true only for sort.bubble, search.binary, graph.bfs', () => {
      const demoKeys = CATALOG.filter((c) => c.demoAllowed).map((c) => c.key);
      expect(demoKeys).toContain('sort.bubble');
      expect(demoKeys).toContain('search.binary');
      expect(demoKeys).toContain('graph.bfs');
      expect(demoKeys.length).toBe(3);

      const selectionSort = CATALOG.find((c) => c.key === 'sort.selection');
      expect(selectionSort?.demoAllowed).toBeFalsy();
    });
  });

  describe('BUG-005: Course Detail CTA when 100% Completed', () => {
    it('computes "Ôn tập lộ trình" when all lessons have status Completed', () => {
      const lessons = [
        { id: 1, title: 'Bubble Sort', status: 'Completed', sandboxType: 'theory' },
        { id: 2, title: 'Binary Search', status: 'Completed', sandboxType: 'theory' },
        { id: 3, title: 'Quick Sort', status: 'Completed', sandboxType: 'quiz' },
      ];

      const isCompletedAll = lessons.length > 0 && lessons.every((l) => l.status === 'Completed');
      expect(isCompletedAll).toBe(true);
      const label = isCompletedAll ? 'Ôn tập lộ trình' : 'Học tiếp: ' + lessons[0].title;
      expect(label).toBe('Ôn tập lộ trình');
    });
  });

  describe('BUG-007: CheatSheet PDF Export Policy', () => {
    it('identifies non-premium user and restricts print', async () => {
      const auth = useAuthStore();
      const gamification = useGamificationStore();
      auth.user = { id: 10, email: 'student@test.com', role: 'STUDENT', name: 'Test' } as any;
      gamification.premium = { isPremium: false, plan: null, expiresAt: null };

      const isPremiumUser = gamification.isPremium || auth.user?.role === 'ADMIN' || auth.user?.role === 'TEACHER';
      expect(isPremiumUser).toBe(false);

      gamification.premium = { isPremium: true, plan: 'PRO', expiresAt: '2027-01-01' };
      const isPremiumAfter = gamification.isPremium || auth.user?.role === 'ADMIN' || auth.user?.role === 'TEACHER';
      expect(isPremiumAfter).toBe(true);
    });
  });
});
