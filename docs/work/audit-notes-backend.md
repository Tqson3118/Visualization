# GHI CHÚ AUDIT — phát hiện lỗ hổng (chờ prompt J backend audit)

> Ngày: 13/08/2026 · Nguồn: user hỏi "xuống free là 30/10 hay sao" → check code thật.

## BUG-1: Job downgrade Premium KHÔNG tồn tại — hết hạn vẫn HeartsMax 30

- **Docs (SRS FR-10.7)**: hết hạn → job downgrade → clamp Hearts về 10 (HeartsMax 30→10; Hearts > 10 → clamp; regen 30 phút/tim).
- **Code**: `GamificationService.cs:840-844` `HeartConfig()` — hết hạn trả `user.HeartsMax` (cột DB vẫn 30, không nơi nào reset). KHÔNG có BackgroundService/HostedService/job downgrade (grep toàn bộ backend/src: 0 kết quả).
- **Hệ quả hiện tại**: user hết hạn Premium vẫn hiển thị 30/30 tim (chỉ regen chậm 30p). Trả lời câu hỏi "30 hay 10": docs nói 10, code thực tế giữ 30.
- **Sửa (khi chạy prompt J)**: hoặc (a) thêm job định kỳ (hosted service mỗi giờ) query `Users WHERE PremiumUntil < now AND HeartsMax = 30` → set HeartsMax=10 + `UPDATE Hearts = MIN(Hearts, 10)` atomic; hoặc (b) lazy fix ngay trong `HeartConfig`/`GetHeartsStatus` + migration set cột khi gọi (ghi rõ quyết định).
- **Check thêm**: API `/premium/status` (PremiumDtos Status "expired") có khớp không; UI Màn 27 hiển thị đúng không.

## Các điểm khác cần đưa vào prompt J (phát hiện trong quá trình)
- (để trống — bổ sung khi audit)
