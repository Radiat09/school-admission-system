#nullable disable

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolAdmission.Api.Data;
using SchoolAdmission.Api.Entities;

#nullable restore

namespace SchoolAdmission.Api.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AdmissionClassModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 128);

        var statusConverter = new ValueConverter<HoldStatus, string>(
            v => v.ToString(),
            v => (HoldStatus)Enum.Parse(typeof(HoldStatus), v));

        modelBuilder.Entity("SchoolAdmission.Api.Entities.AdmissionClass", b =>
        {
            b.Property<string>("Id")
                .HasColumnType("text");
            b.Property<string>("Name")
                .HasColumnType("character varying(200)")
                .HasMaxLength(200);
            b.Property<int>("TotalSeats")
                .HasColumnType("integer");
            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");
            b.Property<DateTime>("UpdatedAt")
                .HasColumnType("timestamp with time zone");
            b.HasKey("Id");
            b.ToTable("AdmissionClasses");
        });

        modelBuilder.Entity("SchoolAdmission.Api.Entities.AdmissionHold", b =>
        {
            b.Property<string>("Id")
                .HasColumnType("text");
            b.Property<string>("ClassId")
                .HasColumnType("text");
            b.Property<string>("StudentId")
                .HasColumnType("character varying(50)")
                .HasMaxLength(50);
            b.Property<HoldStatus>("Status")
                .HasConversion(statusConverter)
                .HasColumnType("character varying(20)")
                .HasMaxLength(20);
            b.Property<DateTime>("ExpiresAt")
                .HasColumnType("timestamp with time zone");
            b.Property<DateTime>("CreatedAt")
                .HasColumnType("timestamp with time zone");
            b.Property<DateTime?>("UpdatedAt")
                .HasColumnType("timestamp with time zone");
            b.Property<DateTime?>("ConfirmedAt")
                .HasColumnType("timestamp with time zone");
            b.HasKey("Id");
            b.HasIndex(new[] { "ClassId", "Status" })
                .HasDatabaseName("IX_AdmissionHolds_ClassId_Status");
            b.HasIndex(new[] { "ExpiresAt", "Status" })
                .HasDatabaseName("IX_AdmissionHolds_ExpiresAt_Status");
            b.HasIndex(new[] { "StudentId" })
                .HasDatabaseName("IX_AdmissionHolds_StudentId");
            b.HasIndex(new[] { "StudentId", "ClassId" })
                .HasDatabaseName("IX_AdmissionHolds_StudentId_ClassId_Active")
                .IsUnique()
                .HasFilter("\"Status\" IN ('HELD','CONFIRMED')");
            b.HasOne("SchoolAdmission.Api.Entities.AdmissionClass", "Class")
                .WithMany()
                .HasForeignKey("ClassId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
            b.ToTable("AdmissionHolds");
        });
#pragma warning restore 612, 618
    }
}
