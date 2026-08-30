import { getCurrentInstance, onMounted, onUnmounted, ref, type MaybeRef } from 'vue';

export type ShortcutMap = Record<string, (event: KeyboardEvent) => void>;

/** Có nên chặn phím tắt khi đang gõ vào ô nhập (tránh nuốt phím) */
function isEditableTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) return false;
  const tag = target.tagName;
  if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || target.isContentEditable) {
    return true;
  }
  if (target.closest('.monaco-editor') || target.closest('[contenteditable="true"]')) {
    return true;
  }
  const activeEl = document.activeElement;
  if (activeEl instanceof HTMLElement) {
    const activeTag = activeEl.tagName;
    if (activeTag === 'INPUT' || activeTag === 'TEXTAREA' || activeTag === 'SELECT' || activeEl.isContentEditable) {
      return true;
    }
    if (activeEl.closest('.monaco-editor') || activeEl.closest('[contenteditable="true"]')) {
      return true;
    }
  }
  return false;
}

/**
 * useKeyboardShortcuts — SDD §3.6: phím tắt theo focus (Màn 05 — FR-3.5).
 * Map: { 'Space': fn, 'ArrowLeft': fn, ... } — dùng e.key, bỏ qua khi đang gõ vào ô nhập.
 */
export function useKeyboardShortcuts(map: ShortcutMap, enabled: MaybeRef<boolean> = true) {
  const isEnabled = typeof enabled === 'boolean' ? ref(enabled) : enabled;

  function handleKeydown(event: KeyboardEvent): void {
    if (!isEnabled.value) return;
    if (isEditableTarget(event.target)) return;
    const handler = map[event.key];
    if (handler) {
      event.preventDefault();
      handler(event);
    }
  }

  if (getCurrentInstance()) {
    onMounted(() => window.addEventListener('keydown', handleKeydown));
    onUnmounted(() => window.removeEventListener('keydown', handleKeydown));
  }
}
