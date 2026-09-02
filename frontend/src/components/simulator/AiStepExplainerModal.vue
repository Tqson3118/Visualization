<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { Bot, Sparkles, Send } from 'lucide-vue-next';
import type { Step } from '@/engines/core/types';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    step: Step | null;
    algorithmTitle?: string;
    pseudocodeLine?: string;
  }>(),
  {
    open: false,
    step: null,
    algorithmTitle: 'Thuật toán',
    pseudocodeLine: '',
  },
);

const emit = defineEmits<{
  close: [];
}>();

const userQuestion = ref('');
const isThinking = ref(false);
const chatHistory = ref<Array<{ sender: 'user' | 'ai'; text: string }>>([]);

// Phân tích tự động bước hiện tại
const currentAnalysis = computed(() => {
  if (!props.step) return null;
  const s = props.step;
  const vars = Object.entries(s.variables)
    .filter(([k, v]) => v !== null && v !== undefined && k !== 'array')
    .map(([k, v]) => `${k} = ${String(v)}`)
    .join(', ');

  const isSwap = s.structure.elements.some((e) => e.status === 'swap');
  const isCompare = s.structure.elements.some((e) => e.status === 'active' || e.status === 'highlight');
  const isDone = s.structure.elements.length > 0 && s.structure.elements.every((e) => e.status === 'done');

  let intent = 'Duyệt và cập nhật trạng thái các phần tử.';
  if (isDone) {
    intent = 'Thuật toán đã hoàn tất! Toàn bộ cấu trúc dữ liệu đã đạt trạng thái đích tối ưu.';
  } else if (isSwap) {
    intent = 'Hoán đổi / Cập nhật giá trị để thỏa mãn bất biến của thuật toán (đưa phần tử về đúng thứ tự hoặc thư giãn khoảng cách).';
  } else if (isCompare) {
    intent = 'So sánh giá trị các phần tử để đưa ra quyết định phân nhánh trong bước tiếp theo.';
  }

  return {
    explanation: s.explanation,
    annotations: s.annotations,
    vars: vars || 'Không có biến cục bộ',
    line: s.pseudocodeLine,
    intent,
    stats: s.stats,
  };
});

async function handleAskAi(): Promise<void> {
  if (!userQuestion.value.trim() || isThinking.value) return;
  const q = userQuestion.value.trim();
  chatHistory.value.push({ sender: 'user', text: q });
  userQuestion.value = '';
  isThinking.value = true;

  try {
    const { askAiStepExplanation } = await import('@/services/aiTutorService');
    const reply = await askAiStepExplanation({
      algorithmTitle: props.algorithmTitle || 'Thuật toán DSA',
      stepIndex: props.step?.index ?? 0,
      explanation: currentAnalysis.value?.explanation || props.step?.explanation,
      variables: currentAnalysis.value?.vars,
      pseudocodeLine: String(currentAnalysis.value?.line ?? ''),
      userQuestion: q,
    });
    chatHistory.value.push({ sender: 'ai', text: reply });
  } catch (err) {
    const errorMsg = err instanceof Error ? err.message : 'Không thể kết nối AI Tutor lúc này.';
    chatHistory.value.push({ sender: 'ai', text: `⚠️ ${errorMsg}` });
  } finally {
    isThinking.value = false;
  }
}

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      chatHistory.value = [];
      userQuestion.value = '';
    }
  },
);
</script>

<template>
  <Modal
    :open="open"
    :title="`AI Giải thích: ${algorithmTitle}`"
    width="580px"
    @close="emit('close')"
  >
    <div class="ai-explainer">
      <!-- Card phân tích thông minh -->
      <div v-if="currentAnalysis" class="ai-explainer__card">
        <div class="ai-explainer__header">
          <div class="flex items-center gap-2 text-purple-400 font-semibold text-sm">
            <Sparkles :size="16" />
            <span>Phân tích ngữ cảnh bước #{{ (step?.index ?? 0) + 1 }}</span>
          </div>
          <span class="text-xs px-2 py-0.5 rounded bg-purple-500/20 text-purple-300 font-mono">
            Dòng mã giả #{{ currentAnalysis.line }}
          </span>
        </div>

        <div class="ai-explainer__body">
          <div class="ai-explainer__section">
            <h4 class="text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Mục đích bước chạy:</h4>
            <p class="text-sm text-slate-200 leading-relaxed font-medium">
              {{ currentAnalysis.intent }}
            </p>
          </div>

          <div class="ai-explainer__section bg-black/30 p-3 rounded-lg border border-white/5">
            <div class="text-xs text-slate-400 mb-1">Chi tiết thực thi:</div>
            <div class="text-sm text-amber-300 font-mono">{{ currentAnalysis.explanation }}</div>
            <div class="text-xs text-slate-400 mt-2">Biến cục bộ: <span class="text-teal-300 font-mono">{{ currentAnalysis.vars }}</span></div>
          </div>

          <div v-if="currentAnalysis.annotations.length > 0" class="ai-explainer__section">
            <h4 class="text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">Ghi chú DSA:</h4>
            <ul class="text-xs text-slate-300 space-y-1 pl-4 list-disc">
              <li v-for="(note, idx) in currentAnalysis.annotations" :key="idx">{{ note }}</li>
            </ul>
          </div>
        </div>
      </div>

      <!-- Chat tương tác với AI -->
      <div class="ai-explainer__chat">
        <div class="ai-explainer__chat-title">
          <Bot :size="15" class="text-purple-400" />
          <span>Hỏi đáp cùng AI DSA Tutor</span>
        </div>

        <div class="ai-explainer__messages">
          <div v-if="chatHistory.length === 0" class="text-xs text-slate-400 text-center py-4">
            Bạn có thắc mắc tại sao thuật toán lại so sánh hoặc hoán đổi ở bước này? Hãy hỏi AI bên dưới.
          </div>
          <div
            v-for="(msg, idx) in chatHistory"
            :key="idx"
            class="ai-explainer__msg"
            :class="msg.sender === 'user' ? 'ai-explainer__msg--user' : 'ai-explainer__msg--ai'"
          >
            <div class="text-xs font-semibold mb-0.5 opacity-70">{{ msg.sender === 'user' ? 'Bạn' : 'AI Tutor' }}</div>
            <div class="text-xs leading-relaxed">{{ msg.text }}</div>
          </div>
          <div v-if="isThinking" class="text-xs text-purple-300 flex items-center gap-1.5 p-2 animate-pulse">
            <Sparkles :size="12" /> AI đang phân tích logic bước chạy...
          </div>
        </div>

        <form class="ai-explainer__input-box" @submit.prevent="handleAskAi">
          <input
            v-model="userQuestion"
            type="text"
            placeholder="Đặt câu hỏi về bước này (VD: Tại sao cần hoán đổi?)..."
            class="ai-explainer__input"
          />
          <Button type="submit" size="sm" variant="primary" :disabled="!userQuestion.trim() || isThinking">
            <Send :size="13" />
          </Button>
        </form>
      </div>

      <div class="flex justify-end pt-2">
        <Button variant="secondary" size="sm" @click="emit('close')">
          Đóng
        </Button>
      </div>
    </div>
  </Modal>
</template>

<style scoped>
.ai-explainer {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.ai-explainer__card {
  background: #12101F;
  border: 1px solid rgba(168, 85, 247, 0.25);
  border-radius: 12px;
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.ai-explainer__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
  padding-bottom: 8px;
}

.ai-explainer__body {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.ai-explainer__chat {
  background: #0E0C17;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 12px;
  padding: 12px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.ai-explainer__chat-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  font-weight: 600;
  color: #E2E8F0;
}

.ai-explainer__messages {
  max-height: 150px;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 4px;
}

.ai-explainer__msg {
  padding: 8px 10px;
  border-radius: 8px;
  max-width: 90%;
}

.ai-explainer__msg--user {
  align-self: flex-end;
  background: #3B82F6;
  color: #FFFFFF;
}

.ai-explainer__msg--ai {
  align-self: flex-start;
  background: #231C38;
  border: 1px solid rgba(168, 85, 247, 0.2);
  color: #E2E8F0;
}

.ai-explainer__input-box {
  display: flex;
  gap: 6px;
  margin-top: 4px;
}

.ai-explainer__input {
  flex: 1;
  background: #181528;
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 8px;
  padding: 6px 10px;
  font-size: 12px;
  color: #FFFFFF;
}

.ai-explainer__input:focus {
  outline: none;
  border-color: #A855F7;
}
</style>
