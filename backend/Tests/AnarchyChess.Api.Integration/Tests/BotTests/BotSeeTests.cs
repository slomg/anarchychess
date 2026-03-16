using AnarchyChess.Ai;
using AnarchyChess.Ai.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.TestInfrastructure;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.Api.TestInfrastructure.Fakes;
using AnarchyChess.EngineShared;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AnarchyChess.Api.Integration.Tests.BotTests;

public class BotSeeTests : BaseIntegrationTest
{
    private readonly IBotSee _botSee;

    public BotSeeTests(AnarchyChessWebApplicationFactory factory)
        : base(factory)
    {
        _botSee = Scope.ServiceProvider.GetRequiredService<IBotSee>();
    }

    [Fact]
    public void SeeCapture_returns_0_for_non_capture()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("b4")] = PieceFactory.White(PieceType.Queen),
                [new("e8")] = PieceFactory.Black(),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("b4"),
            to: new("h4")
        ).Generate();

        _botSee.SeeCapture(move, board).Should().Be(0);
    }

    [Fact]
    public void SeeCapture_returns_piece_value_for_a_free_capture()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("b4")] = PieceFactory.White(PieceType.Queen),
                [new("h4")] = PieceFactory.Black(PieceType.Rook),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("b4"),
            to: new("h4")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("h4").AsIdx())
            .Generate();

        _botSee.SeeCapture(move, board).Should().Be(MaterialValue.GetPieceValue(PieceType.Rook));
    }

    [Fact]
    public void SeeCapture_returns_correct_score_for_a_winning_trade()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("b4")] = PieceFactory.White(PieceType.Rook),
                [new("h4")] = PieceFactory.Black(PieceType.Queen),
                [new("h6")] = PieceFactory.Black(PieceType.Rook),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.White,
            from: new("b4"),
            to: new("h4")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("h4").AsIdx())
            .Generate();

        _botSee
            .SeeCapture(move, board)
            .Should()
            .Be(
                MaterialValue.GetPieceValue(PieceType.Queen)
                    - MaterialValue.GetPieceValue(PieceType.Rook)
            );
    }

    [Fact]
    public void SeeCapture_returns_negative_for_a_self_capture()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e4")] = PieceFactory.White(PieceType.Queen),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("e4"),
            to: new("e4")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e4").AsIdx())
            .RuleFor(x => x.SpecialMoveType, SpecialMoveType.RadioactiveBetaDecay)
            .Generate();

        _botSee.SeeCapture(move, board).Should().Be(-MaterialValue.GetPieceValue(PieceType.Queen));
    }

    [Fact]
    public void SeeCapture_returns_negative_for_a_losing_capture()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Queen),
                [new("e9")] = PieceFactory.Black(PieceType.Pawn),
                [new("f8")] = PieceFactory.Black(PieceType.Rook),
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

        _botSee
            .SeeCapture(move, board)
            .Should()
            .Be(
                MaterialValue.GetPieceValue(PieceType.Rook)
                    - MaterialValue.GetPieceValue(PieceType.Queen)
            );
    }

    [Fact]
    public void SeeCapture_keeps_track_of_white_traitor_rook_ownership()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f2")] = PieceFactory.White(PieceType.Rook),
                [new("e2")] = PieceFactory.Neutral(PieceType.TraitorRook),
                [new("e9")] = PieceFactory.Black(PieceType.Pawn),
                [new("d9")] = PieceFactory.Black(PieceType.Rook),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.TraitorRook,
            BitPieceColor.Neutral,
            from: new("e2"),
            to: new("e9")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e9").AsIdx())
            .Generate();

        _botSee
            .SeeCapture(move, board)
            .Should()
            .Be(MaterialValue.GetPieceValue(PieceType.Pawn) - BotSee.BotTraitorRookValue);
    }

    [Fact]
    public void SeeCapture_keeps_track_of_black_traitor_rook_ownership()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f2")] = PieceFactory.White(PieceType.Rook),
                [new("e2")] = PieceFactory.White(PieceType.Pawn),
                [new("e9")] = PieceFactory.Neutral(PieceType.TraitorRook),
                [new("d9")] = PieceFactory.Black(PieceType.Rook),
            },
            isWhiteToMove: false
        );
        BitMove move = new BitMoveFaker(
            PieceType.TraitorRook,
            BitPieceColor.Neutral,
            from: new("e9"),
            to: new("e2")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e2").AsIdx())
            .Generate();

        _botSee
            .SeeCapture(move, board)
            .Should()
            .Be(MaterialValue.GetPieceValue(PieceType.Pawn) - BotSee.BotTraitorRookValue);
    }

    [Fact]
    public void SeeCapture_counts_traitor_rook_ownership_without_adjacent_pieces_but_on_white_side()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.Neutral(PieceType.TraitorRook),
                [new("e9")] = PieceFactory.Black(PieceType.Rook),
            },
            isWhiteToMove: false
        );
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.Black,
            from: new("e9"),
            to: new("e2")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e2").AsIdx())
            .Generate();

        _botSee.SeeCapture(move, board).Should().Be(BotSee.BotTraitorRookValue);
    }

    [Fact]
    public void SeeCapture_counts_traitor_rook_ownership_without_adjacent_pieces_but_on_black_side()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.White(PieceType.Rook),
                [new("e9")] = PieceFactory.Neutral(PieceType.TraitorRook),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.White,
            from: new("e2"),
            to: new("e9")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e9").AsIdx())
            .Generate();

        _botSee.SeeCapture(move, board).Should().Be(BotSee.BotTraitorRookValue);
    }

    [Fact]
    public void SeeCapture_counts_traitor_rook_loss_even_if_opponent_cant_immedietly_capture()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.Neutral(PieceType.TraitorRook),
                [new("e9")] = PieceFactory.Black(PieceType.Pawn),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.TraitorRook,
            BitPieceColor.Neutral,
            from: new("e2"),
            to: new("e9")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e9").AsIdx())
            .Generate();

        _botSee
            .SeeCapture(move, board)
            .Should()
            .Be(MaterialValue.GetPieceValue(PieceType.Pawn) - BotSee.BotTraitorRookValue);
    }

    [Fact]
    public void SeeCapture_evaluates_traitor_rook_moves_without_capture()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.Neutral(PieceType.TraitorRook),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.TraitorRook,
            BitPieceColor.Neutral,
            from: new("e2"),
            to: new("e9")
        ).Generate();

        _botSee.SeeCapture(move, board).Should().Be(-BotSee.BotTraitorRookValue);
    }

    [Fact]
    public void SeeCapture_values_a_single_king_as_100k()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new("f1")] = PieceFactory.White(PieceType.Queen),
                [new("e1")] = PieceFactory.White(PieceType.King),
                [new("f10")] = PieceFactory.Black(PieceType.King),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("f1"),
            to: new("f10")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f10").AsIdx())
            .Generate();

        _botSee.SeeCapture(move, board).Should().Be(100_000);
    }

    [Fact]
    public void SeeCapture_values_multiple_kings_as_normal()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new("f1")] = PieceFactory.White(PieceType.Queen),
                [new("e1")] = PieceFactory.White(PieceType.King),
                [new("f10")] = PieceFactory.Black(PieceType.King),
                [new("h10")] = PieceFactory.Black(PieceType.King),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Queen,
            BitPieceColor.White,
            from: new("f1"),
            to: new("f10")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("f10").AsIdx())
            .Generate();

        _botSee.SeeCapture(move, board).Should().Be(MaterialValue.GetPieceValue(PieceType.King));
    }

    [Fact]
    public void SeeCapture_always_chooses_the_best_next_capture_for_multiple_capturing_pieces()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e5")] = PieceFactory.Black(PieceType.Pawn),

                [new("a5")] = PieceFactory.White(PieceType.Rook),
                [new("i1")] = PieceFactory.Black(PieceType.Bishop),
                [new("j10")] = PieceFactory.White(PieceType.Bishop),
                [new("e1")] = PieceFactory.Black(PieceType.Queen),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.White,
            from: new("a5"),
            to: new("e5")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e5").AsIdx())
            .Generate();

        _botSee
            .SeeCapture(move, board)
            .Should()
            .Be(
                MaterialValue.GetPieceValue(PieceType.Pawn)
                    - MaterialValue.GetPieceValue(PieceType.Rook)
                    + MaterialValue.GetPieceValue(PieceType.Bishop)
                    - MaterialValue.GetPieceValue(PieceType.Bishop)
            );
    }

    [Fact]
    public void SeeCapture_ignores_multi_step_moves()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new("e6")] = PieceFactory.White(PieceType.Rook),
                [new("e10")] = PieceFactory.Black(PieceType.Bishop),
                [new("f1")] = PieceFactory.Black(PieceType.Bishop),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.White,
            from: new("e6"),
            to: new("e10")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e10").AsIdx())
            .Generate();

        _botSee.SeeCapture(move, board).Should().Be(MaterialValue.GetPieceValue(PieceType.Bishop));
    }

    [Fact]
    public void CheckMultiStep_returns_false_for_pieces_that_cant_multi_step()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>
            {
                [new("e5")] = PieceFactory.White(PieceType.Rook),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Rook,
            BitPieceColor.White,
            from: new("e5"),
            to: new("e10")
        ).Generate();

        _botSee.CheckMultiStep(move, board).Should().BeFalse();
    }

    [Fact]
    public void CheckMultiStep_returns_false_for_regular_bishop_moves()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Bishop),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            BitPieceColor.White,
            from: new("f3"),
            to: new("a8")
        ).Generate();

        _botSee.CheckMultiStep(move, board).Should().BeFalse();
    }

    [Fact]
    public void CheckMultiStep_returns_true_for_bishop_hop()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Bishop),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            BitPieceColor.White,
            from: new("f3"),
            to: new("i4")
        ).Generate();

        _botSee.CheckMultiStep(move, board).Should().BeTrue();
    }

    [Fact]
    public void CheckMultiStep_returns_true_for_a_move_that_would_be_possible_without_hop_but_piece_blocks()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Bishop),
                [new("d5")] = PieceFactory.Black(),
                [new("c6")] = PieceFactory.Black(),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Bishop,
            BitPieceColor.White,
            from: new("f3"),
            to: new("c6")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("c6").AsIdx())
            .Generate();

        _botSee.CheckMultiStep(move, board).Should().BeTrue();
    }

    [Fact]
    public void CheckMultiStep_returns_false_for_regular_checker_move()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Checker),
                [new("e4")] = PieceFactory.Black(),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Checker,
            BitPieceColor.White,
            from: new("f3"),
            to: new("d5")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e4").AsIdx())
            .Generate();

        _botSee.CheckMultiStep(move, board).Should().BeFalse();
    }

    [Fact]
    public void CheckMultiStep_returns_true_for_multiple_checker_captures()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Checker),
                [new("e4")] = PieceFactory.Black(),
                [new("e6")] = PieceFactory.Black(),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Checker,
            BitPieceColor.White,
            from: new("f3"),
            to: new("f7")
        )
            .RuleFor(
                x => x.CapturesMask,
                (UInt128.One << new AlgebraicPoint("e4").AsIdx())
                    | (UInt128.One << new AlgebraicPoint("e6").AsIdx())
            )
            .Generate();

        _botSee.CheckMultiStep(move, board).Should().BeTrue();
    }

    [Fact]
    public void CheckMultiStep_returns_true_for_multiple_checker_non_capture_hops()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Checker),
                [new("e4")] = PieceFactory.White(),
                [new("e6")] = PieceFactory.White(),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Checker,
            BitPieceColor.White,
            from: new("f3"),
            to: new("f7")
        ).Generate();

        _botSee.CheckMultiStep(move, board).Should().BeTrue();
    }

    [Fact]
    public void CheckMultiStep_returns_true_for_a_square_reachable_without_hops_but_not_with_a_capture()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("f3")] = PieceFactory.White(PieceType.Checker),
                [new("g4")] = PieceFactory.White(),
                [new("d5")] = PieceFactory.White(),
                [new("e6")] = PieceFactory.Black(),
            }
        );
        BitMove move = new BitMoveFaker(
            PieceType.Checker,
            BitPieceColor.White,
            from: new("f3"),
            to: new("g5")
        )
            .RuleFor(x => x.CapturesMask, UInt128.One << new AlgebraicPoint("e6").AsIdx())
            .Generate();

        _botSee.CheckMultiStep(move, board).Should().BeTrue();
    }
}
