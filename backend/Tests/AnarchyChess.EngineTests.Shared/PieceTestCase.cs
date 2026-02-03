using System.Text.Json.Serialization;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Extensions;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;

namespace AnarchyChess.EngineTests.Shared;

public record MoveTestCase(
    string To,
    IEnumerable<string>? Trigger = null,
    IEnumerable<string>? Captures = null,
    IEnumerable<IntermediateSquare>? Intermediates = null,
    IEnumerable<MoveSideEffect>? SideEffects = null,
    IEnumerable<PieceSpawn>? Spawns = null,
    SpecialMoveType SpecialMoveType = SpecialMoveType.None,
    ForcedMovePriority ForcedPriority = ForcedMovePriority.None,
    PieceType? PromotesTo = null
);

public class PieceTestCase
{
    public required Piece Piece { get; init; }
    public required AlgebraicPoint Origin { get; init; }
    public GameColor MovingPlayer { get; set; }

    public List<Move> ExpectedMoves { get; init; } = [];
    public List<Move> PriorMoves { get; init; } = [];

    [JsonIgnore]
    public Dictionary<AlgebraicPoint, Piece> BlockedBy { get; } = [];

    [JsonInclude]
    [JsonPropertyName(nameof(BlockedBy))]
    public Dictionary<string, Piece> BlockedBySurrogate
    {
        get => BlockedBy.ToDictionary(x => (string)x.Key.AsAlgebraic(), x => x.Value);
        set
        {
            BlockedBy.Clear();
            foreach (var kvp in value)
                BlockedBy[new AlgebraicPoint(kvp.Key)] = kvp.Value;
        }
    }

    public string TestDecription { get; private set; } = "";

    private readonly ChessBoard _board = new();

    public static PieceTestCase From(string from, Piece piece)
    {
        var origin = new AlgebraicPoint(from);
        PieceTestCase testCase = new()
        {
            Piece = piece,
            Origin = origin,
            MovingPlayer = piece.Color ?? GameColor.White,
        };

        testCase._board.PlacePiece(origin, piece);

        return testCase;
    }

    public PieceTestCase GoesTo(
        string to,
        IEnumerable<string>? trigger = null,
        IEnumerable<string>? captures = null,
        IEnumerable<IntermediateSquare>? intermediates = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        IEnumerable<PieceSpawn>? spawns = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
    {
        ExpectedMoves.Add(
            BuildMove(
                Origin.AsAlgebraic(),
                to,
                Piece,
                trigger,
                captures,
                intermediates,
                sideEffects,
                spawns,
                specialMoveType,
                forcedPriority,
                promotesTo
            )
        );
        return this;
    }

    public PieceTestCase GoesTo(params string[] to)
    {
        foreach (var position in to)
        {
            GoesTo(position);
        }
        return this;
    }

    public PieceTestCase GoesTo(params MoveTestCase[] moves)
    {
        foreach (var move in moves)
        {
            GoesTo(
                move.To,
                move.Trigger,
                move.Captures,
                move.Intermediates,
                move.SideEffects,
                move.Spawns,
                move.SpecialMoveType,
                move.ForcedPriority,
                move.PromotesTo
            );
        }
        return this;
    }

    public PieceTestCase WithPieceAt(string position, Piece piece)
    {
        AlgebraicPoint point = new(position);
        BlockedBy.Add(point, piece);
        _board.PlacePiece(point, piece);
        return this;
    }

    public PieceTestCase WithWhitePieceAt(string position, PieceType? pieceType = null) =>
        WithPieceAt(position, PieceFactory.White(pieceType));

    public PieceTestCase WithBlackPieceAt(string position, PieceType? pieceType = null) =>
        WithPieceAt(position, PieceFactory.Black(pieceType));

    public PieceTestCase WithFriendlyPieceAt(string position, params PieceType[] excludePieces) =>
        WithPieceAt(
            position,
            new PieceFaker(color: Piece.Color)
                .RuleFor(
                    x => x.Type,
                    f => f.PickRandomWithout([.. excludePieces, PieceType.TraitorRook])
                )
                .Generate()
        );

    public PieceTestCase WithEnemyPieceAt(string position, params PieceType[] excludePieces) =>
        WithPieceAt(
            position,
            new PieceFaker(color: Piece.Color?.Invert())
                .RuleFor(
                    x => x.Type,
                    f => f.PickRandomWithout([.. excludePieces, PieceType.TraitorRook])
                )
                .Generate()
        );

    public PieceTestCase WithPriorMove(
        string from,
        string to,
        IEnumerable<string>? trigger = null,
        IEnumerable<string>? captures = null,
        IEnumerable<IntermediateSquare>? intermediates = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        IEnumerable<PieceSpawn>? spawns = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
    {
        var toPoint = new AlgebraicPoint(to);
        var piece =
            _board.PeekPieceAt(toPoint)
            ?? throw new InvalidOperationException($"No Piece Found at {toPoint}");
        var move = BuildMove(
            from,
            to,
            piece,
            trigger,
            captures,
            intermediates,
            sideEffects,
            spawns,
            specialMoveType,
            forcedPriority,
            promotesTo
        );
        PriorMoves.Add(move);
        return this;
    }

    public PieceTestCase WithMovingPlayer(GameColor playerColor)
    {
        MovingPlayer = playerColor;
        return this;
    }

    public PieceTestCase WithDescription(string testDescription)
    {
        TestDecription = testDescription;
        return this;
    }

    public PieceTestCase ForEach<T>(IEnumerable<T> items, Action<T, PieceTestCase> action)
    {
        foreach (var item in items)
        {
            action(item, this);
        }
        return this;
    }

    public override string ToString() =>
        string.IsNullOrWhiteSpace(TestDecription)
            ? $"Piece under test at {Origin}"
            : TestDecription;

    private Move BuildMove(
        string from,
        string to,
        Piece piece,
        IEnumerable<string>? trigger = null,
        IEnumerable<string>? captures = null,
        IEnumerable<IntermediateSquare>? intermediates = null,
        IEnumerable<MoveSideEffect>? sideEffects = null,
        IEnumerable<PieceSpawn>? spawns = null,
        SpecialMoveType specialMoveType = SpecialMoveType.None,
        ForcedMovePriority forcedPriority = ForcedMovePriority.None,
        PieceType? promotesTo = null
    )
    {
        var moveCaptures = captures?.Select(c =>
        {
            AlgebraicPoint pos = new(c);
            return new MoveCapture(
                _board.PeekPieceAt(pos)
                    ?? throw new InvalidOperationException($"No Piece Found at {pos}"),
                pos
            );
        });

        return new Move(
            new AlgebraicPoint(from),
            new AlgebraicPoint(to),
            piece,
            triggerSquares: trigger?.Select(x => new AlgebraicPoint(x)),
            intermediateSquares: intermediates,
            captures: moveCaptures,
            sideEffects: sideEffects,
            pieceSpawns: spawns,
            specialMoveType: specialMoveType,
            forcedPriority: forcedPriority,
            promotesTo: promotesTo
        );
    }
}
