using System.Runtime.CompilerServices;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai.BitPieceDefinition;

public sealed class BitBishopDefinition : IBitPieceDefinition
{
    private static readonly UInt128[,] IlVaticanoTargetBishopMaskByDir =
        MakeIlVaticanoTargetBishopMasks();
    private static readonly UInt128[,] IlVaticanoBetweenMasksByDir = MakeIlVaticanoBetweenMasks();

    private static UInt128[,] MakeIlVaticanoTargetBishopMasks()
    {
        int[] deltaIlVaticanoRanks = [-3, 3, 0, 0];
        int[] deltaIlVaticanoFiles = [0, 0, -3, 3];

        UInt128[,] ilVaticanoMasks = new UInt128[10 * 10, 4];
        for (int rank = 0; rank < 10; rank++)
        {
            for (int file = 0; file < 10; file++)
            {
                int squareIdx = rank * 10 + file;

                for (int direction = 0; direction < 4; direction++)
                {
                    int deltaRank = rank + deltaIlVaticanoRanks[direction];
                    int deltaFile = file + deltaIlVaticanoFiles[direction];
                    if (deltaRank >= 0 && deltaRank < 10 && deltaFile >= 0 && deltaFile < 10)
                    {
                        ilVaticanoMasks[squareIdx, direction] =
                            UInt128.One << (byte)(deltaRank * 10 + deltaFile);
                    }
                }
            }
        }

        return ilVaticanoMasks;
    }

    private static UInt128[,] MakeIlVaticanoBetweenMasks()
    {
        int[] deltaIlVaticanoRanks = [-3, 3, 0, 0];
        int[] deltaIlVaticanoFiles = [0, 0, -3, 3];

        UInt128[,] betweenMasks = new UInt128[10 * 10, 4];
        for (int rank = 0; rank < 10; rank++)
        {
            for (int file = 0; file < 10; file++)
            {
                int squareIdx = rank * 10 + file;

                for (int direction = 0; direction < 4; direction++)
                {
                    int stepRank = Math.Sign(deltaIlVaticanoRanks[direction]);
                    int stepFile = Math.Sign(deltaIlVaticanoFiles[direction]);

                    UInt128 mask = 0;

                    for (int step = 1; step <= 2; step++)
                    {
                        int r = stepRank * step + rank;
                        int f = stepFile * step + file;
                        if (r >= 0 && r < 10 && f >= 0 && f < 10)
                        {
                            mask |= UInt128.One << (r * 10 + f);
                        }
                    }

                    betweenMasks[squareIdx, direction] = mask;
                }
            }
        }

        return betweenMasks;
    }

    public void GenerateMoves(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 visitedMask = 0;
        UInt128 underagePawnsBitboard =
            board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.White)
            | board.BitboardFor(PieceType.UnderagePawn, BitPieceColor.Black);

        GenerateBounces(
            board,
            pieceType,
            color,
            origin: position,
            bounceFrom: position,
            underagePawnsBitboard: underagePawnsBitboard,
            ref visitedMask,
            moves,
            ref moveCount
        );
        GenerateIlVaticanoMoves(
            board,
            pieceType,
            color,
            position,
            underagePawnsBitboard: underagePawnsBitboard,
            moves,
            ref moveCount
        );
    }

    private static void GenerateBounces(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte origin,
        byte bounceFrom,
        UInt128 underagePawnsBitboard,
        ref UInt128 visitedMask,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 attacks = MagicLibrary.GetAttacks(
            MagicLibrary.BishopTable,
            bounceFrom,
            board.Occupancy
        );
        attacks &= ~visitedMask;
        AddUnderagePawnCapture(
            board,
            origin,
            pieceType,
            attacks: ref attacks,
            underagePawnsBitboard: underagePawnsBitboard,
            moves,
            ref moveCount
        );

        attacks &= ~board.BitboardForFriendOf(color);
        if (attacks == 0)
        {
            return;
        }

        visitedMask |= attacks;
        UInt128 edges = attacks & BitboardConstants.EdgeMasks & ~board.Occupancy;

        BitboardHelpers.CreateMoveFromAttacks(
            origin,
            pieceType,
            board,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );

        while (edges != 0)
        {
            byte edgeSquare = (byte)BitboardHelpers.BitScanForward(ref edges);
            GenerateBounces(
                board,
                pieceType,
                color,
                origin: origin,
                bounceFrom: edgeSquare,
                underagePawnsBitboard: underagePawnsBitboard,
                ref visitedMask,
                moves,
                ref moveCount
            );
        }
    }

    private static void GenerateIlVaticanoMoves(
        BitBoard board,
        PieceType pieceType,
        BitPieceColor color,
        byte position,
        UInt128 underagePawnsBitboard,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 friendlyBishops = board.BitboardFor(PieceType.Bishop, color);
        if (friendlyBishops == 0)
        {
            return;
        }

        UInt128 enemyPieces = board.BitboardForEnemyOf(color);

        for (int dir = 0; dir < 4; dir++)
        {
            UInt128 targetBishopMask = IlVaticanoTargetBishopMaskByDir[position, dir];
            if ((friendlyBishops & targetBishopMask) == 0)
            {
                continue;
            }

            UInt128 attacks = IlVaticanoBetweenMasksByDir[position, dir];
            if ((attacks & enemyPieces) != attacks)
            {
                continue;
            }

            ForcedMovePriority forcedMovePriority =
                (underagePawnsBitboard & attacks) != 0
                    ? ForcedMovePriority.UnderagePawn
                    : ForcedMovePriority.None;

            byte targetBishopSquare = (byte)BitboardHelpers.BitScanForward(ref targetBishopMask);
            BitMove move = new()
            {
                From = position,
                To = targetBishopSquare,
                Piece = pieceType,
                SpecialMoveType = SpecialMoveType.IlVaticano,
                ForcedMovePriority = forcedMovePriority,
            };
            while (attacks != 0)
            {
                byte attackSquare = (byte)BitboardHelpers.BitScanForward(ref attacks);
                var capturePiece = board.GetPieceAt(attackSquare);
                move.AddCapture(attackSquare, capturePiece.PieceType, capturePiece.Color);
            }
            moves[moveCount++] = move;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddUnderagePawnCapture(
        BitBoard board,
        byte position,
        PieceType pieceType,
        ref UInt128 attacks,
        UInt128 underagePawnsBitboard,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 underagePawnCapture = attacks & underagePawnsBitboard;
        attacks &= ~underagePawnsBitboard;
        while (underagePawnCapture != 0)
        {
            byte toSquare = (byte)BitboardHelpers.BitScanForward(ref underagePawnCapture);
            var capturePiece = board.GetPieceAt(toSquare);

            BitMove move = new()
            {
                From = position,
                To = toSquare,
                Piece = pieceType,
                ForcedMovePriority = ForcedMovePriority.UnderagePawn,
            };
            move.AddCapture(toSquare, capturePiece.PieceType, capturePiece.Color);
            moves[moveCount++] = move;
        }
    }
}
