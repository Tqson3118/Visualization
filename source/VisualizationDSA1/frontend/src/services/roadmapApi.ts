import { api } from './apiClient';

export interface RoadmapStatsDto {
  enrollCount: number;
  completionCount: number;
  reviewCount: number;
  avgRating: number | null;
  myRating: number | null;
  myCanReview: boolean;
}

export interface CreateRoadmapReviewResult {
  id: string;
  roadmapId: string;
  rating: number;
  createdAt: string;
}

export const roadmapApi = {
  getStats(id: string): Promise<RoadmapStatsDto> {
    return api.get(`/roadmaps/${id}/stats`);
  },

  submitReview(id: string, rating: number): Promise<CreateRoadmapReviewResult> {
    return api.post(`/roadmaps/${id}/review`, { rating });
  },
};
