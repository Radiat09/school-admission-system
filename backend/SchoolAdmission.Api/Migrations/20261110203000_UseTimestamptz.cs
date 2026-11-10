#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace SchoolAdmission.Api.Migrations;

/// <inheritdoc />
public partial class UseTimestamptz : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedAt",
            table: "AdmissionClasses",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "AdmissionClasses",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "ConfirmedAt",
            table: "AdmissionHolds",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedAt",
            table: "AdmissionHolds",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpiresAt",
            table: "AdmissionHolds",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "AdmissionHolds",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp without time zone");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedAt",
            table: "AdmissionClasses",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "AdmissionClasses",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "ConfirmedAt",
            table: "AdmissionHolds",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedAt",
            table: "AdmissionHolds",
            type: "timestamp without time zone",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "ExpiresAt",
            table: "AdmissionHolds",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "AdmissionHolds",
            type: "timestamp without time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");
    }
}
