using System.Globalization;
using System.Text.Json;
using BAVES.Api.Modules.Identity.Models;
using BAVES.Api.Modules.Identity.Models.DTOs;
using BAVES.Api.Modules.Identity.Models.Entities;
using BAVES.Api.Modules.Identity.Models.Enums;
using BAVES.Api.Modules.Identity.Models.Requests;
using BAVES.Api.Modules.Identity.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using BAVES.Api.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Identity.Services;

public sealed class AuthService(UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles,
    SignInManager<ApplicationUser> signIn, IAuthRepository repository, IJwtTokenService tokens,
    TimeProvider clock) : IAuthService
{
    private const string LearnerRole = "LEARNER";
    private static readonly ApplicationUser DummyUser = new();
    private static readonly string DummyHash = new PasswordHasher<ApplicationUser>().HashPassword(DummyUser, Guid.NewGuid().ToString());

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var fullName = request.FullName.Trim();
        if (fullName.Length == 0) throw new ArgumentException("Full name is required.");
        if (await users.FindByEmailAsync(email) is not null) throw new ConflictException("Email is already registered.");
        await EnsureLearnerRoleAsync();
        try
        {
            return await repository.InTransactionAsync(async () =>
            {
                var user = new ApplicationUser
                {
                    UserName = "user-" + Guid.NewGuid().ToString("N"), Email = email, FullName = fullName,
                    Status = UserStatus.Active, CreatedAt = clock.GetUtcNow(), LockoutEnabled = true
                };
                EnsureSuccess(await users.CreateAsync(user, request.Password));
                EnsureSuccess(await users.AddToRoleAsync(user, LearnerRole));
                return await CreateSessionAsync(user, cancellationToken);
            }, cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictException("Email is already registered.");
        }
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            users.PasswordHasher.VerifyHashedPassword(DummyUser, DummyHash, request.Password);
            throw InvalidCredentials();
        }
        if (user.Status != UserStatus.Active || user.TwoFactorEnabled) throw InvalidCredentials();
        var result = await signIn.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded) throw InvalidCredentials();
        return await repository.InTransactionAsync(() => CreateSessionAsync(user, cancellationToken), cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (!JwtTokenService.TryParseRefreshToken(request.RefreshToken, out var userId, out var familyId, out var name))
            throw InvalidRefresh();
        var original = await repository.FindRefreshTokenAsync(userId, name, cancellationToken);
        if (original is null) throw InvalidRefresh();
        RefreshTokenState? state;
        try { state = JsonSerializer.Deserialize<RefreshTokenState>(original.Value); }
        catch (JsonException) { throw InvalidRefresh(); }
        if (state is null) throw InvalidRefresh();
        if (state.RevokedAt is not null)
        {
            await repository.RevokeFamilyAsync(userId, familyId, cancellationToken);
            throw InvalidRefresh();
        }
        var user = await users.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
        var now = clock.GetUtcNow();
        if (state.ExpiresAt <= now || user is null || user.Status != UserStatus.Active || user.TwoFactorEnabled ||
            user.SecurityStamp != state.SecurityStamp || await users.IsLockedOutAsync(user) ||
            !await repository.IsFamilyActiveAsync(userId, familyId, now, cancellationToken))
        {
            await repository.RevokeFamilyAsync(userId, familyId, cancellationToken);
            throw InvalidRefresh();
        }
        var refresh = tokens.CreateRefreshToken(user, familyId, state.ExpiresAt);
        var revoked = JsonSerializer.Serialize(state with { RevokedAt = now });
        if (!await repository.RotateRefreshTokenAsync(original, revoked, refresh.Stored, cancellationToken))
        {
            await repository.RevokeFamilyAsync(userId, familyId, cancellationToken);
            throw InvalidRefresh();
        }
        return await CreateResponseAsync(user, refresh, cancellationToken);
    }

    public async Task LogoutAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (JwtTokenService.TryParseRefreshToken(request.RefreshToken, out var userId, out var familyId, out var name) &&
            await repository.FindRefreshTokenAsync(userId, name, cancellationToken) is not null)
            await repository.RevokeFamilyAsync(userId, familyId, cancellationToken);
    }

    private async Task<AuthResponseDto> CreateSessionAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var refresh = tokens.CreateRefreshToken(user);
        await repository.SaveRefreshTokenAsync(refresh.Stored, cancellationToken);
        // The active-session marker makes JWT revocation checks a composite-key lookup.
        await repository.SaveRefreshTokenAsync(new StoredRefreshToken(user.Id, refresh.FamilyId + ":active",
            refresh.Stored.Value), cancellationToken);
        return await CreateResponseAsync(user, refresh, cancellationToken);
    }

    private async Task<AuthResponseDto> CreateResponseAsync(ApplicationUser user, IssuedRefreshToken refresh,
        CancellationToken cancellationToken)
    {
        var activeRoles = await repository.GetActiveRolesAsync(user.Id, cancellationToken);
        var access = tokens.CreateAccessToken(user, activeRoles, refresh.FamilyId, refresh.ExpiresAt);
        return new AuthResponseDto(access.Value, access.ExpiresAt, refresh.Value, refresh.ExpiresAt,
            UserService.ToProfile(user, activeRoles));
    }

    private async Task EnsureLearnerRoleAsync()
    {
        var role = await roles.FindByNameAsync(LearnerRole);
        if (role is null)
        {
            try
            {
                var result = await roles.CreateAsync(new ApplicationRole
                {
                    Name = LearnerRole, Description = "BA-VES learner", IsActive = true, CreatedAt = clock.GetUtcNow()
                });
                if (!result.Succeeded && !result.Errors.All(e => e.Code == "DuplicateRoleName")) EnsureSuccess(result);
            }
            catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
            {
                // A concurrent registration may have created the default role first.
            }
            role = await roles.FindByNameAsync(LearnerRole);
        }
        if (role is not { IsActive: true }) throw new InvalidOperationException("Learner registration is unavailable.");
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (result.Succeeded) return;
        if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
            throw new ConflictException("Email is already registered.");
        throw new RequestValidationException(result.Errors.GroupBy(e => e.Code)
            .ToDictionary(group => group.Key, group => group.Select(e => e.Description).ToArray()));
    }

    private static UnauthorizedAccessException InvalidCredentials() => new("Invalid email or password.");
    private static UnauthorizedAccessException InvalidRefresh() => new("Refresh token is invalid or expired. Please sign in again.");
}
