<template>
  <div class="flex flex-col h-full lg:flex-row bg-bg-primary min-h-[500px]">
    <!-- Trái: Code Editor -->
    <div class="w-full lg:w-1/2 flex flex-col border-r border-border-default h-[50vh] lg:h-full">
      <div class="flex items-center justify-between px-4 py-2 bg-bg-secondary border-b border-border-default">
        <h3 class="text-xs font-bold text-text-primary uppercase tracking-wider flex items-center gap-2">
          <BaseIcon name="code" class="w-4 h-4 text-accent-emerald" /> 
          Mã nguồn (JavaScript)
        </h3>
        <button 
           @click="analyzeWithAI" 
           :disabled="isAnalyzing"
           class="px-3 py-1 bg-gradient-to-r from-purple-500 to-indigo-500 text-white rounded-md text-[10px] font-bold shadow-lg hover:shadow-purple-500/30 transition-all flex items-center gap-1 cursor-pointer">
          <span v-if="isAnalyzing">Đang phân tích...</span>
          <span v-else>✨ Phân tích bằng AI</span>
        </button>
      </div>
      <div class="flex-1 min-h-0 relative">
        <PureMonacoEditor
          v-model="code"
          language="javascript"
          theme="vs-dark"
          :options="editorOptions"
        />
      </div>
    </div>

    <!-- Phải: Sơ đồ thuật toán -->
    <div class="w-full lg:w-1/2 flex flex-col bg-bg-default relative overflow-hidden h-[50vh] lg:h-full">
      <div class="flex items-center justify-between px-4 py-2 bg-bg-secondary border-b border-border-default z-10">
        <h3 class="text-xs font-bold text-text-primary uppercase tracking-wider flex items-center gap-2">
          <BaseIcon name="git-merge" class="w-4 h-4 text-accent-cyan" /> 
          Sơ đồ Khối Động
        </h3>
        <button class="px-2 py-1 bg-accent-cyan/10 text-accent-cyan rounded text-[10px] font-bold border border-accent-cyan/20 cursor-default">
          Babel AST Engine
        </button>
      </div>
      <div class="flex-1 p-4 overflow-auto flex items-center justify-center relative bg-bg-secondary/30" ref="mermaidContainer">
         <div v-html="mermaidSvg" class="mermaid-diagram transition-all duration-300" :key="renderKey"></div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import PureMonacoEditor from '../../../components/editor/PureMonacoEditor.vue';
import { FlowchartGenerator } from '../engine/FlowchartGenerator';
import mermaid from 'mermaid';

const isAnalyzing = ref(false);

async function analyzeWithAI() {
  if (!code.value.trim()) return;
  isAnalyzing.value = true;
  try {
    const response = await fetch('http://localhost:5055/api/AIVisualizer/generate', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ code: code.value, language: 'javascript' })
    });
    
    if (!response.ok) {
        throw new Error('API Error: ' + response.statusText);
    }
    
    const data = await response.json();
    console.log("=========================================");
    console.log("🧠 DỮ LIỆU JSON TỪ GEMINI AI TRẢ VỀ:");
    console.log(data.jsonData);
    console.log("=========================================");
    alert('🎉 AI đã phân tích xong! Hãy bấm F12 (DevTools) mở tab Console để xem mảng JSON Frame cực xịn nhé!');
  } catch (err) {
    console.error(err);
    alert('Lỗi kết nối đến Backend AI! Hãy chắc chắn Backend đã chạy.');
  } finally {
    isAnalyzing.value = false;
  }
}

const code = ref(`// Gõ thuật toán của bạn vào đây
// Sơ đồ bên phải sẽ tự động vẽ theo thời gian thực!

let a = 10;
let b = 20;

if (a > b) {
  let max = a;
} else {
  let max = b;
}

for(let i = 0; i < 5; i++) {
   a = a + i;
}
`);

const mermaidSvg = ref('');
const renderKey = ref(0);
let timeout: any = null;

const editorOptions = {
  minimap: { enabled: false },
  fontSize: 14,
  fontFamily: "'Fira Code', 'JetBrains Mono', monospace",
  wordWrap: 'on',
  lineHeight: 24,
  padding: { top: 16 },
  scrollBeyondLastLine: false,
};

onMounted(() => {
  mermaid.initialize({ 
    startOnLoad: false,
    theme: 'dark',
    securityLevel: 'loose',
    flowchart: {
      htmlLabels: true,
      curve: 'basis'
    }
  });
  renderFlowchart();
});

watch(code, () => {
  if (timeout) clearTimeout(timeout);
  timeout = setTimeout(() => {
    renderFlowchart();
  }, 400);
});

async function renderFlowchart() {
  const mmdSyntax = FlowchartGenerator.generate(code.value);
  try {
    const { svg } = await mermaid.render('mermaid-chart-' + Date.now(), mmdSyntax);
    mermaidSvg.value = svg;
    renderKey.value++;
  } catch (err) {
    // Silent error for incomplete typing
  }
}
</script>

<style>
.mermaid-diagram svg {
  max-width: 100%;
  height: auto;
  font-family: inherit;
}
.mermaid-diagram .node rect, .mermaid-diagram .node circle, .mermaid-diagram .node ellipse, .mermaid-diagram .node polygon, .mermaid-diagram .node path {
  fill: var(--color-bg-hover, #1e293b);
  stroke: var(--color-border-default, #334155);
  stroke-width: 2px;
}
.mermaid-diagram .node .label {
  color: var(--color-text-primary, #f1f5f9);
  font-family: 'Fira Code', monospace;
  font-size: 12px;
}
.mermaid-diagram .edgePath .path {
  stroke: var(--color-border-subtle, #475569);
  stroke-width: 2px;
}
.mermaid-diagram .edgeLabel {
  background-color: var(--color-bg-primary, #0f172a);
  color: var(--color-text-secondary, #94a3b8);
  font-size: 11px;
}
</style>
