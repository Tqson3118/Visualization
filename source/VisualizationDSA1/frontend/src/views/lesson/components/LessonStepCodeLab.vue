<template>
  <div class="lesson-step-codelab flex flex-col lg:flex-row h-full w-full bg-bg-secondary overflow-hidden text-text-primary font-sans">
    
    <div class="w-full lg:w-1/2 h-full flex flex-col border-r border-border-subtle bg-bg-secondary overflow-hidden">
      
      <div class="flex border-b border-border-subtle bg-bg-secondary px-4 shrink-0">
        <button
          v-for="tab in problemTabs"
          :key="tab.id"
          @click="activeTab = tab.id"
          class="py-3 px-4 text-xs font-bold transition-all border-b-2 cursor-pointer"
          :class="activeTab === tab.id ? 'border-accent text-accent' : 'border-transparent text-text-muted hover:text-text-primary'"
        >
          {{ tab.name }}
          <span v-if="tab.badge" class="ml-1 px-1.5 py-0.5 rounded-full text-[10px] bg-accent/20 text-accent">{{ tab.badge }}</span>
        </button>
      </div>

      
      <div v-show="activeTab === 'problem'" class="flex-1 overflow-y-auto p-5 space-y-4">
        <div class="flex items-center justify-between border-b border-border-subtle pb-3">
          <div>
            <div class="flex items-center gap-1.5 text-xs font-bold text-accent uppercase tracking-wider">
              <svg class="w-4 h-4 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
              </svg>
              <span>Step {{ stepNumber }} / {{ totalSteps }} — Code Lab</span>
            </div>
            <h2 class="text-lg font-extrabold text-text-primary mt-0.5">{{ problemTitle }}</h2>
          </div>
          <span class="px-2.5 py-1 rounded-full text-[10px] font-bold bg-accent-green/80 text-accent-green border border-accent-green/30">
            Easy
          </span>
        </div>

        <div class="text-xs leading-relaxed text-text-secondary space-y-3">
          <div v-if="codelabDescription" class="whitespace-pre-wrap">{{ codelabDescription }}</div>
          <p v-else>Thực hành code theo đề bài của bài học. Chạy Testcases để kiểm tra kết quả của bạn.</p>
          <div v-for="(ex, idx) in codelabExamples" :key="idx" class="p-3 rounded-xl bg-bg-secondary border border-border-subtle space-y-1">
            <span class="text-[10px] font-bold text-text-muted uppercase font-mono">Example {{ idx + 1 }}:</span>
            <div class="font-mono text-xs text-accent">Input: {{ ex.input }}</div>
            <div class="font-mono text-xs text-accent-green">Output: {{ ex.expectedOutput }}</div>
          </div>
        </div>

        
        <div class="pt-2 border-t border-border-subtle">
          <span class="text-[11px] font-bold text-text-muted uppercase">Performance Limits:</span>
          <ul class="text-xs text-text-muted list-disc list-inside mt-1 space-y-0.5 font-mono">
            <li>N ≤ 1000 elements</li>
            <li>Time Limit ≤ 1000ms</li>
            <li>Memory Limit ≤ 128MB</li>
          </ul>
        </div>
      </div>

      
      <div v-show="activeTab === 'testcases'" class="flex-1 overflow-y-auto p-5 space-y-3">
        <h3 class="text-xs font-bold uppercase text-text-muted">Testcases ({{ testCaseResults.length }} tests)</h3>
        <div v-for="(tc, idx) in sampleTestcases" :key="idx" class="p-3.5 rounded-xl bg-bg-secondary border border-border-subtle space-y-2 text-xs">
          <div class="flex items-center justify-between">
            <span class="font-bold text-text-secondary">Testcase #{{ idx + 1 }} {{ tc.isHidden ? '(Hidden)' : '' }}</span>
            <span v-if="testResults[idx]" :class="testResults[idx].passed ? 'text-accent-green' : 'text-accent-red'" class="font-bold text-[11px]">
              {{ testResults[idx].passed ? '✓ PASSED' : '✕ FAILED' }}
            </span>
          </div>
          <div v-if="!tc.isHidden" class="font-mono text-[11px] text-text-muted">
            <div>Input: <span class="text-accent">{{ tc.input }}</span></div>
            <div>Expected: <span class="text-accent-green">{{ tc.expectedOutput }}</span></div>
          </div>
          <div v-else class="text-[11px] text-text-muted italic">
            Hidden testcase used for accuracy evaluation on Submit.
          </div>
        </div>
        <div v-if="testResults.length === 0" class="text-text-muted text-xs italic text-center py-8">
          Run or Submit your code to see test results.
        </div>
      </div>

      
      <div v-show="activeTab === 'hints'" class="flex-1 overflow-y-auto p-5 space-y-3">
        <h3 class="text-xs font-bold uppercase text-text-muted">Tiered Hints</h3>
        <div
          v-for="(hint, idx) in availableHints"
          :key="idx"
          class="p-4 rounded-xl bg-bg-secondary border border-border-subtle space-y-2"
        >
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-1.5 text-xs font-bold text-accent">
              <svg class="w-4 h-4 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 01-2 2h-0a2 2 0 01-2-2v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
              </svg>
              <span>Hint #{{ idx + 1 }}</span>
            </div>
            <button @click="toggleHint(idx)" class="text-[11px] text-text-muted hover:text-white cursor-pointer">
              {{ revealedHints[idx] ? 'Ẩn' : 'Xem' }}
            </button>
          </div>
          <p v-if="revealedHints[idx]" class="text-xs text-text-secondary leading-relaxed whitespace-pre-wrap">
            {{ hint.content }}
          </p>
        </div>
        <div v-if="availableHints.length === 0" class="text-center py-8 text-text-disabled text-xs">
          Chưa có gợi ý cho bài này.
        </div>
      </div>

      
      <div v-show="activeTab === 'ranking'" class="flex-1 overflow-y-auto p-5 space-y-3">
        <h3 class="text-xs font-bold uppercase text-text-muted">Performance Leaderboard</h3>
        <div v-if="leaderboard.length > 0" class="space-y-2">
          <div v-for="(r, idx) in leaderboard" :key="r.id" class="flex items-center justify-between p-3 rounded-xl bg-bg-secondary border border-border-subtle text-xs">
            <div class="flex items-center gap-3">
              <span class="w-5 h-5 rounded-full bg-bg-surface text-text-secondary font-bold text-[10px] flex items-center justify-center">
                #{{ idx + 1 }}
              </span>
              <span class="font-bold text-text-primary">{{ r.username }}</span>
            </div>
            <div class="flex items-center gap-4 text-[11px] font-mono">
              <span class="text-accent">{{ r.runtimeMs }}ms</span>
              <span class="text-accent-green">{{ r.memoryMb }}MB</span>
              <span class="text-accent-yellow font-bold">{{ r.score }} XP</span>
            </div>
          </div>
        </div>
        <div v-else class="text-center py-10 text-text-disabled text-xs">
          Chưa có dữ liệu xếp hạng.
        </div>
      </div>
    </div>

    
    <div class="w-full lg:w-1/2 h-full flex flex-col bg-bg-secondary">
      
      <div class="px-4 py-2.5 border-b border-border-subtle bg-bg-secondary flex items-center justify-between shrink-0">
        <div class="flex items-center gap-2">
          <span class="text-xs font-bold text-text-secondary font-mono">Solution.cs</span>
          <span class="text-[10px] px-2 py-0.5 rounded bg-bg-surface text-text-muted font-mono">C# .NET 9</span>
        </div>
        <button @click="resetCode" class="text-[11px] text-text-muted hover:text-white cursor-pointer">
          Reset to Starter Code
        </button>
      </div>

      
      <div class="flex-1 min-h-0" ref="editorContainer"></div>

      
      <div class="p-4 border-t border-border-subtle bg-bg-secondary flex items-center justify-between shrink-0">
        <div class="flex items-center gap-2">
          <button @click="runTestcases" :disabled="isRunning"
            class="px-4 py-2 bg-bg-surface hover:bg-bg-hover text-text-primary rounded-xl text-xs font-bold transition-all border border-border-subtle disabled:opacity-50 cursor-pointer">
            {{ isRunning ? 'Running...' : 'Run Testcases' }}
          </button>
        </div>
        <button @click="submitSolution" :disabled="isSubmitting"
          class="px-5 py-2 bg-accent hover:bg-accent text-white rounded-xl text-xs font-bold transition-all shadow-lg shadow-accent/30 disabled:opacity-50 cursor-pointer flex items-center gap-2">
          <span>Submit Solution</span>
          <svg class="w-4 h-4" fill="currentColor" viewBox="0 0 24 24">
            <path d="M2.01 21L23 12 2.01 3 2 10l15 2-15 2z" />
          </svg>
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, shallowRef, computed, watch } from 'vue';
import loader from '@monaco-editor/loader';
import type * as monaco from 'monaco-editor';
import { api } from '@/services/apiClient';

interface CodelabTestCaseResult {
  Passed: boolean;
  [key: string]: unknown;
}

interface CodelabRunResult {
  passed: boolean;
  status: string;
  errorMessage: string;
  runtimeMs: number;
  memoryBytes: number;
  testCaseResultsJson?: string;
  score?: number;
}

const props = withDefaults(defineProps<{
  problemTitle?: string;
  codelab?: any;
  stepNumber?: number;
  totalSteps?: number;
}>(), {
  problemTitle: 'Thực hành',
  codelab: null,
  stepNumber: 4,
  totalSteps: 4,
});

const emit = defineEmits<{
  (e: 'completeStep'): void;
}>();

const activeTab = ref('problem');
const revealedHints = ref<boolean[]>([]);
const isRunning = ref(false);
const isSubmitting = ref(false);
const editorContainer = ref<HTMLElement | null>(null);
const editorInstance = shallowRef<monaco.editor.IStandaloneCodeEditor | null>(null);

const problemTabs = computed<Array<{ id: string; name: string; badge?: string }>>(() => {
  const tabs: Array<{ id: string; name: string; badge?: string }> = [
    { id: 'problem', name: 'Problem' },
    { id: 'testcases', name: 'Testcases' },
  ];
  // Chỉ hiện tab Hints khi codelab có gợi ý thật
  if (availableHints.value.length > 0) {
    tabs.push({ id: 'hints', name: 'Hints' });
  }
  // Ẩn tab leaderboard khi chưa có dữ liệu xếp hạng (P2)
  if (leaderboard.value.length > 0) {
    tabs.push({ id: 'ranking', name: 'Leaderboard' });
  }
  return tabs;
});

const availableHints = computed<Array<{ content: string; xpCost?: number }>>(() => {
  if (Array.isArray(props.codelab?.hints)) return props.codelab.hints;
  return [];
});

function toggleHint(idx: number): void {
  const next = [...revealedHints.value];
  next[idx] = !next[idx];
  revealedHints.value = next;
}

const testCaseResults = ref<CodelabTestCaseResult[]>([]);

const defaultCode = computed(() => props.codelab?.initialCode || '');

const userCode = ref('');

// We don't watch props.codelab to reset the code dynamically after initialization 
// to prevent accidental overwrites when the user is typing.
userCode.value = defaultCode.value;

const sampleTestcases = computed<{input: string, expectedOutput: string, isHidden: boolean}[]>(() => {
  if (props.codelab && props.codelab.testCases) {
    return props.codelab.testCases;
  }
  return [];
});

const codelabDescription = computed(() => props.codelab?.description || '');
const codelabExamples = computed<Array<{ input: string; expectedOutput: string }>>(() => {
  const ex = props.codelab?.examples;
  return Array.isArray(ex) ? ex : [];
});

const leaderboard = ref<Array<{ id: string; username: string; runtimeMs: number; memoryMb: number; score: number }>>([]);

const testResults = ref<Array<{ passed: boolean }>>([]);

function resetCode(): void {
  if (editorInstance.value) {
    editorInstance.value.setValue(defaultCode.value);
  }
  userCode.value = defaultCode.value;
}

async function runTestcases(): Promise<void> {
  if (!props.codelab) return;
  isRunning.value = true;
  activeTab.value = 'testcases';
  try {
    const result = await api.post<CodelabRunResult>(`/codelabs/${props.codelab.id}/run`, {
      code: userCode.value,
      language: 'csharp'
    });
    if (result.testCaseResultsJson) {
      const parsedResults = JSON.parse(result.testCaseResultsJson) as CodelabTestCaseResult[];
      testResults.value = parsedResults.map((p) => ({ passed: p.Passed }));
      testCaseResults.value = parsedResults;
    }
  } catch (error) {
    console.error('Run failed', error);
  } finally {
    isRunning.value = false;
  }
}

async function submitSolution(): Promise<void> {
  if (!props.codelab) return;
  isSubmitting.value = true;
  activeTab.value = 'testcases';
  try {
    const result = await api.post<CodelabRunResult>(`/codelabs/${props.codelab.id}/submit`, {
      code: userCode.value,
      language: 'csharp'
    });
    if (result.testCaseResultsJson) {
      const parsedResults = JSON.parse(result.testCaseResultsJson) as CodelabTestCaseResult[];
      testResults.value = parsedResults.map((p) => ({ passed: p.Passed }));
      testCaseResults.value = parsedResults;
    }
    if (result.passed) {
      emit('completeStep');
    }
  } catch (error) {
    console.error('Submit failed', error);
  } finally {
    isSubmitting.value = false;
  }
}

onMounted(async () => {
  try {
    const monacoInstance = await loader.init();
    if (editorContainer.value) {
      editorInstance.value = monacoInstance.editor.create(editorContainer.value, {
        value: defaultCode.value,
        language: 'csharp',
        theme: 'vs-dark',
        automaticLayout: true,
        minimap: { enabled: false },
        fontSize: 13,
        fontFamily: "'Fira Code', 'JetBrains Mono', monospace",
        padding: { top: 16 },
        scrollBeyondLastLine: false,
        renderLineHighlight: 'all',
        lineNumbers: 'on',
        bracketPairColorization: { enabled: true },
        formatOnPaste: true,
        cursorBlinking: 'smooth',
        cursorSmoothCaretAnimation: 'on',
        smoothScrolling: true,
      });

      editorInstance.value?.onDidChangeModelContent(() => {
        userCode.value = editorInstance.value?.getValue() || '';
      });

      resizeHandler = () => editorInstance.value?.layout();
      window.addEventListener('resize', resizeHandler);
    }
  } catch (error) {
    console.error('Failed to initialize Monaco editor', error);
  }
});

let resizeHandler: (() => void) | null = null;

onUnmounted(() => {
  if (editorInstance.value) {
    editorInstance.value.dispose();
  }
  if (resizeHandler) {
    window.removeEventListener('resize', resizeHandler);
    resizeHandler = null;
  }
});


defineExpose({ userCode });
</script>

<style scoped>

</style>