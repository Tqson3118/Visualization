import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import AdminGamificationSettingsTab from '@/components/admin/AdminGamificationSettingsTab.vue';
import * as adminApi from '@/api/admin';
import { useUiStore } from '@/stores/ui';

vi.mock('@/api/admin', () => ({
  fetchGamificationSettings: vi.fn(),
  updateGamificationSettings: vi.fn(),
  resetGamificationSettings: vi.fn(),
}));

describe('AdminGamificationSettingsTab.vue', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('renders settings inputs after loading from API', async () => {
    vi.mocked(adminApi.fetchGamificationSettings).mockResolvedValueOnce({
      theoryBaseXp: 50,
      quizBaseXp: 50,
      codelabBaseXp: 100,
      streakBonusXp: 20,
      heartsMaxFree: 10,
      heartsMaxPremium: 30,
      heartRegenMinutes: 30,
      sessionHours: 36,
    });

    const wrapper = mount(AdminGamificationSettingsTab);

    // Wait for async loadSettings
    await vi.waitFor(() => {
      expect(wrapper.find('#theory-xp').exists()).toBe(true);
    });

    const theoryInput = wrapper.find<HTMLInputElement>('#theory-xp');
    expect(theoryInput.element.value).toBe('50');

    const codelabInput = wrapper.find<HTMLInputElement>('#codelab-xp');
    expect(codelabInput.element.value).toBe('100');

    const freeHeartsInput = wrapper.find<HTMLInputElement>('#free-hearts');
    expect(freeHeartsInput.element.value).toBe('10');

    const premHeartsInput = wrapper.find<HTMLInputElement>('#premium-hearts');
    expect(premHeartsInput.element.value).toBe('30');
  });

  it('submits updated settings and shows toast notification', async () => {
    const ui = useUiStore();
    const showToastSpy = vi.spyOn(ui, 'showToast');

    vi.mocked(adminApi.fetchGamificationSettings).mockResolvedValueOnce({
      theoryBaseXp: 50,
      quizBaseXp: 50,
      codelabBaseXp: 100,
      streakBonusXp: 20,
      heartsMaxFree: 10,
      heartsMaxPremium: 30,
      heartRegenMinutes: 30,
      sessionHours: 36,
    });

    vi.mocked(adminApi.updateGamificationSettings).mockResolvedValueOnce({
      theoryBaseXp: 60,
      quizBaseXp: 50,
      codelabBaseXp: 120,
      streakBonusXp: 20,
      heartsMaxFree: 10,
      heartsMaxPremium: 30,
      heartRegenMinutes: 30,
      sessionHours: 36,
    });

    const wrapper = mount(AdminGamificationSettingsTab);

    await vi.waitFor(() => {
      expect(wrapper.find('#theory-xp').exists()).toBe(true);
    });

    const theoryInput = wrapper.find<HTMLInputElement>('#theory-xp');
    await theoryInput.setValue(60);

    const codelabInput = wrapper.find<HTMLInputElement>('#codelab-xp');
    await codelabInput.setValue(120);

    await wrapper.find('form').trigger('submit.prevent');

    expect(adminApi.updateGamificationSettings).toHaveBeenCalledWith(
      expect.objectContaining({
        theoryBaseXp: 60,
        codelabBaseXp: 120,
      }),
    );

    expect(showToastSpy).toHaveBeenCalledWith(
      expect.stringContaining('Đã lưu cấu hình Gamification thành công'),
      'success',
    );
  });
});
