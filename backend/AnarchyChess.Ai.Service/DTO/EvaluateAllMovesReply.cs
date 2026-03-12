using AnarchyChess.Ai.Models;

namespace AnarchyChess.Ai.Service.DTO;

public record EvaluateAllMovesReply(MoveEvaluation[] Moves);
