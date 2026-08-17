// @vitest-environment jsdom
import { describe, it, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mount } from '@vue/test-utils';
import { generateInsertionSortFrames } from '../algorithms/insertionSort';
import { enrichFramesWithIds } from '../helpers/sortingIdEnricher';
import { useVcrStore } from '../../vcr-player/store/useVcrStore';
import SortingVisualizerDispatcher from '../components/SortingVisualizerDispatcher.vue';

function sorted(a: number[]): number[] { return [...a].sort((x, y) => x - y); }

const CASES: Array<{ name: string; input: number[] }> = [
  { name: 'empty', input: [] },
  { name: 'single', input: [7] },
  { name: 'two sorted', input: [1, 2] },
  { name: 'two reverse', input: [2, 1] },
  { name: 'all equal', input: [5, 5, 5, 5] },
  { name: 'duplicates', input: [3, 3, 2, 1, 3] },
  { name: 'negatives', input: [-5, -1, -9, -3] },
  { name: 'mixed sign', input: [-4, 0, 7, -2, 10] },
  { name: 'already sorted', input: [1, 2, 3, 4, 5] },
  { name: 'reverse', input: [5, 4, 3, 2, 1] },
  { name: 'nearly sorted', input: [1, 3, 2, 4, 5] },
  { name: 'default 6', input: [45, 12, 85, 32, 9, 60] },
  { name: 'many dups', input: [2, 2, 1, 1, 2, 1] },
];

describe('Insertion Sort — full pipeline fuzz', () => {
  it.each(CASES)('$name: generator + enrich + VCR playback không lỗi và kết quả đúng', ({ input }) => {
    setActivePinia(createPinia());
    const vcr = useVcrStore();

    const frames = generateInsertionSortFrames(input);
    expect(frames.length).toBeGreaterThan(0);

    enrichFramesWithIds(frames);

    // Mọi frame: display khớp arrayState, đủ id, id hợp lệ
    for (const f of frames) {
      expect(f.arrayStateWithIds!.length).toBe(f.arrayState.length);
      expect(f.arrayStateWithIds!.map((i) => i.value)).toEqual(f.arrayState);
    }

    // BỘ ID BẤT BIẾN giữa mọi frame — không ghost id → transition-group không bao giờ
    // sinh cột thừa (lỗi "7 cột xuất hiện rồi biến mất")
    if (input.length > 0) {
      const firstIds = frames[0].arrayStateWithIds!.map((i) => i.id).sort((a, b) => a - b);
      for (const f of frames) {
        const ids = f.arrayStateWithIds!.map((i) => i.id).sort((a, b) => a - b);
        expect(ids).toEqual(firstIds);
        expect(new Set(ids).size).toBe(ids.length);
      }
    }

    // Frame cuối: mảng đã sort + đủ sortedIndices
    const last = frames[frames.length - 1];
    expect(last.arrayState).toEqual(sorted(input));
    expect(last.sortedIndices.length).toBe(Math.max(input.length, 0));
    for (const idx of last.sortedIndices) {
      expect(last.arrayState[idx]).toBe(sorted(input)[idx]);
    }

    // Mỗi frame sau frame trước chỉ đổi TỐI ĐA 1 vị trí (bước đơn giản, animation mượt)
    // — ngoại trừ insert (1 slot nhận key)
    for (let k = 1; k < frames.length; k++) {
      const a = frames[k - 1].arrayState;
      const b = frames[k].arrayState;
      let diff = 0;
      for (let i = 0; i < a.length; i++) if (a[i] !== b[i]) diff++;
      expect(diff).toBeLessThanOrEqual(2);
    }

    // VCR: nhét frames + chạy hết qua stepNext không lỗi
    vcr.playbackFrames = frames;
    vcr.reset();
    let guard = 0;
    while (vcr.currentFrameIndex < vcr.totalFrames - 1 && guard < frames.length * 2) {
      vcr.stepNext();
      guard++;
    }
    expect(vcr.isAtEnd).toBe(true);
  });

  it.each(CASES)('$name: dispatcher render từng frame thật không lỗi template', ({ input }) => {
    const frames = generateInsertionSortFrames(input);
    enrichFramesWithIds(frames);
    for (const frame of frames) {
      const wrapper = mount(SortingVisualizerDispatcher, {
        props: { frame },
        global: { stubs: { BaseIcon: { template: '<span />' } } },
      });
      expect(wrapper.findComponent({ name: 'InsertionSortVisualizer' }).exists()).toBe(true);
      if (input.length > 0) expect(wrapper.text().length).toBeGreaterThan(0);
    }
  });
});