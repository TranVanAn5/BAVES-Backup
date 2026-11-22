using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Artifact.Models.Enums;

namespace BAVES.Api.Modules.Artifact.Models.Requests;

public sealed class ArtifactTemplateSectionInputRequest
{
    [Required(ErrorMessage = "SectionKey là bắt buộc.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "SectionKey phải từ 1 đến 80 ký tự.")]
    public string SectionKey { get; set; } = null!;

    [Required(ErrorMessage = "Title là bắt buộc.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title phải từ 1 đến 200 ký tự.")]
    public string Title { get; set; } = null!;

    [MaxLength(2000)]
    public string? Instructions { get; set; }

    public int DisplayOrder { get; set; } = 1;

    public bool IsRequired { get; set; } = true;
}

public sealed class CreateArtifactTemplateRequest
{
    [Required(ErrorMessage = "Code là bắt buộc.")]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Code phải từ 2 đến 60 ký tự.")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Name là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name phải từ 3 đến 200 ký tự.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ArtifactType là bắt buộc.")]
    public ArtifactType ArtifactType { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool PublishImmediately { get; set; }

    [Required(ErrorMessage = "Sections là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Biểu mẫu phải có ít nhất 1 đề mục (Section).")]
    public List<ArtifactTemplateSectionInputRequest> Sections { get; set; } = new();
}

public sealed class UpdateArtifactTemplateRequest
{
    [Required(ErrorMessage = "Name là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name phải từ 3 đến 200 ký tự.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "ArtifactType là bắt buộc.")]
    public ArtifactType ArtifactType { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Sections là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Biểu mẫu phải có ít nhất 1 đề mục (Section).")]
    public List<ArtifactTemplateSectionInputRequest> Sections { get; set; } = new();
}

public sealed class CreateArtifactRequest
{
    [Required(ErrorMessage = "ArtifactTemplateId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "ArtifactTemplateId phải lớn hơn 0.")]
    public int ArtifactTemplateId { get; set; }

    [Required(ErrorMessage = "Title là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title phải từ 3 đến 200 ký tự.")]
    public string Title { get; set; } = null!;
}

public sealed class UpdateArtifactDraftRequest
{
    public string? DraftStructuredContentJson { get; set; }

    public string? DraftPlainTextContent { get; set; }
}

public sealed class CreateArtifactVersionRequest
{
    [Required(ErrorMessage = "StructuredContentJson là bắt buộc.")]
    public string StructuredContentJson { get; set; } = null!;

    [Required(ErrorMessage = "PlainTextContent là bắt buộc.")]
    public string PlainTextContent { get; set; } = null!;
}

public sealed class CreateSubmissionRequest
{
    [Required(ErrorMessage = "ArtifactVersionId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "ArtifactVersionId phải lớn hơn 0.")]
    public int ArtifactVersionId { get; set; }
}
