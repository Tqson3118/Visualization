import { describe, it, expect } from 'vitest';
import '@/engines/catalog';
import { getSimulation } from '@/engines/registry';

describe('QA bst-delete duplicate id check', () => {
  it('delete 50 (2 children) - scan steps for duplicate element ids', () => {
    const sim = getSimulation('tree.bst-delete');
    if (!sim) throw new Error('missing sim');
    const input = { kind: 'tree', data: { keys: [50, 30, 70, 20, 40, 60, 80], operation: 'delete', value: 50 } };
    const steps = sim.generate(input);
    const report: string[] = [];
    steps.forEach((s: any, i: number) => {
      const ids = s.structure.elements.map((e: any) => e.id);
      const dup = ids.filter((id: string, k: number) => ids.indexOf(id) !== k);
      if (dup.length) report.push(`step ${i + 1} (line ${s.pseudocodeLine}): dup [${[...new Set(dup)].join(', ')}] | ${s.explanation}`);
    });
    console.log('TOTAL steps:', steps.length);
    console.log(report.length ? report.join('\n') : 'NO duplicate ids found');
    for (const idx of [3, 4, 5, 6]) {
      if (idx < steps.length) {
        const els = steps[idx].structure.elements.map((e: any) => `${e.id}(${e.label},${e.status})`);
        console.log(`step ${idx + 1} elements: ${els.join(' | ')}`);
      }
    }
    expect(steps.length).toBeGreaterThan(0);
  });
});
