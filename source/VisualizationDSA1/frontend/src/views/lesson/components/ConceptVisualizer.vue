<template>
  <div class="concept-visualizer w-full h-full flex items-center justify-center overflow-hidden relative bg-bg-primary">
    <div class="max-w-xl w-full px-4 py-6">
      <div class="text-center mb-6">
        <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-accent/15 text-accent border border-accent/30 text-[11px] font-bold uppercase tracking-wider">
          {{ topicLabel }}
        </div>
        <h3 class="text-lg font-extrabold text-text-primary mt-3">{{ title }}</h3>
        <p class="text-sm text-text-secondary mt-1">{{ description }}</p>
      </div>

      <div class="relative glass-panel rounded-2xl p-6 min-h-[220px] flex items-center justify-center">
        <component :is="sceneComponent" />
      </div>

      <div class="mt-5 flex items-center justify-between gap-3">
        <span class="text-xs text-text-muted">Minh hoạ trực quan — điều chỉnh tốc độ nếu cần</span>
        <select
          class="bg-bg-surface border border-border-default rounded-lg px-2 py-1 text-xs text-text-secondary"
          :value="animationSpeed"
          @change="onSpeedChange"
        >
          <option :value="0.5">0.5x</option>
          <option :value="1">1x</option>
          <option :value="2">2x</option>
        </select>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, type Component } from 'vue';
import { h } from 'vue';

const props = defineProps<{
  algorithm: string;
  title?: string;
  description?: string;
  speed?: number;
}>();

const emit = defineEmits<{
  (e: 'speedChange', speed: number): void;
}>();

const animationSpeed = computed(() => props.speed ?? 1);

function onSpeedChange(e: Event): void {
  const target = e.target as HTMLSelectElement;
  emit('speedChange', parseFloat(target.value));
}

const topicLabel = computed(() => {
  const algo = props.algorithm.toLowerCase();
  if (algo.includes('encapsulation')) return 'OOP · Đóng gói';
  if (algo.includes('inheritance')) return 'OOP · Kế thừa';
  if (algo.includes('polymorphism')) return 'OOP · Đa hình';
  if (algo.includes('abstraction')) return 'OOP · Trừu tượng';
  if (algo.includes('srp')) return 'SOLID · S';
  if (algo.includes('ocp')) return 'SOLID · O';
  if (algo.includes('lsp')) return 'SOLID · L';
  if (algo.includes('isp')) return 'SOLID · I';
  if (algo.includes('dip')) return 'SOLID · D';
  if (algo.includes('strategy')) return 'Design Pattern';
  return 'Khái niệm';
});

const sceneComponent = computed<Component>(() => {
  const algo = props.algorithm.toLowerCase();
  if (algo.includes('encapsulation')) return { render: renderEncapsulation };
  if (algo.includes('inheritance')) return { render: renderInheritance };
  if (algo.includes('polymorphism')) return { render: renderPolymorphism };
  if (algo.includes('abstraction')) return { render: renderAbstraction };
  if (algo.includes('srp')) return { render: renderSrp };
  if (algo.includes('ocp')) return { render: renderOcp };
  if (algo.includes('lsp')) return { render: renderLsp };
  if (algo.includes('isp')) return { render: renderIsp };
  if (algo.includes('dip')) return { render: renderDip };
  return { render: () => renderGeneric(props.title, props.description) };
});

function renderGeneric(title: string | undefined, description: string | undefined) {
  return h('div', { class: 'flex flex-col items-center justify-center gap-4 py-6 text-center' }, [
    h('div', { class: 'w-16 h-16 rounded-2xl bg-accent/15 border-2 border-accent/40 flex items-center justify-center' }, [
      h('svg', { class: 'w-8 h-8 text-accent', fill: 'none', stroke: 'currentColor', 'stroke-width': 1.5, viewBox: '0 0 24 24' }, [
        h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z' }),
      ]),
    ]),
    h('div', { class: 'text-sm font-bold text-text-primary' }, title || 'Khái niệm trực quan'),
    h('div', { class: 'text-xs text-text-muted max-w-xs' }, description || 'Hình minh hoạ trực quan cho khái niệm này đang được chuẩn bị.'),
  ]);
}

function renderEncapsulation() {
  return h('div', { class: 'relative w-56 h-40 mx-auto' }, [
    h('div', { class: 'absolute inset-0 flex items-center justify-center' }, [
      h('div', { class: 'w-28 h-32 rounded-xl border-2 border-accent-cyan/50 bg-bg-surface shadow-lg relative overflow-hidden encap-box' }, [
        h('div', { class: 'absolute top-0 left-0 right-0 h-6 bg-accent-cyan/20 flex items-center justify-center gap-1 border-b border-accent-cyan/30' }, [
          h('span', { class: 'w-2 h-2 rounded-full bg-accent-cyan/70' }),
          h('span', { class: 'text-[8px] font-mono text-text-secondary' }, 'private'),
        ]),
        h('div', { class: 'absolute top-8 left-3 right-3 space-y-2' }, [
          h('div', { class: 'h-2.5 rounded bg-text-secondary/20 encap-data' }),
          h('div', { class: 'h-2.5 rounded bg-text-secondary/20 encap-data' }),
          h('div', { class: 'h-2.5 rounded bg-text-secondary/20 encap-data' }),
        ]),
        h('div', { class: 'absolute bottom-3 left-1/2 -translate-x-1/2 w-10 h-10 rounded-full bg-accent/20 border-2 border-accent/60 flex items-center justify-center encap-lock' }, [
          h('svg', { class: 'w-5 h-5 text-accent', fill: 'none', stroke: 'currentColor', 'stroke-width': 2, viewBox: '0 0 24 24' }, [
            h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z' }),
          ]),
        ]),
      ]),
    ]),
    h('div', { class: 'absolute -left-6 top-1/2 -translate-y-1/2' }, [
      h('div', { class: 'text-[10px] font-mono text-accent-green bg-accent-green/10 border border-accent-green/30 rounded-lg px-2 py-1' }, 'get()'),
    ]),
    h('div', { class: 'absolute -right-6 top-1/2 -translate-y-1/2' }, [
      h('div', { class: 'text-[10px] font-mono text-accent-warm bg-accent-warm/10 border border-accent-warm/30 rounded-lg px-2 py-1' }, 'set()'),
    ]),
  ]);
}

function renderInheritance() {
  return h('div', { class: 'flex flex-col items-center gap-5 py-4' }, [
    h('div', { class: 'px-5 py-2.5 rounded-xl border-2 border-accent/50 bg-accent/10 text-sm font-bold text-text-primary' }, 'Animal'),
    h('svg', { class: 'w-6 h-6 text-text-muted', fill: 'none', stroke: 'currentColor', 'stroke-width': 2, viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M19 9l-7 7-7-7' }),
    ]),
    h('div', { class: 'flex gap-6' }, [
      h('div', { class: 'px-4 py-2 rounded-lg border border-border-default bg-bg-surface text-xs font-semibold text-text-secondary inh-child' }, 'Dog'),
      h('div', { class: 'px-4 py-2 rounded-lg border border-border-default bg-bg-surface text-xs font-semibold text-text-secondary inh-child' }, 'Cat'),
      h('div', { class: 'px-4 py-2 rounded-lg border border-border-default bg-bg-surface text-xs font-semibold text-text-secondary inh-child' }, 'Bird'),
    ]),
  ]);
}

function renderPolymorphism() {
  return h('div', { class: 'flex items-center gap-6 py-4' }, [
    h('div', { class: 'text-xs font-mono text-text-muted' }, 'Shape.draw()'),
    h('div', { class: 'flex flex-col gap-3' }, [
      h('div', { class: 'flex items-center gap-3' }, [
        h('div', { class: 'w-12 h-12 rounded-lg border-2 border-accent-cyan/60 bg-accent-cyan/15 animate-pulse' }),
        h('span', { class: 'text-xs text-text-secondary' }, 'Rectangle'),
      ]),
      h('div', { class: 'flex items-center gap-3' }, [
        h('div', { class: 'w-12 h-12 rounded-full border-2 border-accent-green/60 bg-accent-green/15 animate-pulse' }),
        h('span', { class: 'text-xs text-text-secondary' }, 'Circle'),
      ]),
      h('div', { class: 'flex items-center gap-3' }, [
        h('div', { class: 'w-12 h-12 border-2 border-accent-warm/60 bg-accent-warm/15 rotate-45' }),
        h('span', { class: 'text-xs text-text-secondary' }, 'Square'),
      ]),
    ]),
  ]);
}

function renderAbstraction() {
  return h('div', { class: 'flex flex-col items-center gap-4 py-4' }, [
    h('div', { class: 'px-5 py-3 rounded-xl border-2 border-dashed border-accent-purple/60 bg-accent-purple/10 text-sm font-bold text-text-primary' }, 'IBankAccount'),
    h('svg', { class: 'w-6 h-6 text-text-muted', fill: 'none', stroke: 'currentColor', 'stroke-width': 2, viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M19 9l-7 7-7-7' }),
    ]),
    h('div', { class: 'flex gap-4' }, [
      h('div', { class: 'px-4 py-2 rounded-lg border border-border-default bg-bg-surface text-xs font-semibold text-text-secondary' }, 'SavingsAccount'),
      h('div', { class: 'px-4 py-2 rounded-lg border border-border-default bg-bg-surface text-xs font-semibold text-text-secondary' }, 'CheckingAccount'),
    ]),
  ]);
}

function renderSrp() {
  return h('div', { class: 'flex flex-col items-center gap-4 py-4' }, [
    h('div', { class: 'px-5 py-3 rounded-xl border-2 border-accent-red/50 bg-accent-red/10 text-sm font-bold text-text-primary' }, 'ReportPrinter'),
    h('svg', { class: 'w-6 h-6 text-text-muted', fill: 'none', stroke: 'currentColor', 'stroke-width': 2, viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M19 9l-7 7-7-7' }),
    ]),
    h('div', { class: 'flex gap-3 flex-wrap justify-center' }, [
      h('div', { class: 'px-3 py-2 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'Format'),
      h('div', { class: 'px-3 py-2 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'Send to Printer'),
      h('div', { class: 'px-3 py-2 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'Log'),
    ]),
    h('span', { class: 'text-[10px] text-accent-green font-bold' }, '✔ Mỗi class chỉ 1 lý do để thay đổi'),
  ]);
}

function renderOcp() {
  return h('div', { class: 'flex flex-col items-center gap-4 py-4' }, [
    h('div', { class: 'px-5 py-3 rounded-xl border-2 border-accent-green/50 bg-accent-green/10 text-sm font-bold text-text-primary' }, 'Open for Extension'),
    h('div', { class: 'flex gap-2 items-center' }, [
      h('span', { class: 'text-[10px] font-mono text-text-muted' }, 'interface'),
      h('div', { class: 'px-3 py-1.5 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'Plugin A'),
      h('div', { class: 'px-3 py-1.5 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'Plugin B'),
      h('div', { class: 'px-3 py-1.5 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'Plugin C'),
    ]),
    h('span', { class: 'text-[10px] text-accent-cyan font-bold' }, 'Mở rộng mà không sửa code gốc'),
  ]);
}

function renderLsp() {
  return h('div', { class: 'flex flex-col items-center gap-4 py-4' }, [
    h('div', { class: 'flex gap-6 items-center' }, [
      h('div', { class: 'flex flex-col items-center gap-2' }, [
        h('div', { class: 'w-14 h-14 rounded-xl border-2 border-accent-green/50 bg-accent-green/10 flex items-center justify-center text-xs font-bold' }, 'Bird'),
        h('span', { class: 'text-[10px] text-text-secondary' }, 'fly()'),
      ]),
      h('span', { class: 'text-text-muted text-lg' }, '→'),
      h('div', { class: 'flex flex-col items-center gap-2' }, [
        h('div', { class: 'w-14 h-14 rounded-xl border-2 border-accent/50 bg-accent/10 flex items-center justify-center text-xs font-bold' }, 'Penguin'),
        h('span', { class: 'text-[10px] text-accent-red' }, '✕ fly() breaks'),
      ]),
    ]),
    h('span', { class: 'text-[10px] text-accent-warm font-bold' }, 'Lớp con phải thay thế được lớp cha'),
  ]);
}

function renderIsp() {
  return h('div', { class: 'flex flex-col items-center gap-4 py-4' }, [
    h('div', { class: 'text-xs font-mono text-accent-red/80 line-through' }, 'IWorker { run(); eat(); sleep(); }'),
    h('svg', { class: 'w-6 h-6 text-text-muted', fill: 'none', stroke: 'currentColor', 'stroke-width': 2, viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M19 9l-7 7-7-7' }),
    ]),
    h('div', { class: 'flex gap-4 flex-wrap justify-center' }, [
      h('div', { class: 'px-3 py-2 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'IWorkable { run() }'),
      h('div', { class: 'px-3 py-2 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'IFeedable { eat() }'),
      h('div', { class: 'px-3 py-2 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'IRestable { sleep() }'),
    ]),
  ]);
}

function renderDip() {
  return h('div', { class: 'flex flex-col items-center gap-4 py-4' }, [
    h('div', { class: 'px-5 py-2.5 rounded-xl border-2 border-accent/50 bg-accent/10 text-sm font-bold text-text-primary' }, 'Service (High-level)'),
    h('svg', { class: 'w-6 h-6 text-text-muted', fill: 'none', stroke: 'currentColor', 'stroke-width': 2, viewBox: '0 0 24 24' }, [
      h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M15 13l-3-3m0 0l-3 3m3-3v12' }),
    ]),
    h('div', { class: 'px-4 py-1.5 rounded-lg border-2 border-dashed border-accent-purple/50 bg-accent-purple/10 text-[11px] font-bold text-text-secondary' }, 'IStorage (abstraction)'),
    h('div', { class: 'flex gap-4' }, [
      h('div', { class: 'px-3 py-1.5 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'SqlStorage'),
      h('div', { class: 'px-3 py-1.5 rounded-lg border border-border-default bg-bg-surface text-[11px] font-semibold text-text-secondary' }, 'RedisStorage'),
    ]),
  ]);
}
</script>

<style scoped>
.encap-box .encap-lock { animation: encapPulse 2.4s ease-in-out infinite; }
.encap-box .encap-data { animation: encapData 2.4s ease-in-out infinite; }
.inh-child { animation: inhPop 2.4s ease-in-out infinite; }

@keyframes encapPulse {
  0%, 100% { transform: translateX(-50%) scale(1); }
  50% { transform: translateX(-50%) scale(1.1); }
}
@keyframes encapData {
  0%, 100% { opacity: 0.35; }
  50% { opacity: 1; }
}
@keyframes inhPop {
  0%, 100% { transform: translateY(0); }
  50% { transform: translateY(-4px); }
}
</style>
