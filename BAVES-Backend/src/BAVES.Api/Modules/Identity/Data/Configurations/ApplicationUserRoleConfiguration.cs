using Entity = BAVES.Api.Modules.Identity.Models.Entities.ApplicationUserRole;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Identity.Data.Configurations;

public sealed class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("UserRoles", "identity");
        builder.HasKey(x => new { x.UserId, x.RoleId });
        builder.Property(x => x.AssignedAt).HasPrecision(3);
        builder.HasOne(x => x.User)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.AssignedByUser)
            .WithMany(x => x.RoleAssignmentsMade)
            .HasForeignKey(x => x.AssignedByUserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
