using BAVES.Api.Modules.Analysis.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.Entities;

public class MessageEvidenceLink
{
    public int Id { get; set; }

    public int MessageId { get; set; }

    public int AnalysisItemId { get; set; }

    public EvidenceRelationshipType RelationshipType { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public AnalysisItem AnalysisItem { get; set; } = null!;

}
