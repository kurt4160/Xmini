using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Xmini.Migrations
{
    /// <inheritdoc />
    public partial class RenameLikesTweets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Like_AspNetUsers_ApplicationUserId",
                table: "Like");

            migrationBuilder.DropForeignKey(
                name: "FK_Like_Tweet_TweetId",
                table: "Like");

            migrationBuilder.DropForeignKey(
                name: "FK_Tweet_AspNetUsers_ApplicationUserId",
                table: "Tweet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tweet",
                table: "Tweet");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Like",
                table: "Like");

            migrationBuilder.RenameTable(
                name: "Tweet",
                newName: "Tweets");

            migrationBuilder.RenameTable(
                name: "Like",
                newName: "Likes");

            migrationBuilder.RenameIndex(
                name: "IX_Tweet_ApplicationUserId",
                table: "Tweets",
                newName: "IX_Tweets_ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Like_TweetId",
                table: "Likes",
                newName: "IX_Likes_TweetId");

            migrationBuilder.RenameIndex(
                name: "IX_Like_ApplicationUserId",
                table: "Likes",
                newName: "IX_Likes_ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Tweets",
                type: "nvarchar(280)",
                maxLength: 280,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tweets",
                table: "Tweets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Likes",
                table: "Likes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_AspNetUsers_ApplicationUserId",
                table: "Likes",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Tweets_TweetId",
                table: "Likes",
                column: "TweetId",
                principalTable: "Tweets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tweets_AspNetUsers_ApplicationUserId",
                table: "Tweets",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_AspNetUsers_ApplicationUserId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Tweets_TweetId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_Tweets_AspNetUsers_ApplicationUserId",
                table: "Tweets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tweets",
                table: "Tweets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Likes",
                table: "Likes");

            migrationBuilder.RenameTable(
                name: "Tweets",
                newName: "Tweet");

            migrationBuilder.RenameTable(
                name: "Likes",
                newName: "Like");

            migrationBuilder.RenameIndex(
                name: "IX_Tweets_ApplicationUserId",
                table: "Tweet",
                newName: "IX_Tweet_ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_TweetId",
                table: "Like",
                newName: "IX_Like_TweetId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_ApplicationUserId",
                table: "Like",
                newName: "IX_Like_ApplicationUserId");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "Tweet",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(280)",
                oldMaxLength: 280);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tweet",
                table: "Tweet",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Like",
                table: "Like",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Like_AspNetUsers_ApplicationUserId",
                table: "Like",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Like_Tweet_TweetId",
                table: "Like",
                column: "TweetId",
                principalTable: "Tweet",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tweet_AspNetUsers_ApplicationUserId",
                table: "Tweet",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
