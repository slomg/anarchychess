using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class MoveOrderingTests
{
    private readonly MoveOrdering _ordering = new();

    private void OrderMoves(BitBoard board, BitMove[,] killers, int[,] history, Span<BitMove> moves)
    {
        Span<int> scores = stackalloc int[moves.Length];
        _ordering.ScoreMoves(board, depth: 0, killers, history, scores, moves, moves.Length);

        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = _ordering.GetNextHighestMove(i, moves, scores, moves.Length);
        }
    }

    [Fact]
    public void OrderMoves_correctly_prioritizes_all_types()
    {
        BitMove killer = new()
        {
            From = 1,
            To = 2,
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        BitMove promotion = new()
        {
            From = 3,
            To = 4,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            PromotesTo = PieceType.Queen,
        };
        BitMove capture = new()
        {
            From = 5,
            To = 6,
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask = 1,
        };
        BitMove special = new()
        {
            From = 7,
            To = 8,
            Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };
        BitMove quiet = new()
        {
            From = 9,
            To = 10,
            Piece = new BitPiece { Type = PieceType.Bishop, Color = BitPieceColor.White },
        };

        BitMove[,] killers = new BitMove[1, 2];
        killers[0, 0] = killer;

        Span<BitMove> moves = [quiet, capture, special, promotion, killer];
        OrderMoves(new BitBoard(), killers, new int[100, 100], moves);

        moves[0].Should().BeEquivalentTo(killer);
        moves[1].Should().BeEquivalentTo(capture);
        moves[2].Should().BeEquivalentTo(promotion);
        moves[3].Should().BeEquivalentTo(special);
        moves[4].Should().BeEquivalentTo(quiet);
    }

    [Fact]
    public void OrderMoves_sorts_captures_by_mvv_lva()
    {
        BitBoard board = BitBoard.FromPieces(
            new()
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
        OrderMoves(board, new BitMove[1, 2], new int[100, 100], moves);

        moves[0].Should().BeEquivalentTo(goodCapture);
        moves[1].Should().BeEquivalentTo(badCapture);
    }

    [Fact]
    public void OrderMoves_prioritizes_quiet_moves_by_history_heuristic()
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
        OrderMoves(new BitBoard(), new BitMove[1, 2], history, moves);

        moves[0].Should().BeEquivalentTo(moveHigh);
        moves[1].Should().BeEquivalentTo(moveLow);
    }

    [Fact]
    public void SelectAndPromoteHighestMove_moves_highest_to_front()
    {
        Span<BitMove> moves =
        [
            new BitMove
            {
                From = 0,
                To = 1,
                Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            },
            new BitMove
            {
                From = 0,
                To = 1,
                Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
                PromotesTo = PieceType.Queen,
            },
            new BitMove
            {
                From = 2,
                To = 3,
                Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
                CapturesMask = 1,
            },
        ];
        BitMove[,] killers = new BitMove[1, 2];
        int[,] history = new int[100, 100];

        BitMove best = _ordering.SelectAndPromoteHighestMove(
            new BitBoard(),
            0,
            killers,
            history,
            moves,
            moves.Length
        );

        best.Should().BeEquivalentTo(moves[2]);
        moves[0].Should().BeEquivalentTo(best);
    }
}
