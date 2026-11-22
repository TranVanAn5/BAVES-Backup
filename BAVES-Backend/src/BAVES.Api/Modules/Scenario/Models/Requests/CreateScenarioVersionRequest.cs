namespace BAVES.Api.Modules.Scenario.Models.Requests;

public sealed record CreateScenarioVersionRequest
{
    public string? Summary { get; init; }
    public bool CopyFromLatest { get; init; } = true;
}
