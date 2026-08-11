// @vitest-environment jsdom
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount, flushPromises } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import RoadmapReviewModal from '../../../components/rating/RoadmapReviewModal.vue';
import { roadmapApi } from '../../../services/roadmapApi';
import { useToastStore } from '../../../composables/useToast';

vi.mock('../../../services/roadmapApi', () => ({
  roadmapApi: {
    submitReview: vi.fn(),
    getStats: vi.fn(),
  },
}));

const mockedSubmitReview = vi.mocked(roadmapApi.submitReview);

describe('RoadmapReviewModal — modal chấm sao 1-5 (G3.9.5)', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  function mountModal() {
    return mount(RoadmapReviewModal, {
      props: { show: true, roadmapId: 'roadmap-1' },
    });
  }

  it('hiển thị 5 sao và nút "Gửi đánh giá"', () => {
    const wrapper = mountModal();
    const stars = wrapper.findAll('button[aria-label^="Đánh giá"]');
    expect(stars).toHaveLength(5);
    expect(wrapper.text()).toContain('Gửi đánh giá');
  });

  it('chọn sao 5 → gửi POST đúng body { rating: 5 }', async () => {
    mockedSubmitReview.mockResolvedValueOnce({
      id: 'rev-1',
      roadmapId: 'roadmap-1',
      rating: 5,
      createdAt: '2026-08-06T',
    });
    const wrapper = mountModal();

    await wrapper.get('button[aria-label="Đánh giá 5 sao"]').trigger('click');
    await wrapper.find('button.bg-gradient-to-r').trigger('click');

    await flushPromises();

    expect(mockedSubmitReview).toHaveBeenCalledTimes(1);
    expect(mockedSubmitReview).toHaveBeenCalledWith('roadmap-1', 5);
    expect(wrapper.emitted('submitted')).toBeTruthy();
  });

  it('chưa chọn sao → nút gửi bị disabled, không gọi API', async () => {
    const wrapper = mountModal();

    // Nút gửi là button có class gradient
    const submit = wrapper.find('button.bg-gradient-to-r');
    expect(submit.attributes('disabled')).toBeDefined();

    await submit.trigger('click');
    await flushPromises();
    expect(mockedSubmitReview).not.toHaveBeenCalled();
  });

  it('nhận 403 ROADMAP_NOT_COMPLETED → hiện toast lỗi, không emit submitted', async () => {
    mockedSubmitReview.mockRejectedValueOnce({ error: 'ROADMAP_NOT_COMPLETED', message: 'Bạn cần hoàn thành roadmap trước khi đánh giá.' });
    const wrapper = mountModal();

    await wrapper.get('button[aria-label="Đánh giá 4 sao"]').trigger('click');
    await wrapper.find('button.bg-gradient-to-r').trigger('click');
    await flushPromises();

    const toasts = useToastStore().toasts;
    const errorToast = toasts.find(t => t.type === 'error');
    expect(errorToast).toBeTruthy();
    expect(errorToast?.message).toContain('hoàn thành roadmap');
    expect(wrapper.emitted('submitted')).toBeFalsy();
  });

  it('nhận 409 ALREADY_REVIEWED → hiện toast "đã đánh giá", không gửi lại', async () => {
    mockedSubmitReview.mockRejectedValueOnce({ error: 'ALREADY_REVIEWED', message: 'Bạn đã đánh giá roadmap này.' });
    const wrapper = mountModal();

    await wrapper.get('button[aria-label="Đánh giá 3 sao"]').trigger('click');
    await wrapper.find('button.bg-gradient-to-r').trigger('click');
    await flushPromises();

    const toasts = useToastStore().toasts;
    expect(toasts.some(t => t.type === 'error' && t.message.includes('đã đánh giá'))).toBe(true);
    expect(wrapper.emitted('submitted')).toBeFalsy();
  });
});
