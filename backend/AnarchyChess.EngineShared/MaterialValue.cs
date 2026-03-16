namespace AnarchyChess.EngineShared;

public static class MaterialValue
{
    public static int GetPieceValue(PieceType type) =>
        type switch
        {
            PieceType.King => 350,
            PieceType.Queen => 1000,
            PieceType.Pawn => 100,
            PieceType.Rook => 500,
            PieceType.Bishop => 300,
            PieceType.Horsey => 300,

            PieceType.Knook => 400,
            PieceType.Antiqueen => 300,
            PieceType.UnderagePawn => 150,
            PieceType.SterilePawn => 80,
            PieceType.Checker => 350,

            _ => 0,
        };
}
