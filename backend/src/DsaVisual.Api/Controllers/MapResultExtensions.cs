using DsaVisual.Api.Dtos;
using DsaVisual.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace DsaVisual.Api.Controllers;

/// <summary>
/// Map Result&lt;T&gt; của Service → ActionResult chuẩn (SDD §5.3.2, §5.2).
/// Success → 200 + value; Fail → HTTP status theo ErrorCodes catalog + envelope { error }.
/// </summary>
public static class MapResultExtensions
{
    public static ActionResult MapResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
        {
            return controller.Ok(result.Value);
        }

        return ToErrorActionResult(controller, result.ErrorCode!, result.ErrorMessage!, result.FieldErrors);
    }

    public static ActionResult MapResult(this ControllerBase controller, Result result)
    {
        if (result.IsSuccess)
        {
            return controller.NoContent();
        }

        return ToErrorActionResult(controller, result.ErrorCode!, result.ErrorMessage!, result.FieldErrors);
    }

    private static ActionResult ToErrorActionResult(
        ControllerBase controller,
        string errorCode,
        string message,
        Dictionary<string, string[]>? fieldErrors)
    {
        var details = fieldErrors?
            .SelectMany(kv => kv.Value.Select(v => new ErrorDetailDto(kv.Key, v)))
            .ToList();

        var response = ErrorResponseDto.Create(
            errorCode,
            message,
            fieldErrors?.Keys.FirstOrDefault(),
            details);

        return new ObjectResult(response) { StatusCode = ErrorCodes.GetHttpStatus(errorCode) };
    }
}
