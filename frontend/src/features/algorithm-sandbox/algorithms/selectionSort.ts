import type { SortFrame } from '../types/sorting.types';

export function generateSelectionSortFrames(inputArray: number[]): SortFrame[] {
  const frames: SortFrame[] = [];
  const arr = [...inputArray];
  const n = arr.length;
  const sortedIndices: number[] = [];
  let step = 0;
  let comparisons = 0;
  let swaps = 0;

  frames.push({
    stepIndex: step++,
    arrayState: [...arr],
    comparingIndices: null,
    pivotIndex: null,
    swappedIndices: null,
    sortedIndices: [],
    selectionMinIndex: null,
    description: 'Khởi tạo mảng dữ liệu đầu vào',
    algorithm: 'selection',
    variables: { i: '-', j: '-', minIdx: '-', comparisons: 0, swaps: 0 },
  });

  for (let i = 0; i < n - 1; i++) {
    let minIdx = i;

    frames.push({
      stepIndex: step++,
      arrayState: [...arr],
      comparingIndices: null,
      pivotIndex: null,
      swappedIndices: null,
      sortedIndices: [...sortedIndices],
      selectionMinIndex: minIdx,
      description: `minIdx = i = ${i} — coi arr[${i}]=${arr[i]} là phần tử nhỏ nhất trong đoạn`,
      algorithm: 'selection',
      variables: { i, j: '-', minIdx, comparisons, swaps },
    });

    for (let j = i + 1; j < n; j++) {
      comparisons++;
      frames.push({
        stepIndex: step++,
        arrayState: [...arr],
        comparingIndices: [minIdx, j],
        pivotIndex: null,
        swappedIndices: null,
        sortedIndices: [...sortedIndices],
        selectionMinIndex: minIdx,
        description: `So sánh arr[${j}]=${arr[j]} với arr[${minIdx}]=${arr[minIdx]}`,
        algorithm: 'selection',
        variables: { i, j, minIdx, comparisons, swaps },
      });

      if (arr[j] < arr[minIdx]) {
        const old = minIdx;
        minIdx = j;
        frames.push({
          stepIndex: step++,
          arrayState: [...arr],
          comparingIndices: [minIdx, j],
          pivotIndex: null,
          swappedIndices: null,
          sortedIndices: [...sortedIndices],
          selectionMinIndex: minIdx,
          description: `arr[${j}]=${arr[j]} < arr[${old}]=${arr[old]} → minIdx = ${j}`,
          algorithm: 'selection',
          variables: { i, j, minIdx, comparisons, swaps },
        });
      }
    }

    if (minIdx !== i) {
      [arr[i], arr[minIdx]] = [arr[minIdx], arr[i]];
      swaps++;
      frames.push({
        stepIndex: step++,
        arrayState: [...arr],
        comparingIndices: null,
        pivotIndex: null,
        swappedIndices: [i, minIdx],
        sortedIndices: [...sortedIndices],
        selectionMinIndex: minIdx,
        description: `Hoán vị: arr[${i}]↔arr[${minIdx}] → [${arr[i]}, ${arr[minIdx]}]`,
        algorithm: 'selection',
        variables: { i, j: '-', minIdx, comparisons, swaps },
      });
    } else {
      frames.push({
        stepIndex: step++,
        arrayState: [...arr],
        comparingIndices: null,
        pivotIndex: null,
        swappedIndices: null,
        sortedIndices: [...sortedIndices],
        selectionMinIndex: minIdx,
        description: `minIdx = i = ${i} → không cần hoán vị`,
        algorithm: 'selection',
        variables: { i, j: '-', minIdx, comparisons, swaps },
      });
    }

    sortedIndices.push(i);
    frames.push({
      stepIndex: step++,
      arrayState: [...arr],
      comparingIndices: null,
      pivotIndex: null,
      swappedIndices: null,
      sortedIndices: [...sortedIndices],
      selectionMinIndex: null,
      description: `arr[${i}] = ${arr[i]} đã yên vị ✓`,
      algorithm: 'selection',
      variables: { i, j: '-', minIdx: '-', comparisons, swaps },
    });
  }

  if (n > 0) sortedIndices.push(n - 1);
  frames.push({
    stepIndex: step++,
    arrayState: [...arr],
    comparingIndices: null,
    pivotIndex: null,
    swappedIndices: null,
    sortedIndices: [...sortedIndices],
    selectionMinIndex: null,
    description: '✅ Selection Sort hoàn thành! Mảng đã được sắp xếp tăng dần.',
    algorithm: 'selection',
    variables: { i: '-', j: '-', minIdx: '-', comparisons, swaps },
  });

  return frames;
}