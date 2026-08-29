<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import type { Component } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  Check,
  Circle,
  Clock,
  Download,
  Flame,
  Frame,
  Image as ImageIcon,
  KeyRound,
  Lock,
  Medal,
  Package,
  Pencil,
  RefreshCw,
  ShoppingBag,
  Target,
  Trophy,
  Users,
  User,
  Mail,
  ShieldCheck,
  Calendar,
  Sparkles,
  Upload,
  Crown,
} from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useProgressStore } from '@/stores/progress';
import * as progressApi from '@/api/progress';
import * as authApi from '@/api/auth';
import type { InventoryItemDto } from '@/api/gamification';
import { avatarImageUrl, avatarVariant, equipGroup, equippedItem, frameVariant } from '@/utils/equipment';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import Tabs from '@/components/ui/Tabs.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import Input from '@/components/ui/Input.vue';
import Modal from '@/components/ui/Modal.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';
import { messages } from '@/i18n/vi';

// Gamification dashboard
import XpProgressCard from '@/components/gamification/XpProgressCard.vue';
import StreakCard from '@/components/gamification/StreakCard.vue';
import QuestProgressCard from '@/components/gamification/QuestProgressCard.vue';
import BadgeGrid from '@/components/gamification/BadgeGrid.vue';

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const gamification = useGamificationStore();
const progressStore = useProgressStore();
const ui = useUiStore();

const tab = ref<'overview' | 'progress' | 'achievements' | 'inventory' | 'settings'>('overview');
const loading = ref(true);
const loadError = ref('');

// Profile Name Form
const displayNameInput = ref('');
const nameUpdating = ref(false);

// Password Form
const passwordForm = ref({ current: '', next: '' });
const passwordError = ref('');
const passwordBusy = ref(false);

// Avatar management
const avatarUrlInput = ref('');
const avatarError = ref('');
const avatarUploading = ref(false);
const avatarFileInput = ref<HTMLInputElement | null>(null);
const avatarLocalFile = ref<File | null>(null);
const avatarLocalPreview = ref<string | null>(null);

const PRESET_AVATARS = [
  { key: 'cyber', name: 'Cyber Hacker', url: '/assets/avatars/cyber-hacker.svg' },
  { key: 'gold', name: 'Gold Knight', url: '/assets/avatars/gold-knight.svg' },
  { key: 'neon', name: 'Neon Ninja', url: '/assets/avatars/neon-ninja.svg' },
  { key: 'wizard', name: 'Wizard', url: '/assets/avatars/wizard.svg' },
  { key: 'bot', name: 'AI Bot', url: '/assets/avatars/ai-bot.svg' },
];

const isPremiumUser = computed(() => {
  return gamification.isPremium || auth.user?.role === 'ADMIN' || auth.user?.role === 'TEACHER';
});

function triggerDeviceUpload(): void {
  if (!isPremiumUser.value) {
    ui.showToast('Tính năng tải ảnh từ thiết bị chỉ dành cho tài khoản Premium.', 'warning');
    router.push('/premium');
    return;
  }
  avatarFileInput.value?.click();
}

function onAvatarFileChange(event: Event): void {
  if (!isPremiumUser.value) {
    avatarError.value = 'Tính năng tải ảnh từ thiết bị chỉ dành cho tài khoản Premium.';
    return;
  }
  const input = event.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const file = input.files[0];
  if (file.size > 3 * 1024 * 1024) {
    avatarError.value = 'Kích thước ảnh không được vượt quá 3MB.';
    return;
  }
  avatarError.value = '';
  avatarLocalFile.value = file;
  const reader = new FileReader();
  reader.onload = (e) => {
    avatarLocalPreview.value = e.target?.result as string;
  };
  reader.readAsDataURL(file);
}

function cancelLocalAvatar(): void {
  avatarLocalFile.value = null;
  avatarLocalPreview.value = null;
  if (avatarFileInput.value) avatarFileInput.value.value = '';
}

async function uploadLocalAvatar(): Promise<void> {
  if (!avatarLocalFile.value) return;
  if (!isPremiumUser.value) {
    avatarError.value = 'Tính năng tải ảnh từ thiết bị chỉ dành cho tài khoản Premium.';
    return;
  }
  avatarUploading.value = true;
  avatarError.value = '';
  try {
    const base64 = await new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = reject;
      reader.readAsDataURL(avatarLocalFile.value!);
    });

    const res = await fetch('/api/upload-avatar', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        image: base64,
        name: avatarLocalFile.value.name,
      }),
    });

    const data = await res.json();
    if (!res.ok || !data?.url) {
      throw new Error(data?.error || 'Không thể tải ảnh lên máy chủ lưu trữ.');
    }

    await authApi.updateProfile({ avatarUrl: data.url });
    await auth.fetchMe();
    ui.showToast('Tải lên và cập nhật ảnh đại diện từ thiết bị thành công!', 'success');
    cancelLocalAvatar();
  } catch (err) {
    avatarError.value = err instanceof Error ? err.message : 'Tải ảnh thất bại.';
  } finally {
    avatarUploading.value = false;
  }
}

async function updateAvatarUrl(url: string | null): Promise<void> {
  if (url) {
    const trimmed = url.trim();
    if (trimmed.length > 500) {
      avatarError.value = 'Đường dẫn ảnh không được vượt quá 500 ký tự.';
      return;
    }
    if (!trimmed.startsWith('http://') && !trimmed.startsWith('https://') && !trimmed.startsWith('/')) {
      avatarError.value = 'Đường dẫn ảnh phải bắt đầu bằng https://, http:// hoặc /assets/...';
      return;
    }
  }

  avatarUploading.value = true;
  avatarError.value = '';
  try {
    await authApi.updateProfile({ avatarUrl: url ? url.trim() : null });
    await auth.fetchMe();
    ui.showToast(url ? 'Cập nhật ảnh đại diện thành công!' : 'Đã xóa ảnh đại diện về mặc định.', 'success');
    avatarUrlInput.value = '';
    cancelLocalAvatar();
  } catch (err) {
    avatarError.value = err instanceof Error ? err.message : 'Không thể cập nhật ảnh.';
  } finally {
    avatarUploading.value = false;
  }
}

async function onUpdateDisplayName(): Promise<void> {
  if (displayNameInput.value.trim().length < 2) {
    ui.showToast('Họ và tên phải từ 2 ký tự trở lên.', 'warning');
    return;
  }
  nameUpdating.value = true;
  try {
    await authApi.updateProfile({ displayName: displayNameInput.value.trim() });
    await auth.fetchMe();
    ui.showToast('Cập nhật họ tên thành công!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Cập nhật thất bại.', 'error');
  } finally {
    nameUpdating.value = false;
  }
}

const level = computed(() => gamification.level);
const xp = computed(() => gamification.xp);
const isTeacherPending = computed(() => auth.role === 'TEACHER_PENDING');

onMounted(async () => {
  loading.value = true;
  // Initialize tab from route query if present
  const qTab = route.query.tab as string;
  if (qTab && ['overview', 'progress', 'achievements', 'inventory', 'settings'].includes(qTab)) {
    tab.value = qTab as typeof tab.value;
  }

  await Promise.allSettled([
    gamification.fetchAll(),
    gamification.fetchQuests(),
    gamification.fetchInventory(),
    gamification.fetchAchievements(),
    auth.fetchMe().catch(() => undefined),
  ]);

  if (auth.user) {
    displayNameInput.value = auth.user.displayName || '';
  }

  try {
    await progressStore.fetchOverview();
  } catch {
    loadError.value = messages.profile.progressLoadError;
  }
  loading.value = false;
});

watch(
  () => route.query.tab,
  (newTab) => {
    if (newTab && typeof newTab === 'string' && ['overview', 'progress', 'achievements', 'inventory', 'settings'].includes(newTab)) {
      tab.value = newTab as typeof tab.value;
    }
  },
);

async function retryOverview(): Promise<void> {
  loadError.value = '';
  loading.value = true;
  try {
    await progressStore.fetchOverview();
  } catch {
    loadError.value = messages.profile.progressLoadError;
  }
  loading.value = false;
}

function changeTab(next: string): void {
  tab.value = next as typeof tab.value;
  void router.replace({ query: { ...route.query, tab: next } });
}

function goSettings(): void {
  changeTab('settings');
}

async function onChangePassword(): Promise<void> {
  passwordError.value = '';
  if (passwordForm.value.next.length < 6) {
    passwordError.value = messages.profile.passwordTooShort;
    return;
  }
  passwordBusy.value = true;
  try {
    await authApi.changePassword({ currentPassword: passwordForm.value.current, newPassword: passwordForm.value.next });
    ui.showToast(messages.profile.passwordChanged, 'success');
    passwordForm.value = { current: '', next: '' };
  } catch (err) {
    passwordError.value = err instanceof Error ? err.message : messages.profile.passwordTooShort;
  } finally {
    passwordBusy.value = false;
  }
}

// ── 2FA State & Actions (B1) ──
const isDevMode = import.meta.env.DEV;
const twoFactorModalOpen = ref(false);
const twoFactorDisableModalOpen = ref(false);
const twoFactorBusy = ref(false);
const twoFactorError = ref('');
const twoFactorSending = ref(false);

const twoFaDigits = reactive<string[]>(['', '', '', '', '', '']);
const twoFaCode = computed(() => twoFaDigits.join(''));
const twoFaExpiresSeconds = ref(300);
const twoFaResendCooldownSeconds = ref(0);

let twoFaTimerInterval: number | null = null;
let twoFaCooldownInterval: number | null = null;

const twoFaTimeFormatted = computed(() => {
  const m = Math.floor(twoFaExpiresSeconds.value / 60);
  const s = twoFaExpiresSeconds.value % 60;
  return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
});

function startTwoFaTimer(seconds = 300): void {
  if (twoFaTimerInterval) clearInterval(twoFaTimerInterval);
  twoFaExpiresSeconds.value = seconds;
  twoFaTimerInterval = window.setInterval(() => {
    if (twoFaExpiresSeconds.value > 0) {
      twoFaExpiresSeconds.value--;
    } else {
      if (twoFaTimerInterval) clearInterval(twoFaTimerInterval);
    }
  }, 1000);
}

function startTwoFaCooldown(seconds = 60): void {
  if (twoFaCooldownInterval) clearInterval(twoFaCooldownInterval);
  twoFaResendCooldownSeconds.value = seconds;
  twoFaCooldownInterval = window.setInterval(() => {
    if (twoFaResendCooldownSeconds.value > 0) {
      twoFaResendCooldownSeconds.value--;
    } else {
      if (twoFaCooldownInterval) clearInterval(twoFaCooldownInterval);
    }
  }, 1000);
}

onBeforeUnmount(() => {
  if (twoFaTimerInterval) clearInterval(twoFaTimerInterval);
  if (twoFaCooldownInterval) clearInterval(twoFaCooldownInterval);
});

function onTwoFaDigitInput(index: number, event: Event): void {
  const target = event.target as HTMLInputElement;
  const val = target.value.replace(/\D/g, '');

  if (!val) {
    twoFaDigits[index] = '';
    return;
  }

  if (val.length > 1) {
    const chars = val.slice(0, 6).split('');
    chars.forEach((c, idx) => {
      if (index + idx < 6) {
        twoFaDigits[index + idx] = c;
      }
    });
    const nextIdx = Math.min(index + chars.length, 5);
    focusTwoFaDigit(nextIdx);
    return;
  }

  twoFaDigits[index] = val;
  if (index < 5) {
    focusTwoFaDigit(index + 1);
  }
}

function onTwoFaDigitKeyDown(index: number, event: KeyboardEvent): void {
  if (event.key === 'Backspace') {
    if (!twoFaDigits[index] && index > 0) {
      twoFaDigits[index - 1] = '';
      focusTwoFaDigit(index - 1);
    } else {
      twoFaDigits[index] = '';
    }
  } else if (event.key === 'ArrowLeft' && index > 0) {
    focusTwoFaDigit(index - 1);
  } else if (event.key === 'ArrowRight' && index < 5) {
    focusTwoFaDigit(index + 1);
  }
}

function onTwoFaDigitPaste(event: ClipboardEvent): void {
  event.preventDefault();
  const pasted = event.clipboardData?.getData('text') ?? '';
  const digitsOnly = pasted.replace(/\D/g, '').slice(0, 6);
  if (digitsOnly) {
    digitsOnly.split('').forEach((char, idx) => {
      if (idx < 6) twoFaDigits[idx] = char;
    });
    const nextIdx = Math.min(digitsOnly.length, 5);
    focusTwoFaDigit(nextIdx);
  }
}

function focusTwoFaDigit(index: number): void {
  const el = document.getElementById(`twofa-digit-${index}`) as HTMLInputElement | null;
  el?.focus();
  el?.select();
}

async function openEnable2FaModal(): Promise<void> {
  twoFaDigits.splice(0, 6, '', '', '', '', '', '');
  twoFactorError.value = '';
  twoFactorBusy.value = true;
  try {
    const res = await authApi.send2FaCode();
    startTwoFaTimer(res.expiresInSeconds || 300);
    startTwoFaCooldown(60);
    twoFactorModalOpen.value = true;
    setTimeout(() => focusTwoFaDigit(0), 150);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể gửi mã OTP xác thực.', 'error');
  } finally {
    twoFactorBusy.value = false;
  }
}

async function handleResend2FaOtp(): Promise<void> {
  if (twoFaResendCooldownSeconds.value > 0 || twoFactorSending.value) return;
  twoFactorSending.value = true;
  twoFactorError.value = '';
  try {
    const res = await authApi.send2FaCode();
    startTwoFaTimer(res.expiresInSeconds || 300);
    startTwoFaCooldown(60);
    twoFaDigits.splice(0, 6, '', '', '', '', '', '');
    setTimeout(() => focusTwoFaDigit(0), 100);
    ui.showToast(res.message || 'Đã gửi lại mã OTP đến email.', 'success');
  } catch (err) {
    twoFactorError.value = err instanceof Error ? err.message : 'Không thể gửi lại mã OTP.';
  } finally {
    twoFactorSending.value = false;
  }
}

async function handleVerify2Fa(): Promise<void> {
  const code = twoFaCode.value.trim();
  if (code.length !== 6) {
    twoFactorError.value = 'Vui lòng nhập đủ 6 chữ số OTP.';
    return;
  }
  twoFactorBusy.value = true;
  twoFactorError.value = '';
  try {
    await authApi.verify2FaCode(code);
    await auth.fetchMe();
    twoFactorModalOpen.value = false;
    ui.showToast('Đã bật xác thực hai lớp (2FA) thành công!', 'success');
  } catch (err) {
    twoFactorError.value = err instanceof Error ? err.message : 'Mã OTP không chính xác hoặc đã hết hạn.';
  } finally {
    twoFactorBusy.value = false;
  }
}

function openDisable2FaModal(): void {
  twoFactorDisableModalOpen.value = true;
}

async function handleConfirmDisable2Fa(): Promise<void> {
  twoFactorBusy.value = true;
  try {
    await authApi.toggle2Fa(false);
    await auth.fetchMe();
    twoFactorDisableModalOpen.value = false;
    ui.showToast('Đã tắt xác thực hai lớp (2FA).', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tắt 2FA.', 'error');
  } finally {
    twoFactorBusy.value = false;
  }
}

const overview = computed(() => progressStore.overview);

const levelProgressPct = computed(() => {
  const o = overview.value;
  if (!o || o.lessonsTotal === 0) return 0;
  return Math.min(100, Math.round((o.lessonsViewed / o.lessonsTotal) * 100));
});

const quickLinks: Array<{ label: string; to: string; icon: Component }> = [
  { label: 'Thử thách hằng ngày', to: 'quests', icon: Target },
  { label: 'Bảng xếp hạng', to: 'leaderboard', icon: Trophy },
  { label: 'Cửa hàng', to: 'shop', icon: ShoppingBag },
  { label: 'Lớp học', to: 'classes', icon: Users },
];

const achievements = computed(() => gamification.achievements);

// ── Kho đồ (Màn N-8): avatar + khung viền từ /me/inventory (data thật) ──
const equippingId = ref<number | null>(null);

const avatarItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === 'avatar'));
const frameItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === 'frame'));
const consumableItems = computed(() => gamification.inventory.filter((item) => equipGroup(item) === null));

const invGroups = computed(() =>
  [
    { key: 'avatar', label: 'Avatar tùy biến', icon: ImageIcon, items: avatarItems.value },
    { key: 'frame', label: 'Khung viền hồ sơ', icon: Frame, items: frameItems.value },
  ].filter((group) => group.items.length > 0),
);

const equippedAvatar = computed(() => equippedItem(gamification.inventory, 'avatar'));
const equippedFrame = computed(() => equippedItem(gamification.inventory, 'frame'));

const avatarThemeClass = computed(() => {
  const key = equippedAvatar.value?.itemKey;
  return key ? `profile__avatar--${avatarVariant(key)}` : '';
});
const frameThemeClass = computed(() => {
  const key = equippedFrame.value?.itemKey;
  return key ? `profile__avatar-frame--${frameVariant(key)}` : '';
});

async function toggleEquip(item: InventoryItemDto): Promise<void> {
  equippingId.value = item.itemId;
  try {
    await gamification.equipItem(item.itemId, !item.isEquipped);
    ui.showToast(item.isEquipped ? 'Đã gỡ trang bị.' : `Đã trang bị "${item.name}".`, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể trang bị vật phẩm.', 'error');
  } finally {
    equippingId.value = null;
  }
}

// ── Skill radar (vue-echarts) ──
const skillData = computed(() =>
  (overview.value?.topics ?? []).map((topic) => ({
    name: topic.name.length > 18 ? `${topic.name.slice(0, 18)}…` : topic.name,
    value: topic.progressPct,
  })),
);

function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

const radarOption = computed(() => {
  void ui.theme;
  const indexMuted = cssVar('--color-index-muted', '#6B7385');
  const dataCore = cssVar('--vdsa-purple', '#8b5cf6');
  const ink = cssVar('--color-canvas-ink', '#0D1020');

  return {
    tooltip: {
      trigger: 'item' as const,
      backgroundColor: ink,
      borderColor: indexMuted,
      textStyle: { color: indexMuted, fontSize: 12 },
    },
    radar: {
      indicator: skillData.value.map((skill) => ({ name: skill.name, max: 100 })),
      radius: '68%',
      splitNumber: 4,
      axisName: { color: indexMuted, fontSize: 11 },
      splitLine: { lineStyle: { color: indexMuted, opacity: 0.35 } },
      splitArea: {
        areaStyle: {
          color: ['rgba(139,92,246,0.06)', 'rgba(139,92,246,0.12)', 'rgba(139,92,246,0.18)', 'rgba(139,92,246,0.24)', 'rgba(139,92,246,0.3)'],
        },
      },
      axisLine: { lineStyle: { color: indexMuted } },
    },
    series: [
      {
        type: 'radar' as const,
        data: [
          {
            value: skillData.value.map((skill) => skill.value),
            name: 'Độ phủ kỹ năng',
          },
        ],
        areaStyle: { color: 'rgba(139,92,246,0.25)' },
        lineStyle: { color: dataCore, width: 2 },
        symbol: 'circle' as const,
        symbolSize: 5,
        itemStyle: { color: dataCore },
      },
    ],
  };
});

async function reloadProgress(): Promise<void> {
  try {
    await progressStore.fetchOverview();
    ui.showToast('Đã tải lại tiến độ.', 'success');
  } catch {
    ui.showToast(messages.profile.progressLoadError, 'error');
  }
}

function csvExport(): void {
  void progressApi.fetchOverview().then(async (data) => {
    const rows = [
      ['lessonsViewed', data.lessonsViewed],
      ['lessonsTotal', data.lessonsTotal],
      ['exercisesCompleted', data.exercisesCompleted],
      ['exercisesTotal', data.exercisesTotal],
      ['avgScore', data.avgScore ?? ''],
    ];
    const csv = '\uFEFF' + rows.map((r) => r.join(',')).join('\n');
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'progress.csv';
    link.click();
    URL.revokeObjectURL(url);
  });
}
</script>

<template>
  <main class="profile container">
    <!-- Banner hồ sơ giảng viên chờ duyệt (TEACHER_PENDING) -->
    <div v-if="isTeacherPending" class="profile__banner" role="status">
      <Clock :size="16" aria-hidden="true" />
      <span>Hồ sơ giảng viên của bạn đang chờ xét duyệt — tính năng giảng dạy sẽ mở khóa khi Admin duyệt.</span>
    </div>

    <!-- Hero profile card — surface band level-2 -->
    <header class="profile__hero">
      <div class="profile__hero-main">
        <span class="profile__avatar-frame" :class="frameThemeClass">
          <img
            v-if="equippedAvatar && avatarImageUrl(equippedAvatar.itemKey)"
            :src="avatarImageUrl(equippedAvatar.itemKey)"
            :alt="equippedAvatar.name"
            class="profile__avatar profile__avatar-image"
            :class="avatarThemeClass"
          />
          <img
            v-else-if="auth.user?.avatarUrl"
            :src="auth.user.avatarUrl"
            :alt="auth.user?.displayName ?? 'Avatar'"
            class="profile__avatar profile__avatar-image"
          />
          <span v-else class="profile__avatar" :class="avatarThemeClass" aria-hidden="true">
            {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
          </span>
        </span>
        <div class="profile__identity">
          <h1 class="profile__name">{{ auth.user?.displayName ?? 'Người dùng' }}</h1>
          <p class="profile__email">{{ auth.user?.email }}</p>
          <div class="profile__chips">
            <Badge variant="primary">Lv {{ level }}</Badge>
            <Badge variant="success" class="profile__streak-chip"><Flame :size="12" class="profile__flame" /> {{ gamification.streakDays }} ngày streak</Badge>
            <Badge v-if="gamification.isPremium" variant="warning">Premium</Badge>
            <Badge v-if="auth.role === 'TEACHER'" variant="secondary">Giảng viên</Badge>
            <Badge v-else-if="auth.role === 'ADMIN'" variant="success">Quản trị viên</Badge>
          </div>
        </div>
        <div class="profile__actions">
          <Button variant="secondary" size="sm" @click="goSettings">
            <Pencil :size="14" /> Chỉnh sửa
          </Button>
        </div>
      </div>

      <!-- Stat hierarchy: 1 hero duy nhất = XP -->
      <div class="profile__stats-row">
        <BlockToken
          label="XP"
          :value="xp.toLocaleString('vi-VN')"
          index="01 · tích lũy"
          class="profile__stats-hero"
        />
        <div class="profile__stat-block profile__stat-block--level">
          <span class="profile__stat-label">Level</span>
          <div class="profile__stat-line">
            <span class="profile__stat-value">{{ level }}</span>
            <span class="profile__stat-unit">CẤP</span>
          </div>
        </div>
        <BlockToken size="sm" tone="resolved" label="Streak" :value="gamification.streakDays" index="ngày" class="profile__streak-token" />
        <div class="profile__stat-block profile__stat-block--gems">
          <span class="profile__stat-label">Gems</span>
          <div class="profile__stat-line">
            <span class="profile__stat-value">{{ gamification.gems }}</span>
            <span class="profile__stat-unit">GEMS</span>
          </div>
        </div>
        <div class="profile__stat-block profile__stat-block--hearts">
          <span class="profile__stat-label">Tim</span>
          <div class="profile__stat-line">
            <span class="profile__stat-value">{{ gamification.hearts }}/{{ gamification.heartsMax }}</span>
            <span class="profile__stat-unit">TIM</span>
          </div>
        </div>
      </div>

      <div class="profile__level-progress">
        <div class="profile__level-progress-head">
          <span class="profile__level-progress-label">Tiến độ lộ trình</span>
          <span class="profile__level-progress-note">
            {{ overview?.lessonsViewed ?? 0 }}/{{ overview?.lessonsTotal ?? 0 }} bài học
          </span>
        </div>
        <ProgressBar
          :value="levelProgressPct"
          show-label
          :variant="levelProgressPct >= 100 ? 'success' : 'default'"
        />
      </div>
    </header>

    <!-- Tabs: Tổng quan / Tiến độ / Túi đồ / Thành tích / Cài đặt -->
    <Tabs
      :tabs="[
        { key: 'overview', label: 'Tổng quan' },
        { key: 'progress', label: 'Tiến độ' },
        { key: 'inventory', label: 'Túi đồ', badge: gamification.inventory.length > 0 ? gamification.inventory.length : undefined },
        { key: 'achievements', label: 'Thành tích' },
        { key: 'settings', label: 'Cài đặt' },
      ]"
      v-model="tab"
      @change="changeTab"
    />

    <div v-if="loading" class="profile__loading" aria-busy="true">
      <Skeleton v-for="i in 4" :key="i" height="48px" />
    </div>

    <!-- ═══ TAB 1: TỔNG QUAN ═══ -->
    <section v-else-if="tab === 'overview'" class="profile__panel">
      <EmptyState
        v-if="loadError"
        icon="alert-circle"
        title="Không tải được tiến độ"
        :description="loadError"
        :action-label="messages.common.retry"
        @action="retryOverview"
      />
      <template v-else>
        <div class="profile__overview-grid">
          <div class="card profile__overview-card">
            <h2 class="profile__panel-title">Tiến độ tổng</h2>
            <p class="text-muted">Bài học đã xem: <span class="profile__mono">{{ overview?.lessonsViewed ?? 0 }}/{{ overview?.lessonsTotal ?? 0 }}</span></p>
            <p class="text-muted">Bài tập đã làm: <span class="profile__mono">{{ overview?.exercisesCompleted ?? 0 }}/{{ overview?.exercisesTotal ?? 0 }}</span></p>
            <p class="text-muted">Điểm trung bình: <span class="profile__mono">{{ overview?.avgScore ?? '—' }}</span></p>
            <div class="profile__overview-progress">
              <span class="text-muted">Hoàn thành bài học</span>
              <ProgressBar :value="levelProgressPct" :variant="levelProgressPct >= 100 ? 'success' : 'default'" />
            </div>
          </div>
          <div class="card profile__overview-card">
            <h2 class="profile__panel-title">Điểm đến nhanh</h2>
            <div class="profile__quick">
              <RouterLink v-for="(link, i) in quickLinks" :key="link.to" class="profile__quick-link" :to="{ name: link.to }">
                <span class="profile__quick-idx">{{ String(i + 1).padStart(2, '0') }}</span>
                <component :is="link.icon" :size="16" aria-hidden="true" />
                {{ link.label }}
              </RouterLink>
            </div>
          </div>
        </div>

        <!-- Skill radar -->
        <div class="card profile__radar-card">
          <div class="profile__radar-head">
            <h2 class="profile__panel-title">Skill radar</h2>
            <span class="text-muted">Độ phủ kỹ năng theo chủ đề</span>
          </div>
          <div v-if="skillData.length > 1" class="profile__radar-canvas">
            <VChartLazy :option="radarOption" height="300px" />
          </div>
          <EmptyState
            v-else
            icon="target"
            title="Chưa có dữ liệu kỹ năng"
            description="Hoàn thành bài học/bài tập trong từng chủ đề để vẽ radar kỹ năng của bạn."
          />
          <p class="profile__radar-note">
            Điểm mỗi trục = phần trăm hoàn thành chủ đề tương ứng (bài học + bài tập), tính từ tiến độ thực tế.
          </p>
        </div>

        <!-- Gamification dashboard -->
        <div class="profile__gamification">
          <div class="profile__gamification-row">
            <XpProgressCard
              :level="gamification.level"
              :xp="gamification.xp"
              :xp-into-level="gamification.xpIntoLevel"
              :xp-for-next-level="gamification.xpForNextLevel"
              :level-progress-pct="gamification.levelProgressPct"
              :loading="gamification.loading"
            />
            <StreakCard
              :streak-days="gamification.streakDays"
              :freeze-available="gamification.freezeAvailable"
              :loading="gamification.loading"
            />
          </div>
          <QuestProgressCard :quests="gamification.quests" :loading="gamification.loading" />
          <BadgeGrid :badges="gamification.achievements" :loading="gamification.loading" />
        </div>
      </template>
    </section>

    <!-- ═══ TAB 2: TIẾN ĐỘ ═══ -->
    <section v-else-if="tab === 'progress'" class="profile__panel">
      <div class="profile__progress-actions">
        <Button variant="secondary" size="sm" @click="reloadProgress">
          <RefreshCw :size="14" /> Làm mới
        </Button>
        <Button variant="ghost" size="sm" @click="csvExport">
          <Download :size="14" /> Xuất CSV
        </Button>
      </div>
      <EmptyState
        v-if="loadError"
        icon="alert-circle"
        title="Không tải được tiến độ"
        :description="loadError"
        :action-label="messages.common.retry"
        @action="retryOverview"
      />
      <EmptyState
        v-else-if="!overview || overview.topics.length === 0"
        icon="target"
        title="Chưa có dữ liệu tiến độ"
        description="Học vài bài học đầu tiên để thấy tiến độ ở đây."
      />
      <div v-else class="profile__topics">
        <article v-for="topic in overview.topics" :key="topic.id" class="card profile__topic">
          <div class="profile__topic-head">
            <h3 class="profile__topic-name">{{ topic.name }}</h3>
            <span class="profile__topic-pct">{{ topic.progressPct }}%</span>
          </div>
          <ProgressBar :value="topic.progressPct" :variant="topic.progressPct >= 100 ? 'success' : 'default'" />
          <ul class="profile__topic-lessons">
            <li v-for="lesson in topic.lessons" :key="lesson.id" class="profile__topic-lesson">
              <span :class="lesson.completed ? 'profile__done' : 'profile__todo'">
                <Check v-if="lesson.completed" :size="14" aria-hidden="true" />
                <Circle v-else :size="10" aria-hidden="true" />
                {{ lesson.title }}
              </span>
              <Badge v-if="lesson.bestScore !== null" variant="primary">{{ lesson.bestScore }} điểm</Badge>
            </li>
          </ul>
        </article>
      </div>
    </section>

    <!-- ═══ TAB 3: TÚI ĐỒ ═══ -->
    <section v-else-if="tab === 'inventory'" class="profile__panel">
      <div v-if="gamification.inventory.length > 0" class="profile__inv-groups">
        <section v-for="group in invGroups" :key="group.key" class="profile__inv-group">
          <h2 class="profile__panel-title profile__inv-title">{{ group.label }}</h2>
          <div class="profile__inv-grid">
            <article
              v-for="item in group.items"
              :key="item.id"
              class="card profile__inv-card"
              :class="{ 'profile__inv-card--equipped': item.isEquipped }"
            >
              <span class="profile__inv-icon" aria-hidden="true">
                <img
                  v-if="avatarImageUrl(item.itemKey)"
                  :src="avatarImageUrl(item.itemKey)"
                  :alt="item.name"
                  class="w-10 h-10 rounded-full object-cover shadow-sm"
                />
                <component :is="group.icon" v-else :size="20" />
              </span>
              <div class="profile__inv-body">
                <p class="profile__inv-name">{{ item.name }}</p>
                <div class="flex items-center gap-1.5 mt-0.5">
                  <Badge v-if="item.isEquipped" variant="success" class="text-[10px]">✨ Đang dùng</Badge>
                  <Badge v-else variant="muted" class="text-[10px]">x{{ item.quantity }}</Badge>
                </div>
              </div>
              <Button
                size="sm"
                :variant="item.isEquipped ? 'secondary' : 'primary'"
                :loading="equippingId === item.itemId"
                :disabled="equippingId !== null && equippingId !== item.itemId"
                @click="toggleEquip(item)"
              >
                {{ item.isEquipped ? 'Gỡ trang bị' : 'Trang bị' }}
              </Button>
            </article>
          </div>
        </section>

        <section v-if="consumableItems.length > 0" class="profile__inv-group">
          <h2 class="profile__panel-title profile__inv-title">Vật phẩm tiêu hao & hỗ trợ</h2>
          <ul class="card profile__inv-other">
            <li v-for="item in consumableItems" :key="item.id" class="profile__inv-other-row">
              <div class="flex items-center gap-2">
                <Package :size="16" class="text-vdsa-purple" aria-hidden="true" />
                <span class="profile__inv-name">{{ item.name }}</span>
              </div>
              <Badge variant="primary">Số lượng: {{ item.quantity }}</Badge>
            </li>
          </ul>
        </section>
      </div>

      <EmptyState
        v-else
        icon="package"
        title="Túi đồ trống"
        description="Bạn chưa sở hữu vật phẩm nào. Ghé thăm Cửa hàng để trang bị avatar và khung viền độc đáo!"
        action-label="Đến Cửa hàng ngay"
        @action="router.push('/shop')"
      />
    </section>

    <!-- ═══ TAB 4: THÀNH TÍCH ═══ -->
    <section v-else-if="tab === 'achievements'" class="profile__panel">
      <div class="profile__achievements">
        <div
          v-for="ach in achievements"
          :key="ach.id"
          class="profile__achievement"
          :class="{ 'profile__achievement--locked': !ach.earnedAt }"
        >
          <span class="profile__achievement-icon" :class="{ 'profile__achievement-icon--open': ach.earnedAt }">
            <img v-if="ach.iconUrl" :src="ach.iconUrl" :alt="ach.name" class="profile__achievement-img" />
            <component :is="ach.earnedAt ? Medal : Lock" v-else :size="20" aria-hidden="true" />
          </span>
          <p class="profile__achievement-label">{{ ach.name }}</p>
          <p v-if="ach.description" class="profile__achievement-desc">{{ ach.description }}</p>
          <Badge :variant="ach.earnedAt ? 'success' : 'muted'">{{ ach.earnedAt ? 'Đã mở' : 'Chưa mở' }}</Badge>
        </div>
      </div>
      <EmptyState
        v-if="achievements.every((a) => !a.earnedAt)"
        icon="party-popper"
        title="Chưa có huy hiệu"
        description="Bắt đầu học để mở huy hiệu đầu tiên nhé!"
      />
    </section>

    <!-- ═══ TAB 5: CÀI ĐẶT ═══ -->
    <section v-else class="profile__panel">
      <div class="card profile__settings space-y-6">
        <!-- 1. Thông tin cá nhân -->
        <section>
          <h2 class="profile__panel-title mb-3">Thông tin tài khoản</h2>
          <form class="space-y-4 max-w-lg" @submit.prevent="onUpdateDisplayName">
            <Input v-model="displayNameInput" label="Họ và tên hiển thị" placeholder="Nhập họ và tên..." required />

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Địa chỉ Email</label>
                <div class="px-3 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl text-xs text-vdsa-muted flex items-center justify-between">
                  <span>{{ auth.user?.email }}</span>
                  <Badge variant="success" class="text-[10px]">Đã xác thực</Badge>
                </div>
              </div>

              <div>
                <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Vai trò hệ thống</label>
                <div class="px-3 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl text-xs text-vdsa-muted flex items-center gap-1.5">
                  <ShieldCheck :size="14" class="text-vdsa-purple" />
                  <span class="font-bold text-white">{{ auth.role }}</span>
                </div>
              </div>
            </div>

            <Button type="submit" size="sm" variant="primary" :loading="nameUpdating">
              Lưu thay đổi họ tên
            </Button>
          </form>
        </section>

        <hr class="profile__divider" />

        <!-- 2. Ảnh đại diện -->
        <section>
          <div class="flex flex-wrap items-center justify-between gap-2 mb-3">
            <div>
              <h2 class="profile__panel-title">Ảnh đại diện</h2>
              <p class="text-xs text-vdsa-muted mt-0.5">Tải ảnh từ thiết bị, chọn Avatar mẫu có sẵn hoặc nhập liên kết ảnh trực tiếp.</p>
            </div>
            <Button
              v-if="auth.user?.avatarUrl"
              variant="secondary"
              size="sm"
              :loading="avatarUploading"
              @click="updateAvatarUrl(null)"
            >
              Xóa ảnh (Về mặc định)
            </Button>
          </div>

          <!-- Hidden Device File Input -->
          <input
            ref="avatarFileInput"
            type="file"
            accept="image/png, image/jpeg, image/jpg, image/webp, image/gif"
            class="hidden"
            @change="onAvatarFileChange"
          />

          <!-- Device File Selected Preview Banner -->
          <div
            v-if="avatarLocalFile && avatarLocalPreview"
            class="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 rounded-xl bg-vdsa-accent/10 border border-vdsa-accent/40 mb-4 overflow-hidden"
          >
            <div class="flex items-center gap-3 w-full sm:w-auto min-w-0 flex-1">
              <div class="w-14 h-14 rounded-full overflow-hidden border-2 border-vdsa-accent shrink-0 shadow-md">
                <img :src="avatarLocalPreview" alt="Preview" class="w-full h-full object-cover" />
              </div>
              <div class="min-w-0 flex-1 overflow-hidden">
                <p class="text-xs font-bold text-white truncate block max-w-full" :title="avatarLocalFile.name">
                  {{ avatarLocalFile.name }}
                </p>
                <p class="text-[11px] text-vdsa-muted shrink-0">
                  {{ (avatarLocalFile.size / 1024).toFixed(1) }} KB · Sẵn sàng tải lên
                </p>
              </div>
            </div>
            <div class="flex items-center gap-2 w-full sm:w-auto justify-end shrink-0">
              <Button
                variant="primary"
                size="sm"
                :loading="avatarUploading"
                class="gap-1.5 shrink-0"
                @click="uploadLocalAvatar"
              >
                <Upload :size="14" /> Tải lên & Lưu ngay
              </Button>
              <Button
                variant="ghost"
                size="sm"
                :disabled="avatarUploading"
                class="shrink-0"
                @click="cancelLocalAvatar"
              >
                Hủy
              </Button>
            </div>
          </div>

          <!-- Main Avatar Settings Box -->
          <div class="flex flex-col sm:flex-row items-start sm:items-center gap-4 p-4 rounded-xl bg-vdsa-surface border border-vdsa-border mb-5">
            <div class="profile__avatar-preview shrink-0">
              <img
                v-if="auth.user?.avatarUrl"
                :src="auth.user.avatarUrl"
                alt="Avatar"
                class="profile__avatar-preview-img"
                @error="avatarError = 'Không thể tải ảnh từ URL này. Vui lòng kiểm tra lại liên kết.'"
              />
              <span v-else class="profile__avatar-preview-placeholder font-bold text-white text-lg">
                {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
              </span>
            </div>

            <div class="flex-1 w-full space-y-3">
              <!-- Upload from device button (Only for Premium / VIP) -->
              <div v-if="isPremiumUser" class="flex flex-wrap items-center gap-2">
                <Button
                  type="button"
                  variant="primary"
                  size="sm"
                  :loading="avatarUploading"
                  class="gap-1.5"
                  @click="triggerDeviceUpload"
                >
                  <Upload :size="14" /> Tải ảnh từ thiết bị
                </Button>
                <span class="inline-flex items-center gap-1 text-[10px] font-bold px-2 py-0.5 rounded-full bg-amber-500/15 text-amber-400 border border-amber-500/30">
                  <Crown :size="11" /> VIP
                </span>
                <span class="text-[11px] text-vdsa-muted">Hỗ trợ JPG, PNG, WEBP (tối đa 3MB)</span>
              </div>
              <div v-else class="flex flex-wrap items-center gap-2 p-2.5 rounded-xl bg-amber-500/5 border border-amber-500/20">
                <router-link
                  to="/premium"
                  class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold bg-amber-500/20 text-amber-400 hover:bg-amber-500/30 border border-amber-500/40 transition-colors"
                >
                  <Crown :size="13" class="text-amber-400" />
                  <span>Tải ảnh từ thiết bị</span>
                  <span class="text-[10px] uppercase tracking-wider bg-amber-500 text-black px-1.5 py-0.2 rounded font-black">PRO</span>
                </router-link>
                <span class="text-[11px] text-vdsa-muted">Chỉ dành cho tài khoản Premium. Hãy nâng cấp để tải ảnh tùy biến!</span>
              </div>

              <!-- Direct URL input -->
              <div>
                <label class="block text-[11px] font-bold text-vdsa-secondary uppercase mb-1">Hoặc dán URL ảnh trực tiếp</label>
                <div class="flex gap-2">
                  <input
                    v-model="avatarUrlInput"
                    type="url"
                    placeholder="https://example.com/my-avatar.png hoặc /assets/avatars/..."
                    class="flex-1 px-3 py-1.5 bg-vdsa-bg-secondary border border-vdsa-border rounded-xl text-xs text-white placeholder:text-vdsa-disabled focus:outline-none focus:border-vdsa-accent"
                  />
                  <Button
                    variant="secondary"
                    size="sm"
                    :loading="avatarUploading"
                    :disabled="!avatarUrlInput.trim()"
                    @click="updateAvatarUrl(avatarUrlInput.trim())"
                  >
                    Lưu URL
                  </Button>
                </div>
              </div>
            </div>
          </div>

          <!-- Preset Avatars Gallery -->
          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-2">Hoặc chọn nhanh Avatar mẫu có sẵn</label>
            <div class="grid grid-cols-2 sm:grid-cols-5 gap-3">
              <button
                v-for="p in PRESET_AVATARS"
                :key="p.key"
                type="button"
                class="p-3 rounded-xl border flex flex-col items-center gap-2 transition-all cursor-pointer group text-left"
                :class="auth.user?.avatarUrl === p.url
                  ? 'bg-vdsa-accent/20 border-vdsa-accent shadow-lg shadow-vdsa-accent/20 ring-1 ring-vdsa-accent'
                  : 'bg-vdsa-surface border-vdsa-border hover:border-vdsa-accent/60 hover:bg-vdsa-hover'"
                @click="updateAvatarUrl(p.url)"
              >
                <img :src="p.url" :alt="p.name" class="w-12 h-12 object-contain group-hover:scale-110 transition-transform" />
                <span class="text-[11px] font-bold text-white text-center truncate w-full">{{ p.name }}</span>
                <span v-if="auth.user?.avatarUrl === p.url" class="text-[10px] text-vdsa-green font-semibold flex items-center gap-0.5">
                  <Check :size="10" /> Đang dùng
                </span>
                <span v-else class="text-[10px] text-vdsa-muted group-hover:text-white transition-colors">
                  Chọn avatar
                </span>
              </button>
            </div>
          </div>

          <p v-if="avatarError" class="profile__avatar-error mt-3 font-medium" role="alert">{{ avatarError }}</p>
        </section>

        <hr class="profile__divider" />

        <!-- 3. Đổi mật khẩu -->
        <section>
          <h2 class="profile__panel-title mb-3">Bảo mật & Đổi mật khẩu</h2>
          <form class="profile__password max-w-lg" novalidate @submit.prevent="onChangePassword">
            <Input v-model="passwordForm.current" label="Mật khẩu hiện tại" type="password" autocomplete="current-password" required />
            <Input v-model="passwordForm.next" label="Mật khẩu mới (tối thiểu 6 ký tự)" type="password" autocomplete="new-password" required />
            <p v-if="passwordError" class="profile__password-error" role="alert">{{ passwordError }}</p>
            <Button type="submit" size="sm" :loading="passwordBusy">{{ messages.profile.savePassword }}</Button>
          </form>
        </section>

        <hr class="profile__divider" />

        <!-- 4. Xác thực hai yếu tố (2FA - B1) -->
        <section>
          <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
            <div>
              <h2 class="profile__panel-title flex items-center gap-2">
                <ShieldCheck :size="18" class="text-vdsa-purple" />
                Xác thực hai lớp (2FA) qua Email
              </h2>
              <p class="text-xs text-vdsa-muted mt-1">
                Tăng cường bảo mật bằng mã OTP 6 chữ số gửi về hộp thư email khi đăng nhập.
              </p>
            </div>
            <div class="flex items-center gap-3">
              <Badge :variant="auth.user?.twoFactorEnabled ? 'success' : 'muted'">
                {{ auth.user?.twoFactorEnabled ? 'Đang bật' : 'Đang tắt' }}
              </Badge>
              <Button
                :variant="auth.user?.twoFactorEnabled ? 'danger' : 'primary'"
                size="sm"
                :loading="twoFactorBusy"
                @click="auth.user?.twoFactorEnabled ? openDisable2FaModal() : openEnable2FaModal()"
              >
                {{ auth.user?.twoFactorEnabled ? 'Tắt 2FA' : 'Bật 2FA' }}
              </Button>
            </div>
          </div>
        </section>
      </div>
    </section>

    <!-- Modal 1: Kích hoạt xác thực 2FA -->
    <Modal
      :open="twoFactorModalOpen"
      title="Bật xác thực hai lớp (2FA)"
      class="max-w-md"
      @close="twoFactorModalOpen = false"
    >
      <div class="space-y-4">
        <p class="text-xs text-vdsa-secondary leading-relaxed">
          Mã xác thực gồm 6 chữ số đã được gửi tới email <strong class="text-white">{{ auth.user?.email }}</strong>. Vui lòng nhập mã để hoàn tất kích hoạt.
        </p>

        <!-- Dev Mode hint -->
        <div v-if="isDevMode" class="p-2.5 rounded-xl bg-indigo-500/10 border border-indigo-500/30 text-xs text-indigo-300 flex items-center gap-2">
          <KeyRound :size="14" class="text-indigo-400 shrink-0" />
          <span><strong>Dev mode:</strong> mã OTP mặc định <code class="bg-indigo-950/80 px-1 py-0.5 rounded text-white font-mono font-bold">123456</code></span>
        </div>

        <!-- 6 ô nhập mã OTP -->
        <div class="space-y-2">
          <label class="block text-xs font-bold text-vdsa-secondary uppercase text-center">Nhập mã OTP 6 chữ số</label>
          <div class="profile__twofa-otp-boxes" @paste="onTwoFaDigitPaste">
            <input
              v-for="(_, index) in twoFaDigits"
              :id="`twofa-digit-${index}`"
              :key="index"
              v-model="twoFaDigits[index]"
              type="text"
              inputmode="numeric"
              maxlength="1"
              autocomplete="one-time-code"
              class="profile__twofa-otp-input"
              :class="{ 'profile__twofa-otp-input--error': twoFactorError }"
              @input="onTwoFaDigitInput(index, $event)"
              @keydown="onTwoFaDigitKeyDown(index, $event)"
            />
          </div>
        </div>

        <!-- Countdown & Gửi lại mã -->
        <div class="flex items-center justify-between text-xs px-1">
          <div class="flex items-center gap-1.5 text-vdsa-muted">
            <Clock :size="14" :class="twoFaExpiresSeconds < 60 ? 'text-rose-400' : 'text-vdsa-muted'" />
            <span>Hiệu lực: <strong :class="twoFaExpiresSeconds < 60 ? 'text-rose-400' : 'text-white'" class="font-mono">{{ twoFaTimeFormatted }}</strong></span>
          </div>

          <button
            type="button"
            class="text-vdsa-accent hover:underline font-semibold disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-1"
            :disabled="twoFaResendCooldownSeconds > 0 || twoFactorSending"
            @click="handleResend2FaOtp"
          >
            <RefreshCw v-if="twoFactorSending" :size="12" class="animate-spin" />
            <span v-if="twoFaResendCooldownSeconds > 0">Gửi lại mã ({{ twoFaResendCooldownSeconds }}s)</span>
            <span v-else>Gửi lại mã OTP</span>
          </button>
        </div>

        <p v-if="twoFactorError" class="text-xs text-rose-500 text-center font-medium" role="alert">
          {{ twoFactorError }}
        </p>

        <div class="flex items-center justify-end gap-2 pt-3 border-t border-vdsa-border">
          <Button variant="ghost" size="sm" :disabled="twoFactorBusy" @click="twoFactorModalOpen = false">
            Hủy
          </Button>
          <Button
            variant="primary"
            size="sm"
            :loading="twoFactorBusy"
            :disabled="twoFaCode.length !== 6"
            @click="handleVerify2Fa"
          >
            Xác nhận & Bật 2FA
          </Button>
        </div>
      </div>
    </Modal>

    <!-- Modal 2: Tắt xác thực 2FA (Confirm Modal) -->
    <Modal
      :open="twoFactorDisableModalOpen"
      title="Tắt xác thực hai lớp (2FA)?"
      class="max-w-md"
      @close="twoFactorDisableModalOpen = false"
    >
      <div class="space-y-4">
        <div class="p-4 rounded-xl bg-rose-500/10 border border-rose-500/30">
          <p class="text-sm font-medium text-white leading-relaxed">
            Bạn có chắc chắn muốn tắt xác thực hai lớp (2FA)?
            <br /><br />
            <span class="text-xs text-rose-300">
              Khi tắt, tài khoản của bạn sẽ chỉ được bảo vệ bằng mật khẩu và có nguy cơ rủi ro bảo mật cao hơn.
            </span>
          </p>
        </div>

        <div class="flex items-center justify-end gap-3 pt-3 border-t border-vdsa-border">
          <Button variant="ghost" size="sm" :disabled="twoFactorBusy" @click="twoFactorDisableModalOpen = false">
            Hủy
          </Button>
          <Button
            variant="danger"
            size="sm"
            :loading="twoFactorBusy"
            @click="handleConfirmDisable2Fa"
          >
            Tắt xác thực 2FA
          </Button>
        </div>
      </div>
    </Modal>
  </main>
</template>

<style scoped>
.profile {
  --p-purple: #8b5cf6;
  --p-purple-light: #a78bfa;
  --p-purple-dark: #7c3aed;
  --primary: oklch(0.56 0.24 293);
  --primary-foreground: oklch(0.99 0 0);
  --ring: oklch(0.56 0.24 293);
  --data-core: var(--p-purple);
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 960px;
}

.profile .card {
  background: color-mix(in srgb, var(--color-card) 72%, transparent);
  backdrop-filter: blur(10px);
  border-color: color-mix(in srgb, var(--p-purple) 22%, var(--color-border-subtle));
  box-shadow:
    inset 0 1px 0 color-mix(in srgb, #fff 6%, transparent),
    0 8px 28px color-mix(in srgb, var(--p-purple) 7%, transparent);
  transition: border-color 180ms cubic-bezier(0.16, 1, 0.3, 1), box-shadow 180ms cubic-bezier(0.16, 1, 0.3, 1);
}
.profile .card:hover {
  border-color: color-mix(in srgb, var(--p-purple) 45%, var(--color-border-subtle));
  box-shadow:
    inset 0 1px 0 color-mix(in srgb, #fff 8%, transparent),
    0 10px 34px color-mix(in srgb, var(--p-purple) 12%, transparent);
}

/* ── Hero profile ── */
.profile__hero {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg);
  border: 1px solid color-mix(in srgb, var(--p-purple) 30%, var(--color-border-subtle));
  border-radius: var(--radius-lg);
  background:
    linear-gradient(160deg, color-mix(in srgb, var(--p-purple) 7%, transparent) 0%, transparent 42%),
    color-mix(in srgb, var(--color-card-raised) 78%, transparent);
  backdrop-filter: blur(14px);
  box-shadow:
    inset 0 1px 0 color-mix(in srgb, #fff 7%, transparent),
    0 10px 36px color-mix(in srgb, var(--p-purple) 10%, transparent);
  overflow: hidden;
}

.profile__hero::before {
  content: '';
  position: absolute;
  inset: 0 0 auto 0;
  height: 3px;
  background: linear-gradient(90deg, var(--p-purple-light), var(--p-purple), transparent 80%);
}

.profile__banner {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, #eab308 14%, transparent);
  border: 1px solid color-mix(in srgb, #eab308 40%, transparent);
  color: #fde047;
  font-size: var(--text-sm);
}

.profile__hero-main {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.profile__avatar-frame {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 5px;
  border-radius: 50%;
  transition: all 250ms ease;
}

.profile__avatar-frame--neon {
  background: linear-gradient(135deg, #ec4899, #22d3ee);
  box-shadow: 0 0 16px rgba(236, 72, 153, 0.45);
}
.profile__avatar-frame--gold {
  background: linear-gradient(135deg, #f59e0b, #fde68a, #f59e0b);
  box-shadow: 0 0 18px rgba(250, 204, 21, 0.5);
}
.profile__avatar-frame--cyber {
  background: linear-gradient(135deg, #22d3ee, #6366f1);
  box-shadow: 0 0 16px rgba(34, 211, 238, 0.45);
}
.profile__avatar-frame--fire {
  background: linear-gradient(135deg, #ef4444, #f97316);
  box-shadow: 0 0 16px rgba(239, 68, 68, 0.5);
}
.profile__avatar-frame--ice {
  background: linear-gradient(135deg, #7dd3fc, #93c5fd);
  box-shadow: 0 0 14px rgba(125, 211, 252, 0.5);
}
.profile__avatar-frame--default {
  background: linear-gradient(135deg, var(--p-purple-light), var(--p-purple-dark));
  box-shadow: 0 0 14px rgba(168, 85, 247, 0.4);
}

.profile__avatar {
  width: 68px;
  height: 68px;
  border-radius: 50%;
  background: var(--p-purple);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-2xl);
  font-weight: 800;
  object-fit: cover;
  transition: all 250ms ease;
}

.profile__avatar--cyber { background: linear-gradient(135deg, #0e7490, #155e75); color: #a5f3fc; }
.profile__avatar--gold { background: linear-gradient(135deg, #b45309, #92400e); color: #fef3c7; }
.profile__avatar--neon { background: linear-gradient(135deg, #be185d, #6b21a8); color: #fbcfe8; }
.profile__avatar--wizard { background: linear-gradient(135deg, #6d28d9, #4c1d95); color: #ddd6fe; }
.profile__avatar--bot { background: linear-gradient(135deg, #0f766e, #134e4a); color: #99f6e4; }

.profile__identity {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
  min-width: 200px;
}

.profile__name {
  font-size: var(--text-2xl);
  font-weight: 800;
  margin: 0;
  color: var(--color-text-primary);
}

.profile__email {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  margin: 0;
}

.profile__chips {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 4px;
  flex-wrap: wrap;
}

.profile__flame { color: #f97316; }

.profile__actions {
  margin-left: auto;
}

.profile__stats-row {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.profile__stat-block {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--space-xs) var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
}

.profile__stat-label {
  font-size: 10px;
  font-weight: 700;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  letter-spacing: 0.05em;
}

.profile__stat-line {
  display: flex;
  align-items: baseline;
  gap: 4px;
}

.profile__stat-value {
  font-size: var(--text-base);
  font-weight: 700;
  color: var(--color-text-primary);
  font-family: var(--font-mono);
}

.profile__stat-unit {
  font-size: 10px;
  font-weight: 600;
  color: var(--color-text-tertiary);
}

.profile__level-progress {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.profile__level-progress-head {
  display: flex;
  justify-content: space-between;
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
}

/* ── Panel & Tabs ── */
.profile__panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.profile__overview-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-md);
}

@media (max-width: 720px) {
  .profile__overview-grid {
    grid-template-columns: 1fr;
  }
}

.profile__overview-card {
  padding: var(--space-lg);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.profile__panel-title {
  font-size: var(--text-base);
  font-weight: 700;
  color: var(--color-text-primary);
  margin: 0 0 var(--space-xs) 0;
}

.profile__mono {
  font-family: var(--font-mono);
  font-weight: 700;
  color: var(--color-text-primary);
}

.profile__overview-progress {
  margin-top: var(--space-sm);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.profile__quick {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-xs);
  margin-top: var(--space-xs);
}

.profile__quick-link {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: var(--radius-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  color: var(--color-text-primary);
  font-size: var(--text-xs);
  text-decoration: none;
  font-weight: 600;
  transition: all 180ms ease;
}

.profile__quick-link:hover {
  background: color-mix(in srgb, var(--p-purple) 15%, var(--color-surface));
  border-color: var(--p-purple-light);
}

.profile__quick-idx {
  font-family: var(--font-mono);
  font-size: 10px;
  color: var(--color-text-tertiary);
}

/* ── Radar ── */
.profile__radar-card {
  padding: var(--space-lg);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.profile__radar-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.profile__radar-note {
  font-size: 11px;
  color: var(--color-text-tertiary);
  margin: 0;
}

/* ── Gamification ── */
.profile__gamification {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.profile__gamification-row {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-md);
}

@media (max-width: 720px) {
  .profile__gamification-row {
    grid-template-columns: 1fr;
  }
}

/* ── Progress Tab ── */
.profile__progress-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-xs);
}

.profile__topics {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.profile__topic {
  padding: var(--space-md) var(--space-lg);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.profile__topic-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.profile__topic-name {
  font-size: var(--text-sm);
  font-weight: 700;
  margin: 0;
}

.profile__topic-pct {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 700;
  color: var(--p-purple-light);
}

.profile__topic-lessons {
  list-style: none;
  padding: 0;
  margin: var(--space-xs) 0 0 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.profile__topic-lesson {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: var(--text-xs);
}

.profile__done {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--color-text-primary);
}

.profile__todo {
  display: flex;
  align-items: center;
  gap: 6px;
  color: var(--color-text-tertiary);
}

/* ── Inventory Tab ── */
.profile__inv-groups {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.profile__inv-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.profile__inv-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: var(--space-md);
}

@media (max-width: 800px) {
  .profile__inv-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
@media (max-width: 500px) {
  .profile__inv-grid {
    grid-template-columns: 1fr;
  }
}

.profile__inv-card {
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  position: relative;
}

.profile__inv-card--equipped {
  border-color: #10b981;
  background: color-mix(in srgb, #10b981 6%, var(--color-card));
}

.profile__inv-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--p-purple) 15%, transparent);
  color: var(--p-purple-light);
  display: flex;
  align-items: center;
  justify-content: center;
}

.profile__inv-name {
  font-size: var(--text-sm);
  font-weight: 700;
  margin: 0;
}

.profile__inv-other {
  list-style: none;
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  margin: 0;
}

.profile__inv-other-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  border-radius: var(--radius-md);
  background: var(--color-surface);
}

/* ── Achievements Tab ── */
.profile__achievements {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: var(--space-md);
}

.profile__achievement {
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  background: var(--color-card);
  border: 1px solid var(--color-border-subtle);
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: 6px;
}

.profile__achievement--locked {
  opacity: 0.55;
  filter: grayscale(0.8);
}

.profile__achievement-icon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--color-surface);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-tertiary);
}

.profile__achievement-icon--open {
  background: color-mix(in srgb, #eab308 20%, transparent);
  color: #facc15;
}

.profile__achievement-label {
  font-size: var(--text-sm);
  font-weight: 700;
  margin: 0;
}

.profile__achievement-desc {
  font-size: 11px;
  color: var(--color-text-tertiary);
  margin: 0;
}

/* ── Settings Tab ── */
.profile__settings {
  padding: var(--space-xl);
  border-radius: var(--radius-lg);
}

.profile__divider {
  border: none;
  border-top: 1px solid var(--color-border-subtle);
  margin-block: var(--space-md);
}

.profile__avatar-upload {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.profile__avatar-preview {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: var(--color-surface);
  border: 2px dashed var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
}

.profile__avatar-preview-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile__avatar-preview-placeholder {
  font-size: 24px;
}

.profile__avatar-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.profile__avatar-input {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
}

.profile__avatar-error,
.profile__password-error {
  font-size: 12px;
  color: #ef4444;
  margin: 0;
}

.profile__password {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* 2FA OTP 6-Box Styling */
.profile__twofa-otp-boxes {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
  margin-block: 4px;
}

.profile__twofa-otp-input {
  width: 44px;
  height: 52px;
  text-align: center;
  font-size: 22px;
  font-weight: 700;
  font-family: var(--font-mono, monospace);
  background: var(--color-surface, #161b22);
  border: 1.5px solid var(--color-border, #30363d);
  border-radius: 10px;
  color: #ffffff;
  transition: all 150ms ease;
}

.profile__twofa-otp-input:focus {
  outline: none;
  border-color: var(--color-accent, #6366f1);
  box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.25);
  background: var(--color-card, #0d1117);
}

.profile__twofa-otp-input--error {
  border-color: var(--color-destructive, #ef4444);
}
</style>