// @vitest-environment jsdom
// Stress toàn cục: MỌI thuật toán sorting (9) + MỌI chế độ stack/queue (4)
// — sinh frames → enrich → VCR chạy hết → xác nhận không lỗi + kết quả đúng.
import { describe, it, expect } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { useVcrStore } from '../../features/vcr-player/store/useVcrStore';
import { useSortingAnimation } from '../../features/algorithm-sandbox/composables/useSortingAnimation';
import type { SortAlgorithm } from '../../features/algorithm-sandbox/types/sorting.types';
import { generateDsSteps, generateRandomOps, type DsMode } from './stackQueueEngine';

function sorted(a: number[]): number[] { return [...a].sort((x, y) => x - y); }

describe('FINAL: mọi sandbox chạy animation không lỗi', () => {
  it('Sorting: 9 thuật toán × 25 mảng ngẫu nhiên → VCR playback hết frames, kết quả đúng', () => {
    setActivePinia(createPinia());
    const vcr = useVcrStore();
    const allAlgos: SortAlgorithm[] = ['bubble', 'selection', 'insertion', 'quick', 'merge', 'heap', 'radix', 'counting', 'bucket'];

    for (const algo of allAlgos) {
      for (let t = 0; t < 25; t++) {
        const n = 4 + Math.floor(Math.random() * 12);
        const input = Array.from({ length: n }, () => Math.floor(Math.random() * 85) + 5);
        const sorting = useSortingAnimation();
        vcr.rawInputArray = input.join(', ');
        sorting.selectAlgorithm(algo);

        const frames = sorting.sortFrames.value;
        expect(frames.length).toBeGreaterThan(0);
        expect(frames.every((f) => f.algorithm === algo)).toBe(true);

        // Mọi frame display khớp arrayState (id-enrich không lỗi)
        for (const f of frames) {
          expect(f.arrayStateWithIds!.length).toBe(f.arrayState.length);
          expect(f.arrayStateWithIds!.map((i) => i.value)).toEqual(f.arrayState);
        }

        // Frame cuối đã sort
        expect(frames[frames.length - 1].arrayState).toEqual(sorted(input));

        // VCR chạy hết
        vcr.playbackFrames = frames;
        vcr.reset();
        let guard = 0;
        while (vcr.currentFrameIndex < vcr.totalFrames - 1 && guard < frames.length * 2) {
          vcr.stepNext();
          guard++;
        }
        expect(vcr.isAtEnd).toBe(true);
      }
    }
  });

  it('Stack/Queue: 4 chế độ × 50 chuỗi ngẫu nhiên → steps hợp lệ, chạy hết VCR', () => {
    setActivePinia(createPinia());
    const vcr = useVcrStore();
    const modes: DsMode[] = ['stack', 'queue', 'circular', 'deque'];

    for (const mode of modes) {
      for (let t = 0; t < 50; t++) {
        const cap = 3 + Math.floor(Math.random() * 7);
        const ops = generateRandomOps(mode, cap);
        const steps = generateDsSteps(mode, ops, cap);

        expect(steps.length).toBeGreaterThan(0);
        for (const s of steps) {
          expect(s.cells.length).toBe(cap);
          expect(s.stepIndex).toBeGreaterThanOrEqual(0);
          expect(typeof s.indices).toBe('object');
          // display không có giá trị NaN/undefined
          for (const c of s.cells) expect(c.val === null || typeof c.val === 'number').toBe(true);
        }
        // Bước cuối không bị treo giữa chừng (luôn có isFinal)
        expect(steps[steps.length - 1].isFinal).toBe(true);

        vcr.playbackFrames = steps as never;
        vcr.reset();
        let guard = 0;
        while (vcr.currentFrameIndex < vcr.totalFrames - 1 && guard < steps.length * 2) {
          vcr.stepNext();
          guard++;
        }
        expect(vcr.isAtEnd).toBe(true);
      }
    }
  });
});