using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public partial class BitBoard
{
    private void ApplySpecialMove(BitMove move)
    {
        switch (move.SpecialMoveType)
        {
            case SpecialMoveType.KingsideCastle:
                ref UInt128 rookKingsideBitboard = ref BitboardFor(
                    PieceType.Rook,
                    move.Piece.Color
                );
                CastleInfo kingsideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)move.Piece.Color,
                    (int)CastleType.Kingside
                ];

                MovePiece(
                    ref rookKingsideBitboard,
                    from: kingsideCastleInfo.RookStart,
                    to: kingsideCastleInfo.RookDest,
                    move.Piece.Color
                );
                break;

            case SpecialMoveType.QueensideCastle:
                ref UInt128 rookQueensideBitboard = ref BitboardFor(
                    PieceType.Rook,
                    move.Piece.Color
                );
                CastleInfo queensideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)move.Piece.Color,
                    (int)CastleType.Queenside
                ];

                MovePiece(
                    ref rookQueensideBitboard,
                    from: queensideCastleInfo.RookStart,
                    to: queensideCastleInfo.RookDest,
                    move.Piece.Color
                );
                break;

            case SpecialMoveType.VerticalCastle:
                ref UInt128 rookVerticalBitboard = ref BitboardFor(
                    PieceType.Rook,
                    move.Piece.Color
                );
                CastleInfo verticalCastleInfo = BitboardConstants.CastlesByColor[
                    (int)move.Piece.Color,
                    (int)CastleType.Vertical
                ];

                MovePiece(
                    ref rookVerticalBitboard,
                    from: verticalCastleInfo.RookStart,
                    to: verticalCastleInfo.RookDest,
                    move.Piece.Color
                );
                break;

            case SpecialMoveType.IlVaticano:
                ref UInt128 targetBishopBitboard = ref BitboardFor(
                    PieceType.Bishop,
                    move.Piece.Color
                );
                MovePiece(ref targetBishopBitboard, from: move.To, to: move.From, move.Piece.Color);
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
                ref UInt128 rookKingsideBitboard = ref BitboardFor(
                    PieceType.Rook,
                    undoState.Piece.Color
                );
                CastleInfo kingsideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)undoState.Piece.Color,
                    (int)CastleType.Kingside
                ];

                MovePiece(
                    ref rookKingsideBitboard,
                    from: kingsideCastleInfo.RookDest,
                    to: kingsideCastleInfo.RookStart,
                    undoState.Piece.Color
                );
                break;

            case SpecialMoveType.QueensideCastle:
                ref UInt128 rookQueensideBitboard = ref BitboardFor(
                    PieceType.Rook,
                    undoState.Piece.Color
                );
                CastleInfo queensideCastleInfo = BitboardConstants.CastlesByColor[
                    (int)undoState.Piece.Color,
                    (int)CastleType.Queenside
                ];

                MovePiece(
                    ref rookQueensideBitboard,
                    from: queensideCastleInfo.RookDest,
                    to: queensideCastleInfo.RookStart,
                    undoState.Piece.Color
                );
                break;

            case SpecialMoveType.VerticalCastle:
                ref UInt128 rookVerticalBitboard = ref BitboardFor(
                    PieceType.Rook,
                    undoState.Piece.Color
                );
                CastleInfo verticalCastleInfo = BitboardConstants.CastlesByColor[
                    (int)undoState.Piece.Color,
                    (int)CastleType.Vertical
                ];

                MovePiece(
                    ref rookVerticalBitboard,
                    from: verticalCastleInfo.RookDest,
                    to: verticalCastleInfo.RookStart,
                    undoState.Piece.Color
                );
                break;

            case SpecialMoveType.IlVaticano:
                ref UInt128 targetBishopBitboard = ref BitboardFor(
                    PieceType.Bishop,
                    undoState.Piece.Color
                );
                MovePiece(
                    ref targetBishopBitboard,
                    from: undoState.From,
                    to: undoState.To,
                    undoState.Piece.Color
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
