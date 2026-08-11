using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiController]
    [Route("api/v1/concepts/admin/users")]
    [RequireJwtRole("Teacher,Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly MediatR.IMediator _mediator;

        public AdminUsersController(MediatR.IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "")
        {
            var result = await _mediator.Send(new VisualizationDSA.Application.Features.Admin.Queries.GetUsers.GetUsersQuery 
            { 
                Page = page, 
                PageSize = pageSize, 
                Search = search 
            });

            return Ok(new
            {
                users = result.Items.Select(u => new
                {
                    id = u.Id.ToString(),
                    u.Email,
                    u.Username,
                    u.Role,
                    u.IsPremium,
                    u.TotalXP,
                    u.CurrentLevel,
                    u.StreakDays,
                    isActive = u.IsActive,
                    createdAt = u.CreatedAt,
                    lastLogin = u.LastLoginAt
                }),
                total = result.TotalCount,
                page,
                pageSize
            });
        }
    }
}
