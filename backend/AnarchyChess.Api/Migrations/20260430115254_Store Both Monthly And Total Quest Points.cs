using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnarchyChess.Api.Migrations
{
    /// <inheritdoc />
    public partial class StoreBothMonthlyAndTotalQuestPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "points",
                table: "quest_points",
                newName: "monthly_points"
            );

            migrationBuilder.AddColumn<int>(
                name: "total_points",
                table: "quest_points",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.Sql(
                @"UPDATE quest_points SET total_points = monthly_points WHERE total_points = 0;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "total_points", table: "quest_points");

            migrationBuilder.RenameColumn(
                name: "monthly_points",
                table: "quest_points",
                newName: "points"
            );
        }
    }
}
