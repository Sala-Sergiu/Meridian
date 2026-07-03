using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meridian.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddConfidentialityTemplateCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TemplateCards",
                columns: new[] { "Id", "Description", "Order", "TemplateId", "Title", "Type", "Url" },
                values: new object[] { 8, "Company and client information stays inside the company — required reading.", 8, 1, "Data & client confidentiality", 1, "/resources/data-confidentiality" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TemplateCards",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
