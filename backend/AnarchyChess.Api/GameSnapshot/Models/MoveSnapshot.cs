using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MoveSnapshot")]
public record MoveSnapshot(
    MovePath Path,
    string Fen,
    GameColor MovedBy,
    string San,
    double TimeLeft
);
