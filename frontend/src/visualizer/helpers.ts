// visualizer/helpers.ts — tiện ích dùng chung cho shared visualizer (pure, test được).
import type { InputConfig, SimulationGenerator } from '@/engines/core/types';

/** Input mặc định từ inputSchema (bản pure — tương tự defaultInputFromSchema trong stores/simulation). */
export function defaultInputFromSchema(gen: SimulationGenerator): InputConfig {
  const data: Record<string, unknown> = {};
  for (const field of gen.inputSchema.fields) {
    data[field.name] = field.default;
  }
  return { kind: gen.inputSchema.kind, data };
}
