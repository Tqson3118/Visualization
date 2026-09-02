import { createPinia, setActivePinia } from 'pinia';
import { mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { nextTick, reactive } from 'vue';

const mockCurrentRoute = reactive({ fullPath: '/' });
const mockRouterReplace = vi.fn();
const mockRouterPush = vi.fn();

vi.mock('vue-router', () => ({
  useRoute: () => mockCurrentRoute,
  useRouter: () => ({
    replace: mockRouterReplace,
    push: mockRouterPush,
  }),
  RouterLink: {
    props: ['to'],
    template: '<a :data-to="JSON.stringify(to)"><slot /></a>',
  },
}));

vi.mock('@/api/gamification', () => ({
  fetchHearts: vi.fn().mockResolvedValue({ hearts: 10, heartsMax: 10, lastHeartAt: null, nextHeartAt: null }),
  fetchGamificationSummary: vi.fn(),
  fetchQuests: vi.fn(),
  claimQuest: vi.fn(),
  fetchInventory: vi.fn().mockResolvedValue([]),
  buyItem: vi.fn(),
  equipItem: vi.fn(),
  fetchAchievements: vi.fn(),
  fetchStreak: vi.fn().mockResolvedValue({ streakDays: 0, freezeAvailable: 0 }),
  fetchPremiumStatus: vi.fn().mockResolvedValue({ isPremium: false, plan: null, expiresAt: null }),
  spendHeart: vi.fn(),
  enterNode: vi.fn(),
}));

import AppHeader from '@/components/layout/AppHeader.vue';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';

describe('AppHeader — Adversarial & Extreme Boundary Tests', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
    mockCurrentRoute.fullPath = '/';
    window.scrollY = 0;
  });

  describe('Adversarial 1: Boundary & Malformed DisplayName / Avatar', () => {
    it('ADV-001: Chuỗi displayName rỗng "" không được để nút avatar trống', () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: '', email: 'u@test.edu', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const userBtn = wrapper.find('.app-header__user');
      const text = userBtn.text().trim();
      // Nếu displayName là "" thì charAt(0) ra "" dẫn đến span trống — cần hiển thị ký tự đại diện 'U' hoặc không rỗng
      expect(text).not.toBe('');
      expect(text).toBe('U');
    });

    it('ADV-002: Chuỗi displayName chỉ toàn khoảng trắng "   " không render ký tự trắng', () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: '   ', email: 'u@test.edu', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const userBtn = wrapper.find('.app-header__user');
      expect(userBtn.text().trim()).not.toBe('');
    });

    it('ADV-003: XSS payload trong displayName không gây lỗi hoặc inject DOM bất thường', () => {
      const auth = useAuthStore();
      auth.user = {
        id: 1,
        displayName: '<script>alert("XSS")</script><img src=x onerror=alert(1)>',
        email: 'u@test.edu',
        role: 'STUDENT',
        avatarUrl: null,
        createdAt: '',
      } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      expect(wrapper.find('script').exists()).toBe(false);
      // Nút user không bị vỡ DOM
      expect(wrapper.find('.app-header__user').exists()).toBe(true);
    });

    it('ADV-004: Khi avatarUrl bị lỗi 404 (error event), phải có ký tự fallback hiển thị thay vì nút rỗng hoàn toàn', async () => {
      const auth = useAuthStore();
      auth.user = {
        id: 1,
        displayName: 'Nguyễn Nam',
        email: 'u@test.edu',
        role: 'STUDENT',
        avatarUrl: 'https://invalid-domain-404.com/avatar.png',
        createdAt: '',
      } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const img = wrapper.find('.app-header__user-avatar-image');
      expect(img.exists()).toBe(true);

      // Trigger error event
      await img.trigger('error');
      await nextTick();

      // Khi img bị display: none, bên trong button phải có nội dung nhìn thấy được (text ký tự N)
      const userBtn = wrapper.find('.app-header__user');
      expect(userBtn.text().trim()).toBe('N');
    });

    it('ADV-005: DisplayName siêu dài 500 ký tự không làm tràn button', () => {
      const auth = useAuthStore();
      auth.user = {
        id: 1,
        displayName: 'A'.repeat(500),
        email: 'u@test.edu',
        role: 'STUDENT',
        avatarUrl: null,
        createdAt: '',
      } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const userBtn = wrapper.find('.app-header__user');
      expect(userBtn.text().trim().length).toBe(1);
    });
  });

  describe('Adversarial 2: Phân quyền & Chuyển đổi trạng thái nhanh (Race conditions)', () => {
    it('ADV-006: Chuyển đổi liên tục vai trò: STUDENT -> TEACHER -> ADMIN -> null', async () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Test', email: 'u@test.edu', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      expect(wrapper.findAll('.app-header__nav a').some((a) => a.text().includes('Studio'))).toBe(false);

      // Chuyển sang TEACHER
      auth.user = { ...auth.user, role: 'TEACHER' } as any;
      await nextTick();
      expect(wrapper.findAll('.app-header__nav a').some((a) => a.text().includes('Studio'))).toBe(true);

      // Chuyển sang ADMIN
      auth.user = { ...auth.user, role: 'ADMIN' } as any;
      await nextTick();
      expect(wrapper.findAll('.app-header__nav a').some((a) => a.text().includes('Studio'))).toBe(true);

      // Đăng xuất (role = null)
      auth.user = null;
      auth.accessToken = null;
      auth.status = 'idle';
      await nextTick();
      expect(wrapper.findAll('.app-header__nav a').some((a) => a.text().includes('Studio'))).toBe(false);
      expect(wrapper.find('.app-header__login').exists()).toBe(true);
    });

    it('ADV-007: Vai trò TEACHER_PENDING không được thấy link Studio trên nav', async () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Pending Teacher', email: 'u@test.edu', role: 'TEACHER_PENDING' as any, avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      expect(wrapper.findAll('.app-header__nav a').some((a) => a.text().includes('Studio'))).toBe(false);
    });
  });

  describe('Adversarial 3: Keyboard Accessibility (WCAG 2.1)', () => {
    it('ADV-008: Nhấn phím Escape khi dropdown menu đang mở phải đóng menu', async () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Nam', email: 'u@test.edu', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader, { attachTo: document.body });
      await wrapper.find('.app-header__user').trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(true);

      // Trigger Escape key
      window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
      await nextTick();
      expect(wrapper.find('.app-header__menu').exists()).toBe(false);
      wrapper.unmount();
    });

    it('ADV-009: Nhấn phím Escape khi mobile nav đang mở phải đóng mobile nav', async () => {
      const wrapper = mount(AppHeader, { attachTo: document.body });
      await wrapper.find('.app-header__burger').trigger('click');
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(true);

      // Trigger Escape key
      window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
      await nextTick();
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(false);
      wrapper.unmount();
    });
  });
});
