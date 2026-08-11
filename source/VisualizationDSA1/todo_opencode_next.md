# TODO LIST CHO OPENCODE (Xử lý B1 & B4)

**Bối cảnh:** Lệnh mới từ PM: **QUY TRÌNH MỚI** - Từ nay trở đi, sau khi code và test xong, **KHÔNG ĐƯỢC COMMIT HAY PUSH**. Bạn phải dừng lại, báo cáo kết quả cho PM review xác nhận, sau đó mới được cấp phép commit!

**Nhiệm vụ của bạn (opencode):**

1. [ ] **Tháo gỡ Mock Quiz (B1):**
   - Mở file `frontend/src/features/quiz-system/components/BackendQuizWorkspace.vue`.
   - Xóa bỏ mảng fallback cứng `FALLBACK_QUIZZES` (khoảng dòng 214-229).
   - Sửa logic trong khối try/catch của hàm `loadQuizCatalog()`: Nếu API thất bại, hãy set state lỗi rõ ràng (ví dụ: `errorMessage.value = "..."`) thay vì gán bằng dữ liệu giả.

2. [ ] **Tháo gỡ Mock Sorting (B4):**
   - Mở file `frontend/src/features/custom-input/store/useInputStore.ts` (và các file liên quan).
   - Xóa bỏ hàm `generateDummyBubbleSortResult` đang được dùng để sinh kết quả giả khi không gọi được Backend. Thay vào đó, hiển thị lỗi thông báo API thất bại.
   - Sửa lại các file spec (`algorithmApi.spec.ts`, `useAnimationStore.spec.ts`...) nếu chúng bị phụ thuộc vào hàm sinh mock này, hãy chuyển sang mock tĩnh trực tiếp bên trong test.

3. [ ] **Xác minh qua Kiểm thử:**
   - Chạy `npm run build-only` hoặc `npm run type-check` ở frontend để đảm bảo không vỡ type.
   - Chạy `npx vitest run` để đảm bảo các unit test vẫn pass.

4. [ ] **Cập nhật Tracking:**
   - Mở `plan/review/mock-remaining.md` và đánh dấu `[x]` vào **B1** và **B4**.
   - Cập nhật `plan/tracking/progress.md`: Ghi chú rõ đã gỡ bỏ logic fallback ở Frontend (Quiz & Animation).

5. [ ] **DỪNG LẠI VÀ BÁO CÁO:**
   - Hoàn thành 4 bước trên, bạn hãy quay lại đây báo cáo kết quả test và danh sách file đã sửa. **Tuyệt đối không chạy lệnh `git commit` lúc này.**
