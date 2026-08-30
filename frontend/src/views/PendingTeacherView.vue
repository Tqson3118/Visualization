<script setup lang="ts">
import { useRouter } from 'vue-router';
import { Clock, GraduationCap, LogOut, Home, CheckCircle2 } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth';
import Button from '@/components/ui/Button.vue';
import Card from '@/components/ui/Card.vue';
import Badge from '@/components/ui/Badge.vue';

const auth = useAuthStore();
const router = useRouter();

async function handleLogout(): Promise<void> {
  await auth.logout();
  await router.replace({ name: 'login' });
}

function goHome(): void {
  void router.push({ name: 'home' });
}
</script>

<template>
  <div class="pending-teacher min-h-[calc(100vh-var(--app-header-h,68px))] flex items-center justify-center p-4 bg-[#0b0a12]">
    <Card class="max-w-lg w-full p-8 bg-[#161b22] border border-[#30363d] rounded-2xl shadow-2xl space-y-6 text-center">
      <!-- Icon badge -->
      <div class="w-16 h-16 rounded-full bg-amber-500/15 border border-amber-500/30 text-amber-400 flex items-center justify-center mx-auto shadow-lg shadow-amber-950/20">
        <Clock :size="32" class="animate-pulse" />
      </div>

      <div class="space-y-2">
        <div class="inline-flex items-center gap-1.5 px-3 py-1 rounded-full bg-amber-500/10 text-amber-300 text-xs font-mono font-bold">
          <GraduationCap :size="14" /> Đang Chờ Phê Duyệt Giảng Viên
        </div>
        <h1 class="text-2xl font-black text-white tracking-tight">
          Hồ sơ của bạn đang được xét duyệt!
        </h1>
        <p class="text-sm text-slate-300 leading-relaxed">
          Xin chào <strong class="text-white">{{ auth.user?.displayName || auth.user?.email }}</strong>, tài khoản Giảng viên của bạn đã được ghi nhận và đang chờ Quản trị viên (Admin) phê duyệt quyền truy cập Studio biên soạn.
        </p>
      </div>

      <!-- Detail Steps Box -->
      <div class="p-4 rounded-xl bg-[#0d1117] border border-[#21262d] text-left space-y-3 text-xs">
        <div class="flex items-start gap-2.5 text-slate-300">
          <CheckCircle2 :size="16" class="text-emerald-400 shrink-0 mt-0.5" />
          <div>
            <strong class="text-white block">1. Đăng ký thành công</strong>
            <span class="text-slate-400 text-[11px]">Thông tin hồ sơ và minh chứng chuyên môn đã được gửi tới hội đồng admin.</span>
          </div>
        </div>
        <div class="flex items-start gap-2.5 text-slate-300">
          <Clock :size="16" class="text-amber-400 shrink-0 mt-0.5" />
          <div>
            <strong class="text-white block">2. Đang kiểm tra & kích hoạt</strong>
            <span class="text-slate-400 text-[11px]">Thời gian xét duyệt thông thường từ 12-24 giờ làm việc. Bạn sẽ nhận được email thông báo ngay khi được phê duyệt.</span>
          </div>
        </div>
      </div>

      <div class="flex items-center justify-center gap-3 pt-2">
        <Button variant="secondary" size="md" class="gap-1.5 text-xs" @click="goHome">
          <Home :size="14" /> Về trang chủ
        </Button>
        <Button variant="danger" size="md" class="gap-1.5 text-xs" @click="handleLogout">
          <LogOut :size="14" /> Đăng xuất
        </Button>
      </div>
    </Card>
  </div>
</template>
