namespace BAVES.Api.Modules.Identity.Models;

public sealed record RefreshTokenState(
    string SecurityStamp, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt,
    DateTimeOffset? RevokedAt = null);

public sealed record StoredRefreshToken(int UserId, string Name, string Value);
