using Microsoft.AspNetCore.Identity;

namespace BAVES.Api.Modules.Identity.Models.Entities;

public class ApplicationRole : IdentityRole<int>
{
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<ApplicationUserRole> UserRoles { get; set; } = new List<ApplicationUserRole>();

}
