<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import {
  AlertCircle,
  Check,
  CheckCircle2,
  Code,
  Eye,
  FileCode,
  Flame,
  HelpCircle,
  Info,
  Play,
  Plus,
  RefreshCw,
  Sparkles,
  Trash2,
  XCircle,
} from 'lucide-vue-next';

import { runCodelabTask, type CodelabCaseResult } from '@/features/lesson/utils/codelabExecutor';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Input from '@/components/ui/Input.vue';

export interface CodeLabFormState {
  enabled: boolean;
  exerciseId?: number | null;
  title: string;
  description: string;
  difficulty: 'Easy' | 'Medium' | 'Hard';
  entryFunction: string;
  durationMinutes: number;
  maxScore: number;
  starterCode: string;
  solutionCode: string;
  testCases: Array<{
    input: string;
    expected: string;
    isHidden: boolean;
  }>;
}

const props = defineProps<{
  modelValue: CodeLabFormState;
  lessonTitle?: string;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', value: CodeLabFormState): void;
}>();

const localState = reactive<CodeLabFormState>({
  enabled: props.modelValue?.enabled ?? false,
  exerciseId: props.modelValue?.exerciseId ?? null,
  title: props.modelValue?.title || `Thực hành: ${props.lessonTitle || 'Giải thuật DSA'}`,
  description: props.modelValue?.description || 'Viết hàm thực thi giải thuật theo đúng yêu cầu bài toán.',
  difficulty: props.modelValue?.difficulty || 'Easy',
  entryFunction: props.modelValue?.entryFunction || 'solve',
  durationMinutes: props.modelValue?.durationMinutes || 20,
  maxScore: props.modelValue?.maxScore || 100,
  starterCode:
    props.modelValue?.starterCode ||
    `/**
 * @param {any} input
 * @return {any}
 */
function solve(input) {
  // Viết mã nguồn giải thuật của bạn ở đây
  return input;
}`,
  solutionCode:
    props.modelValue?.solutionCode ||
    `function solve(input) {
  // Code giải mẫu của Giảng viên để verify testcases
  return input;
}`,
  testCases:
    props.modelValue?.testCases && props.modelValue.testCases.length > 0
      ? JSON.parse(JSON.stringify(props.modelValue.testCases))
      : [
          { input: '[1, 2, 3]', expected: '[1, 2, 3]', isHidden: false },
          { input: '[5, 4, 3, 2, 1]', expected: '[5, 4, 3, 2, 1]', isHidden: false },
          { input: '[]', expected: '[]', isHidden: true },
        ],
});

// Watch sync from props
watch(
  () => props.modelValue,
  (newVal) => {
    if (!newVal) return;
    if (newVal.enabled !== localState.enabled) localState.enabled = newVal.enabled;
    if (newVal.exerciseId !== localState.exerciseId) localState.exerciseId = newVal.exerciseId;
    if (newVal.title !== localState.title) localState.title = newVal.title;
    if (newVal.description !== localState.description) localState.description = newVal.description;
    if (newVal.difficulty !== localState.difficulty) localState.difficulty = newVal.difficulty;
    if (newVal.entryFunction !== localState.entryFunction) localState.entryFunction = newVal.entryFunction;
    if (newVal.durationMinutes !== localState.durationMinutes) localState.durationMinutes = newVal.durationMinutes;
    if (newVal.maxScore !== localState.maxScore) localState.maxScore = newVal.maxScore;
    if (newVal.starterCode !== localState.starterCode) localState.starterCode = newVal.starterCode;
    if (newVal.solutionCode !== localState.solutionCode) localState.solutionCode = newVal.solutionCode;
    if (JSON.stringify(newVal.testCases) !== JSON.stringify(localState.testCases)) {
      localState.testCases = JSON.parse(JSON.stringify(newVal.testCases || []));
    }
  },
  { deep: true },
);

// Emit changes
watch(
  localState,
  () => {
    emit('update:modelValue', {
      enabled: localState.enabled,
      exerciseId: localState.exerciseId,
      title: localState.title,
      description: localState.description,
      difficulty: localState.difficulty,
      entryFunction: localState.entryFunction,
      durationMinutes: localState.durationMinutes,
      maxScore: localState.maxScore,
      starterCode: localState.starterCode,
      solutionCode: localState.solutionCode,
      testCases: JSON.parse(JSON.stringify(localState.testCases)),
    });
  },
  { deep: true },
);

// ── Test Runner State ──
const isTesting = ref(false);
const testResults = ref<CodelabCaseResult[]>([]);
const testError = ref<string | null>(null);
const testRunCount = ref(0);

const testSummary = computed(() => {
  if (testResults.value.length === 0) return null;
  const passedCount = testResults.value.filter((r) => r.passed).length;
  const total = testResults.value.length;
  const allPassed = passedCount === total && !testError.value;
  return {
    passedCount,
    total,
    allPassed,
    percent: Math.round((passedCount / total) * 100),
  };
});

function addTestCase(): void {
  localState.testCases.push({
    input: '[0]',
    expected: '[0]',
    isHidden: false,
  });
}

function removeTestCase(idx: number): void {
  if (localState.testCases.length > 1) {
    localState.testCases.splice(idx, 1);
  }
}

// Preset DSA Template
function applyTemplate(type: 'sort' | 'twosum' | 'stack'): void {
  if (type === 'sort') {
    localState.title = 'Bài tập: Cài đặt Bubble Sort';
    localState.entryFunction = 'bubbleSort';
    localState.description =
      'Hãy cài đặt hàm `bubbleSort(arr)` sắp xếp mảng các số nguyên theo thứ tự tăng dần bằng thuật toán Nổi bọt.';
    localState.starterCode = `/**
 * @param {number[]} arr
 * @return {number[]}
 */
function bubbleSort(arr) {
  // Sắp xếp mảng tăng dần
  return arr;
}`;
    localState.solutionCode = `function bubbleSort(arr) {
  const a = [...arr];
  const n = a.length;
  for (let i = 0; i < n - 1; i++) {
    for (let j = 0; j < n - i - 1; j++) {
      if (a[j] > a[j + 1]) {
        const temp = a[j];
        a[j] = a[j + 1];
        a[j + 1] = temp;
      }
    }
  }
  return a;
}`;
    localState.testCases = [
      { input: '[[5, 2, 9, 1, 5, 6]]', expected: '[1, 2, 5, 5, 6, 9]', isHidden: false },
      { input: '[[3, -1, 0, 8, 2]]', expected: '[-1, 0, 2, 3, 8]', isHidden: false },
      { input: '[[100]]', expected: '[100]', isHidden: false },
      { input: '[[]]', expected: '[]', isHidden: true },
      { input: '[[9, 8, 7, 6, 5, 4, 3, 2, 1]]', expected: '[1, 2, 3, 4, 5, 6, 7, 8, 9]', isHidden: true },
    ];
  } else if (type === 'twosum') {
    localState.title = 'Bài tập: Two Sum (Tổng hai số)';
    localState.entryFunction = 'twoSum';
    localState.description =
      'Cho một mảng số nguyên `nums` và một số nguyên `target`, hãy trả về chỉ số của hai số sao cho tổng của chúng bằng `target`.';
    localState.starterCode = `/**
 * @param {number[]} nums
 * @param {number} target
 * @return {number[]}
 */
function twoSum(nums, target) {
  return [0, 1];
}`;
    localState.solutionCode = `function twoSum(nums, target) {
  const map = new Map();
  for (let i = 0; i < nums.length; i++) {
    const complement = target - nums[i];
    if (map.has(complement)) {
      return [map.get(complement), i];
    }
    map.set(nums[i], i);
  }
  return [];
}`;
    localState.testCases = [
      { input: '[[2, 7, 11, 15], 9]', expected: '[0, 1]', isHidden: false },
      { input: '[[3, 2, 4], 6]', expected: '[1, 2]', isHidden: false },
      { input: '[[3, 3], 6]', expected: '[0, 1]', isHidden: false },
      { input: '[[1, 5, 8, 12, 20], 17]', expected: '[1, 3]', isHidden: true },
    ];
  } else if (type === 'stack') {
    localState.title = 'Bài tập: Kiểm tra Dãy ngoặc hợp lệ';
    localState.entryFunction = 'isValidParentheses';
    localState.description =
      'Cho một chuỗi `s` chỉ chứa các ký tự `()[]{}`. Hãy xác định xem chuỗi ngoặc đầu vào có hợp lệ hay không (sử dụng Stack).';
    localState.starterCode = `/**
 * @param {string} s
 * @return {boolean}
 */
function isValidParentheses(s) {
  return true;
}`;
    localState.solutionCode = `function isValidParentheses(s) {
  const stack = [];
  const map = { ')': '(', '}': '{', ']': '[' };
  for (const ch of s) {
    if (ch === '(' || ch === '{' || ch === '[') {
      stack.push(ch);
    } else if (map[ch]) {
      if (stack.length === 0 || stack.pop() !== map[ch]) {
        return false;
      }
    }
  }
  return stack.length === 0;
}`;
    localState.testCases = [
      { input: '["()"]', expected: 'true', isHidden: false },
      { input: '["()[]{}"]', expected: 'true', isHidden: false },
      { input: '["(]"]', expected: 'false', isHidden: false },
      { input: '["([)]"]', expected: 'false', isHidden: true },
      { input: '["{[]}"]', expected: 'true', isHidden: true },
    ];
  }
  testResults.value = [];
  testError.value = null;
}

// ── Run Solution against Testcases ──
async function runSolutionTest(): Promise<void> {
  if (!localState.solutionCode.trim()) {
    testError.value = 'Vui lòng nhập Code giải mẫu của Giảng viên để chạy kiểm thử.';
    return;
  }
  if (localState.testCases.length === 0) {
    testError.value = 'Vui lòng thêm ít nhất 1 Testcase.';
    return;
  }

  isTesting.value = true;
  testError.value = null;
  testRunCount.value++;

  try {
    const formattedTestcases = localState.testCases.map((tc) => ({
      input: tc.input,
      expectedOutput: tc.expected,
      isHidden: tc.isHidden,
    }));

    const result = await runCodelabTask(
      localState.solutionCode,
      formattedTestcases,
      localState.entryFunction || 'solve',
      2000,
    );

    if (result.timedOut) {
      testError.value = result.error || 'Time Limit Exceeded (Quá thời gian thực thi 2000ms).';
      testResults.value = [];
    } else if (!result.ok && !result.results?.length) {
      testError.value = result.error || 'Lỗi biên dịch hoặc thực thi code giải mẫu.';
      testResults.value = [];
    } else {
      testResults.value = result.results || [];
    }
  } catch (err: any) {
    testError.value = err?.message || String(err);
    testResults.value = [];
  } finally {
    isTesting.value = false;
  }
}
</script>

<template>
  <div class="codelab-tab flex flex-col h-full p-6 overflow-y-auto max-w-7xl mx-auto space-y-6">
    <!-- Header & Toggle -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-5 rounded-2xl bg-gradient-to-r from-emerald-950/40 via-vdsa-surface to-emerald-950/20 border border-emerald-500/30 shadow-lg">
      <div class="flex items-center gap-3.5">
        <div class="w-11 h-11 rounded-xl bg-emerald-500/20 text-emerald-400 border border-emerald-500/30 flex items-center justify-center shrink-0">
          <Code :size="22" />
        </div>
        <div>
          <div class="flex items-center gap-2">
            <h2 class="text-base font-black text-white">Bài tập Lập trình (Code Lab)</h2>
            <Badge v-if="localState.enabled" variant="success" class="text-[11px]">Đang bật</Badge>
            <Badge v-else variant="muted" class="text-[11px]">Chưa kích hoạt</Badge>
          </div>
          <p class="text-xs text-slate-300 mt-0.5">
            Cho phép học viên nộp code JavaScript thực tế và hệ thống tự động chấm điểm qua bộ Testcase.
          </p>
        </div>
      </div>

      <div class="flex items-center gap-3 shrink-0">
        <label class="relative inline-flex items-center cursor-pointer select-none">
          <input
            v-model="localState.enabled"
            type="checkbox"
            class="sr-only peer"
          />
          <div class="w-11 h-6 bg-slate-800 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-slate-300 after:border after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-emerald-600"></div>
          <span class="ml-2.5 text-xs font-bold text-white">
            {{ localState.enabled ? 'Kích hoạt Code Lab' : 'Tắt Code Lab' }}
          </span>
        </label>
      </div>
    </div>

    <!-- Disabled Placeholder State -->
    <div v-if="!localState.enabled" class="p-12 rounded-3xl bg-vdsa-surface/60 border border-vdsa-border border-dashed text-center space-y-4">
      <div class="w-14 h-14 rounded-2xl bg-slate-800 text-slate-400 flex items-center justify-center mx-auto">
        <FileCode :size="28" />
      </div>
      <div class="max-w-md mx-auto">
        <h3 class="text-base font-bold text-white">Bài học này chưa đính kèm bài tập Code Lab</h3>
        <p class="text-xs text-slate-400 mt-1">
          Nếu bài học này cần học viên thực hành code (ví dụ: cài đặt thuật toán, giải bài toán), hãy bật nút kích hoạt ở trên để cấu hình đề bài và bộ Testcase.
        </p>
      </div>
      <Button variant="primary" size="sm" class="bg-emerald-600 hover:bg-emerald-500 border-emerald-500" @click="localState.enabled = true">
        <Plus :size="14" /> Bật Code Lab ngay
      </Button>
    </div>

    <!-- Active Workspace: 2-Column Authoring & Solution Playground -->
    <div v-else class="grid grid-cols-1 lg:grid-cols-12 gap-6">
      <!-- ══════════════════════════════════════════════════════
           LEFT COLUMN: ĐỀ BÀI & CẤU HÌNH TESTCASES (6 cols)
           ══════════════════════════════════════════════════════ -->
      <section class="lg:col-span-6 space-y-5">
        <!-- Quick Templates -->
        <div class="p-3.5 rounded-xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between gap-2 flex-wrap">
          <span class="text-xs font-bold text-slate-300 flex items-center gap-1.5">
            <Sparkles :size="14" class="text-amber-400" /> Mẫu bài toán DSA:
          </span>
          <div class="flex items-center gap-1.5 flex-wrap">
            <button
              type="button"
              class="px-2.5 py-1 rounded-lg text-xs font-semibold bg-white/5 hover:bg-white/10 text-slate-200 border border-white/10 transition-colors"
              @click="applyTemplate('sort')"
            >
              Bubble Sort
            </button>
            <button
              type="button"
              class="px-2.5 py-1 rounded-lg text-xs font-semibold bg-white/5 hover:bg-white/10 text-slate-200 border border-white/10 transition-colors"
              @click="applyTemplate('twosum')"
            >
              Two Sum
            </button>
            <button
              type="button"
              class="px-2.5 py-1 rounded-lg text-xs font-semibold bg-white/5 hover:bg-white/10 text-slate-200 border border-white/10 transition-colors"
              @click="applyTemplate('stack')"
            >
              Valid Parentheses
            </button>
          </div>
        </div>

        <!-- Meta Info -->
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-3.5">
          <h3 class="text-xs font-extrabold text-white uppercase tracking-wider">Thông tin bài tập</h3>

          <div>
            <label class="block text-xs font-bold text-slate-300 mb-1">Tiêu đề bài toán <span class="text-rose-400">*</span></label>
            <Input v-model="localState.title" placeholder="VD: Sắp xếp mảng số nguyên bằng Bubble Sort" required />
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div>
              <label class="block text-xs font-bold text-slate-300 mb-1">Tên hàm Entry</label>
              <Input v-model="localState.entryFunction" placeholder="solve" class="font-mono text-xs" />
            </div>
            <div>
              <label class="block text-xs font-bold text-slate-300 mb-1">Độ khó</label>
              <select v-model="localState.difficulty" class="w-full h-9 rounded-lg border border-slate-700 bg-slate-900 px-3 text-xs text-slate-100 focus:outline-none focus:ring-1 focus:ring-emerald-500">
                <option value="Easy">Dễ (Easy)</option>
                <option value="Medium">Trung bình (Medium)</option>
                <option value="Hard">Khó (Hard)</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-bold text-slate-300 mb-1">Điểm tối đa</label>
              <Input v-model.number="localState.maxScore" type="number" min="10" max="1000" class="text-xs" />
            </div>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-300 mb-1">Mô tả đề bài & Ràng buộc (Markdown)</label>
            <textarea
              v-model="localState.description"
              rows="4"
              class="w-full rounded-lg border border-slate-700 bg-slate-900 p-2.5 text-xs text-slate-100 focus:outline-none focus:ring-1 focus:ring-emerald-500 font-sans leading-relaxed"
              placeholder="Mô tả input, output, ví dụ và giới hạn bài toán..."
            ></textarea>
          </div>
        </div>

        <!-- Testcases Builder -->
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-3.5">
          <div class="flex items-center justify-between">
            <div>
              <h3 class="text-xs font-extrabold text-white uppercase tracking-wider flex items-center gap-2">
                Danh sách Test Cases ({{ localState.testCases.length }})
              </h3>
              <p class="text-[11px] text-slate-400 mt-0.5">Input định dạng JSON Array chứa các tham số truyền vào hàm.</p>
            </div>
            <Button size="sm" variant="secondary" class="h-7 text-xs" @click="addTestCase">
              <Plus :size="13" /> Thêm Testcase
            </Button>
          </div>

          <div class="space-y-2.5 max-h-[360px] overflow-y-auto pr-1">
            <div
              v-for="(tc, idx) in localState.testCases"
              :key="idx"
              class="p-3 rounded-xl bg-slate-950/60 border border-slate-800 space-y-2 relative group hover:border-slate-700 transition-colors"
            >
              <div class="flex items-center justify-between gap-2">
                <span class="text-xs font-mono font-bold text-slate-300">#Testcase {{ idx + 1 }}</span>
                <div class="flex items-center gap-3">
                  <label class="inline-flex items-center gap-1.5 text-[11px] text-slate-400 cursor-pointer select-none">
                    <input v-model="tc.isHidden" type="checkbox" class="rounded bg-slate-900 border-slate-700 text-emerald-500 focus:ring-0" />
                    <span>Ẩn với học viên</span>
                  </label>
                  <button
                    v-if="localState.testCases.length > 1"
                    type="button"
                    class="text-slate-500 hover:text-rose-400 p-1 transition-colors"
                    title="Xóa testcase này"
                    @click="removeTestCase(idx)"
                  >
                    <Trash2 :size="13" />
                  </button>
                </div>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 font-mono text-xs">
                <div>
                  <span class="text-[10px] uppercase font-bold text-slate-500 block mb-0.5">Input (JSON Array)</span>
                  <input
                    v-model="tc.input"
                    type="text"
                    placeholder="VD: [[5, 2, 9, 1]]"
                    class="w-full h-8 rounded bg-slate-900 border border-slate-800 px-2 text-xs text-white focus:outline-none focus:border-emerald-500"
                  />
                </div>
                <div>
                  <span class="text-[10px] uppercase font-bold text-slate-500 block mb-0.5">Expected Output (JSON)</span>
                  <input
                    v-model="tc.expected"
                    type="text"
                    placeholder="VD: [1, 2, 5, 9]"
                    class="w-full h-8 rounded bg-slate-900 border border-slate-800 px-2 text-xs text-emerald-300 focus:outline-none focus:border-emerald-500"
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- ══════════════════════════════════════════════════════
           RIGHT COLUMN: SOLUTION PLAYGROUND & TEST RUNNER (6 cols)
           ══════════════════════════════════════════════════════ -->
      <section class="lg:col-span-6 space-y-5">
        <!-- Starter Code Box (Cho học viên) -->
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-2.5">
          <div class="flex items-center justify-between">
            <h3 class="text-xs font-extrabold text-white uppercase tracking-wider flex items-center gap-1.5">
              <FileCode :size="14" class="text-sky-400" /> Starter Code (Học viên nhận được)
            </h3>
            <span class="text-[11px] text-slate-400">JavaScript ES6</span>
          </div>
          <textarea
            v-model="localState.starterCode"
            rows="5"
            class="w-full rounded-xl border border-slate-800 bg-[#090d16] p-3 text-xs font-mono text-slate-200 focus:outline-none focus:ring-1 focus:ring-sky-500 leading-relaxed"
            placeholder="Mã khung ban đầu cho học sinh..."
          ></textarea>
        </div>

        <!-- Teacher's Solution Code Box (Solution Playground) -->
        <div class="p-4 rounded-2xl bg-gradient-to-b from-vdsa-surface to-slate-900/90 border border-emerald-500/30 space-y-3 shadow-xl">
          <div class="flex items-center justify-between flex-wrap gap-2">
            <div>
              <h3 class="text-xs font-extrabold text-emerald-400 uppercase tracking-wider flex items-center gap-1.5">
                <Play :size="14" class="text-emerald-400" /> Solution Playground (Code giải mẫu GV)
              </h3>
              <p class="text-[11px] text-slate-400 mt-0.5">
                Chạy thử code giải mẫu để đảm bảo 100% Testcases cho kết quả đúng.
              </p>
            </div>

            <Button
              size="sm"
              variant="primary"
              class="bg-emerald-600 hover:bg-emerald-500 text-white font-bold h-8 text-xs gap-1.5"
              :loading="isTesting"
              @click="runSolutionTest"
            >
              <Play :size="13" /> Chạy thử nghiệm Testcase
            </Button>
          </div>

          <textarea
            v-model="localState.solutionCode"
            rows="7"
            class="w-full rounded-xl border border-slate-800 bg-[#060911] p-3 text-xs font-mono text-emerald-200 focus:outline-none focus:ring-1 focus:ring-emerald-500 leading-relaxed"
            placeholder="Nhập code giải hoàn chỉnh của bạn để test thử..."
          ></textarea>

          <!-- Run Results Display -->
          <div v-if="testError" class="p-3 rounded-xl bg-rose-500/10 border border-rose-500/30 text-xs text-rose-300 flex items-start gap-2">
            <AlertCircle :size="16" class="text-rose-400 shrink-0 mt-0.5" />
            <div class="font-mono whitespace-pre-wrap">{{ testError }}</div>
          </div>

          <div v-else-if="testSummary" class="space-y-2.5 pt-1">
            <!-- Summary Banner -->
            <div
              class="p-3 rounded-xl border flex items-center justify-between gap-3 text-xs font-bold"
              :class="testSummary.allPassed ? 'bg-emerald-500/10 border-emerald-500/40 text-emerald-300' : 'bg-amber-500/10 border-amber-500/40 text-amber-300'"
            >
              <div class="flex items-center gap-2">
                <CheckCircle2 v-if="testSummary.allPassed" :size="18" class="text-emerald-400" />
                <AlertCircle v-else :size="18" class="text-amber-400" />
                <span>
                  {{ testSummary.allPassed ? 'Tuyệt vời! 100% Testcase đều Passed' : `Cảnh báo: Chỉ pass ${testSummary.passedCount}/${testSummary.total} Testcases` }}
                </span>
              </div>
              <Badge :variant="testSummary.allPassed ? 'success' : 'warning'">
                {{ testSummary.percent }}% Đạt
              </Badge>
            </div>

            <!-- Case Breakdown List -->
            <div class="space-y-1.5 max-h-[220px] overflow-y-auto font-mono text-xs">
              <div
                v-for="(res, rIdx) in testResults"
                :key="rIdx"
                class="p-2.5 rounded-lg border flex items-center justify-between gap-2"
                :class="res.passed ? 'bg-emerald-950/20 border-emerald-800/30 text-slate-200' : 'bg-rose-950/30 border-rose-800/40 text-rose-200'"
              >
                <div class="flex items-center gap-2 truncate">
                  <span
                    class="w-5 h-5 rounded-full flex items-center justify-center text-[10px] font-bold shrink-0"
                    :class="res.passed ? 'bg-emerald-500/20 text-emerald-400' : 'bg-rose-500/20 text-rose-400'"
                  >
                    {{ rIdx + 1 }}
                  </span>
                  <span class="truncate text-slate-300">Input: {{ res.input }}</span>
                </div>

                <div class="flex items-center gap-2 shrink-0">
                  <span v-if="!res.passed" class="text-[11px] text-rose-300">
                    Actual: {{ res.actualOutput || res.error || 'null' }}
                  </span>
                  <Badge :variant="res.passed ? 'success' : 'danger'" class="text-[10px] py-0">
                    {{ res.passed ? 'PASSED' : 'FAILED' }}
                  </Badge>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  </div>
</template>
