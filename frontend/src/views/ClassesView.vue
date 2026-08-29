<script setup lang="ts">
// ClassesView — Màn 19: danh sách lớp + nhập mã mời 6 ký tự + tạo lớp (Teacher)
// View-quality Phase 1 (Nhóm D): banner = surface band level-2 + mono strip
// block-token dữ liệu thật (DESIGN §1/#1); card level-1 không shadow (hover chỉ
// đổi border); mã mời = block-token tối canvas-ink (quyết định #4/#5).
import { computed, nextTick, onMounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { BarChart3, GraduationCap, KeyRound, Plus, UserRound, Users } from 'lucide-vue-next';

import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import type { ClassDto } from '@/api/types';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Card from '@/components/ui/Card.vue';

const classStore = useClassStore();
const auth = useAuthStore();
const ui = useUiStore();
const router = useRouter();

const loading = ref(true);
const joinOpen = ref(false);
const createOpen = ref(false);
const inviteCode = ref('');
const joinError = ref('');
const newClassName = ref('');
const newClassDesc = ref('');
const creating = ref(false);

/** Id input mã mời trong Join modal — dùng để tự focus khi mở modal. */
const joinInputId = 'class-join-code-input';

const isTeacher = computed(() => auth.role === 'TEACHER' || auth.role === 'ADMIN');

/** API list KHÔNG trả `role` (chỉ OwnerId — ClassService.ToDto) → tính từ owner. */
const isManagerOf = (cls: ClassDto): boolean => cls.ownerId === auth.user?.id || auth.role === 'ADMIN';

/** Strip banner: block-token dữ liệu thật — số lớp (tối đa 5 block) + index mono. */
const stripBlocks = computed<boolean[]>(() => {
  const count = Math.min(classStore.classes.length, 5);
  const size = Math.max(count, 1);
  return Array.from({ length: size }, (_, i) => i < count);
});

const stripLabel = computed(() => {
  const total = classStore.classes.length;
  const members = classStore.classes.reduce((sum, cls) => sum + cls.memberCount, 0);
  return messages.classes.stripLabel(total, members);
});

// Phân trang danh sách lớp
const PAGE_SIZE = 6;
const page = ref(1);
const totalPages = computed(() => Math.max(1, Math.ceil(classStore.classes.length / PAGE_SIZE)));
const pagedClasses = computed(() => {
  const start = (page.value - 1) * PAGE_SIZE;
  return classStore.classes.slice(start, start + PAGE_SIZE);
});

/** Empty state học viên: nêu rõ mã mời 6 ký tự (ghép từ i18n sẵn có — goal 3.6 #1). */

const emptyStudentDesc = computed(
  () => `${messages.classes.emptyStudentDesc} ${messages.classes.joinCodeHint}`,
);

// Nổi bật input mã mời: tự focus khi mở Join modal (EmptyState → nút "Nhập mã lớp").
watch(joinOpen, (open) => {
  if (!open) return;
  void nextTick(() => document.getElementById(joinInputId)?.focus());
});

onMounted(async () => {
  try {
    await classStore.fetchClasses();
  } catch {
    ui.showToast(messages.classes.loadError, 'error');
  } finally {
    loading.value = false;
  }
});

/** Chuẩn hóa mã mời: viết hoa + chỉ A-Z0-9 + tối đa 6 ký tự (UX: nhập sai tự sửa). */
function onInviteInput(value: string): void {
  inviteCode.value = value.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 6);
}

watch(inviteCode, (val) => {
  const norm = val.toUpperCase().replace(/[^A-Z0-9]/g, '').slice(0, 6);
  if (norm !== val) inviteCode.value = norm;
});

function goDetail(id: number): void {
  void router.push({ name: 'class-detail', params: { id: String(id) } });
}

async function join(): Promise<void> {
  if (inviteCode.value.length !== 6) {
    joinError.value = messages.classes.joinCodeError;
    return;
  }
  joinError.value = '';
  try {
    const joined = await classStore.joinByCode(inviteCode.value);
    ui.showToast(messages.classes.joinSuccess, 'success');
    joinOpen.value = false;
    void router.push({ name: 'class-detail', params: { id: String(joined.id) } });
  } catch (err) {
    joinError.value = err instanceof Error ? err.message : messages.classes.joinGenericError;
  }
}

async function createClass(): Promise<void> {
  if (newClassName.value.trim().length < 3) {
    ui.showToast(messages.classes.createNameTooShort, 'warning');
    return;
  }
  creating.value = true;
  try {
    const created = await classStore.createClass(newClassName.value.trim(), newClassDesc.value || undefined);
    ui.showToast(messages.classes.createSuccess(created.inviteCode), 'success');
    createOpen.value = false;
    void router.push({ name: 'class-detail', params: { id: String(created.id) } });
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.createFailed, 'error');
  } finally {
    creating.value = false;
  }
}
</script>

<template>
  <section class="classes container">
    <!-- Banner: surface band level-2 (DESIGN §1/#1 — KHÔNG gradient, KHÔNG shadow) -->
    <header class="classes__hero">
      <div class="classes__hero-inner">
        <div class="classes__hero-main">
          <div class="classes__hero-badges">
            <Badge variant="primary">{{ messages.classes.badge }}</Badge>
          </div>
          <h1 class="classes__hero-title">{{ messages.classes.title }}</h1>
          <p class="classes__hero-desc">{{ messages.classes.subtitle }}</p>
          <div class="classes__hero-actions">
            <Button v-if="isTeacher" size="lg" @click="createOpen = true">
              <Plus :size="16" aria-hidden="true" /> {{ messages.classes.createBtn }}
            </Button>
            <Button v-else size="lg" @click="joinOpen = true">
              <KeyRound :size="16" aria-hidden="true" /> {{ messages.classes.joinBtn }}
            </Button>
          </div>
        </div>

        <!-- Mono strip: block-token dữ liệu thật (số lớp) + index mono (quyết định #4) -->
        <div class="classes__hero-strip" aria-hidden="true">
          <div class="classes__strip-panel">
            <div class="classes__strip-blocks">
              <span
                v-for="(filled, i) in stripBlocks"
                :key="i"
                class="classes__strip-block"
                :class="{ 'classes__strip-block--empty': !filled }"
                :style="{ '--i': i }"
              />
            </div>
            <div class="classes__strip-index">
              <span v-for="(_, i) in stripBlocks" :key="i">{{ String(i).padStart(2, '0') }}</span>
            </div>
          </div>
          <p class="classes__strip-caption">{{ stripLabel }}</p>
        </div>
      </div>
    </header>

    <div v-if="loading" class="classes__loading" aria-busy="true">
      <div class="classes__grid">
        <Skeleton v-for="i in 3" :key="i" height="150px" />
      </div>
    </div>

    <EmptyState
      v-else-if="classStore.classes.length === 0"
      icon="user"
      :title="isTeacher ? messages.classes.emptyTeacherTitle : messages.classes.emptyStudentTitle"
      :description="isTeacher ? messages.classes.emptyTeacherDesc : emptyStudentDesc"
      :action-label="isTeacher ? messages.classes.createBtn : messages.classes.joinBtn"
      @action="isTeacher ? (createOpen = true) : (joinOpen = true)"
    />

    <div v-else class="classes__content-wrap">
      <div class="classes__grid">
        <Card
          v-for="cls in pagedClasses"
          :key="cls.id"
          class="classes__card"
          :class="{ 'classes__card--manager': isManagerOf(cls) }"
          role="button"
          tabindex="0"
          :aria-label="cls.name"
          @click="goDetail(cls.id)"
          @keydown.enter="goDetail(cls.id)"
          @keydown.space.prevent="goDetail(cls.id)"
        >
          <div class="classes__card-head">
            <span
              class="classes__card-icon"
              :class="{ 'classes__card-icon--manager': isManagerOf(cls) }"
              aria-hidden="true"
            >
              <GraduationCap v-if="isManagerOf(cls)" :size="18" />
              <UserRound v-else :size="18" />
            </span>
            <div class="classes__card-meta">
              <h3 class="classes__card-name">{{ cls.name }}</h3>
              <p class="classes__card-desc">{{ cls.description || messages.classes.noDescription }}</p>
            </div>
            <Badge :variant="isManagerOf(cls) ? 'primary' : 'muted'">
              {{ isManagerOf(cls) ? messages.classes.roleManager : messages.classes.roleMember }}
            </Badge>
          </div>
          <footer class="classes__card-foot">
            <span class="classes__card-stat">
              <Users :size="13" aria-hidden="true" />
              {{ messages.classes.members(cls.memberCount) }}
            </span>
            <div class="flex items-center gap-2">
              <!-- Nút xem Báo cáo & Thống kê dành cho Giáo viên -->
              <Button
                v-if="isManagerOf(cls)"
                size="sm"
                variant="ghost"
                class="text-xs h-7 gap-1"
                @click.stop="router.push({ name: 'class-report', params: { id: String(cls.id) } })"
              >
                <BarChart3 :size="13" /> Thống kê lớp
              </Button>
              <!-- Mã mời = block-token tối (dữ liệu tuần tự — quyết định #4/#5) -->
              <span v-if="isManagerOf(cls)" class="classes__invite-chip" :title="messages.classes.inviteLabel">
                <KeyRound :size="12" aria-hidden="true" />
                <code>{{ cls.inviteCode }}</code>
              </span>
            </div>
          </footer>
        </Card>
      </div>

      <!-- Phân trang danh sách lớp -->
      <div v-if="totalPages > 1" class="flex items-center justify-center gap-3 pt-6" aria-label="Phân trang danh sách lớp">
        <Button variant="ghost" size="sm" :disabled="page <= 1" @click="page -= 1">
          Trang trước
        </Button>
        <span class="text-xs text-muted-foreground font-mono">Trang {{ page }}/{{ totalPages }}</span>
        <Button variant="ghost" size="sm" :disabled="page >= totalPages" @click="page += 1">
          Trang sau
        </Button>
      </div>
    </div>


    <!-- Modal nhập mã -->
    <Modal :open="joinOpen" :title="messages.classes.joinTitle" @close="joinOpen = false">
      <form novalidate @submit.prevent="join">
        <div class="classes__join-field">
          <Input
            :id="joinInputId"
            v-model="inviteCode"
            :label="messages.classes.joinCodeLabel"
            :placeholder="messages.classes.joinCodePlaceholder"
            :hint="messages.classes.joinCodeHint"
            :icon="KeyRound"
            :maxlength="6"
            autocomplete="off"
            :error="joinError"
            @update:model-value="onInviteInput"
          />
        </div>
        <div class="classes__modal-actions">
          <Button variant="ghost" @click="joinOpen = false">{{ messages.classes.cancel }}</Button>
          <Button type="submit" :disabled="inviteCode.length !== 6">{{ messages.classes.joinSubmit }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal tạo lớp -->
    <Modal :open="createOpen" :title="messages.classes.createTitle" @close="createOpen = false">
      <form class="classes__create" novalidate @submit.prevent="createClass">
        <Input
          v-model="newClassName"
          :label="messages.classes.createNameLabel"
          :placeholder="messages.classes.createNamePlaceholder"
          required
        />
        <Input
          v-model="newClassDesc"
          :label="messages.classes.createDescLabel"
          :placeholder="messages.classes.createDescPlaceholder"
        />
        <div class="classes__modal-actions">
          <Button variant="ghost" @click="createOpen = false">{{ messages.classes.cancel }}</Button>
          <Button type="submit" :loading="creating">{{ messages.classes.createSubmit }}</Button>
        </div>
      </form>
    </Modal>
  </section>
</template>

<style scoped>
.classes {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Banner: surface band level-2 (DESIGN §6) — không gradient, không shadow ── */
.classes__hero {
  border-bottom: 1px solid var(--border-subtle);
  background: var(--card-raised);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.classes__hero-inner {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.classes__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
  flex: 1 1 320px;
}

.classes__hero-badges { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.classes__hero-title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  margin: 0;
  color: var(--foreground);
}

.classes__hero-desc {
  color: var(--foreground-secondary);
  font-size: var(--text-sm);
  max-width: 60ch;
  margin: 0;
}

.classes__hero-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
  margin-top: var(--space-sm);
}

/* ── Mono strip: block-token dữ liệu thật (khoảnh khắc đầu tư duy nhất) ── */
.classes__hero-strip { flex: 0 1 260px; display: flex; flex-direction: column; gap: var(--space-sm); }

.classes__strip-panel {
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.25);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.classes__strip-blocks {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.classes__strip-block {
  height: 28px;
  border-radius: var(--radius-sm);
  background: var(--data-core);
  opacity: 0;
  transform: translateY(6px);
  animation: classes-strip-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
  animation-delay: calc(var(--i) * 45ms + 60ms);
}

.classes__strip-block--empty {
  background: transparent;
  border: 1px dashed var(--data-core);
  opacity: 1;
  transform: none;
  animation: none;
}

.classes__strip-index {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.classes__strip-index span {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  text-align: center;
}

.classes__strip-caption {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  letter-spacing: 0.08em;
  text-align: right;
}

@keyframes classes-strip-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .classes__strip-block {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

/* ── Loading ── */
.classes__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

/* ── Grid lớp học: 1 cột mobile → 2 tablet (≥640) → 3 desktop (≥1024) ──
   (DESIGN §8 card grid + goal 3.6 #3 — thay auto-fill vì auto-fill cho 4 cột ở 1366) */
.classes__grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr);
  gap: var(--space-md);
}

@media (min-width: 640px) {
  .classes__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (min-width: 1024px) {
  .classes__grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: var(--space-lg);
  }
}

/* Card level-1: hover chỉ đổi border → strong (DESIGN §6 — cấm shadow) */
.classes__card {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg);
  cursor: pointer;
  min-width: 0;
  border-color: var(--border);
  transition: border-color 150ms;
}

.classes__card:hover { border-color: var(--border-strong); }

.classes__card:focus-visible {
  outline: 2px solid var(--ring);
  outline-offset: 2px;
}

/* Role cue (goal 3.6 #2): card Quản lý = border-left accent semantic info (token §2.2,
   precedent LeaderboardView) + icon riêng. Khai báo SAU `.classes__card:hover` để
   accent giữ nguyên khi hover (hover chỉ đổi 3 cạnh còn lại sang border-strong). */
.classes__card--manager {
  border-left: 3px solid var(--info);
}

.classes__card--manager:hover {
  border-left-color: var(--info);
}

.classes__card-icon--manager {
  background: color-mix(in srgb, var(--info) 14%, transparent);
  color: var(--info);
}

.classes__card-head { display: flex; align-items: flex-start; gap: var(--space-sm); min-width: 0; }

.classes__card-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.classes__card-meta { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--space-xs); }

.classes__card-name {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.classes__card-desc {
  font-size: var(--text-sm);
  color: var(--foreground-secondary);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.classes__card-foot {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
  border-top: 1px solid var(--border);
  padding-top: var(--space-sm);
}

.classes__card-stat {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  white-space: nowrap;
}

/* ── Invite-code chip: block-token tối (signature Data Bench — quyết định #4/#5) ── */
.classes__invite-chip {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-sm);
  border: 1px solid rgba(66, 85, 255, 0.3);
  background: var(--canvas-ink);
  color: var(--resolved);
  white-space: nowrap;
  min-height: 24px;
}

/* ── Join field (goal 3.6 #1): input mã mời nổi bật — surface level-2
   (card-raised + border-subtle) + ring khi focus (DESIGN §6, precedent SimulatorView) ── */
.classes__join-field {
  border: 1px solid var(--border-subtle);
  background: var(--card-raised);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  transition: border-color 150ms ease, box-shadow 150ms ease;
}

.classes__join-field:focus-within {
  border-color: var(--ring);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--ring) 22%, transparent);
}

/* ── Modal ── */
.classes__modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  margin-top: var(--space-md);
}

.classes__create { display: flex; flex-direction: column; gap: var(--space-sm); }

@media (max-width: 640px) {
  .classes__hero { padding: var(--space-lg); }
  .classes__hero-strip { flex-basis: 100%; }
  .classes__strip-caption { text-align: left; }
}
</style>
