<template>
  <div class="lesson-step-codelab flex flex-col lg:flex-row h-full w-full bg-vdsa-bg-secondary text-vdsa-secondary overflow-hidden font-sans text-[13px]">
    <!-- No Task -->
    <div v-if="!codelabTask" class="flex-1 flex items-center justify-center">
      <div class="text-center p-8">
        <h3 class="text-base font-bold text-white mb-2">Chưa có bài tập Code Lab</h3>
        <button @click="$emit('completeLesson')" class="mt-4 px-4 py-2 bg-vdsa-accent hover:bg-vdsa-accent-dark text-white rounded-xl font-semibold transition-colors shadow-vdsa-accent">Hoàn thành bài học</button>
      </div>
    </div>

    <template v-else>
      <!-- LEFT PANEL: Description / Hints -->
      <div class="w-full lg:w-1/2 h-full flex flex-col border-r border-vdsa-border bg-vdsa-bg-secondary">
        <!-- Tabs -->
        <div class="flex items-center bg-vdsa-surface border-b border-vdsa-border px-2 h-[40px] shrink-0 overflow-x-auto custom-scrollbar">
          <button
            @click="activeLeftTab = 'description'"
            class="flex items-center gap-1.5 px-3 py-1 text-[13px] font-semibold rounded-md transition-colors"
            :class="activeLeftTab === 'description' ? 'bg-vdsa-hover text-white' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover/50'"
          >
            <BaseIcon name="document" class="w-3.5 h-3.5 text-vdsa-purple-light" />
            Description
          </button>
          <button
            v-if="activeTask?.hints && activeTask.hints.length > 0"
            @click="activeLeftTab = 'hints'"
            class="flex items-center gap-1.5 px-3 py-1 text-[13px] font-semibold rounded-md transition-colors ml-1"
            :class="activeLeftTab === 'hints' ? 'bg-vdsa-hover text-white' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover/50'"
          >
            <BaseIcon name="info" class="w-3.5 h-3.5 text-vdsa-yellow" />
            Hints
          </button>
        </div>

        <!-- Left Content -->
        <div class="flex-1 overflow-y-auto p-5 custom-scrollbar">
          <div v-show="activeLeftTab === 'description'">
            <h1 class="text-2xl font-bold text-white mb-3">{{ currentTaskIndex + 1 }}. {{ problemTitle }}</h1>

            <div v-if="Array.isArray(codelabTask) && codelabTask.length > 1" class="flex gap-2 mb-4 overflow-x-auto pb-2 custom-scrollbar">
              <button
                v-for="(task, idx) in codelabTask"
                :key="task.id || idx"
                @click="currentTaskIndex = idx"
                class="px-3 py-1.5 rounded-lg text-[12px] font-bold transition-colors whitespace-nowrap flex items-center gap-2"
                :class="currentTaskIndex === idx ? 'bg-vdsa-accent text-white' : 'bg-vdsa-surface text-vdsa-muted hover:bg-vdsa-hover hover:text-white border border-vdsa-border'"
              >
                <span>Bài {{ idx + 1 }}: {{ task.title || `Task ${idx + 1}` }}</span>
                <BaseIcon v-if="completedTasks.has(idx)" name="check-circle" class="w-3.5 h-3.5 text-vdsa-green" />
              </button>
            </div>

            <div class="flex gap-2 mb-6">
              <span class="px-2.5 py-1 rounded-full bg-vdsa-green/20 text-vdsa-green text-[11px] font-semibold">Easy</span>
            </div>

            <div class="text-[14px] leading-relaxed text-vdsa-secondary space-y-4">
              <p class="whitespace-pre-line" v-html="formatMarkdown(activeTask?.description ?? '')"></p>
            </div>

            <!-- Examples -->
            <div class="mt-8 space-y-6">
              <div v-for="(tc, i) in sampleTestcases" :key="i">
                <p class="font-bold text-white mb-2 text-[14px]">Example {{ i + 1 }}:</p>
                <div class="bg-vdsa-surface border-l-2 border-vdsa-border p-3 rounded-r-lg font-mono text-[13px] text-vdsa-text">
                  <div class="mb-1"><span class="font-bold text-white">Input:</span> {{ tc.input }}</div>
                  <div><span class="font-bold text-white">Output:</span> {{ tc.expectedOutput }}</div>
                </div>
              </div>
            </div>

            <!-- Constraints -->
            <div class="mt-8 mb-8">
              <p class="font-bold text-white mb-2 text-[14px]">Constraints:</p>
              <ul class="list-disc pl-5 space-y-1.5 font-mono text-[13px] text-vdsa-muted">
                <li><code class="bg-vdsa-surface px-1.5 py-0.5 rounded text-vdsa-text">Hàm entry: {{ activeTask?.entryFunction || 'solution' }}</code></li>
                <li><code class="bg-vdsa-surface px-1.5 py-0.5 rounded text-vdsa-text">Thời gian chạy tối đa: 1500ms</code></li>
                <li><code class="bg-vdsa-surface px-1.5 py-0.5 rounded text-vdsa-text">Ngôn ngữ: JavaScript (ES6)</code></li>
              </ul>
            </div>
          </div>

          <div v-show="activeLeftTab === 'hints'" class="space-y-3">
             <div v-for="(hint, i) in activeTask?.hints ?? []" :key="i" class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-2">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-1.5 text-xs font-bold text-white">
                  <BaseIcon name="info" class="w-4 h-4 text-vdsa-yellow" />
                  <span>Hint {{ i + 1 }}</span>
                </div>
                <button @click="toggleHint(i)" class="text-[11px] text-vdsa-muted hover:text-white cursor-pointer">
                  {{ shownHints.includes(i) ? 'Hide' : 'Show' }}
                </button>
              </div>
              <p v-if="shownHints.includes(i)" class="text-[13px] text-vdsa-text leading-relaxed">{{ hint }}</p>
             </div>
          </div>
        </div>
      </div>

      <!-- RIGHT PANEL: Editor & Console -->
      <div class="w-full lg:w-1/2 h-full flex flex-col bg-vdsa-bg">
        <!-- Editor Header -->
        <div class="flex items-center justify-between px-3 h-[40px] bg-vdsa-surface border-b border-vdsa-border shrink-0">
          <div class="flex items-center gap-3">
            <span class="flex items-center gap-1.5 text-xs font-semibold text-vdsa-text bg-vdsa-hover px-2 py-1 rounded">
              <BaseIcon name="code" class="w-3.5 h-3.5 text-vdsa-green" />
              Code
            </span>
            <select
              :value="selectedLanguage"
              @change="onLanguageChange(($event.target as HTMLSelectElement).value as SupportedLanguage)"
              class="bg-vdsa-bg border border-vdsa-border text-xs text-white rounded px-2 py-1 focus:border-vdsa-accent outline-none font-mono cursor-pointer hover:border-vdsa-border-strong"
            >
              <option v-for="opt in languageOptions" :key="opt.value" :value="opt.value">
                {{ opt.label }}
              </option>
            </select>
          </div>
          <button @click="resetCode" class="text-vdsa-muted hover:text-white p-1 rounded hover:bg-vdsa-active transition-colors" title="Reset to Starter Code">
            <BaseIcon name="refresh" class="w-4 h-4" />
          </button>
        </div>

        <!-- Editor Container -->
        <div class="flex-1 min-h-[150px] relative">
          <div ref="editorContainer" class="absolute inset-0"></div>
        </div>

        <!-- Console / Testcases (Bottom Right) -->
        <div class="flex flex-col border-t border-vdsa-border bg-vdsa-bg transition-all duration-300" :class="isConsoleExpanded ? 'h-[280px]' : 'h-[40px]'">
          <!-- Console Header -->
          <div class="flex items-center px-3 h-[40px] bg-vdsa-surface border-b border-vdsa-border shrink-0 gap-4 cursor-pointer" @click="isConsoleExpanded = true">
            <button
              @click.stop="isConsoleExpanded = true; activeConsoleTab = 'testcase'"
              class="flex items-center gap-1.5 text-xs font-semibold transition-colors"
              :class="activeConsoleTab === 'testcase' && isConsoleExpanded ? 'text-white' : 'text-vdsa-muted hover:text-white'"
            >
              <BaseIcon name="check-circle" class="w-3.5 h-3.5 text-vdsa-green" />
              Testcase
            </button>
            <button
              @click.stop="isConsoleExpanded = true; activeConsoleTab = 'result'"
              class="flex items-center gap-1.5 text-xs font-semibold transition-colors"
              :class="activeConsoleTab === 'result' && isConsoleExpanded ? 'text-white' : 'text-vdsa-muted hover:text-white'"
            >
              <BaseIcon name="terminal" class="w-3.5 h-3.5 text-vdsa-purple-light" />
              Test Result
            </button>
            <div class="flex-1"></div>
            <button @click.stop="isConsoleExpanded = !isConsoleExpanded" class="text-vdsa-muted hover:text-white">
              <BaseIcon :name="isConsoleExpanded ? 'chevron-down' : 'chevron-up'" class="w-4 h-4" />
            </button>
          </div>

          <!-- Console Content -->
          <div v-show="isConsoleExpanded" class="flex-1 overflow-y-auto p-4 custom-scrollbar">
            <!-- Testcase Tab -->
            <div v-show="activeConsoleTab === 'testcase'">
               <div class="flex flex-wrap gap-2 mb-4">
                 <button
                    v-for="(tc, i) in sampleTestcases" :key="i"
                    @click="activeTestCaseIndex = i"
                    class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
                    :class="activeTestCaseIndex === i ? 'bg-vdsa-hover text-white' : 'text-vdsa-muted hover:bg-vdsa-hover/50'"
                 >
                    Case {{ i + 1 }}
                 </button>
               </div>
               <div v-if="sampleTestcases[activeTestCaseIndex]" class="space-y-4">
                 <div>
                   <div class="text-[12px] text-vdsa-muted mb-1 font-semibold">Input</div>
                   <div class="bg-vdsa-surface border border-vdsa-border rounded-lg p-2.5 font-mono text-[13px] text-white">
                     {{ sampleTestcases[activeTestCaseIndex].input }}
                   </div>
                 </div>
               </div>
            </div>

            <!-- Test Result Tab -->
            <div v-show="activeConsoleTab === 'result'">
               <div v-if="caseResults.length === 0 && !runError && !isRunning" class="text-vdsa-muted text-sm flex items-center justify-center h-full pt-10">
                  You must run your code first.
               </div>
               <div v-else-if="isRunning" class="text-vdsa-muted text-sm flex items-center gap-2 pt-10 pl-2">
                  <BaseIcon name="spinner" class="w-4 h-4 animate-spin text-vdsa-purple-light" /> Running...
               </div>
               <div v-else-if="runError" class="text-vdsa-red font-mono text-[13px] whitespace-pre-wrap pt-2">
                  {{ runError }}
               </div>
               <div v-else class="space-y-4">
                 <h3 class="text-[20px] font-bold" :class="allPassed ? 'text-vdsa-green' : 'text-vdsa-red'">
                   {{ allPassed ? 'Accepted' : 'Wrong Answer' }}
                 </h3>
                 <div class="flex flex-wrap gap-2 mb-4">
                   <button
                      v-for="(res, i) in caseResults" :key="i"
                      @click="activeTestResultIndex = i"
                      class="px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors flex items-center gap-1.5"
                      :class="activeTestResultIndex === i ? 'bg-vdsa-hover text-white' : 'text-vdsa-muted hover:bg-vdsa-hover/50'"
                   >
                      <div class="w-1.5 h-1.5 rounded-full" :class="res.passed ? 'bg-vdsa-green' : 'bg-vdsa-red'"></div>
                      Case {{ i + 1 }}
                   </button>
                 </div>

                 <div v-if="caseResults[activeTestResultIndex]" class="space-y-4 pb-4">
                   <div v-if="activeTask?.testCases[activeTestResultIndex].isHidden" class="text-vdsa-muted text-sm italic">
                     Hidden testcase
                   </div>
                   <template v-else>
                     <div>
                       <div class="text-[12px] text-vdsa-muted mb-1 font-semibold">Input</div>
                       <div class="bg-vdsa-surface border border-vdsa-border rounded-lg p-2.5 font-mono text-[13px] text-white">
                         {{ activeTask?.testCases[activeTestResultIndex].input }}
                       </div>
                     </div>
                     <div>
                       <div class="text-[12px] text-vdsa-muted mb-1 font-semibold">Expected Output</div>
                       <div class="bg-vdsa-surface border border-vdsa-border rounded-lg p-2.5 font-mono text-[13px] text-white">
                         {{ activeTask?.testCases[activeTestResultIndex].expectedOutput }}
                       </div>
                     </div>
                     <div>
                       <div class="text-[12px] text-vdsa-muted mb-1 font-semibold">Actual Output</div>
                       <div class="bg-vdsa-surface border border-vdsa-border rounded-lg p-2.5 font-mono text-[13px]" :class="caseResults[activeTestResultIndex].passed ? 'text-vdsa-green' : 'text-vdsa-red'">
                         {{ caseResults[activeTestResultIndex].actualOutput ?? caseResults[activeTestResultIndex].error }}
                       </div>
                     </div>
                   </template>
                 </div>
               </div>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="h-[48px] border-t border-vdsa-border bg-vdsa-surface flex items-center justify-between px-4 shrink-0">
          <div class="text-[12px] text-vdsa-muted flex items-center gap-2">
            <span v-if="runError" class="text-vdsa-red font-semibold">Error</span>
            <span v-else-if="allPassed && caseResults.length > 0" class="text-vdsa-green font-semibold">Accepted</span>
            <span v-else>Saved</span>
          </div>
          <div class="flex items-center gap-2">
            <button
              @click="runTestcases"
              :disabled="isRunning"
              class="px-5 py-1.5 rounded text-[13px] font-semibold transition-colors bg-vdsa-hover hover:bg-vdsa-active text-white disabled:opacity-50"
            >
              Run
            </button>
            <button
              @click="submitSolution"
              :disabled="isSubmitting || !allPassed"
              class="px-5 py-1.5 rounded text-[13px] font-semibold transition-colors bg-vdsa-green hover:bg-vdsa-accent-green-light text-white disabled:opacity-50 flex items-center gap-1.5"
            >
              <span>Submit</span>
              <BaseIcon name="check" class="w-3.5 h-3.5" v-if="allPassed"/>
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, shallowRef, computed, watch } from 'vue';
import * as monaco from 'monaco-editor';
import loader from '@monaco-editor/loader';
import BaseIcon from '../../../shared/components/BaseIcon.vue';
import type { CodeLabTask } from '../../../features/lesson/types/lesson.types';
import { runCodelabTask, type CodelabCaseResult } from '../../../features/lesson/utils/codelabExecutor';
import { submitCodelab } from '../../../features/lesson/services/lessonApi';
import { sanitizeHtml } from '@/utils/sanitize';

const props = withDefaults(defineProps<{
  problemTitle?: string;
  codelabTask?: CodeLabTask | CodeLabTask[] | null;
  /** Id exercise CODE trên máy chủ — bắt buộc nộp để MÁY CHỦ chấm lại (Jint) trước khi pass. */
  exerciseId?: string | null;
}>(), {
  problemTitle: 'Thực hành lập trình',
  codelabTask: null,
  exerciseId: null,
});

const emit = defineEmits<{
  (e: 'completeLesson'): void;
}>();

const currentTaskIndex = ref(0);
const completedTasks = ref<Set<number>>(new Set());
const activeTask = computed(() => {
  if (!props.codelabTask) return null;
  if (Array.isArray(props.codelabTask)) {
    return props.codelabTask[currentTaskIndex.value];
  }
  return props.codelabTask as CodeLabTask;
});

const activeLeftTab = ref<'description' | 'hints'>('description');
const activeConsoleTab = ref<'testcase' | 'result'>('testcase');
const isConsoleExpanded = ref(true);
const activeTestCaseIndex = ref(0);
const activeTestResultIndex = ref(0);

const shownHints = ref<number[]>([]);
const isRunning = ref(false);
const isSubmitting = ref(false);
const runError = ref<string | null>(null);
const caseResults = ref<CodelabCaseResult[]>([]);
const editorContainer = ref<HTMLElement | null>(null);
const editorInstance = shallowRef<monaco.editor.IStandaloneCodeEditor | null>(null);

type SupportedLanguage = 'javascript' | 'python' | 'cpp' | 'csharp';

const selectedLanguage = ref<SupportedLanguage>('javascript');

const languageOptions = [
  { value: 'javascript', label: 'JavaScript' },
  { value: 'python', label: 'Python 3' },
  { value: 'cpp', label: 'C++ (GCC)' },
  { value: 'csharp', label: 'C# (.NET)' },
];

function getStarterCodeForLanguage(lang: SupportedLanguage, baseCode: string, entryFn: string = 'solution'): string {
  if (lang === 'javascript') return baseCode;
  if (lang === 'python') {
    return `def ${entryFn}(*args):\n    # TODO: Triển khai giải thuật bằng Python\n    pass\n`;
  }
  if (lang === 'cpp') {
    return `#include <iostream>\n#include <vector>\n#include <string>\n\nusing namespace std;\n\n// TODO: Triển khai giải thuật C++\nauto ${entryFn}() {\n    return 0;\n}\n`;
  }
  if (lang === 'csharp') {
    return `using System;\nusing System.Collections.Generic;\n\npublic class Solution {\n    public static object ${entryFn}() {\n        // TODO: Triển khai giải thuật C#\n        return null;\n    }\n}\n`;
  }
  return baseCode;
}

function onLanguageChange(newLang: SupportedLanguage): void {
  selectedLanguage.value = newLang;
  if (editorInstance.value) {
    const model = editorInstance.value.getModel();
    if (model) {
      const monacoLang = newLang === 'csharp' ? 'csharp' : newLang === 'cpp' ? 'cpp' : newLang;
      monaco.editor.setModelLanguage(model, monacoLang);
    }
    const starter = getStarterCodeForLanguage(newLang, activeTask.value?.initialCode ?? '', activeTask.value?.entryFunction ?? 'solution');
    editorInstance.value.setValue(starter);
  }
}

const allPassed = computed(() =>
  caseResults.value.length > 0 && caseResults.value.length === (activeTask.value?.testCases.length ?? 0)
  && caseResults.value.every(r => r.passed)
);

const sampleTestcases = computed(() => (activeTask.value?.testCases ?? []).filter(tc => !tc.isHidden));

function formatMarkdown(text: string): string {
  if (!text) return '';
  const converted = text.replace(/`([^`]+)`/g, '<code class="bg-vdsa-surface text-vdsa-yellow px-1.5 py-0.5 rounded font-mono text-[13px]">$1</code>');
  return sanitizeHtml(converted);
}

function currentCode(): string {
  return editorInstance.value?.getValue() ?? '';
}

function resetCode(): void {
  if (!activeTask.value) return;
  const starter = getStarterCodeForLanguage(selectedLanguage.value, activeTask.value.initialCode, activeTask.value.entryFunction ?? 'solution');
  editorInstance.value?.setValue(starter);
  caseResults.value = [];
  runError.value = null;
  activeConsoleTab.value = 'testcase';
}

function toggleHint(i: number): void {
  const idx = shownHints.value.indexOf(i);
  if (idx >= 0) shownHints.value.splice(idx, 1);
  else shownHints.value.push(i);
}

async function runTestcases(): Promise<void> {
  if (!activeTask.value || isRunning.value) return;
  isRunning.value = true;
  runError.value = null;
  isConsoleExpanded.value = true;
  activeConsoleTab.value = 'result';
  activeTestResultIndex.value = 0;

  try {
    const result = await runCodelabTask(
      currentCode(),
      activeTask.value.testCases,
      activeTask.value.entryFunction ?? 'solution',
    );
    if (result.timedOut) {
      runError.value = result.error ?? 'Time Limit Exceeded.';
      caseResults.value = [];
    } else {
      caseResults.value = result.results ?? [];
      if (!result.ok && !result.results) {
        runError.value = result.error ?? 'Execution Error';
      }
    }
  } catch (err: unknown) {
    runError.value = err instanceof Error ? err.message : String(err);
  } finally {
    isRunning.value = false;
  }
}

async function submitSolution(): Promise<void> {
  if (isSubmitting.value || !allPassed.value) return;
  isSubmitting.value = true;
  try {
    // Nghiệp vụ 15/08: bài ASM ưu tiên MÁY CHỦ chấm code chạy (Jint).
    // Nếu máy chủ trả 403 (phân quyền) / 400 / lỗi mạng → fallback sang bộ test runner client-side.
    if (props.exerciseId && Array.isArray(props.codelabTask)) {
      try {
        const serverResult = await submitCodelab(props.exerciseId, currentCode(), activeTask.value?.id ?? '');
        if (serverResult.error) {
          runError.value = `Máy chủ chấm: ${serverResult.error}`;
          caseResults.value = [];
          activeConsoleTab.value = 'result';
          isConsoleExpanded.value = true;
          return;
        }
        if (typeof serverResult.total === 'number' && serverResult.total > 0 && (!serverResult.passed || serverResult.passed < serverResult.total)) {
          runError.value = `Wrong Answer — máy chủ chấm ${serverResult.passed}/${serverResult.total} test. Hãy kiểm tra code và thử lại.`;
          caseResults.value = (serverResult.results ?? []).map(r => ({
            input: '',
            expectedOutput: '',
            actualOutput: r.error ?? '',
            passed: r.passed,
            isHidden: true,
          }));
          activeConsoleTab.value = 'result';
          isConsoleExpanded.value = true;
          return;
        }
      } catch (err: unknown) {
        console.warn('Server code submit failed/skipped, falling back to client-side runner:', err);
      }
    }

    const result = await runCodelabTask(
      currentCode(),
      activeTask.value!.testCases,
      activeTask.value!.entryFunction ?? 'solution',
    );
    if (result.timedOut || !result.ok || result.results.some(r => !r.passed)) {
      runError.value = 'Wrong Answer. Please check hidden testcases.';
      caseResults.value = result.results ?? [];
      activeConsoleTab.value = 'result';
      isConsoleExpanded.value = true;
      return;
    }

    if (Array.isArray(props.codelabTask)) {
      if (!completedTasks.value.has(currentTaskIndex.value)) {
        completedTasks.value.add(currentTaskIndex.value);
      }

      if (completedTasks.value.size === props.codelabTask.length) {
        emit('completeLesson');
      } else {
        // Automatically switch to the next uncompleted task if any
        const nextIncomplete = props.codelabTask.findIndex((_, idx) => !completedTasks.value.has(idx));
        if (nextIncomplete !== -1) {
          // Optional: Delay slightly before auto-switching
          setTimeout(() => {
            currentTaskIndex.value = nextIncomplete;
          }, 1500);
        }
      }
    } else {
      emit('completeLesson');
    }
  } catch (err: unknown) {
    runError.value = err instanceof Error ? err.message : String(err);
  } finally {
    isSubmitting.value = false;
  }
}

watch(activeTask, (newTask) => {
  if (newTask) {
    editorInstance.value?.setValue(newTask.initialCode);
  }
  caseResults.value = [];
  runError.value = null;
});

onMounted(async () => {
  if (!activeTask.value) return;
  try {
    const monacoInstance = await loader.init();
    if (editorContainer.value) {
      editorInstance.value = monacoInstance.editor.create(editorContainer.value, {
        value: activeTask.value.initialCode,
        language: 'javascript',
        theme: 'vs-dark',
        automaticLayout: true,
        minimap: { enabled: false },
        fontSize: 14,
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
      monacoInstance.editor.defineTheme('dsa-dark', {
        base: 'vs-dark',
        inherit: true,
        rules: [],
        colors: {
          'editor.background': '#09090e',
          'editor.lineHighlightBackground': '#ffffff1a' // Đậm và sáng hơn để dễ nhìn (10% white)
        }
      });
      monacoInstance.editor.setTheme('dsa-dark');
    }
  } catch (error) {
    console.error('Failed to initialize Monaco editor', error);
  }
});

onUnmounted(() => {
  editorInstance.value?.dispose();
  editorInstance.value = null;
});
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: var(--color-bg-hover);
  border-radius: 4px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: var(--color-bg-active);
}
</style>
