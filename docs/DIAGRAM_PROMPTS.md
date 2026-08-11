# HƯỚNG DẪN SINH ẢNH SƠ ĐỒ (DIAGRAM_PROMPTS)

> Mục đích: chuẩn bị ảnh sơ đồ cho báo cáo Word. Phân loại theo chất lượng Mermaid:
> - **NHÓM A — Mermaid làm TỐT → giữ Mermaid trong docs** (render thành ảnh bằng mmdc khi cần).
> - **NHÓM B — Mermaid làm KHÔNG tốt (ERD, Use Case) → sinh ảnh bằng ChatGPT** với prompt mô tả chi tiết.
> Mọi tên (actor/use case/bảng/cột/ID FR-UC) lấy NGUYÊN VĂN từ Mermaid trong `docs/SRS.md`/`docs/SDD.md` — ảnh phải trùng docs 100%.

## NHÓM A — Giữ Mermaid (KHÔNG gen ảnh ChatGPT)

| Sơ đồ | Nguồn Mermaid | Ghi chú |
|---|---|---|
| Sequence UC-01 chạy mô phỏng | SRS §6 | Mermaid sequence đẹp, giữ |
| Sequence trừ tim atomic (UC-25 / FR-10.1) | SDD §5 | giữ |
| Sequence nộp bài + chấm điểm (UC-06) | SRS §6 | giữ |
| Sitemap / sơ đồ luồng màn hình | SDD §8 (20.2.1) | giữ |
| Activity / state machine mô phỏng | SDD §3 (12.8) | giữ |
| Class Simulation Engine EDV | SDD §4 | giữ |
| Class kiến trúc backend | SDD §5 | giữ |
| Deployment | DEPLOY §1 | giữ |

> Trong báo cáo Word, Mermaid hiển thị dạng code block. Muốn thành ảnh: task 9 chạy `npx -y @mermaid-js/mermaid-cli` (mmdc) render từng sơ đồ nhóm A ra `tailieu/diagrams/`. Nếu mmdc lỗi (thiếu Chrome) → để nguyên code block, không chặn tiến độ.

## NHÓM B — Gen ảnh ChatGPT (6 ảnh bắt buộc)

| # | File ảnh | Sơ đồ | Style vẽ (trong prompt) |
|---|---|---|---|
| 1 | `01-usecase-tong-quan.png` | Use case tổng thể (3 tác nhân: Người học / Giảng viên / Quản trị viên) | Người que → elip trong khung hệ thống |
| 2 | `02-usecase-hoc-vien.png` | Use case người học (UC-01..08, 14, 17..19, 21..32) | như trên |
| 3 | `03-usecase-giang-vien.png` | Use case giảng viên (UC-09..11, 20) | như trên |
| 4 | `04-usecase-admin.png` | Use case quản trị viên (UC-12, 13) | như trên |
| 5 | `05-erd-tong-quan.png` | ERD tổng quan: các cụm nghiệp vụ (Auth, Học tập, Engine, Lớp học, Gamification, Code Runner) + quan hệ giữa cụm | Hình vuông/thoi theo cụm, đường nối có nhãn 1-n |
| 6 | `06-erd-chi-tiet.png` | ERD chi tiết 32 bảng (đầy đủ cột, PK/FK, đường nối khóa ngoại) | Khung bảng: tiêu đề + danh sách cột, PK/FK đánh dấu |

## Template prompt CHUẨN cho NHÓM B (copy + điền [ ])

```
Vẽ sơ đồ [TÊN SƠ ĐỒ] cho báo cáo đồ án tốt nghiệp, theo đúng dữ liệu sau.

DỮ LIỆU (nguồn docs/SRS.md hoặc docs/SDD.md — KHÔNG được đổi tên):
[Dán: mermaid gốc + danh sách tên đầy đủ bằng lời, VD:
- Tác nhân: Người học, Giảng viên, Quản trị viên
- Use cases: UC-01 Chạy mô phỏng giải thuật, UC-02 Tạo tài khoản, ... (liệt kê hết)
hoặc cho ERD: danh sách bảng + cột chính + quan hệ]

PHONG CÁCH VẼ (bắt buộc):
[usecase] Tác nhân = hình NGƯỜI QUE (stick figure) xếp bên trái, nối đường liền nét
tới use case hình ELIP đặt bên trong khung hệ thống. Mỗi elip ghi tên use case
(kèm mã UC-xx).
[erd-tổng-quan] Mỗi cụm nghiệp vụ = 1 hình VUÔNG/HÌNH THOI lớn ghi tên cụm + vài
bảng đại diện nhỏ bên trong; đường nối giữa cụm có nhãn quan hệ (1-n, n-n).
[erd-chi-tiết] Mỗi bảng = khung bảng (tiêu đề bảng, dưới là danh sách cột, cột khóa
chính đánh dấu PK, khóa ngoại đánh dấu FK); đường nối khóa ngoại giữa các bảng.

QUY TẮC:
- Giữ NGUYÊN mọi tên và mã ID (UC-xx, FR-xx, tên bảng, tên cột) — không thêm/bớt.
- Tiếng Việt cho tên hiển thị; tên bảng/cột kỹ thuật giữ tiếng Anh.
- Chữ ≥ 10pt, nền trắng, tỷ lệ cân đối, không watermark.
- Nếu ảnh quá nhiều nội dung: tách thành 2 ảnh cùng style (báo lại để gộp).
```

## Quy trình (tự động hóa trong task 9)

1. Task 9 trích mermaid + liệt kê tên đầy đủ (use case/bảng) từ docs cuối cùng.
2. Điền vào template → ghi `tailieu/diagram-prompts.md`: 6 prompt NHÓM B hoàn chỉnh + danh sách sơ đồ NHÓM A.
3. User sáng dậy: dán 6 prompt vào ChatGPT → ảnh về đặt đúng tên `tailieu/diagrams/`.
4. Chạy lại pandoc build docx (ảnh nhóm B) + mmdc (nhóm A nếu muốn).
