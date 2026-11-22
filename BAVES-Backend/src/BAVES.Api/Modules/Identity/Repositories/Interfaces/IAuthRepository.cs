using BAVES.Api.Modules.Identity.Models;
using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Models.Enums;

namespace BAVES.Api.Modules.Identity.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<T> InTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken);
    Task<StoredRefreshToken?> FindRefreshTokenAsync(int userId, string name, CancellationToken cancellationToken);
    Task SaveRefreshTokenAsync(StoredRefreshToken token, CancellationToken cancellationToken);
    Task<bool> RotateRefreshTokenAsync(StoredRefreshToken original, string revokedValue,
        StoredRefreshToken replacement, CancellationToken cancellationToken);
    Task RevokeFamilyAsync(int userId, string familyId, CancellationToken cancellationToken);
    Task<bool> IsFamilyActiveAsync(int userId, string familyId, DateTimeOffset now, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetActiveRolesAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(
        string? search,
        string? role,
        UserStatus? status,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> SetUserRolesAsync(
        int targetUserId,
        int assignedByUserId,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken);
}
