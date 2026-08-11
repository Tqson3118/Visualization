import type { AlgorithmResult } from '../types/algorithm.types';
import { generateDummyResult } from './dummyGenerators';
import { API_BASE_URL } from '@/services/apiConfig';

const API_BASE = API_BASE_URL;

export async function executeDSAAlgorithm(
  algorithmId: string,
  inputData: number[],
): Promise<AlgorithmResult> {
  try {
    const response = await fetch(`${API_BASE}/api/v1/algorithms/execute`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept-Encoding': 'gzip, br',
      },
      body: JSON.stringify({
        algorithmId,
        dataType: 'integer-array',
        inputData,
      }),
    });

    if (!response.ok) {
      throw new Error(`HTTP Error ${response.status}`);
    }

    return (await response.json()) as AlgorithmResult;
  } catch {
    const demo = generateDummyResult(algorithmId, inputData);
    return { ...demo, demo: true };
  }
}
