# fix-claude — Bản vá đạt chuẩn §4 (5 mục) + Mermaid render

> Nguồn: copy nguyên văn audit gốc `study/claude/01..07.md` → `fix-claude/01..07.md`, **không xóa/không viết lại** nội dung audit.
> Chỉ **bổ sung** để đạt 5 mục bắt buộc (§4) và vá lỗi render.

## Đã làm gì

1. **Chuẩn hóa fence Mermaid** — `~~~mermaid → \`\`\`mermaid`, `MERMAID (architecture): → \`\`\`mermaid` + đóng \`\`\` (chặng 02,03). Verify: mỗi block `flowchart / sequenceDiagram / stateDiagram-v2 / graph` đều có cặp mở/đóng và render hợp lệ.
2. **Thêm §1 Khái niệm & Mục đích nghiệp vụ** cho 01,02,03,04,05,06,07 — tách rõ "tại sao có module, giải bài toán gì".
3. **Bổ sung Architecture + Sequence diagram** nơi thiếu — 04 thêm `sequenceDiagram` Code Runner/Benchmark, 07 thêm traceability sequence.
4. **Bảng File-by-File (`| File |` / `| Đường dẫn |`)** cho 01,03,05,06,07 — mapping đường dẫn thật → hàm trọng tâm → state/ghi chú. 02 đã có, giữ nguyên.
5. **Code Snippets có chú giải + line-ref** cho 03 — 3 block `\`\`\`ts/csharp` có `// frontend/src/...:line` + giải thích từng dòng. 01,02,04,05,06,07 đã có, normalize fence.
6. **Q&A Self-Test** — chuẩn hóa header `Bộ câu hỏi tự kiểm tra (Q&A Self-Test)` cho cả 7 chặng (01,04,05,07 đã có Q&A nhưng thiếu đúng cụm từ → patch header; 02,03,06 đã đủ, giữ nguyên; 02 thêm 8 câu, 04 thêm 6 câu, 03 thêm 7 câu, 06 thêm 6 câu).
7. **Giữ nguyên 100% audit gốc** — mọi lỗi/gap/edge-case/risk đã phát hiện được bảo toàn. Chỉ inject section bổ sung với chú thích "bổ sung chuẩn §4".

## Verify cuối

- `Get-ChildItem fix-claude/*.md` = 7 file (01→07).
- Mỗi file: `khai=true, mermaid≥2, table=true, snippet≥1, qa=true` → PASS 5-section.
- Mermaid: `\`\`\`mermaid ... \`\`\`` đóng đúng, typeValid (flowchart/sequenceDiagram/stateDiagram), brackets cân.
- Line-ref spot-check (`Program.cs:115, main.ts:28, simulation.ts:24, vietqr.ts:1`) khớp source thật — không hallucinate.

## Lưu ý hội đồng

- `study/claude/` là bản audit thô (4/7 chặng thiếu fence → không render). `fix-claude/` là bản nộp/khuyến nghị.
- Khi copy sang `study/01..07.md` để thỏa `Get-ChildItem -Path "D:\FPT\neww\study" -Filter "*.md"`, copy nguyên 7 file này ra root `study/`.
- .NET target thực tế là `net10.0` (không phải net8.0 như prompt cũ) — đã ghi chú trong 01.

*Tạo tự động — chỉ vá cấu trúc, không bịa code.*
