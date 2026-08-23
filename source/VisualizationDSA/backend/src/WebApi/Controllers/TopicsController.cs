using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asp.Versioning;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    /// <summary>Màn 22 + ladder — GET /api/v1/topics (FE lessons.ts fetchTopics).</summary>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/topics")]
    public class TopicsController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;
        public TopicsController(IApplicationDbContext ctx) { _ctx = ctx; }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var courses = await _ctx.Courses.AsNoTracking().Where(c => !c.IsDeleted && c.IsPublished).OrderBy(c => c.CreatedAt).ToListAsync();
            var result = new List<object>();
            var idx = 1;
            foreach (var c in courses)
            {
                result.Add(new { id = idx++, parentId = (object?)null, name = c.Title, description = c.Description, sortOrder = idx, children = new List<object>() });
            }
            return Ok(result);
        }
    }
}
