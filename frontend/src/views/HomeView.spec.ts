// Tests for HomeView — verifying merged Guest Landing & Authenticated Dashboard with Source 2 Layout.
import { mount } from '@vue/test-utils';
import { nextTick } from 'vue';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { messages } from '@/i18n/vi';

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
  achievements: [] as Array<{ id: number; code: string; name: string; description: string | null; iconUrl: string | null; earnedAt: string | null }>,
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

  describe('1. Chế độ Khách (Guest / Landing Layout — Source 2)', () => {
    it('Tầng 1: Hero Section có tiêu đề gradient, 2 CTA và 4 chỉ số trust', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.hero').exists()).toBe(true);
      expect(wrapper.find('h1.hero__title').exists()).toBe(true);
      expect(wrapper.find('p.hero__sub').exists()).toBe(true);
      expect(wrapper.findAll('.hero__actions a').length).toBe(2);
      expect(wrapper.find('.hero__trust').exists()).toBe(true);
      expect(wrapper.findAll('.trust-item').length).toBe(4);
      wrapper.unmount();
    });

    it('Tầng 1: Hero Algorithmic Stage có tiêu đề Quick Sort, chip complexity, nút điều khiển, speed slider và status badge', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.stage-panel').exists()).toBe(true);
      expect(wrapper.find('.stage-title').text()).toContain('Quick Sort');
      expect(wrapper.find('.stage-complexity').text()).toContain('O(N log N)');
      expect(wrapper.findAll('.stage-btn').length).toBe(3);
      expect(wrapper.find('.stage-speed__input').exists()).toBe(true);
      expect(wrapper.find('.stage-speed__label').text()).toContain('1×');
      expect(wrapper.findAll('.preview-bar').length).toBe(6);
      expect(wrapper.find('.stage-status__badge').text()).toBe(messages.home.previewPhase[0]);
      wrapper.unmount();
    });

    it('Tầng 1: nút Step bước tới phase và speed slider cập nhật nhãn tốc độ', async () => {
      const wrapper = mount(HomeView);
      const stepBtn = wrapper.findAll('.stage-btn')[1];
      await stepBtn.trigger('click');
      expect(wrapper.find('.stage-status__badge').text()).toBe(messages.home.previewPhase[1]);

      await wrapper.find('.stage-speed__input').setValue('2');
      expect(wrapper.find('.stage-speed__label').text()).toContain('2×');
      wrapper.unmount();
    });

    it('Tầng 2: Bento Grid 4 cột hiển thị đủ 7 modules và large card có live mini visualizer 7 cột', () => {
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

      const miniBars = wrapper.findAll('.mini-bar');
      expect(miniBars.length).toBe(7);
      expect(miniBars[0].attributes('data-value')).toBe('7');
      wrapper.unmount();
    });

    it('Tầng 2: Mini visualizer tự hoán đổi cột theo chu kỳ (compare → swap)', async () => {
      vi.useFakeTimers();
      const wrapper = mount(HomeView);
      const read = () =>
        wrapper.findAll('.mini-bar').map((b) => b.attributes('data-value') ?? '?');
      expect(read()).toEqual(['7', '2', '9', '4', '5', '1', '8']);

      // tick 1: so sánh cặp (7, 2) → 7 > 2 nên hoán đổi
      vi.advanceTimersByTime(800);
      await nextTick();
      expect(read()).toEqual(['2', '7', '9', '4', '5', '1', '8']);

      // tick 2: so sánh cặp (7, 9) → giữ nguyên thứ tự
      vi.advanceTimersByTime(800);
      await nextTick();
      expect(read()).toEqual(['2', '7', '9', '4', '5', '1', '8']);

      // tick 3: so sánh cặp (9, 4) → hoán đổi
      vi.advanceTimersByTime(800);
      await nextTick();
      expect(read()).toEqual(['2', '7', '4', '9', '5', '1', '8']);

      vi.useRealTimers();
      wrapper.unmount();
    });

    it('Tầng 3: Thư viện thuật toán có bộ lọc nhóm và lưới thẻ', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.algogrid-section').exists()).toBe(true);
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

    it('Tầng 5: Roadmap 4 node dọc — line phát sáng, node active và mô tả 4 bước', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.roadmap-mockup').exists()).toBe(true);
      expect(wrapper.find('.road-line').exists()).toBe(true);
      const nodes = wrapper.findAll('.rm-node');
      expect(nodes.length).toBe(4);
      expect(wrapper.find('.rm-node--active').exists()).toBe(true);
      expect(wrapper.text()).toContain('Lý thuyết nền tảng');
      expect(wrapper.text()).toContain('Mô phỏng trực quan');
      expect(wrapper.text()).toContain('Thực hành Codelab');
      expect(wrapper.text()).toContain('Trắc nghiệm củng cố');
      wrapper.unmount();
    });

    it('Tầng 5: Codelab auto-typing — gõ dần từng ký tự rồi 3 Testcase PASSED tuần tự', async () => {
      vi.useFakeTimers();
      const wrapper = mount(HomeView);
      expect(wrapper.find('.codelab-mockup').exists()).toBe(true);
      expect(wrapper.find('.codelab-run').exists()).toBe(true);
      // Đang gõ: nút Chạy bị khóa, code chưa đầy đủ
      expect(wrapper.find('.codelab-run').attributes('disabled')).toBeDefined();
      expect(wrapper.find('.codelab-caret').exists()).toBe(true);

      // Gõ xong (~252 ký tự × 22ms ≈ 5.5s) → 900ms tự chạy → chấm → 3 testcase
      vi.advanceTimersByTime(9500);
      await nextTick();
      expect(wrapper.findAll('.codelab-testcase__badge').length).toBe(3);
      expect(wrapper.find('.codelab-testcase__badge').text()).toContain('PASSED');
      expect(wrapper.find('.codelab-run--done').exists()).toBe(true);
      expect(wrapper.find('.codelab-testcase__meta').text()).toContain('ms · Beats');

      // Vòng lặp: sau 2.5s reset — nút quay về trạng thái gõ
      vi.advanceTimersByTime(2500);
      await nextTick();
      expect(wrapper.find('.codelab-caret').exists()).toBe(true);

      vi.useRealTimers();
      wrapper.unmount();
    });

    it('Tầng 5C: Rank Ladder 5 bậc — highlight rank hiện tại, huy hiệu empty state và 3 thống kê', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.rank-section').exists()).toBe(true);
      const tiers = wrapper.findAll('.rank-tier');
      expect(tiers.length).toBe(5);
      expect(wrapper.find('.rank-tier--current').exists()).toBe(true);
      expect(wrapper.find('.rank-tier--current').text()).toContain('Rank hiện tại');
      // Guest Lv 1 → bậc "Tập sự" được highlight + CTA đăng ký
      expect(wrapper.find('.rank-tier--current').text()).toContain('Tập sự');
      expect(wrapper.find('.rank-foot__cta').text()).toContain('Đăng ký để leo hạng');
      // Huy hiệu: guest chưa có → empty state; thống kê 3 mục
      expect(wrapper.find('.rank-empty').exists()).toBe(true);
      expect(wrapper.findAll('.rank-stat').length).toBe(3);
      wrapper.unmount();
    });

    it('Tầng 6: CTA Section có nút Tạo tài khoản miễn phí', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.cta-card').exists()).toBe(true);
      expect(wrapper.find('.landing-footer').exists()).toBe(false);
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

      mockGamification.achievements = [
        {
          id: 1,
          code: 'first-lesson',
          name: 'Bước chân đầu tiên',
          description: null,
          iconUrl: null,
          earnedAt: '2026-01-01T00:00:00Z',
        },
        {
          id: 2,
          code: 'streak-7',
          name: 'Tuần lửa',
          description: null,
          iconUrl: null,
          earnedAt: null,
        },
      ];
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

    // FIX H1 — streak off-by-one: công thức idx <= ((streak-1) % 7)
    it('Streak 1 ngày → chỉ ngày đầu (T2) trong tuần được sáng', () => {
      mockGamification.streakDays = 1;
      const wrapper = mount(HomeView);
      const dayChips = wrapper.findAll('.streak-card .w-8.h-8');
      expect(dayChips.length).toBe(7);
      const lit = dayChips.filter((c) => c.classes().includes('bg-orange-500/20'));
      expect(lit.length).toBe(1);
      expect(lit[0].text()).toBe('T2');
      wrapper.unmount();
    });

    it('Streak 7 ngày → cả 7 ngày trong tuần đều sáng', () => {
      mockGamification.streakDays = 7;
      const wrapper = mount(HomeView);
      const dayChips = wrapper.findAll('.streak-card .w-8.h-8');
      const lit = dayChips.filter((c) => c.classes().includes('bg-orange-500/20'));
      expect(lit.length).toBe(7);
      wrapper.unmount();
    });

    it('Streak 8 ngày → tuần mới reset, chỉ 1 ngày sáng', () => {
      mockGamification.streakDays = 8;
      const wrapper = mount(HomeView);
      const dayChips = wrapper.findAll('.streak-card .w-8.h-8');
      const lit = dayChips.filter((c) => c.classes().includes('bg-orange-500/20'));
      expect(lit.length).toBe(1);
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

    it('Rank Ladder: highlight đúng cấp bậc thật (Lv.4 → Chiến binh), huy hiệu đã mở và CTA leaderboard', () => {
      const wrapper = mount(HomeView);
      expect(wrapper.find('.rank-tier--current').text()).toContain('Chiến binh');
      expect(wrapper.find('.rank-foot__cta').text()).toContain('Xem bảng xếp hạng');
      // Chỉ huy hiệu đã mở (earnedAt) được hiển thị — bỏ huy hiệu khóa
      expect(wrapper.findAll('.rank-badge').length).toBe(1);
      expect(wrapper.find('.rank-badge__name').text()).toBe('Bước chân đầu tiên');
      expect(wrapper.find('.rank-empty').exists()).toBe(false);
      // Thống kê có dữ liệu lessonsTotal thật (20)
      expect(wrapper.find('.rank-stat__value').text()).toBe('44+');
      wrapper.unmount();
    });
  });
});
