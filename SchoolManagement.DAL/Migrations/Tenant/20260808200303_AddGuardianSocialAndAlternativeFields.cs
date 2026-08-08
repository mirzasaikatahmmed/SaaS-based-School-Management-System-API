using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Tenant
{
    /// <inheritdoc />
    public partial class AddGuardianSocialAndAlternativeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "alternative_parent_mobile",
                schema: "tenant_template",
                table: "guardians",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternative_parent_name",
                schema: "tenant_template",
                table: "guardians",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "alternative_parent_relation",
                schema: "tenant_template",
                table: "guardians",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "facebook_url",
                schema: "tenant_template",
                table: "guardians",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "linkedin_url",
                schema: "tenant_template",
                table: "guardians",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "twitter_url",
                schema: "tenant_template",
                table: "guardians",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "alternative_parent_mobile",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "alternative_parent_name",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "alternative_parent_relation",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "facebook_url",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "linkedin_url",
                schema: "tenant_template",
                table: "guardians");

            migrationBuilder.DropColumn(
                name: "twitter_url",
                schema: "tenant_template",
                table: "guardians");
        }
    }
}
