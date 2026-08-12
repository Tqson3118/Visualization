<script setup lang="ts">
// RegisterView — Màn 02 đăng ký: validation inline, checklist mật khẩu sống,
// checkbox "Tôi là giảng viên" + đồng ý chính sách (SDD §8.4A Màn 02).
import { computed, reactive, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';

import { useAuthStore } from '@/stores/auth';
import { ApiError } from '@/api/client';
import { messages } from '@/i18n/vi';
import { isValidEmail, validatePassword } from '@/utils/validators';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

const auth = useAuthStore();
const router = useRouter();

const form = reactive({
  displayName: '',
  email: '',
  password: '',
  confirmPassword: '',
  isTeacher: false,
  agreePolicy: false,
});

const touched = reactive({ displayName: false, email: false, password: false, confirmPassword: false });
const fieldErrors = reactive<Record<string, string>>({});
const submitError = ref('');
const submitting = ref(false);
const registeredTeacher = ref(false);

const passwordRules = computed(() => [
  { key: 'length', ok: form.password.length >= 8 && form.password.length <= 64, label: '8-64 ký tự' },
  { key: 'upper', ok: /[A-Z]/.test(form.password), label: 'Có chữ hoa' },
  { key: 'lower', ok: /[a-z]/.test(form.password), label: 'Có chữ thường' },
  { key: 'digit', ok: /\d/.test(form.password), label: 'Có chữ số' },
  { key: 'special', ok: /[^A-Za-z0-9]/.test(form.password), label: 'Có ký tự đặc biệt' },
]);

const passwordOkCount = computed(() => passwordRules.value.filter((r) => r.ok).length);

function validate(): boolean {
  const errors: Record<string, string> = {};
  if (!form.displayName.trim() || form.displayName.trim().length < 2) {
    errors.displayName = 'Họ tên phải từ 2 ký tự';
  }
  if (!isValidEmail(form.email)) errors.email = messages.auth.invalidEmail;
  const pwd = validatePassword(form.password);
  if (!pwd.ok) errors.password = messages.auth.passwordRequirement;
  if (form.confirmPassword !== form.password) errors.confirmPassword = 'Mật khẩu xác nhận không khớp';
  if (!form.agreePolicy) errors.agreePolicy = 'Bạn phải đồng ý chính sách';
  Object.assign(fieldErrors, errors);
  return Object.keys(errors).length === 0;
}

function onBlur(field: keyof typeof touched): void {
  touched[field] = true;
  validate();
}

async function onSubmit(): Promise<void> {
  submitError.value = '';
  if (!validate()) return;
  submitting.value = true;
  try {
    await auth.register({
      displayName: form.displayName.trim(),
      email: form.email.trim().toLowerCase(),
      password: form.password,
      isTeacher: form.isTeacher,
    });
    if (form.isTeacher) {
      registeredTeacher.value = true;
    } else {
      await router.replace({ name: 'path' });
    }
  } catch (err) {
    if (err instanceof ApiError) {
      submitError.value = err.message;
    } else {
      submitError.value = messages.auth.loginFailed;
    }
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <main class="register">
    <form class="register__card card" novalidate @submit.prevent="onSubmit">
      <h1 class="register__title">{{ messages.auth.registerTitle }}</h1>

      <p v-if="registeredTeacher" class="register__pending" role="status">
        🎉 Đăng ký thành công! Tài khoản giảng viên đang <strong>chờ duyệt</strong> — bạn sẽ nhận email
        khi được duyệt.
        <RouterLink :to="{ name: 'login' }">Về đăng nhập</RouterLink>
      </p>

      <template v-if="!registeredTeacher">
        <Input
          v-model="form.displayName"
          label="Họ tên"
          :error="touched.displayName ? fieldErrors.displayName : ''"
          placeholder="Nguyễn Văn A"
          autocomplete="name"
          required
          @blur="onBlur('displayName')"
        />

        <Input
          v-model="form.email"
          label="Email"
          type="email"
          :error="touched.email ? fieldErrors.email : ''"
          :placeholder="messages.auth.emailPlaceholder"
          autocomplete="email"
          required
          @blur="onBlur('email')"
        />

        <Input
          v-model="form.password"
          label="Mật khẩu"
          type="password"
          :error="touched.password ? fieldErrors.password : ''"
          :placeholder="messages.auth.passwordPlaceholder"
          autocomplete="new-password"
          required
          @blur="onBlur('password')"
        />

        <div class="register__checklist" role="list" :aria-label="'Yêu cầu mật khẩu'">
          <span
            v-for="rule in passwordRules"
            :key="rule.key"
            class="register__check"
            :class="{ 'register__check--ok': rule.ok }"
          >
            {{ rule.ok ? '✓' : '○' }} {{ rule.label }}
          </span>
        </div>

        <Input
          v-model="form.confirmPassword"
          label="Xác nhận mật khẩu"
          type="password"
          :error="touched.confirmPassword ? fieldErrors.confirmPassword : ''"
          autocomplete="new-password"
          required
          @blur="onBlur('confirmPassword')"
        />

        <label class="register__row">
          <input v-model="form.isTeacher" type="checkbox" />
          <span>Tôi là giảng viên (tài khoản chờ Admin duyệt)</span>
        </label>

        <label class="register__row">
          <input v-model="form.agreePolicy" type="checkbox" />
          <span>Đồng ý với <RouterLink :to="{ name: 'privacy' }">chính sách bảo mật</RouterLink></span>
        </label>
        <p v-if="fieldErrors.agreePolicy" class="register__error" role="alert">
          {{ fieldErrors.agreePolicy }}
        </p>

        <p v-if="submitError" class="register__error" role="alert">{{ submitError }}</p>

        <Button type="submit" class="register__submit" :loading="submitting" block>
          {{ messages.auth.registerSubmit }}
        </Button>

        <p class="text-muted register__switch">
          {{ messages.auth.hasAccount }}
          <RouterLink :to="{ name: 'login' }">{{ messages.auth.toLogin }}</RouterLink>
        </p>
      </template>
    </form>
  </main>
</template>

<style scoped>
.register {
  min-height: 70vh;
  display: flex;
  align-items: flex-start;
  justify-content: center;
  padding: var(--space-lg);
}

.register__card {
  width: 100%;
  max-width: 26rem;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.register__title { font-size: var(--text-2xl); margin-bottom: var(--space-md); }

.register__checklist {
  display: flex;
  flex-wrap: wrap;
  gap: 4px 12px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  padding-inline: var(--space-xs);
}

.register__check--ok { color: var(--color-success); font-weight: 600; }

.register__row {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  cursor: pointer;
}

.register__error {
  color: var(--color-destructive);
  font-size: var(--text-sm);
}

.register__submit { margin-top: var(--space-md); }

.register__pending {
  background: color-mix(in srgb, var(--color-success) 12%, transparent);
  border: 1px solid var(--color-success);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  font-size: var(--text-sm);
}

.register__switch { text-align: center; margin-top: var(--space-md); }
</style>
