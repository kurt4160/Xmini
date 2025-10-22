using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xmini.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToTweet2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Tweets",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Tweets");
        }
    }
}
