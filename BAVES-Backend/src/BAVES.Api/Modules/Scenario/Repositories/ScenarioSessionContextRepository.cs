using BAVES.Api.Modules.Scenario.Data;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace BAVES.Api.Modules.Scenario.Repositories;

/// <summary>
/// Hiện thực truy vấn read-only cho ngữ cảnh khởi tạo Simulation Session.
/// </summary>
public sealed class ScenarioSessionContextRepository(ScenarioDbContext db)
    : IScenarioSessionContextRepository
{
    public async Task<ScenarioVersionInfoDto?> GetVersionInfoAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await db.ScenarioVersions
            .AsNoTracking()
            .Where(v => v.Id == versionId)
            .Select(v => new ScenarioVersionInfoDto(
                v.Id,
                v.ScenarioId,
                v.VersionNo,
                v.Difficulty,
                v.IsPublished))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScenarioPersonaInfoDto>> GetActivePersonasAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await db.StakeholderPersonas
            .AsNoTracking()
            .Where(p => p.ScenarioVersionId == versionId && p.IsActive)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.Id)
            .Select(p => new ScenarioPersonaInfoDto(
                p.Id,
                p.Name,
                p.StakeholderRole,
                p.DisplayOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, ScenarioPersonaInfoDto>> GetPersonasByIdsAsync(
        IReadOnlyCollection<int> personaIds,
        CancellationToken cancellationToken = default)
    {
        if (personaIds.Count == 0)
        {
            return new Dictionary<int, ScenarioPersonaInfoDto>();
        }

        var personas = await db.StakeholderPersonas
            .AsNoTracking()
            .Where(p => personaIds.Contains(p.Id))
            .Select(p => new ScenarioPersonaInfoDto(
                p.Id,
                p.Name,
                p.StakeholderRole,
                p.DisplayOrder))
            .ToListAsync(cancellationToken);

        return personas.ToDictionary(p => p.Id);
    }

    public async Task<StakeholderScenarioContextDto?> GetScenarioInterviewContextAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        // Join ScenarioVersions với Scenarios để lấy tên công ty (companyName).
        return await db.ScenarioVersions
            .AsNoTracking()
            .Where(v => v.Id == versionId)
            .Select(v => new StakeholderScenarioContextDto(
                v.Id,
                v.Scenario.Title,
                v.BusinessContext,
                v.BusinessProblem,
                v.Assignment))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<StakeholderPersonaDto?> GetPersonaInterviewContextAsync(
        int personaId,
        CancellationToken cancellationToken = default)
    {
        // Chỉ đọc, không theo dõi thay đổi.
        return await db.StakeholderPersonas
            .AsNoTracking()
            .Where(p => p.Id == personaId)
            .Select(p => new StakeholderPersonaDto(
                p.Id,
                p.Name,
                p.StakeholderRole,
                p.Department,
                p.Responsibilities,
                p.Goals,
                p.PainPoints,
                p.BusinessConcerns,
                p.CommunicationStyle,
                p.Personality))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StakeholderKnowledgeItemDto>> GetAllowedKnowledgeAsync(
        int personaId,
        CancellationToken cancellationToken = default)
    {
        // Join StakeholderKnowledgeAccess với KnowledgeItem để lấy đúng tập knowledge
        // mà persona được gán. Đây là ranh giới an toàn: runtime KHÔNG tự truy cập DB,
        // .NET chỉ gửi những gì persona thực sự được phép biết.
        return await db.StakeholderKnowledgeAccess
            .AsNoTracking()
            .Where(a => a.StakeholderPersonaId == personaId)
            .OrderBy(a => a.KnowledgeItemId)
            .Select(a => new StakeholderKnowledgeItemDto(
                a.KnowledgeItem.Id,
                a.KnowledgeItem.Code,
                a.KnowledgeItem.Title,
                a.KnowledgeItem.Content,
                a.KnowledgeItem.Category.ToString(),
                a.KnowledgeItem.Importance.ToString(),
                a.AccessType.ToString(),
                a.IsHidden,
                a.DisclosureRule))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ScenarioEventInfoDto>> GetActiveEventsAsync(
        int versionId,
        CancellationToken cancellationToken = default)
    {
        return await db.ScenarioEvents
            .AsNoTracking()
            .Where(e => e.ScenarioVersionId == versionId && e.IsActive)
            .OrderBy(e => e.DisplayOrder)
            .ThenBy(e => e.Id)
            .Select(e => new ScenarioEventInfoDto(
                e.Id,
                e.ScenarioVersionId,
                e.Title,
                e.Description,
                e.TriggerType,
                e.EventContent,
                e.DisplayOrder,
                e.IsActive,
                e.TriggerConditionJson))
            .ToListAsync(cancellationToken);
    }

    public async Task<ScenarioEventInfoDto?> GetEventByIdAsync(
        int scenarioEventId,
        CancellationToken cancellationToken = default)
    {
        return await db.ScenarioEvents
            .AsNoTracking()
            .Where(e => e.Id == scenarioEventId)
            .Select(e => new ScenarioEventInfoDto(
                e.Id,
                e.ScenarioVersionId,
                e.Title,
                e.Description,
                e.TriggerType,
                e.EventContent,
                e.DisplayOrder,
                e.IsActive,
                e.TriggerConditionJson))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
