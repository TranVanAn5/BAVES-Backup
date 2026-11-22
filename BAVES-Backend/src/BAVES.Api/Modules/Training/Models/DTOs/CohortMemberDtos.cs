using BAVES.Api.Modules.Training.Models.Enums;

namespace BAVES.Api.Modules.Training.Models.DTOs;

/// <summary>
/// Thông tin một Học viên trong Lớp học kèm hồ sơ định danh.
/// </summary>
public sealed record CohortMemberDto(
    int Id,
    int CohortId,
    int LearnerId,
    string FullName,
    string? Email,
    string? AvatarUrl,
    CohortMemberStatus Status,
    DateTimeOffset JoinedAt
);

/// <summary>
/// Kết quả thêm học viên hàng loạt vào Lớp học.
/// </summary>
public sealed record BatchAddCohortMembersResultDto(
    int CohortId,
    int AddedCount,
    int ReactivatedCount,
    int AlreadyActiveCount,
    IReadOnlyList<string> NotFoundIdentities,
    IReadOnlyList<CohortMemberDto> AddedOrReactivatedMembers
);
