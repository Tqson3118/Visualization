<template>
  <div class="courses-list-view w-full px-4 md:px-8 py-8 animate-fade-in bg-vdsa-bg min-h-[calc(100vh-var(--app-header-h,68px))]">

    <header class="mb-10 text-left flex flex-col md:flex-row md:items-end justify-between gap-6">
      <div class="flex-1">
        <h1 class="text-4xl font-extrabold text-white m-0 tracking-tight">
          Lộ trình học DSA
        </h1>
        <p class="text-vdsa-secondary mt-2 text-lg">
          Ba lộ trình trực quan theo trình độ — Cơ bản, Trung cấp và Nâng cao — giúp bạn làm chủ cấu trúc dữ liệu và giải thuật từng bước.
        </p>
      </div>
      <div v-if="authStore.user" class="stats-glass px-6 py-3 rounded-lg border border-vdsa-border flex items-center gap-4 bg-vdsa-surface backdrop-blur">
        <div class="text-left">
          <div class="text-xs text-vdsa-muted uppercase font-semibold tracking-wider">Cấp độ của bạn</div>
          <div class="text-xl font-bold text-white">Cấp {{ authStore.user.level ?? 1 }}</div>
        </div>
        <div class="w-[1px] h-8 bg-vdsa-border"></div>
        <div class="text-left">
          <div class="text-xs text-vdsa-muted uppercase font-semibold tracking-wider">Tích lũy</div>
          <div class="text-xl font-bold text-white">{{ authStore.user.xp ?? 0 }} XP</div>
        </div>
      </div>
    </header>

    <CourseFilter
      :topics="courseStore.topics"
      :selected-topic="courseStore.selectedTopic"
      :difficulties="courseStore.difficulties"
      :selected-difficulty="courseStore.selectedDifficulty"
      :search-query="courseStore.searchQuery"
      @update:topic="courseStore.setTopic"
      @update:difficulty="courseStore.setDifficulty"
      @update:searchQuery="courseStore.setSearchQuery"
    />

    <div v-if="courseStore.isLoading" class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 md:gap-6 mt-6" role="status" aria-label="Đang tải lộ trình">
      <div v-for="i in 4" :key="i" class="rounded-2xl overflow-hidden border border-vdsa-border-subtle bg-vdsa-surface">
        <div class="h-36 bg-vdsa-active animate-pulse"></div>
        <div class="p-4 space-y-3">
          <div class="h-4 bg-vdsa-active rounded w-3/4 animate-pulse"></div>
          <div class="h-3 bg-vdsa-active rounded w-1/2 animate-pulse"></div>
          <div class="h-1.5 bg-vdsa-active rounded w-full animate-pulse mt-4"></div>
          <div class="h-8 bg-vdsa-active rounded animate-pulse mt-2"></div>
        </div>
      </div>
    </div>

    <div v-else-if="courseStore.filteredCourses.length === 0" class="empty-state text-center py-20 bg-vdsa-surface rounded-lg border border-vdsa-border mt-6" role="status">
      <div class="text-5xl mb-4" aria-hidden="true"><BaseIcon name="search" class="w-14 h-14 text-vdsa-muted mx-auto" /></div>
      <h3 class="text-xl font-bold text-white">Không tìm thấy lộ trình phù hợp</h3>
      <p class="text-vdsa-secondary mt-2">Vui lòng thay đổi bộ lọc hoặc quay lại sau.</p>
    </div>

    <!-- Khi đang lọc theo một Chủ đề cụ thể, lọc cấp độ, hoặc có từ khóa tìm kiếm -->
    <template v-if="courseStore.selectedTopic !== 'All' || courseStore.selectedDifficulty !== 'All' || courseStore.searchQuery.trim()">
      <section class="mt-8">
        <div class="flex items-center justify-between mb-4">
          <h2 class="flex items-center gap-2.5 text-xl font-extrabold text-white tracking-tight">
            <span class="w-1.5 h-6 rounded-full bg-gradient-to-b from-vdsa-accent to-vdsa-accent/40" aria-hidden="true"></span>
            {{ courseStore.searchQuery.trim() ? `Kết quả tìm kiếm cho "${courseStore.searchQuery}"` : (courseStore.selectedTopic !== 'All' ? courseStore.selectedTopic : `Lộ trình trình độ ${courseStore.selectedDifficulty}`) }}
            <span class="text-xs font-semibold text-vdsa-muted">({{ courseStore.filteredCourses.length }} lộ trình)</span>
          </h2>
          <button
            v-if="courseStore.selectedTopic !== 'All' || courseStore.selectedDifficulty !== 'All'"
            @click="resetFilters"
            class="text-xs font-semibold text-purple-400 hover:text-purple-300 hover:underline transition-colors cursor-pointer"
          >
            ← Xem tất cả lộ trình
          </button>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 md:gap-6" role="list">
          <router-link
            v-for="course in courseStore.filteredCourses"
            :key="course.id"
            :to="{ name: 'path-detail', params: { id: course.id } }"
            class="course-card-link block"
            :aria-label="`Xem chi tiết lộ trình ${course.title}`"
            role="listitem"
          >
            <CourseCard :course="course" />
          </router-link>
        </div>
      </section>
    </template>

    <!-- Mặc định (Tất cả): Hiển thị mục Tất cả ở đầu tiên, sau đó tới từng Chủ đề kiến thức -->
    <template v-else>
      <div v-if="groupedCourses.length > 0" class="mt-6 space-y-10">
        <!-- 1. MỤC TẤT CẢ LỘ TRÌNH Ở ĐẦU TIÊN -->
        <section class="course-topic-section" aria-label="Tất cả lộ trình">
          <div class="flex items-center justify-between mb-4 pb-2 border-b border-white/5">
            <h2 class="flex items-center gap-2.5 text-lg md:text-xl font-bold text-white tracking-tight">
              <span class="w-2.5 h-2.5 rounded-full bg-gradient-to-r from-purple-500 to-indigo-500 shadow-sm shadow-purple-500/50" aria-hidden="true"></span>
              Tất cả
              <span class="text-xs font-semibold text-vdsa-muted">({{ courseStore.filteredCourses.length }} lộ trình)</span>
            </h2>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 md:gap-6" role="list" aria-label="Danh sách tất cả lộ trình">
            <router-link
              v-for="course in courseStore.filteredCourses"
              :key="'all-' + course.id"
              :to="{ name: 'path-detail', params: { id: course.id } }"
              class="course-card-link block"
              :aria-label="`Xem chi tiết lộ trình ${course.title}`"
              role="listitem"
            >
              <CourseCard :course="course" />
            </router-link>
          </div>
        </section>

        <!-- 2. CÁC PHÂN NHÓM THEO CHỦ ĐỀ Ở DƯỚI (GIỮ NGUYÊN) -->
        <section
          v-for="[topic, list] in groupedCourses"
          :key="topic"
          class="course-topic-section"
          :aria-label="'Lộ trình chủ đề ' + topic"
        >
          <div class="flex items-center justify-between mb-4">
            <h2 class="flex items-center gap-2.5 text-lg md:text-xl font-bold text-white tracking-tight">
              <span class="w-2 h-2 rounded-full bg-purple-500" aria-hidden="true"></span>
              {{ topic }}
              <span class="text-xs font-semibold text-vdsa-muted">({{ list.length }} lộ trình)</span>
            </h2>
            <button
              @click="filterByTopic(topic)"
              class="text-xs font-semibold text-purple-400 hover:text-purple-300 hover:underline transition-colors flex items-center gap-1 cursor-pointer"
            >
              Lọc theo chủ đề này &rarr;
            </button>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4 md:gap-6" role="list" :aria-label="'Danh sách lộ trình ' + topic">
            <router-link
              v-for="course in list"
              :key="course.id"
              :to="{ name: 'path-detail', params: { id: course.id } }"
              class="course-card-link block"
              :aria-label="`Xem chi tiết lộ trình ${course.title}`"
              role="listitem"
            >
              <CourseCard :course="course" />
            </router-link>
          </div>
        </section>
      </div>
    </template>

    <div v-if="courseStore.error" class="mt-6 text-center py-10 bg-vdsa-surface rounded-lg border border-vdsa-border">
      <div class="text-5xl mb-4"><BaseIcon name="warning" class="w-14 h-14 text-vdsa-red mx-auto" /></div>
      <h3 class="text-xl font-bold text-white">Đang dùng dữ liệu ngoại tuyến (Offline Mode)</h3>
      <p class="text-vdsa-secondary mt-2">{{ courseStore.error }}</p>
      <button
        @click="courseStore.loadCourses"
        class="mt-4 px-6 py-2.5 bg-vdsa-accent text-white font-semibold rounded-xl hover:bg-vdsa-accent-light transition-colors"
      >
        Thử kết nối lại máy chủ
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, computed } from 'vue';
import { useAuthStore } from '@/stores/auth';
import { useCourseStore } from '@/features/courses/store/useCourseStore';
import CourseCard from '@/features/courses/components/CourseCard.vue';
import CourseFilter from '@/features/courses/components/CourseFilter.vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';

const authStore = useAuthStore();
const courseStore = useCourseStore();

const groupedCourses = computed(() => {
  const groups = new Map<string, typeof courseStore.filteredCourses>();
  for (const c of courseStore.filteredCourses) {
    const rawKey = ((c as any).topicName as string | undefined) || c.category || 'Chủ đề khác';
    const key = rawKey.replace(/^Module\s*\d+\s*:\s*/i, '').trim();
    if (!groups.has(key)) groups.set(key, []);
    groups.get(key)!.push(c);
  }
  return Array.from(groups.entries());
});

function filterByTopic(topic: string): void {
  courseStore.setTopic(topic);
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

function resetFilters(): void {
  courseStore.setTopic('All');
  courseStore.setDifficulty('All');
  courseStore.setSearchQuery('');
}

onMounted(async () => {
  await courseStore.loadCourses();
});
</script>

<style scoped>
.courses-list-view {
  min-height: calc(100vh - var(--app-header-h, 68px));
  background-color: #0d0d11;
}

.course-card-link {
  text-decoration: none;
  color: inherit;
}

.animate-fade-in {
  animation: fadeIn 0.6s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(12px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
