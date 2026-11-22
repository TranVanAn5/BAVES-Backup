using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Scenario.Models.Requests;

public sealed record CreateScenarioRequest
{
    [Required]
    public int ApplicationId { get; init; }

    [Required]
    [MaxLength(80)]
    public string Code { get; init; } = null!;

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public string? Summary { get; init; }

    public string? BusinessContext { get; init; }

    public string? BusinessProblem { get; init; }

    public string? BusinessObjectives { get; init; }

    public string? BARole { get; init; }

    public string? Assignment { get; init; }
}
