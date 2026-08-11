export interface TabItem {
  readonly id: string;
  readonly path: string;
  readonly name: string;
  readonly requiresAuth?: boolean;
  readonly requiresRole?: string;
}

export interface TabGroup {
  readonly groupName: string;
  readonly items: readonly TabItem[];
}

export const APP_TABS: readonly (TabGroup | TabItem)[] = [
  {
    groupName: 'Học tập',
    items: [
      { id: 'algorithms',    path: '/algorithms',     name: 'Thư viện thuật toán' },
      { id: 'roadmap',       path: '/courses',        name: 'Lộ trình / Roadmap' },
      { id: 'classrooms',    path: '/classrooms',     name: 'Lớp học của tôi', requiresAuth: true },
      { id: 'ai-assistant',  path: '/ai-assistant',   name: 'AI Assistant', requiresAuth: true },
      { id: 'gamification',  path: '/gamification',   name: 'Bảng xếp hạng' },
      { id: 'docs',          path: '/docs/intro/intro',  name: 'Tài liệu' },
    ]
  },
  {
    groupName: 'Mở rộng từ bài học',
    items: [
      { id: 'sorting',     path: '/sorting',     name: 'Sắp xếp' },
      { id: 'graph',       path: '/graph',       name: 'Đồ thị' },
      { id: 'code-ide',    path: '/code-ide',    name: 'Gỡ lỗi Code' },
      { id: 'playground',  path: '/playground',  name: 'Playground' }
    ]
  },
  {
    groupName: 'Hồ sơ',
    items: [
      { id: 'profile',      path: '/profile',      name: 'Hồ sơ', requiresAuth: true },
      { id: 'gems-shop',    path: '/gems-shop',    name: 'Cửa hàng Gems' },
      { id: 'export-share', path: '/export-share', name: 'Export / Share' },
    ]
  },
  {
    groupName: 'Quản lý',
    items: [
      { id: 'dashboard',      path: '/dashboard',      name: 'Bảng điều khiển', requiresAuth: true },
      { id: 'teacher-studio', path: '/teacher-studio', name: 'Teacher Studio', requiresAuth: true, requiresRole: 'Teacher' },
      { id: 'teacher',        path: '/teacher',        name: 'Quản lý Giảng viên', requiresAuth: true, requiresRole: 'Teacher' },
      { id: 'admin',          path: '/admin',          name: 'Quản trị', requiresAuth: true, requiresRole: 'Admin' },
    ]
  }
] as const;

