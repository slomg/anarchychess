using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Analysis.Models;

public record AnalysisPosition(
    string Fen,
    string San,
    IReadOnlyCollection<MovePath> LegalMoves,
    GameColor SideToMove,
    GameEndStatus? EndStatus
);
