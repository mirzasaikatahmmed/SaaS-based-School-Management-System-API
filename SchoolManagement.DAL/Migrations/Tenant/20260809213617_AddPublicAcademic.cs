using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddPublicAcademic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "website_content_pages",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    title_bn = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    body_html = table.Column<string>(type: "text", nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_content_pages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_handnotes",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    published_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    class_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    teacher_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_handnotes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "website_online_class_videos",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    class_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    title = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    teacher_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    youtube_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    youtube_video_id = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    class_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_website_online_class_videos", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_website_content_pages_slug",
                schema: "tenant_template",
                table: "website_content_pages",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_website_online_class_videos_class_name",
                schema: "tenant_template",
                table: "website_online_class_videos",
                column: "class_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "website_content_pages",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_handnotes",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "website_online_class_videos",
                schema: "tenant_template");
        }
    }
}
