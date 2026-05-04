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

            case SpecialMoveType.Throw:
                UInt128 enemyPieces = BitboardForEnemyOf(move.Piece.Color);
                if ((enemyPieces & (UInt128.One << move.To)) != 0)
                {
                    AddStun(move.To, forTurns: 4);
                }
                else
                {
                    AddStun(move.To, forTurns: 2);
                }
                break;

            case SpecialMoveType.QueentumTunnel:
                SpawnPiece(PieceType.Antiqueen, move.Piece.Color, at: move.From);
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

            case SpecialMoveType.RadioactiveBetaDecay:
                RemovePiece(PieceType.Rook, undoState.Piece.Color, at: (byte)(undoState.To - 1));
                RemovePiece(PieceType.Horsey, undoState.Piece.Color, at: (byte)(undoState.To + 1));

                byte pawnSpawn =
                    undoState.Piece.Color is BitPieceColor.White
                        ? (byte)(undoState.To + 10)
                        : (byte)(undoState.To - 10);
                RemovePiece(PieceType.SterilePawn, undoState.Piece.Color, at: pawnSpawn);
                break;

            case SpecialMoveType.Throw:
                UInt128 enemyPieces = BitboardForEnemyOf(undoState.Piece.Color);
                if ((enemyPieces & (UInt128.One << undoState.To)) != 0)
                {
                    RemoveStun(undoState.To, forTurns: 4);
                }
                else
                {
                    RemoveStun(undoState.To, forTurns: 2);
                }
                break;

            case SpecialMoveType.QueentumTunnel:
                SpawnPiece(PieceType.Antiqueen, undoState.Piece.Color, at: undoState.To);
                break;
        }
    }
}
