namespace AnarchyChess.Ai.Service.DTO;

public record EvaluateAllMovesReply(IReadOnlyCollection<AiEngineMove> Moves);
