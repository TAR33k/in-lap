using InLap.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InLap.Infrastructure.Persistence
{
    public class InLapDbContext : DbContext
    {
        public InLapDbContext(DbContextOptions<InLapDbContext> options) : base(options)
        {
        }

        public DbSet<UploadRecord> Uploads => Set<UploadRecord>();
        public DbSet<ReportRecord> Reports => Set<ReportRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UploadRecord>(entity =>
            {
                entity.ToTable("Uploads");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileName)
                      .IsRequired()
                      .HasMaxLength(260);

                entity.Property(e => e.StoredPath)
                      .IsRequired()
                      .HasMaxLength(1024);

                entity.Property(e => e.CreatedAtUtc)
                      .IsRequired();

                entity.HasOne(e => e.Report)
                      .WithOne(r => r.Upload)
                      .HasForeignKey<ReportRecord>(r => r.UploadId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReportRecord>(entity =>
            {
                entity.ToTable("Reports");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.UploadId)
                      .IsRequired();

                entity.Property(e => e.SummaryJson)
                      .IsRequired();

                entity.Property(e => e.LlmRaw)
                      .IsRequired();

                entity.Property(e => e.Article)
                      .IsRequired();

                entity.Property(e => e.CreatedAtUtc)
                      .IsRequired();
            });
        }
    }
}
