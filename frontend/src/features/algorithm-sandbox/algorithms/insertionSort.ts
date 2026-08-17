import type { SortFrame } from '../types/sorting.types';

export function generateInsertionSortFrames(inputArray: number[]): SortFrame[] {
  const frames: SortFrame[] = [];
  const arr = [...inputArray];
  const n = arr.length;
  const sortedIndices: number[] = n > 0 ? [0] : [];
  let step = 0;
  let comparisons = 0;
  let shifts = 0;

  frames.push({
    stepIndex: step++,
    arrayState: [...arr],
    comparingIndices: null,
    pivotIndex: null,
    swappedIndices: null,
    sortedIndices: [...sortedIndices],
    insertionGapIndex: null,
    insertionKeyIndex: null,
    description: 'Khởi tạo mảng dữ liệu đầu vào — arr[0] đứng riêng đã là đoạn đã sắp xếp',
    algorithm: 'insertion',
    variables: { i: '-', j: '-', key: '-', comparisons: 0, shifts: 0 },
  });

  for (let i = 1; i < n; i++) {
    const key = arr[i];
    let j = i - 1;

    frames.push({
      stepIndex: step++,
      arrayState: [...arr],
      comparingIndices: null,
      pivotIndex: null,
      swappedIndices: null,
      sortedIndices: [...sortedIndices],
      insertionGapIndex: i,
      insertionKeyIndex: i,
      description: `Lấy key = arr[${i}] = ${key} — tìm vị trí chèn vào đoạn [0..${j}]`,
      algorithm: 'insertion',
      variables: { i, j, key, comparisons, shifts },
    });

    while (j >= 0 && arr[j] > key) {
      comparisons++;
      frames.push({
        stepIndex: step++,
        arrayState: [...arr],
        comparingIndices: [j, j + 1],
        pivotIndex: null,
        swappedIndices: null,
        sortedIndices: [...sortedIndices],
        insertionGapIndex: j + 1,
        insertionKeyIndex: j + 1,
        description: `So sánh arr[${j}]=${arr[j]} và key=${key}`,
        algorithm: 'insertion',
        variables: { i, j, key, comparisons, shifts },
      });

      arr[j + 1] = arr[j];
      shifts++;
      frames.push({
        stepIndex: step++,
        arrayState: [...arr],
        comparingIndices: null,
        pivotIndex: null,
        swappedIndices: [j, j + 1],
        sortedIndices: [...sortedIndices],
        insertionGapIndex: j,
        insertionKeyIndex: j,
        description: `arr[${j}]=${arr[j]} > key → dời arr[${j}] sang arr[${j + 1}]`,
        algorithm: 'insertion',
        variables: { i, j, key, comparisons, shifts },
      });
      j--;
    }

    if (j >= 0) {
      comparisons++;
      frames.push({
        stepIndex: step++,
        arrayState: [...arr],
        comparingIndices: null,
        pivotIndex: null,
        swappedIndices: null,
        sortedIndices: [...sortedIndices],
        insertionGapIndex: j + 1,
        insertionKeyIndex: j + 1,
        description: `arr[${j}]=${arr[j]} ≤ key=${key} → dừng dịch chuyển`,
        algorithm: 'insertion',
        variables: { i, j, key, comparisons, shifts },
      });
    }

    arr[j + 1] = key;
    for (let k = 0; k <= i; k++) {
      if (!sortedIndices.includes(k)) sortedIndices.push(k);
    }
    sortedIndices.sort((a, b) => a - b);
    frames.push({
      stepIndex: step++,
      arrayState: [...arr],
      comparingIndices: null,
      pivotIndex: null,
      swappedIndices: [j + 1, j + 1],
      sortedIndices: [...sortedIndices],
      insertionGapIndex: null,
      insertionKeyIndex: null,
      description: `Chèn key=${key} vào arr[${j + 1}] — đoạn [0..${i}] đã sắp xếp ✓`,
      algorithm: 'insertion',
      variables: { i, j: '-', key: '-', comparisons, shifts },
    });
  }

  frames.push({
    stepIndex: step++,
    arrayState: [...arr],
    comparingIndices: null,
    pivotIndex: null,
    swappedIndices: null,
    sortedIndices: [...sortedIndices],
    insertionGapIndex: null,
    insertionKeyIndex: null,
    description: '✅ Insertion Sort hoàn thành! Mảng đã được sắp xếp tăng dần.',
    algorithm: 'insertion',
    variables: { i: '-', j: '-', key: '-', comparisons, shifts },
  });

  return frames;
}
