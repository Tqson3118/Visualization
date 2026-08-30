/**
 * verify-round2-bugs.mjs
 * Script xác minh độc lập 4 lỗi vừa rà soát được ở Vòng 2.
 */
import { buildGraphEdges } from '../src/engines/generators/helpers.js';
import { createStructureHashTableGenerator } from '../src/engines/generators/structure/structures.js';
import { createMergeGenerator } from '../src/engines/generators/sort/merge.js';

console.log('=== TEST 1: Grid Graph Edges (BUG-LOGIC-02) ===');
// Đồ thị grid 6 đỉnh = 2 hàng x 3 cột (hàng 0: 0,1,2; hàng 1: 3,4,5)
// Các cạnh dọc đúng phải là (0,3), (1,4), (2,5)
const gridEdges = buildGraphEdges({
  preset: 'grid',
  vertices: 6,
  directed: false,
  weighted: false,
  edges: 6,
  source: 0,
  target: null,
});
console.log('Grid edges generated for 2x3 grid:', gridEdges);
const hasVerticalEdge = gridEdges.some(([u, v]) => (u === 0 && v === 3) || (u === 1 && v === 4) || (u === 2 && v === 5));
console.log('Has vertical connections (0-3, 1-4, 2-5):', hasVerticalEdge);

console.log('\n=== TEST 2: HashTable Structure with 2 keys (BUG-LOGIC-03) ===');
const hashGen = createStructureHashTableGenerator();
const steps = hashGen.generate({ kind: 'hashtable', data: { keys: [10, 20], tableSize: 5 } });
const searchStep = steps.find(s => s.explanation.includes('search('));
console.log('Search step explanation with 2 keys:', searchStep?.explanation);

console.log('\n=== TEST 3: Merge Sort Done Status (BUG-LOGIC-04) ===');
const mergeGen = createMergeGenerator();
const mergeSteps = mergeGen.generate({ kind: 'array', data: { values: [5, 3, 8, 1] } });
const midStep = mergeSteps.find(s => s.explanation.includes('ghi t về a[0..1]'));
console.log('Mid step explanation:', midStep?.explanation);
console.log('Elements statuses at mid step:', midStep?.structure?.elements.map(e => `${e.label}:${e.status}`));
