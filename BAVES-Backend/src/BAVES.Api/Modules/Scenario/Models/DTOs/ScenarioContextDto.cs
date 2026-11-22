namespace BAVES.Api.Modules.Scenario.Models.DTOs;

/// <summary>
/// Context "core" của một Scenario Version, dùng để Learner đọc trước khi
/// bắt đầu phỏng vấn các stakeholder. Chỉ chứa các trường mô tả nghiệp vụ,
/// KHÔNG bao gồm personas, knowledge items, business rules, hay events.
/// </summary>
public sealed record ScenarioContextDto(
    int Id,
    int ScenarioId,
    string ScenarioCode,
    string ScenarioTitle,
    int VersionNo,
    string? Summary,
    string BusinessContext,
    string BusinessProblem,
    string BusinessObjectives,
    string? BARole,
    string? Assignment,
    string? InScope,
    string? OutOfScope,
    string? KnownConstraints,
    string? Difficulty,
    DateTimeOffset? PublishedAt,
    bool IsFreeAccess = false
);
