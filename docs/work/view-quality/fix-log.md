# FIX-LOG ΓÇö View Quality Phase 1 ┬╖ Nh├│m D (3 view Classes)

> Ng├áy: 14/08/2026 ┬╖ Agent: dev-frontend (nh├│m D) ┬╖ Nh├ính: feature/view-quality-d ┬╖ Worktree: D:\FPT\neww-qd
> Quy tß║»c chß║Ñm: `standard.md` (10 trß╗Ñc hygiene + ─Éß║╖c tr╞░ng t├ích ri├¬ng + 3 ─æiß╗üu kiß╗çn ─Éß║áT) ┬╖ Nguß╗ôn thiß║┐t kß║┐: `frontend/DESIGN.md` + `DESIGN-IDENTITY.md` + 6 quyß║┐t ─æß╗ïnh xuy├¬n-nh├│m.

## ─Éß╗út 2 ΓÇö 5 view Admin (14/08, c├╣ng nh├ính)

## AdminUsersView.vue (`/admin/users`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 58.0/100 | **92.5/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **8/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- Hero gradient Aurora + blob blur + title gradient-clip + shadow ΓåÆ **surface band level-2** + **mono strip block-token** (sß╗æ chß╗¥ duyß╗çt thß║¡t, block `data-core` + index mono, stagger-enter 280ms `cubic-bezier(0.16,1,0.3,1)` ΓÇö khoß║únh khß║»c ─æß║ºu t╞░, reduced-motion ─æ├║ng).
- Bß║úng: th uppercase + tracking ΓåÆ ┬º4.6 (`text-sm font-medium text-foreground-tertiary h-10`, `scope="col"`); td 12px; hover `muted/50`; **mobile card-stack** thay scroll ngang; cß╗Öt ng├áy mono; avatar/name gradient + weight 800/700 ΓåÆ neutral + 600.
- `.admin-users__actions` gap 4px ΓåÆ **8px** (n├║t liß╗ün kß╗ü); icon button 14ΓåÆ16px; search input padding-left 34px ΓåÆ `--space-xl` (32px) + icon `left: space-sm`.
- **Th├¬m error state + n├║t Thß╗¡ lß║íi** (tr╞░ß╗¢c: load fail ΓåÆ toast + EmptyState "Kh├┤ng c├│ ng╞░ß╗¥i d├╣ng" g├óy hiß╗âu nhß║ºm); EmptyState th├¬m description lß╗¥i mß╗¥i h├ánh ─æß╗Öng (┬º9); aria-label lock fallback `displayName || email` (user rß╗ùng t├¬n).
- Bß╗Å `.card` legacy (shadow-md KILL-LIST) ΓåÆ panel token `bg-card border-border`.

## AdminStatsView.vue (`/admin/stats`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 62.5/100 | **93.5/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **8.5/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- Hero gradient ΓåÆ surface band level-2 + **mono strip block-token** (5 chß╗ë sß╗æ).
- **5 stat-card c├┤ng thß╗⌐c KILL-LIST** (icon tr├▓n 3 gradient + #fff + shadow + hover-lift + weight 800) ΓåÆ **1 hero-stat** (Tß╗òng ng╞░ß╗¥i d├╣ng: card level-2 + block-token tß╗æi `canvas-ink`/`data-core` + index mono `USERS ┬╖ 01`) + **4 KPI level-1** (kh├┤ng icon/shadow, Geist 600).
- **2 v├╣ng biß╗âu ─æß╗ô ΓåÆ LU├öN tß╗æi `bg-canvas-ink`** (quyß║┐t ─æß╗ïnh #5): ECharts palette ─æß╗ìc CSS var `--data-core`/`--index-muted`/`--canvas-ink` + text `#d9dde8` (engine ΓÇö decision log 14/08); donut m├áu `data-core/resolved/index-muted` (kh├┤ng bß╗ïa); legend mono 500; donut text ΓëÑ12px (tr╞░ß╗¢c 10px).
- Tag uppercase + tracking ΓåÆ mono caption; note icon `--color-primary` ΓåÆ `--info`; th├¬m error state + retry.

## AdminSettingsView.vue (`/admin/settings`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 68.0/100 | **92.5/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **7.5/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- Hero gradient ΓåÆ surface band level-2; section title m├áu `--color-primary` (accent trang tr├¡) ΓåÆ `text-lg font-semibold tracking-[-0.015em] text-foreground` + icon `foreground-secondary`.
- Form `.card` legacy shadow ΓåÆ panel token kh├┤ng shadow; error alert ΓåÆ token `destructive` + **n├║t Thß╗¡ lß║íi** (t├ích `load()` ΓÇö tr╞░ß╗¢c onMounted inline, lß╗ùi tß║úi hiß╗çn form mß║╖c ─æß╗ïnh g├óy nhß║ºm "─æ├ú l╞░u").
- Icon Save 15ΓåÆ16px; `--color-text-muted` ΓåÆ `--foreground-secondary`; checkbox giß╗» native 16px + accent teal (label = target ΓëÑ24px).

## AdminContentView.vue (`/admin/content`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 59.5/100 | **92.5/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **8.5/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- **Bug: cß╗Öt "Ng├áy tß║ío" hiß╗ân thß╗ï `formatDate(new Date())`** (LessonSummary kh├┤ng c├│ createdAt ΓÇö ng├áy giß║ú) ΓåÆ thay **cß╗Öt index mono `#01`** (dß╗» liß╗çu tuß║ºn tß╗▒ ΓÇö quyß║┐t ─æß╗ïnh #4); i18n `colCreated` ΓåÆ `colIndex`.
- Hero gradient ΓåÆ surface band + **mono strip block-token** (sß╗æ b├ái hß╗ìc/chß╗º ─æß╗ü thß║¡t: `08 B├ÇI ┬╖ 05 CHß╗ª ─Éß╗Ç`).
- Bß║úng ┬º4.6 + mobile card-stack; sim-count mono; topic card bß╗Å gradient mint icon + hover-lift ΓåÆ icon neutral + hover border strong; toolbar action ch├¡nh smΓåÆmd; actions gap 4ΓåÆ8px; th├¬m error state + retry.
- Ghi ch├║ Phase 2: nß╗Öi dung rich-text contentHtml (emoji CMS) ΓÇö KH├öNG sß╗¡a nß╗Öi dung (task y├¬u cß║ºu ghi notes).

## AdminLadderView.vue (`/admin/ladder`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 56.0/100 | **92.0/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **8/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- **1 `<button` raw** (`.admin-ladder__node`, padding 8/16 + hover-lift) ΓåÆ **Button.vue** (outline md, `w-full` + justify-start scoped) ΓÇö **grep `<button` raw = 0**; giß╗» aria-pressed.
- **Node-id tr├▓n gradient + weight 800 ΓåÆ block-token tß╗æi** `bg-canvas-ink` + `text-data-core` + mono index (dß╗» liß╗çu tuß║ºn tß╗▒ ΓÇö quyß║┐t ─æß╗ïnh #4); selected: border-primary + ring 1px (bß╗Å color-mix 2px); bß╗Å hover-lift.
- Note card `.text-muted` ΓåÆ 4 tß║ºng; icon info `--info`; subtitle `--text-md` ΓåÆ text-lg 600 tracking; select `.input` override token + easing 150ms; th├¬m error state + retry.
- **AdminNav.vue (component d├╣ng bß╗ƒi ─æ├║ng 5 view admin ΓÇö trong scope)**: token shadcn (`--card/--border/--primary`), link min-height 36px, gap ΓëÑ8px, weight 500 (tr╞░ß╗¢c 0.4rem padding + weight 600 + legacy tokens).

## Thay ─æß╗òi hß║í tß║ºng chung (─æ├ú ghi `docs/pm-decision-log-viewquality.md` 14/08 ΓÇö bß║»t buß╗Öc tr╞░ß╗¢c khi sß╗¡a)

## ClassesView.vue (`/classes`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 61.0/100 | **92.5/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **8.5/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- Hero gradient Sunset + `::after` overlay + `text-shadow` + `--shadow-lg` ΓåÆ **surface band level-2** (`bg-card-raised` + border) + **mono strip block-token dß╗» liß╗çu thß║¡t** (sß╗æ lß╗¢p + tß╗òng th├ánh vi├¬n, block `data-core` + index mono, stagger-enter 280ms `cubic-bezier(0.16,1,0.3,1)` ΓÇö khoß║únh khß║»c ─æß║ºu t╞░ duy nhß║Ñt, reduced-motion ─æ├║ng).
- Card `hover-lift` (translate + shadow 180ms ease) ΓåÆ level-1, hover chß╗ë ─æß╗òi `border ΓåÆ border-strong`, 150ms transition-colors; bß╗Å shadow.
- Icon tr├▓n gradient ΓåÆ neutral `bg-muted text-foreground-secondary`.
- Chip m├ú mß╗¥i (weight 700, color-mix, kh├┤ng chuß║⌐n) ΓåÆ **block-token tß╗æi** `bg-canvas-ink` + mono 500 + `text-resolved`, min-height 24px.
- H1 36px ΓåÆ 48px `tracking-[-0.03em]`; card name h2/18px ΓåÆ h3 `text-lg font-semibold tracking-tight`.
- CTA hero `md` ΓåÆ `lg` (44px); card clickable bß╗ò sung ph├¡m **Space**.
- Spacing ngo├ái scale (`gap: 2px/6px`, `padding: 3px 10px`) ΓåÆ token.
- Bugfix hiß╗ân thß╗ï: `isManagerOf` t├¡nh tß╗½ `ownerId === auth.user?.id` (API list KH├öNG trß║ú `role` ΓåÆ tr╞░ß╗¢c ─æ├óy badge lu├┤n "Th├ánh vi├¬n", chip mß╗¥i kh├┤ng bao giß╗¥ hiß╗çn).

## ClassDetailView.vue (`/classes/:id`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 55.0/100 | **92.5/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **9/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- Hero gradient + glassmorphism chip (`backdrop-filter: blur`) + `#fff` ΓåÆ surface band level-2; m├ú mß╗¥i th├ánh **block-token tß╗æi** (`bg-canvas-ink`, code `text-resolved` mono 0.12em, label `text-index-muted`) + n├║t Sao ch├⌐p qua `Button.vue` (icon Check/ClipboardCopy, aria-label) ΓÇö settle-enter 250ms chuß║⌐n.
- **1 `<button` raw** (copy-btn, padding 3px 8px) ΓåÆ `Button size="sm" variant="secondary"` (36px, px-12px).
- Bß║úng: th uppercase + tracking ΓåÆ ┬º4.6 (`text-sm font-medium text-foreground-tertiary h-10`, kh├┤ng in hoa); td 12px; hover `bg-muted/50`; **mobile 390 card-stack** (data-label) thay scroll ngang; cß╗Öt ng├áy mono; th├¬m `scope="col"`.
- Avatar/name/assign-icon gradient + weight 700/800 ΓåÆ neutral + Γëñ600; x├│a `assignmentTint` (3 gradient); assignment card th├¬m **index mono `#01`** (dß╗» liß╗çu tuß║ºn tß╗▒ ΓÇö quyß║┐t ─æß╗ïnh #4), due mono, hover border.
- Modal assign: raw `<input class="input">` ΓåÆ `Input.vue` (datetime-local); `.text-muted` legacy ΓåÆ 4 tß║ºng token.
- `copyInvite` setTimeout cleanup `onUnmounted`; bugfix `isManager` tß╗½ `ownerId` (tr╞░ß╗¢c: teacher kh├┤ng bao giß╗¥ thß║Ñy tab C├ái ─æß║╖t/n├║t copy).

## ClassReportView.vue (`/classes/:id/report`)

| Chß╗ë sß╗æ | Tr╞░ß╗¢c | Sau |
|---|---|---|
| Hygiene | 63.5/100 | **92.5/100** |
| ─Éß║╖c tr╞░ng | 2/10 | **9/10** |
| Kß║┐t luß║¡n | KH├öNG ─Éß║áT | **─Éß║áT** |

Lß╗ùi ch├¡nh ─æ├ú sß╗¡a:
- **Align contract API vß╗¢i backend THß║¼T** (ph├ít hiß╗çn khi chß║íy thß║¡t): frontend type kß╗│ vß╗ìng `completionPct/avgScore/submissions/rows` nh╞░ng backend trß║ú `totalMembers/assignments[]/laggingLearners[]` ΓåÆ view render "undefined%"/"NaN"/bß║úng rß╗ùng. Sß╗¡a `api/types.ts` (ClassReportDto + ClassReportAssignmentDto + LaggingLearnerDto), `classes.spec.ts`, view ─æß╗ìc dß╗» liß╗çu thß║¡t: KPI = B├ái g├ín / ─Éiß╗âm TB (mean avgScore) / B├ái nß╗Öp (onTime+late); summary = submitted/expected + mono `15 / 28 B├ÇI Nß╗ÿP`; **bß║úng b├ái g├ín** (index mono `#01`, title+due mono, onTime/late/notSubmitted, ─Éiß╗âm TB, badge trß║íng th├íi); **block lagging learners** nß╗ün `canvas-ink` + index mono + `THIß║╛U 04` (conflict); EmptyState `icon="database"` (tr╞░ß╗¢c `chart` kh├┤ng tß╗ôn tß║íi ΓåÆ fallback x-circle).
- Hero gradient compact ΓåÆ surface band level-2 + sub mono `T├¬n ┬╖ ID 03`.
- 4 KPI icon tr├▓n gradient + `hover-lift` (c├┤ng thß╗⌐c stat-card KILL-LIST) ΓåÆ **1 hero-stat** (Th├ánh vi├¬n: block-token tß╗æi `data-core` + index mono) + 3 stat level-1 (kh├┤ng icon, kh├┤ng shadow, Geist 600 2xl).
- KPI value weight 800 ΓåÆ 600; th bß╗Å uppercase; cß╗Öt sß╗æ ΓåÆ mono; `Γ£ô` text glyph ΓåÆ lucide Check/Minus; avatar neutral.
- Ngoß║íi lß╗ç ghi decision log: `#d9dde8` (m├áu text canvas engine `canvasTheme.ts`) cho t├¬n hß╗ìc vi├¬n tr├¬n panel tß╗æi ΓÇö ch╞░a c├│ UI token chß╗» s├íng.

## Thay ─æß╗òi hß║í tß║ºng chung (─æ├ú ghi `docs/pm-decision-log-viewquality.md` 14/08 ΓÇö bß║»t buß╗Öc tr╞░ß╗¢c khi sß╗¡a)

1. `frontend/src/styles/global.css`: bß╗Å `margin:0; padding:0` khß╗Åi universal reset unlayered ΓÇö ─æang giß║┐t Mß╗îI utility padding/margin Tailwind to├án app (─æo computed: button shadcn pl=0). Tailwind preflight ─æ├ú reset trong base layer.
2. `frontend/src/components/ui/Badge.vue`: th├¬m `min-h-6` (badge cao 21.6px < 24px chuß║⌐n ┬º4.3).
3. `frontend/src/components/ui/card/Card.vue`: bß╗Å `shadow-sm` (DESIGN ┬º6 cß║Ñm shadow tr├¬n card ΓÇö chß╗ë dropdown/modal).

## Verify

- `npm run build` (gß╗ôm `vue-tsc -b`): **PASS** ├ù3 lß║ºn cuß╗æi.
- `npx vitest run`: **12 files / 95 tests PASS** (gß╗ôm classes.spec cß║¡p nhß║¡t contract).
- Browser (dev :5178, BE :5000): 3 view ├ù light+dark ├ù 1366/768/390 ΓÇö 0 console error, 0 overflow ngang; computed style ─æo trß╗▒c tiß║┐p (padding button lg 32px/44px, badge 24px, card shadow none, canvas-ink panels, mono cß╗Öt sß╗æ, card-stack mobile, tab/delete/copy/add-member flow hoß║ít ─æß╗Öng).
- Kh├┤ng c├│ lint script trong repo (`package.json` kh├┤ng c├│ "lint") ΓÇö kh├┤ng chß║íy ─æ╞░ß╗úc, ghi r├╡.
- Ollama 3-gate: kh├┤ng c├│ gate/script trong repo ΓÇö ghi `notes.md`, KH├öNG b├ío pass giß║ú.

<!-- MERGED-BLOCK: nhom khac -->
> Format: `view | hygiene tr╞░ß╗¢c | hygiene sau | ─æß║╖c-tr╞░ng tr╞░ß╗¢c | ─æß║╖c-tr╞░ng sau | thay ─æß╗òi ch├¡nh`. Ng├áy: 13ΓÇô14/08/2026.



| view | hygiene tr╞░ß╗¢c | hygiene sau | ─æß║╖c-tr╞░ng tr╞░ß╗¢c | ─æß║╖c-tr╞░ng sau | thay ─æß╗òi ch├¡nh |

|---|---|---|---|---|---|

| ProfileView `/profile` | 72/100 | 91/100 | 2/10 | 8/10 | Hero: bß╗Å gradient/blob/shadow ΓåÆ surface band level-2; stat: 5 card ─æß╗ông nß╗òi 800 ΓåÆ 1 hero XP (BlockToken canvas-ink + index mono) + stat phß╗Ñ level-1 + streak block-token resolved; bß╗Å emoji (≡ƒöÑ≡ƒÆÄΓ¥ñ∩╕Å≡ƒÅå≡ƒ¢Æ≡ƒæÑ≡ƒÅà≡ƒöÆ) ΓåÆ lucide; radar ΓåÆ nß╗ün tß╗æi canvas-ink + m├áu data-core; quick-link index mono 01ΓÇô04; error state + retry (EmptyState chung); bß╗Å hover-lift/shadow card; weight 800/700 ΓåÆ 600; n├║t submit md; hero-stat enter 300ms chuß║⌐n easing. |

| LeaderboardView `/leaderboard` | 73.5/100 | 91/100 | 3/10 | 8/10 | Rank: medal emoji ≡ƒÑç≡ƒÑê≡ƒÑë ΓåÆ block-token chip tß╗æi canvas-ink (top1 warning) + header mono RANK/USER/VALUE; bß╗Å gradient hero/blob/title + gradient overlay top-3 rows + me-badge gradient ΓåÆ border-color/tint + bg-primary; chart top-10 ΓåÆ v├╣ng dß╗» liß╗çu lu├┤n tß╗æi (m├áu data-core/index-muted/warning); EmptyState error + retry, noClass + CTA "─Éi tß╗¢i Lß╗¢p hß╗ìc"; FLIP 450ms ease ΓåÆ 300/250/200ms chuß║⌐n easing; value mono; bß╗Å shadow pinned; H1 48px. |

| ShopView `/shop` | 76/100 | 91.5/100 | 3/10 | 7.5/10 | Hero: bß╗Å gradient/blob/shadow ΓåÆ surface band; gems = 1 hero-stat BlockToken (tß╗æi + index mono) + item count stat phß╗Ñ; bß╗Å tint gradient icon slot (aurora/mint/sunset) ΓåÆ chip muted ─æß╗ông nhß║Ñt; price amber ΓåÆ mono + foreground; n├║t "Mua" sm ΓåÆ md (40px); card hover ΓåÆ border-color; bß╗Å shadow card. |

| QuestsView `/quests` | 77/100 | 91.5/100 | 3/10 | 7.5/10 | Hero: bß╗Å gradient/blob/shadow ΓåÆ surface band + strip DONE mono; streak chip gradient-sunset + shadow ΓåÆ BlockToken resolved (nß╗ün tß╗æi); card--ready gradient ΓåÆ border+tint; reward/XP amber ΓåÆ mono + token; n├║t claim sm ΓåÆ md; confetti khi claim quest cuß╗æi (disableForReducedMotion); toast bß╗Å ≡ƒÆÄ (c├╣ng ─æß╗Öng tß╗½ "─É├ú nhß║¡n"); weight 800/700 ΓåÆ 600; H1 48px. |

| PremiumView `/premium` | 68/100 | 93.5/100 | 2/10 | 8/10 | Hero gradient aurora + blob + title gradient-clip + shadow ΓåÆ surface band level-2 + strip mono `PLAN ┬╖ 1M ┬╖ 3M ┬╖ 12M` (dß╗» liß╗çu tuß║ºn tß╗▒, canvas-ink); highlight plan gradient+shadow-lg+scale ΓåÆ border+tint success (pattern quests__card--ready); bß╗Å shadow mß╗ìi card (`.card` override); bß║úng so s├ính: emoji Γ£öΓ£ÿΓ¥ñ ΓåÆ lucide Check/X (success/quaternary), th hß║┐t uppercase/tracking d╞░╞íng ΓåÆ ┬º4.6, mobile ΓåÆ card-stack (thead ß║⌐n + data-label); gi├í/countdown/account/amount/CK ΓåÆ mono; success Γ£ö emoji ΓåÆ BlockToken resolved (PREMIUM + t├¬n g├│i) + enter 300ms chuß║⌐n (khoß║únh khß║»c ─æß║ºu t╞░ duy nhß║Ñt) + confetti; bß╗Å emoji ≡ƒÄë i18n; price/cß╗Öt premium/countdown bß╗Å accent primary trang tr├¡; H1 48px; weight 800/700 ΓåÆ 600; clear redirectTimer khi unmount; n├║t copy sm ΓåÆ md; QR frame trß║»ng giß╗» (functional ΓÇö decision log). |

| SubscriptionView `/account/subscription` | 72/100 | 93.5/100 | 2/10 | 8/10 | Hero gradient + blob + shadow + title gradient ΓåÆ surface band level-2; days-left chip primary tint + padding 14px ΓåÆ hero-stat BlockToken resolved "C├ÆN Lß║áI n ng├áy" (1 hero-stat/m├án, LU├öN tß╗æi) + enter 300ms chuß║⌐n (khoß║únh khß║»c ─æß║ºu t╞░); error state Mß╗ÜI (gß╗ìi API trß╗▒c tiß║┐p ΓÇö store nuß╗æt lß╗ùi) + EmptyState retry "Thß╗¡ lß║íi"; bß╗Å emoji Γ¥ñ (benefits/loses ΓåÆ i18n); ng├áy hß║┐t hß║ín ΓåÆ mono tabular; weight 700/800 ΓåÆ 600; H1 48px; gap 6px/14px lß║╗ ΓåÆ token; bß╗Å shadow card. |



## Ghi ch├║ chung

- **Fix app-wide** (xem `docs/pm-decision-log-viewquality.md`): X├ôA block reset `*{margin/padding:0}` khß╗Åi global.css (unlayered ─æ├¿ mß╗ìi padding utility Tailwind ΓåÆ button shadcn padding 0px; Tailwind preflight trong `@layer base` thay thß║┐ ΓÇö verify sau fix: button padding 12px); Tabs.vue `min-h-9` (tab 22px ΓåÆ 38px).

- Component chung mß╗¢i: `frontend/src/components/ui/BlockToken.vue`.

- Ngoß║íi lß╗ç duy nhß║Ñt "0 hex rß╗¥i" (decision log 14/08): QR frame `bg-white` ΓÇö chuß║⌐n QR ISO/IEC 18004 y├¬u cß║ºu nß╗ün s├íng ─æß╗â scan (qrcode lib vß║╜ module ─æen tr├¬n nß╗ün trong suß╗æt).

- Verify thß╗▒c tß║┐ (Playwright + route-mock /api/v1 v├¼ BE :5000 trß║ú 404 cho route thß║¡t): 2 view light+dark+390px ΓÇö overflowX = 0, console error = 0 (trß╗½ 1 lß╗ùi 500 mock c├│ chß╗º ─æ├¡ch khi test error state), button ΓëÑ40px, block-token nß╗ün `rgb(13,16,32)` = canvas-ink ß╗ƒ mß╗ìi theme, hero `bgImage: none`, card `box-shadow: none`, H1 48px/-0.03em, table mobile thead hidden + ::before data-label "Free/Premium", success flow: QR 208px ΓåÆ countdown ΓåÆ confirm ΓåÆ BlockToken resolved enter 300ms `cubic-bezier(0.16,1,0.3,1)` ΓåÆ redirect home.

- Cß║únh b├ío console duy nhß║Ñt c├▓n lß║íi (component chung, c├│ ß╗ƒ mß╗ìi Modal): `[WARNING] Missing Description or aria-describedby for DialogContent` ΓÇö shadcn-vue Dialog nß╗Öi bß╗Ö; ─æß╗ü xuß║Ñt Phase 2 th├¬m `aria-describedby`/description v├áo Modal.vue.

- N├║t md 40px giß╗» nguy├¬n tr├¬n mobile (─æß╗ông nhß║Ñt nh├│m C ─æ├ú chß╗æt ΓÇö ┬º8 khuyß║┐n nghß╗ï 44px cho CTA ch├¡nh; ghi notes, Phase 2 c├ón nhß║»c Button auto-upgrade lg theo breakpoint).

- Build: `npm run build` PASS (vue-tsc + vite). Unit test: `npm run test` ΓÇö 12 files / 95 tests PASS.

- Ollama 3 gate ├ù 6 view (qwen2.5vl:3b): kh├┤ng lß╗ùi spacing (gate2); bß║ún sß║»c nhß║¡n diß╗çn "DSA Visual / data structure learning" + block tß╗æi (gate3) ΓÇö chi tiß║┐t `docs/work/view-quality/ollama-log/<view>.md`. Gate1 2 view mß╗¢i: Premium 7.5/10, Subscription 8.5/10 (nhß║¡n x├⌐t "card shadow"/"font lß╗¢n h╞ín dark" l├á hallucination model 3b ΓÇö computed style khß║│ng ─æß╗ïnh shadow none, font giß╗æng hß╗çt).



<!-- MERGED-BLOCK: nhom khac -->

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

---

## Đợt 3 — 3 view bị sót (Phase 2 bổ sung, 14/08/2026 · dev-frontend · nhánh feature/view-quality-merge-check)

## LadderView.vue (`/ladder/:nodeId`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 76.5/100 | **94.5/100** |
| Đặc trưng | 3/10 | **8.5/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- **Emoji 🪜 vỡ glyph/font** (bằng chứng r2-fixed-07, L85) → **lucide `ListOrdered`** 20px trong ô vuông muted 44px, màu tertiary (không còn phụ thuộc font emoji của user).
- Banner gradient sunset + blob blur + `shadow-md` (L127-161) → **surface band level-2** (card-raised + border-subtle, không shadow) + **kicker mono** `PRACTICE LADDER · NODE 0X` + **strip block-token tối** (3 bậc Quiz/Lab/Code + index mono `01 · 20%` … — trọng số thật, quyết định xuyên-nhóm #1/#4).
- H1 `--text-3xl` gradient-clip → `--text-4xl` 600 `-0.03em`; Badge primary → muted; `gap: 6px` → `--space-xs`; `←/→` ký tự → lucide ArrowLeft/ArrowRight 16px.
- Motion enter chrome 280ms `cubic-bezier(0.16,1,0.3,1)` (khoảnh khắc đầu tư duy nhất); media 640: chrome padding md, actions xếp dọc.
- i18n: thêm section `practiceLadder` (view trước hardcode tiếng Việt trong template).

## NodeHubView.vue (`/path/:topicId/node/:nodeId`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 78/100 | **94/100** |
| Đặc trưng | 4/10 | **8/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- Banner gradient sunset + blob + **dark-overlay hack** (L247-253) → surface band level-2 + **kicker mono dữ liệu thật** `NODE 04 · SORT.BUBBLE` (route + simKey).
- Easing: Motion `easeOut` → `[0.16,1,0.3,1]` 280ms; panel transition 180ms `ease` → enter 200ms `cubic-bezier(0.16,1,0.3,1)` / exit 150ms `cubic-bezier(0.7,0,0.84,0)`.
- Icon gradient + shadow-md → ô muted 44px + lucide 20px tertiary; H1 clamp/#fff/text-shadow → `--text-4xl` 600; Badge primary → muted; CTA `size="sm"` (36px) → md (40px) + Play 16px; `←` → lucide ArrowLeft.
- Fallback-title `--text-md` → `--text-xl` 600; `gap: 2px` → `--space-xs`; i18n bỏ 📖/▶.

## FinalTestView.vue (`/path/:topicId/final-test`)

| Chỉ số | Trước | Sau |
|---|---|---|
| Hygiene | 78.5/100 | **94/100** |
| Đặc trưng | 4/10 | **8/10** |
| Kết luận | KHÔNG ĐẠT | **ĐẠT** |

Lỗi chính đã sửa:
- Banner gradient aurora + blob + overlay hack → surface band level-2 + **kicker mono** `FINAL TEST · PASS ≥ 70%` (threshold thật).
- **Rules strip KILL-LIST** (3 card `shadow-sm` + hover `shadow-md` + lift, L281-289) → level-1 (surface + border, không shadow), hover chỉ đổi border → border-strong, transition border-color 150ms chuẩn; giá trị số mono + weight 600 (bỏ 700); label 400 tertiary.
- Icon gradient → ô muted 32px tertiary; H1 chuẩn; Badge primary → muted; `font-weight: 700` (L313) → 600.
- i18n: bỏ `←` khỏi backToMap, bỏ 🏅 khỏi toastPassed (giữ nguyên nội dung).

## Kiểm tra chung
- `<button` raw: **0/3 view** (mọi nút qua Button.vue h-40px px-16px — không cần decision log).
- Hex rời / hardcode spacing: 0 (đã bỏ gap 6px/2px, #fff, text-shadow, gradient).
- Build: `npm run build` = **PASS** (bao gồm vue-tsc -b).
- Visual: dev :5180, DOM assertions + computed style light/dark + 3 mốc breakpoint (1536/768/390), console error = 0/3 view. Ollama gate không chạy được (model không đọc được ảnh) — ghi thật tại `ollama-log/LadderView.md` + `NodeHubView.md` + `FinalTestView.md`.
- Ghi chú ngoài phạm vi: LessonDetail.vue (component chung) còn emoji nội dung 📝🎯👉📚 — đã ghi notes.md.

---

