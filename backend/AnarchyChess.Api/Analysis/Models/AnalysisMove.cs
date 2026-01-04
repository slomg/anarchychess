using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameLogic.Models;

namespace AnarchyChess.Api.Analysis.Models;

public record AnalysisMove(string Fen, AlgebraicPoint PiecePosition, MoveKey MoveKey);
