<script setup lang="ts">
// HomeView — Trang chủ tích hợp trọn vẹn UI & Hệ thống Animation của Source 2 (VisualizationDSA1).
// Bao gồm:
// 1. Mesh Blobs động đa tầng & Glow Pulse xoay chuyển mượt mà
// 2. QuickSort Preview Animation 9 phases với transition & glow màu sắc Pivot/Compare/Swap/Sorted
// 3. Bento Grid 4 cột với 3D Tilt, Icon bounce & Visual Sorting Bars Wave Animation
// 4. GSAP Animated Numbers (XP Counter, Streak Counter, Trust Indicators)
// 5. Codelab Monaco syntax highlight & Testcases Pass Glow
// 6. AI Mentor Chat Bubbles & Floating Sparkles
// 7. Roadmap Nodes Pulse & Hover slide
// 8. Testimonials Slider với Dot glow
// 9. Interactive Live Sandbox với Active / Sorted Bar Jumping Animations
// 10. Dashboard Animated XP Wheel SVG & Ambient Floating Trophy

import { computed, nextTick, onMounted, onUnmounted, ref, shallowRef, watch } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { gsap } from 'gsap';
import { ScrollTrigger } from 'gsap/ScrollTrigger';
import { SplitText } from 'gsap/SplitText';
import {
  Activity,
  ArrowRight,
  ArrowUpDown,
  Award,
  BookOpen,
  Boxes,
  Check,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  Code2,
  Compass,
  Crown,
  Eye,
  Flame,
  FlaskConical,
  Gauge,
  Gem,
  GitFork,
  Heart,
  Layers,
  Lock,
  Network,
  Pause,
  Play,
  RotateCcw,
  Search,
  ShieldCheck,
  Sparkles,
  Star,
  StepForward,
  Target,
  Terminal,
  Trophy,
  User,
  X,
  Zap,
} from 'lucide-vue-next';

import type { Component } from 'vue';
import type { InputConfig, Step } from '@/engines/core/types';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { CATALOG } from '@/engines/catalog';
import type { CatalogMeta } from '@/engines/catalog';
import { getSimulation } from '@/engines/registry';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import { buttonVariants } from '@/components/ui/button';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import { useUiStore } from '@/stores/ui';

gsap.registerPlugin(ScrollTrigger, SplitText);

const router = useRouter();
const authStore = useAuthStore();
const gamificationStore = useGamificationStore();
const progressStore = useProgressStore();
const uiStore = useUiStore();

/* ── Template Refs ── */
const homeRef = ref<HTMLElement | null>(null);
const heroTitleRef = ref<HTMLElement | null>(null);

/* ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   1. GUEST / LANDING PRESENTATION STATE & ANIMATIONS
   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ */

/* ── 3 Demos công khai ── */
const DEMO_ICONS: Record<string, Component> = {
  'sort.bubble': ArrowUpDown,
  'search.binary': Search,
  'graph.bfs': Network,
};

const demos = computed(() =>
  CATALOG.filter((c) => c.demoAllowed).map((c) => ({
    key: c.key,
    title: c.title,
    dataStructure: c.dataStructure,
    level: c.level,
    complexity: c.complexity,
    icon: DEMO_ICONS[c.key] ?? Play,
  })),
);

const stats = computed(() => ({
  visuals: CATALOG.length,
  groups: new Set(CATALOG.map((c) => c.dataStructure)).size,
  levels: new Set(CATALOG.map((c) => c.level)).size,
}));

function openDemo(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

/* ── QuickSort Preview Animation (9 Phases từ Source 2) ── */
interface PreviewBar {
  height: number;
  label: string;
  cls: '' | 'pivot' | 'compare' | 'swap' | 'sorted';
}

const QUICK_SORT_PHASES: PreviewBar[][] = [
  // 0 — khởi tạo, chọn pivot (vàng)
  [{ height: 80, label: '8', cls: '' }, { height: 30, label: '3', cls: '' }, { height: 90, label: '9', cls: '' }, { height: 40, label: '4', cls: '' }, { height: 70, label: '7', cls: '' }, { height: 50, label: '5', cls: 'pivot' }],
  // 1 — so sánh cặp (tím)
  [{ height: 80, label: '8', cls: 'compare' }, { height: 30, label: '3', cls: 'compare' }, { height: 90, label: '9', cls: '' }, { height: 40, label: '4', cls: '' }, { height: 70, label: '7', cls: '' }, { height: 50, label: '5', cls: 'pivot' }],
  // 2 — swap (đỏ)
  [{ height: 30, label: '3', cls: 'swap' }, { height: 80, label: '8', cls: 'swap' }, { height: 90, label: '9', cls: '' }, { height: 40, label: '4', cls: '' }, { height: 70, label: '7', cls: '' }, { height: 50, label: '5', cls: 'pivot' }],
  // 3 — so sánh cặp tiếp (tím)
  [{ height: 30, label: '3', cls: '' }, { height: 80, label: '8', cls: '' }, { height: 90, label: '9', cls: 'compare' }, { height: 40, label: '4', cls: 'compare' }, { height: 70, label: '7', cls: '' }, { height: 50, label: '5', cls: 'pivot' }],
  // 4 — swap tiếp (đỏ)
  [{ height: 30, label: '3', cls: '' }, { height: 80, label: '8', cls: '' }, { height: 40, label: '4', cls: 'swap' }, { height: 90, label: '9', cls: 'swap' }, { height: 70, label: '7', cls: '' }, { height: 50, label: '5', cls: 'pivot' }],
  // 5 — pivot hạ cánh đúng vị trí → vùng sorted (xanh)
  [{ height: 30, label: '3', cls: 'sorted' }, { height: 40, label: '4', cls: '' }, { height: 50, label: '5', cls: 'pivot' }, { height: 70, label: '7', cls: '' }, { height: 80, label: '8', cls: '' }, { height: 90, label: '9', cls: 'sorted' }],
  // 6 — so sánh phần trái (tím)
  [{ height: 30, label: '3', cls: 'sorted' }, { height: 40, label: '4', cls: 'compare' }, { height: 50, label: '5', cls: 'compare' }, { height: 70, label: '7', cls: '' }, { height: 80, label: '8', cls: '' }, { height: 90, label: '9', cls: 'sorted' }],
  // 7 — swap phần trái (đỏ)
  [{ height: 30, label: '3', cls: 'sorted' }, { height: 50, label: '5', cls: 'swap' }, { height: 40, label: '4', cls: 'swap' }, { height: 70, label: '7', cls: '' }, { height: 80, label: '8', cls: '' }, { height: 90, label: '9', cls: 'sorted' }],
  // 8 — toàn bộ sorted (xanh)
  [{ height: 30, label: '3', cls: 'sorted' }, { height: 40, label: '4', cls: 'sorted' }, { height: 50, label: '5', cls: 'sorted' }, { height: 70, label: '7', cls: 'sorted' }, { height: 80, label: '8', cls: 'sorted' }, { height: 90, label: '9', cls: 'sorted' }],
];

const phaseIndex = ref(0);
const isPreviewPlaying = ref(true);
let previewTimer: ReturnType<typeof setInterval> | null = null;

const currentPhase = computed(() => QUICK_SORT_PHASES[phaseIndex.value] ?? QUICK_SORT_PHASES[0]);

function advancePreview(): void {
  phaseIndex.value = (phaseIndex.value + 1) % QUICK_SORT_PHASES.length;
}

function startPreview(): void {
  if (previewTimer) clearInterval(previewTimer);
  previewTimer = setInterval(advancePreview, 1200);
  isPreviewPlaying.value = true;
}

function stopPreview(): void {
  if (previewTimer) clearInterval(previewTimer);
  previewTimer = null;
  isPreviewPlaying.value = false;
}

function togglePreviewPlay(): void {
  if (isPreviewPlaying.value) {
    stopPreview();
  } else {
    startPreview();
  }
}

function stepPreview(): void {
  stopPreview();
  advancePreview();
}

function resetPreview(): void {
  phaseIndex.value = 0;
  startPreview();
}

/* ── Bento Grid Module Definitions (4 Cột) ── */
interface BentoFeature {
  id: string;
  title: string;
  subtitle: string;
  description: string;
  icon: Component;
  tag: string;
  route: string;
  accentClass: string;
  size: 'large' | 'medium' | 'small';
}

const bentoFeatures: BentoFeature[] = [
  {
    id: 'sort',
    title: 'Thuật Toán Sắp Xếp Trực Quan',
    subtitle: '7 thuật toán sắp xếp kinh điển',
    description: 'Quan sát Bubble, Quick, Merge, Heap Sort hoạt ảnh mượt mà từng bước. Dễ dàng nhận biết vị trí Pivot, Swap và vùng đã sắp xếp.',
    icon: ArrowUpDown,
    tag: 'Algorithms',
    route: '/simulations',
    accentClass: 'text-primary',
    size: 'large',
  },
  {
    id: 'graph',
    title: 'Sân Chơi Đồ Thị (Graph Sandbox)',
    subtitle: 'Tự do kéo thả đỉnh & kết nối cạnh',
    description: 'Tương tác trực tiếp với các đỉnh đồ thị, gán trọng số và chạy thuật toán tìm đường ngắn nhất Dijkstra, BFS, DFS sinh động.',
    icon: Network,
    tag: 'Graph',
    route: '/simulations',
    accentClass: 'text-amber-500',
    size: 'medium',
  },
  {
    id: 'gamification',
    title: 'Gamification Học Mà Chơi',
    subtitle: 'Tích lũy XP, Streak & Mở khóa Huy hiệu',
    description: 'Biến việc học giải thuật thành hành trình thú vị với chuỗi ngày liên tục (Streak), thăng cấp độ và leo bảng xếp hạng.',
    icon: Trophy,
    tag: 'Gamification',
    route: '/leaderboard',
    accentClass: 'text-purple-500',
    size: 'medium',
  },
  {
    id: 'oop',
    title: 'OOP',
    subtitle: 'Lập trình Hướng đối tượng',
    description: 'Đóng gói, Kế thừa, Đa hình & Trừu tượng.',
    icon: Boxes,
    tag: 'OOP',
    route: '/simulations',
    accentClass: 'text-cyan-500',
    size: 'small',
  },
  {
    id: 'solid',
    title: '5 Nguyên Lý SOLID',
    subtitle: 'Thiết kế phần mềm linh hoạt',
    description: 'Làm chủ SRP, OCP, LSP, ISP, DIP chuẩn công nghiệp.',
    icon: ShieldCheck,
    tag: 'SOLID',
    route: '/simulations',
    accentClass: 'text-emerald-500',
    size: 'small',
  },
  {
    id: 'patterns',
    title: 'Design Patterns',
    subtitle: 'Mẫu thiết kế kinh điển',
    description: 'Singleton, Factory, Observer, Strategy trực quan.',
    icon: GitFork,
    tag: 'Patterns',
    route: '/simulations',
    accentClass: 'text-pink-500',
    size: 'small',
  },
  {
    id: 'di',
    title: 'DI / IoC',
    subtitle: 'Dependency Injection',
    description: 'Quản lý phụ thuộc mã nguồn chuẩn Clean Architecture.',
    icon: Sparkles,
    tag: 'Architecture',
    route: '/simulations',
    accentClass: 'text-blue-500',
    size: 'small',
  },
];

/* ── Algorithm Catalog Filter Groups ── */
type CatalogGroup =
  | 'all'
  | 'sort'
  | 'search'
  | 'graph'
  | 'tree'
  | 'heap'
  | 'hash'
  | 'linear'
  | 'structure';

interface CatalogGroupDef {
  key: CatalogGroup;
  label: string;
  match: (meta: CatalogMeta) => boolean;
}

const catalogGroups: CatalogGroupDef[] = [
  { key: 'all', label: messages.home.catalogFilterAll, match: () => true },
  { key: 'sort', label: messages.home.catalogFilterSort, match: (m) => m.key.startsWith('sort.') },
  { key: 'search', label: messages.home.catalogFilterSearch, match: (m) => m.key.startsWith('search.') },
  { key: 'graph', label: messages.home.catalogFilterGraph, match: (m) => m.key.startsWith('graph.') },
  { key: 'tree', label: messages.home.catalogFilterTree, match: (m) => m.key.startsWith('tree.') },
  { key: 'heap', label: messages.home.catalogFilterHeap, match: (m) => m.key.startsWith('heap.') },
  { key: 'hash', label: messages.home.catalogFilterHash, match: (m) => m.key.startsWith('hash.') },
  {
    key: 'linear',
    label: messages.home.catalogFilterLinear,
    match: (m) =>
      m.key.startsWith('stack.') || m.key.startsWith('queue.') || m.key.startsWith('list.'),
  },
  { key: 'structure', label: messages.home.catalogFilterStructure, match: (m) => m.category === 'structure' },
];

const activeGroup = ref<CatalogGroup>('all');
const searchQuery = ref('');
const isCatalogExpanded = ref(false);

const catalogCounts = computed<Record<CatalogGroup, number>>(() => {
  const counts = {} as Record<CatalogGroup, number>;
  for (const group of catalogGroups) counts[group.key] = CATALOG.filter(group.match).length;
  return counts;
});

const filteredCatalog = computed(() => {
  const group = catalogGroups.find((g) => g.key === activeGroup.value);
  let list = group ? CATALOG.filter(group.match) : CATALOG;
  const q = searchQuery.value.trim().toLowerCase();
  if (q) {
    list = list.filter(
      (m) =>
        m.title.toLowerCase().includes(q) ||
        m.key.toLowerCase().includes(q) ||
        m.dataStructure.toLowerCase().includes(q) ||
        m.complexity.average.toLowerCase().includes(q) ||
        m.complexity.space.toLowerCase().includes(q),
    );
  }
  return list;
});

const displayedCatalog = computed(() => {
  if (searchQuery.value.trim()) return filteredCatalog.value;
  if (isCatalogExpanded.value) return filteredCatalog.value;
  return filteredCatalog.value.slice(0, 8);
});

const canExpandCatalog = computed(() => {
  return !searchQuery.value.trim() && filteredCatalog.value.length > 8;
});

function levelLabel(level: CatalogMeta['level']): string {
  return level === 'advanced' ? messages.explore.levelAdvanced : messages.explore.levelBasic;
}

/* ── Testimonials Carousel ── */
interface Testimonial {
  name: string;
  role: string;
  quote: string;
  initials: string;
  rating: number;
}

const testimonials: Testimonial[] = [
  {
    name: 'Trần Hoàng Long',
    role: 'Sinh viên Kỹ thuật Phần mềm (FPT University)',
    quote: 'Việc nhìn thấy từng bước biến đổi của con trỏ và mảng giúp em hiểu bản chất thuật toán nhanh hơn 10 lần so với chỉ đọc code chay.',
    initials: 'TL',
    rating: 5,
  },
  {
    name: 'Nguyễn Thu Trang',
    role: 'Frontend Developer @ Tech Solutions',
    quote: 'Giao diện trực quan, mô phỏng mượt mà và hệ thống Practice Ladder giúp mình tự tin vượt qua các vòng phỏng vấn Coding Interview.',
    initials: 'TT',
    rating: 5,
  },
  {
    name: 'Vũ Đức Minh',
    role: 'Sinh viên năm 2 ĐH Bách Khoa',
    quote: 'Phần Sandbox thực hành và Codelab chấm tự động cực kỳ tiện lợi. Đồ thị và Tree trực quan hóa rất đẹp mắt và dễ hiểu.',
    initials: 'DM',
    rating: 5,
  },
];

const currentTestimonial = ref(0);
let testimonialTimer: ReturnType<typeof setInterval> | null = null;

function nextTestimonial(): void {
  currentTestimonial.value = (currentTestimonial.value + 1) % testimonials.length;
}

function prevTestimonial(): void {
  currentTestimonial.value =
    (currentTestimonial.value - 1 + testimonials.length) % testimonials.length;
}

function goToTestimonial(index: number): void {
  currentTestimonial.value = index;
}

/* ── Interactive Live Sandbox ── */
const sandboxInput = ref('8, 3, 5, 1, 9, 2');
const sandboxArray = ref([8, 3, 5, 1, 9, 2]);
const sandboxActiveIndices = ref<[number, number] | null>(null);
const sandboxSortedIndices = ref<number[]>([]);
const sandboxRunning = ref(false);

function resetSandbox(): void {
  const parsed = sandboxInput.value
    .split(',')
    .map((s) => parseInt(s.trim(), 10))
    .filter((n) => !isNaN(n));
  sandboxArray.value = parsed.length ? parsed.slice(0, 8) : [8, 3, 5, 1, 9, 2];
  sandboxActiveIndices.value = null;
  sandboxSortedIndices.value = [];
  sandboxRunning.value = false;
}

async function runSandboxSort(): Promise<void> {
  if (sandboxRunning.value) return;
  sandboxRunning.value = true;
  sandboxSortedIndices.value = [];
  const arr = [...sandboxArray.value];
  const n = arr.length;

  for (let i = 0; i < n; i++) {
    for (let j = 0; j < n - i - 1; j++) {
      sandboxActiveIndices.value = [j, j + 1];
      await new Promise((r) => setTimeout(r, 260));
      if (arr[j] > arr[j + 1]) {
        const temp = arr[j];
        arr[j] = arr[j + 1];
        arr[j + 1] = temp;
        sandboxArray.value = [...arr];
        await new Promise((r) => setTimeout(r, 220));
      }
    }
    sandboxSortedIndices.value.push(n - i - 1);
  }
  sandboxActiveIndices.value = null;
  sandboxRunning.value = false;
}

/* ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   2. AUTHENTICATED DASHBOARD STATE & ANIMATIONS
   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ */

const levelThresholds = [0, 100, 300, 600, 1000, 1500, 2200, 3000];
const circumference = 2 * Math.PI * 46;

const userLevel = computed(() => gamificationStore.level || 1);
const userXP = computed(() => gamificationStore.xp || 0);
const userStreak = computed(() => gamificationStore.streakDays || 0);

const xpProgressPercent = computed(() => {
  const lvl = userLevel.value;
  if (lvl <= 0) return 0;
  if (lvl >= levelThresholds.length) return 100;
  const prev = levelThresholds[lvl - 1] ?? 0;
  const next = levelThresholds[lvl] ?? 100;
  const range = next - prev;
  if (range <= 0) return 100;
  return Math.min(100, Math.max(0, ((userXP.value - prev) / range) * 100));
});

const xpToNextLevel = computed(() => {
  const lvl = userLevel.value;
  if (lvl >= levelThresholds.length) return 0;
  return Math.max(0, (levelThresholds[lvl] ?? 100) - userXP.value);
});

const dashOffset = computed(() => {
  return circumference * (1 - xpProgressPercent.value / 100);
});

const daysOfWeek = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

const enrolledTopics = computed(() => {
  return progressStore.overview?.topics ?? [];
});

const recentActivities = computed(() => {
  const list = [];
  if (userStreak.value > 0) {
    list.push({
      id: 'act-streak',
      title: `Chuỗi học tập ${userStreak.value} ngày`,
      desc: 'Giữ vững lửa học tập mỗi ngày!',
      icon: Flame,
      time: 'Hôm nay',
    });
  }
  if (progressStore.overview?.exercisesCompleted) {
    list.push({
      id: 'act-exercise',
      title: `Hoạt động ${progressStore.overview.exercisesCompleted} bài tập`,
      desc: `Điểm trung bình: ${progressStore.overview.avgScore ?? 100}%`,
      icon: CheckCircle2,
      time: 'Gần đây',
    });
  }
  if (gamificationStore.questDone > 0) {
    list.push({
      id: 'act-quest',
      title: `Đã nhận thưởng ${gamificationStore.questDone} nhiệm vụ`,
      desc: 'Tích lũy Gems và XP thành công.',
      icon: Target,
      time: 'Hôm nay',
    });
  }
  return list;
});

const topBadges = computed(() => {
  return (gamificationStore.achievements ?? []).slice(0, 4);
});

/* ── 3D Card Tilt Animation (Theo Chuột) ──
   Dùng CSS variables (--rx/--ry) + transform perspective trên wrapper
   .tilt-card — KHÔNG dùng GSAP ở đây (tránh xung đột transition với
   spring-hover; chỉ kích hoạt trên thiết bị có hover chuột). */
const tiltSupported =
  typeof window !== 'undefined' && window.matchMedia('(hover: hover)').matches;

function handleTilt(e: MouseEvent): void {
  if (typeof window !== 'undefined' && window.matchMedia('(hover: none)').matches) return;
  if (!tiltSupported) return;
  const card = e.currentTarget as HTMLElement;
  const rect = card.getBoundingClientRect();
  const x = (e.clientX - rect.left) / rect.width - 0.5;
  const y = (e.clientY - rect.top) / rect.height - 0.5;
  card.style.setProperty('--rx', `${(-y * 12).toFixed(2)}deg`);
  card.style.setProperty('--ry', `${(x * 12).toFixed(2)}deg`);
}

function handleTiltReset(e: MouseEvent): void {
  if (!tiltSupported) return;
  const card = e.currentTarget as HTMLElement;
  card.style.removeProperty('--rx');
  card.style.removeProperty('--ry');
}

/* ── Bento Sorting Wave — cột thật (không gradient trang trí) ── */
interface BentoWaveBar {
  height: number;
  opacity: number;
}

const BENTO_WAVE_BARS: BentoWaveBar[] = [
  { height: 30, opacity: 0.4 },
  { height: 55, opacity: 0.6 },
  { height: 40, opacity: 0.45 },
  { height: 70, opacity: 0.55 },
  { height: 50, opacity: 0.35 },
  { height: 85, opacity: 0.6 },
  { height: 62, opacity: 0.5 },
  { height: 45, opacity: 0.42 },
];

/* ── GSAP Master Animations ── */
let splitInstance: InstanceType<typeof SplitText> | null = null;
let gsapCtx: gsap.Context | null = null;

const prefersReducedMotion = (): boolean =>
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function initAnimations(): void {
  if (prefersReducedMotion() || !homeRef.value) return;

  gsapCtx = gsap.context(() => {
    // 1. Hero Title Split Text Animation
    if (heroTitleRef.value) {
      splitInstance = new SplitText(heroTitleRef.value, { type: 'words' });
      gsap.from(splitInstance.words, {
        y: 35,
        opacity: 0,
        rotateX: -20,
        stagger: 0.04,
        duration: 0.7,
        ease: 'power3.out',
        delay: 0.1,
      });
    }

    // 2. Hero Elements Fade-Up Sequence
    gsap.from('.hero__sub', { y: 20, opacity: 0, duration: 0.6, ease: 'power2.out', delay: 0.3 });
    gsap.from('.hero__actions', { y: 20, opacity: 0, duration: 0.6, ease: 'power2.out', delay: 0.4 });
    gsap.from('.hero__trust', { y: 20, opacity: 0, duration: 0.6, ease: 'power2.out', delay: 0.5 });
    gsap.from('.hero__preview', {
      y: 40, opacity: 0, scale: 0.96, duration: 0.8, ease: 'power2.out', delay: 0.3,
    });

    // 3. Bento Grid Stagger Animation on Scroll
    gsap.from('.bento-card', {
      y: 40,
      opacity: 0,
      stagger: 0.08,
      duration: 0.6,
      ease: 'power2.out',
      scrollTrigger: { trigger: '.bento-grid', start: 'top 85%', once: true },
    });

    // 4. Extended Sections Reveal on Scroll
    // (Chỉ chạy cho guest — khi authed 4 section marketing bị v-if ẩn khỏi
    // DOM, tránh ScrollTrigger log "target not found" trên console.)
    if (!authStore.isAuthenticated) {
      gsap.from('.extended-text', {
        x: -40, opacity: 0, duration: 0.7, ease: 'power2.out',
        scrollTrigger: { trigger: '.extended-section', start: 'top 80%', once: true },
      });
      gsap.from('.extended-visual', {
        x: 40, opacity: 0, duration: 0.7, ease: 'power2.out',
        scrollTrigger: { trigger: '.extended-section', start: 'top 80%', once: true },
      });
    }

    // 5. Dashboard Stagger & Number Counters
    if (authStore.isAuthenticated) {
      gsap.from('.greeting-banner', { y: -25, opacity: 0, duration: 0.6, ease: 'power3.out' });
      gsap.from('.dash-card', {
        y: 25, opacity: 0, stagger: 0.06, duration: 0.5, ease: 'power2.out', delay: 0.15,
      });

      // Animated XP & Streak Numbers
      gsap.fromTo('.xp-num',
        { innerHTML: 0 },
        { innerHTML: userXP.value, duration: 1.5, ease: 'power2.out', snap: { innerHTML: 1 } },
      );
      gsap.fromTo('.streak-num',
        { innerHTML: 0 },
        { innerHTML: userStreak.value, duration: 1.2, ease: 'power2.out', snap: { innerHTML: 1 } },
      );
    }
  }, homeRef.value);
}

/* ── Lifecycle ── */
onMounted(async () => {
  startPreview();

  if (authStore.isAuthenticated) {
    void gamificationStore.fetchAll();
    void gamificationStore.fetchQuests();
    void gamificationStore.fetchAchievements();
    void progressStore.fetchOverview();
  }

  testimonialTimer = setInterval(nextTestimonial, 5000);
  await nextTick();
  initAnimations();
});

onUnmounted(() => {
  stopPreview();
  if (testimonialTimer) clearInterval(testimonialTimer);
  splitInstance?.revert();
  gsapCtx?.revert();
});
</script>

<template>
  <div class="home-unified" ref="homeRef">

    <!-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         A. BỐ CỤC DASHBOARD HỌC VIÊN (SOURCE 2 DASHBOARD LAYOUT)
         ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ -->
    <section v-if="authStore.isAuthenticated" id="sec-dashboard" class="dashboard-section">
      <div class="container">
        
        <!-- 1. Banner Chào Mừng (Greeting Banner Toàn Chiều Rộng) -->
        <div class="greeting-banner glass-panel spring-hover">
          <div class="greeting-banner__content">
            <h1 class="greeting-banner__title font-display text-2xl mb-2">
              Chào mừng <span class="greeting-banner__name text-gradient">{{ authStore.user?.displayName || authStore.user?.email || 'Học viên' }}</span> quay trở lại!
            </h1>
            <p class="greeting-banner__sub text-muted-foreground">
              Level <span class="text-accent font-bold">{{ userLevel }}</span> · <span class="text-accent-warm font-bold font-mono"><span class="xp-num">{{ userXP }}</span> XP</span> ·
              <span v-if="authStore.role === 'TEACHER' || authStore.role === 'ADMIN'" class="role-tag role-tag--teacher">
                {{ authStore.role === 'ADMIN' ? 'Quản trị viên' : 'Giảng viên' }}
              </span>
              <span v-else class="role-tag role-tag--student">Sinh viên</span>
            </p>
          </div>
          <div class="greeting-banner__graphic ambient-float" aria-hidden="true">
            <Trophy class="w-16 h-16 text-primary opacity-25" />
          </div>
        </div>

        <!-- 2. Lưới Bảng Điều Khiển (Dashboard Grid) -->
        <div class="dashboard__grid">

          <!-- Lộ trình đang học (Enrolled roadmaps — Chiếm full width) -->
          <div class="dash-card enrolled-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-3">
              <BookOpen class="w-4 h-4 text-primary inline-block mr-1.5 align-text-bottom" />
              Lộ trình đang học
            </h3>
            <div v-if="enrolledTopics.length === 0" class="enrolled-empty text-center py-6">
              <p class="text-text-muted text-xs mb-3">Bạn chưa đăng ký lộ trình nào.</p>
              <RouterLink :to="{ name: 'path' }" class="enrolled-cta inline-flex items-center gap-1.5 px-4 py-2 rounded-xl bg-primary text-white text-xs font-bold hover:opacity-90 transition-all">
                <Compass class="w-3.5 h-3.5" />
                Khám phá Lộ trình
              </RouterLink>
            </div>
            <div v-else class="enrolled-grid grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-3">
              <RouterLink
                v-for="topic in enrolledTopics"
                :key="topic.id"
                :to="{ name: 'path-topic', params: { topicId: topic.id } }"
                class="enrolled-item flex items-center gap-3 p-3 rounded-xl border border-border-subtle bg-bg-surface/50 hover:border-primary transition-all"
              >
                <div class="flex-1 min-w-0">
                  <h4 class="text-xs font-bold text-text-primary truncate">{{ topic.name }}</h4>
                  <div class="flex items-center gap-2 mt-1.5">
                    <div class="flex-1 h-1.5 bg-muted rounded-full overflow-hidden">
                      <div class="h-full bg-gradient-to-r from-primary to-emerald-500 rounded-full transition-all" :style="{ width: topic.progressPct + '%' }"></div>
                    </div>
                    <span class="text-[10px] font-bold text-primary font-mono">{{ topic.progressPct }}%</span>
                  </div>
                </div>
                <ChevronRight class="w-4 h-4 text-text-muted shrink-0" />
              </RouterLink>
            </div>
          </div>

          <!-- Khám phá Lộ trình học (Quickstart steps — Chiếm full width) -->
          <div class="dash-card quickstart-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-1">
              <Compass class="w-4 h-4 text-primary inline-block mr-1.5 align-text-bottom" />
              Khám phá Lộ trình học
            </h3>
            <p class="quickstart-intro text-xs text-muted-foreground mb-3">Bắt đầu hành trình chinh phục Thuật toán bằng cách đi theo lộ trình được thiết kế sẵn:</p>
            <div class="quickstart-steps">
              <RouterLink :to="{ name: 'path' }" class="quickstart-item">
                <div class="quickstart-item__content">
                  <span class="quickstart-item__title text-primary font-semibold">Xem Bản đồ Lộ trình</span>
                  <span class="quickstart-item__desc text-xs text-muted-foreground">Học qua từng bài học, mô phỏng trực quan và bài tập thực hành.</span>
                </div>
                <ArrowRight class="quickstart-item__arrow text-primary w-5 h-5 shrink-0" />
              </RouterLink>
              
              <RouterLink :to="{ name: 'classes' }" class="quickstart-item">
                <div class="quickstart-item__content">
                  <span class="quickstart-item__title font-semibold">Tham gia Lớp học</span>
                  <span class="quickstart-item__desc text-xs text-muted-foreground">Nhập mã từ Giảng viên để theo dõi tiến độ cùng lớp.</span>
                </div>
                <ArrowRight class="quickstart-item__arrow w-5 h-5 shrink-0" />
              </RouterLink>
            </div>
          </div>

          <!-- Tiến trình XP (XP Wheel SVG Animated) -->
          <div class="dash-card xp-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-2">Tiến trình XP</h3>
            <div class="xp-wheel">
              <svg viewBox="0 0 100 100" class="xp-wheel__svg">
                <defs>
                  <linearGradient id="xp-wheel-grad" x1="0%" y1="0%" x2="100%" y2="100%">
                    <stop offset="0%" stop-color="var(--color-primary)" />
                    <stop offset="100%" stop-color="#10B981" />
                  </linearGradient>
                </defs>
                <circle cx="50" cy="50" r="46" fill="none" stroke="rgba(0,0,0,0.06)" stroke-width="8" />
                <circle
                  cx="50"
                  cy="50"
                  r="46"
                  fill="none"
                  stroke="url(#xp-wheel-grad)"
                  stroke-width="8"
                  stroke-linecap="round"
                  :stroke-dasharray="circumference"
                  :stroke-dashoffset="dashOffset"
                  class="xp-wheel__progress"
                />
              </svg>
              <div class="xp-wheel__center">
                <span class="xp-wheel__level font-bold text-primary">Lv.{{ userLevel }}</span>
                <span class="xp-wheel__xp text-xs font-mono"><span class="xp-num font-bold">{{ userXP }}</span> XP</span>
              </div>
            </div>
            <p class="xp-card__hint text-xs text-muted-foreground text-center mt-2">{{ xpToNextLevel }} XP để lên level tiếp theo</p>
          </div>

          <!-- Chuỗi ngày học (Streak Card với Flame Pulse) -->
          <div class="dash-card streak-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-2">Chuỗi ngày học</h3>
            <div class="flex items-center justify-center gap-4 py-2">
              <Flame class="w-10 h-10 text-orange-500 flame-animated" />
              <div>
                <div class="text-2xl font-bold text-orange-500 font-mono"><span class="streak-num">{{ userStreak }}</span> Ngày</div>
                <div class="text-xs text-muted-foreground">Giữ lửa học tập!</div>
              </div>
            </div>
            <div class="flex justify-between mt-4">
              <div
                v-for="(d, idx) in daysOfWeek"
                :key="d"
                class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold font-mono transition-all"
                :class="userStreak > 0 && idx <= (userStreak % 7) ? 'bg-orange-500/20 text-orange-500 border border-orange-500/50 shadow-[0_0_10px_rgba(249,115,22,0.3)]' : 'bg-muted text-muted-foreground border border-border'"
              >
                {{ d }}
              </div>
            </div>
          </div>

          <!-- Daily Quests Card -->
          <div class="dash-card quests-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-2">
              <Target class="w-4 h-4 text-emerald-500 inline-block mr-1 align-text-bottom" />
              Nhiệm vụ hôm nay
            </h3>
            <div v-if="gamificationStore.quests.length === 0" class="text-xs text-muted-foreground py-4 text-center">
              Đang tải danh sách nhiệm vụ...
            </div>
            <div v-else class="space-y-2.5">
              <div
                v-for="quest in gamificationStore.quests.slice(0, 3)"
                :key="quest.id"
                class="flex items-center justify-between py-1 border-b border-border/50 text-xs"
              >
                <div class="flex items-center gap-2">
                  <span class="w-4 h-4 rounded-full border border-border flex items-center justify-center" :class="{ 'bg-emerald-500 border-emerald-500 text-white': quest.claimed }">
                    <Check v-if="quest.claimed" class="w-2.5 h-2.5" />
                  </span>
                  <span :class="{ 'line-through text-muted-foreground': quest.claimed }">{{ quest.title }}</span>
                </div>
                <span class="text-amber-500 font-mono font-semibold">+{{ quest.rewardXp }} XP</span>
              </div>
              <div class="text-right pt-1">
                <RouterLink :to="{ name: 'quests' }" class="text-xs text-primary font-medium hover:underline inline-flex items-center gap-1">
                  Xem tất cả nhiệm vụ <ArrowRight class="w-3 h-3" />
                </RouterLink>
              </div>
            </div>
          </div>

          <!-- Hoạt động gần đây -->
          <div class="dash-card recent-activity-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-2">Hoạt động gần đây</h3>
            <div v-if="recentActivities.length === 0" class="empty-state text-center py-4">
              <Activity class="w-8 h-8 text-muted-foreground mx-auto mb-1 opacity-50" />
              <span class="text-muted-foreground text-xs">Chưa có hoạt động gần đây</span>
            </div>
            <div v-else class="space-y-2">
              <div v-for="activity in recentActivities" :key="activity.id" class="flex items-center gap-2.5 text-xs py-1">
                <component :is="activity.icon" class="w-4 h-4 text-primary shrink-0" />
                <div class="flex-1 min-w-0">
                  <h4 class="font-semibold truncate">{{ activity.title }}</h4>
                  <p class="text-[11px] text-muted-foreground truncate">{{ activity.desc }}</p>
                </div>
                <span class="text-[10px] text-muted-foreground shrink-0">{{ activity.time }}</span>
              </div>
            </div>
          </div>

          <!-- Huy hiệu đã mở -->
          <div class="dash-card badges-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-2">Huy hiệu đã mở</h3>
            <div v-if="topBadges.length === 0" class="text-center py-4">
              <Trophy class="w-8 h-8 text-muted-foreground mx-auto mb-1 opacity-40" />
              <span class="text-xs text-muted-foreground">Chưa có huy hiệu nào. Hãy hoàn thành bài học!</span>
            </div>
            <div v-else class="grid grid-cols-2 gap-2">
              <div v-for="badge in topBadges" :key="badge.id" class="p-2 rounded-lg bg-muted/60 flex items-center gap-2">
                <Award class="w-4 h-4 text-amber-500 shrink-0" />
                <span class="text-xs font-semibold truncate">{{ badge.name }}</span>
              </div>
            </div>
          </div>

          <!-- Truy cập nhanh (Quicklinks) -->
          <div class="dash-card quicklinks-card glass-panel spring-hover">
            <h3 class="dash-card__title font-bold text-sm mb-2">Truy cập nhanh</h3>
            <div class="grid grid-cols-2 gap-2">
              <RouterLink :to="{ name: 'path' }" class="quicklink-btn flex items-center gap-2 p-2 rounded-lg bg-muted/60 hover:bg-muted text-xs font-medium transition-all">
                <BookOpen class="w-4 h-4 text-primary" />
                <span>Bản đồ Lộ trình</span>
              </RouterLink>
              <RouterLink :to="{ name: 'leaderboard' }" class="quicklink-btn flex items-center gap-2 p-2 rounded-lg bg-muted/60 hover:bg-muted text-xs font-medium transition-all">
                <Trophy class="w-4 h-4 text-amber-500" />
                <span>Bảng xếp hạng</span>
              </RouterLink>
              <RouterLink :to="{ name: 'shop' }" class="quicklink-btn flex items-center gap-2 p-2 rounded-lg bg-muted/60 hover:bg-muted text-xs font-medium transition-all">
                <Gem class="w-4 h-4 text-purple-500" />
                <span>Cửa hàng Gems</span>
              </RouterLink>
              <RouterLink :to="{ name: 'simulations' }" class="quicklink-btn flex items-center gap-2 p-2 rounded-lg bg-muted/60 hover:bg-muted text-xs font-medium transition-all">
                <Layers class="w-4 h-4 text-emerald-500" />
                <span>Thư viện mô phỏng</span>
              </RouterLink>
            </div>
          </div>

        </div>
      </div>
    </section>

    <!-- ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
         B. BỐ CỤC LANDING PAGE TRANG KHÁCH (SOURCE 2 LANDING LAYOUT)
         ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ -->

    <!-- ── TẦNG 1: HERO SECTION TRUNG TÂM & MESH BACKGROUND ── -->
    <section v-if="!authStore.isAuthenticated" id="sec-hero" class="hero relative overflow-hidden">
      <!-- Gradient Mesh Floating Blobs -->
      <div class="hero__mesh" aria-hidden="true">
        <div class="mesh-blob mesh-blob--1"></div>
        <div class="mesh-blob mesh-blob--2"></div>
        <div class="mesh-blob mesh-blob--3"></div>
      </div>

      <!-- Pulse Glow & Dot Grid Texture -->
      <div class="hero__glow z-0" aria-hidden="true"></div>
      <div class="hero__particles z-0" aria-hidden="true"></div>

      <div class="hero__content" data-aos="fade-up">
        <!-- Pulse Badge -->
        <div class="hero__badge spring-hover">
          <span class="pulse-dot"></span>
          <span>Nền tảng học thuật chuẩn bị ra mắt</span>
        </div>

        <!-- Hero Title -->
        <h1 class="hero__title font-display" ref="heroTitleRef">
          <span class="hero__prefix text-primary drop-shadow-md">~/</span>
          Khám phá thuật toán theo cách <span class="text-gradient">sống động nhất</span>
        </h1>

        <!-- Hero Subtitle -->
        <p class="hero__sub font-sans">
          Trực quan hóa cấu trúc dữ liệu, đồ thị, và design patterns. 
          Hệ thống gamification giúp bạn biến việc học code thành một hành trình thú vị.
        </p>

        <!-- Hero Actions -->
        <div class="hero__actions">
          <RouterLink
            v-if="!authStore.isAuthenticated"
            :to="{ name: 'register' }"
            class="btn-primary hero-btn spring-hover inline-flex items-center"
          >
            <Play class="size-4 mr-2" />
            Bắt đầu học ngay
          </RouterLink>
          <RouterLink
            v-else
            :to="{ name: 'path' }"
            class="btn-primary hero-btn spring-hover inline-flex items-center"
          >
            <Compass class="size-4 mr-2" />
            {{ messages.home.continueLearning }}
          </RouterLink>

          <RouterLink
            :to="{ name: 'simulations' }"
            class="btn-ghost hero-btn spring-hover inline-flex items-center"
          >
            <Layers class="size-4 mr-2" />
            Khám phá Thư viện thuật toán
          </RouterLink>
        </div>

        <!-- 4 Chỉ số Trust Indicators -->
        <div class="hero__trust" data-aos="fade-up" data-aos-delay="300">
          <div class="trust-item">
            <span class="trust-number font-mono">{{ stats.visuals }}+</span>
            <span class="trust-label">Thuật toán trực quan hóa</span>
          </div>
          <div class="trust-divider" aria-hidden="true"></div>
          <div class="trust-item">
            <span class="trust-number font-mono">4</span>
            <span class="trust-label">Bước học mỗi bài</span>
          </div>
          <div class="trust-divider" aria-hidden="true"></div>
          <div class="trust-item">
            <span class="trust-number font-mono">100%</span>
            <span class="trust-label">Tiếng Việt</span>
          </div>
          <div class="trust-divider" aria-hidden="true"></div>
          <div class="trust-item">
            <span class="trust-number font-mono">∞</span>
            <span class="trust-label">Thực hành</span>
          </div>
        </div>
      </div>

      <!-- Hero Terminal Preview Window with Smooth QuickSort Animation -->
      <div class="hero__preview z-10" aria-hidden="true" data-aos="fade-up" data-aos-delay="400">
        <div class="glass-panel preview-window spring-hover">
          <div class="preview-header">
            <div class="terminal-dots">
              <span class="terminal-dot terminal-dot--close"></span>
              <span class="terminal-dot terminal-dot--min"></span>
              <span class="terminal-dot terminal-dot--max"></span>
            </div>
            <div class="preview-title font-mono text-xs text-muted-foreground">quick-sort.ts</div>
            <div class="preview-actions flex items-center gap-1">
              <button class="preview-btn" aria-label="Run/Pause" :title="isPreviewPlaying ? 'Tạm dừng' : 'Chạy'" @click="togglePreviewPlay">
                <Pause v-if="isPreviewPlaying" class="w-3.5 h-3.5" aria-hidden="true" />
                <Play v-else class="w-3.5 h-3.5" aria-hidden="true" />
              </button>
              <button class="preview-btn" aria-label="Step" title="Bước tới" @click="stepPreview">
                <StepForward class="w-3.5 h-3.5" aria-hidden="true" />
              </button>
              <button class="preview-btn" aria-label="Reset" title="Đặt lại" @click="resetPreview">
                <RotateCcw class="w-3.5 h-3.5" aria-hidden="true" />
              </button>
            </div>
          </div>

          <div class="preview-body">
            <div class="bars-container">
              <div
                v-for="(bar, i) in currentPhase"
                :key="i"
                class="bar preview-bar"
                :class="bar.cls ? `bar--${bar.cls}` : ''"
                :style="{ height: bar.height + '%' }"
              >
                <span class="preview-bar-label">{{ bar.label }}</span>
              </div>
            </div>
            <p class="preview-phase-label" aria-live="polite">{{ messages.home.previewPhase[phaseIndex] }}</p>
          </div>
        </div>
      </div>
    </section>

    <!-- ── TẦNG 2: BENTO GRID FEATURES (4 CỘT) ── -->
    <section v-if="!authStore.isAuthenticated" id="sec-bento" class="features-section">
      <div class="features-header text-center mb-12" data-aos="fade-up">
        <h2 class="font-display text-3xl mb-3 text-heading">Không chỉ là code, đó là nghệ thuật</h2>
        <p class="text-muted-foreground max-w-2xl mx-auto text-sm">Trải nghiệm nền tảng giáo dục kết hợp với giao diện trực quan hiện đại, mang lại cảm hứng sáng tạo vô tận.</p>
      </div>

      <div class="bento-grid">
        <div
          v-for="feat in bentoFeatures"
          :key="feat.id"
          class="bento-card glass-panel tilt-card"
          :class="`bento-${feat.size}`"
          @mousemove="handleTilt"
          @mouseleave="handleTiltReset"
        >
          <div class="bento-content">
            <div class="bento-icon" :class="feat.accentClass">
              <component :is="feat.icon" />
            </div>
            <h3 class="font-display text-lg text-heading mb-1.5 font-bold">{{ feat.title }}</h3>
            <p class="text-muted-foreground text-xs leading-relaxed">{{ feat.description }}</p>
          </div>
          
          <!-- Animated Sorting Wave Bars Graphic for Large Card -->
          <div v-if="feat.id === 'sort'" class="bento-visual visual-sorting mt-4" aria-hidden="true">
            <div
              v-for="(bar, i) in BENTO_WAVE_BARS"
              :key="i"
              class="bento-wave-bar"
              :style="{ height: bar.height + '%', opacity: bar.opacity, animationDelay: i * 0.1 + 's' }"
            ></div>
          </div>

          <div class="bento-footer mt-3">
            <RouterLink :to="feat.route" class="text-xs font-semibold text-primary inline-flex items-center gap-1 hover:underline">
              Khám phá ngay <ArrowRight class="w-3 h-3" />
            </RouterLink>
          </div>
        </div>
      </div>
    </section>

    <!-- ── 3 DEMO CARDS NHANH (FR-7.6) ── -->
    <section id="sec-demos" class="home__section home__demos">
      <div class="container">
        <div class="home__section-head">
          <span class="home__kicker home__kicker--center">
            <span class="font-mono">{{ messages.home.demoBadge }}</span>
          </span>
          <h2 class="home__section-title">{{ messages.home.demoTabTitle }}</h2>
          <p class="home__section-desc">{{ messages.home.demoTabDesc }}</p>
        </div>

        <div class="home__demos-grid">
          <Card
            v-for="demo in demos"
            :key="demo.key"
            class="home__demo tilt-card"
            @mousemove="handleTilt"
            @mouseleave="handleTiltReset"
          >
            <CardHeader>
              <div class="home__demo-thumb" aria-hidden="true">
                <div v-if="demo.key === 'sort.bubble'" class="home__thumb-bars">
                  <span class="home__thumb-bar" />
                  <span class="home__thumb-bar" />
                  <span class="home__thumb-bar" />
                  <span class="home__thumb-bar home__thumb-bar--done" />
                  <span class="home__thumb-bar home__thumb-bar--done" />
                </div>
                <div v-else-if="demo.key === 'search.binary'" class="home__thumb-row">
                  <span class="home__thumb-block" />
                  <span class="home__thumb-block" />
                  <span class="home__thumb-block home__thumb-block--found" />
                  <span class="home__thumb-block" />
                  <span class="home__thumb-block" />
                </div>
                <div v-else class="home__thumb-graph">
                  <span class="home__thumb-node" />
                  <span class="home__thumb-edge" />
                  <span class="home__thumb-node home__thumb-node--visited" />
                  <span class="home__thumb-edge" />
                  <span class="home__thumb-node" />
                  <span class="home__thumb-edge" />
                  <span class="home__thumb-node" />
                </div>
              </div>
              <CardTitle class="home__demo-title">
                <component :is="demo.icon" :size="16" class="home__demo-title-icon" aria-hidden="true" />
                {{ demo.title }}
              </CardTitle>
              <div class="home__demo-meta">
                <Badge variant="secondary" class="home__demo-badge">{{ demo.dataStructure }}</Badge>
                <span class="home__demo-level">{{ levelLabel(demo.level) }}</span>
              </div>
            </CardHeader>
            <CardContent class="home__demo-content">
              <div class="home__demo-chips">
                <span class="home__chip">{{ messages.home.catalogTime }} {{ demo.complexity.average }}</span>
                <span class="home__chip">{{ messages.home.catalogSpace }} {{ demo.complexity.space }}</span>
              </div>
              <Button
                type="button"
                class="w-full"
                :aria-label="`${messages.home.demoOpen} ${demo.title}`"
                @click="openDemo(demo.key)"
              >
                <Play class="size-4" aria-hidden="true" />
                {{ messages.home.demoRun }}
                <ArrowRight class="size-4" aria-hidden="true" />
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    </section>

    <!-- ── TẦNG 3: ALGORITHM LIBRARY GRID ── -->
    <section id="sec-catalog" class="algogrid-section">
      <div class="container">
        <div class="algogrid-header text-center mb-10" data-aos="fade-up">
          <h2 class="font-display text-3xl mb-3 text-heading">Thư viện thuật toán trực quan</h2>
          <p class="text-muted-foreground max-w-2xl mx-auto text-sm">30+ thuật toán & cấu trúc dữ liệu, sắp xếp theo nhóm — xem animation từng bước, tự nhập dữ liệu, chạy code của bạn.</p>
        </div>

        <div class="home__catalog-toolbar mb-8">
          <div class="home__catalog-search">
            <Search class="home__catalog-search-icon" :size="16" aria-hidden="true" />
            <input
              v-model="searchQuery"
              type="search"
              class="home__catalog-search-input"
              :placeholder="messages.home.catalogSearchPlaceholder"
            />
            <button
              v-if="searchQuery"
              type="button"
              class="home__catalog-search-clear"
              @click="searchQuery = ''"
            >
              <X :size="14" />
            </button>
          </div>

          <div class="home__filters mt-4" role="tablist" aria-label="Bộ lọc danh mục thuật toán">
            <Button
              v-for="group in catalogGroups"
              :key="group.key"
              type="button"
              variant="ghost"
              size="sm"
              class="home__filter"
              :class="{ 'home__filter--active': group.key === activeGroup }"
              @click="activeGroup = group.key"
            >
              <span>{{ group.label }}</span>
              <span class="home__filter-count" aria-hidden="true">{{ catalogCounts[group.key] }}</span>
            </Button>
          </div>
        </div>

        <div v-if="displayedCatalog.length === 0" class="home__catalog-empty" role="status">
          <p class="home__catalog-empty-text">{{ messages.home.catalogNoResults }}</p>
          <Button variant="secondary" size="sm" @click="searchQuery = ''; activeGroup = 'all'">
            {{ messages.common.retry }}
          </Button>
        </div>

        <div v-else class="home__catalog">
          <Card
            v-for="item in displayedCatalog"
            :key="item.key"
            class="home__catalog-card glass-panel tilt-card"
            @mousemove="handleTilt"
            @mouseleave="handleTiltReset"
          >
            <CardHeader class="home__catalog-head">
              <CardTitle class="home__catalog-title">{{ item.title }}</CardTitle>
              <div class="home__catalog-meta">
                <Badge variant="secondary" class="home__catalog-badge">{{ item.dataStructure }}</Badge>
                <span class="home__catalog-level">{{ levelLabel(item.level) }}</span>
              </div>
            </CardHeader>
            <CardContent class="home__catalog-content">
              <div class="home__catalog-chips">
                <span class="home__chip">{{ messages.home.catalogTime }} {{ item.complexity.average }}</span>
                <span class="home__chip">{{ messages.home.catalogSpace }} {{ item.complexity.space }}</span>
              </div>
              <Button
                type="button"
                variant="secondary"
                size="sm"
                class="home__catalog-cta"
                :aria-label="messages.home.catalogOpen(item.title)"
                @click="openDemo(item.key)"
              >
                {{ messages.home.catalogPractice }}
                <ArrowRight class="size-4" aria-hidden="true" />
              </Button>
            </CardContent>
          </Card>
        </div>

        <div v-if="canExpandCatalog || isCatalogExpanded" class="home__catalog-expand mt-6 text-center">
          <Button
            type="button"
            variant="secondary"
            size="lg"
            class="home__catalog-expand-btn"
            @click="isCatalogExpanded = !isCatalogExpanded"
          >
            <template v-if="!isCatalogExpanded">
              {{ messages.home.catalogViewAll(filteredCatalog.length) }}
              <ArrowRight class="size-4 ml-1" />
            </template>
            <template v-else>
              {{ messages.home.catalogCollapse }}
            </template>
          </Button>
        </div>
      </div>
    </section>

    <!-- ── TẦNG 4: FREEMIUM SECTION ── -->
    <section v-if="!authStore.isAuthenticated" id="sec-freemium" class="freemium-section">
      <div class="container">
        <div class="freemium-header text-center mb-10" data-aos="fade-up">
          <h2 class="font-display text-3xl mb-3 text-heading">Miễn phí để bắt đầu, Premium để bứt phá</h2>
          <p class="text-muted-foreground max-w-2xl mx-auto text-sm">Mô hình Freemium giúp bạn học thử miễn phí và nâng cấp khi cần thêm sức mạnh.</p>
        </div>

        <div class="freemium-grid grid grid-cols-1 md:grid-cols-3 gap-6" data-aos="fade-up">
          <div class="freemium-card glass-panel spring-hover p-6 rounded-2xl flex flex-col">
            <div class="freemium-icon text-rose-500 mb-3"><Heart class="w-8 h-8 fill-rose-500/20" /></div>
            <h3 class="font-display text-lg text-heading mb-2 font-bold">Hearts (Tim)</h3>
            <p class="text-xs text-muted-foreground leading-relaxed mb-4 flex-1">Mỗi bài học có giới hạn số lần thử. Sai quá nhiều thì hết tim và phải đợi hồi phục — tạo động lực tập trung.</p>
            <RouterLink :to="{ name: 'help' }" :class="buttonVariants({ variant: 'outline', size: 'sm', class: 'w-full' })">
              {{ messages.home.freemiumHeartsCta }}
            </RouterLink>
          </div>

          <div class="freemium-card glass-panel spring-hover p-6 rounded-2xl flex flex-col">
            <div class="freemium-icon text-cyan-500 mb-3"><Gem class="w-8 h-8 text-cyan-400" /></div>
            <h3 class="font-display text-lg text-heading mb-2 font-bold">Gems (Đá quý)</h3>
            <p class="text-xs text-muted-foreground leading-relaxed mb-4 flex-1">Tiêu gems mua gợi ý, đóng băng streak, khung avatar — tiêu được chứ không chỉ là số đẹp.</p>
            <RouterLink :to="{ name: 'shop' }" :class="buttonVariants({ variant: 'outline', size: 'sm', class: 'w-full' })">
              {{ messages.home.freemiumGemsCta }}
            </RouterLink>
          </div>

          <div class="freemium-card glass-panel spring-hover freemium-card--premium p-6 rounded-2xl border border-amber-500/40 bg-gradient-to-br from-amber-500/10 to-transparent flex flex-col">
            <div class="freemium-icon text-amber-500 mb-3"><Crown class="w-8 h-8" /></div>
            <h3 class="font-display text-lg text-heading mb-2 font-bold flex items-center justify-between">
              Premium VIP
              <Badge variant="secondary" class="text-[10px]">Cao cấp</Badge>
            </h3>
            <p class="text-xs text-muted-foreground leading-relaxed mb-4 flex-1">Mở khóa toàn bộ visualizer cao cấp, AI không giới hạn, không quảng cáo và thanh toán chuyển khoản bảo mật.</p>
            <RouterLink :to="{ name: 'premium' }" :class="buttonVariants({ variant: 'default', size: 'sm', class: 'w-full' })">
              Nâng cấp Premium
            </RouterLink>
          </div>
        </div>
      </div>
    </section>

    <!-- ── TẦNG 5: BỐ CỤC SO LE 2 CỘT (EXTENDED SECTIONS) ── -->

    <!-- Deep-dive 1: Roadmap Lộ trình -->
    <section v-if="!authStore.isAuthenticated" class="extended-section roadmap-section">
      <div class="extended-container">
        <div class="extended-text" data-aos="fade-right">
          <h2 class="font-display text-3xl mb-4 text-heading">Học tập qua Lộ trình (Roadmap) thay vì Mò mẫm</h2>
          <p class="text-muted-foreground mb-6 text-sm">Hệ thống bài học được thiết kế chuẩn sư phạm, dẫn dắt bạn qua 4 bước vững chắc: Lý thuyết ➔ Trực quan hoá ➔ Thực hành Code ➔ Trắc nghiệm.</p>
          <ul class="feature-list text-muted-foreground text-xs space-y-2">
            <li><span class="text-primary font-bold">●</span> Lộ trình từ cơ bản đến nâng cao (Mảng, Cây, Đồ thị, Quy hoạch động).</li>
            <li><span class="text-primary font-bold">●</span> Theo dõi tiến độ học tập chi tiết theo từng chủ đề.</li>
            <li><span class="text-primary font-bold">●</span> Nhận chứng nhận khi hoàn thành khóa học.</li>
          </ul>
        </div>
        <div class="extended-visual" aria-hidden="true" data-aos="fade-left">
          <div class="roadmap-mockup glass-panel spring-hover">
            <div class="rm-node completed">
              <div class="rm-icon"><Check class="w-3.5 h-3.5 text-emerald-500" /></div>
              <div class="rm-label font-sans font-medium text-xs">Mảng & Chuỗi (Arrays & Strings)</div>
            </div>
            <div class="rm-line completed"></div>
            <div class="rm-node active">
              <div class="rm-icon"><Zap class="w-3.5 h-3.5 text-primary" /></div>
              <div class="rm-label font-sans font-medium text-xs">Đồ Thị BFS & DFS</div>
              <span class="rm-pulse-dot" aria-hidden="true"></span>
            </div>
            <div class="rm-line"></div>
            <div class="rm-node">
              <div class="rm-icon"><Lock class="w-3.5 h-3.5 text-muted-foreground" /></div>
              <div class="rm-label font-sans font-medium text-xs">Quy Hoạch Động (DP)</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Deep-dive 2: Codelab Thực hành Trực tiếp (Reverse 2 cột) -->
    <section v-if="!authStore.isAuthenticated" class="extended-section codelab-section reverse">
      <div class="extended-container">
        <div class="extended-text" data-aos="fade-right">
          <h2 class="font-display text-3xl mb-4 text-heading">Thực hành Code Trực tiếp (Codelab)</h2>
          <p class="text-muted-foreground mb-6 text-sm">Không chỉ dừng lại ở việc xem animation. Bạn sẽ được cấp ngay một trình soạn thảo Monaco chuyên nghiệp và hệ thống chấm điểm tự động đa ngôn ngữ ngay trên trình duyệt.</p>
          <ul class="feature-list text-muted-foreground text-xs space-y-2">
            <li><span class="text-emerald-500 font-bold">●</span> Chấm code tự động chuẩn LeetCode (Judge0 API).</li>
            <li><span class="text-emerald-500 font-bold">●</span> Hỗ trợ TypeScript, Python, C++, Java.</li>
            <li><span class="text-emerald-500 font-bold">●</span> Đánh giá Time & Space Complexity thực tế.</li>
          </ul>
        </div>
        <div class="extended-visual" aria-hidden="true" data-aos="fade-left">
          <div class="codelab-mockup glass-panel spring-hover">
            <div class="terminal-header flex items-center justify-between px-4 py-2 bg-black/40 border-b border-border/40">
              <div class="terminal-dots flex gap-1.5">
                <span class="w-2.5 h-2.5 rounded-full bg-red-400"></span>
                <span class="w-2.5 h-2.5 rounded-full bg-amber-400"></span>
                <span class="w-2.5 h-2.5 rounded-full bg-emerald-400"></span>
              </div>
              <div class="terminal-title font-mono text-foreground-secondary text-xs">two-sum.ts</div>
            </div>
            <div class="terminal-body font-mono text-xs p-4 bg-canvas-ink text-gray-200">
              <div class="code-line"><span class="text-purple-400">function</span> <span class="text-blue-400">twoSum</span>(nums: <span class="text-amber-400">number[]</span>, target: <span class="text-amber-400">number</span>) {</div>
              <div class="code-line pl-4">  <span class="text-purple-400">const</span> map = <span class="text-purple-400">new</span> <span class="text-amber-400">Map</span>();</div>
              <div class="code-line pl-4">  <span class="text-purple-400">for</span> (<span class="text-purple-400">let</span> i = <span class="text-emerald-400">0</span>; i &lt; nums.length; i++) {</div>
              <div class="code-line pl-8 text-gray-400">    // Logic tra cứu bù trừ O(N)...</div>
              <div class="code-line pl-4">  }</div>
              <div class="code-line">}</div>
              <div class="codelab-output mt-4 text-emerald-400 flex items-center gap-1.5 font-bold animate-pulse">
                <CheckCircle2 class="w-3.5 h-3.5 inline-block" />
                <span>All 15/15 Test Cases Passed! (12ms · Beats 98%)</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Deep-dive 3: AI Assistant Mentor -->
    <section v-if="!authStore.isAuthenticated" class="extended-section ai-section">
      <div class="extended-container">
        <div class="extended-text" data-aos="fade-right">
          <h2 class="font-display text-3xl mb-4 text-heading">AI Assistant - Người Mentor Tận Tụy</h2>
          <p class="text-muted-foreground mb-6 text-sm">Mắc kẹt ở một bài toán khó? Trợ lý ảo AI luôn túc trực 24/7 để gợi ý hướng giải quyết, giải thích lỗi sai (Bug) và tối ưu hóa đoạn code của bạn mà không hề tiết lộ đáp án.</p>
        </div>
        <div class="extended-visual" aria-hidden="true" data-aos="fade-left">
          <div class="ai-mockup glass-panel spring-hover">
            <div class="ai-chat user">Tại sao Quick Sort bị O(N²) ở TH xấu nhất?</div>
            <div class="ai-chat bot">
              <span class="ai-avatar" aria-hidden="true">AI</span>
              <div class="ai-msg text-xs leading-relaxed">
                Trường hợp xấu nhất <strong>O(N²)</strong> xảy ra khi mảng đã được sắp xếp sẵn và bạn luôn chọn pivot là phần tử cuối/đầu. Khi đó, mảng bị chia thành 1 phần có N-1 phần tử và 1 phần có 0 phần tử.
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- ── TẦNG 6: TESTIMONIALS CAROUSEL ── -->
    <section v-if="!authStore.isAuthenticated" id="sec-testimonials" class="testimonials-section">
      <div class="testimonials-header text-center mb-12" data-aos="fade-up">
        <h2 class="font-display text-3xl mb-3 text-heading">Người học nói gì về VisualizationDSA?</h2>
        <p class="text-muted-foreground max-w-2xl mx-auto text-sm">Trải nghiệm của những người đã học thuật toán theo cách trực quan.</p>
      </div>

      <div class="testimonials-carousel" data-aos="fade-up">
        <Transition name="fade-slide" mode="out-in">
          <div :key="currentTestimonial" class="testimonial-card glass-panel">
            <div class="testimonial-rating flex justify-center gap-1 mb-3">
              <Star v-for="i in testimonials[currentTestimonial].rating" :key="i" class="w-5 h-5 text-amber-400 fill-current" />
            </div>
            <blockquote class="testimonial-quote text-base italic text-center max-w-xl mx-auto mb-6">
              "{{ testimonials[currentTestimonial].quote }}"
            </blockquote>
            <div class="testimonial-author flex flex-col items-center">
              <div class="testimonial-avatar w-12 h-12 rounded-full bg-primary text-white flex items-center justify-center font-bold text-sm mb-2 shadow-md">
                {{ testimonials[currentTestimonial].initials }}
              </div>
              <cite class="testimonial-name font-bold text-sm not-italic">{{ testimonials[currentTestimonial].name }}</cite>
              <p class="testimonial-role text-muted-foreground text-xs">{{ testimonials[currentTestimonial].role }}</p>
            </div>
          </div>
        </Transition>

        <div class="testimonial-controls flex items-center justify-center gap-4 mt-6">
          <button class="carousel-btn p-2 rounded-full border border-border hover:bg-muted transition-colors" @click="prevTestimonial" aria-label="Previous testimonial">
            <ChevronLeft class="w-5 h-5" />
          </button>
          <div class="testimonial-dots flex gap-1.5">
            <button
              v-for="(_, index) in testimonials"
              :key="index"
              class="testimonial-dot w-2.5 h-2.5 rounded-full transition-all"
              :class="currentTestimonial === index ? 'bg-primary w-6 shadow-[0_0_12px_rgba(0,126,114,0.6)]' : 'bg-border'"
              @click="goToTestimonial(index)"
              :aria-label="`Go to testimonial ${index + 1}`"
            />
          </div>
          <button class="carousel-btn p-2 rounded-full border border-border hover:bg-muted transition-colors" @click="nextTestimonial" aria-label="Next testimonial">
            <ChevronRight class="w-5 h-5" />
          </button>
        </div>
      </div>
    </section>

    <!-- ── TẦNG 7: SANDBOX TƯƠNG TÁC ── -->
    <section id="sec-sandbox" class="home__section home__sandbox-section">
      <div class="container">
        <Card class="home__sandbox-card glass-panel">
          <div class="home__sandbox-header flex items-center justify-between flex-wrap gap-2 mb-3">
            <div class="home__sandbox-title-wrap flex items-center gap-2">
              <Terminal class="size-5 text-primary" />
              <h3 class="home__sandbox-title font-bold text-lg">Thử Nghiệm Thuật Toán Trực Tiếp</h3>
            </div>
            <span class="home__sandbox-badge px-2.5 py-1 rounded-full bg-primary/10 text-primary text-xs font-mono">Interactive Sandbox</span>
          </div>

          <p class="home__sandbox-desc text-xs text-muted-foreground mb-4">
            Nhập một dãy số bất kỳ (cách nhau bằng dấu phẩy) và quan sát thuật toán Bubble Sort tự động chạy ngay tại chỗ!
          </p>

          <div class="home__sandbox-input-row flex gap-2 flex-wrap mb-4">
            <input
              v-model="sandboxInput"
              type="text"
              class="home__sandbox-input flex-1 min-w-[200px] h-10 px-3 rounded-lg border border-border bg-card font-mono text-sm"
              placeholder="Ví dụ: 8, 3, 5, 1, 9, 2"
              :disabled="sandboxRunning"
              @keyup.enter="resetSandbox(); runSandboxSort()"
            />
            <Button
              type="button"
              variant="primary"
              :disabled="sandboxRunning"
              @click="resetSandbox(); runSandboxSort()"
            >
              <Play class="size-4 mr-1" />
              Chạy sắp xếp
            </Button>
            <Button
              type="button"
              variant="secondary"
              :disabled="sandboxRunning"
              @click="resetSandbox"
            >
              <RotateCcw class="size-4 mr-1" />
              Đặt lại
            </Button>
          </div>

          <div class="home__sandbox-stage flex gap-2 p-6 rounded-xl bg-canvas-ink border border-border-subtle min-h-[90px] items-center justify-center flex-wrap">
            <span class="home__sandbox-live font-mono" aria-hidden="true">{{ messages.home.sandboxLiveBadge }}</span>
            <div
              v-for="(val, idx) in sandboxArray"
              :key="idx"
              class="home__sandbox-bar flex flex-col items-center justify-center w-12 h-14 rounded-lg bg-data-core text-white font-mono font-bold transition-all duration-300"
              :class="{
                'home__sandbox-bar--active': sandboxActiveIndices?.includes(idx),
                'home__sandbox-bar--sorted': sandboxSortedIndices.includes(idx),
              }"
            >
              <span class="home__sandbox-bar-val text-base">{{ val }}</span>
              <span class="home__sandbox-bar-idx text-[9px] opacity-60">[{{ idx }}]</span>
            </div>
          </div>
        </Card>
      </div>
    </section>

    <!-- ── TẦNG 8: CTA SECTION & FOOTER ── -->
    <section v-if="!authStore.isAuthenticated" id="sec-cta" class="cta-section">
      <div class="cta-card glass-panel spring-hover">
        <h2 class="font-display text-3xl mb-3 text-heading font-bold">Sẵn sàng nâng cao trình độ?</h2>
        <p class="text-muted-foreground mb-8 max-w-lg mx-auto text-sm">Gia nhập cộng đồng sinh viên lập trình Việt Nam và làm chủ Cấu trúc Dữ liệu & Giải thuật ngay hôm nay.</p>
        <RouterLink
          v-if="!authStore.isAuthenticated"
          :to="{ name: 'register' }"
          class="btn-primary hero-btn mx-auto spring-hover inline-flex items-center"
        >
          Tạo tài khoản miễn phí
          <ArrowRight class="size-4 ml-2" />
        </RouterLink>
        <RouterLink
          v-else
          :to="{ name: 'path' }"
          class="btn-primary hero-btn mx-auto spring-hover inline-flex items-center"
        >
          Tiếp tục học ngay
          <ArrowRight class="size-4 ml-2" />
        </RouterLink>
      </div>
    </section>

    <!-- Landing Footer -->
    <footer class="landing-footer border-t border-border mt-16 py-8 px-6 text-center text-xs text-muted-foreground">
      <div class="max-w-4xl mx-auto flex flex-col md:flex-row justify-between items-center gap-4">
        <div>&copy; 2026 VisualizationDSA. Nền tảng học tập thuật toán.</div>
        <div class="flex gap-4">
          <RouterLink :to="{ name: 'help' }" class="hover:text-primary transition-colors">Tài liệu</RouterLink>
          <RouterLink :to="{ name: 'path' }" class="hover:text-primary transition-colors">Thư viện Lộ trình</RouterLink>
          <RouterLink :to="{ name: 'premium' }" class="hover:text-primary transition-colors">Nâng cấp Premium</RouterLink>
        </div>
      </div>
    </footer>

  </div>
</template>

<style scoped>
/* ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
   HomeView — Full Animation Suite (Mesh Blobs, 3D Tilt, Bars)
   ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ */

.home-unified {
  display: flex;
  flex-direction: column;
  width: 100%;
  overflow-x: hidden;
}

.container {
  max-width: 1200px;
  margin-inline: auto;
  padding-inline: var(--space-lg);
  width: 100%;
}

/* ── Glassmorphism Utility ── */
.glass-panel {
  background: var(--color-card);
  border: 1px solid var(--color-border);
  backdrop-filter: blur(16px);
  border-radius: var(--radius-xl);
}

.spring-hover {
  transition: transform 0.35s cubic-bezier(0.175, 0.885, 0.32, 1.275), box-shadow 0.35s ease;
}
.spring-hover:hover {
  transform: translateY(-4px);
  box-shadow: 0 16px 36px -10px rgba(0, 0, 0, 0.12);
}

/* Card tilt theo chuột — transform qua CSS vars (--rx/--ry), hover lift qua --lift
   (tách khỏi .spring-hover để không xung đột transition; card không tilt giữ nguyên). */
.tilt-card {
  transform: perspective(800px) rotateX(var(--rx, 0deg)) rotateY(var(--ry, 0deg)) translateY(var(--lift, 0px));
  transition: transform 0.15s ease-out, box-shadow 0.35s ease;
  will-change: transform;
}
.tilt-card:hover {
  --lift: -4px;
  box-shadow: 0 16px 36px -10px rgba(0, 0, 0, 0.12);
}

/* ━━ DASHBOARD PRESENTATION ━━ */
.dashboard-section {
  padding-top: var(--space-xl);
  padding-bottom: var(--space-xl);
  background: linear-gradient(180deg, color-mix(in srgb, var(--color-primary) 6%, transparent) 0%, transparent 100%);
}

.greeting-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 2rem;
  margin-bottom: 2rem;
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-primary) 10%, transparent), rgba(168, 85, 247, 0.08));
  border: 1px solid color-mix(in srgb, var(--color-primary) 20%, transparent);
}

.greeting-banner__title {
  font-weight: 700;
  color: var(--color-foreground);
}

.greeting-banner__name {
  background: linear-gradient(135deg, var(--color-primary), #06b6d4);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.ambient-float {
  animation: ambientFloat 4s ease-in-out infinite alternate;
}

@keyframes ambientFloat {
  0% { transform: translateY(0px) rotate(0deg); }
  100% { transform: translateY(-8px) rotate(4deg); }
}

.flame-animated {
  animation: flamePulse 1.8s ease-in-out infinite alternate;
}

@keyframes flamePulse {
  0% { transform: scale(1); filter: drop-shadow(0 0 2px rgba(249, 115, 22, 0.3)); }
  100% { transform: scale(1.12); filter: drop-shadow(0 0 8px rgba(249, 115, 22, 0.5)); }
}

.role-tag {
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 600;
}
.role-tag--teacher { background: rgba(234, 179, 8, 0.15); color: #d97706; }
.role-tag--student { background: rgba(99, 102, 241, 0.15); color: var(--color-primary); }

.dashboard__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 1.5rem;
}

.dash-card {
  padding: 1.5rem;
}

.enrolled-card,
.quickstart-card {
  grid-column: 1 / -1;
}

.quickstart-steps {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: 0.75rem;
}

.quickstart-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-radius: 10px;
  background: var(--color-card-raised);
  border: 1px solid var(--color-border);
  text-decoration: none;
  transition: all 0.25s ease;
}
.quickstart-item:hover {
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 126, 114, 0.15);
}

.quickstart-item__title {
  font-size: 0.875rem;
  display: block;
}
.quickstart-item__desc {
  font-size: 0.75rem;
}

.xp-wheel {
  position: relative;
  width: 100px;
  height: 100px;
  margin: 0 auto;
}
.xp-wheel__svg {
  width: 100%;
  height: 100%;
  transform: rotate(-90deg);
}
.xp-wheel__progress {
  transition: stroke-dashoffset 0.8s cubic-bezier(0.16, 1, 0.3, 1);
}
.xp-wheel__center {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

/* ━━ HERO SECTION (SOURCE 2 ANIMATED MESH & GLOW) ━━ */
.hero {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 85vh;
  padding: 4.5rem 1.5rem 3rem;
  text-align: center;
  width: 100%;
}

/* Gradient Mesh Blobs */
.hero__mesh {
  position: absolute;
  inset: 0;
  overflow: hidden;
  z-index: 0;
  pointer-events: none;
}

.mesh-blob {
  position: absolute;
  border-radius: 50%;
  filter: blur(100px);
  opacity: 0.45;
}

.mesh-blob--1 {
  width: 450px;
  height: 450px;
  top: -80px;
  left: -80px;
  background: radial-gradient(circle, var(--color-primary) 0%, rgba(0, 126, 114, 0.2) 70%, transparent 100%);
  animation: meshFloat1 12s ease-in-out infinite alternate;
}

.mesh-blob--2 {
  width: 380px;
  height: 380px;
  bottom: -60px;
  right: -60px;
  background: radial-gradient(circle, #06b6d4 0%, rgba(6, 182, 212, 0.2) 70%, transparent 100%);
  animation: meshFloat2 15s ease-in-out infinite alternate;
}

.mesh-blob--3 {
  width: 320px;
  height: 320px;
  top: 45%;
  left: 45%;
  transform: translate(-50%, -50%);
  background: radial-gradient(circle, #a855f7 0%, rgba(168, 85, 247, 0.2) 70%, transparent 100%);
  animation: meshFloat3 14s ease-in-out infinite alternate;
}

@keyframes meshFloat1 {
  0% { transform: translate(0, 0) scale(1); }
  100% { transform: translate(60px, 40px) scale(1.1); }
}

@keyframes meshFloat2 {
  0% { transform: translate(0, 0) scale(1); }
  100% { transform: translate(-50px, -30px) scale(1.15); }
}

@keyframes meshFloat3 {
  0% { transform: translate(-50%, -50%) scale(0.9); }
  100% { transform: translate(-40%, -60%) scale(1.1); }
}

.hero__glow {
  position: absolute;
  top: 30%;
  left: 50%;
  width: 80vw;
  height: 80vw;
  max-width: 700px;
  max-height: 700px;
  transform: translate(-50%, -50%);
  background: radial-gradient(circle, color-mix(in srgb, var(--color-primary) 20%, transparent) 0%, transparent 60%);
  pointer-events: none;
  animation: pulseGlow 6s ease-in-out infinite alternate;
}

@keyframes pulseGlow {
  0% { opacity: 0.4; transform: translate(-50%, -50%) scale(0.9); }
  100% { opacity: 0.8; transform: translate(-50%, -50%) scale(1.15); }
}

.hero__particles {
  position: absolute;
  inset: 0;
  background-image: radial-gradient(var(--color-border) 1px, transparent 1px);
  background-size: 32px 32px;
  opacity: 0.35;
  mask-image: radial-gradient(ellipse 50% 50% at 50% 50%, #000 0%, transparent 100%);
  -webkit-mask-image: radial-gradient(ellipse 50% 50% at 50% 50%, #000 0%, transparent 100%);
}

.hero__content {
  position: relative;
  z-index: 10;
  max-width: 800px;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.hero__badge {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 16px;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-full);
  font-size: var(--text-xs);
  font-weight: 500;
  color: var(--color-primary);
  margin-bottom: 1.5rem;
}

.pulse-dot {
  width: 8px;
  height: 8px;
  background: var(--color-primary);
  border-radius: 50%;
  box-shadow: 0 0 0 0 var(--color-primary);
  animation: dotPulse 2s infinite;
}

@keyframes dotPulse {
  0% { box-shadow: 0 0 0 0 color-mix(in srgb, var(--color-primary) 60%, transparent); }
  70% { box-shadow: 0 0 0 10px transparent; }
  100% { box-shadow: 0 0 0 0 transparent; }
}

.hero__title {
  font-size: clamp(2.25rem, 5.5vw, 4.25rem);
  font-weight: 700;
  line-height: 1.15;
  margin-bottom: 1.25rem;
  letter-spacing: -0.02em;
}

/* Dark mode: tiêu đề hero bị chìm trên nền #042F2E — dùng --color-foreground
   (dark = #CCFBF1, token dự án) cho phần chữ thường; span .text-gradient giữ nguyên. */
:global(.dark) .hero__title {
  color: var(--color-foreground);
}

.text-gradient {
  background: linear-gradient(135deg, var(--color-primary), #06b6d4);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.hero__sub {
  font-size: clamp(0.95rem, 1.8vw, 1.15rem);
  color: var(--color-text-secondary);
  max-width: 620px;
  margin-bottom: 2rem;
  line-height: 1.6;
}

.hero__actions {
  display: flex;
  gap: 1rem;
  flex-wrap: wrap;
  justify-content: center;
}

.hero-btn {
  padding: 12px 28px;
  font-size: var(--text-sm);
  font-weight: 600;
  border-radius: var(--radius-full);
  text-decoration: none;
}

.btn-primary {
  background: var(--color-primary);
  color: var(--color-on-primary);
  border: 1px solid var(--color-primary);
  box-shadow: 0 4px 14px color-mix(in srgb, var(--color-primary) 35%, transparent);
}
.btn-primary:hover {
  opacity: 0.94;
  box-shadow: 0 6px 20px color-mix(in srgb, var(--color-primary) 50%, transparent);
}

.btn-ghost {
  background: var(--color-card);
  color: var(--color-foreground);
  border: 1px solid var(--color-border);
}
.btn-ghost:hover {
  background: var(--color-muted);
}

.hero__trust {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 2rem;
  margin-top: 2.5rem;
  padding: 1rem 2rem;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-full);
  flex-wrap: wrap;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
}

.trust-item {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.trust-number {
  font-size: 1.35rem;
  font-weight: 700;
  color: var(--color-primary);
}

.trust-label {
  font-size: 0.7rem;
  color: var(--color-text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.trust-divider {
  width: 1px;
  height: 24px;
  background: var(--color-border);
}

/* ── PREVIEW GRAPHIC & 3D TILT ── */
.hero__preview {
  margin-top: 3rem;
  width: 100%;
  max-width: 720px;
  perspective: 1000px;
}

.preview-window {
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: 0 24px 48px -12px rgba(15, 23, 42, 0.25), 0 0 40px color-mix(in srgb, var(--color-primary) 15%, transparent);
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.12);
  transform: rotateX(5deg) translateY(0);
  transition: transform 0.5s cubic-bezier(0.175, 0.885, 0.32, 1.275), box-shadow 0.5s ease;
}

.hero__preview:hover .preview-window {
  transform: rotateX(0deg) translateY(-8px);
  box-shadow: 0 32px 64px -12px rgba(15, 23, 42, 0.35), 0 0 60px color-mix(in srgb, var(--color-primary) 30%, transparent);
}

.preview-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: rgba(0, 0, 0, 0.35);
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.terminal-dots {
  display: flex;
  gap: 6px;
}

.terminal-dot {
  width: 9px;
  height: 9px;
  border-radius: 50%;
}
.terminal-dot--close { background: #f87171; }
.terminal-dot--min { background: #fbbf24; }
.terminal-dot--max { background: #34d399; }

.preview-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  background: rgba(255, 255, 255, 0.08);
  border: 1px solid rgba(255, 255, 255, 0.12);
  color: rgba(255, 255, 255, 0.85);
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}
.preview-btn:hover {
  background: var(--color-primary);
  border-color: var(--color-primary);
  color: white;
  box-shadow: 0 0 14px color-mix(in srgb, var(--color-primary) 30%, transparent);
  transform: scale(1.06);
}
.preview-btn:focus-visible {
  outline: 2px solid var(--color-primary);
  outline-offset: 2px;
}

.preview-body {
  height: 200px;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: flex-end;
  gap: 10px;
}

.bars-container {
  display: flex;
  align-items: flex-end;
  justify-content: center;
  gap: 12px;
  height: 128px;
  width: 100%;
}

.preview-phase-label {
  font-family: var(--font-mono);
  font-size: 11px;
  line-height: 1.4;
  color: #d9dde8;
  opacity: 0.85;
  text-align: center;
  min-height: 16px;
  margin: 0;
}

.preview-bar {
  width: 38px;
  border-radius: 4px 4px 0 0;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  padding-bottom: 4px;
  background: var(--color-data-core);
  transition: height 0.6s cubic-bezier(0.16, 1, 0.3, 1), background 0.35s ease, box-shadow 0.35s ease, transform 0.35s ease;
}

.bar--pivot {
  background: #fbbf24 !important;
  box-shadow: 0 0 16px rgba(251, 191, 36, 0.6);
  transform: translateY(-4px);
}

.bar--compare {
  background: #a855f7 !important;
  box-shadow: 0 0 14px rgba(168, 85, 247, 0.5);
}

.bar--swap {
  background: #f43f5e !important;
  box-shadow: 0 0 16px rgba(244, 63, 94, 0.6);
  transform: translateY(-6px);
}

.bar--sorted {
  background: #10b981 !important;
  box-shadow: 0 0 16px rgba(16, 185, 129, 0.6);
}

.preview-bar-label {
  font-family: var(--font-mono);
  font-size: 11px;
  font-weight: 700;
  color: white;
}

/* ━━ BENTO GRID (4 CỘT) ━━ */
.features-section {
  padding: 5rem 1.5rem;
  max-width: 1200px;
  margin: 0 auto;
}

.bento-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.5rem;
}

.bento-card {
  border-radius: var(--radius-xl);
  padding: 1.75rem;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.bento-large { grid-column: span 2; grid-row: span 2; }
.bento-medium { grid-column: span 2; grid-row: span 1; }
.bento-small { grid-column: span 1; grid-row: span 1; }

.bento-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background: var(--color-muted);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 1.25rem;
  transition: all 0.3s ease;
}
.bento-icon svg {
  width: 22px;
  height: 22px;
}

.bento-card:hover .bento-icon {
  transform: scale(1.1) rotate(4deg);
  box-shadow: 0 0 20px color-mix(in srgb, var(--color-primary) 40%, transparent);
}

/* Sorting Wave Animation on Large Bento Card — các cột thật, stagger theo delay */
.bento-visual.visual-sorting {
  position: relative;
  height: 90px;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  gap: 6px;
  background: color-mix(in srgb, var(--color-primary) 5%, transparent);
  border-radius: var(--radius-lg);
  padding: 10px;
}

.bento-wave-bar {
  width: 12px;
  background: var(--color-primary);
  border-radius: 3px 3px 0 0;
  animation: barsWave 2.4s ease-in-out infinite;
  transform-origin: bottom;
}

@keyframes barsWave {
  0%, 100% { transform: scaleY(1); }
  50% { transform: scaleY(1.25); }
}

/* ━━ ALGORITHM GRID ━━ */
.algogrid-section {
  padding: 4rem 1.5rem;
}

.home__catalog {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 1.25rem;
}

.home__catalog-search {
  position: relative;
  max-width: 480px;
  margin: 0 auto;
}
.home__catalog-search-icon {
  position: absolute;
  left: 1rem;
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-tertiary);
}
.home__catalog-search-input {
  width: 100%;
  height: 42px;
  padding: 0 2.5rem 0 2.75rem;
  border-radius: var(--radius-full);
  border: 1px solid var(--color-border);
  background: var(--color-card);
  font-size: var(--text-sm);
}
.home__catalog-search-clear {
  position: absolute;
  right: 0.75rem;
  top: 50%;
  transform: translateY(-50%);
  border: none;
  background: transparent;
  cursor: pointer;
  color: var(--color-text-tertiary);
}

.home__filters {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: 0.5rem;
}

.home__filter--active {
  background: var(--color-primary) !important;
  color: var(--color-on-primary) !important;
}

.home__catalog-card {
  display: flex;
  flex-direction: column;
}

.home__catalog-chips {
  display: flex;
  gap: 6px;
  margin-bottom: 1rem;
}

.home__chip {
  font-family: var(--font-mono);
  font-size: 10px;
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  background: var(--color-muted);
}

.home__catalog-cta {
  margin-top: auto;
  border-radius: var(--radius-full);
  border-color: var(--color-border-strong);
  padding-inline: 1rem;
  transition: all 0.2s ease;
}
.home__catalog-cta:hover {
  border-color: var(--color-primary);
  color: var(--color-primary);
  transform: translateX(4px);
}

/* ━━ 3 DEMOS SECTION ━━ */
.home__demos-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: var(--space-lg);
}

.home__demo {
  display: flex;
  flex-direction: column;
}

.home__demo-thumb {
  height: 90px;
  background: var(--color-canvas-ink);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-sm);
}

.home__thumb-bars {
  display: flex;
  align-items: flex-end;
  gap: 6px;
  height: 48px;
}
.home__thumb-bar {
  width: 10px;
  height: 24px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 2px;
}
.home__thumb-bar:nth-child(2) { height: 36px; }
.home__thumb-bar:nth-child(3) { height: 18px; }
.home__thumb-bar--done { background: #34d399; height: 44px; }

.home__thumb-row { display: flex; gap: 4px; }
.home__thumb-block { width: 22px; height: 22px; border-radius: 4px; background: rgba(255, 255, 255, 0.15); }
.home__thumb-block--found { background: var(--color-primary); }

.home__thumb-graph { display: flex; align-items: center; gap: 4px; }
.home__thumb-node { width: 14px; height: 14px; border-radius: 50%; background: rgba(255, 255, 255, 0.2); }
.home__thumb-node--visited { background: #34d399; }
.home__thumb-edge { width: 14px; height: 2px; background: rgba(255, 255, 255, 0.12); }

.home__demo-title {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-base);
  font-weight: 600;
}
.home__demo-meta {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 4px;
}
.home__demo-chips {
  display: flex;
  gap: 6px;
  margin-bottom: 1rem;
}

/* ━━ EXTENDED 2-COLUMN DEEP-DIVES ━━ */
.extended-section {
  padding: 5rem 1.5rem;
}

.extended-container {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 3.5rem;
}

@media (min-width: 960px) {
  .extended-container {
    flex-direction: row;
    justify-content: space-between;
  }
  .extended-section.reverse .extended-container {
    flex-direction: row-reverse;
  }
}

.extended-text {
  flex: 1;
  max-width: 500px;
}

.extended-visual {
  flex: 1;
  width: 100%;
  max-width: 540px;
  display: flex;
  justify-content: center;
}

/* Roadmap Mockup */
.roadmap-mockup {
  width: 100%;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
}

.rm-node {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  padding: 0.875rem 1rem;
  background: var(--color-card-raised);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  transition: all 0.3s ease;
}

.rm-node:hover {
  transform: translateX(6px);
  border-color: var(--color-primary);
  box-shadow: 0 4px 20px rgba(0, 126, 114, 0.2);
}

.rm-node.completed {
  border-color: rgba(16, 185, 129, 0.4);
  background: rgba(16, 185, 129, 0.05);
}

.rm-node.active {
  position: relative;
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 8%, transparent);
  box-shadow: 0 0 16px color-mix(in srgb, var(--color-primary) 30%, transparent);
}

.rm-pulse-dot {
  position: absolute;
  top: 10px;
  right: 12px;
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--color-primary);
  animation: rmPulse 2s infinite;
}

@keyframes rmPulse {
  0% { box-shadow: 0 0 0 0 color-mix(in srgb, var(--color-primary) 55%, transparent); }
  70% { box-shadow: 0 0 0 8px transparent; }
  100% { box-shadow: 0 0 0 0 transparent; }
}

.rm-icon {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--color-muted);
}

.rm-line {
  width: 2px;
  height: 24px;
  background: var(--color-border);
}
.rm-line.completed {
  background: #10b981;
}

/* Codelab Mockup */
.codelab-mockup {
  width: 100%;
  border-radius: var(--radius-xl);
  overflow: hidden;
}

/* AI Mockup */
.ai-mockup {
  width: 100%;
  padding: 1.5rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.ai-chat {
  max-width: 88%;
  padding: 0.875rem 1rem;
  border-radius: var(--radius-lg);
  font-size: 0.8125rem;
  line-height: 1.5;
}

.ai-chat.user {
  align-self: flex-end;
  background: var(--color-muted);
  border-bottom-right-radius: 0;
}

.ai-chat.bot {
  align-self: flex-start;
  background: color-mix(in srgb, var(--color-primary) 8%, transparent);
  border: 1px solid var(--color-border-strong);
  border-bottom-left-radius: 0;
  display: flex;
  gap: 0.75rem;
}

.ai-avatar {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--color-primary) 20%, transparent);
  color: var(--color-primary);
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.02em;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

/* ━━ TESTIMONIALS CAROUSEL ━━ */
.testimonials-section {
  padding: 5rem 1.5rem;
}

.testimonials-carousel {
  max-width: 760px;
  margin: 0 auto;
}

.testimonial-card {
  padding: 2.5rem 2rem;
  text-align: center;
}

/* Transition đổi testimonial — fade + slide nhẹ */
.fade-slide-enter-active,
.fade-slide-leave-active {
  transition: opacity 0.3s ease, transform 0.3s ease;
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateY(12px);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateY(-12px);
}

/* ━━ SANDBOX SECTION ━━ */
.home__sandbox-section {
  padding-block: 4rem;
}

.home__sandbox-card {
  padding: 2rem;
}

.home__sandbox-stage {
  position: relative;
}

.home__sandbox-live {
  position: absolute;
  top: 8px;
  left: 12px;
  font-size: 10px;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: rgba(255, 255, 255, 0.55);
}

.home__sandbox-bar--active {
  background: #fbbf24 !important;
  color: #000 !important;
  transform: translateY(-8px) scale(1.05);
  box-shadow: 0 0 16px rgba(251, 191, 36, 0.6);
}

.home__sandbox-bar--sorted {
  background: #10b981 !important;
  color: #fff !important;
  box-shadow: 0 0 12px rgba(16, 185, 129, 0.4);
}

/* ━━ CTA SECTION ━━ */
.cta-section {
  padding: 4rem 1.5rem;
  display: flex;
  justify-content: center;
}

.cta-card {
  text-align: center;
  padding: 3.5rem 2rem;
  border-radius: var(--radius-2xl);
  width: 100%;
  max-width: 800px;
  background: linear-gradient(135deg, color-mix(in srgb, var(--color-primary) 12%, var(--color-card)), var(--color-card));
  border: 1px solid var(--color-border);
  box-shadow: 0 0 50px color-mix(in srgb, var(--color-primary) 15%, transparent);
}

/* ━━ RESPONSIVE ━━ */
@media (max-width: 1024px) {
  .bento-grid { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 640px) {
  .bento-grid { grid-template-columns: 1fr; }
  .bento-large, .bento-medium, .bento-small { grid-column: span 1; grid-row: span 1; }
  .hero__trust {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: 1rem 2rem;
    padding: 1.25rem 1rem;
    border-radius: var(--radius-lg);
  }
  .trust-divider { display: none; }
  .dashboard__grid { grid-template-columns: 1fr; }
}
</style>
