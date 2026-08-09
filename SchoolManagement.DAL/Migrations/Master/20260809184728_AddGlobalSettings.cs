using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddGlobalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "global_settings",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    site_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    site_title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    site_logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    site_favicon_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    admin_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    support_phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    default_timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Asia/Dhaka"),
                    default_currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "BDT"),
                    default_currency_symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "৳"),
                    default_locale = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "en-US"),
                    default_date_format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DD/MM/YYYY"),
                    maintenance_mode = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    maintenance_message = table.Column<string>(type: "text", nullable: true),
                    max_upload_size_mb = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    allowed_file_types = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false, defaultValue: "jpg,jpeg,png,gif,pdf,doc,docx,xls,xlsx,csv"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_global_settings", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "global_settings",
                schema: "public");
        }
    }
}
