import { flushPromises, mount } from '@vue/test-utils';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { messages } from '@/i18n/vi';
import * as authApi from '@/api/auth';

// ── Task L / B0 — form đăng ký & OTP: mock store auth + router + authApi ──
const { registerMock, replaceMock } = vi.hoisted(() => ({
  registerMock: vi.fn(),
  replaceMock: vi.fn(),
}));

vi.mock('@/stores/auth', () => ({
  useAuthStore: () => ({ register: registerMock }),
}));

vi.mock('@/api/auth', () => ({
  sendRegisterOtp: vi.fn(),
  verifyRegisterOtp: vi.fn(),
  register: vi.fn(),
}));

vi.mock('vue-router', () => ({
  useRouter: () => ({ replace: replaceMock }),
  RouterLink: { template: '<a><slot /></a>' },
}));

import RegisterView from './RegisterView.vue';

const validPassword = 'MatKhau@123';

async function fillStudentFields(wrapper: ReturnType<typeof mount>): Promise<void> {
  await wrapper.find(`input[placeholder="${messages.auth.displayNamePlaceholder}"]`).setValue('Nguyễn Minh');
  await wrapper.find(`input[placeholder="${messages.auth.emailPlaceholder}"]`).setValue('Minh@university.edu.vn');
  const passwordInputs = wrapper.findAll('input[type="password"]');
  await passwordInputs[0].setValue(validPassword); // password
  await passwordInputs[1].setValue(validPassword); // confirmPassword
  await wrapper.find('input[type="checkbox"]').setValue(true);
}

async function selectRole(wrapper: ReturnType<typeof mount>, label: string): Promise<void> {
  const button = wrapper.findAll('button.register__role-option').find((b) => b.text().includes(label));
  expect(button, `role button "${label}"`).toBeTruthy();
  await button!.trigger('click');
}

function teacherInputs(wrapper: ReturnType<typeof mount>) {
  return {
    department: wrapper.find(`input[placeholder="${messages.auth.departmentPlaceholder}"]`),
    staffCode: wrapper.find(`input[placeholder="${messages.auth.staffCodePlaceholder}"]`),
    bio: wrapper.find('#register-teacher-bio'),
  };
}

async function enterOtpDigits(wrapper: ReturnType<typeof mount>, code = '123456'): Promise<void> {
  const digits = code.split('');
  for (let i = 0; i < digits.length; i++) {
    const input = wrapper.find(`#otp-digit-${i}`);
    if (input.exists()) {
      await input.setValue(digits[i]);
      await input.trigger('input');
    }
  }
}

describe('RegisterView — form đăng ký giảng viên & OTP (Task B0, A2, A3)', () => {
  beforeEach(() => {
    registerMock.mockReset();
    replaceMock.mockReset();
    vi.mocked(authApi.sendRegisterOtp).mockReset();
    vi.mocked(authApi.verifyRegisterOtp).mockReset();

    vi.mocked(authApi.sendRegisterOtp).mockResolvedValue({
      message: 'OTP đã gửi',
      expiresInSeconds: 300,
    });
    vi.mocked(authApi.verifyRegisterOtp).mockResolvedValue({
      otpToken: 'mock_otp_token_123',
      expiresInSeconds: 300,
      message: 'OTP hợp lệ',
    });
    registerMock.mockResolvedValue(undefined);
  });

  it('mặc định chọn "Sinh viên" → form con giảng viên không hiện', () => {
    const wrapper = mount(RegisterView);

    expect(wrapper.find('.register__teacher').exists()).toBe(false);
    expect(wrapper.find('#register-teacher-bio').exists()).toBe(false);
    const buttons = wrapper.findAll('button.register__role-option');
    expect(buttons[0].attributes('aria-pressed')).toBe('true'); // Sinh viên
    expect(buttons[1].attributes('aria-pressed')).toBe('false'); // Giảng viên
  });

  it('chọn "Giảng viên" → form con hiện (Khoa/Bộ môn, Mã GV, textarea)', async () => {
    const wrapper = mount(RegisterView);

    await selectRole(wrapper, 'Giảng viên');

    const teacher = wrapper.find('.register__teacher');
    expect(teacher.exists()).toBe(true);
    expect(
      wrapper.find(`input[placeholder="${messages.auth.departmentPlaceholder}"]`).exists(),
    ).toBe(true);
    expect(
      wrapper.find(`input[placeholder="${messages.auth.staffCodePlaceholder}"]`).exists(),
    ).toBe(true);
    expect(wrapper.find('#register-teacher-bio').exists()).toBe(true);
    // Chuyển lại Sinh viên → form con ẩn
    await selectRole(wrapper, 'Sinh viên');
    expect(wrapper.find('.register__teacher').exists()).toBe(false);
  });

  it('submit khi chọn Giảng viên thiếu StaffCode → fieldErrors hiện, KHÔNG gọi API', async () => {
    const wrapper = mount(RegisterView);
    await selectRole(wrapper, 'Giảng viên');

    // Blur StaffCode trống để touched=true → hiện lỗi inline
    await wrapper.find(`input[placeholder="${messages.auth.staffCodePlaceholder}"]`).trigger('blur');

    expect(wrapper.text()).toContain(messages.auth.staffCodeRequired);

    // Điền đầy đủ các field khác rồi submit — vẫn phải chặn ở validation GV
    await fillStudentFields(wrapper);
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(authApi.sendRegisterOtp).not.toHaveBeenCalled();
    expect(registerMock).not.toHaveBeenCalled();
  });

  it('submit khi chưa blur field nào → lỗi inline hiện ngay tại field thiếu, KHÔNG gọi API', async () => {
    const wrapper = mount(RegisterView);
    await selectRole(wrapper, 'Giảng viên');

    // Không blur field nào — bấm submit ngay: lỗi inline phải hiện
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(wrapper.text()).toContain(messages.auth.staffCodeRequired);
    expect(authApi.sendRegisterOtp).not.toHaveBeenCalled();
    expect(registerMock).not.toHaveBeenCalled();
  });

  it('submit đủ thông tin giảng viên → gửi OTP -> verify OTP -> gọi auth.register với otpToken và isTeacher=true, không redirect', async () => {
    const wrapper = mount(RegisterView);
    await selectRole(wrapper, 'Giảng viên');

    await fillStudentFields(wrapper);
    const { department, staffCode, bio } = teacherInputs(wrapper);
    await department.setValue('  Khoa Công nghệ thông tin  ');
    await staffCode.setValue('  GV001  ');
    await bio.setValue('  10 năm giảng dạy  ');

    // Bước 1: Tiếp tục
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(authApi.sendRegisterOtp).toHaveBeenCalledWith('minh@university.edu.vn');
    expect(wrapper.text()).toContain('Xác thực Email');

    // Bước 2: Nhập OTP
    await enterOtpDigits(wrapper, '123456');
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(authApi.verifyRegisterOtp).toHaveBeenCalledWith('minh@university.edu.vn', '123456');
    expect(registerMock).toHaveBeenCalledTimes(1);
    expect(registerMock).toHaveBeenCalledWith({
      displayName: 'Nguyễn Minh',
      email: 'minh@university.edu.vn',
      password: validPassword,
      isTeacher: true,
      department: 'Khoa Công nghệ thông tin',
      staffCode: 'GV001',
      teacherBio: '10 năm giảng dạy',
      otpToken: 'mock_otp_token_123',
    });
    expect(replaceMock).not.toHaveBeenCalled();
    expect(wrapper.text()).toContain(messages.auth.teacherPendingSuccess);
  });

  it('submit vai trò Sinh viên → gửi OTP -> verify OTP -> auth.register -> redirect /courses', async () => {
    const wrapper = mount(RegisterView);
    await fillStudentFields(wrapper);

    // Bước 1
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(authApi.sendRegisterOtp).toHaveBeenCalledWith('minh@university.edu.vn');

    // Bước 2
    await enterOtpDigits(wrapper, '123456');
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(registerMock).toHaveBeenCalledTimes(1);
    const payload = registerMock.mock.calls[0][0] as Record<string, unknown>;
    expect(payload.isTeacher).toBe(false);
    expect(payload.otpToken).toBe('mock_otp_token_123');
    expect(payload).not.toHaveProperty('department');
    expect(payload).not.toHaveProperty('staffCode');
    expect(payload).not.toHaveProperty('teacherBio');
    expect(payload).not.toHaveProperty('academicDegree');
    expect(payload).not.toHaveProperty('profileLink');
    expect(replaceMock).toHaveBeenCalledWith({ name: 'courses' });
  });

  // ── Block 2.3 — học vị + link hồ sơ giảng viên ──

  it('submit teacher với profileLink không hợp lệ → lỗi inline hiện, KHÔNG gọi API', async () => {
    const wrapper = mount(RegisterView);
    await selectRole(wrapper, 'Giảng viên');

    await fillStudentFields(wrapper);
    await wrapper.find(`input[placeholder="${messages.auth.departmentPlaceholder}"]`).setValue('Khoa CNTT');
    await wrapper.find(`input[placeholder="${messages.auth.staffCodePlaceholder}"]`).setValue('GV001');
    await wrapper.find(`input[placeholder="${messages.auth.profileLinkPlaceholder}"]`).setValue('linkedin.com/in/abc');

    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(wrapper.text()).toContain(messages.auth.profileLinkInvalid);
    expect(authApi.sendRegisterOtp).not.toHaveBeenCalled();
    expect(registerMock).not.toHaveBeenCalled();
  });

  it('submit teacher với học vị + profileLink hợp lệ → payload gửi kèm cả hai (đã trim)', async () => {
    const wrapper = mount(RegisterView);
    await selectRole(wrapper, 'Giảng viên');

    await fillStudentFields(wrapper);
    await wrapper.find(`input[placeholder="${messages.auth.departmentPlaceholder}"]`).setValue('Khoa CNTT');
    await wrapper.find(`input[placeholder="${messages.auth.staffCodePlaceholder}"]`).setValue('GV001');
    await wrapper.find('#register-academic-degree').setValue('Thạc sĩ');
    await wrapper.find(`input[placeholder="${messages.auth.profileLinkPlaceholder}"]`).setValue(' https://www.linkedin.com/in/abc ');
    await wrapper.find('#register-teacher-bio').setValue('10 năm giảng dạy');

    await wrapper.find('form').trigger('submit');
    await flushPromises();

    await enterOtpDigits(wrapper, '123456');
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(registerMock).toHaveBeenCalledTimes(1);
    expect(registerMock).toHaveBeenCalledWith({
      displayName: 'Nguyễn Minh',
      email: 'minh@university.edu.vn',
      password: validPassword,
      isTeacher: true,
      department: 'Khoa CNTT',
      staffCode: 'GV001',
      academicDegree: 'Thạc sĩ',
      profileLink: 'https://www.linkedin.com/in/abc',
      teacherBio: '10 năm giảng dạy',
      otpToken: 'mock_otp_token_123',
    });
    expect(replaceMock).not.toHaveBeenCalled();
  });

  it('submit teacher KHÔNG điền học vị/link → payload không chứa 2 field mới', async () => {
    const wrapper = mount(RegisterView);
    await selectRole(wrapper, 'Giảng viên');

    await fillStudentFields(wrapper);
    await wrapper.find(`input[placeholder="${messages.auth.departmentPlaceholder}"]`).setValue('Khoa CNTT');
    await wrapper.find(`input[placeholder="${messages.auth.staffCodePlaceholder}"]`).setValue('GV001');

    await wrapper.find('form').trigger('submit');
    await flushPromises();

    await enterOtpDigits(wrapper, '123456');
    await wrapper.find('form').trigger('submit');
    await flushPromises();

    expect(registerMock).toHaveBeenCalledTimes(1);
    const payload = registerMock.mock.calls[0][0] as Record<string, unknown>;
    expect(payload).not.toHaveProperty('academicDegree');
    expect(payload).not.toHaveProperty('profileLink');
  });
});

