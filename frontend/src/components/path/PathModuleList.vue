<script setup lang="ts">
import { ref } from 'vue';
import {
  BookOpen,
  Check,
  ChevronDown,
  ChevronRight,
  Code2,
  HelpCircle,
  Lock,
  CalendarClock,
  Zap,
  Clock,
  Sparkles,
} from 'lucide-vue-next';
import { formatDate } from '@/utils/format';

export interface PathModuleLesson {
  id: number | string;
  nodeId?: number;
  lessonId?: number;
  pathItemId?: number;
  assignmentId?: number;
  title: string;
  sandboxType?: string;
  quizId?: number;
  status?: string;
  isCompleted?: boolean;
  locked?: boolean;
  isLocked?: boolean;
  xpReward?: number;
  dueAt?: string | null;
  allowLateSubmission?: boolean;
  bestScore?: number | null;
  orderIndex?: number;
}

export interface PathModuleGroup {
  id?: number | string;
  pathItemId?: number;
  title: string;
  dueAt?: string | null;
  allowLateSubmission?: boolean;
  lessons: PathModuleLesson[];
}

const props = withDefaults(
  defineProps<{
    modules: PathModuleGroup[];
    showDeadlines?: boolean;
    readonly?: boolean;
    isTeacher?: boolean;
    deadlineLevel?: 'folder' | 'lesson' | 'both' | 'none';
  }>(),
  {
    showDeadlines: false,
    readonly: false,
    isTeacher: false,
    deadlineLevel: 'lesson',
  },
);

const emit = defineEmits<{
  (e: 'selectLesson', lesson: PathModuleLesson): void;
  (e: 'editDeadline', item: PathModuleLesson | PathModuleGroup): void;
}>();

const expandedModules = ref<number[]>([0]);

function toggleModule(mIdx: number): void {
  const i = expandedModules.value.indexOf(mIdx);
  if (i === -1) {
    expandedModules.value.push(mIdx);
  } else {
    expandedModules.value.splice(i, 1);
  }
}

function toggleAllModules(): void {
  if (expandedModules.value.length === props.modules.length) {
    expandedModules.value = [];
  } else {
    expandedModules.value = props.modules.map((_, i) => i);
  }
}

function pad(n: number): string {
  return String(n).padStart(2, '0');
}

function cleanTitle(title: string): string {
  return (title || '').replace(/^[0-9]+[.\s-]*/, '').trim() || title;
}
</script>

<template>
  <div class="path-module-list space-y-4">
    <div v-if="modules.length > 1" class="flex items-center justify-between gap-4 mb-1">
      <span class="text-xs font-bold uppercase tracking-wider text-slate-400">
        {{ modules.reduce((sum, m) => sum + m.lessons.length, 0) }} bài học · {{ modules.length }} chủ đề
      </span>
      <button
        type="button"
        class="text-xs font-bold text-purple-400 hover:text-purple-300 transition-colors cursor-pointer"
        @click="toggleAllModules"
      >
        {{ expandedModules.length === modules.length ? 'Thu gọn tất cả' : 'Mở rộng tất cả' }}
      </button>
    </div>

    <div
      v-for="(module, mIdx) in modules"
      :key="mIdx"
      class="module-group rounded-2xl border border-[#262438] bg-[#12111a] overflow-hidden shadow-lg"
    >
      <!-- Module Header Row -->
      <div
        class="w-full flex items-center justify-between gap-4 p-4 text-left hover:bg-[#171624] transition-colors group cursor-pointer"
        @click="toggleModule(mIdx)"
      >
        <div class="flex items-center gap-4 min-w-0 flex-1">
          <span class="shrink-0 text-xl font-black text-slate-600 group-hover:text-purple-400 transition-colors w-9 text-center font-mono">
            {{ pad(mIdx + 1) }}
          </span>
          <div class="flex-1 min-w-0">
            <h3 class="text-sm md:text-base font-bold text-white leading-snug group-hover:text-purple-300 transition-colors">
              {{ module.title }}
            </h3>
            <p class="text-xs text-slate-400 mt-0.5">{{ module.lessons.length }} mục bài học & bài tập</p>
          </div>
        </div>

        <div class="flex items-center gap-2.5 shrink-0">
          <!-- Folder Deadline badge if showDeadlines -->
          <div v-if="showDeadlines" class="text-right">
            <span
              v-if="module.dueAt"
              class="inline-flex items-center gap-1 text-[11px] font-bold px-2.5 py-1 rounded-lg"
              :class="
                new Date(module.dueAt).getTime() < Date.now()
                  ? 'bg-rose-500/20 text-rose-300 border border-rose-500/30'
                  : 'bg-indigo-500/20 text-indigo-300 border border-indigo-500/30'
              "
            >
              <Clock class="w-3 h-3" />
              <span>Hạn: {{ formatDate(module.dueAt) }}</span>
            </span>
          </div>

          <!-- Teacher Folder Deadline Edit Action -->
          <button
            v-if="isTeacher && (deadlineLevel === 'folder' || deadlineLevel === 'both')"
            type="button"
            class="p-1.5 rounded-lg bg-[#1a1928] hover:bg-[#242238] border border-[#302e48] text-slate-300 hover:text-white text-xs font-bold flex items-center gap-1 transition-colors cursor-pointer"
            title="Thiết lập deadline cho module"
            :data-testid="'deadline-edit-module-' + (module.pathItemId ?? mIdx)"
            @click.stop="emit('editDeadline', module)"
          >
            <CalendarClock class="w-3.5 h-3.5 text-purple-400" />
            <span class="hidden sm:inline">Hạn chót module</span>
          </button>

          <!-- Expand / Collapse chevron -->
          <div
            class="w-8 h-8 rounded-full bg-[#181724] border border-[#2c2a40] flex items-center justify-center shrink-0 transition-transform duration-300"
            :class="expandedModules.includes(mIdx) ? 'rotate-180' : ''"
          >
            <ChevronDown class="w-4 h-4 text-slate-400 group-hover:text-white transition-colors" />
          </div>
        </div>
      </div>

      <!-- Module Lesson Items -->
      <div v-show="expandedModules.includes(mIdx)" class="border-t border-[#222033] divide-y divide-[#1e1d2c]">
        <div v-if="module.lessons.length === 0" class="p-4 text-center text-xs text-slate-500 italic">
          Chương này chưa có mục bài học nào.
        </div>
        <div
          v-for="(lesson, idx) in module.lessons"
          :key="lesson.id"
          class="w-full flex flex-wrap items-center justify-between gap-3 px-4 py-3.5 transition-colors"
          :class="[
            lesson.locked || lesson.isLocked
              ? 'bg-[#0f0e17] opacity-60'
              : (lesson.status === 'Completed' || lesson.isCompleted
                ? 'bg-emerald-950/20 hover:bg-emerald-950/30'
                : 'bg-[#14131f] hover:bg-[#191827]'),
          ]"
        >
          <!-- Left: Index/Status Icon + Title + Tags -->
          <div
            class="flex items-center gap-3.5 min-w-0 flex-1 cursor-pointer"
            @click="emit('selectLesson', lesson)"
          >
            <span
              class="shrink-0 w-8 h-8 rounded-lg border flex items-center justify-center text-xs font-bold"
              :class="[
                lesson.status === 'Completed' || lesson.isCompleted
                  ? 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30'
                  : (lesson.locked || lesson.isLocked
                    ? 'bg-[#181724] text-slate-500 border-[#2b293d]'
                    : 'bg-purple-500/10 text-purple-300 border-purple-500/20'),
              ]"
            >
              <Check v-if="lesson.status === 'Completed' || lesson.isCompleted" class="w-4 h-4 text-emerald-400" />
              <Lock v-else-if="lesson.locked || lesson.isLocked" class="w-3.5 h-3.5 text-slate-500" />
              <span v-else class="font-mono text-slate-300">{{ idx + 1 }}</span>
            </span>

            <div class="min-w-0 flex-1">
              <span class="block text-xs md:text-sm font-bold text-white leading-snug truncate">
                {{ cleanTitle(lesson.title) }}
              </span>
              <div class="flex items-center gap-2 mt-1 flex-wrap">
                <span
                  v-if="lesson.locked || lesson.isLocked"
                  class="font-extrabold uppercase text-[9px] tracking-wider text-slate-500 flex items-center gap-1"
                >
                  <Lock class="w-3 h-3" /> Bị khóa
                </span>
                <template v-else>
                  <span
                    v-if="lesson.sandboxType === 'codelab'"
                    class="font-extrabold uppercase text-[9px] tracking-wider bg-emerald-500/10 text-emerald-400 px-1.5 py-0.5 rounded border border-emerald-500/20"
                  >
                    Code Lab
                  </span>
                  <span
                    v-else-if="lesson.sandboxType === 'quiz'"
                    class="font-extrabold uppercase text-[9px] tracking-wider bg-amber-500/10 text-orange-400 px-1.5 py-0.5 rounded border border-amber-500/20"
                  >
                    Quiz
                  </span>
                  <span
                    v-else
                    class="font-extrabold uppercase text-[9px] tracking-wider bg-sky-500/10 text-sky-400 px-1.5 py-0.5 rounded border border-sky-500/20"
                  >
                    Lý thuyết
                  </span>

                  <span v-if="lesson.xpReward" class="flex items-center gap-0.5 text-[10px] font-bold text-amber-400">
                    <Zap class="w-3 h-3" /> +{{ lesson.xpReward }} XP
                  </span>

                  <span v-if="lesson.bestScore !== undefined && lesson.bestScore !== null" class="text-[10px] font-bold text-purple-300">
                    Điểm cao nhất: {{ lesson.bestScore }}
                  </span>
                </template>
              </div>
            </div>
          </div>

          <!-- Right: Deadline badge & Teacher deadline edit button -->
          <div class="flex items-center gap-2.5 shrink-0">
            <!-- Deadline Info -->
            <div v-if="showDeadlines" class="text-right">
              <span
                v-if="lesson.dueAt"
                class="inline-flex items-center gap-1 text-[11px] font-bold px-2 py-0.5 rounded-lg"
                :class="
                  new Date(lesson.dueAt).getTime() < Date.now()
                    ? 'bg-rose-500/20 text-rose-300 border border-rose-500/30'
                    : 'bg-indigo-500/20 text-indigo-300 border border-indigo-500/30'
                "
              >
                <Clock class="w-3 h-3" />
                <span>Hạn: {{ formatDate(lesson.dueAt) }}</span>
              </span>
              <span v-else class="text-[10px] font-medium text-slate-500">
                Không có hạn nộp
              </span>
            </div>

            <!-- Teacher Edit Deadline Action -->
            <button
              v-if="isTeacher && (deadlineLevel === 'lesson' || deadlineLevel === 'both')"
              type="button"
              class="p-1.5 rounded-lg bg-[#1a1928] hover:bg-[#242238] border border-[#302e48] text-slate-300 hover:text-white text-xs font-bold flex items-center gap-1 transition-colors cursor-pointer"
              title="Thiết lập deadline"
              :data-testid="'deadline-edit-' + (lesson.pathItemId ?? lesson.id)"
              @click.stop="emit('editDeadline', lesson)"
            >
              <CalendarClock class="w-3.5 h-3.5 text-purple-400" />
              <span class="hidden sm:inline">Hạn chót</span>
            </button>

            <!-- Navigation Chevron -->
            <button
              v-if="!lesson.locked && !lesson.isLocked"
              type="button"
              class="p-1 rounded-lg text-slate-400 hover:text-white cursor-pointer"
              @click="emit('selectLesson', lesson)"
            >
              <ChevronRight class="w-4 h-4" />
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
