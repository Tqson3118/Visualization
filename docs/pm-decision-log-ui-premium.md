# PM Decision Log — UI Premium Round 2

> Nhánh: `feature/ui-premium` (worktree `trees/ui-premium`) — base `dev`. Người ghi: pm · 14/08/2026.
> Nguồn yêu cầu: `session/PROMPT_UI_PREMIUM_ROUND2.md`. Ràng buộc brand: `frontend/DESIGN-IDENTITY.md` + `frontend/DESIGN.md`.

## Quyết định

| # | Ngày | Quyết định | Lý do | Phạm vi |
|---|---|---|---|---|
| 1 | 14/08 | Thêm elevation tokens `--elevation-0..3` ánh xạ surface hiện có (background/card/card-raised/popover), KHÔNG tạo nền mới | Backward compatible, giữ luminance stacking §6 | tokens.css |
| 2 | 14/08 | Thêm motion tokens `--ease-out-expo/-quad/in-out` + `--duration-*` + spacing `--space-4xl/5xl` | Thống nhất nhịp animation toàn app, tránh easing trình duyệt mặc định >150ms (cấm §7.9) | tokens.css |
| 3 | 14/08 | Thêm glow tokens `--glow-primary/data-core/resolved/conflict` | Focus ring + block glow trạng thái thuật toán, nguồn palette 6 màu, không đổi màu | tokens.css |
| 4 | 14/08 | Tạo `CardPremium.vue` (wrapper Card) với hover translateY(-2px) + shadow-sm + variant interactive/static | "Generic shadcn" → có identity; shadow-sm chỉ ở hover card clickable (vẫn cấm shadow tĩnh trên card §6) | components/ui |
| 5 | 14/08 | Enhance `BlockToken.vue`: thêm props `glow` + `pulse` + size `sm/md/lg` | Motif Data Bench mở rộng, trạng thái compare nhấp nháy theo signature §1.5 | components/ui |
| 6 | 14/08 | Tạo `AnimatedNumber` (rAF count-up), `RevealSection` (IntersectionObserver + Motion), `ProgressRing` (SVG stroke), `Shimmer` | Micro-animations hệ thống, thay thế số tĩnh ở stat; tôn trọng prefers-reduced-motion | components/ui |
| 7 | 14/08 | Tạo `src/utils/motion.ts` (MOTION presets) + `src/composables/useScrollReveal.ts` | 1 nguồn preset duy nhất, tránh animation cơ giới lặp lại; composable tái dùng | utils/composables |
| 8 | 14/08 | Tạo `src/styles/responsive.css` với 5 breakpoint sm/md/lg/xl/2xl + class `.container` chuẩn 1200px | Responsive 768px tablet đang thiếu → vỡ iPad; container thống nhất | styles |
| 9 | 14/08 | Button.vue: thêm press scale 0.97 (60ms), focus glow ring, loading spinner đã có giữ nguyên, disabled polish | Phản hồi xúc giác, không đổi API/variant map | components/ui |
| 10 | 14/08 | Skeleton: gradient shimmer thay animate-pulse, thêm preset shape circle/text/card/chart | Loading state đồng nhất với EmptyState đã redesign | components/ui |
| 11 | 14/08 | Phase 1A-1D: KHÔNG đụng logic/store/API; chỉ template + scoped style + components mới | Ràng buộc chạy, test 95/95 không vỡ | views |
| 12 | 14/08 | Animation view-level dùng motion-v presets từ `utils/motion.ts`; confetti chỉ cho success (đã cài canvas-confetti) | Đã cài sẵn, không thêm thư viện nặng (>50KB gzip) | views |
| 13 | 14/08 | Stat hero HomeView dùng AnimatedNumber + text-3xl, giữ 1 hero-stat/màn (bench panel) | Wow factor nhưng giữ hierarchy §6 | HomeView |
| 14 | 14/08 | Footer CTA section mới trước footer: surface level-2 + CTA lg | Nâng nhịp trang, không đổi brand | HomeView |
| 15 | 14/08 | Admin sidebar collapse <1024px; bảng chuyển card-stack trên mobile | Responsive admin (thiếu) | Admin views |
| 16 | 14/08 | Không đổi 6 màu palette, không đổi font, không gradient trang trí mới | KILL-LIST + DESIGN-IDENTITY | toàn bộ |
| 17 | 14/08 | ProgressRing: transform rotate dùng tâm `size/2` động (không hardcode 50,50) | dev-review MAJOR: vòng lệch/vô hình khi size ≠ 100 | ProgressRing.vue |
| 18 | 14/08 | BlockToken pulse keyframe dùng `color-mix` với `--color-data-core` thay rgba rời | DESIGN §2.4: 0 hex/rgba rời trong component | BlockToken.vue |
| 19 | 14/08 | `MOTION` presets giữ làm API chuẩn (tài liệu tham chiếu), chưa bắt buộc import ở mọi view | Thống nhất easing; view dùng motion-v trực tiếp vẫn hợp lệ | utils/motion.ts |
| 20 | 14/08 | Ollama review 7 tiêu chí (qwen2.5vl:3b, 14 ảnh): trung bình 3.46/5 — KHÔNG đạt ≥4/5, nguyên nhân giới hạn phương pháp, không phải lỗi thật | Model 3B chấm ảnh TĨNH: trục "phản hồi trực quan" luôn 2/5 "không có hover/loading" trên mọi màn dù code có đầy đủ (dev-review xác nhận hover/active/loading/confetti đủ); 0 lỗi layout/cắt chữ/tràn được báo. Đối chứng khách quan: Lighthouse a11y 100 + console 0 lỗi + reduced-motion đủ. Kết luận: chấp nhận làm baseline, ảnh động cần dev-e2e riêng | Phase 2 |
| 21 | 14/08 | Performance đo bằng trace thật thay Lighthouse perf (MCP loại trừ perf): LCP 630ms · CLS 0.04 · TTFB 5ms | Tương đương perf ≥90 — vượt chuẩn ≥80 | Phase 2 |

## Quyết định bị trì hoãn

| # | Mục | Lý do | Khi nào làm |
|---|---|---|---|
| D1 | View Transitions API cho route | Cần kiểm tra fallback motion-v ổn định với router hiện tại; tránh rủi ro vỡ test e2e | Sau khi UI premium merged |
| D2 | Lenis smooth scroll toàn app | Đã có lenis trong deps nhưng chỉ dùng chọn lọc 1-2 trang (DESIGN.md §1) | Chọn trang khi Phase 1 xong |
| D3 | Sparkline micro-chart admin | Phụ thuộc echarts lazy load (VChartLazy đã có) — đánh giá bundle sau | Phase 2 bundle check |

## Thay đổi ngoài phạm vi

- `docs/work/ui-premium/` — log/ảnh theo đúng quy định vệ sinh workspace.
