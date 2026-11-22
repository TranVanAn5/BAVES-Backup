using BAVES.Api.Modules.Identity.Models.Enums;
using Microsoft.AspNetCore.Identity;

namespace BAVES.Api.Modules.Identity.Models.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string FullName { get; set; } = null!;

    public string? AvatarUrl { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

    public ICollection<ApplicationUserRole> RoleAssignmentsMade { get; set; } = new List<ApplicationUserRole>();

}
