using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BAVES.Api.Modules.Simulation.Controllers;

/// <summary>
/// AI Coach feedback của một Simulation Session.
///
/// Theo kế hoạch API (mục 6.4), cả hai endpoint thuộc quyền "L" — chỉ Learner,
/// và chỉ trên session của chính họ.
///
/// Luồng: POST xin coach nhận xét và LƯU lại; GET đọc nhận xét đã lưu,
/// KHÔNG gọi lại LLM (tránh tốn token và bảo đảm nội dung learner đã thấy không đổi).
/// </summary>
[ApiController]
[Route("api/sessions")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Policy = Policies.LearnerOnly)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SessionCoachFeedbackController(
    ICoachFeedbackService coachFeedbackService) : ControllerBase
{
    /// <summary>
    /// [Endpoint] POST /api/sessions/{sessionId}/coach-feedback
    /// Xin AI Coach nhận xét cho cửa sổ hội thoại hiện tại của session.
    ///
    /// Luồng xử lý:
    ///   1. Kiểm tra session thuộc learner đang đăng nhập (nếu không → 404).
    ///   2. Kiểm tra session còn Active (nếu không → 409).
    ///   3. Lấy 20 tin nhắn gần nhất của session + BA Plan (nếu có).
    ///   4. Gọi AI Runtime POST /v1/coach/feedback.
    ///   5. Lưu kết quả và trả 201 Created.
    ///
    /// Thông tin kết nối AI (BYOK) đọc từ header X-AI-Provider / X-AI-Model / X-AI-Key.
    /// API key chỉ được chuyển tiếp cho AI Runtime, không lưu DB và không ghi log.
    /// </summary>
    [HttpPost("{sessionId:int}/coach-feedback")]
    [ProducesResponseType<CoachFeedbackDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<CoachFeedbackDto>> RequestCoachFeedback(
        int sessionId,
        [FromHeader(Name = "X-AI-Provider")] string? headerProvider,
        [FromHeader(Name = "X-AI-Model")] string? headerModel,
        [FromHeader(Name = "X-AI-Key")] string? headerApiKey,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        // BYOK: mặc định provider OpenAI nếu client không gửi header,
        // đồng bộ với cách SendAgentMessage đang xử lý.
        var provider = !string.IsNullOrWhiteSpace(headerProvider) ? headerProvider.Trim() : "OpenAI";
        var model = !string.IsNullOrWhiteSpace(headerModel) ? headerModel.Trim() : null;
        var apiKey = !string.IsNullOrWhiteSpace(headerApiKey) ? headerApiKey.Trim() : null;

        var result = await coachFeedbackService.RequestCoachFeedbackAsync(
            learnerId,
            sessionId,
            new AgentAiConnection(provider, model, apiKey),
            cancellationToken);

        return Created($"/api/sessions/{sessionId}/coach-feedback", result);
    }

    /// <summary>
    /// [Endpoint] GET /api/sessions/{sessionId}/coach-feedback
    /// Đọc bản nhận xét MỚI NHẤT của coach đã lưu cho session. KHÔNG gọi LLM.
    ///
    /// Trả 404 nếu session không tồn tại/không thuộc learner, hoặc session chưa
    /// từng được coach nhận xét (client cần gọi POST trước).
    /// </summary>
    [HttpGet("{sessionId:int}/coach-feedback")]
    [ProducesResponseType<CoachFeedbackDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CoachFeedbackDto>> GetCoachFeedback(
        int sessionId,
        CancellationToken cancellationToken)
    {
        var learnerId = GetLearnerIdOrThrow();

        var result = await coachFeedbackService.GetLatestCoachFeedbackAsync(
            learnerId, sessionId, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Đọc user id từ claim "sub" của JWT.
    /// [Authorize] ở class đã bảo đảm token hợp lệ; vẫn kiểm tra định dạng
    /// để tránh lỗi parse lúc runtime.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">Claim "sub" thiếu hoặc không phải số nguyên.</exception>
    private int GetLearnerIdOrThrow()
    {
        if (!int.TryParse(
                User.FindFirstValue(JwtRegisteredClaimNames.Sub),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        return learnerId;
    }
}
