<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import { useRouter } from 'vue-router';
import { Plus, Trash2, ArrowUp, ArrowDown, ExternalLink, BookOpen, Layers, Sparkles, Trophy } from 'lucide-vue-next';
import { courseApi, type CourseDetailDto, type CourseUpsertPayload } from '@/services/courseApi';
import { useUiStore } from '@/stores/ui';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Badge from '@/components/ui/Badge.vue';

export interface LessonOption {
  id: number;
  title: string;
  simulationCount?: number;
}

const props = defineProps<{
  open: boolean;
  courseId?: string | number | null;
  lessons: LessonOption[];
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'saved', course: CourseDetailDto): void;
}>();

const ui = useUiStore();
const router = useRouter();

const activeTab = ref<'info' | 'objectives' | 'curriculum'>('info');
const loading = ref(false);
const saving = ref(false);

const form = reactive({
  title: '',
  description: '',
  category: 'Cấu trúc dữ liệu',
  difficulty: 'Beginner',
  sortOrder: 1,
  isActive: true,
  learningObjectives: [] as string[],
  keyOutcomes: [] as { title: string; desc: string }[],
  highlights: [] as { title: string; description: string }[],
});

// Dynamic items inside curriculum
interface CourseNodeItem {
  id?: number;
  title: string;
  lessonId?: number;
  sortOrder: number;
}

const curriculumNodes = ref<CourseNodeItem[]>([]);
const newObjectiveText = ref('');
const newOutcomeTitle = ref('');
const newOutcomeDesc = ref('');
const newHighlightTitle = ref('');
const newHighlightDesc = ref('');
const selectedLessonIdToAdd = ref<number | ''>('');

watch(
  () => props.open,
  async (isOpen, oldOpen) => {
    if (!isOpen || isOpen === oldOpen) return;
    activeTab.value = 'info';
    if (props.courseId) {
      await loadCourseDetail(props.courseId);
    } else {
      resetForm();
    }
  },
  { immediate: true },
);

function resetForm(): void {
  form.title = '';
  form.description = '';
  form.category = 'Cấu trúc dữ liệu';
  form.difficulty = 'Beginner';
  form.sortOrder = 1;
  form.isActive = true;
  form.learningObjectives = [
    'Nắm vững các cấu trúc dữ liệu và giải thuật cốt lõi',
    'Thực hành mô phỏng từng bước và giải bài tập trên web',
  ];
  form.keyOutcomes = [
    { title: 'Thành thạo lập trình DSA', desc: 'Cài đặt và vận dụng thuật toán giải quyết bài toán thực tế' },
    { title: 'Tự tin phỏng vấn kỹ thuật', desc: 'Phân tích độ phức tạp thời gian và không gian Big-O chính xác' },
  ];
  form.highlights = [
    { title: 'Mô phỏng từng bước', description: 'Trực quan hóa thuật toán chạy thực tế' },
    { title: 'Chấm điểm tự động', description: 'Làm trắc nghiệm và nộp code testcase tức thì' },
  ];
  curriculumNodes.value = [];
}

async function loadCourseDetail(id: string | number): Promise<void> {
  loading.value = true;
  try {
    const data = await courseApi.getCourseById(String(id));
    form.title = data.title;
    form.description = data.description;
    form.category = data.category || 'Cấu trúc dữ liệu';
    form.difficulty = data.difficulty || 'Beginner';
    form.sortOrder = 1;
    form.isActive = data.isPublished ?? true;
    form.learningObjectives = Array.isArray(data.learningObjectives) ? [...data.learningObjectives] : [];
    
    // Key outcomes
    if (Array.isArray(data.keyOutcomes)) {
      form.keyOutcomes = data.keyOutcomes.map((item) => {
        if (typeof item === 'string') return { title: 'Kết quả đạt được', desc: item };
        return item as unknown as { title: string; desc: string };
      });
    } else {
      form.keyOutcomes = [];
    }

    // Highlights
    form.highlights = Array.isArray(data.highlights)
      ? data.highlights.map((h) => ({ title: h.title, description: h.description }))
      : [];

    // Curriculum Lessons / Nodes
    curriculumNodes.value = (data.lessons ?? []).map((l, index) => ({
      id: Number(l.id) || undefined,
      title: l.title,
      lessonId: Number(l.id) || undefined,
      sortOrder: l.orderIndex || index + 1,
    }));
  } catch (err) {
    ui.showToast('Không tải được thông tin lộ trình.', 'error');
  } finally {
    loading.value = false;
  }
}

// ── Learning Objectives Operations ──
function addObjective(): void {
  const text = newObjectiveText.value.trim();
  if (!text) return;
  form.learningObjectives.push(text);
  newObjectiveText.value = '';
}

function removeObjective(index: number): void {
  form.learningObjectives.splice(index, 1);
}

// ── Key Outcomes Operations ──
function addOutcome(): void {
  const title = newOutcomeTitle.value.trim();
  const desc = newOutcomeDesc.value.trim();
  if (!title || !desc) {
    ui.showToast('Vui lòng nhập cả tiêu đề và mô tả kết quả.', 'warning');
    return;
  }
  form.keyOutcomes.push({ title, desc });
  newOutcomeTitle.value = '';
  newOutcomeDesc.value = '';
}

function removeOutcome(index: number): void {
  form.keyOutcomes.splice(index, 1);
}

// ── Highlights Operations ──
function addHighlight(): void {
  const title = newHighlightTitle.value.trim();
  const description = newHighlightDesc.value.trim();
  if (!title || !description) {
    ui.showToast('Vui lòng nhập cả tiêu đề và mô tả điểm nổi bật.', 'warning');
    return;
  }
  form.highlights.push({ title, description });
  newHighlightTitle.value = '';
  newHighlightDesc.value = '';
}

function removeHighlight(index: number): void {
  form.highlights.splice(index, 1);
}

// ── Curriculum Nodes Operations ──
function addLessonToCurriculum(): void {
  if (selectedLessonIdToAdd.value === '') return;
  const lesson = props.lessons.find((l) => l.id === Number(selectedLessonIdToAdd.value));
  if (!lesson) return;

  const alreadyAdded = curriculumNodes.value.some((n) => n.lessonId === lesson.id);
  if (alreadyAdded) {
    ui.showToast('Bài học này đã có trong lộ trình.', 'warning');
    return;
  }

  curriculumNodes.value.push({
    title: lesson.title,
    lessonId: lesson.id,
    sortOrder: curriculumNodes.value.length + 1,
  });

  selectedLessonIdToAdd.value = '';
}

function moveNodeUp(index: number): void {
  if (index <= 0) return;
  const temp = curriculumNodes.value[index];
  curriculumNodes.value[index] = curriculumNodes.value[index - 1];
  curriculumNodes.value[index - 1] = temp;
  updateSortIndices();
}

function moveNodeDown(index: number): void {
  if (index >= curriculumNodes.value.length - 1) return;
  const temp = curriculumNodes.value[index];
  curriculumNodes.value[index] = curriculumNodes.value[index + 1];
  curriculumNodes.value[index + 1] = temp;
  updateSortIndices();
}

function removeNode(index: number): void {
  curriculumNodes.value.splice(index, 1);
  updateSortIndices();
}

function updateSortIndices(): void {
  curriculumNodes.value.forEach((node, idx) => {
    node.sortOrder = idx + 1;
  });
}

// ── Save Entire Course ──
async function handleSave(): Promise<void> {
  if (form.title.trim().length < 3) {
    ui.showToast('Tiêu đề lộ trình phải từ 3 ký tự trở lên.', 'warning');
    activeTab.value = 'info';
    return;
  }

  saving.value = true;
  try {
    const payload: CourseUpsertPayload = {
      title: form.title.trim(),
      description: form.description.trim(),
      category: form.category,
      difficulty: form.difficulty,
      sortOrder: form.sortOrder,
      isActive: form.isActive,
      learningObjectives: form.learningObjectives,
      keyOutcomes: form.keyOutcomes.map((o) => o.desc || o.title),
      highlights: form.highlights,
    };

    let savedCourse: CourseDetailDto;
    if (props.courseId) {
      savedCourse = await courseApi.updateCourse(props.courseId, payload);
    } else {
      savedCourse = await courseApi.createCourse(payload);
    }

    // Save Curriculum Nodes if newly created / updated
    const targetCourseId = savedCourse.id || props.courseId;
    if (targetCourseId) {
      // Sync added nodes
      for (const node of curriculumNodes.value) {
        if (!node.id && node.lessonId) {
          try {
            await courseApi.addCourseNode(targetCourseId, {
              title: node.title,
              lessonId: node.lessonId,
              sortOrder: node.sortOrder,
            });
          } catch {
            // continue
          }
        }
      }
    }

    ui.showToast(props.courseId ? 'Đã cập nhật lộ trình thành công!' : 'Đã tạo lộ trình mới thành công!', 'success');
    emit('saved', savedCourse);
    emit('close');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể lưu lộ trình.', 'error');
  } finally {
    saving.value = false;
  }
}

function previewOnWeb(): void {
  if (props.courseId) {
    router.push(`/path/${props.courseId}`);
  }
}
</script>

<template>
  <Modal
    :open="open"
    :title="courseId ? 'Chỉnh sửa Lộ trình học' : 'Tạo Lộ trình học mới (Course Builder)'"
    class="max-w-4xl"
    @close="emit('close')"
  >
    <div v-if="loading" class="py-12 text-center text-vdsa-muted">
      <div class="inline-block w-8 h-8 border-4 border-accent/20 border-t-accent rounded-full animate-spin"></div>
      <p class="mt-3 text-sm font-semibold">Đang tải dữ liệu lộ trình...</p>
    </div>

    <div v-else class="space-y-6">
      <!-- Sub-Tabs Navigation -->
      <div class="flex border-b border-vdsa-border gap-2 pb-2">
        <button
          type="button"
          class="px-4 py-2 text-sm font-bold rounded-lg transition-colors flex items-center gap-2"
          :class="activeTab === 'info' ? 'bg-vdsa-accent text-white shadow-md' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover'"
          @click="activeTab = 'info'"
        >
          <Layers :size="16" /> 1. Thông tin chung
        </button>
        <button
          type="button"
          class="px-4 py-2 text-sm font-bold rounded-lg transition-colors flex items-center gap-2"
          :class="activeTab === 'objectives' ? 'bg-vdsa-accent text-white shadow-md' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover'"
          @click="activeTab = 'objectives'"
        >
          <Sparkles :size="16" /> 2. Mục tiêu & Điểm nổi bật
        </button>
        <button
          type="button"
          class="px-4 py-2 text-sm font-bold rounded-lg transition-colors flex items-center gap-2"
          :class="activeTab === 'curriculum' ? 'bg-vdsa-accent text-white shadow-md' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover'"
          @click="activeTab = 'curriculum'"
        >
          <BookOpen :size="16" /> 3. Cấu trúc bài học ({{ curriculumNodes.length }})
        </button>
      </div>

      <!-- TAB 1: THÔNG TIN CHUNG -->
      <div v-if="activeTab === 'info'" class="space-y-4">
        <Input v-model="form.title" label="Tên Lộ trình / Khóa học" placeholder="Ví dụ: Cấu trúc dữ liệu nâng cao & Đồ thị" required />

        <div>
          <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Mô tả tổng quan</label>
          <textarea
            v-model="form.description"
            rows="3"
            class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl p-3 text-sm text-white focus:outline-none focus:border-accent transition-colors placeholder:text-vdsa-disabled resize-y"
            placeholder="Giới thiệu khái quát về nội dung, kiến thức đạt được và đối tượng phù hợp..."
          ></textarea>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Danh mục</label>
            <select
              v-model="form.category"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            >
              <option value="Cấu trúc dữ liệu">Cấu trúc dữ liệu</option>
              <option value="Giải thuật">Giải thuật</option>
              <option value="Cơ bản">Cơ bản</option>
              <option value="Trung cấp">Trung cấp</option>
              <option value="Nâng cao">Nâng cao</option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Cấp độ</label>
            <select
              v-model="form.difficulty"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            >
              <option value="Beginner">Beginner (Cơ bản)</option>
              <option value="Intermediate">Intermediate (Trung cấp)</option>
              <option value="Advanced">Advanced (Nâng cao)</option>
            </select>
          </div>
        </div>

        <div class="flex items-center gap-3 pt-2">
          <input
            id="course-active-toggle"
            v-model="form.isActive"
            type="checkbox"
            class="w-4 h-4 rounded text-accent focus:ring-accent accent-purple-600 cursor-pointer"
          />
          <label for="course-active-toggle" class="text-sm font-semibold text-white cursor-pointer select-none">
            Công khai Lộ trình (Hiển thị ngay trên trang danh sách Lộ trình học)
          </label>
        </div>
      </div>

      <!-- TAB 2: MỤC TIÊU & ĐIỂM NỔI BẬT -->
      <div v-if="activeTab === 'objectives'" class="space-y-6">
        <!-- Mục tiêu học tập -->
        <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-3">
          <div class="flex items-center justify-between">
            <h4 class="text-sm font-bold text-white uppercase tracking-wider flex items-center gap-2">
              <Sparkles :size="16" class="text-vdsa-yellow" /> Mục tiêu học tập (Learning Objectives)
            </h4>
            <Badge variant="secondary">{{ form.learningObjectives.length }} mục tiêu</Badge>
          </div>

          <div class="space-y-2">
            <div
              v-for="(obj, i) in form.learningObjectives"
              :key="i"
              class="flex items-center justify-between gap-3 p-2.5 rounded-lg bg-vdsa-bg-secondary border border-vdsa-border text-sm text-vdsa-secondary group"
            >
              <span class="flex items-center gap-2">
                <span class="w-5 h-5 rounded-full bg-vdsa-green/20 text-vdsa-green font-bold text-xs flex items-center justify-center shrink-0">
                  {{ i + 1 }}
                </span>
                {{ obj }}
              </span>
              <button
                type="button"
                class="text-vdsa-muted hover:text-vdsa-red transition-colors p-1"
                title="Xóa mục tiêu này"
                @click="removeObjective(i)"
              >
                <Trash2 :size="14" />
              </button>
            </div>
          </div>

          <div class="flex gap-2 pt-2">
            <input
              v-model="newObjectiveText"
              type="text"
              placeholder="Thêm mục tiêu mới (VD: Làm chủ bảng băm tra cứu O(1))..."
              class="flex-1 bg-vdsa-bg border border-vdsa-border rounded-lg px-3 py-2 text-xs text-white placeholder:text-vdsa-disabled focus:outline-none focus:border-accent"
              @keydown.enter.prevent="addObjective"
            />
            <Button size="sm" type="button" @click="addObjective"><Plus :size="14" /> Thêm</Button>
          </div>
        </div>

        <!-- Điểm nổi bật (Highlights) -->
        <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-3">
          <div class="flex items-center justify-between">
            <h4 class="text-sm font-bold text-white uppercase tracking-wider flex items-center gap-2">
              <Trophy :size="16" class="text-vdsa-purple-light" /> Tại sao chọn lộ trình này? (Highlights)
            </h4>
            <Badge variant="secondary">{{ form.highlights.length }} điểm nổi bật</Badge>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
            <div
              v-for="(hl, i) in form.highlights"
              :key="i"
              class="p-3 rounded-lg bg-vdsa-bg-secondary border border-vdsa-border relative group"
            >
              <button
                type="button"
                class="absolute top-2 right-2 text-vdsa-muted hover:text-vdsa-red transition-colors p-1"
                title="Xóa"
                @click="removeHighlight(i)"
              >
                <Trash2 :size="14" />
              </button>
              <h5 class="text-xs font-bold text-white pr-6">{{ hl.title }}</h5>
              <p class="text-xs text-vdsa-muted mt-1">{{ hl.description }}</p>
            </div>
          </div>

          <div class="p-3 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border space-y-2 mt-2">
            <span class="block text-[11px] font-bold text-vdsa-secondary uppercase">Thêm điểm nổi bật mới</span>
            <div class="grid grid-cols-1 sm:grid-cols-12 gap-2">
              <div class="sm:col-span-5">
                <input
                  v-model="newHighlightTitle"
                  type="text"
                  placeholder="Tiêu đề (VD: Mô phỏng từng bước)"
                  class="w-full bg-vdsa-bg border border-vdsa-border rounded-lg px-3 py-2 text-xs text-white placeholder:text-vdsa-disabled focus:outline-none focus:border-accent"
                />
              </div>
              <div class="sm:col-span-5">
                <input
                  v-model="newHighlightDesc"
                  type="text"
                  placeholder="Mô tả chi tiết..."
                  class="w-full bg-vdsa-bg border border-vdsa-border rounded-lg px-3 py-2 text-xs text-white placeholder:text-vdsa-disabled focus:outline-none focus:border-accent"
                  @keydown.enter.prevent="addHighlight"
                />
              </div>
              <div class="sm:col-span-2 flex">
                <Button size="sm" type="button" class="w-full shrink-0 whitespace-nowrap justify-center" @click="addHighlight">
                  <Plus :size="14" /> Thêm
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- TAB 3: CẤU TRÚC BÀI HỌC (CURRICULUM) -->
      <div v-if="activeTab === 'curriculum'" class="space-y-4">
        <!-- Add Lesson Row -->
        <div class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border flex flex-col sm:flex-row items-center gap-3">
          <div class="flex-1 w-full">
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1">Chọn bài học đưa vào Lộ trình</label>
            <select
              v-model="selectedLessonIdToAdd"
              class="w-full bg-vdsa-bg border border-vdsa-border rounded-lg px-3 py-2 text-xs text-white focus:outline-none focus:border-accent"
            >
              <option value="">-- Chọn bài học có sẵn trong hệ thống --</option>
              <option v-for="l in lessons" :key="l.id" :value="l.id">
                #{{ l.id }} - {{ l.title }} ({{ l.simulationCount }} mô phỏng)
              </option>
            </select>
          </div>
          <div class="self-end pt-2 sm:pt-0">
            <Button size="sm" type="button" :disabled="selectedLessonIdToAdd === ''" @click="addLessonToCurriculum">
              <Plus :size="14" /> Thêm vào Lộ trình
            </Button>
          </div>
        </div>

        <!-- Curriculum Table -->
        <div class="border border-vdsa-border rounded-xl overflow-hidden bg-vdsa-surface">
          <div v-if="curriculumNodes.length === 0" class="p-8 text-center text-vdsa-muted text-sm">
            Chưa có bài học nào trong lộ trình này. Hãy chọn bài học ở trên để thêm vào!
          </div>

          <table v-else class="w-full text-left text-xs border-collapse">
            <thead>
              <tr class="bg-vdsa-bg-secondary text-vdsa-muted border-b border-vdsa-border uppercase">
                <th class="p-3 w-12 text-center">STT</th>
                <th class="p-3">Tên bài học</th>
                <th class="p-3 w-32 text-center">Thao tác thứ tự</th>
                <th class="p-3 w-16 text-center">Xóa</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="(node, idx) in curriculumNodes"
                :key="idx"
                class="border-b border-vdsa-border/60 hover:bg-vdsa-hover transition-colors"
              >
                <td class="p-3 text-center font-bold text-vdsa-muted">{{ idx + 1 }}</td>
                <td class="p-3">
                  <div class="font-bold text-white text-sm">{{ node.title }}</div>
                  <div v-if="node.lessonId" class="text-xs text-vdsa-secondary">Mã bài học: #{{ node.lessonId }}</div>
                </td>
                <td class="p-3 text-center">
                  <div class="flex items-center justify-center gap-1">
                    <button
                      type="button"
                      class="p-1 rounded bg-vdsa-surface hover:bg-vdsa-border text-vdsa-secondary hover:text-white disabled:opacity-30 disabled:cursor-not-allowed"
                      :disabled="idx === 0"
                      title="Lên trên"
                      @click="moveNodeUp(idx)"
                    >
                      <ArrowUp :size="14" />
                    </button>
                    <button
                      type="button"
                      class="p-1 rounded bg-vdsa-surface hover:bg-vdsa-border text-vdsa-secondary hover:text-white disabled:opacity-30 disabled:cursor-not-allowed"
                      :disabled="idx === curriculumNodes.length - 1"
                      title="Xuống dưới"
                      @click="moveNodeDown(idx)"
                    >
                      <ArrowDown :size="14" />
                    </button>
                  </div>
                </td>
                <td class="p-3 text-center">
                  <button
                    type="button"
                    class="p-1 text-vdsa-muted hover:text-vdsa-red transition-colors"
                    title="Xóa bài học khỏi lộ trình"
                    @click="removeNode(idx)"
                  >
                    <Trash2 :size="14" />
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex items-center justify-between pt-4 border-t border-vdsa-border">
        <div>
          <button
            v-if="courseId"
            type="button"
            class="text-xs font-bold text-accent hover:underline flex items-center gap-1.5"
            @click="previewOnWeb"
          >
            <ExternalLink :size="14" /> Xem trang lộ trình trên Web
          </button>
        </div>

        <div class="flex items-center gap-3">
          <Button variant="ghost" size="md" type="button" @click="emit('close')">Hủy</Button>
          <Button variant="primary" size="md" type="button" :disabled="saving" @click="handleSave">
            <span v-if="saving" class="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin mr-2"></span>
            {{ courseId ? 'Lưu thay đổi' : 'Tạo Lộ trình' }}
          </Button>
        </div>
      </div>
    </div>
  </Modal>
</template>
