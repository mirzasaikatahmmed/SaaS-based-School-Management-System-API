using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddDeactivateReasonMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "deactivate_reason_id",
                schema: "tenant_template",
                table: "students",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "deactivate_reasons",
                schema: "tenant_template",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deactivate_reasons", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_students_deactivate_reason_id",
                schema: "tenant_template",
                table: "students",
                column: "deactivate_reason_id");

            migrationBuilder.CreateIndex(
                name: "IX_deactivate_reasons_reason",
                schema: "tenant_template",
                table: "deactivate_reasons",
                column: "reason",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_students_deactivate_reasons_deactivate_reason_id",
                schema: "tenant_template",
                table: "students",
                column: "deactivate_reason_id",
                principalSchema: "tenant_template",
                principalTable: "deactivate_reasons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_students_deactivate_reasons_deactivate_reason_id",
                schema: "tenant_template",
                table: "students");

            migrationBuilder.DropTable(
                name: "deactivate_reasons",
                schema: "tenant_template");

            migrationBuilder.DropIndex(
                name: "IX_students_deactivate_reason_id",
                schema: "tenant_template",
                table: "students");

            migrationBuilder.DropColumn(
                name: "deactivate_reason_id",
                schema: "tenant_template",
                table: "students");
        }
    }
}
