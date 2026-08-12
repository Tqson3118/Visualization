<script setup lang="ts">
import { reactive, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';

import { useAuthStore } from '@/stores/auth';
import { messages } from '@/i18n/vi';
import { isValidEmail } from '@/utils/validators';

const auth = useAuthStore();
const route = useRoute();
const router = useRouter();

const form = reactive({
  email: '',
  password: '',
});

const submitError = ref('');
const submitting = ref(false);

async function onSubmit(): Promise<void> {
  submitError.value = '';
  if (!isValidEmail(form.email)) {
    submitError.value = messages.auth.invalidEmail;
    return;
  }
  submitting.value = true;
  try {
    await auth.login(form.email, form.password);
    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/path';
    await router.replace(redirect);
  } catch {
    submitError.value = messages.auth.loginFailed;
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <main class="login">
    <form class="login__card card" novalidate @submit.prevent="onSubmit">
      <h1 class="login__title">{{ messages.auth.loginTitle }}</h1>

      <label class="label" for="email">{{ messages.auth.email }}</label>
      <input
        id="email"
        v-model="form.email"
        class="input"
        type="email"
        autocomplete="email"
        :placeholder="messages.auth.emailPlaceholder"
      />

      <label class="label mt-md" for="password">{{ messages.auth.password }}</label>
      <input
        id="password"
        v-model="form.password"
        class="input"
        type="password"
        autocomplete="current-password"
        :placeholder="messages.auth.passwordPlaceholder"
      />

      <p v-if="submitError" class="login__error" role="alert">{{ submitError }}</p>

      <button class="btn btn-primary login__submit" type="submit" :disabled="submitting">
        {{ submitting ? messages.common.loading : messages.auth.loginSubmit }}
      </button>

      <p class="text-muted login__switch">
        {{ messages.auth.noAccount }}
        <RouterLink :to="{ name: 'register' }">{{ messages.auth.toRegister }}</RouterLink>
      </p>
    </form>
  </main>
</template>

<style scoped>
.login {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
}

.login__card {
  width: 100%;
  max-width: 24rem;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.login__title {
  font-size: var(--text-2xl);
  margin-bottom: var(--space-md);
}

.login__error {
  color: var(--color-destructive);
  font-size: var(--text-sm);
  margin-top: var(--space-sm);
}

.login__submit {
  margin-top: var(--space-md);
}

.login__switch {
  text-align: center;
  margin-top: var(--space-md);
}
</style>
