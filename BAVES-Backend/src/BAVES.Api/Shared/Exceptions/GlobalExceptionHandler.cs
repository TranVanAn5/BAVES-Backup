using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Shared.Exceptions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Resource conflict"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            RequestValidationException => (StatusCodes.Status400BadRequest, "Invalid request"),
            AiRuntimeException aiError => (aiError.StatusCode, "AI Runtime error"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
        };

        if (statusCode >= 500)
            logger.LogError(exception, "Request failed with status code {StatusCode}", statusCode);
        else
            logger.LogWarning("Request failed with status code {StatusCode}: {Title}", statusCode, title);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        if (exception is RequestValidationException validation)
            problemDetails.Extensions["errors"] = validation.Errors;

        // Trả kèm mã lỗi chuẩn của AI Runtime (InvalidApiKey, RateLimited, Timeout...)
        // để client biết chính xác nguyên nhân thất bại.
        if (exception is AiRuntimeException aiRuntime)
            problemDetails.Extensions["errorCode"] = aiRuntime.ErrorCode;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.Headers.CacheControl = "no-store";
        if (statusCode == StatusCodes.Status401Unauthorized)
            httpContext.Response.Headers.WWWAuthenticate = "Bearer";
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
