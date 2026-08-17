<template>
  <div class="flex flex-col sm:flex-row items-start sm:items-center gap-4 flex-wrap bg-vdsa-bg-secondary/40 p-4 rounded-xl border border-vdsa-border-subtle">

    <div class="relative flex-1 min-w-[200px] w-full sm:w-auto">
      <input
        :value="searchQuery"
        @input="$emit('update:searchQuery', ($event.target as HTMLInputElement).value)"
        type="text"
        placeholder="Tìm kiếm khóa học..."
        class="w-full px-4 py-2 pl-10 bg-vdsa-bg-secondary border border-vdsa-border-subtle rounded-xl text-sm text-white placeholder-text-muted focus:outline-none focus:border-vdsa-accent/50 transition-colors"
      />
      <BaseIcon name="search" class="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-vdsa-muted" />
    </div>


    <div class="flex items-center gap-1.5 flex-wrap">
      <span class="text-[10px] text-vdsa-muted font-bold uppercase tracking-wider mr-2">Danh mục</span>
      <button
        v-for="cat in categories"
        :key="cat"
        @click="$emit('update:category', cat)"
        class="px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all cursor-pointer"
        :class="selectedCategory === cat
          ? 'bg-vdsa-accent text-white shadow-lg shadow-vdsa-accent/30 border border-vdsa-accent/50'
          : 'bg-vdsa-bg-secondary text-vdsa-muted hover:text-white border border-vdsa-border-subtle hover:border-vdsa-border hover:bg-vdsa-surface'"
      >
        {{ cat === 'All' ? 'Tất cả' : cat }}
      </button>
    </div>


    <div class="flex items-center gap-1.5 flex-wrap">
      <span class="text-[10px] text-vdsa-muted font-bold uppercase tracking-wider mr-2">Độ khó</span>
      <button
        v-for="diff in difficulties"
        :key="diff"
        @click="$emit('update:difficulty', diff)"
        class="px-3 py-1.5 rounded-lg text-[11px] font-bold transition-all cursor-pointer"
        :class="selectedDifficulty === diff
          ? 'bg-vdsa-accent text-white shadow-lg shadow-vdsa-accent/30 border border-vdsa-accent/50'
          : 'bg-vdsa-bg-secondary text-vdsa-muted hover:text-white border border-vdsa-border-subtle hover:border-vdsa-border hover:bg-vdsa-surface'"
      >
        {{ diff === 'All' ? 'Tất cả' : (diff === 'Easy' || diff === 'Beginner' ? 'Dễ' : diff === 'Medium' || diff === 'Intermediate' ? 'Trung bình' : 'Khó') }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import BaseIcon from '@/shared/components/BaseIcon.vue';

defineProps<{
  categories: string[];
  difficulties: string[];
  selectedCategory: string;
  selectedDifficulty: string;
  searchQuery: string;
}>();

defineEmits<{
  (e: 'update:category', value: string): void;
  (e: 'update:difficulty', value: string): void;
  (e: 'update:searchQuery', value: string): void;
}>();
</script>
