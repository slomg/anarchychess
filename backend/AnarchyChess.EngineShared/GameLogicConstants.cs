namespace AnarchyChess.EngineShared;

public static class GameLogicConstants
{
    public const int BoardWidth = 10;
    public const int BoardHeight = 10;

    public const int MinEnPassantTriggerDistance = 2;
    public const int MaxEnPassantTriggerDistance = 3;

    public static readonly IReadOnlySet<PieceType> PawnLikePieces = new HashSet<PieceType>(
        [PieceType.Pawn, PieceType.UnderagePawn, PieceType.SterilePawn]
    );

    public const int PawnLikeMask =
        1 << (int)PieceType.Pawn
        | 1 << (int)PieceType.UnderagePawn
        | 1 << (int)PieceType.SterilePawn;

    public static readonly IReadOnlyCollection<PieceType> PromotablePieces =
    [
        PieceType.Queen,
        PieceType.Rook,
        PieceType.Bishop,
        PieceType.Horsey,
        PieceType.Knook,
        PieceType.Antiqueen,
        PieceType.Checker,
    ];

    public static readonly AlgebraicPoint WhiteOmnipotentPawnSquare = new("h3");
    public static readonly byte WhiteOmnipotentPawnIdx = WhiteOmnipotentPawnSquare.AsIdx();
    public static readonly UInt128 WhiteOmnipotentPawnMask = UInt128.One << WhiteOmnipotentPawnIdx;

    public static readonly AlgebraicPoint BlackOmnipotentPawnSquare = new("h8");
    public static readonly byte BlackOmnipotentPawnIdx = BlackOmnipotentPawnSquare.AsIdx();
    public static readonly UInt128 BlackOmnipotentPawnMask = UInt128.One << BlackOmnipotentPawnIdx;
}
