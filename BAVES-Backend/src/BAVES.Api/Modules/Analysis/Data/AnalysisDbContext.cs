using AnalysisItemEntity = BAVES.Api.Modules.Analysis.Models.Entities.AnalysisItem;
using MessageEvidenceLinkEntity = BAVES.Api.Modules.Analysis.Models.Entities.MessageEvidenceLink;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Analysis.Data;

public sealed class AnalysisDbContext(DbContextOptions<AnalysisDbContext> options) : DbContext(options)
{
    public DbSet<AnalysisItemEntity> AnalysisItems => Set<AnalysisItemEntity>();
    public DbSet<MessageEvidenceLinkEntity> MessageEvidenceLinks => Set<MessageEvidenceLinkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("analysis");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalysisDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Analysis.Data.Configurations");
    }
}
