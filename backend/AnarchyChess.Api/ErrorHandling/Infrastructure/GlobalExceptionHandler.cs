using AnarchyChess.Api.ErrorHandling.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Diagnostics;

namespace AnarchyChess.Api.ErrorHandling.Infrastructure;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        var error = Error.Failure(description: "Internal Server Error");
        await error.ToActionResult().ExecuteResultAsync(new() { HttpContext = httpContext });
        _logger.LogError(exception, "Error on path {Path}:", httpContext.Request.Path);
        return true;
    }
}
