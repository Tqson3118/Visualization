<script setup lang="ts">
// ForgotPasswordView — Màn N-2: gửi link khôi phục mật khẩu (FR-1.6)
import { ref } from 'vue';
import { RouterLink } from 'vue-router';

import * as authApi from '@/api/auth';
import { isValidEmail } from '@/utils/validators';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const email = ref('');
const error = ref('');
const sent = ref(false);
const submitting = ref(false);

async function onSubmit(): Promise<void> {
  error.value = '';
  if (!isValidEmail(email.value)) {
    error.value = 'Email không hợp lệ';
    return;
  }
  submitting.value = true;
  try {
    await authApi.forgotPassword(email.value.trim());
    sent.value = true;
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Không thể gửi email khôi phục.';
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <main class="forgot">
    <div class="forgot__card card">
      <h1 class="forgot__title">Quên mật khẩu?</h1>

      <div v-if="sent" class="forgot__sent" role="status">
        <p>📧 Đã gửi link khôi phục tới <strong>{{ email }}</strong> (hiệu lực 30 phút, dùng 1 lần).</p>
        <p class="text-muted">Kiểm tra hộp thư (kể cả thư rác).</p>
        <RouterLink :to="{ name: 'login' }">← Về đăng nhập</RouterLink>
      </div>

      <template v-else>
        <p class="forgot__desc text-muted">
          Nhập email đăng ký — chúng tôi sẽ gửi link đặt lại mật khẩu cho bạn.
        </p>
        <form novalidate @submit.prevent="onSubmit">
          <Input
            v-model="email"
            label="Email"
            type="email"
            :error="error"
            :placeholder="'ban@truong.edu.vn'"
            autocomplete="email"
            required
          />
          <Button type="submit" class="forgot__submit" :loading="submitting" block>
            Gửi link khôi phục
          </Button>
        </form>
        <RouterLink class="forgot__back" :to="{ name: 'login' }">← Quay lại đăng nhập</RouterLink>
      </template>
    </div>
  </main>
</template>

<style scoped>
.forgot {
  min-height: 70vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

.forgot__card {
  width: 100%;
  max-width: 24rem;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.forgot__title { font-size: var(--text-2xl); }

.forgot__desc { font-size: var(--text-sm); }

.forgot__submit { margin-top: var(--space-md); }

.forgot__sent { display: flex; flex-direction: column; gap: var(--space-sm); font-size: var(--text-sm); }

.forgot__back { font-size: var(--text-sm); }
</style>
