using BAVES.Api.Modules.Content.Data;
using BAVES.Api.Modules.Content.Models.DTOs;
using BAVES.Api.Modules.Content.Models.Enums;
using BAVES.Api.Modules.Content.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Content.Repositories;

/// <summary>
/// Mọi LINQ / raw SQL đều nằm ở đây 
/// </summary>
public sealed class ContentReadRepository(ContentDbContext db) : IContentReadRepository
{
    public async Task<IReadOnlyList<IndustryDto>> GetActiveIndustriesAsync(
        CancellationToken cancellationToken = default)
    {
        return await db.Industries
            .AsNoTracking()
            .Where(x => x.Status == ContentStatus.Active)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new IndustryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ApplicationDto>> GetActiveApplicationsAsync(
        int? industryId,
        CancellationToken cancellationToken = default)
    {
        var query = db.Applications
            .AsNoTracking()
            .Where(x => x.Status == ContentStatus.Active);

        if (industryId.HasValue)
        {
            query = query.Where(x => x.IndustryId == industryId.Value);
        }

        return await query
            .OrderBy(x => x.IndustryId)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new ApplicationDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                Status = x.Status,
                DisplayOrder = x.DisplayOrder,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                Industry = new ApplicationIndustryDto
                {
                    Id = x.Industry.Id,
                    Code = x.Industry.Code,
                    Name = x.Industry.Name
                }
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, ApplicationDto>> GetApplicationsByIdsAsync(
    IReadOnlyList<int> applicationIds,
    CancellationToken cancellationToken = default)
    {
        if (applicationIds.Count == 0)
        {
            return new Dictionary<int, ApplicationDto>();
        }

        return await db.Applications
            .AsNoTracking()
            .Where(a => applicationIds.Contains(a.Id))
            .Select(a => new ApplicationDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                Status = a.Status,
                DisplayOrder = a.DisplayOrder,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,
                Industry = new ApplicationIndustryDto
                {
                    Id = a.Industry.Id,
                    Code = a.Industry.Code,
                    Name = a.Industry.Name
                }
            })
            .ToDictionaryAsync(a => a.Id, cancellationToken);
    }

    public async Task<bool> ApplicationExistsAsync(
        int applicationId,
        CancellationToken cancellationToken = default)
    {
        // AnyAsync: chỉ cần biết "có tồn tại không" → SQL Server dừng ở EXISTS.
        return await db.Applications
            .AsNoTracking()
            .AnyAsync(a => a.Id == applicationId, cancellationToken);
    }
}
