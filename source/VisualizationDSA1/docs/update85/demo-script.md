# 🎬 KỊCH BẢN DEMO 10 PHÚT — VisualizationDSA

> **Chuẩn bị:** Docker đang chạy (frontend 5173, backend 5055, DB 5433, Judge0 2358). Đã seed data (G4.1).
> **Tài khoản:** demo@visualizationdsa.dev / Demo@2024 (Teacher, Premium) · admin@visualizationdsa.dev / Admin@2024.

---

## 0:00 – 0:30 · Mở đầu (30s)
- Trình chiếu Landing (redesign D11): Hero + grid thuật toán thật (30+ thuật toán) + Freemium section (Hearts/Gems/Premium).
- 1 câu: "Nền tảng giúp sinh viên NHÌN THẤY thuật toán chạy từng bước, thay vì học vẹt."

## 0:30 – 2:00 · Thư viện thuật toán (1.5 phút)
- Mở `/algorithms` — grid nhóm Sorting/Graph/OOP.
- Lọc "Graph" + tìm "dijkstra" → thấy badge Premium + độ khó Hard.
- Click 1 card → vào Visualizer.

## 2:00 – 3:30 · Visualizer Graph (1.5 phút)
- Sân chơi đồ thị: kéo thả node, thêm edge, chạy BFS/DFS/Dijkstra.
- VCR: Play/Pause/Step — panel phụ bên phải (Pseudocode/Trace) collapse được.
- Nhấn "?" xem tour hướng dẫn.

## 3:30 – 6:00 · Lesson Study (2.5 phút)
- Mở `/courses` → chọn 1 roadmap (Sorting) → vào bài.
- **1 trang cuộn**: Lý thuyết markdown → Visualizer mini → Quiz → CodeLab.
- Làm quiz → pass (≥70%) → hoàn thành bài → +XP, modal thành công.
- Chỉ rõ stepper: ✓ đã xong / ● đang học / 🔒 khóa.

## 6:00 – 7:30 · Gõ code xem code chạy (1.5 phút)
- Mở `/code-ide`: viết code bubble sort → Run → xem animation từng dòng.
- Hoặc Practice Ladder: Quiz → Lab → LeetCode (Judge0 chấm thật).

## 7:30 – 8:30 · Gamification (1 phút)
- Dashboard: streak calendar (dữ liệu thật), XP wheel, nhiệm vụ ngày.
- Hearts (Duolingo-style): show hết tim → modal xem ad + recovery timer.
- Gems Shop: mua frame/avatar → equip.

## 8:30 – 9:30 · Teacher & Admin (1 phút)
- Teacher Studio: tạo roadmap, thêm node (UTF-8 chuẩn), xuất bản → Pending.
- Admin: tab "Duyệt Teacher" + "Duyệt Roadmap" (API thật) → duyệt → user thành Teacher.

## 9:30 – 10:00 · Premium & Kết (30s)
- Checkout: tạo đơn → QR VietQR. (Dev) bấm "Mô phỏng: đã thanh toán" → Premium tự mở khóa.
- Tổng kết + trả lời 3 câu hỏi hội đồng.

---

## Fallback Plan (nếu lỗi giữa chừng)

| Sự cố | Fallback |
|---|---|
| Judge0 không chạy | Mở `/graph` sân chơi (không cần Judge0) + giới thiệu IDE animation engine |
| Backend chậm/500 | Dùng demo account có sẵn; nếu vẫn lỗi → trình bày Landing + Docs + Visualizer local |
| SePay không kết nối | Giới thiệu flow qua `simulate-demo-webhook` (dev-only), nhấn mạnh cơ chế HMAC verify |
| Mất mạng | Tài liệu diagrams.md + design-system.md trình chiếu, nói kiến trúc |

## 3 câu hỏi hội đồng (chuẩn bị sẵn)

1. **Khác VisuAlgo?** → "Cho người học GÕ CODE của mình và xem nó chạy từng dòng qua AST instrumentation (Web Worker sandbox)."
2. **Phần khó nhất?** → "Engine chuyển code → frame animation, chống vòng lặp vô hạn, đồng bộ pseudocode đa ngôn ngữ."
3. **Bảo mật?** → "Đã vá: JWT từ env, payment có auth + webhook signature (HMAC), không secret trong repo, quiz ẩn đáp án khỏi payload."
