using AnarchyChess.Ai;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.GameLogic;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.BotTests;

public class BotHeuristicsTests : BaseIntegrationTest
{
    private readonly IBotHeuristics _botHeuristics;

    private readonly IBitMoveGenerator _bitMoveGenerator;
    private readonly IBotService _botService;

    public BotHeuristicsTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _botHeuristics = Scope.ServiceProvider.GetRequiredService<IBotHeuristics>();

        _bitMoveGenerator = Scope.ServiceProvider.GetRequiredService<IBitMoveGenerator>();
        _botService = Scope.ServiceProvider.GetRequiredService<IBotService>();
    }

    private BotHeuristicContext CreateContext(BitMove move, ChessBoard board)
    {
        BitBoard bitboard = _botService.ConvertBoardToBit(board);

        BitBoard bitboardAfterMove = new(bitboard);
        bitboardAfterMove.MakeMove(move);

        BitMove[] opponentMoves = new BitMove[EngineConstants.MaxMoves];
        int opponentMoveCount = 0;
        _bitMoveGenerator.Generate(bitboardAfterMove, opponentMoves, ref opponentMoveCount);

        return new(
            Board: board,
            Bitboard: bitboard,
            BitboardAfterMove: bitboardAfterMove,
            OpponentMoves: opponentMoves,
            OpponentMoveCount: opponentMoveCount
        );
    }

    [Fact]
    public void IsNonCentralPawn_returns_false_for_non_pawn()
    {
        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            from: new("a2"),
            to: new("b3")
        ).Generate();

        _botHeuristics.IsNonCentralPawn(move).Should().BeFalse();
    }

    [Theory]
    [InlineData("a2", "a4", true)]
    [InlineData("c2", "c5", true)]
    [InlineData("d8", "d6", false)]
    [InlineData("e3", "e4", false)]
    [InlineData("f2", "f4", false)]
    [InlineData("g7", "g6", false)]
    [InlineData("h2", "h5", true)]
    [InlineData("j8", "j6", true)]
    public void IsNonCentralPawn_detects_non_central_pawn_push(
        string from,
        string to,
        bool isNonCentral
    )
    {
        BitMove move = new BitMoveFaker(PieceType.Pawn, from: new(from), to: new(to)).Generate();

        _botHeuristics.IsNonCentralPawn(move).Should().Be(isNonCentral);
    }

    [Theory]
    [InlineData("b2", "a3", true)]
    [InlineData("i2", "j3", true)]
    [InlineData("a2", "b3", false)]
    [InlineData("j2", "i3", false)]
    [InlineData("e2", "e3", false)]
    public void IsEdge_detects_edge_moves(string from, string to, bool expected)
    {
        BitMove move = new BitMoveFaker(from: new(from), to: new(to)).Generate();
        _botHeuristics.IsEdge(move).Should().Be(expected);
    }

    [Fact]
    public void IsRecapture_returns_false_when_no_previous_moves()
    {
        ChessBoard board = new();
        BitMove move = new BitMoveFaker().RuleFor(x => x.CapturesMask, UInt128.One << 5).Generate();
        var context = CreateContext(move, board);

        _botHeuristics.IsRecapture(move, context).Should().BeFalse();
    }

    [Fact]
    public void IsRecapture_returns_false_when_last_move_was_not_a_capture()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("c2")] = PieceFactory.White(PieceType.Queen),
                [new("i2")] = PieceFactory.Black(PieceType.Bishop),
            }
        );
        board.PlayMove(
            new MoveFaker(GameColor.White, PieceType.Queen)
                .RuleFor(x => x.From, new AlgebraicPoint("c2"))
                .RuleFor(x => x.To, new AlgebraicPoint("f5"))
        );

        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            BitPieceColor.Black,
            from: new("i2"),
            to: new("f5")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f5").AsIdx())
            .Generate();

        _botHeuristics.IsRecapture(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void IsRecapture_returns_false_when_not_capturing_last_move()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("c2")] = PieceFactory.White(PieceType.Queen),
                [new("f5")] = PieceFactory.Black(),
                [new("g4")] = PieceFactory.Black(),
                [new("i2")] = PieceFactory.Black(PieceType.Bishop),
            }
        );
        board.PlayMove(
            new MoveFaker(GameColor.White, PieceType.Queen)
                .RuleFor(x => x.From, new AlgebraicPoint("c2"))
                .RuleFor(x => x.To, new AlgebraicPoint("f5"))
                .RuleFor(x => x.Captures, [new MoveCapture(new("f5"), board)])
        );

        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            BitPieceColor.Black,
            from: new("i2"),
            to: new("g4")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("g4").AsIdx())
            .Generate();

        _botHeuristics.IsRecapture(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void IsRecapture_returns_true_when_recapturing_the_last_move_capture()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("c2")] = PieceFactory.White(PieceType.Queen),
                [new("f5")] = PieceFactory.Black(),
                [new("i2")] = PieceFactory.Black(PieceType.Bishop),
            }
        );
        board.PlayMove(
            new MoveFaker(GameColor.White, PieceType.Queen)
                .RuleFor(x => x.From, new AlgebraicPoint("c2"))
                .RuleFor(x => x.To, new AlgebraicPoint("f5"))
                .RuleFor(x => x.Captures, [new MoveCapture(new("f5"), board)])
        );

        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            BitPieceColor.Black,
            from: new("i2"),
            to: new("f5")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f5").AsIdx())
            .Generate();

        _botHeuristics.IsRecapture(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void IsBackwards_returns_false_for_neutral_piece()
    {
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.Neutral,
            from: new("e4"),
            to: new("e3")
        ).Generate();

        _botHeuristics.IsBackwards(move).Should().BeFalse();
    }

    [Theory]
    [InlineData(BitPieceColor.White, "e4", "e3", true)]
    [InlineData(BitPieceColor.White, "e4", "e5", false)]
    [InlineData(BitPieceColor.Black, "e4", "e5", true)]
    [InlineData(BitPieceColor.Black, "e4", "e3", false)]
    [InlineData(BitPieceColor.White, "e4", "f4", false)]
    [InlineData(BitPieceColor.Black, "e4", "f4", false)]
    public void IsBackwards_detects_direction_correctly(
        BitPieceColor color,
        string from,
        string to,
        bool expected
    )
    {
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            color,
            from: new(from),
            to: new(to)
        ).Generate();

        _botHeuristics.IsBackwards(move).Should().Be(expected);
    }
}
