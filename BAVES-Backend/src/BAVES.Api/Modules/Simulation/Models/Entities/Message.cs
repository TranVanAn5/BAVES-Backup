using BAVES.Api.Modules.Simulation.Models.Enums;

namespace BAVES.Api.Modules.Simulation.Models.Entities;

public class Message
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int? SessionAgentId { get; set; }

    public MessageSenderType SenderType { get; set; }

    public string Content { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public SimulationSession Session { get; set; } = null!;

    public SessionAgent? SessionAgent { get; set; }

    public ICollection<SessionAgentKnowledgeState> SessionAgentKnowledgeStates { get; set; } = new List<SessionAgentKnowledgeState>();

}
