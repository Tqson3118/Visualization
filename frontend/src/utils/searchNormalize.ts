/**
 * Chuẩn hóa chuỗi tiếng Việt phục vụ tìm kiếm không dấu, không phân biệt hoa/thường.
 * Chuyển chữ hoa thành thường, loại bỏ toàn bộ dấu thanh/dấu mũ NFD, thay đ/Đ thành d.
 */
export function normalizeVi(text: string | null | undefined): string {
  if (!text) return '';
  return text
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .replace(/đ/g, 'd')
    .replace(/Đ/g, 'd')
    .trim();
}
