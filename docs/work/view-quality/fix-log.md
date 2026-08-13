# FIX-LOG — View Quality Phase 1 · Nhóm D (3 view Classes)

> Ngày: 14/08/2026 · Agent: dev-frontend (nhóm D) · Nhánh: feature/view-quality-d · Worktree: D:\FPT\neww-qd
> Quy tắc chấm: `standard.md` (10 trục hygiene + Đặc trưng tách riêng + 3 điều kiện ĐẠT) · Nguồn thiết kế: `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` + 6 quyết định xuyên-nhóm.

## Đợt 2 — 5 view Admin (14/08, cùng nhánh)

## AdminUsersView.vue (`/admin/users`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 58.0/100 | **92.5/100** |
| Đặc trưng | 2/10 | **8/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- Hero gradient Aurora + blob blur + title gradient-clip + shadow → **surface band level-2** + **mono strip block-token** (số chờ duyệt thật, block `data-core` + index mono, stagger-enter 280ms `cubic-bezier(0.16,1,0.3,1)` — khoảnh khắc đầu tư, reduced-motion đúng).
- Bảng: th uppercase + tracking → §4.6 (`text-sm font-medium text-foreground-tertiary h-10`, `scope="col"`); td 12px; hover `muted/50`; **mobile card-stack** thay scroll ngang; cột ngày mono; avatar/name gradient + weight 800/700 → neutral + 600.
- `.admin-users__actions` gap 4px → **8px** (nút liền kề); icon button 14→16px; search input padding-left 34px → `--space-xl` (32px) + icon `left: space-sm`.
- **Thêm error state + nút Thử lại** (trước: load fail → toast + EmptyState "Không có người dùng" gây hiểu nhầm); EmptyState thêm description lời mời hành động (§9); aria-label lock fallback `displayName || email` (user rỗng tên).
- Bỏ `.card` legacy (shadow-md KILL-LIST) → panel token `bg-card border-border`.

## AdminStatsView.vue (`/admin/stats`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 62.5/100 | **93.5/100** |
| Đặc trưng | 2/10 | **8.5/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- Hero gradient → surface band level-2 + **mono strip block-token** (5 chỉ số).
- **5 stat-card công thức KILL-LIST** (icon tròn 3 gradient + #fff + shadow + hover-lift + weight 800) → **1 hero-stat** (Tổng người dùng: card level-2 + block-token tối `canvas-ink`/`data-core` + index mono `USERS · 01`) + **4 KPI level-1** (không icon/shadow, Geist 600).
- **2 vùng biểu đồ → LUÔN tối `bg-canvas-ink`** (quyết định #5): ECharts palette đọc CSS var `--data-core`/`--index-muted`/`--canvas-ink` + text `#d9dde8` (engine — decision log 14/08); donut màu `data-core/resolved/index-muted` (không bịa); legend mono 500; donut text ≥12px (trước 10px).
- Tag uppercase + tracking → mono caption; note icon `--color-primary` → `--info`; thêm error state + retry.

## AdminSettingsView.vue (`/admin/settings`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 68.0/100 | **92.5/100** |
| Đặc trưng | 2/10 | **7.5/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- Hero gradient → surface band level-2; section title màu `--color-primary` (accent trang trí) → `text-lg font-semibold tracking-[-0.015em] text-foreground` + icon `foreground-secondary`.
- Form `.card` legacy shadow → panel token không shadow; error alert → token `destructive` + **nút Thử lại** (tách `load()` — trước onMounted inline, lỗi tải hiện form mặc định gây nhầm "đã lưu").
- Icon Save 15→16px; `--color-text-muted` → `--foreground-secondary`; checkbox giữ native 16px + accent teal (label = target ≥24px).

## AdminContentView.vue (`/admin/content`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 59.5/100 | **92.5/100** |
| Đặc trưng | 2/10 | **8.5/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- **Bug: cột "Ngày tạo" hiển thị `formatDate(new Date())`** (LessonSummary không có createdAt — ngày giả) → thay **cột index mono `#01`** (dữ liệu tuần tự — quyết định #4); i18n `colCreated` → `colIndex`.
- Hero gradient → surface band + **mono strip block-token** (số bài học/chủ đề thật: `08 BÀI · 05 CHỦ ĐỀ`).
- Bảng §4.6 + mobile card-stack; sim-count mono; topic card bỏ gradient mint icon + hover-lift → icon neutral + hover border strong; toolbar action chính sm→md; actions gap 4→8px; thêm error state + retry.
- Ghi chú Phase 2: nội dung rich-text contentHtml (emoji CMS) — KHÔNG sửa nội dung (task yêu cầu ghi notes).

## AdminLadderView.vue (`/admin/ladder`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 56.0/100 | **92.0/100** |
| Đặc trưng | 2/10 | **8/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- **1 `<button` raw** (`.admin-ladder__node`, padding 8/16 + hover-lift) → **Button.vue** (outline md, `w-full` + justify-start scoped) — **grep `<button` raw = 0**; giữ aria-pressed.
- **Node-id tròn gradient + weight 800 → block-token tối** `bg-canvas-ink` + `text-data-core` + mono index (dữ liệu tuần tự — quyết định #4); selected: border-primary + ring 1px (bỏ color-mix 2px); bỏ hover-lift.
- Note card `.text-muted` → 4 tầng; icon info `--info`; subtitle `--text-md` → text-lg 600 tracking; select `.input` override token + easing 150ms; thêm error state + retry.
- **AdminNav.vue (component dùng bởi đúng 5 view admin — trong scope)**: token shadcn (`--card/--border/--primary`), link min-height 36px, gap ≥8px, weight 500 (trước 0.4rem padding + weight 600 + legacy tokens).

## Thay đổi hạ tầng chung (đã ghi `docs/pm-decision-log-viewquality.md` 14/08 — bắt buộc trước khi sửa)

## ClassesView.vue (`/classes`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 61.0/100 | **92.5/100** |
| Đặc trưng | 2/10 | **8.5/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- Hero gradient Sunset + `::after` overlay + `text-shadow` + `--shadow-lg` → **surface band level-2** (`bg-card-raised` + border) + **mono strip block-token dữ liệu thật** (số lớp + tổng thành viên, block `data-core` + index mono, stagger-enter 280ms `cubic-bezier(0.16,1,0.3,1)` — khoảnh khắc đầu tư duy nhất, reduced-motion đúng).
- Card `hover-lift` (translate + shadow 180ms ease) → level-1, hover chỉ đổi `border → border-strong`, 150ms transition-colors; bỏ shadow.
- Icon tròn gradient → neutral `bg-muted text-foreground-secondary`.
- Chip mã mời (weight 700, color-mix, không chuẩn) → **block-token tối** `bg-canvas-ink` + mono 500 + `text-resolved`, min-height 24px.
- H1 36px → 48px `tracking-[-0.03em]`; card name h2/18px → h3 `text-lg font-semibold tracking-tight`.
- CTA hero `md` → `lg` (44px); card clickable bổ sung phím **Space**.
- Spacing ngoài scale (`gap: 2px/6px`, `padding: 3px 10px`) → token.
- Bugfix hiển thị: `isManagerOf` tính từ `ownerId === auth.user?.id` (API list KHÔNG trả `role` → trước đây badge luôn "Thành viên", chip mời không bao giờ hiện).

## ClassDetailView.vue (`/classes/:id`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 55.0/100 | **92.5/100** |
| Đặc trưng | 2/10 | **9/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- Hero gradient + glassmorphism chip (`backdrop-filter: blur`) + `#fff` → surface band level-2; mã mời thành **block-token tối** (`bg-canvas-ink`, code `text-resolved` mono 0.12em, label `text-index-muted`) + nút Sao chép qua `Button.vue` (icon Check/ClipboardCopy, aria-label) — settle-enter 250ms chuẩn.
- **1 `<button` raw** (copy-btn, padding 3px 8px) → `Button size="sm" variant="secondary"` (36px, px-12px).
- Bảng: th uppercase + tracking → §4.6 (`text-sm font-medium text-foreground-tertiary h-10`, không in hoa); td 12px; hover `bg-muted/50`; **mobile 390 card-stack** (data-label) thay scroll ngang; cột ngày mono; thêm `scope="col"`.
- Avatar/name/assign-icon gradient + weight 700/800 → neutral + ≤600; xóa `assignmentTint` (3 gradient); assignment card thêm **index mono `#01`** (dữ liệu tuần tự — quyết định #4), due mono, hover border.
- Modal assign: raw `<input class="input">` → `Input.vue` (datetime-local); `.text-muted` legacy → 4 tầng token.
- `copyInvite` setTimeout cleanup `onUnmounted`; bugfix `isManager` từ `ownerId` (trước: teacher không bao giờ thấy tab Cài đặt/nút copy).

## ClassReportView.vue (`/classes/:id/report`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 63.5/100 | **92.5/100** |
| Đặc trưng | 2/10 | **9/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- **Align contract API với backend THẬT** (phát hiện khi chạy thật): frontend type kỳ vọng `completionPct/avgScore/submissions/rows` nhưng backend trả `totalMembers/assignments[]/laggingLearners[]` → view render "undefined%"/"NaN"/bảng rỗng. Sửa `api/types.ts` (ClassReportDto + ClassReportAssignmentDto + LaggingLearnerDto), `classes.spec.ts`, view đọc dữ liệu thật: KPI = Bài gán / Điểm TB (mean avgScore) / Bài nộp (onTime+late); summary = submitted/expected + mono `15 / 28 BÀI NỘP`; **bảng bài gán** (index mono `#01`, title+due mono, onTime/late/notSubmitted, Điểm TB, badge trạng thái); **block lagging learners** nền `canvas-ink` + index mono + `THIẾU 04` (conflict); EmptyState `icon="database"` (trước `chart` không tồn tại → fallback x-circle).
- Hero gradient compact → surface band level-2 + sub mono `Tên · ID 03`.
- 4 KPI icon tròn gradient + `hover-lift` (công thức stat-card KILL-LIST) → **1 hero-stat** (Thành viên: block-token tối `data-core` + index mono) + 3 stat level-1 (không icon, không shadow, Geist 600 2xl).
- KPI value weight 800 → 600; th bỏ uppercase; cột số → mono; `✓` text glyph → lucide Check/Minus; avatar neutral.
- Ngoại lệ ghi decision log: `#d9dde8` (màu text canvas engine `canvasTheme.ts`) cho tên học viên trên panel tối — chưa có UI token chữ sáng.

## Thay đổi hạ tầng chung (đã ghi `docs/pm-decision-log-viewquality.md` 14/08 — bắt buộc trước khi sửa)

1. `frontend/src/styles/global.css`: bỏ `margin:0; padding:0` khỏi universal reset unlayered — đang giết MỌI utility padding/margin Tailwind toàn app (đo computed: button shadcn pl=0). Tailwind preflight đã reset trong base layer.
2. `frontend/src/components/ui/Badge.vue`: thêm `min-h-6` (badge cao 21.6px < 24px chuẩn §4.3).
3. `frontend/src/components/ui/card/Card.vue`: bỏ `shadow-sm` (DESIGN §6 cấm shadow trên card — chỉ dropdown/modal).

## Verify

- `npm run build` (gồm `vue-tsc -b`): **PASS** ×3 lần cuối.
- `npx vitest run`: **12 files / 95 tests PASS** (gồm classes.spec cập nhật contract).
- Browser (dev :5178, BE :5000): 3 view × light+dark × 1366/768/390 — 0 console error, 0 overflow ngang; computed style đo trực tiếp (padding button lg 32px/44px, badge 24px, card shadow none, canvas-ink panels, mono cột số, card-stack mobile, tab/delete/copy/add-member flow hoạt động).
- Không có lint script trong repo (`package.json` không có "lint") — không chạy được, ghi rõ.
- Ollama 3-gate: không có gate/script trong repo — ghi `notes.md`, KHÔNG báo pass giả.
