using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meridian.Dal.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplateCardResourceLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 1,
                column: "Url",
                value: "/resources/safety-basics");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 2,
                column: "Url",
                value: "/resources/employee-handbook");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 3,
                column: "Url",
                value: "/resources/dev-setup");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 4,
                column: "Url",
                value: "/resources/meet-your-team");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 5,
                column: "Url",
                value: "/resources/your-manager");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 6,
                column: "Url",
                value: "/resources/hr-contact");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 7,
                column: "Url",
                value: "/resources/it-helpdesk");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 1,
                column: "Url",
                value: "https://intranet.meridian.local/safety/basics");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 2,
                column: "Url",
                value: "https://intranet.meridian.local/handbook");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 3,
                column: "Url",
                value: "https://intranet.meridian.local/it/dev-setup");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 4,
                column: "Url",
                value: "https://chat.meridian.local/channels/team");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 5,
                column: "Url",
                value: "mailto:manager@meridian.local");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 6,
                column: "Url",
                value: "mailto:hr@meridian.local");

            migrationBuilder.UpdateData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 7,
                column: "Url",
                value: "mailto:it@meridian.local");
        }
    }
}
