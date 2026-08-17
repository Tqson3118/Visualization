<script setup lang="ts">
// CodeToVisualView — F3: Playground Code-to-Visual bằng DSL giới hạn.
// Flow: textarea DSL -> parseDsl (từ chối lệnh lạ kèm line) -> runTrace (interpreter
// in-process, không exec code) -> eventsToSteps -> simulationStore.loadSteps -> playback.
// Tái sử dụng: ControlBar + CanvasArea + PseudocodePanel + simulation store (SDD §3.2).
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { storeToRefs } from 'pinia';

import ControlBar from '@/components/simulator/ControlBar.vue';
import CanvasArea from '@/components/simulator/CanvasArea.vue';
import PseudocodePanel from '@/components/simulator/PseudocodePanel.vue';
import Button from '@/components/ui/Button.vue';
import { useSimulationStore } from '@/stores/simulation';
import { messages } from '@/i18n/vi';
import { parseDsl } from '@/features/code-to-visual/dsl/parser';
import { runTrace } from '@/features/code-to-visual/dsl/trace';
import { eventsToSteps } from '@/features/code-to-visual/dsl/toSimSteps';
import type { ConsoleLogEntry } from '@/features/code-to-visual/dsl/types';
import { Play, RotateCcw, Settings2 } from 'lucide-vue-next';

const sim = useSimulationStore();
const { steps, currentStep, currentIndex, status, speed } = storeToRefs(sim);

const editor = ref<string>(messages.codeToVisual.editorPlaceholder);
const arrayInput = ref('5, 3, 8, 1, 9, 2');
const logs = ref<ConsoleLogEntry[]>([]);
const sourceLines = ref<string[]>([]);
const dslError = ref<{ line: number; message: string } | null>(null);
const rendering = ref(false);

const initialArray = computed<number[]>(() => {
  const parts = arrayInput.value.split(',').map((p) => p.trim()).filter((p) => p !== '');
  const nums = parts.map(Number);
  if (nums.length < 2 || nums.length > 50) return [];
  if (!nums.every((n) => Number.isInteger(n))) return [];
  return nums;
});

const arrayInvalid = computed(() => initialArray.value.length === 0 || initialArray.value.length < 2);
const canRun = computed(() => !arrayInvalid.value && editor.value.trim().length > 0);

function nowTs(): string {
  return new Date().toLocaleTimeString('vi-VN', { hour12: false });
}

function pushLog(text: string, type: ConsoleLogEntry['type']): void {
  logs.value = logs.value.concat([{ text, type, timestamp: nowTs() }]);
}

async function run(): Promise<void> {
  if (!canRun.value) return;
  dslError.value = null;
  logs.value = [];
  rendering.value = true;
  try {
    const parsed = parseDsl(editor.value);
    sourceLines.value = parsed.lines;
    const initial = { array: initialArray.value, stack: [], queue: [] };
    const { events, error, logs: traceLogs } = runTrace(parsed.ops, initial);
    logs.value = logs.value.concat(traceLogs);
    if (error) {
      dslError.value = { line: error.line, message: error.message };
      pushLog(messages.codeToVisual.consoleError.replace('{line}', String(error.line)).replace('{message}', error.message), 'error');
    }
    const steps = eventsToSteps(events);
    sim.loadSteps('Code-to-Visual', steps);
  } catch (err) {
    const dslErr = err as { line?: number; message?: string };
    dslError.value = { line: dslErr.line ?? 0, message: dslErr.message ?? String(err) };
    pushLog(dslError.value.message, 'error');
    sim.clearSteps();
    sourceLines.value = editor.value.split(/\r?\n/);
  } finally {
    rendering.value = false;
  }
}

function clearAll(): void {
  dslError.value = null;
  logs.value = [];
  editor.value = '';
  sourceLines.value = [];
  sim.clearSteps();
}

watch(
  () => editor.value,
  () => {
    if (dslError.value) dslError.value = null;
  },
);

onBeforeUnmount(() => {
  sim.stopPlayback();
});

const hasSteps = computed(() => steps.value.length > 0);
const currentVars = computed(() => currentStep.value?.variables ?? {});
</script>

<template>
  <main class="code-visual container">
    <header class="code-visual__hero">
      <nav class="code-visual__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'home' }">Trang chủ</RouterLink>
        <span aria-hidden="true">/</span>
        <span>{{ messages.codeToVisual.breadcrumbRoot }}</span>
      </nav>
      <h1 class="code-visual__title">{{ messages.codeToVisual.title }}</h1>
      <p class="code-visual__subtitle">{{ messages.codeToVisual.subtitle }}</p>
    </header>

    <div class="code-visual__grid">
      <!-- TRÁI: editor + console -->
      <section class="code-visual__editor-pane" aria-label="Editor">
        <div class="code-visual__pane-head">
          <h2 class="code-visual__pane-title">
            <Settings2 :size="15" aria-hidden="true" />
            {{ messages.codeToVisual.editorLabel }}
          </h2>
          <div class="code-visual__pane-actions">
            <Button variant="ghost" size="sm" :disabled="!canRun || rendering" @click="run">
              <Play :size="14" aria-hidden="true" />
              {{ rendering ? '...' : messages.codeToVisual.run }}
            </Button>
            <Button variant="ghost" size="sm" :disabled="!hasSteps && editor === ''" @click="clearAll">
              <RotateCcw :size="14" aria-hidden="true" />
              {{ messages.codeToVisual.clear }}
            </Button>
          </div>
        </div>

        <textarea
          v-model="editor"
          class="code-visual__editor"
          :aria-label="messages.codeToVisual.editorLabel"
          :placeholder="messages.codeToVisual.editorPlaceholder"
          spellcheck="false"
          data-testid="dsl-editor"
        />

        <div class="code-visual__array">
          <label class="code-visual__array-label" for="dsl-array">
            {{ messages.codeToVisual.arrayInputLabel }}
          </label>
          <input
            id="dsl-array"
            v-model="arrayInput"
            class="code-visual__array-input"
            :class="{ 'code-visual__array-input--error': arrayInvalid }"
            type="text"
            :aria-invalid="arrayInvalid"
            data-testid="array-input"
          />
          <p v-if="arrayInvalid" class="code-visual__array-error" role="alert">
            {{ messages.codeToVisual.arrayInvalid }}
          </p>
          <p v-else class="code-visual__array-hint">{{ messages.codeToVisual.arrayInputHint }}</p>
        </div>

        <div class="code-visual__console" aria-label="Console">
          <h3 class="code-visual__console-title">{{ messages.codeToVisual.consoleTitle }}</h3>
          <div class="code-visual__console-body">
            <p v-if="logs.length === 0" class="code-visual__console-empty">
              {{ messages.codeToVisual.consoleEmpty }}
            </p>
            <p
              v-for="(log, idx) in logs"
              :key="idx"
              class="code-visual__console-line"
              :class="'code-visual__console-line--' + log.type"
            >
              <span class="code-visual__console-ts">{{ log.timestamp }}</span>
              {{ log.text }}
            </p>
          </div>
        </div>

        <details class="code-visual__docs">
          <summary>{{ messages.codeToVisual.docsTitle }}</summary>
          <ul>
            <li v-for="(doc, idx) in messages.codeToVisual.docs" :key="idx">
              <code>{{ doc }}</code>
            </li>
          </ul>
        </details>
      </section>

      <!-- PHẢI: input array (đã ở trái) + canvas + VCR playback -->
      <section class="code-visual__viz-pane" aria-label="Trực quan hóa">
        <div class="code-visual__pane-head">
          <h2 class="code-visual__pane-title">{{ messages.codeToVisual.outputTitle }}</h2>
          <span v-if="hasSteps" class="code-visual__count">
            {{ messages.codeToVisual.stepsLabel(steps.length) }}
          </span>
        </div>

        <div v-if="!hasSteps" class="code-visual__empty" data-testid="empty-state">
          <p>{{ messages.codeToVisual.outputEmpty }}</p>
        </div>

        <div v-if="dslError" class="code-visual__error-badge" role="alert" data-testid="dsl-error">
          <span class="code-visual__error-line">{{ messages.codeToVisual.errorBadge }} {{ dslError.line }}</span>
          {{ dslError.message }}
        </div>

        <template v-else>
          <div class="code-visual__canvas-wrap">
            <CanvasArea
              :structure="currentStep?.structure ?? null"
              :sim-key="'code-to-visual'"
              :empty-text="messages.simulator.canvasPlaceholder"
            />
          </div>

          <ControlBar
            :current-index="currentIndex"
            :total-frames="steps.length"
            :status="status"
            :speed="speed"
            @play="sim.play"
            @pause="sim.pause"
            @step-back="sim.stepBack"
            @step-forward="sim.stepForward"
            @reset="sim.reset"
            @set-speed="sim.setSpeed"
          />

          <PseudocodePanel
            :pseudocode="sourceLines.length > 0 ? sourceLines : editor.split(/\r?\n/)"
            :active-line="currentStep?.pseudocodeLine ?? 0"
            :variables="currentVars"
            :collapsed="false"
          />


          <p v-if="currentStep" class="code-visual__explanation" data-testid="explanation">
            {{ currentStep.explanation }}
          </p>
        </template>
      </section>
    </div>
  </main>
</template>

<style scoped>
.code-visual {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding-block: var(--space-md) var(--space-2xl);
}
.code-visual__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}
.code-visual__breadcrumb {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-mono);
  font-size: 12px;
  color: var(--color-text-tertiary);
}
.code-visual__breadcrumb a { color: var(--color-primary); font-weight: 600; text-decoration: none; }
.code-visual__title { font-size: 24px; font-weight: 700; letter-spacing: -0.02em; margin: 0; }
.code-visual__subtitle { margin: 0; font-size: var(--text-sm); color: var(--color-text-muted); max-width: 72ch; }

.code-visual__grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
  gap: var(--space-md);
  align-items: start;
}

.code-visual__editor-pane,
.code-visual__viz-pane {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
}

.code-visual__pane-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
}
.code-visual__pane-title {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-base);
  font-weight: 600;
  margin: 0;
}
.code-visual__pane-actions { display: flex; gap: 6px; }

.code-visual__editor {
  width: 100%;
  min-height: 240px;
  resize: vertical;
  font-family: var(--font-mono);
  font-size: 13px;
  line-height: 1.5;
  color: var(--color-foreground);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-sm);
}

.code-visual__array { display: flex; flex-direction: column; gap: var(--space-xs); }
.code-visual__array-label { font-size: var(--text-xs); font-weight: 600; color: var(--color-text-secondary); }
.code-visual__array-input {
  padding: var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-foreground);
  font-family: var(--font-mono);
  font-size: 13px;
}
.code-visual__array-input--error { border-color: var(--color-destructive); }
.code-visual__array-error { color: var(--color-destructive); font-size: var(--text-xs); margin: 0; }
.code-visual__array-hint { color: var(--color-text-muted); font-size: var(--text-xs); margin: 0; }

.code-visual__console {
  display: flex;
  flex-direction: column;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  overflow: hidden;
}
.code-visual__console-title {
  margin: 0;
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-xs);
  font-weight: 600;
  border-bottom: 1px solid var(--color-border);
}
.code-visual__console-body {
  max-height: 200px;
  overflow-y: auto;
  padding: var(--space-sm) var(--space-md);
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.code-visual__console-empty { color: var(--color-text-tertiary); font-size: var(--text-xs); margin: 0; }
.code-visual__console-line { margin: 0; font-family: var(--font-mono); font-size: 12px; display: flex; gap: 8px; }
.code-visual__console-ts { color: var(--color-text-tertiary); flex-shrink: 0; }
.code-visual__console-line--info { color: var(--color-text-secondary); }
.code-visual__console-line--success { color: var(--color-success); }
.code-visual__console-line--warn { color: var(--color-warning); }
.code-visual__console-line--error { color: var(--color-destructive); }

.code-visual__docs summary { cursor: pointer; font-size: var(--text-sm); font-weight: 600; }
.code-visual__docs ul { margin: var(--space-sm) 0 0; padding-left: var(--space-lg); display: flex; flex-direction: column; gap: 4px; }
.code-visual__docs code { font-family: var(--font-mono); font-size: 12px; }

.code-visual__canvas-wrap { min-width: 0; width: 100%; }
.code-visual__empty {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 260px;
  border: 1px dashed var(--color-border-strong);
  border-radius: var(--radius-lg);
  color: var(--color-text-muted);
  text-align: center;
  padding: var(--space-md);
}
.code-visual__count {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  padding: 2px 8px;
  border-radius: var(--radius-full);
}
.code-visual__error-badge {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-destructive);
  background: color-mix(in srgb, var(--color-destructive) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-destructive) 30%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}
.code-visual__error-line {
  font-family: var(--font-mono);
  font-weight: 700;
  background: var(--color-destructive);
  color: var(--color-on-primary);
  padding: 1px 6px;
  border-radius: var(--radius-full);
  flex-shrink: 0;
}
.code-visual__explanation {
  margin: 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}

@media (max-width: 900px) {
  .code-visual__grid { grid-template-columns: minmax(0, 1fr); }
}
</style>