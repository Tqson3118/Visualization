<template>
  <div class="ide-workspace-container">
    
    <div class="ide-editor-panel">
      <div class="flex-[6] min-h-0"><MonacoEditorPanel /></div>
      <div class="flex-[4] min-h-0 border-t" style="border-color: rgba(255, 255, 255, 0.05);">
        <CompilerConsole />
      </div>
    </div>

    
    <div class="ide-canvas-panel">
      <ArrayInputBar
        v-model="inputArrayText"
        :is-valid="inputValid"
        :is-compiling="compilerStore.isCompiling"
        @parse="parseInputArray"
        @run="runCompilation"
      />

      
      <div class="flex-1 rounded-xl overflow-hidden border shadow-lg relative min-h-0" style="border-color: rgba(255, 255, 255, 0.05);">
        <CanvasLayer />
        <div v-if="!hasFrames" class="absolute inset-0 flex items-center justify-center" style="background: rgba(15, 23, 42, 0.8);">
          <div class="text-center px-8">
            <svg xmlns="http://www.w3.org/2000/svg" width="40" height="40" viewBox="0 0 24 24"
              fill="none" stroke="currentColor" stroke-width="1.5" class="mx-auto mb-3 text-text-disabled">
              <polyline points="16 18 22 12 16 6" /><polyline points="8 6 2 12 8 18" />
            </svg>
            <p class="text-sm text-text-muted">
              Viết mã sắp xếp bên trái, nhấn <span class="text-accent font-semibold">RUN</span> để xem hoạt ảnh.
            </p>
          </div>
        </div>
      </div>

      
      <div class="mt-2 rounded-xl overflow-hidden border shadow-lg h-32" style="border-color: rgba(255, 255, 255, 0.05);">
        <AnimControlPanel />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue';
import MonacoEditorPanel from './MonacoEditorPanel.vue';
import CompilerConsole from './CompilerConsole.vue';
import ArrayInputBar from './ArrayInputBar.vue';
import CanvasLayer from '../../animation-engine/components/CanvasLayer.vue';
import AnimControlPanel from '../../animation-engine/components/AnimControlPanel.vue';
import { useLiveCompilerStore } from '../store/useLiveCompilerStore';
import { useAnimationStore } from '../../animation-engine/store/useAnimationStore';

const props = withDefaults(defineProps<{
  initialCode?: string;
  initialArray?: number[];
  /**
   * Khi embed trong màn bài học (step Code-to-Viz), animStore dùng chung với
   * LessonVisualizer (bước Viz). Bật preserveAnimation để KHÔNG clear animStore
   * khi mount — tránh xoá mất hoạt ảnh viz của bước phía trên. /code-ide không
   * truyền prop này → giữ nguyên hành vi cũ (clear khi mount).
   */
  preserveAnimation?: boolean;
}>(), {
  initialCode: undefined,
  initialArray: undefined,
  preserveAnimation: false,
});

const compilerStore = useLiveCompilerStore();
const animStore = useAnimationStore();

// Nạp props vào store NGAY tại setup (trước khi MonacoEditorPanel mount)
// để editor khởi tạo đúng code/input của bài học — /code-ide không truyền props nên không đổi hành vi.
function applyInitialProps(): void {
  if (props.initialCode && props.initialCode.trim()) {
    compilerStore.setSourceCode(props.initialCode);
  }
  if (props.initialArray && props.initialArray.length > 0) {
    compilerStore.setInputArray(props.initialArray);
  }
}
applyInitialProps();

const defaultArrayText = props.initialArray && props.initialArray.length > 0
  ? props.initialArray.join(', ')
  : '5, 3, 8, 1, 9, 2, 7, 4, 6';
const inputArrayText = ref(defaultArrayText);
const inputValid = ref(true);
const hasFrames = computed(() => animStore.totalSteps > 0);

function parseInputArray(): void {
  const text = inputArrayText.value.trim();
  if (!text) { inputValid.value = false; return; }
  const parts = text.split(',').map((s) => s.trim());
  const numbers: number[] = [];
  for (const part of parts) {
    const num = Number(part);
    if (isNaN(num) || !isFinite(num)) { inputValid.value = false; return; }
    numbers.push(num);
  }
  if (numbers.length < 2 || numbers.length > 50) { inputValid.value = false; return; }
  inputValid.value = true;
  compilerStore.setInputArray(numbers);
}

function runCompilation(): void {
  parseInputArray();
  if (!inputValid.value) return;
  compilerStore.compileAndExecuteCode();
}

// Khi prop thay đổi (đổi bài học) → cập nhật store + chạy mới, tránh giữ nhầm code bài trước.
watch(
  () => [props.initialCode, props.initialArray],
  () => {
    applyInitialProps();
    if (props.initialArray && props.initialArray.length > 0) {
      inputArrayText.value = props.initialArray.join(', ');
    }
    animStore.clear();
    compilerStore.clearLogs();
    if (inputValid.value) {
      void compilerStore.compileAndExecuteCode();
    }
  },
);

onMounted(() => {
  if (!props.preserveAnimation) {
    animStore.clear();
  }
  compilerStore.clearLogs();
  parseInputArray();
});

onBeforeUnmount(() => {
  compilerStore.cancelExecution();
});
</script>

<style scoped>
.ide-workspace-container { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; width: 100%; height: 100%; font-family: 'Outfit', sans-serif; }
.ide-editor-panel { display: flex; flex-direction: column; background: rgba(30, 41, 59, 0.4); border: 1px solid rgba(255, 255, 255, 0.05); border-radius: 16px; overflow: hidden; backdrop-filter: blur(10px); }
.ide-canvas-panel { display: flex; flex-direction: column; background: rgba(15, 23, 42, 0.6); border: 1px solid rgba(255, 255, 255, 0.05); border-radius: 16px; padding: 16px; }

@media (max-width: 900px) {
  .ide-workspace-container { grid-template-columns: 1fr; grid-template-rows: auto 1fr; overflow-y: auto; }
  .ide-editor-panel { min-height: 45vh; }
}
</style>
