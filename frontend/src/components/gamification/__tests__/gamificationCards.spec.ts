import { mount } from '@vue/test-utils';
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest';
import type { Component } from 'vue';

// jsdom 29 thiếu matchMedia/IntersectionObserver → stub để ui-kit render deterministic.
beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query: string) => ({
    matches: true,
    media: query,
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    addListener: vi.fn(),
    removeListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })));
  vi.stubGlobal('IntersectionObserver', class {
    private cb: IntersectionObserverCallback;
    constructor(cb: IntersectionObserverCallback) {
      this.cb = cb;
    }
    observe = () => {
      this.cb([{ isIntersecting: true } as IntersectionObserverEntry], this as unknown as IntersectionObserver);
    };
    disconnect = vi.fn();
    unobserve = vi.fn();
  });
});

afterAll(() => {
  vi.unstubAllGlobals();
});

let XpProgressCard: Component;
let StreakCard: Component;
let QuestProgressCard: Component;
let BadgeGrid: Component;

beforeAll(async () => {
  XpProgressCard = (await import('../XpProgressCard.vue')).default;
  StreakCard = (await import('../StreakCard.vue')).default;
  QuestProgressCard = (await import('../QuestProgressCard.vue')).default;
  BadgeGrid = (await import('../BadgeGrid.vue')).default;
});

describe('XpProgressCard', () => {
  it('loading → skeleton (aria-busy), không hiển thị số', () => {
    const wrapper = mount(XpProgressCard, {
      props: { level: 0, xp: 0, xpIntoLevel: 0, xpForNextLevel: 0, levelProgressPct: 0, loading: true },
    });
    expect(wrapper.find('[aria-busy="true"]').exists()).toBe(true);
  });

  it('XP = 0 → level 1, 0% tiến tới level kế (không crash)', () => {
    const wrapper = mount(XpProgressCard, {
      props: { level: 1, xp: 0, xpIntoLevel: 0, xpForNextLevel: 100, levelProgressPct: 0, loading: false },
    });
    expect(wrapper.text()).toContain('1');
    expect(wrapper.text()).toContain('0 / 100 XP');
  });

  it('XP gần level kế → hiển thị đúng into/need với phần trăm tính đúng', () => {
    const wrapper = mount(XpProgressCard, {
      props: { level: 4, xp: 999, xpIntoLevel: 99, xpForNextLevel: 700, levelProgressPct: 14, loading: false },
    });
    expect(wrapper.text()).toContain('4');
    expect(wrapper.text()).toContain('99 / 700 XP');
  });
});

describe('StreakCard', () => {
  it('streak > 0 → hiển thị số ngày + đông cứng', () => {
    const wrapper = mount(StreakCard, { props: { streakDays: 5, freezeAvailable: 2, loading: false } });
    expect(wrapper.text()).toContain('5');
    expect(wrapper.text()).toContain('ngày streak');
    expect(wrapper.text()).toContain('2 đông cứng');
  });

  it('loading → skeleton', () => {
    const wrapper = mount(StreakCard, { props: { streakDays: 0, freezeAvailable: 0, loading: true } });
    expect(wrapper.find('[aria-busy="true"]').exists()).toBe(true);
  });
});

describe('QuestProgressCard', () => {
  const quests = [
    { id: 1, title: 'Hoàn thành 1 bài học', description: '', target: 1, current: 1, rewardGems: 5, rewardXp: 20, claimed: true },
    { id: 2, title: 'Làm 3 quiz', description: '', target: 3, current: 1, rewardGems: 5, rewardXp: 20, claimed: false },
    { id: 3, title: 'Học 2 chủ đề', description: '', target: 2, current: 0, rewardGems: 5, rewardXp: 20, claimed: false },
  ];

  it('3 trạng thái có class khác nhau: done / progress / todo', () => {
    const wrapper = mount(QuestProgressCard, { props: { quests, loading: false } });
    const rows = wrapper.findAll('.quest-progress-card__row');
    expect(rows.length).toBe(3);
    expect(rows[0].classes()).toContain('quest-progress-card__row--done');
    expect(rows[1].classes()).toContain('quest-progress-card__row--progress');
    expect(rows[2].classes()).toContain('quest-progress-card__row--todo');
    expect(wrapper.text()).toContain('1 / 3');
  });

  it('quest claimed → badge đã nhận thưởng', () => {
    const wrapper = mount(QuestProgressCard, { props: { quests, loading: false } });
    expect(wrapper.text()).toContain('Đã nhận thưởng');
  });

  it('empty → empty state', () => {
    const wrapper = mount(QuestProgressCard, { props: { quests: [], loading: false, error: null } });
    expect(wrapper.text()).toContain('Chưa có nhiệm vụ hôm nay');
  });

  it('error → role=alert với thông báo', () => {
    const wrapper = mount(QuestProgressCard, { props: { quests: [], loading: false, error: 'Lỗi mạng' } });
    expect(wrapper.find('[role="alert"]').text()).toBe('Lỗi mạng');
  });

  it('loading → skeleton', () => {
    const wrapper = mount(QuestProgressCard, { props: { quests: [], loading: true } });
    expect(wrapper.find('[aria-busy="true"]').exists()).toBe(true);
  });
});

describe('BadgeGrid', () => {
  const badges = [
    { id: 1, code: 'first_lesson', name: 'Khởi đầu', description: 'Hoàn thành bài học đầu tiên', iconUrl: null, earnedAt: '2026-08-01T00:00:00Z' },
    { id: 2, code: 'streak_7', name: 'Kiên trì', description: '7 ngày liên tiếp', iconUrl: null, earnedAt: null },
  ];

  it('unlocked vs locked khác nhau bằng class + icon lock', () => {
    const wrapper = mount(BadgeGrid, { props: { badges, loading: false } });
    const items = wrapper.findAll('.badge-grid__item');
    expect(items.length).toBe(2);
    expect(items[0].classes()).toContain('badge-grid__item--unlocked');
    expect(items[1].classes()).toContain('badge-grid__item--locked');
    expect(wrapper.text()).toContain('1 / 2');
  });

  it('empty → empty state', () => {
    const wrapper = mount(BadgeGrid, { props: { badges: [], loading: false } });
    expect(wrapper.text()).toContain('Chưa có huy hiệu');
  });

  it('loading → skeleton', () => {
    const wrapper = mount(BadgeGrid, { props: { badges: [], loading: true } });
    expect(wrapper.find('[aria-busy="true"]').exists()).toBe(true);
  });
});