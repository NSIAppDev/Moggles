using Microsoft.EntityFrameworkCore;
using Moggles.Domain;

namespace Moggles.Data
{
    public class TogglesContext : DbContext
    {
        public TogglesContext(DbContextOptions<TogglesContext> options) : base(options)
        {
        }

        public DbSet<Application> Applications { get; set; }
        public DbSet<DeployEnvironment> DeployEnvironments { get; set; }
        public virtual DbSet<FeatureToggle> FeatureToggles { get; set; }
        public DbSet<FeatureToggleStatus> FeatureToggleStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //relationships
            modelBuilder.Entity<FeatureToggleStatus>().HasOne(fts => fts.Environment).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FeatureToggleStatus>().HasOne(fts => fts.FeatureToggle).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<DeployEnvironment>().HasOne(de => de.Application).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FeatureToggle>().HasOne(ft => ft.Application).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<FeatureToggle>().HasMany(ft => ft.FeatureToggleStatuses).WithOne(fts => fts.FeatureToggle).OnDelete(DeleteBehavior.Cascade);

            //default values
            modelBuilder.Entity<DeployEnvironment>().Property(de => de.SortOrder).HasDefaultValue(500);
            modelBuilder.Entity<FeatureToggleStatus>().Property(fts => fts.LastUpdated).HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<FeatureToggle>().Property(ft => ft.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            //fixed lengths
            modelBuilder.Entity<Application>().Property(a => a.AppName).HasMaxLength(100);
            modelBuilder.Entity<DeployEnvironment>().Property(de => de.EnvName).HasMaxLength(50);
            modelBuilder.Entity<FeatureToggle>().Property(ft => ft.ToggleName).HasMaxLength(80);
            modelBuilder.Entity<FeatureToggle>().Property(ft => ft.Notes).HasMaxLength(500);

            //indexes
            modelBuilder.Entity<Application>().HasIndex(a => a.AppName);
            modelBuilder.Entity<DeployEnvironment>().HasIndex(a => a.EnvName);
            modelBuilder.Entity<FeatureToggle>().HasIndex(f => f.ToggleName);
        }
    }
}