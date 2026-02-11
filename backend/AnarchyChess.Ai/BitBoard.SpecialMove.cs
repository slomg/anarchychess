using AnarchyChess.Ai.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public partial class BitBoard
{
    private void ApplySpecialMove(BitMove move)
    {
        switch (move.SpecialMoveType)
        {
            case SpecialMoveType.KingsideCastle:
                CastleInfo kingsideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)move.Piece.Color,
                    (int)CastleType.Kingside
                ];

                MovePiece(
                    PieceType.Rook,
                    move.Piece.Color,
                    from: kingsideCastleInfo.RookStart,
                    to: kingsideCastleInfo.RookDest
                );
                break;

            case SpecialMoveType.QueensideCastle:
                CastleInfo queensideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)move.Piece.Color,
                    (int)CastleType.Queenside
                ];

                MovePiece(
                    PieceType.Rook,
                    move.Piece.Color,
                    from: queensideCastleInfo.RookStart,
                    to: queensideCastleInfo.RookDest
                );
                break;

            case SpecialMoveType.VerticalCastle:
                CastleInfo verticalCastleInfo = BitboardConstants.CastlesByColor[
                    (int)move.Piece.Color,
                    (int)CastleType.Vertical
                ];

                MovePiece(
                    PieceType.Rook,
                    move.Piece.Color,
                    from: verticalCastleInfo.RookStart,
                    to: verticalCastleInfo.RookDest
                );
                break;

            case SpecialMoveType.IlVaticano:
                MovePiece(PieceType.Bishop, move.Piece.Color, from: move.To, to: move.From);
                break;

            case SpecialMoveType.RadioactiveBetaDecay:
                SpawnPiece(PieceType.Rook, move.Piece.Color, at: (byte)(move.To - 1));
                SpawnPiece(PieceType.Horsey, move.Piece.Color, at: (byte)(move.To + 1));

                byte pawnSpawn =
                    move.Piece.Color is BitPieceColor.White
                        ? (byte)(move.To + 10)
                        : (byte)(move.To - 10);
                SpawnPiece(PieceType.SterilePawn, move.Piece.Color, at: pawnSpawn);
                break;

            case SpecialMoveType.OmnipotentPawnSpawn:
                SpawnPiece(PieceType.Pawn, move.Piece.Color, at: move.To);
                break;
        }
    }

    private void UndoSpecialMove(MoveUndoState undoState)
    {
        switch (undoState.SpecialMoveType)
        {
            case SpecialMoveType.KingsideCastle:
                CastleInfo kingsideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)undoState.Piece.Color,
                    (int)CastleType.Kingside
                ];

                MovePiece(
                    PieceType.Rook,
                    undoState.Piece.Color,
                    from: kingsideCastleInfo.RookDest,
                    to: kingsideCastleInfo.RookStart
                );
                break;

            case SpecialMoveType.QueensideCastle:
                CastleInfo queensideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)undoState.Piece.Color,
                    (int)CastleType.Queenside
                ];

                MovePiece(
                    PieceType.Rook,
                    undoState.Piece.Color,
                    from: queensideCastleInfo.RookDest,
                    to: queensideCastleInfo.RookStart
                );
                break;

            case SpecialMoveType.VerticalCastle:
                CastleInfo verticalCastleInfo = BitboardConstants.CastlesByColor[
                    (int)undoState.Piece.Color,
                    (int)CastleType.Vertical
                ];

                MovePiece(
                    PieceType.Rook,
                    undoState.Piece.Color,
                    from: verticalCastleInfo.RookDest,
                    to: verticalCastleInfo.RookStart
                );
                break;

            case SpecialMoveType.IlVaticano:
                MovePiece(
                    PieceType.Bishop,
                    undoState.Piece.Color,
                    from: undoState.From,
                    to: undoState.To
                );
                break;

            case SpecialMoveType.RadioactiveBetaDecay:
                RemovePiece(PieceType.Rook, undoState.Piece.Color, at: (byte)(undoState.To - 1));
                RemovePiece(PieceType.Horsey, undoState.Piece.Color, at: (byte)(undoState.To + 1));

                byte pawnSpawn =
                    undoState.Piece.Color is BitPieceColor.White
                        ? (byte)(undoState.To + 10)
                        : (byte)(undoState.To - 10);
                RemovePiece(PieceType.SterilePawn, undoState.Piece.Color, at: pawnSpawn);
                break;
        }
    }
}
