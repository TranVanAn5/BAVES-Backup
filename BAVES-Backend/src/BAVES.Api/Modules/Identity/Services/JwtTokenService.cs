using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BAVES.Api.Modules.Identity.Models;
using BAVES.Api.Modules.Identity.Models.Entities;
using BAVES.Api.Modules.Identity.Models.Enums;
using BAVES.Api.Modules.Identity.Repositories.Interfaces;
using BAVES.Api.Modules.Identity.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BAVES.Api.Modules.Identity.Services;

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock,
    UserManager<ApplicationUser> users, IAuthRepository repository) : IJwtTokenService
{
    public const string SecurityStampClaim = "security_stamp";
    public const string RoleClaim = "role";

    public IssuedAccessToken CreateAccessToken(ApplicationUser user, IReadOnlyList<string> roles,
        string familyId, DateTimeOffset sessionExpiresAt)
    {
        var settings = options.Value;
        var now = clock.GetUtcNow();
        var expiry = now.AddMinutes(settings.AccessTokenMinutes);
        if (expiry > sessionExpiresAt) expiry = sessionExpiresAt;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            new(JwtRegisteredClaimNames.Sid, familyId),
            new(SecurityStampClaim, user.SecurityStamp!),
            new("name", user.FullName)
        };
        if (user.Email is not null) claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        claims.AddRange(roles.Select(role => new Claim(RoleClaim, role)));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            now.UtcDateTime, expiry.UtcDateTime, credentials);
        return new IssuedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiry);
    }

    public IssuedRefreshToken CreateRefreshToken(ApplicationUser user, string? familyId = null,
        DateTimeOffset? expiresAt = null)
    {
        familyId ??= Guid.NewGuid().ToString("N");
        var now = clock.GetUtcNow();
        var expiry = expiresAt ?? now.AddDays(options.Value.RefreshTokenDays);
        var secret = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        var token = $"{user.Id.ToString(CultureInfo.InvariantCulture)}.{familyId}.{secret}";
        var name = familyId + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        var state = JsonSerializer.Serialize(new RefreshTokenState(user.SecurityStamp!, now, expiry));
        return new IssuedRefreshToken(token, familyId, new StoredRefreshToken(user.Id, name, state), expiry);
    }

    public async Task<bool> ValidateSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var stamp = principal.FindFirstValue(SecurityStampClaim);
        if (string.IsNullOrWhiteSpace(stamp)) return false;
        if (!int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), NumberStyles.None,
            CultureInfo.InvariantCulture, out var userId) || userId <= 0 ||
            !Guid.TryParseExact(principal.FindFirstValue(JwtRegisteredClaimNames.Sid), "N", out var family)) return false;
        var user = await users.FindByIdAsync(userId.ToString(CultureInfo.InvariantCulture));
        if (user is null || user.Status != UserStatus.Active || user.TwoFactorEnabled ||
            user.SecurityStamp != stamp ||
            await users.IsLockedOutAsync(user)) return false;
        if (!await repository.IsFamilyActiveAsync(user.Id, family.ToString("N"), clock.GetUtcNow(), cancellationToken)) return false;
        var activeRoles = await repository.GetActiveRolesAsync(user.Id, cancellationToken);
        return principal.FindAll(RoleClaim).All(claim => activeRoles.Contains(claim.Value, StringComparer.Ordinal));
    }

    public static bool TryParseRefreshToken(string token, out int userId, out string familyId, out string name)
    {
        userId = 0;
        familyId = name = string.Empty;
        if (string.IsNullOrWhiteSpace(token) || token.Length > 256) return false;
        var parts = token.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out userId) ||
            userId <= 0 || !Guid.TryParseExact(parts[1], "N", out var family) || parts[2].Length != 43) return false;
        try
        {
            if (Base64UrlEncoder.DecodeBytes(parts[2]).Length != 32) return false;
        }
        catch (FormatException)
        {
            return false;
        }
        familyId = family.ToString("N");
        name = familyId + ":" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        return true;
    }
}
