using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Archi.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserPrdFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AddColumn<bool>(
                name: "IsVaultMember",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OauthId",
                table: "users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_OauthId",
                table: "users",
                column: "OauthId",
                unique: true,
                filter: "\"OauthId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_OauthId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsVaultMember",
                table: "users");

            migrationBuilder.DropColumn(
                name: "OauthId",
                table: "users");
        }
    }
}
