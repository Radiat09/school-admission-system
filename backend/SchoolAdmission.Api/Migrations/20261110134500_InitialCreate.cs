#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace SchoolAdmission.Api.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AdmissionClasses",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                TotalSeats = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdmissionClasses", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "AdmissionHolds",
            columns: table => new
            {
                Id = table.Column<string>(type: "text", nullable: false),
                ClassId = table.Column<string>(type: "text", nullable: false),
                StudentId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                ConfirmedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdmissionHolds", x => x.Id);
                table.ForeignKey(
                    name: "FK_AdmissionHolds_AdmissionClasses_ClassId",
                    column: x => x.ClassId,
                    principalTable: "AdmissionClasses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AdmissionHolds_ClassId_Status",
            table: "AdmissionHolds",
            columns: new[] { "ClassId", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_AdmissionHolds_ExpiresAt_Status",
            table: "AdmissionHolds",
            columns: new[] { "ExpiresAt", "Status" });

        migrationBuilder.CreateIndex(
            name: "IX_AdmissionHolds_StudentId",
            table: "AdmissionHolds",
            column: "StudentId");

        migrationBuilder.CreateIndex(
            name: "IX_AdmissionHolds_StudentId_ClassId_Active",
            table: "AdmissionHolds",
            columns: new[] { "StudentId", "ClassId" },
            unique: true,
            filter: "\"Status\" IN ('HELD','CONFIRMED')");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "AdmissionHolds");

        migrationBuilder.DropTable(
            name: "AdmissionClasses");
    }
}
