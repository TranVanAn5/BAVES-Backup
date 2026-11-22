using BAVES.Api.Modules.Analysis.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.DTOs;

/// <summary>
/// Liên kết bằng chứng giữa một mục phân tích và một tin nhắn trong transcript.
/// Trả về cho client sau khi tạo (POST) hoặc gỡ (DELETE) liên kết.
/// </summary>
public sealed record EvidenceLinkDto(
    int Id,
    int AnalysisItemId,
    int MessageId,
    EvidenceRelationshipType RelationshipType,
    DateTimeOffset CreatedAt);
