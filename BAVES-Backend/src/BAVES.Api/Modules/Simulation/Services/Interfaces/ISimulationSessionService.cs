using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Requests;

namespace BAVES.Api.Modules.Simulation.Services.Interfaces;

/// <summary>
/// Service xử lý nghiệp vụ Simulation Session cho Learner.
/// Bám sát các quy tắc trong BA-VES: learner bắt đầu một phiên luyện tập từ một
/// ScenarioVersion đã publish; hệ thống tạo session, khởi tạo SessionAgents từ
/// StakeholderPersonas, và trả về thông tin session cho client.
/// </summary>
public interface ISimulationSessionService
{
    /// <summary>
    /// Bắt đầu một Simulation Session mới cho learner từ một ScenarioVersion đã publish.
    /// Validate version tồn tại, đã publish, có ít nhất 1 persona active;
    /// tính AttemptNo tiếp theo; tạo SimulationSession ở trạng thái Active
    /// cùng các SessionAgents khởi đầu từ personas active.
    /// </summary>
    /// <param name="learnerId">User id lấy từ JWT (sub claim).</param>
    /// <param name="request">Body chứa ScenarioVersionId.</param>
    /// <param name="cancellationToken">Token huỷ.</param>
    /// <returns>SessionDto vừa tạo.</returns>
    Task<SessionDto> StartSessionAsync(
        int learnerId,
        StartSessionRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Lấy thông tin chi tiết một Simulation Session thuộc về learner.
    /// Trả về null nếu session không tồn tại hoặc không thuộc learner này.
    /// </summary>
    /// <param name="learnerId">User id lấy từ JWT (sub claim).</param>
    /// <param name="sessionId">Id của session cần truy vấn.</param>
    /// <param name="cancellationToken">Token huỷ.</param>
    /// <returns>SessionDto nếu tìm thấy; null nếu không tồn tại hoặc không thuộc learner.</returns>
    Task<SessionDto?> GetSessionByIdAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default
    );
    /// <summary>
    /// Lấy danh sách trả các stakehoder persona đã gắn vào một session của Learner.
    /// Trả về null nêu session không tồn tại hoặc không thuộc Learner này
    /// </summary>
    Task<IReadOnlyList<SessionAgentDto>?> GetSessionAgentsAsync(
         int learnerId,
         int sessionId,
         CancellationToken cancellationToken = default
        );

    /// <summary>
    /// Kiểm tra một simulation session có tồn tại và thuộc về learner hay không.
    /// Dùng bởi module khác (Planning) để xác thực quyền sở hữu session.
    /// </summary>
    /// <returns>true nếu session tồn tại và LearnerId khớp; false nếu không.</returns>
    Task<bool> SessionExistsAndOwnedByAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Kiểm tra một tin nhắn trong transcript có tồn tại và thuộc về session hay không.
    /// Dùng bởi module khác (Analysis) để xác thực bằng chứng gắn với
    /// một tin nhắn của đúng session chứa mục phân tích.
    /// </summary>
    /// <returns>true nếu tin nhắn tồn tại và thuộc session; false nếu không.</returns>
    Task<bool> MessageBelongsToSessionAsync(
        int messageId,
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gửi tin nhắn của learner tới một stakeholder agent trong session và nhận về
    /// câu trả lời của AI stakeholder.
    /// Quy trình:
    ///   1. Kiểm tra session thuộc learner và còn Active.
    ///   2. Lấy persona + allowed knowledge (lọc theo StakeholderKnowledgeAccess).
    ///   3. Dựng payload và gọi AI Runtime (POST /v1/stakeholder/respond).
    ///   4. Lưu Message(Learner) + Message(Stakeholder) và cập nhật trạng thái agent.
    /// </summary>
    /// <param name="learnerId">User id lấy từ JWT (sub claim).</param>
    /// <param name="sessionId">Session chứa agent.</param>
    /// <param name="agentId">SessionAgent cần phỏng vấn.</param>
    /// <param name="request">Nội dung tin nhắn của learner.</param>
    /// <param name="aiConnection">Kết nối AI theo BYOK (provider/model/apiKey) — không lưu DB.</param>
    /// <param name="cancellationToken">Token huỷ.</param>
    /// <returns>Tin nhắn learner + tin nhắn stakeholder + các knowledge item đã dùng.</returns>
    /// <exception cref="Shared.Exceptions.NotFoundException">Session/agent không tồn tại hoặc không thuộc learner.</exception>
    /// <exception cref="Shared.Exceptions.ConflictException">Session không còn Active.</exception>
    /// <exception cref="Shared.Exceptions.AiRuntimeException">AI Runtime lỗi (key sai, rate limit, timeout).</exception>
    Task<SendAgentMessageResultDto> SendAgentMessageAsync(
        int learnerId,
        int sessionId,
        int agentId,
        SendAgentMessageRequest request,
        AgentAiConnection aiConnection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy transcript của một stakeholder agent trong session, có phân trang.
    /// Trả null nếu session không tồn tại, không thuộc learner, hoặc agent không thuộc session.
    /// </summary>
    Task<AgentMessagePageDto?> GetAgentMessagesAsync(
        int learnerId,
        int sessionId,
        int agentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách event đã xảy ra trong session, có phân trang, sắp theo thứ tự kích hoạt.
    /// Trả null nếu session không tồn tại hoặc không thuộc learner (controller map 404).
    /// Trả trang rỗng nếu session hợp lệ nhưng chưa có event nào.
    /// </summary>
    /// <param name="learnerId">User id lấy từ JWT (sub claim).</param>
    /// <param name="sessionId">Session cần lấy event.</param>
    /// <param name="page">Trang hiện tại (bắt đầu từ 1).</param>
    /// <param name="pageSize">Số bản ghi mỗi trang.</param>
    /// <param name="isInstructorOrAdmin">
    /// true nếu người gọi là Instructor/Admin — khi đó được xem session của learner khác.
    /// </param>
    /// <param name="cancellationToken">Token huỷ.</param>
    Task<SessionEventPageDto?> GetSessionEventsAsync(
        int learnerId,
        int sessionId,
        int page,
        int pageSize,
        bool isInstructorOrAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trigger một event thuộc kịch bản của session. Chỉ Instructor/Admin được gọi
    /// (endpoint gắn [Authorize(Roles = "INSTRUCTOR,ADMIN")])
    ///  Kiểm tra session tồn tại.
    ///  Kiểm tra session còn Active.
    ///  Kiểm tra event gốc (<paramref name="scenarioEventId"/>) tồn tại, đã active
    ///  và thuộc đúng ScenarioVersion mà session đang pin.
    ///  Chỉ cho phép trigger thủ công event có TriggerType = Manual.
    ///  Bảo đảm mỗi event chỉ ghi nhận một lần trong session (idempotent):
    ///  Nếu đã trigger trước đó thì trả lại bản ghi cũ với AlreadyTriggered = true.
    ///  Tạo SessionEvent với EventContentSnapshot đóng băng nội dung event.
    /// </summary>
    Task<TriggerSessionEventResultDto> TriggerSessionEventAsync(
        int sessionId,
        int scenarioEventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh mục event của kịch bản mà session đang dùng, kèm trạng thái đã trigger hay chưa.
    /// Giúp client biết event nào còn có thể kích hoạt.
    /// Trả null nếu session không tồn tại hoặc không thuộc learner.
    /// </summary>
    Task<int> CountAttemptsByAssignmentAsync(
        int learnerId,
        int assignmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignmentSessionSnapshotDto>> GetLearnerSessionsByAssignmentIdsAsync(
        int learnerId,
        IReadOnlyCollection<int> assignmentIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LearnerSessionSummaryDto>> GetSessionsByAssignmentIdAsync(
        int assignmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LearnerSessionSummaryDto>> GetSessionsByLearnerIdsAsync(
        IReadOnlyCollection<int> learnerIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AgentMessageDto>> GetMessagesByIdsAsync(
        int sessionId,
        IReadOnlyCollection<int> messageIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionHistoryItemDto>> GetLearnerSessionsHistoryAsync(
        int learnerId,
        SessionStatus? status,
        int? scenarioVersionId,
        int? assignmentId,
        CancellationToken cancellationToken = default);

    Task<SessionDto?> CompleteSessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<SessionDto?> AbandonSessionAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ScenarioEventAvailabilityDto>?> GetSessionEventCatalogAsync(
        int learnerId,
        int sessionId,
        bool isInstructorOrAdmin,
        CancellationToken cancellationToken = default);
}

