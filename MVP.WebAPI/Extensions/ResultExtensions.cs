using Microsoft.AspNetCore.Mvc;
using MVP.Application.Interfaces;

namespace MVP.WebAPI.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this ApplicationResult<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Data);
        }

        return MapErrorToActionResult(result.ErrorType, result.ErrorMessage);
    }

    public static IActionResult ToActionResult(this ApplicationResult result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return MapErrorToActionResult(result.ErrorType, result.ErrorMessage);
    }

    private static IActionResult MapErrorToActionResult(ErrorType type, string? message)
    {
        var errorResponse = new { Error = message ?? "An error occurred during the operation." };

        return type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(errorResponse),
            ErrorType.Validation => new BadRequestObjectResult(errorResponse),
            ErrorType.Conflict => new ConflictObjectResult(errorResponse),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(errorResponse),
            _ => new BadRequestObjectResult(errorResponse)
        };
    }
}
