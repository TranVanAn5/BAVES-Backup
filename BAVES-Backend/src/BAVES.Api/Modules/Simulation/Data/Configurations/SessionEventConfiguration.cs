using Entity = BAVES.Api.Modules.Simulation.Models.Entities.SessionEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Simulation.Data.Configurations;

public sealed class SessionEventConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("SessionEvents", "simulation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.TriggerSource).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.TriggeredAt).HasPrecision(3);
        builder.Property(x => x.EventContentSnapshot).IsRequired();
        builder.HasIndex(x => new { x.SessionId, x.SequenceNo }).IsUnique();
        builder.HasOne(x => x.Session)
            .WithMany(x => x.SessionEvents)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.ScenarioEventId);
    }
}
