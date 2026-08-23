/**
 * vietqr.ts — Sinh payload EMVCo VietQR (GP-T7).
 *
 * Build payload theo chuẩn EMVCo Merchant-Presented QR (QR tiêu chuẩn VietQR/NAPAS):
 * - `00` Payload Format Indicator = '01'
 * - `01` Point of Initiation = '11' STATIC — lý do: demo không có backend ngân hàng cấp
 *       URL thanh toán cập nhật được; số tiền đã biết tại thời điểm sinh (theo gói) nên
 *       QR static là đủ. Dynamic ('12') chỉ cần khi phải thay đổi số tiền/trạng thái
 *       sau khi in QR — ngoài phạm vi demo.
 * - `52` BIN ngân hàng (970422 = MB Bank) · `53` '704' (VND) · `54` số tiền (KHÔNG dấu phẩy)
 * - `58` 'VN' · `59` tên chủ TK · `60` thành phố (tùy chọn)
 * - `62` merchant account info — ĐƠN GIẢN HÓÁ: chứa sub-tag `01`='QRIBFTTA' (service code
 *       chuyển khoản NAPAS) + sub-tag `08`=nội dung CK (các app ngân hàng VN hiển thị và
 *       điền sẵn lời nhắn; EMVCo định nghĩa 08 là Terminal Label nhưng NAPAS/ngân hàng VN
 *       dùng như lời nhắn). Bỏ GUID `26`/`51` (A000000727) — QR vẫn quét được vì app ngân
 *       hàng nhận diện qua BIN + số TK + QRIBFTTA.
 * - `63` '04' + CRC16-CCITT (poly 0x1021, init 0xFFFF, không reflect, không xorout) trên
 *       toàn bộ payload (bao gồm chuỗi "6304").
 *
 * KHÔNG gọi API ngân hàng / vietqr.io online — payload tự sinh, `qrcode` (npm, MIT) chỉ
 * render ảnh QR từ chuỗi này.
 */

export interface VietQrBeneficiary {
  bankBin: string; // VD '970422' (MB Bank)
  bankNumber: string; // VD '83863112088386'
  name: string; // VD 'NGUYEN THI NHU HOA'
}

const CCITT_POLY = 0x1021;

/** Mã hóa chuỗi thành mảng byte UTF-8 (TLV/CRC của EMVCo tính theo BYTE, không phải ký tự). */
function toUtf8Bytes(data: string): number[] {
  const bytes: number[] = [];
  for (let i = 0; i < data.length; i++) {
    let code = data.codePointAt(i) as number;
    if (code > 0xffff) i += 1; // bỏ qua nửa sau của surrogate pair
    if (code <= 0x7f) {
      bytes.push(code);
    } else if (code <= 0x7ff) {
      bytes.push(0xc0 | (code >> 6), 0x80 | (code & 0x3f));
    } else if (code <= 0xffff) {
      bytes.push(0xe0 | (code >> 12), 0x80 | ((code >> 6) & 0x3f), 0x80 | (code & 0x3f));
    } else {
      bytes.push(
        0xf0 | (code >> 18),
        0x80 | ((code >> 12) & 0x3f),
        0x80 | ((code >> 6) & 0x3f),
        0x80 | (code & 0x3f),
      );
    }
  }
  return bytes;
}

/** Số BYTE UTF-8 của chuỗi (độ dài TLV theo chuẩn EMVCo). */
export function utf8Length(data: string): number {
  return toUtf8Bytes(data).length;
}

/** Helper TLV: độ dài 2 chữ số thập phân (pad 0 bên trái). */
export function getLength(data: string): string {
  return String(utf8Length(data)).padStart(2, '0');
}

/**
 * CRC16-CCITT (FALSE): poly 0x1021, init 0xFFFF, không reflect in/out, không xorout —
 * đúng thuật toán VietQR/EMVCo dùng cho tag 63. Vector chuẩn: getCrc16('123456789') === '29B1'.
 */
export function getCrc16(data: string): string {
  let crc = 0xffff;
  for (const byte of toUtf8Bytes(data)) {
    crc ^= byte << 8;
    for (let bit = 0; bit < 8; bit++) {
      crc = crc & 0x8000 ? ((crc << 1) ^ CCITT_POLY) & 0xffff : (crc << 1) & 0xffff;
    }
  }
  return crc.toString(16).toUpperCase().padStart(4, '0');
}

/**
 * Build chuỗi EMVCo VietQR hoàn chỉnh (kèm CRC tag 63).
 * @param amount số tiền VND — VD 49000 (KHÔNG dấu phẩy, không format)
 * @param content nội dung CK tự động DSV{userId}T{months} (VD DSV1002T3)
 */
export function buildVietQrPayload(
  beneficiary: VietQrBeneficiary,
  amount: number,
  content: string,
): string {
  const tlv = (id: string, value: string): string => `${id}${getLength(value)}${value}`;

  const head = [
    tlv('00', '01'), // payload format
    tlv('01', '11'), // static (xem chú thích đầu file)
    tlv('52', beneficiary.bankBin),
    tlv('53', '704'), // VND
    tlv('54', String(amount)), // VD 49000 — không dấu phẩy
    tlv('58', 'VN'), // country
    tlv('59', beneficiary.name), // tên chủ TK
    tlv('60', 'HANOI'), // thành phố (tùy chọn)
    tlv('62', tlv('01', 'QRIBFTTA') + tlv('08', content)), // service code + nội dung CK
  ].join('');

  const crcBlock = `${head}6304`;
  return `${crcBlock}${getCrc16(crcBlock)}`;
}
