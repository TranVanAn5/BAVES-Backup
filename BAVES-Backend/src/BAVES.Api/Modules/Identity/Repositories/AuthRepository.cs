using System.Text.Json;
using System.Data;
using BAVES.Api.Modules.Identity.Data;
using BAVES.Api.Modules.Identity.Models;
using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Models.Entities;
using BAVES.Api.Modules.Identity.Models.Enums;
using BAVES.Api.Modules.Identity.Repositories.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Identity.Repositories;

public sealed class AuthRepository(IdentityDbContext db) : IAuthRepository
{
    public const string TokenProvider = "BAVES.RefreshToken";

    public Task<T> InTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken) =>
        db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var result = await operation();
            await transaction.CommitAsync(cancellationToken);
            return result;
        });

    public async Task<StoredRefreshToken?> FindRefreshTokenAsync(int userId, string name,
        CancellationToken cancellationToken)
    {
        var token = await db.UserTokens.AsNoTracking().SingleOrDefaultAsync(
            x => x.UserId == userId && x.LoginProvider == TokenProvider && x.Name == name,
            cancellationToken);
        return token?.Value is null ? null : new StoredRefreshToken(token.UserId, token.Name, token.Value);
    }

    public async Task SaveRefreshTokenAsync(StoredRefreshToken token, CancellationToken cancellationToken)
    {
        db.UserTokens.Add(ToEntity(token));
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<bool> RotateRefreshTokenAsync(StoredRefreshToken original, string revokedValue,
        StoredRefreshToken replacement, CancellationToken cancellationToken) =>
        db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            var consumed = await db.UserTokens
                .Where(x => x.UserId == original.UserId && x.LoginProvider == TokenProvider &&
                    x.Name == original.Name && x.Value == original.Value)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Value, revokedValue), cancellationToken);
            if (consumed != 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                return false;
            }
            await SaveRefreshTokenAsync(replacement, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        });

    public Task RevokeFamilyAsync(int userId, string familyId, CancellationToken cancellationToken) =>
        db.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
        {
            var prefix = familyId + ":";
            await using var transaction = await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            await db.UserTokens.Where(x => x.UserId == userId && x.LoginProvider == TokenProvider &&
                x.Name.StartsWith(prefix)).ExecuteDeleteAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });

    public async Task<bool> IsFamilyActiveAsync(int userId, string familyId, DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var token = await FindRefreshTokenAsync(userId, familyId + ":active", cancellationToken);
        if (token is null) return false;
        try
        {
            return JsonSerializer.Deserialize<RefreshTokenState>(token.Value) is { RevokedAt: null } state &&
                state.ExpiresAt > now;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetActiveRolesAsync(int userId, CancellationToken cancellationToken) =>
        await db.UserRoles.AsNoTracking().Where(x => x.UserId == userId && x.Role.IsActive)
            .Select(x => x.Role.Name!).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<UserProfileDto>> GetUsersAsync(
        string? search,
        string? role,
        UserStatus? status,
        CancellationToken cancellationToken)
    {
        var query = db.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(u => u.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var normalizedRole = role.Trim().ToUpperInvariant();
            query = query.Where(u => u.UserRoles.Any(ur =>
                ur.Role.IsActive && ur.Role.NormalizedName == normalizedRole));
        }

        var entities = await query
            .OrderByDescending(u => u.CreatedAt)
            .ThenBy(u => u.Id)
            .ToListAsync(cancellationToken);

        return entities.Select(u => new UserProfileDto(
            u.Id,
            u.Email,
            u.FullName,
            u.AvatarUrl,
            u.Status.ToString(),
            u.UserRoles
                .Where(ur => ur.Role.IsActive && ur.Role.Name != null)
                .Select(ur => ur.Role.Name!)
                .OrderBy(r => r)
                .ToList(),
            u.CreatedAt
        )).ToList();
    }

    public async Task<IReadOnlyList<string>> SetUserRolesAsync(
        int targetUserId,
        int assignedByUserId,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken)
    {
        var normalizedNames = roleNames
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => r.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var roles = await db.Roles
            .AsNoTracking()
            .Where(r => r.IsActive && r.NormalizedName != null && normalizedNames.Contains(r.NormalizedName))
            .ToListAsync(cancellationToken);

        if (roles.Count != normalizedNames.Count)
        {
            var foundNames = roles.Select(r => r.NormalizedName!).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var invalidRoles = normalizedNames.Where(n => !foundNames.Contains(n)).ToList();
            throw new RequestValidationException(new Dictionary<string, string[]>
            {
                ["Roles"] = [$"Role không tồn tại hoặc không khả dụng: {string.Join(", ", invalidRoles)}."]
            });
        }

        var existingUserRoles = await db.UserRoles
            .Where(ur => ur.UserId == targetUserId)
            .ToListAsync(cancellationToken);

        if (existingUserRoles.Count > 0)
        {
            db.UserRoles.RemoveRange(existingUserRoles);
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var role in roles)
        {
            db.UserRoles.Add(new ApplicationUserRole
            {
                UserId = targetUserId,
                RoleId = role.Id,
                AssignedByUserId = assignedByUserId,
                AssignedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);

        return roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .OrderBy(r => r)
            .ToList();
    }

    private static IdentityUserToken<int> ToEntity(StoredRefreshToken token) => new()
    {
        UserId = token.UserId, LoginProvider = TokenProvider, Name = token.Name, Value = token.Value
    };
}
