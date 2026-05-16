using System;
using Archi.Api.Contracts.Archive;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Archi.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddArchiveAndVibeTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "users",
                newName: "created_at");

            migrationBuilder.CreateTable(
                name: "archive_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    category = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    metadata = table.Column<ArchiveMetadata>(type: "jsonb", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    referral_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archive_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_archive_items_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vibe_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vibe_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_vibe_tags_archive_items_item_id",
                        column: x => x.item_id,
                        principalTable: "archive_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_archive_items_external_id",
                table: "archive_items",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "IX_archive_items_user_id_external_id_category",
                table: "archive_items",
                columns: new[] { "user_id", "external_id", "category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vibe_tags_item_id",
                table: "vibe_tags",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_vibe_tags_tag_name",
                table: "vibe_tags",
                column: "tag_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vibe_tags");

            migrationBuilder.DropTable(
                name: "archive_items");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "users",
                newName: "CreatedAt");
        }
    }
}
