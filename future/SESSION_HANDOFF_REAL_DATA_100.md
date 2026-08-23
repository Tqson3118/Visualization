# HANDOFF — Real Data 100% / 23 Screenshots

**Repo:** D:/FPT/neww
**Branch:** feature/real-data-100 (ahead 1 of origin/dev)
**Prompt:** future/PROMPT_REAL_DATA_100_SCREENSHOTS_v5.md

## Read first
- docs/superpowers/plans/2026-08-19-real-data-100.md
- future/real-data-consistency.md
- future/real-data-probe-before.md
- source/VisualizationDSA/AGENTS.md

## Completed this session
- Read entire v5 prompt and repository AGENTS instructions.
- Added frontend/vite.real.config.ts: Vue + Tailwind, @ alias, port 5174, proxy to backend 5055.
- Corrected root docker-compose.yml SQL Server fallback password and backend connection fallback to Dsa!2026Pass; recreated neww-sqlserver-1; health verified healthy.
- Removed hardcoded SQL connection/JWT key from production config. Development appsettings has local JWT key so login works.
- Repaired future/start_backend.ps1 path escaping and environment variables.
- Fixed frontend/src/views/AdminStatsView.vue KPI type narrowing.
- Added missing coverage in RealDataSeeder.cs: 6 Codelabs, 180 ModuleItems, 8 TheoryArticles, 3 LearningPaths, 18 LearningPathNodes, 12 LessonReviews.
- Replaced stale seed success message in Program.cs.
- Added local credential/log patterns to .gitignore.

## Verified
- SQL Server healthy.
- Authenticated student probe returned 200 for auth/me, progress/me, me/gamification, me/hearts, me/streak, me/quests, me/inventory, achievements, premium/status, simulations.
- Backend build PASS, 0 errors after stopping WebApi process before build.
- Frontend build PASS.
- Vitest PASS: 646/646.
- Current DB counts: Users 137; Courses 20; CourseModules 60; Lessons 100; Quizzes 52; QuizQuestions 418; Classrooms 12; Enrollments 120; Orders 30; Badges 22 definitions and 14 earned for anchor; Quests 7; ShopItems 10; anchor inventory 2; CourseReviews 80; LessonComments 2; LessonReviews 12; GemTransactions 16; Codelabs 6; ModuleItems 180; TheoryArticles 8; LearningPaths 3; LearningPathNodes 18.

## Blocking / unresolved
- No checkpoint commits yet. Do not claim final READY.
- Cold public frontend sends empty POST /api/v1/auth/refresh and gets 415; screenshot flow must intercept only refresh as prompt allows, or use minimal development-only handling.
- AdminStatsView still has hardcoded role donut Student 70 / Teacher 20 / Admin 10; replace with real role counts from backend before screenshots.
- Need write future/real-data-probe-after.md and run full endpoint probe.
- Need run SQL consistency checks from prompt section 2.5, especially review/progress, gem ledger, premium/order, duplicate emails and role validity.
- Documentation assets missing: tailieu/diagrams/dsa-visual-schema.dbml, BUSINESS-DATABASE-GUIDE.md, word_format_run.py, docx verifiers; screenshot folder lacks complete required v5 set.
- Existing root working tree is intentionally dirty with unrelated migration changes. Never git reset; never git add -A; stage explicit paths only.
- Prompt contradictions: admin text says 128 vs actual 137 users; target says 14 badges vs 22 definitions/14 earned. Report actual counts with explanation.
- git diff --check reports pre-existing trailing whitespace in source/VisualizationDSA/plan/tracking/errors.md and that file is invalid UTF-8; do not rewrite casually.

## Runtime
- Backend may be running as WebApi on port 5055. Check Get-NetTCPConnection -LocalPort 5055 -State Listen and Get-Process -Name WebApi.
- Frontend should be at http://127.0.0.1:5174. Verify after resume.
- SQL Server container neww-sqlserver-1 at localhost:1433, password Dsa!2026Pass.

## Next-session order
1. pwd; git status --short --branch; read this file, prompt, and consistency report.
2. Add real role distribution field to AdminStatsController and typed AdminStatsDto; replace hardcoded ROLES; run focused test, build, Vitest.
3. Probe all Phase A endpoints with student/admin and save real-data-probe-after.md.
4. Run all prompt 2.5 consistency queries and fix seed if required.
5. Capture exactly 23 screenshots with Playwright 1440x900 dark theme; only intercept POST /api/v1/auth/refresh; all data real; DOM and console QC.
6. Create DBML/database guide/API sync and implement safe DOCX verification/embed flow; do not embed benchmark image and skip Hình 4.10.
7. Run final gates and stage explicit paths for checkpoint commits.

## Changed files this session
- docs/superpowers/plans/2026-08-19-real-data-100.md
- frontend/vite.real.config.ts
- frontend/src/views/AdminStatsView.vue
- docker-compose.yml
- .gitignore
- future/start_backend.ps1
- future/real-data-probe-before.md
- future/real-data-consistency.md
- source/VisualizationDSA/backend/src/Infrastructure/Data/RealDataSeeder.cs
- source/VisualizationDSA/backend/src/WebApi/Program.cs
- source/VisualizationDSA/backend/src/WebApi/appsettings.json
- source/VisualizationDSA/backend/src/WebApi/appsettings.Production.json

## Additional frontend contract audit
- Frontend contracts confirmed: refresh response is {accessToken,expiresIn}; quests use raw backend shape {id,questId,title,type,progress,target,claimed,reward:{gems,xp}}; inventory uses {id,itemId,itemKey,name,quantity,type,isEquipped,expiresAt}; leaderboard uses PagedResponse {items,page,pageSize,total,totalPages}.
- mockApi.ts mocks only a subset of the new endpoints; its fallback returns 404. Screenshot Phase C must not reuse mockApi except the permitted refresh interception.
- AdminStatsView still has illustrative hardcoded role distribution and stale KPI comments/grid assumptions; replace with real role counts and reconcile layout.
- Frontend settings canonical path is /settings, although prompt table mentions /admin/settings; alias or document both before final gates.
- Additional routes needing probe/verification include learning-path, topics, exercises, favorites, code-runs, benchmark, class detail, admin users/content/settings.

## Runtime note
- Job pwsh-140 exited 1 after a long run. Its captured output showed the real-data seed completed and inserted the new coverage rows; the likely failure is the service bind/termination state, so verify port 5055 before restarting.

## Resume instruction
Start with: Đã đọc handoff, tiếp tục từ mục Next-session order.
