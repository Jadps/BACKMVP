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
        var problemDetails = new ProblemDetails
        {
            Detail = message ?? "An error occurred during the operation.",
            Title = GetTitleForErrorType(type),
            Status = GetStatusForErrorType(type)
        };

        return type switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(problemDetails),
            ErrorType.Validation => new BadRequestObjectResult(problemDetails),
            ErrorType.Conflict => new ConflictObjectResult(problemDetails),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(problemDetails),
            ErrorType.Forbidden => new ObjectResult(problemDetails) { StatusCode = StatusCodes.Status403Forbidden },
            _ => new BadRequestObjectResult(problemDetails)
        };
    }

    private static string GetTitleForErrorType(ErrorType type) => type switch
    {
        ErrorType.NotFound => "Resource Not Found",
        ErrorType.Validation => "Validation Error",
        ErrorType.Conflict => "Business Rule Conflict",
        ErrorType.Unauthorized => "Unauthorized Access",
        ErrorType.Forbidden => "Access Forbidden",
        _ => "Bad Request"
    };

    private static int GetStatusForErrorType(ErrorType type) => type switch
    {
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        _ => StatusCodes.Status400BadRequest
    };
}
