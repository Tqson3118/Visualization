import { describe, it, expect, vi, beforeEach } from 'vitest';
import { roadmapApi } from '../../../services/roadmapApi';
import * as apiClient from '../../../services/apiClient';

vi.mock('../../../services/apiClient', () => ({
  api: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('roadmapApi — Roadmap Review & Stats (G3.9)', () => {
  const mockedApi = vi.mocked(apiClient.api);

  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getStats', () => {
    it('GET /roadmaps/{id}/stats', async () => {
      const stats = {
        enrollCount: 45,
        completionCount: 12,
        reviewCount: 8,
        avgRating: 4.5,
        myRating: null,
        myCanReview: false,
      };
      mockedApi.get.mockResolvedValueOnce(stats);

      const result = await roadmapApi.getStats('roadmap-1');

      expect(mockedApi.get).toHaveBeenCalledWith('/roadmaps/roadmap-1/stats');
      expect(result.enrollCount).toBe(45);
      expect(result.avgRating).toBe(4.5);
    });
  });

  describe('submitReview', () => {
    it('POST /roadmaps/{id}/review với body { rating }', async () => {
      mockedApi.post.mockResolvedValueOnce({ id: 'rev-1', roadmapId: 'roadmap-1', rating: 5, createdAt: '2026-08-06T' });

      const result = await roadmapApi.submitReview('roadmap-1', 5);

      expect(mockedApi.post).toHaveBeenCalledWith('/roadmaps/roadmap-1/review', { rating: 5 });
      expect(result.id).toBe('rev-1');
      expect(result.rating).toBe(5);
    });

    it('lan truyền lỗi 409 ALREADY_REVIEWED', async () => {
      mockedApi.post.mockRejectedValueOnce({
        error: 'ALREADY_REVIEWED',
        message: 'Bạn đã đánh giá roadmap này.',
      });

      await expect(roadmapApi.submitReview('roadmap-1', 4)).rejects.toMatchObject({ error: 'ALREADY_REVIEWED' });
    });

    it('lan truyền lỗi 403 ROADMAP_NOT_COMPLETED', async () => {
      mockedApi.post.mockRejectedValueOnce({
        error: 'ROADMAP_NOT_COMPLETED',
        message: 'Bạn cần hoàn thành roadmap trước khi đánh giá.',
      });

      await expect(roadmapApi.submitReview('roadmap-1', 3)).rejects.toMatchObject({ error: 'ROADMAP_NOT_COMPLETED' });
    });
  });
});
