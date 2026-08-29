<script setup lang="ts">
import { Check, Circle, Download, RefreshCw } from 'lucide-vue-next';
import { useProgressStore } from '@/stores/progress';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const props = defineProps<{
  loadError?: string;
}>();

const emit = defineEmits<{
  retry: [];
}>();

const progressStore = useProgressStore();

async function reloadProgress(): Promise<void> {
  await progressStore.fetchOverview();
}

function csvExport(): void {
  const ov = progressStore.overview;
  if (!ov) return;
  const rows: string[] = ['Topic,Lesson,Completed,BestScore'];
  for (const t of ov.topics) {
    for (const l of t.lessons) {
      rows.push(`"${t.name}","${l.title}",${l.completed},${l.bestScore ?? ''}`);
    }
  }
  const blob = new Blob([rows.join('\n')], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `dsa_progress_${new Date().toISOString().slice(0, 10)}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <div class="profile__progress-panel">
    <div class="profile__progress-actions">
      <Button variant="secondary" size="sm" @click="reloadProgress">
        <RefreshCw :size="14" /> Làm mới
      </Button>
      <Button variant="ghost" size="sm" @click="csvExport">
        <Download :size="14" /> Xuất CSV
      </Button>
    </div>

    <EmptyState
      v-if="loadError"
      icon="alert-circle"
      title="Không tải được tiến độ"
      :description="loadError"
      :action-label="messages.common.retry"
      @action="emit('retry')"
    />
    <EmptyState
      v-else-if="!progressStore.overview || progressStore.overview.topics.length === 0"
      icon="target"
      title="Chưa có dữ liệu tiến độ"
      description="Học vài bài học đầu tiên để thấy tiến độ ở đây."
    />
    <div v-else class="profile__topics">
      <article v-for="topic in progressStore.overview.topics" :key="topic.id" class="card profile__topic">
        <div class="profile__topic-head">
          <h3 class="profile__topic-name">{{ topic.name }}</h3>
          <span class="profile__topic-pct">{{ topic.progressPct }}%</span>
        </div>
        <ProgressBar :value="topic.progressPct" :variant="topic.progressPct >= 100 ? 'success' : 'default'" />
        <ul class="profile__topic-lessons">
          <li v-for="lesson in topic.lessons" :key="lesson.id" class="profile__topic-lesson">
            <span :class="lesson.completed ? 'profile__done' : 'profile__todo'">
              <Check v-if="lesson.completed" :size="14" aria-hidden="true" />
              <Circle v-else :size="10" aria-hidden="true" />
              {{ lesson.title }}
            </span>
            <Badge v-if="lesson.bestScore !== null" variant="primary">{{ lesson.bestScore }} điểm</Badge>
          </li>
        </ul>
      </article>
    </div>
  </div>
</template>

<style scoped>
.profile__progress-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 16px);
}

.profile__progress-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm, 8px);
}

.profile__topics {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: var(--space-md, 16px);
}

.profile__topic {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  padding: var(--space-md, 16px);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm, 8px);
}

.profile__topic-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.profile__topic-name {
  font-size: var(--text-sm, 13px);
  font-weight: 700;
  color: #ffffff;
  margin: 0;
}

.profile__topic-pct {
  font-family: var(--font-mono, monospace);
  font-size: var(--text-xs, 12px);
  font-weight: 700;
  color: var(--color-accent, #6366f1);
}

.profile__topic-lessons {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 200px;
  overflow-y: auto;
}

.profile__topic-lesson {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  font-size: var(--text-xs, 12px);
  padding: 4px 6px;
  border-radius: 4px;
}

.profile__topic-lesson:hover {
  background: rgba(255, 255, 255, 0.03);
}

.profile__done {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #10b981;
  font-weight: 500;
}

.profile__todo {
  display: flex;
  align-items: center;
  gap: 6px;
  color: rgba(255, 255, 255, 0.5);
}
</style>
