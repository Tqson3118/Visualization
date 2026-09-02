import { ref, computed } from 'vue';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';

/**
 * useHeartSystem — Quản lý tiêu thụ tim và trạng thái tim tập trung.
 * Ngăn chặn lỗi trừ 2 tim (Double spend heart) khi vừa mở khóa lộ trình vừa vào bài 1.
 */
export function useHeartSystem() {
  const gamification = useGamificationStore();
  const ui = useUiStore();
  const auth = useAuthStore();

  const isDeducting = ref(false);

  const hearts = computed(() => gamification.hearts);
  const maxHearts = computed(() => gamification.heartsMax || 5);
  const hasHearts = computed(() => hearts.value > 0);

  /**
   * Trừ 1 tim một cách an toàn kèm thông báo lỗi nếu hết tim
   * @param reason Lý do trừ tim
   * @param isOwner Cờ bỏ qua trừ tim nếu là chủ sở hữu hoặc admin
   */
  async function spendHeartSafely(reason = 'Bắt đầu học', isOwner = false): Promise<boolean> {
    if (!auth.isAuthenticated) return true;
    if (auth.user?.role === 'ADMIN' || isOwner) return true;

    if (!hasHearts.value) {
      ui.showToast('Bạn đã hết tim. Hãy chờ hồi phục hoặc nâng cấp Premium để tiếp tục!', 'warning');
      return false;
    }

    isDeducting.value = true;
    try {
      await gamification.spendHeart();
      return true;
    } catch (err: any) {
      const errorCode = err?.response?.data?.code || err?.code || '';
      if (errorCode === 'HEARTS_EMPTY' || String(err?.message || '').includes('HEARTS_EMPTY') || String(err?.message || '').includes('hết tim')) {
        ui.showToast('Bạn đã hết tim. Hãy chờ hồi phục hoặc nâng cấp Premium!', 'warning');
        return false;
      }
      ui.showToast(err?.message || 'Không thể sử dụng tim', 'error');
      return false;
    } finally {
      isDeducting.value = false;
    }
  }

  /**
   * Tham gia node bài học với cơ chế guard
   * @param pathId ID lộ trình
   * @param nodeId ID node bài học
   * @param skipCharge Bỏ qua trừ tim (dùng khi vừa mở khóa lộ trình)
   * @param isOwner Cờ bỏ qua trừ tim nếu là chủ sở hữu hoặc admin
   */
  async function enterLessonNode(pathId: number, nodeId: number, skipCharge = false, isOwner = false): Promise<boolean> {
    if (!auth.isAuthenticated) return true;
    if (auth.user?.role === 'ADMIN' || isOwner) return true;
    if (skipCharge) return true;

    try {
      const res = await gamification.enterNode(pathId, nodeId);
      if (typeof res?.heartsLeft === 'number') {
        gamification.hearts = res.heartsLeft;
      }
      return true;
    } catch (err: any) {
      const errorCode = err?.response?.data?.code || err?.code || '';
      if (errorCode === 'HEARTS_EMPTY' || String(err?.message || '').includes('HEARTS_EMPTY') || String(err?.message || '').includes('hết tim')) {
        ui.showToast('Bạn đã hết tim. Hãy chờ hồi phục hoặc nâng cấp Premium để học tiếp!', 'warning');
        return false;
      }
      console.warn('Enter node warning:', err);
      return true;
    }
  }

  return {
    hearts,
    maxHearts,
    hasHearts,
    isDeducting,
    spendHeartSafely,
    enterLessonNode,
  };
}
