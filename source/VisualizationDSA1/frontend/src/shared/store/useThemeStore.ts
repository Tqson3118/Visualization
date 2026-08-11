import { defineStore } from 'pinia';
import { ref } from 'vue';

export type Theme = 'dark' | 'light';

const LEGACY_DARK = 'terminal-dark';
const STORAGE_KEY = 'app-theme';

export const useThemeStore = defineStore('theme', () => {
  const currentTheme = ref<Theme>('dark');

  function initTheme() {
    const savedTheme = localStorage.getItem(STORAGE_KEY) as Theme | null;
    // Fallback đọc cả key cũ 'terminal-dark' → 'dark' để dữ liệu người dùng cũ vẫn hoạt động
    if (savedTheme === 'dark' || savedTheme === 'light') {
      currentTheme.value = savedTheme;
    } else if (savedTheme === LEGACY_DARK) {
      currentTheme.value = 'dark';
      localStorage.setItem(STORAGE_KEY, 'dark');
    } else {
      // Default to dark theme instead of relying on OS preference
      currentTheme.value = 'dark';
    }
    applyTheme(currentTheme.value);
  }

  function toggleTheme() {
    currentTheme.value = currentTheme.value === 'dark' ? 'light' : 'dark';
    localStorage.setItem(STORAGE_KEY, currentTheme.value);
    applyTheme(currentTheme.value);
  }

  function applyTheme(theme: Theme) {
    document.documentElement.setAttribute('data-theme', theme);
  }

  return {
    currentTheme,
    initTheme,
    toggleTheme,
    applyTheme
  };
});
