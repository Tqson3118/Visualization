<template>
  <div class="roadmap-editor p-6 max-w-7xl mx-auto h-full flex flex-col">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6 shrink-0">
      <div class="flex items-center space-x-4">
        <button class="btn btn-secondary !p-2" @click="router.push('/teacher-studio')">
          <BaseIcon name="chevron-left" class="w-5 h-5" />
        </button>
        <div>
          <h1 class="text-2xl font-bold text-text-primary flex items-center">
            {{ roadmap?.name || 'Đang tải...' }}
            <span v-if="roadmap" class="ml-3 px-2 py-1 text-xs rounded bg-bg-hover text-text-secondary">
              {{ roadmap.status }}
            </span>
          </h1>
        </div>
      </div>
      <div class="flex space-x-3">
        <button class="btn btn-primary" @click="showAddNodeModal = true" :disabled="!roadmap">
          Thêm Node mới
        </button>
        <button class="btn btn-secondary bg-accent/20 text-accent hover:bg-accent/40" @click="publishRoadmap" :disabled="!canPublish">
          Xuất bản Lộ trình
        </button>
      </div>
    </div>

    <!-- Main Content -->
    <div class="flex flex-col lg:flex-row flex-1 gap-6 min-h-0">
      <!-- Sidebar (Nodes List) -->
      <div class="w-full lg:w-1/3 lg:max-w-xs glass-panel rounded-xl flex flex-col overflow-hidden lg:shrink-0">
        <div class="p-4 border-b border-border-default font-bold text-text-primary flex justify-between items-center">
          <span>Danh sách Bài giảng (Nodes)</span>
          <span class="text-xs bg-bg-hover px-2 py-1 rounded">{{ roadmap?.nodes?.length || 0 }}</span>
        </div>
        
        <div class="flex-1 overflow-y-auto p-4 space-y-3 max-h-64 lg:max-h-none">
          <div v-if="!roadmap?.nodes?.length" class="text-center text-text-muted py-8 text-sm">
            Chưa có Node nào. Nhấn "Thêm Node mới" để bắt đầu.
          </div>
          
          <div 
            v-for="(node, index) in sortedNodes" 
            :key="node.id"
            class="p-3 rounded-lg border cursor-pointer transition-colors"
            :class="selectedNode?.id === node.id ? 'bg-accent-dark/40 border-border-accent' : 'bg-bg-secondary border-border-default hover:border-border-default'"
            @click="selectNode(node)"
          >
            <div class="flex justify-between items-start mb-1">
              <h4 class="font-bold text-text-primary text-sm">
                {{ index + 1 }}. {{ node.name }}
              </h4>
              <span class="text-xs px-1.5 py-0.5 rounded" :class="node.isComplete ? 'bg-green-900/50 text-green-400' : 'bg-red-900/50 text-accent-red'">
                {{ node.isComplete ? 'Hoàn tất' : 'Thiếu Practice' }}
              </span>
            </div>
            <p class="text-xs text-text-secondary line-clamp-1 mb-2">{{ node.description }}</p>
            <div class="flex gap-2 text-xs text-text-muted">
              <span v-if="node.quizId" class="text-accent"><BaseIcon name="quiz" class="w-3 h-3 inline-block mr-0.5 align-text-bottom" />Quiz</span>
              <span v-if="node.labId" class="text-green-400"><BaseIcon name="code-ide" class="w-3 h-3 inline-block mr-0.5 align-text-bottom" />Lab</span>
              <span v-if="node.leetCodeId" class="text-accent-warm"><BaseIcon name="code-ide" class="w-3 h-3 inline-block mr-0.5 align-text-bottom" />LC</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Editor Panel -->
      <div class="w-full lg:w-2/3 glass-panel rounded-xl overflow-y-auto p-6">
        <div v-if="!selectedNode" class="h-full flex flex-col items-center justify-center text-text-muted">
          <BaseIcon name="hand-click" class="w-16 h-16 mb-4 opacity-50" />
          <p>Chọn một Node bên trái để chỉnh sửa chi tiết</p>
        </div>
        
        <div v-else>
          <div class="flex justify-between items-center mb-6">
            <h2 class="text-xl font-bold text-text-primary">Chỉnh sửa: {{ selectedNode.name }}</h2>
            <button class="btn btn-secondary !py-1 !px-2 text-accent-red hover:bg-accent-red/20 border-accent-red/50" @click="deleteSelectedNode">
              Xóa Node
            </button>
          </div>
          
          <div class="space-y-8">
            <!-- Section 1: Content -->
            <div class="glass-panel p-5 rounded-lg border border-border-default">
              <h3 class="font-bold text-text-primary mb-4 border-b border-border-default pb-2">1. Nội dung (Content)</h3>
              
              <div class="mb-5">
                <label class="block text-xs font-semibold text-text-secondary mb-1.5 uppercase tracking-wider">Nội dung văn bản (Rich Text / Markdown)</label>
                <textarea 
                  v-model="editorForm.contentJson"
                  rows="5"
                  class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent focus:ring-4 focus:ring-accent/10 transition-all resize-y"
                  placeholder="Nhập nội dung bài giảng tại đây..."
                ></textarea>
              </div>
              
              <div class="mb-5">
                <label class="block text-xs font-semibold text-text-secondary mb-1.5 uppercase tracking-wider">Video URL (Tùy chọn)</label>
                <input 
                  v-model="editorForm.videoUrl"
                  type="url"
                  class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent focus:ring-4 focus:ring-accent/10 transition-all"
                  placeholder="https://youtube.com/..."
                />
              </div>

              <div class="mb-5">
                <label class="block text-xs font-semibold text-text-secondary mb-1.5 uppercase tracking-wider">Visualizer ID (Thuật toán minh họa)</label>
                <input 
                  v-model="editorForm.visualizerId"
                  type="text"
                  class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent focus:ring-4 focus:ring-accent/10 transition-all"
                  placeholder="Nhập ID của Visualizer (nếu có)"
                />
              </div>

              <!-- G3.5.6 — Visualizer Config -->
              <div class="mb-5">
                <label class="block text-xs font-semibold text-text-secondary mb-1.5 uppercase tracking-wider">Visualizer Config (bám bài học — G3.5.6)</label>
                <div class="grid grid-cols-1 md:grid-cols-3 gap-3 mb-3">
                  <div>
                    <label class="block text-[10px] font-semibold text-text-muted mb-1">Thuật toán / Khái niệm</label>
                    <select
                      v-model="vizForm.algorithm"
                      class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-3 py-2.5 text-text-primary focus:outline-none focus:border-border-accent transition-all"
                    >
                      <option value="">— Chọn —</option>
                      <option v-for="algo in algorithmOptions" :key="algo.id" :value="algo.id">{{ algo.name }}</option>
                    </select>
                  </div>
                  <div>
                    <label class="block text-[10px] font-semibold text-text-muted mb-1">Input mẫu (dữ liệu demo)</label>
                    <input
                      v-model="vizForm.sampleInput"
                      type="text"
                      class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-3 py-2.5 text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent transition-all"
                      placeholder="VD: 5,3,8,1,9,2,7"
                    />
                  </div>
                  <div>
                    <label class="block text-[10px] font-semibold text-text-muted mb-1">Tốc độ</label>
                    <select
                      v-model="vizForm.speed"
                      class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-3 py-2.5 text-text-primary focus:outline-none focus:border-border-accent transition-all"
                    >
                      <option :value="0.5">0.5x</option>
                      <option :value="1">1x</option>
                      <option :value="2">2x</option>
                    </select>
                  </div>
                </div>
                <p class="text-[10px] text-text-muted mb-2">Học sinh sẽ thấy đúng thuật toán/khái niệm này trong màn Trực Quan Hóa của bài (LessonVisualizer).</p>
              </div>
              
              <div class="flex justify-end">
                <button class="btn btn-primary btn-sm" @click="saveNodeContent" :disabled="isSaving">
                  Lưu Nội dung
                </button>
              </div>
            </div>
            
            <!-- Section 2: Practice -->
            <div class="glass-panel p-5 rounded-lg border border-border-default">
              <h3 class="font-bold text-text-primary mb-4 border-b border-border-default pb-2">
                2. Bài tập thực hành (Cần ít nhất 1 bài tập)
              </h3>
              
              <div class="grid grid-cols-2 gap-5 mb-5">
                <div>
                  <label class="block text-xs font-semibold text-text-secondary mb-1.5 uppercase tracking-wider">Quiz ID</label>
                  <input 
                    v-model="editorForm.quizId"
                    type="text"
                    class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent focus:ring-4 focus:ring-accent/10 transition-all"
                    placeholder="ID bài Quiz"
                  />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-text-secondary mb-1.5 uppercase tracking-wider">Lab ID</label>
                  <input 
                    v-model="editorForm.labId"
                    type="text"
                    class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent focus:ring-4 focus:ring-accent/10 transition-all"
                    placeholder="ID bài Lab"
                  />
                </div>
                <div class="col-span-2">
                  <label class="block text-xs font-semibold text-text-secondary mb-1.5 uppercase tracking-wider">LeetCode Problem ID</label>
                  <input 
                    v-model="editorForm.leetCodeId"
                    type="text"
                    class="w-full bg-bg-secondary/50 border border-border-default/80 rounded-xl px-4 py-3 text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent focus:ring-4 focus:ring-accent/10 transition-all"
                    placeholder="Nhập mã bài toán LeetCode"
                  />
                </div>
              </div>
              
              <div class="flex justify-end">
                <button class="btn btn-primary btn-sm" @click="saveNodePractice" :disabled="isSaving">
                  Lưu Bài tập
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add Node Modal -->
    <div v-if="showAddNodeModal" class="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
      <div class="glass-panel rounded-xl max-w-lg w-full shadow-2xl overflow-hidden">
        <div class="px-6 py-4 border-b border-border-default flex justify-between items-center">
          <h3 class="text-lg font-bold text-text-primary">Thêm Node mới</h3>
          <button class="text-text-secondary hover:text-text-primary" @click="showAddNodeModal = false">
            <BaseIcon name="x" class="w-5 h-5" />
          </button>
        </div>
        
        <form @submit.prevent="handleAddNode" class="p-6">
          <div class="mb-4">
            <label class="block text-sm font-medium text-text-secondary mb-1">Tên Node</label>
            <input 
              v-model="newNodeForm.name" 
              type="text" 
              required 
              class="w-full bg-bg-secondary border border-border-default rounded-lg px-4 py-2 text-text-primary"
            />
          </div>
          
          <div class="mb-4">
            <label class="block text-sm font-medium text-text-secondary mb-1">Mô tả ngắn</label>
            <input 
              v-model="newNodeForm.description" 
              type="text" 
              required 
              class="w-full bg-bg-secondary border border-border-default rounded-lg px-4 py-2 text-text-primary"
            />
          </div>
          
          <div class="flex gap-4 mb-6">
            <div class="flex-1">
              <label class="block text-sm font-medium text-text-secondary mb-1">Độ khó</label>
              <select v-model="newNodeForm.difficulty" class="w-full bg-bg-secondary border border-border-default rounded-lg px-4 py-2 text-text-primary">
                <option value="Easy">Dễ</option>
                <option value="Medium">Trung bình</option>
                <option value="Hard">Khó</option>
              </select>
            </div>
            <div class="w-24">
              <label class="block text-sm font-medium text-text-secondary mb-1">Thứ tự</label>
              <input v-model.number="newNodeForm.sortOrder" type="number" min="0" class="w-full bg-bg-secondary border border-border-default rounded-lg px-4 py-2 text-text-primary" />
            </div>
          </div>
          
          <div class="flex justify-end space-x-3">
            <button type="button" class="btn btn-secondary" @click="showAddNodeModal = false">Hủy</button>
            <button type="submit" class="btn btn-primary" :disabled="isSubmitting">Thêm</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useConfirmDialogStore } from '@/features/ui/store/useConfirmDialogStore';
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { teacherStudioService, type CustomRoadmapDto, type CustomNodeDto } from '@/services/TeacherStudioService';
import { useToastStore } from '@/composables/useToast';
import BaseIcon from '@/shared/components/BaseIcon.vue';

const route = useRoute();
const router = useRouter();
const toastStore = useToastStore();
const confirmDialogStore = useConfirmDialogStore();

const roadmapId = computed(() => route.params.id as string);
const roadmap = ref<CustomRoadmapDto | null>(null);
const selectedNode = ref<CustomNodeDto | null>(null);

const sortedNodes = computed(() => {
  if (!roadmap.value?.nodes) return [];
  return [...roadmap.value.nodes].sort((a, b) => a.sortOrder - b.sortOrder);
});

// Modals & States
const showAddNodeModal = ref(false);
const isSubmitting = ref(false);
const isSaving = ref(false);

const newNodeForm = ref({
  name: '',
  description: '',
  difficulty: 'Medium',
  sortOrder: 0
});

const editorForm = ref({
  contentJson: '',
  videoUrl: '',
  visualizerId: '',
  quizId: '',
  labId: '',
  leetCodeId: ''
});

const vizForm = ref({
  algorithm: '',
  sampleInput: '',
  speed: 1
});

const algorithmOptions = [
  { id: 'bubble-sort', name: 'Bubble Sort (Sắp xếp nổi bọt)' },
  { id: 'selection-sort', name: 'Selection Sort (Sắp xếp chọn)' },
  { id: 'insertion-sort', name: 'Insertion Sort (Sắp xếp chèn)' },
  { id: 'quick-sort', name: 'Quick Sort (Sắp xếp nhanh)' },
  { id: 'merge-sort', name: 'Merge Sort (Sắp xếp trộn)' },
  { id: 'heap-sort', name: 'Heap Sort (Sắp xếp đống)' },
  { id: 'binary-search', name: 'Binary Search (Tìm kiếm nhị phân)' },
  { id: 'linear-search', name: 'Linear Search (Tìm kiếm tuần tự)' },
  { id: 'sliding-window', name: 'Sliding Window (Cửa sổ trượt)' },
  { id: 'stack', name: 'Stack (Ngăn xếp)' },
  { id: 'queue', name: 'Queue (Hàng đợi)' },
  { id: 'bst', name: 'Binary Search Tree (Cây BST)' },
  { id: 'bfs', name: 'BFS (Duyệt theo chiều rộng)' },
  { id: 'dfs', name: 'DFS (Duyệt theo chiều sâu)' },
  { id: 'dijkstra', name: 'Dijkstra (Đường đi ngắn nhất)' },
  { id: 'bellman-ford', name: 'Bellman-Ford' },
  { id: 'kruskal', name: 'Kruskal (Cây khung nhỏ nhất)' },
  { id: 'prim', name: 'Prim (Cây khung nhỏ nhất)' },
  { id: 'encapsulation', name: 'OOP · Encapsulation (Đóng gói)' },
  { id: 'inheritance', name: 'OOP · Inheritance (Kế thừa)' },
  { id: 'polymorphism', name: 'OOP · Polymorphism (Đa hình)' },
  { id: 'abstraction', name: 'OOP · Abstraction (Trừu tượng)' },
  { id: 'solid-srp', name: 'SOLID · Single Responsibility' },
  { id: 'solid-ocp', name: 'SOLID · Open/Closed' },
  { id: 'solid-lsp', name: 'SOLID · Liskov Substitution' },
  { id: 'solid-isp', name: 'SOLID · Interface Segregation' },
  { id: 'solid-dip', name: 'SOLID · Dependency Inversion' },
  { id: 'strategy', name: 'Design Pattern · Strategy' },
];

function parseVisualizerConfig(raw?: string): { algorithm: string; sampleInput: string; speed: number } {
  if (!raw) return { algorithm: '', sampleInput: '', speed: 1 };
  try {
    const parsed = JSON.parse(raw);
    return {
      algorithm: typeof parsed.algorithm === 'string' ? parsed.algorithm : '',
      sampleInput: typeof parsed.sampleInput === 'string' ? parsed.sampleInput : '',
      speed: typeof parsed.speed === 'number' ? parsed.speed : 1,
    };
  } catch {
    return { algorithm: '', sampleInput: '', speed: 1 };
  }
}

function serializeVisualizerConfig(): string {
  return JSON.stringify({
    algorithm: vizForm.value.algorithm,
    sampleInput: vizForm.value.sampleInput,
    speed: vizForm.value.speed,
  });
}

const canPublish = computed(() => {
  if (!roadmap.value) return false;
  if (roadmap.value.status !== 'Draft' && roadmap.value.status !== 'Rejected') return false;
  if (!roadmap.value.nodes || roadmap.value.nodes.length === 0) return false;
  return roadmap.value.nodes.every(n => n.isComplete);
});

const loadRoadmap = async () => {
  try {
    const res = await teacherStudioService.getRoadmap(roadmapId.value);
    roadmap.value = res;
    
    if (roadmap.value && sortedNodes.value.length > 0) {
      newNodeForm.value.sortOrder = sortedNodes.value.length;
    }
  } catch (err) {
    toastStore.error('Lỗi khi tải thông tin Lộ trình');
  }
};

const selectNode = (node: CustomNodeDto) => {
  selectedNode.value = node;
  editorForm.value = {
    contentJson: node.contentJson || '',
    videoUrl: node.videoUrl || '',
    visualizerId: node.visualizerId || '',
    quizId: node.quizId || '',
    labId: node.labId || '',
    leetCodeId: node.leetCodeId || ''
  };
  vizForm.value = parseVisualizerConfig(node.visualizerConfig);
};

const handleAddNode = async () => {
  isSubmitting.value = true;
  try {
    const res = await teacherStudioService.addNode(roadmapId.value, newNodeForm.value);
    if (roadmap.value) {
      roadmap.value.nodes.push(res);
    }
    showAddNodeModal.value = false;
    toastStore.success('Thêm Node thành công');
    selectNode(res);
    newNodeForm.value.name = '';
    newNodeForm.value.description = '';
    newNodeForm.value.sortOrder += 1;
  } catch (err: any) {
    toastStore.error(err.response?.data?.message || 'Lỗi khi thêm Node');
  } finally {
    isSubmitting.value = false;
  }
};

const saveNodeContent = async () => {
  if (!selectedNode.value) return;
  isSaving.value = true;
  try {
    const payload: { contentJson: string; videoUrl?: string; visualizerId?: string; visualizerConfig?: string } = {
      contentJson: editorForm.value.contentJson,
      videoUrl: editorForm.value.videoUrl || undefined,
      visualizerConfig: vizForm.value.algorithm ? serializeVisualizerConfig() : undefined
    };
    // visualizerId rỗng → KHÔNG đưa vào payload (backend Guid? không nhận chuỗi "")
    if (editorForm.value.visualizerId) {
      payload.visualizerId = editorForm.value.visualizerId;
    }
    
    const res = await teacherStudioService.updateNodeContent(roadmapId.value, selectedNode.value.id, payload);
    updateNodeInList(res);
    toastStore.success('Lưu nội dung thành công');
  } catch (err: any) {
    toastStore.error('Lỗi khi lưu nội dung');
  } finally {
    isSaving.value = false;
  }
};

const saveNodePractice = async () => {
  if (!selectedNode.value) return;
  isSaving.value = true;
  try {
    const payload = {
      quizId: editorForm.value.quizId,
      labId: editorForm.value.labId,
      leetCodeId: editorForm.value.leetCodeId
    };
    
    // Convert empty strings to null or undefined
    const cleanPayload: any = {};
    if (payload.quizId) cleanPayload.quizId = payload.quizId;
    if (payload.labId) cleanPayload.labId = payload.labId;
    if (payload.leetCodeId) cleanPayload.leetCodeId = payload.leetCodeId;
    
    const res = await teacherStudioService.updateNodePractice(roadmapId.value, selectedNode.value.id, cleanPayload);
    updateNodeInList(res);
    toastStore.success('Lưu bài tập thành công');
  } catch (err: any) {
    toastStore.error(err.response?.data?.message || 'Lỗi khi lưu bài tập');
  } finally {
    isSaving.value = false;
  }
};

const deleteSelectedNode = async () => {
  if (!selectedNode.value) return;
  if (!(await confirmDialogStore.confirmDialog({ title: 'Xác nhận', message: 'Bạn có chắc muốn xóa Node này?', variant: 'danger', confirmText: 'Xác nhận', cancelText: 'Hủy' }))) return;
  
  try {
    await teacherStudioService.deleteNode(roadmapId.value, selectedNode.value.id);
    if (roadmap.value) {
      roadmap.value.nodes = roadmap.value.nodes.filter(n => n.id !== selectedNode.value!.id);
    }
    selectedNode.value = null;
    toastStore.success('Xóa Node thành công');
  } catch (err) {
    toastStore.error('Lỗi khi xóa Node');
  }
};

const updateNodeInList = (updatedNode: CustomNodeDto) => {
  if (!roadmap.value) return;
  const idx = roadmap.value.nodes.findIndex(n => n.id === updatedNode.id);
  if (idx !== -1) {
    roadmap.value.nodes[idx] = updatedNode;
  }
  if (selectedNode.value?.id === updatedNode.id) {
    selectedNode.value = updatedNode;
  }
};

const publishRoadmap = async () => {
  if (!roadmap.value) return;
  if (!(await confirmDialogStore.confirmDialog({ title: 'Xác nhận', message: `Bạn có chắc muốn xuất bản lộ trình này dưới dạng ${roadmap.value.visibility}?`, variant: 'danger', confirmText: 'Xác nhận', cancelText: 'Hủy' }))) return;
  
  try {
    const res = await teacherStudioService.publishRoadmap(roadmapId.value, roadmap.value.visibility);
    roadmap.value = res;
    toastStore.success('Lộ trình đã được gửi phê duyệt (hoặc xuất bản)');
  } catch (err: any) {
    const msg = err.response?.data?.message || 'Lỗi khi xuất bản';
    toastStore.error(msg);
  }
};

onMounted(() => {
  loadRoadmap();
});
</script>
