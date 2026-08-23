import { client, getData } from './client';
import type { FavoriteDto } from './types';

/** Endpoint theo API_REFERENCE §4.9 (Favorites & Misc) */
export const FAVORITE_ENDPOINTS = {
  list: '/favorites',
  detail: (id: number) => `/favorites/${id}`,
} as const;

export async function fetchFavorites(): Promise<FavoriteDto[]> {
  return getData<FavoriteDto[]>({ method: 'GET', url: FAVORITE_ENDPOINTS.list });
}

export async function addFavorite(payload: { simKey: string; input?: unknown }): Promise<FavoriteDto> {
  return getData<FavoriteDto>({ method: 'POST', url: FAVORITE_ENDPOINTS.list, data: payload });
}

export async function removeFavorite(id: number): Promise<void> {
  await client.delete(FAVORITE_ENDPOINTS.detail(id));
}
