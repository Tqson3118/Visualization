/** DTO dùng chung (API_REFERENCE §1.3, §3.11) */

/** Phản hồi phân trang chuẩn: { items, page, pageSize, total, totalPages } */
export interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
  totalPages: number;
}
