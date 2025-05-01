using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialMediaAPI.Migrations
{
    public partial class newPropertiesToFollow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BlockedAt",
                schema: "public",
                table: "Follows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnblockedAt",
                schema: "public",
                table: "Follows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UnfollowedAt",
                schema: "public",
                table: "Follows",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "Follows",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "CURRENT_TIMESTAMP");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockedAt",
                schema: "public",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "UnblockedAt",
                schema: "public",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "UnfollowedAt",
                schema: "public",
                table: "Follows");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "Follows");
        }
    }
}
