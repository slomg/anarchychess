using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AnarchyChess.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteMovesFromGameArchives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "move_side_effect_archives");

            migrationBuilder.DropTable(
                name: "piece_spawn_archives");

            migrationBuilder.DropTable(
                name: "move_archives");

            migrationBuilder.DropColumn(
                name: "country_code",
                table: "player_archives");

            migrationBuilder.DropColumn(
                name: "final_time_remaining",
                table: "player_archives");

            migrationBuilder.DropColumn(
                name: "new_rating",
                table: "player_archives");

            migrationBuilder.DropColumn(
                name: "rating_change",
                table: "player_archives");

            migrationBuilder.DropColumn(
                name: "game_source",
                table: "game_archives");

            migrationBuilder.DropColumn(
                name: "initial_fen",
                table: "game_archives");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country_code",
                table: "player_archives",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "final_time_remaining",
                table: "player_archives",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "new_rating",
                table: "player_archives",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rating_change",
                table: "player_archives",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "game_source",
                table: "game_archives",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "initial_fen",
                table: "game_archives",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "move_archives",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    captures = table.Column<byte[]>(type: "smallint[]", nullable: false),
                    from_idx = table.Column<byte>(type: "smallint", nullable: false),
                    game_archive_game_token = table.Column<string>(type: "text", nullable: true),
                    move_number = table.Column<int>(type: "integer", nullable: false),
                    promotes_to = table.Column<int>(type: "integer", nullable: true),
                    san = table.Column<string>(type: "text", nullable: false),
                    time_left = table.Column<double>(type: "double precision", nullable: false),
                    to_idx = table.Column<byte>(type: "smallint", nullable: false),
                    triggers = table.Column<byte[]>(type: "smallint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_move_archives", x => x.id);
                    table.ForeignKey(
                        name: "fk_move_archives_game_archives_game_archive_game_token",
                        column: x => x.game_archive_game_token,
                        principalTable: "game_archives",
                        principalColumn: "game_token");
                });

            migrationBuilder.CreateTable(
                name: "move_side_effect_archives",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from_idx = table.Column<byte>(type: "smallint", nullable: false),
                    move_archive_id = table.Column<int>(type: "integer", nullable: true),
                    to_idx = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_move_side_effect_archives", x => x.id);
                    table.ForeignKey(
                        name: "fk_move_side_effect_archives_move_archives_move_archive_id",
                        column: x => x.move_archive_id,
                        principalTable: "move_archives",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "piece_spawn_archives",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    color = table.Column<int>(type: "integer", nullable: true),
                    move_archive_id = table.Column<int>(type: "integer", nullable: true),
                    pos_idx = table.Column<byte>(type: "smallint", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_piece_spawn_archives", x => x.id);
                    table.ForeignKey(
                        name: "fk_piece_spawn_archives_move_archives_move_archive_id",
                        column: x => x.move_archive_id,
                        principalTable: "move_archives",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_move_archives_game_archive_game_token",
                table: "move_archives",
                column: "game_archive_game_token");

            migrationBuilder.CreateIndex(
                name: "ix_move_side_effect_archives_move_archive_id",
                table: "move_side_effect_archives",
                column: "move_archive_id");

            migrationBuilder.CreateIndex(
                name: "ix_piece_spawn_archives_move_archive_id",
                table: "piece_spawn_archives",
                column: "move_archive_id");
        }
    }
}
