using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace homeCookAPI.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreateDateToCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedRecipes_AspNetUsers_UserId",
                table: "SavedRecipes");

            migrationBuilder.RenameColumn(
                name: "SavedAt",
                table: "SavedRecipes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Likes",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SavedRecipes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Likes",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SavedRecipes_AspNetUsers_UserId",
                table: "SavedRecipes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedRecipes_AspNetUsers_UserId",
                table: "SavedRecipes");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "SavedRecipes",
                newName: "SavedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Likes",
                newName: "CreatedDate");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "SavedRecipes",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Likes",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_AspNetUsers_UserId",
                table: "Likes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedRecipes_AspNetUsers_UserId",
                table: "SavedRecipes",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
