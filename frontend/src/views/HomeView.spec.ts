// Tests for HomeView — verifying merged Guest Landing & Authenticated Dashboard with Source 2 Layout.
import { flushPromises, mount } from '@vue/test-utils';
import { nextTick } from 'vue';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const { pushMock } = vi.hoisted(() => ({ pushMock: vi.fn() }));

const mockAuth = {
  isAuthenticated: false,
  user: null as { displayName: string; email: string; role: string } | null,
  role: null as string | null,
};

const mockGamification = {
  level: 1,
  xp: 0,
  streakDays: 0,
  hearts: 5,
  heartsMax: 5,
  gems: 100,
  questDone: 0,
  quests: [] as Array<{ id: number; title: string; claimed: boolean; rewardXp: number; rewardGems: number }>,
  achievements: [] as Array<{ id: number; title: string; name: string }>,
  fetchAll: vi.fn(),
  fetchQuests: vi.fn(),
  fetchAchievements: vi.fn(),
};

const mockProgress = {
  overview: null as {
    lessonsViewed: number;
    lessonsTotal: number;
    exercisesCompleted: number;
    exercisesTotal: number;
    avgScore: number | null;
    topics: Array<{ id: number; name: string; progressPct: number }>;
  } | null,
  fetchOverview: vi.fn(),
};

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
  RouterLink: { template: '<a><slot /></a>' },
}));

vi.mock('@/stores/auth', () => ({
  useAuthStore: () => mockAuth,
}));

vi.mock('@/stores/gamification', () => ({
  useGamificationStore: () => mockGamification,
}));

vi.mock('@/stores/progress', () => ({
  useProgressStore: () => mockProgress,
}));

vi.mock('@/stores/ui', () => ({
  useUiStore: () => ({ showToast: vi.fn() }),
}));

import HomeView from './HomeView.vue';

describe('HomeView — Source 2 Layout & Presentation Tests', () => {
  beforeEach(() => {
    pushMock.mockReset();
    mockAuth.isAuthenticated = false;
    mockAuth.user = null;
    mockAuth.role = null;

    mockGamification.level = 1;
    mockGamification.xp = 0;
    mockGamification.streakDays = 0;
    mockGamification.quests = [];
    mockGamification.achievements = [];

    mockProgress.overview = null;

    window.matchMedia = vi.fn().mockReturnValue({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    });
  });

  describe('1. Chế độ Khách (Guest / Landing Layout — 8 Tầng Source 2)', () => {
    it('Tầng 1: Hero Section có badge pulse, tiêu đề gradient, 2 CTA và 4 chỉ số trust', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.hero').exists()).toBe(true);
      expect(wrapper.find('.hero__badge').exists()).toBe(true);
      expect(wrapper.find('.pulse-dot').exists()).toBe(true);
      expect(wrapper.find('h1.hero__title').exists()).toBe(true);
      expect(wrapper.find('p.hero__sub').exists()).toBe(true);
      expect(wrapper.findAll('.hero__actions a').length).toBe(2);
      expect(wrapper.find('.hero__trust').exists()).toBe(true);
      expect(wrapper.findAll('.trust-item').length).toBe(4);
      wrapper.unmount();
    });

    it('Tầng 1: Hero Terminal Preview có macOS dots, nút điều khiển và cột bar hoạt ảnh', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.preview-window').exists()).toBe(true);
      expect(wrapper.find('.terminal-dots').exists()).toBe(true);
      expect(wrapper.find('.bars-container').exists()).toBe(true);
      expect(wrapper.findAll('.preview-bar').length).toBe(6);
      wrapper.unmount();
    });

    it('Tầng 2: Bento Grid 4 cột hiển thị đủ 7 modules (Sorting, Graph, Gamification, OOP, SOLID, Patterns, DI)', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.bento-grid').exists()).toBe(true);
      const bentoCards = wrapper.findAll('.bento-card');
      expect(bentoCards.length).toBe(7);
      expect(wrapper.find('.bento-large').exists()).toBe(true);
      expect(wrapper.findAll('.bento-medium').length).toBe(2);
      expect(wrapper.findAll('.bento-small').length).toBe(4);
      expect(wrapper.text()).toContain('Thuật Toán Sắp Xếp Trực Quan');
      expect(wrapper.text()).toContain('Sân Chơi Đồ Thị');
      expect(wrapper.text()).toContain('Gamification Học Mà Chơi');
      wrapper.unmount();
    });

    it('Tầng 3: Thư viện thuật toán có thanh tìm kiếm, bộ lọc nhóm và lưới thẻ', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.algogrid-section').exists()).toBe(true);
      expect(wrapper.find('.home__catalog-search-input').exists()).toBe(true);
      expect(wrapper.findAll('.home__filter').length).toBeGreaterThan(5);
      expect(wrapper.find('.home__catalog').exists()).toBe(true);
      wrapper.unmount();
    });

    it('3 Demo cards công khai: 3 thẻ có thumbnail khác nhau và nút Chạy thử', async () => {
      const wrapper = mount(HomeView);
      const cards = wrapper.findAll('.home__demo');
      expect(cards.length).toBe(3);
      expect(wrapper.find('.home__thumb-bars').exists()).toBe(true);
      expect(wrapper.find('.home__thumb-row').exists()).toBe(true);
      expect(wrapper.find('.home__thumb-graph').exists()).toBe(true);

      const runButton = wrapper.findAll('.home__demo button')[0];
      await runButton.trigger('click');
      expect(pushMock).toHaveBeenCalledWith({ name: 'simulator', params: { key: 'sort.bubble' } });
      wrapper.unmount();
    });

    it('Tầng 4: Mô hình Freemium có 3 thẻ (Hearts, Gems, Premium VIP)', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.freemium-section').exists()).toBe(true);
      const freemiumCards = wrapper.findAll('.freemium-card');
      expect(freemiumCards.length).toBe(3);
      expect(wrapper.find('.freemium-card--premium').exists()).toBe(true);
      wrapper.unmount();
    });

    it('Tầng 5: Bố cục So le 2 cột Deep-Dives (Roadmap, Codelab, AI Mentor)', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.roadmap-mockup').exists()).toBe(true);
      expect(wrapper.find('.codelab-mockup').exists()).toBe(true);
      expect(wrapper.find('.codelab-output').text()).toContain('All 15/15 Test Cases Passed!');
      expect(wrapper.find('.ai-mockup').exists()).toBe(true);
      expect(wrapper.text()).toContain('AI Assistant - Người Mentor Tận Tụy');
      wrapper.unmount();
    });

    it('Tầng 6: Testimonials Carousel có chấm sao, trích dẫn quote và nút chuyển slide', async () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.testimonials-carousel').exists()).toBe(true);
      expect(wrapper.find('.testimonial-quote').exists()).toBe(true);
      expect(wrapper.find('.testimonial-rating').exists()).toBe(true);
      expect(wrapper.findAll('.carousel-btn').length).toBe(2);
      wrapper.unmount();
    });

    it('Tầng 7: Sandbox tương tác có input mảng và nút Chạy sắp xếp / Đặt lại', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.home__sandbox-input').exists()).toBe(true);
      expect(wrapper.findAll('.home__sandbox-bar').length).toBe(6);
      wrapper.unmount();
    });

    it('Tầng 8: CTA Section & Footer có nút Tạo tài khoản và các link điều hướng', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.cta-card').exists()).toBe(true);
      expect(wrapper.find('.landing-footer').exists()).toBe(true);
      wrapper.unmount();
    });
  });

  describe('2. Chế độ Đã đăng nhập (Authenticated Dashboard Layout Source 2)', () => {
    beforeEach(() => {
      mockAuth.isAuthenticated = true;
      mockAuth.user = {
        displayName: 'Nguyễn Văn A',
        email: 'vana@fpt.edu.vn',
        role: 'STUDENT',
      };
      mockAuth.role = 'STUDENT';

      mockGamification.level = 4;
      mockGamification.xp = 650;
      mockGamification.streakDays = 5;
      mockGamification.quests = [
        { id: 1, title: 'Hoàn thành 1 bài mô phỏng', claimed: true, rewardXp: 50, rewardGems: 10 },
        { id: 2, title: 'Giải 1 bài Codelab', claimed: false, rewardXp: 100, rewardGems: 20 },
      ];

      mockProgress.overview = {
        lessonsViewed: 8,
        lessonsTotal: 20,
        exercisesCompleted: 5,
        exercisesTotal: 15,
        avgScore: 92,
        topics: [
          { id: 1, name: 'Mảng & Chuỗi (Arrays & Strings)', progressPct: 75 },
          { id: 2, name: 'Sắp xếp & Tìm kiếm', progressPct: 40 },
        ],
      };
    });

    it('hiển thị Greeting Banner với tên người dùng, Lv.4, 650 XP và tag Sinh viên', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.dashboard-section').exists()).toBe(true);
      expect(wrapper.find('.greeting-banner').exists()).toBe(true);
      expect(wrapper.find('.greeting-banner__name').text()).toBe('Nguyễn Văn A');
      expect(wrapper.text()).toContain('Level 4');
      expect(wrapper.text()).toContain('650 XP');
      expect(wrapper.find('.role-tag--student').exists()).toBe(true);
      wrapper.unmount();
    });

    it('hiển thị Lộ trình đang học (Enrolled Card) và Khám phá Lộ trình học (Quickstart Card)', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.enrolled-card').exists()).toBe(true);
      expect(wrapper.text()).toContain('Mảng & Chuỗi (Arrays & Strings)');
      expect(wrapper.text()).toContain('75%');
      expect(wrapper.find('.quickstart-card').exists()).toBe(true);
      expect(wrapper.text()).toContain('Xem Bản đồ Lộ trình');
      expect(wrapper.text()).toContain('Tham gia Lớp học');
      wrapper.unmount();
    });

    it('hiển thị Tiến trình XP (XP Wheel SVG) và Chuỗi ngày học Streak (Streak Card)', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.xp-wheel').exists()).toBe(true);
      expect(wrapper.find('.xp-wheel__svg').exists()).toBe(true);
      expect(wrapper.find('.streak-card').exists()).toBe(true);
      expect(wrapper.find('.streak-num').text()).toBe('5');
      wrapper.unmount();
    });

    it('hiển thị Nhiệm vụ hôm nay, Hoạt động gần đây và Truy cập nhanh', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.quests-card').exists()).toBe(true);
      expect(wrapper.text()).toContain('Hoàn thành 1 bài mô phỏng');
      expect(wrapper.find('.recent-activity-card').exists()).toBe(true);
      expect(wrapper.find('.quicklinks-card').exists()).toBe(true);
      expect(wrapper.findAll('.quicklink-btn').length).toBe(4);
      wrapper.unmount();
    });
  });
});
