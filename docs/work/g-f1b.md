# G-F1b — Phase 1b ux-foundation: thay 13 component UI tự xây bằng shadcn-vue

> Ngày: 12/08/2026 · Nhánh: `feature/ux-foundation` · Tác giả commit: **son**
> Phạm vi: `frontend/src/components/ui/*` + toast → vue-sonner · KHÔNG đụng `components/simulator/*` + `engines/*` (canvas engine giữ nguyên), KHÔNG đổi view business logic.

## Mục tiêu
Thay 13 component UI tự xây (CSS scoped + design tokens cũ) bằng shadcn-vue, **ưu tiên wrapper giữ nguyên tên file + API cũ** để 33 view không vỡ. Sửa call site CHỈ khi không map được prop.

## Kết quả từng mục

| # | Mục | Kết quả | Cách thay | Call site |
|---|-----|--------|-----------|-----------|
| 1 | Button | **DONE** | Wrapper giữ file+API (`variant primary/secondary/ghost/danger → default/outline/ghost/destructive`, `size sm/md/lg`, `loading`, `block`, `type`), render bằng `buttonVariants` shadcn | 34 file (không sửa) |
| 2 | Input | **DONE** | Wrapper giữ API (label/error/hint/icon/v-model/type/required/maxlength), control = shadcn Input; thêm class `ui-input` để giữ e2e selector | 9 file (không sửa) |
| 3 | Modal | **DONE** | Wrapper giữ API (`open/title/closable/width` + `@close` + slot default/`#footer`) bọc shadcn Dialog (DialogScrollContent — giữ scroll nội dung dài) | 7 file (không sửa) |
| 4 | Drawer | **DONE** | Wrapper giữ API (`open/title/width` + `@close`) bọc vaul-vue Drawer (shadcn drawer), `direction="right"` giữ dạng panel phải | 1 file (không sửa) |
| 5 | Badge | **DONE** | Wrapper giữ API (`variant default/primary/success/warning/danger/muted/secondary`) dùng `badgeVariants` + custom tailwind cho success/warning/muted | 18 file (không sửa) |
| 6 | ProgressBar | **DONE** | Wrapper giữ API (`value/showLabel/variant/size`) dùng shadcn Progress; màu variant qua selector `[&_[data-value]]:bg-*` | 5 file (không sửa) |
| 7 | Skeleton | **DONE** | Wrapper giữ API (`height/width/lines/circle`) dùng shadcn Skeleton | 23 file (không sửa) |
| 8 | Tabs | **DONE** | Wrapper giữ API (`tabs[]/modelValue` + `@change` + slot) dùng shadcn Tabs (TabsList/TabsTrigger) | 0 file |
| 9 | Tooltip | **DONE** | Wrapper giữ API (`text/position` + slot) dùng shadcn Tooltip (reka-ui) | 1 file (simulator, không sửa) |
| 10 | Select | **DONE** | Wrapper giữ API (`label/options/modelValue/placeholder`) dùng shadcn Select | 0 file |
| 11 | EmptyState | **DONE** | Không có shadcn tương đương → giữ API cũ, **restyle bằng tailwind/shadcn tokens** (bg-muted icon circle, text-muted-foreground, border-primary button) | 21 file (không sửa) |
| 12 | Card | **DONE** | Wrapper giữ API (`interactive/padded`) dùng shadcn Card | 0 file |
| 13 | ToastContainer | **DONE** | Thay bằng **vue-sonner**: `src/lib/toast.ts` (toast.success/error/warning/info/show/dismiss) + `useToast` delegate + `uiStore.showToast` giữ API cũ (shim → sonner) + App.vue mount `<Toaster position="top-right" rich-colors close-button>`. Xóa `ToastContainer.vue` + `.css` (0 call site) | 27 file giữ nguyên qua store shim |

## Files tạo mới (shadcn-vue CLI: `npx shadcn-vue@latest add button input dialog drawer badge progress skeleton tabs tooltip select card`)
- `frontend/src/components/ui/{button,input,dialog,drawer,badge,progress,skeleton,tabs,tooltip,select,card}/` — 57 file shadcn chuẩn (chưa chỉnh tay, trừ `DialogScrollContent.vue` thêm `style` prop để Modal wrapper truyền `maxWidth` — tránh Vue warn attr rơi vào `DialogPortal`).
- `frontend/src/lib/toast.ts` — helper toast vue-sonner.

## Files sửa
- `frontend/src/components/ui/{Button,Input,Modal,Drawer,Badge,ProgressBar,Skeleton,Tabs,Tooltip,Select,Card,EmptyState}.vue` — wrapper (giữ tên+API).
- `frontend/src/App.vue` — bỏ markup toast tự xây (ui.toasts/dismissToast), mount `<Toaster />`.
- `frontend/src/stores/ui.ts` — bỏ `toasts`/`toastSeq`; `showToast/dismissToast` giữ API cũ nhưng delegate sang vue-sonner (shim → 27 call site `ui.showToast(...)` không đổi).
- `frontend/src/composables/useToast.ts` — delegate sang `@/lib/toast`.
- `frontend/package.json` + lock — `@vueuse/core` (14.4.0), `vaul-vue` (0.4.1) do shadcn CLI thêm.

## Files xóa
- `frontend/src/components/ui/ToastContainer.vue`, `ToastContainer.css` (0 call site — App.vue render toast inline cũ, không dùng file này).

## Giữ nguyên
- `ConfirmModal.vue`/`BottomSheet.vue`/`BaseIcon.vue` — call site còn dùng / chưa có shadcn thay thế dễ dàng (ConfirmModal chưa nối vào uiStore — dead code hiện hữu, giữ theo quyết định log).
- `components/simulator/*` + `engines/*` — KHÔNG đụng.

## Verify
- `npm run build` (vue-tsc + vite) → **0 lỗi** ✓ (sửa 2 lỗi type emit Input/Select).
- `npm test` (vitest) → **72/72 PASS** ✓.
- Grep: không còn import tới `ToastContainer` (chỉ comment trong App.vue); 39 file vẫn import qua đường dẫn wrapper cũ (đúng mục đích — không phải sửa call site).
- `npm run test:e2e` (Playwright, mock backend): **simulator 3/3 PASS** (canvas + Button/Modal wrapper render tốt, hết Vue warn "Extraneous style" của Modal). **auth/ladder/code-runner 8 FAIL — đã xác nhận PRE-EXISTING** tại base commit 20f956d (stash + chạy lại cho kết quả giống hệt): mockApi trả 200 cho `POST /auth/refresh` → app luôn ở trạng thái authenticated → guard không redirect sang /login. Không liên quan thay đổi G-F1b.

## Ghi chú follow-up
- e2e auth-flow cần sửa mock (refresh trả 401 khi chưa login) — thuộc test-infra, ngoài scope G-F1b.
- `Modal` dùng `DialogScrollContent` (scroll dài) thay vì `DialogContent` — nếu cần style chính xác hơn có thể tinh chỉnh class trong wrapper.
- `Badge` success/warning dùng màu emerald/amber thay vì token `--color-success/warning` legacy (không còn trong theme OKLCH).
