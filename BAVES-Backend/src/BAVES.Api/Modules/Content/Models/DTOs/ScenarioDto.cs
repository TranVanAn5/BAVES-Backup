using BAVES.Api.Modules.Content.Models.Enums;

namespace BAVES.Api.Modules.Content.Models.DTOs;

public sealed class ScenarioDto
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }

    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public ContentStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// True nếu scenario này có ít nhất một phiên bản đã được publish.
    /// Dùng để hỗ trợ filter ?isPublished=true mà không cần load toàn bộ danh sách version.
    /// </summary>
    public bool HasPublishedVersion { get; set; }

    public int? PublishedVersionId { get; set; }

    public string? Difficulty { get; set; }

    public bool IsFreeAccess { get; set; }

    public ScenarioApplicationDto Application { get; set; } = null!;
}

public sealed class ScenarioApplicationDto
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
}
