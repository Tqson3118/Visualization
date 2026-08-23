import { describe, expect, it } from 'vitest';

import { buildVietQrPayload, getCrc16, getLength, utf8Length } from './vietqr';

// GP-T7: QR chuyển khoản MB Bank — thông tin TK theo pm-decision-log-gp.md (ĐÃ CHỐT)
const MB_BENEFICIARY = {
  bankBin: '970422',
  bankNumber: '83863112088386',
  name: 'NGUYEN THI NHU HOA',
} as const;

describe('lib/vietqr', () => {
  it('getCrc16 khớp vector chuẩn CRC-16/CCITT-FALSE ("123456789" → 29B1)', () => {
    expect(getCrc16('123456789')).toBe('29B1');
  });

  it('getCrc16 ổn định (deterministic) trên payload EMVCo', () => {
    const head = '000201010211520697042253037045405490005802VN5918NGUYEN THI NHU HOA6005HANOI';
    expect(getCrc16(`${head}6304`)).toBe(getCrc16(`${head}6304`));
  });

  it('getLength tính theo BYTE UTF-8 (chữ thường tiếng Việt = 3 byte)', () => {
    expect(getLength('AB')).toBe('02');
    expect(getLength('NGUYEN THI NHU HOA')).toBe('18');
    expect(utf8Length('ạ')).toBe(3); // U+1EA1 = 3 byte UTF-8
    expect(getLength('ạ')).toBe('03');
  });

  it('buildVietQrPayload tạo payload EMVCo hợp lệ (static, MB Bank)', () => {
    const payload = buildVietQrPayload(MB_BENEFICIARY, 49000, 'DSV1002T3');

    expect(payload.startsWith('000201010211')).toBe(true); // format + static
    expect(payload).toContain('5206970422'); // BIN MB Bank 970422
    expect(payload).toContain('5303704'); // VND
    expect(payload).toContain('540549000'); // 49000 — KHÔNG dấu phẩy
    expect(payload).toContain('5802VN');
    expect(payload).toContain('5918NGUYEN THI NHU HOA');
    expect(payload).toContain('6005HANOI');
    // tag 62: 01=QRIBFTTA + 08=nội dung CK DSV1002T3 (đơn giản hóa VietQR — ghi rõ trong vietqr.ts)
    expect(payload).toContain('62250108QRIBFTTA0809DSV1002T3');
  });

  it('CRC cuối khớp CRC tính lại trên toàn bộ payload (kèm 6304)', () => {
    const payload = buildVietQrPayload(MB_BENEFICIARY, 129000, 'DSV1002T3');

    expect(payload).toMatch(/6304[0-9A-F]{4}$/);
    const embedded = payload.slice(-4);
    expect(getCrc16(payload.slice(0, -4))).toBe(embedded);
  });

  it('amount các gói không dấu phẩy: 129000 / 399000', () => {
    expect(buildVietQrPayload(MB_BENEFICIARY, 129000, 'X')).toContain('5406129000');
    expect(buildVietQrPayload(MB_BENEFICIARY, 399000, 'X')).toContain('5406399000');
  });

  it('deterministic — cùng input ra cùng payload (QR ổn định khi tải lại)', () => {
    const a = buildVietQrPayload(MB_BENEFICIARY, 399000, 'DSV7T12');
    const b = buildVietQrPayload(MB_BENEFICIARY, 399000, 'DSV7T12');
    expect(a).toBe(b);
  });
});
