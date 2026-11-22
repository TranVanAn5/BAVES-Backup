using BAVES.Api.Modules.Commerce.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BAVES.Api.Modules.Commerce.Data;

public class CommerceDbContext : DbContext
{
    public CommerceDbContext(DbContextOptions<CommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanScope> PlanScopes => Set<PlanScope>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();
    public DbSet<Entitlement> Entitlements => Set<Entitlement>();
    public DbSet<CreditLedger> CreditLedgers => Set<CreditLedger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("commerce");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommerceDbContext).Assembly,
            t => t.Namespace?.Contains("Commerce") == true);
        base.OnModelCreating(modelBuilder);
    }
}
