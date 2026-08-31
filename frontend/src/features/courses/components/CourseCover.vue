<template>
  <svg
    :viewBox="VIEW_BOX"
    class="course-cover-svg w-full h-full block"
    preserveAspectRatio="xMidYMid slice"
    role="img"
    :aria-label="`Ảnh bìa lộ trình ${course.title}`"
  >
    <defs>
      <linearGradient :id="gradId" x1="0" y1="0" x2="1" y2="1">
        <stop offset="0%" :stop-color="theme.from" />
        <stop offset="100%" :stop-color="theme.to" />
      </linearGradient>
      <linearGradient :id="`${gradId}-accent`" x1="0" y1="0" x2="0" y2="1">
        <stop offset="0%" stop-color="rgba(255,255,255,0.4)" />
        <stop offset="100%" stop-color="rgba(255,255,255,0.05)" />
      </linearGradient>
    </defs>

    <!-- Nền Gradient phong cách hiện đại -->
    <rect :fill="`url(#${gradId})`" width="100%" height="100%" />
    
    <!-- Họa tiết trừu tượng -->
    <circle cx="85%" cy="20%" r="45%" fill="rgba(255,255,255,0.07)" />
    <circle cx="10%" cy="90%" r="35%" fill="rgba(0,0,0,0.25)" />
    <circle cx="70%" cy="110%" r="30%" fill="rgba(255,255,255,0.05)" />

    <!-- Biểu tượng trọng tâm theo nhóm -->
    <g transform="translate(150 72)">
      <!-- Sorting: Các cột mảng trực quan tăng dần -->
      <g v-if="iconKey === 'sorting'" transform="translate(-60 -30)">
        <rect x="0" y="32" width="18" height="28" rx="4" fill="rgba(255,255,255,0.45)" />
        <rect x="24" y="22" width="18" height="38" rx="4" fill="rgba(255,255,255,0.65)" />
        <rect x="48" y="12" width="18" height="48" rx="4" fill="rgba(255,255,255,0.85)" />
        <rect x="72" y="0" width="18" height="60" rx="4" fill="#FFFFFF" />
        <rect x="96" y="26" width="18" height="34" rx="4" fill="rgba(255,255,255,0.6)" />
      </g>

      <!-- Searching: Kính lúp phóng đại phần tử -->
      <g v-else-if="iconKey === 'searching'" transform="translate(-35 -30)" stroke="#FFFFFF" fill="none" stroke-width="6" stroke-linecap="round">
        <circle cx="28" cy="28" r="22" />
        <line x1="44" y1="44" x2="68" y2="68" stroke-width="8" />
        <circle cx="28" cy="28" r="10" fill="rgba(255,255,255,0.3)" stroke="none" />
      </g>

      <!-- Tree: Cấu trúc cây nhị phân cân đối -->
      <g v-else-if="iconKey === 'tree'" transform="translate(-50 -32)" fill="none" stroke="rgba(255,255,255,0.8)" stroke-width="4">
        <line x1="50" y1="12" x2="25" y2="38" />
        <line x1="50" y1="12" x2="75" y2="38" />
        <line x1="25" y1="38" x2="12" y2="60" />
        <line x1="25" y1="38" x2="38" y2="60" />
        <line x1="75" y1="38" x2="62" y2="60" />
        <line x1="75" y1="38" x2="88" y2="60" />
        <circle cx="50" cy="12" r="10" fill="#FFFFFF" stroke="none" />
        <circle cx="25" cy="38" r="8" fill="rgba(255,255,255,0.9)" stroke="none" />
        <circle cx="75" cy="38" r="8" fill="rgba(255,255,255,0.9)" stroke="none" />
        <circle cx="12" cy="60" r="6" fill="rgba(255,255,255,0.7)" stroke="none" />
        <circle cx="38" cy="60" r="6" fill="rgba(255,255,255,0.7)" stroke="none" />
        <circle cx="62" cy="60" r="6" fill="rgba(255,255,255,0.7)" stroke="none" />
        <circle cx="88" cy="60" r="6" fill="rgba(255,255,255,0.7)" stroke="none" />
      </g>

      <!-- Graph: Đồ thị kết nối đỉnh & cạnh -->
      <g v-else-if="iconKey === 'graph'" transform="translate(-45 -28)" fill="none" stroke="rgba(255,255,255,0.8)" stroke-width="3">
        <polygon points="45,5 85,30 65,70 25,70 5,30" stroke="rgba(255,255,255,0.4)" fill="rgba(255,255,255,0.06)" />
        <line x1="45" y1="5" x2="65" y2="70" />
        <line x1="5" y1="30" x2="85" y2="30" />
        <circle cx="45" cy="5" r="7" fill="#FFFFFF" stroke="none" />
        <circle cx="85" cy="30" r="7" fill="#FFFFFF" stroke="none" />
        <circle cx="65" cy="70" r="7" fill="#FFFFFF" stroke="none" />
        <circle cx="25" cy="70" r="7" fill="#FFFFFF" stroke="none" />
        <circle cx="5" cy="30" r="7" fill="#FFFFFF" stroke="none" />
      </g>

      <!-- Default DSA: Khối token cấu trúc dữ liệu -->
      <g v-else transform="translate(-48 -24)">
        <rect x="0" y="0" width="28" height="28" rx="6" fill="rgba(255,255,255,0.85)" />
        <rect x="34" y="0" width="28" height="28" rx="6" fill="rgba(255,255,255,0.7)" />
        <rect x="68" y="0" width="28" height="28" rx="6" fill="rgba(255,255,255,0.55)" />
        <rect x="0" y="34" width="28" height="28" rx="6" fill="rgba(255,255,255,0.7)" />
        <rect x="34" y="34" width="28" height="28" rx="6" fill="#FFFFFF" />
        <rect x="68" y="34" width="28" height="28" rx="6" fill="rgba(255,255,255,0.7)" />
      </g>
    </g>

    <!-- Nhãn chủ đề tinh tế góc dưới -->
    <text
      x="16"
      y="144"
      font-family="'JetBrains Mono', Consolas, monospace"
      font-size="12"
      font-weight="700"
      fill="rgba(255,255,255,0.7)"
      letter-spacing="1.2"
    >
      {{ badgeLabel }}
    </text>
  </svg>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const VIEW_BOX = '0 0 300 160';

export interface CoverCourse {
  id: string;
  title: string;
  category?: string;
}

const props = defineProps<{
  course: CoverCourse;
}>();

interface CoverTheme {
  from: string;
  to: string;
}

const THEMES: Record<string, CoverTheme> = {
  // Tiếng Việt
  'Sắp xếp': { from: '#4F46E5', to: '#7C3AED' },
  'Tìm kiếm': { from: '#0284C7', to: '#2563EB' },
  'Cấu trúc dữ liệu': { from: '#0D9488', to: '#0284C7' },
  'Giải thuật': { from: '#D97706', to: '#DC2626' },
  'Cơ bản': { from: '#7C3AED', to: '#A855F7' },
  'DSA': { from: '#3B82F6', to: '#8B5CF6' },
  'Đồ thị': { from: '#059669', to: '#0D9488' },
  'Cây': { from: '#10B981', to: '#06B6D4' },
  // Tiếng Anh fallback
  'Sorting': { from: '#4F46E5', to: '#7C3AED' },
  'Searching': { from: '#0284C7', to: '#2563EB' },
  'DataStructure': { from: '#0D9488', to: '#0284C7' },
  'Algorithm': { from: '#D97706', to: '#DC2626' },
  'Tree/Graph': { from: '#059669', to: '#0D9488' },
  'Graph': { from: '#059669', to: '#0D9488' },
};

const DEFAULT_THEME: CoverTheme = { from: '#312E81', to: '#4C1D95' };

const category = computed(() => props.course.category ?? '');
const title = computed(() => props.course.title ?? '');

const theme = computed<CoverTheme>(() => {
  const cat = category.value;
  const tit = title.value;
  if (THEMES[cat]) return THEMES[cat];
  if (tit.includes('Sắp xếp') || cat.includes('Sắp xếp')) return THEMES['Sắp xếp'];
  if (tit.includes('Tìm kiếm') || cat.includes('Tìm kiếm')) return THEMES['Tìm kiếm'];
  if (tit.includes('Cây') || tit.includes('Tree')) return THEMES['Cây'];
  if (tit.includes('Đồ thị') || tit.includes('Graph')) return THEMES['Đồ thị'];
  if (cat.includes('Cấu trúc') || tit.includes('Data Structure')) return THEMES['Cấu trúc dữ liệu'];
  return DEFAULT_THEME;
});

const iconKey = computed(() => {
  const c = category.value.toLowerCase();
  const t = title.value.toLowerCase();
  if (c.includes('sort') || c.includes('sắp xếp') || t.includes('sắp xếp')) return 'sorting';
  if (c.includes('search') || c.includes('tìm kiếm') || t.includes('tìm kiếm')) return 'searching';
  if (c.includes('tree') || c.includes('cây') || t.includes('cây') || t.includes('tree')) return 'tree';
  if (c.includes('graph') || c.includes('đồ thị') || t.includes('đồ thị') || t.includes('graph')) return 'graph';
  return 'dsa';
});

const badgeLabel = computed(() => {
  const c = category.value;
  if (!c) return 'DSA';
  const upper = c.toUpperCase();
  if (upper.length <= 20) return upper;
  return upper.slice(0, 20);
});

const gradId = computed(() => `cover-grad-${String(props.course.id || '').replace(/[^a-zA-Z0-9_-]/g, '') || 'default'}`);
</script>
