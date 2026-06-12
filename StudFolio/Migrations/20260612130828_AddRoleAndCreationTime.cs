using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudFolio.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleAndCreationTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "Avatar", "CreationTime", "EducationInstitution", "Email", "Firstname", "Lastname", "Middlename", "Password", "Role", "Specialty" },
                values: new object[] { 1, "favicon.svg", new DateTime(2026, 6, 12, 0, 0, 0, 0, DateTimeKind.Utc), null, "support@studfolio.if.ua", "StudFolio", "Система", null, "AQAAAAIAAYagAAAAEN3N7JHMuMh/dXzJqbDLaMzI0fW70e+WtfaU4HkMvfFw+Ez70Pb5RavSm8e0v/Mcpg==", "owner", null });
        }
    }
}
