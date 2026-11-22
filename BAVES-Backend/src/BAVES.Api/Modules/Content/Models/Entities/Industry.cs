using BAVES.Api.Modules.Content.Models.Enums;

namespace BAVES.Api.Modules.Content.Models.Entities;

public class Industry
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ContentStatus Status { get; set; } = ContentStatus.Active;

    public int DisplayOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<BusinessApplication> Applications { get; set; } = new List<BusinessApplication>();

}
