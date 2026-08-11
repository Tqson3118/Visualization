import type { RouteRecordRaw } from 'vue-router';





export const routes: RouteRecordRaw[] = [
  
  { path: '/',              name: 'landing',       component: () => import('../views/landing/LandingView.vue'),           meta: { title: 'Chào mừng',        public: true } },
  { path: '/dashboard',     name: 'dashboard',     component: () => import('../views/dashboard/DashboardView.vue'),         meta: { title: 'Bảng điều khiển',   requiresAuth: true } },


  
  { path: '/sorting',       name: 'sorting',       component: () => import('../views/sorting/SortingView.vue'),          meta: { title: 'Sắp xếp',         icon: 'sorting' } },
  { path: '/searching',     name: 'searching',     component: () => import('../views/searching/SearchingView.vue'),      meta: { title: 'Tìm kiếm',        icon: 'search' } },
  { path: '/code-ide',      name: 'code-ide',      component: () => import('../views/code-ide/CodeIDEView.vue'),          meta: { title: 'Gỡ lỗi Code',     icon: 'code-ide' } },
  { path: '/playground',    name: 'playground',    component: () => import('../views/playground/PlaygroundView.vue'),      meta: { title: 'Playground',       icon: 'playground' } },
  { path: '/graph',         name: 'graph',         component: () => import('../views/graph/GraphView.vue'),            meta: { title: 'Đồ thị',          icon: 'graph' } },
  { path: '/checkout',      name: 'checkout',      component: () => import('../views/checkout/PremiumCheckoutView.vue'),  meta: { title: 'Nâng cấp Premium', icon: 'checkout' } },

  
  { path: '/docs/:pathMatch(.*)*', name: 'docs', component: () => import('../views/docs/DocsView.vue'), meta: { title: 'Tài liệu Tham khảo', icon: 'book' } },
  { path: '/oop', redirect: '/docs/oop' },
  { path: '/solid', redirect: '/docs/solid' },
  { path: '/di', redirect: '/docs/di' },
  { path: '/patterns', redirect: '/docs/patterns' },
  { path: '/quiz',          name: 'quiz',          component: () => import('../views/quiz/BackendQuizView.vue'),      meta: { title: 'Trắc nghiệm',     icon: 'quiz' } },
  { path: '/gamification',  name: 'gamification',  component: () => import('../views/gamification/GamificationEngineView.vue'),meta: { title: 'Bảng xếp hạng',   icon: 'gamification' } },

  

  { path: '/profile',       name: 'profile',       component: () => import('../views/profile/ProfileView.vue'),          meta: { title: 'Hồ sơ cá nhân',   requiresAuth: true } },
  { path: '/courses',       name: 'courses',       component: () => import('../views/courses/CoursesListView.vue'),      meta: { title: 'Khóa học',        icon: 'learning-path', public: true } },
  { path: '/courses/:id',   name: 'course-detail', component: () => import('../views/courses/CourseDetailView.vue'),    meta: { title: 'Chi tiết Khóa học', public: true } },
  { path: '/lessons/:id',   name: 'lesson-study',  component: () => import('../views/lesson/LessonStudyView.vue'),     meta: { title: 'Học Bài giảng',    requiresAuth: true } },



  
  { path: '/admin',         name: 'admin',         component: () => import('../views/admin/AdminPanelView.vue'),       meta: { title: 'Quản trị Admin',  requiresAuth: true, requiresRole: 'Admin' } },

  
  { path: '/:pathMatch(.*)*', name: 'not-found', component: () => import('../views/not-found/NotFoundView.vue'), meta: { title: 'Trang không tồn tại', public: true } },
];
