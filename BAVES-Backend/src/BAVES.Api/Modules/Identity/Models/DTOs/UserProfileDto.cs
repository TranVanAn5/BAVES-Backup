namespace BAVES.Api.Modules.Identity.Models.DTOs;

public sealed record UserProfileDto(
    int Id, string? Email, string FullName, string? AvatarUrl,
    string Status, IReadOnlyList<string> Roles, DateTimeOffset CreatedAt);

public sealed record UserSummaryDto(
    int Id,
    string? Email,
    string FullName,
    string? AvatarUrl,
    string Status);
