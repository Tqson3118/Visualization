<script setup lang="ts">
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  BarChart3,
  BookOpen,
  ClipboardList,
  GraduationCap,
  LayoutDashboard,
  Network,
  ShieldAlert,
  Users,
  Settings,
  Shield,
  ChevronRight,
  School,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';

const props = withDefaults(
  defineProps<{
    activeTab?: 'overview' | 'curriculum' | 'exercises' | 'feedback' | 'classes' | 'users' | 'stats' | 'settings' | string;
  }>(),
  {
    activeTab: '',
  },
);

const emit = defineEmits<{
  (e: 'update:activeTab', tab: 'overview' | 'curriculum' | 'exercises' | 'feedback'): void;
}>();

const auth = useAuthStore();
const router = useRouter();
const route = useRoute();

const isAdmin = computed(() => auth.role === 'ADMIN');
const isTeacher = computed(() => auth.role === 'TEACHER' || auth.role === 'ADMIN');

interface NavItem {
  key: string;
  label: string;
  icon: any;
  to?: string;
  tabKey?: 'overview' | 'curriculum' | 'exercises' | 'feedback';
}

const teacherNavItems: NavItem[] = [
  {
    key: 'overview',
    label: 'Tổng quan Studio',
    icon: LayoutDashboard,
    tabKey: 'overview',
    to: '/studio?tab=overview',
  },
  {
    key: 'curriculum',
    label: 'Giáo trình & Lộ trình',
    icon: Network,
    tabKey: 'curriculum',
    to: '/studio?tab=curriculum',
  },
  {
    key: 'classes',
    label: 'Quản lý Lớp học',
    icon: School,
    to: '/admin/classes',
  },
  {
    key: 'feedback',
    label: 'Báo cáo & Phản hồi',
    icon: ShieldAlert,
    tabKey: 'feedback',
    to: '/studio?tab=feedback',
  },
];

const adminNavItems: NavItem[] = [
  {
    key: 'users',
    label: 'Quản lý Tài khoản',
    icon: Shield,
    to: '/admin/users',
  },
  {
    key: 'stats',
    label: 'Thống kê Hệ thống',
    icon: BarChart3,
    to: '/admin/stats',
  },
  {
    key: 'settings',
    label: 'Cài đặt Nền tảng',
    icon: Settings,
    to: '/admin/settings',
  },
];

function isItemActive(item: NavItem): boolean {
  if (props.activeTab && props.activeTab === item.key) return true;
  
  if (route.path === '/studio' || route.path === '/admin/content') {
    const currentTab = (route.query.tab as string) || 'overview';
    return item.tabKey === currentTab;
  }
  
  if (item.to && route.path.startsWith(item.to.split('?')[0])) {
    return true;
  }
  
  return false;
}

function handleNavClick(item: NavItem): void {
  if ((route.path === '/studio' || route.path === '/admin/content') && item.tabKey) {
    emit('update:activeTab', item.tabKey);
    void router.replace({ query: { ...route.query, tab: item.tabKey } });
  } else if (item.to) {
    void router.push(item.to);
  }
}
</script>

<template>
  <div class="studio-shell flex flex-col md:flex-row min-h-[calc(100vh-var(--app-header-h,68px))] bg-[#0b0a12] text-white">
    <!-- Left Sidebar (Width ~260px on desktop, horizontal bar on mobile) -->
    <aside class="w-full md:w-64 bg-[#12111a] border-b md:border-b-0 md:border-r border-[#262438] p-3 md:p-4 flex flex-col justify-between shrink-0 shadow-lg">
      <div class="space-y-3 md:space-y-6">
        <!-- Brand / Studio Title -->
        <div class="px-1 md:px-2 py-1 flex items-center justify-between">
          <div class="flex items-center gap-2.5">
            <span class="p-1.5 md:p-2 rounded-xl bg-purple-500/20 text-purple-400 border border-purple-500/30">
              <GraduationCap :size="18" />
            </span>
            <div>
              <h2 class="text-xs md:text-sm font-black text-white tracking-wide">
                {{ isAdmin ? 'Admin & Studio' : 'Teacher Studio' }}
              </h2>
              <p class="text-[10px] md:text-[11px] text-slate-400 font-medium">
                {{ isAdmin ? 'Quyền Quản trị viên' : 'Giảng viên chính thức' }}
              </p>
            </div>
          </div>
        </div>

        <!-- Teacher / Content Section -->
        <div>
          <span class="hidden md:block text-[10px] uppercase font-bold text-slate-400 px-3 tracking-wider mb-2">
            Không gian Giảng dạy
          </span>
          <div class="relative after:pointer-events-none md:after:hidden after:absolute after:right-0 after:top-0 after:bottom-0 after:w-8 after:bg-gradient-to-l after:from-[#12111a] after:to-transparent">
            <nav class="flex md:flex-col gap-1 overflow-x-auto no-scrollbar md:overflow-visible pb-1 md:pb-0 touch-pan-x" role="tablist" aria-label="Studio navigation">
              <button
                v-for="item in teacherNavItems"
                :key="item.key"
                type="button"
                role="tab"
                :aria-selected="isItemActive(item)"
                :data-testid="`tab-${item.key}`"
                class="flex items-center justify-between gap-2 px-3 py-2 md:py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer text-left shrink-0 md:w-full"
                :class="
                  isItemActive(item)
                    ? 'bg-purple-600 text-white shadow-md shadow-purple-950/50'
                    : 'text-slate-400 hover:text-white hover:bg-white/5'
                "
                @click="handleNavClick(item)"
              >
                <div class="flex items-center gap-2 md:gap-2.5 whitespace-nowrap">
                  <component :is="item.icon" :size="16" />
                  <span>{{ item.label }}</span>
                </div>
                <ChevronRight v-if="!isItemActive(item)" :size="13" class="opacity-40 hidden md:block" />
              </button>
            </nav>
          </div>
        </div>

        <!-- Admin Console Link for Admins -->
        <div v-if="isAdmin" class="pt-2 md:pt-4 border-t border-[#262438] space-y-1">
          <span class="hidden md:block text-[10px] uppercase font-bold text-slate-400 px-3 tracking-wider mb-2">
            Hệ thống Quản trị
          </span>
          <div class="relative after:pointer-events-none md:after:hidden after:absolute after:right-0 after:top-0 after:bottom-0 after:w-8 after:bg-gradient-to-l after:from-[#12111a] after:to-transparent">
            <div class="flex md:flex-col gap-1 overflow-x-auto no-scrollbar md:overflow-visible pb-1 md:pb-0 touch-pan-x">
              <button
                v-for="item in adminNavItems"
                :key="item.key"
                type="button"
                class="flex items-center justify-between gap-2 px-3 py-2 md:py-2.5 rounded-xl text-xs font-bold transition-all cursor-pointer text-left shrink-0 md:w-full"
                :class="
                  isItemActive(item)
                    ? 'bg-purple-600 text-white shadow-md shadow-purple-950/50'
                    : 'text-slate-400 hover:text-white hover:bg-white/5'
                "
                @click="handleNavClick(item)"
              >
                <div class="flex items-center gap-2 md:gap-2.5 whitespace-nowrap">
                  <component :is="item.icon" :size="16" />
                  <span>{{ item.label }}</span>
                </div>
                <ChevronRight v-if="!isItemActive(item)" :size="13" class="opacity-40 hidden md:block" />
              </button>
            </div>
          </div>
        </div>
      </div>
    </aside>

    <!-- Main Content Area -->
    <main class="flex-1 p-3 md:p-6 overflow-y-auto max-w-7xl min-w-0">
      <slot />
    </main>
  </div>
</template>
