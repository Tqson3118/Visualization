<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { Check, Crown, ShieldCheck, Trash2, Upload } from 'lucide-vue-next';
import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import * as authApi from '@/api/auth';
import { equippedItem } from '@/utils/equipment';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Input from '@/components/ui/Input.vue';
import Modal from '@/components/ui/Modal.vue';
import TwoFactorModal from './TwoFactorModal.vue';

const router = useRouter();
const auth = useAuthStore();
const gamification = useGamificationStore();
const ui = useUiStore();

// Profile Name Form
const displayNameInput = ref(auth.user?.displayName || '');
const nameUpdating = ref(false);

// Password Form
const passwordForm = ref({ current: '', next: '' });
const passwordError = ref('');
const passwordBusy = ref(false);

// Avatar management
const avatarError = ref('');
const avatarUploading = ref(false);
const avatarFileInput = ref<HTMLInputElement | null>(null);
const avatarLocalFile = ref<File | null>(null);
const avatarLocalPreview = ref<string | null>(null);
const isPremiumUser = computed(() => {
  return gamification.isPremium || auth.user?.role === 'ADMIN' || auth.user?.role === 'TEACHER';
});

function triggerDeviceUpload(): void {
  if (!isPremiumUser.value) {
    ui.showToast('Tính năng tải ảnh từ thiết bị chỉ dành cho tài khoản Premium.', 'warning');
    void router.push('/premium');
    return;
  }
  avatarFileInput.value?.click();
}

function onAvatarFileChange(event: Event): void {
  if (!isPremiumUser.value) {
    avatarError.value = 'Tính năng tải ảnh từ thiết bị chỉ dành cho tài khoản Premium.';
    return;
  }
  const input = event.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const file = input.files[0];
  if (file.size > 3 * 1024 * 1024) {
    avatarError.value = 'Kích thước ảnh không được vượt quá 3MB.';
    return;
  }
  avatarError.value = '';
  avatarLocalFile.value = file;
  const reader = new FileReader();
  reader.onload = (e) => {
    avatarLocalPreview.value = e.target?.result as string;
  };
  reader.readAsDataURL(file);
}

function cancelLocalAvatar(): void {
  avatarLocalFile.value = null;
  avatarLocalPreview.value = null;
  if (avatarFileInput.value) avatarFileInput.value.value = '';
}

async function uploadLocalAvatar(): Promise<void> {
  if (!avatarLocalFile.value) return;
  if (!isPremiumUser.value) {
    avatarError.value = 'Tính năng tải ảnh từ thiết bị chỉ dành cho tài khoản Premium.';
    return;
  }
  avatarUploading.value = true;
  avatarError.value = '';
  try {
    const base64 = await new Promise<string>((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = reject;
      reader.readAsDataURL(avatarLocalFile.value!);
    });

    const { client } = await import('@/api/client');
    const res = await client.post<{ url: string }>('/auth/me/avatar', {
      image: base64,
      name: avatarLocalFile.value.name,
    });

    if (res.data?.url) {
      await auth.fetchMe();
      await gamification.fetchInventory();
      ui.showToast('Tải lên và cập nhật ảnh đại diện từ thiết bị thành công!', 'success');
      cancelLocalAvatar();
    } else {
      throw new Error('Không nhận được đường dẫn ảnh sau khi tải lên.');
    }
  } catch (err) {
    avatarError.value = err instanceof Error ? err.message : 'Tải ảnh thất bại.';
  } finally {
    avatarUploading.value = false;
  }
}

onMounted(() => {
  void gamification.fetchPremium();
});

async function updateAvatarUrl(url: string | null): Promise<void> {
  if (url && !isPremiumUser.value) {
    avatarError.value = 'Tính năng sử dụng ảnh đại diện tùy chỉnh chỉ dành cho tài khoản Premium.';
    return;
  }
  if (url) {
    const trimmed = url.trim();
    if (trimmed.length > 500) {
      avatarError.value = 'Đường dẫn ảnh không được vượt quá 500 ký tự.';
      return;
    }
    if (!trimmed.startsWith('http://') && !trimmed.startsWith('https://') && !trimmed.startsWith('/')) {
      avatarError.value = 'Đường dẫn ảnh phải bắt đầu bằng https://, http:// hoặc /assets/...';
      return;
    }
  }

  avatarUploading.value = true;
  avatarError.value = '';
  try {
    if (!url) {
      const equippedAv = equippedItem(gamification.inventory, 'avatar');
      if (equippedAv) {
        try {
          await gamification.equipItem(equippedAv.itemId, false);
          await gamification.fetchInventory();
        } catch {
          // ignore unequip error
        }
      }
    }
    await authApi.updateProfile({ avatarUrl: url ? url.trim() : '' });
    await auth.fetchMe();
    ui.showToast(url ? 'Cập nhật ảnh đại diện thành công!' : 'Đã xóa ảnh đại diện về mặc định.', 'success');
    cancelLocalAvatar();
  } catch (err) {
    avatarError.value = err instanceof Error ? err.message : 'Không thể cập nhật ảnh.';
  } finally {
    avatarUploading.value = false;
  }
}

async function onUpdateDisplayName(): Promise<void> {
  if (displayNameInput.value.trim().length < 2) {
    ui.showToast('Họ và tên phải từ 2 ký tự trở lên.', 'warning');
    return;
  }
  nameUpdating.value = true;
  try {
    await authApi.updateProfile({ displayName: displayNameInput.value.trim() });
    await auth.fetchMe();
    ui.showToast('Cập nhật họ tên thành công!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Cập nhật thất bại.', 'error');
  } finally {
    nameUpdating.value = false;
  }
}

async function onChangePassword(): Promise<void> {
  passwordError.value = '';
  if (passwordForm.value.next.length < 6) {
    passwordError.value = messages.profile.passwordTooShort;
    return;
  }
  passwordBusy.value = true;
  try {
    await authApi.changePassword({ currentPassword: passwordForm.value.current, newPassword: passwordForm.value.next });
    ui.showToast(messages.profile.passwordChanged, 'success');
    passwordForm.value = { current: '', next: '' };
  } catch (err) {
    passwordError.value = err instanceof Error ? err.message : messages.profile.passwordTooShort;
  } finally {
    passwordBusy.value = false;
  }
}

// 2FA State & Actions
const twoFactorModalOpen = ref(false);
const twoFactorDisableModalOpen = ref(false);
const twoFactorBusy = ref(false);

async function openEnable2FaModal(): Promise<void> {
  twoFactorBusy.value = true;
  try {
    await authApi.send2FaCode();
    twoFactorModalOpen.value = true;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể gửi mã xác thực 2FA.', 'error');
  } finally {
    twoFactorBusy.value = false;
  }
}

function openDisable2FaModal(): void {
  twoFactorDisableModalOpen.value = true;
}

async function handleConfirmDisable2Fa(): Promise<void> {
  twoFactorBusy.value = true;
  try {
    await authApi.toggle2Fa(false);
    await auth.fetchMe();
    ui.showToast('Đã tắt xác thực hai lớp (2FA).', 'info');
    twoFactorDisableModalOpen.value = false;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tắt 2FA. Vui lòng thử lại.', 'error');
  } finally {
    twoFactorBusy.value = false;
  }
}
</script>

<template>
  <div class="card profile__settings space-y-6">
    <!-- 1. Thông tin cá nhân -->
    <section>
      <h2 class="profile__panel-title mb-3">Thông tin tài khoản</h2>
      <form class="space-y-4 max-w-lg" @submit.prevent="onUpdateDisplayName">
        <Input v-model="displayNameInput" label="Họ và tên hiển thị" placeholder="Nhập họ và tên..." required />

        <div class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Địa chỉ Email</label>
            <div class="px-3 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl text-xs text-vdsa-muted flex items-center justify-between">
              <span>{{ auth.user?.email }}</span>
              <Badge variant="success" class="text-[10px]">Đã xác thực</Badge>
            </div>
          </div>

          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Vai trò hệ thống</label>
            <div class="px-3 py-2 bg-vdsa-surface border border-vdsa-border rounded-xl text-xs text-vdsa-muted flex items-center gap-1.5">
              <ShieldCheck :size="14" class="text-vdsa-purple" />
              <span class="font-bold text-white">{{ auth.role }}</span>
            </div>
          </div>
        </div>

        <Button type="submit" size="sm" variant="primary" :loading="nameUpdating">
          Lưu thay đổi họ tên
        </Button>
      </form>
    </section>

    <hr class="profile__divider" />

    <!-- 2. Ảnh đại diện -->
    <section>
      <div class="flex flex-wrap items-center justify-between gap-2 mb-3">
        <div>
          <h2 class="profile__panel-title">Ảnh đại diện</h2>
          <p class="text-xs text-vdsa-muted mt-0.5">Tải ảnh từ thiết bị, chọn Avatar mẫu có sẵn hoặc nhập liên kết ảnh trực tiếp.</p>
        </div>
        <Button
          v-if="auth.user?.avatarUrl"
          variant="secondary"
          size="sm"
          :loading="avatarUploading"
          @click="updateAvatarUrl(null)"
        >
          Xóa ảnh (Về mặc định)
        </Button>
      </div>

      <!-- Hidden Device File Input -->
      <input
        ref="avatarFileInput"
        type="file"
        accept="image/png, image/jpeg, image/jpg, image/webp, image/gif"
        class="hidden"
        @change="onAvatarFileChange"
      />

      <!-- Device File Selected Preview Banner -->
      <div
        v-if="avatarLocalFile && avatarLocalPreview"
        class="flex flex-col sm:flex-row items-center justify-between gap-4 p-4 rounded-xl bg-vdsa-accent/10 border border-vdsa-accent/40 mb-4 overflow-hidden"
      >
        <div class="flex items-center gap-3 w-full sm:w-auto min-w-0 flex-1">
          <div class="w-14 h-14 rounded-full overflow-hidden border-2 border-vdsa-accent shrink-0 shadow-md">
            <img :src="avatarLocalPreview" alt="Preview" class="w-full h-full object-cover" />
          </div>
          <div class="min-w-0 flex-1 overflow-hidden">
            <p class="text-xs font-bold text-white truncate block max-w-full" :title="avatarLocalFile.name">
              {{ avatarLocalFile.name }}
            </p>
            <p class="text-[11px] text-vdsa-muted shrink-0">
              {{ (avatarLocalFile.size / 1024).toFixed(1) }} KB · Sẵn sàng tải lên
            </p>
          </div>
        </div>
        <div class="flex items-center gap-2 w-full sm:w-auto justify-end shrink-0">
          <Button
            variant="primary"
            size="sm"
            :loading="avatarUploading"
            class="gap-1.5 shrink-0"
            @click="uploadLocalAvatar"
          >
            <Upload :size="14" /> Tải lên & Lưu ngay
          </Button>
          <Button
            variant="ghost"
            size="sm"
            :disabled="avatarUploading"
            class="shrink-0"
            @click="cancelLocalAvatar"
          >
            Hủy
          </Button>
        </div>
      </div>

      <!-- Main Avatar Settings Box -->
      <div class="flex flex-col sm:flex-row items-start sm:items-center gap-4 p-4 rounded-xl bg-vdsa-surface border border-vdsa-border mb-5">
          <div class="profile__avatar-preview shrink-0 flex flex-col items-center">
            <img
              v-if="auth.user?.avatarUrl"
              :src="auth.user.avatarUrl"
              alt="Avatar"
              class="profile__avatar-preview-img"
              @error="avatarError = 'Không thể tải ảnh từ URL này. Vui lòng kiểm tra lại liên kết.'"
            />
            <span v-else class="profile__avatar-preview-placeholder font-bold text-white text-lg">
              {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
            </span>
            <button
              v-if="auth.user?.avatarUrl"
              type="button"
              class="mt-2 text-[11px] font-semibold text-rose-400 hover:text-rose-300 hover:underline flex items-center gap-1 cursor-pointer transition-colors"
              :disabled="avatarUploading"
              @click="updateAvatarUrl(null)"
            >
              <Trash2 :size="12" /> Xóa ảnh
            </button>
          </div>

        <div class="flex-1 w-full space-y-3">
          <!-- Upload from device button (Only for Premium / VIP) -->
          <div v-if="isPremiumUser" class="flex flex-wrap items-center gap-2">
            <Button
              type="button"
              variant="primary"
              size="sm"
              :loading="avatarUploading"
              class="gap-1.5"
              @click="triggerDeviceUpload"
            >
              <Upload :size="14" /> Tải ảnh từ thiết bị
            </Button>
            <Button
              v-if="auth.user?.avatarUrl"
              type="button"
              variant="ghost"
              size="sm"
              :disabled="avatarUploading"
              class="gap-1.5 text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 border border-rose-500/25"
              @click="updateAvatarUrl(null)"
            >
              <Trash2 :size="13" /> Đặt lại ảnh mặc định
            </Button>
            <span class="inline-flex items-center gap-1 text-[10px] font-bold px-2 py-0.5 rounded-full bg-amber-500/15 text-amber-400 border border-amber-500/30">
              <Crown :size="11" /> VIP
            </span>
            <span class="text-[11px] text-vdsa-muted">Hỗ trợ JPG, PNG, WEBP (tối đa 3MB)</span>
          </div>
          <div v-else class="flex flex-wrap items-center gap-2 p-2.5 rounded-xl bg-amber-500/5 border border-amber-500/20">
            <RouterLink
              to="/premium"
              class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold bg-amber-500/20 text-amber-400 hover:bg-amber-500/30 border border-amber-500/40 transition-colors"
            >
              <Crown :size="13" class="text-amber-400" />
              <span>Tải ảnh từ thiết bị</span>
              <span class="text-[10px] uppercase tracking-wider bg-amber-500 text-black px-1.5 py-0.2 rounded font-black">PRO</span>
            </RouterLink>
            <Button
              v-if="auth.user?.avatarUrl"
              type="button"
              variant="ghost"
              size="sm"
              :disabled="avatarUploading"
              class="gap-1.5 text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 border border-rose-500/25"
              @click="updateAvatarUrl(null)"
            >
              <Trash2 :size="13" /> Đặt lại ảnh mặc định
            </Button>
            <span class="text-[11px] text-vdsa-muted">Chỉ dành cho tài khoản Premium. Hãy nâng cấp để tải ảnh tùy biến!</span>
          </div>
        </div>
      </div>

      <p v-if="avatarError" class="profile__avatar-error mt-3 font-medium" role="alert">{{ avatarError }}</p>
    </section>

    <hr class="profile__divider" />

    <!-- 3. Đổi mật khẩu -->
    <section>
      <h2 class="profile__panel-title mb-3">Bảo mật & Đổi mật khẩu</h2>
      <form class="profile__password max-w-lg" novalidate @submit.prevent="onChangePassword">
        <Input v-model="passwordForm.current" label="Mật khẩu hiện tại" type="password" autocomplete="current-password" required />
        <Input v-model="passwordForm.next" label="Mật khẩu mới (tối thiểu 6 ký tự)" type="password" autocomplete="new-password" required />
        <p v-if="passwordError" class="profile__password-error" role="alert">{{ passwordError }}</p>
        <Button type="submit" size="sm" :loading="passwordBusy">{{ messages.profile.savePassword }}</Button>
      </form>
    </section>

    <hr class="profile__divider" />

    <!-- 4. Xác thực hai yếu tố (2FA - B1) -->
    <section>
      <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
        <div>
          <h2 class="profile__panel-title flex items-center gap-2">
            <ShieldCheck :size="18" class="text-vdsa-purple" />
            Xác thực hai lớp (2FA) qua Email
          </h2>
          <p class="text-xs text-vdsa-muted mt-1">
            Tăng cường bảo mật bằng mã OTP 6 chữ số gửi về hộp thư email khi đăng nhập.
          </p>
        </div>
        <div class="flex items-center gap-3">
          <Badge :variant="auth.user?.twoFactorEnabled ? 'success' : 'muted'">
            {{ auth.user?.twoFactorEnabled ? 'Đang bật' : 'Đang tắt' }}
          </Badge>
          <Button
            :variant="auth.user?.twoFactorEnabled ? 'danger' : 'primary'"
            size="sm"
            :loading="twoFactorBusy"
            @click="auth.user?.twoFactorEnabled ? openDisable2FaModal() : openEnable2FaModal()"
          >
            {{ auth.user?.twoFactorEnabled ? 'Tắt 2FA' : 'Bật 2FA' }}
          </Button>
        </div>
      </div>
    </section>

    <!-- Two Factor Enable Modal -->
    <TwoFactorModal
      :open="twoFactorModalOpen"
      @close="twoFactorModalOpen = false"
      @success="auth.fetchMe()"
    />

    <!-- Two Factor Disable Modal -->
    <Modal
      :open="twoFactorDisableModalOpen"
      title="Tắt xác thực hai lớp (2FA)?"
      class="max-w-md"
      @close="twoFactorDisableModalOpen = false"
    >
      <div class="space-y-4">
        <div class="p-4 rounded-xl bg-rose-500/10 border border-rose-500/30">
          <p class="text-sm font-medium text-white leading-relaxed">
            Bạn có chắc chắn muốn tắt xác thực hai lớp (2FA)?
            <br /><br />
            <span class="text-xs text-rose-300">
              Khi tắt, tài khoản của bạn sẽ chỉ được bảo vệ bằng mật khẩu và có nguy cơ rủi ro bảo mật cao hơn.
            </span>
          </p>
        </div>

        <div class="flex items-center justify-end gap-3 pt-3 border-t border-vdsa-border">
          <Button variant="ghost" size="sm" :disabled="twoFactorBusy" @click="twoFactorDisableModalOpen = false">
            Hủy
          </Button>
          <Button
            variant="danger"
            size="sm"
            :loading="twoFactorBusy"
            @click="handleConfirmDisable2Fa"
          >
            Tắt xác thực 2FA
          </Button>
        </div>
      </div>
    </Modal>
  </div>
</template>

<style scoped>
.profile__settings {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-xl, 16px);
  padding: var(--space-xl, 24px);
}

.profile__panel-title {
  font-size: var(--text-md, 15px);
  font-weight: 700;
  color: #ffffff;
  margin: 0;
}

.profile__divider {
  border: none;
  height: 1px;
  background: var(--color-border, #30363d);
  margin-block: var(--space-lg, 24px);
}

.profile__avatar-preview {
  width: 64px;
  height: 64px;
  border-radius: var(--radius-full, 9999px);
  background: linear-gradient(135deg, var(--color-primary, #7c3aed), #a855f7);
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  border: 2px solid var(--color-border, #30363d);
}

.profile__avatar-preview-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.profile__avatar-error,
.profile__password-error {
  font-size: var(--text-xs, 12px);
  color: #ef4444;
  margin: 0;
}

.profile__password {
  display: flex;
  flex-direction: column;
  gap: var(--space-md, 16px);
}
</style>
