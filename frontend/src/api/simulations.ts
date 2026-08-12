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

// ── Stub CRUD (body TODO) ──

export async function fetchSimulations(): Promise<SimulationMetaDto[]> {
  // TODO: getData({ method: 'GET', url: SIMULATION_ENDPOINTS.list })
  return Promise.reject(new Error('TODO: simulationsApi.fetchSimulations chưa triển khai'));
}

export async function fetchSimulation(key: string): Promise<SimulationDetailDto> {
  // TODO: getData({ method: 'GET', url: SIMULATION_ENDPOINTS.detail(key) })
  return Promise.reject(new Error('TODO: simulationsApi.fetchSimulation chưa triển khai'));
}

export async function fetchInputSchema(key: string): Promise<unknown> {
  // TODO: getData({ method: 'GET', url: SIMULATION_ENDPOINTS.schema(key) })
  return Promise.reject(new Error('TODO: simulationsApi.fetchInputSchema chưa triển khai'));
}

export async function runDemoSimulation(key: string, input?: unknown): Promise<unknown> {
  // TODO: getData({ method: 'GET', url: SIMULATION_ENDPOINTS.demoRun(key), params: input })
  return Promise.reject(new Error('TODO: simulationsApi.runDemoSimulation chưa triển khai'));
}
