using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sxgl.Database.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class v109 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 1,
                column: "Auth1",
                value: "system,user,xy,tea,stu");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 1,
                column: "Auth1",
                value: "system,user,xy");
        }
    }
}
