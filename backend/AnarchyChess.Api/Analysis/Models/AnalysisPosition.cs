using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Analysis.Models;

public record AnalysisPosition(
    string Fen,
    string San,
    IReadOnlyCollection<MovePath> LegalMoves,
    GameColor SideToMove,
    GameEndStatus? EndStatus
);
