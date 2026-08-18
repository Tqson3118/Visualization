/** Parse sandboxConfig JSON ({"demo":"binary-search"}) → demo id, trả null nếu không có/hỏng. */
export function parseSandboxDemo(sandboxConfig: string): string | null {
  if (!sandboxConfig) return null;
  try {
    const parsed = JSON.parse(sandboxConfig) as { demo?: unknown };
    return typeof parsed.demo === 'string' && parsed.demo.length > 0 ? parsed.demo : null;
  } catch {
    return null;
  }
}

/**
 * Parse sandboxConfig JSON ({"simulationKey":"sort.bubble"}) → catalog key mô phỏng
 * (nguồn D6b: backend /concepts/lessons/{id} trả sandboxConfig chứa simulationKey cho
 * node dạng LAB). Trả null nếu không có/hỏng.
 */
export function parseSandboxSimulationKey(sandboxConfig: string): string | null {
  if (!sandboxConfig) return null;
  try {
    const parsed = JSON.parse(sandboxConfig) as { simulationKey?: unknown };
    return typeof parsed.simulationKey === 'string' && parsed.simulationKey.length > 0
      ? parsed.simulationKey
      : null;
  } catch {
    return null;
  }
}
