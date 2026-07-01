using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Meridian.Dal.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OnboardingTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OnboardingTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateCards_OnboardingTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "OnboardingTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "OnboardingTemplates",
                columns: new[] { "Id", "Name" },
                values: new object[] { 1, "Default Onboarding" });

            migrationBuilder.InsertData(
                table: "TemplateCards",
                columns: new[] { "Id", "Description", "Order", "TemplateId", "Title", "Type", "Url" },
                values: new object[,]
                {
                    { 1, "Required reading before your first day on site: evacuation routes, incident reporting and protective equipment.", 1, 1, "Workplace safety basics", 1, "https://intranet.meridian.local/safety/basics" },
                    { 2, "Company policies, benefits and day-to-day practicalities.", 2, 1, "Employee handbook", 0, "https://intranet.meridian.local/handbook" },
                    { 3, "Step-by-step guide to get your workstation and accounts ready.", 3, 1, "Development environment setup", 0, "https://intranet.meridian.local/it/dev-setup" },
                    { 4, "Your team's channel — say hello and find your onboarding buddy.", 4, 1, "Meet your team", 2, "https://chat.meridian.local/channels/team" },
                    { 5, "Direct line to your manager for questions and 1:1 scheduling.", 5, 1, "Your manager", 2, "mailto:manager@meridian.local" },
                    { 6, "Contracts, payroll and anything people-related.", 6, 1, "HR contact", 2, "mailto:hr@meridian.local" },
                    { 7, "Hardware, accounts and access issues.", 7, 1, "IT helpdesk", 2, "mailto:it@meridian.local" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCards_TemplateId_Order",
                table: "TemplateCards",
                columns: new[] { "TemplateId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateCards");

            migrationBuilder.DropTable(
                name: "OnboardingTemplates");
        }
    }
}
