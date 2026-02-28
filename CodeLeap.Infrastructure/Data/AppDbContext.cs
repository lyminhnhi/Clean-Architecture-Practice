using CodeLeap.Domain.Common;
using CodeLeap.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeLeap.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === APPLY AUDIT FIELDS FOR ALL BaseEntity ===
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime>(nameof(BaseEntity.CreatedAt))
                        .IsRequired();

                    modelBuilder.Entity(entityType.ClrType)
                        .Property<DateTime?>(nameof(BaseEntity.UpdatedAt))
                        .IsRequired();
                }
            }

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();
        }

        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var utcNow = DateTime.UtcNow;

            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e =>
                    e.State == EntityState.Added ||
                    e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                }
                else
                {
                    entry.Entity.UpdatedAt = utcNow;
                }
            }
        }
    }
}