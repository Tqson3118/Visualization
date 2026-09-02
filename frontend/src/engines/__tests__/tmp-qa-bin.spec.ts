import { describe, expect, it } from 'vitest';
import '../catalog';
import { getSimulation } from '../registry';

describe('A11: binary search unsorted disclosure', () => {
  it('unsorted input [9,1,7,3] target 3 - list all step explanations', () => {
    const gen = getSimulation('search.binary')!;
    const steps = gen.generate({ kind: 'array', data: { target: 3, values: [9, 1, 7, 3] } });
    steps.forEach((s, i) => {
      const arr = s.structure.elements.map(e => e.label).join(',');
      const st = s.structure.elements.map(e => e.status).filter(x => x !== 'default').join('|');
      console.log(`step ${i + 1} [line ${s.pseudocodeLine}] arr=[${arr}] status=${st} :: ${s.explanation}`);
    });
    expect(steps.length).toBeGreaterThan(0);
  });
  it('A12: avl-insert vars at final step (balance chip)', () => {
    const gen = getSimulation('tree.avl-insert')!;
    const steps = gen.generate({ kind: 'tree', data: { keys: [50, 30, 70, 20, 40], value: 10 } });
    const last = steps[steps.length - 1];
    console.log('AVL final vars:', JSON.stringify(last.variables));
    const rot = steps.filter(s => /xoay|rotation/i.test(s.explanation));
    console.log('rotation steps:', rot.map(s => `#${steps.indexOf(s) + 1} ${s.explanation.slice(0, 80)}`).join(' || '));
  });
  it('A13: structure.queue dequeue visual model (shift vs pointer)', () => {
    const gen = getSimulation('structure.queue')!;
    const steps = gen.generate({ kind: 'queue', data: { values: [5, 8, 13, 21] } });
    steps.forEach((s, i) => {
      const arr = s.structure.elements.map(e => `${e.label}(${e.status})`).join(' | ');
      console.log(`step ${i + 1} [line ${s.pseudocodeLine}] :: ${s.explanation.slice(0, 100)}`);
      console.log(`   cells: ${arr}`);
    });
  });
});
