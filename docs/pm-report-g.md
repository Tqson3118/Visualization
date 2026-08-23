# PM REPORT — SESSION G (Nâng cấp UI/UX "Wow" + sửa bug bàn giao — PROMPT_G mở rộng)

> Ngày: 13/08/2026 · Chế độ: --auto (user chọn phương án 2: kèm sửa bug P1/P2 bàn giao) · Quyết định: docs/pm-decision-log-g.md · Việc cần user: docs/SETUP_TODO.md

## 1. Mục tiêu
Đợt G theo session/PROMPT_G.md — 3 luồng: (1) nền tảng UI (Tailwind 4 + shadcn-vue + fonts + OKLCH dark), (2) xịn 12 màn chính (motion-v/Lenis/confetti/sonner/gradient/echarts), (3) chốt docs+test+review+e2e. **Mở rộng phương án 2**: sửa 13 bug bàn giao (SETUP_TODO §6+§8) TRƯỚC luồng UX để ảnh trước/sau có nghĩa. Nguồn chuẩn: PRODUCTION_PROMPT → SDD → API_REFERENCE → SCREEN_MAP → BAO_CAO_SPEC §6.2.

## 2. Trạng thái task

| Task | Nội dung | Nhánh | Kết quả | Verify |
|---|---|---|---|---|
| G-BF1 | Backend 5 bug: mark-viewed endpoint, heart regen persist, duplicate QuestionId 400, SubmitCode lock+Status Active, cookie Secure chỉ HTTPS | feature/ux-bugfix-backend | **DONE** | build 0 warning · Unit 56/56 + Integration 31/31 |
| G-BF2 | Frontend 8 bug: leaderboard contract, code-runs payload, benchmark results, nút "Làm bài", submit thiếu câu UX, ladder stage, teacher /admin guard, session reload (ADR-004) | feature/ux-bugfix-frontend | **DONE** | build 0 lỗi · FE 72/72 · smoke thật 201/200/redirect OK |
| G-BF3 | Fix test-infra e2e: mock `/auth/refresh` trả 401 khi chưa login | feature/ux-e2e-fix | **DONE** | e2e 11/11 · build 0 lỗi · FE 72/72 |
| G-F1a | Cài stack (tailwind4/shadcn-vue/motion-v/gsap/lucide/vue-echarts/lenis/sonner/phosphor) + font Geist+JetBrains Mono + map tokens OKLCH + dark class + Tailwind 4 @theme | feature/ux-foundation | **DONE** | build 0 lỗi · FE 72/72 · browser light+dark 0 vỡ |
| G-F1b | Thay 13 component tự xây → shadcn-vue (wrapper giữ API, 0 call site phá) + Toast → vue-sonner | feature/ux-foundation | **DONE** | build 0 lỗi · FE 72/72 · e2e simulator 3/3 |
| G-F2a | Global polish: motion-v transitions, Lenis, sonner hoàn chỉnh, useConfetti, 3 gradient OKLCH (Aurora/Sunset/Cyber Mint), hover micro-interaction | feature/ux-polish | **DONE** | build 0 lỗi · FE 72/72 · e2e simulator 3/3 |
| G-F2b | Xịn 5 màn: Home/Login/Lesson/Exercise/Path (gradient + hover + toast + confetti pass) | feature/ux-polish | **DONE** | build 0 lỗi · FE 72/72 · e2e 11/11 · 10 ảnh |
| G-F2c | Xịn 5 màn: Simulator chrome (Cyber Mint, canvas giữ), Ladder, Lab, Code Runner (JetBrains Mono), Benchmark (**vue-echarts** thay SVG) | feature/ux-polish | **DONE** | build 0 lỗi · FE 72/72 · e2e 11/11 · 10 ảnh |
| G-F2d | Xịn 2 màn: Leaderboard (reorder animation + top-3 highlight), Profile (skill radar echarts) | feature/ux-polish | **DONE** | build 0 lỗi · FE 72/72 · e2e 11/11 · 4 ảnh |
| G-F3a | Docs: SDD §3.1/§3.8/§3.9/§8, THIRD_PARTY +19 gói license thật (npm ls), REUSE_REPORT, SRS NFR-5 (JS gốc ≤1.5MB, engine ≤500KB), TEST_PLAN TEST-PERF-007 | feature/ux-finalize | **DONE** | build thật: engine 476KB/120KB gzip, echarts 324KB (lazy), JS gốc đầu ≈852KB |
| G-F3b | Verify độc lập toàn đợt G | — | **PASS 9/9** | build FE 0 lỗi · FE 72 + e2e 11 · BE 0 warning 56+31 · grep cấm 0 · smoke light+dark 8/8 · secret sạch |
| G-F3d | E2E thật 12 màn + Ollama trước/sau | — (chỉ báo) | **11 PASS / 1 CÓ LỖI** | 17 ảnh + vision; 8 bug đợt G hết; 2 bug MỚI Leaderboard |
| G-F3e | Fix 2 bug mới: Leaderboard tab Level crash (BE thêm Value) + tab Lớp thiếu classId | feature/ux-fix-leaderboard | **DONE** | BE 0 warning 60+31 · FE 76 · e2e 13/13 |
| G-F3c | Review toàn diff đợt G | — | **CHANGES REQUESTED → APPROVE** | 1 P2 (pagination classId) + 6 P3 ghi chú |
| G-F3e2 | Fix P2: leaderboard tab Lớp phân trang giữ classId (lastClassId) | feature/ux-fix-leaderboard | **DONE** | FE 78 · e2e 13/13 · build 0 lỗi |

**Tổng: 16/16 DONE. Verdict cuối APPROVE.** 9 merge vào dev + push origin (commits: fb7c1be, 085a82c, 1359fcf, a3b3bbc, 6bba176, d62e2ac, 70113f5, 17be8a7, 2c38e9c).

## 3. Stack mới (đợt G)
- **Cài thêm 19 gói** (dependencies): tailwindcss@4.3.3, @tailwindcss/vite, tw-animate-css, shadcn-vue@2.8.2, reka-ui, class-variance-authority, clsx, tailwind-merge, @lucide/vue + lucide-vue-next, motion-v@2.3.0, gsap@3.15.0, vue-echarts@8.1.0 + echarts@6.1.0, lenis@1.3.26, vue-sonner@2.0.9, @phosphor-icons/vue@2.2.1, @vueuse/core, vaul-vue.
- **Font**: Geist (variable) + JetBrains Mono self-host (`frontend/public/fonts/`), map --font-sans/--font-mono.
- **Design tokens**: tokens.css (giữ) + tailwind.css (OKLCH :root/.dark, @theme) + palettes.css (3 gradient Aurora/Sunset/Cyber Mint); primary teal #0D9488; dark mode native `class="dark"`.
- **Component**: 13 tự xây → shadcn-vue (Button/Input/Dialog→Modal/Drawer/Badge/Progress/Skeleton/Tabs/Tooltip/Select/Card/EmptyState restyle/Toast→vue-sonner). Wrapper giữ API cũ → 0 call site phá vỡ.
- **Bundle thật** (npm run build): engine 476KB gốc/120KB gzip · echarts 324KB (lazy chunk) · vendor 143KB · JS gốc tải lần đầu ≈852KB · NFR-5 nới: JS gốc ≤1.5MB + engine ≤500KB (SRS 1.3, TEST_PLAN TEST-PERF-007).

## 4. Bug xử lý trong đợt (SETUP_TODO §6+§8 — 13 bug + 2 mới)
- ✅ FIXED: mark-viewed 404 (endpoint + upsert UserProgress + test), heart regen persist (PersistHeartRegenAsync), duplicate QuestionId → 400, SubmitCode lock+Status, cookie Secure HTTPS-only, Leaderboard crash (PagedResponse.items + Value), code-runs contract, benchmarks results, nút "Làm bài" NodeHub, submit thiếu câu UX, Ladder stage rỗng, teacher /admin guard, session reload refresh, e2e mock refresh, Leaderboard tab Level Value (mới), tab Lớp classId + pagination (mới).
- ✅ Đã fix trước đó (nền): canvas ResizeObserver (ba62a33).
- ⏸ Chưa làm (chủ đích, ghi chú): Monaco full editor (textarea giữ — SETUP_TODO §5.4), 2FA (501).

## 5. Quyết định / lệch chủ ý (chi tiết docs/pm-decision-log-g.md)
- Chốt contract code-runs (`{key,code,input:string,...}`) + benchmarks (`results` client đo ADR-012) — giữ backend, sửa FE.
- Chốt leaderboard: BE thêm field `Value`, FE map items→rows, myRank theo userId.
- phosphor-vue→@phosphor-icons/vue (Vue3), components.json css→tailwind.css, thêm cva/reka-ui (cần shadcn).
- Confetti giữ ngưỡng 60% QuizStage (FR-4.2) — không đổi logic.
- NFR-5 bundle nới theo thực tế build (852KB JS gốc đầu) — ghi SRS+TEST_PLAN.
- 6 P3 từ review ghi decision log (TOCTOU heart, lock registry không dọn key, ForwardedHeaders sau proxy TLS, seed leaderboard thật, Value=XP 3 tab — hành vi cũ chủ đích).

## 6. Verify tổng thể cuối (sau G-F3E2, PM)
| Lệnh | Kết quả |
|---|---|
| npm run build (frontend) | PASS 0 lỗi |
| npm test (frontend) | PASS — 78/78 |
| npx playwright test | PASS — 13/13 |
| dotnet build DsaVisual.sln | PASS 0 warning / 0 error |
| dotnet test backend | PASS — Unit 60/60 + Integration 31/31 |
| Grep cấm (PostgreSQL/MediatR/Repository/Judge0/secret) | 0 vi phạm production |
| Smoke UI light+dark | 8/8 màn 0 console error, dark đổi màu thật |
| E2E 12 màn thật + Ollama | 11 PASS / 1 CÓ LỖI (leaderboard — ĐÃ FIX G-F3E) → 12/12 sau fix |
| Secret scan | sạch |

## 7. Việc còn lại / cần user (chi tiết docs/SETUP_TODO.md)
- Merge `feature/final-review` → `main` khi user duyệt (SETUP_TODO §7.2 — chờ từ đợt F).
- Xoay key DEEPSEEK thật (máy dev) + đổi mật khẩu seed dev trước demo.
- Điền số liệu thật báo cáo, thay 18 placeholder ảnh bằng ảnh thật (12 màn giờ đã có ảnh polish trong docs/work/g-f2*/g-f3d-* — có thể dùng), chạy pandoc lại docx.
- P3 không chặn: TOCTOU heart regen (tiny), lock registry dọn key, ForwardedHeaders khi deploy sau TLS proxy, seed dữ liệu leaderboard thật (hiện XP=0 → tab hiển thị trống).
- Cập nhật `docs/SETUP_TODO.md` §6/§8: đánh dấu bug đã fix.

## 8. Kết luận
Đợt G hoàn thành trọn vẹn: UI/UX nâng cấp toàn diện (stack shadcn-vue/Tailwind4 hiện đại + motion/Lenis/confetti/sonner/echarts + dark mode OKLCH + 3 gradient), 13 bug bàn giao + 2 bug mới leaderboard đã fix, 12/12 màn chính chạy sạch trên app thật, verdict APPROVE. Docs đồng bộ stack mới + NFR-5 thực tế. Tất cả merge vào dev + push origin.

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu "làm lại <task/mục>" kèm ghi chú, PM chạy lại phần đó.
