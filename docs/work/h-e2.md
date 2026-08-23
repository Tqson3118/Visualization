# H-E2 — Khám phá + Phụ trợ: Simulations / CheatSheet / Help / Privacy / 404

Đợt H round 2 (nhóm E2) — polish 5 view chưa nâng cấp theo chuẩn H.
Branch: `feature/ux-h-e2` (từ `dev` @ e5adff7). Chỉ đụng 6 file.

## Thay đổi

| View | Nâng cấp chính |
|---|---|
| `SimulationsView.vue` | Chrome hero **Cyber Mint** (breadcrumb + icon chip + badge "3 demo miễn phí" + stats 44/14/2) đồng bộ BenchmarkView; tab bar → **shadcn Tabs**; card lưới → **shadcn Card** + icon lucide theo loại (sort/search/stack/queue/list/tree/heap/hash/graph/structure) + hover-lift; giữ nguyên logic lọc/phân trang + mọi aria-label |
| `CheatSheetView.vue` | Chrome hero Cyber Mint + breadcrumb (Khám phá) + badge "44 mục"; giữ CheatSheetTable |
| `HelpView.vue` | Hero **Aurora soft** + breadcrumb; FAQ card lucide ChevronDown (rotate 180°); form → **shadcn Input/Button** + label/icon, success state CheckCircle2 |
| `PrivacyView.vue` | Hero Aurora soft + breadcrumb; **TOC sticky** (anchor #sec-1..6, Lenis anchors:true), section divider, scroll-margin-top |
| `NotFoundView.vue` | 404 ring gradient Aurora + Motion fade-up + Compass icon; min-height calc (không overflow với shell) |
| `i18n/vi.ts` | **Chỉ THÊM** 3 namespace mới: `explore`, `cheatsheet`, `help` (không sửa/xóa chuỗi cũ) |

## Contrast (probe WCAG computed thật — 18 selector × 2 theme = 36/36 PASS ≥ 4.5:1)

- Chrome mint light/dark: overlay 62% → **68%** + hero sub dùng `color-mix(foreground 92%)` (text-muted chỉ 3.47:1 light → fail AA).
- Card key: override `--muted-foreground` shadcn dark (3.34:1) → legacy `--color-text-muted` (16.4:1 dark / 5.34:1 light).
- Help/Privacy hero sub: foreground 92% trên aurora-soft (4.26 → 6.21:1 light).

## Verify (số thật)

- `npm run build` → **0 lỗi** (vue-tsc + vite build ✓)
- `npm test` → **89/89 passed** (11 files)
- Playwright render spec tạm (đã xoá, không commit): **17/17 passed** — 4 view × light/dark × 1366×768 + 390×844: 0 console error (trừ nhiễu 401 `/auth/refresh` sẵn có của mockApi boot), 0 horizontal overflow; 404: route sai → NotFoundView + nút "Về trang chủ" → URL `/`.

## Ghi chú

- Không có lint script trong package.json (bỏ qua bước 3 checklist).
- Nhiễu console 401 khi chưa login là hành vi sẵn có (mockApi trả 401 cho /auth/refresh — mọi spec e2e cũ đều có), không phải do view.
- BenchmarkView giữ overlay 62% (ngoài phạm vi — đã qua Ollama review đợt trước).
- Đề xuất sau: BenchmarkView chrome overlay 62% → 68% cho đồng bộ contrast; CheatSheetTable dùng `--muted-foreground` dark 3.3:1 trên bảng — xem xét đổi token legacy khi có đợt riêng.

## H-E2 round 2 — Fix contrast (dev-e2e P2, PM duyệt)

2 điểm contrast fail từ dev-e2e đo thực tế → sửa xong, đo lại bằng Playwright (spec tạm đã xoá, không commit).

### A. Badge success trên chrome mint (Simulations + Benchmark)

- Trước: `Badge.vue:26` tint `bg-emerald-500/15 text-emerald-700 dark:text-emerald-400` — 3.05:1 light / 2.87:1 dark (text-xs cần 4.5:1).
- Sau: chip **opaque** `border-emerald-200 bg-emerald-100 text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300` — contrast không phụ thuộc nền gradient chrome nữa.
- Đo lại (computed style thật): Simulations + Benchmark **6.70:1 light / 9.97:1 dark** (cùng component → 10 call site success khác tự hưởng lợi, không cần sửa view).

### B. Chữ "404" trên ring gradient Aurora (dark)

- Trước: `NotFoundView.vue:54-65` chữ trắng trên `--gradient-aurora` dark (sáng 0.72–0.86) = 2.29:1 (large text fail 3:1).
- Sau: thêm scrim tối đúng pattern HomeView/LoginView: `.dark .not-found__ring { background-image: linear-gradient(rgba(4,47,46,0.68), rgba(4,47,46,0.68)), var(--gradient-aurora); }`.
- Đo lại (sample pixel thật từ element screenshot, ẩn glyph): **4.68:1 light / 6.15:1 dark** (≥ 3:1).

### Verify

- `npm run build` → 0 lỗi; `npm test` → 89/89 PASS.
- Playwright spec tạm: 8/8 PASS (badge × 2 view × 2 theme ≥ 4.5; ring × 2 theme ≥ 3; 3 view × 2 theme: 0 console error — trừ 401 boot sẵn có, 0 overflow).
- Không đổi token chính (teal #0D9488, 3 gradient palette giữ nguyên); chỉ 2 file sửa: `components/ui/Badge.vue` + `views/NotFoundView.vue`.
- Lưu ý kỹ thuật: Playwright `addInitScript` đặt class lên `document.documentElement` bị mất sau khi Chromium parse HTML (documentElement bị thay thế — đã verify bằng trang HTML tĩnh) → đo dark phải áp class trên document sống sau `goto`.
