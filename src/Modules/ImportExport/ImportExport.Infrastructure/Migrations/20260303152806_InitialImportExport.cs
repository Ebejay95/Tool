using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ImportExport.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialImportExport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "importexport");

            migrationBuilder.CreateTable(
                name: "MappingProfiles",
                schema: "importexport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EntityTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FieldRules = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MappingProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MappingProfiles_UserId_EntityTypeName",
                schema: "importexport",
                table: "MappingProfiles",
                columns: new[] { "UserId", "EntityTypeName" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MappingProfiles",
                schema: "importexport");
        }
    }
}
