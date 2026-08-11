// Nguồn duy nhất cho base URL của backend (P2.9)
// Đọc từ env VITE_API_BASE_URL; fallback localhost:5055 cho demo/dev offline.
export const API_BASE_URL: string = (import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5055').replace(/\/+$/, '');
