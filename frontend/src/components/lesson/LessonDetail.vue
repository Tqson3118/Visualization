<script setup lang="ts">
// LessonDetail — nội dung bài học + ghi chú + đánh giá (Màn 04/31 tab Lý thuyết)
// Ghi chú: autosave 1s (debounce) lưu localStorage; đánh giá sao 1-5 chỉ khi ĐÃ mark-viewed.
import { computed, onBeforeUnmount, ref, watch } from 'vue';

import { useLessonStore } from '@/stores/lesson';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import BaseIcon from '@/components/ui/BaseIcon.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Card from '@/components/ui/Card.vue';
import Drawer from '@/components/ui/Drawer.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ProseContent from '@/components/ui/ProseContent.vue';
import { getCatalogMeta } from '@/engines/catalog';
import { toast } from '@/lib/toast';
import { submitLessonFeedback, reportLesson } from '@/api/lessons';

const props = withDefaults(
  defineProps<{
    lessonId: number | string;
    /** G-F2b: ẩn header bài học khi view cha (LessonView) đã có hero gradient riêng */
    hideHeader?: boolean;
  }>(),
  {
    hideHeader: false,
  },
);

const emit = defineEmits<{
  'open-simulation': [key: string];
  'open-exercise': [id: number];
}>();

const lessonStore = useLessonStore();
const ui = useUiStore();

const notesOpen = ref(false);
const noteText = ref('');
const noteSavedAt = ref('');
const ratingOpen = ref(false);
const rating = ref(0);
const ratingSubmitted = ref(false);
const reportOpen = ref(false);
const reportReason = ref('');
const reportSubmitted = ref(false);

let autosaveTimer: ReturnType<typeof setTimeout> | null = null;

const lesson = computed(() => lessonStore.currentLesson);
const loading = computed(() => lessonStore.loading);

watch(
  () => props.lessonId,
  (id) => {
    void lessonStore.fetchLesson(Number(id)).catch(() => {
      ui.showToast('Không thể tải bài học.', 'error');
    });
    loadNote(Number(id));
  },
  { immediate: true },
);

function loadNote(lessonId: number): void {
  try {
    noteText.value = localStorage.getItem(`dsa-note-${lessonId}`) ?? '';
  } catch {
    noteText.value = '';
  }
}

function onNoteInput(): void {
  if (autosaveTimer) clearTimeout(autosaveTimer);
  autosaveTimer = setTimeout(() => saveNote(), 1000);
}

function saveNote(): void {
  try {
    localStorage.setItem(`dsa-note-${props.lessonId}`, noteText.value);
    noteSavedAt.value = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
  } catch {
    // localStorage không khả dụng
  }
}

async function markViewed(): Promise<void> {
  try {
    await lessonStore.markViewed(Number(props.lessonId));
    ui.showToast('Đã đánh dấu bài học!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể đánh dấu.', 'error');
  }
}

async function submitRating(): Promise<void> {
  if (rating.value === 0) return;
  try {
    // POST /lessons/{id}/feedback (FR-7.4) — upsert, 1 lần/người
    await submitLessonFeedback(Number(props.lessonId), { rating: rating.value });
    ratingSubmitted.value = true;
    toast.success('Cảm ơn bạn đã đánh giá!');
  } catch (err) {
    toast.error(err instanceof Error ? err.message : 'Không thể gửi đánh giá.');
  }
}

// Báo cáo bài học vi phạm (v2.15) — POST /lessons/{id}/report → BugReports CONTENT_VIOLATION
async function submitReport(): Promise<void> {
  if (reportReason.value.trim().length < 5) return;
  try {
    await reportLesson(Number(props.lessonId), reportReason.value.trim());
    reportSubmitted.value = true;
    toast.success('Đã gửi báo cáo. Cảm ơn bạn đã giúp cộng đồng!');
  } catch (err) {
    toast.error(err instanceof Error ? err.message : 'Không thể gửi báo cáo.');
  }
}

onBeforeUnmount(() => {
  if (autosaveTimer) clearTimeout(autosaveTimer);
});
</script>

<template>
  <section class="lesson-detail">
    <div v-if="loading" class="lesson-detail__loading">
      <Skeleton height="28px" width="60%" />
      <Skeleton height="16px" :lines="4" />
    </div>

    <EmptyState
      v-else-if="!lesson"
      icon="book"
      :title="messages.common.notFound"
      description="Bài học không tồn tại hoặc đã bị ẩn."
    />

    <template v-else>
      <header v-if="!hideHeader" class="lesson-detail__header">
        <div>
          <h2 class="lesson-detail__title">{{ lesson.title }}</h2>
          <p class="lesson-detail__desc">{{ lesson.description }}</p>
        </div>
        <div class="lesson-detail__actions">
          <Badge v-if="lesson.progress?.viewed" variant="success">Đã học</Badge>
          <Button size="sm" variant="ghost" @click="notesOpen = true">📝 Ghi chú</Button>
          <Button size="sm" variant="secondary" @click="ratingOpen = true">★ Đánh giá</Button>
          <Button size="sm" variant="ghost" class="lesson-detail__report-btn" @click="reportOpen = true">
            🚩 Báo cáo vi phạm
          </Button>
          <Button size="sm" :disabled="lesson.progress?.viewed" @click="markViewed">
            {{ lesson.progress?.viewed ? 'Đã đánh dấu' : 'Đánh dấu đã học' }}
          </Button>
        </div>
      </header>

      <!-- Rich content: ProseContent auto-detect HTML thật vs plain text (escape + wrap đoạn) -->
      <article class="lesson-detail__content">
        <ProseContent :content="lesson.contentHtml || 'Bài học đang được biên soạn.'" />
      </article>

      <!-- Mô phỏng liên quan -->
      <section v-if="lesson.simulations && lesson.simulations.length > 0" class="lesson-detail__section">
        <h3 class="lesson-detail__section-title">Mô phỏng liên quan</h3>
        <div class="lesson-detail__sims">
          <Card
            v-for="sim in lesson.simulations"
            :key="sim.simulationKey"
            :padded="false"
            class="lesson-detail__sim hover-lift"
          >
            <div class="lesson-detail__sim-icon" aria-hidden="true">
              <BaseIcon name="play" :size="18" />
            </div>
            <div class="lesson-detail__sim-info">
              <p class="lesson-detail__sim-title">{{ sim.title || getCatalogMeta(sim.simulationKey)?.title || sim.simulationKey }}</p>
              <p class="text-muted lesson-detail__sim-key">{{ sim.simulationKey }}</p>
            </div>
            <Button size="sm" @click="emit('open-simulation', sim.simulationKey)">Mở mô phỏng</Button>
          </Card>
        </div>
      </section>

      <!-- Bài tập liên quan -->
      <section v-if="lesson.exercises && lesson.exercises.length > 0" class="lesson-detail__section">
        <h3 class="lesson-detail__section-title">Bài tập liên quan</h3>
        <div class="lesson-detail__sims">
          <Card v-for="ex in lesson.exercises" :key="ex.id" :padded="false" class="lesson-detail__sim hover-lift">
            <div class="lesson-detail__sim-icon" aria-hidden="true">
              <BaseIcon name="puzzle" :size="18" />
            </div>
            <div class="lesson-detail__sim-info">
              <p class="lesson-detail__sim-title">{{ ex.title }}</p>
              <Badge variant="muted">{{ ex.type }}</Badge>
            </div>
            <Button size="sm" @click="emit('open-exercise', ex.id)">Làm bài</Button>
          </Card>
        </div>
      </section>
    </template>

    <!-- Drawer ghi chú (FR-2.6 — autosave 1s) -->
    <Drawer :open="notesOpen" :title="`Ghi chú — ${lesson?.title ?? ''}`" @close="notesOpen = false">
      <textarea
        v-model="noteText"
        class="lesson-detail__note"
        rows="10"
        placeholder="Ghi chú của bạn về bài học này... (tự động lưu sau 1 giây)"
        @input="onNoteInput"
      />
      <p class="lesson-detail__note-status text-muted">
        {{ noteSavedAt ? `Đã lưu lúc ${noteSavedAt}` : 'Nhập để bắt đầu — lưu tự động' }}
      </p>
    </Drawer>

    <!-- Modal đánh giá (FR-7.4) -->
    <Teleport to="body">
      <Transition name="rating-fade">
        <div v-if="ratingOpen" class="lesson-detail__rating-overlay" @click.self="ratingOpen = false">
          <div class="lesson-detail__rating card">
            <h3 class="lesson-detail__rating-title">Đánh giá bài học</h3>
            <p class="lesson-detail__rating-note text-muted">
              Chỉ được đánh giá sau khi đã "Đánh dấu đã học" (ẩn danh, 1 lần/người).
            </p>
            <div class="lesson-detail__stars" role="radiogroup" aria-label="Số sao">
              <button
                v-for="star in 5"
                :key="star"
                type="button"
                class="lesson-detail__star"
                :class="{ 'lesson-detail__star--on': star <= rating }"
                :aria-label="`${star} sao`"
                @click="rating = star"
              >
                ★
              </button>
            </div>
            <p v-if="ratingSubmitted" class="lesson-detail__rating-done" role="status">
              Bạn đã đánh giá ★{{ rating }}
            </p>
            <div class="lesson-detail__rating-actions">
              <Button variant="ghost" @click="ratingOpen = false">{{ messages.common.close }}</Button>
              <Button :disabled="rating === 0 || ratingSubmitted" @click="submitRating">Gửi đánh giá</Button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>

    <!-- Modal báo cáo vi phạm (v2.15) -->
    <Teleport to="body">
      <Transition name="rating-fade">
        <div v-if="reportOpen" class="lesson-detail__rating-overlay" @click.self="reportOpen = false">
          <div class="lesson-detail__rating card">
            <h3 class="lesson-detail__rating-title">Báo cáo bài học vi phạm</h3>
            <p class="lesson-detail__rating-note text-muted">
              Báo cáo sẽ được gửi tới Admin xử lý (nội dung không phù hợp, sai kiến thức, 18+, spam...).
            </p>
            <textarea
              v-model="reportReason"
              class="lesson-detail__note"
              rows="4"
              maxlength="2000"
              placeholder="Mô tả lý do báo cáo (tối thiểu 5 ký tự)..."
            />
            <p v-if="reportSubmitted" class="lesson-detail__rating-done" role="status">
              Đã gửi báo cáo thành công
            </p>
            <div class="lesson-detail__rating-actions">
              <Button variant="ghost" @click="reportOpen = false">{{ messages.common.close }}</Button>
              <Button
                variant="danger"
                :disabled="reportReason.trim().length < 5 || reportSubmitted"
                @click="submitReport"
              >
                Gửi báo cáo
              </Button>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </section>
</template>

<style scoped>
.lesson-detail { display: flex; flex-direction: column; gap: var(--space-lg); }

.lesson-detail__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.lesson-detail__header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
  padding-bottom: var(--space-md);
  border-bottom: 1px solid var(--color-border);
}

.lesson-detail__title { font-size: var(--text-xl); }
.lesson-detail__desc { color: var(--color-text-muted); font-size: var(--text-sm); margin-top: 4px; }

.lesson-detail__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

/* Typography nội dung do ProseContent (ui/ProseContent.vue) đảm nhiệm.
   Chỉ giữ khoảng cách với các block khác (title/meta - parent flex gap). */
.lesson-detail__content { min-width: 0; }

.lesson-detail__section { display: flex; flex-direction: column; gap: var(--space-sm); }

.lesson-detail__section-title { font-size: var(--text-md); }

.lesson-detail__sims { display: flex; flex-direction: column; gap: var(--space-sm); }

.lesson-detail__sim { display: flex; align-items: center; gap: var(--space-md); padding: var(--space-md); }

.lesson-detail__sim-icon {
  flex-shrink: 0;
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.lesson-detail__sim-info { display: flex; flex-direction: column; gap: 4px; min-width: 0; flex: 1; }

.lesson-detail__sim-title { font-weight: 700; font-size: var(--text-sm); }
.lesson-detail__sim-key { font-size: var(--text-xs); font-family: var(--font-mono); }

.lesson-detail__note {
  width: 100%;
  font-family: inherit;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  resize: vertical;
}

.lesson-detail__note-status { font-size: var(--text-xs); margin-top: var(--space-sm); }

.lesson-detail__rating-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-md);
}

.lesson-detail__rating { width: 100%; max-width: 420px; display: flex; flex-direction: column; gap: var(--space-md); }

.lesson-detail__rating-title { font-size: var(--text-md); }
.lesson-detail__rating-note { font-size: var(--text-xs); }

.lesson-detail__stars { display: flex; gap: var(--space-xs); }

.lesson-detail__star {
  background: none;
  border: none;
  font-size: 2rem;
  color: var(--color-border);
  cursor: pointer;
  transition: var(--transition-fast);
}

.lesson-detail__star--on { color: var(--color-warning); }

.lesson-detail__rating-done { color: var(--color-success); font-weight: 700; }

.lesson-detail__rating-actions { display: flex; justify-content: flex-end; gap: var(--space-sm); }

.rating-fade-enter-active, .rating-fade-leave-active { transition: opacity 200ms ease; }
.rating-fade-enter-from, .rating-fade-leave-to { opacity: 0; }
</style>
