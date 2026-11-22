namespace BAVES.Api.Modules.Scenario.Models.Requests;

public sealed record ValidateAiConnectionRequest
{
    public string? Provider { get; init; }
    public string? Model { get; init; }
    public string? ApiKey { get; init; }
}
