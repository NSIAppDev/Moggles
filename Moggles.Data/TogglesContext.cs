using Microsoft.EntityFrameworkCore;
using Moggles.Domain;

namespace Moggles.Data
{
    public class TogglesContext : DbContext
    {
        public TogglesContext(DbContextOptions<TogglesContext> options) : base(options)
        {
        }

        public DbSet<SQLApplication> Applications { get; set; }
        public DbSet<SQLDeployEnvironment> DeployEnvironments { get; set; }
        public virtual DbSet<SQLFeatureToggle> FeatureToggles { get; set; }
        public DbSet<SQLFeatureToggleStatus> FeatureToggleStatuses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //relationships
            modelBuilder.Entity<SQLFeatureToggleStatus>().HasOne(fts => fts.Environment).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SQLFeatureToggleStatus>().HasOne(fts => fts.FeatureToggle).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SQLDeployEnvironment>().HasOne(de => de.Application).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SQLFeatureToggle>().HasOne(ft => ft.Application).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<SQLFeatureToggle>().HasMany(ft => ft.FeatureToggleStatuses).WithOne(fts => fts.FeatureToggle).OnDelete(DeleteBehavior.Cascade);

            //default values
            modelBuilder.Entity<SQLDeployEnvironment>().Property(de => de.SortOrder).HasDefaultValue(500);
            modelBuilder.Entity<SQLFeatureToggleStatus>().Property(fts => fts.LastUpdated).HasDefaultValueSql("GETUTCDATE()");
            modelBuilder.Entity<SQLFeatureToggle>().Property(ft => ft.CreatedDate).HasDefaultValueSql("GETUTCDATE()");

            //fixed lengths
            modelBuilder.Entity<SQLApplication>().Property(a => a.AppName).HasMaxLength(100);
            modelBuilder.Entity<SQLDeployEnvironment>().Property(de => de.EnvName).HasMaxLength(50);
            modelBuilder.Entity<SQLFeatureToggle>().Property(ft => ft.ToggleName).HasMaxLength(80);
            modelBuilder.Entity<SQLFeatureToggle>().Property(ft => ft.Notes).HasMaxLength(500);

            //indexes
            modelBuilder.Entity<SQLApplication>().HasIndex(a => a.AppName);
            modelBuilder.Entity<SQLDeployEnvironment>().HasIndex(a => a.EnvName);
            modelBuilder.Entity<SQLFeatureToggle>().HasIndex(f => f.ToggleName);

            modelBuilder.Entity<SQLApplication>().Ignore(a => a.NewId);
            modelBuilder.Entity<SQLDeployEnvironment>().Ignore(a => a.NewId);
            modelBuilder.Entity<SQLFeatureToggle>().Ignore(a => a.NewId);
            modelBuilder.Entity<SQLFeatureToggleStatus>().Ignore(a => a.NewId);
        }
    }
}