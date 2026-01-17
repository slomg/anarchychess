using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnarchyChess.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteWinStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "win_streaks");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "win_streaks",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    current_streak_games = table.Column<string[]>(type: "text[]", nullable: false),
                    highest_streak_games = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_win_streaks", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_win_streaks_users_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
