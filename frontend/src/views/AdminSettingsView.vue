<script setup lang="ts">
// AdminSettingsView — Màn N-5: cấu hình hệ thống (GET/PUT /settings)
import { onMounted, reactive, ref } from 'vue';

import * as adminApi from '@/api/admin';
import type { SystemSettingsDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const ui = useUiStore();
const loading = ref(true);
const saving = ref(false);
const error = ref('');

const form = reactive<SystemSettingsDto>({
  siteName: 'DSA Visual',
  allowedDomains: [],
  passwordPolicy: { minLength: 8, requireUppercase: true, requireDigit: true, requireSpecial: true },
  uploadMaxMb: 5,
  sandboxSeconds: 10,
  sandboxMemoryMb: 64,
});

const domainsText = ref('');

onMounted(async () => {
  try {
    const settings = await adminApi.fetchSettings();
    Object.assign(form, settings);
    domainsText.value = (settings.allowedDomains ?? []).join(', ');
  } catch {
    error.value = 'Không thể tải cấu hình (backend chưa khả dụng — hiển thị mặc định).';
  } finally {
    loading.value = false;
  }
});

async function save(): Promise<void> {
  saving.value = true;
  error.value = '';
  try {
    const payload: Partial<SystemSettingsDto> = {
      ...form,
      allowedDomains: domainsText.value.split(',').map((d) => d.trim()).filter(Boolean),
    };
    await adminApi.updateSettings(payload);
    ui.showToast('Đã lưu cấu hình hệ thống.', 'success');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Lưu thất bại.';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <main class="admin-settings container">
    <h1 class="admin-settings__title">⚙️ Cấu hình hệ thống</h1>

    <AdminNav active="settings" />

    <div v-if="loading" class="admin-settings__loading">
      <Skeleton v-for="i in 5" :key="i" height="56px" />
    </div>

    <form v-else class="admin-settings__form card" novalidate @submit.prevent="save">
      <p v-if="error" class="admin-settings__error" role="alert">{{ error }}</p>

      <Input v-model="form.siteName" label="Tên hệ thống" />

      <Input
        :model-value="domainsText"
        label="Domain email được phép đăng ký (phân cách dấu phẩy)"
        placeholder="university.edu.vn, school.edu.vn"
        @update:model-value="domainsText = $event"
      />

      <div class="admin-settings__row">
        <Input v-model.number="form.passwordPolicy.minLength" label="Độ dài mật khẩu tối thiểu" type="number" min="6" max="32" />
        <Input v-model.number="form.uploadMaxMb" label="Giới hạn upload (MB)" type="number" min="1" max="50" />
      </div>

      <div class="admin-settings__row">
        <Input v-model.number="form.sandboxSeconds" label="Giới hạn sandbox (giây)" type="number" min="1" max="30" />
        <Input v-model.number="form.sandboxMemoryMb" label="Giới hạn bộ nhớ sandbox (MB)" type="number" min="16" max="256" />
      </div>

      <fieldset class="admin-settings__checks">
        <legend class="label">Chính sách mật khẩu</legend>
        <label><input v-model="form.passwordPolicy.requireUppercase" type="checkbox" /> Yêu cầu chữ hoa</label>
        <label><input v-model="form.passwordPolicy.requireDigit" type="checkbox" /> Yêu cầu chữ số</label>
        <label><input v-model="form.passwordPolicy.requireSpecial" type="checkbox" /> Yêu cầu ký tự đặc biệt</label>
      </fieldset>

      <div class="admin-settings__actions">
        <Button type="submit" :loading="saving">Lưu cấu hình</Button>
      </div>
    </form>
  </main>
</template>

<style scoped>
.admin-settings {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 720px;
}

.admin-settings__title { font-size: var(--text-2xl); }

.admin-settings__form { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-settings__error { color: var(--color-destructive); font-size: var(--text-sm); }

.admin-settings__row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

.admin-settings__checks { border: none; display: flex; flex-direction: column; gap: var(--space-xs); }

.admin-settings__checks label {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.admin-settings__actions { display: flex; justify-content: flex-end; }
</style>
