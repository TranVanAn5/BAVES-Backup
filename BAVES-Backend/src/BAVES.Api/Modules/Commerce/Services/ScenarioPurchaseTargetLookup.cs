using BAVES.Api.Modules.Commerce.Services.Interfaces;
using BAVES.Api.Modules.Content.Data;
using BAVES.Api.Modules.Scenario.Data;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Commerce.Services;

public sealed class ScenarioPurchaseTargetLookup(ScenarioDbContext scenarios, ContentDbContext content)
    : IScenarioPurchaseTargetLookup
{
    public async Task<ScenarioPurchaseTarget?> FindByVersionIdAsync(int scenarioVersionId, CancellationToken ct = default)
    {
        var version = await scenarios.ScenarioVersions
            .AsNoTracking()
            .Where(v => v.Id == scenarioVersionId)
            .Select(v => new
            {
                v.ScenarioId,
                v.Scenario.ApplicationId,
                v.IsFreeAccess,
                Difficulty = v.Difficulty.ToString()
            })
            .FirstOrDefaultAsync(ct);

        if (version is null)
        {
            return null;
        }

        var industryId = await content.Applications
            .AsNoTracking()
            .Where(application => application.Id == version.ApplicationId)
            .Select(application => (int?)application.IndustryId)
            .FirstOrDefaultAsync(ct);

        return new ScenarioPurchaseTarget(
            version.ScenarioId,
            version.ApplicationId,
            industryId,
            version.IsFreeAccess,
            version.Difficulty);
    }
}