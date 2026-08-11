using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIVisualizerController : ControllerBase
{
    private readonly IAIVisualizerService _aiService;

    public AIVisualizerController(IAIVisualizerService aiService)
    {
        _aiService = aiService;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateVisualization([FromBody] AIVisualizerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new { message = "Code is required" });
        }

        var result = await _aiService.GenerateVisualizationAsync(request);
        return Ok(result);
    }
}
