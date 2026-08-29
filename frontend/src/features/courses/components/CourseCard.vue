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
          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-wider bg-vdsa-yellow text-black shadow-sm whitespace-nowrap shrink-0"
        >
          Premium
        </span>
        <span
          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-wider whitespace-nowrap shrink-0"
          :class="difficultyBadgeClass"
        >
          {{ difficultyLabel }}
        </span>
        <span
          v-if="course.category && course.category !== difficultyLabel"
          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-wider bg-vdsa-surface/80 text-vdsa-secondary whitespace-nowrap shrink-0"
        >
          {{ course.category }}
        </span>
      </div>
    </div>


    <div class="p-4 flex flex-col flex-1">
      <div class="flex items-start justify-between mb-2">
        <h3 class="text-base font-bold text-white line-clamp-1 mr-2">{{ course.title }}</h3>
        <span class="text-[10px] font-bold text-vdsa-yellow flex items-center gap-0.5 whitespace-nowrap bg-vdsa-yellow/30 px-1.5 py-0.5 rounded">
          <BaseIcon name="zap" class="w-3 h-3" />
          {{ course.xpReward }} XP
        </span>
      </div>

      <p class="text-xs text-vdsa-muted line-clamp-2 mb-4 flex-1">{{ course.description }}</p>

      <div class="flex items-center justify-between pt-3 border-t border-vdsa-border-subtle">
        <div class="flex items-center gap-2 text-[10px] text-vdsa-muted font-medium">
          <span class="flex items-center gap-1">
            <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path></svg>
            {{ course.totalLessons }} bài
          </span>
        </div>

        <div class="flex items-center gap-3">

          <div v-if="authStore.isAuthenticated && isEnrolled" class="flex items-center gap-2">
            <div class="w-16 h-1.5 bg-vdsa-surface rounded-full overflow-hidden">
              <div
                class="h-full rounded-full transition-all duration-500"
                :style="{ width: progressPercent + '%' }"
                :class="progressPercent === 100 ? 'bg-vdsa-green' : 'bg-vdsa-accent'"
              />
            </div>
            <span class="text-[9px] font-bold tabular-nums" :class="progressPercent === 100 ? 'text-vdsa-green' : 'text-vdsa-muted'">
              {{ progressPercent }}%
            </span>
          </div>

          <router-link
            :to="{ name: 'path-detail', params: { id: course.id } }"
            class="px-4 py-1.5 rounded-lg text-[10px] font-bold transition-all shadow-lg cursor-pointer whitespace-nowrap shrink-0"
            :class="(!isEnrolled) ? 'bg-vdsa-surface border border-vdsa-border hover:bg-vdsa-hover text-white shadow-none' : (progressPercent === 100 ? 'bg-vdsa-green hover:bg-vdsa-green text-white shadow-vdsa-accent/30' : 'bg-vdsa-accent hover:bg-vdsa-accent text-white shadow-vdsa-accent/30')"
          >
            {{ !isEnrolled ? 'Xem chi tiết' : (progressPercent === 100 ? 'Ôn tập' : 'Tiếp tục học') }}
          </router-link>
        </div>
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
  const progress = courseStore.getCourseProgress(props.course.id);
  return progress.progressPercent;
});

const isEnrolled = computed(() => {
  return courseStore.isEnrolled(props.course.id);
});

const difficultyLabel = computed(() => {
  const map: Record<string, string> = {
    Easy: 'Cơ bản',
    Beginner: 'Cơ bản',
    Medium: 'Trung cấp',
    Intermediate: 'Trung cấp',
    Hard: 'Nâng cao',
    Advanced: 'Nâng cao',
  };
  return map[props.course.difficulty] ?? props.course.difficulty;
});

const difficultyBadgeClass = computed(() => {
  switch (props.course.difficulty) {
    case 'Easy':
    case 'Beginner':
      return 'bg-vdsa-green/20 text-vdsa-green border border-vdsa-green/30';
    case 'Medium':
    case 'Intermediate':
      return 'bg-vdsa-yellow/20 text-vdsa-yellow border border-vdsa-yellow/30';
    case 'Hard':
    case 'Advanced':
      return 'bg-vdsa-red/20 text-vdsa-red border border-vdsa-red/30';
    default: return 'bg-slate-500/20 text-vdsa-muted';
  }
});
</script>
