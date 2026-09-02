import { createPinia, setActivePinia } from 'pinia';
import { mount } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { nextTick } from 'vue';

import HeartsGemsWidget from '@/components/simulator/HeartsGemsWidget.vue';
import { useGamificationStore } from '@/stores/gamification';
import * as gamificationApi from '@/api/gamification';

vi.mock('@/api/gamification', () => ({
  fetchHearts: vi.fn().mockResolvedValue({
    hearts: 10,
    heartsMax: 10,
    lastHeartAt: null,
    nextHeartAt: null,
  }),
  fetchGamificationSummary: vi.fn(),
  fetchQuests: vi.fn(),
  claimQuest: vi.fn(),
  fetchInventory: vi.fn(),
  buyItem: vi.fn(),
  equipItem: vi.fn(),
  fetchAchievements: vi.fn(),
  fetchStreak: vi.fn().mockResolvedValue({ streakDays: 0, freezeAvailable: 0 }),
  fetchPremiumStatus: vi.fn().mockResolvedValue({ isPremium: false, plan: null, expiresAt: null }),
  spendHeart: vi.fn(),
  enterNode: vi.fn(),
}));

const globalStubs = {
  RouterLink: {
    props: ['to'],
    template: '<a :href="typeof to === \'string\' ? to : to?.path || to?.name"><slot /></a>',
  },
  Tooltip: {
    props: ['text'],
    template: '<div class="tooltip-stub" :data-tooltip="text"><slot /></div>',
  },
  BaseIcon: {
    props: ['name'],
    template: '<span class="icon-stub" :data-name="name" />',
  },
};

describe('HeartsGemsWidget — Adversarial & Boundary Tests', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  afterEach(() => {
    document.body.innerHTML = '';
  });

  describe('Boundary 1: Extreme Values & Malformed Data', () => {
    it('ADV-WGT-001: Số gems cực lớn (999,999,999) không làm crash hoặc lỗi render', async () => {
      const store = useGamificationStore();
      store.gems = 999999999;

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      const gemsChip = wrapper.find('.hearts-gems__chip[aria-label="Đá quý"]');
      expect(gemsChip.exists()).toBe(true);
      expect(gemsChip.text()).toContain('999999999');
    });

    it('ADV-WGT-002: lastHeartAt là chuỗi ngày tháng không hợp lệ ("invalid-date") không ném exception', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 5,
        heartsMax: 10,
        lastHeartAt: 'invalid-date-format',
        nextHeartAt: null,
      });

      expect(() => {
        mount(HeartsGemsWidget, {
          global: { stubs: globalStubs },
        });
      }).not.toThrow();
    });

    it('ADV-WGT-003: Số tim vượt quá heartsMax (15/10) không hiển thị countdown âm', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 15,
        heartsMax: 10,
        lastHeartAt: new Date().toISOString(),
        nextHeartAt: null,
      });

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      const tooltip = wrapper.find('.tooltip-stub');
      expect(tooltip.attributes('data-tooltip')).toBe('Tim đầy');
    });

    it('ADV-WGT-004: API fetchHearts() bị lỗi 500 mạng không làm sập widget', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockRejectedValue(new Error('500 Internal Server Error'));

      expect(() => {
        mount(HeartsGemsWidget, {
          global: { stubs: globalStubs },
        });
      }).not.toThrow();
    });
  });

  describe('Boundary 2: Popover Escape Key & Outside Click Behavior', () => {
    it('ADV-WGT-005: Nhấn phím Escape khi popover tim đang mở phải đóng popover', async () => {
      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
        attachTo: document.body,
      });
      await nextTick();

      // Mở popover
      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();
      expect(document.querySelector('.hearts-gems__pop')).not.toBeNull();

      // Nhấn Escape
      window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
      await nextTick();

      // Popover phải đóng lại
      expect(document.querySelector('.hearts-gems__pop')).toBeNull();
      wrapper.unmount();
    });
  });
});
