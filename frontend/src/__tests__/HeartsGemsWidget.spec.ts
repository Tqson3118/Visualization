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

describe('HeartsGemsWidget — Module I, J, K', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
    vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
      hearts: 10,
      heartsMax: 10,
      lastHeartAt: null,
      nextHeartAt: null,
    });
  });

  afterEach(() => {
    document.body.innerHTML = '';
  });

  // ── Module I: Hiển thị số liệu ──
  describe('Module I: Hiển thị số liệu (U-HGW-001 ~ U-HGW-009)', () => {
    it('U-HGW-001: Hiển thị hearts label "7/10"', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 7,
        heartsMax: 10,
        lastHeartAt: null,
        nextHeartAt: null,
      });

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      const heartsBtn = wrapper.find('button.hearts-gems__chip');
      expect(heartsBtn.text()).toContain('7/10');
    });

    it('U-HGW-002: Hiển thị hearts label đầy "10/10"', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 10,
        heartsMax: 10,
        lastHeartAt: null,
        nextHeartAt: null,
      });

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      const heartsBtn = wrapper.find('button.hearts-gems__chip');
      expect(heartsBtn.text()).toContain('10/10');
      expect(heartsBtn.classes()).not.toContain('hearts-gems__chip--empty');
    });

    it('U-HGW-003: Hiển thị hearts = 0 có class --empty', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 0,
        heartsMax: 10,
        lastHeartAt: null,
        nextHeartAt: null,
      });

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      const heartsBtn = wrapper.find('button.hearts-gems__chip');
      expect(heartsBtn.text()).toContain('0/10');
      expect(heartsBtn.classes()).toContain('hearts-gems__chip--empty');
    });

    it('U-HGW-004: Hiển thị gems chính xác', async () => {
      const store = useGamificationStore();
      store.gems = 250;

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      const chips = wrapper.findAll('.hearts-gems__chip');
      const gemsChip = chips.find((c) => c.attributes('aria-label') === 'Đá quý');
      expect(gemsChip).toBeDefined();
      expect(gemsChip?.text()).toContain('250');
    });

    it('U-HGW-005: Hiển thị streak khi streakDays > 0', async () => {
      const store = useGamificationStore();
      store.streakDays = 5;

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      const streakChip = wrapper.find('.hearts-gems__chip[aria-label="Chuỗi ngày"]');
      expect(streakChip.exists()).toBe(true);
      expect(streakChip.text()).toContain('🔥 5');
    });

    it('U-HGW-006: Ẩn streak khi streakDays = 0', async () => {
      const store = useGamificationStore();
      store.streakDays = 0;

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      const streakChip = wrapper.find('.hearts-gems__chip[aria-label="Chuỗi ngày"]');
      expect(streakChip.exists()).toBe(false);
    });

    it('U-HGW-007: Hearts chip aria-label đúng', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 7,
        heartsMax: 10,
        lastHeartAt: null,
        nextHeartAt: null,
      });

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      const heartsBtn = wrapper.find('button.hearts-gems__chip');
      expect(heartsBtn.attributes('aria-label')).toBe('Tim: 7/10');
    });

    it('U-HGW-008: Gems chip aria-label đúng', async () => {
      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      const gemsChip = wrapper.find('.hearts-gems__chip[aria-label="Đá quý"]');
      expect(gemsChip.exists()).toBe(true);
      expect(gemsChip.attributes('title')).toBe('Đá quý');
    });

    it('U-HGW-009: Streak chip aria-label đúng', async () => {
      const store = useGamificationStore();
      store.streakDays = 5;

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      const streakChip = wrapper.find('.hearts-gems__chip[aria-label="Chuỗi ngày"]');
      expect(streakChip.exists()).toBe(true);
      expect(streakChip.attributes('aria-label')).toBe('Chuỗi ngày');
      expect(streakChip.attributes('title')).toBe('Chuỗi ngày');
    });
  });

  // ── Module J: Popover Tim ──
  describe('Module J: Popover Tim (U-POP-001 ~ U-POP-008)', () => {
    it('U-POP-001: Click hearts chip mở popover', async () => {
      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      expect(document.querySelector('.hearts-gems__pop')).toBeNull();

      const heartsBtn = wrapper.find('button.hearts-gems__chip');
      await heartsBtn.trigger('click');
      await nextTick();

      expect(document.querySelector('.hearts-gems__pop')).not.toBeNull();
    });

    it('U-POP-002: Popover title = "Tim của bạn"', async () => {
      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();

      const title = document.querySelector('.hearts-gems__pop-title');
      expect(title?.textContent).toBe('Tim của bạn');
    });

    it('U-POP-003: Popover hiện countdown khi tim chưa đầy và lastHeartAt được set', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 5,
        heartsMax: 10,
        lastHeartAt: new Date(Date.now() - 5 * 60 * 1000).toISOString(),
        nextHeartAt: null,
      });

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();

      const desc = document.querySelector('.hearts-gems__pop-desc');
      expect(desc).not.toBeNull();
      expect(desc?.textContent).toContain('Tim tiếp theo sau');
    });

    it('U-POP-004: Popover hiện link Premium nếu chưa Premium', async () => {
      const store = useGamificationStore();
      store.premium = { isPremium: false, plan: null, expiresAt: null };

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();

      const premiumLink = document.querySelector('.hearts-gems__pop-link');
      expect(premiumLink).not.toBeNull();
      expect(premiumLink?.textContent).toContain('Nâng cấp Premium');
    });

    it('U-POP-005: Popover ẩn link Premium nếu đã Premium', async () => {
      const store = useGamificationStore();
      store.premium = { isPremium: true, plan: 'PRO', expiresAt: '2026-12-31' };

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();

      const premiumLink = document.querySelector('.hearts-gems__pop-link');
      expect(premiumLink).toBeNull();
    });

    it('U-POP-006: Free user có regenMinutes = 30', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 5,
        heartsMax: 10,
        lastHeartAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
        nextHeartAt: null,
      });

      const store = useGamificationStore();
      store.premium = { isPremium: false, plan: null, expiresAt: null };

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();

      const desc = document.querySelector('.hearts-gems__pop-desc');
      expect(desc).not.toBeNull();
      expect(desc?.textContent).toContain('30 phút/tim');
      expect(desc?.textContent).toContain('Miễn phí');
    });

    it('U-POP-007: Premium user có regenMinutes = 10', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 5,
        heartsMax: 30,
        lastHeartAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
        nextHeartAt: null,
      });

      const store = useGamificationStore();
      store.premium = { isPremium: true, plan: 'PRO', expiresAt: '2026-12-31' };

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();

      const desc = document.querySelector('.hearts-gems__pop-desc');
      expect(desc).not.toBeNull();
      expect(desc?.textContent).toContain('10 phút/tim');
      expect(desc?.textContent).toContain('Cao cấp');
    });

    it('U-POP-008: Click "Đóng" đóng popover', async () => {
      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      await wrapper.find('button.hearts-gems__chip').trigger('click');
      await nextTick();
      expect(document.querySelector('.hearts-gems__pop')).not.toBeNull();

      const closeBtn = document.querySelector('.hearts-gems__pop-close') as HTMLButtonElement;
      closeBtn?.click();
      await nextTick();

      expect(document.querySelector('.hearts-gems__pop')).toBeNull();
    });
  });

  // ── Module K: Timer & Auto-refresh ──
  describe('Module K: Timer & Auto-refresh (U-TMR-001 ~ U-TMR-005)', () => {
    it('U-TMR-001: Mount gọi fetchHearts', async () => {
      const store = useGamificationStore();
      const spy = vi.spyOn(store, 'fetchHearts');

      mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      expect(spy).toHaveBeenCalledTimes(1);
    });

    it('U-TMR-002: Mount khởi tạo setInterval tick 1000ms', async () => {
      const spyInterval = vi.spyOn(globalThis, 'setInterval');

      mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      expect(spyInterval).toHaveBeenCalledWith(expect.any(Function), 1000);
    });

    it('U-TMR-003: Unmount dọn dẹp clearInterval', async () => {
      const spyClear = vi.spyOn(globalThis, 'clearInterval');

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();

      wrapper.unmount();
      expect(spyClear).toHaveBeenCalled();
    });

    it('U-TMR-004: Tooltip hiện "Tim đầy" khi hearts = heartsMax', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 10,
        heartsMax: 10,
        lastHeartAt: null,
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

    it('U-TMR-005: Tooltip hiện countdown khi hearts < heartsMax và lastHeartAt set', async () => {
      vi.mocked(gamificationApi.fetchHearts).mockResolvedValue({
        hearts: 7,
        heartsMax: 10,
        lastHeartAt: new Date(Date.now() - 2 * 60 * 1000).toISOString(),
        nextHeartAt: null,
      });

      const wrapper = mount(HeartsGemsWidget, {
        global: { stubs: globalStubs },
      });
      await nextTick();
      await nextTick();

      const tooltip = wrapper.find('.tooltip-stub');
      expect(tooltip.attributes('data-tooltip')).toContain('Tim hồi sau:');
    });
  });
});
