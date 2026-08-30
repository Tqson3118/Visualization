import { test as base, expect, type Page } from '@playwright/test';
import { loginViaUi } from '../helpers/auth';

type TeacherFixtures = {
  teacherPage: Page;
  adminPage: Page;
};

export const test = base.extend<TeacherFixtures>({
  teacherPage: async ({ page, context }, use) => {
    await context.addCookies([
      {
        name: 'dsa.session',
        value: '1',
        domain: 'localhost',
        path: '/',
      },
    ]);
    await loginViaUi(page, 'teacher@test.edu', 'Teacher@123', { role: 'TEACHER' });
    await use(page);
  },
  adminPage: async ({ page, context }, use) => {
    await context.addCookies([
      {
        name: 'dsa.session',
        value: '1',
        domain: 'localhost',
        path: '/',
      },
    ]);
    await loginViaUi(page, 'admin@test.edu', 'Admin@123', { role: 'ADMIN' });
    await use(page);
  },
});

export { expect };
