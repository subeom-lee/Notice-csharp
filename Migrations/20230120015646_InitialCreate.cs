using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notice.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Categoryid = table.Column<int>(name: "Category_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Categoryid1 = table.Column<int>(name: "Category_id1", type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Categoryid);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_Category_id1",
                        column: x => x.Categoryid1,
                        principalTable: "Categories",
                        principalColumn: "Category_id");
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    postid = table.Column<int>(name: "post_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    contents = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDatetime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    Categoryid = table.Column<int>(name: "Category_id", type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.postid);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Category_id1",
                table: "Categories",
                column: "Category_id1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}
