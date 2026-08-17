// @vitest-environment jsdom
import { describe, it, expect } from 'vitest';
import { generateInsertionSortFrames } from '../algorithms/insertionSort';
import { enrichFramesWithIds } from '../helpers/sortingIdEnricher';

describe('Insertion random-array stress', () => {
  it('500 mảng ngẫu nhiên (4-15 phần tử, âm/trùng): bộ id bất biến, display đúng, không cột thừa', () => {
    for (let t = 0; t < 500; t++) {
      const n = 4 + Math.floor(Math.random() * 12);
      const input = Array.from({ length: n }, () => Math.floor(Math.random() * 60) - 20);
      const frames = generateInsertionSortFrames(input);
      enrichFramesWithIds(frames);

      const sorted = [...input].sort((a, b) => a - b);
      const firstIds = frames[0].arrayStateWithIds!.map((i) => i.id).sort((a, b) => a - b);
      for (const f of frames) {
        expect(f.arrayStateWithIds!.length).toBe(f.arrayState.length);
        expect(f.arrayStateWithIds!.map((i) => i.value)).toEqual(f.arrayState);
        const ids = f.arrayStateWithIds!.map((i) => i.id).sort((a, b) => a - b);
        expect(ids).toEqual(firstIds);
      }
      expect(frames[frames.length - 1].arrayState).toEqual(sorted);
    }
  });
});
