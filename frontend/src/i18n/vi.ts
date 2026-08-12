/**
 * Mọi chuỗi giao diện — SDD §3.8.5: không nhúng chuỗi cứng ngoài file này.
 * MVP chỉ tiếng Việt (SDD §3.1).
 */
export const messages = {
  app: {
    name: 'DSA Visual',
    tagline: 'Học Cấu trúc dữ liệu & Giải thuật trực quan',
  },

  nav: {
    home: 'Trang chủ',
    login: 'Đăng nhập',
    register: 'Đăng ký',
    logout: 'Đăng xuất',
    path: 'Lộ trình',
    simulations: 'Mô phỏng',
    profile: 'Hồ sơ',
    admin: 'Quản trị',
  },

  common: {
    loading: 'Đang tải...',
    save: 'Lưu',
    cancel: 'Hủy',
    confirm: 'Xác nhận',
    close: 'Đóng',
    retry: 'Thử lại',
    back: 'Quay lại',
    next: 'Tiếp theo',
    notFound: 'Không tìm thấy',
    error: 'Có lỗi xảy ra',
    success: 'Thành công',
    comingSoon: 'Tính năng đang được xây dựng',
    processing: 'Đang xử lý...',
  },

  home: {
    heroTitle: 'Trực quan hóa mọi giải thuật, từng bước một',
    heroSubtitle:
      'Xem mã chạy thật, quan sát cấu trúc dữ liệu biến đổi từng thao tác, luyện tập theo lộ trình cá nhân.',
    ctaExplore: 'Khám phá mô phỏng',
    ctaStart: 'Bắt đầu học',
    featureVisual: {
      title: 'Mô phỏng từng bước',
      desc: 'Mọi thuật toán chạy thật trong trình duyệt — trực quan khớp 100% với code.',
    },
    featurePath: {
      title: 'Lộ trình cá nhân',
      desc: 'Học theo lộ trình từ cơ bản đến nâng cao, theo dõi tiến độ mỗi chủ đề.',
    },
    featurePractice: {
      title: 'Luyện tập & chấm điểm',
      desc: 'Trắc nghiệm, thực hành thao tác và viết code — chấm điểm tự động ngay lập tức.',
    },
    // G-F2b — Màn 01: section demo công khai (FR-7.6) + stats
    demoBadge: 'Demo công khai',
    demoTabTitle: 'Thử ngay 3 mô phỏng',
    demoTabDesc: 'Chạy thật ngay trong trình duyệt — không cần đăng ký, không trừ tim.',
    demoRun: 'Chạy thử',
    demoComplexity: 'Độ phức tạp',
    demoOpen: 'Mở mô phỏng',
    statsTitle: 'DSA Visual bằng số',
    statsVisuals: 'mô phỏng trực quan',
    statsGroups: 'nhóm CTDL & Giải thuật',
    statsLevels: 'cấp độ từ cơ bản đến nâng cao',
    statsNote: 'Số liệu từ danh mục nội dung — cập nhật theo từng phiên bản.',
  },

  auth: {
    loginTitle: 'Đăng nhập',
    loginSubtitle: 'Chào mừng trở lại — tiếp tục lộ trình học của bạn.',
    registerTitle: 'Đăng ký tài khoản',
    email: 'Email',
    emailPlaceholder: 'ban@truong.edu.vn',
    password: 'Mật khẩu',
    passwordPlaceholder: 'Nhập mật khẩu',
    displayName: 'Họ tên',
    loginSubmit: 'Đăng nhập',
    registerSubmit: 'Đăng ký',
    noAccount: 'Chưa có tài khoản?',
    hasAccount: 'Đã có tài khoản?',
    toRegister: 'Đăng ký ngay',
    toLogin: 'Đăng nhập ngay',
    forgotPassword: 'Quên mật khẩu?',
    loginFailed: 'Đăng nhập thất bại. Vui lòng kiểm tra lại.',
    invalidEmail: 'Email không hợp lệ',
    passwordRequirement:
      'Mật khẩu phải từ 8-64 ký tự, gồm chữ hoa, chữ thường, số và ký tự đặc biệt',
    // G-F2b — Màn 02: split layout (brand panel)
    brandTagline: 'Học Cấu trúc dữ liệu & Giải thuật trực quan',
    brandPoint1: 'Trực quan từng bước chạy của thuật toán',
    brandPoint2: 'Lộ trình cá nhân từ cơ bản đến nâng cao',
    brandPoint3: 'Trắc nghiệm, thực hành và code — chấm điểm tức thì',
  },

  simulator: {
    play: 'Chạy',
    pause: 'Tạm dừng',
    stepForward: 'Bước tới',
    stepBack: 'Bước lùi',
    reset: 'Đặt lại',
    speed: 'Tốc độ',
    explanation: 'Giải thích',
    variables: 'Biến',
    stats: 'Thống kê',
    stepOf: 'Bước {current}/{total}',
    inputConfig: 'Cấu hình đầu vào',
    dataTooLarge: 'Dữ liệu lớn, mô phỏng có thể chậm',
    controlsTitle: 'Điều khiển mô phỏng',
    canvasPlaceholder: 'Khu vực vẽ cấu trúc dữ liệu — renderer sẽ được gắn ở task renderer (SDD §4.4)',
    notFound: 'Không tìm thấy mô phỏng với key này',
    simError: 'Không thể nạp mô phỏng',
    // Nhãn loại bước trace — SDD §4.0.3 (TraceKind)
    kinds: {
      declare: 'Khai báo',
      assign: 'Gán',
      compare: 'So sánh',
      swap: 'Hoán đổi',
      loop: 'Vòng lặp',
      call: 'Gọi hàm',
      return: 'Trả về',
    } as const,
  },

  bottomsheet: {
    dragToClose: 'Kéo để đóng',
  },

  toast: {
    serverError: 'Đã có lỗi xảy ra, vui lòng thử lại',
    networkError: 'Không kết nối được máy chủ. Kiểm tra mạng của bạn.',
    rateLimited: (seconds: number) =>
      `Bạn đang thao tác quá nhanh, vui lòng thử lại sau ${seconds} giây`,
    rateLimitedUnknown: 'Bạn đang thao tác quá nhanh, vui lòng thử lại sau ít phút',
    sessionExpired: 'Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại',
  },

  notFound: {
    title: 'Trang không tồn tại',
    desc: 'Địa chỉ bạn truy cập không có hoặc đã bị di chuyển.',
    backHome: 'Về trang chủ',
  },

  placeholder: {
    title: 'Đang phát triển',
    desc: 'Trang này sẽ được triển khai trong các bước tiếp theo.',
  },
} as const;

export type MessageKey = keyof typeof messages;
