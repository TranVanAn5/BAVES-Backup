using BAVES.Api.Modules.Commerce.Services.Interfaces;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Collections.Generic;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Models.Requests;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Authorization;
using BAVES.Api.Shared.Pagination;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BAVES.Api.Modules.Simulation.Controllers;

[ApiController]
[Route("api/sessions")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "LEARNER,INSTRUCTOR,ADMIN"
)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class SessionsController(
    ISimulationSessionService sessionService,
    IEntitlementService entitlementService) : ControllerBase
{
    /// <summary>
    /// [Endpoint 20] Bắt đầu một Simulation Session mới cho Learner.
    /// Body yêu cầu: { "scenarioVersionId": int }.
    /// Phiên chỉ được tạo khi:
    ///   - ScenarioVersion tồn tại và đã được publish.
    ///   - Version có ít nhất 1 stakeholder persona active.
    /// Khi không hợp lệ, trả 404 (không tồn tại) hoặc 409 (xung đột nghiệp vụ).
    /// </summary>
    [HttpPost]
    [ProducesResponseType<SessionDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> StartSession(
        [FromBody] StartSessionRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        // Kiểm tra quyền truy cập kịch bản mô phỏng (Entitlement Gatekeeper)
        if (!IsInstructorOrAdmin)
        {
            var hasAccess = await entitlementService.CanStartSessionAsync(learnerId, request.ScenarioVersionId, cancellationToken);
            if (!hasAccess)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    code = "PAYMENT_REQUIRED",
                    message = "Bạn chưa mua gói hoặc đã hết số lượt thực hành cho kịch bản này. Vui lòng đăng ký gói mô phỏng.",
                    scenarioVersionId = request.ScenarioVersionId
                });
            }
        }

        var result = await sessionService.StartSessionAsync(learnerId, request, cancellationToken);

        // Ghi nhận đã sử dụng 1 lượt phiên
        await entitlementService.RecordSessionUsedAsync(learnerId, request.ScenarioVersionId, cancellationToken);

        // 201 Created + Location header trỏ về resource
        return CreatedAtAction(
            actionName: nameof(GetSessionById),  // sẽ implement ở bước GET /api/sessions/{id}
            routeValues: new { id = result.Id },
            value: result
        );
    }
    /// <summary>
    /// [Endpoint 21] Lấy thông tin chi tiết một Simulation Session của learner.
    /// Session phải thuộc về learner đang đăng nhập; nếu không sẽ trả 404.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionDto>> GetSessionById(
    int id,
    CancellationToken cancellationToken
)
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }
        var session = await sessionService.GetSessionByIdAsync(learnerId, id, cancellationToken);
        if (session is null)
        {
            return NotFound();
        }
        return Ok(session);
    }
    ///<summary>
    ///[Endpoint 22] Lấy danh sách stakeholder persona (agent) của một session.
    /// Session phải thuộc về learner đang đăng nhập; nếu không sẽ trả 404.
    /// Trả về danh sách rỗng nếu session hợp lệ nhưng chưa có agent nào.
    /// </summary>
    [HttpGet("{id:int}/agents")]
    [ProducesResponseType<IReadOnlyList<SessionAgentDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<SessionAgentDto>>> GetSessionAgents(
        int id,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken
        )
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
                CultureInfo.InvariantCulture, out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }
        var agents = await sessionService.GetSessionAgentsAsync(learnerId, id, cancellationToken);
        if (agents is null)
        {
            return NotFound();
        }
        return this.OkPaged(agents, page, pageSize);
    }

    /// <summary>
    /// [Endpoint 23] Gửi tin nhắn của learner tới một stakeholder agent và nhận về
    /// câu trả lời của AI stakeholder.
    ///
    /// Luồng xử lý:
    ///   1. Kiểm tra session thuộc learner và còn trạng thái Active (nếu không → 404 / 409).
    ///   2. Kiểm tra agent thuộc chính session đó (nếu không → 404).
    ///   3. Nạp persona + allowed knowledge (đã lọc theo StakeholderKnowledgeAccess).
    ///   4. Gọi AI Runtime (POST /v1/stakeholder/respond) để lấy câu trả lời.
    ///   5. Lưu tin nhắn learner + tin nhắn stakeholder, cập nhật trạng thái agent.
    ///
    /// Thông tin kết nối AI (BYOK) đọc từ header X-AI-Provider / X-AI-Model / X-AI-Key.
    /// API key chỉ được chuyển tiếp cho AI Runtime, không lưu DB và không ghi log.
    /// </summary>
    [HttpPost("{sessionId:int}/agents/{agentId:int}/messages")]
    [ProducesResponseType<SendAgentMessageResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SendAgentMessageResultDto>> SendAgentMessage(
        int sessionId,
        int agentId,
        [FromBody] SendAgentMessageRequest request,
        [FromHeader(Name = "X-AI-Provider")] string? headerProvider,
        [FromHeader(Name = "X-AI-Model")] string? headerModel,
        [FromHeader(Name = "X-AI-Key")] string? headerApiKey,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        // BYOK: ưu tiên lấy từ Headers theo đúng chuẩn BYOK đã cam kết, nếu không có thì để null.
        var provider = !string.IsNullOrWhiteSpace(headerProvider) ? headerProvider.Trim() : "OpenAI";
        var model = !string.IsNullOrWhiteSpace(headerModel) ? headerModel.Trim() : null;
        var apiKey = !string.IsNullOrWhiteSpace(headerApiKey) ? headerApiKey.Trim() : null;

        var result = await sessionService.SendAgentMessageAsync(
            learnerId,
            sessionId,
            agentId,
            request,
            new AgentAiConnection(provider, model, apiKey),
            cancellationToken);

        return Created(
            $"/api/sessions/{sessionId}/agents/{agentId}/messages",
            result);
    }

    /// <summary>
    /// [Endpoint 24] Lấy transcript của một stakeholder agent trong session.
    /// Session phải thuộc về learner đang đăng nhập; agent phải thuộc chính session đó.
    /// Phân trang theo ?page= và ?pageSize= (mặc định 1 và 20).
    /// </summary>
    [HttpGet("{sessionId:int}/agents/{agentId:int}/messages")]
    [ProducesResponseType<AgentMessagePageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgentMessagePageDto>> GetAgentMessages(
        int sessionId,
        int agentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        // Chặn giá trị phân trang không hợp lệ trước khi tính skip (tránh lỗi SQL).
        var safePage = page < 1 ? 1 : page;
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var result = await sessionService.GetAgentMessagesAsync(
            learnerId, sessionId, agentId, safePage, safePageSize, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    /// <summary>
    /// [Endpoint 25] Lấy danh sách event đã xảy ra trong session, có phân trang.
    ///
    /// Mỗi bản ghi là một "sự kiện" của kịch bản đã được kích hoạt, kèm
    /// EventContentSnapshot đóng băng nội dung tại thời điểm trigger.
    /// Session phải thuộc learner đang đăng nhập; nếu không sẽ trả 404.
    /// Phân trang theo ?page= và ?pageSize= (mặc định 1 và 20).
    /// </summary>
    [HttpGet("{sessionId:int}/events")]
    [ProducesResponseType<SessionEventPageDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionEventPageDto>> GetSessionEvents(
        int sessionId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        // Chặn giá trị phân trang không hợp lệ trước khi tính skip (tránh lỗi SQL).
        var safePage = page < 1 ? 1 : page;
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        var result = await sessionService.GetSessionEventsAsync(
            learnerId, sessionId, safePage, safePageSize, IsInstructorOrAdmin, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    /// <summary>
    /// [Endpoint 26] Lấy danh mục event của kịch bản mà session đang dùng,
    /// kèm trạng thái đã trigger hay chưa.
    ///
    /// Dùng để client biết event nào còn có thể kích hoạt (hiển thị nút Trigger).
    /// Session phải thuộc learner đang đăng nhập; nếu không sẽ trả 404.
    /// </summary>
    [HttpGet("{sessionId:int}/event-catalog")]
    [ProducesResponseType<IReadOnlyList<ScenarioEventAvailabilityDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ScenarioEventAvailabilityDto>>> GetSessionEventCatalog(
        int sessionId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await sessionService.GetSessionEventCatalogAsync(
            learnerId, sessionId, IsInstructorOrAdmin, cancellationToken);

        if (result is null)
        {
            return NotFound();
        }
        return this.OkPaged(result, page, pageSize);
    }

    /// <summary>
    /// [Endpoint 27] Trigger một event của kịch bản trong session hiện tại.
    ///
    /// Theo kế hoạch API (mục 6.4), endpoint này thuộc quyền "I" — Instructor có quyền
    /// quản lý phiên. Learner KHÔNG được tự trigger event; event đến với learner qua
    /// luồng tự động (MessageCount / TimeElapsed / Condition) hoặc do giảng viên kích hoạt.
    ///
    /// Luồng xử lý:
    ///   1. Kiểm tra session tồn tại (nếu không → 404).
    ///   2. Kiểm tra session còn Active (nếu không → 409).
    ///   3. Kiểm tra event gốc thuộc đúng ScenarioVersion của session (nếu không → 404).
    ///   4. Chỉ cho phép trigger thủ công event có TriggerType = Manual (nếu không → 409).
    ///   5. Mỗi event chỉ ghi nhận một lần trong session; trigger lại trả 200 với
    ///      AlreadyTriggered = true thay vì tạo bản ghi trùng.
    /// </summary>
    [HttpPost("{sessionId:int}/events/{eventId:int}/trigger")]
    // Endpoint thuộc quyềnInstructor/Admin.
    [Authorize(Policy = Policies.InstructorOrAdmin)]
    [ProducesResponseType<TriggerSessionEventResultDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TriggerSessionEventResultDto>> TriggerSessionEvent(
        int sessionId,
        int eventId,
        CancellationToken cancellationToken)
    {
        var result = await sessionService.TriggerSessionEventAsync(
            sessionId, eventId, cancellationToken);

        // Lần đầu trigger → 201 Created kèm Location tới danh sách event.
        // Trigger lại (idempotent) → 200 OK, không tạo tài nguyên mới.
        if (result.AlreadyTriggered)
        {
            return Ok(result);
        }

        return Created($"/api/sessions/{sessionId}/events", result);
    }

    /// <summary>
    /// true nếu người gọi là Instructor hoặc Admin.
    /// Dùng để quyết định TriggerSource và có được phép thao tác trên session
    /// không thuộc sở hữu của mình hay không.
    /// </summary>
    /// <summary>
    /// Lấy danh sách toàn bộ lịch sử các phiên mô phỏng của Learner (hỗ trợ lọc theo status, scenarioVersionId, assignmentId).
    /// </summary>
    [HttpGet]
    [HttpGet("/api/me/sessions")]
    [ProducesResponseType<IReadOnlyList<SessionHistoryItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<SessionHistoryItemDto>>> GetMySessions(
        [FromQuery] SessionStatus? status,
        [FromQuery] int? scenarioVersionId,
        [FromQuery] int? assignmentId,
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await sessionService.GetLearnerSessionsHistoryAsync(
            learnerId, status, scenarioVersionId, assignmentId, cancellationToken);
        return this.OkPaged(result, page, pageSize);
    }

    /// <summary>
    /// Đánh dấu hoàn thành (Complete) một phiên mô phỏng của Learner.
    /// Chuyển trạng thái từ Active hoặc Submitted sang Completed.
    /// </summary>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> CompleteSession(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await sessionService.CompleteSessionAsync(learnerId, id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Chủ động hủy bỏ / dừng giữa chừng (Abandon) một phiên mô phỏng đang Active của Learner.
    /// Không cho phép abandon phiên đã Submitted hoặc Completed.
    /// </summary>
    [HttpPost("{id:int}/abandon")]
    [ProducesResponseType<SessionDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionDto>> AbandonSession(
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var learnerId))
        {
            throw new UnauthorizedAccessException();
        }

        var result = await sessionService.AbandonSessionAsync(learnerId, id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    private bool IsInstructorOrAdmin => User.IsInRole("INSTRUCTOR") || User.IsInRole("ADMIN");

    /// <summary>
    /// Đọc user id từ claim "sub" của JWT.
    /// Tách ra helper để tránh lặp lại khối parse ở mỗi action.
    /// </summary>
    private bool TryGetCurrentUserId(out int userId)
    {
        return int.TryParse(
            User.FindFirstValue(JwtRegisteredClaimNames.Sub),
            NumberStyles.None,
            CultureInfo.InvariantCulture,
            out userId);
    }
}
