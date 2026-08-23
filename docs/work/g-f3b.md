# G-F3B - VERIFY DOC LAP TOAN DOT G (dev-test / Phase 3b)

> Ngay: 13/08/2026 - Nhanh: `dev` @ `6bba176` (da merge G-BF1/G-BF2/G-BF3/G-F1/G-F2) - Nguoi: dev-test (doc lap)
> Pham vi: 9 hang muc - build FE, unit FE, e2e, build BE, unit BE, integration BE, grep cam, smoke UI light+dark, secret scan.
> Quyet dinh: docs/pm-decision-log-g.md - Nen dot D/E/F: FE unit 72, BE unit 44, BE integration 27, e2e 11.

## KET QUA TONG HOP

| # | Hang muc | Yeu cau | Ket qua | Bang chung |
|---|---|---|---|---|
| 1 | `npm run build` (frontend/) | 0 loi | **PASS** | `vue-tsc -b && vite build` EXIT=0, 3358 modules, 0 error |
| 2 | `npm test` (frontend/) | 72/72 PASS | **PASS** | 8 test files, **72 passed** (0 failed) |
| 3 | `npx playwright test` (frontend/) | 11/11 PASS | **PASS** | 11 passed, 0 failed (22.4s) |
| 4 | `dotnet build DsaVisual.sln` (backend/) | 0 warning / 0 error | **PASS** | Build succeeded, **0 Warning(s) 0 Error(s)** |
| 5 | `dotnet test` UnitTests | 56/56 PASS | **PASS** | Passed 56, Failed 0 (44 cu + 12 moi dot G) |
| 6 | `dotnet test` IntegrationTests | 31/31 PASS | **PASS** | Testcontainers MsSql chay docker 29.4.2, Passed 31, Failed 0 |
| 7 | Grep cam backend/src + frontend/src | khong vi pham | **PASS** | 0 match PostgreSQL/Npgsql/MediatR/Judge0; "Repository" chi 3 comment khang dinh KHONG dung; secret dev co comment ro |
| 8 | Smoke UI light+dark 3 man | 0 console error + layout khong vo | **PASS** | 8/8 man (4 trang x light/dark) overflow=false, 0 console error; 8 anh chup |
| 9 | Secret scan (git grep) | khong secret that trong commit | **PASS** | Khong DSA__Jwt__Secret that / connection string that / API key that (sk-/ghp_/AKIA/AIza: 0 match) |

**TONG: 9/9 PASS** - dot G merge tren `dev` DAT.

---

## 1. `npm run build` (frontend/) - PASS

```
> dsa-visual-frontend@0.1.0 build
> vue-tsc -b && vite build
vite v8.2.1 building client environment for production...
transforming... 3358 modules transformed.
dist/assets/engine-BrgXnv-N.js   476.03 kB | gzip: 120.02 kB
dist/assets/echarts-CE_jCTv4.js  323.63 kB | gzip: 110.46 kB
built in 1.71s      (EXIT=0)
```

- `vue-tsc -b` chay sach (0 loi type), `vite build` 0 loi.
- Bundle goc `index-*.js` 105.73 kB; engine 476 kB (so that sau build - NFR-5, xem ghi chu).

## 2. `npm test` (frontend/) - PASS 72/72

```
 Test Files  8 passed (8)
      Tests  72 passed (72)
```

- Files: coreAnimationEngine 6, compileWorker 7, seedData 7, classes 6, stepExecutor 16, auth 5, renderers 15, catalog 10 = **72**.
- Khop nen dot D/E/F (72) - dot G khong them/bot unit FE.

## 3. `npx playwright test` (frontend/) - PASS 11/11

```
Running 11 tests using 1 worker
auth.spec.ts 3 | code-runner.spec.ts 3 | ladder.spec.ts 2 | simulator.spec.ts 3
11 passed (22.4s)
```

- Ca 11 test (route-mock /api/v1) PASS - G-BF3 (mock refresh 401 khi chua login) hoat dong dung.

## 4. `dotnet build DsaVisual.sln` (backend/) - PASS 0W/0E

```
DsaVisual.Application, DsaVisual.Api, DsaVisual.UnitTests, DsaVisual.Api.Tests, DsaVisual.IntegrationTests
Build succeeded.  0 Warning(s)  0 Error(s)
```

- 5 project build sach, net10.0, EXIT=0.

## 5. `dotnet test` UnitTests - PASS 56/56

```
dotnet test tests\DsaVisual.UnitTests --no-build
Passed! - Failed: 0, Passed: 56, Skipped: 0, Total: 56, Duration: 1 s
```

- 56 = 44 cu (dot D/E/F) + 12 moi dot G (G-BF1: mark-viewed 4, heart regen 4, duplicate Q 1, submit lock+status 3). Khop g-bf1.md.

## 6. `dotnet test` IntegrationTests - PASS 31/31 (Testcontainers THAT)

```
dotnet test tests\DsaVisual.IntegrationTests --no-build
Passed! - Failed: 0, Passed: 31, Skipped: 0, Total: 31, Duration: 5 s
```

- Docker 29.4.2 dang chay -> container MsSql Testcontainers khoi tao thanh cong, **KHONG SKIP**.
- 31 = 27 cu + 4 moi dot G (LessonsIntegrationTests mark-viewed 3, AuthIntegrationTests cookie Secure 1).

## 7. Grep cam - PASS

| Pattern | backend/src | frontend/src | Ghi chu |
|---|---|---|---|
| `PostgreSQL` / `Npgsql` | 0 match | 0 match | - |
| `MediatR` | 0 match | 0 match | - |
| `Judge0` | 0 match | 0 match | - |
| `Repository` | 3 match | 0 match | **3 comment giai thich ro** (Program.cs:95, AppDbContext.cs:8, LessonService.cs:15): "KHONG Repository - SDD 5.1" -> duoc phep |
| Secret that (khoa API/DB/mat khau hardcode) | khong hardcode production | 0 match | Xem chi tiet duoi |

Chi tiet secret trong `backend/src`:
- `appsettings.json:24` - `Password=CHANGE_ME` -> placeholder ro rang (khong phai secret that).
- `appsettings.Development.json:4,20` - `dev-only-secret-...` + `DsaVisual@Dev123` -> **dev-only credentials**; Seed/README.md ghi ro "CHI DUNG LOCAL - khong phai production", "Bat buoc doi truoc khi deploy production"; Program.cs:33 comment "khong hardcode secret; appsettings chi chua placeholder dev" -> duoc phep (co comment giai thich ro).
- `SeedData.cs:55-57` - `DevPassword: "Admin@123"/"Teacher@123"/"Student@123"` -> password seed DEV, co comment `// ADMIN / TEACHER / STUDENT` + README bang "Mat khau seed DEV (CHI DUNG LOCAL)" -> duoc phep (seed dev).
- `docker-compose.yml` - `${MSSQL_SA_PASSWORD:-DsaVisual@Dev123}` + `${DSA__Jwt__Secret:-change-me-dev-secret-...}` -> env placeholder co default dev, comment "placeholder dev - doi khi deploy" -> duoc phep.

**Ket luan item 7: PASS** - khong vi pham; moi dev credential deu co comment/ghi chu ro rang.

## 8. Smoke UI light+dark - PASS

Cach chay: dev server vite port **5173** (moi, serve code dev hien tai) + Playwright chromium 1440x900. Dung **route-mock /api/v1** (helpers e2e) - ly do: docker backend dang chay la **STALE** (image build truoc dot G, `POST /lessons/1/mark-viewed` -> 404 du code da co) -> smoke qua mock de verify UI render dung, tranh console error gia tu backend cu.

Man (4 trang x light/dark = 8 phien):
- `/` (home) - light + `class="dark"`
- `/login` (login) - light + dark
- `/learn/1` (lesson) - light + dark (da login qua UI mock -> session giu cung phien)
- `/path` (learning path) - light + dark

Ket qua bang chung (script capture network + console):
```
ALL 8 screens: overflow=false, 0 console error
```

- **Overflow ngang**: `scrollWidth == clientWidth == 1440` o ca 8 phien -> khong vo layout.
- **Console**: 0 console error / 0 pageerror (sau khi mock du `/auth/me`, `/me/hearts`, `/lessons/1`, `/topics`, `/progress/me`...).
- **Component render du**: `bodyChildren >= 2`; da assert selector chinh (`.home`, `#email`, `main`) hien huu.
- **Dark mode that su doi mau** (computed style): body `rgb(240, 253, 250)` (light teal) -> `rgb(4, 47, 46)` (dark OKLCH teal) khi them `class="dark"` -> token OKLCH dark hoat dong.

Anh chup (docs/work):
| Anh | Man | Theme |
|---|---|---|
| `g-f3b-smoke-01-home.png` / `-dark.png` | Home `/` | light / dark |
| `g-f3b-smoke-02-login.png` / `-dark.png` | Login `/login` | light / dark |
| `g-f3b-smoke-03-lesson.png` / `-dark.png` | Lesson `/learn/1` | light / dark |
| `g-f3b-smoke-13-path.png` / `-dark.png` | Path `/path` | light / dark |

## 9. Secret scan (git grep) - PASS

| Kiem tra | Ket qua |
|---|---|
| `DSA__Jwt__Secret` that trong commit | Khong - chi placeholder: `.env.example` (`change-me-32-characters-minimum-secret`), docker-compose (`${DSA__Jwt__Secret:-change-me-dev-secret-...}`), Seed/README (`dev-secret-...`), Program.cs (doc tu config + throw neu thieu) |
| Connection string that | Khong - `appsettings.json` dung `CHANGE_ME`; dev placeholder `DsaVisual@Dev123` chi trong Development.json/Seed README/docker-compose (co ghi chu dev-only) |
| API key that (`sk-`, `ghp_`, `AKIA`, `AIza`) | **0 match** toan repo tracked |
| File `.env` that | Khong commit - chi `.env.example` tracked; `frontend/.env.development`/`.env.production` bi gitignore (chi VITE_API_BASE_URL, khong secret) |
| Supabase/Cloudinary | 0 match ngoai comment docker-compose "KHONG Postgres/Redis/Judge0/Cloudinary" |

**Ket luan item 9: PASS** - khong co secret that trong code commit.

---

## Ghi chu / follow-up

- **Backend docker STALE**: `neww-backend-1` (port 5000) dang chay image build truoc dot G - endpoint moi G-BF1 (`POST /lessons/{id}/mark-viewed`) chua co trong container (404). Neu can demo backend moi: `docker compose up -d --build` (hoac `dotnet run` voi env `DSA__Jwt__Secret` + `ConnectionStrings__Default`). Khong anh huong code/test tren nhanh dev.
- **Grep cam "Repository"**: 3 match deu la comment khang dinh KHONG dung Repository pattern (SDD 5.1 / quyet dinh A-1) - duoc phep, khong phai vi pham.
- **Dev credentials commit**: `appsettings.Development.json`, `Seed/README.md`, `SeedData.cs`, `docker-compose.yml` chua dev-only password (`DsaVisual@Dev123`, `Admin@123`...) - TAT CA co comment/ghi chu ro rang "dev/local - khong production". Neu hoi dong muon cung hinh hon (0 credential trong git), de xuat: chuyen JWT dev secret sang chi env (gitignore .env) hoac dung dotnet user-secrets - xem docs/SETUP_TODO.md.
- **Smoke spec tam**: `tests/smoke/g-f3b-smoke.spec.ts` + `playwright.smoke.config.ts` da xoa sau khi chup (khong luu trong repo - chi luu anh docs/work/g-f3b-smoke-*.png).
- **docs/pm-report-g.md**: chua co (de xuat dev-docs/phuc tao) - g-f3b nay la bang chung doc lap cho dot G.
- **Phan cong commit**: chi commit file bao cao nay (docs/work/g-f3b.md) bang `commit-as.ps1 phuc` theo yeu cau.
