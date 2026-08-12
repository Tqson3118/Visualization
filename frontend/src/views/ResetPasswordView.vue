<script setup lang="ts">
// ResetPasswordView — Màn N-2: đặt lại mật khẩu bằng token 1 lần (FR-1.6)
import { computed, reactive, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';

import * as authApi from '@/api/auth';
import { validatePassword } from '@/utils/validators';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const route = useRoute();
const router = useRouter();

const form = reactive({ password: '', confirm: '' });
const error = ref('');
const success = ref(false);
const submitting = ref(false);

const token = computed(() => String(route.query.token ?? ''));

const passwordRules = computed(() => [
  { ok: form.password.length >= 8 && form.password.length <= 64, label: '8-64 ký tự' },
  { ok: /[A-Z]/.test(form.password), label: 'Chữ hoa' },
  { ok: /\d/.test(form.password), label: 'Chữ số' },
  { ok: /[^A-Za-z0-9]/.test(form.password), label: 'Ký tự đặc biệt' },
]);

async function onSubmit(): Promise<void> {
  error.value = '';
  if (!validatePassword(form.password).ok) {
    error.value = 'Mật khẩu phải từ 8-64 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt';
    return;
  }
  if (form.confirm !== form.password) {
    error.value = 'Mật khẩu xác nhận không khớp';
    return;
  }
  if (!token.value) {
    error.value = 'Thiếu token khôi phục — hãy mở lại link trong email của bạn.';
    return;
  }
  submitting.value = true;
  try {
    await authApi.resetPassword({ token: token.value, newPassword: form.password });
    success.value = true;
    setTimeout(() => void router.replace({ name: 'login' }), 2000);
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Không thể đặt lại mật khẩu.';
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <main class="reset">
    <div class="reset__card card">
      <h1 class="reset__title">Đặt lại mật khẩu</h1>

      <div v-if="success" class="reset__success" role="status">
        <p>✅ Mật khẩu đã được đặt lại thành công!</p>
        <p class="text-muted">Đang chuyển về trang đăng nhập...</p>
      </div>

      <template v-else>
        <form novalidate @submit.prevent="onSubmit">
          <Input
            v-model="form.password"
            label="Mật khẩu mới"
            type="password"
            autocomplete="new-password"
            placeholder="Nhập mật khẩu mới"
            required
          />
          <div class="reset__checklist">
            <span v-for="(rule, idx) in passwordRules" :key="idx" :class="{ 'reset__check--ok': rule.ok }">
              {{ rule.ok ? '✓' : '○' }} {{ rule.label }}
            </span>
          </div>
          <Input
            v-model="form.confirm"
            label="Xác nhận mật khẩu mới"
            type="password"
            autocomplete="new-password"
            placeholder="Nhập lại mật khẩu mới"
            required
          />
          <p v-if="error" class="reset__error" role="alert">{{ error }}</p>
          <Button type="submit" class="reset__submit" :loading="submitting" block>
            Đặt lại mật khẩu
          </Button>
        </form>
        <RouterLink class="reset__back" :to="{ name: 'login' }">← Quay lại đăng nhập</RouterLink>
      </template>
    </div>
  </main>
</template>

<style scoped>
.reset {
  min-height: 70vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

.reset__card {
  width: 100%;
  max-width: 24rem;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.reset__title { font-size: var(--text-2xl); }

.reset__checklist {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 12px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

.reset__check--ok { color: var(--color-success); }

.reset__error { color: var(--color-destructive); font-size: var(--text-sm); }

.reset__submit { margin-top: var(--space-md); }

.reset__success { display: flex; flex-direction: column; gap: var(--space-sm); font-size: var(--text-sm); }

.reset__back { font-size: var(--text-sm); }
</style>
