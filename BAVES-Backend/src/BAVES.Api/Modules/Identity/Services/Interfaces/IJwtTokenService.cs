using System.Security.Claims;
using BAVES.Api.Modules.Identity.Models;
using BAVES.Api.Modules.Identity.Models.Entities;

namespace BAVES.Api.Modules.Identity.Services.Interfaces;

public interface IJwtTokenService
{
    IssuedAccessToken CreateAccessToken(ApplicationUser user, IReadOnlyList<string> roles,
        string familyId, DateTimeOffset sessionExpiresAt);
    IssuedRefreshToken CreateRefreshToken(ApplicationUser user, string? familyId = null,
        DateTimeOffset? expiresAt = null);
    Task<bool> ValidateSessionAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}
