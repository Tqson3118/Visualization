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

/** 1.234.567 — an toàn với null, undefined, NaN, Infinity */
export function formatNumber(value: number | null | undefined): string {
  if (
    value === null ||
    value === undefined ||
    typeof value !== 'number' ||
    !Number.isFinite(value)
  ) {
    return '0';
  }
  return numberFormat.format(value);
}

export function parseDateSafely(value: Date | string | number): Date {
  if (value instanceof Date) return value;
  if (typeof value === 'string') {
    return new Date(value.trim());
  }
  return new Date(value);
}

/** 12/08/2026 — chấp nhận Date | ISO string | timestamp; trả '--/--/----' nếu không hợp lệ */
export function formatDate(value: Date | string | number): string {
  if (value === null || value === undefined) return '--/--/----';
  const d = parseDateSafely(value);
  if (Number.isNaN(d.getTime())) return '--/--/----';
  return dateFormat.format(d);
}

/** 12/08/2026, 14:30; trả '--/--/----, --:--' nếu không hợp lệ */
export function formatDateTime(value: Date | string | number): string {
  if (value === null || value === undefined) return '--/--/----, --:--';
  const d = parseDateSafely(value);
  if (Number.isNaN(d.getTime())) return '--/--/----, --:--';
  return dateTimeFormat.format(d);
}

/** Phân số 0..1 → "66%" (không dùng số thập phân 0-100); an toàn với NaN/Infinity/null */
export function formatPercent(fraction: number): string {
  if (
    fraction === null ||
    fraction === undefined ||
    typeof fraction !== 'number' ||
    !Number.isFinite(fraction)
  ) {
    return '0%';
  }
  return percentFormat.format(fraction);
}

/** mm:ss (VD: 90 → "1:30"); an toàn với số âm, NaN, Infinity */
export function formatDuration(totalSeconds: number): string {
  if (
    totalSeconds === null ||
    totalSeconds === undefined ||
    typeof totalSeconds !== 'number' ||
    !Number.isFinite(totalSeconds) ||
    totalSeconds <= 0
  ) {
    return '0:00';
  }
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = Math.floor(totalSeconds % 60);
  return `${minutes}:${String(seconds).padStart(2, '0')}`;
}
