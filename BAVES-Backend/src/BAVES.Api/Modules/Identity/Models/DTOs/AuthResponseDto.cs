namespace BAVES.Api.Modules.Identity.Models.DTOs;

public sealed record AuthResponseDto(
    string AccessToken, DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken, DateTimeOffset RefreshTokenExpiresAt,
    UserProfileDto User)
{
    public string TokenType => "Bearer";
}
