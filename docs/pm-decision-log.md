
## [2026-08-14] Khoi dong PROMPT_URGENT_FIX_REDESIGN (urgent-fix)
- Quyet dinh: Tao worktree D:\FPT\neww\trees\urgent-fix -b feature/urgent-fix tu origin/dev (HEAD bcba9dd). Tat ca fix/redesign lam tren worktree nay, PR base dev. Khong dung D:\FPT\neww (tree chinh).
- Anh huong: toan bo FE work dot nay; tree chinh chi dung de fetch/worktree.

## [2026-08-14] Nhom C redesign - 2 lech spec (da chap nhan)
- Quyet dinh 1 (C2 NodeHub): Practice tab GIU LadderShell (stepper quiz/lab/code that) thay vi card list "Lam bai" nhu spec 3.5.2. Ly do: thay se pha chuc nang that; nguyen tac "GIU NGUYEN chuc nang" cao hon.
- Quyet dinh 2 (C3 Catalog): giu hover shadow nhe (scale 1.01 + shadow-md chi khi hover, co prefers-reduced-motion guard) du DESIGN.md §6 cam shadow card — task 3.3.3 yeu cau shadow.
- Anh huong: NodeHubView.vue, SimulationsView.vue; bo sung i18n + API node-completion cho buoc sau.

## [2026-08-14] Verify tong + quyet dinh truoc PR urgent-fix
- Quyet dinh: (1) Giu nguyen hanh vi practice mode: nguoi dung co the practice ngay vi trace generate san khi load (steps.length > 0 luon voi sim hop le) — bug goc "bam khong thay gi" da het, hint van hien khi steps that su rong. Khong doi sang !hasPlayedOnce (giam tien ich). (2) TODO: them DialogTitle an cho Drawer AdminUsers (warning a11y reka-ui, khong chan). (3) TODO: can admin account verify AdminUsers drawer bang tay (da verify bang admin@system.local trong e2e).
- Anh huong: ManualPracticePanel.vue + SimulatorView.vue (giu nguyen); AdminUsersView.vue (TODO a11y).
- Verify tong: vue-tsc 0 loi, 158/158 tests PASS, build OK, E2E APPROVE (Ollama vision 6.5-7.4/10, console 0 loi moi, responsive OK).

## [2026-08-14] Hoan thien 2 TODO FE (commit 6f6d285 vao PR #22)
- Quyet dinh: (1) i18n hoa ~71 key moi vao vi.ts (practice/simulator/nodeHub/admin.users/admin.ladder), chuyen toan bo hardcode o 5 file; (2) a11y: Drawer.vue dung DrawerTitle + DrawerDescription sr-only (het warning reka-ui), AdminUsersView truyen description dong; (3) 1b user-passed count GIU NGUYEN an + TODO(backend) vi ExerciseSummaryDto tren dev chua co field (PR #23 chua merge, khong co file do).
- Anh huong: vi.ts, ManualPracticePanel.vue, SimulatorView.vue, NodeHubView.vue, AdminLadderView.vue, AdminUsersView.vue, ui/Drawer.vue. Verify: 0 loi type, 158/158 tests, build OK, console 0 warning.

## [2026-08-14] 1b user-passed count — hoan tat (2 commit moi vao PR #22)
- Quyet dinh: (1) Backend them CompletedByUserCount vao ExerciseSummaryDto (DTO-only, KHONG migration) — dem distinct user co best score >= MaxScore, hop 2 nguon ExerciseSubmissions + CodeSubmissions (cach A pre-load 2 query); 5 unit test moi; docs API_REFERENCE §4.6 cap nhat. (2) FE them field optional + badge "N user da qua" trong AdminLadderView + i18n.
- Anh huong: ExerciseSummaryDto.cs, ExerciseService.cs, ExerciseServiceTests.cs, API_REFERENCE.md (commit ea6c9a0, tac gia bao); exercises.ts, AdminLadderView.vue, vi.ts (commit f0fba71, tac gia son). Verify: backend 142 unit + 77 integration PASS; FE 0 loi type, 158/158 vitest, build OK.

## [2026-08-14] Refactor UI CSS — ch?y theo file dispatch
- Quy?t d?nh: th?c hi?n PROMPT_REFACTOR_VIEWS_CSS.md theo dúng spec (4 shared components + 9 views, gi?m >=35% scoped CSS, build + 174/174 test PASS). User dã l?nh "d?c và th?c hi?n" + "continue task" -> coi plan dã duy?t, không h?i l?i checkpoint.
- Quy?t d?nh: làm vi?c trên worktree m?i trees/refactor-ui-css (branch feature/refactor-ui-css, base dev, PR base dev) dúng m?c 7 spec; junction node_modules -> frontend/node_modules main d? không cài l?i.
- Quy?t d?nh: tách thành 6 task nh? (A: 4 components; B: 5 admin views; C: 3 class views; D: HomeView; E: verify d?c l?p) — ch?ng v? context; commit theo 4 commit spec (components -> admin -> classes -> home).
- ?nh hu?ng: frontend/src/components/{ui,admin}/* m?i; 9 views trong frontend/src/views/; docs/work/refactor-ui-css.md.

## [2026-08-14] Refactor UI CSS — Task B k?t qu? + ch?p nh?n l?ch
- Quy?t d?nh: ch?p nh?n 4 test m?i AdminHeroStrip (dev-frontend thêm khi fix bug size dóng bang — computed) ? b? test gi? 178/178 PASS; tiêu chu?n "174/174" hi?u là "toàn b? suite PASS" (178/178).
- Quy?t d?nh: chu?n hóa StatCard hero value = var(--data-core) (gi?ng ClassReportView, dúng DESIGN §6) thay vì #d9dde8 cu c?a AdminStatsView — ch?p nh?n d?i màu nh? d? 2 view nh?t quán; dev dã verify browser render OK.
- ?nh hu?ng: AdminHeroStrip.vue (computed size), StatCard.vue (prop index + head hero), 5 admin views; docs/work/refactor-ui-css.md.

## [2026-08-14] Refactor UI CSS — ch?p nh?n k?t qu? verify
- Quy?t d?nh: ch?p nh?n m?c gi?m scoped CSS 9 views = -28.3% (3127 -> 2243 dòng), DU?I m?c tiêu spec 35%. Lý do: ph?n chênh n?m ? block d?c thù view (table card-stack, markdown editor, drawer rows, charts, lagging panel) — c?t thêm s? ch?m visual dã QA duy?t, r?i ro regression > l?i ích dòng CSS. Spec u?c gi?m 1330+ dòng l?c quan so v?i keep-list th?c t?.
- Quy?t d?nh: ch?p nh?n AdminHeroStrip.spec.ts (51 dòng) là file test di kèm component m?i — n?m ngoài danh sách file spec nhung h?p l?.
- Quy?t d?nh: t?o PR feature/refactor-ui-css -> dev (KHÔNG main).
- ?nh hu?ng: 4 components m?i, 9 views, docs/work/refactor-ui-css.md, docs/pm-decision-log.md.

## [2026-08-14] Backend Stabilize - khoi dong (PROMPT_BACKEND_STABILIZE.md)
- Quyet dinh: Thuc hien task dispatch BACKEND_STABILIZE (2 file session giong nhau, 1 task duy nhat): fix flaky perf#9 (timezone UTC+7 trong PerformanceGuardRegressionTests) + xu ly NU1903 (Testcontainers.MsSql). Tao worktree trees/backend-stabilize -b feature/backend-stabilize tu dev (da tao xong), PR base dev. Dung chuong trinh: fix test -> nang cap package -> review -> commit (bao) -> PR.
- Anh huong: backend/tests/DsaVisual.IntegrationTests/PerformanceGuardRegressionTests.cs, DsaVisual.IntegrationTests.csproj, docs/work/backend-stabilize.md.

## [2026-08-14] UI Redesign Admin/Teacher/Home - khoi dong (PROMPT_UI_REDESIGN_ADMIN_TEACHER_HOME.md)
- Quyet dinh: worktree trees/ui-redesign CHUA ton tai (file dispatch gia dinh da tao) -> da tao moi 	rees/ui-redesign -b feature/ui-redesign.
- Quyet dinh: PR #27 (refactor css) CHUA merge vao dev; 4 shared components (PageHero/AdminHeroStrip/StatCard/DetailSection) + DESIGN.md chi co tren feature/refactor-ui-css (2735a2c). De dam bao dung thu tu (refactor truoc, redesign sau) va co du components, tao branch feature/ui-redesign TU feature/refactor-ui-css (KHONG tu dev), PR cuoi base dev.
- Anh huong: worktree trees/ui-redesign moi; toan bo 9 views + docs/work/ui-redesign.md; PR base dev.

## [2026-08-14] Backend Stabilize - Task 3 ket qua (commit 5d26974)
- Quyet dinh 1: KHONG nang Testcontainers.MsSql (giu 4.13.0) - dev da verify NuGet + GitHub releases 02/07/2026: 4.13.0 la stable moi nhat, KHONG ton tai 4.15.x nhu gia dinh trong PROMPT_BACKEND_STABILIZE.md. Thay vao do them explicit SSH.NET 2026.0.0 (remediation chuan NU1903 transitive, ban va advisory GHSA-q939-rpr3-3284 ra 09/08/2026 sau release Testcontainers).
- Quyet dinh 2: Ghi nhan flake lanh 1 lan/16 lan chay: run dau sau build lanh 77/78 (1 test fail khong ro ten, khong tai dien; baseline stash 3/3 xanh). 3 lan lien tiep gan nhat 78/78, 78/78, 78/78 dat tieu chuan file prompt. De xuat truy vet rieng neu user muon (de lai muc ton dong).
- Anh huong: DsaVisual.IntegrationTests.csproj (+SSH.NET), docs/work/backend-stabilize.md. Verify: build 0 warning/0 error, dotnet list package --vulnerable = sach, integration 78/78 x3.

## [2026-08-14] Backend Stabilize - hoan tat, tao PR #28 (base dev)
- Quyet dinh: Sau khi dev-review APPROVE (pham vi sach 3 file, grep cam sach, SSH.NET 2026.0.0 verified tren NuGet), push feature/backend-stabilize len origin va tao PR #28 base dev (KHONG main) - https://github.com/Tqson3118/Visualization/pull/28.
- Quyet dinh: Flake lanh 77/78 (1 lan/16) khong do thay doi nay (baseline 3/3 xanh) - de lai muc ton dong, truy vet rieng neu tai dien.
- Anh huong: 2 commit (0557185 fix perf#9, 5d26974 NU1903) san sang merge; docs/work/backend-stabilize.md da cap nhat day du.

## [2026-08-14] UI Redesign - Task 2 (teacher) ket qua + chap nhan fallback du lieu
- Quyet dinh: chap nhan phuong an cua dev-ux khi backend thieu field: ClassMemberDto khong co progress/avgScore, ClassDto khong co assignmentCount -> cot tien do + diem TB dung du lieu that hien co (missingCount tu report.laggingLearners, StatCard mini 1 chi so). Khong sua backend trong dot nay (ngoai pham vi FE redesign); ghi nhan de xuat mo rong.
- Quyet dinh: nut "Nhan nhac" = copy loi nhac vao clipboard (khong co API notify/email) - chap nhan, de xuat backend sau.
- Anh huong: ClassesView.vue, ClassDetailView.vue, ClassReportView.vue, vi.ts; commit ce368c9.

## [2026-08-14] UI Redesign - E2E round 2 + quyet dinh dung viec duoi diem Ollama
- Quyet dinh: CHAP NHAN ket qua E2E round 2 (retest sau fix e1a4e13): khong con diem 2/5, 3 target fix chinh deu tang (Classes@768 Phan hoi 2->4, AdminStats@768 Luong 2->4, ClassDetail@768 Thoa man 2->3), overflow = 0 (15/15), console 0 loi, DOM checks sach. Dung viec duoi diem: cac tieu chi <=3 con lai (Tham my 15/15 = 3, Tiep can/Phan hoi/Luong/Thoa man 3 o nhieu moc) co bang chung nhieu thang cham giua 2 luot (view doi chung admin-users khong sua cung tut diem dong loat) - fix tiep se duoi theo nhieu cua model qwen2.5vl:3b, rui ro vo visual da on dinh.
- Anh huong: khong doi gi them; 5 commit cua dot nay giu nguyen; chuyen sang review + PR.

## [2026-08-14] UI Redesign - review dot 1 (dev-review CHANGES REQUESTED) + quyet dinh xu ly
- Quyet dinh: giao fix cho dev-ux cac muc: MAJOR HomeView.play() them stopPlayback() dau ham (khoi phuc guard cu, fix timer leak); MINOR ClassReportView 3 chuoi hardcode -> i18n; copyReminder + copyInvite them clipboard guard + catch; :not(:has()) -> :class binding; AdminContentView toggleLesson xoa cache khi fetch fail de retry; doi role=tree -> list/listitem (bo semantics cay khong co arrow-nav); bo class thua --role/--status.
- Quyet dinh: CHAP NHAN giu value moi cua 2 key cu detailTabMembers ('Thanh vien'->'Hoc vien') va detailTabAssignments ('Lo trinh da gan'->'Bai tap') - chu dich, tab ngan gon; ghi log thay vi revert.
- Quyet dinh: CHAP NHAN :deep([data-value]) transition + :deep([role=tab]) trong ClassDetailView (view-scoped, khong pha shared component; chuyen vao component chung se ngoai pham vi dot nay) - ghi lai de xuat sau.
- Quyet dinh: AdminContentView sua bai o trang thai draft GIU draft (khong tu chuyen pendingreview nhu base) - cai tien hop ly, chap nhan, ghi log thay doi hanh vi.
- Anh huong: HomeView.vue, ClassReportView.vue, ClassesView.vue, AdminContentView.vue, vi.ts; commit fix moi sau e1a4e13.

## [2026-08-14] UI Redesign - Review PM Antigravity HomeView (17 diem) - da xac minh, quyet dinh fix
- Xac minh BLOCKER: 	ext-secondary (15 cho) va 	ext-muted (5 cho) dung Tailwind --secondary/--muted = mau NEN sang oklch(0.93/0.945) -> chu tang hinh o light theme. Fix: thay bang 	ext-muted-foreground (nhat quan voi 20 cho da dung).
- Xac minh nguon: HomeView co 2 khu sim rieng: (a) QuickSort Preview CU (L980) dung ky tu unicode thoat + nut nho - fix: lucide Play/Pause/StepForward/RotateCcw >=32px + sub-label 9 phase; (b) Hero Data Bench moi Task 1 da dung lucide + trace (giu nguyen).
- Quyet dinh Guest vs Member: khi authed, an 4 section marketing (Hero gioi thieu, Bento, Testimonials, CTA tao tai khoan) - giu Dashboard + Demos + Catalog + Sandbox + Freemium (co CTA premium huu ich). Dashboard authed da co duong dan hoc ro (path/classes). Thay doi hanh vi co chi dao tu PM Antigravity.
- Quyet dinh giu dark motif: Sandbox stage GIU g-canvas-ink (DESIGN.md bat buoc vung du lieu luon toi) - chi them border-subtle + badge context de hai hoa, KHONG doi nen sang.
- Quyet dinh khac: bo emoji trong 3 title Freemium (vi pham cam emoji - da co icon lucide); handleTilt gate (hover:hover) + CSS vars thay GSAP rotate; bento wave 6-8 cot animation thay gradient tinh; testimonials them Transition fade-slide; XP wheel them SVG gradient + giam glow flame; bento/trust responsive breakpoints; mockup roadmap/codelab/ai cai thien contrast; CTA freemium them cho Hearts/Gems (route ton tai, khong bia route).
- Anh huong: HomeView.vue + vi.ts (them key moi); verify build + 178/178; chup lai home-1366/390.

## [2026-08-15] UI Redesign - Review lan 2 (PM Antigravity) - 4 diem ton dong + quy tac moi
- Quyet dinh: fix 4 diem PM Antigravity xac nhan: (1) Testimonials: bo spring-hover khoi testimonial-card (transition transform ghi de fade-slide -> Vue khong bat transitionend -> ke?t opacity:0); (2) an them freemium-section + 3 extended-section khi authed (chi giu Demos/Catalog/Sandbox); (3) handleTilt them guard (hover: none) ngay dau ham; (4) them rule dark cho hero__title.
- QUY TAC MOI tu PM Antigravity: tu nay KHONG chay E2E/dev-e2e nua - Antigravity tu check tren trinh duyet; PM chi can verify build + vitest + bao cao.
- Anh huong: HomeView.vue; khong can chup anh lai.

## [2026-08-15] REFACTOR TOAN DIEN HomeView + theme toggle - chi dao PM Antigravity
- Quyet dinh: thuc hien refactor toan dien HomeView theo yeu cau Antigravity: (1) XOA: hero badge + prefix ~/, terminal macOS cu (thay bang Algorithmic Stage), search bar catalog (giu filter tabs), ai-section, testimonials, sandbox cu, landing-footer; (2) THIET KE LAI: Hero Algorithmic Stage (title + O(N log N) + lucide controls + speed slider + mau Pivot/Compare/Swap/Sorted + status badge), Bento mini-visualizer 7 cot doi mau, Demo cards typography + thumbnail, Roadmap 4 node doc + line gradient scaleY, Codelab auto-typing + 3 testcase PASSED, Rank Ladder (5 bac) + achievements that + streak + stats tu CATALOG/gamificationStore/progressStore; (3) GSAP ScrollTrigger: reveal blur->clear stagger 0.1 power3.out, road line draw, mesh blobs theme-aware, card hover glow + icon rotate.
- QUYET DINH LECH KEEP-LIST (do chu du an chi dao truc tiep): chap nhan hover glow ox-shadow 0 0 25px rgba(0,126,114,0.2) + icon xoay nhe + bong do nhe thumbnail demo (truoc day cam shadow the phang); XOA cac khoi vua fix o task 12/14 (testimonials/sandbox) - yeu cau moi de len.
- QUYET DINH TEST: cho phep dev-ux CAP NHAT HomeView.spec.ts theo hanh vi moi (xoa test phan da xoa, sua selector phan thiet ke lai, them test khoi moi neu hop ly) - KHONG phep neu lo: moi test phai pass 100%.
- QUYET DINH header: them nut toggle theme vao AppHeader.vue goi uiStore.toggleTheme() (da co trong stores/ui.ts, chua wire).
- Anh huong: HomeView.vue (rewrite lon), HomeView.spec.ts, AppHeader.vue, vi.ts (key moi).
