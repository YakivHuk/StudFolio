using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudFolio.Migrations
{
    /// <inheritdoc />
    public partial class RenamePreviewUrlToPreview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreviewUrl",
                table: "Posts",
                newName: "Preview");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Preview",
                table: "Posts",
                newName: "PreviewUrl");
        }
    }
}
