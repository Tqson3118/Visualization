using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Base controller: helper CurrentUserId()/CurrentRole() (SDD §5.7.1) — đọc từ JWT claims.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected int CurrentUserId() => int.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)!.Value);

    protected string CurrentRole() => User.FindFirst(ClaimTypes.Role)!.Value;
}
