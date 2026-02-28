using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sxgl.Database.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class v103 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "XyDm",
                table: "xyb",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Dm",
                table: "xyb",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dm",
                table: "xyb");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "xyb",
                newName: "XyDm");
        }
    }
}
