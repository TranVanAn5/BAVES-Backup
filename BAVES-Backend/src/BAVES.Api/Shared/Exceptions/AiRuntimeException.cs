using Microsoft.AspNetCore.Http;

namespace BAVES.Api.Shared.Exceptions;

/// <summary>
/// Lỗi khi gọi AI Runtime (Python service) không thành công.
/// Mã lỗi quy chuẩn do runtime trả về, được map sang HTTP tương ứng:
///   - InvalidApiKey    → 422 Unprocessable Entity
///   - RateLimited      → 429 Too Many Requests
///   - Timeout/Unavailable → 503 Service Unavailable
///   - InvalidOutput    → 502 Bad Gateway
/// </summary>
public sealed class AiRuntimeException(
    string message,
    string errorCode = "Unavailable",
    int statusCode = StatusCodes.Status503ServiceUnavailable) : Exception(message)
{
    /// <summary>
    /// Mã lỗi chuẩn do AI runtime trả về (InvalidApiKey, RateLimited, Timeout, InvalidOutput).
    /// </summary>
    public string ErrorCode { get; } = errorCode;

    /// <summary>
    /// HTTP status code sẽ trả về cho client.
    /// </summary>
    public int StatusCode { get; } = statusCode;
}
