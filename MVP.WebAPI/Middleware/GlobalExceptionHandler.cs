using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MVP.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MVP.WebAPI.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        if (exception is NotFoundException notFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            problemDetails.Title = "Recurso no encontrado";
            problemDetails.Status = StatusCodes.Status404NotFound;
            problemDetails.Detail = notFoundException.Message;
        }
        else if (exception is ValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Error de validación";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Detail = validationException.Message;
        }
        else if (exception is ArgumentException || exception is FormatException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Petición inválida";
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Detail = "Uno o más parámetros tienen un formato incorrecto.";
        }
        else
        {
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            problemDetails.Title = "Error interno del servidor";
            problemDetails.Status = StatusCodes.Status500InternalServerError;
            problemDetails.Detail = "Ocurrió un error inesperado al procesar la solicitud. El equipo técnico ha sido notificado.";
        }

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}
