using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VisualizationDSA.Application.Common.Interfaces;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/roadmaps")]
    public class RoadmapController : ControllerBase
    {
        private readonly IRoadmapReviewService _reviewService;

        public RoadmapController(IRoadmapReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        private Guid? GetUserId()
        {
            var userIdStr = JwtHelper.ExtractSubFromToken(Request);
            return userIdStr != null && Guid.TryParse(userIdStr, out var id) ? id : null;
        }

        // ── G3.9.3 — POST /api/v1/roadmaps/{id}/review [Student] ──
        // Đánh giá 1 lần duy nhất: gửi lại → 409 ALREADY_REVIEWED (KHÔNG update/upsert).
        [HttpPost("{id}/review")]
        [Authorize]
        public async Task<IActionResult> CreateReview(Guid id, [FromBody] CreateRoadmapReviewDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var result = await _reviewService.SubmitReviewAsync(userId.Value, id, dto.Rating);
            if (result.IsSuccess)
            {
                return StatusCode(201, result.Review);
            }

            return result.ErrorCode switch
            {
                "INVALID_RATING" => BadRequest(new { error = result.ErrorCode, message = result.ErrorMessage }),
                "ROADMAP_NOT_FOUND" => NotFound(new { error = result.ErrorCode, message = result.ErrorMessage }),
                "ALREADY_REVIEWED" => Conflict(new { error = result.ErrorCode, message = result.ErrorMessage }),
                "ROADMAP_NOT_COMPLETED" => StatusCode(403, new { error = result.ErrorCode, message = result.ErrorMessage }),
                _ => StatusCode(500, new { error = "INTERNAL_ERROR", message = "Đã xảy ra lỗi không xác định." })
            };
        }

        // ── G3.9.4 — GET /api/v1/roadmaps/{id}/stats [public] ──
        // Không token → myRating = null, myCanReview = false.
        [HttpGet("{id}/stats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStats(Guid id)
        {
            var exists = await _reviewService.RoadmapExistsAsync(id);
            if (!exists) return NotFound(new { error = "ROADMAP_NOT_FOUND", message = "Không tìm thấy lộ trình." });

            var userId = GetUserId();
            var stats = await _reviewService.GetStatsAsync(id, userId);

            return Ok(new
            {
                enrollCount = stats.EnrollCount,
                completionCount = stats.CompletionCount,
                reviewCount = stats.ReviewCount,
                avgRating = stats.AvgRating,
                myRating = stats.MyRating,
                myCanReview = stats.MyCanReview
            });
        }
    }

    public class CreateRoadmapReviewDto
    {
        public int Rating { get; set; }
    }
}
