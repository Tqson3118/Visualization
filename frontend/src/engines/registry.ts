// engines/registry.ts — theo SDD §4.5 (Registry plugin — ADR-003)
import type { SimulationGenerator } from './core/types';

export type GeneratorFactory = () => SimulationGenerator;

const registry = new Map<string, GeneratorFactory>();

export function registerSimulation(key: string, factory: GeneratorFactory): void {
  registry.set(key, factory);
}

export function getSimulation(key: string): SimulationGenerator | undefined {
  const factory = registry.get(key);
  return factory ? factory() : undefined;
}

/** Tất cả mô phỏng đã đăng ký (mỗi lần gọi sinh instance mới) */
export function listSimulations(): SimulationGenerator[] {
  return Array.from(registry.values()).map((factory) => factory());
}

/**
 * TODO (task 2 — catalog): engines/catalog.ts phải khai báo mọi mô phỏng và
 * ĐỒNG BỘ key với shared/simulation-catalog.json (44 mô phỏng — SDD §4.5, §9.9).
 * CI so sánh 2 danh sách key → khác → fail build.
 */
