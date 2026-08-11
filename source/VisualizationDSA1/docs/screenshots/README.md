# 📸 Screenshot UI — Hướng dẫn chạy (G1/G2)

> Công cụ tự chụp screenshot **dark + light × 4 breakpoint (375/768/1024/1440)** cho từng màn UI.
> Output: `docs/screenshots/<BATCH>/<màn>_<theme>_<breakpoint>.png`

## 1. Yêu cầu

- Node 18+ (repo dùng v24.15.0).
- Backend Docker đang chạy (5055) + có dữ liệu seed (tài khoản demo `demo@visualizationdsa.dev` / `Demo@2024`).
- Playwright (cài ở bước 3).

## 2. Khởi động frontend (dev server)

Cổng 5173 đang bị Docker chiếm → chạy ở **5174**:

```bash
cd VisualizationDSA/frontend
npm run dev -- --port 5174
```

Giữ terminal này mở (script sẽ gọi `http://localhost:5174`).

## 3. Cài playwright

```bash
cd VisualizationDSA/scripts/screenshots
npm init -y
npm i playwright
npx playwright install chromium
```

## 4. Chạy

```bash
cd VisualizationDSA/scripts/screenshots
node screenshot.mjs
```

Kết quả ví dụ:

```
lesson-study | dark  | 375   | h-overflow=0px
lesson-study | light | 1440  | h-overflow=0px
...
✅ Xong 48 ảnh → docs/screenshots/G1.1
```

## 5. Tùy biến (env)

| Biến | Mặc định | Ý nghĩa |
|---|---|---|
| `VDS_BASE` | `http://localhost:5174` | URL dev server |
| `VDS_BATCH` | `G1.1` | Thư mục con output |
| `VDS_OUT` | `docs/screenshots/<BATCH>` | Thư mục đích |
| `VDS_EMAIL` / `VDS_PASS` | demo / Demo@2024 | Tài khoản login |

Ví dụ chụp bộ màn G2 vào thư mục riêng:

```bash
VDS_BATCH=G2 node screenshot.mjs
```

## 6. Thêm/chỉnh danh sách màn

Sửa mảng `SCREENS` trong `screenshot.mjs`:

```js
{ name: 'landing',   path: '/',             auth: false },
{ name: 'dashboard', path: '/dashboard',    auth: true  },
{ name: 'profile',   path: '/profile',      auth: true  },
```

- `auth: true` → script tự đăng nhập UI (form `/login`) trước khi chụp.
- `waitFor: 'selector'` → chờ 1 selector cụ thể (tránh chụp lúc loading).
- `waitMs: 2000` → tăng thời gian chờ render.

## 7. Tiêu chí đạt (theo plan G1 DoD)

- Mọi ảnh có `h-overflow = 0px` (không tràn ngang / không scrollbar dư).
- Nội dung đáy không bị cắt (do `overflow-hidden` trên view root đã gỡ).
- Đúng token (không màu hardcode — kiểm tra bằng mắt trên ảnh).

## 8. Lưu ý

- Guided tour tự động bị tắt bằng `guided_tour_seen` trong localStorage (nếu không sẽ che cả màn hình).
- Màn cần role Admin/Teacher như `/admin`, `/teacher-studio/:id` cần tài khoản đúng vai — thêm account tương ứng trước khi chụp.
- Script chỉ ĐỌC (chụp ảnh), không ghi dữ liệu vào hệ thống.
- **CORS:** backend Docker chỉ cho phép origin `http://localhost:5173`. Nếu chạy dev server ở port khác (5174), các API gọi trực tiếp sẽ bị CORS chặn → màn không có dữ liệu (layout vẫn chụp được). Muốn có dữ liệu: chạy dev ở 5173 (tạm dừng Docker frontend) hoặc thêm origin trong `backend/src/WebApi/Program.cs` (block development).

