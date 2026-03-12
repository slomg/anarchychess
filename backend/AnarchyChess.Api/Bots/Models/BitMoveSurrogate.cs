using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Bots.Models;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Bots.Models.BitPieceSurrogate")]
public readonly record struct BitPieceSurrogate(PieceType Type, BitPieceColor Color);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Bots.Models.BitMoveSurrogate")]
public readonly record struct BitMoveSurrogate(
    byte From,
    byte To,
    BitPieceSurrogate Piece,
    UInt128 CapturesMask,
    PieceType? PromotesTo,
    ForcedMovePriority ForcedMovePriority,
    SpecialMoveType SpecialMoveType
);

[GenerateSerializer]
[Alias("AnarchyChess.Api.Bots.Models.AiEngineMoveSurrogate")]
public readonly record struct MoveEvaluationSurrogate(BitMoveSurrogate Move, int EvalForBot);

[RegisterConverter]
public sealed class AiEngineMoveSurrogateConverter
    : IConverter<MoveEvaluation, MoveEvaluationSurrogate>
{
    public MoveEvaluation ConvertFromSurrogate(in MoveEvaluationSurrogate surrogate) =>
        new(
            Move: new BitMove()
            {
                From = surrogate.Move.From,
                To = surrogate.Move.To,
                Piece = new BitPiece()
                {
                    Type = surrogate.Move.Piece.Type,
                    Color = surrogate.Move.Piece.Color,
                },
                CapturesMask = surrogate.Move.CapturesMask,
                PromotesTo = surrogate.Move.PromotesTo,
                ForcedMovePriority = surrogate.Move.ForcedMovePriority,
                SpecialMoveType = surrogate.Move.SpecialMoveType,
            },
            EvalForBot: surrogate.EvalForBot
        );

    public MoveEvaluationSurrogate ConvertToSurrogate(in MoveEvaluation value) =>
        new(
            Move: new BitMoveSurrogate(
                value.Move.From,
                value.Move.To,
                Piece: new BitPieceSurrogate(value.Move.Piece.Type, value.Move.Piece.Color),
                CapturesMask: value.Move.CapturesMask,
                PromotesTo: value.Move.PromotesTo,
                ForcedMovePriority: value.Move.ForcedMovePriority,
                SpecialMoveType: value.Move.SpecialMoveType
            ),
            EvalForBot: value.EvalForBot
        );
}
