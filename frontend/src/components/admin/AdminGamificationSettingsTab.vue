<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import {
  Zap,
  Heart,
  Flame,
  Clock,
  RotateCcw,
  Save,
  Sparkles,
  BookOpen,
  Code2,
  HelpCircle,
  ShieldCheck,
  Award,
} from 'lucide-vue-next';
import * as adminApi from '@/api/admin';
import type { GamificationSettingsDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Badge from '@/components/ui/Badge.vue';

const ui = useUiStore();

const loading = ref(true);
const saving = ref(false);
const resetting = ref(false);

const form = reactive<GamificationSettingsDto>({
  theoryBaseXp: 50,
  quizBaseXp: 50,
  codelabBaseXp: 100,
  streakBonusXp: 20,
  heartsMaxFree: 10,
  heartsMaxPremium: 30,
  heartRegenMinutes: 30,
  sessionHours: 36,
});

async function loadSettings(): Promise<void> {
  loading.value = true;
  try {
    const data = await adminApi.fetchGamificationSettings();
    if (data) {
      form.theoryBaseXp = data.theoryBaseXp ?? 50;
      form.quizBaseXp = data.quizBaseXp ?? 50;
      form.codelabBaseXp = data.codelabBaseXp ?? 100;
      form.streakBonusXp = data.streakBonusXp ?? 20;
      form.heartsMaxFree = data.heartsMaxFree ?? 10;
      form.heartsMaxPremium = data.heartsMaxPremium ?? 30;
      form.heartRegenMinutes = data.heartRegenMinutes ?? 30;
      form.sessionHours = data.sessionHours ?? 36;
    }
  } catch (err: any) {
    ui.showToast(err?.message || 'Không thể tải cấu hình Gamification.', 'error');
  } finally {
    loading.value = false;
  }
}

async function handleSave(): Promise<void> {
  if (form.heartsMaxPremium < form.heartsMaxFree) {
    ui.showToast('Số tim Premium không thể nhỏ hơn số tim Free!', 'warning');
    return;
  }

  saving.value = true;
  try {
    const updated = await adminApi.updateGamificationSettings(form);
    if (updated) {
      form.theoryBaseXp = updated.theoryBaseXp;
      form.quizBaseXp = updated.quizBaseXp;
      form.codelabBaseXp = updated.codelabBaseXp;
      form.streakBonusXp = updated.streakBonusXp;
      form.heartsMaxFree = updated.heartsMaxFree;
      form.heartsMaxPremium = updated.heartsMaxPremium;
      form.heartRegenMinutes = updated.heartRegenMinutes;
      form.sessionHours = updated.sessionHours;
    }
    ui.showToast('Đã lưu cấu hình Gamification thành công! Toàn bộ hệ thống đã được cập nhật.', 'success');
  } catch (err: any) {
    ui.showToast(err?.message || 'Lưu cấu hình Gamification thất bại.', 'error');
  } finally {
    saving.value = false;
  }
}

async function handleReset(): Promise<void> {
  if (!confirm('Bạn có chắc chắn muốn khôi phục cấu hình Gamification về mặc định ban đầu?')) {
    return;
  }

  resetting.value = true;
  try {
    const res = await adminApi.resetGamificationSettings();
    if (res) {
      form.theoryBaseXp = res.theoryBaseXp;
      form.quizBaseXp = res.quizBaseXp;
      form.codelabBaseXp = res.codelabBaseXp;
      form.streakBonusXp = res.streakBonusXp;
      form.heartsMaxFree = res.heartsMaxFree;
      form.heartsMaxPremium = res.heartsMaxPremium;
      form.heartRegenMinutes = res.heartRegenMinutes;
      form.sessionHours = res.sessionHours;
    }
    ui.showToast('Đã khôi phục cấu hình Gamification về mặc định thành công!', 'info');
  } catch (err: any) {
    ui.showToast(err?.message || 'Khôi phục mặc định thất bại.', 'error');
  } finally {
    resetting.value = false;
  }
}

onMounted(() => {
  void loadSettings();
});
</script>

<template>
  <div class="gamification-settings-tab space-y-6">
    <div v-if="loading" class="space-y-4">
      <Skeleton v-for="i in 3" :key="i" height="120px" class="rounded-2xl" />
    </div>

    <form v-else @submit.prevent="handleSave" class="space-y-6">
      <!-- Live Preview Bar -->
      <div class="p-4 rounded-2xl bg-gradient-to-r from-purple-950/40 via-[#181628] to-emerald-950/30 border border-purple-500/30 shadow-lg">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
          <div class="flex items-center gap-2">
            <div class="p-2 rounded-xl bg-purple-500/20 text-purple-400 border border-purple-500/30">
              <Sparkles :size="20" />
            </div>
            <div>
              <h3 class="text-sm font-bold text-white flex items-center gap-2">
                Trực quan hóa Điểm thưởng & Kinh tế
                <Badge variant="success" size="sm">Hoạt động thời gian thực</Badge>
              </h3>
              <p class="text-xs text-slate-400">Các huy hiệu bên dưới hiển thị đúng theo tham số bạn cấu hình</p>
            </div>
          </div>

          <div class="flex flex-wrap items-center gap-2">
            <div class="px-3 py-1.5 rounded-xl bg-blue-500/15 border border-blue-500/30 text-blue-300 text-xs font-bold flex items-center gap-1.5">
              <BookOpen :size="14" />
              <span>Lý thuyết: +{{ form.theoryBaseXp }} XP</span>
            </div>
            <div class="px-3 py-1.5 rounded-xl bg-amber-500/15 border border-amber-500/30 text-amber-300 text-xs font-bold flex items-center gap-1.5">
              <HelpCircle :size="14" />
              <span>Quiz: +{{ form.quizBaseXp }} XP</span>
            </div>
            <div class="px-3 py-1.5 rounded-xl bg-emerald-500/15 border border-emerald-500/30 text-emerald-300 text-xs font-bold flex items-center gap-1.5">
              <Code2 :size="14" />
              <span>Codelab: +{{ form.codelabBaseXp }} XP</span>
            </div>
            <div class="px-3 py-1.5 rounded-xl bg-rose-500/15 border border-rose-500/30 text-rose-300 text-xs font-bold flex items-center gap-1.5">
              <Heart :size="14" class="fill-rose-400" />
              <span>Free: {{ form.heartsMaxFree }} Tim / Pro: {{ form.heartsMaxPremium }} Tim</span>
            </div>
          </div>
        </div>
      </div>

      <!-- 1. Điểm Thưởng XP -->
      <div class="settings-card card p-5 rounded-2xl bg-[#141320] border border-[#27243c] space-y-4">
        <div class="flex items-center gap-3 pb-3 border-b border-[#27243c]">
          <div class="p-2.5 rounded-xl bg-amber-500/20 text-amber-400 border border-amber-500/30">
            <Zap :size="22" />
          </div>
          <div>
            <h2 class="text-base font-bold text-white">Chính Sách Điểm Thưởng Kinh Nghiệm (XP)</h2>
            <p class="text-xs text-slate-400">Điều chỉnh mức điểm thưởng XP cho từng hoạt động học tập của học viên</p>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 pt-2">
          <!-- Theory XP -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="theory-xp">
              <BookOpen :size="14" class="text-blue-400" />
              Bài học Lý thuyết (Theory)
            </label>
            <div class="relative">
              <input
                id="theory-xp"
                v-model.number="form.theoryBaseXp"
                type="number"
                min="0"
                max="1000"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="50"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-amber-400 pointer-events-none">XP</span>
            </div>
            <p class="text-[11px] text-slate-400">Cộng khi hoàn thành bài đọc</p>
          </div>

          <!-- Quiz XP -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="quiz-xp">
              <HelpCircle :size="14" class="text-amber-400" />
              Trắc nghiệm (Quiz)
            </label>
            <div class="relative">
              <input
                id="quiz-xp"
                v-model.number="form.quizBaseXp"
                type="number"
                min="0"
                max="1000"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="50"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-amber-400 pointer-events-none">XP</span>
            </div>
            <p class="text-[11px] text-slate-400">Điểm tối đa khi pass bài trắc nghiệm</p>
          </div>

          <!-- Codelab XP -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="codelab-xp">
              <Code2 :size="14" class="text-emerald-400" />
              Thực hành Code Lab
            </label>
            <div class="relative">
              <input
                id="codelab-xp"
                v-model.number="form.codelabBaseXp"
                type="number"
                min="0"
                max="1000"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="100"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-amber-400 pointer-events-none">XP</span>
            </div>
            <p class="text-[11px] text-slate-400">Cộng khi hoàn thành bài code lab</p>
          </div>

          <!-- Streak Bonus XP -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="streak-xp">
              <Flame :size="14" class="text-orange-400" />
              Thưởng Chuỗi Streak
            </label>
            <div class="relative">
              <input
                id="streak-xp"
                v-model.number="form.streakBonusXp"
                type="number"
                min="0"
                max="1000"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="20"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-amber-400 pointer-events-none">XP/ngày</span>
            </div>
            <p class="text-[11px] text-slate-400">Cộng thưởng khi giữ streak hàng ngày</p>
          </div>
        </div>
      </div>

      <!-- 2. Kinh Tế Trái Tim & Phiên Học -->
      <div class="settings-card card p-5 rounded-2xl bg-[#141320] border border-[#27243c] space-y-4">
        <div class="flex items-center gap-3 pb-3 border-b border-[#27243c]">
          <div class="p-2.5 rounded-xl bg-rose-500/20 text-rose-400 border border-rose-500/30">
            <Heart :size="22" class="fill-rose-400" />
          </div>
          <div>
            <h2 class="text-base font-bold text-white">Kinh Tế Trái Tim & Thời Hạn Phiên Học</h2>
            <p class="text-xs text-slate-400">Quy định cơ chế trừ tim, hồi phục tim tự động và thời hạn session học tập</p>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 pt-2">
          <!-- Free Max Hearts -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="free-hearts">
              <Heart :size="14" class="text-rose-400" />
              Tim tối đa (Free)
            </label>
            <div class="relative">
              <input
                id="free-hearts"
                v-model.number="form.heartsMaxFree"
                type="number"
                min="1"
                max="100"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="10"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-rose-400 pointer-events-none">Tim</span>
            </div>
            <p class="text-[11px] text-slate-400">Giới hạn tim của học viên miễn phí</p>
          </div>

          <!-- Premium Max Hearts -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="premium-hearts">
              <Award :size="14" class="text-purple-400" />
              Tim tối đa (Premium)
            </label>
            <div class="relative">
              <input
                id="premium-hearts"
                v-model.number="form.heartsMaxPremium"
                type="number"
                min="1"
                max="100"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="30"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-purple-400 pointer-events-none">Tim</span>
            </div>
            <p class="text-[11px] text-slate-400">Giới hạn tim mở rộng cho gói Pro</p>
          </div>

          <!-- Heart Regen Minutes -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="heart-regen">
              <Clock :size="14" class="text-cyan-400" />
              Thời gian hồi 1 Tim
            </label>
            <div class="relative">
              <input
                id="heart-regen"
                v-model.number="form.heartRegenMinutes"
                type="number"
                min="1"
                max="1440"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="30"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-cyan-400 pointer-events-none">Phút</span>
            </div>
            <p class="text-[11px] text-slate-400">Thời gian tự động hồi 1 tim (Free)</p>
          </div>

          <!-- Session Duration Hours -->
          <div class="space-y-1.5">
            <label class="text-xs font-semibold text-slate-300 flex items-center gap-1.5" for="session-hours">
              <ShieldCheck :size="14" class="text-emerald-400" />
              Thời hạn Session Node
            </label>
            <div class="relative">
              <input
                id="session-hours"
                v-model.number="form.sessionHours"
                type="number"
                min="1"
                max="168"
                class="w-full bg-[#1b192c] border border-[#322f4d] focus:border-purple-500 rounded-xl px-3.5 py-2.5 text-sm text-white font-mono font-bold focus:outline-none transition-colors"
                placeholder="36"
                required
              />
              <span class="absolute right-3.5 top-1/2 -translate-y-1/2 text-xs font-bold text-emerald-400 pointer-events-none">Giờ</span>
            </div>
            <p class="text-[11px] text-slate-400">Thời hạn session trước khi trừ tim lại</p>
          </div>
        </div>
      </div>

      <!-- Action Buttons Bar -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-4 pt-2">
        <button
          type="button"
          @click="handleReset"
          :disabled="resetting || saving"
          class="w-full sm:w-auto px-5 py-2.5 rounded-xl bg-[#1e1c30] hover:bg-[#282542] text-slate-300 hover:text-white border border-[#353254] font-bold text-xs sm:text-sm flex items-center justify-center gap-2 transition-all cursor-pointer disabled:opacity-50"
        >
          <RotateCcw :size="16" :class="{ 'animate-spin': resetting }" />
          <span>Khôi phục mặc định</span>
        </button>

        <div class="flex items-center gap-3 w-full sm:w-auto">
          <button
            type="submit"
            :disabled="saving || resetting"
            class="w-full sm:w-auto px-7 py-2.5 rounded-xl bg-gradient-to-r from-purple-600 via-indigo-600 to-emerald-600 hover:from-purple-500 hover:to-emerald-500 text-white font-bold text-xs sm:text-sm shadow-lg shadow-purple-600/30 flex items-center justify-center gap-2 transition-all cursor-pointer hover:scale-[1.02] disabled:opacity-50"
          >
            <Save :size="16" :class="{ 'animate-pulse': saving }" />
            <span>{{ saving ? 'Đang lưu cấu hình...' : 'Lưu cấu hình Gamification' }}</span>
          </button>
        </div>
      </div>
    </form>
  </div>
</template>

<style scoped>
.gamification-settings-tab {
  animation: fadeIn 0.25s ease-out;
}
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
