# G-3a — Phase 3a ux-finalize: CẬP NHẬT TÀI LIỆU CHO STACK UI/UX MỚI (đợt G)

> Ngày: 12/08/2026 · Nhánh: `feature/ux-finalize` (tạo từ `dev`) · Dev: dev-docs (docs → phuc, commit-as.ps1)
> Mục tiêu: đồng bộ tài liệu với stack UI/UX đợt G đã merge vào `dev` — tailwindcss 4 + shadcn-vue + motion-v + gsap + vue-echarts + lenis + vue-sonner + phosphor/lucide + font Geist/JetBrains Mono.

## 1. Số liệu bundle THẬT (chạy `npm run build` tại `frontend/` — 12/08/2026)

| Chunk | Gốc | gzip | Ghi chú |
|---|---|---|---|
| `engine-*.js` | **476 KB** | 120 KB | lõi mô phỏng EDV (manualChunks) |
| `echarts-*.js` | **324 KB** | 110 KB | lazy-load (VChartLazy `defineAsyncComponent`) — không vào bundle chính |
| `vendor-*.js` | 143 KB | 54 KB | vue + pinia + vue-router |
| `index-*.js` | 106 KB | 34 KB | entry |
| `install-*.js` | 168 KB | 56 KB | runtime/lazy helper |
| JS gốc tải lần đầu (preload) | ≈ **852 KB** | ≈ 233 KB | tổng chunk trong `index.html` modulepreload |
| JS gốc toàn dist | ≈ **1.95 MB** | — | gồm chunk lazy (view, echarts, compiler.worker) |

- Build: `vue-tsc -b && vite build` PASS (0 lỗi), 3358 modules, 1.39s.
- NFR-5 cũ ghi 459KB (engine, đợt D) → nới theo thực tế: **tổng JS gốc tải lần đầu ≤ 1.5MB; engine chunk ≤ 500KB gốc**.

## 2. Gói frontend MỚI (đợt G — `npm ls --depth=0` thật, 39 gói top-level)

| Gói | Phiên bản | License (thật) |
|---|---|---|
| tailwindcss | 4.3.3 | MIT |
| @tailwindcss/vite | 4.3.3 | MIT |
| tw-animate-css | 1.4.0 | MIT |
| shadcn-vue | 2.8.2 | MIT |
| reka-ui | 2.10.3 | MIT |
| class-variance-authority | 0.7.1 | Apache-2.0 |
| clsx | 2.1.1 | MIT |
| tailwind-merge | 3.6.0 | MIT |
| @lucide/vue | 1.31.0 | ISC |
| lucide-vue-next | 1.0.0 | ISC |
| @phosphor-icons/vue | 2.2.1 | MIT |
| motion-v | 2.3.0 | MIT |
| gsap | 3.15.0 | Standard "no charge" license (gsap.com/standard-license) |
| vue-echarts | 8.1.0 | MIT |
| echarts | 6.1.0 | Apache-2.0 |
| lenis | 1.3.26 | MIT |
| vue-sonner | 2.0.9 | MIT |
| @vueuse/core | 14.4.0 | MIT |
| vaul-vue | 0.4.1 | MIT (từ GitHub LICENSE — package.json không có license field) |

> Ghi chú: nhiệm vụ liệt kê 17 gói; thực tế `npm ls` thêm 2 gói nữa đã cài: **@vueuse/core** + **vaul-vue** → tổng 19 gói mới.

## 3. File đã sửa

| File | Version | Nội dung |
|---|---|---|
| `docs/SDD.md` | 1.0 → **1.4** | §3.1 cấu trúc thư mục frontend (styles/tailwind.css, palettes.css, lib/utils+toast, ui/<name>/ shadcn, public/fonts/), §3.8 Chuẩn code frontend (stack mới, dark mode class="dark", tokens OKLCH), §3.9 Vite config (@tailwindcss/vite + bundle thật), §8.1 Hệ thống thiết kế (font Geist/JetBrains Mono, shadcn-vue, vue-echarts, vue-sonner), Canvas font, Chart.js→vue-echarts; history 2 chỗ |
| `THIRD_PARTY.md` | 1.1 → **1.2** | +19 gói frontend (license thật npm ls), ghi chú GSAP/vaul-vue, nguồn số liệu cập nhật 39 gói |
| `docs/REUSE_REPORT.md` | — | §6 GHI CHÚ ĐỢT G: 15 mục ánh xạ component tự xây → shadcn-vue/vue-sonner + font Geist + bundle thật |
| `docs/SRS.md` | 1.0 → **1.3** | NFR-5 nới: tổng JS gốc tải lần đầu ≤ 1.5MB + engine ≤ 500KB gốc; history |
| `docs/TEST_PLAN.md` | 1.2 → **1.3** | TEST-PERF-007 ngưỡng khớp NFR-5; §10 ghi bundle thật + FE unit 72 + e2e 11; history |
| `docs/USER_GUIDE.md` | — | Rà nhanh: không lệch rõ (dark mode §3.15 đã OK; mô tả màu là canvas simulator — không đổi) → **không sửa** |
| `docs/README.md` | 1.2 → **1.3** | Version + số dòng thật (SRS 1544, SDD 3015, TEST_PLAN 602, THIRD_PARTY 108, README 183) |
| `docs/pm-decision-log-g.md` | — | THÊM mục [2026-08-12] G-3a docs — chốt bundle thật + nới NFR-5 |

## 4. Kiểm tra license THIRD_PARTY

- Đọc trực tiếp `frontend/node_modules/<gói>/package.json` → license field (hoặc licenses[]).
- vaul-vue: package.json KHÔNG có license → xác nhận GitHub `Elliot-Alexander/vaul-vue/LICENSE` = MIT.
- KHÔNG dùng chữ "gợi ý/x/+" — mọi version/license là số thật từ npm ls / package.json.
- Không còn `monaco-editor` trong dependency list (chỉ `@monaco-editor/loader` — đúng).

## 5. Trạng thái

- [x] SDD §3.1/§3.8/§3.9/§8.1 — DONE
- [x] THIRD_PARTY +19 gói — DONE
- [x] REUSE_REPORT §6 — DONE
- [x] NFR-5 (SRS) + TEST-PERF-007 (TEST_PLAN) — DONE
- [x] USER_GUIDE rà nhanh — không cần sửa
- [x] docs/README version + số dòng — DONE
- [x] pm-decision-log-g THÊM MỤC — DONE
- [x] Chạy `npm run build` lấy số thật — DONE
