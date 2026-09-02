<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { RouterLink } from 'vue-router';
import { MessageSquare, CheckCircle2, Clock, BookOpen, UserRound, ArrowRight } from 'lucide-vue-next';
import { courseApi, type CourseFeedbackDto } from '@/services/courseApi';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const feedbackList = ref<CourseFeedbackDto[]>([]);
const loading = ref(true);
const filter = ref<'all' | 'resolved' | 'pending'>('all');

onMounted(async () => {
  try {
    const list = await courseApi.getMyCourseFeedback();
    feedbackList.value = list || [];
  } catch (err) {
    console.error('Failed to load my feedback:', err);
    feedbackList.value = [];
  } finally {
    loading.value = false;
  }
});

const filteredList = computed(() => {
  if (filter.value === 'resolved') {
    return feedbackList.value.filter((f) => f.status === 'Resolved' || Boolean(f.replyText));
  }
  if (filter.value === 'pending') {
    return feedbackList.value.filter((f) => f.status !== 'Resolved' && !f.replyText);
  }
  return feedbackList.value;
});

function typeLabel(type: string): string {
  switch (type) {
    case 'Bug':
      return 'Báo lỗi';
    case 'Request':
      return 'Đề xuất';
    default:
      return 'Góp ý';
  }
}

function typeBadgeVariant(type: string): 'default' | 'danger' | 'secondary' | 'primary' | 'success' | 'warning' | 'muted' {
  switch (type) {
    case 'Bug':
      return 'danger';
    case 'Request':
      return 'warning';
    default:
      return 'primary';
  }
}

function statusLabel(status: string, hasReply: boolean): string {
  if (status === 'Resolved' || hasReply) return 'Đã phản hồi';
  if (status === 'Read') return 'Đã đọc';
  return 'Chờ phản hồi';
}

function statusBadgeVariant(status: string, hasReply: boolean): 'default' | 'danger' | 'secondary' | 'primary' | 'success' | 'warning' | 'muted' {
  if (status === 'Resolved' || hasReply) return 'success';
  if (status === 'Read') return 'primary';
  return 'muted';
}

import { formatDateTime } from '@/utils/format';

function formatDate(dateStr: string): string {
  if (!dateStr) return '';
  return formatDateTime(dateStr);
}
</script>

<template>
  <div class="profile__feedback-panel">
    <!-- Filter header -->
    <div class="profile__feedback-header">
      <div>
        <h2 class="profile__feedback-title">Ý kiến & Phản hồi của tôi</h2>
        <p class="profile__feedback-sub">Theo dõi toàn bộ câu trả lời từ Giảng viên cho các góp ý của bạn</p>
      </div>

      <div class="profile__feedback-filters">
        <button
          type="button"
          class="profile__filter-btn"
          :class="{ 'profile__filter-btn--active': filter === 'all' }"
          @click="filter = 'all'"
        >
          Tất cả ({{ feedbackList.length }})
        </button>
        <button
          type="button"
          class="profile__filter-btn"
          :class="{ 'profile__filter-btn--active': filter === 'resolved' }"
          @click="filter = 'resolved'"
        >
          Đã trả lời ({{ feedbackList.filter(f => f.status === 'Resolved' || f.replyText).length }})
        </button>
        <button
          type="button"
          class="profile__filter-btn"
          :class="{ 'profile__filter-btn--active': filter === 'pending' }"
          @click="filter = 'pending'"
        >
          Chờ phản hồi ({{ feedbackList.filter(f => f.status !== 'Resolved' && !f.replyText).length }})
        </button>
      </div>
    </div>

    <!-- Loading Skeleton -->
    <div v-if="loading" class="profile__feedback-loading">
      <Skeleton v-for="i in 3" :key="i" height="130px" class="rounded-xl" />
    </div>

    <!-- Feedback list -->
    <div v-else-if="filteredList.length > 0" class="profile__feedback-list">
      <article
        v-for="item in filteredList"
        :key="item.id"
        class="profile__feedback-card"
        :class="{ 'profile__feedback-card--resolved': item.replyText || item.status === 'Resolved' }"
      >
        <!-- Card head: Course title + Badges + Date -->
        <header class="profile__feedback-card-head">
          <div class="profile__feedback-course-info">
            <RouterLink
              v-if="item.courseId"
              :to="{ name: 'path-detail', params: { id: item.courseId } }"
              class="profile__feedback-course-link group"
            >
              <BookOpen :size="15" class="text-vdsa-accent shrink-0" />
              <span class="font-bold text-white group-hover:text-vdsa-accent-light transition-colors truncate">
                {{ item.courseTitle || `Khóa học #${item.courseId}` }}
              </span>
              <ArrowRight :size="13" class="opacity-0 group-hover:opacity-100 transition-opacity text-vdsa-accent shrink-0" />
            </RouterLink>
            <span v-else class="font-bold text-white">Góp ý chung</span>
          </div>

          <div class="profile__feedback-meta">
            <Badge :variant="typeBadgeVariant(item.type)">{{ typeLabel(item.type) }}</Badge>
            <Badge :variant="statusBadgeVariant(item.status, Boolean(item.replyText))">
              {{ statusLabel(item.status, Boolean(item.replyText)) }}
            </Badge>
            <span class="profile__feedback-date">{{ formatDate(item.createdAt) }}</span>
          </div>
        </header>

        <!-- User content -->
        <p class="profile__feedback-content">{{ item.content }}</p>

        <!-- Teacher reply -->
        <div v-if="item.replyText" class="profile__teacher-reply">
          <div class="profile__teacher-reply-head">
            <span class="profile__teacher-icon">
              <UserRound :size="13" />
            </span>
            <span class="profile__teacher-name">{{ item.repliedByName || 'Giảng viên' }}</span>
            <span class="profile__teacher-tag">Đã phản hồi</span>
            <span v-if="item.repliedAt" class="profile__teacher-date">{{ formatDate(item.repliedAt) }}</span>
          </div>
          <p class="profile__teacher-text">{{ item.replyText }}</p>
        </div>
      </article>
    </div>

    <!-- Empty state -->
    <EmptyState
      v-else
      icon="message-square"
      title="Chưa có ý kiến nào"
      description="Khi bạn gửi góp ý hoặc báo lỗi tại các lộ trình học, câu trả lời từ Giảng viên sẽ hiển thị tại đây!"
    />
  </div>
</template>

<style scoped>
.profile__feedback-panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg, 20px);
}

.profile__feedback-header {
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 16px);
  padding-bottom: var(--space-md, 16px);
  border-bottom: 1px solid var(--color-border, #30363d);
}

@media (min-width: 640px) {
  .profile__feedback-header {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}

.profile__feedback-title {
  font-size: var(--text-lg, 18px);
  font-weight: 700;
  color: var(--color-foreground, #ffffff);
  margin: 0;
}

.profile__feedback-sub {
  font-size: var(--text-xs, 12px);
  color: var(--color-text-secondary, #8b949e);
  margin-top: 4px;
}

.profile__feedback-filters {
  display: flex;
  gap: 6px;
  background: var(--color-card, #161b22);
  padding: 4px;
  border-radius: var(--radius-lg, 10px);
  border: 1px solid var(--color-border, #30363d);
}

.profile__filter-btn {
  padding: 6px 12px;
  font-size: var(--text-xs, 12px);
  font-weight: 600;
  color: var(--color-text-secondary, #8b949e);
  background: transparent;
  border: none;
  border-radius: var(--radius-md, 6px);
  cursor: pointer;
  transition: all 150ms ease;
}

.profile__filter-btn:hover {
  color: var(--color-foreground, #ffffff);
}

.profile__filter-btn--active {
  background: var(--color-surface-hover, rgba(255, 255, 255, 0.1));
  color: var(--color-foreground, #ffffff);
}

.profile__feedback-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 16px);
}

.profile__feedback-card {
  background: var(--color-card, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-xl, 14px);
  padding: var(--space-lg, 20px);
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 14px);
  transition: border-color 150ms ease;
}

.profile__feedback-card--resolved {
  border-color: rgba(168, 85, 247, 0.3);
}

.profile__feedback-card:hover {
  border-color: var(--color-border-strong, #484f58);
}

.profile__feedback-card-head {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm, 10px);
}

@media (min-width: 640px) {
  .profile__feedback-card-head {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}

.profile__feedback-course-link {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-sm, 14px);
}

.profile__feedback-meta {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

.profile__feedback-date {
  font-size: var(--text-xs, 11px);
  color: var(--color-text-tertiary, #6e7681);
}

.profile__feedback-content {
  font-size: var(--text-sm, 13px);
  color: var(--color-text-secondary, #c9d1d9);
  line-height: 1.6;
  margin: 0;
}

.profile__teacher-reply {
  background: var(--color-surface, #0d1117);
  border: 1px solid var(--color-border-subtle, rgba(168, 85, 247, 0.2));
  border-left: 3px solid var(--color-primary, #a855f7);
  border-radius: var(--radius-lg, 10px);
  padding: var(--space-md, 14px);
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.profile__teacher-reply-head {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

.profile__teacher-icon {
  width: 20px;
  height: 20px;
  border-radius: 9999px;
  background: rgba(168, 85, 247, 0.15);
  color: #a855f7;
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile__teacher-name {
  font-size: var(--text-xs, 12px);
  font-weight: 700;
  color: #ffffff;
}

.profile__teacher-tag {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #a855f7;
  background: rgba(168, 85, 247, 0.12);
  padding: 1px 6px;
  border-radius: 4px;
}

.profile__teacher-date {
  font-size: 11px;
  color: var(--color-text-tertiary, #6e7681);
}

.profile__teacher-text {
  font-size: var(--text-sm, 13px);
  color: var(--color-text-secondary, #c9d1d9);
  line-height: 1.6;
  margin: 0;
}
</style>
