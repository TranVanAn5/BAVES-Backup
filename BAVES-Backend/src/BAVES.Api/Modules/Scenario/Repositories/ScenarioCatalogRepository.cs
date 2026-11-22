using BAVES.Api.Modules.Scenario.Data;
using BAVES.Api.Modules.Scenario.Models.DTOs;
using BAVES.Api.Modules.Scenario.Models.Enums;
using BAVES.Api.Modules.Scenario.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Scenario.Repositories;

/// <summary>
/// Hiện thực truy vấn catalogue scenario trên ScenarioDbContext.
/// </summary>
public sealed class ScenarioCatalogRepository(ScenarioDbContext db) : IScenarioCatalogRepository
{
    public async Task<IReadOnlyList<ScenarioCatalogItemDto>> GetScenariosAsync(
        int? applicationId,
        ScenarioCatalogVisibilityFilter? visibility = null,
        CancellationToken cancellationToken = default)
    {
        var query = db.Scenarios
            .AsNoTracking()
            .Where(s => s.Status == ScenarioStatus.Published);

        if (applicationId.HasValue)
        {
            query = query.Where(s => s.ApplicationId == applicationId.Value);
        }

        if (visibility is not null && !visibility.SeeAllPublished)
        {
            if (visibility.OwnerUserId.HasValue)
            {
                var ownerId = visibility.OwnerUserId.Value;
                var includeFree = visibility.IncludeFreeAccessPublished;
                // Instructor: scenario mình tạo + (tuỳ chọn) Scenario Library (IsFreeAccess).
                query = query.Where(s =>
                    s.CreatedByUserId == ownerId ||
                    (includeFree &&
                     s.Versions.Any(v => v.IsPublished && v.IsFreeAccess)));
            }
            else if (visibility.AllowedPublishedVersionIds is not null)
            {
                var allowedIds = visibility.AllowedPublishedVersionIds;
                var includeFree = visibility.IncludeFreeAccessPublished;
                query = query.Where(s =>
                    s.Versions.Any(v =>
                        v.IsPublished &&
                        (allowedIds.Contains(v.Id) || (includeFree && v.IsFreeAccess))));
            }
            else
            {
                // Không phải Admin / Instructor / Learner được cấu hình → không lộ catalogue.
                return Array.Empty<ScenarioCatalogItemDto>();
            }
        }

        var rows = await query
            .OrderBy(s => s.ApplicationId)
            .ThenBy(s => s.Title)
            .Select(s => new
            {
                s.Id,
                s.ApplicationId,
                s.Code,
                s.Title,
                s.Description,
                s.Status,
                HasPublishedVersion = s.Versions.Any(v => v.IsPublished),
                s.CreatedAt,
                s.UpdatedAt,
                Published = s.Versions
                    .Where(v => v.IsPublished)
                    .OrderByDescending(v => v.VersionNo)
                    .Select(v => new { v.Id, v.Difficulty, v.IsFreeAccess })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        // Learner: ưu tiên published version đúng bài được giao (không luôn lấy latest).
        if (visibility?.AllowedPublishedVersionIds is not null
            && visibility is { SeeAllPublished: false, OwnerUserId: null })
        {
            var allowedSet = visibility.AllowedPublishedVersionIds.ToHashSet();
            var scenarioIds = rows.Select(r => r.Id).ToList();
            var versionPick = await db.ScenarioVersions
                .AsNoTracking()
                .Where(v => scenarioIds.Contains(v.ScenarioId) && v.IsPublished)
                .Select(v => new { v.Id, v.ScenarioId, v.VersionNo, v.Difficulty, v.IsFreeAccess })
                .ToListAsync(cancellationToken);

            return rows
                .Select(s =>
                {
                    var versions = versionPick.Where(v => v.ScenarioId == s.Id).ToList();
                    var preferred = versions
                        .Where(v => allowedSet.Contains(v.Id))
                        .OrderByDescending(v => v.VersionNo)
                        .FirstOrDefault()
                        ?? versions
                            .Where(v => visibility.IncludeFreeAccessPublished && v.IsFreeAccess)
                            .OrderByDescending(v => v.VersionNo)
                            .FirstOrDefault();

                    if (preferred is null)
                    {
                        return null;
                    }

                    return new ScenarioCatalogItemDto(
                        s.Id,
                        s.ApplicationId,
                        s.Code,
                        s.Title,
                        s.Description,
                        s.Status,
                        true,
                        s.CreatedAt,
                        s.UpdatedAt,
                        preferred.Id,
                        preferred.Difficulty.ToString(),
                        preferred.IsFreeAccess);
                })
                .Where(x => x is not null)
                .Cast<ScenarioCatalogItemDto>()
                .ToList();
        }

        return rows
            .Select(s => new ScenarioCatalogItemDto(
                s.Id,
                s.ApplicationId,
                s.Code,
                s.Title,
                s.Description,
                s.Status,
                s.HasPublishedVersion,
                s.CreatedAt,
                s.UpdatedAt,
                s.Published?.Id,
                s.Published?.Difficulty.ToString(),
                s.Published?.IsFreeAccess ?? false))
            .ToList();
    }
}
