using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AnarchyChess.Api.Migrations
{
    /// <inheritdoc />
    public partial class Vote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vote_option",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vote_option", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vote_option_pairs",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    option_a_id = table.Column<int>(type: "integer", nullable: false),
                    option_b_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vote_option_pairs", x => x.id);
                    table.ForeignKey(
                        name: "fk_vote_option_pairs_vote_option_option_a_id",
                        column: x => x.option_a_id,
                        principalTable: "vote_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_vote_option_pairs_vote_option_option_b_id",
                        column: x => x.option_b_id,
                        principalTable: "vote_option",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pending_user_votes",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    vote_pair_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pending_user_votes", x => x.user_id);
                    table.ForeignKey(
                        name: "fk_pending_user_votes_vote_option_pairs_vote_pair_id",
                        column: x => x.vote_pair_id,
                        principalTable: "vote_option_pairs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_votes",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    vote_pair_id = table.Column<int>(type: "integer", nullable: false),
                    ip_address = table.Column<string>(type: "text", nullable: false),
                    picked_option_a = table.Column<bool>(type: "boolean", nullable: false),
                    vote_weight = table.Column<float>(type: "real", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_votes", x => new { x.user_id, x.vote_pair_id });
                    table.ForeignKey(
                        name: "fk_user_votes_vote_option_pairs_vote_pair_id",
                        column: x => x.vote_pair_id,
                        principalTable: "vote_option_pairs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_pending_user_votes_vote_pair_id",
                table: "pending_user_votes",
                column: "vote_pair_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_votes_ip_address",
                table: "user_votes",
                column: "ip_address");

            migrationBuilder.CreateIndex(
                name: "ix_user_votes_vote_pair_id",
                table: "user_votes",
                column: "vote_pair_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_option_pairs_option_a_id",
                table: "vote_option_pairs",
                column: "option_a_id");

            migrationBuilder.CreateIndex(
                name: "ix_vote_option_pairs_option_b_id",
                table: "vote_option_pairs",
                column: "option_b_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pending_user_votes");

            migrationBuilder.DropTable(
                name: "user_votes");

            migrationBuilder.DropTable(
                name: "vote_option_pairs");

            migrationBuilder.DropTable(
                name: "vote_option");
        }
    }
}
