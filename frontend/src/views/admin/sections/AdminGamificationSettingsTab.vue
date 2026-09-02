<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { Trophy, Heart, Sparkles, Clock, ShieldCheck, RefreshCw, Save } from 'lucide-vue-next';
import { client } from '@/api/client';
import { useUiStore } from '@/stores/ui';

interface GamificationSettings {
  theoryBaseXp: number;
  quizBaseXp: number;
  codelabBaseXp: number;
  streakBonusXp: number;
  heartsMaxFree: number;
  heartsMaxPremium: number;
  heartRegenMinutes: number;
  sessionHours: number;
}

const ui = useUiStore();
const loading = ref(false);
const saving = ref(false);

const form = ref<GamificationSettings>({
  theoryBaseXp: 50,
  quizBaseXp: 50,
  codelabBaseXp: 100,
  streakBonusXp: 20,
  heartsMaxFree: 10,
  heartsMaxPremium: 30,
  heartRegenMinutes: 30,
  sessionHours: 36,
});

async function loadSettings() {
  loading.value = true;
  try {
    const res = await client.get<GamificationSettings>('/admin/gamification/settings');
    if (res.data) {
      form.value = { ...res.data };
    }
  } catch (err: any) {
    console.error('Failed to load gamification settings:', err);
    ui.showToast('Không thể tải cấu hình Gamification từ máy chủ.', 'error');
  } finally {
    loading.value = false;
  }
}

async function handleSave() {
  saving.value = true;
  try {
    const res = await client.put<GamificationSettings>('/admin/gamification/settings', form.value);
    if (res.data) {
      form.value = { ...res.data };
    }
    ui.showToast('Đã lưu cấu hình Gamification thành công! Toàn hệ thống đã cập nhật.', 'success');
  } catch (err: any) {
    console.error('Failed to save settings:', err);
    ui.showToast(err?.response?.data?.message || 'Có lỗi khi lưu cấu hình.', 'error');
  } finally {
    saving.value = false;
  }
}

function handleResetDefaults() {
  form.value = {
    theoryBaseXp: 50,
    quizBaseXp: 50,
    codelabBaseXp: 100,
    streakBonusXp: 20,
    heartsMaxFree: 10,
    heartsMaxPremium: 30,
    heartRegenMinutes: 30,
    sessionHours: 36,
  };
  ui.showToast('Đã khôi phục thông số mặc định (Hãy nhấn Lưu để áp dụng).', 'info');
}

onMounted(() => {
  loadSettings();
});
</script>

<template>
  <div class="space-y-6 max-w-5xl mx-auto p-4 sm:p-6">
    <!-- Header banner -->
    <div class="p-6 rounded-2xl bg-gradient-to-r from-purple-900/40 via-indigo-900/30 to-slate-900/50 border border-purple-500/30 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 shadow-xl">
      <div>
        <div class="flex items-center gap-2.5 text-purple-300 text-xs font-bold uppercase tracking-wider mb-1.5">
          <Trophy class="w-4 h-4 text-purple-400" />
          <span>Hệ Thống Quản Trị Gamification</span>
        </div>
        <h1 class="text-xl sm:text-2xl font-black text-white tracking-tight">Cấu hình Điểm Thưởng & Kinh Tế Trái Tim</h1>
        <p class="text-xs sm:text-sm text-slate-300 mt-1 max-w-2xl">
          Quản lý toàn diện mức XP thưởng cho các loại bài học và cơ chế phục hồi tim. Các thay đổi sẽ áp dụng ngay tức thì cho toàn bộ học viên.
        </p>
      </div>

      <div class="flex items-center gap-2">
        <button
          type="button"
          @click="loadSettings"
          :disabled="loading || saving"
          class="p-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 hover:text-white transition-colors border border-slate-700 cursor-pointer"
          title="Tải lại"
        >
          <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': loading }" />
        </button>
        <button
          type="button"
          @click="handleSave"
          :disabled="loading || saving"
          class="px-4 py-2.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold flex items-center gap-2 shadow-lg shadow-purple-600/30 transition-all cursor-pointer disabled:opacity-50"
        >
          <Save class="w-4 h-4" />
          <span>{{ saving ? 'Đang lưu...' : 'Lưu Thay Đổi' }}</span>
        </button>
      </div>
    </div>

    <!-- Main Config Form Grid -->
    <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
      <!-- Card 1: XP Rewards Economy -->
      <div class="p-5 sm:p-6 rounded-2xl bg-[#0f111a] border border-purple-500/20 shadow-lg space-y-5">
        <div class="flex items-center justify-between pb-3 border-b border-purple-500/20">
          <div class="flex items-center gap-2.5">
            <span class="p-2 rounded-xl bg-amber-500/10 text-amber-400 border border-amber-500/20">
              <Sparkles class="w-5 h-5" />
            </span>
            <div>
              <h3 class="text-sm font-bold text-white tracking-tight">Điểm Thưởng Kinh Nghiệm (XP)</h3>
              <p class="text-[11px] text-slate-400">Mức XP trao khi hoàn thành từng hoạt động</p>
            </div>
          </div>
          <span class="px-2 py-0.5 rounded-md bg-amber-500/10 text-amber-300 text-[10px] font-mono font-bold">
            EXP REWARDS
          </span>
        </div>

        <div class="space-y-4">
          <!-- Theory XP -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Hoàn thành bài Lý thuyết (Theory)</label>
              <span class="text-xs font-bold text-amber-400 font-mono">+{{ form.theoryBaseXp }} XP</span>
            </div>
            <input
              v-model.number="form.theoryBaseXp"
              type="number"
              min="0"
              max="1000"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Cộng khi học sinh đọc bài lý thuyết và xem trực quan lần đầu.</p>
          </div>

          <!-- Quiz XP -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Vượt qua bài Trắc nghiệm (Quiz)</label>
              <span class="text-xs font-bold text-amber-400 font-mono">+{{ form.quizBaseXp }} XP</span>
            </div>
            <input
              v-model.number="form.quizBaseXp"
              type="number"
              min="0"
              max="1000"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Cộng khi học sinh nộp bài trắc nghiệm củng cố kiến thức.</p>
          </div>

          <!-- Codelab XP -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Hoàn thành Thực hành (Code Lab / Lab)</label>
              <span class="text-xs font-bold text-amber-400 font-mono">+{{ form.codelabBaseXp }} XP</span>
            </div>
            <input
              v-model.number="form.codelabBaseXp"
              type="number"
              min="0"
              max="1000"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Cộng khi vượt qua các test cases của bài tập lập trình.</p>
          </div>

          <!-- Streak XP -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Thưởng duy trì chuỗi học (Daily Streak)</label>
              <span class="text-xs font-bold text-amber-400 font-mono">+{{ form.streakBonusXp }} XP</span>
            </div>
            <input
              v-model.number="form.streakBonusXp"
              type="number"
              min="0"
              max="500"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Điểm thưởng chuyên cần khi học sinh đăng nhập học mỗi ngày.</p>
          </div>
        </div>
      </div>

      <!-- Card 2: Heart Economy & Sessions -->
      <div class="p-5 sm:p-6 rounded-2xl bg-[#0f111a] border border-purple-500/20 shadow-lg space-y-5">
        <div class="flex items-center justify-between pb-3 border-b border-purple-500/20">
          <div class="flex items-center gap-2.5">
            <span class="p-2 rounded-xl bg-red-500/10 text-red-400 border border-red-500/20">
              <Heart class="w-5 h-5" />
            </span>
            <div>
              <h3 class="text-sm font-bold text-white tracking-tight">Kinh Tế Trái Tim & Phiên Học</h3>
              <p class="text-[11px] text-slate-400">Quy định số lượng tim, hồi phục và session</p>
            </div>
          </div>
          <span class="px-2 py-0.5 rounded-md bg-red-500/10 text-red-300 text-[10px] font-mono font-bold">
            HEART SYSTEM
          </span>
        </div>

        <div class="space-y-4">
          <!-- Free Max Hearts -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Số tim tối đa (Tài khoản Miễn phí)</label>
              <span class="text-xs font-bold text-red-400 font-mono">{{ form.heartsMaxFree }} 🤍</span>
            </div>
            <input
              v-model.number="form.heartsMaxFree"
              type="number"
              min="1"
              max="50"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Giới hạn tim tối đa cho học viên thông thường.</p>
          </div>

          <!-- Premium Max Hearts -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Số tim tối đa (Tài khoản Premium)</label>
              <span class="text-xs font-bold text-yellow-400 font-mono">{{ form.heartsMaxPremium }} 💛</span>
            </div>
            <input
              v-model.number="form.heartsMaxPremium"
              type="number"
              min="1"
              max="100"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Đặc quyền nâng dung lượng tim cho thành viên VIP.</p>
          </div>

          <!-- Heart Regen Interval -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Thời gian hồi phục 1 tim</label>
              <span class="text-xs font-bold text-cyan-400 font-mono">{{ form.heartRegenMinutes }} phút / 1 🤍</span>
            </div>
            <input
              v-model.number="form.heartRegenMinutes"
              type="number"
              min="1"
              max="1440"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Khoảng thời gian tự động phục hồi 1 tim cho tài khoản thường.</p>
          </div>

          <!-- Session Hours -->
          <div>
            <div class="flex items-center justify-between mb-1.5">
              <label class="text-xs font-semibold text-slate-200">Thời hạn phiên học miễn phí (Session Duration)</label>
              <span class="text-xs font-bold text-emerald-400 font-mono">{{ form.sessionHours }} giờ</span>
            </div>
            <input
              v-model.number="form.sessionHours"
              type="number"
              min="1"
              max="720"
              class="w-full px-3.5 py-2 rounded-xl bg-[#16192b] border border-slate-700/80 text-white font-mono text-sm focus:border-purple-500 focus:outline-none transition-colors"
            />
            <p class="text-[10px] text-slate-400 mt-1">Thời gian học sinh được học/xem lại tự do sau khi đã trừ 1 tim.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Bottom Actions -->
    <div class="p-4 rounded-2xl bg-[#0f111a] border border-slate-800 flex flex-col sm:flex-row items-center justify-between gap-3">
      <div class="flex items-center gap-2 text-xs text-slate-400">
        <ShieldCheck class="w-4 h-4 text-emerald-400" />
        <span>Dữ liệu cấu hình được lưu tự động vĩnh viễn và áp dụng tức thì.</span>
      </div>

      <div class="flex items-center gap-2 w-full sm:w-auto justify-end">
        <button
          type="button"
          @click="handleResetDefaults"
          :disabled="loading || saving"
          class="px-4 py-2 rounded-xl bg-slate-800 hover:bg-slate-700 text-slate-300 text-xs font-semibold transition-colors cursor-pointer"
        >
          Khôi phục mặc định
        </button>
        <button
          type="button"
          @click="handleSave"
          :disabled="loading || saving"
          class="px-5 py-2 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold flex items-center gap-1.5 shadow-lg shadow-purple-600/30 transition-all cursor-pointer disabled:opacity-50"
        >
          <Save class="w-3.5 h-3.5" />
          <span>{{ saving ? 'Đang lưu...' : 'Lưu Thay Đổi' }}</span>
        </button>
      </div>
    </div>
  </div>
</template>
