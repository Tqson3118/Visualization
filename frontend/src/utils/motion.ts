/**
 * MOTION — presets animation thống nhất toàn app (UI-PREMIUM 0C).
 * Dùng cho motion-v `<Motion>` + inline styles. Mọi enter/exit animation
 * trong app phải tham chiếu preset này (hoặc ease/duration token từ CSS)
 * — tránh animation cơ giới lặp lại (DESIGN.md §7.10).
 *
 * Quy tắc: chỉ transform + opacity; enter = ease-out-expo, exit = expo-in;
 * tôn trọng prefers-reduced-motion (kiểm tra tại chỗ dùng).
 */
export const MOTION = {
  /** fade + rise nhẹ — reveal section, stat, card */
  fadeUp: {
    initial: { opacity: 0, y: 20 },
    enter: { opacity: 1, y: 0 },
    transition: { duration: 500, ease: 'easeOut' as const },
  },
  /** fade + hạ xuống — header, kicker */
  fadeDown: {
    initial: { opacity: 0, y: -12 },
    enter: { opacity: 1, y: 0 },
    transition: { duration: 400, ease: 'easeOut' as const },
  },
  /** scale nhẹ — card, question, modal content */
  scaleIn: {
    initial: { opacity: 0, scale: 0.95 },
    enter: { opacity: 1, scale: 1 },
    transition: { duration: 350, ease: 'easeOut' as const },
  },
  /** slide từ trái — panel, timeline, list item */
  slideRight: {
    initial: { opacity: 0, x: -16 },
    enter: { opacity: 1, x: 0 },
    transition: { duration: 400, ease: 'easeOut' as const },
  },
  /** delay stagger: MOTION.stagger(i) → transition.delay */
  stagger: (index: number, base = 80) => ({ delay: index * base }),
} as const;
