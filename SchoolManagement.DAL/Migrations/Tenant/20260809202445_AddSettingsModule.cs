using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddSettingsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "accounting_links_enabled",
                schema: "tenant_template",
                table: "school_settings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "attendance_type",
                schema: "tenant_template",
                table: "school_settings",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DayWise");

            migrationBuilder.AddColumn<string>(
                name: "cron_secret_key",
                schema: "tenant_template",
                table: "school_settings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "default_deposit_account_id",
                schema: "tenant_template",
                table: "school_settings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "default_expense_account_id",
                schema: "tenant_template",
                table: "school_settings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "tenant_template",
                table: "roles",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "academic_sessions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_selected = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_sessions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "database_backups",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    object_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_database_backups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_settings",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    system_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    protocol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "SMTP"),
                    smtp_host = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    smtp_port = table.Column<int>(type: "integer", nullable: false, defaultValue: 587),
                    smtp_username = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    smtp_password = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    smtp_secure = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "TLS"),
                    smtp_auth = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    from_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "email_templates",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body_html = table.Column<string>(type: "text", nullable: false),
                    notify_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "notification_dispatch_log",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    job_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    run_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_dispatch_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature_key = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    can_view = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_add = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_edit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    can_delete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalSchema: "tenant_template",
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sms_settings",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    activated_gateway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "textlocal"),
                    credentials_json = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sms_templates",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    event_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    notify_student = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notify_parent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    dlt_template_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notify_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sms_templates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_academic_sessions_name",
                schema: "tenant_template",
                table: "academic_sessions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_event_key",
                schema: "tenant_template",
                table: "email_templates",
                column: "event_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_dispatch_log_job_name_entity_key_run_date",
                schema: "tenant_template",
                table: "notification_dispatch_log",
                columns: new[] { "job_name", "entity_key", "run_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_role_id_feature_key",
                schema: "tenant_template",
                table: "role_permissions",
                columns: new[] { "role_id", "feature_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sms_templates_event_key",
                schema: "tenant_template",
                table: "sms_templates",
                column: "event_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "academic_sessions",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "database_backups",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "email_settings",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "email_templates",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "notification_dispatch_log",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "sms_settings",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "sms_templates",
                schema: "tenant_template");

            migrationBuilder.DropColumn(
                name: "accounting_links_enabled",
                schema: "tenant_template",
                table: "school_settings");

            migrationBuilder.DropColumn(
                name: "attendance_type",
                schema: "tenant_template",
                table: "school_settings");

            migrationBuilder.DropColumn(
                name: "cron_secret_key",
                schema: "tenant_template",
                table: "school_settings");

            migrationBuilder.DropColumn(
                name: "default_deposit_account_id",
                schema: "tenant_template",
                table: "school_settings");

            migrationBuilder.DropColumn(
                name: "default_expense_account_id",
                schema: "tenant_template",
                table: "school_settings");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "tenant_template",
                table: "roles");
        }
    }
}
