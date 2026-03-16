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
    public void IsSameAsPieceAsLast_returns_false_without_prior_moves()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e5")] = PieceFactory.White(PieceType.Queen),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("e5"),
            to: new("e6")
        ).Generate();

        _botHeuristics.IsSameAsPieceAsLast(move, CreateContext(move, board));
    }

    [Fact]
    public void IsSamePieceAsLast_returns_true_when_the_same_piece_is_moved()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e5")] = PieceFactory.White(PieceType.Queen),
                [new("a7")] = PieceFactory.White(PieceType.Queen),
            },
            moves: [new MoveFaker(GameColor.White, PieceType.Queen, from: new("e3"), to: new("e5"))]
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("e5"),
            to: new("e6")
        ).Generate();

        _botHeuristics.IsSameAsPieceAsLast(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void IsSamePieceAsLast_returns_false_when_a_different_piece_is_moved()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e5")] = PieceFactory.White(PieceType.Queen),
                [new("a7")] = PieceFactory.White(PieceType.Queen),
            },
            moves: [new MoveFaker(GameColor.White, PieceType.Queen, from: new("e3"), to: new("e5"))]
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("a7"),
            to: new("a6")
        ).Generate();

        _botHeuristics.IsSameAsPieceAsLast(move, CreateContext(move, board)).Should().BeFalse();
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
                [new("f5")] = PieceFactory.White(PieceType.Queen),
                [new("i2")] = PieceFactory.Black(PieceType.Bishop),
            },
            moves:
            [
                new MoveFaker(
                    GameColor.White,
                    PieceType.Queen,
                    from: new AlgebraicPoint("c2"),
                    to: new AlgebraicPoint("f5")
                ).Generate(),
            ]
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
            new MoveFaker(
                GameColor.White,
                PieceType.Queen,
                from: new AlgebraicPoint("c2"),
                to: new AlgebraicPoint("f5")
            ).RuleFor(x => x.Captures, [new MoveCapture(new("f5"), board)])
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
            new MoveFaker(
                GameColor.White,
                PieceType.Queen,
                from: new AlgebraicPoint("c2"),
                to: new AlgebraicPoint("f5")
            ).RuleFor(x => x.Captures, [new MoveCapture(new("f5"), board)])
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

    [Fact]
    public void LosesKingCastlingRight_returns_false_for_non_king()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f1")] = PieceFactory.White(PieceType.Queen, hasMoved: false),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("f1"),
            to: new("f2")
        ).Generate();

        _botHeuristics.LosesKingCastlingRight(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void LosesKingCastlingRight_returns_false_if_king_already_moved()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: true),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.King,
            BitPieceColor.White,
            from: new("f1"),
            to: new("f2")
        ).Generate();

        _botHeuristics.LosesKingCastlingRight(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void LosesKingCastlingRight_returns_true_for_unmoved_king()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f1")] = PieceFactory.White(PieceType.King, hasMoved: false),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.King,
            BitPieceColor.White,
            from: new("f1"),
            to: new("f2")
        ).Generate();

        _botHeuristics.LosesKingCastlingRight(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void LosesRookCastlingRight_returns_false_for_non_rook()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("a1")] = PieceFactory.White(PieceType.Queen, hasMoved: false),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("a1"),
            to: new("a2")
        ).Generate();

        _botHeuristics.LosesRookCastlingRight(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void LosesRookCastlingRight_returns_false_if_rook_already_moved()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("j1")] = PieceFactory.White(PieceType.Rook, hasMoved: true),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.White,
            from: new("j1"),
            to: new("j2")
        ).Generate();

        _botHeuristics.LosesRookCastlingRight(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void LosesRookCastlingRight_returns_true_for_unmoved_rook()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("a1")] = PieceFactory.White(PieceType.Rook, hasMoved: false),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.White,
            from: new("a1"),
            to: new("a2")
        ).Generate();

        _botHeuristics.LosesRookCastlingRight(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void CausesForcedMoves_returns_true_when_the_move_causes_a_forced_move_directly()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("b2")] = PieceFactory.White(PieceType.Bishop),
                [new("h9")] = PieceFactory.Black(PieceType.UnderagePawn),
            },
            sideToMove: GameColor.Black
        );

        BitMove move = new BitMoveFaker(
            PieceType.UnderagePawn,
            BitPieceColor.Black,
            from: new("h9"),
            to: new("h8")
        ).Generate();

        _botHeuristics.CausesForcedMove(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void CausesForcedMoves_returns_false_when_the_move_doesnt_cause_a_forced_move_directly_and_is_not_a_capture()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("b2")] = PieceFactory.White(PieceType.Bishop),
                [new("h9")] = PieceFactory.Black(PieceType.UnderagePawn),
                [new("j9")] = PieceFactory.Black(PieceType.UnderagePawn),
            },
            sideToMove: GameColor.Black
        );

        BitMove move = new BitMoveFaker(
            PieceType.UnderagePawn,
            BitPieceColor.Black,
            from: new("h9"),
            to: new("h7")
        ).Generate();

        _botHeuristics.CausesForcedMove(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void CausesForcedMoves_returns_false_when_a_recapture_doesnt_cause_a_forced_move()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("g5")] = PieceFactory.White(PieceType.Queen),
                [new("e9")] = PieceFactory.Black(PieceType.UnderagePawn),
                [new("g8")] = PieceFactory.Black(PieceType.Queen),
                [new("c4")] = PieceFactory.Black(PieceType.Bishop),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("g5"),
            to: new("g8")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("g8").AsIdx())
            .Generate();

        _botHeuristics.CausesForcedMove(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void CausesForcedMoves_returns_true_when_a_recapture_causes_a_forced_move()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("g5")] = PieceFactory.White(PieceType.Queen),
                [new("f9")] = PieceFactory.Black(PieceType.UnderagePawn),
                [new("g8")] = PieceFactory.Black(PieceType.Queen),
                [new("c4")] = PieceFactory.Black(PieceType.Bishop),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("g5"),
            to: new("g8")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("g8").AsIdx())
            .Generate();

        _botHeuristics.CausesForcedMove(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void IsCapturingOpponentHang_returns_false_when_capturing_too_low_value_hanging()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Queen),
                [new("f9")] = PieceFactory.Black(PieceType.SterilePawn),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("f3"),
            to: new("f9")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f9").AsIdx())
            .Generate();

        _botHeuristics.IsCapturingOpponentHang(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void IsCapturingOpponentHang_returns_true_when_capturing_hanging_piece()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Queen),
                [new("f9")] = PieceFactory.Black(PieceType.Bishop),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("f3"),
            to: new("f9")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f9").AsIdx())
            .Generate();

        _botHeuristics.IsCapturingOpponentHang(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void IsHang_returns_true_for_bad_capture()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Queen),
                [new("f8")] = PieceFactory.Black(PieceType.Bishop),
                [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("f3"),
            to: new("f8")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f8").AsIdx())
            .Generate();

        _botHeuristics.IsHang(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void IsHang_returns_false_for_trades()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Queen),
                [new("f8")] = PieceFactory.Black(PieceType.Queen),
                [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("f3"),
            to: new("f8")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f8").AsIdx())
            .Generate();

        _botHeuristics.IsHang(move, CreateContext(move, board)).Should().BeFalse();
    }

    [Fact]
    public void IsHang_returns_true_for_when_we_capture_a_hanging_piece_but_the_opponent_can_capture_a_higher_value_piece()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("b4")] = PieceFactory.White(PieceType.Bishop),
                [new("g9")] = PieceFactory.Black(PieceType.Pawn),
                [new("i5")] = PieceFactory.Black(PieceType.Queen),
                [new("i3")] = PieceFactory.White(PieceType.Queen),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("b4"),
            to: new("g9")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("g9").AsIdx())
            .Generate();

        _botHeuristics.IsHang(move, CreateContext(move, board)).Should().BeTrue();
    }

    [Fact]
    public void IsHang_returns_false_when_we_capture_a_hanging_piece_and_the_opponent_only_has_a_lower_capture()
    {
        ChessBoard board = new(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("b4")] = PieceFactory.White(PieceType.Bishop),
                [new("g9")] = PieceFactory.Black(PieceType.Rook),
                [new("i5")] = PieceFactory.Black(PieceType.Queen),
                [new("i3")] = PieceFactory.White(PieceType.Bishop),
            }
        );

        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("b4"),
            to: new("g9")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("g9").AsIdx())
            .Generate();

        _botHeuristics.IsHang(move, CreateContext(move, board)).Should().BeFalse();
    }
}
