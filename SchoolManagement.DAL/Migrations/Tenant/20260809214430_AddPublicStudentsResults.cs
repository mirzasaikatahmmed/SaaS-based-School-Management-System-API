using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPublicStudentsResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "website_published_results",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    title_bn = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    exam_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    detail_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_published_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_result_analytics",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    exam_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    appeared = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    not_passed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    pass_percent = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    gpa5 = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    gpa5_percent = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: false),
                    gpa4x = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    gpa3x = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    gpa2x = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    gpa1x = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_result_analytics", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_website_result_analytics_exam_type_year",
                schema: "tenant_template",
                table: "website_result_analytics",
                columns: new[] { "exam_type", "year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "website_published_results",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_result_analytics",
                schema: "tenant_template");
        }
    }
}
