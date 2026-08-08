using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.DAL.Migrations.Master
{
    /// <inheritdoc />
    public partial class ExtendTenantsWithSchoolFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                schema: "public",
                table: "tenants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "city",
                schema: "public",
                table: "tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "country",
                schema: "public",
                table: "tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Bangladesh");

            migrationBuilder.AddColumn<string>(
                name: "currency",
                schema: "public",
                table: "tenants",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "BDT");

            migrationBuilder.AddColumn<string>(
                name: "currency_symbol",
                schema: "public",
                table: "tenants",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "৳");

            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "public",
                table: "tenants",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "established_year",
                schema: "public",
                table: "tenants",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "locale",
                schema: "public",
                table: "tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "en-US");

            migrationBuilder.AddColumn<string>(
                name: "logo_url",
                schema: "public",
                table: "tenants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "phone",
                schema: "public",
                table: "tenants",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "school_type",
                schema: "public",
                table: "tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "state",
                schema: "public",
                table: "tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "subscription_expires_at",
                schema: "public",
                table: "tenants",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "timezone",
                schema: "public",
                table: "tenants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Asia/Dhaka");

            migrationBuilder.AddColumn<string>(
                name: "website",
                schema: "public",
                table: "tenants",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_city",
                schema: "public",
                table: "tenants",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_is_active",
                schema: "public",
                table: "tenants",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_school_type",
                schema: "public",
                table: "tenants",
                column: "school_type");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_state",
                schema: "public",
                table: "tenants",
                column: "state");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenants_city",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "IX_tenants_is_active",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "IX_tenants_school_type",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropIndex(
                name: "IX_tenants_state",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "address",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "city",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "country",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "currency",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "currency_symbol",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "email",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "established_year",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "locale",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "logo_url",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "phone",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "school_type",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "state",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "subscription_expires_at",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "timezone",
                schema: "public",
                table: "tenants");

            migrationBuilder.DropColumn(
                name: "website",
                schema: "public",
                table: "tenants");
        }
    }
}
