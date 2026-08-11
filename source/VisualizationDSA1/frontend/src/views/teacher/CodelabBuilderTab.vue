<template>
  <section class="codelab-builder-section">
    <div class="flex justify-between items-center mb-6 flex-wrap gap-3">
      <div>
        <h2 class="section-heading m-0">Quản lý Codelab (Thực hành Code)</h2>
        <p class="text-text-muted text-sm mt-1">Tạo bài tập lập trình với testcases, starter code đa ngôn ngữ, hints tiered</p>
      </div>
      <button type="button" class="btn-primary" @click="createNewCodelab">
        <BaseIcon name="plus" class="w-4 h-4 inline mr-1 align-middle" /> Tạo Codelab mới
      </button>
    </div>

    
    <div class="filters-bar mb-6 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 w-full">
      <div class="relative w-full sm:w-64">
        <input v-model="searchQuery" @input="debouncedSearch" type="text" placeholder="Tìm kiếm codelab..." class="appearance-none w-full bg-bg-secondary text-white border border-border-subtle rounded-full pl-10 pr-4 py-2.5 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-accent/30 focus:border-accent/50 transition-all" />
        <div class="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-4 text-text-muted">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
        </div>
      </div>
      <div class="flex gap-2 w-full sm:w-auto">
        <select v-model="filterDifficulty" class="appearance-none bg-bg-secondary text-white border border-border-subtle rounded-full pl-4 pr-10 py-2.5 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-accent/30 focus:border-accent/50 transition-all cursor-pointer">
          <option value="">Tất cả độ khó</option>
          <option value="1">Dễ (1)</option>
          <option value="2">Dễ (2)</option>
          <option value="3">Trung bình (3)</option>
          <option value="4">Khó (4)</option>
          <option value="5">Rất khó (5)</option>
        </select>
        <select v-model="filterLanguage" class="appearance-none bg-bg-secondary text-white border border-border-subtle rounded-full pl-4 pr-10 py-2.5 text-sm font-medium focus:outline-none focus:ring-2 focus:ring-accent/30 focus:border-accent/50 transition-all cursor-pointer">
          <option value="">Tất cả ngôn ngữ</option>
          <option value="csharp">C#</option>
          <option value="python">Python</option>
          <option value="java">Java</option>
          <option value="javascript">JavaScript</option>
          <option value="cpp">C++</option>
          <option value="go">Go</option>
        </select>
      </div>
    </div>

    
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <span>Đang tải danh sách Codelab...</span>
    </div>

    <div v-else-if="codelabsList.length === 0" class="empty-state">
      <div class="text-5xl mb-4">💻</div>
      <h3 class="text-xl font-bold text-white">Chưa có Codelab nào</h3>
      <p class="text-text-muted mt-2 max-w-md">Tạo Codelab đầu tiên để bắt đầu xây dựng bài tập thực hành</p>
      <button class="btn-primary mt-6" @click="createNewCodelab">
        <BaseIcon name="plus" class="w-4 h-4 inline mr-1" /> Tạo Codelab đầu tiên
      </button>
    </div>

    <div v-else class="codelabs-table-container">
      <table class="codelabs-table">
        <thead>
          <tr>
            <th>Codelab</th>
            <th>Độ khó</th>
            <th>Ngôn ngữ</th>
            <th>Testcases</th>
            <th>XP</th>
            <th class="text-center">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <template v-for="c in codelabsList" :key="c.id">
            <tr @click="toggleCodelabAccordion(c.id)" class="cursor-pointer hover:bg-bg-hover transition-colors">
              <td class="font-bold text-white">
                <span class="inline-block mr-1 transition-transform duration-200" :style="expandedCodelabId === c.id ? 'transform: rotate(90deg)' : ''">▶</span>
                {{ c.title }}
              </td>
              <td><span class="diff-badge" :class="'diff-' + c.difficulty">{{ c.difficulty }}</span></td>
              <td class="text-xs text-text-muted font-mono">{{ c.allowedLanguages }}</td>
              <td class="font-mono text-text-secondary">{{ c.testCaseCount }} tests</td>
              <td class="font-bold text-accent-yellow">+{{ c.xpReward }} XP</td>
              <td>
                <div class="flex justify-center gap-2" @click.stop>
                  <button type="button" class="btn-action btn-action--edit" @click="editCodelab(c)" title="Chỉnh sửa">
                    <BaseIcon name="edit" class="w-3.5 h-3.5 inline mr-1 align-text-bottom" /> Sửa
                  </button>
                  <button type="button" class="btn-action btn-action--delete" @click="deleteCodelab(c.id)" title="Xóa">
                    <BaseIcon name="trash" class="w-3.5 h-3.5 inline mr-1 align-text-bottom" /> Xóa
                  </button>
                </div>
              </td>
            </tr>
            <tr v-if="expandedCodelabId === c.id" class="accordion-row">
              <td colspan="6" class="accordion-cell">
                <div v-if="loadingCodelabDetails[c.id]" class="loading-detail py-4">
                  <div class="spinner spinner--sm"></div>
                  <span>Đang tải chi tiết...</span>
                </div>
                <div v-else class="codelab-detail-panel animate-fade-in">
                  <div class="flex justify-between items-center mb-4">
                    <h4 class="detail-title text-accent font-bold m-0"><BaseIcon name="code" class="w-4 h-4 text-accent inline mr-1 align-text-bottom" /> Chi tiết: {{ c.title }}</h4>
                    <div class="flex gap-2">
                      <button type="button" class="btn-add-inline" @click="addTestCase(c)">
                        <BaseIcon name="plus" class="w-3.5 h-3.5 inline mr-1 align-text-bottom" /> Testcase
                      </button>
                      <button type="button" class="btn-add-inline" @click="addTemplate(c)">
                        <BaseIcon name="file-text" class="w-3.5 h-3.5 inline mr-1 align-text-bottom" /> Template
                      </button>
                    </div>
                  </div>
                  <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                    
                    <div class="space-y-3">
                      <h5 class="text-xs font-semibold text-accent uppercase tracking-wider mb-3">Testcases ({{ c.testcases?.length || 0 }})</h5>
                      <div v-if="!c.testcases?.length" class="empty-state py-4 text-center text-sm">Chưa có testcase nào</div>
                      <div v-else class="space-y-2">
                        <div v-for="tc in c.testcases" :key="tc.id" class="testcase-card p-3 rounded-lg border border-border-subtle bg-bg-secondary/20 flex items-center justify-between">
                          <div class="flex items-center gap-3">
                            <span class="badge" :class="tc.isHidden ? 'badge-rose' : 'badge-emerald'">{{ tc.isHidden ? 'Ẩn' : 'Công khai' }}</span>
                            <span class="text-xs text-text-muted font-mono">Input: {{ tc.input }}</span>
                            <span class="text-xs text-text-muted font-mono">Expected: {{ tc.expectedOutput }}</span>
                          </div>
                          <div class="flex gap-2">
                            <button type="button" class="btn-action btn-action--edit text-xs" @click="editTestCase(c, tc)">Sửa</button>
                            <button type="button" class="btn-action btn-action--delete text-xs" @click="deleteTestCase(c.id, tc.id)">Xóa</button>
                          </div>
                        </div>
                      </div>
                      <button type="button" class="btn-add-inline w-full" @click="addTestCase(c)">
                        <BaseIcon name="plus" class="w-3.5 h-3.5 inline mr-1" /> Thêm Testcase
                      </button>
                    </div>
                    
                    
                    <div class="space-y-3">
                      <h5 class="text-xs font-semibold text-accent-green uppercase tracking-wider mb-3">Starter Templates ({{ c.templates?.length || 0 }})</h5>
                      <div v-if="!c.templates?.length" class="empty-state py-4 text-center text-sm">Chưa có template nào</div>
                      <div v-else class="space-y-2">
                        <div v-for="tmpl in c.templates" :key="tmpl.id" class="template-card p-3 rounded-lg border border-border-subtle bg-bg-secondary/20 flex items-center justify-between">
                          <div class="flex items-center gap-3">
                            <span class="badge badge-emerald text-xs">{{ tmpl.language }}</span>
                            <span class="text-xs text-text-muted">Starter code: {{ tmpl.starterCode?.length || 0 }} ký tự</span>
                          </div>
                          <div class="flex gap-2">
                            <button type="button" class="btn-action btn-action--edit text-xs" @click="editTemplate(c, tmpl)">Sửa</button>
                            <button type="button" class="btn-action btn-action--delete text-xs" @click="deleteTemplate(c.id, tmpl.id)">Xóa</button>
                          </div>
                        </div>
                      </div>
                      <button type="button" class="btn-add-inline w-full" @click="addTemplate(c)">
                        <BaseIcon name="plus" class="w-3.5 h-3.5 inline mr-1" /> Thêm Template
                      </button>
                    </div>
                  </div>
                  
                  
                  <div class="mt-6">
                    <h5 class="text-xs font-semibold text-accent-purple uppercase tracking-wider mb-3">Hints ({{ c.hints?.length || 0 }})</h5>
                    <div v-if="!c.hints?.length" class="empty-state py-4 text-center text-sm">Chưa có hint nào</div>
                    <div v-else class="space-y-2">
                      <div v-for="hint in c.hints" :key="hint.id" class="hint-card p-3 rounded-lg border border-border-subtle bg-bg-secondary/20 flex items-center justify-between">
                        <div class="flex items-center gap-3">
                          <span class="badge badge-purple text-xs">Hint #{{ hint.orderIndex }}</span>
                          <span class="badge" :class="hint.isTiered ? 'badge-amber' : 'badge-slate'">{{ hint.isTiered ? 'Tiered (XP cost)' : 'Thường' }}</span>
                          <span class="text-xs text-text-muted line-clamp-1 max-w-xs">{{ hint.content }}</span>
                        </div>
                        <div class="flex gap-2">
                          <button type="button" class="btn-action btn-action--edit text-xs" @click="editHint(c, hint)">Sửa</button>
                          <button type="button" class="btn-action btn-action--delete text-xs" @click="deleteHint(c.id, hint.id)">Xóa</button>
                        </div>
                      </div>
                    </div>
                    <button type="button" class="btn-add-inline w-full" @click="addHint(c)">
                      <BaseIcon name="plus" class="w-3.5 h-3.5 inline mr-1" /> Thêm Hint
                    </button>
                  </div>
                </div>
              </td>
            </tr>
          </template>
        </tbody>
      </table>
    </div>

    
    <CodelabEditorModal
      v-model:show="showCodelabEditor"
      :editing-codelab="editingCodelab"
      @save="saveCodelab"
    />

    
    <TestCaseModal
      v-model:show="showTestCaseModal"
      :editing-testcase="editingTestCase"
      :parent-codelab="editingCodelabForTestCase"
      @save="saveTestCase"
    />

    
    <TemplateModal
      v-model:show="showTemplateModal"
      :editing-template="editingTemplate"
      :parent-codelab="editingCodelabForTemplate"
      @save="saveTemplate"
    />

    
    <HintModal
      v-model:show="showHintModal"
      :editing-hint="editingHint"
      :parent-codelab="editingCodelabForHint"
      @save="saveHint"
    />

    
    <ConfirmModal
      v-model:show="showConfirmDelete"
      :title="confirmDeleteTitle"
      :message="confirmDeleteMessage"
      :confirm-text="'Xóa'"
      :variant="'danger'"
      @confirm="executeDelete"
    />
  </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useTeacherApi } from './useTeacherApi';
import { useToastStore } from '@/composables/useToast';
import { codelabApi } from '@/features/codelabs/api/codelabApi';
import CodelabEditorModal from './CodelabEditorModal.vue';
import TestCaseModal from './TestCaseModal.vue';
import TemplateModal from './TemplateModal.vue';
import HintModal from './HintModal.vue';
import ConfirmModal from '@/components/ui/ConfirmModal.vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';

const { BASE_URL, getAuthHeaders } = useTeacherApi();
const toastStore = useToastStore();

const codelabsList = ref<any[]>([]);
const loading = ref(false);
const searchQuery = ref('');
const filterDifficulty = ref('');
const filterLanguage = ref('');
const expandedCodelabId = ref<string | null>(null);
const loadingCodelabDetails = ref<Record<string, boolean>>({});

const showCodelabEditor = ref(false);
const editingCodelab = ref<any | null>(null);
const showTestCaseModal = ref(false);
const editingTestCase = ref<any | null>(null);
const editingCodelabForTestCase = ref<any | null>(null);
const showTemplateModal = ref(false);
const editingTemplate = ref<any | null>(null);
const editingCodelabForTemplate = ref<any | null>(null);
const showHintModal = ref(false);
const editingHint = ref<any | null>(null);
const editingCodelabForHint = ref<any | null>(null);

const showConfirmDelete = ref(false);
const confirmDeleteTitle = ref('');
const confirmDeleteMessage = ref('');
const deleteAction = ref<() => Promise<void>>();

async function loadCodelabs() {
  loading.value = true;
  try {
    const params: Record<string, string> = {};
    if (searchQuery.value) params.search = searchQuery.value;
    if (filterDifficulty.value) params.difficulty = filterDifficulty.value;
    if (filterLanguage.value) params.language = filterLanguage.value;
    codelabsList.value = await codelabApi.getCodelabs(params);
  } catch (err) {
    console.error('Failed to load codelabs:', err);
    toastStore.error('Lỗi khi tải danh sách Codelab');
  } finally {
    loading.value = false;
  }
}

async function loadCodelabDetails(codelabId: string) {
  loadingCodelabDetails.value[codelabId] = true;
  try {
    const codelab = await codelabApi.getCodelab(codelabId);
    const idx = codelabsList.value.findIndex(c => c.id === codelabId);
    if (idx >= 0) {
      codelabsList.value[idx] = { ...codelabsList.value[idx], ...codelab };
    }
  } catch (err) {
    console.error(err);
    toastStore.error('Lỗi khi tải chi tiết Codelab');
  } finally {
    loadingCodelabDetails.value[codelabId] = false;
  }
}

function toggleCodelabAccordion(codelabId: string) {
  if (expandedCodelabId.value === codelabId) {
    expandedCodelabId.value = null;
  } else {
    expandedCodelabId.value = codelabId;
    loadCodelabDetails(codelabId);
  }
}

let searchTimeout: ReturnType<typeof setTimeout> | null = null;
function debouncedSearch() {
  if (searchTimeout) clearTimeout(searchTimeout);
  searchTimeout = setTimeout(() => loadCodelabs(), 400);
}

function createNewCodelab() {
  showCodelabEditor.value = true;
  editingCodelab.value = null;
}

function editCodelab(c: any) {
  editingCodelab.value = c;
  showCodelabEditor.value = true;
}

function addTestCase(c: any) {
  editingTestCase.value = null;
  editingCodelabForTestCase.value = c;
  showTestCaseModal.value = true;
}
function editTestCase(c: any, tc: any) {
  editingTestCase.value = tc;
  editingCodelabForTestCase.value = c;
  showTestCaseModal.value = true;
}

function addTemplate(c: any) {
  editingTemplate.value = null;
  editingCodelabForTemplate.value = c;
  showTemplateModal.value = true;
}

function editTemplate(c: any, tmpl: any) {
  editingTemplate.value = tmpl;
  editingCodelabForTemplate.value = c;
  showTemplateModal.value = true;
}

function addHint(c: any) {
  editingHint.value = null;
  editingCodelabForHint.value = c;
  showHintModal.value = true;
}

function editHint(c: any, hint: any) {
  editingHint.value = hint;
  editingCodelabForHint.value = c;
  showHintModal.value = true;
}

async function saveCodelab(codelabData: any) {
  try {
    const payload = {
      ...codelabData,
      examples: typeof codelabData.examples === 'string' && codelabData.examples.trim()
        ? JSON.parse(codelabData.examples)
        : codelabData.examples,
    };
    if (codelabData.id) {
      await codelabApi.updateCodelab(codelabData.id, payload);
      toastStore.success('Cập nhật Codelab thành công!');
    } else {
      await codelabApi.createCodelab(payload);
      toastStore.success('Tạo Codelab thành công!');
    }
    await loadCodelabs();
    showCodelabEditor.value = false;
    editingCodelab.value = null;
  } catch (err: any) {
    console.error('Failed to save Codelab:', err);
    toastStore.error(err.response?.data?.message || 'Lỗi khi lưu Codelab');
  }
}

async function saveTestCase(testCaseData: any) {
  try {
    if (testCaseData.id) {
      await codelabApi.updateTestCase(editingCodelabForTestCase.value?.id, testCaseData.id, testCaseData);
      toastStore.success('Cập nhật Testcase thành công!');
    } else {
      await codelabApi.createTestCase(editingCodelabForTestCase.value?.id, testCaseData);
      toastStore.success('Thêm Testcase thành công!');
    }
    await loadCodelabDetails(editingCodelabForTestCase.value?.id);
    showTestCaseModal.value = false;
    editingTestCase.value = null;
  } catch (err: any) {
    console.error('Failed to save Testcase:', err);
    toastStore.error(err.response?.data?.message || 'Lỗi khi lưu Testcase');
  }
}

async function saveTemplate(templateData: any) {
  try {
    if (templateData.id) {
      await codelabApi.updateTemplate(editingCodelabForTemplate.value?.id, templateData.id, templateData);
      toastStore.success('Cập nhật Template thành công!');
    } else {
      await codelabApi.createTemplate(editingCodelabForTemplate.value?.id, templateData);
      toastStore.success('Thêm Template thành công!');
    }
    await loadCodelabDetails(editingCodelabForTemplate.value?.id);
    showTemplateModal.value = false;
    editingTemplate.value = null;
  } catch (err: any) {
    console.error('Failed to save Template:', err);
    toastStore.error(err.response?.data?.message || 'Lỗi khi lưu Template');
  }
}

async function saveHint(hintData: any) {
  try {
    if (hintData.id) {
      await codelabApi.updateHint(editingCodelabForHint.value?.id, hintData.id, hintData);
      toastStore.success('Cập nhật Hint thành công!');
    } else {
      await codelabApi.createHint(editingCodelabForHint.value?.id, hintData);
      toastStore.success('Thêm Hint thành công!');
    }
    await loadCodelabDetails(editingCodelabForHint.value?.id);
    showHintModal.value = false;
    editingHint.value = null;
  } catch (err: any) {
    console.error('Failed to save Hint:', err);
    toastStore.error(err.response?.data?.message || 'Lỗi khi lưu Hint');
  }
}

async function deleteCodelab(codelabId: string) {
  confirmDeleteTitle.value = 'Xóa Codelab';
  confirmDeleteMessage.value = 'Bạn có chắc chắn muốn xóa Codelab này? Hành động này không thể hoàn tác.';
  deleteAction.value = async () => {
    try {
      await codelabApi.deleteCodelab(codelabId);
      toastStore.success('Xóa Codelab thành công!');
      await loadCodelabs();
    } catch (err: any) {
      console.error('Failed to delete Codelab:', err);
      toastStore.error(err.response?.data?.message || 'Lỗi khi xóa Codelab');
    } finally {
      showConfirmDelete.value = false;
      deleteAction.value = undefined;
    }
  };
  showConfirmDelete.value = true;
}

async function deleteTestCase(codelabId: string, testCaseId: string) {
  try {
    await codelabApi.deleteTestCase(codelabId, testCaseId);
    toastStore.success('Xóa Testcase thành công!');
    await loadCodelabDetails(codelabId);
  } catch (err: any) {
    console.error('Failed to delete Testcase:', err);
    toastStore.error(err.response?.data?.message || 'Lỗi khi xóa Testcase');
  }
}

async function deleteTemplate(codelabId: string, templateId: string) {
  try {
    await codelabApi.deleteTemplate(codelabId, templateId);
    toastStore.success('Xóa Template thành công!');
    await loadCodelabDetails(codelabId);
  } catch (err: any) {
    console.error('Failed to delete Template:', err);
    toastStore.error(err.response?.data?.message || 'Lỗi khi xóa Template');
  }
}

async function deleteHint(codelabId: string, hintId: string) {
  try {
    await codelabApi.deleteHint(codelabId, hintId);
    toastStore.success('Xóa Hint thành công!');
    await loadCodelabDetails(codelabId);
  } catch (err: any) {
    console.error('Failed to delete Hint:', err);
    toastStore.error(err.response?.data?.message || 'Lỗi khi xóa Hint');
  }
}

function executeDelete() {
  if (deleteAction.value) deleteAction.value();
  showConfirmDelete.value = false;
  deleteAction.value = undefined;
}

watch([filterDifficulty, filterLanguage], () => loadCodelabs());

onMounted(() => loadCodelabs());
</script>

<style scoped>
@import "./CodelabBuilderTab.css";
</style>