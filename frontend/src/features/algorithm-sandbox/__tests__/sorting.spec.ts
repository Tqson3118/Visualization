import { describe, it, expect } from 'vitest';
import { generateBubbleSortFrames } from '../algorithms/bubbleSort';
import { generateSelectionSortFrames } from '../algorithms/selectionSort';
import { generateInsertionSortFrames } from '../algorithms/insertionSort';
import { generateQuickSortFrames } from '../algorithms/quickSort';
import { generateMergeSortFrames } from '../algorithms/mergeSort';
import { generateHeapSortFrames } from '../algorithms/heapSort';
import { generateRadixSortFrames } from '../algorithms/radixSort';
import { generateBucketSortFrames } from '../algorithms/bucketSort';
import { enrichFramesWithIds } from '../helpers/sortingIdEnricher';

describe('Sprint 2: Sorting Algorithm Frame Generators', () => {
  const testArray = [5, 3, 8, 4, 2];

  describe('Bubble Sort Frame Generator', () => {
    it('should generate valid frames from input array', () => {
      const frames = generateBubbleSortFrames(testArray);
      expect(frames.length).toBeGreaterThan(0);


      expect(frames[0].arrayState).toEqual(testArray);
      expect(frames[0].algorithm).toBe('bubble');


      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([2, 3, 4, 5, 8]);
      expect(finalFrame.sortedIndices.length).toBe(testArray.length);
    });
  });

  describe('Selection Sort Frame Generator', () => {
    it('should track minIdx and end with sorted array', () => {
      const frames = generateSelectionSortFrames(testArray);
      expect(frames.length).toBeGreaterThan(0);

      expect(frames[0].arrayState).toEqual(testArray);
      expect(frames[0].algorithm).toBe('selection');

      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([2, 3, 4, 5, 8]);
      expect(finalFrame.sortedIndices.length).toBe(testArray.length);
      expect(frames.some((f) => f.variables?.minIdx !== undefined)).toBe(true);
    });

    it('should not mutate the input array', () => {
      const input = [5, 3, 8, 4, 2];
      generateSelectionSortFrames(input);
      expect(input).toEqual([5, 3, 8, 4, 2]);
    });

    it('should track selectionMinIndex during the inner scan', () => {
      const frames = generateSelectionSortFrames([5, 3, 8, 4, 2]);
      const scanFrames = frames.filter((f) => f.comparingIndices !== null);
      expect(scanFrames.length).toBeGreaterThan(0);
      for (const f of scanFrames) {
        expect(typeof f.selectionMinIndex).toBe('number');
        expect(f.selectionMinIndex).toBe(f.variables?.minIdx);
      }
      // Frame khởi tạo / hoàn thành: không còn theo dõi min
      expect(frames[0].selectionMinIndex).toBeNull();
      expect(frames[frames.length - 1].selectionMinIndex).toBeNull();
    });
  });

  describe('Insertion Sort Frame Generator', () => {
    it('should shift elements and end with sorted array', () => {
      const frames = generateInsertionSortFrames(testArray);
      expect(frames.length).toBeGreaterThan(0);

      expect(frames[0].arrayState).toEqual(testArray);
      expect(frames[0].algorithm).toBe('insertion');

      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([2, 3, 4, 5, 8]);
      expect(finalFrame.sortedIndices.length).toBe(testArray.length);
      expect(frames.some((f) => f.variables?.key !== undefined)).toBe(true);
    });

    it('should not mutate the input array', () => {
      const input = [5, 3, 8, 4, 2];
      generateInsertionSortFrames(input);
      expect(input).toEqual([5, 3, 8, 4, 2]);
    });

    it('should display values matching arrayState on every frame after id enrichment (shift regression)', () => {
      const frames = generateInsertionSortFrames([45, 12, 85, 32, 9, 60]);
      enrichFramesWithIds(frames);
      for (const f of frames) {
        expect(f.arrayStateWithIds!.map((i) => i.value)).toEqual(f.arrayState);
      }
    });

    it('should mark insertionGapIndex walking left on each shift and clear it after insert', () => {
      // [4,3,2,1]: vòng i=3 (key=1) dịch 3 bước → gap 2 → 1 → 0 → insert tại 0
      const frames = generateInsertionSortFrames([4, 3, 2, 1]);
      const takeKey = frames.find((f) => f.description.startsWith('Lấy key') && f.variables?.i === 3);
      expect(takeKey?.insertionGapIndex).toBe(3);
      expect(takeKey?.insertionKeyIndex).toBe(3);

      const shiftFrames = frames.filter((f) => f.variables?.i === 3 && f.description.includes('dời arr'));
      const gaps = shiftFrames.map((f) => f.insertionGapIndex);
      expect(gaps).toEqual([2, 1, 0]);

      const insertFrame = frames.find((f) => f.description.includes('Chèn key') && f.variables?.i === 3);
      expect(insertFrame?.insertionGapIndex).toBeNull();
      expect(insertFrame?.insertionKeyIndex).toBeNull();
      expect(frames[0].insertionGapIndex).toBeNull();
      expect(frames[frames.length - 1].insertionGapIndex).toBeNull();
    });
  });

  const ENRICH_CASES: Array<{ input: number[] }> = [
    { input: [5, 3, 8, 4, 2] },
    { input: [3, 3, 2, 1] },
    { input: [4, 2, 4, 1] },
    { input: [1, 2, 3, 4, 5] },
    { input: [9] },
    { input: [7, 7, 7, 7] },
  ];

  describe('Display regression after id enrichment (duplicate-safe)', () => {
    it.each(ENRICH_CASES)('selection: display matches arrayState on every frame ($input)', ({ input }) => {
      const frames = generateSelectionSortFrames(input);
      enrichFramesWithIds(frames);
      for (const f of frames) {
        expect(f.arrayStateWithIds!.map((i) => i.value)).toEqual(f.arrayState);
      }
    });

    it.each(ENRICH_CASES)('insertion: display matches arrayState on every frame ($input)', ({ input }) => {
      const frames = generateInsertionSortFrames(input);
      enrichFramesWithIds(frames);
      for (const f of frames) {
        expect(f.arrayStateWithIds!.map((i) => i.value)).toEqual(f.arrayState);
      }
    });
  });

  describe('Quick Sort Frame Generator', () => {
    it('should generate partition steps and highlight pivot correctly', () => {
      const frames = generateQuickSortFrames(testArray);
      expect(frames.length).toBeGreaterThan(0);


      const partitionFrames = frames.filter(f => f.pivotIndex !== null);
      expect(partitionFrames.length).toBeGreaterThan(0);

      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([2, 3, 4, 5, 8]);
    });
  });

  describe('Merge Sort Frame Generator', () => {
    it('should generate divide and conquer frames and end with sorted array', () => {
      const frames = generateMergeSortFrames(testArray);
      expect(frames.length).toBeGreaterThan(0);

      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([2, 3, 4, 5, 8]);
    });
  });

  describe('Heap Sort Frame Generator', () => {
    it('should build max heap and extract elements iteratively to sort', () => {
      const frames = generateHeapSortFrames(testArray);
      expect(frames.length).toBeGreaterThan(0);

      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([2, 3, 4, 5, 8]);
    });
  });

  describe('Radix Sort Frame Generator', () => {
    const radixArr = [45, 12, 85, 32, 9, 60];

    it('should produce a sorted array in the final frame', () => {
      const frames = generateRadixSortFrames(radixArr);
      expect(frames.length).toBeGreaterThan(0);
      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([9, 12, 32, 45, 60, 85]);
    });

    it('should emit arrayStateWithIds on every frame with stable unique ids', () => {
      const frames = generateRadixSortFrames(radixArr);
      for (const frame of frames) {
        expect(frame.arrayStateWithIds).toBeDefined();
        expect(frame.arrayStateWithIds!.length).toBe(radixArr.length);

        expect(frame.arrayStateWithIds!.map(e => e.value)).toEqual(frame.arrayState);

        const ids = frame.arrayStateWithIds!.map(e => e.id);
        expect(new Set(ids).size).toBe(ids.length);
      }
    });

    it('should emit radixBucketsWithIds on every frame with stable ids matching bucket values', () => {
      const frames = generateRadixSortFrames(radixArr);
      for (const frame of frames) {
        expect(frame.radixBucketsWithIds).toBeDefined();
        expect(frame.radixBucketsWithIds!.length).toBe(10);
        for (let d = 0; d < 10; d++) {
          const bucketVals   = frame.radixBuckets![d];
          const bucketWithId = frame.radixBucketsWithIds![d];
          expect(bucketWithId.map(e => e.value)).toEqual(bucketVals);
        }
      }
    });

    it('should preserve FIFO order — earlier-distributed elements appear first in collect', () => {

      const arr = [21, 31, 41];
      const frames = generateRadixSortFrames(arr);
      const final = frames[frames.length - 1];

      expect(final.arrayState).toEqual([21, 31, 41]);
    });
  });

  describe('Bucket Sort Frame Generator', () => {
    const bucketArr = [45, 12, 85, 32, 9, 60];

    it('should produce a sorted array in the final frame', () => {
      const frames = generateBucketSortFrames(bucketArr);
      expect(frames.length).toBeGreaterThan(0);
      const finalFrame = frames[frames.length - 1];
      expect(finalFrame.arrayState).toEqual([9, 12, 32, 45, 60, 85]);
    });

    it('should emit stable unique ids on every frame matching array values', () => {
      const frames = generateBucketSortFrames(bucketArr);
      for (const frame of frames) {
        expect(frame.arrayStateWithIds).toBeDefined();
        expect(frame.arrayStateWithIds!.length).toBe(bucketArr.length);

        const ids = frame.arrayStateWithIds!.map(e => e.id);
        expect(new Set(ids).size).toBe(ids.length);
      }
    });

    it('should assign correct buckets matching value ranges', () => {
      const frames = generateBucketSortFrames([10, 30, 60, 90]);

      const distributeFinishedFrame = frames.find(f => f.description.includes('Phân phối thành công phần tử A[3]'));
      expect(distributeFinishedFrame).toBeDefined();

      const buckets = distributeFinishedFrame!.bucketSortBuckets!;
      expect(buckets[0]).toContain(10);
      expect(buckets[1]).toContain(30);
      expect(buckets[2]).toContain(60);
      expect(buckets[3]).toContain(90);
    });
  });
});
