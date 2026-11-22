using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Models.Entities;

namespace BAVES.Api.Modules.Simulation.Repositories.Interfaces;

/// <summary>
/// Truy v?n d? li?u Simulation (SimulationSessions, SessionAgents) t? b?ng
/// [simulation].[SimulationSessions] v� [simulation].[SessionAgents].
/// </summary>
public interface ISimulationSessionRepository
{
    /// <summary>
    /// L?y th�ng tin m?t session c?a learner k�m danh s�ch SessionAgents.
    /// Tr? null n?u session kh�ng t?n t?i ho?c kh�ng thu?c learner.
    /// </summary>
    Task<SimulationSession?> GetSessionWithAgentsAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// L?y session theo Id, KH�NG ki?m tra quy?n s? h?u.
    /// Ch? d�ng cho lu?ng Instructor/Admin d� du?c authorize b?ng policy ri�ng.
    /// </summary>
    Task<SimulationSession?> GetSessionByIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// L?y danh s�ch SessionAgents c?a m?t session, s?p theo PersonaId.
    /// Tr? v? danh s�ch r?ng n?u session h?p l? nhung chua c� agent n�o.
    /// </summary>
    Task<IReadOnlyList<SessionAgent>> GetAgentsBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ki?m tra session c� t?n t?i v� thu?c v? learner hay kh�ng.
    /// Tr? true n?u t?n t?i v� LearnerId kh?p.
    /// </summary>
    Task<bool> SessionExistsAndOwnedByAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ki?m tra session c� t?n t?i hay kh�ng, KH�NG ki?m tra quy?n s? h?u.
    /// Ch? d�ng cho lu?ng Instructor/Admin (d� du?c authorize b?ng policy ri�ng).
    /// </summary>
    Task<bool> SessionExistsAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// T�nh s? th? t? l?n th? ti?p theo c?a learner tr�n m?t scenario version.
    /// Tr? v? 1 n?u learner chua c� l?n th? n�o.
    /// </summary>
    Task<int> GetNextAttemptNoAsync(
        int learnerId,
        int scenarioVersionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ki?m tra m?t tin nh?n c� t?n t?i v� thu?c v? session hay kh�ng.
    /// D�ng b?i module kh�c (Analysis) d? x�c th?c b?ng ch?ng g?n v?i
    /// m?t tin nh?n trong transcript c?a d�ng session d�.
    /// </summary>
    /// <returns>true n?u tin nh?n t?n t?i v� thu?c session; false n?u kh�ng.</returns>
    Task<bool> MessageBelongsToSessionAsync(
        int messageId,
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Th�m m?i m?t SimulationSession (k�m SessionAgents con).
    /// Id du?c sinh b?i database sau khi SaveChanges.
    /// </summary>
    void AddSession(SimulationSession session);

    /// <summary>
    /// L?y m?t SessionAgent theo Id, gi?i h?n trong m?t session c? th?.
    /// Tr? null n?u agent kh�ng t?n t?i ho?c kh�ng thu?c session d�.
    /// Kh�ng d�ng AsNoTracking v� c?n theo d�i entity d? c?p nh?t tr?ng th�i agent.
    /// </summary>
    Task<SessionAgent?> GetAgentByIdAsync(
        int sessionId,
        int agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// L?y m?t ph?n transcript c?a agent, s?p theo th?i gian tang d?n.
    /// D�ng cho GET messages (ph�n trang) v� cho vi?c d?ng l?ch s? g?i AI Runtime.
    /// </summary>
    /// <param name="sessionId">Session ch?a agent.</param>
    /// <param name="agentId">Agent c?n l?y transcript.</param>
    /// <param name="skip">S? b?n ghi b? qua (d�ng cho ph�n trang).</param>
    /// <param name="take">S? b?n ghi l?y t?i da.</param>
    /// <param name="cancellationToken">Token hu?.</param>
    Task<IReadOnlyList<Message>> GetMessagesByAgentAsync(
        int sessionId,
        int agentId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// �?m t?ng s? tin nh?n c?a m?t agent (d�ng cho ph�n trang).
    /// </summary>
    Task<int> CountMessagesByAgentAsync(
        int sessionId,
        int agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Th�m m?i m?t Message v�o transcript.
    /// Id du?c sinh b?i database sau khi SaveChanges.
    /// </summary>
    void AddMessage(Message message);

    /// <summary>
    /// L?y m?t ph?n danh s�ch event d� x?y ra trong session, s?p theo SequenceNo tang d?n.
    /// D�ng cho GET /api/sessions/{sessionId}/events (ph�n trang).
    /// </summary>
    /// <param name="sessionId">Session c?n l?y event.</param>
    /// <param name="skip">S? b?n ghi b? qua (d�ng cho ph�n trang).</param>
    /// <param name="take">S? b?n ghi l?y t?i da.</param>
    /// <param name="cancellationToken">Token hu?.</param>
    Task<IReadOnlyList<SessionEvent>> GetEventsBySessionIdAsync(
        int sessionId,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// �?m t?ng s? event d� x?y ra trong session (d�ng cho ph�n trang).
    /// </summary>
    Task<int> CountEventsBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// L?y b?n ghi event c?a session ?ng v?i m?t event g?c trong k?ch b?n.
    /// Tr? null n?u event d� chua t?ng du?c trigger trong session n�y.
    /// D�ng d? b?o d?m m?i ScenarioEvent ch? du?c ghi nh?n m?t l?n trong session.
    /// </summary>
    Task<SessionEvent?> GetSessionEventByScenarioEventIdAsync(
        int sessionId,
        int scenarioEventId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// L?y c�c b?n ghi event c?a session ?ng v?i danh s�ch event g?c (batch),
    /// tr�nh N+1 khi c?n bi?t event n�o d� trigger.
    /// </summary>
    Task<IReadOnlyList<SessionEvent>> GetSessionEventsByScenarioEventIdsAsync(
        int sessionId,
        IReadOnlyCollection<int> scenarioEventIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// T�nh s? th? t? (SequenceNo) ti?p theo cho event trong session.
    /// Tr? v? 1 n?u session chua c� event n�o.
    /// </summary>
    Task<int> GetNextEventSequenceNoAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>Danh sách SprintNo đã Finalized trong session (để mở khóa event AfterSprintEnds).</summary>
    Task<IReadOnlyList<int>> GetFinalizedSprintNosAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Th�m m?i m?t SessionEvent.
    /// Id du?c sinh b?i database sau khi SaveChanges.
    /// </summary>
    void AddSessionEvent(SessionEvent sessionEvent);

    /// <summary>
    /// Th?c thi m?t h�m trong transaction Serializable d? ch?ng ghi tr�ng event
    /// khi hai request trigger d?ng th?i c�ng m?t event.
    /// </summary>
    Task<T> ExecuteInSerializableTransactionAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// L?y <paramref name="take"/> tin nh?n g?n nh?t c?a TO�N B? session (m?i agent),
    /// s?p theo th?i gian TANG d?n d? gi? d�ng m?ch h?i tho?i khi g?i cho coach.
    ///
    /// Kh�c v?i <see cref="GetMessagesByAgentAsync"/>: coach c?n nh�n c? phi�n ph?ng v?n
    /// d? d�nh gi� k? nang, kh�ng ch? m?t stakeholder.
    /// </summary>
    /// <remarks>
    /// Truy v?n l?y theo th? t? gi?m d?n r?i d?o l?i trong b? nh?, v� mu?n N tin nh?n
    /// M?I NH?T nhung v?n tr? v? theo tr�nh t? th?i gian tang d?n.
    /// B? qua tin nh?n h? th?ng (SenderType = System) v� d� kh�ng ph?i lu?t c?a learner
    /// v� kh�ng d�ng d? d�nh gi� k? nang d?t c�u h?i.
    /// </remarks>
    Task<IReadOnlyList<Message>> GetRecentSessionMessagesAsync(
        int sessionId,
        int take,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Th�m m?i m?t b?n ghi CoachFeedback. Id sinh b?i database sau khi SaveChanges.
    /// </summary>
    void AddCoachFeedback(CoachFeedback coachFeedback);

    /// <summary>
    /// L?y b?n CoachFeedback M?I NH?T c?a m?t session.
    /// Tr? null n?u session chua t?ng du?c coach nh?n x�t.
    /// Kh�ng g?i LLM � ch? d?c d? li?u d� luu.
    /// </summary>
    Task<CoachFeedback?> GetLatestCoachFeedbackAsync(
        int sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Luu to�n b? thay d?i (insert / update / delete) trong m?t transaction.
    /// </summary>
    Task<int> CountAttemptsByAssignmentAsync(
        int learnerId,
        int assignmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SimulationSession>> GetLearnerSessionsByAssignmentIdsAsync(
        int learnerId,
        IReadOnlyCollection<int> assignmentIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SimulationSession>> GetSessionsByAssignmentIdAsync(
        int assignmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SimulationSession>> GetSessionsByLearnerIdsAsync(
        IReadOnlyCollection<int> learnerIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Message>> GetMessagesByIdsAsync(
        int sessionId,
        IReadOnlyCollection<int> messageIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SessionHistoryItemDto>> GetLearnerSessionsHistoryAsync(
        int learnerId,
        SessionStatus? status,
        int? scenarioVersionId,
        int? assignmentId,
        CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}