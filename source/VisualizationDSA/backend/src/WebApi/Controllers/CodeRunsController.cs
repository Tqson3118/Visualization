using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 18 — /api/v1/code-runs lưu vết lần chạy code (ADR-012). In-memory cho demo.</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/code-runs")]
    [RequireJwtRole]
    public class CodeRunsController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, object> Store = new();

        [HttpPost]
        public IActionResult Save([FromBody] SaveCodeRunRequest? req)
        {
            var id = Guid.NewGuid().ToString();
            var run = new
            {
                id,
                exerciseId = req?.ExerciseId,
                status = "passed",
                passed = 1,
                total = 1,
                createdAt = DateTime.UtcNow.ToString("o"),
            };
            Store[id] = run;
            return Created(string.Empty, run);
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
            => Store.TryGetValue(id, out var run) ? Ok(run) : NotFound();

        [HttpGet("{id}/trace")]
        public IActionResult Trace(string id)
            => Store.TryGetValue(id, out var run) ? Ok(new List<object>()) : NotFound();
    }

    public class SaveCodeRunRequest
    {
        public string? ExerciseId { get; set; }
        public string? Key { get; set; }
        public string? Code { get; set; }
        public string? Input { get; set; }
        public string? Status { get; set; }
        public int? DurationMs { get; set; }
        public object? Stats { get; set; }
        public string? Output { get; set; }
        public string? Error { get; set; }
        public object? Trace { get; set; }
    }
}
