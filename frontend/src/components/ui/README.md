# UI Kit — ranh giới import (1 kit duy nhất)

## API công khai (app code DÙNG cái này)

File **PascalCase** ở gốc thư mục này: `Button.vue`, `Card.vue` (props `interactive`/`padded`), `Modal.vue`, `Badge.vue`, `Input.vue`, `Select.vue`, `Tabs.vue`, `Drawer.vue`, `Tooltip.vue`, `Skeleton.vue`, `EmptyState.vue`, `StatCard.vue`, `ProgressBar.vue`, ... và bridge `buttonVariants.ts` (cho RouterLink/`<a>`).

## Nội bộ kit (CẤM import từ app code)

Các thư mục **kebab-case** — `button/`, `card/`, `dialog/`, `drawer/`, `select/`, `tabs/`, `tooltip/`, `badge/`, `input/`, `progress/`, `skeleton/` — là lớp style-engine (shadcn-vue + reka-ui + cva). Chỉ các file wrapper PascalCase và file cùng thư mục kit được phép chạm.

Quy tắc được kiểm thực tự động bởi `__tests__/kit-boundary.spec.ts` (chạy cùng `pnpm test`). Nếu cần primitive mới, thêm qua `pnpm dlx shadcn-vue add <component>` rồi bọc bằng wrapper PascalCase trước khi dùng ở view.
