namespace AnarchyChess.Ai;

public static class EngineConstants
{
    public const int AlphaStart = -10_000_000;
    public const int BetaStart = 10_000_000;

    public const int NullMoveReduction = 2;
    public const int FutilityMargin = 350;
    public const int MaxMoves = 256;
    public const int MaxDepth = 32;

    public static readonly int[,] LmrTable = CreateLMR();

    static int[,] CreateLMR()
    {
        int[,] lm = new int[MaxDepth, MaxMoves];
        for (int depth = 1; depth < MaxDepth; depth++)
        {
            for (int move = 1; move < MaxMoves; move++)
            {
                double reduction = 0.99 + Math.Log(depth) * Math.Log(move) / 3.14;

                lm[depth, move] = Math.Max((int)Math.Round(reduction), 1);
            }
        }
        return lm;
    }
}
