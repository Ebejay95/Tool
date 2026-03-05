using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Measures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryFromMeasures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                schema: "measures",
                table: "Measures");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "measures",
                table: "Measures",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }
    }
}
