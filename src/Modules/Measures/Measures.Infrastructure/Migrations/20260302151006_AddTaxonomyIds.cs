using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Measures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxonomyIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryIds",
                schema: "measures",
                table: "Measures",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "TagIds",
                schema: "measures",
                table: "Measures",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryIds",
                schema: "measures",
                table: "Measures");

            migrationBuilder.DropColumn(
                name: "TagIds",
                schema: "measures",
                table: "Measures");
        }
    }
}
