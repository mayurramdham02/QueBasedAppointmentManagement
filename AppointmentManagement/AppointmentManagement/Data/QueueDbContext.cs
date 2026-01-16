using AppointmentManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagement.Data
{
    /// <summary>
    /// Database context for the Urgent Care Queue System.
    /// Manages QueueItems and Providers with proper relationships, indexes, and seed data.
    /// </summary>
    public class QueueDbContext : DbContext
    {
        public QueueDbContext(DbContextOptions<QueueDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for queue items (patients in the queue)
        /// </summary>
        public DbSet<QueueItem> QueueItems { get; set; }

        /// <summary>
        /// DbSet for healthcare providers
        /// </summary>
        public DbSet<Provider> Providers { get; set; }

        /// <summary>
        /// Configure entity relationships, indexes, constraints, and seed data
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== QueueItem Configuration =====
            modelBuilder.Entity<QueueItem>(entity =>
            {
                // Table name
                entity.ToTable("QueueItems");

                // Primary Key
                entity.HasKey(q => q.Id);

                // Configure enum to store as string in database
                entity.Property(q => q.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                // Column configurations with proper naming
                entity.Property(q => q.PatientName)
                    .HasColumnName("PatientName")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(q => q.EstimatedWaitTimeMinutes)
                    .HasColumnName("EstimatedWaitTimeMinutes");

                // Indexes for performance optimization
                entity.HasIndex(q => q.JoinTime)
                    .HasDatabaseName("IX_QueueItems_JoinTime");

                entity.HasIndex(q => q.Status)
                    .HasDatabaseName("IX_QueueItems_Status");

                // Check constraint for PainLevel (1-10) - SQL Server syntax
                entity.ToTable(t => t.HasCheckConstraint("CK_QueueItems_PainLevel", "[PainLevel] >= 1 AND [PainLevel] <= 10"));

                // Configure relationship with Provider (One-to-Many)
                entity.HasOne(q => q.AcceptedByProvider)
                    .WithMany(p => p.AcceptedPatients)
                    .HasForeignKey(q => q.AcceptedByProviderId)
                    .OnDelete(DeleteBehavior.SetNull); // If provider is deleted, set FK to null

                // Default values
                entity.Property(q => q.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(q => q.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ===== Provider Configuration =====
            modelBuilder.Entity<Provider>(entity =>
            {
                // Table name
                entity.ToTable("Providers");

                // Primary Key
                entity.HasKey(p => p.Id);

                // Column configurations
                entity.Property(p => p.ProviderName)
                    .HasColumnName("ProviderName")
                    .HasMaxLength(100)
                    .IsRequired();

                // Unique index on ProviderName (provider names must be unique)
                entity.HasIndex(p => p.ProviderName)
                    .IsUnique()
                    .HasDatabaseName("IX_Providers_ProviderName_Unique");

                // Default values
                entity.Property(p => p.IsOnline)
                    .HasDefaultValue(false);

                entity.Property(p => p.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(p => p.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ===== Seed Data: 2 Providers (both offline initially) =====
            modelBuilder.Entity<Provider>().HasData(
                new Provider
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    ProviderName = "Dr. Smith",
                    IsOnline = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Provider
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ProviderName = "Dr. Johnson",
                    IsOnline = false,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        /// <summary>
        /// Override SaveChanges to automatically update UpdatedAt timestamp
        /// </summary>
        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync to automatically update UpdatedAt timestamp
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Update UpdatedAt timestamp for all modified entities
        /// </summary>
        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity is QueueItem queueItem)
                {
                    queueItem.UpdatedAt = DateTime.UtcNow;
                }
                else if (entry.Entity is Provider provider)
                {
                    provider.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}
