import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * cn — merge class names (shadcn-vue convention).
 * clsx + tailwind-merge: loại bỏ xung đột class Tailwind khi kết hợp.
 * Phase 1a (G): infra nền tảng cho các component shadcn-vue.
 */
export function cn(...inputs: ClassValue[]): string {
  return twMerge(clsx(inputs));
}
