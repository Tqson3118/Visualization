<template>
  <div class="admin-panel max-w-[1280px] mx-auto">
    
    <header class="panel-header">
      <div class="header-main">
        <h1 class="panel-title">
          <BaseIcon name="shield" style="width:28px;height:28px;color:#f87171" />
          Hệ thống Quản trị Admin
        </h1>
        <p class="panel-subtitle">Quản lý người dùng, nội dung và hệ thống.</p>
      </div>

      
      <div class="tabs-nav">
        <button 
          v-for="tab in tabs" 
          :key="tab.id"
          class="tab-btn"
          :class="{ 'tab-btn--active': activeTab === tab.id }"
          @click="activeTab = tab.id"
        >
          <BaseIcon :name="tab.icon" style="width:16px;height:16px" />
          {{ tab.name }}
        </button>
      </div>
    </header>

    
    <div class="panel-content">
      <AdminDashboardTab v-if="activeTab === 'dashboard'" ref="dashboardTabRef" />
      <AdminUsersTab v-else-if="activeTab === 'users'" @refresh-dashboard="refreshDashboard" />
      <AdminQuizzesTab v-else-if="activeTab === 'quizzes'" @refresh-dashboard="refreshDashboard" />
      <AdminTeacherApprovalsTab v-else-if="activeTab === 'teacher-approvals'" />
      <AdminRoadmapApprovalsTab v-else-if="activeTab === 'roadmap-approvals'" />
      <AdminModerationTab v-else-if="activeTab === 'moderation'" />
      <AdminAuditTab v-else-if="activeTab === 'audit'" />
      <AdminSystemTab v-else-if="activeTab === 'system'" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import AdminDashboardTab from './AdminDashboardTab.vue';
import AdminUsersTab from './AdminUsersTab.vue';
import AdminQuizzesTab from './AdminQuizzesTab.vue';
import AdminTeacherApprovalsTab from './AdminTeacherApprovalsTab.vue';
import AdminRoadmapApprovalsTab from './AdminRoadmapApprovalsTab.vue';
import AdminModerationTab from './AdminModerationTab.vue';
import AdminAuditTab from './AdminAuditTab.vue';
import AdminSystemTab from './AdminSystemTab.vue';

interface Tab {
  id: string;
  name: string;
  icon: string;
}

const tabs: Tab[] = [
  { id: 'dashboard', name: 'Tổng quan', icon: 'chart-bar' },
  { id: 'users', name: 'Người dùng & Giảng viên', icon: 'users' },
  { id: 'quizzes', name: 'Ngân hàng Quiz', icon: 'clipboard-list' },
  { id: 'teacher-approvals', name: 'Duyệt Teacher', icon: 'teacher' },
  { id: 'roadmap-approvals', name: 'Duyệt Roadmap', icon: 'roadmap' },
  { id: 'moderation', name: 'Kiểm duyệt nội dung', icon: 'warning' },
  { id: 'audit', name: 'Nhật ký hoạt động', icon: 'shield' },
  { id: 'system', name: 'Hệ thống', icon: 'cog' }
];

const activeTab = ref('dashboard');
const dashboardTabRef = ref<InstanceType<typeof AdminDashboardTab> | null>(null);

function refreshDashboard() {
  if (dashboardTabRef.value) {
    dashboardTabRef.value.loadDashboardData();
  }
}
</script>

<style>
@import "./AdminPanelView.css";
</style>
