using Entity = BAVES.Api.Modules.Simulation.Models.Entities.SessionAgent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Simulation.Data.Configurations;

public sealed class SessionAgentConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("SessionAgents", "simulation", table =>
        {
            table.HasCheckConstraint("CK_SessionAgents_TrustLevel", "[TrustLevel] IS NULL OR [TrustLevel] <= 100");
            table.HasCheckConstraint("CK_SessionAgents_PatienceLevel", "[PatienceLevel] IS NULL OR [PatienceLevel] <= 100");
            table.HasCheckConstraint("CK_SessionAgents_CooperationLevel", "[CooperationLevel] IS NULL OR [CooperationLevel] <= 100");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.MemorySummary);
        builder.Property(x => x.CurrentMood).HasMaxLength(200);
        builder.Property(x => x.LastInteractionAt).HasPrecision(3);
        builder.HasIndex(x => new { x.SessionId, x.PersonaId }).IsUnique();
        builder.HasOne(x => x.Session)
            .WithMany(x => x.SessionAgents)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.PersonaId);
    }
}
