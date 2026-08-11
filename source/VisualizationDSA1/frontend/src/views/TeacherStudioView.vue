<template>
  <div class="ts-page">

    <!-- ── HERO HEADER ── -->
    <div class="ts-hero">
      <div class="ts-hero__bg" aria-hidden="true">
        <div class="ts-hero__blob ts-hero__blob--1"></div>
        <div class="ts-hero__blob ts-hero__blob--2"></div>
      </div>
      <div class="ts-hero__content">
        <div class="ts-hero__left">
          <div class="ts-hero__badge">
            <BaseIcon name="teacher" class="w-4 h-4" />
            <span>Teacher Studio</span>
          </div>
          <h1 class="ts-hero__title">Quản lý Lộ trình <span class="ts-hero__title-accent">của bạn</span></h1>
          <p class="ts-hero__sub">Tạo, biên tập và xuất bản các lộ trình học thuật — theo dõi tiến trình học viên theo thời gian thực.</p>
        </div>
        <div class="ts-hero__actions">
          <button class="ts-btn-create" @click="showCreateModal = true">
            <BaseIcon name="plus" class="w-5 h-5" />
            Tạo Lộ trình mới
          </button>
        </div>
      </div>

      <!-- Stats bar -->
      <div class="ts-stats-bar" v-if="!loading && roadmaps.length > 0">
        <div class="ts-stat-item">
          <span class="ts-stat-value">{{ roadmaps.length }}</span>
          <span class="ts-stat-label">Lộ trình</span>
        </div>
        <div class="ts-stat-divider"></div>
        <div class="ts-stat-item">
          <span class="ts-stat-value">{{ roadmaps.filter(r => r.status === 'Published').length }}</span>
          <span class="ts-stat-label">Đã xuất bản</span>
        </div>
        <div class="ts-stat-divider"></div>
        <div class="ts-stat-item">
          <span class="ts-stat-value">{{ roadmaps.reduce((s, r) => s + r.nodes.length, 0) }}</span>
          <span class="ts-stat-label">Tổng số Nodes</span>
        </div>
        <div class="ts-stat-divider"></div>
        <div class="ts-stat-item">
          <span class="ts-stat-value">{{ Object.values(statsMap).reduce((s, v) => s + (v.enrollCount || 0), 0) }}</span>
          <span class="ts-stat-label">Học viên đã đăng ký</span>
        </div>
      </div>
    </div>

    <!-- ── MAIN CONTENT ── -->
    <div class="ts-content">

      <!-- Loading -->
      <div v-if="loading" class="ts-empty-state">
        <div class="ts-spinner"></div>
        <p class="text-secondary">Đang tải lộ trình...</p>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="ts-empty-state">
        <BaseIcon name="warning" class="w-12 h-12 text-accent-red mb-4" />
        <p class="text-accent-red">{{ error }}</p>
      </div>

      <!-- Grid -->
      <div v-else-if="roadmaps.length > 0" class="ts-grid">
        <div
          v-for="rm in roadmaps"
          :key="rm.id"
          class="ts-card"
        >
          <!-- Thumbnail -->
          <div class="ts-card__thumb">
            <img v-if="rm.thumbnailUrl" :src="rm.thumbnailUrl" alt="Thumbnail" class="ts-card__thumb-img" />
            <div v-else class="ts-card__thumb-fallback">
              <BaseIcon name="map" class="w-12 h-12" />
            </div>
            <div class="ts-card__thumb-overlay"></div>

            <!-- Status badge -->
            <span class="ts-status-badge" :class="`ts-status-badge--${rm.status.toLowerCase()}`">
              <span class="ts-status-dot"></span>
              {{ rm.status }}
            </span>
          </div>

          <!-- Body -->
          <div class="ts-card__body">
            <h3 class="ts-card__title">{{ rm.name }}</h3>
            <p class="ts-card__desc">{{ rm.description }}</p>

            <!-- Meta chips -->
            <div class="ts-card__chips">
              <span class="ts-chip">
                <BaseIcon name="collection" class="w-3.5 h-3.5" />
                {{ rm.nodes.length }} Nodes
              </span>
              <span class="ts-chip" :class="rm.visibility === 'Private' ? 'ts-chip--private' : 'ts-chip--public'">
                <BaseIcon :name="rm.visibility === 'Private' ? 'lock-closed' : 'users'" class="w-3.5 h-3.5" />
                {{ rm.visibility === 'Private' ? 'Cá nhân' : 'Lớp học' }}
              </span>
            </div>

            <!-- Stats row -->
            <div class="ts-card__stats">
              <div class="ts-card__stat-item">
                <BaseIcon name="users" class="w-3.5 h-3.5 text-accent" />
                <span>{{ statsMap[rm.id]?.enrollCount ?? 0 }} học viên</span>
              </div>
              <div class="ts-card__stat-item">
                <BaseIcon name="star" class="w-3.5 h-3.5 text-accent-warm" />
                <span>
                  <StarRatingDisplay
                    v-if="statsMap[rm.id]?.avgRating != null"
                    :value="statsMap[rm.id]?.avgRating ?? null"
                    :count="statsMap[rm.id]?.reviewCount ?? 0"
                    show-value
                    show-count
                  />
                  <span v-else class="text-muted">Chưa có đánh giá</span>
                </span>
              </div>
            </div>

            <!-- Actions -->
            <div class="ts-card__actions">
              <button class="ts-btn-edit" @click="editRoadmap(rm.id)">
                <BaseIcon name="edit" class="w-4 h-4" />
                Biên tập
              </button>
              <button
                class="ts-btn-delete"
                title="Xóa Lộ trình"
                @click="confirmDelete(rm)"
              >
                <BaseIcon name="trash" class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div v-else class="ts-empty-state ts-empty-state--large glass-panel">
        <div class="ts-empty-icon">
          <BaseIcon name="map" class="w-16 h-16" />
        </div>
        <h3 class="ts-empty-title">Chưa có Lộ trình nào</h3>
        <p class="ts-empty-desc">Bắt đầu tạo lộ trình đầu tiên của bạn để chia sẻ kiến thức với học viên.</p>
        <button class="ts-btn-create" @click="showCreateModal = true">
          <BaseIcon name="plus" class="w-5 h-5" />
          Tạo Lộ trình đầu tiên
        </button>
      </div>
    </div>

    <!-- ── CREATE MODAL ── -->
    <div v-if="showCreateModal" class="ts-modal-backdrop" @click.self="showCreateModal = false">
      <div class="ts-modal glass-panel">
        <div class="ts-modal__header">
          <div class="ts-modal__header-left">
            <BaseIcon name="plus" class="w-5 h-5 text-accent" />
            <h3>Tạo Lộ trình mới</h3>
          </div>
          <button class="ts-modal__close" @click="showCreateModal = false">
            <BaseIcon name="x" class="w-5 h-5" />
          </button>
        </div>

        <form @submit.prevent="handleCreateRoadmap" class="ts-modal__body">
          <div class="ts-field">
            <label class="ts-label">Tên Lộ trình <span class="text-accent-red">*</span></label>
            <input
              v-model="newRoadmapForm.name"
              type="text"
              required
              class="ts-input"
              placeholder="VD: Nhập môn Cấu trúc dữ liệu"
            />
          </div>

          <div class="ts-field">
            <label class="ts-label">Mô tả chi tiết <span class="text-accent-red">*</span></label>
            <textarea
              v-model="newRoadmapForm.description"
              required
              rows="3"
              class="ts-input ts-textarea"
              placeholder="Nhập mô tả..."
            ></textarea>
          </div>

          <div class="ts-field">
            <label class="ts-label">Tags</label>
            <div class="ts-tags-list" v-if="tagsArray.length">
              <span v-for="tag in tagsArray" :key="tag" class="ts-tag">
                {{ tag }}
                <button type="button" @click="removeTag(tag)" class="ts-tag__remove">
                  <BaseIcon name="x" class="w-3 h-3" />
                </button>
              </span>
            </div>
            <div class="ts-tag-input-row">
              <input
                v-model="newTagInput"
                type="text"
                @keyup.enter="addTag"
                class="ts-input"
                placeholder="Thêm tag (nhấn Enter)"
              />
              <button type="button" @click="addTag" class="ts-btn-secondary">Thêm</button>
            </div>
          </div>

          <div class="ts-field">
            <label class="ts-label">Ảnh bìa (Thumbnail)</label>
            <input type="file" ref="thumbnailInput" accept="image/*" @change="handleThumbnailUpload" class="hidden" id="thumbnail-upload" />
            <button
              type="button"
              @click="triggerThumbnailInput"
              class="ts-upload-zone"
            >
              <BaseIcon name="image" class="w-8 h-8 text-muted" />
              <span class="text-secondary text-sm">Chọn ảnh bìa (tối đa 2MB)</span>
            </button>
            <div v-if="thumbnailPreview" class="ts-thumb-preview">
              <img :src="thumbnailPreview" alt="Preview" class="ts-thumb-img" />
              <button type="button" @click="removeThumbnail" class="ts-thumb-remove">
                <BaseIcon name="x" class="w-4 h-4" />
              </button>
            </div>
          </div>

          <div class="ts-field">
            <label class="ts-label">Quyền riêng tư</label>
            <select v-model="newRoadmapForm.visibility" class="ts-input ts-select">
              <option value="Private">🔒 Private — Chỉ mình tôi</option>
              <option value="ClassroomOnly">👥 Classroom Only — Chỉ dành cho lớp học</option>
            </select>
          </div>

          <div class="ts-modal__footer">
            <button type="button" class="ts-btn-secondary" @click="showCreateModal = false">Hủy</button>
            <button type="submit" class="ts-btn-create" :disabled="isSubmitting">
              {{ isSubmitting ? 'Đang tạo...' : 'Tạo Lộ trình' }}
            </button>
          </div>
        </form>
      </div>
    </div>

  </div>
</template>

<script setup lang="ts">
import { useConfirmDialogStore } from '@/features/ui/store/useConfirmDialogStore';
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { teacherStudioService, type CustomRoadmapDto } from '@/services/TeacherStudioService';
import { roadmapApi, type RoadmapStatsDto } from '@/services/roadmapApi';
import StarRatingDisplay from '@/components/rating/StarRatingDisplay.vue';
import { useToastStore } from '@/composables/useToast';
import BaseIcon from '@/shared/components/BaseIcon.vue';

const router = useRouter();
const toastStore = useToastStore();
const confirmDialogStore = useConfirmDialogStore();

const roadmaps = ref<CustomRoadmapDto[]>([]);
const loading = ref(true);
const error = ref('');
const statsMap = ref<Record<string, RoadmapStatsDto>>({});

const showCreateModal = ref(false);
const isSubmitting = ref(false);
const newTagInput = ref('');
const thumbnailInput = ref<HTMLInputElement | null>(null);
const thumbnailPreview = ref<string | null>(null);
const newRoadmapForm = ref({
  name: '',
  description: '',
  tags: '[]',
  visibility: 'Private',
  thumbnailUrl: ''
});

const tagsArray = computed(() => {
  try {
    return JSON.parse(newRoadmapForm.value.tags || '[]');
  } catch {
    return [];
  }
});

const addTag = () => {
  const tag = newTagInput.value.trim();
  if (!tag) return;
  const tags = tagsArray.value;
  if (!tags.includes(tag)) {
    tags.push(tag);
    newRoadmapForm.value.tags = JSON.stringify(tags);
    newTagInput.value = '';
  }
};

const removeTag = (tag: string) => {
  const tags = tagsArray.value.filter((t: string) => t !== tag);
  newRoadmapForm.value.tags = JSON.stringify(tags);
};

const triggerThumbnailInput = () => {
  thumbnailInput.value?.click();
};

const handleThumbnailUpload = (event: Event) => {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;
  if (file.size > 2 * 1024 * 1024) { toastStore.error('File quá lớn. Tối đa 2MB.'); input.value = ''; return; }
  if (!file.type.startsWith('image/')) { toastStore.error('Chỉ chấp nhận file ảnh.'); input.value = ''; return; }
  const reader = new FileReader();
  reader.onload = (e) => {
    thumbnailPreview.value = e.target?.result as string;
    newRoadmapForm.value.thumbnailUrl = e.target?.result as string;
  };
  reader.readAsDataURL(file);
};

const removeThumbnail = () => {
  thumbnailPreview.value = null;
  newRoadmapForm.value.thumbnailUrl = '';
  if (thumbnailInput.value) thumbnailInput.value.value = '';
};

const loadRoadmaps = async () => {
  loading.value = true;
  error.value = '';
  try {
    const res = await teacherStudioService.getMyRoadmaps();
    roadmaps.value = res;
    await loadStats();
  } catch (err: any) {
    console.error('Error loading roadmaps:', err);
    roadmaps.value = [];
  } finally {
    loading.value = false;
  }
};

const loadStats = async () => {
  const list = roadmaps.value;
  const result: Record<string, RoadmapStatsDto> = {};
  await Promise.all(
    list.map(async (rm) => {
      try {
        result[rm.id] = await roadmapApi.getStats(rm.id);
      } catch {
        result[rm.id] = { enrollCount: 0, completionCount: 0, reviewCount: 0, avgRating: null, myRating: null, myCanReview: false };
      }
    }),
  );
  statsMap.value = result;
};

const handleCreateRoadmap = async () => {
  if (!newRoadmapForm.value.name || !newRoadmapForm.value.description) return;
  isSubmitting.value = true;
  try {
    const formData = {
      name: newRoadmapForm.value.name,
      description: newRoadmapForm.value.description,
      tags: newRoadmapForm.value.tags,
      visibility: newRoadmapForm.value.visibility,
      thumbnailUrl: newRoadmapForm.value.thumbnailUrl || undefined
    };
    const res = await teacherStudioService.createRoadmap(formData);
    roadmaps.value.unshift(res);
    showCreateModal.value = false;
    toastStore.success('Tạo Lộ trình thành công!');
    newRoadmapForm.value = { name: '', description: '', tags: '[]', visibility: 'Private', thumbnailUrl: '' };
    thumbnailPreview.value = null;
    newTagInput.value = '';
    if (thumbnailInput.value) thumbnailInput.value.value = '';
    router.push(`/teacher-studio/${res.id}`);
  } catch (err: any) {
    toastStore.error(err.response?.data?.message || 'Lỗi khi tạo Lộ trình');
  } finally {
    isSubmitting.value = false;
  }
};

const editRoadmap = (id: string) => { router.push(`/teacher-studio/${id}`); };

const confirmDelete = async (rm: CustomRoadmapDto) => {
  if (rm.status === 'Published' || rm.status === 'Pending') {
    toastStore.error('Chỉ có thể xóa Lộ trình ở trạng thái Draft hoặc Rejected.');
    return;
  }
  if ((await confirmDialogStore.confirmDialog({ title: 'Xác nhận', message: `Bạn có chắc muốn xóa Lộ trình "${rm.name}"? Hành động này không thể hoàn tác.`, variant: 'danger', confirmText: 'Xác nhận', cancelText: 'Hủy' }))) {
    try {
      await teacherStudioService.deleteRoadmap(rm.id);
      roadmaps.value = roadmaps.value.filter(r => r.id !== rm.id);
      toastStore.success('Xóa Lộ trình thành công');
    } catch (err: any) {
      toastStore.error(err.response?.data?.message || 'Lỗi khi xóa Lộ trình');
    }
  }
};

onMounted(() => { loadRoadmaps(); });
</script>

<style scoped>
/* ── PAGE ── */
.ts-page {
  display: flex;
  flex-direction: column;
  min-height: 100%;
  font-family: var(--font-sans, 'Inter', sans-serif);
}

/* ── HERO ── */
.ts-hero {
  position: relative;
  padding: 2.5rem 2rem 0;
  overflow: hidden;
}
.ts-hero__bg {
  position: absolute;
  inset: 0;
  pointer-events: none;
  z-index: 0;
}
.ts-hero__blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(80px);
  opacity: 0.12;
}
.ts-hero__blob--1 {
  width: 500px; height: 500px;
  background: var(--color-accent-primary, #6366f1);
  top: -200px; left: -100px;
}
.ts-hero__blob--2 {
  width: 400px; height: 400px;
  background: var(--color-accent-purple, #a855f7);
  top: -150px; right: 0;
}
.ts-hero__content {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 2rem;
  flex-wrap: wrap;
  padding-bottom: 1.5rem;
}
.ts-hero__left { flex: 1; min-width: 260px; }
.ts-hero__badge {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  background: rgba(99, 102, 241, 0.15);
  border: 1px solid rgba(99, 102, 241, 0.3);
  color: var(--color-accent-primary, #818cf8);
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  padding: 0.3rem 0.75rem;
  border-radius: 100px;
  margin-bottom: 0.75rem;
}
.ts-hero__title {
  font-size: clamp(1.5rem, 3vw, 2.25rem);
  font-weight: 800;
  color: var(--color-text-heading, #f4f4f5);
  line-height: 1.15;
  margin: 0 0 0.6rem 0;
  letter-spacing: -0.02em;
}
.ts-hero__title-accent {
  background: linear-gradient(135deg, var(--color-accent-primary, #818cf8) 0%, var(--color-accent-purple, #c084fc) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}
.ts-hero__sub {
  font-size: 0.875rem;
  color: var(--color-text-secondary, #a1a1aa);
  margin: 0;
  line-height: 1.6;
  max-width: 540px;
}
.ts-hero__actions { display: flex; align-items: center; padding-top: 0.25rem; }

/* Stats bar */
.ts-stats-bar {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: center;
  gap: 0;
  background: var(--color-bg-surface, #18181b);
  border: 1px solid var(--color-border-subtle, #27272a);
  border-bottom: none;
  border-radius: 12px 12px 0 0;
  padding: 1rem 2rem;
  margin-top: 1rem;
}
.ts-stat-item { display: flex; flex-direction: column; gap: 0.1rem; padding: 0 1.5rem; }
.ts-stat-item:first-child { padding-left: 0; }
.ts-stat-value { font-size: 1.5rem; font-weight: 800; color: var(--color-text-heading, #f4f4f5); line-height: 1; }
.ts-stat-label { font-size: 0.7rem; color: var(--color-text-muted, #71717a); font-weight: 500; margin-top: 0.1rem; }
.ts-stat-divider { width: 1px; height: 36px; background: var(--color-border-subtle, #27272a); flex-shrink: 0; }

/* ── CONTENT ── */
.ts-content {
  flex: 1;
  padding: 1.5rem 2rem 2rem;
  background: var(--color-bg-surface, #18181b);
  border: 1px solid var(--color-border-subtle, #27272a);
  border-top: none;
  border-radius: 0 0 12px 12px;
  margin: 0;
}

/* ── GRID ── */
.ts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.25rem;
}

/* ── CARD ── */
.ts-card {
  display: flex;
  flex-direction: column;
  border-radius: 14px;
  background: var(--color-bg-base, #0a0a0f);
  border: 1px solid var(--color-border-default, #27272a);
  overflow: hidden;
  transition: border-color 0.2s ease, transform 0.2s ease, box-shadow 0.2s ease;
}
.ts-card:hover {
  border-color: rgba(99, 102, 241, 0.4);
  transform: translateY(-3px);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.4), 0 0 0 1px rgba(99, 102, 241, 0.1);
}
.ts-card__thumb {
  position: relative;
  height: 160px;
  overflow: hidden;
  flex-shrink: 0;
}
.ts-card__thumb-img {
  width: 100%; height: 100%;
  object-fit: cover;
  transition: transform 0.4s ease;
}
.ts-card:hover .ts-card__thumb-img { transform: scale(1.05); }
.ts-card__thumb-fallback {
  width: 100%; height: 100%;
  display: flex; align-items: center; justify-content: center;
  background: linear-gradient(135deg, rgba(99,102,241,0.12) 0%, rgba(168,85,247,0.2) 100%);
  color: rgba(99, 102, 241, 0.5);
}
.ts-card__thumb-overlay {
  position: absolute; inset: 0;
  background: linear-gradient(180deg, transparent 50%, rgba(10,10,15,0.9) 100%);
  pointer-events: none;
}
.ts-status-badge {
  position: absolute; top: 10px; right: 10px; z-index: 2;
  display: inline-flex; align-items: center; gap: 0.35rem;
  font-size: 0.65rem; font-weight: 700; text-transform: uppercase; letter-spacing: 0.06em;
  padding: 0.25rem 0.6rem; border-radius: 100px;
  backdrop-filter: blur(8px);
}
.ts-status-dot {
  width: 6px; height: 6px; border-radius: 50%;
  animation: pulse 2s infinite;
}
.ts-status-badge--draft { background: rgba(30,30,40,0.85); border: 1px solid rgba(100,100,120,0.3); color: #a1a1aa; }
.ts-status-badge--draft .ts-status-dot { background: #71717a; }
.ts-status-badge--pending { background: rgba(245,158,11,0.15); border: 1px solid rgba(245,158,11,0.3); color: #fbbf24; }
.ts-status-badge--pending .ts-status-dot { background: #f59e0b; }
.ts-status-badge--published { background: rgba(34,197,94,0.12); border: 1px solid rgba(34,197,94,0.3); color: #4ade80; }
.ts-status-badge--published .ts-status-dot { background: #22c55e; }
.ts-status-badge--rejected { background: rgba(239,68,68,0.12); border: 1px solid rgba(239,68,68,0.3); color: #f87171; }
.ts-status-badge--rejected .ts-status-dot { background: #ef4444; }

.ts-card__body {
  flex: 1; display: flex; flex-direction: column;
  padding: 1.1rem 1.25rem 1.25rem;
  gap: 0.6rem;
}
.ts-card__title {
  font-size: 0.95rem; font-weight: 700;
  color: var(--color-text-heading, #f4f4f5);
  margin: 0; line-height: 1.35;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
}
.ts-card__desc {
  font-size: 0.78rem; color: var(--color-text-secondary, #a1a1aa);
  margin: 0; line-height: 1.55;
  display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;
  flex: 1;
}
.ts-card__chips { display: flex; flex-wrap: wrap; gap: 0.4rem; }
.ts-chip {
  display: inline-flex; align-items: center; gap: 0.3rem;
  font-size: 0.7rem; font-weight: 600;
  padding: 0.25rem 0.6rem;
  background: var(--color-bg-surface, #18181b);
  border: 1px solid var(--color-border-subtle, #27272a);
  color: var(--color-text-secondary, #a1a1aa);
  border-radius: 6px;
}
.ts-chip--private { color: #f87171; border-color: rgba(239,68,68,0.2); }
.ts-chip--public { color: #4ade80; border-color: rgba(34,197,94,0.2); }

.ts-card__stats {
  display: flex; flex-direction: column; gap: 0.3rem;
  padding-top: 0.4rem;
  border-top: 1px solid var(--color-border-subtle, #1e1e24);
}
.ts-card__stat-item {
  display: flex; align-items: center; gap: 0.4rem;
  font-size: 0.75rem; color: var(--color-text-secondary, #a1a1aa);
}
.ts-card__actions {
  display: flex; gap: 0.6rem; margin-top: auto; padding-top: 0.4rem;
}

/* ── BUTTONS ── */
.ts-btn-create {
  display: inline-flex; align-items: center; gap: 0.5rem;
  background: var(--color-accent-primary, #6366f1);
  color: #fff;
  font-size: 0.85rem; font-weight: 700;
  padding: 0.6rem 1.25rem;
  border: none; border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 4px 16px rgba(99,102,241,0.35);
}
.ts-btn-create:hover { background: #4f46e5; transform: translateY(-1px); box-shadow: 0 6px 24px rgba(99,102,241,0.45); }
.ts-btn-create:disabled { opacity: 0.5; cursor: not-allowed; transform: none; }

.ts-btn-edit {
  flex: 1; display: inline-flex; align-items: center; justify-content: center; gap: 0.4rem;
  background: rgba(99,102,241,0.12); border: 1px solid rgba(99,102,241,0.25);
  color: var(--color-accent-primary, #818cf8);
  font-size: 0.8rem; font-weight: 600;
  padding: 0.55rem 1rem; border-radius: 8px;
  cursor: pointer; transition: all 0.15s ease;
}
.ts-btn-edit:hover { background: rgba(99,102,241,0.2); border-color: rgba(99,102,241,0.4); }

.ts-btn-delete {
  display: inline-flex; align-items: center; justify-content: center;
  background: rgba(239,68,68,0.08); border: 1px solid rgba(239,68,68,0.2);
  color: #f87171;
  padding: 0.55rem 0.7rem; border-radius: 8px;
  cursor: pointer; transition: all 0.15s ease;
}
.ts-btn-delete:hover { background: rgba(239,68,68,0.18); border-color: rgba(239,68,68,0.4); }

.ts-btn-secondary {
  display: inline-flex; align-items: center; gap: 0.4rem;
  background: var(--color-bg-hover, #27272a); border: 1px solid var(--color-border-subtle, #3f3f46);
  color: var(--color-text-secondary, #a1a1aa);
  font-size: 0.82rem; font-weight: 600;
  padding: 0.55rem 1rem; border-radius: 8px;
  cursor: pointer; transition: all 0.15s ease;
}
.ts-btn-secondary:hover { background: var(--color-bg-active, #3f3f46); color: var(--color-text-heading, #f4f4f5); }

/* ── EMPTY STATE ── */
.ts-empty-state {
  display: flex; flex-direction: column; align-items: center; justify-content: center;
  padding: 4rem 2rem; text-align: center; gap: 0.75rem;
}
.ts-empty-state--large { border-radius: 14px; border: 2px dashed var(--color-border-default, #27272a); }
.ts-empty-icon {
  width: 72px; height: 72px; border-radius: 16px;
  background: rgba(99,102,241,0.1); border: 1px solid rgba(99,102,241,0.2);
  display: flex; align-items: center; justify-content: center;
  color: rgba(99,102,241,0.6);
  margin-bottom: 0.5rem;
}
.ts-empty-title { font-size: 1.1rem; font-weight: 700; color: var(--color-text-heading, #f4f4f5); margin: 0; }
.ts-empty-desc { font-size: 0.875rem; color: var(--color-text-secondary, #a1a1aa); margin: 0; max-width: 380px; }

/* Spinner */
.ts-spinner {
  width: 36px; height: 36px; border-radius: 50%;
  border: 3px solid var(--color-border-subtle, #27272a);
  border-top-color: var(--color-accent-primary, #6366f1);
  animation: spin 0.7s linear infinite;
  margin-bottom: 0.75rem;
}
@keyframes spin { to { transform: rotate(360deg); } }
@keyframes pulse { 0%, 100% { opacity: 1; } 50% { opacity: 0.4; } }

/* ── MODAL ── */
.ts-modal-backdrop {
  position: fixed; inset: 0;
  background: rgba(0,0,0,0.65);
  backdrop-filter: blur(6px);
  display: flex; align-items: center; justify-content: center;
  z-index: 9999; padding: 1rem;
}
.ts-modal {
  width: 100%; max-width: 520px;
  border-radius: 16px;
  overflow: hidden;
  animation: slideUp 0.2s cubic-bezier(0.16,1,0.3,1);
}
@keyframes slideUp { from { opacity:0; transform: translateY(16px) scale(0.97); } to { opacity:1; transform: translateY(0) scale(1); } }
.ts-modal__header {
  display: flex; align-items: center; justify-content: space-between;
  padding: 1.1rem 1.5rem;
  border-bottom: 1px solid var(--color-border-subtle, #27272a);
}
.ts-modal__header-left { display: flex; align-items: center; gap: 0.6rem; }
.ts-modal__header-left h3 { font-size: 1rem; font-weight: 700; color: var(--color-text-heading, #f4f4f5); margin: 0; }
.ts-modal__close {
  background: none; border: none;
  color: var(--color-text-secondary, #a1a1aa);
  cursor: pointer; padding: 0.25rem;
  border-radius: 6px; transition: all 0.15s;
  display: flex; align-items: center;
}
.ts-modal__close:hover { background: var(--color-bg-hover, #27272a); color: var(--color-text-heading, #f4f4f5); }
.ts-modal__body { display: flex; flex-direction: column; gap: 1rem; padding: 1.5rem; max-height: 70vh; overflow-y: auto; }
.ts-modal__footer {
  display: flex; justify-content: flex-end; gap: 0.75rem;
  padding-top: 0.5rem;
  border-top: 1px solid var(--color-border-subtle, #27272a);
}

/* Form */
.ts-field { display: flex; flex-direction: column; gap: 0.4rem; }
.ts-label { font-size: 0.8rem; font-weight: 600; color: var(--color-text-secondary, #a1a1aa); }
.ts-input {
  background: var(--color-bg-secondary, #121316);
  border: 1px solid var(--color-border-subtle, #27272a);
  border-radius: 8px; padding: 0.6rem 0.875rem;
  font-size: 0.85rem; color: var(--color-text-heading, #f4f4f5);
  outline: none; transition: border-color 0.15s, box-shadow 0.15s;
  width: 100%; box-sizing: border-box;
  font-family: inherit;
}
.ts-input:focus { border-color: var(--color-accent-primary, #6366f1); box-shadow: 0 0 0 2px rgba(99,102,241,0.15); }
.ts-textarea { resize: vertical; min-height: 80px; }
.ts-select { cursor: pointer; }

.ts-tags-list { display: flex; flex-wrap: wrap; gap: 0.4rem; margin-bottom: 0.4rem; }
.ts-tag {
  display: inline-flex; align-items: center; gap: 0.3rem;
  background: rgba(99,102,241,0.12); border: 1px solid rgba(99,102,241,0.25);
  color: var(--color-accent-primary, #818cf8);
  font-size: 0.75rem; font-weight: 600;
  padding: 0.2rem 0.6rem; border-radius: 100px;
}
.ts-tag__remove { background: none; border: none; color: inherit; cursor: pointer; padding: 0; display: flex; opacity: 0.7; }
.ts-tag__remove:hover { opacity: 1; }
.ts-tag-input-row { display: flex; gap: 0.5rem; }
.ts-tag-input-row .ts-input { flex: 1; }

.ts-upload-zone {
  width: 100%; border: 2px dashed var(--color-border-subtle, #27272a);
  border-radius: 10px; padding: 1.5rem;
  display: flex; flex-direction: column; align-items: center; gap: 0.5rem;
  background: none; cursor: pointer;
  transition: border-color 0.15s, background 0.15s;
  color: var(--color-text-secondary, #a1a1aa);
}
.ts-upload-zone:hover { border-color: rgba(99,102,241,0.4); background: rgba(99,102,241,0.04); }
.ts-thumb-preview { position: relative; width: 100%; height: 120px; border-radius: 8px; overflow: hidden; }
.ts-thumb-img { width: 100%; height: 100%; object-fit: cover; }
.ts-thumb-remove {
  position: absolute; top: 6px; right: 6px;
  background: rgba(0,0,0,0.6); border: none; border-radius: 50%;
  width: 26px; height: 26px; display: flex; align-items: center; justify-content: center;
  color: #fff; cursor: pointer;
}
.ts-thumb-remove:hover { background: rgba(0,0,0,0.8); }
</style>
