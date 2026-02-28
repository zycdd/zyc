using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace sxgl.Database.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class v102 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "xyb",
                columns: table => new
                {
                    XyDm = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_xyb", x => x.XyDm);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Auth1", "Role" },
                values: new object[] { "system,user", "Jwc" });

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 3,
                column: "Role",
                value: "Teacher");

            migrationBuilder.InsertData(
                table: "auth",
                columns: new[] { "Id", "Auth1", "Role" },
                values: new object[] { 4, "user", "Student" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "xyb");

            migrationBuilder.DeleteData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Auth1", "Role" },
                values: new object[] { "user", "Teacher" });

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 3,
                column: "Role",
                value: "Student");
        }
    }
}
