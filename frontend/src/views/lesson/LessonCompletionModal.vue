<template>
  <Teleport to="body">
    <div v-if="show" class="fixed inset-0 bg-black/75 backdrop-blur-md flex items-center justify-center p-4 z-50 animate-fade-in">
      <div class="bg-[#131220] border border-purple-500/30 rounded-3xl p-6 sm:p-8 max-w-md w-full text-center shadow-[0_0_50px_rgba(168,85,247,0.25)] relative overflow-hidden">
        <!-- Glowing aura -->
        <div class="absolute -top-16 -left-16 w-40 h-40 rounded-full bg-emerald-500/15 blur-[60px] pointer-events-none"></div>
        <div class="absolute -bottom-16 -right-16 w-40 h-40 rounded-full bg-purple-600/20 blur-[60px] pointer-events-none"></div>

        <div class="w-16 h-16 rounded-full bg-emerald-500/20 border border-emerald-500/40 flex items-center justify-center text-emerald-400 mx-auto mb-4 shadow-[0_0_24px_rgba(16,185,129,0.3)] animate-pulse">
          <CheckCircle2 class="w-9 h-9" />
        </div>
        
        <div class="inline-flex items-center gap-1 px-3 py-0.5 rounded-full bg-emerald-500/10 border border-emerald-500/20 text-emerald-400 text-[11px] font-extrabold uppercase tracking-wider mb-2">
          <Sparkles class="w-3 h-3" /> Hoàn thành bài học
        </div>

        <h3 class="text-2xl font-black text-white tracking-tight">Xuất Sắc!</h3>
        <p class="text-slate-300 mt-1.5 text-xs sm:text-sm leading-relaxed">
          Bạn đã hoàn thành bài học.
        </p>

        <div class="my-5 p-4 rounded-2xl bg-[#1a182c] border border-purple-500/20 inline-flex flex-col items-center w-full shadow-inner">
          <span class="text-[11px] text-slate-400 font-bold uppercase tracking-widest flex items-center gap-1">
            <Zap class="w-3.5 h-3.5 text-amber-400" /> Điểm kinh nghiệm nhận được
          </span>
          <span class="text-3xl font-black text-amber-400 mt-1 drop-shadow-[0_0_8px_rgba(245,158,11,0.5)]">
            +{{ xpReward }} XP
          </span>
        </div>

        <div class="flex flex-col gap-3">
          <button
            v-if="nextLessonId"
            @click="$emit('go-next', nextLessonId)"
            class="w-full py-3.5 bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white font-bold rounded-xl transition-all duration-300 cursor-pointer flex items-center justify-center gap-2 shadow-lg shadow-purple-600/30 hover:scale-[1.02]"
          >
            <span>Học bài tiếp theo</span>
            <ArrowRight class="w-4 h-4" />
          </button>

          <button
            @click="$emit('close')"
            class="w-full py-3 bg-[#1e1c30] hover:bg-[#27243f] text-slate-200 hover:text-white font-bold rounded-xl transition-all border border-slate-700/60 cursor-pointer text-xs sm:text-sm"
          >
            Quay lại lộ trình
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { CheckCircle2, Sparkles, Zap, ArrowRight } from 'lucide-vue-next';

defineProps<{
  show: boolean;
  xpReward: number;
  quizId?: string | null;
  nextLessonId?: string | null;
}>();

defineEmits<{
  (e: 'go-next', lessonId: string): void;
  (e: 'close'): void;
}>();
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.25s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.96); }
  to { opacity: 1; transform: scale(1); }
}
</style>
