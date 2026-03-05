using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxonomy.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIdFromTaxonomy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_Label_UserId",
                schema: "taxonomy",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_UserId",
                schema: "taxonomy",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Label_UserId",
                schema: "taxonomy",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UserId",
                schema: "taxonomy",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "taxonomy",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "taxonomy",
                table: "Categories");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Label",
                schema: "taxonomy",
                table: "Tags",
                column: "Label",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Label",
                schema: "taxonomy",
                table: "Categories",
                column: "Label",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tags_Label",
                schema: "taxonomy",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Label",
                schema: "taxonomy",
                table: "Categories");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "taxonomy",
                table: "Tags",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "taxonomy",
                table: "Categories",
                type: "uuid",
                nullable: true);

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
        }
    }
}
