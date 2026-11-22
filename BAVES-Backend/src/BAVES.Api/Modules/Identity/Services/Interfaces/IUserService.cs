using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Models.Enums;
using BAVES.Api.Modules.Identity.Models.Requests;

namespace BAVES.Api.Modules.Identity.Services.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetMeAsync(int userId, CancellationToken cancellationToken);

    Task<UserProfileDto> UpdateMyProfileAsync(
        int userId,
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default);

    Task ChangeMyPasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(
        string? search,
        string? role,
        UserStatus? status,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto?> GetUserByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto?> UpdateUserRolesAsync(
        int adminUserId,
        int targetUserId,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto?> UpdateUserStatusAsync(
        int adminUserId,
        int targetUserId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto> CreateUserAsync(
        int adminUserId,
        CreateAdminUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserProfileDto?> UpdateUserAsync(
        int adminUserId,
        int targetUserId,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken = default);

    Task<UserSummaryDto?> GetUserSummaryByIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<UserSummaryDto?> GetUserSummaryByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<int, UserSummaryDto>> GetUserSummariesByIdsAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserSummaryDto>> GetUserSummariesByEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken = default);
}
