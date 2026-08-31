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
import { messages } from '@/i18n/vi';

describe('AppHeader — Unit Tests (Modules A ~ H)', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
    mockCurrentRoute.fullPath = '/';
    window.scrollY = 0;
  });

  // ── Module A: Render cơ bản & Cấu trúc DOM ──
  describe('Module A: Render cơ bản & Cấu trúc DOM (U-HDR-001 ~ U-HDR-006)', () => {
    it('U-HDR-001: Header render thành công với class app-header', () => {
      const wrapper = mount(AppHeader);
      expect(wrapper.find('header.app-header').exists()).toBe(true);
    });

    it('U-HDR-002: Logo DSA Visual hiển thị với alt="DSA Visual"', () => {
      const wrapper = mount(AppHeader);
      const logoImg = wrapper.find('.app-header__brand-img');
      expect(logoImg.exists()).toBe(true);
      expect(logoImg.attributes('alt')).toBe('DSA Visual');
      expect(logoImg.attributes('src')).toBeDefined();
    });

    it('U-HDR-003: Logo link trỏ về trang chủ { name: \'home\' }', () => {
      const wrapper = mount(AppHeader);
      const brandLink = wrapper.find('.app-header__brand');
      expect(brandLink.attributes('data-to')).toBe(JSON.stringify({ name: 'home' }));
    });

    it('U-HDR-004: Aria-label logo chuẩn cho accessibility', () => {
      const wrapper = mount(AppHeader);
      const brandLink = wrapper.find('.app-header__brand');
      expect(brandLink.attributes('aria-label')).toBe('DSA Visual — Trang chủ');
    });

    it('U-HDR-005: Nav chính có aria-label="Điều hướng chính"', () => {
      const wrapper = mount(AppHeader);
      const nav = wrapper.find('nav.app-header__nav');
      expect(nav.attributes('aria-label')).toBe('Điều hướng chính');
    });

    it('U-HDR-006: Container bên trong có class container', () => {
      const wrapper = mount(AppHeader);
      const inner = wrapper.find('.app-header__inner');
      expect(inner.classes()).toContain('container');
    });
  });

  // ── Module B: Menu điều hướng — Hiển thị link theo vai trò ──
  describe('Module B: Menu điều hướng — Hiển thị link theo vai trò (U-NAV-001 ~ U-NAV-011)', () => {
    it('U-NAV-001: Guest thấy đủ 4 link chính (Lộ trình, Mô phỏng, Thử thách, Cửa hàng)', () => {
      const wrapper = mount(AppHeader);
      const navLinks = wrapper.findAll('.app-header__nav .app-header__link');
      const texts = navLinks.map((l) => l.text());
      expect(texts).toContain(messages.nav.path);
      expect(texts).toContain(messages.nav.simulations);
      expect(texts).toContain('Thử thách');
      expect(texts).toContain('Cửa hàng');
    });

    it('U-NAV-002: Guest KHÔNG thấy link Studio', () => {
      const wrapper = mount(AppHeader);
      const navLinks = wrapper.findAll('.app-header__nav .app-header__link');
      expect(navLinks.some((l) => l.text() === 'Studio')).toBe(false);
    });

    it('U-NAV-003: Student KHÔNG thấy link Studio', () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Student', email: 's@fpt.edu.vn', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const navLinks = wrapper.findAll('.app-header__nav .app-header__link');
      expect(navLinks.some((l) => l.text() === 'Studio')).toBe(false);
    });

    it('U-NAV-004: Teacher thấy link Studio trỏ tới /studio', () => {
      const auth = useAuthStore();
      auth.user = { id: 2, displayName: 'Teacher', email: 't@fpt.edu.vn', role: 'TEACHER', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const studioLink = wrapper.findAll('.app-header__nav .app-header__link').find((l) => l.text().includes('Studio'));
      expect(studioLink).toBeDefined();
      expect(studioLink?.attributes('data-to')).toBe(JSON.stringify({ path: '/studio' }));
    });

    it('U-NAV-005: Admin thấy link Studio trỏ tới /studio', () => {
      const auth = useAuthStore();
      auth.user = { id: 3, displayName: 'Admin', email: 'a@fpt.edu.vn', role: 'ADMIN', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const studioLink = wrapper.findAll('.app-header__nav .app-header__link').find((l) => l.text().includes('Studio'));
      expect(studioLink).toBeDefined();
      expect(studioLink?.attributes('data-to')).toBe(JSON.stringify({ path: '/studio' }));
    });

    it('U-NAV-006: Link "Lộ trình" trỏ đúng route { name: \'path-list\' }', () => {
      const wrapper = mount(AppHeader);
      const link = wrapper.findAll('.app-header__nav .app-header__link').find((l) => l.text() === messages.nav.path);
      expect(link?.attributes('data-to')).toBe(JSON.stringify({ name: 'path-list' }));
    });

    it('U-NAV-007: Link "Mô phỏng" trỏ đúng route { name: \'simulations\' }', () => {
      const wrapper = mount(AppHeader);
      const link = wrapper.findAll('.app-header__nav .app-header__link').find((l) => l.text() === messages.nav.simulations);
      expect(link?.attributes('data-to')).toBe(JSON.stringify({ name: 'simulations' }));
    });

    it('U-NAV-008: Link "Thử thách" trỏ đúng route { name: \'quests\' }', () => {
      const wrapper = mount(AppHeader);
      const link = wrapper.findAll('.app-header__nav .app-header__link').find((l) => l.text() === 'Thử thách');
      expect(link?.attributes('data-to')).toBe(JSON.stringify({ name: 'quests' }));
    });

    it('U-NAV-009: Link "Cửa hàng" trỏ đúng route { name: \'shop\' }', () => {
      const wrapper = mount(AppHeader);
      const link = wrapper.findAll('.app-header__nav .app-header__link').find((l) => l.text() === 'Cửa hàng');
      expect(link?.attributes('data-to')).toBe(JSON.stringify({ name: 'shop' }));
    });

    it('U-NAV-010: Studio target computed là { path: \'/studio\' }', () => {
      const auth = useAuthStore();
      auth.user = { id: 2, displayName: 'Teacher', email: 't@fpt.edu.vn', role: 'TEACHER', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      const studioLink = wrapper.findAll('.app-header__nav .app-header__link').find((l) => l.text().includes('Studio'));
      expect(studioLink?.attributes('data-to')).toBe(JSON.stringify({ path: '/studio' }));
    });

    it('U-NAV-011: Text link sử dụng đúng i18n messages', () => {
      const wrapper = mount(AppHeader);
      const pathLink = wrapper.findAll('.app-header__nav .app-header__link')[0];
      const simLink = wrapper.findAll('.app-header__nav .app-header__link')[1];
      expect(pathLink.text()).toBe(messages.nav.path);
      expect(simLink.text()).toBe(messages.nav.simulations);
    });
  });

  // ── Module C: Khu vực tài khoản — Guest vs Authenticated ──
  describe('Module C: Khu vực tài khoản — Guest vs Authenticated (U-AUTH-001 ~ U-AUTH-007)', () => {
    it('U-AUTH-001: Guest thấy nút Đăng nhập trỏ login', () => {
      const wrapper = mount(AppHeader);
      const loginBtn = wrapper.find('.app-header__login');
      expect(loginBtn.exists()).toBe(true);
      expect(loginBtn.text()).toBe(messages.nav.login);
      expect(loginBtn.attributes('data-to')).toBe(JSON.stringify({ name: 'login' }));
    });

    it('U-AUTH-002: Guest thấy nút Đăng ký trỏ register', () => {
      const wrapper = mount(AppHeader);
      const regBtn = wrapper.find('.app-header__register');
      expect(regBtn.exists()).toBe(true);
      expect(regBtn.text()).toBe(messages.nav.register);
      expect(regBtn.attributes('data-to')).toBe(JSON.stringify({ name: 'register' }));
    });

    it('U-AUTH-003: Guest KHÔNG thấy HeartsGemsWidget', () => {
      const wrapper = mount(AppHeader);
      expect(wrapper.findComponent({ name: 'HeartsGemsWidget' }).exists()).toBe(false);
    });

    it('U-AUTH-004: Guest KHÔNG thấy avatar button', () => {
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user').exists()).toBe(false);
    });

    it('U-AUTH-005: Student thấy HeartsGemsWidget', () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Student', email: 's@fpt.edu.vn', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      expect(wrapper.findComponent({ name: 'HeartsGemsWidget' }).exists()).toBe(true);
    });

    it('U-AUTH-006: Student thấy avatar button', () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Student', email: 's@fpt.edu.vn', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user').exists()).toBe(true);
    });

    it('U-AUTH-007: Student KHÔNG thấy Đăng nhập/Đăng ký', () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Student', email: 's@fpt.edu.vn', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__login').exists()).toBe(false);
      expect(wrapper.find('.app-header__register').exists()).toBe(false);
    });
  });

  // ── Module D: Avatar & Equipment System ──
  describe('Module D: Avatar & Equipment System (U-AVT-001 ~ U-AVT-014)', () => {
    function authWithUser(displayName: string | null, avatarUrl: string | null = null) {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: displayName as any, email: 'u@fpt.edu.vn', role: 'STUDENT', avatarUrl, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';
      return auth;
    }

    it('U-AVT-001: Hiện chữ cái đầu khi không có avatar', () => {
      authWithUser('Tùng');
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user span').text()).toBe('T');
    });

    it('U-AVT-002: Hiện "U" khi displayName null', () => {
      authWithUser(null);
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user span').text()).toBe('U');
    });

    it('U-AVT-003: Hiện avatar từ avatarUrl của user', () => {
      authWithUser('Minh', 'https://example.com/avatar.png');
      const wrapper = mount(AppHeader);
      const img = wrapper.find('.app-header__user-avatar-image');
      expect(img.exists()).toBe(true);
      expect(img.attributes('src')).toBe('https://example.com/avatar.png');
    });

    it('U-AVT-004: Hiện avatar từ equipped item (ưu tiên hơn user.avatarUrl)', () => {
      authWithUser('Minh', 'https://example.com/avatar.png');
      const gmf = useGamificationStore();
      gmf.inventory = [
        { id: 1, itemId: 1, itemKey: 'avatar-cyber-hacker', name: 'Cyber Hacker', quantity: 1, type: 1, isEquipped: true, expiresAt: null },
      ];

      const wrapper = mount(AppHeader);
      const img = wrapper.find('.app-header__user-avatar-image');
      expect(img.attributes('src')).toBe('/assets/avatars/cyber-hacker.svg');
    });

    it('U-AVT-005: Avatar img error ẩn img display:none', async () => {
      authWithUser('Minh', 'https://example.com/broken.png');
      const wrapper = mount(AppHeader);
      const img = wrapper.find('.app-header__user-avatar-image');
      expect(img.exists()).toBe(true);

      await img.trigger('error');
      await nextTick();
      expect(wrapper.find('.app-header__user-avatar-image').exists()).toBe(false);
      expect(wrapper.find('.app-header__user span').text()).toBe('M');
    });

    it('U-AVT-006: Frame class đúng theo equipped frame neon', () => {
      authWithUser('Minh');
      const gmf = useGamificationStore();
      gmf.inventory = [
        { id: 2, itemId: 2, itemKey: 'frame-neon', name: 'Neon Frame', quantity: 1, type: 2, isEquipped: true, expiresAt: null },
      ];
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user-frame').classes()).toContain('app-header__user-frame--neon');
    });

    it('U-AVT-007: Frame class đúng: gold', () => {
      authWithUser('Minh');
      const gmf = useGamificationStore();
      gmf.inventory = [
        { id: 3, itemId: 3, itemKey: 'frame-gold', name: 'Gold Frame', quantity: 1, type: 2, isEquipped: true, expiresAt: null },
      ];
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user-frame').classes()).toContain('app-header__user-frame--gold');
    });

    it('U-AVT-008: Frame class đúng: cyber', () => {
      authWithUser('Minh');
      const gmf = useGamificationStore();
      gmf.inventory = [
        { id: 4, itemId: 4, itemKey: 'frame-cyber', name: 'Cyber Frame', quantity: 1, type: 2, isEquipped: true, expiresAt: null },
      ];
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user-frame').classes()).toContain('app-header__user-frame--cyber');
    });

    it('U-AVT-009: Frame class đúng: fire', () => {
      authWithUser('Minh');
      const gmf = useGamificationStore();
      gmf.inventory = [
        { id: 5, itemId: 5, itemKey: 'frame-fire', name: 'Fire Frame', quantity: 1, type: 2, isEquipped: true, expiresAt: null },
      ];
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user-frame').classes()).toContain('app-header__user-frame--fire');
    });

    it('U-AVT-010: Frame class đúng: ice', () => {
      authWithUser('Minh');
      const gmf = useGamificationStore();
      gmf.inventory = [
        { id: 6, itemId: 6, itemKey: 'frame-ice', name: 'Ice Frame', quantity: 1, type: 2, isEquipped: true, expiresAt: null },
      ];
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user-frame').classes()).toContain('app-header__user-frame--ice');
    });

    it('U-AVT-011: Không có equipped frame -> userFrameClass rỗng', () => {
      authWithUser('Minh');
      const wrapper = mount(AppHeader);
      const frameSpan = wrapper.find('.app-header__user-frame');
      expect(frameSpan.classes()).toEqual(['app-header__user-frame']);
    });

    it('U-AVT-012: Avatar class đúng theo variant: wizard', () => {
      authWithUser('Minh');
      const gmf = useGamificationStore();
      gmf.inventory = [
        { id: 7, itemId: 7, itemKey: 'avatar-wizard', name: 'Wizard Avatar', quantity: 1, type: 1, isEquipped: true, expiresAt: null },
      ];
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user').classes()).toContain('app-header__user-avatar--wizard');
    });

    it('U-AVT-013: Avatar button aria-label = displayName', () => {
      authWithUser('Minh');
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user').attributes('aria-label')).toBe('Minh');
    });

    it('U-AVT-014: Avatar button aria-label fallback = "Hồ sơ" khi null', () => {
      authWithUser(null);
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__user').attributes('aria-label')).toBe('Hồ sơ');
    });
  });

  // ── Module E: Dropdown Menu ──
  describe('Module E: Dropdown Menu (U-MNU-001 ~ U-MNU-015)', () => {
    function setupAuthedUser() {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Minh', email: 'm@fpt.edu.vn', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';
      return auth;
    }

    it('U-MNU-001: Menu đóng ban đầu', () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__menu').exists()).toBe(false);
    });

    it('U-MNU-002: Click avatar mở menu', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(true);
    });

    it('U-MNU-003: Click avatar lần 2 đóng menu', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      const userBtn = wrapper.find('.app-header__user');
      await userBtn.trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(true);
      await userBtn.trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(false);
    });

    it('U-MNU-004: Menu có đủ 5 mục', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');

      const menuItems = wrapper.findAll('.app-header__menu-item');
      expect(menuItems.length).toBe(5);
      const texts = menuItems.map((i) => i.text());
      expect(texts).toContain(messages.nav.profile);
      expect(texts).toContain('Bảng xếp hạng');
      expect(texts).toContain('Premium');
      expect(texts).toContain('Trợ giúp');
      expect(texts).toContain(messages.nav.logout);
    });

    it('U-MNU-005: "Hồ sơ" trỏ route profile', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');

      const profileLink = wrapper.findAll('.app-header__menu-item').find((i) => i.text() === messages.nav.profile);
      expect(profileLink?.attributes('data-to')).toBe(JSON.stringify({ name: 'profile' }));
    });

    it('U-MNU-006: "Bảng xếp hạng" trỏ route leaderboard', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');

      const lbLink = wrapper.findAll('.app-header__menu-item').find((i) => i.text() === 'Bảng xếp hạng');
      expect(lbLink?.attributes('data-to')).toBe(JSON.stringify({ name: 'leaderboard' }));
    });

    it('U-MNU-007: "Premium" trỏ route premium', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');

      const premLink = wrapper.findAll('.app-header__menu-item').find((i) => i.text() === 'Premium');
      expect(premLink?.attributes('data-to')).toBe(JSON.stringify({ name: 'premium' }));
    });

    it('U-MNU-008: "Trợ giúp" trỏ route help', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');

      const helpLink = wrapper.findAll('.app-header__menu-item').find((i) => i.text() === 'Trợ giúp');
      expect(helpLink?.attributes('data-to')).toBe(JSON.stringify({ name: 'help' }));
    });

    it('U-MNU-009: "Đăng xuất" là button với class --danger', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');

      const logoutBtn = wrapper.find('button.app-header__menu-item--danger');
      expect(logoutBtn.exists()).toBe(true);
      expect(logoutBtn.text()).toBe(messages.nav.logout);
    });

    it('U-MNU-010: Click menu item đóng menu', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(true);

      const profileLink = wrapper.findAll('.app-header__menu-item').find((i) => i.text() === messages.nav.profile);
      await profileLink?.trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(false);
    });

    it('U-MNU-011: Click bên ngoài header đóng menu', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader, { attachTo: document.body });
      await wrapper.find('.app-header__user').trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(true);

      // Click outside header
      document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
      await nextTick();
      expect(wrapper.find('.app-header__menu').exists()).toBe(false);
      wrapper.unmount();
    });

    it('U-MNU-012: Route thay đổi đóng menu', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');
      expect(wrapper.find('.app-header__menu').exists()).toBe(true);

      mockCurrentRoute.fullPath = '/profile';
      await nextTick();
      expect(wrapper.find('.app-header__menu').exists()).toBe(false);
    });

    it('U-MNU-013: Đăng xuất gọi auth.logout()', async () => {
      const auth = setupAuthedUser();
      const logoutSpy = vi.spyOn(auth, 'logout').mockResolvedValue();

      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');
      const logoutBtn = wrapper.find('button.app-header__menu-item--danger');
      await logoutBtn.trigger('click');

      expect(logoutSpy).toHaveBeenCalled();
    });

    it('U-MNU-014: Đăng xuất redirect về login qua router.replace', async () => {
      const auth = setupAuthedUser();
      vi.spyOn(auth, 'logout').mockResolvedValue();

      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__user').trigger('click');
      const logoutBtn = wrapper.find('button.app-header__menu-item--danger');
      await logoutBtn.trigger('click');
      await nextTick();

      expect(mockRouterReplace).toHaveBeenCalledWith({ name: 'login' });
    });

    it('U-MNU-015: Menu có Transition animation name="app-menu"', async () => {
      setupAuthedUser();
      const wrapper = mount(AppHeader);
      const transitions = wrapper.findAllComponents({ name: 'Transition' });
      const menuTransition = transitions.find((t) => t.attributes('name') === 'app-menu');
      expect(menuTransition).toBeDefined();
      expect(menuTransition?.attributes('name')).toBe('app-menu');
    });
  });

  // ── Module F: Scroll behavior ──
  describe('Module F: Scroll behavior (U-SCR-001 ~ U-SCR-005)', () => {
    it('U-SCR-001: Ban đầu scrollY=0 không có class app-header--scrolled', () => {
      window.scrollY = 0;
      const wrapper = mount(AppHeader);
      expect(wrapper.find('header').classes()).not.toContain('app-header--scrolled');
    });

    it('U-SCR-002: Scroll > 50px có class app-header--scrolled', async () => {
      const wrapper = mount(AppHeader);
      window.scrollY = 60;
      window.dispatchEvent(new Event('scroll'));
      await nextTick();
      expect(wrapper.find('header').classes()).toContain('app-header--scrolled');
    });

    it('U-SCR-003: Scroll <= 50px không có class app-header--scrolled', async () => {
      const wrapper = mount(AppHeader);
      window.scrollY = 60;
      window.dispatchEvent(new Event('scroll'));
      await nextTick();
      expect(wrapper.find('header').classes()).toContain('app-header--scrolled');

      window.scrollY = 30;
      window.dispatchEvent(new Event('scroll'));
      await nextTick();
      expect(wrapper.find('header').classes()).not.toContain('app-header--scrolled');
    });

    it('U-SCR-004: Scroll listener được đăng ký khi mount', () => {
      const spyAdd = vi.spyOn(window, 'addEventListener');
      mount(AppHeader);
      expect(spyAdd).toHaveBeenCalledWith('scroll', expect.any(Function), { passive: true });
    });

    it('U-SCR-005: Scroll listener được gỡ bỏ khi unmount', () => {
      const spyRemove = vi.spyOn(window, 'removeEventListener');
      const wrapper = mount(AppHeader);
      wrapper.unmount();
      expect(spyRemove).toHaveBeenCalledWith('scroll', expect.any(Function));
    });
  });

  // ── Module G: Mobile Hamburger Menu ──
  describe('Module G: Mobile Hamburger Menu (U-MOB-001 ~ U-MOB-014)', () => {
    it('U-MOB-001: Burger button tồn tại', () => {
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__burger').exists()).toBe(true);
    });

    it('U-MOB-002: Burger aria-label là "Mở menu" khi đóng', () => {
      const wrapper = mount(AppHeader);
      expect(wrapper.find('.app-header__burger').attributes('aria-label')).toBe('Mở menu');
    });

    it('U-MOB-003: Burger aria-label là "Đóng menu" khi mở', async () => {
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');
      expect(wrapper.find('.app-header__burger').attributes('aria-label')).toBe('Đóng menu');
    });

    it('U-MOB-004: Burger aria-expanded cập nhật đúng theo trạng thái mở/đóng', async () => {
      const wrapper = mount(AppHeader);
      const burger = wrapper.find('.app-header__burger');
      expect(burger.attributes('aria-expanded')).toBe('false');
      await burger.trigger('click');
      expect(burger.attributes('aria-expanded')).toBe('true');
      await burger.trigger('click');
      expect(burger.attributes('aria-expanded')).toBe('false');
    });

    it('U-MOB-005: Click burger mở mobile nav', async () => {
      const wrapper = mount(AppHeader);
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(false);
      await wrapper.find('.app-header__burger').trigger('click');
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(true);
    });

    it('U-MOB-006: Mobile nav Guest có đủ 4 link chính + Đăng nhập + Đăng ký', async () => {
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');

      const mobileLinks = wrapper.findAll('.app-header__mobile-link');
      const texts = mobileLinks.map((l) => l.text());
      expect(texts).toContain(messages.nav.path);
      expect(texts).toContain(messages.nav.simulations);
      expect(texts).toContain('Thử thách');
      expect(texts).toContain('Cửa hàng');
      expect(texts).toContain(messages.nav.login);
      expect(texts).toContain(messages.nav.register);
    });

    it('U-MOB-007: Mobile nav Authed có các link tài khoản', async () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Minh', email: 'm@fpt.edu.vn', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');

      const mobileLinks = wrapper.findAll('.app-header__mobile-link');
      const texts = mobileLinks.map((l) => l.text());
      expect(texts).toContain(messages.nav.profile);
      expect(texts).toContain('Bảng xếp hạng');
      expect(texts).toContain('Premium');
      expect(texts).toContain('Trợ giúp');
      expect(texts).toContain(messages.nav.logout);
    });

    it('U-MOB-008: Mobile nav có divider', async () => {
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');
      expect(wrapper.find('.app-header__mobile-divider').exists()).toBe(true);
    });

    it('U-MOB-009: Teacher thấy Studio trong mobile nav', async () => {
      const auth = useAuthStore();
      auth.user = { id: 2, displayName: 'Teacher', email: 't@fpt.edu.vn', role: 'TEACHER', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');

      const studioLink = wrapper.findAll('.app-header__mobile-link').find((l) => l.text().includes('Studio'));
      expect(studioLink).toBeDefined();
    });

    it('U-MOB-010: Click mobile link đóng mobile nav', async () => {
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(true);

      const pathLink = wrapper.findAll('.app-header__mobile-link').find((l) => l.text() === messages.nav.path);
      await pathLink?.trigger('click');
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(false);
    });

    it('U-MOB-011: Route thay đổi đóng mobile nav', async () => {
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(true);

      mockCurrentRoute.fullPath = '/simulations';
      await nextTick();
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(false);
    });

    it('U-MOB-012: Click ngoài header đóng mobile nav', async () => {
      const wrapper = mount(AppHeader, { attachTo: document.body });
      await wrapper.find('.app-header__burger').trigger('click');
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(true);

      document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
      await nextTick();
      expect(wrapper.find('nav.app-header__mobile-nav').exists()).toBe(false);
      wrapper.unmount();
    });

    it('U-MOB-013: Burger bars có class --open khi mở mobile nav', async () => {
      const wrapper = mount(AppHeader);
      await wrapper.find('.app-header__burger').trigger('click');

      const bars = wrapper.findAll('.app-header__burger-bar');
      expect(bars.length).toBe(3);
      bars.forEach((bar) => {
        expect(bar.classes()).toContain('app-header__burger-bar--open');
      });
    });

    it('U-MOB-014: Mobile nav có Transition animation name="app-menu"', async () => {
      const wrapper = mount(AppHeader);
      const transitions = wrapper.findAllComponents({ name: 'Transition' });
      const mobileTransition = transitions.find((t) => t.attributes('name') === 'app-menu');
      expect(mobileTransition).toBeDefined();
    });
  });

  // ── Module H: Gamification trên onMounted ──
  describe('Module H: Gamification trên onMounted (U-GMF-001 ~ U-GMF-002)', () => {
    it('U-GMF-001: Authenticated gọi gamification.fetchInventory() khi mount', () => {
      const auth = useAuthStore();
      auth.user = { id: 1, displayName: 'Minh', email: 'm@fpt.edu.vn', role: 'STUDENT', avatarUrl: null, createdAt: '' } as any;
      auth.accessToken = 'token';
      auth.status = 'authenticated';

      const gmf = useGamificationStore();
      const spyFetch = vi.spyOn(gmf, 'fetchInventory');

      mount(AppHeader);
      expect(spyFetch).toHaveBeenCalled();
    });

    it('U-GMF-002: Guest KHÔNG gọi gamification.fetchInventory() khi mount', () => {
      const gmf = useGamificationStore();
      const spyFetch = vi.spyOn(gmf, 'fetchInventory');

      mount(AppHeader);
      expect(spyFetch).not.toHaveBeenCalled();
    });
  });
});
