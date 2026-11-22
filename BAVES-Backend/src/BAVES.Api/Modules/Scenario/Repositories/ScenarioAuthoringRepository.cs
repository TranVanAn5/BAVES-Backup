using BAVES.Api.Modules.Content.Data;
using BAVES.Api.Modules.Scenario.Data;
using BAVES.Api.Modules.Scenario.Models.Entities;
using BAVES.Api.Modules.Scenario.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using ScenarioEntity = BAVES.Api.Modules.Scenario.Models.Entities.Scenario;

namespace BAVES.Api.Modules.Scenario.Repositories;

/// <summary>
/// Triển khai các truy vấn cơ sở dữ liệu (EF Core) cho nghiệp vụ Scenario Authoring (Backend B).
/// </summary>
public sealed class ScenarioAuthoringRepository(
    ScenarioDbContext scenarioDb,
    ContentDbContext contentDb
) : IScenarioAuthoringRepository
{
    public Task<bool> ApplicationExistsAsync(
        int applicationId,
        CancellationToken cancellationToken = default
    ) =>
        contentDb.Applications
            .AnyAsync(a => a.Id == applicationId, cancellationToken);

    public Task<bool> ScenarioCodeExistsAsync(
        string code,
        CancellationToken cancellationToken = default
    ) =>
        scenarioDb.Scenarios
            .AnyAsync(s => s.Code == code, cancellationToken);

    public async Task AddScenarioAsync(
        ScenarioEntity scenario,
        CancellationToken cancellationToken = default
    )
    {
        scenarioDb.Scenarios.Add(scenario);
        await scenarioDb.SaveChangesAsync(cancellationToken);
    }

    public Task<ScenarioEntity?> GetScenarioWithVersionsAsync(
        int scenarioId,
        CancellationToken cancellationToken = default
    ) =>
        scenarioDb.Scenarios
            .Include(s => s.Versions)
            .FirstOrDefaultAsync(s => s.Id == scenarioId, cancellationToken);

    public Task<ScenarioVersion?> GetLatestVersionWithAggregateAsync(
        int scenarioId,
        CancellationToken cancellationToken = default
    ) =>
        scenarioDb.ScenarioVersions
            .Where(v => v.ScenarioId == scenarioId)
            .OrderByDescending(v => v.VersionNo)
            .Include(v => v.StakeholderPersonas)
                .ThenInclude(p => p.StakeholderKnowledgeAccess)
            .Include(v => v.KnowledgeItems)
            .Include(v => v.BusinessRules)
            .Include(v => v.ScenarioEvents)
            .Include(v => v.Sprints.OrderBy(s => s.SprintNo))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddScenarioVersionAsync(
        ScenarioVersion version,
        CancellationToken cancellationToken = default
    )
    {
        scenarioDb.ScenarioVersions.Add(version);
        await scenarioDb.SaveChangesAsync(cancellationToken);
    }

    public Task<ScenarioVersion?> GetVersionDefinitionAsync(
        int versionId,
        CancellationToken cancellationToken = default
    ) =>
        scenarioDb.ScenarioVersions
            .Include(v => v.Scenario)
            .Include(v => v.StakeholderPersonas.OrderBy(p => p.DisplayOrder))
            .Include(v => v.KnowledgeItems.OrderBy(k => k.Code))
                .ThenInclude(k => k.StakeholderKnowledgeAccess)
                    .ThenInclude(a => a.StakeholderPersona)
            .Include(v => v.BusinessRules.OrderBy(r => r.Code))
            .Include(v => v.ScenarioEvents.OrderBy(e => e.DisplayOrder))
            .Include(v => v.Sprints.OrderBy(s => s.SprintNo))
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

    public Task<ScenarioVersion?> GetVersionWithAggregateForUpdateAsync(
        int versionId,
        CancellationToken cancellationToken = default
    ) =>
        scenarioDb.ScenarioVersions
            .Include(v => v.Scenario)
            .Include(v => v.StakeholderPersonas)
                .ThenInclude(p => p.StakeholderKnowledgeAccess)
            .Include(v => v.KnowledgeItems)
            .Include(v => v.BusinessRules)
            .Include(v => v.ScenarioEvents)
            .Include(v => v.Sprints)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

    public void ClearVersionAggregate(ScenarioVersion version)
    {
        scenarioDb.StakeholderKnowledgeAccess.RemoveRange(
            version.StakeholderPersonas.SelectMany(p => p.StakeholderKnowledgeAccess));
        scenarioDb.StakeholderPersonas.RemoveRange(version.StakeholderPersonas);
        scenarioDb.KnowledgeItems.RemoveRange(version.KnowledgeItems);
        scenarioDb.BusinessRules.RemoveRange(version.BusinessRules);
        scenarioDb.ScenarioEvents.RemoveRange(version.ScenarioEvents);
        // Sprints không xóa trong ClearVersionAggregate — cập nhật riêng / ReplaceSprints.
    }

    public async Task DeleteVersionChildrenAsync(int versionId, CancellationToken cancellationToken = default)
    {
        var personaIds = await scenarioDb.StakeholderPersonas
            .Where(p => p.ScenarioVersionId == versionId)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
        var knowledgeIds = await scenarioDb.KnowledgeItems
            .Where(k => k.ScenarioVersionId == versionId)
            .Select(k => k.Id)
            .ToListAsync(cancellationToken);

        if (personaIds.Count > 0 || knowledgeIds.Count > 0)
        {
            await scenarioDb.StakeholderKnowledgeAccess
                .Where(a =>
                    personaIds.Contains(a.StakeholderPersonaId) ||
                    knowledgeIds.Contains(a.KnowledgeItemId))
                .ExecuteDeleteAsync(cancellationToken);
        }

        await scenarioDb.StakeholderPersonas
            .Where(p => p.ScenarioVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);
        await scenarioDb.KnowledgeItems
            .Where(k => k.ScenarioVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);
        await scenarioDb.BusinessRules
            .Where(r => r.ScenarioVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);
        await scenarioDb.ScenarioEvents
            .Where(e => e.ScenarioVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);
        await scenarioDb.ScenarioSprints
            .Where(s => s.ScenarioVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task<ScenarioVersion?> GetVersionForPublishAsync(
        int versionId,
        CancellationToken cancellationToken = default
    ) =>
        scenarioDb.ScenarioVersions
            .Include(v => v.Scenario)
            .Include(v => v.StakeholderPersonas)
                .ThenInclude(p => p.StakeholderKnowledgeAccess)
            .Include(v => v.KnowledgeItems)
            .Include(v => v.Sprints)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

    public void RemoveScenarioVersion(ScenarioVersion version) =>
        scenarioDb.ScenarioVersions.Remove(version);

    public void RemoveScenario(ScenarioEntity scenario) =>
        scenarioDb.Scenarios.Remove(scenario);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        scenarioDb.SaveChangesAsync(cancellationToken);
}
