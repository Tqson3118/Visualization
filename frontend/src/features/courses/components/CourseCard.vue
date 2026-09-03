<template>
  <div
    class="course-card group relative bg-vdsa-bg-secondary border border-vdsa-border-subtle rounded-2xl overflow-hidden transition-all duration-300 hover:translate-y-[-4px] hover:shadow-2xl hover:border-vdsa-accent/30 flex flex-col"
  >

    <div class="relative h-36 overflow-hidden bg-vdsa-surface shrink-0">
      <img v-if="course.coverImageUrl || course.coverImage" :src="course.coverImageUrl || course.coverImage" alt="Course Cover" class="w-full h-full object-cover" />
      <CourseCover v-else :course="course" class="w-full h-full" />
      <div class="absolute inset-0 bg-gradient-to-t from-vdsa-bg-secondary via-transparent to-transparent" />
      <div class="absolute top-3 right-3 flex gap-1.5 flex-wrap justify-end">
        <span
          v-if="course.isPremium"
          class="px-2 py-0.5 rounded-full text-[11px] font-bold uppercase tracking-wider bg-vdsa-yellow text-black shadow-sm whitespace-nowrap shrink-0"
        >
          Premium
        </span>
        <span
          class="px-2 py-0.5 rounded-full text-[11px] font-bold uppercase tracking-wider whitespace-nowrap shrink-0"
          :class="difficultyBadgeClass"
        >
          {{ difficultyLabel }}
        </span>
      </div>
    </div>


    <div class="p-4 flex flex-col flex-1">
      <div class="flex items-start justify-between mb-2">
        <h3 class="text-base font-bold text-white line-clamp-1 mr-2">{{ course.title }}</h3>
        <span class="text-[10px] font-bold text-vdsa-yellow flex items-center gap-0.5 whitespace-nowrap bg-vdsa-yellow/30 px-1.5 py-0.5 rounded shrink-0">
          <BaseIcon name="zap" class="w-3 h-3" />
          {{ course.xpReward }} XP
        </span>
      </div>

      <p class="text-xs text-vdsa-muted line-clamp-2 mb-3 flex-1">{{ course.description }}</p>

      <!-- Thanh tiến độ nổi bật khi đã ghi danh -->
      <div v-if="authStore.isAuthenticated && isEnrolled" class="mb-3 space-y-1">
        <div class="flex items-center justify-between text-[11px] font-semibold">
          <span class="text-vdsa-muted">Tiến độ hoàn thành</span>
          <span :class="progressPercent === 100 ? 'text-vdsa-green font-bold' : 'text-purple-300 font-bold'">{{ progressPercent }}%</span>
        </div>
        <div class="w-full h-1.5 bg-vdsa-surface rounded-full overflow-hidden border border-white/5">
          <div
            class="h-full rounded-full transition-all duration-500"
            :style="{ width: progressPercent + '%' }"
            :class="progressPercent === 100 ? 'bg-vdsa-green' : 'bg-gradient-to-r from-purple-500 to-indigo-500'"
          />
        </div>
      </div>

      <div class="flex items-center justify-between pt-3 border-t border-vdsa-border-subtle gap-2">
        <div class="flex items-center gap-1.5 text-xs text-vdsa-muted font-medium whitespace-nowrap shrink-0">
          <svg class="w-3.5 h-3.5 shrink-0 text-slate-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path>
          </svg>
          <span class="whitespace-nowrap">{{ course.totalLessons }} bài</span>
        </div>

        <span
          class="inline-flex items-center justify-center px-3.5 py-1.5 rounded-lg text-xs font-bold transition-all shadow-md select-none whitespace-nowrap shrink-0"
          :class="(!isEnrolled) ? 'bg-vdsa-surface border border-vdsa-border group-hover:bg-vdsa-hover text-white shadow-none' : (progressPercent === 100 ? 'bg-vdsa-green group-hover:brightness-110 text-white shadow-vdsa-accent/30' : 'bg-vdsa-accent group-hover:brightness-110 text-white shadow-vdsa-accent/30')"
        >
          {{ !isEnrolled ? 'Xem chi tiết' : (progressPercent === 100 ? 'Ôn tập' : 'Tiếp tục học') }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useAuthStore } from '@/stores/auth';
import type { Course } from '../types/course.types';
import { useCourseStore } from '../store/useCourseStore';
import CourseCover from './CourseCover.vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import { onMounted, ref } from 'vue';

const props = defineProps<{
  course: Course;
}>();

const authStore = useAuthStore();
const courseStore = useCourseStore();

const progressPercent = computed(() => {
  const serverVal = props.course.progressPercent ?? 0;
  const localVal = courseStore.getCourseProgress(props.course.id).progressPercent ?? 0;
  return Math.max(serverVal, localVal);
});

const isEnrolled = computed(() => {
  return courseStore.isEnrolled(props.course.id) || progressPercent.value > 0 || (props.course.completedLessons ?? 0) > 0;
});

const difficultyLabel = computed(() => {
  const diff = props.course.difficulty || '';
  const lower = diff.trim().toLowerCase();
  if (lower === 'easy' || lower === 'beginner' || lower === 'cơ bản' || lower === 'dễ') return 'Cơ bản';
  if (lower === 'medium' || lower === 'intermediate' || lower === 'trung cấp' || lower === 'trung bình') return 'Trung cấp';
  if (lower === 'hard' || lower === 'advanced' || lower === 'nâng cao' || lower === 'khó') return 'Nâng cao';
  return diff || 'Cơ bản';
});

const difficultyBadgeClass = computed(() => {
  switch (difficultyLabel.value) {
    case 'Cơ bản':
      return 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30';
    case 'Trung cấp':
      return 'bg-amber-500/20 text-amber-400 border border-amber-500/30';
    case 'Nâng cao':
      return 'bg-rose-500/20 text-rose-400 border border-rose-500/30';
    default:
      return 'bg-slate-500/20 text-slate-300 border border-slate-500/30';
  }
});
</script>
