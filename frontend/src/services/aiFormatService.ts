/**
 * AI Formatter Service — Hỗ trợ giáo viên định dạng và làm đẹp bài giảng tự động
 * Sử dụng DeepSeek V4 qua OpenAI-compatible endpoint.
 * Giới hạn tối đa 5 lượt gọi / tài khoản (lưu trữ theo user / session).
 */

const AI_ENDPOINT = 'https://api.xkiro.com/v1/chat/completions';
const AI_MODEL = 'deepseek/deepseek-v4-flash-0731';
const AI_API_KEY = 'sk-xt-42a1003011dee04cb60348ff15d6d8c36408cbd75c1a1ae4';

const MAX_AI_USES = 5;
const STORAGE_KEY_PREFIX = 'vdsa_ai_format_uses_';

export function getAiUsageRemaining(userEmailOrId: string = 'default'): number {
  try {
    const key = `${STORAGE_KEY_PREFIX}${userEmailOrId}`;
    const used = parseInt(localStorage.getItem(key) || '0', 10);
    return Math.max(0, MAX_AI_USES - used);
  } catch {
    return MAX_AI_USES;
  }
}

export function incrementAiUsage(userEmailOrId: string = 'default'): number {
  try {
    const key = `${STORAGE_KEY_PREFIX}${userEmailOrId}`;
    const used = parseInt(localStorage.getItem(key) || '0', 10) + 1;
    localStorage.setItem(key, String(used));
    return Math.max(0, MAX_AI_USES - used);
  } catch {
    return 0;
  }
}

const SYSTEM_PROMPT = `Bạn là chuyên gia sư phạm Cấu trúc dữ liệu & Giải thuật (DSA) kiêm chuyên gia Markdown.
Nhiệm vụ của bạn là nhận văn bản bài giảng thô từ giáo viên và định dạng lại thành tài liệu Markdown chuẩn GitHub Flavored Markdown (GFM) tuyệt đẹp.

Yêu cầu định dạng bắt buộc:
1. TIÊU ĐỀ: Sử dụng '# ' cho tiêu đề chính, '## ' cho phần lớn, '### ' cho mục nhỏ (ví dụ: '### 1. Động cơ học', '### 2. Lý thuyết cốt lõi', '### 3. Thuật toán từng bước').
2. KHỐI MÃ NGUỒN: Mọi đoạn code mẫu hoặc giải thuật phải được bọc trong code block đúng ngôn ngữ với 3 dấu backticks (ví dụ: \`\`\`javascript ... \`\`\` hoặc \`\`\`cpp ... \`\`\` hoặc \`\`\`csharp ... \`\`\`).
3. BẢNG BIỂU: Mọi nội dung so sánh độ phức tạp hoặc liệt kê dạng bảng phân cách bằng dấu chấm '·' hoặc tab phải được chuyển thành BẢNG MARKDOWN chuẩn có hàng tiêu đề và phân cách (ví dụ:
| Thuật toán | Tốt nhất | Trung bình | Xấu nhất | Đổi chỗ |
| :--- | :--- | :--- | :--- | :--- |
| Bubble Sort | O(N) | O(N²) | O(N²) | O(N²) |
)
4. CALLOUTS / HỘP THÔNG BÁO:
- Các lưu ý, ghi chú: dùng '> [!NOTE]'
- Các mẹo hay, tối ưu: dùng '> [!TIP]'
- Các cảnh báo, bẫy thường gặp: dùng '> [!WARNING]'
5. DANH SÁCH: Sử dụng '- ' cho gạch đầu dòng và '1. ' cho các bước tuần tự.
6. GIỮ NGUYÊN NỘI DUNG & Ý NGHĨA: Không tự ý cắt bỏ kiến thức quan trọng của giáo viên, chỉ trau chuốt, sắp xếp cấu trúc mạch lạc và chuẩn hóa Markdown.
7. TRẢ VỀ: Chỉ trả về nội dung Markdown đã được format, KHÔNG kèm lời chào, lời giải thích hay bọc toàn bộ phản hồi trong khối \`\`\`markdown.`;

export async function formatLessonWithAi(
  rawContent: string,
  userEmailOrId: string = 'default',
): Promise<string> {
  const remaining = getAiUsageRemaining(userEmailOrId);
  if (remaining <= 0) {
    throw new Error(`Bạn đã sử dụng hết ${MAX_AI_USES} lượt định dạng AI miễn phí.`);
  }

  if (!rawContent || !rawContent.trim()) {
    throw new Error('Nội dung bài học đang trống, vui lòng nhập văn bản trước khi format.');
  }

  const payload = {
    model: AI_MODEL,
    messages: [
      { role: 'system', content: SYSTEM_PROMPT },
      {
        role: 'user',
        content: `Hãy định dạng lại bài giảng thô sau đây thành Markdown chuẩn đẹp mắt:\n\n${rawContent}`,
      },
    ],
    temperature: 0.2,
  };

  const res = await fetch(AI_ENDPOINT, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${AI_API_KEY}`,
    },
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    const errText = await res.text();
    throw new Error(`Lỗi kết nối AI (${res.status}): ${errText || res.statusText}`);
  }

  const data = await res.json();
  let formatted = data?.choices?.[0]?.message?.content || '';

  // Clean up if AI wrapped everything in ```markdown ... ```
  if (formatted.startsWith('```markdown') && formatted.endsWith('```')) {
    formatted = formatted.slice(11, -3).trim();
  } else if (formatted.startsWith('```') && formatted.endsWith('```')) {
    formatted = formatted.slice(3, -3).trim();
  }

  incrementAiUsage(userEmailOrId);
  return formatted;
}
