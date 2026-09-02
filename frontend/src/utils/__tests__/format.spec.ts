import { describe, expect, it } from 'vitest';
import { formatDate, formatDateTime, formatDuration, formatNumber, formatPercent } from '../format';

describe('format utils — Extreme Boundary & Adversarial Tests', () => {
  describe('formatNumber', () => {
    it('định dạng số thông thường', () => {
      expect(formatNumber(1234567)).toBe('1.234.567');
      expect(formatNumber(0)).toBe('0');
    });

    it('an toàn với null, undefined, NaN', () => {
      expect(formatNumber(null)).toBe('0');
      expect(formatNumber(undefined)).toBe('0');
      expect(formatNumber(NaN)).toBe('0');
    });

    it('an toàn với Infinity và -Infinity', () => {
      expect(formatNumber(Infinity)).toBe('0');
      expect(formatNumber(-Infinity)).toBe('0');
    });
  });

  describe('formatDate', () => {
    it('định dạng Date hợp lệ', () => {
      const d = new Date(2026, 7, 12); // tháng 8 (0-indexed)
      expect(formatDate(d)).toBe('12/08/2026');
    });

    it('định dạng ISO string hợp lệ', () => {
      expect(formatDate('2026-08-12T00:00:00Z')).toContain('2026');
    });

    it('không ném RangeError khi truyền chuỗi ngày không hợp lệ ("invalid-date")', () => {
      expect(() => formatDate('invalid-date')).not.toThrow();
      expect(formatDate('invalid-date')).toBe('--/--/----');
    });

    it('không ném lỗi khi truyền null / undefined / NaN', () => {
      expect(() => formatDate(null as any)).not.toThrow();
      expect(() => formatDate(undefined as any)).not.toThrow();
      expect(() => formatDate(NaN as any)).not.toThrow();
      expect(formatDate(NaN as any)).toBe('--/--/----');
    });
  });

  describe('formatDateTime', () => {
    it('định dạng DateTime hợp lệ', () => {
      const d = new Date(2026, 7, 12, 14, 30);
      const res = formatDateTime(d);
      expect(res).toContain('12/08/2026');
    });

    it('không ném RangeError khi chuỗi ngày hỏng', () => {
      expect(() => formatDateTime('not-a-date')).not.toThrow();
      expect(formatDateTime('not-a-date')).toBe('--/--/----, --:--');
    });

    it('không ném lỗi khi null / undefined / NaN', () => {
      expect(() => formatDateTime(null as any)).not.toThrow();
      expect(() => formatDateTime(undefined as any)).not.toThrow();
      expect(formatDateTime(undefined as any)).toBe('--/--/----, --:--');
    });
  });

  describe('formatPercent', () => {
    it('định dạng fraction 0..1', () => {
      expect(formatPercent(0.66)).toBe('66%');
      expect(formatPercent(1)).toBe('100%');
      expect(formatPercent(0)).toBe('0%');
    });

    it('an toàn với NaN, null, undefined, Infinity', () => {
      expect(formatPercent(NaN)).toBe('0%');
      expect(formatPercent(null as any)).toBe('0%');
      expect(formatPercent(undefined as any)).toBe('0%');
      expect(formatPercent(Infinity)).toBe('0%');
    });
  });

  describe('formatDuration', () => {
    it('định dạng số giây thông thường', () => {
      expect(formatDuration(90)).toBe('1:30');
      expect(formatDuration(0)).toBe('0:00');
      expect(formatDuration(5)).toBe('0:05');
    });

    it('an toàn khi số giây âm (< 0)', () => {
      expect(formatDuration(-10)).toBe('0:00');
    });

    it('an toàn khi số giây là NaN / Infinity / null / undefined', () => {
      expect(formatDuration(NaN)).toBe('0:00');
      expect(formatDuration(Infinity)).toBe('0:00');
      expect(formatDuration(null as any)).toBe('0:00');
      expect(formatDuration(undefined as any)).toBe('0:00');
    });
  });
});
