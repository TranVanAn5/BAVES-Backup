using ArtifactTemplateEntity = BAVES.Api.Modules.Artifact.Models.Entities.ArtifactTemplate;
using ArtifactTemplateSectionEntity = BAVES.Api.Modules.Artifact.Models.Entities.ArtifactTemplateSection;
using ArtifactEntity = BAVES.Api.Modules.Artifact.Models.Entities.Artifact;
using ArtifactVersionEntity = BAVES.Api.Modules.Artifact.Models.Entities.ArtifactVersion;
using SubmissionEntity = BAVES.Api.Modules.Artifact.Models.Entities.Submission;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Artifact.Data;

public sealed class ArtifactDbContext(DbContextOptions<ArtifactDbContext> options) : DbContext(options)
{
    public DbSet<ArtifactTemplateEntity> ArtifactTemplates => Set<ArtifactTemplateEntity>();
    public DbSet<ArtifactTemplateSectionEntity> ArtifactTemplateSections => Set<ArtifactTemplateSectionEntity>();
    public DbSet<ArtifactEntity> Artifacts => Set<ArtifactEntity>();
    public DbSet<ArtifactVersionEntity> ArtifactVersions => Set<ArtifactVersionEntity>();
    public DbSet<SubmissionEntity> Submissions => Set<SubmissionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("artifact");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArtifactDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Artifact.Data.Configurations");
    }
}
