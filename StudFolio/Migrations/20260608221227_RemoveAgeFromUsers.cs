using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudFolio.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAgeFromUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Preview",
                table: "Portfolios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Preview",
                table: "Portfolios",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
