using AnarchyChess.Ai.Service.DTO;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.AnarchyBot.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.AnarchyBot.Models.AiEngineMoveSurrogate")]
public readonly record struct AiEngineMoveSurrogate(
    AlgebraicPoint From,
    AlgebraicPoint To,
    IReadOnlyCollection<AlgebraicPoint>? Captures,
    PieceType? PromotesTo,
    int EvalForBot
);

[RegisterConverter]
public sealed class AiEngineMoveReplySurrogateConverter
    : IConverter<AiEngineMoveReply, AiEngineMoveSurrogate>
{
    public AiEngineMoveReply ConvertFromSurrogate(in AiEngineMoveSurrogate surrogate) =>
        new(
            From: surrogate.From,
            To: surrogate.To,
            Captures: surrogate.Captures,
            PromotesTo: surrogate.PromotesTo,
            EvalForBot: surrogate.EvalForBot
        );

    public AiEngineMoveSurrogate ConvertToSurrogate(in AiEngineMoveReply value) =>
        new(
            From: value.From,
            To: value.To,
            Captures: value.Captures,
            PromotesTo: value.PromotesTo,
            EvalForBot: value.EvalForBot
        );
}
