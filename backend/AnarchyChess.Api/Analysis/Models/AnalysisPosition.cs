using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Analysis.Models;

public record AnalysisPosition(
    string Fen,
    string? San,
    MoveOptions MoveOptions,
    GameColor SideToMove,
    GameEndStatus? EndStatus
);
