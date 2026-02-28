using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sxgl.Database.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class v112 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Xf",
                table: "Kcb",
                type: "longtext",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 1,
                column: "Auth1",
                value: "system,user,xy,tea,stu,zy,xq,kc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Xf",
                table: "Kcb");

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 1,
                column: "Auth1",
                value: "system,user,xy,tea,stu");
        }
    }
}
