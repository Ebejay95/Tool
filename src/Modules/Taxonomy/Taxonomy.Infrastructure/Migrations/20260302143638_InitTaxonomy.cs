using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxonomy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitTaxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "taxonomy");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "taxonomy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "taxonomy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                schema: "taxonomy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Label_UserId",
                schema: "taxonomy",
                table: "Categories",
                columns: new[] { "Label", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId",
                schema: "taxonomy",
                table: "Categories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_taxonomy_OutboxMessages_ProcessedOn",
                schema: "taxonomy",
                table: "OutboxMessages",
                column: "ProcessedOn",
                filter: "\"ProcessedOn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Label_UserId",
                schema: "taxonomy",
                table: "Tags",
                columns: new[] { "Label", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_UserId",
                schema: "taxonomy",
                table: "Tags",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories",
                schema: "taxonomy");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "taxonomy");

            migrationBuilder.DropTable(
                name: "Tags",
                schema: "taxonomy");
        }
    }
}
