<template>
  <div class="flex flex-col sm:flex-row items-stretch sm:items-center gap-3 bg-vdsa-bg-secondary/40 p-4 rounded-2xl border border-vdsa-border-subtle relative z-20">
    <!-- 1. Ô tìm kiếm -->
    <div class="relative flex-1 min-w-[200px]">
      <input
        :value="searchQuery"
        @input="$emit('update:searchQuery', ($event.target as HTMLInputElement).value)"
        type="text"
        placeholder="Tìm kiếm lộ trình theo tên, mô tả..."
        class="w-full px-4 py-2.5 pl-10 bg-vdsa-bg-secondary border border-vdsa-border-subtle rounded-xl text-sm text-white placeholder-text-muted focus:outline-none focus:border-vdsa-accent/50 transition-colors"
      />
      <BaseIcon name="search" class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-vdsa-muted" />
    </div>

    <!-- 2. Dropdown bấm mở danh sách Chủ đề -->
    <div v-if="topics && topics.length > 1" ref="topicDropdownRef" class="relative shrink-0">
      <button
        type="button"
        @click="isTopicOpen = !isTopicOpen"
        class="w-full sm:w-auto flex items-center justify-between sm:justify-start gap-2 px-3.5 py-2.5 rounded-xl text-xs font-bold transition-all border cursor-pointer select-none"
        :class="(selectedTopic && selectedTopic !== 'All')
          ? 'bg-purple-600/25 text-purple-200 border-purple-500/60 shadow-sm shadow-purple-500/20'
          : 'bg-vdsa-bg-secondary text-slate-300 border-vdsa-border-subtle hover:border-vdsa-border hover:text-white'"
        aria-haspopup="listbox"
        :aria-expanded="isTopicOpen"
      >
        <span class="text-vdsa-muted font-semibold text-[11px] uppercase tracking-wider">Chủ đề:</span>
        <span class="font-bold max-w-[160px] truncate text-white">
          {{ (selectedTopic && selectedTopic !== 'All') ? selectedTopic : 'Tất cả' }}
        </span>
        <ChevronDown
          :size="14"
          class="transition-transform duration-200 text-slate-400 shrink-0"
          :class="{ 'rotate-180 text-purple-300': isTopicOpen }"
        />
      </button>

      <!-- Danh sách menu Chủ đề mở ra khi bấm -->
      <Transition
        enter-active-class="transition duration-150 ease-out"
        enter-from-class="transform scale-95 opacity-0"
        enter-to-class="transform scale-100 opacity-100"
        leave-active-class="transition duration-100 ease-in"
        leave-from-class="transform scale-100 opacity-100"
        leave-to-class="transform scale-95 opacity-0"
      >
        <div
          v-if="isTopicOpen"
          class="absolute left-0 sm:left-auto sm:right-0 top-full mt-2 w-72 max-h-80 overflow-y-auto z-50 bg-[#141522] border border-slate-700/80 rounded-xl shadow-2xl p-1.5 space-y-1 backdrop-blur-xl"
          role="listbox"
        >
          <div class="px-2.5 py-1.5 text-[10px] font-bold uppercase tracking-wider text-slate-400 border-b border-slate-800/80">
            <span>Chọn chủ đề</span>
          </div>

          <button
            v-for="topic in topics"
            :key="topic"
            type="button"
            @click="handleSelectTopic(topic)"
            class="w-full flex items-center justify-between px-3 py-2 rounded-lg text-xs font-medium transition-all text-left cursor-pointer"
            :class="(selectedTopic || 'All') === topic
              ? 'bg-purple-600/30 text-purple-200 font-bold border border-purple-500/30'
              : 'text-slate-300 hover:bg-white/5 hover:text-white border border-transparent'"
            role="option"
            :aria-selected="(selectedTopic || 'All') === topic"
          >
            <span class="truncate">{{ topic === 'All' ? '🌐 Tất cả chủ đề' : topic }}</span>
            <Check v-if="(selectedTopic || 'All') === topic" :size="14" class="text-purple-400 shrink-0 ml-2" />
          </button>
        </div>
      </Transition>
    </div>

    <!-- 3. Bộ lọc Cấp độ -->
    <div class="flex items-center gap-1.5 overflow-x-auto pb-1 sm:pb-0 no-scrollbar shrink-0">
      <span class="text-[11px] text-vdsa-muted font-bold uppercase tracking-wider mr-1.5 shrink-0">Cấp độ</span>
      <button
        v-for="diff in difficulties"
        :key="diff"
        @click="$emit('update:difficulty', diff)"
        class="px-3 py-2 rounded-lg text-xs transition-all duration-200 cursor-pointer shrink-0 whitespace-nowrap"
        :class="getDifficultyClass(diff, selectedDifficulty === diff)"
      >
        {{ diff === 'All' ? 'Tất cả' : (diff === 'Easy' || diff === 'Beginner' || diff === 'Cơ bản' || diff === 'Dễ' ? 'Cơ bản' : (diff === 'Medium' || diff === 'Intermediate' || diff === 'Trung cấp' || diff === 'Trung bình' ? 'Trung cấp' : 'Nâng cao')) }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { onClickOutside } from '@vueuse/core';
import { ChevronDown, Check } from 'lucide-vue-next';
import BaseIcon from '@/shared/components/BaseIcon.vue';

defineProps<{
  topics?: string[];
  selectedTopic?: string;
  categories?: string[];
  difficulties: string[];
  selectedCategory?: string;
  selectedDifficulty: string;
  searchQuery: string;
}>();

const emit = defineEmits<{
  (e: 'update:topic', value: string): void;
  (e: 'update:category', value: string): void;
  (e: 'update:difficulty', value: string): void;
  (e: 'update:searchQuery', value: string): void;
}>();

const isTopicOpen = ref(false);
const topicDropdownRef = ref<HTMLElement | null>(null);

onClickOutside(topicDropdownRef, () => {
  isTopicOpen.value = false;
});

function handleSelectTopic(topic: string) {
  emit('update:topic', topic);
  isTopicOpen.value = false;
}

function getDifficultyClass(diff: string, isSelected: boolean): string {
  if (!isSelected) {
    return 'bg-vdsa-bg-secondary text-slate-400 font-medium hover:text-white border border-vdsa-border-subtle hover:border-slate-700 hover:bg-vdsa-surface';
  }
  const d = diff.toLowerCase();
  if (d === 'all') {
    return 'bg-purple-600 text-white font-black border-purple-400 shadow-md shadow-purple-600/30 ring-1 ring-purple-400/50 scale-105';
  }
  if (d === 'easy' || d === 'beginner' || d === 'cơ bản' || d === 'dễ') {
    return 'bg-emerald-600 text-white font-black border-emerald-400 shadow-md shadow-emerald-600/30 ring-1 ring-emerald-400/50 scale-105';
  }
  if (d === 'medium' || d === 'intermediate' || d === 'trung cấp' || d === 'trung bình') {
    return 'bg-amber-500 text-slate-950 font-black border-amber-300 shadow-md shadow-amber-500/30 ring-1 ring-amber-300/50 scale-105';
  }
  return 'bg-rose-600 text-white font-black border-rose-400 shadow-md shadow-rose-600/30 ring-1 ring-rose-400/50 scale-105';
}
</script>

<style scoped>
.no-scrollbar {
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
}
.no-scrollbar::-webkit-scrollbar {
  display: none;
}
</style>
