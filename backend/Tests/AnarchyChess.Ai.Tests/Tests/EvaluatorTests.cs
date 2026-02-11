using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class EvaluatorTests
{
    private readonly Evaluator _evaluator = new();

    private readonly int PieceCount = Enum.GetValues<PieceType>().Length;

    [Fact]
    public void EvaluateBoard_returns_positive_score_when_only_our_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Checker),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(8.5f);
    }

    [Fact]
    public void EvaluateBoard_returns_negative_score_when_only_enemy_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.Black(PieceType.Rook),
            [new("b1")] = PieceFactory.Black(PieceType.Checker),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(-8.5f);
    }

    [Fact]
    public void EvaluateBoard_evaluates_traitor_rook_as_0_without_adjacent_pieces()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("h2")] = PieceFactory.White(PieceType.Rook),
            [new("b8")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(0f);
    }

    [Fact]
    public void EvaluateBoard_evaluates_traitor_rook_as_2_when_under_our_control()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("d5")] = PieceFactory.White(PieceType.Pawn),
            [new("e4")] = PieceFactory.White(PieceType.Pawn),
            [new("f5")] = PieceFactory.Black(PieceType.Pawn),
            [new("a1")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(2f);
    }

    [Fact]
    public void EvaluateBoard_evaluates_traitor_rook_as_1_when_adjacent_is_equal()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("d5")] = PieceFactory.White(PieceType.Pawn),
            [new("f5")] = PieceFactory.Black(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(1f);
    }

    [Fact]
    public void EvaluateBoard_evaluates_traitor_rook_as_negative_2_when_under_enemy_control()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e5")] = PieceFactory.Neutral(PieceType.TraitorRook),
            [new("d5")] = PieceFactory.Black(PieceType.Pawn),
            [new("e4")] = PieceFactory.Black(PieceType.Pawn),
            [new("f5")] = PieceFactory.White(PieceType.Pawn),
            [new("a1")] = PieceFactory.White(PieceType.Pawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(-2f);
    }

    [Fact]
    public void EvaluateBoard_sums_own_and_enemy_pieces_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Checker),
            [new("c1")] = PieceFactory.Black(PieceType.Queen),
            [new("d1")] = PieceFactory.Black(PieceType.UnderagePawn),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(-2f);
    }

    [Fact]
    public void EvaluateBoard_handles_a_single_king_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(10_003.5f);
    }

    [Fact]
    public void EvaluateBoard_handles_multiple_kings_correctly()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("e1")] = PieceFactory.White(PieceType.King),
            [new("f1")] = PieceFactory.White(PieceType.King),
            [new("e10")] = PieceFactory.Black(PieceType.King),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        // King score for white: 10_000 / 2 + 3.5 = 5003.5 per king, total = 5003.5 * 2 = 10007
        // King score for black: 10_000 / 1 + 3.5 = 10003.5
        // total score = 10007 - 10003.5 = 3.5
        score.Should().Be(3.5f);
    }

    [Fact]
    public void EvaluateBoard_handles_black_to_move()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Rook),
            [new("b1")] = PieceFactory.White(PieceType.Rook),
            [new("c1")] = PieceFactory.Black(PieceType.Rook),
        };
        BitBoard board = BitBoard.FromPieces(pieces, isWhiteToMove: false);

        float score = _evaluator.EvaluateBoard(board, new int[PieceCount]);

        score.Should().Be(-5f);
    }

    [Fact]
    public void EvaluateBoard_adds_activity_bonus_for_multiple_piece_types()
    {
        Dictionary<AlgebraicPoint, Piece> pieces = new()
        {
            [new("a1")] = PieceFactory.White(PieceType.Bishop),
            [new("b1")] = PieceFactory.White(PieceType.Horsey),
        };
        BitBoard board = BitBoard.FromPieces(pieces);

        int[] activity = new int[PieceCount];
        activity[(int)PieceType.Bishop] = 5; // 5 * 0.06 = 0.3
        activity[(int)PieceType.Horsey] = 4; // 4 * 0.07 = 0.28

        // material: 3 + 3 = 6
        // activity: 0.58
        float score = _evaluator.EvaluateBoard(board, activity);

        score.Should().Be(6.58f);
    }
}
