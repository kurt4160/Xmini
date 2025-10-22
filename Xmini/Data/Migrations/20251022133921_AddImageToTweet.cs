using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xmini.Migrations
{
    /// <inheritdoc />
    public partial class AddImageToTweet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Image",
                table: "Tweets",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Tweets");
        }
    }
}
