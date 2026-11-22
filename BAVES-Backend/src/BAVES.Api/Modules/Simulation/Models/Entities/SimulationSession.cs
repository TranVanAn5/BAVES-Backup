using BAVES.Api.Modules.Simulation.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.Entities;

public class SimulationSession
{
    public int Id { get; set; }

    public int ScenarioVersionId { get; set; }

    public int? AssignmentId { get; set; }

    public int LearnerId { get; set; }

    public int AttemptNo { get; set; }

    public SessionStatus Status { get; set; } = SessionStatus.Active;

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? SubmittedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public ICollection<SessionAgent> SessionAgents { get; set; } = new List<SessionAgent>();

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<SessionEvent> SessionEvents { get; set; } = new List<SessionEvent>();

    public ICollection<CoachFeedback> CoachFeedbacks { get; set; } = new List<CoachFeedback>();

}
