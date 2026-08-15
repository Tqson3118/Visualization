<script setup lang="ts">
// ClassesView — Màn 19: danh sách lớp + nhập mã mời 6 ký tự + tạo lớp (Teacher)
// View-quality Phase 1 (Nhóm D): banner = surface band level-2 + mono strip
// block-token dữ liệu thật (DESIGN §1/#1); card level-1 không shadow (hover chỉ
// đổi border); mã mời = block-token tối canvas-ink (quyết định #4/#5).
import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { Check, GraduationCap, KeyRound, Plus, UserRound } from 'lucide-vue-next';

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
import PageHero from '@/components/ui/PageHero.vue';
import AdminHeroStrip from '@/components/admin/AdminHeroStrip.vue';
import StatCard from '@/components/ui/StatCard.vue';

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

// ── Mã mời 1-click copy (Task 2): click block-token → icon Check ~1.5s rồi revert.
// Lưu theo chính mã (không theo index) để nhiều card không ghi đè trạng thái nhau.
const copiedCode = ref<string | null>(null);
let copyTimer: ReturnType<typeof setTimeout> | undefined;

function copyInvite(code: string): void {
  void navigator.clipboard?.writeText(code).then(() => {
    copiedCode.value = code;
    if (copyTimer) clearTimeout(copyTimer);
    copyTimer = setTimeout(() => {
      copiedCode.value = null;
    }, 1500);
  }).catch(() => {
    // FIX REVIEW: clipboard từ chối/không khả dụng → bỏ qua, không hiện trạng thái copy giả.
  });
}

onUnmounted(() => {
  if (copyTimer) clearTimeout(copyTimer);
});

/** Strip banner: block-token dữ liệu thật — số lớp (tối đa 5 block) + index mono
    (AdminHeroStrip tự clamp count 1..5 — count=0 → 1 block empty). */
const stripLabel = computed(() => {
  const total = classStore.classes.length;
  const members = classStore.classes.reduce((sum, cls) => sum + cls.memberCount, 0);
  return messages.classes.stripLabel(total, members);
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
  <main class="classes container">
    <!-- Banner: PageHero shared (surface band level-2 — DESIGN §1/#1, không gradient) -->
    <PageHero
      :badge="messages.classes.badge"
      :title="messages.classes.title"
      :description="messages.classes.subtitle"
    >
      <template #actions>
        <Button v-if="isTeacher" size="lg" @click="createOpen = true">
          <Plus :size="16" aria-hidden="true" /> {{ messages.classes.createBtn }}
        </Button>
        <Button v-else size="lg" @click="joinOpen = true">
          <KeyRound :size="16" aria-hidden="true" /> {{ messages.classes.joinBtn }}
        </Button>
      </template>
      <!-- Mono strip: block-token dữ liệu thật (số lớp) + index mono (quyết định #4) -->
      <template #side>
        <AdminHeroStrip :count="classStore.classes.length" :label="stripLabel" />
      </template>
    </PageHero>

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
      <!-- Panel tóm tắt nhanh chỉ số lớp học (Task 2) -->
      <div class="classes__summary-bar">
        <div class="classes__summary-item">
          <span class="classes__summary-label">Tổng số lớp</span>
          <span class="classes__summary-val">{{ classStore.classes.length }}</span>
        </div>
        <span class="classes__summary-dot" aria-hidden="true" />
        <div class="classes__summary-item">
          <span class="classes__summary-label">Tổng học viên</span>
          <span class="classes__summary-val">{{ classStore.classes.reduce((sum, c) => sum + c.memberCount, 0) }}</span>
        </div>
        <span class="classes__summary-dot" aria-hidden="true" />
        <div class="classes__summary-item">
          <span class="classes__summary-label">Đang quản lý</span>
          <span class="classes__summary-val">{{ classStore.classes.filter((c) => isManagerOf(c)).length }}</span>
        </div>
      </div>

      <div class="classes__grid">
      <Card
        v-for="cls in classStore.classes"
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
          <!-- StatCard mini (level default — Task 2): chỉ số thật từ API (memberCount) -->
          <StatCard class="classes__stat" :label="messages.classes.statMembers" :value="cls.memberCount" />
          <!-- Mã mời = MONO BLOCK 1-click copy (chỉ manager — giữ nguyên gating quyền
               của chip cũ): block-token tối canvas-ink + icon Check khi đã copy -->
          <Button
            v-if="isManagerOf(cls)"
            variant="ghost"
            class="classes__invite"
            :class="{ 'classes__invite--copied': copiedCode === cls.inviteCode }"
            :title="copiedCode === cls.inviteCode ? messages.classes.inviteCopied : messages.classes.inviteCopyHint"
            :aria-label="copiedCode === cls.inviteCode ? messages.classes.inviteCopied : messages.classes.inviteCopyHint"
            @click.stop="copyInvite(cls.inviteCode)"
          >
            <Check v-if="copiedCode === cls.inviteCode" :size="14" aria-hidden="true" />
            <KeyRound v-else :size="14" aria-hidden="true" />
            <code class="classes__invite-code">{{ cls.inviteCode }}</code>
          </Button>
        </footer>
      </Card>
    </div>
  </div>

    <!-- Modal nhập mã -->
    <Modal :open="joinOpen" :title="messages.classes.joinTitle" @close="joinOpen = false">
      <form novalidate @submit.prevent="join">
        <div class="classes__join-field">
          <Input
            :id="joinInputId"
            :model-value="inviteCode"
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
  </main>
</template>

<style scoped>
.classes {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 68rem;
  margin-inline: auto;
  width: 100%;
}

.classes__content-wrap {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* ── Panel tóm tắt chỉ số lớp (Task 2) ── */
.classes__summary-bar {
  display: inline-flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-sm) var(--space-md);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  align-self: flex-start;
}

.classes__summary-item {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-xs);
}

.classes__summary-label {
  color: var(--color-text-secondary);
}

.classes__summary-val {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--color-foreground);
}

.classes__summary-dot {
  width: 4px;
  height: 4px;
  border-radius: var(--radius-full);
  background: var(--color-border);
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
    gap: var(--space-lg);
  }
}

@media (min-width: 1024px) {
  .classes__grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: var(--space-lg);
  }
}

/* Card level-1: hover/active phản hồi rõ (FIX R1) — border → strong + nền surface-hover,
   active = nền tint primary 8% (press feedback); KHÔNG shadow/scale (§6). */
.classes__card {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg);
  cursor: pointer;
  min-width: 0;
  border-color: var(--border);
  transition: border-color 150ms ease, background-color 150ms ease;
}

.classes__card:hover {
  border-color: var(--border-strong);
  background: var(--color-surface-hover);
}

.classes__card:active {
  background: color-mix(in srgb, var(--primary) 8%, var(--card));
}

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
  align-items: flex-end;
  gap: var(--space-sm);
  flex-wrap: wrap;
  border-top: 1px solid var(--border);
  padding-top: var(--space-md);
}

/* ── StatCard mini (Task 2): stat phụ level-1 trong card — gỡ border/bg/padding của
   Card gốc, chỉ giữ label mono + value; KHÔNG thêm shadow (DESIGN §6) ── */
.classes__stat { flex: 1; min-width: 120px; }

.classes__stat :deep(.stat-card) {
  background: transparent;
  border: none;
  padding: 0;
}

.classes__stat :deep(.stat-card--default .stat-card__head) {
  padding: 0;
  gap: var(--space-xs);
}

.classes__stat :deep(.stat-card--default .stat-card__label) {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  color: var(--foreground-secondary);
}

.classes__stat :deep(.stat-card--default .stat-card__body) {
  padding: var(--space-xs) 0 0;
  gap: 0;
}

.classes__stat :deep(.stat-card--default .stat-card__value) {
  font-size: var(--text-md);
  line-height: 1.4;
}

/* ── Invite-code MONO BLOCK 1-click copy (Task 2): block-token tối canvas-ink ── */
.classes__invite {
  height: auto;
  min-height: 32px;
  gap: var(--space-sm);
  padding: var(--space-xs) 12px;
  border: 1px solid rgba(66, 85, 255, 0.3);
  border-radius: var(--radius-md);
  background: var(--canvas-ink);
  color: var(--resolved);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  white-space: nowrap;
  transition: border-color 150ms ease, background-color 150ms ease;
}

.classes__invite:hover {
  border-color: rgba(66, 85, 255, 0.6);
  background: var(--canvas-ink);
  color: var(--resolved);
}

.classes__invite:focus-visible {
  outline: 2px solid var(--ring);
  outline-offset: 2px;
}

.classes__invite--copied,
.classes__invite--copied:hover {
  border-color: rgba(52, 211, 153, 0.5);
  color: var(--resolved);
}

.classes__invite-code {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.12em;
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
</style>
