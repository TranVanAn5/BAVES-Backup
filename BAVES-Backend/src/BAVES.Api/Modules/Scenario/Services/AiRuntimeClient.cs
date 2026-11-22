using BAVES.Api.Modules.Assessment.Models.DTOs;
using System.Net.Http.Json;
using System.Text.Json;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Requests;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BAVES.Api.Modules.Scenario.Services;

/// <summary>
/// Client for communicating with the AI Runtime (FastAPI) service.
/// Handles validation, stakeholder response, coach feedback, assessment, and analysis confirmation calls.
/// </summary>
public sealed class AiRuntimeClient : IAiRuntimeClient
{
    private readonly HttpClient _httpClient;
    private readonly string _internalToken;

    /// <summary>
    /// Creates a new client.
    /// Timeout is set to 300 seconds.
    /// </summary>
    public AiRuntimeClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpClient.Timeout = TimeSpan.FromSeconds(300);
        _internalToken = configuration["AiRuntime:InternalToken"] ?? "baves-internal-secret-token";
    }

    public async Task<ValidateAiConnectionDto> ValidateConnectionAsync(
        ValidateAiConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var provider = string.IsNullOrWhiteSpace(request.Provider) ? "OpenAI" : request.Provider.Trim();
        var model = request.Model?.Trim();
        var apiKey = request.ApiKey?.Trim() ?? string.Empty;

        var payload = new
        {
            provider,
            model,
            apiKey
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/connections/validate")
        {
            Content = JsonContent.Create(payload)
        };
        httpRequest.Headers.Add("X-Internal-Token", _internalToken);

        try
        {
            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new ValidateAiConnectionDto(
                    false,
                    provider,
                    model,
                    $"AI Runtime returned status {(int)response.StatusCode}."
                );
            }

            var runtimeResult = await response.Content.ReadFromJsonAsync<RuntimeValidateResponse>(cancellationToken: cancellationToken);
            return new ValidateAiConnectionDto(
                runtimeResult?.Valid ?? false,
                runtimeResult?.Provider ?? provider,
                model,
                runtimeResult?.Message ?? "Validation completed."
            );
        }
        catch (HttpRequestException ex)
        {
            return new ValidateAiConnectionDto(
                false,
                provider,
                model,
                $"Could not connect to AI Runtime: {ex.Message}"
            );
        }
    }

    private const int MaxRetries = 2;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(3);

    public async Task<StakeholderAgentResponse> RespondAsync(
        StakeholderAgentRequest request,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/stakeholder/respond")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-Internal-Token", _internalToken);

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var code = TryReadErrorCode(errorBody);
                    var message = TryReadErrorMessage(errorBody);

                    var statusCode = (int)response.StatusCode;
                    if (IsTransientStatusCode(statusCode) && attempt <= MaxRetries)
                    {
                        lastException = MapToAiRuntimeException(code, message, statusCode);
                        await Task.Delay(RetryDelay, cancellationToken);
                        continue;
                    }

                    throw MapToAiRuntimeException(code, message, statusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<StakeholderAgentResponse>(
                    cancellationToken: cancellationToken);
                if (result is null)
                {
                    throw new AiRuntimeException(
                        "AI Runtime returned an empty response.",
                        "InvalidOutput",
                        StatusCodes.Status502BadGateway);
                }

                return result;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new AiRuntimeException(
                    "AI Runtime did not respond in time. Please try again.",
                    "Timeout",
                    StatusCodes.Status503ServiceUnavailable);
            }
            catch (HttpRequestException exception)
            {
                if (attempt <= MaxRetries)
                {
                    lastException = exception;
                    await Task.Delay(RetryDelay, cancellationToken);
                    continue;
                }

                throw new AiRuntimeException(
                    $"Could not connect to AI Runtime: {exception.Message}",
                    "Unavailable",
                    StatusCodes.Status503ServiceUnavailable);
            }
        }

        throw lastException as AiRuntimeException
              ?? new AiRuntimeException("AI Runtime request failed after retries.", "Unavailable",
                  StatusCodes.Status503ServiceUnavailable);
    }

    /// <summary>
    /// Gọi AI Runtime để lấy nhận xét của coach về kỹ năng khơi gợi yêu cầu.
    /// Dùng chung chiến lược retry + map mã lỗi với <see cref="RespondAsync"/>.
    /// </summary>
    public async Task<CoachFeedbackResponse> GetCoachFeedbackAsync(
        CoachFeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/coach/feedback")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-Internal-Token", _internalToken);

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var code = TryReadErrorCode(errorBody);
                    var message = TryReadErrorMessage(errorBody);
                    var statusCode = (int)response.StatusCode;

                    if (IsTransientStatusCode(statusCode) && attempt <= MaxRetries)
                    {
                        lastException = MapToAiRuntimeException(code, message, statusCode);
                        await Task.Delay(RetryDelay, cancellationToken);
                        continue;
                    }

                    throw MapToAiRuntimeException(code, message, statusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<CoachFeedbackResponse>(
                    cancellationToken: cancellationToken);
                if (result is null)
                {
                    throw new AiRuntimeException(
                        "AI Runtime returned an empty response.",
                        "InvalidOutput",
                        StatusCodes.Status502BadGateway);
                }

                return result;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new AiRuntimeException(
                    "AI Runtime did not respond in time. Please try again.",
                    "Timeout",
                    StatusCodes.Status503ServiceUnavailable);
            }
            catch (HttpRequestException exception)
            {
                if (attempt <= MaxRetries)
                {
                    lastException = exception;
                    await Task.Delay(RetryDelay, cancellationToken);
                    continue;
                }

                throw new AiRuntimeException(
                    $"Could not connect to AI Runtime: {exception.Message}",
                    "Unavailable",
                    StatusCodes.Status503ServiceUnavailable);
            }
        }

        throw lastException as AiRuntimeException
              ?? new AiRuntimeException("AI Runtime request failed after retries.", "Unavailable",
                  StatusCodes.Status503ServiceUnavailable);
    }

    /// <summary>
    /// Gửi bài nộp sang AI Assessor Runtime để chấm điểm theo Rubric BABOK.
    /// Gọi POST /v1/assessment/evaluate với header X-Internal-Token.
    /// </summary>
    public async Task<AssessmentRuntimeResponse> EvaluateAssessmentAsync(
        AssessmentRuntimeRequest request,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/assessment/evaluate")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-Internal-Token", _internalToken);

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var code = TryReadErrorCode(errorBody);
                    var message = TryReadErrorMessage(errorBody);
                    var statusCode = (int)response.StatusCode;

                    if (IsTransientStatusCode(statusCode) && attempt <= MaxRetries)
                    {
                        lastException = MapToAiRuntimeException(code, message, statusCode);
                        await Task.Delay(RetryDelay, cancellationToken);
                        continue;
                    }

                    throw MapToAiRuntimeException(code, message, statusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AssessmentRuntimeResponse>(
                    cancellationToken: cancellationToken);
                if (result is null)
                {
                    throw new AiRuntimeException(
                        "AI Runtime returned an empty response.",
                        "InvalidOutput",
                        StatusCodes.Status502BadGateway);
                }

                return result;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new AiRuntimeException(
                    "AI Runtime did not respond in time. Please try again.",
                    "Timeout",
                    StatusCodes.Status503ServiceUnavailable);
            }
            catch (HttpRequestException exception)
            {
                if (attempt <= MaxRetries)
                {
                    lastException = exception;
                    await Task.Delay(RetryDelay, cancellationToken);
                    continue;
                }

                throw new AiRuntimeException(
                    $"Could not connect to AI Runtime: {exception.Message}",
                    "Unavailable",
                    StatusCodes.Status503ServiceUnavailable);
            }
        }

        throw lastException as AiRuntimeException
              ?? new AiRuntimeException("AI Runtime request failed after retries.", "Unavailable",
                  StatusCodes.Status503ServiceUnavailable);
    }

    /// <summary>
    /// Gửi một mục phân tích (AnalysisItem) kèm bằng chứng sang AI Stakeholder để xác nhận.
    /// Gọi POST /v1/analysis/confirm với header X-Internal-Token.
    /// </summary>
    public async Task<AnalysisConfirmRuntimeResponse> ConfirmAnalysisItemAsync(
        AnalysisConfirmRuntimeRequest request,
        CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= MaxRetries + 1; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/analysis/confirm")
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-Internal-Token", _internalToken);

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var code = TryReadErrorCode(errorBody);
                    var message = TryReadErrorMessage(errorBody);
                    var statusCode = (int)response.StatusCode;

                    if (IsTransientStatusCode(statusCode) && attempt <= MaxRetries)
                    {
                        lastException = MapToAiRuntimeException(code, message, statusCode);
                        await Task.Delay(RetryDelay, cancellationToken);
                        continue;
                    }

                    throw MapToAiRuntimeException(code, message, statusCode);
                }

                var result = await response.Content.ReadFromJsonAsync<AnalysisConfirmRuntimeResponse>(
                    cancellationToken: cancellationToken);
                if (result is null)
                {
                    throw new AiRuntimeException(
                        "AI Runtime returned an empty response.",
                        "InvalidOutput",
                        StatusCodes.Status502BadGateway);
                }

                return result;
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new AiRuntimeException(
                    "AI Runtime did not respond in time. Please try again.",
                    "Timeout",
                    StatusCodes.Status503ServiceUnavailable);
            }
            catch (HttpRequestException exception)
            {
                if (attempt <= MaxRetries)
                {
                    lastException = exception;
                    await Task.Delay(RetryDelay, cancellationToken);
                    continue;
                }

                throw new AiRuntimeException(
                    $"Could not connect to AI Runtime: {exception.Message}",
                    "Unavailable",
                    StatusCodes.Status503ServiceUnavailable);
            }
        }

        throw lastException as AiRuntimeException
              ?? new AiRuntimeException("AI Runtime request failed after retries.", "Unavailable",
                  StatusCodes.Status503ServiceUnavailable);
    }

    public async Task<SprintReviewRuntimeResponse> ReviewSprintAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/stakeholder/sprint-review")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add("X-Internal-Token", _internalToken);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new AiRuntimeException(
                $"AI Runtime sprint review failed with status {(int)response.StatusCode}. {errorBody}");
        }

        var result = await response.Content.ReadFromJsonAsync<SprintReviewRuntimeResponse>(cancellationToken: cancellationToken);
        return result ?? throw new AiRuntimeException("AI Runtime returned an empty sprint review.");
    }

    private static bool IsTransientStatusCode(int statusCode) =>
        statusCode is 503 or 502 or 504 or 429;

    private static string? TryReadErrorCode(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? TryReadErrorMessage(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("message", out var message) ? message.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static AiRuntimeException MapToAiRuntimeException(string? code, string? message, int httpStatusCode)
    {
        var detail = message ?? "AI Runtime request failed.";
        return code switch
        {
            "InvalidApiKey" => new AiRuntimeException(detail, "InvalidApiKey", StatusCodes.Status422UnprocessableEntity),
            "RateLimited" => new AiRuntimeException(detail, "RateLimited", StatusCodes.Status429TooManyRequests),
            "Timeout" => new AiRuntimeException(detail, "Timeout", StatusCodes.Status503ServiceUnavailable),
            "InvalidOutput" => new AiRuntimeException(detail, "InvalidOutput", StatusCodes.Status502BadGateway),
            _ when httpStatusCode >= 500 => new AiRuntimeException(detail, "Unavailable", StatusCodes.Status503ServiceUnavailable),
            _ => new AiRuntimeException(detail, code ?? "Error", httpStatusCode)
        };
    }

    private sealed class RuntimeValidateResponse
    {
        public bool Valid { get; set; }
        public string? Provider { get; set; }
        public string? Message { get; set; }
    }
}
