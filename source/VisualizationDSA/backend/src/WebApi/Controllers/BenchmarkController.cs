using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Generic;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 19 — POST /api/v1/benchmarks/run lưu kết quả đo (client đo — ADR-012).</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/benchmarks")]
    public class BenchmarkController : ControllerBase
    {
        [HttpPost("run")]
        [RequireJwtRole]
        public IActionResult Run([FromBody] BenchmarkRequestStorage? body)
        {
            var keys = body?.Keys ?? new List<string>();
            var sizes = body?.Sizes ?? new List<int>();
            return Ok(new { keys, sizes, rows = new List<object>(), conclusion = (string?)null, measuredAt = DateTime.UtcNow.ToString("o") });
        }
    }

    public class BenchmarkRequestStorage
    {
        public List<string>? Keys { get; set; }
        public List<int>? Sizes { get; set; }
        public string? Language { get; set; }
    }
}
