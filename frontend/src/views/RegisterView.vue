<script setup lang="ts">
// RegisterView — Màn 02 đăng ký: validation inline, checklist mật khẩu sống,
// segmented "Đăng ký với vai trò" (Sinh viên/Giảng viên — task L) + form con giảng viên
// + đồng ý chính sách (SDD §8.4A Màn 02).
import { computed, reactive, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';

import { useAuthStore } from '@/stores/auth';
import { ApiError } from '@/api/client';
import { messages } from '@/i18n/vi';
import { isValidEmail, validatePassword } from '@/utils/validators';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';

type RegisterRole = 'student' | 'teacher';

const TEACHER_BIO_MAX = 500;

const auth = useAuthStore();
const router = useRouter();

const form = reactive({
  displayName: '',
  email: '',
  password: '',
  confirmPassword: '',
  department: '',
  staffCode: '',
  teacherBio: '',
  agreePolicy: false,
});

const role = ref<RegisterRole>('student');

const touched = reactive({
  displayName: false,
  email: false,
  password: false,
  confirmPassword: false,
  department: false,
  staffCode: false,
  teacherBio: false,
});
const fieldErrors = reactive<Record<string, string>>({});
const submitError = ref('');
const submitting = ref(false);
const registeredTeacher = ref(false);

const roleOptions = computed<{ value: RegisterRole; label: string }[]>(() => [
  { value: 'student', label: messages.auth.roleStudent },
  { value: 'teacher', label: messages.auth.roleTeacher },
]);

const passwordRules = computed(() => [
  { key: 'length', ok: form.password.length >= 8 && form.password.length <= 64, label: messages.auth.passwordRuleLength },
  { key: 'upper', ok: /[A-Z]/.test(form.password), label: messages.auth.passwordRuleUpper },
  { key: 'lower', ok: /[a-z]/.test(form.password), label: messages.auth.passwordRuleLower },
  { key: 'digit', ok: /\d/.test(form.password), label: messages.auth.passwordRuleDigit },
  { key: 'special', ok: /[^A-Za-z0-9]/.test(form.password), label: messages.auth.passwordRuleSpecial },
]);

function selectRole(next: RegisterRole): void {
  role.value = next;
  validate();
}

function validate(): boolean {
  const errors: Record<string, string> = {};
  if (!form.displayName.trim() || form.displayName.trim().length < 2) {
    errors.displayName = messages.auth.displayNameMinLength;
  }
  if (!isValidEmail(form.email)) errors.email = messages.auth.invalidEmail;
  const pwd = validatePassword(form.password);
  if (!pwd.ok) errors.password = messages.auth.passwordRequirement;
  if (form.confirmPassword !== form.password) errors.confirmPassword = messages.auth.confirmPasswordMismatch;
  if (role.value === 'teacher') {
    if (!form.department.trim()) errors.department = messages.auth.departmentRequired;
    if (!form.staffCode.trim()) errors.staffCode = messages.auth.staffCodeRequired;
    if (form.teacherBio.length > TEACHER_BIO_MAX) errors.teacherBio = messages.auth.teacherBioMax;
  }
  if (!form.agreePolicy) errors.agreePolicy = messages.auth.agreePolicyRequired;
  // Xóa key cũ không còn trong errors — tránh stale error khi chuyển vai trò / nhập lại
  for (const key of Object.keys(fieldErrors)) {
    if (!(key in errors)) delete fieldErrors[key];
  }
  Object.assign(fieldErrors, errors);
  return Object.keys(errors).length === 0;
}

function onBlur(field: keyof typeof touched): void {
  touched[field] = true;
  validate();
}

function markAllTouched(): void {
  for (const key of Object.keys(touched)) {
    touched[key as keyof typeof touched] = true;
  }
}

async function onSubmit(): Promise<void> {
  submitError.value = '';
  markAllTouched(); // submit = chạm mọi field → lỗi inline hiện ngay dù chưa blur
  if (!validate()) return;
  submitting.value = true;
  try {
    const isTeacher = role.value === 'teacher';
    await auth.register({
      displayName: form.displayName.trim(),
      email: form.email.trim().toLowerCase(),
      password: form.password,
      isTeacher,
      ...(isTeacher
        ? {
            department: form.department.trim(),
            staffCode: form.staffCode.trim(),
            teacherBio: form.teacherBio.trim(),
          }
        : {}),
    });
    if (isTeacher) {
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
        {{ messages.auth.teacherPendingSuccess }}
        <RouterLink :to="{ name: 'login' }">{{ messages.auth.teacherPendingToLogin }}</RouterLink>
      </p>

      <template v-if="!registeredTeacher">
        <Input
          v-model="form.displayName"
          :label="messages.auth.displayName"
          :error="touched.displayName ? fieldErrors.displayName : ''"
          :placeholder="messages.auth.displayNamePlaceholder"
          autocomplete="name"
          required
          @blur="onBlur('displayName')"
        />

        <Input
          v-model="form.email"
          :label="messages.auth.email"
          type="email"
          :error="touched.email ? fieldErrors.email : ''"
          :placeholder="messages.auth.emailPlaceholder"
          autocomplete="email"
          required
          @blur="onBlur('email')"
        />

        <Input
          v-model="form.password"
          :label="messages.auth.password"
          type="password"
          :error="touched.password ? fieldErrors.password : ''"
          :placeholder="messages.auth.passwordPlaceholder"
          autocomplete="new-password"
          required
          @blur="onBlur('password')"
        />

        <div class="register__checklist" role="list" :aria-label="messages.auth.passwordChecklistAria">
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
          :label="messages.auth.confirmPassword"
          type="password"
          :error="touched.confirmPassword ? fieldErrors.confirmPassword : ''"
          autocomplete="new-password"
          required
          @blur="onBlur('confirmPassword')"
        />

        <fieldset class="register__role">
          <legend class="label register__role-label">{{ messages.auth.roleLabel }}</legend>
          <div class="register__role-group">
            <button
              v-for="opt in roleOptions"
              :key="opt.value"
              type="button"
              :aria-pressed="role === opt.value"
              class="register__role-option"
              :class="{ 'register__role-option--active': role === opt.value }"
              @click="selectRole(opt.value)"
            >
              {{ opt.label }}
            </button>
          </div>
        </fieldset>

        <div v-if="role === 'teacher'" class="register__teacher">
          <Input
            v-model="form.department"
            :label="messages.auth.department"
            :error="touched.department ? fieldErrors.department : ''"
            :placeholder="messages.auth.departmentPlaceholder"
            autocomplete="organization"
            :maxlength="100"
            required
            @blur="onBlur('department')"
          />

          <Input
            v-model="form.staffCode"
            :label="messages.auth.staffCode"
            :error="touched.staffCode ? fieldErrors.staffCode : ''"
            :placeholder="messages.auth.staffCodePlaceholder"
            autocomplete="off"
            :maxlength="50"
            required
            @blur="onBlur('staffCode')"
          />

          <div class="register__field">
            <label class="label" for="register-teacher-bio">{{ messages.auth.teacherBio }}</label>
            <textarea
              id="register-teacher-bio"
              v-model="form.teacherBio"
              class="input register__bio"
              :placeholder="messages.auth.teacherBioPlaceholder"
              :maxlength="TEACHER_BIO_MAX"
              :aria-invalid="Boolean(touched.teacherBio && fieldErrors.teacherBio)"
              @blur="onBlur('teacherBio')"
            ></textarea>
            <div class="register__bio-meta">
              <p
                v-if="touched.teacherBio && fieldErrors.teacherBio"
                class="register__error"
                role="alert"
              >
                {{ fieldErrors.teacherBio }}
              </p>
              <span class="register__bio-count">{{ form.teacherBio.length }}/{{ TEACHER_BIO_MAX }}</span>
            </div>
          </div>

          <p class="register__note">{{ messages.auth.teacherPendingNote }}</p>
        </div>

        <label class="register__row">
          <input v-model="form.agreePolicy" type="checkbox" />
          <span>
            {{ messages.auth.agreePolicy }}
            <RouterLink :to="{ name: 'privacy' }">{{ messages.auth.privacyPolicy }}</RouterLink>
          </span>
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

/* ── Segmented chọn vai trò ── */
.register__role {
  border: none;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.register__role-label { margin-bottom: 0; }

.register__role-group {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
  padding: 4px;
  background: var(--color-muted);
  border-radius: var(--radius-md);
}

.register__role-option {
  padding: 0.5rem;
  border: 1px solid transparent;
  border-radius: calc(var(--radius-md) - 4px);
  background: transparent;
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-muted);
  transition: var(--transition-fast);
}

.register__role-option:hover { color: var(--color-primary); }

.register__role-option--active {
  background: var(--color-surface);
  border-color: var(--color-border);
  color: var(--color-primary);
  box-shadow: var(--shadow-sm);
}

/* ── Form con giảng viên ── */
.register__teacher {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 4%, transparent);
}

.register__field { display: flex; flex-direction: column; gap: var(--space-xs); }

.register__bio { min-height: 96px; resize: vertical; line-height: 1.5; }

.register__bio-meta {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-sm);
}

.register__bio-count {
  margin-left: auto;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  font-variant-numeric: tabular-nums;
}

.register__note { font-size: var(--text-xs); color: var(--color-text-muted); }

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
