using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Planning.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.Entities;

public class AnalysisItem
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int? ParentAnalysisItemId { get; set; }

    public AnalysisItemType ItemType { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public PriorityLevel? Priority { get; set; }

    public AnalysisItemStatus Status { get; set; } = AnalysisItemStatus.Draft;

    public string? MetadataJson { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public AnalysisItem? ParentAnalysisItem { get; set; }

    public ICollection<AnalysisItem> ChildAnalysisItems { get; set; } = new List<AnalysisItem>();

    public ICollection<MessageEvidenceLink> MessageEvidenceLinks { get; set; } = new List<MessageEvidenceLink>();
}
