using System.Text.Json;
using BAVES.Api.Modules.Planning.Services.Interfaces;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Services.Interfaces;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Entities;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Repositories.Interfaces;
using BAVES.Api.Modules.Simulation.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;

namespace BAVES.Api.Modules.Simulation.Services;

/// <summary>
/// Nghiệp vụ AI Coach: xin nhận xét kỹ năng khơi gợi yêu cầu và đọc lại nhận xét đã lưu.
///
/// Phụ thuộc:
///   ISimulationSessionRepository — truy vấn session, transcript, bảng CoachFeedbacks.
///   IPlanningService             — lấy BA Plan của session (module Planning sở hữu).
///   IAiRuntimeClient             — gọi POST /v1/coach/feedback.
///
/// Không phụ thuộc ISimulationSessionService để tránh vòng DI, vì PlanningService
/// đã phụ thuộc ISimulationSessionService.
/// </summary>
public sealed class CoachFeedbackService(
    ISimulationSessionRepository simulationRepository,
    IPlanningService planningService,
    IAiRuntimeClient aiRuntimeClient
) : ICoachFeedbackService
{
    /// <summary>
    /// Số tin nhắn gần nhất của session được gửi làm cửa sổ hội thoại cho coach.
    /// Giới hạn để giữ prompt trong ngân sách token hợp lý.
    /// </summary>
    private const int ConversationWindowSize = 20;

    public async Task<CoachFeedbackDto> RequestCoachFeedbackAsync(
        int learnerId,
        int sessionId,
        AgentAiConnection aiConnection,
        CancellationToken cancellationToken = default)
    {
        // 1. Session phải tồn tại và thuộc learner → nếu không trả 404
        //    (không lộ sự tồn tại của session của learner khác).
        //    Dùng GetSessionWithAgentsAsync để lấy cả ownership + Status trong MỘT query,
        //    tránh phải kiểm tra tồn tại rồi truy vấn lại.
        var session = await simulationRepository.GetSessionWithAgentsAsync(
            learnerId, sessionId, cancellationToken);

        if (session is null)
        {
            throw new NotFoundException(
                $"Simulation session '{sessionId}' was not found.");
        }

        // 2. Chỉ session đang Active mới được xin nhận xét.
        if (session.Status != SessionStatus.Active)
        {
            throw new ConflictException(
                $"This session is '{session.Status}' and no longer accepts coach feedback requests.");
        }

        // 3. Dựng cửa sổ hội thoại gần nhất của toàn session (mọi stakeholder).
        var recentMessages = await simulationRepository.GetRecentSessionMessagesAsync(
            sessionId, ConversationWindowSize, cancellationToken);

        if (recentMessages.Count == 0)
        {
            // Không có lượt trao đổi nào thì coach không có gì để nhận xét.
            throw new ConflictException(
                "This session has no conversation yet. Please interview a stakeholder before requesting coach feedback.");
        }

        // 4. BA Plan là ngữ cảnh tùy chọn: learner có thể chưa lập kế hoạch.
        var baPlan = await planningService.GetPlanBySessionAsync(
            learnerId, sessionId, cancellationToken);

        // Repository đã loại tin nhắn System; ở đây chỉ map sang contract của AI Runtime.
        var coachMessages = recentMessages
            .Select(m => new CoachChatMessageDto(
                m.SenderType == MessageSenderType.Learner ? "Learner" : "Stakeholder",
                m.Content,
                m.CreatedAt.ToString("O")))
            .ToList();

        // 5. Gọi AI Runtime. Lỗi runtime được map thành AiRuntimeException
        //    để GlobalExceptionHandler trả 422 / 429 / 503.
        var aiRequest = new CoachFeedbackRequest(
            sessionId,
            baPlan is null
                ? null
                : new CoachBaPlanContextDto(
                    baPlan.Objective,
                    baPlan.Approach,
                    baPlan.Questions.Count),
            coachMessages,
            new CoachAiConnectionDto(
                aiConnection.Provider,
                aiConnection.Model,
                aiConnection.ApiKey));

        var aiResponse = await aiRuntimeClient.GetCoachFeedbackAsync(aiRequest, cancellationToken);

        // 6. Lưu kết quả. Mỗi lần gọi tạo một bản ghi mới để giữ lịch sử nhận xét.
        var feedback = new CoachFeedback
        {
            SessionId = sessionId,
            CreatedByUserId = learnerId,
            StrengthsJson = SerializeList(aiResponse.Strengths),
            GapsJson = SerializeList(aiResponse.Gaps),
            UnclearPointsJson = SerializeList(aiResponse.UnclearPoints),
            ImprovementHintsJson = SerializeList(aiResponse.ImprovementHints),
            PromptVersion = aiResponse.PromptVersion ?? "unknown",
            CreatedAt = DateTimeOffset.UtcNow
        };

        simulationRepository.AddCoachFeedback(feedback);
        await simulationRepository.SaveChangesAsync(cancellationToken);

        return ToCoachFeedbackDto(feedback);
    }

    public async Task<CoachFeedbackDto?> GetLatestCoachFeedbackAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // Session phải tồn tại và thuộc learner → nếu không trả null (controller map 404).
        // Truy vấn ownership trước bản ghi feedback: nếu learner không sở hữu session
        // thì không được biết session đó có feedback hay không.
        var sessionExists = await simulationRepository.SessionExistsAndOwnedByAsync(
            learnerId, sessionId, cancellationToken);

        if (!sessionExists)
        {
            return null;
        }

        // Chỉ đọc dữ liệu đã lưu — KHÔNG gọi LLM (đúng cam kết của spec).
        var feedback = await simulationRepository.GetLatestCoachFeedbackAsync(
            sessionId, cancellationToken);

        return feedback is null ? null : ToCoachFeedbackDto(feedback);
    }

    /// <summary>
    /// Chuyển entity sang DTO, giải mã các cột JSON array thành danh sách chuỗi.
    /// </summary>
    private static CoachFeedbackDto ToCoachFeedbackDto(CoachFeedback feedback)
    {
        return new CoachFeedbackDto(
            feedback.Id,
            feedback.SessionId,
            DeserializeList(feedback.StrengthsJson),
            DeserializeList(feedback.GapsJson),
            DeserializeList(feedback.UnclearPointsJson),
            DeserializeList(feedback.ImprovementHintsJson),
            feedback.PromptVersion,
            feedback.CreatedAt);
    }

    private static string SerializeList(IReadOnlyList<string>? values)
    {
        return JsonSerializer.Serialize(values ?? Array.Empty<string>());
    }

    /// <summary>
    /// Giải mã JSON array. Dữ liệu hỏng hoặc không phải mảng chuỗi trả về danh sách rỗng
    /// thay vì ném lỗi — một bản ghi lỗi không nên làm hỏng cả endpoint đọc.
    /// </summary>
    private static IReadOnlyList<string> DeserializeList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}
