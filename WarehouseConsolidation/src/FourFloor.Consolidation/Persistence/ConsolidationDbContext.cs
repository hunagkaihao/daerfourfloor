using FourFloor.Consolidation.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace FourFloor.Consolidation.Persistence;

public sealed class ConsolidationDbContext(DbContextOptions<ConsolidationDbContext> options)
    : DbContext(options)
{
    public DbSet<ConsolidationPlanEntity> Plans => Set<ConsolidationPlanEntity>();
    public DbSet<ConsolidationMoveEntity> Moves => Set<ConsolidationMoveEntity>();
    public DbSet<ConsolidationEventEntity> Events => Set<ConsolidationEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConsolidationPlanEntity>(entity =>
        {
            entity.ToTable("ConsolidationPlans");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Status).HasMaxLength(32);
            entity.Property(item => item.CurrentHole).HasMaxLength(32);
            entity.HasMany(item => item.Moves)
                .WithOne(item => item.Plan)
                .HasForeignKey(item => item.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ConsolidationMoveEntity>(entity =>
        {
            entity.ToTable("ConsolidationMoves");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.PlanId, item.Sequence }).IsUnique();
            entity.Property(item => item.Status).HasMaxLength(32);
            entity.Property(item => item.GroupBarcode).HasMaxLength(100);
            entity.Property(item => item.PalletKey).HasMaxLength(2000);
            entity.Property(item => item.FromCell).HasMaxLength(32);
            entity.Property(item => item.ToCell).HasMaxLength(32);
            entity.Property(item => item.MoveType).HasMaxLength(32);
            entity.Property(item => item.CurrentBoxCode).HasMaxLength(100);
            entity.Property(item => item.AgvReqCode).HasMaxLength(100);
            entity.Property(item => item.AgvTaskCode).HasMaxLength(100);
        });

        modelBuilder.Entity<ConsolidationEventEntity>(entity =>
        {
            entity.ToTable("ConsolidationEvents");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.PlanId, item.OccurredAtUtc });
            entity.Property(item => item.Level).HasMaxLength(32);
            entity.Property(item => item.EventType).HasMaxLength(64);
        });
    }
}
