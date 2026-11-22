using BAVES.Api.Modules.Analysis.Data;
using BAVES.Api.Modules.Analysis.Models.DTOs;
using BAVES.Api.Modules.Analysis.Models.Entities;
using BAVES.Api.Modules.Analysis.Models.Enums;
using BAVES.Api.Modules.Analysis.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Analysis.Repositories;

/// <summary>
/// Hiện thực truy vấn Analysis trên AnalysisDbContext.
/// </summary>
public sealed class AnalysisItemRepository(AnalysisDbContext db) : IAnalysisItemRepository
{
    public async Task<IReadOnlyList<AnalysisItemDto>> GetItemsBySessionAsync(
        int sessionId,
        AnalysisItemType? itemType,
        AnalysisItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = db.AnalysisItems
            .AsNoTracking()
            .Where(i => i.SessionId == sessionId);

        if (itemType.HasValue)
        {
            query = query.Where(i => i.ItemType == itemType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        return await query
            .OrderBy(i => i.CreatedAt)
            .ThenBy(i => i.Id)
            .Select(i => new AnalysisItemDto(
                i.Id,
                i.SessionId,
                i.ParentAnalysisItemId,
                i.ItemType,
                i.Code,
                i.Title,
                i.Description,
                i.Priority,
                i.Status,
                i.MetadataJson,
                i.CreatedByUserId,
                i.CreatedAt,
                i.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountItemsByTypeAsync(
        int sessionId,
        AnalysisItemType itemType,
        CancellationToken cancellationToken = default)
    {
        return await db.AnalysisItems
            .AsNoTracking()
            .CountAsync(i => i.SessionId == sessionId && i.ItemType == itemType, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(
        int sessionId,
        string code,
        CancellationToken cancellationToken = default)
    {
        return await db.AnalysisItems
            .AsNoTracking()
            .AnyAsync(i => i.SessionId == sessionId && i.Code == code, cancellationToken);
    }

    public async Task<AnalysisItem?> GetItemByIdAsync(
        int sessionId,
        int itemId,
        CancellationToken cancellationToken = default)
    {
        return await db.AnalysisItems
            .FirstOrDefaultAsync(i => i.Id == itemId && i.SessionId == sessionId, cancellationToken);
    }

    public async Task<AnalysisItem?> GetItemEntityByIdAsync(
        int itemId,
        CancellationToken cancellationToken = default)
    {
        return await db.AnalysisItems
            .FirstOrDefaultAsync(i => i.Id == itemId, cancellationToken);
    }

    public void AddItem(AnalysisItem item)
    {
        db.AnalysisItems.Add(item);
    }

    public void ArchiveItem(AnalysisItem item)
    {
        item.Status = AnalysisItemStatus.Archived;
        item.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public async Task<IReadOnlyList<MessageEvidenceLink>> GetEvidenceLinksByItemIdAsync(
        int analysisItemId,
        CancellationToken cancellationToken = default)
    {
        return await db.MessageEvidenceLinks
            .AsNoTracking()
            .Where(l => l.AnalysisItemId == analysisItemId)
            .OrderBy(l => l.CreatedAt)
            .ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<MessageEvidenceLink?> GetEvidenceLinkAsync(
        int analysisItemId,
        int linkId,
        CancellationToken cancellationToken = default)
    {
        return await db.MessageEvidenceLinks
            .FirstOrDefaultAsync(
                l => l.Id == linkId && l.AnalysisItemId == analysisItemId,
                cancellationToken);
    }

    public async Task<bool> EvidenceLinkExistsAsync(
        int analysisItemId,
        int messageId,
        EvidenceRelationshipType relationshipType,
        CancellationToken cancellationToken = default)
    {
        return await db.MessageEvidenceLinks
            .AsNoTracking()
            .AnyAsync(
                l => l.AnalysisItemId == analysisItemId
                     && l.MessageId == messageId
                     && l.RelationshipType == relationshipType,
                cancellationToken);
    }

    public void AddEvidenceLink(MessageEvidenceLink link)
    {
        db.MessageEvidenceLinks.Add(link);
    }

    public void RemoveEvidenceLink(MessageEvidenceLink link)
    {
        db.MessageEvidenceLinks.Remove(link);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return db.SaveChangesAsync(cancellationToken);
    }
}
