using System.Globalization;
using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Models.Entities;
using BAVES.Api.Modules.Identity.Models.Enums;
using BAVES.Api.Modules.Identity.Models.Requests;
using BAVES.Api.Modules.Identity.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Identity.Services;

public sealed class UserService(UserManager<ApplicationUser> users, IAuthRepository repository) : IUserService
{
    public async Task<UserProfileDto> GetMeAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await users.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
        if (user is null || user.Status != UserStatus.Active) throw new UnauthorizedAccessException("Account is unavailable.");
        return ToProfile(user, await repository.GetActiveRolesAsync(user.Id, cancellationToken));
    }

    public async Task<UserProfileDto> UpdateMyProfileAsync(
        int userId,
        UpdateMyProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
        if (user is null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Account is unavailable.");
        }

        user.FullName = request.FullName.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();
        user.UpdatedAt = DateTimeOffset.UtcNow;

        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Profile"] = result.Errors.Select(e => e.Description).ToArray()
            });
        }

        return ToProfile(user, await repository.GetActiveRolesAsync(user.Id, cancellationToken));
    }

    public async Task ChangeMyPasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
        if (user is null || user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Account is unavailable.");
        }

        var result = await users.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Password"] = result.Errors.Select(e => e.Description).ToArray()
            });
        }
    }

    public async Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(
        string? search,
        string? role,
        UserStatus? status,
        CancellationToken cancellationToken = default)
    {
        return await repository.GetUsersAsync(search, role, status, cancellationToken);
    }

    public async Task<UserProfileDto?> GetUserByIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
        if (user is null)
        {
            return null;
        }

        var roles = await repository.GetActiveRolesAsync(user.Id, cancellationToken);
        return ToProfile(user, roles);
    }

    public async Task<UserProfileDto?> UpdateUserRolesAsync(
        int adminUserId,
        int targetUserId,
        UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(targetUserId.ToString(CultureInfo.InvariantCulture));
        if (user is null)
        {
            return null;
        }

        var requestedRoles = request.Roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        if (requestedRoles.Count == 0)
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Roles"] = ["Người dùng phải có ít nhất 1 Role."]
            });
        }

        if (adminUserId == targetUserId && !requestedRoles.Contains("ADMIN"))
        {
            throw new ConflictException("Quản trị viên không thể tự gỡ quyền ADMIN của chính mình.");
        }

        var updatedRoles = await repository.SetUserRolesAsync(
            targetUserId, adminUserId, requestedRoles, cancellationToken);

        return ToProfile(user, updatedRoles);
    }

    public async Task<UserProfileDto?> UpdateUserStatusAsync(
        int adminUserId,
        int targetUserId,
        UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(request.Status))
        {
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Status"] = ["Status phải là Active, Suspended hoặc Archived."]
            });
        }

        if (adminUserId == targetUserId && request.Status != UserStatus.Active)
        {
            throw new ConflictException("Quản trị viên không thể tự khóa tài khoản của chính mình.");
        }

        var user = await users.FindByIdAsync(targetUserId.ToString(CultureInfo.InvariantCulture));
        if (user is null)
        {
            return null;
        }

        user.Status = request.Status;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await users.UpdateAsync(user);

        var roles = await repository.GetActiveRolesAsync(user.Id, cancellationToken);
        return ToProfile(user, roles);
    }

    public async Task<UserProfileDto> CreateUserAsync(
        int adminUserId,
        CreateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();
        var roleNames = NormalizeRoleNames(request.Roles);

        if (await users.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException("Email đã được đăng ký.");
        }

        var user = new ApplicationUser
        {
            UserName = "user-" + Guid.NewGuid().ToString("N"),
            Email = email,
            FullName = fullName,
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
            LockoutEnabled = true
        };

        var createResult = await users.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            if (createResult.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                throw new ConflictException("Email đã được đăng ký.");
            }

            throw new RequestValidationException(
                createResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        }

        var assignedRoles = await repository.SetUserRolesAsync(
            user.Id,
            adminUserId,
            roleNames,
            cancellationToken);

        return ToProfile(user, assignedRoles);
    }

    public async Task<UserProfileDto?> UpdateUserAsync(
        int adminUserId,
        int targetUserId,
        UpdateAdminUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(targetUserId.ToString(CultureInfo.InvariantCulture));
        if (user is null)
        {
            return null;
        }

        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();

        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await users.FindByEmailAsync(email);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new ConflictException("Email đã được đăng ký.");
            }

            var setEmail = await users.SetEmailAsync(user, email);
            if (!setEmail.Succeeded)
            {
                throw new RequestValidationException(
                    setEmail.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
            }
        }

        user.FullName = fullName;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        var updateResult = await users.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new RequestValidationException(
                updateResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
        }

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            var token = await users.GeneratePasswordResetTokenAsync(user);
            var reset = await users.ResetPasswordAsync(user, token, request.NewPassword);
            if (!reset.Succeeded)
            {
                throw new RequestValidationException(
                    reset.Errors
                        .GroupBy(e => e.Code)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()));
            }
        }

        IReadOnlyList<string> roles;
        if (request.Roles is { Count: > 0 })
        {
            var roleNames = NormalizeRoleNames(request.Roles);
            if (adminUserId == targetUserId && !roleNames.Contains("ADMIN", StringComparer.OrdinalIgnoreCase))
            {
                throw new ConflictException("Quản trị viên không thể tự gỡ quyền ADMIN của chính mình.");
            }

            roles = await repository.SetUserRolesAsync(
                targetUserId,
                adminUserId,
                roleNames,
                cancellationToken);
        }
        else
        {
            roles = await repository.GetActiveRolesAsync(user.Id, cancellationToken);
        }

        return ToProfile(user, roles);
    }

    private static List<string> NormalizeRoleNames(IEnumerable<string> roles) =>
        roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r =>
            {
                var n = r.Trim().ToUpperInvariant();
                return n is "BAEXPERT" or "BA_EXPERT" ? "EXPERT" : n;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public async Task<UserSummaryDto?> GetUserSummaryByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await users.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new UserSummaryDto(u.Id, u.Email, u.FullName, u.AvatarUrl, u.Status.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<UserSummaryDto?> GetUserSummaryByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalized = users.NormalizeEmail(email.Trim());
        return await users.Users
            .AsNoTracking()
            .Where(u => u.NormalizedEmail == normalized)
            .Select(u => new UserSummaryDto(u.Id, u.Email, u.FullName, u.AvatarUrl, u.Status.ToString()))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, UserSummaryDto>> GetUserSummariesByIdsAsync(
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return new Dictionary<int, UserSummaryDto>();
        }

        var list = await users.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new UserSummaryDto(u.Id, u.Email, u.FullName, u.AvatarUrl, u.Status.ToString()))
            .ToListAsync(cancellationToken);

        return list.ToDictionary(u => u.Id);
    }

    public async Task<IReadOnlyList<UserSummaryDto>> GetUserSummariesByEmailsAsync(
        IReadOnlyCollection<string> emails,
        CancellationToken cancellationToken = default)
    {
        if (emails.Count == 0)
        {
            return Array.Empty<UserSummaryDto>();
        }

        var normalizedSet = emails
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => users.NormalizeEmail(e.Trim()))
            .Distinct()
            .ToList();

        return await users.Users
            .AsNoTracking()
            .Where(u => u.NormalizedEmail != null && normalizedSet.Contains(u.NormalizedEmail))
            .Select(u => new UserSummaryDto(u.Id, u.Email, u.FullName, u.AvatarUrl, u.Status.ToString()))
            .ToListAsync(cancellationToken);
    }

    internal static UserProfileDto ToProfile(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id, user.Email, user.FullName, user.AvatarUrl, user.Status.ToString(), roles, user.CreatedAt);
}
