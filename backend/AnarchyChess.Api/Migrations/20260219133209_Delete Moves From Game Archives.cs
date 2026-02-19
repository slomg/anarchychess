using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AnarchyChess.Api.Migrations
{
    /// <inheritdoc />
    public partial class DeleteMovesFromGameArchives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "color",
                table: "player_archives",
                type: "integer",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "color",
                table: "player_archives",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
