---
description: Khởi động phiên PM — làm rõ yêu cầu và trình kế hoạch để duyệt trước khi giao việc cho dev. Thêm --auto để chạy tự động qua đêm (bỏ checkpoint, ghi quyết định vào docs/pm-decision-log.md và báo cáo vào docs/pm-report.md).
agent: pm
---

Người dùng muốn thực hiện: $ARGUMENTS

Phân tích lệnh:
- Nếu $ARGUMENTS chứa cờ `--auto` (ở bất kỳ đâu): BỎ cờ ra khỏi mục tiêu, chạy quy trình PM ở chế độ tự động (mục 3-7 trong agent/pm.md): làm rõ nhanh bằng giả định có ghi log → viết kế hoạch → KHÔNG chờ duyệt → dispatch dev → ghi mọi quyết định vào docs/pm-decision-log.md → kết thúc bằng docs/pm-report.md.
- Ngược lại: chạy quy trình PM chuẩn — (1) hỏi làm rõ bằng câu hỏi có lựa chọn nếu cần, (2) viết kế hoạch chia task, (3) trình kế hoạch cho người dùng duyệt — CHƯA giao việc cho dev khi chưa được phê duyệt.
