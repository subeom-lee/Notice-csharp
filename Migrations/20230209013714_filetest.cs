using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notice.Migrations
{
    /// <inheritdoc />
    public partial class filetest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachfiles",
                columns: table => new
                {
                    Fileid = table.Column<int>(name: "File_id", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Filedata = table.Column<string>(name: "File_data", type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachfiles", x => x.Fileid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachfiles");
        }
    }
}
