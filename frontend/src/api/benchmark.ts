import { getData } from './client';
import type { BenchmarkRequest, BenchmarkRunDto } from './types';

/** Endpoint theo API_REFERENCE §4.14 — POST /benchmarks/run (Màn 17 — Benchmark Lab) */
export const BENCHMARK_ENDPOINTS = {
  run: '/benchmarks/run',
} as const;

/**
 * Lưu kết quả đo benchmark lên server.
 * Lưu ý: việc ĐO (runMeasure — SDD §4.0.3 v2.5) chạy ngay trên client qua engines;
 * endpoint này chỉ LƯU kết quả để lưu vết/lịch sử (tầng 4 theo 19.9).
 */
export async function runBenchmark(payload: BenchmarkRequest): Promise<BenchmarkRunDto> {
  return getData<BenchmarkRunDto>({ method: 'POST', url: BENCHMARK_ENDPOINTS.run, data: payload });
}
