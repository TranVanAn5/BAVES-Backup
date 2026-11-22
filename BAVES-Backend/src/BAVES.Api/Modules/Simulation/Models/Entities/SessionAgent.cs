
namespace BAVES.Api.Modules.Simulation.Models.Entities;

public class SessionAgent
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int PersonaId { get; set; }

    public string? MemorySummary { get; set; }

    public string? CurrentMood { get; set; }

    public byte? TrustLevel { get; set; }

    public byte? PatienceLevel { get; set; }

    public byte? CooperationLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset? LastInteractionAt { get; set; }

    public SimulationSession Session { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = new List<Message>();

    public ICollection<SessionAgentKnowledgeState> SessionAgentKnowledgeStates { get; set; } = new List<SessionAgentKnowledgeState>();

}
