<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
import {
  BarChart3,
  BookOpen,
  Check,
  Clipboard,
  Copy,
  ExternalLink,
  GraduationCap,
  KeyRound,
  Plus,
  RefreshCw,
  Search,
  Trash2,
  UserPlus,
  Users,
  ShieldAlert,
} from 'lucide-vue-next';

import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { useConfirm } from '@/composables/useConfirm';
import type { ClassDto, ClassMemberDto } from '@/api/types';
import StudioShell from '@/components/studio/StudioShell.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { formatDate } from '@/utils/format';

const classStore = useClassStore();
const auth = useAuthStore();
const ui = useUiStore();
const router = useRouter();
const { confirm: showConfirm } = useConfirm();

const loading = ref(true);
const search = ref('');
const createModalOpen = ref(false);
const memberModalOpen = ref(false);
const activeClassForMembers = ref<ClassDto | null>(null);
const membersLoading = ref(false);
const newMemberEmail = ref('');
const addingMember = ref(false);

const newClassName = ref('');
const newClassDesc = ref('');
const creating = ref(false);
const copiedCode = ref<string | null>(null);

const isAdmin = computed(() => auth.role === 'ADMIN');
const isTeacher = computed(() => auth.role === 'TEACHER' || auth.role === 'ADMIN');

onMounted(loadData);

async function loadData(): Promise<void> {
  loading.value = true;
  try {
    await classStore.fetchClasses();
  } catch (err: any) {
    ui.showToast(err?.message || 'Không thể tải danh sách lớp học', 'error');
  } finally {
    loading.value = false;
  }
}

const filteredClasses = computed(() => {
  const q = search.value.trim().toLowerCase();
  if (!q) return classStore.classes;
  return classStore.classes.filter(
    (c) =>
      c.name.toLowerCase().includes(q) ||
      (c.inviteCode && c.inviteCode.toLowerCase().includes(q)) ||
      (c.description && c.description.toLowerCase().includes(q)),
  );
});

const totalStudents = computed(() =>
  classStore.classes.reduce((sum, c) => sum + (c.memberCount || 0), 0),
);

async function handleCreateClass(): Promise<void> {
  const name = newClassName.value.trim();
  if (!name) {
    ui.showToast('Vui lòng nhập tên lớp học', 'warning');
    return;
  }
  creating.value = true;
  try {
    const created = await classStore.createClass(name, newClassDesc.value.trim() || undefined);
    ui.showToast(`Đã tạo lớp "${created.name}" thành công!`, 'success');
    createModalOpen.value = false;
    newClassName.value = '';
    newClassDesc.value = '';
  } catch (err: any) {
    ui.showToast(err?.message || 'Lỗi tạo lớp học', 'error');
  } finally {
    creating.value = false;
  }
}

async function copyCode(code?: string): Promise<void> {
  if (!code) return;
  try {
    await navigator.clipboard.writeText(code);
    copiedCode.value = code;
    ui.showToast(`Đã sao chép mã mời: ${code}`, 'success');
    setTimeout(() => {
      if (copiedCode.value === code) copiedCode.value = null;
    }, 2500);
  } catch {
    ui.showToast('Không thể sao chép mã mời', 'error');
  }
}

async function openMemberModal(cls: ClassDto): Promise<void> {
  activeClassForMembers.value = cls;
  memberModalOpen.value = true;
  membersLoading.value = true;
  try {
    await classStore.reloadMembers(cls.id);
  } catch (err: any) {
    ui.showToast('Không thể tải danh sách thành viên', 'error');
  } finally {
    membersLoading.value = false;
  }
}

async function handleAddMember(): Promise<void> {
  if (!activeClassForMembers.value || !newMemberEmail.value.trim()) return;
  addingMember.value = true;
  try {
    await classStore.addMember(activeClassForMembers.value.id, newMemberEmail.value.trim());
    ui.showToast('Đã thêm học viên vào lớp', 'success');
    newMemberEmail.value = '';
  } catch (err: any) {
    ui.showToast(err?.message || 'Không thể thêm học viên', 'error');
  } finally {
    addingMember.value = false;
  }
}

async function handleRemoveMember(userId: number): Promise<void> {
  if (!activeClassForMembers.value) return;
  const ok = await showConfirm({
    title: 'Xoá học viên khỏi lớp',
    message: 'Bạn có chắc chắn muốn xoá học viên này khỏi lớp học?',
    confirmLabel: 'Xác nhận xoá',
    variant: 'danger',
  });
  if (!ok) return;

  try {
    await classStore.removeMember(activeClassForMembers.value.id, userId);
    ui.showToast('Đã xoá học viên khỏi lớp', 'success');
  } catch (err: any) {
    ui.showToast(err?.message || 'Không thể xoá học viên', 'error');
  }
}

async function handleDeleteClass(cls: ClassDto): Promise<void> {
  const ok = await showConfirm({
    title: `Xoá lớp "${cls.name}"`,
    message: 'Bạn có chắc muốn xoá lớp học này? Toàn bộ dữ liệu thành viên và bài tập giao sẽ bị huỷ.',
    confirmLabel: 'Xoá vĩnh viễn',
    variant: 'danger',
  });
  if (!ok) return;

  try {
    await classStore.removeClass(cls.id);
    ui.showToast(`Đã xoá lớp "${cls.name}"`, 'success');
  } catch (err: any) {
    ui.showToast(err?.message || 'Lỗi xoá lớp', 'error');
  }
}
</script>

<template>
  <StudioShell active-tab="classes">
    <div class="space-y-6">
      <!-- Header Bar -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-[#14121f] p-5 rounded-2xl border border-[#262438]">
        <div>
          <div class="flex items-center gap-2.5">
            <h1 class="text-xl font-black text-white">Quản lý Lớp học</h1>
            <Badge variant="primary" class="text-xs font-bold">{{ classStore.classes.length }} lớp</Badge>
          </div>
          <p class="text-xs text-slate-400 mt-1">
            Trung tâm quản lý, theo dõi học sinh, cấp mã tham gia và xuất báo cáo tiến độ giảng dạy.
          </p>
        </div>

        <div class="flex items-center gap-3">
          <button
            type="button"
            class="flex items-center gap-2 px-3.5 py-2 rounded-xl bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white text-xs font-bold transition-colors cursor-pointer border border-[#262438]"
            @click="loadData"
          >
            <RefreshCw :size="14" :class="{ 'animate-spin': loading }" />
            <span>Làm mới</span>
          </button>

          <button
            v-if="isTeacher"
            type="button"
            class="flex items-center gap-2 px-4 py-2 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold transition-all shadow-md shadow-purple-950/50 cursor-pointer"
            @click="createModalOpen = true"
          >
            <Plus :size="15" />
            <span>Tạo Lớp Mới</span>
          </button>
        </div>
      </div>

      <!-- KPI Summary Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div class="p-4 rounded-2xl bg-[#14121f] border border-[#262438] flex items-center gap-4">
          <div class="p-3 rounded-xl bg-purple-500/10 text-purple-400 border border-purple-500/20">
            <GraduationCap :size="24" />
          </div>
          <div>
            <p class="text-xs font-bold text-slate-400">Tổng số Lớp học</p>
            <p class="text-2xl font-black text-white mt-0.5">{{ classStore.classes.length }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-[#14121f] border border-[#262438] flex items-center gap-4">
          <div class="p-3 rounded-xl bg-sky-500/10 text-sky-400 border border-sky-500/20">
            <Users :size="24" />
          </div>
          <div>
            <p class="text-xs font-bold text-slate-400">Tổng số Học viên</p>
            <p class="text-2xl font-black text-white mt-0.5">{{ totalStudents }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-[#14121f] border border-[#262438] flex items-center gap-4">
          <div class="p-3 rounded-xl bg-emerald-500/10 text-emerald-400 border border-emerald-500/20">
            <KeyRound :size="24" />
          </div>
          <div>
            <p class="text-xs font-bold text-slate-400">Lớp đang Hoạt động</p>
            <p class="text-2xl font-black text-white mt-0.5">{{ classStore.classes.length }}</p>
          </div>
        </div>
      </div>

      <!-- Search & Filters -->
      <div class="flex items-center gap-3">
        <div class="relative flex-1 max-w-md">
          <Search :size="15" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            v-model="search"
            type="text"
            placeholder="Tìm kiếm theo tên lớp, mã mời..."
            class="w-full pl-9 pr-4 py-2 rounded-xl bg-[#14121f] border border-[#262438] text-xs text-white placeholder-slate-400 focus:outline-none focus:border-purple-500 transition-colors"
          />
        </div>
      </div>

      <!-- Loading Skeleton -->
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div v-for="i in 6" :key="i" class="p-5 rounded-2xl bg-[#14121f] border border-[#262438] space-y-4">
          <Skeleton class="h-6 w-3/4" />
          <Skeleton class="h-4 w-full" />
          <Skeleton class="h-10 w-full" />
        </div>
      </div>

      <!-- Empty State -->
      <EmptyState
        v-else-if="filteredClasses.length === 0"
        icon="school"
        title="Chưa có lớp học nào"
        description="Bắt đầu tạo lớp học đầu tiên để mời học sinh và phân công bài tập giảng dạy."
      >
        <template #actions>
          <button
            type="button"
            class="px-4 py-2 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold transition-colors cursor-pointer"
            @click="createModalOpen = true"
          >
            Tạo Lớp Ngay
          </button>
        </template>
      </EmptyState>

      <!-- Classes Grid -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div
          v-for="cls in filteredClasses"
          :key="cls.id"
          class="p-5 rounded-2xl bg-[#14121f] border border-[#262438] hover:border-purple-500/40 transition-all flex flex-col justify-between space-y-4 group shadow-md"
        >
          <div class="space-y-3">
            <div class="flex items-start justify-between gap-2">
              <h3 class="font-bold text-white text-sm group-hover:text-purple-300 transition-colors line-clamp-1">
                {{ cls.name }}
              </h3>
              <Badge variant="muted" class="text-[10px] shrink-0">ID: {{ cls.id }}</Badge>
            </div>

            <p class="text-xs text-slate-400 line-clamp-2 min-h-[32px]">
              {{ cls.description || 'Không có mô tả cho lớp học này.' }}
            </p>

            <!-- Invite Code Badge & Copy Button -->
            <div class="p-2.5 rounded-xl bg-black/40 border border-[#262438] flex items-center justify-between">
              <div class="flex items-center gap-2">
                <KeyRound :size="13" class="text-amber-400" />
                <span class="text-[11px] font-semibold text-slate-400">Mã mời:</span>
                <span class="font-mono font-bold text-xs text-amber-300 tracking-wider">
                  {{ cls.inviteCode || 'N/A' }}
                </span>
              </div>
              <button
                type="button"
                class="p-1 rounded-lg hover:bg-white/10 text-slate-400 hover:text-white transition-colors cursor-pointer"
                :title="copiedCode === cls.inviteCode ? 'Đã sao chép' : 'Sao chép mã'"
                @click.stop="copyCode(cls.inviteCode)"
              >
                <Check v-if="copiedCode === cls.inviteCode" :size="14" class="text-emerald-400" />
                <Copy v-else :size="14" />
              </button>
            </div>

            <!-- Members & Meta -->
            <div class="flex items-center justify-between text-xs text-slate-400 pt-1">
              <div class="flex items-center gap-1.5">
                <Users :size="13" class="text-sky-400" />
                <span>{{ cls.memberCount || 0 }} học viên</span>
              </div>
              <span class="text-[11px]">{{ formatDate(cls.createdAt) }}</span>
            </div>
          </div>

          <!-- Action Buttons -->
          <div class="pt-3 border-t border-[#262438] flex items-center justify-between gap-2">
            <div class="flex items-center gap-1.5">
              <button
                type="button"
                class="p-2 rounded-xl bg-white/5 hover:bg-sky-500/20 text-slate-300 hover:text-sky-400 transition-colors cursor-pointer border border-[#262438]"
                title="Quản lý thành viên"
                @click="openMemberModal(cls)"
              >
                <Users :size="14" />
              </button>

              <button
                type="button"
                class="p-2 rounded-xl bg-white/5 hover:bg-purple-500/20 text-slate-300 hover:text-purple-400 transition-colors cursor-pointer border border-[#262438]"
                title="Xem Báo cáo nộp bài"
                @click="router.push(`/classes/${cls.id}/report`)"
              >
                <BarChart3 :size="14" />
              </button>

              <button
                type="button"
                class="p-2 rounded-xl bg-white/5 hover:bg-red-500/20 text-slate-300 hover:text-red-400 transition-colors cursor-pointer border border-[#262438]"
                title="Xoá lớp học"
                @click="handleDeleteClass(cls)"
              >
                <Trash2 :size="14" />
              </button>
            </div>

            <button
              type="button"
              class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-purple-600/20 hover:bg-purple-600 text-purple-300 hover:text-white text-xs font-bold transition-all cursor-pointer border border-purple-500/30"
              @click="router.push(`/classes/${cls.id}`)"
            >
              <span>Vào lớp</span>
              <ExternalLink :size="12" />
            </button>
          </div>
        </div>
      </div>

      <!-- Modal Tạo Lớp Học Mới -->
      <Modal
        :open="createModalOpen"
        title="Tạo Lớp Học Mới"
        description="Hệ thống sẽ tự động tạo mã mời 6 ký tự để học sinh tham gia."
        @close="createModalOpen = false"
      >
        <form @submit.prevent="handleCreateClass" class="space-y-4">
          <div>
            <label class="block text-xs font-bold text-slate-300 mb-1.5">Tên lớp học *</label>
            <Input
              v-model="newClassName"
              placeholder="VD: Cấu trúc Dữ liệu & Giải thuật K18"
              required
            />
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-300 mb-1.5">Mô tả / Ghi chú</label>
            <textarea
              v-model="newClassDesc"
              rows="3"
              placeholder="Nhập thông tin giới thiệu lớp học hoặc lịch trình..."
              class="w-full px-3 py-2 rounded-xl bg-[#0b0a12] border border-[#262438] text-xs text-white placeholder-slate-400 focus:outline-none focus:border-purple-500"
            ></textarea>
          </div>

          <div class="flex items-center justify-end gap-2 pt-2">
            <Button variant="ghost" type="button" @click="createModalOpen = false">Huỷ</Button>
            <Button variant="primary" type="submit" :loading="creating">Tạo lớp</Button>
          </div>
        </form>
      </Modal>

      <!-- Modal Quản Lý Thành Viên Lớp -->
      <Modal
        :open="memberModalOpen"
        :title="`Danh sách thành viên: ${activeClassForMembers?.name || ''}`"
        @close="memberModalOpen = false"
      >
        <div class="space-y-4">
          <!-- Add Member by Email -->
          <form @submit.prevent="handleAddMember" class="flex items-center gap-2">
            <Input
              v-model="newMemberEmail"
              type="email"
              placeholder="Nhập email học viên cần thêm..."
              class="flex-1"
              required
            />
            <Button variant="primary" type="submit" :loading="addingMember" class="shrink-0">
              <UserPlus :size="14" class="mr-1.5" />
              Thêm
            </Button>
          </form>

          <!-- Members List -->
          <div v-if="membersLoading" class="space-y-2 py-4">
            <Skeleton v-for="i in 3" :key="i" class="h-10 w-full" />
          </div>

          <div v-else-if="classStore.members.length === 0" class="py-6 text-center text-xs text-slate-400">
            Chưa có học viên nào trong lớp. Hãy chia sẻ mã mời
            <strong class="text-amber-300 font-mono">{{ activeClassForMembers?.inviteCode }}</strong>
            hoặc thêm qua email ở trên.
          </div>

          <div v-else class="max-h-64 overflow-y-auto space-y-2 pr-1">
            <div
              v-for="member in classStore.members"
              :key="member.userId"
              class="p-2.5 rounded-xl bg-[#0b0a12] border border-[#262438] flex items-center justify-between"
            >
              <div class="flex items-center gap-2.5">
                <div class="w-7 h-7 rounded-full bg-purple-500/20 text-purple-300 flex items-center justify-center font-bold text-xs">
                  {{ (member.displayName || member.email || '?').charAt(0).toUpperCase() }}
                </div>
                <div>
                  <p class="text-xs font-bold text-white">{{ member.displayName || member.email }}</p>
                  <p class="text-[10px] text-slate-400">{{ member.email }}</p>
                </div>
              </div>

              <button
                type="button"
                class="p-1.5 rounded-lg hover:bg-red-500/20 text-slate-400 hover:text-red-400 transition-colors cursor-pointer"
                title="Xoá học viên"
                @click="handleRemoveMember(member.userId)"
              >
                <Trash2 :size="13" />
              </button>
            </div>
          </div>
        </div>
      </Modal>
    </div>
  </StudioShell>
</template>
