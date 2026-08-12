using Asp.Versioning;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Công khai — API_REFERENCE.md §4.2: site-info/faqs. KHÔNG có endpoint sinh bước trên server
/// (POST /simulations/run đã cắt — A-4, ADR-001: chạy phía client).
/// </summary>
[ApiVersion("1.0")]
[Route("api/v1/public")]
[AllowAnonymous]
public class PublicController(
    AppDbContext db,
    ISimulationCatalogService catalog) : ApiControllerBase
{
    /// <summary>Số liệu trang chủ (số CTDL/GT/bài học) — dữ liệu công khai.</summary>
    [HttpGet("site-info")]
    public async Task<ActionResult<SiteInfoDto>> GetSiteInfo(CancellationToken ct)
    {
        var list = await catalog.GetListAsync(ct);
        var structures = list.IsSuccess ? list.Value!.Count(s => s.Category == "structure") : 0;
        var algorithms = list.IsSuccess ? list.Value!.Count(s => s.Category == "algorithm") : 0;
        var lessons = await db.Lessons.AsNoTracking()
            .CountAsync(l => l.DeletedAt == null && l.Status == DsaVisual.Application.Persistence.Entities.LessonStatus.Active, ct);

        return Ok(new SiteInfoDto { Structures = structures, Algorithms = algorithms, Lessons = lessons });
    }

    /// <summary>FAQ tĩnh — dữ liệu công khai.</summary>
    [HttpGet("faqs")]
    public ActionResult<List<FaqDto>> GetFaqs() => Ok(new[]
    {
        new FaqDto
        {
            Question = "DSA Visual là gì?",
            Answer = "Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật: mô phỏng từng bước, bài tập tương tác, lộ trình học."
        },
        new FaqDto
        {
            Question = "Tôi cần tài khoản để làm gì?",
            Answer = "Để lưu tiến độ, làm bài tập, tham gia lớp học và nhận phần thưởng (gems, quest, huy hiệu)."
        },
        new FaqDto
        {
            Question = "Mô phỏng chạy ở đâu?",
            Answer = "Toàn bộ mô phỏng chạy phía trình duyệt (client) — máy chủ chỉ lưu kết quả, không thực thi code."
        },
        new FaqDto
        {
            Question = "Tim (hearts) hồi như thế nào?",
            Answer = "Miễn phí: 10 tim, hồi 1 tim mỗi 30 phút. Premium: 30 tim, hồi 1 tim mỗi 10 phút."
        }
    });
}
