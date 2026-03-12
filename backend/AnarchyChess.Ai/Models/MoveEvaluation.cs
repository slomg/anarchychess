using ProtoBuf;

namespace AnarchyChess.Ai.Models;

[ProtoContract]
public record MoveEvaluation(BitMove Move, int EvalForBot);
