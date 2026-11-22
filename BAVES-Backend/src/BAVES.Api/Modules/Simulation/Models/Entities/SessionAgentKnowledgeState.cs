
namespace BAVES.Api.Modules.Simulation.Models.Entities;

public class SessionAgentKnowledgeState
{
    public int Id { get; set; }

    public int SessionAgentId { get; set; }

    public int KnowledgeItemId { get; set; }

    public bool IsRevealed { get; set; }

    public DateTimeOffset? RevealedAt { get; set; }

    public int? RevealMessageId { get; set; }

    public SessionAgent SessionAgent { get; set; } = null!;

    public Message? RevealMessage { get; set; }

}
