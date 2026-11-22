using Entity = BAVES.Api.Modules.Simulation.Models.Entities.SimulationSession;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Simulation.Data.Configurations;

public sealed class SimulationSessionConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("SimulationSessions", "simulation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.StartedAt).HasPrecision(3);
        builder.Property(x => x.SubmittedAt).HasPrecision(3);
        builder.Property(x => x.CompletedAt).HasPrecision(3);
        builder.HasIndex(x => x.ScenarioVersionId);
        builder.HasIndex(x => x.AssignmentId);
        builder.HasIndex(x => x.LearnerId);
    }
}
