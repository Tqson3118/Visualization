/**
 * Validator — API_REFERENCE §3.1 (RegisterRequest):
 * email hợp lệ ≤ 256; password 8-64, chữ hoa + số + ký tự đặc biệt.
 */

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const UPPERCASE_REGEX = /[A-Z]/;
const LOWERCASE_REGEX = /[a-z]/;
const DIGIT_REGEX = /\d/;
const SPECIAL_REGEX = /[^A-Za-z0-9]/;
const URL_REGEX = /^https?:\/\/\S+$/i;

export function isValidEmail(email: string): boolean {
  return email.length <= 256 && EMAIL_REGEX.test(email);
}

/** URL hợp lệ (http/https, không chứa khoảng trắng) — RegisterRequest.profileLink */
export function isValidUrl(url: string): boolean {
  return url.length <= 2048 && URL_REGEX.test(url);
}

export interface PasswordRule {
  ok: boolean;
  errors: string[];
}

/** Mật khẩu mạnh: 8-64 ký tự, chữ hoa + chữ thường + số + ký tự đặc biệt */
export function validatePassword(password: string): PasswordRule {
  const errors: string[] = [];
  if (password.length < 8 || password.length > 64) {
    errors.push('password_length');
  }
  if (!UPPERCASE_REGEX.test(password)) errors.push('password_uppercase');
  if (!LOWERCASE_REGEX.test(password)) errors.push('password_lowercase');
  if (!DIGIT_REGEX.test(password)) errors.push('password_digit');
  if (!SPECIAL_REGEX.test(password)) errors.push('password_special');
  return { ok: errors.length === 0, errors };
}

export function isStrongPassword(password: string): boolean {
  return validatePassword(password).ok;
}
