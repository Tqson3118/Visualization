import { computed, ref } from 'vue';

import type { PagedResponse } from '@/api/types';

export interface PaginationState<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  loading: boolean;
  error: string | null;
}

export type PageFetcher<T> = (page: number, pageSize: number) => Promise<PagedResponse<T>>;

/**
 * usePagination — SDD §3.6: state {items,page,pageSize,total,loading,error} + load/goToPage/refresh.
 * Dùng cho mọi danh sách (lessons, exercises, leaderboard, admin users...).
 */
export function usePagination<T>(fetcher: PageFetcher<T>, initialPageSize = 20) {
  const items = ref<T[]>([]) as { value: T[] };
  const page = ref(1);
  const pageSize = ref(initialPageSize);
  const total = ref(0);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const totalPages = computed(() => (total.value === 0 ? 0 : Math.ceil(total.value / pageSize.value)));

  async function load(nextPage = page.value): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      const response = await fetcher(nextPage, pageSize.value);
      items.value = response.items;
      total.value = response.total;
      page.value = response.page;
    } catch (err) {
      error.value = err instanceof Error ? err.message : String(err);
    } finally {
      loading.value = false;
    }
  }

  async function goToPage(nextPage: number): Promise<void> {
    if (nextPage < 1 || nextPage > totalPages.value) return;
    await load(nextPage);
  }

  async function refresh(): Promise<void> {
    await load(page.value);
  }

  return { items, page, pageSize, total, totalPages, loading, error, load, goToPage, refresh };
}
