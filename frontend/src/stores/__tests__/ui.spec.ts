import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it } from 'vitest';
import { useUiStore } from '../ui';

describe('useUiStore — Comprehensive Unit Tests', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    document.documentElement.className = '';
  });

  describe('Theme management', () => {
    it('khởi tạo với theme dark và áp dụng class dark lên <html>', () => {
      const ui = useUiStore();
      expect(ui.theme).toBe('dark');
      expect(document.documentElement.classList.contains('dark')).toBe(true);
      expect(document.documentElement.style.colorScheme).toBe('dark');
    });

    it('toggleTheme() chuyển đổi qua lại giữa dark và light, đồng bộ class và colorScheme', () => {
      const ui = useUiStore();
      expect(ui.theme).toBe('dark');

      ui.toggleTheme();
      expect(ui.theme).toBe('light');
      expect(document.documentElement.classList.contains('light')).toBe(true);
      expect(document.documentElement.classList.contains('dark')).toBe(false);
      expect(document.documentElement.style.colorScheme).toBe('light');

      ui.toggleTheme();
      expect(ui.theme).toBe('dark');
      expect(document.documentElement.classList.contains('dark')).toBe(true);
      expect(document.documentElement.classList.contains('light')).toBe(false);
    });
  });

  describe('Modal management', () => {
    it('openModal và closeModal resolve promise', async () => {
      const ui = useUiStore();
      let resolvedValue: boolean | null = null;

      ui.openModal('confirm-delete', { id: 123 }, (val) => {
        resolvedValue = val;
      });

      expect(ui.modalState.open).toBe(true);
      expect(ui.modalState.kind).toBe('confirm-delete');
      expect(ui.modalState.payload).toEqual({ id: 123 });

      ui.closeModal(true);
      expect(ui.modalState.open).toBe(false);
      expect(ui.modalState.kind).toBeNull();
      expect(resolvedValue).toBe(true);
    });
  });
});
