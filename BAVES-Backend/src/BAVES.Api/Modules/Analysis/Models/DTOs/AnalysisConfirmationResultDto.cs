using BAVES.Api.Modules.Analysis.Models.Enums;

namespace BAVES.Api.Modules.Analysis.Models.DTOs;

/// <summary>
/// Kết quả khi Học viên gửi một Analysis Item cho AI Stakeholder xác nhận.
/// </summary>
public sealed record AnalysisConfirmationResultDto(
    int AnalysisItemId,
    string Code,
    string Title,
    AnalysisItemStatus ItemStatus,
    int AgentId,
    int PersonaId,
    string PersonaName,
    string StakeholderRole,
    string Outcome,
    string Reasoning,
    string? SuggestedClarification,
    IReadOnlyList<int> EvidenceMessageIds,
    DateTimeOffset ConfirmedAt
);

/// <summary>
/// Thông tin chi tiết liên kết bằng chứng kèm nội dung tin nhắn trích dẫn từ buổi phỏng vấn.
/// </summary>
public sealed record EvidenceLinkDetailDto(
    int Id,
    int AnalysisItemId,
    int MessageId,
    EvidenceRelationshipType RelationshipType,
    string? SenderType,
    string? MessageContent,
    DateTimeOffset CreatedAt
);
