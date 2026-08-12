import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import type { CodeRunSummary } from '@/api/codeRunner';

/** Store codeRunner theo SDD §3.2 — Module I (Code Runner, ADR-012: sandbox Web Worker client) */
export type RunState = 'idle' | 'running' | 'passed' | 'failed' | 'error';

export const useCodeRunnerStore = defineStore('codeRunner', () => {
  const editorCode = ref('');
  const runState = ref<RunState>('idle');
  const lastRun = ref<CodeRunSummary | null>(null);
  const submissions = ref<CodeRunSummary[]>([]);

  const isRunning = computed(() => runState.value === 'running');

  async function loadTemplate(key: string): Promise<void> {
    // TODO (task Module I): nạp template code từ engines catalog theo key
    void key;
    return Promise.reject(new Error('TODO: codeRunnerStore.loadTemplate chưa triển khai'));
  }

  async function run(): Promise<void> {
    // TODO (task Module I): chạy trong sandbox Web Worker client (ADR-012)
    return Promise.reject(new Error('TODO: codeRunnerStore.run chưa triển khai'));
  }

  async function submit(): Promise<void> {
    // TODO (task Module I): nộp → codeRunnerApi.submitCode
    return Promise.reject(new Error('TODO: codeRunnerStore.submit chưa triển khai'));
  }

  async function fetchHistory(): Promise<void> {
    // TODO: submissions.value = await codeRunnerApi.fetchMyCodeSubmissions(...)
    return Promise.reject(new Error('TODO: codeRunnerStore.fetchHistory chưa triển khai'));
  }

  return {
    editorCode,
    runState,
    lastRun,
    submissions,
    isRunning,
    loadTemplate,
    run,
    submit,
    fetchHistory,
  };
});
