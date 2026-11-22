using System.ComponentModel.DataAnnotations;

namespace BAVES.Api.Modules.Identity.Models.Requests;

public sealed class RefreshTokenRequest
{
    [Required, MaxLength(256)]
    public string RefreshToken { get; init; } = null!;
}
