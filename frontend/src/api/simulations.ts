import { client, getData } from './client';
import type { PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.5 */
export const SIMULATION_ENDPOINTS = {
  list: '/simulations',
  detail: (key: string) => `/simulations/${key}`,
  schema: (key: string) => `/simulations/${key}/schema`,
  demoRun: (key: string) => `/public/simulations/${key}/run`,
  benchmark: '/benchmarks/run',
} as const;

// ── DTO (API_REFERENCE §3.6) ──

export interface ComplexityInfo {
  best: string;
  average: string;
  worst: string;
  space: string;
}

export interface SimulationMetaDto {
  key: string;
  title: string;
  dataStructure: string;
  category: 'structure' | 'algorithm';
  level: 'basic' | 'advanced';
  complexity: ComplexityInfo;
  tags: string[];
  demoAllowed: boolean;
}

export interface SimulationDetailDto extends SimulationMetaDto {
  inputSchema: unknown; // InputSchema từ engines/core/types — nạp khi engine hoàn thiện
  pseudocode: string[];
}

// ── CRUD (API_REFERENCE §4.5) ──

export async function fetchSimulations(): Promise<PagedResponse<SimulationMetaDto>> {
  return getData<PagedResponse<SimulationMetaDto>>({ method: 'GET', url: SIMULATION_ENDPOINTS.list });
}

export async function fetchSimulation(key: string): Promise<SimulationDetailDto> {
  return getData<SimulationDetailDto>({ method: 'GET', url: SIMULATION_ENDPOINTS.detail(key) });
}

export async function fetchInputSchema(key: string): Promise<unknown> {
  return getData<unknown>({ method: 'GET', url: SIMULATION_ENDPOINTS.schema(key) });
}

export async function runDemoSimulation(key: string, input?: unknown): Promise<unknown> {
  return getData<unknown>({ method: 'GET', url: SIMULATION_ENDPOINTS.demoRun(key), params: input });
}

export { client };
