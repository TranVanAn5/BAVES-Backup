using Entity = BAVES.Api.Modules.Simulation.Models.Entities.SessionAgentKnowledgeState;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Simulation.Data.Configurations;

public sealed class SessionAgentKnowledgeStateConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("SessionAgentKnowledgeStates", "simulation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RevealedAt).HasPrecision(3);
        builder.HasIndex(x => new { x.SessionAgentId, x.KnowledgeItemId }).IsUnique();
        builder.HasOne(x => x.SessionAgent)
            .WithMany(x => x.SessionAgentKnowledgeStates)
            .HasForeignKey(x => x.SessionAgentId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasIndex(x => x.KnowledgeItemId);
        builder.HasOne(x => x.RevealMessage)
            .WithMany(x => x.SessionAgentKnowledgeStates)
            .HasForeignKey(x => x.RevealMessageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
