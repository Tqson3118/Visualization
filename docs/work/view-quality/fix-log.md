# FIX-LOG ΓÇö View quality Phase 1 (Nh├│m B: 4 view hß╗ìc tß║¡p)

> Ghi tr╞░ß╗¢c/sau cho tß╗½ng view (13/08/2026 ┬╖ dev-engine). Chi tiß║┐t lß╗ùi: `audit-<view>.md`. Quyß║┐t ─æß╗ïnh: `docs/pm-decision-log-viewquality.md`.

## PathView `/path/:topicId`

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 59.5/100 | 86/100 |
| ─Éß║╖c tr╞░ng | 2/10 | 8.5/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | animation 7 < 8.4; thß╗ï-gi├íc 5 < 8.4 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** (hygiene ΓëÑ80, kh├┤ng trß╗Ñc d╞░ß╗¢i s├án, ─æß║╖c tr╞░ng ΓëÑ7) |

─É├ú sß╗¡a:
1. **Node-edge graph thß║¡t**: chuyß╗ân pill Duolingo ΓåÆ VueFlow (`PathGraph.vue` lazy `defineAsyncComponent` + `PathFlowNode.vue` block-token). Node = canvas-ink + index mono `NODE 01/08` + chip trß║íng th├íi (─ÉANG Hß╗îC/─É├â QUA/KH├ôA) + icon lucide (Play/CheckCircle2/Lock/Flag); edge smoothstep, m├áu `resolved` khi node nguß╗ôn ─æ├ú qua; snake 2 cß╗Öt desktop / cß╗Öt ─æ╞ín <640px (kh├┤ng tr├án 390). Bundle entry 106.44 ΓåÆ 106.59 kB (delta +0.15 kB); vue-flow 154.55 kB nß║▒m trong lazy chunk PathGraph (sß╗æ ─æß║ºy ─æß╗º ß╗ƒ decision log).
2. **Banner**: bß╗Å gradient aurora + shadow-md ΓåÆ surface band level-2 (`bg-card-raised` + border-subtle); kicker mono `LEARNING PATH ┬╖ TOPIC 01`; H1 + icon `Route` (bß╗Å ≡ƒÄ»); progress panel level-1 + bß╗Ö ─æß║┐m mono `00/04`.
3. **Emoji ΓåÆ lucide**: ≡ƒÄ»≡ƒöÆΓ¡ÉΓû╢≡ƒÅüΓ¥ñ ΓåÆ Route/Lock/Play/CheckCircle2/Flag/Heart (5 chß╗ù).
4. **Raw `<button>` (2) ΓåÆ role="button" div trong custom node** (canvas exception, decision log mß╗Ñc 2) + keyboard Enter/Space k├¡ch hoß║ít node (tested).
5. **Weight 700 ΓåÆ 600/500** (kicker, label, node); tracking d╞░╞íng chß╗ë c├▓n tr├¬n label mono ngß║»n.
6. **Easing popover**: `ease 200ms` ΓåÆ enter `cubic-bezier(0.16,1,0.3,1)` 200ms / exit `cubic-bezier(0.7,0,0.84,0)` 150ms + prefers-reduced-motion.
7. **Hover node**: bß╗Å `hover-lift` (lift+shadow) ΓåÆ border-color 150ms chuß║⌐n.

Kiß╗âm chß╗⌐ng runtime (playwright, BE :5000 thß║¡t, /path/1): graph 4 node + 4 edge + final node render; click + Enter mß╗ƒ popover ─æ├║ng; 0 console error/warn; 0 overflow 390px; light+dark ─æ├║ng token.

## LessonView `/learn/:lessonId`

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 67/100 | 87.5/100 |
| ─Éß║╖c tr╞░ng | 3/10 | 8/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | animation 6 < 8.4; thß╗ï-gi├íc 6 < 8.4 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** |

─É├ú sß╗¡a:
1. **Banner**: bß╗Å gradient sunset + text trß║»ng + text-shadow + shadow-lg ΓåÆ surface band level-2; bß╗Å lß╗¢p `.dark` phß╗º (v├í gradient) v├á `::after` overlay.
2. **N├║t "Hß╗ìc tiß║┐p"**: h├ánh ─æß╗Öng giß║ú (toast placeholder) ΓåÆ mß╗ƒ m├┤ phß╗Ång ─æß║ºu ti├¬n cß╗ºa b├ái thß║¡t (tested: /learn/1 ΓåÆ /simulator/sort.bubble); kh├┤ng c├│ simulation ΓåÆ chuyß╗ân tab L├╜ thuyß║┐t.
3. **"ΓåÉ Vß╗ü lß╗Ö tr├¼nh"** ΓåÆ lucide `ArrowLeft`.
4. **Weight 700 ΓåÆ 600/500** (theory-meta dd, quiz-title); quiz-icon tint primary ΓåÆ neutral `bg-muted` (accent chß╗ë interactive).
5. **Hover card quiz**: bß╗Å `hover-lift` ΓåÆ border-color transition chuß║⌐n.
6. **Breadcrumb mono**; theory card bß╗Å shadow-sm; pre/code giß╗» token.

Ghi nhß║¡n (ngo├ái phß║ím vi view): content CMS c├▓n emoji (≡ƒÄ» "Sß║»p xß║┐p c╞í bß║ún", ≡ƒæë, ≡ƒôÜ trong `contentHtml` cß╗ºa b├ái 1) ΓÇö l├á dß╗» liß╗çu nß╗Öi dung qua `v-html`, kh├┤ng phß║úi code view ΓåÆ Phase 2 (bi├¬n soß║ín nß╗Öi dung / sanitize).

## SimulatorView `/simulator/:key`

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 61.5/100 | 84.5/100 |
| ─Éß║╖c tr╞░ng | 6/10 | 8.5/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | thß╗ï-gi├íc 4 < 8.4; interactive 8 < 9.6 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** |

─É├ú sß╗¡a:
1. **Chrome header**: bß╗Å gradient-mint + blob blur + shadow + text-gradient title ΓåÆ surface band level-2; H1 text-3xl; subtitle c├│ chip Big-O mono.
2. **4 raw `<button>` ΓåÆ Button.vue**: favorite/share = `size="icon"` 40├ù40 (Button.vue th├¬m size icon ΓÇö decision log mß╗Ñc 3) + aria-pressed; toggle Call stack/Legend = ghost sm + lucide ChevronDown/ChevronRight (bß╗Å Γû╛Γû╕).
3. **Canvas frame = motif tß╗æi lan tß╗Åa**: khung canvas-wrap `bg-canvas-ink` + border index-muted 45%; meta bar mono index-muted (bß╗Å 6px/10px/2px hardcode ΓåÆ token).
4. **ControlBar/StatsBar (toolbar)**: indicator + step chip bß╗Å gradient-mint ΓåÆ `bg-primary` + mono; bß╗Å shadow-sm (control-bar, play, bp-badge); weight 700 ΓåÆ 500/600; `#fff` ΓåÆ token.
5. **States**: `__error/__empty/__loading` bß╗Å class `.card` (shadow-md) ΓåÆ `simulator__panel` (bg-card, border, rounded-lg).
6. **Footer ph├¡m tß║»t ΓåÆ mono** (thß╗⌐ "─æo ─æ╞░ß╗úc").

Giß╗» NGUY├èN: CanvasArea + engine + renderer (kh├┤ng ─æß╗Ñng). Ghi nhß║¡n Phase 2: PseudocodePanel/LegendPanel/ExplainPanel/ManualPracticePanel/DemoBanner c├▓n gradient/hex/700 (ngo├ái phß║ím vi khung ngo├ái).

## ExerciseView `/exercise/:id`

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 78/100 | 86.5/100 |
| ─Éß║╖c tr╞░ng | 3/10 | 7.5/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | ─æß║╖c tr╞░ng < 7 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** |

─É├ú sß╗¡a:
1. **Toast**: bß╗Å emoji ≡ƒÄë ΓåÆ "Ho├án th├ánh b├ái tß║¡p!".
2. **Toolbar**: bß╗Å class `.card` (shadow-md) ΓåÆ surface band level-2; kicker mono (bß╗Å 700 + tracking d╞░╞íng rß╗¥i); H1 text-2xl + tracking ├óm.
3. **N├║t toggle** th├¬m `aria-pressed` (trß║íng th├íi bß║¡t/tß║»t cho SR).
4. **Breadcrumb mono**.
5. Th├¬m skeleton/empty/error ─æ├ú c├│ sß║╡n ΓÇö giß╗».

Ghi nhß║¡n: QuizStage (component d├╣ng chung) l├á phß║ºn lß╗¢n UI ΓÇö n├óng ─æß║╖c tr╞░ng tiß║┐p ß╗ƒ Phase 2 nß║┐u cß║ºn ΓëÑ8.

## Kß║┐t quß║ú verify
- `npm run build` (vue-tsc + vite): **PASS** ΓÇö entry index 106.59 kB (tr╞░ß╗¢c 106.44, delta +0.15 kB); PathGraph lazy 154.55 kB.
- `npm test`: **95/95 PASS**.
- Smoke runtime (dev :5176 + BE :5000, ─æ─âng k├╜ user thß║¡t): 4 view render ─æ├║ng light+dark; PathView graph node-edge hoß║ít ─æß╗Öng; 0 console error/warn (trß╗½ 401 refresh khi ch╞░a ─æ─âng nhß║¡p ΓÇö h├ánh vi c┼⌐ cß╗ºa app); 0 overflow 390px; button 40├ù40 icon, node keyboard Enter OK.
- Ollama 3-gate (qwen2.5vl:3b): 5/6 ß║únh Gate 1 = C├ô (nhß║¡n diß╗çn app DSA); Gate 2 = kh├┤ng lß╗ùi UI r├╡. Gate 3 model kh├┤ng ra ─æiß╗âm sß╗æ sß║ích (rambling) ΓÇö ghi log th├┤, dß╗▒a v├áo audit chß╗º quan cho ─æß║╖c tr╞░ng.

## LabView /ladder/:nodeId/lab

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 67/100 | 86/100 |
| ─Éß║╖c tr╞░ng | 5/10 | 8/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | thß╗ï-gi├íc 7 < 8.4 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** |

─É├ú sß╗¡a:
1. **Banner surface band level-2**: bß╗Å icon tile gradient-mint + shadow; th├¬m kicker mono INTERACTIVE LAB ┬╖ NODE 01 + H1 text-3xl tracking ├óm + sub text-sm; breadcrumb mono.
2. **Info cards**: bß╗Å hover shadow-md + translateY ΓåÆ hover border-color 150ms easing chuß║⌐n; icon tile gradient ΓåÆ g-muted + icon foreground-secondary 16px; title text-lg/600.
3. **LabStage canvas = s├ón khß║Ñu tß╗æi** (quyß║┐t ─æß╗ïnh #5): nß╗ün canvas-ink, block data-core 18% + border data-core, index mono 12px index-muted, done ΓåÆ 
esolved; bß╗Å gradient cell--done + shadow.
4. **Bß╗Å emoji ≡ƒÄë** (toast + win message) ΓåÆ text thuß║ºn; bß╗Å weight 800/700 ΓåÆ 600/500.
5. **"ΓåÉ Vß╗ü Ladder"** ΓåÆ lucide ArrowLeft + Button ghost (bß╗Å k├╜ tß╗▒ ΓåÉ).
6. **Cell raw button** (canvas exception ΓÇö decision log): th├¬m ria-pressed + ria-label "├ö x: gi├í trß╗ï y"; bß╗Å shadow hover ΓåÆ border-color + translateY(-1px); easing chuß║⌐n + prefers-reduced-motion.
7. **EmptyState component chung** cho LabStage khi kh├┤ng c├│ dß╗» liß╗çu (motif [ ] + icon database + copy ┬º9).

Kiß╗âm chß╗⌐ng runtime (playwright, BE :5000 + dev :5176, /ladder/1/lab): banner + 3 info cards + 6 cell block render; computed style x├íc nhß║¡n canvasBg rgb(13,16,32) = canvas-ink, cellBg data-core 18%, idx JetBrains Mono #6B7385, cellWeight 600; 0 console error/warn; 0 overflow 390px (375 < 390); light + dark screenshot l╞░u ollama-log/lab-{light,dark}.png.

## CodeRunnerView /code/:key

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 71/100 | 88.5/100 |
| ─Éß║╖c tr╞░ng | 6/10 | 8.5/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | thß╗ï-gi├íc 6.5 < 8.4 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** |

─É├ú sß╗¡a:
1. **Bß╗Å ghi ch├║ dev lß╗Ö UI** (r2-fixed-09): "* Monaco editor sß║╜ ─æ╞░ß╗úc bß║¡t khi c├ái g├│i monaco-editor (SDD M├án 16...)" ΓåÆ thay bß║▒ng caption ph├¡m tß║»t mono hß╗»u ├¡ch: "Ctrl+Enter chß║íy code ┬╖ Ctrl+Z ho├án t├íc ┬╖ Ctrl+Shift+Z l├ám lß║íi" (kbd chuß║⌐n).
2. **Chrome header**: bß╗Å gradient-mint + border tint primary + shadow-md + ::after overlay ΓåÆ surface band level-2 (bg-card-raised + border-border-subtle); kicker mono CODE CHALLENGE ┬╖ key; H1 text-3xl text-foreground (bß╗Å ≡ƒÆ╗ + text-gradient); sub text-sm text-secondary.
3. **V├╣ng code LU├öN tß╗æi** (quyß║┐t ─æß╗ïnh #5): editor-wrap canvas-ink + gutter tß╗æi + textarea text color-mix(white 85%, index-muted) mono 14px (bß╗Å 13px + nß╗ün theo theme); output-box c┼⌐ng canvas-ink.
4. **Panel bß╗Å shadow-sm** ΓåÆ border chuß║⌐n (elevation level-1); panel-title weight 700ΓåÆ600, icon 15ΓåÆ16px, bß╗Å tint primary tr├¬n svg.
5. **ΓùÇ Γû╢ ΓåÆ lucide StepBack/StepForward** + Button size icon + aria-label; "Γû╢ Chß║íy" ΓåÆ lucide Play; icon History 16px.
6. **Empty state ΓåÆ EmptyState component chung** (motif [ ] + icon database + copy ┬º9 "Kh├┤ng t├¼m thß║Ñy b├ái thß╗¡ th├ích" + action "Vß╗ü danh mß╗Ñc").
7. Spacing: gap 6pxΓåÆ8px (gap-2), margin 4pxΓåÆspace-xs, textarea padding 14pxΓåÆ16px, gutter 48px + padding token.
8. Breadcrumb mono; lß╗ïch sß╗¡ nß╗Öp bß╗Å class .card (shadow) ΓåÆ panel border chuß║⌐n + date mono.

Kiß╗âm chß╗⌐ng runtime (playwright, /code/sort.bubble + /code/not-a-real-key): editor-wrap computed bg rgb(13,16,32), taColor s├íng, chromeBg card-raised + boxShadow none + backgroundImage none; noteText = ph├¡m tß║»t (kh├┤ng c├▓n Monaco); empty state render motif [0 1 2] + action; 0 console error; 0 overflow 390px; light+dark screenshot ollama-log/code-{light,dark}.png. Giß╗» nguy├¬n: textarea aria-label + "Th├ánh c├┤ng ┬╖ Xms" (e2e).

## BenchmarkView /benchmark/:k1/:k2 (+ BenchmarkPanel ΓÇö nß╗Öi dung view, decision log P1-B3 mß╗Ñc 1)

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 65/100 | 87.5/100 |
| ─Éß║╖c tr╞░ng | 5/10 | 8/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | thß╗ï-gi├íc 5 < 8.4; depth 4.5 < 4.8; interactive 9.5 < 9.6 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** |

─É├ú sß╗¡a:
1. **Chrome header**: bß╗Å gradient-mint + border tint + shadow-md + ::after overlay ΓåÆ surface band level-2 (bg-card-raised + border-subtle) + kicker mono `BENCHMARK ┬╖ sort.bubble vs sort.merge` + H1 text-3xl/600/-0.02em (bß╗Å text-gradient).
2. **Label "Miß╗àn ph├¡ tim (20.4)"** (BenchmarkPanel.vue:282 + BenchmarkView.vue:50): KH├öNG c├│ trong i18n ΓÇö hardcode; "(20.4)" l├á sß╗æ mß╗Ñc SDD lß╗Ö UI ΓåÆ **"Kh├┤ng tß╗æn tim"** (r├╡ ngh─⌐a: benchmark kh├┤ng trß╗½ tim ng╞░ß╗¥i hß╗ìc; VD "T├¼m kiß║┐m tuyß║┐n t├¡nh ΓÇö 20.4ms" trong prompt kh├┤ng ─æ├║ng ngß╗» ngh─⌐a m├án h├¼nh ΓÇö decision log mß╗Ñc 2).
3. **Empty state**: icon "scissors" (c├óy k├⌐o v├┤ ngh─⌐a) ΓåÆ EmptyState chung icon "hourglass" (─æo thß╗¥i gian) + copy ┬º9 "Ch╞░a c├│ sß╗æ liß╗çu ─æo ΓÇö chß╗ìn 2+ giß║úi thuß║¡t ph├¡a tr├¬n rß╗ôi bß║Ñm Chß║íy benchmarkΓÇª".
4. **Chips raw `<button>` (5-17)** ΓåÆ Button.vue sm variant primary/secondary + aria-pressed; bß╗Å gradient chip + shadow-sm + translateY hover; easing --transition-fast ΓåÆ border-color 150ms cubic-bezier chuß║⌐n.
5. **Kß║┐t quß║ú benchmark = v├╣ng dß╗» liß╗çu LU├öN tß╗æi**: bß║úng + chart + kß║┐t luß║¡n nß╗ün canvas-ink; n = index mono, duration = block-token data-core (border/14%), so s├ính = mono index-muted; palette ECharts ─æß╗ìc CSS var canvas palette (data-core/resolved/conflict/warning/info), fallback hex chß╗ë ph├▓ng SSR (pattern cssVar CodeRunnerView).
6. **Mobile Γëñ640px**: bß║úng ΓåÆ card-stack (td::before data-label mono), bß╗Å scroll ngang bß║úng ch├¡nh (┬º8).
7. ΓÜû/Γû╢ ΓåÆ lucide Scale/Play; weight 800 ΓåÆ 600/500; select chuß║⌐n h├│a 40px; breadcrumb mono; kß║┐t quß║ú region enter 250ms easing chuß║⌐n + prefers-reduced-motion.

Kiß╗âm chß╗⌐ng runtime (chrome-devtools, /benchmark/sort.bubble/sort.merge ΓÇö ─æ─âng k├╜ user QA mß╗¢i): banner surface band (dark #134E4A / light #F7FDFD, boxShadow none, backgroundImage none); chips Button sm + aria-pressed; chß║íy benchmark thß║¡t ΓåÆ bß║úng tß╗æi rgb(13,16,32) + block data-core (bg 14%/border 45%/mono) + n index mono 12px + chart + conclusion tß╗æi; console 0 error/warn/issue (─æ├ú fix ECharts containLabel ΓåÆ LegacyGridContainLabel + select name); 390px card-stack (thead none, tr grid, td::before data-label, wrap overflow visible, scrollW=clientW=390). Screenshot ollama-log/benchmark-{light,dark,mobile-light}.png ΓÇö Gate1 C├ô, Gate2 sß║ích, Gate3 8/10.

## PathRedirectView /path (redirect tß╗½ /learn ΓÇö topic selector 5 chß╗º ─æß╗ü)

| Hß║íng mß╗Ñc | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 72.5/100 | 86/100 |
| ─Éß║╖c tr╞░ng | 4/10 | 8/10 |
| Trß╗Ñc d╞░ß╗¢i s├án | thß╗ï-gi├íc 6.5 < 8.4; ─æß║╖c tr╞░ng 4 < 7 | KH├öNG c├▓n |
| ─Éß║ít/kh├┤ng | KH├öNG ─Éß║áT | **─Éß║áT** |

─É├ú sß╗¡a:
1. **Banner**: header phß║│ng + ≡ƒÄ» ΓåÆ surface band level-2 + kicker mono "LEARNING PATH ┬╖ CHß╗îN CHß╗ª ─Éß╗Ç" + H1 text-3xl/600/-0.02em + Route icon lucide.
2. **Card topic**: `<article role="button" tabindex="0">` (thiß║┐u Space) ΓåÆ RouterLink (Enter/Space/focus-visible native); bß╗Å openTopic() (logic qua router).
3. **Bß╗Å .card/.card--interactive** (shadow-md + hover shadow-lg/translateY/scale) ΓåÆ card level-1, hover border-color 150ms chuß║⌐n.
4. **Index v├▓ng tr├▓n primary + weight 800** ΓåÆ kicker mono index-muted `TOPIC 01/05` trong card (bß╗Å accent trang tr├¡).
5. **EmptyState icon "map"** (KH├öNG tß╗ôn tß║íi trong SVG_PATHS ΓåÆ fallback x-circle ├óm thß║ºm) ΓåÆ "book".
6. Skeleton 88px ΓåÆ 150px khß╗¢p card thß║¡t; card grid stagger enter 240ms (max 8├ù40ms) + prefers-reduced-motion; card title text-lg/600; note fallback text-tertiary xs.

Kiß╗âm chß╗⌐ng runtime (chrome-devtools, /path): banner band + 5 card RouterLink (href /path/1..5, focus-visible ring, Enter/Space native); computed: chrome bg card-raised (light #F7FDFD / dark #134E4A), shadow none, card shadow none, kicker JetBrains Mono 12px tertiary, H1 36px/600/tracking -0.72px; 0 console error; 0 overflow 390px (scrollW=clientW=390); 5/6 ß║únh Ollama Gate1 C├ô (model nhiß╗àu "emoji" tr├¬n 2 ß║únh ΓÇö notes.md mß╗Ñc P1-B3.2). Screenshot ollama-log/path-redirect-{light,dark,mobile-dark}.png.

<!-- MERGED-BLOCK: nhom khac -->
> Bß║úng tr╞░ß╗¢c/sau theo `standard.md` (10 trß╗Ñc hygiene + ─Éß║╖c tr╞░ng t├ích ri├¬ng). Audit chi tiß║┐t tß╗½ng view tß║íi `audit-<view>.md`. Ng├áy: 13/08/2026.

| view | hygiene tr╞░ß╗¢c | hygiene sau | ─æß║╖c-tr╞░ng tr╞░ß╗¢c | ─æß║╖c-tr╞░ng sau | thay ─æß╗òi ch├¡nh |
|---|---|---|---|---|---|
| HomeView `/` | 64.5 | 95.0 | 2 | 9 | Bß╗Å header nß╗Öi bß╗Ö (fix 2 header chß╗ông ΓÇö AppHeader to├án cß╗Ñc giß╗»); hero: bß╗Å gradient aurora + blob + shadow ΓåÆ surface band level-2 + panel demo tß╗æi canvas-ink chß║íy **mini-sim step THß║¼T tß╗½ engine** (sort.bubble / search.binary / graph.bfs qua `getSimulation()` + `generate()`, DOM block + index mono, status data-core/resolved/conflict, counter `b╞░ß╗¢c x/y`, tabs aria-pressed, prefers-reduced-motion dß╗½ng autoplay); demo-btn `<button>` raw ΓåÆ Button.vue; icon squares gradient ΓåÆ muted surface; stat-card bß╗Å gradient/800/shadow ΓåÆ level-1 Geist 600; CTA RouterLink qua `buttonVariants`; font-weight Γëñ600; tracking ├óm heading; hover card chß╗ë ─æß╗òi border |
| LoginView `/login` | 65.5 | 95.0 | 2 | 8 | Aside gradient aurora + blob + glassmorphism ΓåÆ panel tß╗æi `canvas-ink` + badge mono + block-token strip + index mono; title hß║┐t gradient (H1 48px/600/-0.03em); shell bß╗Å `shadow-xl` (elevation = surface + border); icon BaseIcon ΓåÆ lucide-vue-next (Input.vue hß╗ù trß╗ú icon Component); n├║t submit size lg (44px); forgot/back link hit-target ΓëÑ 24px |
| RegisterView `/register` | 61.0 | 95.0 | 2 | 8 | Nh╞░ LoginView + segmented vai tr├▓ 2 `<button>` raw ΓåÆ Button.vue ghost + `aria-pressed` + active bg-card/border (bß╗Å shadow-sm); Γ£ô/Γùï glyph ΓåÆ lucide Check/Circle; Motion easing `easeOut 0.32s` ΓåÆ `[0.16,1,0.3,1] 0.28s`; teacher box bß╗Å t├┤ primary 4% ΓåÆ `bg-muted`; textarea `.input` legacy ΓåÆ token class; checkbox hit 18px; giß╗» nguy├¬n selector e2e (form.register__card / label.register__row / button.register__role-option) |
| ForgotPasswordView `/forgot-password` | 66.0 | 95.0 | 2 | 8 | Aside tß╗æi Data Bench (nh╞░ tr├¬n); bß╗Å `ΓåÉ` k├╜ tß╗▒ ΓåÆ ArrowLeft lucide; sent-icon tr├▓n ΓåÆ square radius-md (hß║┐t "icon tr├▓n"); Motion easing chuß║⌐n; shell hß║┐t shadow; weight Γëñ600 |
| ResetPasswordView `/reset-password` | 64.0 | 95.0 | 2 | 8 | Nh╞░ Forgot + timer redirect `setTimeout` c├│ `onUnmounted` cleanup (trß╗Ñc 9); checklist Γ£ô/Γùï ΓåÆ lucide; v-for key ß╗òn ─æß╗ïnh (key rule.key thay idx) |
| NotFoundView `404` | 55.5 | 96.5 | 1 | 9 | Bß╗Å v├▓ng gradient aurora + Compass + shadow-xl + 800 + heading gradient ΓåÆ panel tß╗æi canvas-ink "mß║úng mß║Ñt index": block 4┬╖0┬╖4 (data-core) + block 03 dashed conflict "out of bounds" + index mono 00-03 + label mono; CTA qua buttonVariants lg; Motion 280ms chuß║⌐n |
| PrivacyView `/privacy` | 54.0 | 94.0 | 1 | 7 | Hero aurora-soft + gradient icon/title + shadow ΓåÆ surface band level-2; TOC raw `<button>` ΓåÆ native anchor `#sec-N` (Lenis smooth, bß╗Å JS); index mono 01-06 TOC + section; H1 48px; toc-link 8/12px; bß╗Å shadow TOC; h2 section H4 24px/-0.015em |
| HelpView `/help` | 54.0 | 94.5 | 1 | 7 | Hero aurora-soft + gradient ΓåÆ surface band level-2; FAQ `.card` legacy (shadow + all 250ms ease) ΓåÆ token card; trigger raw `<button>` ΓåÆ buttonVariants ghost + aria-controls; chevron/FAQ transition easing chuß║⌐n; submit lg 44px; FAQ index mono 01-06; sent-title 700ΓåÆ600 |
| SimulationsView `/simulations` | 53.0 | 94.0 | 3 | 9 | Chrome gradient mint + shadow ΓåÆ surface band level-2; strip block-token + index mono trong banner; stat bß╗Å gradient/800 ΓåÆ Geist 600 + label tertiary; card bß╗Å hover-lift ΓåÆ hover ─æß╗òi border + Space key; BenchmarkPanel/CheatSheetTable ΓåÆ defineAsyncComponent (lazy tab); badge min-h-6 (Badge.vue shared) |
| CheatSheetView `/cheatsheet` | 55.0 | 94.0 | 3 | 9 | Chrome gradient mint ├ù3 (bg/icon/title) + shadow + overlay ΓåÆ surface band level-2 + strip Big-O block-token `BIG-O 00ΓÇô04` (chip tß╗æi canvas-ink + index mono); H1 48px/600/-0.03em; badge count primaryΓåÆmuted; raw `<button>` ├ù2 (chip lß╗ìc + "Γû╢ Xem m├┤ phß╗Ång") ΓåÆ Button shadcn outline sm (aria-pressed; Play lucide); Big-O text mono trß║ºn ΓåÆ block-token chip tß╗æi canvas-ink + mono text-sm; th hß║┐t uppercase/tracking d╞░╞íng + scope=col; bß║úng mobile Γëñ640 ΓåÆ card-stack (cß║Ñm scroll ngang ┬º8, td data-label); hardcode chuß╗ùi ΓåÆ i18n cheatsheet.*; search input th├¬m name (a11y issue 0) |

## Ghi ch├║ chung
- **HOTFIX to├án app (─æ├ú ghi decision log)**: `global.css` reset `* { padding: 0 }` unlayered ─æ├¿ mß╗ìi padding utility Tailwind (─æo thß║¡t: input c├│ `px-3 pl-9` nh╞░ng computed `paddingLeft=0px`) ΓåÆ ─æ╞░a v├áo `@layer base`; kh├┤i phß╗Ñc padding thiß║┐t kß║┐ cho mß╗ìi shadcn control (kh├┤ng chß╗ë 5 view).
- **HOTFIX 2 to├án app (─æ├ú ghi decision log)**: `global.css` `a { color: var(--color-primary) }` unlayered ─æ├¿ `text-primary-foreground` tr├¬n mß╗ìi RouterLink-as-button ΓåÆ chß╗» teal tr├¬n nß╗ün teal ~1.2:1 (fail WCAG, ─æo thß╗▒c tß║┐ CTA HomeView + 404) ΓåÆ ─æ╞░a v├áo `@layer base` + `a.inline-flex:hover { text-decoration: none }`; sau sß╗¡a CTA 404 = trß║»ng/teal, contrast 21:1.
- **Page transition easing**: `220ms ease` ΓåÆ enter `cubic-bezier(0.16,1,0.3,1)` / exit `cubic-bezier(0.7,0,0.84,0)` (global.css, ß║únh h╞░ß╗ƒng mß╗ìi route).
- **Input.vue** (d├╣ng chung 10 view): th├¬m prop `icon` nhß║¡n `string | Component` ΓÇö string legacy vß║½n qua BaseIcon, Component mß╗¢i (lucide) ΓÇö backward-compatible.
- **Badge.vue** (shared, decision log 2026-08-13): th├¬m `min-h-6` ΓÇö badge height ΓëÑ 24px (trß╗Ñc 5f); ─æo thß╗▒c tß║┐ 25.6px.
- Bß║▒ng chß╗⌐ng re-audit: ─æo computed style (padding button 32/12px, input 12px+icon, height 44/40/36px), 3 mß╗æc breakpoint (1366/768/390 ΓÇö kh├┤ng overflow, kh├┤ng ─æ├¿ chß╗»), console error = 0, 95/95 test PASS, build PASS.
