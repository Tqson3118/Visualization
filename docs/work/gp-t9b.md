# GP-T9b — Sửa nhận xét xấu UI Review Vòng 2 (39 nhận xét)

- **Ngày:** 2026-08-13
- **Người thực hiện:** dev-ux (celscin-coder)
- **Nhánh:** `feature/ux-review2` (từ `dev` HEAD `45960af`)
- **Cách xác minh:** do model 3B có nhiễu và ảnh PNG không đọc trực tiếp được, dev-ux xác minh bằng **đo thực tế**:
  1. Playwright đo computed color của từng phần tử trên 12 màn × 2 theme (1366×768, login thật `student@demo.local`) → tính contrast WCAG (script `frontend/audit-t9b.mjs` → `docs/work/r2-audit-before.json`)
  2. Đo horizontal overflow + console error mỗi màn (24/24: **0px overflow, 0 console error**)
  3. Tính contrast toán học cho gradient (chữ trắng trên từng stop) vì gradient là `background-image` không đo được bằng computed color

---

## 1. Bảng trạng thái 39 nhận xét

| # | Màn (theme) | Mô tả ngắn | Mức | Trạng thái | Ghi chú / file:dòng |
|---|-------------|-----------|-----|-----------|---------------------|
| 1 | Tất cả ⓒ | Header nền sáng, chữ "DSA Visual" khó đọc | P1 | **TỪ CHỐI** | Header đo **9.48:1 (light) / 10.66:1 (dark)** — đạt; brand `--color-foreground` trên `--color-surface`. Model nhiễu. Active link nav (primary) đã cải thiện 3.74→4.97:1 qua token primary (`tokens.css:10`; `tailwind.css:55`) |
| 2 | 01-Home (light) | Hero title font nhỏ, không nổi bật | P2 | **ĐÃ SỬA** | `HomeView.vue:275` — clamp `2rem→5vw→3.5rem` (48px→56px); gradient tối đi (`palettes.css:19`) → chữ trắng ≥ 4.5:1 |
| 3 | 01-Home (light) | 2 CTA sát nhau nhìn thành chồng lấn | P2 | **ĐÃ SỬA** | `HomeView.vue:286` — gap 16px→24px (`--space-lg`) |
| 4 | 01-Home (light) | Nền gradient vùng sáng chói, màu không đều | P2 | **ĐÃ SỬA** | `HomeView.vue:226` — blob opacity 0.5→0.28; gradient light tối đi (`palettes.css:19-39`) |
| 5 | 01-Home (light) | Footer thiếu thông tin liên hệ | P2 | **ĐÃ SỬA** | `App.vue:55` — thêm link "Liên hệ" → trang Trợ giúp (có form contact) |
| 6 | 01-Home (dark) | Header+hero dark quá sáng; CTA spacing lệch | P1 | **ĐÃ SỬA** | `HomeView.vue:264-265` — dark scrim `rgba(4,47,46,0.62)` trên hero (chữ trắng 5.4-7.7:1 sau scrim); CTA gap 24px. Header dark đo 10.66:1 — phần header từ chối |
| 7 | 02-Login (light) | Panel quá sáng, chữ nhỏ, spacing field lệch | P2 | **ĐÃ SỬA** | `LoginView.vue:250,258` — title 2xl→3xl, gap form 8→16px (`--space-md`) |
| 8 | 02-Login (dark) | Chữ khó đọc, contrast kém, spacing lệch (TB) | P2 | **ĐÃ SỬA** | `LoginView.vue:153` — dark scrim `rgba(4,47,46,0.62)` trên aside (chữ trắng ≥5.4:1); spacing đồng bộ với #7. Token dark đo 10.66:1 đạt — phần contrast từ chối |
| 9 | 03-Lesson (light) | Header nền sáng khó đọc | P2 | **TỪ CHỐI** | Nhận xét chéo #1 — header đo 9.48:1 đạt |
| 10 | 03-Lesson (light) | Tiêu đề "Bubble Sort" chữ nhỏ | P2 | **ĐÃ SỬA** | `LessonView.vue:222` — hero title 2xl→3xl (30→36px) |
| 11 | 03-Lesson (light) | Nút "Đánh dấu đã học" tràn/cắt layout | P1 | **TỪ CHỐI** | Đo overflow **0px** (24 màn); `.lesson-view__hero-actions` flex-wrap — không tràn viewport |
| 12 | 03-Lesson (dark) | Tiêu đề+nội dung nhỏ, contrast kém; footer lệch | P2 | **ĐÃ SỬA** | `LessonView.vue:222,232` — title 3xl + dark scrim `rgba(4,47,46,0.62)` (chữ trắng ≥5.7:1); footer dùng token chung (đo 9.52:1) |
| 13 | 04-Simulator (light) | Sidebar control spacing lệch | P2 | **ĐÃ SỬA** | `PseudocodePanel.vue:182,189` — line padding 3px→4px, gap 1→2px |
| 14 | 04-Simulator (dark) | Sidebar quá rộng chiếm diện tích | P2 | **TỪ CHỐI** | Grid `3fr 6fr 3fr` → sidebar ~300px/1200px (25%) — chuẩn thiết kế SDD §8.5 (3/12) |
| 15 | 04-Simulator (dark) | Nút "Tự thực hành" thiếu khoảng cách | P2 | **ĐÃ SỬA** | `SimulatorView.vue:432` — `.simulator__actions` gap 8→16px |
| 16 | 04-Simulator (dark) | "Cấu hình đầu vào" contrast kém trên nền tối | P1 | **TỪ CHỐI** | Button secondary = outline → text-primary. Dark primary `#2DD4BF` trên chrome tối đo **7.77:1** — đạt |
| 17 | 05-Exercise (light) | Option thiếu padding, "Câu 1/5" căn lệch | P2 | **ĐÃ SỬA** | `QuizStage.vue:302` — option padding 8→12px (≥12px theo gợi ý); header `justify-between` căn đều |
| 18 | 05-Exercise (dark) | Chữ trong bảng contrast kém, spacing lệch | P2 | **ĐÃ SỬA** | `QuizStage.vue:302` — padding đồng bộ #17; token dark đo pass (surface/foreground 10.66:1) |
| 19 | 06-Path (light) | Thẻ spacing không đều, thiếu CTA rõ ràng | P2 | **TỪ CHỐI** | Duolingo-style trail: connector 28px + gap 4px đều; CTA "Bắt đầu" đã có trong popover node (`PathView.vue:258-263`) |
| 20 | 06-Path (dark) | Chữ trong thẻ không đủ sáng | P1 | **TỪ CHỐI** | Đo dark: node label = foreground/surface **10.66:1**; badge muted = 6.07:1 — đạt |
| 21 | 07-Ladder (light) | "Practice Ladder" màu gần giống nền | P1 | **ĐÃ SỬA** | `LadderView.vue:183` — title 2xl→3xl; sunset gradient light tối đi (`palettes.css:28`) → text-gradient ≥ 4.67:1 trên nền |
| 22 | 07-Ladder (light) | Card "Đang học"/"Lab" spacing lệch | P2 | **TỪ CHỐI** | Stepper gap 16px + shell gap 24px đồng đều (đo) |
| 23 | 07-Ladder (light) | "Tính năng đang xây dựng" khó đọc | P2 | **TỪ CHỐI** | Badge muted (bg-muted/text-muted-foreground): light 4.79:1 / dark 6.07:1 — đạt; EmptyState title = text-foreground |
| 24 | 08-Lab (light) | Mô tả bài font nhỏ, contrast kém | P2 | **ĐÃ SỬA** | `LabView.vue:142` — info-text `text-sm`→`text-base`; `tokens.css:26` text-muted `#56706D` = 5.12:1 |
| 25 | 08-Lab (light) | Interactive Lab chồng lấn layout | P1 | **TỪ CHỐI** | Đo overflow **0px**; `.lab-stage__canvas` flex-wrap + cells 64px — không chồng lấn |
| 26 | 08-Lab (dark) | Modal thiếu border dễ nhầm | P2 | **ĐÃ SỬA** | `tailwind.css:108` — dark `--border` 0.32→0.4 (rõ trên card tối); DialogScrollContent đã có `border-border` |
| 27 | 09-Code (light) | Nền editor quá tối trong light theme | P1 | **ĐÃ SỬA** | `CodeRunnerView.vue:313,345,353` — editor theme-aware: light = surface+foreground (9.48:1), dark = dark surface (10.66:1) |
| 28 | 09-Code (light) | Empty state "Chưa có kết quả" mờ | P2 | **ĐÃ SỬA** | `EmptyState.vue:30` — icon `text-muted-foreground/70`→`text-foreground/80`; `tokens.css:26` text-muted tối → 5.34:1 |
| 29 | 09-Code (light) | Dòng code mẫu syntax highlight kém | P2 | **TỪ CHỐI** | Chưa có Monaco (textarea plain — không có syntax highlight để chỉnh); sau #27 editor text = foreground trên surface → 9.48:1 |
| 30 | 10-Benchmark (light) | Empty state thiếu nhất quán màu | P2 | **TỪ CHỐI** | BenchmarkPanel đã dùng `EmptyState` chung toàn app (icon+title+desc+action) — nhất quán sẵn; icon contrast cải thiện qua `EmptyState.vue:30` |
| 31 | 10-Benchmark (light) | Nút "Chạy benchmark" không nổi bật | P2 | **ĐÃ SỬA** | `BenchmarkPanel.vue:311` — size `lg`; primary `#007E72` (white-on 4.97:1) qua `tokens.css:10` |
| 32 | 10-Benchmark (light+dark) | Spacing nút chọn lệch; "Benchmark Lab" không rõ | P2 | **ĐÃ SỬA** | `BenchmarkPanel.vue:397` — chips gap 4→8px; `BenchmarkView.vue:126` — title 2xl→3xl; mint gradient tối (`palettes.css:37`) |
| 33 | 11-Leaderboard (light) | Empty state không nổi bật | P2 | **TỪ CHỐI** | Đã dùng EmptyState chung (icon target + title + desc); icon contrast qua `EmptyState.vue:30` |
| 34 | 11-Leaderboard (light) | Thiếu spacing giữa phần tử bảng | P2 | **ĐÃ SỬA** | `LeaderboardView.vue:432` — row padding 8→12px (`0.75rem`) |
| 35 | 11-Leaderboard (dark) | Header "DSA Visual" + menu khó đọc | P1 | **TỪ CHỐI** | Nhận xét chéo #1 — dark đo 10.66:1 (brand) / 9.52:1 (link) đạt |
| 36 | 12-Profile (light) | Hero "Sinh viên mẫu" chữ khó đọc | P1 | **ĐÃ SỬA** | Aurora gradient light tối đi (`palettes.css:19`) → text-gradient name trên nền ≥ 4.64:1 |
| 37 | 12-Profile (light) | Progress bar chồng lấn (TB) | P1 | **TỪ CHỐI** | Đo overflow **0px**; hero = flex column (không absolute) — không thể chồng lấn. Model tưởng tượng |
| 38 | 12-Profile (dark) | "0 ngày streak" khó đọc | P1 | **ĐÃ SỬA** | Streak chip = sunset gradient + `--color-on-primary`: light sunset tối đi (`palettes.css:28`) → chữ trắng ≥ 4.93:1; dark on-primary `#042F2E` trên gradient sáng ≥ 5.7:1 |
| 39 | 12-Profile (dark) | Modal "Chỉnh sửa" chữ nhỏ | P2 | **TỪ CHỐI** | Không có modal "Chỉnh sửa" — nút mở **tab Cài đặt** (`ProfileView.vue:213`); label Input = text-sm foreground (9.48:1) |

**Tổng: 23 ĐÃ SỬA · 16 TỪ CHỐI (lý do: đo đạt contrast 4.5:1+ / overflow 0px / model nhiễu-tưởng tượng / thiết kế có sẵn)**

---

## 2. Tóm tắt thay đổi (16 file)

### Token (nguồn — sửa đúng chỗ)
| File | Thay đổi | Contrast sau |
|------|----------|--------------|
| `src/styles/palettes.css:19-39` | Light gradient tối đi: Aurora `0.72/0.8/0.66 → 0.52/0.53/0.48`, Sunset `0.83/0.79/0.68 → 0.55/0.55/0.45`, Mint `0.87/0.8/0.72 → 0.54/0.53/0.5` | Chữ trắng trên gradient **4.68–8.23:1** (trước 1.29–3.31:1) |
| `src/styles/tokens.css:10,26,27` | Light: primary `#0D9488→#007E72`, text-muted `#5E7A77→#56706D`, text-disabled `#9CB5B2→#56706D` | primary-on-white 4.97:1 · text-muted 5.12:1 (bg) / 5.34:1 (white) |
| `src/styles/tailwind.css:55,72,108` | shadcn primary/ring `oklch(0.52 0.12 185)`; dark border `0.32→0.4` | white-on-primary 4.97:1 · border dark rõ |

### Component / View
| File | Thay đổi | Nhận xét |
|------|----------|----------|
| `src/views/HomeView.vue` | Hero title clamp 3.5rem, CTA gap 24px, blob opacity 0.28, dark scrim 0.62 | #2 #3 #4 #6 |
| `src/views/LessonView.vue` | Title 3xl, dark scrim 0.62 | #10 #12 |
| `src/views/LoginView.vue` | Title 3xl, form gap 16px, aside dark scrim | #7 #8 |
| `src/views/CodeRunnerView.vue` | Editor theme-aware (light: surface/foreground; dark: dark) | #27 |
| `src/components/ui/EmptyState.vue` | Icon contrast `text-foreground/80` | #28 (dùng chung #30 #33) |
| `src/components/ladder/QuizStage.vue` | Option padding 12px 16px | #17 #18 |
| `src/components/benchmark/BenchmarkPanel.vue` | Chips gap 8px, nút Chạy size lg | #31 #32 |
| `src/views/BenchmarkView.vue` | Title 3xl | #32 |
| `src/views/LeaderboardView.vue` | Row padding 12px | #34 |
| `src/views/LabView.vue` | Info text base | #24 |
| `src/App.vue` | Footer thêm "Liên hệ" | #5 |
| `src/components/simulator/PseudocodePanel.vue` | Line padding/gap tăng nhẹ | #13 |
| `src/views/SimulatorView.vue` | Actions gap 16px | #15 |
| `src/views/LadderView.vue` | Title 3xl | #21 |

---

## 3. Verify

| Hạng mục | Kết quả |
|----------|---------|
| `npm run build` | ✅ 0 lỗi (vue-tsc + vite build) |
| `npm test` (vitest) | ✅ **89/89 PASS** |
| `npx playwright test` | ✅ **13/13 PASS** |
| Contrast text-muted dark | ✅ đo thực tế: `#99F6E4` trên surface `#0F3D3A` = **9.52:1**; trên bg `#042F2E` = 11.48:1 |
| Contrast text-muted light | ✅ `#56706D` trên bg `#F0FDFA` = **5.12:1**; trên white = 5.34:1 (trước 4.45:1) |
| Contrast primary light | ✅ `#007E72` trên white = **4.97:1** (trước 3.74:1); white-on-primary 4.97:1 |
| Contrast chữ trắng trên gradient light | ✅ **4.68–8.23:1** (từng stop, tính toán học) |
| Contrast chữ trắng trên gradient dark (+scrim 0.62) | ✅ **5.37–8.30:1** |
| Header (AppHeader) | ✅ light 9.48:1 · dark 10.66:1 (brand), link light 5.34:1 · dark 9.52:1 |
| Horizontal overflow | ✅ 0px cả 24 màn (light+dark) |
| Console error | ✅ 0 lỗi (loại trừ 401 refresh guest như đợt G) |
| Ảnh sau | ✅ `docs/work/r2-fixed-01..12[-dark].png` (24 file) + `r2-fixed-screenshot-results.json` |

**Ảnh:** `docs/work/r2-fixed-*.png` (trước = `r2-*.png` của GP-T9a, sau = `r2-fixed-*.png`)

---

## 4. Ghi chú

- Model qwen2.5vl:3b có nhiễu đáng kể: các nhận xét "header khó đọc" (#1/#9/#35), "dark text kém contrast" (#16/#20/#35), "chồng lấn" (#11/#25/#37) đều **bị bác bỏ bằng đo thực tế** (contrast 4.5:1+ / overflow 0px) — đúng như cảnh báo trong gp-t9a §3.
- 23 nhận xét sửa được chủ yếu nằm trong 3 cụm token: (a) light gradient quá sáng (ảnh hưởng chữ trắng + text-gradient), (b) `--color-text-muted`/`--color-primary` light sát dưới 4.5:1, (c) dark hero thiếu scrim cho chữ trắng.
- Không đụng `engines/*`, `CanvasArea.vue`, renderers — chỉ chrome/token/view.
- Commit: xem git log nhánh `feature/ux-review2`.
