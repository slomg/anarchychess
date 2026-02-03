using AnarchyChess.Api.Game.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Analysis.Models;

public record AnalysisMove(string Fen, AlgebraicPoint PiecePosition, MoveKey MoveKey);
