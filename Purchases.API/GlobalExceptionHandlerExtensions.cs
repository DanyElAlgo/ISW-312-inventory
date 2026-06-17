using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Purchases.API.Exceptions;

namespace Purchases.API;

/// <summary>
/// Translates unhandled exceptions into RFC-7807 <see cref="ProblemDetails"/> responses that
/// match Section 1.1 of the integration guide (camelCase fields: status, title, detail, instance,
/// traceId). The shared frontend reads <c>detail</c> (falling back to <c>title</c>) for its toast.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = GlobalExceptionHandlerExtensions.MapException(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        else
            _logger.LogWarning("Handled exception ({Status}): {Message}", status, exception.Message);

        httpContext.Response.StatusCode = status;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
            },
        });
    }
}

public static class GlobalExceptionHandlerExtensions
{
    /// <summary>Maps an exception to the agreed (status, title) per the guide's Section 1.1 table.</summary>
    public static (int Status, string Title) MapException(Exception exception) => exception switch
    {
        NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
        InvalidOperationException => (StatusCodes.Status400BadRequest, "Operacion invalida"),
        InventoryUnavailableException => (StatusCodes.Status503ServiceUnavailable, "Servicio no disponible"),
        _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor"),
    };
}
