using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace sxgl.Database.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class v105 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "zyb",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Dm = table.Column<string>(type: "longtext", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Xyid = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zyb", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 3,
                column: "Role",
                value: "Xy");

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 4,
                column: "Role",
                value: "Teacher");

            migrationBuilder.InsertData(
                table: "auth",
                columns: new[] { "Id", "Auth1", "Role" },
                values: new object[] { 5, "", "Student" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "zyb");

            migrationBuilder.DeleteData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 3,
                column: "Role",
                value: "Teacher");

            migrationBuilder.UpdateData(
                table: "auth",
                keyColumn: "Id",
                keyValue: 4,
                column: "Role",
                value: "Student");
        }
    }
}
