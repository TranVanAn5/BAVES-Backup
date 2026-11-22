using Entity = BAVES.Api.Modules.Simulation.Models.Entities.Message;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Simulation.Data.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("Messages", "simulation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.SenderType).HasConversion<string>().HasMaxLength(40);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.CreatedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.SessionId, x.CreatedAt });
        builder.HasOne(x => x.Session)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(x => x.SessionAgent)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.SessionAgentId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
