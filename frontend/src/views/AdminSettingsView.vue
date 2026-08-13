<script setup lang="ts">
// AdminSettingsView — Màn N-5: cấu hình hệ thống (GET/PUT /settings)
// H-B: hero Aurora soft + form chia section (Chung / Chính sách mật khẩu /
// Sandbox & Upload) + checkbox styled + error alert; logic/API giữ nguyên.
import { onMounted, reactive, ref } from 'vue';
import { AlertTriangle, Cpu, Globe, KeyRound, Save, Settings, ShieldCheck } from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { SystemSettingsDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Badge from '@/components/ui/Badge.vue';

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
    error.value = messages.admin.settings.loadError;
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
    ui.showToast(messages.admin.settings.saved, 'success');
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Lưu thất bại.';
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <main class="admin-settings container">
    <!-- Hero gradient Aurora soft -->
    <header class="admin-settings__hero">
      <div class="admin-settings__hero-body">
        <span class="admin-settings__hero-icon" aria-hidden="true"><Settings :size="24" /></span>
        <div class="admin-settings__hero-title-wrap">
          <h1 class="admin-settings__title">{{ messages.admin.settings.title }}</h1>
          <p class="admin-settings__sub">{{ messages.admin.settings.subtitle }}</p>
        </div>
        <Badge variant="primary" class="admin-settings__hero-badge">
          <ShieldCheck :size="12" /> {{ messages.admin.badge }}
        </Badge>
      </div>
    </header>

    <AdminNav active="settings" />

    <div v-if="loading" class="admin-settings__loading" aria-busy="true">
      <Skeleton v-for="i in 5" :key="i" height="72px" />
    </div>

    <form v-else class="admin-settings__form card" novalidate @submit.prevent="save">
      <div v-if="error" class="admin-settings__error" role="alert">
        <AlertTriangle :size="16" aria-hidden="true" />
        <span>{{ error }}</span>
      </div>

      <!-- Chung -->
      <section class="admin-settings__section">
        <h2 class="admin-settings__section-title">
          <Globe :size="16" aria-hidden="true" /> {{ messages.admin.settings.sectionGeneral }}
        </h2>
        <Input v-model="form.siteName" :label="messages.admin.settings.siteName" />
        <Input
          :model-value="domainsText"
          :label="messages.admin.settings.domains"
          :placeholder="messages.admin.settings.domainsPlaceholder"
          @update:model-value="domainsText = $event"
        />
      </section>

      <!-- Chính sách mật khẩu -->
      <section class="admin-settings__section">
        <h2 class="admin-settings__section-title">
          <KeyRound :size="16" aria-hidden="true" /> {{ messages.admin.settings.sectionPassword }}
        </h2>
        <div class="admin-settings__row">
          <Input v-model.number="form.passwordPolicy.minLength" :label="messages.admin.settings.minLength" type="number" min="6" max="32" />
        </div>
        <fieldset class="admin-settings__checks">
          <legend class="visually-hidden">{{ messages.admin.settings.sectionPassword }}</legend>
          <label class="admin-settings__check">
            <input v-model="form.passwordPolicy.requireUppercase" type="checkbox" class="admin-settings__checkbox" />
            <span>{{ messages.admin.settings.requireUpper }}</span>
          </label>
          <label class="admin-settings__check">
            <input v-model="form.passwordPolicy.requireDigit" type="checkbox" class="admin-settings__checkbox" />
            <span>{{ messages.admin.settings.requireDigit }}</span>
          </label>
          <label class="admin-settings__check">
            <input v-model="form.passwordPolicy.requireSpecial" type="checkbox" class="admin-settings__checkbox" />
            <span>{{ messages.admin.settings.requireSpecial }}</span>
          </label>
        </fieldset>
      </section>

      <!-- Sandbox & Upload -->
      <section class="admin-settings__section">
        <h2 class="admin-settings__section-title">
          <Cpu :size="16" aria-hidden="true" /> {{ messages.admin.settings.sectionSandbox }}
        </h2>
        <div class="admin-settings__row">
          <Input v-model.number="form.uploadMaxMb" :label="messages.admin.settings.uploadMax" type="number" min="1" max="50" />
          <Input v-model.number="form.sandboxSeconds" :label="messages.admin.settings.sandboxSeconds" type="number" min="1" max="30" />
        </div>
        <div class="admin-settings__row">
          <Input v-model.number="form.sandboxMemoryMb" :label="messages.admin.settings.sandboxMemory" type="number" min="16" max="256" />
        </div>
      </section>

      <div class="admin-settings__actions">
        <Button type="submit" :loading="saving" class="admin-settings__save">
          <Save :size="15" /> {{ messages.admin.settings.save }}
        </Button>
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
  max-width: 760px;
}

/* ── Hero gradient Aurora soft ── */
.admin-settings__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.admin-settings__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.admin-settings__hero::before {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  top: -120px;
  right: -60px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.admin-settings__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.admin-settings__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.admin-settings__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.admin-settings__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.admin-settings__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.admin-settings__hero-badge { margin-left: auto; }

.admin-settings__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

/* ── Form ── */
.admin-settings__form { display: flex; flex-direction: column; gap: var(--space-xl); }

.admin-settings__section { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-settings__section + .admin-settings__section { border-top: 1px solid var(--color-border); padding-top: var(--space-xl); }

.admin-settings__section-title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-md);
  color: var(--color-primary);
}

.admin-settings__error {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  color: var(--color-destructive);
  font-size: var(--text-sm);
  background: color-mix(in srgb, var(--color-destructive) 8%, var(--color-surface));
  border: 1px solid color-mix(in srgb, var(--color-destructive) 35%, var(--color-border));
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}

.admin-settings__row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

.admin-settings__checks { border: none; display: flex; flex-direction: column; gap: var(--space-sm); margin: 0; padding: 0; }

.admin-settings__check {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  cursor: pointer;
  width: fit-content;
}

.admin-settings__checkbox {
  width: 16px;
  height: 16px;
  accent-color: var(--color-primary);
  cursor: pointer;
}

.admin-settings__actions { display: flex; justify-content: flex-end; border-top: 1px solid var(--color-border); padding-top: var(--space-lg); }

@media (max-width: 640px) {
  .admin-settings__hero-badge { margin-left: 0; }
  .admin-settings__row { grid-template-columns: 1fr; }
}
</style>
