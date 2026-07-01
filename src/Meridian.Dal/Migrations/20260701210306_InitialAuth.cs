using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Meridian.Dal.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "DisplayName", "Email", "PasswordHash", "Role" },
                values: new object[,]
                {
                    { 1, "Nadia NewHire", "newhire@meridian.local", "$2a$11$KXtREvvcnStxCV6zgiZM6.TJpeMcEwQ1vn4jSljs24Z8MTrCHazrC", 0 },
                    { 2, "Hannah HR", "hr@meridian.local", "$2a$11$Uu62dUNUzOfsiD9yy38yfehjpeoYwRwYqEEqdZ.8dq7BsFn1TvXXi", 1 },
                    { 3, "Marcus Manager", "manager@meridian.local", "$2a$11$gIxvyg/V6Qbjt5HrOmhiYOWE/0kbmsYsgRosAa4jdWZTSszDh8lZm", 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
