<template>
  <div ref="container" class="w-full h-full min-h-[300px]"></div>
</template>
<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount, shallowRef } from 'vue';
import * as monaco from 'monaco-editor';
import "monaco-editor/esm/vs/language/typescript/monaco.contribution";
import "monaco-editor/min/vs/editor/editor.main.css";
import editorWorker from "monaco-editor/esm/vs/editor/editor.worker?worker";
import tsWorker from "monaco-editor/esm/vs/language/typescript/ts.worker?worker";

if (!(globalThis as any).MonacoEnvironment) {
  (globalThis as any).MonacoEnvironment = {
    getWorker: (_: any, label: string) => {
      if (label === "typescript" || label === "javascript") return new tsWorker();
      return new editorWorker();
    }
  };
}

const props = defineProps({
  modelValue: { type: String, default: '' },
  language: { type: String, default: 'javascript' },
  theme: { type: String, default: 'vs-dark' },
  options: { type: Object, default: () => ({}) }
});

const emit = defineEmits(['update:modelValue']);
const container = ref<HTMLElement | null>(null);
const editor = shallowRef<monaco.editor.IStandaloneCodeEditor | null>(null);

onMounted(() => {
  if (container.value) {
    editor.value = monaco.editor.create(container.value, {
      value: props.modelValue,
      language: props.language,
      theme: props.theme,
      automaticLayout: true,
      ...props.options
    });

    editor.value.onDidChangeModelContent(() => {
      emit('update:modelValue', editor.value?.getValue() || '');
    });
  }
});

watch(() => props.modelValue, (newVal) => {
  if (editor.value && editor.value.getValue() !== newVal) {
    editor.value.setValue(newVal);
  }
});

watch(() => props.theme, (theme) => {
  if (editor.value) editor.value.updateOptions({ theme });
});

onBeforeUnmount(() => {
  editor.value?.dispose();
});
</script>
