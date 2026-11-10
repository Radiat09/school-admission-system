using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Api.Entities;

namespace SchoolAdmission.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AdmissionClass> AdmissionClasses { get; set; }
    public DbSet<AdmissionHold> AdmissionHolds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AdmissionClass>(entity =>
        {
            entity.ToTable("AdmissionClasses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TotalSeats).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).IsRequired().HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<AdmissionHold>(entity =>
        {
            entity.ToTable("AdmissionHolds");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.ClassId).IsRequired();
            entity.Property(e => e.StudentId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ExpiresAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.Property(e => e.CreatedAt).IsRequired().HasColumnType("timestamp with time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(e => e.ConfirmedAt).HasColumnType("timestamp with time zone");

            entity.HasOne(e => e.Class)
                .WithMany()
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ClassId, e.Status })
                .HasDatabaseName("IX_AdmissionHolds_ClassId_Status");
            entity.HasIndex(e => new { e.ExpiresAt, e.Status })
                .HasDatabaseName("IX_AdmissionHolds_ExpiresAt_Status");
            entity.HasIndex(e => e.StudentId)
                .HasDatabaseName("IX_AdmissionHolds_StudentId");

            entity.HasIndex(e => new { e.StudentId, e.ClassId })
                .HasDatabaseName("IX_AdmissionHolds_StudentId_ClassId_Active")
                .IsUnique()
                .HasFilter("\"Status\" IN ('HELD','CONFIRMED')");
        });
    }
}
