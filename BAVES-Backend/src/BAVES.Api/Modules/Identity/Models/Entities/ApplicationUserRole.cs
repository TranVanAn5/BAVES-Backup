using Microsoft.AspNetCore.Identity;

namespace BAVES.Api.Modules.Identity.Models.Entities;

public class ApplicationUserRole : IdentityUserRole<int>
{
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;

    public int? AssignedByUserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public ApplicationRole Role { get; set; } = null!;

    public ApplicationUser? AssignedByUser { get; set; }

}
