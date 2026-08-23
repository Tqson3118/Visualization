# H-A — Docs: thay 12 ảnh UI thật vào BAO_CAO + build docx

> Ngày: 13/08/2026 · Nhánh: `feature/ux-h-docs` (từ dev, HEAD bd1aff0) · Người thực hiện: dev-docs
> Nguồn chuẩn: BAO_CAO_SPEC §6.2 (tên ảnh `screenshots/NN-ten-man.png`), ảnh FINAL `docs/work/final-*.png` (đợt H, 12/12 PASS — pm-decision-log-h).

## 1. 12 ảnh đã copy (docs/work → tailieu/screenshots/)

| # | Nguồn (docs/work/) | Đích (tailieu/screenshots/) | Bytes | Ghi chú |
|---|---|---|---|---|
| 1 | final-01-home.png | 01-home.png | 291.913 | đủ nguồn, không phải thay thế |
| 2 | final-02-login.png | 02-login.png | 187.909 | đủ nguồn (bản light; dark có final-02-login-dark.png không dùng) |
| 3 | final-04-lesson-detail.png | 04-lesson-detail.png | 246.258 | đủ nguồn |
| 4 | final-05-simulator.png | 05-simulator.png | 82.847 | đủ nguồn (bản light) |
| 5 | final-06-exercise.png | 06-exercise.png | 72.890 | đủ nguồn |
| 6 | final-13-learning-path.png | 13-learning-path.png | 73.526 | đủ nguồn |
| 7 | final-14-ladder.png | 14-ladder.png | 135.901 | đủ nguồn (chụp lại sau fix H-FINAL-1) |
| 8 | final-15-lab.png | 15-lab.png | 91.722 | đủ nguồn |
| 9 | final-16-code-runner.png | 16-code-runner.png | 118.734 | đủ nguồn |
| 10 | final-17-benchmark.png | 17-benchmark.png | 112.216 | đủ nguồn |
| 11 | final-24-leaderboard.png | 24-leaderboard.png | 134.760 | đủ nguồn (bản light) |
| 12 | final-32-profile.png | 32-profile.png | 149.725 | đủ nguồn (bản light) |

→ 12/12 nguồn có sẵn trong docs/work/, KHÔNG cần thay bằng file e2e/g-f3d/h-* nào.

## 2. Thay link ảnh trong tailieu/BAO_CAO.md (chỉ 12 dòng UI, giữ caption + cấu trúc)

| Dòng | Trước | Sau |
|---|---|---|
| 743 | `placeholders/01-home.png` | `screenshots/01-home.png` |
| 750 | `placeholders/02-login.png` | `screenshots/02-login.png` |
| 757 | `placeholders/04-lesson-detail.png` | `screenshots/04-lesson-detail.png` |
| 764 | `placeholders/05-simulator.png` | `screenshots/05-simulator.png` |
| 792 | `placeholders/06-exercise.png` | `screenshots/06-exercise.png` |
| 799 | `placeholders/13-learning-path.png` | `screenshots/13-learning-path.png` |
| 806 | `placeholders/14-ladder.png` | `screenshots/14-ladder.png` |
| 813 | `placeholders/15-lab.png` | `screenshots/15-lab.png` |
| 820 | `placeholders/16-code-runner.png` | `screenshots/16-code-runner.png` |
| 827 | `placeholders/17-benchmark.png` | `screenshots/17-benchmark.png` |
| 834 | `placeholders/24-leaderboard.png` | `screenshots/24-leaderboard.png` |
| 841 | `placeholders/32-profile.png` | `screenshots/32-profile.png` |

- Caption `*Hình 4.X: … (ảnh placeholder — chụp thật thay sau)*` GIỮ NGUYÊN theo chỉ thị PM (ghi rõ rủi ro mục 5).
- Các ảnh use-case/ERD trong BAO_CAO.md (diagrams/) KHÔNG đụng; parts/*.md (tài liệu nguồn hướng dẫn) KHÔNG sửa.

### 2b. Cập nhật caption (PM duyệt 13/08/2026 — ảnh đã thật, bỏ text lỗi thời)

- Dòng 739: `"…Ảnh hiện tại là placeholder, sẽ được thay bằng ảnh chụp thật khi hoàn thiện giao diện."` → `"…ảnh chụp từ ứng dụng thật (13/08/2026)."`
- 12 caption UI (dòng 744, 751, 758, 765, 793, 800, 807, 814, 821, 828, 835, 842): cụm `(ảnh placeholder — chụp thật thay sau)` → `(Ảnh chụp từ ứng dụng — 13/08/2026)`. GIỮ nguyên phần mô tả nội dung chính của từng caption.
- KHÔNG sửa caption diagrams (Hình 3.1–3.4, Hình 4.13/4.14) — ngoài phạm vi duyệt (12 màn UI); parts/*.md giữ nguyên.
- Build lại lần 2 (17:22): exit 0, docx = 2.516.493 bytes, vẫn 1 WARNING Hình 3.1 (xem mục 3).

## 3. Pandoc build docx

```
& "C:\Users\Administrator\AppData\Local\Pandoc\pandoc.exe" BAO_CAO.md -o BaoCaoDoAn.docx --toc --resource-path=tailieu
```

- Exit code: **0** (thành công).
- Kích thước: **2.516.495 bytes (2,4 MB)** — so với bản cũ 304.960 bytes (12/08/2026 12:56) → **tăng ~2,2 MB** (ảnh thật nặng hơn placeholder).
- Ảnh nhúng trong docx (word/media/): **17/17** — 12 ảnh UI thật (kích thước khớp 100% với tailieu/screenshots/) + 5 ảnh diagrams (02-06).
- **1 WARNING**: thiếu `diagrams/01-usecase-tong-quan.png` (Hình 3.1) — file bị XÓA trên disk bởi luồng diagram song song (worktree ` D` từ trước), pandoc thay bằng description. KHÔNG thuộc 12 màn UI của task này; không tự sửa link. → PM kiểm tra luồng diagram sinh lại 01-usecase rồi build lại.

## 4. Commit

```
feature/ux-h-docs (từ dev): 
git add tailieu/screenshots/ tailieu/BAO_CAO.md tailieu/BaoCaoDoAn.docx docs/work/h-docs.md
git commit -m "docs: H-A thay 12 anh UI that vao BAO_CAO + build docx (phuc)"
```
- KHÔNG push / KHÔNG merge (theo chỉ thị).

## 5. Rủi ro / việc cần PM quyết

1. ~~Caption vẫn ghi "(ảnh placeholder — chụp thật thay sau)"~~ — ĐÃ XỬ LÝ (PM duyệt 13/08/2026): 12 caption UI + dòng 739 đổi sang "(Ảnh chụp từ ứng dụng — 13/08/2026)" (mục 2b). Còn lại: caption diagrams Hình 3.1–3.4 + Hình 4.13/4.14 vẫn ghi placeholder — ảnh diagrams đang do luồng diagram xử lý, PM quyết có cập nhật không sau khi luồng đó chốt.
2. **Hình 3.1 (diagrams/01-usecase-tong-quan.png) đang thiếu trên disk** — ảnh bị xóa bởi luồng diagram (không phải task này). Docx hiện tại thay Hình 3.1 bằng description. Cần luồng diagram hoàn tất rồi build lại lần cuối.
3. Không dùng bản dark của 12 màn (02/05/24/32 có bản dark trong docs/work) — báo cáo đang trình bày light theme, đúng caption hiện có.
