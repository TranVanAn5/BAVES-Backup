using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Scenario.Models.Requests;

public sealed class RejectScenarioVersionRequest
{
    [Required]
    [MinLength(10)]
    [MaxLength(4000)]
    public string Comment { get; set; } = null!;
}
