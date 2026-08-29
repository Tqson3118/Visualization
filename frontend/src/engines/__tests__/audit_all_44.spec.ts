import { describe, it } from 'vitest';
import { CATALOG } from '../catalog';
import { getSimulation } from '../registry';
import { writeFileSync } from 'node:fs';

describe('Audit All 44 Algorithms', () => {
  it('runs and outputs a complete breakdown of each algorithm', () => {
    const results = CATALOG.map((meta, i) => {
      const gen = getSimulation(meta.key);
      if (!gen) {
        return {
          id: i + 1,
          key: meta.key,
          title: meta.title,
          dataStructure: meta.dataStructure,
          category: meta.category,
          level: meta.level,
          complexity: meta.complexity,
          status: 'MISSING_GENERATOR',
        };
      }

      const defaultInput: Record<string, unknown> = {};
      for (const f of gen.inputSchema.fields) {
        defaultInput[f.name] = f.default;
      }

      try {
        const steps = gen.generate({ kind: gen.inputSchema.kind, data: defaultInput });
        return {
          id: i + 1,
          key: meta.key,
          title: meta.title,
          dataStructure: meta.dataStructure,
          category: meta.category,
          level: meta.level,
          complexity: meta.complexity,
          defaultInputKind: gen.inputSchema.kind,
          defaultInputValues: defaultInput,
          stepsCount: steps.length,
          pseudocodeLines: gen.pseudocode.length,
          pseudocodeSnippet: gen.pseudocode.slice(0, 3),
          sampleStep0: steps[0]?.explanation,
          sampleStepMid: steps[Math.floor(steps.length / 2)]?.explanation,
          sampleStepEnd: steps[steps.length - 1]?.explanation,
          structureKind: steps[0]?.structure.kind,
          status: 'SUCCESS',
        };
      } catch (err: any) {
        return {
          id: i + 1,
          key: meta.key,
          title: meta.title,
          dataStructure: meta.dataStructure,
          category: meta.category,
          level: meta.level,
          complexity: meta.complexity,
          status: 'GENERATE_ERROR',
          error: err.message,
        };
      }
    });

    writeFileSync(
      'C:/Users/Administrator/.gemini/antigravity/brain/3cb60dd3-0af9-4a1a-bc8f-b1917a78d0ae/scratch/all_44_audit_report.json',
      JSON.stringify(results, null, 2),
      'utf-8'
    );
  });
});
