namespace BAVES.Api.Modules.Identity.Models;

public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAt);
public sealed record IssuedRefreshToken(string Value, string FamilyId,
    StoredRefreshToken Stored, DateTimeOffset ExpiresAt);
