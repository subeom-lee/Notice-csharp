using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notice.Migrations
{
    /// <inheritdoc />
    public partial class updatecategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_Category_id1",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Category_id1",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Category_id1",
                table: "Categories");

            migrationBuilder.AddColumn<string>(
                name: "CategoryValue",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryValue",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "Category_id1",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Category_id1",
                table: "Categories",
                column: "Category_id1");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_Category_id1",
                table: "Categories",
                column: "Category_id1",
                principalTable: "Categories",
                principalColumn: "Category_id");
        }
    }
}
