using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xmini.Migrations
{
    /// <inheritdoc />
    public partial class AddContentTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundPictureContentType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureContentType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundPictureContentType",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureContentType",
                table: "AspNetUsers");
        }
    }
}
