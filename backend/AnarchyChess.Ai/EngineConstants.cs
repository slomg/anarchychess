using AnarchyChess.Ai.Evaluation;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Ai;

public static class EngineConstants
{
    public const int AlphaStart = -10_000_000;
    public const int BetaStart = 10_000_000;

    public const int NullMoveReduction = 2;
    public const int MaxMoves = 256;
    public const int MaxDepth = 32;

    public static readonly int DeltaPruningMargin = MaterialEvaluator.GetPieceValue(PieceType.Pawn);

    public static readonly int[,] LmrTable = CreateLMR();

    static int[,] CreateLMR()
    {
        int[,] lmr = new int[MaxDepth, MaxMoves];
        for (int depth = 1; depth < MaxDepth; depth++)
        {
            for (int move = 1; move < MaxMoves; move++)
            {
                int reduction = (int)Math.Round(0.99 + Math.Log(depth) * Math.Log(move) / 3.14);
                if (depth > 2 && depth - reduction < 2)
                {
                    reduction = depth - 2;
                }

                lmr[depth, move] = reduction;
            }
        }
        return lmr;
    }
}
