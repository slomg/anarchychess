using System.Runtime.CompilerServices;
using AnarchyChess.Ai.Helpers;
using AnarchyChess.Ai.MagicTables;
using AnarchyChess.Ai.Models;
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
        BitPiece piece,
        byte position,
        ref UInt128 seenThrows,
        int depth,
        int maxDepth,
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
            piece,
            origin: position,
            bounceFrom: position,
            underagePawnsBitboard: underagePawnsBitboard,
            ref visitedMask,
            moves,
            ref moveCount
        );
        GenerateIlVaticanoMoves(
            board,
            piece,
            position,
            underagePawnsBitboard: underagePawnsBitboard,
            moves,
            ref moveCount
        );
    }

    private static void GenerateBounces(
        BitBoard board,
        BitPiece piece,
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
            origin,
            piece,
            attacks: ref attacks,
            underagePawnsBitboard: underagePawnsBitboard,
            moves,
            ref moveCount
        );

        attacks &= ~board.BitboardForFriendOf(piece.Color);
        if (attacks == 0)
        {
            return;
        }

        visitedMask |= attacks;
        UInt128 edges = attacks & BitboardConstants.EdgeMasks & ~board.Occupancy;

        BitboardHelpers.CreateMoveFromAttacks(
            origin,
            piece,
            attacks,
            board.Occupancy,
            moves,
            ref moveCount
        );

        while (edges != 0)
        {
            byte edgeSquare = BitboardHelpers.BitScanForward(ref edges);
            GenerateBounces(
                board,
                piece,
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
        BitPiece piece,
        byte position,
        UInt128 underagePawnsBitboard,
        Span<BitMove> moves,
        ref int moveCount
    )
    {
        UInt128 friendlyBishops = board.BitboardFor(PieceType.Bishop, piece.Color);
        if (friendlyBishops == 0)
        {
            return;
        }

        UInt128 enemyPieces = board.BitboardForEnemyOf(piece.Color);

        for (int dir = 0; dir < 4; dir++)
        {
            UInt128 targetBishopMask = IlVaticanoTargetBishopMaskByDir[position, dir];
            if ((friendlyBishops & targetBishopMask) == 0)
            {
                continue;
            }

            UInt128 captures = IlVaticanoBetweenMasksByDir[position, dir];
            if ((captures & enemyPieces) != captures)
            {
                continue;
            }

            ForcedMovePriority forcedMovePriority =
                (underagePawnsBitboard & captures) != 0
                    ? ForcedMovePriority.UnderagePawn
                    : ForcedMovePriority.None;

            byte targetBishopSquare = BitboardHelpers.BitScanForward(ref targetBishopMask);
            BitMove move = new()
            {
                From = position,
                To = position,
                Piece = piece,
                CapturesMask = captures,
                SpecialMoveType = SpecialMoveType.IlVaticano,
                ForcedMovePriority = forcedMovePriority,
            };
            moves[moveCount++] = move;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddUnderagePawnCapture(
        byte position,
        BitPiece piece,
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
            byte toSquare = BitboardHelpers.BitScanForward(ref underagePawnCapture);

            BitMove move = new()
            {
                From = position,
                To = toSquare,
                Piece = piece,
                CapturesMask = UInt128.One << toSquare,
                ForcedMovePriority = ForcedMovePriority.UnderagePawn,
            };
            moves[moveCount++] = move;
        }
    }
}
