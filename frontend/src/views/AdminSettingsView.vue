<script setup lang="ts">
// AdminSettingsView — Màn N-5: cấu hình hệ thống (GET/PUT /settings)
// View-quality 14/08 (Nhóm D): banner surface band level-2; section title
// không dùng accent (chỉ interactive); error alert semantic + nút Thử lại;
// panel form không shadow (DESIGN §6).
import { onMounted, reactive, ref } from 'vue';
import { AlertTriangle, Bug, Cpu, Globe, KeyRound, RefreshCw, Save, Settings, ShieldCheck } from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { BugReportDto } from '@/api/admin';
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

// ── Báo cáo lỗi & vi phạm (v2.15 — Vấn đề 7/12) ──
const reports = ref<BugReportDto[]>([]);
const reportsLoading = ref(true);
const reportsError = ref('');
const adminNotes = reactive<Record<number, string>>({});

async function loadReports(): Promise<void> {
  reportsLoading.value = true;
  reportsError.value = '';
  try {
    reports.value = await adminApi.fetchBugReports();
  } catch {
    reportsError.value = 'Không thể tải báo cáo lỗi & vi phạm.';
  } finally {
    reportsLoading.value = false;
  }
}

async function saveReport(report: BugReportDto): Promise<void> {
  try {
    const saved = await adminApi.updateBugReport(report.id, {
      status: report.status,
      adminNote: adminNotes[report.id]?.trim() || undefined,
    });
    Object.assign(report, saved);
    adminNotes[report.id] = '';
    ui.showToast('Đã cập nhật báo cáo.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Cập nhật báo cáo thất bại.', 'error');
  }
}

const reportStatusLabel: Record<BugReportDto['status'], string> = {
  NEW: 'Mới',
  PROCESSING: 'Đang xử lý',
  RESOLVED: 'Đã xử lý',
  CLOSED: 'Đã đóng',
};

async function load(): Promise<void> {
  loading.value = true;
  error.value = '';
  try {
    const settings = await adminApi.fetchSettings();
    Object.assign(form, settings);
    domainsText.value = (settings.allowedDomains ?? []).join(', ');
  } catch {
    error.value = messages.admin.settings.loadError;
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  void load();
  void loadReports();
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
    <!-- Banner: surface band level-2 (DESIGN §1/#1 — KHÔNG gradient, KHÔNG shadow) -->
    <header class="admin-settings__hero">
      <div class="admin-settings__hero-inner">
        <div class="admin-settings__hero-main">
          <div class="admin-settings__hero-badges">
            <Badge variant="primary">
              <ShieldCheck :size="12" /> {{ messages.admin.badge }}
            </Badge>
          </div>
          <h1 class="admin-settings__title">{{ messages.admin.settings.title }}</h1>
          <p class="admin-settings__sub">{{ messages.admin.settings.subtitle }}</p>
        </div>
      </div>
    </header>

    <AdminNav active="settings" />

    <div v-if="loading" class="admin-settings__loading" aria-busy="true">
      <Skeleton v-for="i in 5" :key="i" height="72px" />
    </div>

    <form v-else class="admin-settings__form" novalidate @submit.prevent="save">
      <div v-if="error" class="admin-settings__error" role="alert">
        <AlertTriangle :size="16" aria-hidden="true" />
        <span class="admin-settings__error-text">{{ error }}</span>
        <Button size="sm" variant="secondary" type="button" @click="load">
          <RefreshCw :size="14" /> {{ messages.admin.settings.retry }}
        </Button>
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
          <Save :size="16" /> {{ messages.admin.settings.save }}
        </Button>
      </div>
    </form>

    <!-- Báo cáo lỗi & vi phạm (v2.15) -->
    <section class="admin-settings__reports">
      <h2 class="admin-settings__section-title">
        <Bug :size="16" aria-hidden="true" /> Báo cáo lỗi &amp; vi phạm
      </h2>
      <p class="admin-settings__reports-sub">
        Báo cáo lỗi từ người dùng và báo cáo vi phạm bài học (CONTENT_VIOLATION). Chọn trạng thái và
        nhập phản hồi (AdminNote) để xử lý.
      </p>

      <div v-if="reportsLoading" class="admin-settings__loading" aria-busy="true">
        <Skeleton v-for="i in 3" :key="i" height="64px" />
      </div>

      <div v-else-if="reportsError" class="admin-settings__error" role="alert">
        <AlertTriangle :size="16" aria-hidden="true" />
        <span class="admin-settings__error-text">{{ reportsError }}</span>
        <Button size="sm" variant="secondary" @click="loadReports">
          <RefreshCw :size="14" /> Thử lại
        </Button>
      </div>

      <div v-else-if="reports.length === 0" class="admin-settings__reports-empty text-muted">
        Chưa có báo cáo nào.
      </div>

      <div v-else class="admin-settings__reports-list">
        <article v-for="report in reports" :key="report.id" class="admin-settings__report">
          <div class="admin-settings__report-head">
            <Badge :variant="report.status === 'NEW' ? 'primary' : report.status === 'PROCESSING' ? 'warning' : 'success'">
              {{ reportStatusLabel[report.status] }}
            </Badge>
            <span class="admin-settings__report-date text-muted">
              {{ new Date(report.createdAt).toLocaleString('vi-VN') }}
            </span>
          </div>
          <p class="admin-settings__report-desc">{{ report.description }}</p>
          <p v-if="report.context" class="admin-settings__report-context text-muted">
            <code>{{ report.context }}</code>
          </p>
          <p v-if="report.adminNote" class="admin-settings__report-note">
            <strong>Phản hồi:</strong> {{ report.adminNote }}
          </p>
          <div class="admin-settings__report-actions">
            <select v-model="report.status" class="admin-settings__report-status">
              <option v-for="(label, key) in reportStatusLabel" :key="key" :value="key">{{ label }}</option>
            </select>
            <input
              v-model="adminNotes[report.id]"
              class="admin-settings__report-note-input"
              placeholder="Phản hồi của Admin (tùy chọn)..."
            />
            <Button size="sm" @click="saveReport(report)">Lưu</Button>
          </div>
        </article>
      </div>
    </section>
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

/* ── Banner: surface band level-2 (DESIGN §6) — không gradient, không shadow ── */
.admin-settings__hero {
  border-bottom: 1px solid var(--border-subtle);
  background: var(--card-raised);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.admin-settings__hero-inner {
  display: flex;
  align-items: flex-end;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.admin-settings__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
}

.admin-settings__hero-badges { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.admin-settings__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  margin: 0;
  color: var(--foreground);
}

.admin-settings__sub {
  color: var(--foreground-secondary);
  font-size: var(--text-sm);
  max-width: 60ch;
  margin: 0;
}

.admin-settings__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

/* ── Form (panel không shadow — DESIGN §6) ── */
.admin-settings__form {
  display: flex;
  flex-direction: column;
  gap: var(--space-xl);
  padding: var(--space-xl);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.admin-settings__error {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
  color: var(--destructive);
  font-size: var(--text-sm);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}

.admin-settings__error-text { flex: 1; min-width: 200px; }

.admin-settings__section { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-settings__section + .admin-settings__section { border-top: 1px solid var(--border); padding-top: var(--space-xl); }

.admin-settings__section-title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin: 0;
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--foreground);
}

.admin-settings__section-title :deep(svg) { color: var(--foreground-secondary); }

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
  accent-color: var(--primary);
  cursor: pointer;
}

.admin-settings__actions { display: flex; justify-content: flex-end; border-top: 1px solid var(--border); padding-top: var(--space-lg); }

/* ── Báo cáo lỗi & vi phạm ── */
.admin-settings__reports {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-xl);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.admin-settings__reports-sub { color: var(--foreground-secondary); font-size: var(--text-sm); margin: 0; }

.admin-settings__reports-empty { font-size: var(--text-sm); padding: var(--space-md) 0; }

.admin-settings__reports-list { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-settings__report {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  padding: var(--space-md);
}

.admin-settings__report-head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }

.admin-settings__report-date { font-size: var(--text-xs); }

.admin-settings__report-desc { margin: 0; font-size: var(--text-sm); }

.admin-settings__report-context { margin: 0; font-size: var(--text-xs); overflow-wrap: anywhere; }

.admin-settings__report-context code {
  background: var(--surface-muted, color-mix(in srgb, var(--foreground) 6%, transparent));
  padding: 2px 6px;
  border-radius: 4px;
  font-family: var(--font-mono);
}

.admin-settings__report-note { margin: 0; font-size: var(--text-sm); color: var(--foreground); }

.admin-settings__report-actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

.admin-settings__report-status {
  padding: 6px 8px;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--card);
  color: var(--foreground);
  font-size: var(--text-sm);
}

.admin-settings__report-note-input {
  flex: 1;
  min-width: 180px;
  padding: 6px 10px;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--card);
  color: var(--foreground);
  font-size: var(--text-sm);
  font-family: inherit;
}

@media (max-width: 640px) {
  .admin-settings__hero { padding: var(--space-lg); }
  .admin-settings__form { padding: var(--space-lg); }
  .admin-settings__row { grid-template-columns: 1fr; }
}
</style>
