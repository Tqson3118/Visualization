<script setup lang="ts">
// ClassesView — Màn 19: danh sách lớp + nhập mã mời 6 ký tự + tạo lớp (Teacher)
// H-C: hero gradient Sunset (vùng học tập) + Card shadcn + invite-code chip + modal giữ logic.
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import { GraduationCap, KeyRound, Plus, Users } from 'lucide-vue-next';

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

const isTeacher = computed(() => auth.role === 'TEACHER' || auth.role === 'ADMIN');

const isManagerOf = (cls: ClassDto): boolean => cls.role === 'OWNER' || cls.role === 'TEACHER';

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

async function join(): Promise<void> {
  if (inviteCode.value.length !== 6) {
    joinError.value = messages.classes.joinCodeError;
    return;
  }
  joinError.value = '';
  try {
    const joined = await classStore.joinClass(inviteCode.value);
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
    <!-- Hero gradient Sunset (vùng học tập — cùng pattern LessonView) -->
    <header class="classes__hero">
      <div class="classes__hero-badges">
        <Badge variant="primary">{{ messages.classes.badge }}</Badge>
      </div>
      <h1 class="classes__hero-title">{{ messages.classes.title }}</h1>
      <p class="classes__hero-desc">{{ messages.classes.subtitle }}</p>
      <div class="classes__hero-actions">
        <Button v-if="isTeacher" size="md" @click="createOpen = true">
          <Plus :size="16" aria-hidden="true" /> {{ messages.classes.createBtn }}
        </Button>
        <Button v-else size="md" @click="joinOpen = true">
          <KeyRound :size="16" aria-hidden="true" /> {{ messages.classes.joinBtn }}
        </Button>
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
      :description="isTeacher ? messages.classes.emptyTeacherDesc : messages.classes.emptyStudentDesc"
      :action-label="isTeacher ? messages.classes.createBtn : messages.classes.joinBtn"
      @action="isTeacher ? (createOpen = true) : (joinOpen = true)"
    />

    <div v-else class="classes__grid">
      <Card
        v-for="cls in classStore.classes"
        :key="cls.id"
        class="classes__card hover-lift"
        role="button"
        tabindex="0"
        :aria-label="cls.name"
        @click="router.push({ name: 'class-detail', params: { id: String(cls.id) } })"
        @keydown.enter="router.push({ name: 'class-detail', params: { id: String(cls.id) } })"
      >
        <div class="classes__card-head">
          <span class="classes__card-icon" aria-hidden="true"><GraduationCap :size="18" /></span>
          <div class="classes__card-meta">
            <h2 class="classes__card-name">{{ cls.name }}</h2>
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
          <span v-if="isManagerOf(cls)" class="classes__invite-chip" :title="messages.classes.inviteLabel">
            <KeyRound :size="12" aria-hidden="true" />
            <code>{{ cls.inviteCode }}</code>
          </span>
        </footer>
      </Card>
    </div>

    <!-- Modal nhập mã -->
    <Modal :open="joinOpen" :title="messages.classes.joinTitle" @close="joinOpen = false">
      <form novalidate @submit.prevent="join">
        <Input
          :model-value="inviteCode"
          :label="messages.classes.joinCodeLabel"
          :placeholder="messages.classes.joinCodePlaceholder"
          :hint="messages.classes.joinCodeHint"
          :maxlength="6"
          :error="joinError"
          @update:model-value="onInviteInput"
        />
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
}

/* ── Hero gradient Sunset (cùng pattern LessonView — GP-T9b dark overlay) ── */
.classes__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-xl);
  border-radius: var(--radius-xl);
  background-image: var(--gradient-sunset);
  color: #fff;
  box-shadow: var(--shadow-lg);
}

.classes__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: linear-gradient(120deg, rgba(255, 255, 255, 0.16), transparent 55%);
}

.dark .classes__hero::after {
  background: rgba(4, 47, 46, 0.62);
}

.classes__hero-badges { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.classes__hero-title {
  font-size: var(--text-3xl);
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.classes__hero-desc {
  color: rgba(255, 255, 255, 0.92);
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

/* ── Loading ── */
.classes__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

/* ── Grid lớp học ── */
.classes__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--space-md);
}

.classes__card {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg);
  cursor: pointer;
  min-width: 0;
}

.classes__card-head { display: flex; align-items: flex-start; gap: var(--space-sm); min-width: 0; }

.classes__card-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background-image: var(--gradient-sunset);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}

.classes__card-meta { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }

.classes__card-name {
  font-size: var(--text-md);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.classes__card-desc {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
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
  border-top: 1px dashed var(--color-border);
  padding-top: var(--space-sm);
}

.classes__card-stat {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  white-space: nowrap;
}

/* ── Invite-code chip (signature của nhóm màn lớp học) ── */
.classes__invite-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 700;
  letter-spacing: 0.06em;
  padding: 3px 10px;
  border-radius: var(--radius-sm);
  border: 1px dashed color-mix(in srgb, var(--color-primary) 50%, var(--color-border));
  background: color-mix(in srgb, var(--color-primary) 8%, transparent);
  color: var(--color-primary);
  white-space: nowrap;
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
}
</style>
