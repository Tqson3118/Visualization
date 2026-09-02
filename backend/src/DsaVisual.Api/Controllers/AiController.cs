using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

[ApiVersion("1.0")]
[Route("api/v1/ai")]
[Authorize]
public class AiController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<AiController> logger) : ApiControllerBase
{
    private const string DefaultEndpoint = "https://api.xkiro.com/v1/chat/completions";
    private const string DefaultModel = "qwen/qwen3.5-flash:free";

    private const string FormatSystemPrompt = @"Bạn là chuyên gia sư phạm Cấu trúc dữ liệu & Giải thuật (DSA) kiêm chuyên gia Markdown.
Nhiệm vụ của bạn là nhận văn bản bài giảng thô từ giáo viên và định dạng lại thành tài liệu Markdown chuẩn GitHub Flavored Markdown (GFM) tuyệt đẹp.

Yêu cầu định dạng bắt buộc:
1. TIÊU ĐỀ: Sử dụng '# ' cho tiêu đề chính, '## ' cho phần lớn, '### ' cho mục nhỏ (ví dụ: '### 1. Động cơ học', '### 2. Lý thuyết cốt lõi', '### 3. Thuật toán từng bước').
2. KHỐI MÃ NGUỒN: Mọi đoạn code mẫu hoặc giải thuật phải được bọc trong code block đúng ngôn ngữ với 3 dấu backticks (ví dụ: ```javascript ... ``` hoặc ```cpp ... ``` hoặc ```csharp ... ```).
3. BẢNG BIỂU: Mọi nội dung so sánh độ phức tạp hoặc liệt kê dạng bảng phân cách bằng dấu chấm '·' hoặc tab phải được chuyển thành BẢNG MARKDOWN chuẩn có hàng tiêu đề và phân cách.
4. CALLOUTS / HỘP THÔNG BÁO:
- Các lưu ý, ghi chú: dùng '> [!NOTE]'
- Các mẹo hay, tối ưu: dùng '> [!TIP]'
- Các cảnh báo, bẫy thường gặp: dùng '> [!WARNING]'
5. DANH SÁCH: Sử dụng '- ' cho gạch đầu dòng và '1. ' cho các bước tuần tự.
6. GIỮ NGUYÊN NỘI DUNG & Ý NGHĨA: Không tự ý cắt bỏ kiến thức quan trọng của giáo viên, chỉ trau chuốt, sắp xếp cấu trúc mạch lạc và chuẩn hóa Markdown.
7. TRẢ VỀ: Chỉ trả về nội dung Markdown đã được format, KHÔNG kèm lời chào, lời giải thích hay bọc toàn bộ phản hồi trong khối ```markdown.";

    private const string TutorSystemPrompt = @"Bạn là trợ lý AI Sư phạm Cấu trúc Dữ liệu & Giải thuật (DSA Tutor).
Nhiệm vụ DUY NHẤT của bạn là giải thích thuật toán, cấu trúc dữ liệu, phân tích bước chạy và giải đáp câu hỏi liên quan đến DSA/khoa học máy tính.
QUY TẮC AN TOÀN & BẢO MẬT BẮT BUỘC:
1. TUYỆT ĐỐI TỪ CHỐI cung cấp mật khẩu, thông tin cá nhân, API keys, tài khoản hay dữ liệu hệ thống nội bộ.
2. Với các câu hỏi KHÔNG LIÊN QUAN đến DSA/lập trình (như thời tiết, tin tức, đời sống...), hãy từ chối lịch sự, ngắn gọn trong 1-2 câu và hướng dẫn người học quay lại chủ đề thuật toán đang chạy.
3. Luôn trả lời bằng tiếng Việt sư phạm, dễ hiểu, chuẩn Markdown (dưới 150 từ).";

    public sealed class FormatTheoryRequest
    {
        public string RawContent { get; set; } = string.Empty;
    }

    public sealed class ExplainStepRequest
    {
        public string AlgorithmTitle { get; set; } = string.Empty;
        public int StepIndex { get; set; }
        public JsonElement? Explanation { get; set; }
        public JsonElement? Variables { get; set; }
        public JsonElement? PseudocodeLine { get; set; }
        public string UserQuestion { get; set; } = string.Empty;
    }

    [HttpPost("format-theory")]
    [Authorize(Roles = "TEACHER,ADMIN")]
    public async Task<ActionResult<object>> FormatTheory([FromBody] FormatTheoryRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RawContent))
        {
            return BadRequest(new { message = "Nội dung bài học đang trống, vui lòng nhập văn bản trước khi format." });
        }

        var endpoint = configuration["DSA:Ai:Endpoint"] ?? DefaultEndpoint;
        var model = configuration["DSA:Ai:Model"] ?? DefaultModel;
        var apiKey = configuration["DSA:Ai:ApiKey"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("AI ApiKey is not configured in DSA:Ai:ApiKey.");
            return StatusCode(500, new { message = "Dịch vụ AI chưa được cấu hình khóa API phía máy chủ." });
        }

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(60);

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = FormatSystemPrompt },
                new { role = "user", content = $"Hãy định dạng lại bài giảng thô sau đây thành Markdown chuẩn đẹp mắt:\n\n{request.RawContent}" }
            },
            temperature = 0.2
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            using var response = await client.SendAsync(httpRequest, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(ct);
                logger.LogError("AI provider returned error {StatusCode}: {ErrorText}", response.StatusCode, errorText);
                return StatusCode((int)response.StatusCode, new { message = $"Lỗi kết nối nhà cung cấp AI ({response.StatusCode})" });
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseJson);

            var content = string.Empty;
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var textEl))
                {
                    content = textEl.GetString() ?? string.Empty;
                }
            }

            var formatted = content.Trim();
            if (formatted.StartsWith("```markdown") && formatted.EndsWith("```"))
            {
                formatted = formatted[11..^3].Trim();
            }
            else if (formatted.StartsWith("```") && formatted.EndsWith("```"))
            {
                formatted = formatted[3..^3].Trim();
            }

            return Ok(new { formatted });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call AI service");
            return StatusCode(500, new { message = $"Lỗi xử lý AI: {ex.Message}" });
        }
    }

    [HttpPost("explain-step")]
    public async Task<ActionResult<object>> ExplainStep([FromBody] ExplainStepRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.UserQuestion))
        {
            return BadRequest(new { message = "Câu hỏi không được để trống." });
        }

        var endpoint = configuration["DSA:Ai:Endpoint"] ?? DefaultEndpoint;
        var model = configuration["DSA:Ai:Model"] ?? DefaultModel;
        var apiKey = configuration["DSA:Ai:ApiKey"] ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("AI ApiKey is not configured in DSA:Ai:ApiKey.");
            return StatusCode(500, new { message = "Dịch vụ AI chưa được cấu hình khóa API phía máy chủ." });
        }

        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(30);

        var explanationStr = request.Explanation.HasValue ? request.Explanation.Value.ToString() : string.Empty;
        var variablesStr = request.Variables.HasValue ? request.Variables.Value.ToString() : string.Empty;
        var pseudocodeLineStr = request.PseudocodeLine.HasValue ? request.PseudocodeLine.Value.ToString() : string.Empty;

        var contextInfo = $"Thuật toán: {request.AlgorithmTitle}\nBước thực thi: #{request.StepIndex + 1}\nGiải thích bước: {explanationStr}\nBiến cục bộ: {variablesStr}\nMã giả dòng: {pseudocodeLineStr}";

        var payload = new
        {
            model,
            messages = new object[]
            {
                new { role = "system", content = TutorSystemPrompt },
                new { role = "user", content = $"Ngữ cảnh bước chạy hiện tại:\n{contextInfo}\n\nCâu hỏi của sinh viên: {request.UserQuestion}" }
            },
            temperature = 0.3
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        try
        {
            using var response = await client.SendAsync(httpRequest, ct);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync(ct);
                logger.LogError("AI provider returned error {StatusCode}: {ErrorText}", response.StatusCode, errorText);
                return StatusCode((int)response.StatusCode, new { message = $"Lỗi kết nối AI Tutor ({response.StatusCode})" });
            }

            var responseJson = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(responseJson);

            var content = string.Empty;
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];
                if (firstChoice.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var textEl))
                {
                    content = textEl.GetString() ?? string.Empty;
                }
            }

            return Ok(new { reply = content.Trim() });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to call AI Tutor service");
            return StatusCode(500, new { message = $"Lỗi AI Tutor: {ex.Message}" });
        }
    }
}
