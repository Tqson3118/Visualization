<script setup lang="ts">
// ClassesView — Màn 19: danh sách lớp + nhập mã mời 6 ký tự + tạo lớp (Teacher)
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';

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

onMounted(async () => {
  try {
    await classStore.fetchClasses();
  } catch {
    ui.showToast('Không thể tải danh sách lớp.', 'error');
  } finally {
    loading.value = false;
  }
});

function normalizeCode(event: Event): void {
  inviteCode.value = (event.target as HTMLInputElement).value
    .toUpperCase()
    .replace(/[^A-Z0-9]/g, '')
    .slice(0, 6);
}

async function join(): Promise<void> {
  if (inviteCode.value.length !== 6) {
    joinError.value = 'Mã mời phải đủ 6 ký tự.';
    return;
  }
  joinError.value = '';
  try {
    const joined = await classStore.joinClass(inviteCode.value);
    ui.showToast('Đã tham gia lớp!', 'success');
    joinOpen.value = false;
    void router.push({ name: 'class-detail', params: { id: String(joined.id) } });
  } catch (err) {
    joinError.value = err instanceof Error ? err.message : 'Mã mời không hợp lệ hoặc đã hết hạn.';
  }
}

async function createClass(): Promise<void> {
  if (newClassName.value.trim().length < 3) {
    ui.showToast('Tên lớp phải từ 3 ký tự.', 'warning');
    return;
  }
  creating.value = true;
  try {
    const created = await classStore.createClass(newClassName.value.trim(), newClassDesc.value || undefined);
    ui.showToast('Đã tạo lớp! Mã mời: ' + created.inviteCode, 'success');
    createOpen.value = false;
    void router.push({ name: 'class-detail', params: { id: String(created.id) } });
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Tạo lớp thất bại.', 'error');
  } finally {
    creating.value = false;
  }
}
</script>

<template>
  <main class="classes container">
    <header class="classes__header">
      <div>
        <h1 class="classes__title">👥 Lớp học</h1>
        <p class="text-muted classes__sub">Mã mời 6 ký tự · Student nhập mã để tham gia</p>
      </div>
      <div class="classes__actions">
        <Button v-if="isTeacher" size="sm" @click="createOpen = true">+ Tạo lớp mới</Button>
        <Button v-else size="sm" variant="secondary" @click="joinOpen = true">Nhập mã lớp</Button>
      </div>
    </header>

    <div v-if="loading" class="classes__loading">
      <Skeleton v-for="i in 3" :key="i" height="88px" />
    </div>

    <EmptyState
      v-else-if="classStore.classes.length === 0"
      icon="user"
      :title="isTeacher ? 'Chưa có lớp nào' : 'Bạn chưa tham gia lớp nào'"
      :description="isTeacher ? 'Tạo lớp đầu tiên và mời học viên bằng mã 6 ký tự.' : 'Nhập mã mời từ giảng viên để tham gia lớp.'"
      :action-label="isTeacher ? 'Tạo lớp' : 'Nhập mã lớp'"
      @action="isTeacher ? (createOpen = true) : (joinOpen = true)"
    />

    <div v-else class="classes__grid">
      <article
        v-for="cls in classStore.classes"
        :key="cls.id"
        class="classes__card card card--interactive"
        role="button"
        tabindex="0"
        @click="router.push({ name: 'class-detail', params: { id: String(cls.id) } })"
        @keydown.enter="router.push({ name: 'class-detail', params: { id: String(cls.id) } })"
      >
        <header class="classes__card-head">
          <h2 class="classes__card-name">{{ cls.name }}</h2>
          <Badge :variant="cls.role === 'OWNER' || cls.role === 'TEACHER' ? 'primary' : 'muted'">
            {{ cls.role === 'OWNER' || cls.role === 'TEACHER' ? 'Quản lý' : 'Thành viên' }}
          </Badge>
        </header>
        <p class="classes__card-desc text-muted">{{ cls.description || 'Chưa có mô tả' }}</p>
        <footer class="classes__card-foot">
          <span>{{ cls.memberCount }} thành viên</span>
          <span v-if="cls.role === 'OWNER' || cls.role === 'TEACHER'" class="classes__invite">
            Mã mời: <code>{{ cls.inviteCode }}</code>
          </span>
        </footer>
      </article>
    </div>

    <!-- Modal nhập mã -->
    <Modal :open="joinOpen" title="Tham gia lớp" @close="joinOpen = false">
      <form novalidate @submit.prevent="join">
        <Input
          :model-value="inviteCode"
          label="Mã mời (6 ký tự)"
          placeholder="ABC123"
          :maxlength="6"
          :error="joinError"
          @update:model-value="inviteCode = $event"
        />
        <div class="classes__modal-actions">
          <Button variant="ghost" @click="joinOpen = false">Hủy</Button>
          <Button type="submit" :disabled="inviteCode.length !== 6">Tham gia</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal tạo lớp -->
    <Modal :open="createOpen" title="Tạo lớp mới" @close="createOpen = false">
      <form class="classes__create" novalidate @submit.prevent="createClass">
        <Input v-model="newClassName" label="Tên lớp *" placeholder="Lập trình C - Nhóm 2" required />
        <Input v-model="newClassDesc" label="Mô tả" placeholder="Mô tả ngắn về lớp..." />
        <div class="classes__modal-actions">
          <Button variant="ghost" @click="createOpen = false">Hủy</Button>
          <Button type="submit" :loading="creating">Tạo lớp</Button>
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

.classes__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.classes__title { font-size: var(--text-2xl); }
.classes__sub { font-size: var(--text-sm); margin-top: 4px; }

.classes__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--space-md);
}

.classes__card { display: flex; flex-direction: column; gap: var(--space-sm); cursor: pointer; }

.classes__card-head { display: flex; justify-content: space-between; align-items: center; gap: var(--space-sm); }

.classes__card-name { font-size: var(--text-md); }

.classes__card-desc { font-size: var(--text-sm); flex: 1; }

.classes__card-foot {
  display: flex;
  justify-content: space-between;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

.classes__invite code { font-family: var(--font-mono); font-weight: 700; color: var(--color-primary); }

.classes__modal-actions { display: flex; justify-content: flex-end; gap: var(--space-sm); margin-top: var(--space-md); }

.classes__create { display: flex; flex-direction: column; gap: var(--space-sm); }
</style>
