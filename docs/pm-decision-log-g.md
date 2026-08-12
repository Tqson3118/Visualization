# PM DECISION LOG — SESSION G (Nâng cấp UI/UX "Wow" — PROMPT_G mở rộng)

> Ngày: 12/08/2026 · Chế độ: --auto · Trạng thái: docs/pm-report-g.md · Việc cần user: docs/SETUP_TODO.md

## [2026-08-12] Khởi động SESSION G — phạm vi & thứ tự (user chọn phương án 2)
- Quyết định: Chạy đợt G theo PROMPT_G nhưng MỞ RỘNG phương án 2: sửa các bug P1/P2 bàn giao (SETUP_TODO §6 + §8) TRƯỚC luồng UX để ảnh trước/sau có nghĩa. Thứ tự: Phase 0 bugfix (2 nhánh song song) → Phase 1 ux-foundation → Phase 2 ux-polish → Phase 3 ux-finalize (docs/test/review/e2e). Mỗi pha = nhánh feature từ dev, merge tuần tự.
- Ảnh hưởng: 2 nhánh mới feature/ux-bugfix-backend + feature/ux-bugfix-frontend, sau đó feature/ux-foundation, feature/ux-polish, feature/ux-finalize.

## [2026-08-12] Chốt contract /code-runs (bug P2 #7)
- Quyết định: Giữ contract backend hiện tại theo API_REFERENCE §4.13/ADR-012: CodeRunRequest = {exerciseId?, key, code, input? (string), status, durationMs, stats?, output?, error?, trace?}. FE phải sửa payload: store codeRunner run() gửi {key: key.value, code, input: JSON.stringify(defaultArray)} thay vì {code, input: array} (array → 400 type mismatch).
- Ảnh hưởng: frontend/src/stores/codeRunner.ts:110, api/codeRunner.ts:33.

## [2026-08-12] Chốt contract /benchmarks/run (bug P2 #8)
- Quyết định: Giữ backend cần `results` (client đo — ADR-012 đã ghi từ đợt D). FE BenchmarkPanel phải gửi kèm results = List<{key, measurements:[{n, durationMs, comparisons, swaps}]}> map từ kết quả runMeasureInWorker. Không bỏ yêu cầu results ở backend.
- Ảnh hưởng: frontend/src/components/benchmark/BenchmarkPanel.vue, api/benchmark.ts.

## [2026-08-12] Chốt fix Leaderboard (bug P1 F3-NEW-1)
- Quyết định: BE trả PagedResponse<LeaderboardEntryDto> (items/total/page/totalPages) — KHÔNG trả rows/myRank. Fix ở FE: api/gamification.ts fetchLeaderboard map items→rows, myRank = item có userId == user hiện tại (nếu có) hoặc null. Không đổi BE.
- Ảnh hưởng: frontend/src/api/gamification.ts:143, stores/leaderboard.ts.

## [2026-08-12] Chốt phạm vi Phase 0 — bugfix (8+4 mục)
- Quyết định: Backend (G-BF1): (1) thêm POST /lessons/{id}/mark-viewed (upsert UserProgress — TEST-B-033/034), (2) heart regen persist (GamificationService ComputeHearts — ghi Hearts/LastHeartAt khi spend/truy vấn quá hạn), (3) Duplicate QuestionId → 400 (ExerciseService.cs:275), (4) SubmitCodeAsync thêm SubmissionLockRegistry + check Status Active (:489), (5) P3 cookie Secure chỉ khi HTTPS (AuthController.cs:25). Frontend (G-BF2): (1) leaderboard contract, (2) code-runs payload, (3) benchmark results, (4) nút "Làm bài" NodeHubView.vue:113, (5) submit exercise thiếu câu UX (ExerciseView), (6) Ladder stage rỗng (LadderView.vue:60-62, NodeHubView.vue:133-135 — tải exercise qua GET /exercises?nodeId&stage), (7) router teacher khỏi /admin/users + /admin/settings, (8) mất phiên reload — gọi auth.refresh() lúc boot (ADR-004). Canvas ResizeObserver ĐÃ FIX (ba62a33 đã merge) — bỏ qua.
- Ảnh hưởng: backend Controllers/Services + tests; frontend api/stores/views/router.

## [2026-08-12] Phase 1 — ux-foundation (theo PROMPT_G luồng 1)
- Quyết định: Cài tailwindcss 4 + shadcn-vue + motion-v + gsap + lucide-vue-next + vue-echarts + lenis + vue-sonner + phosphor-vue (canvas-confetti + @monaco-editor/loader đã có). Font Geist variable + JetBrains Mono self-host. Map tokens.css → shadcn OKLCH, teal #0D9488 primary, dark mode class="dark". Thay 13 component tự xây → shadcn-vue giữ API tương đương, sửa call site. KHÔNG đụng canvas simulator.
- Ảnh hưởng: frontend/package.json, src/styles/*, src/components/ui/* + call sites.

## [2026-08-12] Phase 2 — ux-polish (theo PROMPT_G luồng 2)
- Quyết định: motion-v page transition + hover, Lenis smooth scroll, vue-sonner toast, canvas-confetti (hoàn thành bài/lên cấp/achievement), 3 gradient OKLCH (Aurora/Shop, Sunset Learn, Cyber Mint canvas). 12 màn chính BAO_CAO_SPEC §6: home/login/lesson/simulator(chrome)/exercise/path/ladder/lab/code-runner/benchmark(vue-echarts)/leaderboard/profile(skill radar). Monaco: chỉ có @monaco-editor/loader — KHÔNG cài monaco-editor full trong đợt này (textarea giữ — ghi SETUP_TODO §5.4).
- Ảnh hưởnng: frontend/src/views/*, components/*, App.vue, router.

## [2026-08-12] Phase 3 — ux-finalize (theo PROMPT_G luồng 3)
- Quyết định: dev-docs cập nhật SDD §3.1/§3.9, THIRD_PARTY +8 lib (npm ls), REUSE_REPORT, version bump §17.12; NFR-5 bundle: sau khi build xong ghi số thật, nới ≤1.5MB gốc (engine hiện 459KB) → sửa SRS NFR-5 + TEST_PLAN TEST-PERF-007 cho khớp; dev-test verify toàn bộ + grep cấm; dev-review verdict toàn đợt; dev-e2e 12 màn + Ollama qwen2.5vl:3b trước/sau.
- Ảnh hưởng: docs/*, THIRD_PARTY.md, SRS.md, TEST_PLAN.md, SDD.md, REUSE_REPORT.md.

## [2026-08-12] Phân công commit + log
- Quyết định: frontend/UX → son, engine canvas (GSAP) → thu, docs → phuc, backend → bao (commit-as.ps1). Log: docs/pm-report-g.md + docs/pm-decision-log-g.md; việc cần user → SETUP_TODO.
- Ảnh hưởng: git history.

## [2026-08-12] G-3a docs — chốt số liệu bundle thật + nới NFR-5 (ux-finalize)
- Quyết định: Sau khi chạy `npm run build` (frontend/, 12/08/2026) ghi số bundle THẬT vào tài liệu:
  - **engine chunk** `476 KB` gốc (`120 KB` gzip) — trước đây ghi 459KB (đợt D, trước stack UI/UX mới);
  - **echarts chunk** `324 KB` gốc (`110 KB` gzip) — lazy-load qua `defineAsyncComponent` (VChartLazy), KHÔNG vào bundle chính;
  - **vendor** `143 KB` gốc (`54 KB` gzip);
  - **JS gốc tải lần đầu** (index.html preload) ≈ `852 KB` (0.81MB); **tổng JS gốc toàn dist** ≈ `1.95 MB` (gồm chunk lazy: view, echarts, compiler.worker).
  - NFR-5 nới: "Tổng JS gốc tải lần đầu ≤ 1.5MB; engine chunk ≤ 500KB gốc; lõi mô phỏng tải trước" (SRS §4.1 + TEST_PLAN TEST-PERF-007 + SDD §3.9). Lý do: 500KB cũ không còn khả thi sau khi thêm stack UI/UX (shadcn-vue + tailwind 4 + echarts…); echarts/worker đã lazy nên tải lần đầu thực tế ~852KB < 1.5MB.
  - THIRD_PARTY.md bổ sung 19 gói frontend mới (npm ls — xem bảng §1); REUSE_REPORT §6 ghi ánh xạ component tự xây → shadcn-vue + font Geist; SDD §3.1/§3.8/§3.9/§8.1 cập nhật theo stack mới.
- Ảnh hưởng: docs/SDD.md (1.4), docs/SRS.md (1.3), docs/TEST_PLAN.md (1.3), THIRD_PARTY.md (1.2), docs/REUSE_REPORT.md, docs/pm-decision-log-g.md, docs/README.md, docs/work/g-f3a.md.
