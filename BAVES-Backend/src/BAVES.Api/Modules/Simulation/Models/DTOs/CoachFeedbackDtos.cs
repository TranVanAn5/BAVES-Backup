namespace BAVES.Api.Modules.Simulation.Models.DTOs;

/// <summary>
/// Một nhận xét của coach đã được lưu, trả về cho client.
/// Bốn nhóm nhận xét tương ứng output schema của agent coach
/// (agent-runtime/app/agents/coach/AGENT.md).
/// </summary>
public sealed record CoachFeedbackDto(
    int Id,
    int SessionId,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Gaps,
    IReadOnlyList<string> UnclearPoints,
    IReadOnlyList<string> ImprovementHints,
    string PromptVersion,
    DateTimeOffset CreatedAt);
