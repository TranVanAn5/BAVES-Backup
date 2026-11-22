namespace BAVES.Api.Modules.Scenario.Models.DTOs;

public sealed record ValidateAiConnectionDto(
    bool Valid,
    string Provider,
    string? Model,
    string Message
);
