<template>
  <div class="reset-password-page flex items-center justify-center min-h-screen px-4 py-8" style="background: var(--color-bg-primary);">
    <div class="glass-panel w-full max-w-md p-8 rounded-2xl">
      <div class="text-center mb-8">
        <BaseIcon name="shield" class="w-10 h-10 text-accent mx-auto mb-3" />
        <h1 class="text-2xl font-bold text-text-primary">Đặt lại mật khẩu</h1>
        <p class="text-text-muted text-sm mt-2">Nhập mật khẩu mới cho tài khoản của bạn</p>
      </div>

      <form v-if="!submitted" @submit.prevent="submitReset" class="space-y-4">
        <div>
          <label for="reset-password" class="block text-sm font-medium text-text-secondary mb-1.5">Mật khẩu mới</label>
          <input
            id="reset-password"
            v-model="newPassword"
            type="password"
            required
            minlength="8"
            placeholder="Tối thiểu 8 ký tự"
            class="w-full bg-bg-secondary border border-border-default rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:border-accent focus:outline-none transition-colors"
          />
        </div>
        <div>
          <label for="confirm-password" class="block text-sm font-medium text-text-secondary mb-1.5">Nhập lại mật khẩu</label>
          <input
            id="confirm-password"
            v-model="confirmPassword"
            type="password"
            required
            minlength="8"
            placeholder="Nhập lại mật khẩu mới"
            class="w-full bg-bg-secondary border border-border-default rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:border-accent focus:outline-none transition-colors"
          />
        </div>

        <div v-if="error" class="text-sm text-accent-red bg-accent-red/10 border border-accent-red/20 rounded-lg p-3">{{ error }}</div>
        <p v-else-if="newPassword && confirmPassword && newPassword !== confirmPassword" class="text-xs text-accent-red">Mật khẩu xác nhận không khớp.</p>

        <button
          type="submit"
          :disabled="submitting || newPassword.length < 8 || newPassword !== confirmPassword"
          class="w-full px-6 py-3 bg-accent text-white font-bold rounded-xl hover:bg-accent-light transition-all disabled:opacity-60 disabled:cursor-not-allowed"
        >
          {{ submitting ? 'Đang xử lý...' : 'Đặt lại mật khẩu' }}
        </button>
      </form>

      <div v-else class="text-center py-8">
        <BaseIcon name="check-circle" class="w-12 h-12 text-accent-green mx-auto mb-4" />
        <h2 class="text-lg font-bold text-text-primary mb-2">Đặt lại mật khẩu thành công!</h2>
        <p class="text-sm text-text-muted mb-6">Bạn có thể đăng nhập bằng mật khẩu mới ngay bây giờ.</p>
        <router-link to="/login" class="btn-primary inline-block px-6 py-3 rounded-xl">Đăng nhập ngay</router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import { API_BASE_URL } from '@/services/apiConfig';

const route = useRoute();
const BASE_URL = API_BASE_URL;

const newPassword = ref('');
const confirmPassword = ref('');
const submitting = ref(false);
const submitted = ref(false);
const error = ref('');

const token = route.query.token as string;

onMounted(() => {
  if (!token) {
    error.value = 'Thiếu token đặt lại mật khẩu. Vui lòng kiểm tra link trong email.';
  }
});

async function submitReset(): Promise<void> {
  if (!token) return;
  submitting.value = true;
  error.value = '';
  try {
    const res = await fetch(`${BASE_URL}/api/v1/auth/reset-password`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token, newPassword: newPassword.value }),
    });
    if (res.ok) {
      submitted.value = true;
    } else {
      const err = await res.json();
      error.value = err.message ?? 'Không thể đặt lại mật khẩu.';
    }
  } catch (err) {
    error.value = 'Không thể kết nối máy chủ.';
  } finally {
    submitting.value = false;
  }
}
</script>
