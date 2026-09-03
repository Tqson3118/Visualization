<template>
  <Teleport to="body">
    <div v-if="show" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-md animate-fade-in">
      <div class="relative w-full max-w-lg bg-[#12111d] border border-purple-500/30 rounded-3xl p-6 sm:p-8 text-center shadow-[0_0_60px_rgba(168,85,247,0.35)] overflow-hidden">
        
        <!-- Glowing background effects -->
        <div class="absolute -top-20 -left-20 w-52 h-52 rounded-full bg-purple-600/20 blur-[80px] pointer-events-none"></div>
        <div class="absolute -bottom-20 -right-20 w-52 h-52 rounded-full bg-amber-500/20 blur-[80px] pointer-events-none"></div>

        <!-- Trophy & Star Header -->
        <div class="relative mx-auto mb-4 w-20 h-20 sm:w-24 sm:h-24 rounded-full bg-gradient-to-tr from-amber-500/20 via-purple-500/20 to-emerald-500/20 border border-amber-400/40 flex items-center justify-center shadow-[0_0_30px_rgba(245,158,11,0.35)] animate-bounce-short">
          <Trophy class="w-10 h-10 sm:w-12 sm:h-12 text-amber-400 drop-shadow-[0_0_14px_rgba(245,158,11,0.8)]" />
        </div>

        <div class="inline-flex items-center gap-1.5 px-3.5 py-1 rounded-full bg-amber-400/10 border border-amber-400/30 text-amber-300 text-xs font-black uppercase tracking-widest mb-2">
          <Sparkles class="w-3.5 h-3.5 text-amber-400" />
          <span>Hoàn Thành Lộ Trình</span>
        </div>

        <h2 class="text-2xl sm:text-3xl font-black text-white tracking-tight">
          Xuất Sắc! Bạn Đã Về Đích!
        </h2>
        <p class="text-slate-300 text-xs sm:text-sm mt-2 max-w-sm mx-auto leading-relaxed">
          Bạn đã hoàn thành 100% tất cả các bài học và thử thách trong lộ trình <strong class="text-purple-300">{{ courseTitle }}</strong>.
        </p>

        <!-- Thành tích đạt được (Milestone Summary) -->
        <div class="my-6 p-4 sm:p-5 rounded-2xl bg-[#181628] border border-purple-500/20 text-center shadow-inner">
          <div class="grid grid-cols-2 gap-3">
            <div class="p-3 rounded-xl bg-[#1f1c34] border border-emerald-500/20 flex flex-col items-center justify-center">
              <span class="text-[11px] text-slate-400 font-bold uppercase tracking-wider flex items-center gap-1">
                <CheckCircle2 class="w-3.5 h-3.5 text-emerald-400" /> Bài hoàn thành
              </span>
              <span class="text-xl font-black text-emerald-400 mt-0.5">+{{ lessonXp ?? 50 }} XP</span>
            </div>

            <div class="p-3 rounded-xl bg-[#1f1c34] border border-amber-500/20 flex flex-col items-center justify-center">
              <span class="text-[11px] text-slate-400 font-bold uppercase tracking-wider flex items-center gap-1">
                <Zap class="w-3.5 h-3.5 text-amber-400" /> Tổng điểm lộ trình
              </span>
              <span class="text-xl font-black text-amber-400 mt-0.5">{{ totalXp }} XP</span>
            </div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div class="flex flex-col sm:flex-row gap-3 justify-center">
          <button
            type="button"
            @click="$emit('close')"
            class="px-5 py-3 rounded-xl bg-[#1e1c30] hover:bg-[#27243f] text-slate-300 hover:text-white border border-slate-700/60 font-bold text-xs sm:text-sm transition-all cursor-pointer"
          >
            Quay lại lộ trình
          </button>
          
          <button
            type="button"
            @click="$emit('explore-more')"
            class="px-6 py-3 rounded-xl bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 text-white font-bold text-xs sm:text-sm shadow-lg shadow-purple-600/30 transition-all flex items-center justify-center gap-2 cursor-pointer hover:scale-[1.02]"
          >
            <span>Khám phá lộ trình khác</span>
            <ArrowRight class="w-4 h-4" />
          </button>
        </div>

      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { Trophy, Sparkles, CheckCircle2, Zap, ArrowRight } from 'lucide-vue-next';

defineProps<{
  show: boolean;
  courseTitle: string;
  totalXp?: number;
  lessonXp?: number;
}>();

defineEmits<{
  (e: 'close'): void;
  (e: 'explore-more'): void;
}>();
</script>

<style scoped>
@keyframes bounceShort {
  0%, 100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-8px);
  }
}
.animate-bounce-short {
  animation: bounceShort 2s ease-in-out infinite;
}
.animate-fade-in {
  animation: fadeIn 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}
@keyframes fadeIn {
  from { opacity: 0; transform: scale(0.96); }
  to { opacity: 1; transform: scale(1); }
}
</style>