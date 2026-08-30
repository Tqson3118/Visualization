<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { Settings, Users, GraduationCap, Globe } from 'lucide-vue-next';
import type { Topic, LessonStatusValue } from '@/api/lessons';
import * as classesApi from '@/api/classes';
import type { ClassDto } from '@/api/types';
import Input from '@/components/ui/Input.vue';

const props = defineProps<{
  topicId: number;
  description: string;
  sortOrder: number;
  status: LessonStatusValue;
  isClassOnly: boolean;
  selectedClassId?: number | null;
  topics: Topic[];
}>();

const emit = defineEmits<{
  (e: 'update:topicId', val: number): void;
  (e: 'update:description', val: string): void;
  (e: 'update:sortOrder', val: number): void;
  (e: 'update:status', val: LessonStatusValue): void;
  (e: 'update:isClassOnly', val: boolean): void;
  (e: 'update:selectedClassId', val: number | null): void;
}>();

const teacherClasses = ref<ClassDto[]>([]);
const loadingClasses = ref(false);

onMounted(async () => {
  loadingClasses.value = true;
  try {
    teacherClasses.value = await classesApi.fetchClasses();
  } catch {
    teacherClasses.value = [];
  } finally {
    loadingClasses.value = false;
  }
});
</script>

<template>
  <div class="settings-tab flex flex-col h-full p-6 overflow-y-auto max-w-4xl mx-auto space-y-6">
    <!-- Header -->
    <div class="border-b border-vdsa-border pb-4">
      <h2 class="text-lg font-black text-white flex items-center gap-2">
        <Settings class="text-vdsa-purple" :size="20" />
        Cấu hình Thuộc tính & Phạm vi Bài học
      </h2>
      <p class="text-xs text-vdsa-muted mt-1">
        Quản lý chủ đề liên kết, vị trí hiển thị trong lộ trình, và quyền truy cập của sinh viên.
      </p>
    </div>

    <div class="space-y-5">
      <!-- Chủ đề Topic -->
      <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-2">
        <label class="text-xs font-bold uppercase tracking-wider text-slate-300 block">
          Chủ đề Lộ trình (Topic / Chapter)
        </label>
        <select
          :value="topicId"
          @change="emit('update:topicId', Number(($event.target as HTMLSelectElement).value))"
          class="w-full px-3.5 py-2 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border text-xs text-white outline-none focus:border-vdsa-accent"
        >
          <option v-for="t in topics" :key="t.id" :value="t.id">
            {{ t.name }}
          </option>
        </select>
        <p class="text-[11px] text-vdsa-muted">
          Bài học sẽ được nhóm và sắp xếp theo chương mục thuộc chủ đề này trên giao diện học sinh.
        </p>
      </div>

      <!-- Mô tả tóm tắt -->
      <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-2">
        <label class="text-xs font-bold uppercase tracking-wider text-slate-300 block">
          Mô tả tóm tắt (Summary)
        </label>
        <textarea
          :value="description"
          @input="emit('update:description', ($event.target as HTMLTextAreaElement).value)"
          rows="3"
          class="w-full px-3.5 py-2 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border text-xs text-white outline-none focus:border-vdsa-accent resize-none"
          placeholder="Tóm tắt ngắn gọn 1-2 câu về mục tiêu và nội dung chính của bài giảng..."
        />
      </div>

      <!-- Thứ tự & Trạng thái -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-2">
          <label class="text-xs font-bold uppercase tracking-wider text-slate-300 block">
            Thứ tự hiển thị (Sort Order)
          </label>
          <input
            :value="sortOrder"
            @input="emit('update:sortOrder', Number(($event.target as HTMLInputElement).value))"
            type="number"
            min="1"
            class="w-full px-3.5 py-2 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border text-xs text-white outline-none focus:border-vdsa-accent"
          />
        </div>

        <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-2">
          <label class="text-xs font-bold uppercase tracking-wider text-slate-300 block">
            Trạng thái phát hành
          </label>
          <select
            :value="status"
            @change="emit('update:status', ($event.target as HTMLSelectElement).value as LessonStatusValue)"
            class="w-full px-3.5 py-2 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border text-xs text-white outline-none focus:border-vdsa-accent"
          >
            <option value="active">Kích hoạt (Active)</option>
            <option value="draft">Bản nháp (Draft)</option>
            <option value="hidden">Tạm ẩn (Hidden)</option>
          </select>
        </div>
      </div>

      <!-- Phạm vi phát hành & LE2: Chọn lớp khi nội bộ -->
      <div class="p-5 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-4">
        <div>
          <label class="text-xs font-bold uppercase tracking-wider text-slate-300 block">
            Phạm vi phát hành (Publication Scope)
          </label>
          <p class="text-[11px] text-vdsa-muted mt-0.5">
            Xác định ai có thể nhìn thấy và học bài giảng này.
          </p>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <!-- Option 1: Public -->
          <label
            class="p-4 rounded-xl border transition-all cursor-pointer flex items-start gap-3"
            :class="
              !isClassOnly
                ? 'border-vdsa-accent bg-vdsa-accent/10 shadow-md shadow-purple-950/20'
                : 'border-vdsa-border bg-vdsa-bg-secondary hover:border-slate-600'
            "
          >
            <input
              type="radio"
              :checked="!isClassOnly"
              @change="emit('update:isClassOnly', false)"
              class="mt-1 text-vdsa-purple focus:ring-0 cursor-pointer"
            />
            <div>
              <span class="text-xs font-bold text-white flex items-center gap-1.5">
                <Globe :size="14" class="text-sky-400" /> Công khai toàn hệ thống
              </span>
              <span class="text-[11px] text-slate-400 block mt-1 leading-relaxed">
                Bài học sẽ xuất hiện trên Lộ trình chung cho toàn bộ người dùng và sinh viên tự do học tập.
              </span>
            </div>
          </label>

          <!-- Option 2: Class Only -->
          <label
            class="p-4 rounded-xl border transition-all cursor-pointer flex items-start gap-3"
            :class="
              isClassOnly
                ? 'border-emerald-500 bg-emerald-500/10 shadow-md shadow-emerald-950/20'
                : 'border-vdsa-border bg-vdsa-bg-secondary hover:border-slate-600'
            "
          >
            <input
              type="radio"
              :checked="isClassOnly"
              @change="emit('update:isClassOnly', true)"
              class="mt-1 text-emerald-500 focus:ring-0 cursor-pointer"
            />
            <div>
              <span class="text-xs font-bold text-emerald-300 flex items-center gap-1.5">
                <GraduationCap :size="14" class="text-emerald-400" /> Nội bộ Lớp học của tôi
              </span>
              <span class="text-[11px] text-slate-400 block mt-1 leading-relaxed">
                Kích hoạt ngay lập tức cho các thành viên trong lớp học của bạn, không cần qua bước duyệt công khai.
              </span>
            </div>
          </label>
        </div>

        <!-- LE2: Class Dropdown Selector when isClassOnly is enabled -->
        <div v-if="isClassOnly" class="pt-3 border-t border-vdsa-border/60 space-y-2">
          <label class="text-xs font-bold text-emerald-300 flex items-center gap-1.5">
            <Users :size="13" /> Chọn Lớp học để gán trực tiếp bài giảng này:
          </label>
          <select
            :value="selectedClassId || ''"
            @change="emit('update:selectedClassId', ($event.target as HTMLSelectElement).value ? Number(($event.target as HTMLSelectElement).value) : null)"
            class="w-full px-3.5 py-2 rounded-xl bg-vdsa-bg-secondary border border-emerald-500/40 text-xs text-white outline-none focus:border-emerald-400"
          >
            <option value="">-- Chọn lớp học (tùy chọn) --</option>
            <option v-for="c in teacherClasses" :key="c.id" :value="c.id">
              {{ c.name }} (Mã mời: {{ c.inviteCode }})
            </option>
          </select>
          <p v-if="teacherClasses.length === 0 && !loadingClasses" class="text-[11px] text-amber-300">
            Bạn chưa tạo lớp học nào. Bạn có thể vào mục "Lớp học" để tạo lớp trước khi gán.
          </p>
        </div>
      </div>
    </div>
  </div>
</template>
