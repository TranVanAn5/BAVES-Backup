using IndustryEntity = BAVES.Api.Modules.Content.Models.Entities.Industry;
using BusinessApplicationEntity = BAVES.Api.Modules.Content.Models.Entities.BusinessApplication;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Content.Data;

public sealed class ContentDbContext(DbContextOptions<ContentDbContext> options) : DbContext(options)
{
    public DbSet<IndustryEntity> Industries => Set<IndustryEntity>();
    public DbSet<BusinessApplicationEntity> Applications => Set<BusinessApplicationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("content");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContentDbContext).Assembly,
            type => type.Namespace == "BAVES.Api.Modules.Content.Data.Configurations");
    }
}
