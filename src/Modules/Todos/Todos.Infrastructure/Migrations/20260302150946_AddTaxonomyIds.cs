using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Todos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxonomyIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoryIds",
                schema: "todos",
                table: "Todos",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "TagIds",
                schema: "todos",
                table: "Todos",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryIds",
                schema: "todos",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "TagIds",
                schema: "todos",
                table: "Todos");
        }
    }
}
