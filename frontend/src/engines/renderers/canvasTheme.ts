// engines/renderers/canvasTheme.ts — màu/layout cho CanvasPainter (SDD §4.4, §8.3)
//
// NGUỒN BÊ: source/VisualizationDSA3/frontend/src/core/renderers/canvasTheme.ts (V3).
// Chọn V3 thay vì V1: V3 đọc CSS variables với fallback + refreshCanvasColors()
// (đổi theme không cần reload); V1 hardcode màu. Bê NGUYÊN VẸN.

function getCssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

function initColors() {
  return {
    default:  getCssVar('--color-accent-primary', '#4255ff'),
    compare:  getCssVar('--color-accent-primary-light', '#6b7bff'),
    swap:     getCssVar('--color-accent-red', '#f87171'),
    sorted:   getCssVar('--color-accent-green', '#34d399'),
    text:     getCssVar('--color-text-primary', '#d9dde8'),
    muted:    getCssVar('--color-text-muted', '#6b7385'),
    bgDark:   getCssVar('--canvas-bg', '#0d1020'),
    gridDark: getCssVar('--canvas-grid-color', 'rgba(66, 85, 255, 0.04)'),
  };
}

export const CANVAS_COLORS = initColors();

export function refreshCanvasColors() {
  const c = initColors();
  Object.assign(CANVAS_COLORS, c);
}

export const CANVAS_LAYOUT = {
  margin: 30,
  marginBottom: 100,
  paddingTop: 45,
  borderRadius: 6,
  rowGap: 40,
};

export function hexToRgba(colorStr: string, alpha: number): string {
  if (!colorStr) return `rgba(0, 0, 0, ${alpha})`;
  const str = colorStr.trim();
  if (str.startsWith('rgb')) {
    const match = str.match(/\d+(\.\d+)?/g);
    if (match && match.length >= 3) {
      return `rgba(${match[0]}, ${match[1]}, ${match[2]}, ${alpha})`;
    }
  }
  const clean = str.replace('#', '');
  if (clean.length === 3) {
    const r = parseInt(clean[0] + clean[0], 16);
    const g = parseInt(clean[1] + clean[1], 16);
    const b = parseInt(clean[2] + clean[2], 16);
    return `rgba(${r}, ${g}, ${b}, ${alpha})`;
  }
  const r = parseInt(clean.substring(0, 2), 16) || 0;
  const g = parseInt(clean.substring(2, 4), 16) || 0;
  const b = parseInt(clean.substring(4, 6), 16) || 0;
  return `rgba(${r}, ${g}, ${b}, ${alpha})`;
}
