using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class MoveOrderingTests
{
    [Fact]
    public void SortMoves_correctly_prioritizes_all_types()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.White(PieceType.Pawn),
                [new("f3")] = PieceFactory.Black(),
                [new("f5")] = PieceFactory.Black(),
            }
        );

        BitMove ttMove = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("e5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };
        BitMove killer = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("e3").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        BitMove promotion = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("e4").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            PromotesTo = PieceType.Queen,
        };
        BitMove capture = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("f3").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask = 1,
        };
        BitMove stunThrow = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("f5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.Throw,
            CapturesMask = UInt128.One << new AlgebraicPoint("f5").AsIdx(),
        };
        BitMove regularThrow = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("f6").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.Throw,
        };
        BitMove quiet = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("e6").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Bishop, Color = BitPieceColor.White },
        };

        BitMove[,] killers = new BitMove[1, 2];
        killers[0, 0] = killer;

        Span<BitMove> moves = [quiet, stunThrow, promotion, capture, regularThrow, killer, ttMove];
        MoveOrdering.SortMoves(
            board,
            depth: 0,
            killers,
            new int[100, 100],
            packedTtMove: ttMove.Pack(),
            moves,
            moves.Length
        );

        moves[0].Should().BeEquivalentTo(ttMove);
        moves[1].Should().BeEquivalentTo(killer);
        moves[2].Should().BeEquivalentTo(capture);
        moves[3].Should().BeEquivalentTo(promotion);
        moves[4].Should().BeEquivalentTo(stunThrow);
        moves[5].Should().BeEquivalentTo(regularThrow);
        moves[6].Should().BeEquivalentTo(quiet);
    }

    [Fact]
    public void SortMoves_sorts_captures_by_mvv_lva()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e4")] = PieceFactory.White(PieceType.Queen),
                [new("d5")] = PieceFactory.Black(PieceType.Pawn),
                [new("f5")] = PieceFactory.Black(PieceType.Rook),
            },
            isWhiteToMove: true
        );

        BitMove badCapture = new()
        {
            From = new AlgebraicPoint("e4").AsIdx(),
            To = new AlgebraicPoint("d5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask = UInt128.One << new AlgebraicPoint("d5").AsIdx(),
        };

        BitMove goodCapture = new()
        {
            From = new AlgebraicPoint("e4").AsIdx(),
            To = new AlgebraicPoint("f5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask = UInt128.One << new AlgebraicPoint("f5").AsIdx(),
        };

        Span<BitMove> moves = [badCapture, goodCapture];
        MoveOrdering.SortMoves(
            board,
            depth: 0,
            new BitMove[1, 2],
            new int[100, 100],
            packedTtMove: 0,
            moves,
            moves.Length
        );

        moves[0].Should().BeEquivalentTo(goodCapture);
        moves[1].Should().BeEquivalentTo(badCapture);
    }

    [Fact]
    public void SortMoves_prioritizes_quiet_moves_by_history_heuristic()
    {
        BitMove moveLow = new()
        {
            From = 0,
            To = 1,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };
        BitMove moveHigh = new()
        {
            From = 2,
            To = 3,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };

        int[,] history = new int[100, 100];
        history[moveLow.From, moveLow.To] = 10;
        history[moveHigh.From, moveHigh.To] = 50;

        Span<BitMove> moves = [moveLow, moveHigh];
        MoveOrdering.SortMoves(
            new BitBoard(),
            depth: 0,
            new BitMove[1, 2],
            history,
            packedTtMove: 0,
            moves,
            moves.Length
        );

        moves[0].Should().BeEquivalentTo(moveHigh);
        moves[1].Should().BeEquivalentTo(moveLow);
    }

    [Fact]
    public void SortMoves_orders_stun_throw_by_stunned_piece_value()
    {
        BitBoard board = BitBoard.FromPieces(
            new Dictionary<AlgebraicPoint, Piece>()
            {
                [new("e2")] = PieceFactory.White(PieceType.Pawn),
                [new("f5")] = PieceFactory.Black(PieceType.Pawn),
                [new("g5")] = PieceFactory.Black(PieceType.Queen),
            }
        );

        BitMove lowStun = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("f5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.Throw,
            CapturesMask = UInt128.One << new AlgebraicPoint("f5").AsIdx(),
        };

        BitMove highStun = new()
        {
            From = new AlgebraicPoint("e2").AsIdx(),
            To = new AlgebraicPoint("g5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.Throw,
            CapturesMask = UInt128.One << new AlgebraicPoint("g5").AsIdx(),
        };

        Span<BitMove> moves = [lowStun, highStun];

        MoveOrdering.SortMoves(
            board,
            depth: 0,
            new BitMove[1, 2],
            new int[100, 100],
            packedTtMove: 0,
            moves,
            moves.Length
        );

        moves[0].Should().BeEquivalentTo(highStun);
        moves[1].Should().BeEquivalentTo(lowStun);
    }

    [Fact]
    public void ScoreMoves_fills_scores_array_correctly()
    {
        BitMove move1 = new()
        {
            From = 0,
            To = 1,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };
        BitMove move2 = new()
        {
            From = 2,
            To = 3,
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        BitMove move3 = new()
        {
            From = 4,
            To = 5,
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
        };

        Span<BitMove> moves = [move1, move2, move3];
        Span<int> scores = stackalloc int[moves.Length];

        int[,] history = new int[100, 100];
        history[move1.From, move1.To] = 5;
        history[move2.From, move2.To] = 10;
        history[move3.From, move3.To] = 20;

        MoveOrdering.ScoreMoves(
            new BitBoard(),
            depth: 0,
            killerMoves: new BitMove[1, 2],
            historyHeuristic: history,
            scores: scores,
            moves: moves,
            packedTtMove: -1,
            moveCount: moves.Length
        );

        scores[0].Should().Be(5);
        scores[1].Should().Be(10);
        scores[2].Should().Be(20);
    }

    [Fact]
    public void GetNextHighestMove_selects_and_swaps_highest_move()
    {
        BitMove move1 = new()
        {
            From = 0,
            To = 1,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };
        BitMove move2 = new()
        {
            From = 2,
            To = 3,
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        BitMove move3 = new()
        {
            From = 4,
            To = 5,
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
        };

        Span<BitMove> moves = [move1, move2, move3];
        Span<int> scores = [10, 30, 20];

        BitMove best0 = MoveOrdering.GetNextHighestMove(0, moves, scores, moves.Length);

        best0.Should().Be(move2);
        moves[0].Should().Be(move2);
        moves[1].Should().Be(move1);
        moves[2].Should().Be(move3);
        scores[0].Should().Be(30);
        scores[1].Should().Be(10);
        scores[2].Should().Be(20);
    }

    [Fact]
    public void GetNextHighestMove_selects_next_highest_move_at_later_index()
    {
        BitMove move1 = new()
        {
            From = 0,
            To = 1,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };
        BitMove move2 = new()
        {
            From = 2,
            To = 3,
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        BitMove move3 = new()
        {
            From = 4,
            To = 5,
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
        };

        Span<BitMove> moves = [move1, move2, move3];
        Span<int> scores = [30, 10, 20];

        BitMove best1 = MoveOrdering.GetNextHighestMove(1, moves, scores, moves.Length);

        best1.Should().Be(move3);
        moves[0].Should().Be(move1);
        moves[1].Should().Be(move3);
        moves[2].Should().Be(move2);
        scores[0].Should().Be(30);
        scores[1].Should().Be(20);
        scores[2].Should().Be(10);
    }
}
