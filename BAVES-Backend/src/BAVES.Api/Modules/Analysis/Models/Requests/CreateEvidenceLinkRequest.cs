using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Analysis.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.Requests;

/// <summary>
/// Body cho POST /api/analysis-items/{itemId}/evidence.
/// Tạo một liên kết "bằng chứng" giữa một mục phân tích và một tin nhắn
/// trong transcript phỏng vấn stakeholder.
/// </summary>
public sealed record CreateEvidenceLinkRequest
{
    /// <summary>
    /// Id của tin nhắn trong transcript của session (bảng [simulation].[Messages]).
    /// Tin nhắn phải thuộc đúng session chứa mục phân tích này.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int MessageId { get; init; }

    /// <summary>
    /// Loại quan hệ bằng chứng:
    /// Supports (0), Contradicts (1), Clarifies (2), Confirms (3).
    /// </summary>
    public EvidenceRelationshipType RelationshipType { get; init; }
}
