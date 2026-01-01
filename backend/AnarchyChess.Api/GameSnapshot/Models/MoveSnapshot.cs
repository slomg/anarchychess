namespace AnarchyChess.Api.GameSnapshot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.GameSnapshot.Models.MoveSnapshot")]
public record MoveSnapshot(MovePath Path, string Fen, string San, double TimeLeft);
