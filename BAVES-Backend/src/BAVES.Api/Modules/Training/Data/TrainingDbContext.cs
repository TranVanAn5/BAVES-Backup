using CohortEntity = BAVES.Api.Modules.Training.Models.Entities.Cohort;
using CohortMemberEntity = BAVES.Api.Modules.Training.Models.Entities.CohortMember;
using AssignmentEntity = BAVES.Api.Modules.Training.Models.Entities.Assignment;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Training.Data;

public sealed class TrainingDbContext(DbContextOptions<TrainingDbContext> options) : DbContext(options)
{
    public DbSet<CohortEntity> Cohorts => Set<CohortEntity>();
    public DbSet<CohortMemberEntity> CohortMembers => Set<CohortMemberEntity>();
    public DbSet<AssignmentEntity> Assignments => Set<AssignmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("training");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TrainingDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Training.Data.Configurations");
    }
}
