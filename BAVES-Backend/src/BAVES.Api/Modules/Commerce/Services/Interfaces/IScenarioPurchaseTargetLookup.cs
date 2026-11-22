namespace BAVES.Api.Modules.Commerce.Services.Interfaces;

public sealed record ScenarioPurchaseTarget(
    int ScenarioId,
    int ApplicationId,
    int? IndustryId,
    bool IsFreeAccess = false,
    string? Difficulty = null);

public interface IScenarioPurchaseTargetLookup
{
    Task<ScenarioPurchaseTarget?> FindByVersionIdAsync(int scenarioVersionId, CancellationToken ct = default);
}
