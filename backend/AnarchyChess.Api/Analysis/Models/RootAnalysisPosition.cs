using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.GameSnapshot.Models;

namespace AnarchyChess.Api.Analysis.Models;

public record RootAnalysisPosition(string Fen, GameColor SideToMove, MoveOptions MoveOptions);
