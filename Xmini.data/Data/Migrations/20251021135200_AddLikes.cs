using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xmini.Migrations
{
    /// <inheritdoc />
    public partial class AddLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Like",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TweetId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Like", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Like_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Like_Tweet_TweetId",
                        column: x => x.TweetId,
                        principalTable: "Tweet",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Like_ApplicationUserId",
                table: "Like",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Like_TweetId",
                table: "Like",
                column: "TweetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Like");
        }
    }
}
