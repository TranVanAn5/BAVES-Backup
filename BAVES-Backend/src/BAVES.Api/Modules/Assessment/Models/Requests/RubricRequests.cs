using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Assessment.Models.Requests;

public sealed class RubricCriterionInputRequest
{
    [Required(ErrorMessage = "BabokTaskId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "BabokTaskId phải lớn hơn 0.")]
    public int BabokTaskId { get; set; }

    [Required(ErrorMessage = "Code là bắt buộc.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "Code phải từ 1 đến 80 ký tự.")]
    public string Code { get; set; } = null!;

    [Required(ErrorMessage = "Name là bắt buộc.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name phải từ 1 đến 200 ký tự.")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Description là bắt buộc.")]
    [MaxLength(2000)]
    public string Description { get; set; } = null!;

    [Range(0.0001, 100.0, ErrorMessage = "Weight phải lớn hơn 0 và không vượt quá 100.")]
    public decimal Weight { get; set; }

    [Range(1.0, 1000.0, ErrorMessage = "MaxScore phải từ 1 đến 1000.")]
    public decimal MaxScore { get; set; } = 100m;

    [MaxLength(2000)]
    public string? EvidenceGuidance { get; set; }

    public int DisplayOrder { get; set; } = 1;

    public bool IsRequired { get; set; } = true;
}

public sealed class CreateRubricRequest
{
    [Required(ErrorMessage = "ScenarioVersionId là bắt buộc.")]
    [Range(1, int.MaxValue, ErrorMessage = "ScenarioVersionId phải lớn hơn 0.")]
    public int ScenarioVersionId { get; set; }

    [Required(ErrorMessage = "Name là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name phải từ 3 đến 200 ký tự.")]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool PublishImmediately { get; set; }

    [Required(ErrorMessage = "Criteria là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Rubric phải có ít nhất 1 tiêu chí chấm điểm.")]
    public List<RubricCriterionInputRequest> Criteria { get; set; } = new();
}

public sealed class UpdateRubricRequest
{
    [Required(ErrorMessage = "Name là bắt buộc.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Name phải từ 3 đến 200 ký tự.")]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Criteria là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Rubric phải có ít nhất 1 tiêu chí chấm điểm.")]
    public List<RubricCriterionInputRequest> Criteria { get; set; } = new();
}
