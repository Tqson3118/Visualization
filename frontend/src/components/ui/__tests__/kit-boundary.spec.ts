import { describe, expect, it } from 'vitest';
import { readFileSync, readdirSync, statSync } from 'node:fs';
import { join, resolve } from 'node:path';

// Ranh giới kit (DESIGN — 1 UI kit duy nhất):
// app code KHÔNG được import trực tiếp các thư mục kebab-case (nội bộ shadcn).
// Chỉ file kit (components/ui/**) và bridge buttonVariants.ts được phép.
const SRC = resolve(process.cwd(), 'src');
const BARE = /^(badge|button|card|dialog|drawer|input|progress|select|skeleton|tabs|tooltip)$/;
const RE_BARE = /from\s+['"]@\/components\/ui\/(badge|button|card|dialog|drawer|input|progress|select|skeleton|tabs|tooltip)['"]/;
const RE_SUB = /from\s+['"]@\/components\/ui\/[a-z][\w-]*\//;

function walk(dir: string, acc: string[] = []): string[] {
  for (const name of readdirSync(dir)) {
    if (name === 'node_modules' || name === 'dist' || name.startsWith('.')) continue;
    const p = join(dir, name);
    const st = statSync(p);
    if (st.isDirectory()) walk(p, acc);
    else if (/\.(vue|ts)$/.test(name)) acc.push(p);
  }
  return acc;
}

describe('UI kit import boundary', () => {
  it('app code không import trực tiếp nội bộ kit (kebab-case dirs)', () => {
    const files = walk(SRC).filter((f) => !f.includes('node_modules'));
    const violations: string[] = [];
    for (const f of files) {
      const src = readFileSync(f, 'utf8');
      const rel = f.split('src').pop() ?? f;
      if (RE_BARE.test(src)) violations.push(`${rel}: import bare kebab module (dùng wrapper PascalCase)`);
      else if (RE_SUB.test(src) && !rel.includes('components\\ui') && !rel.includes('components/ui')) {
        violations.push(`${rel}: import subpath nội bộ kit`);
      }
    }
    expect(violations, `\n${violations.join('\n')}`).toEqual([]);
  });
});
