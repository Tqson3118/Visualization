# 📊 SƠ ĐỒ ĐỒ ÁN — VisualizationDSA (G4.4.1/4.4.2/4.4.3)

> **Ngày:** 2026-08-06 · **Mermaid syntax** — dán vào https://mermaid.live để xem render.

---

## 1. Use Case Diagram (49 UC — theo use-cases.md)

```mermaid
graph TD
    subgraph Guest["Khách (Guest)"]
        UC01[UC-01 Đăng ký]
        UC02[UC-02 Đăng nhập]
        UC03[UC-03 Khôi phục mật khẩu]
        UC04[UC-04 Duyệt thư viện]
        UC05[UC-05 Xem tài liệu]
        UC06[UC-06 Xem thử visualizer]
    end

    subgraph Student["Học viên (Student) — kế thừa Guest"]
        UC07[UC-07 Đăng ký roadmap]
        UC08[UC-08 Học bài theo roadmap]
        UC09[UC-09 Trực quan hóa tương tác]
        UC10[UC-10 Gõ code xem code chạy]
        UC11[UC-11 Làm quiz]
        UC12[UC-12 Đánh dấu hoàn thành]
        UC13[UC-13 Xem tiến độ]
        UC14[UC-14 Quản lý hồ sơ]
        UC15[UC-15 Lịch sử quiz]
        UC16[UC-16 Thông báo]
        UC17[UC-17 XP/Streak]
        UC18[UC-18 Mua vật phẩm Gems]
        UC19[UC-19 Tim bổ sung]
        UC20[UC-20 AI Assistant]
        UC21[UC-21 Nâng cấp Premium]
        UC22[UC-22 Embed/Export]
        UC23[UC-23 Tham gia lớp]
        UC24[UC-24 Danh sách lớp]
        UC25[UC-25 Rời lớp]
        UC26[UC-26 Nộp đơn Teacher]
        UC27[UC-27 Xem trạng thái đơn]
    end

    subgraph Teacher["Giảng viên (Teacher) — kế thừa Student"]
        UC28[UC-28 Tạo roadmap]
        UC29[UC-29 Quản lý bài học]
        UC30[UC-30 Clone roadmap]
        UC31[UC-31 Xuất bản roadmap]
        UC32[UC-32 Sửa/xóa roadmap]
        UC33[UC-33 Tạo lớp]
        UC34[UC-34 Mã tham gia]
        UC35[UC-35 Danh sách học viên]
        UC36[UC-36 Gỡ học sinh]
        UC37[UC-37 Đổi roadmap/đóng lớp]
        UC38[UC-38 Thống kê lớp]
        UC39[UC-39 Thống kê roadmap]
        UC40[UC-40 Thông báo duyệt]
    end

    subgraph Admin["Quản trị (Admin) — toàn quyền"]
        UC41[UC-41 Dashboard]
        UC42[UC-42 Quản lý người dùng]
        UC43[UC-43 Duyệt Teacher]
        UC44[UC-44 Duyệt Roadmap]
        UC45[UC-45 Ngân hàng quiz]
        UC46[UC-46 Quản lý thanh toán]
        UC47[UC-47 Broadcast thông báo]
        UC48[UC-48 Báo cáo nội dung]
        UC49[UC-49 Giám sát lớp]
    end
```

---

## 2. Sequence Diagram — Học bài theo roadmap (UC-08/UC-11/UC-12)

```mermaid
sequenceDiagram
    participant S as Học viên
    participant FE as Frontend (Vue)
    participant BE as Backend (.NET)
    participant DB as PostgreSQL

    S->>FE: Mở lesson /lessons/:id
    FE->>BE: GET /api/v1/concepts/lessons/:id
    BE->>DB: Load lesson + module items + progress
    DB-->>BE: lesson, status, lastActiveFrame
    BE-->>FE: { title, contentMd, quizId, status, lastScrollPercent }
    FE->>FE: Resume scroll (G3.1.2)

    loop Mỗi step
        S->>FE: Làm quiz → submit
        FE->>BE: POST /api/v1/concepts/quiz/submit
        BE->>DB: Chấm điểm + trừ 1 heart nếu fail (G3.3.3)
        DB-->>BE: score, passed
        BE-->>FE: { score, passed }
    end

    S->>FE: Hoàn thành bài
    FE->>BE: POST /api/v1/concepts/lessons/:id/complete
    BE->>DB: Kiểm tra step lock (G3.1.1)
    alt Quiz/Codelab chưa pass
        BE-->>FE: 403 STEP_LOCKED
    else Đủ điều kiện
        BE->>DB: Mark Completed + cộng XP
        BE-->>FE: { xpAwarded, totalXp }
    end
```

---

## 3. Sequence Diagram — Duyệt Teacher (UC-43)

```mermaid
sequenceDiagram
    participant T as Học viên
    participant A as Admin
    participant BE as Backend
    participant DB as PostgreSQL

    T->>BE: POST /api/v1/teacher-applications { schoolName, reason }
    BE->>DB: Lưu đơn (Pending)
    BE-->>T: 201 { status: Pending }

    A->>BE: GET /api/v1/admin/teacher-applications?status=Pending
    BE-->>A: Danh sách đơn

    A->>BE: PATCH /api/v1/admin/teacher-applications/{id}/approve
    BE->>DB: Set role Teacher + tạo notification
    DB-->>BE: OK
    BE-->>A: { status: Approved }
    BE-->>T: Notification "Đơn đã được duyệt"
```

---

## 4. Sequence Diagram — Thanh toán SePay (UC-21)

```mermaid
sequenceDiagram
    participant S as Học viên
    participant FE as Frontend
    participant BE as Backend
    participant SEP as SePay

    S->>FE: Nhấn Nâng cấp Premium
    FE->>BE: POST /api/v1/payments/order (Bearer)
    BE-->>FE: { paymentCode, amount, qrUrl }
    FE->>S: Hiển thị QR VietQR

    S->>SEP: Chuyển khoản đúng mã VDSAxxxxxx
    SEP->>BE: POST /api/v1/payments/sepay-webhook (HMAC X-SePay-Signature)
    BE->>BE: Verify signature (SePay:WebhookSecret)
    BE->>BE: Match paymentCode + transferAmount
    BE->>DB: Mark Completed + SetPremiumStatus(true)
    BE-->>SEP: 200 { success }

    FE->>BE: GET /api/v1/payments/orders/{id}/status (poll)
    BE-->>FE: { status: Completed }
    FE->>S: Hiển thị thành công + confetti
```

---

## 5. Architecture Diagram (Clean Architecture)

```mermaid
graph TB
    subgraph FE["Frontend (Vue 3 + TS + Tailwind)"]
        V[Views & Pages]
        ST[Pinia Stores]
        API[API Services]
        CANVAS[Canvas 2D Engine<br/>Lerp 60FPS]
    end

    subgraph BE["Backend (.NET 9 — Clean Architecture)"]
        WEB[WebApi Controllers<br/>/api/v1/*]
        APP[Application Layer<br/>MediatR, DTOs, Services]
        DOM[Domain Layer<br/>Entities, Domain Rules]
        INFRA[Infrastructure<br/>EF Core, Seeder, Judge0, SePay]
    end

    subgraph EXT["External"]
        PG[(PostgreSQL 5433)]
        JUDGE0[Judge0 API 2358]
        SEPAY[SePay Webhook]
        REDIS[(Redis)]
    end

    V --> ST --> API
    API --> WEB
    CANVAS --> ST
    WEB --> APP --> DOM
    WEB --> INFRA
    INFRA --> PG
    INFRA --> JUDGE0
    INFRA --> SEPAY
    INFRA --> REDIS
```

---

## 6. Công nghệ & Cổng

| Thành phần | Công nghệ | Cổng |
|---|---|---|
| Frontend | Vue 3, Vite, Pinia, Tailwind | 5173 |
| Backend | .NET 9, Clean Architecture, MediatR, EF Core | 5055 |
| Database | PostgreSQL 15 | 5433 |
| Judge0 | Judge0 CE 1.13 | 2358 |
| Redis | Redis 6 | 6379 |
