<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  BarChart3,
  BookOpen,
  ClipboardList,
  Crown,
  GraduationCap,
  LayoutDashboard,
  Network,
  ShieldAlert,
  Users,
  Settings,
  Shield,
  ChevronRight,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';

const props = defineProps<{
  activeTab: 'overview' | 'curriculum' | 'exercises' | 'feedback';
}>();

const emit = defineEmits<{
  (e: 'update:activeTab', tab: 'overview' | 'curriculum' | 'exercises' | 'feedback'): void;
}>();

const auth = useAuthStore();
const router = useRouter();

const isAdmin = computed(() => auth.role === 'ADMIN');

const navItems = [
  {
    key: 'overview' as const,
    label: 'Tổng quan Studio',
    icon: LayoutDashboard,
    isTab: true,
  },
  {
    key: 'curriculum' as const,
    label: 'Lộ trình & Bài giảng',
    icon: Network,
    isTab: true,
  },
  {
    key: 'exercises' as const,
    label: 'Quiz & Codelab',
    icon: ClipboardList,
    isTab: true,
  },
  {
    key: 'classes',
    label: 'Lớp học của tôi',
    icon: Users,
    to: '/classes',
    isTab: false,
  },
  {
    key: 'feedback' as const,
    label: 'Báo cáo & Phản hồi',
    icon: ShieldAlert,
    isTab: true,
  },
];

function handleNavClick(item: typeof navItems[0]): void {
  if (item.isTab) {
    emit('update:activeTab', item.key as any);
  } else if (item.to) {
    void router.push(item.to);
  }
}
</script>

<template>
  <div class="studio-shell flex flex-col md:flex-row min-h-[calc(100vh-64px)] bg-[#0b0a12] text-white">
    <!-- Left Sidebar (Width ~260px) -->
    <aside class="w-full md:w-64 bg-vdsa-surface border-r border-vdsa-border p-4 flex flex-col justify-between shrink-0">
      <div class="space-y-6">
        <!-- Brand / Studio Title -->
        <div class="px-2 py-1">
          <div class="flex items-center gap-2">
            <span class="p-2 rounded-xl bg-purple-500/20 text-purple-300">
              <GraduationCap :size="18" />
            </span>
            <div>
              <h2 class="text-sm font-black text-white">Teacher Studio</h2>
              <p class="text-[11px] text-vdsa-muted">{{ isAdmin ? 'Quyền Quản trị viên' : 'Giảng viên chính thức' }}</p>
            </div>
          </div>
        </div>

        <!-- Navigation Menu -->
        <nav class="space-y-1" aria-label="Studio navigation">
          <button
            v-for="item in navItems"
            :key="item.key"
            type="button"
            class="w-full flex items-center justify-between px-3 py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer text-left"
            :class="
              item.isTab && activeTab === item.key
                ? 'bg-vdsa-accent text-white shadow-md shadow-purple-950/40'
                : 'text-slate-400 hover:text-white hover:bg-white/5'
            "
            @click="handleNavClick(item)"
          >
            <div class="flex items-center gap-2.5">
              <component :is="item.icon" :size="16" />
              <span>{{ item.label }}</span>
            </div>
            <ChevronRight v-if="!item.isTab" :size="13" class="opacity-50" />
          </button>
        </nav>

        <!-- Admin Console Link for Admins (Task 22) -->
        <div v-if="isAdmin" class="pt-4 border-t border-vdsa-border space-y-1">
          <span class="text-[10px] uppercase font-bold text-vdsa-muted px-3 block tracking-wider">
            Quản trị viên (Admin)
          </span>
          <button
            type="button"
            class="w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-bold text-slate-300 hover:text-white hover:bg-white/5 cursor-pointer text-left"
            @click="router.push('/admin/users')"
          >
            <Shield :size="15" class="text-amber-400" />
            <span>Quản lý Tài khoản</span>
          </button>
          <button
            type="button"
            class="w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-bold text-slate-300 hover:text-white hover:bg-white/5 cursor-pointer text-left"
            @click="router.push('/admin/stats')"
          >
            <BarChart3 :size="15" class="text-sky-400" />
            <span>Thống kê Hệ thống</span>
          </button>
          <button
            type="button"
            class="w-full flex items-center gap-2.5 px-3 py-2 rounded-xl text-xs font-bold text-slate-300 hover:text-white hover:bg-white/5 cursor-pointer text-left"
            @click="router.push('/admin/settings')"
          >
            <Settings :size="15" class="text-purple-400" />
            <span>Cài đặt Nền tảng</span>
          </button>
        </div>
      </div>

      <!-- Bottom Banner Premium -->
      <div class="mt-6 p-3.5 rounded-2xl bg-gradient-to-br from-purple-900/40 to-indigo-950/60 border border-purple-500/30 space-y-2">
        <div class="flex items-center gap-2">
          <Crown :size="16" class="text-amber-400" />
          <span class="text-xs font-black text-white">DSA Pro Premium</span>
        </div>
        <p class="text-[11px] text-slate-300 leading-relaxed">
          Mở khóa không giới hạn mô phỏng nâng cao và AI giải thuật.
        </p>
        <button
          type="button"
          class="w-full py-1.5 rounded-lg bg-amber-500 hover:bg-amber-400 text-slate-950 text-xs font-black transition-colors cursor-pointer"
          @click="router.push('/premium')"
        >
          Nâng cấp ngay
        </button>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="flex-1 p-6 overflow-y-auto max-w-7xl">
      <slot />
    </main>
  </div>
</template>
