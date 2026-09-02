<script setup lang="ts">
import { computed, onMounted, ref, reactive } from 'vue';
import {
  ShoppingBag,
  Plus,
  Edit2,
  Trash2,
  Gem,
  Coins,
  ArrowUpRight,
  ArrowDownLeft,
  Sparkles,
  Search,
  Filter,
  RefreshCw,
  Image,
  Frame,
  Heart,
  CheckCircle2,
  Upload,
} from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminShopItemDto, AdminGemTransactionDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import StudioShell from '@/components/studio/StudioShell.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { compressImage, setCustomShopAsset, avatarImageUrl } from '@/utils/equipment';

const ui = useUiStore();

const activeTab = ref<'items' | 'transactions'>('items');
const loading = ref(true);
const items = ref<AdminShopItemDto[]>([]);
const transactions = ref<AdminGemTransactionDto[]>([]);

// Filter items
const searchQuery = ref('');
const filterType = ref<string>('all');

// Modals
const createModalOpen = ref(false);
const editModalOpen = ref(false);
const saving = ref(false);
const editingItem = ref<AdminShopItemDto | null>(null);

const form = reactive({
  itemKey: '',
  name: '',
  priceGems: 100,
  type: 1, // 1=Avatar, 2=Frame, 0=Consumable
  maxStack: 1,
  durationHours: null as number | null,
  imageUrl: '',
});

async function loadData(): Promise<void> {
  loading.value = true;
  try {
    const [shopItems, gemTxs] = await Promise.all([
      adminApi.fetchAdminShopItems(),
      adminApi.fetchAdminGemTransactions(100),
    ]);
    items.value = shopItems || [];
    transactions.value = gemTxs || [];
  } catch (err) {
    ui.showToast('Không thể tải dữ liệu Cửa hàng & Gamification.', 'error');
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  void loadData();
});

const filteredItems = computed(() => {
  return items.value.filter((item) => {
    const matchesSearch =
      !searchQuery.value ||
      item.name.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      item.itemKey.toLowerCase().includes(searchQuery.value.toLowerCase());

    const matchesType =
      filterType.value === 'all' ||
      (filterType.value === 'avatar' && item.type === 1) ||
      (filterType.value === 'frame' && item.type === 2) ||
      (filterType.value === 'consumable' && item.type === 0);

    return matchesSearch && matchesType;
  });
});

function openCreateModal(): void {
  form.itemKey = '';
  form.name = '';
  form.priceGems = 100;
  form.type = 1;
  form.maxStack = 1;
  form.durationHours = null;
  form.imageUrl = '';
  createModalOpen.value = true;
}

function openEditModal(item: AdminShopItemDto): void {
  editingItem.value = item;
  form.itemKey = item.itemKey;
  form.name = item.name;
  form.priceGems = item.priceGems;
  form.type = item.type;
  form.maxStack = item.maxStack;
  form.durationHours = item.durationHours;
  form.imageUrl = item.imageUrl || avatarImageUrl(item.itemKey) || '';
  editModalOpen.value = true;
}

async function handleFileUpload(e: Event): Promise<void> {
  const target = e.target as HTMLInputElement;
  const file = target.files?.[0];
  if (!file) return;

  try {
    // 1. Nén ảnh tự động về kích thước tối đa 256x256 (~20-35KB), preview tức thì
    const compressedBase64 = await compressImage(file, 256, 0.85);
    form.imageUrl = compressedBase64;

    // 2. Tải lên server để nhận link tĩnh /uploads/shop/...
    try {
      const res = await adminApi.uploadShopAsset({ image: compressedBase64, name: file.name });
      if (res?.url) {
        form.imageUrl = res.url;
      }
    } catch {
      // Backend offline hoặc chưa có endpoint -> dùng base64 đã nén siêu nhẹ
    }

    ui.showToast('Đã tải và xử lý ảnh thành công!', 'success');
  } catch (err: any) {
    ui.showToast(err.message || 'Không thể xử lý ảnh.', 'error');
  }
}

async function handleCreateItem(): Promise<void> {
  if (!form.itemKey.trim() || !form.name.trim()) {
    ui.showToast('Vui lòng điền đầy đủ Mã định danh và Tên vật phẩm.', 'warning');
    return;
  }
  saving.value = true;
  try {
    const normKey = form.itemKey.trim().toLowerCase();
    await adminApi.createAdminShopItem({
      itemKey: normKey,
      name: form.name.trim(),
      priceGems: Number(form.priceGems),
      type: Number(form.type),
      maxStack: Number(form.type === 1 || form.type === 2 ? 1 : form.maxStack),
      durationHours: form.durationHours,
    });

    if (form.imageUrl && form.imageUrl.trim()) {
      const imgUrl = form.imageUrl.trim();
      setCustomShopAsset(normKey, imgUrl);
      try {
        await adminApi.saveCustomShopAsset(normKey, imgUrl);
      } catch {}
    }

    ui.showToast('Đã thêm vật phẩm mới thành công!', 'success');
    createModalOpen.value = false;
    await loadData();
  } catch (err: any) {
    ui.showToast(err.response?.data?.message || err.message || 'Không thể tạo vật phẩm.', 'error');
  } finally {
    saving.value = false;
  }
}

async function handleUpdateItem(): Promise<void> {
  if (!editingItem.value || !form.name.trim()) return;
  saving.value = true;
  try {
    const normKey = editingItem.value.itemKey.trim().toLowerCase();
    await adminApi.updateAdminShopItem(editingItem.value.id, {
      name: form.name.trim(),
      priceGems: Number(form.priceGems),
      type: Number(form.type),
      maxStack: Number(form.type === 1 || form.type === 2 ? 1 : form.maxStack),
      durationHours: form.durationHours,
    });

    if (form.imageUrl && form.imageUrl.trim()) {
      const imgUrl = form.imageUrl.trim();
      setCustomShopAsset(normKey, imgUrl);
      try {
        await adminApi.saveCustomShopAsset(normKey, imgUrl);
      } catch {}
    }

    ui.showToast('Đã cập nhật vật phẩm thành công!', 'success');
    editModalOpen.value = false;
    await loadData();
  } catch (err: any) {
    ui.showToast(err.response?.data?.message || err.message || 'Không thể cập nhật vật phẩm.', 'error');
  } finally {
    saving.value = false;
  }
}

async function handleDeleteItem(item: AdminShopItemDto): Promise<void> {
  if (!confirm(`Bạn có chắc chắn muốn xóa vật phẩm "${item.name}" khỏi cửa hàng?`)) return;
  try {
    await adminApi.deleteAdminShopItem(item.id);
    ui.showToast(`Đã xóa "${item.name}" thành công!`, 'success');
    await loadData();
  } catch (err: any) {
    ui.showToast(err.response?.data?.message || err.message || 'Không thể xóa vật phẩm.', 'error');
  }
}

function typeBadge(type: number): { label: string; variant: 'primary' | 'success' | 'warning' | 'secondary' | 'muted' } {
  switch (type) {
    case 1:
      return { label: 'Avatar', variant: 'primary' };
    case 2:
      return { label: 'Khung viền', variant: 'warning' };
    case 0:
      return { label: 'Tiêu hao (Hồi Tim)', variant: 'success' };
    default:
      return { label: 'Vật phẩm', variant: 'muted' };
  }
}

function formatDate(dateStr: string): string {
  if (!dateStr) return '';
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
</script>

<template>
  <StudioShell active-tab="shop">
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-6 rounded-3xl bg-vdsa-surface border border-vdsa-border">
        <div>
          <h1 class="text-xl md:text-2xl font-black text-white flex items-center gap-2.5">
            <span class="p-2 rounded-2xl bg-purple-500/20 text-purple-400 border border-purple-500/30">
              <ShoppingBag :size="22" />
            </span>
            Quản lý Cửa hàng & Gamification
          </h1>
          <p class="text-xs text-vdsa-muted mt-1">
            Quản lý kho Avatar, Khung viền, giá Gems và kiểm soát dòng tiền ảo toàn sàn
          </p>
        </div>

        <div class="flex items-center gap-2.5">
          <Button variant="ghost" size="sm" @click="loadData" :loading="loading">
            <RefreshCw :size="14" /> Làm mới
          </Button>
          <Button variant="primary" size="sm" @click="openCreateModal">
            <Plus :size="14" /> Thêm vật phẩm
          </Button>
        </div>
      </div>

      <!-- Stats Overview Cards -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 md:gap-4">
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-purple-500/15 text-purple-400 flex items-center justify-center font-bold">
            <ShoppingBag :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Tổng vật phẩm</span>
            <p class="text-lg font-black text-white">{{ items.length }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-blue-500/15 text-blue-400 flex items-center justify-center font-bold">
            <Image :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Avatars</span>
            <p class="text-lg font-black text-white">{{ items.filter(i => i.type === 1).length }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-amber-500/15 text-amber-400 flex items-center justify-center font-bold">
            <Frame :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Khung viền</span>
            <p class="text-lg font-black text-white">{{ items.filter(i => i.type === 2).length }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-emerald-500/15 text-emerald-400 flex items-center justify-center font-bold">
            <Gem :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Lượt mua đã cấp</span>
            <p class="text-lg font-black text-white">{{ items.reduce((s, i) => s + i.ownersCount, 0) }}</p>
          </div>
        </div>
      </div>

      <!-- Main Tabs -->
      <div class="flex items-center gap-2 border-b border-vdsa-border pb-2">
        <button
          type="button"
          class="px-4 py-2 rounded-xl text-xs font-bold transition-all cursor-pointer"
          :class="activeTab === 'items' ? 'bg-purple-600 text-white' : 'text-slate-400 hover:text-white hover:bg-white/5'"
          @click="activeTab = 'items'"
        >
          Danh mục Vật phẩm Cửa hàng ({{ items.length }})
        </button>
        <button
          type="button"
          class="px-4 py-2 rounded-xl text-xs font-bold transition-all cursor-pointer"
          :class="activeTab === 'transactions' ? 'bg-purple-600 text-white' : 'text-slate-400 hover:text-white hover:bg-white/5'"
          @click="activeTab = 'transactions'"
        >
          Nhật ký Dòng tiền Gems ({{ transactions.length }})
        </button>
      </div>

      <!-- TAB 1: Danh mục Vật phẩm -->
      <div v-if="activeTab === 'items'" class="space-y-4">
        <!-- Search & Filter Bar -->
        <div class="flex flex-col sm:flex-row items-center justify-between gap-3">
          <div class="relative flex-1 w-full sm:max-w-xs">
            <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo tên hoặc mã item..."
              class="w-full pl-9 pr-3 py-2 text-xs bg-vdsa-surface border border-vdsa-border rounded-xl text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            />
          </div>

          <div class="flex items-center gap-2 w-full sm:w-auto">
            <select
              v-model="filterType"
              class="px-3 py-2 text-xs bg-vdsa-surface border border-vdsa-border rounded-xl text-white focus:outline-none focus:border-purple-500"
            >
              <option value="all">Tất cả phân loại</option>
              <option value="avatar">Avatar (Type 1)</option>
              <option value="frame">Khung viền (Type 2)</option>
              <option value="consumable">Tiêu hao (Type 0)</option>
            </select>
          </div>
        </div>

        <!-- Table -->
        <div class="overflow-x-auto rounded-2xl border border-vdsa-border bg-vdsa-surface">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-vdsa-bg-secondary border-b border-vdsa-border text-[11px] uppercase font-bold text-vdsa-muted">
                <th class="p-3">ID / Mã Key</th>
                <th class="p-3">Tên vật phẩm</th>
                <th class="p-3">Phân loại</th>
                <th class="p-3">Giá Gems</th>
                <th class="p-3">Max Stack</th>
                <th class="p-3">Người sở hữu</th>
                <th class="p-3 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-vdsa-border text-xs">
              <tr v-for="item in filteredItems" :key="item.id" class="hover:bg-white/5 transition-colors">
                <td class="p-3 font-mono">
                  <span class="text-purple-400 font-bold mr-1">#{{ item.id }}</span>
                  <span class="text-slate-300 font-medium">{{ item.itemKey }}</span>
                </td>
                <td class="p-3 font-bold text-white flex items-center gap-2.5">
                  <div class="w-8 h-8 rounded-lg bg-black/40 border border-white/10 flex items-center justify-center overflow-hidden shrink-0 shadow-inner">
                    <img
                      v-if="item.imageUrl || avatarImageUrl(item.itemKey)"
                      :src="item.imageUrl || avatarImageUrl(item.itemKey)"
                      :alt="item.name"
                      class="w-full h-full object-cover"
                    />
                    <component :is="item.type === 1 ? Image : item.type === 2 ? Frame : Heart" v-else :size="15" class="text-purple-400" />
                  </div>
                  <span>{{ item.name }}</span>
                </td>
                <td class="p-3">
                  <Badge :variant="typeBadge(item.type).variant">{{ typeBadge(item.type).label }}</Badge>
                </td>
                <td class="p-3 font-mono font-bold text-amber-400">
                  <span class="flex items-center gap-1"><Gem :size="13" /> {{ item.priceGems }}</span>
                </td>
                <td class="p-3 font-mono text-slate-400">
                  {{ item.maxStack }}
                </td>
                <td class="p-3 font-mono text-emerald-400 font-semibold">
                  {{ item.ownersCount }} users
                </td>
                <td class="p-3 text-right space-x-2">
                  <button
                    type="button"
                    class="px-2.5 py-1 rounded bg-vdsa-bg-secondary hover:bg-slate-700 text-white text-xs font-medium cursor-pointer inline-flex items-center gap-1 transition-colors"
                    @click="openEditModal(item)"
                  >
                    <Edit2 :size="12" /> Sửa
                  </button>
                  <button
                    type="button"
                    class="px-2.5 py-1 rounded bg-rose-500/15 hover:bg-rose-500/30 text-rose-400 text-xs font-medium cursor-pointer inline-flex items-center gap-1 transition-colors"
                    @click="handleDeleteItem(item)"
                  >
                    <Trash2 :size="12" /> Xóa
                  </button>
                </td>
              </tr>
              <tr v-if="filteredItems.length === 0">
                <td colspan="7" class="p-6 text-center text-xs text-slate-500">
                  Không tìm thấy vật phẩm nào phù hợp với bộ lọc.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- TAB 2: Nhật ký Dòng tiền Gems -->
      <div v-else-if="activeTab === 'transactions'" class="space-y-4">
        <div class="overflow-x-auto rounded-2xl border border-vdsa-border bg-vdsa-surface">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="bg-vdsa-bg-secondary border-b border-vdsa-border text-[11px] uppercase font-bold text-vdsa-muted">
                <th class="p-3">Mã TX</th>
                <th class="p-3">Người dùng</th>
                <th class="p-3">Biến động</th>
                <th class="p-3">Nguồn / Hoạt động</th>
                <th class="p-3">Thời gian</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-vdsa-border text-xs">
              <tr v-for="tx in transactions" :key="tx.id" class="hover:bg-white/5 transition-colors">
                <td class="p-3 font-mono text-purple-400 font-bold">#{{ tx.id }}</td>
                <td class="p-3">
                  <p class="font-bold text-white">{{ tx.userDisplayName }}</p>
                  <p class="text-[11px] text-slate-400">{{ tx.userEmail }}</p>
                </td>
                <td class="p-3 font-mono font-bold">
                  <span
                    class="inline-flex items-center gap-1 px-2 py-0.5 rounded"
                    :class="tx.amount >= 0 ? 'bg-emerald-500/15 text-emerald-400' : 'bg-rose-500/15 text-rose-400'"
                  >
                    <component :is="tx.amount >= 0 ? ArrowUpRight : ArrowDownLeft" :size="13" />
                    {{ tx.amount >= 0 ? `+${tx.amount}` : tx.amount }} Gems
                  </span>
                </td>
                <td class="p-3 text-slate-300 font-medium">
                  {{ tx.refType || 'Hệ thống' }} <span v-if="tx.refId" class="text-slate-500">({{ tx.refId }})</span>
                </td>
                <td class="p-3 text-slate-400 text-[11px]">
                  {{ formatDate(tx.createdAt) }}
                </td>
              </tr>
              <tr v-if="transactions.length === 0">
                <td colspan="5" class="p-6 text-center text-xs text-slate-500">
                  Chưa có giao dịch Gems nào được ghi nhận.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal Tạo vật phẩm mới -->
    <Modal :open="createModalOpen" title="Thêm Vật phẩm Cửa hàng Mới" :is-dirty="Boolean(form.itemKey || form.name)" @close="createModalOpen = false">
      <form class="space-y-4 pt-1" novalidate @submit.prevent="handleCreateItem">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Mã định danh (ItemKey) *</label>
            <input
              v-model="form.itemKey"
              type="text"
              placeholder="ví dụ: avatar-dragon, frame-galaxy"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
              required
            />
          </div>
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Tên vật phẩm *</label>
            <input
              v-model="form.name"
              type="text"
              placeholder="ví dụ: Rồng Thần Vũ Trụ"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
              required
            />
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Phân loại *</label>
            <select
              v-model="form.type"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500"
            >
              <option :value="1">Avatar (Type 1)</option>
              <option :value="2">Khung viền (Type 2)</option>
              <option :value="0">Tiêu hao / Tim (Type 0)</option>
            </select>
          </div>
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Giá Gems *</label>
            <input
              v-model="form.priceGems"
              type="number"
              min="0"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500"
            />
          </div>
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Giới hạn Stack</label>
            <input
              v-model="form.maxStack"
              type="number"
              min="1"
              :disabled="form.type === 1 || form.type === 2"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500 disabled:opacity-50"
            />
          </div>
        </div>

        <!-- Image URL / Upload from device with Live Preview for Avatar (1) & Frame (2) -->
        <div v-if="form.type === 1 || form.type === 2" class="p-3.5 bg-[#131120] border border-[#2e2c44] rounded-xl space-y-2.5">
          <div class="flex items-center justify-between">
            <label class="block text-[11px] font-bold text-slate-300 uppercase">
              {{ form.type === 1 ? 'Hình ảnh Avatar *' : 'Hình ảnh Khung viền *' }}
            </label>
            <span class="text-[10px] text-slate-400 italic">Chọn tải từ máy hoặc dán link URL</span>
          </div>

          <div class="flex items-center gap-2.5 flex-wrap sm:flex-nowrap">
            <!-- Upload from Device -->
            <label class="flex items-center gap-1.5 px-3 py-2 text-xs font-bold text-purple-300 bg-purple-500/10 hover:bg-purple-500/20 border border-purple-500/30 rounded-lg transition-colors cursor-pointer shrink-0 shadow-sm">
              <Upload :size="14" />
              <span>Tải ảnh từ máy</span>
              <input
                type="file"
                accept="image/*"
                class="hidden"
                @change="handleFileUpload"
              />
            </label>

            <!-- Or Enter URL -->
            <input
              v-model="form.imageUrl"
              type="text"
              :placeholder="form.type === 1 ? 'hoặc dán link: /assets/avatars/... hoặc https://...' : 'hoặc dán link: /assets/frames/... hoặc https://...'"
              class="flex-1 min-w-[200px] px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            />

            <!-- Live Preview -->
            <div class="w-10 h-10 rounded-xl bg-black/40 border border-white/10 flex items-center justify-center overflow-hidden shrink-0 shadow-inner">
              <img v-if="form.imageUrl" :src="form.imageUrl" alt="Preview" class="w-full h-full object-contain" />
              <component :is="form.type === 1 ? Image : Frame" v-else class="w-5 h-5 text-slate-600" />
            </div>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 pt-3 border-t border-[#2e2c44]">
          <Button variant="ghost" @click="createModalOpen = false">Hủy</Button>
          <Button type="submit" :loading="saving">Lưu vật phẩm</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal Chỉnh sửa vật phẩm -->
    <Modal :open="editModalOpen" title="Chỉnh sửa Vật phẩm Cửa hàng" @close="editModalOpen = false">
      <form class="space-y-4 pt-1" novalidate @submit.prevent="handleUpdateItem">
        <div>
          <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Tên vật phẩm *</label>
          <input
            v-model="form.name"
            type="text"
            class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500"
            required
          />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Giá Gems *</label>
            <input
              v-model="form.priceGems"
              type="number"
              min="0"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500"
            />
          </div>
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Phân loại</label>
            <select
              v-model="form.type"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500"
            >
              <option :value="1">Avatar (Type 1)</option>
              <option :value="2">Khung viền (Type 2)</option>
              <option :value="0">Tiêu hao / Tim (Type 0)</option>
            </select>
          </div>
        </div>

        <!-- Image URL / Upload from device with Live Preview for Avatar (1) & Frame (2) -->
        <div v-if="form.type === 1 || form.type === 2" class="p-3.5 bg-[#131120] border border-[#2e2c44] rounded-xl space-y-2.5">
          <div class="flex items-center justify-between">
            <label class="block text-[11px] font-bold text-slate-300 uppercase">
              {{ form.type === 1 ? 'Hình ảnh Avatar *' : 'Hình ảnh Khung viền *' }}
            </label>
            <span class="text-[10px] text-slate-400 italic">Chọn tải từ máy hoặc dán link URL</span>
          </div>

          <div class="flex items-center gap-2.5 flex-wrap sm:flex-nowrap">
            <!-- Upload from Device -->
            <label class="flex items-center gap-1.5 px-3 py-2 text-xs font-bold text-purple-300 bg-purple-500/10 hover:bg-purple-500/20 border border-purple-500/30 rounded-lg transition-colors cursor-pointer shrink-0 shadow-sm">
              <Upload :size="14" />
              <span>Tải ảnh từ máy</span>
              <input
                type="file"
                accept="image/*"
                class="hidden"
                @change="handleFileUpload"
              />
            </label>

            <!-- Or Enter URL -->
            <input
              v-model="form.imageUrl"
              type="text"
              :placeholder="form.type === 1 ? 'hoặc dán link: /assets/avatars/... hoặc https://...' : 'hoặc dán link: /assets/frames/... hoặc https://...'"
              class="flex-1 min-w-[200px] px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            />

            <!-- Live Preview -->
            <div class="w-10 h-10 rounded-xl bg-black/40 border border-white/10 flex items-center justify-center overflow-hidden shrink-0 shadow-inner">
              <img v-if="form.imageUrl" :src="form.imageUrl" alt="Preview" class="w-full h-full object-contain" />
              <component :is="form.type === 1 ? Image : Frame" v-else class="w-5 h-5 text-slate-600" />
            </div>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 pt-3 border-t border-[#2e2c44]">
          <Button variant="ghost" @click="editModalOpen = false">Hủy</Button>
          <Button type="submit" :loading="saving">Cập nhật</Button>
        </div>
      </form>
    </Modal>
  </StudioShell>
</template>
