using Entity = BAVES.Api.Modules.Simulation.Models.Entities.CoachFeedback;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BAVES.Api.Modules.Simulation.Data.Configurations;

public sealed class CoachFeedbackConfiguration : IEntityTypeConfiguration<Entity>
{
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("CoachFeedbacks", "simulation");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        // Bốn nhóm nhận xét là JSON array chuỗi, độ dài không giới hạn thực tế
        // (mỗi ý có thể là một đoạn văn) nên dùng nvarchar(max).
        builder.Property(x => x.StrengthsJson).IsRequired();
        builder.Property(x => x.GapsJson).IsRequired();
        builder.Property(x => x.UnclearPointsJson).IsRequired();
        builder.Property(x => x.ImprovementHintsJson).IsRequired();

        builder.Property(x => x.PromptVersion).IsRequired().HasMaxLength(100);
        builder.Property(x => x.CreatedAt).HasPrecision(3);

        // GET /coach-feedback luôn lấy bản mới nhất của một session
        // → index bao phủ đúng truy vấn đó.
        builder.HasIndex(x => new { x.SessionId, x.CreatedAt });

        builder.HasOne(x => x.Session)
            .WithMany(x => x.CoachFeedbacks)
            .HasForeignKey(x => x.SessionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
