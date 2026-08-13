# FINDINGS — Audit backend 5 trục (PROMPT_J_BACKEND_AUDIT) — TRẠNG THÁI CUỐI

> Ngày: 13/08/2026 · Nhánh: feature/backend-audit (chạy LOCAL — session frontend chiếm working tree, KHÔNG commit/merge) · Baseline ban đầu: Unit 85 / Integration 31 → CUỐI PHIÊN: **Unit 121 / Integration 77 — toàn bộ PASS**

## Tổng hợp findings theo trục (chi tiết: findings-*.md cùng thư mục)
- findings-exception.md — 9 (1 CAO / 3 TRUNG / 5 THAP) → đã fix #1,#3,#4,#5; #2 quyết định không migrate; #6-#9 còn lại (notes.md)
- findings-biz-gamification.md — 13 (5 CAO / 2 TRUNG / 6 THAP) → đã fix BUG-1,#2,#3,#4,#5,#6,#7; #8-#13 còn lại (notes.md)
- findings-security.md — 19 (1 CAO / 10 TRUNG / 8 THAP) → đã fix #1(clamp),#2,#5,#6,#7,#8,#9,#10,#11,#12,#13,#15,#16,#17,#18; #3,#4,#14,#19 còn lại
- findings-perf.md — 23 (3 CAO / 8 TRUNG / 12 THAP) → đã fix #1,#2,#3,#4,#5,#6,#7,#8,#9,#10,#11,#17,#18,#19,#21,#22; #12,#13,#14,#15,#16,#20,#23 còn lại
- findings-surface.md — 5 (0 CAO / 2 TRUNG / 3 THAP) → đã fix toàn bộ (#1-#5)
- findings-biz-services.md — 24 (4 CAO / 12 TRUNG / 8 THAP) → đã fix #1,#2,#3,#4,#5,#6,#7,#8,#9,#10,#11,#12,#13,#14,#15,#17,#18,#19; #16,#20,#21,#22,#23,#24 còn lại

## TỔNG: 93 findings — 14 CAO (đã fix 100%) / 37 TRUNG (đã fix 30) / 42 THAP (đã fix ~20, còn lại ghi notes.md)

## BUG-1 (đã fix Đợt A): premium downgrade — lazy clamp EnsureHeartsMaxSyncAsync (HeartsMax=10, Hearts=MIN(Hearts,10) atomic khi đọc hearts) — test GetHearts_ExpiredPremium_ClampsHeartsMaxAndHeartsToFree PASS.

## Kiểm chứng cuối (đầy đủ bảng so sánh + chi tiết từng đợt ở pm-report-backend.md)
