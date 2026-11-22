using BAVES.Api.Modules.Content.Models.Enums;

namespace BAVES.Api.Modules.Content.Models.Entities;

public class BusinessApplication
{
    public int Id { get; set; }

    public int IndustryId { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Active;

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Industry Industry { get; set; } = null!;

}
