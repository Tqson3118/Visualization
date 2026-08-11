<script setup lang="ts">
import { useToastStore } from '@/composables/useToast';
import { ref, computed, nextTick } from 'vue';
import { useAuthStore } from '../features/auth/store/useAuthStore';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import { api } from '@/services/apiClient';
import { marked } from 'marked';
import DOMPurify from 'dompurify';

const toastStore = useToastStore();

function renderMarkdown(text: string): string {
  try {
    const rawHtml = marked.parse(text) as string;
    return DOMPurify.sanitize(rawHtml);
  } catch (err) {
    return text;
  }
}

const authStore = useAuthStore();
const messages = ref<{id: number; role: 'user' | 'ai'; text: string}[]>([
  { id: 1, role: 'ai', text: 'Chào bạn! Tôi là Trợ lý AI chuyên môn về Cấu trúc dữ liệu và Giải thuật. Bạn cần hỗ trợ bài toán hay muốn tôi Debug lỗi code nào hôm nay?' }
]);
const inputMessage = ref('');
const isTyping = ref(false);
const messagesContainer = ref<HTMLElement | null>(null);

// Fetch real token quota from API
const hintTokens = ref(0);
const quotaError = ref(false);
const quotaLoading = ref(true);
const DEFAULT_FALLBACK_QUOTA = 5;
let quotaAttempts = 0;
let disposed = false;

const fetchQuota = async (): Promise<void> => {
  quotaLoading.value = true;
  try {
    const res = await api.get<{ remaining: number }>('/ai/chat/quota');
    hintTokens.value = res.remaining;
    quotaError.value = false;
    quotaAttempts = 0;
  } catch (error) {
    console.error('Failed to fetch AI quota', error);
    quotaError.value = true;
    // Fallback quota mặc định để UI không hiện "?" và không chặn gửi im lặng
    hintTokens.value = DEFAULT_FALLBACK_QUOTA;
    // Retry 1 lần sau 2s (phân biệt lỗi tạm thời vs lỗi vĩnh viễn)
    quotaAttempts += 1;
    if (quotaAttempts < 2) {
      setTimeout(() => { if (!disposed) void fetchQuota(); }, 2000);
    }
  } finally {
    quotaLoading.value = false;
  }
};

fetchQuota();

const scrollToBottom = async () => {
  await nextTick();
  if (messagesContainer.value) {
    messagesContainer.value.scrollTop = messagesContainer.value.scrollHeight;
  }
};

const sendMessage = async () => {
  if (!inputMessage.value.trim() || isTyping.value) return;

  // C4 — Enter phải tuân theo trạng thái nút Gửi (disabled): không bypass
  if (!authStore.isAuthenticated) {
    toastStore.info('Vui lòng đăng nhập để sử dụng Trợ lý AI.');
    return;
  }

  if (hintTokens.value <= 0) {
    toastStore.info('Bạn đã dùng hết lượt AI hôm nay. Hạn mức sẽ làm mới theo ngày.');
    return;
  }

  const userMsg = inputMessage.value;
  messages.value.push({
    id: Date.now(),
    role: 'user',
    text: userMsg
  });
  inputMessage.value = '';
  isTyping.value = true;
  await scrollToBottom();

  // Call real AI Chat API
  try {
    const aiResponse = await api.post<{ content: string }>('/ai/chat', { prompt: userMsg });
    
    // Deduct token locally for UX (Backend already deducted it)
    hintTokens.value = Math.max(0, hintTokens.value - 1);

    messages.value.push({
      id: Date.now() + 1,
      role: 'ai',
      text: aiResponse.content
    });
  } catch (error: any) {
    const errorMsg = error.response?.data?.message || '⚠️ Xin lỗi, kết nối đến Server AI đang gặp sự cố.';
    messages.value.push({
      id: Date.now() + 1,
      role: 'ai',
      text: errorMsg
    });
  } finally {
    isTyping.value = false;
    await scrollToBottom();
  }
};
</script>

<template>
  <div class="h-full w-full p-4 lg:p-6 flex flex-col items-center animate-fade-in relative overflow-hidden">
    
    <div class="w-full max-w-4xl flex-1 flex flex-col glass-panel rounded-2xl overflow-hidden shadow-2xl relative">
      
      <!-- Chat Header -->
      <div class="px-6 py-4 bg-bg-secondary/90 border-b border-border-default backdrop-blur-md flex items-center justify-between z-10 shrink-0">
        <div class="flex items-center gap-4">
          <div class="w-12 h-12 rounded-full bg-gradient-to-br from-accent to-accent-purple flex items-center justify-center shadow-lg shadow-indigo-500/20 border border-border-default">
            <BaseIcon name="ai-assistant" class="w-6 h-6 text-text-primary" />
          </div>
          <div>
            <h2 class="text-lg font-bold text-text-primary flex items-center gap-2">
              Chuyên gia AI
            </h2>
            <p class="text-xs text-accent-green flex items-center gap-1">
              <span class="w-1.5 h-1.5 rounded-full bg-accent-green animate-pulse"></span>
              Đang trực tuyến
            </p>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <div class="flex flex-col items-end">
            <span class="text-xs text-text-secondary">Lượt AI còn lại hôm nay</span>
            <div class="flex items-center gap-1 text-sm font-bold" :class="hintTokens > 0 ? 'text-accent-warm' : 'text-accent-red'">
              <BaseIcon name="zap" class="w-4 h-4" />
              {{ quotaLoading ? '...' : `${hintTokens} lượt` }}
            </div>
            <span v-if="quotaError" class="text-[10px] text-accent-red">
              Không tải được hạn mức — đang dùng mức mặc định
            </span>
          </div>
          <button @click="$router.push('/gems-shop')" class="p-2 bg-bg-hover hover:bg-bg-hover rounded-lg transition" title="Cửa hàng Gems">
            <BaseIcon name="gem" class="w-4 h-4 text-text-secondary" />
          </button>
        </div>
      </div>

      <!-- Chat Messages Area -->
      <div ref="messagesContainer" class="flex-1 overflow-y-auto p-6 space-y-6 custom-scrollbar bg-[url('/grid-bg.svg')] bg-center bg-cover bg-no-repeat bg-fixed bg-opacity-5">
        
        <div v-for="msg in messages" :key="msg.id" class="flex w-full" :class="msg.role === 'user' ? 'justify-end' : 'justify-start'">
          
          <div v-if="msg.role === 'ai'" class="w-8 h-8 rounded-full bg-accent/20 border border-border-accent flex items-center justify-center mr-3 shrink-0 mt-1">
            <BaseIcon name="ai-assistant" class="w-4 h-4 text-accent" />
          </div>

          <div 
            class="max-w-[80%] md:max-w-[70%] rounded-2xl px-5 py-3 text-sm leading-relaxed"
            :class="msg.role === 'user' 
              ? 'bg-accent text-text-primary rounded-tr-none shadow-lg shadow-indigo-600/20' 
              : 'bg-bg-hover border border-border-default text-text-primary rounded-tl-none shadow-md'"
          >
            <!-- AI trả markdown/code → render + sanitize (G3.8.4) -->
            <div v-if="msg.role === 'ai'" class="ai-markdown" v-html="renderMarkdown(msg.text)"></div>
            <p v-else class="whitespace-pre-wrap">{{ msg.text }}</p>
          </div>

        </div>

        <!-- Typing Indicator -->
        <div v-if="isTyping" class="flex w-full justify-start animate-fade-in">
          <div class="w-8 h-8 rounded-full bg-accent/20 border border-border-accent flex items-center justify-center mr-3 shrink-0">
            <BaseIcon name="ai-assistant" class="w-4 h-4 text-accent" />
          </div>
          <div class="bg-bg-hover border border-border-default rounded-2xl rounded-tl-none px-4 py-4 flex items-center gap-1.5 shadow-md">
            <div class="w-1.5 h-1.5 rounded-full bg-accent animate-bounce" style="animation-delay: 0ms;"></div>
            <div class="w-1.5 h-1.5 rounded-full bg-accent animate-bounce" style="animation-delay: 150ms;"></div>
            <div class="w-1.5 h-1.5 rounded-full bg-accent animate-bounce" style="animation-delay: 300ms;"></div>
          </div>
        </div>

      </div>

      <!-- Chat Input Area -->
      <div class="px-6 py-4 bg-bg-secondary border-t border-border-default shrink-0">
        <form @submit.prevent="sendMessage" class="relative flex items-end gap-3">
          <div class="flex-1 bg-bg-primary border border-border-default focus-within:border-border-accent rounded-xl overflow-hidden transition-colors shadow-inner flex flex-col">
            <textarea 
              v-model="inputMessage" 
              rows="2" 
              placeholder="Nhắn tin cho AI chuyên gia... (Có thể dán code vào đây)"
              class="w-full bg-transparent text-text-primary px-4 py-3 outline-none resize-none text-sm placeholder:text-text-muted custom-scrollbar"
              @keydown.enter.exact.prevent="sendMessage"
            ></textarea>
            <div class="px-4 py-2 bg-bg-secondary/50 flex items-center justify-between border-t border-border-default">
              <span class="text-[10px] text-text-muted font-mono">Nhấn Enter để gửi, Shift+Enter để xuống dòng</span>
              <span class="text-[10px] text-text-muted font-bold" :class="hintTokens > 0 ? '' : 'text-accent-red'">
                Hạn mức hôm nay
              </span>
            </div>
          </div>
          <button 
            type="submit" 
            :disabled="isTyping || !inputMessage.trim() || hintTokens <= 0"
            class="h-12 px-6 bg-accent hover:bg-accent disabled:bg-bg-hover disabled:text-text-muted disabled:cursor-not-allowed text-text-primary font-bold rounded-xl transition-colors shadow-lg shadow-indigo-600/20 flex items-center justify-center"
          >
            <span class="hidden sm:inline-block mr-2">Gửi</span>
            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 transform rotate-45 mb-1" viewBox="0 0 20 20" fill="currentColor">
              <path d="M10.894 2.553a1 1 0 00-1.788 0l-7 14a1 1 0 001.169 1.409l5-1.429A1 1 0 009 15.571V11a1 1 0 112 0v4.571a1 1 0 00.725.962l5 1.428a1 1 0 001.17-1.408l-7-14z" />
            </svg>
          </button>
        </form>
      </div>

    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.1);
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.2);
}

.animate-fade-in {
  animation: fadeIn 0.4s ease-out;
}
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

.ai-markdown { line-height: 1.6; }
.ai-markdown > *:first-child { margin-top: 0; }
.ai-markdown > *:last-child { margin-bottom: 0; }
.ai-markdown p { margin: 0.5em 0; }
.ai-markdown pre {
  background: var(--code-bg);
  border: 1px solid var(--code-border);
  border-radius: var(--radius-md);
  padding: 0.75rem 1rem;
  overflow-x: auto;
  font-family: var(--font-mono);
  font-size: 0.8rem;
  margin: 0.5rem 0;
}
.ai-markdown code {
  font-family: var(--font-mono);
  font-size: 0.82em;
  background: var(--code-bg);
  border: 1px solid var(--code-border);
  border-radius: 4px;
  padding: 0.1em 0.35em;
}
.ai-markdown pre code { background: transparent; border: none; padding: 0; }
.ai-markdown ul, .ai-markdown ol { padding-left: 1.5rem; margin: 0.5rem 0; }
.ai-markdown li { margin: 0.2rem 0; }
.ai-markdown h1, .ai-markdown h2, .ai-markdown h3, .ai-markdown h4 {
  color: var(--color-text-heading);
  margin: 0.75rem 0 0.4rem;
  line-height: 1.3;
}
.ai-markdown h1 { font-size: 1.15rem; }
.ai-markdown h2 { font-size: 1.05rem; }
.ai-markdown h3 { font-size: 0.95rem; }
.ai-markdown h4 { font-size: 0.9rem; }
.ai-markdown a { color: var(--color-accent-primary); text-decoration: underline; }
.ai-markdown blockquote {
  border-left: 3px solid var(--color-border-accent);
  padding-left: 0.75rem;
  margin: 0.5rem 0;
  color: var(--color-text-secondary);
}
.ai-markdown table { border-collapse: collapse; margin: 0.5rem 0; width: 100%; }
.ai-markdown th, .ai-markdown td { border: 1px solid var(--color-border-default); padding: 0.35rem 0.6rem; text-align: left; }

</style>
