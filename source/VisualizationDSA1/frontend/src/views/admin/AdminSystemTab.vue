<template>
  <section class="tab-section fade-in">
    <div class="system-layout">
      <div class="card card--system-status">
        <h3 class="card-heading"><BaseIcon name="cog" style="width:18px;height:18px" /> Thông tin Máy chủ & API <span class="ml-2 px-2 py-0.5 rounded-md bg-accent-warm/20 text-accent-warm text-[10px] font-bold uppercase tracking-wider">Demo</span></h3>
        <div class="system-info-grid">
          <div class="info-item"><span class="info-label">API Base URL:</span><span class="info-val"><code>{{ BASE_URL }}</code></span></div>
          <div class="info-item"><span class="info-label">Phương thức xác thực:</span><span class="info-val">JWT (Bearer Token)</span></div>
          <div class="info-item"><span class="info-label">Trạng thái kết nối CSDL:</span><span class="info-val text-success">Đang kết nối (PostgreSQL) <BaseIcon name="check" class="w-3 h-3 inline-block align-text-bottom" /></span></div>
          <div class="info-item"><span class="info-label">Môi trường hoạt động:</span><span class="info-val"><code>Production-safe mode</code></span></div>
        </div>
        <div class="system-actions">
          <button class="btn-primary" @click="runSystemDiagnostics"><BaseIcon name="lightning" style="width:15px;height:15px" /> Chạy chẩn đoán hệ thống</button>
        </div>
      </div>
      <div class="card card--settings">
        <h3 class="card-heading"><BaseIcon name="tool" style="width:18px;height:18px" /> Cài đặt hệ thống</h3>
        <div class="settings-form">
          <div class="setting-row">
            <div class="setting-desc"><span class="setting-title">Cho phép Đăng ký tài khoản</span><p class="setting-sub">Cho phép người dùng mới tạo tài khoản qua OAuth hoặc Stateless Email.</p></div>
            <input type="checkbox" checked class="setting-checkbox" disabled />
          </div>
          <div class="setting-row">
            <div class="setting-desc"><span class="setting-title">Bảo trì Timeline VCR</span><p class="setting-sub">Khóa tạm thời timeline visualizer để cập nhật giải thuật cốt lõi.</p></div>
            <input type="checkbox" class="setting-checkbox" />
          </div>
        </div>
      </div>
      <div class="card card--broadcast">
        <h3 class="card-heading"><BaseIcon name="bell" style="width:18px;height:18px" /> Gửi thông báo toàn hệ thống (Broadcast)</h3>
        <div class="broadcast-form">
          <input v-model="broadcastMessage" type="text" placeholder="Nội dung thông báo gửi tới tất cả học viên..."
            class="bg-bg-hover border border-border-subtle px-3 py-2 rounded-xl text-sm text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent w-full"
            @keyup.enter="sendBroadcast" />
          <div class="flex items-center gap-2 mt-3">
            <button class="btn-primary flex items-center gap-1" :disabled="sendingBroadcast" @click="sendBroadcast">
              <BaseIcon name="send" style="width:15px;height:15px" /> {{ sendingBroadcast ? 'Đang gửi...' : 'Gửi thông báo' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useToastStore } from '@/composables/useToast';

import { useAdminApi } from './useAdminApi';

const toastStore = useToastStore();
const { BASE_URL, pushLog, getAuthHeaders } = useAdminApi();

const broadcastMessage = ref('');
const sendingBroadcast = ref(false);

function runSystemDiagnostics(): void {
  pushLog('INFO', '[Demo] Bắt đầu chẩn đoán hệ thống (dữ liệu mẫu)...');
  setTimeout(() => {
    pushLog('INFO', '[Demo] Ping API Server: 25ms - Khỏe mạnh ✓');
    pushLog('INFO', '[Demo] CSDL PostgreSQL: OK - Mức chiếm dụng đĩa 4.2%');
    pushLog('INFO', '[Demo] Hệ thống chẩn đoán kết thúc không phát hiện lỗi.');
    toastStore.success('Diagnostics (Demo) hoàn tất.');
  }, 1000);
}

async function sendBroadcast(): Promise<void> {
  const message = broadcastMessage.value.trim();
  if (!message) {
    toastStore.error('Vui lòng nhập nội dung thông báo.');
    return;
  }
  sendingBroadcast.value = true;
  try {
    const res = await fetch(`${BASE_URL}/api/v1/notifications/broadcast`, {
      method: 'POST',
      headers: getAuthHeaders(),
      body: JSON.stringify({ message, deepLink: '/' }),
    });
    if (res.ok) {
      const data = await res.json();
      toastStore.success(`Đã gửi thông báo tới ${data.sentCount ?? 0} học viên.`);
      broadcastMessage.value = '';
      pushLog('INFO', `Broadcast: ${message}`);
    } else {
      const err = await res.json().catch(() => ({}));
      toastStore.error(err.message || 'Không thể gửi thông báo.');
    }
  } catch {
    toastStore.error('Lỗi kết nối khi gửi thông báo.');
  } finally {
    sendingBroadcast.value = false;
  }
}
</script>
