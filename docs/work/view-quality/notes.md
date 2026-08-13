# NOTES ΓÇö View Quality Phase 1 ┬╖ Nh├│m D

> Ng├áy: 14/08/2026 ┬╖ Agent: dev-frontend (nh├│m D ΓÇö 3 view Classes + 5 view Admin) ┬╖ Nh├ính: feature/view-quality-d

## Ollama 3-gate ΓÇö KH├öNG CHß║áY ─É╞»ß╗óC (kh├┤ng b├ío pass giß║ú)

- Task y├¬u cß║ºu "Ollama 3 gate ri├¬ng ghi ollama-log/<view>.md". Trong repo/worktree KH├öNG c├│:
  - script/CLI gate Ollama (`docs/work/` chß╗ë c├│ `bundle-*.txt`, `scorecard.md`, `standard.md`; grep `ollama` to├án repo chß╗ë ra `docs/work/teacher-register/ollama.md` ΓÇö t├ái liß╗çu phi├¬n kh├íc, kh├┤ng phß║úi gate).
  - endpoint/key LLM n├áo ─æß╗â gß╗ìi gate ─æ├ính gi├í 8 view.
- Thay thß║┐ ─æ├ú l├ám (kh├┤ng thay thß║┐ 1-1 cho gate LLM, chß╗ë l├á kiß╗âm chß╗⌐ng kh├ích quan):
  - ─Éo computed style bß║▒ng Chrome DevTools (padding button/badge/card shadow/mono/contrast token).
  - Kiß╗âm console error = 0, overflow ngang = 0 ß╗ƒ 3 mß╗æc 1366/768/390, light + dark.
  - Chß║íy flow thß║¡t qua BE :5000 (login teacher/admin, tß║ío lß╗¢p, th├¬m member, copy m├ú, tab, x├│a lß╗¢p, report dß╗» liß╗çu thß║¡t; admin: users/stats/content/ladder/settings + modal + tab + select node).
- Nß║┐u muß╗æn gate Ollama: cß║ºn pm/c╞í chß║┐ bß╗ò sung script + model key ΓÇö ghi lß║íi ─æß╗â Phase 1 tß╗òng hß╗úp.

## Quan s├ít kh├íc (ngo├ái phß║ím vi sß╗¡a ΓÇö ─æß╗â c├íc nh├│m/task sau xß╗¡ l├╜)

1. **Dark mode to├án app ch╞░a ─æ╞░ß╗úc wire**: `ui.theme` + `toggleTheme()` tß╗ôn tß║íi trong `stores/ui.ts` nh╞░ng KH├öNG c├│ chß╗ù n├áo th├¬m class `dark` v├áo `<html>` (grep `documentElement` chß╗ë c├│ canvas chart). 3 view ─æ├ú test dark bß║▒ng c├ích th├¬m class thß╗º c├┤ng ΓÇö token OK. Task hß║í tß║ºng (App.vue/main.ts) n├¬n wire sß╗¢m.
2. **`lucide-vue-next@1.0.0` bß╗ï npm deprecate** (khuyß║┐n nghß╗ï `@lucide/vue`) ΓÇö quyß║┐t ─æß╗ïnh xuy├¬n-nh├│m vß║½n chß╗æt lucide-vue-next duy nhß║Ñt; ghi lß║íi khi npm install (b├ío pm nß║┐u muß╗æn ─æß╗òi).
3. **`EmptyState` icon prop** nhß║¡n t├¬n trong `SVG_PATHS` (`utils/emojiParser.ts`) ΓÇö `chart`/`bar-chart` kh├┤ng tß╗ôn tß║íi ΓåÆ fallback x-circle. View report ─æ├ú ─æß╗òi sang `database`; n├¬n th├¬m `bar-chart` v├áo SVG_PATHS nß║┐u view kh├íc cß║ºn.
4. **`DialogContent` thiß║┐u `aria-describedby`** ΓÇö cß║únh b├ío reka-ui ß╗ƒ mß╗ìi Modal (kh├┤ng chß╗ë 3 view n├áy): `Modal.vue` (ui chung) render DialogTitle nh╞░ng kh├┤ng c├│ Description ΓåÆ warn console. ─Éß╗ü xuß║Ñt fix ß╗ƒ task component chung (th├¬m DialogDescription sr-only).
5. **G├ín nß╗Öi dung (assign) lu├┤n 400**: backend y├¬u cß║ºu lessonId/exerciseId (ClassService.AddAssignmentAsync), view chß╗ë gß╗¡i null ΓåÆ ─æ├ú c├│ ghi ch├║ backlog trong i18n (`detailAssignNote`). Kh├┤ng phß║úi lß╗ùi view.
6. **`ClassMemberDto` frontend d├╣ng `id` nh╞░ng backend trß║ú `userId`** (`ClassService.GetByIdAsync` map `UserId = u.Id`) ΓÇö bß║úng th├ánh vi├¬n hiß╗ân thß╗ï OK v├¼ chß╗ë d├╣ng `member.id` cho `:key`/remove; removeMember d├╣ng ─æ├║ng id=userId. ─Éß╗â ├╜ khi ─æß╗Ñng member DTO.
7. **`#d9dde8` exception** (chß╗» tr├¬n panel canvas-ink): khi task token ┬º2.4 th├¬m `--canvas-text` ΓåÆ thay thß║┐ tß║íi `.class-report__lagging-name` + AdminStatsView (chart/donut/hero-value) + strip caption (─æ├ú ghi decision log).

## Nh├│m D admin ΓÇö quan s├ít bß╗ò sung (14/08)

8. **Select/textarea ch╞░a c├│ wrapper shadcn** (AdminContent modal: topic/status/sim select + textarea HTML; AdminLadder: node/exercise select): giß╗» native `.input` + scoped override token (`--card/--border/--foreground`, text-sm, transition 150ms). ─Éß╗ü xuß║Ñt task component: th├¬m `Select.vue`/`Textarea.vue` wrapper shadcn cho Phase 2.
9. **Nß╗Öi dung rich-text CMS (contentHtml)**: view kh├┤ng render nß╗Öi dung b├ái hß╗ìc (chß╗ë quß║ún l├╜ metadata) ΓåÆ KH├öNG gß║╖p emoji nß╗Öi dung; nß║┐u Phase 2 render contentHtml, r├á emoji icon + microcopy (ghi theo task).
10. **AdminContentView ─æ├ú bß╗Å cß╗Öt "Ng├áy tß║ío"** v├¼ `LessonSummary` kh├┤ng c├│ `createdAt` (tr╞░ß╗¢c render `formatDate(new Date())` ΓÇö ng├áy giß║ú) ΓåÆ thay cß╗Öt index mono `#01`. Nß║┐u cß║ºn ng├áy thß║¡t: backend bß╗ò sung `createdAt` v├áo DTO lesson rß╗ôi th├¬m lß║íi cß╗Öt.
11. **Tablet 768**: bß║úng admin giß╗» scroll ngang trong container (card-stack chß╗ë Γëñ640 ΓÇö theo precedent nh├│m Classes). DESIGN ┬º8 ghi "768 ß║⌐n cß╗Öt phß╗Ñ" ΓÇö c├ón nhß║»c n├óng l├¬n 768 cho Phase 2 nß║┐u cß║ºn (quyß║┐t ─æß╗ïnh nh├│m, kh├┤ng tß╗▒ l├ám).

<!-- MERGED-BLOCK: nhom khac -->
- **`/dashboard` l├á redirect ΓåÆ `/profile`** (router/index.ts d├▓ng 193ΓÇô196) ΓÇö KH├öNG c├│ view ri├¬ng, kh├┤ng cß║ºn xß╗¡ l├╜ (ghi ch├║ theo task).
- **BE :5000 c├│ phß║ún hß╗ôi nh╞░ng trß║ú 404 cho route thß║¡t** (14/08/2026 ΓÇö swagger/index.html 200 nh╞░ng `/api/v1/me` 404): visual check 6 view bß║▒ng Playwright + route interception mock `/api/v1/*` (auth/refresh, auth/me, premium/status, me/hearts, me/streak...). X├íc nhß║¡n 2 view mß╗¢i (Premium/Subscription) render ─æß╗º trß║íng th├íi (data/empty/error/success checkout), light+dark+390px, overflowX = 0, console error = 0 (trß╗½ 1 lß╗ùi 500 mock chß╗º ─æ├¡ch khi test error state).
- **Cß║únh b├ío console duy nhß║Ñt c├▓n lß║íi (to├án app, component chung)**: `[WARNING] Missing Description or aria-describedby="undefined" for DialogContent` ΓÇö shadcn-vue Dialog nß╗Öi bß╗Ö, xuß║Ñt hiß╗çn vß╗¢i Mß╗îI Modal (c├│ tr╞░ß╗¢c nh├│m C). ─Éß╗ü xuß║Ñt Phase 2: Modal.vue th├¬m description/aria-describedby mß║╖c ─æß╗ïnh.
- **Cß║únh b├ío c├▓n lß║íi tß╗½ tr╞░ß╗¢c (LeaderboardView)**: `[ECharts] Can't get DOM width or height` ß╗ƒ mobile (Γëñ640px) ΓÇö chart `display:none` nh╞░ng VChartLazy vß║½n init. ─Éß╗ü xuß║Ñt Phase 2: render VChartLazy c├│ ─æiß╗üu kiß╗çn theo breakpoint.
- **N├║t md 40px tr├¬n mobile** (Premium/Subscription + 4 view c┼⌐): ┬º8 khuyß║┐n nghß╗ï CTA ch├¡nh ΓëÑ44px mobile ΓÇö nh├│m C giß╗» 40px ─æß╗ông nhß║Ñt (hit target ΓëÑ24├ù24 vß║½n ─æß║ít). ─Éß╗ü xuß║Ñt Phase 2: Button.vue auto-upgrade `size="lg"` theo breakpoint hoß║╖c document.
- **`cssVar()` tr├╣ng lß║╖p** giß╗»a ProfileView + LeaderboardView (v├á c├│ thß╗â view kh├íc): ─æß╗ü xuß║Ñt ─æß║⌐y sang `src/composables/useThemeCssVar.ts` ß╗ƒ Phase 2 ΓÇö ngo├ái phß║ím vi nh├│m C.
- Ollama gate: ─æ├ú chß║íy qwen2.5vl:3b 3 gate ├ù 6 view ΓåÆ `docs/work/view-quality/ollama-log/<view>.md`. Kß║┐t quß║ú 2 view mß╗¢i: Premium gate1 7.5/10, Subscription 8.5/10; gate2 kh├┤ng lß╗ùi spacing; gate3 nhß║¡n diß╗çn ─æ╞░ß╗úc "DSA learning app" + block tß╗æi. Model 3b c├│ hallucination (chß║Ñm "card shadow"/"font dark to h╞ín" khi computed style khß║│ng ─æß╗ïnh ng╞░ß╗úc lß║íi) ΓÇö ─æiß╗âm tham khß║úo, kh├┤ng thay thß║┐ assertion Playwright.

<!-- MERGED-BLOCK: nhom khac -->
> 13/08/2026 ┬╖ dev-engine. Ghi ch├║ ngo├ái phß║ím vi ─æ├ú gß║╖p, kh├┤ng tß╗▒ sß╗¡a.



## Component d├╣ng chung bß╗ï ─æß╗Ñng tß╗æi thiß╗âu

1. `frontend/src/components/ui/Button.vue` ΓÇö th├¬m `size: 'icon' | 'icon-sm' | 'icon-lg'` (map thß║│ng `buttonVariants` ┬º4.1, backward compatible). L├╜ do: SimulatorView favorite/share cß║ºn n├║t icon chuß║⌐n 40├ù40; tr├ính 4 raw `<button>`.

2. `frontend/src/components/simulator/ControlBar.vue` + `StatsBar.vue` ΓÇö bß╗Å gradient chip ΓåÆ primary solid + mono; bß╗Å shadow. ─É├óy l├á "toolbar/panel th├┤ng tin" cß╗ºa SimulatorView (trong phß║ím vi khung ngo├ái).

3. `frontend/src/components/ui/Card.vue` (wrapper) + shadcn `card/Card.vue` vß║½n c├│ `shadow-sm` mß║╖c ─æß╗ïnh + `hover:-translate-y-0.5 hover:shadow-lg` cho interactive ΓÇö Lß╗åCH DESIGN.md ┬º4.2/┬º6 (card cß║Ñm shadow, hover chß╗ë ─æß╗òi border). KH├öNG sß╗¡a global v├¼ ß║únh h╞░ß╗ƒng 36 view ΓÇö ─æß╗â Phase 2 ─æß╗òi 1 n╞íi.



## Nß╗Öi dung CMS c├▓n emoji (kh├┤ng phß║úi code view)

- B├ái hß╗ìc 1 `contentHtml` chß╗⌐a ≡ƒÄ» ("Sß║»p xß║┐p c╞í bß║ún"), ≡ƒæë, ≡ƒôÜ ΓÇö render qua `v-html` (LessonDetail). Emoji trong nß╗Öi dung b├ái giß║úng = dß╗» liß╗çu, kh├┤ng phß║úi icon chß╗⌐c n─âng UI. Phase 2: quy ╞░ß╗¢c bi├¬n soß║ín nß╗Öi dung / sanitize, KH├öNG sß╗¡a trong view.



## Component simulator c├▓n vi phß║ím nhß║╣ (ngo├ái khung ngo├ái)

- `PseudocodePanel.vue`: gradient (1), shadow (4), weight 700 (2), 2 raw `<button>` (breakpoint toggle ΓÇö dß║íng editor/table-cell, cß║ºn decision log).

- `LegendPanel.vue`: 6 hex rß╗¥i (m├áu legend canvas ΓÇö n├¬n ─æß╗ìc tß╗½ `canvasTheme.ts`).

- `ManualPracticePanel.vue` / `DemoBanner.vue`: weight 700.

ΓåÆ Phase 2 khi c├│ task ri├¬ng cho simulator panels.



## ─Éß║╖c tr╞░ng ExerciseView

- ─Éß║ít 7.5 nhß╗¥ toolbar Data Bench (kicker mono + surface band) nh╞░ng QuizStage (component chung) vß║½n generic quiz. Muß╗æn ΓëÑ8: Phase 2 sß╗¡a QuizStage (block-token cho c├óu hß╗Åi/─æ├íp ├ín, mono cho sß╗æ c├óu).



## Ollama 3-gate

- qwen2.5vl:3b context 4096 ΓÇö ß║únh fullpage > ~120KB fail; phß║úi resize 1024px cho LessonView. Gate 3 (─æiß╗âm ─æß║╖c tr╞░ng) model trß║ú lß╗¥i lan man kh├┤ng ra sß╗æ sß║ích ΓÇö log th├┤ giß╗» nguy├¬n tß║íi `ollama-log/`, ─æiß╗âm ─æß║╖c tr╞░ng ch├¡nh thß╗⌐c lß║Ñy tß╗½ audit chß╗º quan (─æ├ú ghi).

- Task P1-B3 (BenchmarkView + PathRedirectView): benchmark-{light,dark,mobile-light} + path-redirect-{light,dark,mobile-dark} ΓåÆ Gate 1 = C├ô (nhß║¡n diß╗çn app DSA Visual) 5/6 ß║únh; 1 ß║únh (benchmark-mobile-light) model trß║ú lß╗¥i l╞░ß╗íng lß╗▒ "kh├┤ng phß║úi app hß╗ìc DSA" d├╣ tß╗▒ nhß║¡n diß╗çn "app DSA Visual" (m├án mobile chß╗ë thß║Ñy card-stack + kh├┤ng thß║Ñy canvas). Gate 2: benchmark sß║ích; path-redirect model b├ío "c├│ emoji icon (icon cß╗ºa c├íc topic)" + "layout lß╗çch" ΓÇö **nhiß╗àu model** (grep + DOM snapshot x├íc nhß║¡n view kh├┤ng c├▓n emoji, kh├┤ng overflow 390px; model c├│ thß╗â nhß║ºm icon Route lucide/progress th├ánh emoji ΓÇö ─æ├║ng hß║ín chß║┐ model ─æ├ú ghi). Gate 3: 7ΓÇô8/10 (khß╗¢p audit chß╗º quan 8/10).



## Tooling

- Playwright MCP ghi screenshot v├áo cwd server = `D:\FPT\neww` (main worktree) ΓÇö ─æ├ú dß╗ìn sß║ích artifacts sau khi chß║íy; nß║┐u chß║íy lß║íi, ─æß╗òi output dir.

- Dev server ─æ├ú chß║íy ß╗ƒ :5176 (proxy /api ΓåÆ :5000, BE ─æang chß║íy).



## Task P1-B2 (LabView + CodeRunnerView) ΓÇö ngo├ái phß║ím vi ─æ├ú gß║╖p

1. **Icon "≡ƒ¬£ bß║¡c thang vß╗í glyph" (r2-fixed-07)** nß║▒m ß╗ƒ LadderView.vue:85 (≡ƒ¬£ Practice Ladder) ΓÇö LadderView l├á view KH├üC, ngo├ái phß║ím vi task 2-view; empty state icon puzzle (icon="puzzle") ß╗ƒ QuizStage.vue:157 (Bß║¡c 1). ΓåÆ Task LadderView (╞░u ti├¬n CAO, scorecard c├▓n trß╗æng) xß╗¡ l├╜: ≡ƒ¬£ ΓåÆ lucide, quiz empty state d├╣ng EmptyState chung vß╗¢i icon theo ngß╗» cß║únh.

2. **BenchmarkPanel.vue:289** c├▓n <button> raw chip chß╗ìn thuß║¡t to├ín (BenchmarkView) ΓÇö task BenchmarkView xß╗¡ l├╜.

3. LabStage.vue cell l├á <button> raw nh╞░ng nß║▒m trong v├╣ng canvas dß╗» liß╗çu (decision log mß╗Ñc 2) ΓÇö hß╗úp lß╗ç theo standard.md trß╗Ñc 5, ─æ├ú th├¬m aria-pressed + aria-label.

4. --color-text-primary light theme = ar(--color-foreground) (#134E4A) ΓÇö tr├¬n nß╗ün canvas-ink l├á qu├í tß╗æi; c├íc view mß╗¢i d├╣ng color-mix(white 85%, index-muted) cho text v├╣ng dß╗» liß╗çu. Phase 2: th├¬m token --canvas-text ch├¡nh thß╗⌐c.



## Task P1-B3 (BenchmarkView + PathRedirectView) ΓÇö ngo├ái phß║ím vi / cho Phase 2

1. **ECharts 6 deprecation**: `grid.containLabel` ─æ├ú bß╗ï thay bß║▒ng `LegacyGridContainLabel` (import `echarts/features`) ΓÇö ─æ├ú sß╗¡a trong BenchmarkPanel ─æß╗â hß║┐t warning console. View kh├íc d├╣ng ECharts vß╗¢i containLabel ΓåÆ ├íp dß╗Ñng t╞░╞íng tß╗▒.

2. **Ollama Gate 2 nhiß╗àu tr├¬n path-redirect**: model b├ío "emoji icon (icon cß╗ºa c├íc topic)" + "layout lß╗çch" d├╣ grep + DOM snapshot sß║ích (view kh├┤ng c├▓n emoji, kh├┤ng overflow 390px) ΓÇö th├¬m 1 bß║▒ng chß╗⌐ng cho hß║ín chß║┐ qwen2.5vl:3b ─æ├ú ghi; kh├┤ng h├ánh ─æß╗Öng theo.

3. **`--color-card-raised` + theme khi test**: app c├│ watcher tß╗▒ bß║¡t/tß║»t class `dark` theo uiStore (localStorage) ΓÇö thao t├íc class tay bß╗ï ─æ├¿; ─æß╗ông thß╗¥i getComputedStyle ─æß╗ìc trong C├ÖNG evaluate vß╗¢i toggle class c├│ thß╗â trß║ú gi├í trß╗ï stale (─æß╗ìc lß║íi ß╗ƒ evaluate kß║┐ tiß║┐p l├á ch├¡nh x├íc). Kh├┤ng phß║úi bug view.

4. **Card `minmax(300px,1fr)`** ß╗ƒ /path cho 5 card ΓåÆ 3+2 cß╗Öt tß║íi 1200px container ΓÇö grid auto-fill chuß║⌐n, chß║Ñp nhß║¡n (model gß╗ìi "layout lß╗çch" l├á nhß║ºm).



<!-- MERGED-BLOCK: nhom khac -->

Ng├áy: 13/08/2026 ┬╖ Worktree `D:\FPT\neww-qa` (nh├ính `feature/view-quality-a`).







## 1. Hero mini-sim Home ΓÇö ─æ├ú l├ám mß╗⌐c n├áo, Phase 2 l├ám g├¼ tiß║┐p



- ─É├â L├ÇM: mini-sim chß║íy **step THß║¼T tß╗½ engine** (`getSimulation()` tß╗½ `engines/registry` ΓÇö catalog.ts tß╗▒ ─æ─âng k├╜ 44 generator khi import; `generate()` trß║ú `Step[]` c├│ `structure.elements` (label + status) + explanation tiß║┐ng Viß╗çt). Render bß║▒ng **DOM block** (kh├┤ng canvas): block-token + index mono + status ΓåÆ m├áu `data-core/resolved/conflict` + swap-pop 240ms. 3 demo ─æ├║ng FR-7.6: `sort.bubble` (99 b╞░ß╗¢c), `search.binary` (13), `graph.bfs` (29) ΓÇö data 6 phß║ºn tß╗¡ cß╗æ ─æß╗ïnh, autoplay 380ms/b╞░ß╗¢c, lß║╖p nhß║╣ sau 1.4s, dß╗½ng nß║┐u `prefers-reduced-motion`.



- CH╞»A L├ÇM (Phase 2 ΓÇö cß║ºn session/harness ─æß║ºy ─æß╗º): render bß║▒ng `renderers/arrayRenderer.ts` + `painter/canvasPainter.ts` tr├¬n canvas thß║¡t (─æ├║ng quyß║┐t ─æß╗ïnh xuy├¬n-nh├│m "hero chß║íy renderer canvas thß║¡t"); animation "nhß╗ïp thß╗ƒ" compare tß║ºn sß╗æ t─âng dß║ºn; swap bß║▒ng spring thß║¡t. DOM-block hiß╗çn tß║íi l├á xß║Ñp xß╗ë trung thß╗▒c (c├╣ng nguß╗ôn trace), ─æß╗º cho Phase 1, ghi r├╡ ─æß╗â pm quyß║┐t ─æß╗ïnh Phase 2.



- L╞░u ├╜: engine chunk (478kB) ─æ├ú ─æ╞░ß╗úc tß║úi ß╗ƒ Home tr╞░ß╗¢c ─æ├│ v├¼ Home import CATALOG ΓÇö kh├┤ng ─æß╗òi.







## 2. Component d├╣ng chung ΓÇö ─æ├ú sß╗¡a (tß╗æi thiß╗âu, c├│ decision log)



1. `src/styles/global.css`:



   - Reset `*{padding:0}` unlayered ΓåÆ `@layer base` ΓÇö **HOTFIX to├án app**: tr╞░ß╗¢c ─æ├óy mß╗ìi padding utility Tailwind (shadcn button/input) bß╗ï ─æ├¿ th├ánh 0px (─æo thß║¡t: input `px-3 pl-9` ΓåÆ computed 0px; chß╗» chß║ím viß╗ün). ß║ónh h╞░ß╗ƒng mß╗ìi view theo chiß╗üu *kh├┤i phß╗Ñc ─æ├║ng thiß║┐t kß║┐*.



   - Page transition easing: `220ms ease` ΓåÆ enter `cubic-bezier(0.16,1,0.3,1)` / exit `cubic-bezier(0.7,0,0.84,0)` (KILL-LIST V2 cß║Ñm easing mß║╖c ─æß╗ïnh >150ms).



2. `src/components/ui/Input.vue` (d├╣ng chung 10 view): prop `icon` nhß║¡n th├¬m `Component` (lucide-vue-next); string c┼⌐ vß║½n qua BaseIcon ΓÇö backward-compatible, view kh├íc kh├┤ng ─æß╗òi h├ánh vi.



3. KH├öNG sß╗¡a: `AppHeader.vue` (c├▓n `<button>` raw `.app-header__user` + menu ΓÇö thuß╗Öc phß║ím vi kh├íc), `Button.vue`, `ui/card/*`, `ui/input/*`. AppHeader vß║½n render brand + login/register cho kh├ích tr├¬n mß╗ìi route ΓÇö HomeView ─æ├ú bß╗Å header nß╗Öi bß╗Ö n├¬n kh├┤ng c├▓n tr├╣ng.







## 3. Quyß║┐t ─æß╗ïnh ghi th├¬m trong `docs/pm-decision-log-viewquality.md`



- Bß╗Å header nß╗Öi bß╗Ö HomeView (fix 2 header).



- Hero: bß╗Å gradient/blob ΓåÆ surface band + panel tß╗æi canvas-ink + mini-sim engine thß║¡t (DOM block, Phase 2 = canvas).



- Auth aside ΓåÆ panel tß╗æi Data Bench; tr├¬n panel lu├┤n tß╗æi d├╣ng `rgba(255,255,255,ΓÇª)` / `white/*` (kh├┤ng phß║úi hex, kh├┤ng theo theme) ΓÇö t╞░╞íng ─æ╞░╞íng engine canvasTheme fallback (#d9dde8 / #6b7385); `border-subtle` theo theme sai m├áu tr├¬n nß╗ün tß╗æi n├¬n kh├┤ng d├╣ng.



- HOTFIX reset padding (mß╗Ñc 2.1) + easing page transition (mß╗Ñc 2.1) + Input.vue additive.



- Segmented vai tr├▓ Register qua Button.vue ghost (giß╗» selector e2e `button.register__role-option`).







## 4. Verify



- `npm run build` (vue-tsc -b + vite build): **PASS** (chß║íy 3 lß║ºn sau mß╗ùi ─æß╗út sß╗¡a).



- `npm run test` (vitest): **95/95 PASS** (gß╗ôm RegisterView.spec.ts ΓÇö selector e2e giß╗» nguy├¬n).



- Kh├┤ng c├│ lint script trong package.json (chß╗ë dev/build/test/test:e2e) ΓÇö ghi r├╡, kh├┤ng tß╗▒ ─æo├ín.



- Dev server `npm run dev -- --port 5175` tß╗½ worktree (proxy /api ΓåÆ :5000 sß║╡n trong vite.config). ─Éo thß║¡t bß║▒ng chrome-devtools MCP: 3 mß╗æc breakpoint (1366├ù768 / 768 / 390├ù844) ΓÇö kh├┤ng overflow, kh├┤ng ─æ├¿ chß╗»; computed padding button/input ─æ├║ng chuß║⌐n; console error = 0; demo tab switching OK; dark mode OK (panel dß╗» liß╗çu vß║½n tß╗æi).



- ß║ónh: `docs/work/view-quality/shots/` (10 ß║únh light/dark 1366├ù768).



- Ollama: chß║íy ─æ╞░ß╗úc (`qwen2.5vl:3b` @ :11434), 3 gate ├ù 5 view ghi tß║íi `docs/work/view-quality/ollama-log/`. **L╞░u ├╜ ─æß╗Ö tin cß║¡y**: model 3B VL chß║Ñm yß║┐u (tß╗▒ m├óu thuß║½n ΓÇö Login gate1 "kh├┤ng glassmorphism" vs Reset gate1 "c├│ glassmorphism"; Reset gate1 bß╗ïa "gradient + blob" d├╣ DOM chß╗⌐ng minh `backgroundImage: none`). Gate 3 (bß║ún sß║»c): model nhß║¡n diß╗çn ─æ├║ng "DSA Visual ΓÇö dß║íy cß║Ñu tr├║c dß╗» liß╗çu/giß║úi thuß║¡t" tr├¬n cß║ú 5 view, kh├┤ng view n├áo kß║┐t luß║¡n "chung chung" ΓåÆ d├╣ng l├ám t├¡n hiß╗çu phß╗Ñ. Bß║▒ng chß╗⌐ng ch├¡nh: DOM/computed assertions ß╗ƒ tr├¬n.







## 5. Vi phß║ím c├▓n lß║íi c├│ chß╗º ─æ├¡ch (─æ├ú ghi decision log)



- `rgba(255,255,255,ΓÇª)` tr├¬n panel tß╗æi (kh├┤ng phß║úi @theme token) ΓÇö bß║»t buß╗Öc v├¼ panel LU├öN tß╗æi kh├┤ng theo theme; t╞░╞íng ─æ╞░╞íng engine fallback.



- Dot "live" pulse 2s (opacity, cubic-bezier chuß║⌐n) ß╗ƒ hero bench ΓÇö ambient, bß╗ï cß║»t khi prefers-reduced-motion.



- Motion shell auth views (280ms) chß╗ông vß╗¢i page transition ΓÇö chß║Ñp nhß║¡n (nhß║╣), c├│ thß╗â bß╗Å sau khi thß╗æng nhß║Ñt route transition to├án app.







## 6. ─Éß╗út 2 (4 view c├▓n lß║íi: NotFound/Privacy/Help/Simulations) ΓÇö ghi ch├║ bß╗ò sung



- **HOTFIX 2 to├án app**: `global.css` `a { color }` unlayered ─æ├¿ `text-primary-foreground` tr├¬n RouterLink-as-button (─æo thß║¡t CTA HomeView + 404: chß╗» #007E72 tr├¬n nß╗ün primary ~1.2:1) ΓåÆ ─æ╞░a v├áo `@layer base` + `a.inline-flex:hover { text-decoration: none }`. Sau sß╗¡a CTA 404 trß║»ng/teal contrast 21:1; HomeView CTA c┼⌐ng ─æ╞░ß╗úc sß╗¡a (regression tß╗æt). Xem decision log.



- **Badge.vue** (shared): th├¬m `min-h-6` (height badge ΓëÑ 24px, trß╗Ñc 5f) ΓÇö ─æo 25.6px. Decision log.



- **Card.vue** base c├▓n `shadow-sm` ΓÇö SimulationsView override `shadow-none` tß║íi call site; bß║ún th├ón Card.vue l├á shared cß║ºn nh├│m kh├íc/Phase 2 xß╗¡ l├╜ chung (ghi ─æß╗â pm biß║┐t, KH├öNG sß╗¡a trong task n├áy).



- **Theme toggle ch╞░a ─æ╞░ß╗úc wire** (pre-existing): `stores/ui.ts` c├│ `theme` ref nh╞░ng kh├┤ng ai apply `.dark` class l├¬n `<html>` (chß╗ë truyß╗ün cho Toaster). Dark mode chß╗ë test ─æ╞░ß╗úc bß║▒ng class thß╗º c├┤ng ΓÇö ─æß╗ü xuß║Ñt task kh├íc wire theme (toggle + localStorage + prefers-color-scheme).



- **BenchmarkPanel** (shared, ngo├ái phß║ím vi): heading "ΓÜû Benchmark Lab" c├▓n emoji icon + "Γû╢ Chß║íy benchmark" glyph ΓÇö vi phß║ím icon lucide nh╞░ng nß║▒m trong component kh├íc view ΓåÆ ghi ─æß╗â nh├│m sß╗ƒ hß╗»u sß╗¡a.



- Ollama 3B VL chß║Ñm yß║┐u tr├¬n trang text (privacy gate1 "chung chung" d├╣ c├│ index mono; help "CO DAU VET" sau khi th├¬m index) ΓÇö d├╣ng l├ám t├¡n hiß╗çu phß╗Ñ; bß║▒ng chß╗⌐ng ch├¡nh = DOM/computed + ti├¬u ch├¡ "chi tiß║┐t chß╗ë app n├áy c├│" trong standard.md.



- ß║ónh ─æß╗út 2: 8 ß║únh light/dark (notfound/privacy/help/simulations) l╞░u `docs/work/view-quality/shots/` (bß║ún gß╗æc chß╗Ñp bß║▒ng chrome-devtools MCP tß║íi temp; copy 2 ─æß║íi diß╗çn v├áo shots nß║┐u cß║ºn ΓÇö hiß╗çn log ollama ─æ├ú ─æß╗º).







## 7. ─Éß╗út 3 (view cuß╗æi nh├│m A: CheatSheetView `/cheatsheet` ΓÇö 14/08/2026) ΓÇö ghi ch├║ bß╗ò sung



- **Visual check kh├┤ng cß║ºn sß╗¡a repo**: `/cheatsheet` c├│ `meta.requiresAuth` ΓåÆ route guard chß║╖n khi BE kh├┤ng reachable qua vite proxy (`localhost:5000` l├á WSL/Docker relay ΓÇö Node proxy kh├┤ng kß║┐t nß╗æi ─æ╞░ß╗úc d├╣ PowerShell gß╗ìi thß║│ng OK). Giß║úi ph├íp: mock BE tß║ím ß╗ƒ temp (`%TEMP%\opencode\mock-be\server.js`, port 5999, CORS cho origin 5175, trß║ú refresh/me/hearts giß║ú) + chß║íy dev vß╗¢i `VITE_API_BASE_URL=http://localhost:5999/api/v1` ΓÇö **KH├öNG ─æß╗Ñng vite.config/repo**. API client ─æß╗ìc env n├áy sß║╡n (client.ts).



- **Dark mode test**: d├╣ng `.dark` class thß╗º c├┤ng tr├¬n `<html>` (theme toggle ch╞░a wire ΓÇö mß╗Ñc 6). L╞»U ├¥ khi ─æo: sau khi mutate class, ─æß╗ìc getComputedStyle 1 thuß╗Öc t├¡nh ─æ╞ín c├│ thß╗â trß║ú gi├í trß╗ï stale (─æo ─æ╞░ß╗úc chrome bg light d├╣ html var = dark) ΓÇö phß║úi ─æß╗ìc cß║ú cß╗Ñm (html var + element var + bg) trong 1 script ─æß╗â force style recalc; nh├│m B/C ─æo dark n├¬n l├ám vß║¡y.



- **C├▓n lß║íi c├│ chß╗º ─æ├¡ch (─æ├ú ghi decision log)**: chip Big-O tß╗æi d├╣ng `rgba(255,255,255,0.92)` (panel LU├öN tß╗æi ΓÇö tiß╗ün lß╗ç mß╗Ñc 3); strip Big-O banner (decorative aria-hidden); `.input` global `transition: border-color 200ms ease` (global.css:169) ΓÇö file shared, d├╣ng ß╗ƒ search cß╗ºa view nh╞░ng ngo├ái phß║ím vi ΓåÆ ─æß╗ü xuß║Ñt task chung ─æß╗òi sang `150ms cubic-bezier(0.16,1,0.3,1)`.



- **Ollama**: chß║íy ─æ╞░ß╗úc (qwen2.5vl:3b @ :11434), 3 gate light + 1 gate dark ─æß╗üu PASS ΓÇö Gate 1 "RO RANG APP HOC CTDL", Gate 2/3 + dark: kh├┤ng overflow/overlap, contrast r├╡. Log: `docs/work/view-quality/ollama-log/cheatsheet.md`. ß║ónh: `docs/work/view-quality/shots/cheatsheet-{light,dark}-1366.png`.



- Verify: `npm run build` PASS (vue-tsc -b + vite build); `npm run test` PASS (95/95). Console error = 0; a11y issue = 0 (─æ├ú th├¬m `name` cho search input). ─Éo 3 mß╗æc: 1366 (table full), 768 (table khß╗¢p, hScroll 0), 390 (card-stack ΓÇö thead ß║⌐n, tr = card, td data-label, hScroll 0).

---

## Phase 2 bổ sung (14/08/2026 · dev-frontend · feature/view-quality-merge-check) — 3 view sót: Ladder / NodeHub / FinalTest

- **Ollama gate 3 câu KHÔNG chạy được ở đợt này**: agent chạy model deepseek-v4-flash — không hỗ trợ đọc ảnh. Đã verify thay bằng DOM assertions + computed style light/dark + 3 mốc breakpoint + console error = 0/3 view (chi tiết ollama-log/LadderView.md + NodeHubView.md + FinalTestView.md, kèm screenshot ladder light/dark). Đề xuất: reviewer có mắt chạy lại gate 3 câu trên screenshot đã lưu.
- **Ngoài phạm vi 3 view — phát hiện để Phase sau**: LessonDetail.vue (component chung của LessonView + NodeHubView tab Lý thuyết) còn emoji nội dung: 📝 Ghi chú, ★ Đánh giá, 🎯 Sắp xếp cơ bản… (heading), 👉 Bấm, 📚 Tham khảo — KILL-LIST icon emoji, cần task riêng (LessonView + component).
- **Dark mode chưa wire** (lặp lại quan sát cũ, xác nhận lại đợt này): không có chỗ nào gắn class dark vào <html>; đợt này verify dark bằng cách tự thêm class — token OK.