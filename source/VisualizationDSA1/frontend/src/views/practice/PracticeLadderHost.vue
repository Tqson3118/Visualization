<template>
  <div class="practice-ladder-host h-full w-full flex flex-col bg-bg-secondary overflow-hidden">
    <header class="px-4 md:px-6 py-3 border-b border-border-subtle bg-bg-secondary backdrop-blur-md flex items-center justify-between shrink-0 shadow-lg z-20 gap-3">
      <div class="flex items-center gap-2 md:gap-3 min-w-0">
        <router-link :to="courseId ? `/courses/${courseId}` : '/courses'" class="text-xs font-semibold text-text-muted hover:text-text-primary transition-colors flex items-center gap-1 shrink-0">
          <span>←</span> <span class="hidden sm:inline">Quay lại</span>
        </router-link>
        <span class="text-text-disabled hidden sm:inline">|</span>
        <h2 class="text-sm font-extrabold text-text-primary line-clamp-1">Thực hành bài học</h2>
      </div>
      <div class="flex items-center gap-2 font-mono text-xs shrink-0">
        <span class="px-2.5 py-1 rounded-lg bg-accent-green/15 text-accent-green border border-accent-green/25 font-bold">Practice Ladder</span>
      </div>
    </header>

    <main v-if="nodeId && sessionId" class="flex-1 min-h-0 overflow-y-auto bg-bg-primary p-4 md:p-6">
      <div class="max-w-6xl mx-auto h-full">
        <PracticeLadder :node-id="nodeId" :session-id="sessionId" @completed="onCompleted" />
      </div>
    </main>

    <div v-else class="flex-1 flex flex-col items-center justify-center gap-4 py-20">
      <div class="w-10 h-10 border-4 border-accent border-t-transparent rounded-full animate-spin"></div>
      <p class="text-text-muted text-sm">Đang mở bài học...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import PracticeLadder from '@/components/practice/PracticeLadder.vue';
import { sessionApi, OutOfHeartsError } from '@/features/gamification-engine/service/sessionApi';
import { useSessionStore } from '@/features/gamification-engine/store/useSessionStore';

const route = useRoute();
const router = useRouter();
const sessionStore = useSessionStore();

const nodeId = ref(route.params.nodeId as string);
const sessionId = ref(route.query.sessionId as string);
const courseId = route.query.courseId as string;

onMounted(async () => {
  if (!sessionId.value) {
    try {
      const session = await sessionApi.enterNode(nodeId.value);
      sessionId.value = session.sessionId;
    } catch (err) {
      if (err instanceof OutOfHeartsError) {
        sessionStore.showOutOfHearts(err.recoveryInfo);
        return;
      }
      router.push(courseId ? `/courses/${courseId}` : '/courses');
    }
  }
});

function onCompleted(): void {
  router.push(courseId ? `/courses/${courseId}` : '/courses');
}
</script>
