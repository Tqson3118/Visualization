# KẾ HOẠCH GIAI ĐOẠN 2 (PM MASTER PLAN v2)

> **Giai đoạn 1 đã xong** (xem `docs/pm-report.md`): 12/12 file bàn giao đạt chuẩn §17.2 + checklist §17.9 ✔ + PRODUCTION_PROMPT v2.6.
> **Còn lại 3 việc → 3 SESSION SONG SONG** (đủ, không cần thêm): A=Báo cáo Word, B=Bê code, C=Git+Push.

## SESSION A — BÁO CÁO WORD (task A6)
Log: `docs/pm-decision-log-a.md` + `docs/pm-report-a.md`
- Nguồn: `docs/BAO_CAO_SPEC.md` + `docs/DIAGRAM_PROMPTS.md` + 12 file docs + `tailieu/NET202_Project document_6 (1).pdf` (S-Clinic — chuẩn độ sâu)
- Việc: sinh `tailieu/BAO_CAO.md` (khuôn trường PHẦN 1-7, bản đầy đủ ~50-70 trang, 4 thành viên thật, mốc 12/05→11/08/2026, KHÔNG "20 tuần") → sinh ảnh placeholder 1920×1080 có nhãn vào `tailieu/placeholders/` → `tailieu/diagram-prompts.md` (6 prompt NHÓM B cho ChatGPT) → build `tailieu/BaoCaoDoAn.docx` bằng pandoc (+`--toc`, reference-doc nếu có) → verify docx mở được, mục lục đủ
- CẤM: học thuật hóa (§5.0A), placeholder rỗng, số liệu bịa, "20 tuần/16 tuần"

## SESSION B — BÊ CODE TỪ 3 SOURCE (task B1)
Log: `docs/pm-decision-log-b.md` + `docs/pm-report-b.md`
- Nguồn code v1 (CHƯA BÊ, cần quyết định): `source/VisualizationDSA1` (**ưu tiên — mới nhất + design-system + .env.example**), `source/VisualizationDSA3` (nội dung curriculum DSA + test guides), `source/VisualizationDSA` (bản gốc làm nền)
- Việc: scan cả 3 → `docs/REUSE_REPORT.md` (bảng: file | chức năng | bê? | lý do | nơi đích) → copy có chọn lọc vào `frontend/` + `backend/` (skeleton v2 theo SDD) → build thử nếu môi trường cho phép
- QUY TẮC BÊ: ✔ design-system, component UI, canvas renderers (chỉnh theo EDV), thuật toán (chỉnh StepExecutor), seed/nội dung học (từ V3), docker/nginx pattern, phím tắt/control bar. ✘ PostgreSQL-specific, Repository pattern, feature đã cắt (embed widget, SOLID sandbox, side-by-side), secret (chỉ .env.example)
- Code KHÔNG phải là kiến trúc v2 (docs ghi rõ "đặc tả dự kiến") → REUSE_REPORT ghi chú mọi điểm cần chỉnh

## SESSION C — GIT + QUY TRÌNH PUSH (mới)
Log: `docs/pm-decision-log-c.md` + `docs/pm-report-c.md`
- Việc:
  1. `git init` tại root `D:\FPT\neww` + `.gitignore` chuẩn (node_modules, .env, bin/obj, dist, .playwright-mcp, secrets) — **KHÔNG commit .env/secret**
  2. Nhánh: `main` (đầu tiên — tài liệu ổn định) → `dev` (tích hợp) → feature sau
  3. Commit hiện trạng: nhóm theo module, mỗi nhóm 1 commit với **tên thành viên khớp phân công docs** (dùng `.\commit-as.ps1`): backend→Mai Tiểu Bảo, frontend→Thái Quang Sơn, engine/visual→Huỳnh Lê Minh Thư, tài liệu/test→Trần Viết Tâm Phúc; nhóm nhỏ còn lại dùng tên người phù hợp nhất
  4. Remote: `origin https://github.com/Tqson3118/Visualization.git` → push `main` + `dev`
  5. Ghi rõ vào `docs/SETUP_TODO.md`: cần điền **email GitHub thật** vào `commit-as.ps1` (hiện placeholder — commit vẫn hiện tên nhưng không avatar)
- QUY TRÌNH VỀ SAU (cho mọi task code — quy tắc cố định):
  1. Branch: `feature/<tên-task-TO>` (task TO = 1 feature hoàn chỉnh: VD `feature/auth-jwt`, `feature/simulation-engine` — KHÔNG tách nhỏ quá)
  2. Làm xong → PM review code trước khi merge (nạp skill `requesting-code-review`; lỗi → sửa lại tối đa 2 lần)
  3. Tạo PR `feature/x → dev` (dùng `gh pr create` nếu có gh+auth; không thì in URL để người dùng bấm) — mô tả PR: task, file, verify
  4. Merge vào `dev` → `dev` ổn định mới merge `main` (điểm phát hành)
  5. Commit message: Conventional Commits `feat:/fix:/docs:/test:/refactor:`
- ⚠ Session C KHÔNG tạo PR — chỉ init + push hiện trạng; PR áp dụng cho feature code sau (đêm sau)

## Prompt khởi động (3 session — mở 3 cửa sổ opencode riêng)

### SESSION A — Báo cáo Word
```
/pm "Chạy SESSION A trong docs/PM_MASTER_PLAN.md: tạo báo cáo Word theo
docs/BAO_CAO_SPEC.md + docs/DIAGRAM_PROMPTS.md. ĐỌC TRƯỚC tailieu/NET202_Project document_6 (1).pdf
(chuẩn độ sâu S-Clinic §5.0A). TIẾT KIỆM CONTEXT: đọc file docs THEO TỪNG PHẦN đang viết
(không nạp toàn bộ 1 lúc) — mỗi chương chỉ mở nguồn của chương đó. Sinh tailieu/BAO_CAO.md
(khuôn trường PHẦN 1-7, đầy đủ, mốc thời gian 12/05→11/08/2026, 4 thành viên thật, CẤM '20 tuần'),
tạo ảnh placeholder 1920x1080 có nhãn trong tailieu/placeholders/, ghi tailieu/diagram-prompts.md
(6 prompt), build tailieu/BaoCaoDoAn.docx bằng pandoc, verify file mở được. Ghi quyết định vào
docs/pm-decision-log-a.md, trạng thái vào docs/pm-report-a.md." --auto
```

### SESSION B — Bê code từ 3 source
```
/pm "Chạy SESSION B trong docs/PM_MASTER_PLAN.md: bê code từ 3 source —
source/VisualizationDSA1 (ưu tiên), source/VisualizationDSA3 (curriculum), source/VisualizationDSA
(nền). QUY TẮC THEO DOCS (bắt buộc): ĐỌC TRƯỚC docs/SDD.md §11.1 + §12.1 (cấu trúc solution
backend 2 project + cây thư mục frontend) + NFR-17 (cấm Repository, DbContext trực tiếp) —
skeleton frontend/ backend/ PHẢI khớp cây thư mục SDD; cấm bê nguyên trạng Domain/Infrastructure/
Repository/PostgreSQL/secret; code cũ chỉ là tham khảo, mọi file bê sang đều điều chỉnh cho khớp
kiến trúc mới (EDV thay AST, SQL Server). Scan cả 3, ghi docs/REUSE_REPORT.md (bảng bê/không bê +
lý do + nơi đích + KHỚP MỤC NÀO TRONG SDD), copy có chọn lọc. QUY TẮC BÊ: design-system/
component/canvas renderer (chỉnh EDV)/thuật toán/seed/docker; CẤM PostgreSQL, Repository, feature
đã cắt, secret. CUỐI TASK: compliance check — so skeleton vs SDD, liệt kê LỆCH (nếu có) vào
pm-report-b.md; build thử nếu được, không thì ghi rõ. Ghi quyết định vào docs/pm-decision-log-b.md,
trạng thái vào docs/pm-report-b.md." --auto
```

### SESSION C — Git init + push
```
/pm "Chạy SESSION C trong docs/PM_MASTER_PLAN.md: git init tại root, tạo .gitignore chuẩn
(không commit .env/node_modules/secret), tạo nhánh main + dev, commit hiện trạng theo nhóm
file với tên thành viên khớp phân công docs (dùng commit-as.ps1: backend→Mai Tiểu Bảo,
frontend→Thái Quang Sơn, engine→Huỳnh Lê Minh Thư, docs/test→Trần Viết Tâm Phúc), add remote
https://github.com/Tqson3118/Visualization.git và push main + dev. Ghi mọi việc cần người dùng
(điền email thật commit-as.ps1...) vào docs/SETUP_TODO.md. Ghi quyết định vào
docs/pm-decision-log-c.md, trạng thái vào docs/pm-report-c.md." --auto
```
