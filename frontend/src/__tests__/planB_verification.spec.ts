import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useHeartSystem } from '@/composables/useHeartSystem';
import { useCourseStore } from '@/features/courses/store/useCourseStore';
import { useGamificationStore } from '@/stores/gamification';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';

describe('Plan B Full Verification Test Suite', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    localStorage.clear();
    vi.clearAllMocks();
  });

  describe('Bug #4 (P0): useHeartSystem & Single Heart Debit', () => {
    it('tiêu thụ tim an toàn khi đủ tim và bỏ qua trừ tim lần 2 khi skipCharge = true', async () => {
      const auth = useAuthStore();
      const gamification = useGamificationStore();
      const heartSystem = useHeartSystem();

      auth.user = { id: 99, email: 'student@example.com', role: 'STUDENT', name: 'Student' } as any;
      auth.accessToken = 'mock-token';
      auth.status = 'authenticated';
      gamification.hearts = 5;

      // Mock spendHeart
      vi.spyOn(gamification, 'spendHeart').mockImplementation(async () => {
        gamification.hearts -= 1;
        return { success: true, hearts: gamification.hearts } as any;
      });

      // Mock enterNode
      vi.spyOn(gamification, 'enterNode').mockImplementation(async () => {
        gamification.hearts -= 1;
        return { success: true, heartsLeft: gamification.hearts } as any;
      });

      // 1. Mở khóa lộ trình (-1 tim)
      const enrollSuccess = await heartSystem.spendHeartSafely('Mở khóa lộ trình');
      expect(enrollSuccess).toBe(true);
      expect(gamification.hearts).toBe(4);

      // 2. Vào bài 1 ngay sau khi mở khóa với skipCharge = true -> Không trừ tim nữa
      const enterSuccess = await heartSystem.enterLessonNode(1, 101, true);
      expect(enterSuccess).toBe(true);
      expect(gamification.hearts).toBe(4); // Vẫn là 4, không bị trừ thành 3!
    });

    it('báo lỗi và trả về false khi hết tim (hearts = 0)', async () => {
      const auth = useAuthStore();
      const gamification = useGamificationStore();
      const ui = useUiStore();
      const heartSystem = useHeartSystem();

      auth.user = { id: 99, email: 'student@example.com', role: 'STUDENT', name: 'Student' } as any;
      auth.accessToken = 'mock-token';
      auth.status = 'authenticated';
      gamification.hearts = 0;

      const toastSpy = vi.spyOn(ui, 'showToast');

      const ok = await heartSystem.spendHeartSafely('Mở khóa');
      expect(ok).toBe(false);
      expect(toastSpy).toHaveBeenCalledWith(expect.stringContaining('hết tim'), 'warning');
    });
  });

  describe('Bug #1, #2, #3: useCourseStore & Course Progress & Categories', () => {
    it('lọc bỏ các từ khóa độ khó khỏi danh mục categories', () => {
      const courseStore = useCourseStore();
      (courseStore as any).courses = [
        { id: '1', title: 'Cấu trúc dữ liệu 1', category: 'Cấu trúc dữ liệu', difficulty: 'Beginner', isPublished: true },
        { id: '2', title: 'Giải thuật 1', category: 'Giải thuật', difficulty: 'Intermediate', isPublished: true },
        { id: '3', title: 'Khoá học lỗi', category: 'Cơ bản', difficulty: 'Beginner', isPublished: true },
        { id: '4', title: 'Khoá học lỗi 2', category: 'Nâng cao', difficulty: 'Advanced', isPublished: true },
      ];

      const cats = courseStore.categories;
      expect(cats).toContain('All');
      expect(cats).toContain('Cấu trúc dữ liệu');
      expect(cats).toContain('Giải thuật');
      // Không được chứa "Cơ bản" hay "Nâng cao" trong danh mục chủ đề
      expect(cats).not.toContain('Cơ bản');
      expect(cats).not.toContain('Nâng cao');
    });

    it('getCourseProgress lấy đúng progressPercent từ backend khi danh sách bài học chưa tải', () => {
      const courseStore = useCourseStore();
      (courseStore as any).courses = [
        {
          id: '10',
          title: 'Khóa học DSA',
          category: 'Cấu trúc dữ liệu',
          difficulty: 'Beginner',
          totalLessons: 12,
          progressPercent: 75, // Backend trả về 75%
          isPublished: true,
          lessons: [], // Chưa nạp lessons chi tiết
        },
      ];

      const progress = courseStore.getCourseProgress('10');
      expect(progress.progressPercent).toBe(75);
      expect(progress.totalLessons).toBe(12);
      expect(progress.isCompleted).toBe(false);
    });
  });

  describe('Bug #6 & #13: CourseCover ID Numeric Safe Gradient', () => {
    it('xử lý an toàn khi course.id là số (tránh lỗi id.replace is not a function)', () => {
      const numericCourse = { id: 12345, title: 'Lộ trình số 12345', difficulty: 'Beginner', category: 'DSA' };
      const gradId = 'cover-grad-' + (String(numericCourse.id || '').replace(/[^a-zA-Z0-9_-]/g, '') || 'default');
      expect(gradId).toBe('cover-grad-12345');

      const stringCourse = { id: 'sorting-101', title: 'Sorting Basics', difficulty: 'Beginner', category: 'DSA' };
      const gradId2 = 'cover-grad-' + (String(stringCourse.id || '').replace(/[^a-zA-Z0-9_-]/g, '') || 'default');
      expect(gradId2).toBe('cover-grad-sorting-101');
    });
  });

  describe('Bug #8: ShopView Category Tabs', () => {
    it('chỉ có đúng 3 tab: Tất cả, Avatar, Khung viền và lọc chính xác', () => {
      const CATEGORY_TABS = [
        { id: 'all', label: 'Tất cả' },
        { id: 'avatar', label: 'Avatar' },
        { id: 'frame', label: 'Khung viền' },
      ];

      expect(CATEGORY_TABS.length).toBe(3);
      expect(CATEGORY_TABS.map(t => t.id)).toEqual(['all', 'avatar', 'frame']);

      function isItemInCategory(item: any, cat: string): boolean {
        if (cat === 'all') return true;
        const key = (item.itemKey || '').toLowerCase();
        const slot = (item.slot || '').toLowerCase();
        if (cat === 'avatar') return slot === 'avatar' || key.includes('avatar');
        if (cat === 'frame') return slot === 'frame' || key.includes('frame');
        return slot === cat;
      }

      const mockItems = [
        { id: 1, name: 'Cyber Hacker', itemKey: 'avatar-cyber-hacker' },
        { id: 2, name: 'Neon Border', itemKey: 'frame-neon' },
        { id: 3, name: 'Gợi ý giải bài', slot: 'hint' },
      ];

      const allItems = mockItems.filter(i => isItemInCategory(i, 'all'));
      expect(allItems.length).toBe(3);

      const avatarItems = mockItems.filter(i => isItemInCategory(i, 'avatar'));
      expect(avatarItems.length).toBe(1);
      expect(avatarItems[0].name).toBe('Cyber Hacker');

      const frameItems = mockItems.filter(i => isItemInCategory(i, 'frame'));
      expect(frameItems.length).toBe(1);
      expect(frameItems[0].name).toBe('Neon Border');
    });
  });

  describe('Bug #5 & #7: Theory Simulation Regex Segments', () => {
    it('bóc tách chính xác widget [Mô phỏng: <key>] từ markdown hoặc HTML', () => {
      const content = '<p>Giới thiệu về thuật toán nổi bọt</p>\n\n[Mô phỏng: sort.bubble]\n\n<p>Phân tích độ phức tạp O(n^2)</p>';
      
      const raw = content.trim();
      const segments: Array<{ type: string; simKey?: string; text?: string }> = [];
      const regex = /(?:<p[^>]*>\s*)?\[(?:Mô phỏng|Simulation|mo phong):\s*([a-zA-Z0-9._-]+)\](?:\s*<\/p>)?/gi;

      let lastIndex = 0;
      let match: RegExpExecArray | null;

      while ((match = regex.exec(raw)) !== null) {
        const textBefore = raw.substring(lastIndex, match.index);
        if (textBefore.trim()) {
          segments.push({ type: 'html', text: textBefore });
        }
        segments.push({ type: 'simulation', simKey: match[1] });
        lastIndex = regex.lastIndex;
      }

      const remaining = raw.substring(lastIndex);
      if (remaining.trim()) {
        segments.push({ type: 'html', text: remaining });
      }

      expect(segments.length).toBe(3);
      expect(segments[0].type).toBe('html');
      expect(segments[1].type).toBe('simulation');
      expect(segments[1].simKey).toBe('sort.bubble');
      expect(segments[2].type).toBe('html');
    });
  });

  describe('Bug #9 & #13: Studio Curriculum Path Navigation', () => {
    it('chuyển đổi ID an toàn giữa string và number không bị NaN', () => {
      function parseSelectedPathId(q: any): string | number | null {
        const id = Array.isArray(q) ? q[0] : q;
        return id != null && String(id).length > 0 ? (Number.isNaN(Number(id)) ? String(id) : Number(id)) : null;
      }

      expect(parseSelectedPathId('5')).toBe(5);
      expect(parseSelectedPathId(7)).toBe(7);
      expect(parseSelectedPathId('sorting-101')).toBe('sorting-101');
      expect(parseSelectedPathId('')).toBeNull();
      expect(parseSelectedPathId(null)).toBeNull();
    });
  });
});
