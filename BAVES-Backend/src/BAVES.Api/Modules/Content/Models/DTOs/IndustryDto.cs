using BAVES.Api.Modules.Content.Models.Enums;

namespace BAVES.Api.Modules.Content.Models.DTOs;

public sealed class IndustryDto
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ContentStatus Status { get; set; }

    public int DisplayOrder { get; set; }
}
