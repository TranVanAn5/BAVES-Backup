using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Scenario.Models.Requests;

public sealed class UpsertScenarioSprintRequest
{
    [Required]
    public string Focus { get; set; } = string.Empty;

    /// <summary>Số ngày hoặc tuần (tối thiểu 1).</summary>
    [Range(1, 365)]
    public int DurationValue { get; set; } = 1;

    /// <summary>"days" hoặc "weeks".</summary>
    public string DurationUnit { get; set; } = "weeks";

    public List<string>? TemplateCodes { get; set; }
}

public sealed class ReplaceScenarioSprintsRequest
{
    [Required]
    [MinLength(1)]
    public List<UpsertScenarioSprintRequest> Sprints { get; set; } = [];
}
