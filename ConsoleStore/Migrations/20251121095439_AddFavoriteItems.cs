using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleStore.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_favoriteItems_Products_ProductId",
                table: "favoriteItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_favoriteItems",
                table: "favoriteItems");

            migrationBuilder.RenameTable(
                name: "favoriteItems",
                newName: "FavoriteItems");

            migrationBuilder.RenameIndex(
                name: "IX_favoriteItems_ProductId",
                table: "FavoriteItems",
                newName: "IX_FavoriteItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FavoriteItems",
                table: "FavoriteItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteItems_Products_ProductId",
                table: "FavoriteItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteItems_Products_ProductId",
                table: "FavoriteItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FavoriteItems",
                table: "FavoriteItems");

            migrationBuilder.RenameTable(
                name: "FavoriteItems",
                newName: "favoriteItems");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteItems_ProductId",
                table: "favoriteItems",
                newName: "IX_favoriteItems_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_favoriteItems",
                table: "favoriteItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_favoriteItems_Products_ProductId",
                table: "favoriteItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
