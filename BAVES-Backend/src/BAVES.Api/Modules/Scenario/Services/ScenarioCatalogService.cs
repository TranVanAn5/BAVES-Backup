using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Repositories.Interfaces;
using BAVES.Api.Modules.Scenario.Services.Interfaces;

namespace BAVES.Api.Modules.Scenario.Services;

/// <summary>
/// Triển khai IScenarioCatalogService: chỉ điều phối, không chứa truy vấn DB.
/// </summary>
public sealed class ScenarioCatalogService(
    IScenarioCatalogRepository repository,
    IScenarioSessionContextRepository sessionContextRepository) : IScenarioCatalogService
{
    public async Task<IReadOnlyList<ScenarioCatalogItemDto>> GetScenariosAsync(
        int? applicationId,
        ScenarioCatalogVisibilityFilter? visibility = null,
        CancellationToken cancellationToken = default)
    {
        return await repository.GetScenariosAsync(applicationId, visibility, cancellationToken);
    }

    public async Task<ScenarioVersionInfoDto?> GetVersionInfoAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetVersionInfoAsync(versionId, cancellationToken);
    }

    public async Task<IReadOnlyList<ScenarioPersonaInfoDto>> GetActivePersonasAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetActivePersonasAsync(versionId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, ScenarioPersonaInfoDto>> GetPersonasByIdsAsync(
        IReadOnlyCollection<int> personaIds,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetPersonasByIdsAsync(personaIds, cancellationToken);
    }

    public async Task<StakeholderScenarioContextDto?> GetScenarioInterviewContextAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetScenarioInterviewContextAsync(
            versionId, cancellationToken);
    }

    public async Task<StakeholderPersonaDto?> GetPersonaInterviewContextAsync(
        int personaId,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetPersonaInterviewContextAsync(
            personaId, cancellationToken);
    }

    public async Task<IReadOnlyList<StakeholderKnowledgeItemDto>> GetAllowedKnowledgeAsync(
        int personaId,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetAllowedKnowledgeAsync(
            personaId, cancellationToken);
    }

    public async Task<IReadOnlyList<ScenarioEventInfoDto>> GetActiveEventsAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetActiveEventsAsync(versionId, cancellationToken);
    }

    public async Task<ScenarioEventInfoDto?> GetEventByIdAsync(
        int scenarioEventId,
        CancellationToken cancellationToken = default)
    {
        return await sessionContextRepository.GetEventByIdAsync(scenarioEventId, cancellationToken);
    }
}
