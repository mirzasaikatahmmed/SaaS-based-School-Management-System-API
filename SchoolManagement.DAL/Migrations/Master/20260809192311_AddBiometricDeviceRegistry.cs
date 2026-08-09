using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Master
{
    /// <inheritdoc />
    public partial class AddBiometricDeviceRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "biometric_device_registry",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    serial_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schema_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    device_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    att_log_stamp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "0"),
                    oper_log_stamp = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "0"),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_device_registry", x => x.id);
                    table.ForeignKey(
                        name: "FK_biometric_device_registry_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalSchema: "public",
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_biometric_device_registry_serial_number",
                schema: "public",
                table: "biometric_device_registry",
                column: "serial_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_biometric_device_registry_tenant_id",
                schema: "public",
                table: "biometric_device_registry",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "biometric_device_registry",
                schema: "public");
        }
    }
}
