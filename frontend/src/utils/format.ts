/** Định dạng số/ngày theo locale vi-VN (Intl) — SDD §3.1 utils/format.ts */

const numberFormat = new Intl.NumberFormat('vi-VN');

const dateFormat = new Intl.DateTimeFormat('vi-VN', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
});

const dateTimeFormat = new Intl.DateTimeFormat('vi-VN', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

const percentFormat = new Intl.NumberFormat('vi-VN', {
  style: 'percent',
  maximumFractionDigits: 0,
});

/** 1.234.567 — an toàn với null, undefined, NaN */
export function formatNumber(value: number | null | undefined): string {
  if (value === null || value === undefined || typeof value !== 'number' || Number.isNaN(value)) {
    return '0';
  }
  return numberFormat.format(value);
}

/** 12/08/2026 — chấp nhận Date | ISO string | timestamp */
export function formatDate(value: Date | string | number): string {
  return dateFormat.format(new Date(value));
}

/** 12/08/2026, 14:30 */
export function formatDateTime(value: Date | string | number): string {
  return dateTimeFormat.format(new Date(value));
}

/** Phân số 0..1 → "66%" (không dùng số thập phân 0-100) */
export function formatPercent(fraction: number): string {
  return percentFormat.format(fraction);
}

/** mm:ss (VD: 90 → "1:30") */
export function formatDuration(totalSeconds: number): string {
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = Math.floor(totalSeconds % 60);
  return `${minutes}:${String(seconds).padStart(2, '0')}`;
}
