<template>
  <div class="profile-page-wrapper">
    <div class="profile-page-container glass-panel">
      <div class="settings-modal-header">
        <div class="header-title-box">
          <BaseIcon name="profile" class="w-4 h-4 text-accent mr-2" />
          <h1 class="header-title">Hồ sơ cá nhân</h1>
        </div>

        <div class="header-right-actions">
          <div class="user-tier-badge" :class="{ 'user-tier-badge--pro': authStore.isPremium }">
            <BaseIcon :name="authStore.isPremium ? 'diamond' : 'badge'" class="w-3.5 h-3.5 mr-1" />
            <span>{{ authStore.isPremium ? 'PRO' : 'Standard' }}</span>
          </div>
        </div>
      </div>

      
      <div class="settings-modal-body">
        
        <aside class="modal-sidebar">
          <div class="sidebar-user-card">
            <div class="user-avatar relative" :class="{ 'user-avatar--pro': authStore.isPremium }">
              <img v-if="authStore.currentUser?.avatarUrl" :src="authStore.currentUser.avatarUrl" alt="Avatar" class="w-full h-full object-cover rounded-full" />
              <span v-else>{{ initials }}</span>
              <div v-if="authStore.currentUser?.avatarFrameType" class="absolute -inset-2 pointer-events-none" :class="getFrameClass(authStore.currentUser.avatarFrameType)"></div>
            </div>
            <div class="user-meta">
              <span class="user-display-name">{{ currentNickname || authStore.userName }}</span>
              <span class="user-email-text">{{ authStore.currentUser?.email }}</span>
            </div>
          </div>

          <nav class="sidebar-nav">
            <div class="nav-group">
              <span class="nav-group-label">HỒ SƠ CÁ NHÂN</span>
              <button class="nav-item" :class="{ 'nav-item--active': activeTab === 'general' }" @click="activeTab = 'general'">
                <BaseIcon name="profile" class="nav-icon" />
                <span>General</span>
              </button>
              <button class="nav-item" :class="{ 'nav-item--active': activeTab === 'progress' }" @click="activeTab = 'progress'">
                <BaseIcon name="medal" class="nav-icon" />
                <span>Badges & Progress</span>
                <span class="nav-badge-pill">{{ badgesCount }}</span>
              </button>
              <button class="nav-item" :class="{ 'nav-item--active': activeTab === 'history' }" @click="activeTab = 'history'">
                <BaseIcon name="clipboard-list" class="nav-icon" />
                <span>Quiz History</span>
              </button>
            </div>

            <div class="nav-group">
              <span class="nav-group-label">HỆ THỐNG & BẢO MẬT</span>
              <button class="nav-item" :class="{ 'nav-item--active': activeTab === 'teacher' }" @click="activeTab = 'teacher'">
                <BaseIcon name="teacher" class="nav-icon" />
                <span>Nộp đơn Teacher</span>
              </button>
              <button class="nav-item" :class="{ 'nav-item--active': activeTab === 'security' }" @click="activeTab = 'security'">
                <BaseIcon name="shield" class="nav-icon" />
                <span>Security</span>
              </button>
            </div>
          </nav>
        </aside>
        
        
        <main class="modal-content-panel">
          <!-- Premium Profile Header with Cover Photo & Stats -->
          <div class="profile-premium-header" v-if="activeTab === 'general' || activeTab === 'progress'">
            <div class="cover-photo-section relative">
              <div class="cover-photo" :style="{ backgroundImage: coverPhotoStyle }">
                <div class="cover-gradient"></div>
              </div>
              <div class="profile-avatar-section">
                <div class="avatar-upload-wrapper">
                  <img 
                    v-if="authStore.currentUser?.avatarUrl" 
                    :src="authStore.currentUser.avatarUrl" 
                    alt="Avatar" 
                    class="profile-avatar-image" 
                  />
                  <span v-else class="avatar-placeholder">{{ initials }}</span>
                  <div v-if="authStore.currentUser?.avatarFrameType" class="avatar-frame" :class="getFrameClass(authStore.currentUser.avatarFrameType)"></div>
                  <label class="avatar-upload-btn" @click="avatarInput?.click()" title="Đổi avatar">
                    <BaseIcon name="camera" class="w-5 h-5" />
                    <input type="file" ref="avatarInput" accept="image/*" @change="handleAvatarUpload" class="hidden" />
                  </label>
                  <label class="cover-upload-btn" @click="coverInput?.click()" title="Đổi ảnh bìa">
                    <BaseIcon name="image" class="w-5 h-5" />
                    <input type="file" ref="coverInput" accept="image/*" @change="handleCoverUpload" class="hidden" />
                  </label>
                </div>
              </div>
              <div class="profile-meta-section">
                <div class="profile-name-row">
                  <h2 class="profile-display-name">{{ currentNickname || authStore.userName }}</h2>
                  <span class="role-chip" :class="`role-chip--${authStore.userRole.toLowerCase()}`">
                    {{ roleLabel }}
                  </span>
                </div>
                <p class="profile-email">{{ authStore.currentUser?.email }}</p>
              </div>
            </div>

            <!-- Stats Grid -->
            <div class="stats-grid" v-if="activeTab === 'general' || activeTab === 'progress'">
              <div class="stat-card" v-for="stat in profileStats" :key="stat.key">
                <div class="stat-icon" :class="stat.iconClass">
                  <BaseIcon :name="stat.icon" class="w-5 h-5" />
                </div>
                <div class="stat-content">
                  <span class="stat-value">{{ stat.value }}</span>
                  <span class="stat-label">{{ stat.label }}</span>
                </div>
              </div>
            </div>
          </div>

          <section class="modal-content-panel modal-content-panel--inner">
            <ProfileGeneralTab v-if="activeTab === 'general'" />
            <ProfileProgressTab v-else-if="activeTab === 'progress'" />
            <ProfileHistoryTab v-else-if="activeTab === 'history'" />
            <ProfileSecurityTab v-else-if="activeTab === 'security'" />
            <section v-else-if="activeTab === 'teacher'" class="p-6">
              <h3 class="text-lg font-bold text-text-primary mb-4">Nộp đơn trở thành Giáo viên</h3>
              <TeacherApplicationForm />
            </section>
          </section>
        </main>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useToastStore } from '@/composables/useToast';
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import ProfileGeneralTab from './ProfileGeneralTab.vue';
import ProfileProgressTab from './ProfileProgressTab.vue';
import ProfileHistoryTab from './ProfileHistoryTab.vue';
import ProfileSecurityTab from './ProfileSecurityTab.vue';
import TeacherApplicationForm from '../../components/profile/TeacherApplicationForm.vue';

const router = useRouter();
const authStore = useAuthStore();
const toastStore = useToastStore();
const activeTab = ref<'general' | 'progress' | 'history' | 'security' | 'teacher'>('general');

const avatarInput = ref<HTMLInputElement | null>(null);
const coverInput = ref<HTMLInputElement | null>(null);

onMounted(async () => {
  await authStore.loadStatelessProfile();
});

const initials = computed(() => {
  const name = authStore.currentUser?.nickname || authStore.userName;
  return name ? name.charAt(0).toUpperCase() : 'U';
});

const currentNickname = computed(() => authStore.currentUser?.nickname);
const badgesCount = computed(() => authStore.currentUser?.badges?.length || 0);

const getFrameClass = (frameType: string) => {
  const type = frameType.toLowerCase();
  switch (type) {
    case 'neon': return 'ring-4 ring-accent-purple shadow-[0_0_15px_var(--color-accent-purple-glow)] rounded-full';
    case 'gold': return 'ring-4 ring-accent-yellow shadow-[0_0_15px_var(--color-accent-yellow-glow)] rounded-full';
    case 'cyber': return 'ring-4 ring-accent-cyan shadow-[0_0_15px_var(--color-accent-cyan-glow)] rounded-full';
    case 'fire': return 'ring-4 ring-accent-red shadow-[0_0_15px_var(--color-accent-red-glow)] rounded-full';
    case 'ice': return 'ring-4 ring-accent-blue-light shadow-[0_0_15px_var(--color-accent-blue-glow)] rounded-full';
    case 'diamond': return 'ring-4 ring-accent-blue shadow-[0_0_15px_var(--color-accent-blue-glow)] rounded-full';
    default: return 'ring-2 ring-accent rounded-full';
  }
};

const coverPhotoStyle = computed(() => {
  // coverPhotoUrl not yet on backend type, fallback to gradient
  return 'linear-gradient(135deg, var(--color-accent-primary-dim) 0%, var(--color-accent-purple-dim) 100%)';
});

const profileStats = computed(() => {
  const user = authStore.currentUser;
  if (!user) return [];
  
  const lvl = authStore.userLevel;
  const thresholds = [0, 100, 300, 600, 1000, 1500, 2200, 3000];
  const xpToNextVal = lvl >= thresholds.length ? 0 : thresholds[lvl] - authStore.userXP;
  
  return [
    { key: 'xp', label: 'XP', value: authStore.userXP.toLocaleString(), icon: 'diamond', iconClass: 'stat-icon-xp' },
    { key: 'level', label: 'Level', value: authStore.userLevel, icon: 'trending-up', iconClass: 'stat-icon-level' },
    { key: 'streak', label: 'Streak', value: authStore.currentUser?.streakDays || 0, icon: 'fire', iconClass: 'stat-icon-streak' },
    { key: 'badges', label: 'Huy hiệu', value: authStore.currentUser?.badges?.length || 0, icon: 'medal', iconClass: 'stat-icon-badges' },
    { key: 'xpNext', label: 'XP tới level', value: xpToNextVal > 0 ? xpToNextVal : 'MAX', icon: 'target', iconClass: 'stat-icon-xpnext' },
  ];
});

const roleLabel = computed(() => {
  switch (authStore.userRole) {
    case 'Admin': return 'Quản trị viên';
    case 'Teacher': return 'Giảng viên';
    default: return 'Học viên';
  }
});

async function handleAvatarUpload(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;
  
  if (!file.type.startsWith('image/')) {
    toastStore.info('Vui lòng chọn file ảnh');
    return;
  }
  
  const formData = new FormData();
  formData.append('avatar', file);
  
  try {
    const response = await fetch('/api/v1/users/me/avatar', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${authStore.getAccessToken()}`
      },
      body: formData
    });
    
    if (response.ok) {
      const data = await response.json();
      await authStore.loadStatelessProfile();
    } else {
      const error = await response.json();
      toastStore.error(error.message || 'Upload avatar thất bại');
    }
  } catch (err) {
    console.error('Upload avatar error:', err);
    toastStore.error('Có lỗi xảy ra khi upload avatar');
  } finally {
    const inputEl = document.getElementById('avatarInput') as HTMLInputElement;
    if (inputEl) inputEl.value = '';
  }
}

async function handleCoverUpload(event: Event) {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (!file) return;
  
  if (!file.type.startsWith('image/')) {
    toastStore.info('Vui lòng chọn file ảnh');
    return;
  }
  
  const formData = new FormData();
  formData.append('cover', file);
  
  try {
    const response = await fetch('/api/v1/users/me/cover', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${authStore.getAccessToken()}`
      },
      body: formData
    });
    
    if (response.ok) {
      await authStore.loadStatelessProfile();
    } else {
      const error = await response.json();
      toastStore.error(error.message || 'Upload ảnh bìa thất bại');
    }
  } catch (err) {
    console.error('Upload cover error:', err);
    toastStore.error('Có lỗi xảy ra khi upload ảnh bìa');
  } finally {
    const inputEl = document.getElementById('coverInput') as HTMLInputElement;
    if (inputEl) inputEl.value = '';
  }
}
</script>

<style>
@import "./ProfileView.css";
</style>