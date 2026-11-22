using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Identity.Models.Requests;

public sealed class RegisterRequest
{
    [Required, MaxLength(200)]
    public string FullName { get; init; } = null!;

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; init; } = null!;

    [Required, StringLength(128, MinimumLength = 8)]
    public string Password { get; init; } = null!;
}
