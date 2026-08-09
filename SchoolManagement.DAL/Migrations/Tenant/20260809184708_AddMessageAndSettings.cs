using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddMessageAndSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "messages",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sender_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    parent_message_id = table.Column<Guid>(type: "uuid", nullable: true),
                    subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    attachment_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    attachment_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_deleted_by_sender = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_messages_messages_parent_message_id",
                        column: x => x.parent_message_id,
                        principalSchema: "tenant_template",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_messages_users_sender_id",
                        column: x => x.sender_id,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "school_settings",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    school_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    school_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Asia/Dhaka"),
                    currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "BDT"),
                    currency_symbol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "৳"),
                    date_format = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "DD/MM/YYYY"),
                    language = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "en"),
                    allow_student_login = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    allow_guardian_login = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_fees_in_student_panel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_attendance_in_student_panel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    show_result_in_student_panel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    student_panel_notice_message = table.Column<string>(type: "text", nullable: true),
                    system_logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    text_logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    printing_logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    report_card_logo_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    payment_gateways = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    active_gateways = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "[]"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_school_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "message_recipients",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_important = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_recipients", x => x.id);
                    table.ForeignKey(
                        name: "FK_message_recipients_messages_message_id",
                        column: x => x.message_id,
                        principalSchema: "tenant_template",
                        principalTable: "messages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_message_recipients_users_recipient_id",
                        column: x => x.recipient_id,
                        principalSchema: "tenant_template",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "idx_message_recipients_message",
                schema: "tenant_template",
                table: "message_recipients",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "idx_message_recipients_recipient",
                schema: "tenant_template",
                table: "message_recipients",
                column: "recipient_id");

            migrationBuilder.CreateIndex(
                name: "idx_messages_sender",
                schema: "tenant_template",
                table: "messages",
                column: "sender_id");

            migrationBuilder.CreateIndex(
                name: "IX_messages_parent_message_id",
                schema: "tenant_template",
                table: "messages",
                column: "parent_message_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "message_recipients",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "school_settings",
                schema: "tenant_template");

            migrationBuilder.DropTable(
                name: "messages",
                schema: "tenant_template");
        }
    }
}
