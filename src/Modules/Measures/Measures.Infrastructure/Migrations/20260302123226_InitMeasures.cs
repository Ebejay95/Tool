using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Measures.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitMeasures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "measures");

            migrationBuilder.CreateTable(
                name: "Measures",
                schema: "measures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsoId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CostEur = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EffortHours = table.Column<double>(type: "double precision", nullable: false),
                    ImpactRisk = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<int>(type: "integer", nullable: false),
                    Dependencies = table.Column<string>(type: "jsonb", nullable: false),
                    Justification = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ConfDataQuality = table.Column<int>(type: "integer", nullable: false),
                    ConfDataSourceCount = table.Column<int>(type: "integer", nullable: false),
                    ConfDataRecency = table.Column<int>(type: "integer", nullable: false),
                    ConfSpecificity = table.Column<int>(type: "integer", nullable: false),
                    GraphDependentsCount = table.Column<int>(type: "integer", nullable: false),
                    GraphImpactMultiplier = table.Column<double>(type: "double precision", nullable: false),
                    GraphTotalCost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GraphCostEfficiency = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measures", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "measures",
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

            migrationBuilder.CreateIndex(
                name: "IX_Measures_IsoId_UserId",
                schema: "measures",
                table: "Measures",
                columns: new[] { "IsoId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Measures_UserId",
                schema: "measures",
                table: "Measures",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_measures_OutboxMessages_ProcessedOn",
                schema: "measures",
                table: "OutboxMessages",
                column: "ProcessedOn",
                filter: "\"ProcessedOn\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Measures",
                schema: "measures");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "measures");
        }
    }
}
