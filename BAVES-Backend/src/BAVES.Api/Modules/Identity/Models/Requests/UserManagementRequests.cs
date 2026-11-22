using System.ComponentModel.DataAnnotations;
using BAVES.Api.Modules.Identity.Models.Enums;

namespace BAVES.Api.Modules.Identity.Models.Requests;

public sealed class UpdateMyProfileRequest
{
    [Required(ErrorMessage = "FullName là bắt buộc.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "FullName phải từ 2 đến 150 ký tự.")]
    public string FullName { get; set; } = null!;

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}

public sealed class ChangePasswordRequest
{
    [Required(ErrorMessage = "CurrentPassword là bắt buộc.")]
    public string CurrentPassword { get; set; } = null!;

    [Required(ErrorMessage = "NewPassword là bắt buộc.")]
    [MinLength(8, ErrorMessage = "NewPassword phải có ít nhất 8 ký tự.")]
    public string NewPassword { get; set; } = null!;
}

public sealed class UpdateUserRolesRequest
{
    [Required(ErrorMessage = "Roles là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Người dùng phải có ít nhất 1 Role.")]
    public List<string> Roles { get; set; } = new();
}

public sealed class UpdateUserStatusRequest
{
    [Required(ErrorMessage = "Status là bắt buộc.")]
    public UserStatus Status { get; set; }
}

public sealed class CreateAdminUserRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "FullName là bắt buộc.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "FullName phải từ 2 đến 150 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Password là bắt buộc.")]
    [MinLength(8, ErrorMessage = "Password phải có ít nhất 8 ký tự.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Roles là bắt buộc.")]
    [MinLength(1, ErrorMessage = "Người dùng phải có ít nhất 1 Role.")]
    public List<string> Roles { get; set; } = new();
}

public sealed class UpdateAdminUserRequest
{
    [Required(ErrorMessage = "FullName là bắt buộc.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "FullName phải từ 2 đến 150 ký tự.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(256)]
    public string Email { get; set; } = null!;

    [MinLength(8, ErrorMessage = "Password mới phải có ít nhất 8 ký tự.")]
    public string? NewPassword { get; set; }

    [MinLength(1, ErrorMessage = "Người dùng phải có ít nhất 1 Role.")]
    public List<string>? Roles { get; set; }
}
