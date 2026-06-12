using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudFolio.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndCreationTimeWithSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "user");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "Avatar", "CreationTime", "EducationInstitution", "Email", "Firstname", "Lastname", "Middlename", "Password", "Role", "Specialty" },
                values: new object[] { 1, "favicon.svg", new DateTime(2026, 6, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, "support@studfolio.if.ua", "StudFolio", "Система", null, "AQAAAAIAAYagAAAAEN3N7JHMuMh/dXzJqbDLaMzI0fW70e+WtfaU4HkMvfFw+Ez70Pb5RavSm8e0v/Mcpg==", "owner", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
