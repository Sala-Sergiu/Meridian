using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Meridian.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkCalendar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OfficeDaysMask = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSchedules_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicHolidays", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "EmployeeSchedules",
                columns: new[] { "Id", "OfficeDaysMask", "UserId" },
                values: new object[,]
                {
                    { 1, 7, 1 },
                    { 2, 7, 2 },
                    { 3, 7, 3 }
                });

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { 2, new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day after New Year" },
                    { 3, new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Epiphany" },
                    { 4, new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), "St John the Baptist" },
                    { 5, new DateTime(2026, 1, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Union Day" },
                    { 6, new DateTime(2026, 4, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Orthodox Good Friday" },
                    { 7, new DateTime(2026, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Orthodox Easter" },
                    { 8, new DateTime(2026, 4, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), "Orthodox Easter Monday" },
                    { 9, new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Labour Day" },
                    { 10, new DateTime(2026, 5, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Orthodox Pentecost" },
                    { 11, new DateTime(2026, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Pentecost Monday & Children's Day" },
                    { 12, new DateTime(2026, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Assumption of Mary" },
                    { 13, new DateTime(2026, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "St Andrew's Day" },
                    { 14, new DateTime(2026, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Day" },
                    { 15, new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { 16, new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Second Day of Christmas" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSchedules_UserId",
                table: "EmployeeSchedules",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PublicHolidays_Date",
                table: "PublicHolidays",
                column: "Date",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeSchedules");

            migrationBuilder.DropTable(
                name: "PublicHolidays");
        }
    }
}
