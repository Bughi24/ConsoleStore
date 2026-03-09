using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsoleStore.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTabless : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Pric",
                table: "OrderItems",
                newName: "Price");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "OrderItems",
                newName: "Pric");
        }
    }
}
