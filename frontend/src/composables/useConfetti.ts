import type { Options } from 'canvas-confetti';
import confetti from 'canvas-confetti';

/**
 * useConfetti + fireConfetti — confetti toàn cục (Phase 2a G — POLISH).
 *
 * G-F2a: dùng confetti.create() trên 1 canvas FIXED dùng chung (resize:true)
 * — không tạo canvas mới mỗi lần bắn, pointer-events:none để không chặn thao
 * tác. Tôn trọng prefers-reduced-motion (không bắn nếu user giảm chuyển động).
 *
 * fireConfetti(type):
 *   - 'success'      → 2 pháo bên màn hình, vàng + mint (hoàn thành bài nhỏ)
 *   - 'levelup'      → bùng nổ trung tâm vàng + shapes + pháo phụ (lên cấp/premium)
 *   - 'achievement'  → bùng nổ vừa phải màu Aurora teal-cyan-violet (huy hiệu)
 *   - 'node-pass'    → pháo nhỏ mint-teal từ trên xuống (vượt node trên path)
 */

export type ConfettiType = 'success' | 'levelup' | 'achievement' | 'node-pass';

const GOLD_COLORS = ['#ffd700', '#ff8c00', '#ffb347', '#ffeaa7', '#fdcb6e'];
const AURORA_COLORS = ['#2dd4bf', '#22d3ee', '#67e8f9', '#818cf8', '#a78bfa', '#5eead4'];
const MINT_COLORS = ['#5eead4', '#2dd4bf', '#99f6e4', '#14b8a6', '#6ee7b7'];

type ConfettiFn = ReturnType<typeof confetti.create>;

let confettiCanvas: HTMLCanvasElement | null = null;
let confettiInstance: ConfettiFn | null = null;

/** Lấy canvas-confetti instance trên 1 canvas fixed dùng chung (lazy). */
function getConfetti(): ConfettiFn | null {
  if (typeof window === 'undefined') return null;
  if (!confettiCanvas) {
    confettiCanvas = document.createElement('canvas');
    const style = confettiCanvas.style;
    style.position = 'fixed';
    style.inset = '0';
    style.width = '100%';
    style.height = '100%';
    style.pointerEvents = 'none';
    style.zIndex = '9999'; // dưới Toaster (sonner ~ 999999999) — toast vẫn click được
    document.body.appendChild(confettiCanvas);
    confettiInstance = confetti.create(confettiCanvas, { resize: true });
  }
  return confettiInstance;
}

/** prefers-reduced-motion: bật → bỏ animation (0 giật, tôn trọng user). */
function prefersReducedMotion(): boolean {
  return (
    typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-reduced-motion: reduce)').matches
  );
}

/** Hai pháo nhỏ từ 2 bên màn hình. */
function fireSideCannons(shoot: ConfettiFn, colors: string[], count = 55): void {
  shoot({
    particleCount: count,
    angle: 60,
    spread: 60,
    startVelocity: 45,
    origin: { x: 0, y: 0.7 },
    colors,
  });
  shoot({
    particleCount: count,
    angle: 120,
    spread: 60,
    startVelocity: 45,
    origin: { x: 1, y: 0.7 },
    colors,
  });
}

/** Bắn confetti theo loại sự kiện (module-level — dùng được ở mọi nơi). */
export function fireConfetti(type: ConfettiType): void {
  const shoot = getConfetti();
  if (!shoot || prefersReducedMotion()) return;

  switch (type) {
    case 'success': {
      fireSideCannons(shoot, [...GOLD_COLORS, ...MINT_COLORS]);
      break;
    }
    case 'levelup': {
      shoot({
        particleCount: 140,
        spread: 90,
        startVelocity: 42,
        origin: { y: 0.5 },
        colors: [...GOLD_COLORS, '#ffffff'],
        shapes: ['circle', 'square'],
        scalar: 1.1,
      });
      window.setTimeout(() => {
        fireSideCannons(shoot, GOLD_COLORS, 45);
      }, 400);
      break;
    }
    case 'achievement': {
      shoot({
        particleCount: 100,
        spread: 75,
        startVelocity: 38,
        origin: { y: 0.55 },
        colors: AURORA_COLORS,
        shapes: ['star', 'circle'],
        scalar: 1.05,
      });
      window.setTimeout(() => {
        shoot({
          particleCount: 40,
          spread: 50,
          startVelocity: 30,
          origin: { y: 0.45 },
          colors: AURORA_COLORS,
          shapes: ['star'],
        });
      }, 250);
      break;
    }
    case 'node-pass': {
      shoot({
        particleCount: 70,
        spread: 65,
        startVelocity: 28,
        gravity: 0.9,
        ticks: 220,
        origin: { y: 0.35 },
        colors: MINT_COLORS,
      });
      break;
    }
  }
}

/** Compatibility: giữ nguyên API useConfetti() cũ (G-F1) — delegate sang fireConfetti. */
export function useConfetti() {
  return {
    fireSuccess: (): void => fireConfetti('success'),
    fireQuizPass: (): void => fireConfetti('achievement'),
    firePremium: (): void => fireConfetti('levelup'),
  };
}

export default fireConfetti;
