using BAVES.Api.Modules.Simulation.Models.DTOs;
using BAVES.Api.Modules.Simulation.Data;
using BAVES.Api.Modules.Simulation.Models.Entities;
using BAVES.Api.Modules.Simulation.Models.Enums;
using BAVES.Api.Modules.Simulation.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Simulation.Repositories;

/// <summary>
/// Hiện thực truy vấn Simulation trên SimulationDbContext.
/// </summary>
public sealed class SimulationSessionRepository(SimulationDbContext db)
    : ISimulationSessionRepository
{
    public async Task<SimulationSession?> GetSessionWithAgentsAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // AsNoTracking: chỉ đọc, không theo dõi thay đổi.
        // Include: lấy kèm SessionAgents trong cùng 1 query (tránh N+1).
        return await db.SimulationSessions
            .AsNoTracking()
            .Include(s => s.SessionAgents)
            .FirstOrDefaultAsync(
                s => s.Id == sessionId && s.LearnerId == learnerId,
                cancellationToken);
    }

    public async Task<SimulationSession?> GetSessionByIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await db.SimulationSessions
            .AsNoTracking()
            .Include(s => s.SessionAgents)
            .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<IReadOnlyList<SessionAgent>> GetAgentsBySessionIdAsync(        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await db.SessionAgents
            .AsNoTracking()
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.PersonaId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SessionExistsAndOwnedByAsync(
        int learnerId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // AnyAsync: chỉ cần biết "có tồn tại và thuộc learner không" → trả bool,
        // SQL Server dừng ở EXISTS, không tải dữ liệu về RAM.
        return await db.SimulationSessions
            .AsNoTracking()
            .AnyAsync(
                s => s.Id == sessionId && s.LearnerId == learnerId,
                cancellationToken);
    }

    public async Task<bool> SessionExistsAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await db.SimulationSessions
            .AsNoTracking()
            .AnyAsync(s => s.Id == sessionId, cancellationToken);
    }

    public async Task<int> GetNextAttemptNoAsync(
        int learnerId,
        int scenarioVersionId,
        CancellationToken cancellationToken = default)
    {
        // Select sang int? (nullable) rồi MaxAsync → nếu chưa có lần thử nào
        // Max trả null → +1 = null → ?? 1 = 1.
        //null + 1 = null (ko phải lỗi — đây là "lifted operator" của C#)
        var maxAttemptNo = await db.SimulationSessions
            .Where(s => s.LearnerId == learnerId && s.ScenarioVersionId == scenarioVersionId)
            .Select(s => (int?)s.AttemptNo)
            .MaxAsync(cancellationToken);

        return maxAttemptNo + 1 ?? 1; //?? = "nếu bên trái null thì lấy bên phải".
    }

    public async Task<bool> MessageBelongsToSessionAsync(
        int messageId,
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // AnyAsync: chỉ cần biết "tin nhắn có tồn tại và thuộc session không" → trả bool,
        // SQL Server dừng ở EXISTS, không tải dữ liệu về RAM.
        return await db.Messages
            .AsNoTracking()
            .AnyAsync(
                m => m.Id == messageId && m.SessionId == sessionId,
                cancellationToken);
    }

    public async Task<SessionAgent?> GetAgentByIdAsync(
        int sessionId,
        int agentId,
        CancellationToken cancellationToken = default)
    {
        // Không AsNoTracking: cần theo dõi entity để SaveChanges sinh UPDATE
        // (cập nhật MemorySummary / trust / patience / cooperation sau mỗi lượt hỏi).
        // Điều kiện SessionId == sessionId bảo đảm agent nằm đúng session đang xử lý.
        return await db.SessionAgents
            .FirstOrDefaultAsync(
                a => a.Id == agentId && a.SessionId == sessionId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetMessagesByAgentAsync(
        int sessionId,
        int agentId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        // Chỉ đọc, không theo dõi thay đổi.
        // Lọc theo cả SessionId (qua agent) và SessionAgentId để không lẫn tin nhắn
        // của các agent khác trong cùng session.
        return await db.Messages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId && m.SessionAgentId == agentId)
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountMessagesByAgentAsync(
        int sessionId,
        int agentId,
        CancellationToken cancellationToken = default)
    {
        return await db.Messages
            .AsNoTracking()
            .CountAsync(
                m => m.SessionId == sessionId && m.SessionAgentId == agentId,
                cancellationToken);
    }

    public void AddMessage(Message message)
    {
        db.Messages.Add(message);
    }

    public void AddSession(SimulationSession session)
    {
        db.SimulationSessions.Add(session);
    }

    public async Task<IReadOnlyList<SessionEvent>> GetEventsBySessionIdAsync(
        int sessionId,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        // Sắp theo SequenceNo để client hiển thị đúng thứ tự diễn biến của kịch bản.
        return await db.SessionEvents
            .AsNoTracking()
            .Where(e => e.SessionId == sessionId)
            .OrderBy(e => e.SequenceNo)
            .ThenBy(e => e.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountEventsBySessionIdAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return db.SessionEvents
            .AsNoTracking()
            .CountAsync(e => e.SessionId == sessionId, cancellationToken);
    }

    public async Task<SessionEvent?> GetSessionEventByScenarioEventIdAsync(
        int sessionId,
        int scenarioEventId,
        CancellationToken cancellationToken = default)
    {
        return await db.SessionEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.SessionId == sessionId && e.ScenarioEventId == scenarioEventId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<SessionEvent>> GetSessionEventsByScenarioEventIdsAsync(
        int sessionId,
        IReadOnlyCollection<int> scenarioEventIds,
        CancellationToken cancellationToken = default)
    {
        if (scenarioEventIds.Count == 0)
        {
            return Array.Empty<SessionEvent>();
        }

        return await db.SessionEvents
            .AsNoTracking()
            .Where(e => e.SessionId == sessionId && scenarioEventIds.Contains(e.ScenarioEventId))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextEventSequenceNoAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        // Select sang int? rồi Max → session chưa có event nào thì Max trả null → ?? 1.
        var maxSequenceNo = await db.SessionEvents
            .Where(e => e.SessionId == sessionId)
            .Select(e => (int?)e.SequenceNo)
            .MaxAsync(cancellationToken);

        return maxSequenceNo + 1 ?? 1;
    }

    public async Task<IReadOnlyList<int>> GetFinalizedSprintNosAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return await db.SessionSprintReviews
            .AsNoTracking()
            .Where(r => r.SessionId == sessionId && r.Status == "Finalized")
            .Select(r => r.SprintNo)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);
    }

    public void AddSessionEvent(SessionEvent sessionEvent)
    {
        db.SessionEvents.Add(sessionEvent);
    }

    public async Task<T> ExecuteInSerializableTransactionAsync<T>(
        Func<Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        // DbContext được cấu hình EnableRetryOnFailure nên EF dùng SqlServerRetryingExecutionStrategy.
        // Strategy này KHÔNG cho phép tự mở transaction bằng BeginTransactionAsync —
        // phải chạy toàn bộ khối (mở transaction + logic + commit) bên trong
        // CreateExecutionStrategy().ExecuteAsync để EF có thể retry như một đơn vị.
        var strategy = db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            // Serializable: hai request trigger đồng thời cùng một event sẽ bị tuần tự hoá,
            // nhờ đó bước "kiểm tra đã tồn tại chưa rồi mới insert" không bị race.
            await using var transaction = await db.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

            var result = await action();

            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetRecentSessionMessagesAsync(
        int sessionId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            return Array.Empty<Message>();
        }

        // Lấy N tin nhắn mới nhất (sắp giảm dần), sau đó đảo lại thành tăng dần
        // để coach đọc đúng mạch hội thoại.
        var recent = await db.Messages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId
                        && m.SenderType != MessageSenderType.System)
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        recent.Reverse();
        return recent;
    }

    public void AddCoachFeedback(CoachFeedback coachFeedback)
    {
        db.CoachFeedbacks.Add(coachFeedback);
    }

    public async Task<int> CountAttemptsByAssignmentAsync(
        int learnerId,
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await db.SimulationSessions
            .AsNoTracking()
            .CountAsync(s => s.LearnerId == learnerId && s.AssignmentId == assignmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<SimulationSession>> GetLearnerSessionsByAssignmentIdsAsync(
        int learnerId,
        IReadOnlyCollection<int> assignmentIds,
        CancellationToken cancellationToken = default)
    {
        if (assignmentIds.Count == 0)
        {
            return Array.Empty<SimulationSession>();
        }

        return await db.SimulationSessions
            .AsNoTracking()
            .Where(s => s.LearnerId == learnerId && s.AssignmentId.HasValue && assignmentIds.Contains(s.AssignmentId.Value))
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SimulationSession>> GetSessionsByAssignmentIdAsync(
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        return await db.SimulationSessions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SimulationSession>> GetSessionsByLearnerIdsAsync(
        IReadOnlyCollection<int> learnerIds,
        CancellationToken cancellationToken = default)
    {
        if (learnerIds.Count == 0)
        {
            return Array.Empty<SimulationSession>();
        }

        return await db.SimulationSessions
            .AsNoTracking()
            .Where(s => learnerIds.Contains(s.LearnerId))
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetMessagesByIdsAsync(
        int sessionId,
        IReadOnlyCollection<int> messageIds,
        CancellationToken cancellationToken = default)
    {
        if (messageIds.Count == 0)
        {
            return Array.Empty<Message>();
        }

        return await db.Messages
            .AsNoTracking()
            .Where(m => m.SessionId == sessionId && messageIds.Contains(m.Id))
            .OrderBy(m => m.CreatedAt)
            .ThenBy(m => m.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SessionHistoryItemDto>> GetLearnerSessionsHistoryAsync(
        int learnerId,
        SessionStatus? status,
        int? scenarioVersionId,
        int? assignmentId,
        CancellationToken cancellationToken = default)
    {
        var query = db.SimulationSessions
            .AsNoTracking()
            .Where(s => s.LearnerId == learnerId);

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (scenarioVersionId.HasValue)
        {
            query = query.Where(s => s.ScenarioVersionId == scenarioVersionId.Value);
        }

        if (assignmentId.HasValue)
        {
            query = query.Where(s => s.AssignmentId == assignmentId.Value);
        }

        return await query
            .OrderByDescending(s => s.StartedAt)
            .ThenByDescending(s => s.Id)
            .Select(s => new SessionHistoryItemDto(
                s.Id,
                s.ScenarioVersionId,
                s.AssignmentId,
                s.AttemptNo,
                s.Status,
                s.SessionAgents.Count,
                s.Messages.Count,
                s.StartedAt,
                s.SubmittedAt,
                s.CompletedAt))
            .ToListAsync(cancellationToken);
    }

    public Task<CoachFeedback?> GetLatestCoachFeedbackAsync(
        int sessionId,
        CancellationToken cancellationToken = default)
    {
        return db.CoachFeedbacks
            .AsNoTracking()
            .Where(f => f.SessionId == sessionId)
            .OrderByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}