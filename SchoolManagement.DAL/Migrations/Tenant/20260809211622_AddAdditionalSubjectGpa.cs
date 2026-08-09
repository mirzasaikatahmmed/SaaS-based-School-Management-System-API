using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddAdditionalSubjectGpa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "can_be_additional",
                schema: "tenant_template",
                table: "subjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_continuous_assessment",
                schema: "tenant_template",
                table: "subjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "additional_subject_id",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_subject_enrollments_additional_subject_id",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                column: "additional_subject_id");

            migrationBuilder.AddForeignKey(
                name: "FK_student_subject_enrollments_subjects_additional_subject_id",
                schema: "tenant_template",
                table: "student_subject_enrollments",
                column: "additional_subject_id",
                principalSchema: "tenant_template",
                principalTable: "subjects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_student_subject_enrollments_subjects_additional_subject_id",
                schema: "tenant_template",
                table: "student_subject_enrollments");

            migrationBuilder.DropIndex(
                name: "IX_student_subject_enrollments_additional_subject_id",
                schema: "tenant_template",
                table: "student_subject_enrollments");

            migrationBuilder.DropColumn(
                name: "can_be_additional",
                schema: "tenant_template",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "is_continuous_assessment",
                schema: "tenant_template",
                table: "subjects");

            migrationBuilder.DropColumn(
                name: "additional_subject_id",
                schema: "tenant_template",
                table: "student_subject_enrollments");
        }
    }
}
