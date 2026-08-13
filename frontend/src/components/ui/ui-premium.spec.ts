import { flushPromises, mount } from '@vue/test-utils';
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest';
import type { Component } from 'vue';

// ── Stub browser API jsdom thiếu (jsdom 29 không có matchMedia/IntersectionObserver/rAF):
// bật prefers-reduced-motion = true → AnimatedNumber/ProgressRing/RevealSection hiển thị
// thẳng giá trị cuối (deterministic, không phụ thuộc animation).
beforeAll(() => {
  vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query: string) => ({
    matches: true, // reduced-motion ON → skip animation
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
      // Fire ngay với isIntersecting=true → reveal/animate chạy once rồi disconnect.
      this.cb([{ isIntersecting: true } as IntersectionObserverEntry], this as unknown as IntersectionObserver);
    };
    disconnect = vi.fn();
    unobserve = vi.fn();
  });
  // rAF no-op (KHÔNG gọi cb): motion-v frameloop không bị lặp vô hạn trong jsdom;
  // reduced-motion=true nên các component không cần rAF thật.
  vi.stubGlobal('requestAnimationFrame', (() => 0) as typeof requestAnimationFrame);
  vi.stubGlobal('cancelAnimationFrame', (() => 0) as typeof cancelAnimationFrame);
});

afterAll(() => {
  vi.unstubAllGlobals();
});

// Import động SAU khi stub global (component đọc matchMedia ở module scope lúc import).
let BlockToken: Component;
let CardPremium: Component;
let AnimatedNumber: Component;
let ProgressRing: Component;
let RevealSection: Component;

beforeAll(async () => {
  BlockToken = (await import('./BlockToken.vue')).default;
  CardPremium = (await import('./CardPremium.vue')).default;
  AnimatedNumber = (await import('./AnimatedNumber.vue')).default;
  ProgressRing = (await import('./ProgressRing.vue')).default;
  RevealSection = (await import('./RevealSection.vue')).default;
});

describe('BlockToken — block-token Data Bench (UI-PREMIUM)', () => {
  it('render value + label + index mono', () => {
    const wrapper = mount(BlockToken, { props: { value: 42, label: 'XP', index: '01' } });
    expect(wrapper.text()).toContain('42');
    expect(wrapper.text()).toContain('XP');
    expect(wrapper.text()).toContain('01');
    expect(wrapper.find('.ui-blocktoken').exists()).toBe(true);
  });

  it('size sm/md/lg → class min-width khác nhau', () => {
    const sm = mount(BlockToken, { props: { value: 1, size: 'sm' } });
    const lg = mount(BlockToken, { props: { value: 1, size: 'lg' } });
    expect(sm.find('.ui-blocktoken').classes().join(' ')).toContain('min-w-16');
    expect(lg.find('.ui-blocktoken').classes().join(' ')).toContain('min-w-36');
  });

  it('tone resolved + glow → class glow-resolved', () => {
    const wrapper = mount(BlockToken, { props: { value: 7, tone: 'resolved', glow: true } });
    expect(wrapper.find('.ui-blocktoken').classes().join(' ')).toContain('ui-blocktoken--glow-resolved');
  });

  it('glow + pulse → class modifier', () => {
    const wrapper = mount(BlockToken, { props: { value: 3, glow: true, pulse: true } });
    const cls = wrapper.find('.ui-blocktoken').classes().join(' ');
    expect(cls).toContain('ui-blocktoken--glow');
    expect(cls).toContain('ui-blocktoken--pulse');
  });
});

describe('CardPremium — card micro-feedback (UI-PREMIUM)', () => {
  it('render title + description + content slot', () => {
    const wrapper = mount(CardPremium, {
      props: { title: 'Bubble Sort', description: 'Mảng · Cấp độ basic' },
      slots: { default: '<p class="test-body">nội dung</p>' },
    });
    expect(wrapper.text()).toContain('Bubble Sort');
    expect(wrapper.text()).toContain('Mảng · Cấp độ basic');
    expect(wrapper.find('.test-body').exists()).toBe(true);
  });

  it('variant interactive → class interactive', () => {
    const wrapper = mount(CardPremium, { props: { variant: 'interactive', title: 'X' } });
    expect(wrapper.find('.ui-cardpremium').classes().join(' ')).toContain('ui-cardpremium--interactive');
  });

  it('glow resolved → class glow-resolved (khớp CSS)', () => {
    const wrapper = mount(CardPremium, { props: { glow: 'resolved', title: 'X' } });
    expect(wrapper.find('.ui-cardpremium').classes().join(' ')).toContain('ui-cardpremium--glow-resolved');
  });
});

describe('AnimatedNumber — count-up (UI-PREMIUM)', () => {
  it('reduced-motion → hiển thị ngay giá trị đích', async () => {
    const wrapper = mount(AnimatedNumber, { props: { value: 44, suffix: '+' } });
    await flushPromises();
    expect(wrapper.text()).toContain('44');
    expect(wrapper.text()).toContain('+');
  });

  it('decimals = 1 → format thập phân', async () => {
    const wrapper = mount(AnimatedNumber, { props: { value: 3.14, decimals: 1 } });
    await flushPromises();
    expect(wrapper.text()).toContain('3,1');
  });
});

describe('ProgressRing — vòng tiến trình SVG (UI-PREMIUM)', () => {
  it('render SVG + aria-valuenow đúng', async () => {
    const wrapper = mount(ProgressRing, { props: { progress: 50 } });
    await flushPromises();
    const svg = wrapper.find('svg');
    expect(svg.attributes('role')).toBe('progressbar');
    expect(svg.attributes('aria-valuenow')).toBe('50');
    expect(svg.findAll('circle').length).toBe(2);
  });

  it('transform rotate dùng tâm theo size (fix dev-review MAJOR)', async () => {
    const wrapper = mount(ProgressRing, { props: { progress: 25, size: 64 } });
    await flushPromises();
    const bar = wrapper.findAll('circle')[1];
    expect(bar.attributes('transform')).toBe('rotate(-90 32 32)');
    const small = mount(ProgressRing, { props: { progress: 25, size: 44 } });
    await flushPromises();
    expect(small.findAll('circle')[1].attributes('transform')).toBe('rotate(-90 22 22)');
  });

  it('stroke-dashoffset giảm theo progress', async () => {
    const full = mount(ProgressRing, { props: { progress: 100, size: 64, strokeWidth: 8 } });
    await flushPromises();
    const zero = mount(ProgressRing, { props: { progress: 0, size: 64, strokeWidth: 8 } });
    await flushPromises();
    const offsetFull = Number(full.findAll('circle')[1].attributes('stroke-dashoffset'));
    const offsetZero = Number(zero.findAll('circle')[1].attributes('stroke-dashoffset'));
    expect(offsetFull).toBeLessThan(offsetZero);
  });
});

describe('RevealSection — scroll reveal wrapper (UI-PREMIUM)', () => {
  it('reduced-motion → visible ngay, render slot', async () => {
    const wrapper = mount(RevealSection, {
      slots: { default: '<p class="reveal-body">nội dung reveal</p>' },
    });
    await flushPromises();
    expect(wrapper.find('.reveal-body').exists()).toBe(true);
    expect(wrapper.find('.reveal-body').text()).toBe('nội dung reveal');
  });

  it('immediate = true → render slot ngay', async () => {
    const wrapper = mount(RevealSection, {
      props: { immediate: true, delay: 100 },
      slots: { default: '<span>X</span>' },
    });
    await flushPromises();
    expect(wrapper.text()).toContain('X');
  });
});
