<script setup lang="ts">
// AdminSettingsView — Màn N-5: cấu hình hệ thống (GET/PUT /settings)
// View-quality 14/08 (Nhóm D): banner surface band level-2; error alert semantic + nút Thử lại.
// Task 3b (ui-redesign): cài đặt nhóm thành card level-1 (Bảo mật / Hệ thống) theo
// DESIGN §6 — mỗi card có icon + mô tả ngắn; checkbox chính sách mật khẩu → switch
// toggle mượt (role="switch", knob translate, focus ring); GIỮ NGUYÊN API/save hiện có.
import { onMounted, reactive, ref } from 'vue';
import { AlertTriangle, Bug, Cpu, RefreshCw, Save, ShieldCheck } from 'lucide-vue-next';

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
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import PageHero from '@/components/ui/PageHero.vue';

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
    reportsError.value = messages.admin.settings.reportsLoadError;
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
  NEW: messages.admin.settings.reportStatusNew,
  PROCESSING: messages.admin.settings.reportStatusProcessing,
  RESOLVED: messages.admin.settings.reportStatusResolved,
  CLOSED: messages.admin.settings.reportStatusClosed,
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
    <!-- Banner: surface band level-2 (PageHero — DESIGN §1/#1: KHÔNG gradient, KHÔNG shadow) -->
    <PageHero :title="messages.admin.settings.title" :description="messages.admin.settings.subtitle">
      <template #badges>
        <Badge variant="primary">
          <ShieldCheck :size="12" /> {{ messages.admin.badge }}
        </Badge>
      </template>
    </PageHero>

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

      <div class="admin-settings__grid">
        <!-- Bảo mật: domain cho phép + chính sách mật khẩu -->
        <Card class="admin-settings__group">
          <CardHeader class="admin-settings__group-head">
            <span class="admin-settings__group-icon" aria-hidden="true"><ShieldCheck :size="18" /></span>
            <div class="admin-settings__group-meta">
              <CardTitle class="admin-settings__group-title">{{ messages.admin.settings.groupSecurity }}</CardTitle>
              <CardDescription class="admin-settings__group-desc">{{ messages.admin.settings.groupSecurityDesc }}</CardDescription>
            </div>
          </CardHeader>
          <CardContent class="admin-settings__group-body">
            <Input
              :model-value="domainsText"
              :label="messages.admin.settings.domains"
              :placeholder="messages.admin.settings.domainsPlaceholder"
              :disabled="saving"
              @update:model-value="domainsText = $event"
            />
            <div class="admin-settings__subgroup">
              <p class="admin-settings__subgroup-title">{{ messages.admin.settings.sectionPassword }}</p>
              <Input
                v-model.number="form.passwordPolicy.minLength"
                :label="messages.admin.settings.minLength"
                type="number"
                min="6"
                max="32"
                :disabled="saving"
              />
              <div class="admin-settings__switches">
                <div class="admin-settings__switch-row">
                  <span class="admin-settings__switch-label">{{ messages.admin.settings.requireUpper }}</span>
                  <button
                    type="button"
                    role="switch"
                    :aria-checked="form.passwordPolicy.requireUppercase"
                    :aria-label="messages.admin.settings.requireUpper"
                    class="admin-settings__switch"
                    :class="{ 'admin-settings__switch--on': form.passwordPolicy.requireUppercase }"
                    :disabled="saving"
                    @click="form.passwordPolicy.requireUppercase = !form.passwordPolicy.requireUppercase"
                  >
                    <span class="admin-settings__switch-knob" aria-hidden="true" />
                  </button>
                </div>
                <div class="admin-settings__switch-row">
                  <span class="admin-settings__switch-label">{{ messages.admin.settings.requireDigit }}</span>
                  <button
                    type="button"
                    role="switch"
                    :aria-checked="form.passwordPolicy.requireDigit"
                    :aria-label="messages.admin.settings.requireDigit"
                    class="admin-settings__switch"
                    :class="{ 'admin-settings__switch--on': form.passwordPolicy.requireDigit }"
                    :disabled="saving"
                    @click="form.passwordPolicy.requireDigit = !form.passwordPolicy.requireDigit"
                  >
                    <span class="admin-settings__switch-knob" aria-hidden="true" />
                  </button>
                </div>
                <div class="admin-settings__switch-row">
                  <span class="admin-settings__switch-label">{{ messages.admin.settings.requireSpecial }}</span>
                  <button
                    type="button"
                    role="switch"
                    :aria-checked="form.passwordPolicy.requireSpecial"
                    :aria-label="messages.admin.settings.requireSpecial"
                    class="admin-settings__switch"
                    :class="{ 'admin-settings__switch--on': form.passwordPolicy.requireSpecial }"
                    :disabled="saving"
                    @click="form.passwordPolicy.requireSpecial = !form.passwordPolicy.requireSpecial"
                  >
                    <span class="admin-settings__switch-knob" aria-hidden="true" />
                  </button>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <!-- Hệ thống: tên hệ thống + sandbox/upload -->
        <Card class="admin-settings__group">
          <CardHeader class="admin-settings__group-head">
            <span class="admin-settings__group-icon" aria-hidden="true"><Cpu :size="18" /></span>
            <div class="admin-settings__group-meta">
              <CardTitle class="admin-settings__group-title">{{ messages.admin.settings.groupSystem }}</CardTitle>
              <CardDescription class="admin-settings__group-desc">{{ messages.admin.settings.groupSystemDesc }}</CardDescription>
            </div>
          </CardHeader>
          <CardContent class="admin-settings__group-body">
            <Input v-model="form.siteName" :label="messages.admin.settings.siteName" :disabled="saving" />
            <div class="admin-settings__row">
              <Input v-model.number="form.uploadMaxMb" :label="messages.admin.settings.uploadMax" type="number" min="1" max="50" :disabled="saving" />
              <Input v-model.number="form.sandboxSeconds" :label="messages.admin.settings.sandboxSeconds" type="number" min="1" max="30" :disabled="saving" />
            </div>
            <div class="admin-settings__row">
              <Input v-model.number="form.sandboxMemoryMb" :label="messages.admin.settings.sandboxMemory" type="number" min="16" max="256" :disabled="saving" />
            </div>
          </CardContent>
        </Card>
      </div>

      <div class="admin-settings__actions">
        <Button type="submit" :loading="saving" class="admin-settings__save">
          <Save :size="16" /> {{ messages.admin.settings.save }}
        </Button>
      </div>
    </form>

    <!-- Báo cáo lỗi & vi phạm (v2.15) — card level-1 cùng hệ nhóm -->
    <Card class="admin-settings__reports">
      <CardHeader class="admin-settings__group-head">
        <span class="admin-settings__group-icon" aria-hidden="true"><Bug :size="18" /></span>
        <div class="admin-settings__group-meta">
          <CardTitle class="admin-settings__group-title">{{ messages.admin.settings.sectionReports }}</CardTitle>
          <CardDescription class="admin-settings__group-desc">{{ messages.admin.settings.reportsSub }}</CardDescription>
        </div>
      </CardHeader>
      <CardContent class="admin-settings__group-body">
        <div v-if="reportsLoading" class="admin-settings__loading" aria-busy="true">
          <Skeleton v-for="i in 3" :key="i" height="64px" />
        </div>

        <div v-else-if="reportsError" class="admin-settings__error" role="alert">
          <AlertTriangle :size="16" aria-hidden="true" />
          <span class="admin-settings__error-text">{{ reportsError }}</span>
          <Button size="sm" variant="secondary" @click="loadReports">
            <RefreshCw :size="14" /> {{ messages.admin.settings.reportsRetry }}
          </Button>
        </div>

        <div v-else-if="reports.length === 0" class="admin-settings__reports-empty text-muted">
          {{ messages.admin.settings.reportsEmpty }}
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
              <select v-model="report.status" class="admin-settings__report-status" :disabled="saving">
                <option v-for="(label, key) in reportStatusLabel" :key="key" :value="key">{{ label }}</option>
              </select>
              <input
                v-model="adminNotes[report.id]"
                class="admin-settings__report-note-input"
                :placeholder="messages.admin.settings.reportNotePlaceholder"
                :disabled="saving"
              />
              <Button size="sm" @click="saveReport(report)">{{ messages.admin.settings.reportSave }}</Button>
            </div>
          </article>
        </div>
      </CardContent>
    </Card>
  </main>
</template>

<style scoped>
.admin-settings {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 960px;
}

.admin-settings__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

/* ── Form (panel không shadow — DESIGN §6) ── */
.admin-settings__form {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
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

/* ── Nhóm cài đặt: card level-1, mô tả ngắn dưới tiêu đề (Task 3b) ── */
.admin-settings__grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
  align-items: start;
}

.admin-settings__group { min-width: 0; }

.admin-settings__group-head { display: flex; flex-direction: row; align-items: flex-start; gap: var(--space-sm); padding-bottom: var(--space-sm); }

.admin-settings__group-icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.admin-settings__group-meta { min-width: 0; }

.admin-settings__group-title {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
}

.admin-settings__group-desc { font-size: var(--text-sm); color: var(--foreground-secondary); }

.admin-settings__group-body {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.admin-settings__subgroup {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding-top: var(--space-md);
  border-top: 1px solid var(--border);
}

.admin-settings__subgroup-title {
  margin: 0;
  font-size: var(--text-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--foreground-tertiary);
}

.admin-settings__row { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

/* ── Switch toggle (Task 3b): knob translate mượt + focus ring ── */
.admin-settings__switches { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-settings__switch-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
}

.admin-settings__switch-label { font-size: var(--text-sm); color: var(--foreground); }

.admin-settings__switch {
  position: relative;
  width: 36px;
  height: 20px;
  padding: 0;
  border: 1px solid var(--border-strong);
  border-radius: var(--radius-full);
  background: var(--muted);
  cursor: pointer;
  flex-shrink: 0;
  transition: background-color 150ms, border-color 150ms;
}

.admin-settings__switch:hover { border-color: var(--primary); }

.admin-settings__switch:focus-visible { outline: 2px solid var(--ring); outline-offset: 2px; }

.admin-settings__switch:disabled { opacity: 0.55; cursor: not-allowed; }

.admin-settings__switch--on {
  background: var(--primary);
  border-color: var(--primary);
}

.admin-settings__switch-knob {
  position: absolute;
  top: 2px;
  left: 2px;
  width: 14px;
  height: 14px;
  border-radius: 50%;
  background: var(--card-raised);
  transition: transform 180ms cubic-bezier(0.16, 1, 0.3, 1);
}

.admin-settings__switch--on .admin-settings__switch-knob { transform: translateX(16px); }

.admin-settings__actions { display: flex; justify-content: flex-end; border-top: 1px solid var(--border); padding-top: var(--space-lg); }

/* ── Báo cáo lỗi & vi phạm ── */
.admin-settings__reports { min-width: 0; }

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

.admin-settings__report-date {
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  font-variant-numeric: tabular-nums;
}

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

/* Input/select báo cáo: padding chữ-viền ≥ 8px + chiều cao ≥ 36px (DESIGN §4.4) */
.admin-settings__report-status {
  min-height: 36px;
  padding: 8px 12px;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--card);
  color: var(--foreground);
  font-size: var(--text-sm);
  font-family: inherit;
  transition: border-color 150ms;
}

.admin-settings__report-status:focus-visible { outline: 2px solid var(--ring); outline-offset: 1px; border-color: var(--primary); }

.admin-settings__report-note-input {
  flex: 1;
  min-width: 180px;
  min-height: 36px;
  padding: 8px 12px;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--card);
  color: var(--foreground);
  font-size: var(--text-sm);
  font-family: inherit;
  transition: border-color 150ms;
}

.admin-settings__report-note-input::placeholder { color: var(--foreground-quaternary); }

.admin-settings__report-note-input:focus-visible { outline: 2px solid var(--ring); outline-offset: 1px; border-color: var(--primary); }

@media (max-width: 800px) {
  .admin-settings__grid { grid-template-columns: 1fr; }
}

@media (max-width: 640px) {
  .admin-settings__row { grid-template-columns: 1fr; }
}
</style>
