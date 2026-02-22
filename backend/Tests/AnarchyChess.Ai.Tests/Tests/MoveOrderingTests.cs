using AnarchyChess.Ai.Models;
using AnarchyChess.Api.TestInfrastructure.Factories;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests;

public class MoveOrderingTests
{
    private readonly MoveOrdering _ordering = new();

    [Fact]
    public void OrderMoves_prioritizes_captures_over_quiet_moves()
    {
        BitBoard board = BitBoard.FromPieces(
            new()
            {
                [new("e4")] = PieceFactory.White(PieceType.Pawn),
                [new("d5")] = PieceFactory.Black(PieceType.Queen),
            },
            isWhiteToMove: true
        );

        BitMove capture = new()
        {
            From = new AlgebraicPoint("e4").AsIdx(),
            To = new AlgebraicPoint("d5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            CapturesMask = 1,
        };

        BitMove quiet = new()
        {
            From = new AlgebraicPoint("e4").AsIdx(),
            To = new AlgebraicPoint("e5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };

        Span<BitMove> moves = [quiet, capture];
        _ordering.OrderMoves(board, depth: 0, new BitMove[1, 2], new int[100, 100], moves, 2);

        moves[0].Should().BeEquivalentTo(capture);
        moves[1].Should().BeEquivalentTo(quiet);
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
            CapturesMask = 1,
        };

        BitMove goodCapture = new()
        {
            From = new AlgebraicPoint("e4").AsIdx(),
            To = new AlgebraicPoint("f5").AsIdx(),
            Piece = new BitPiece { Type = PieceType.Queen, Color = BitPieceColor.White },
            CapturesMask = 1 << 1,
        };

        Span<BitMove> moves = [badCapture, goodCapture];
        _ordering.OrderMoves(board, depth: 0, new BitMove[1, 2], new int[100, 100], moves, 2);

        moves[0].Should().BeEquivalentTo(goodCapture);
        moves[1].Should().BeEquivalentTo(badCapture);
    }

    [Fact]
    public void OrderMoves_prioritizes_promotions_over_special_and_killer()
    {
        BitBoard board = BitBoard.FromPieces([]);

        BitMove promotion = new()
        {
            From = 5,
            To = 6,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            PromotesTo = PieceType.Queen,
        };

        BitMove special = new()
        {
            From = 3,
            To = 4,
            Piece = new BitPiece { Type = PieceType.King, Color = BitPieceColor.White },
            SpecialMoveType = SpecialMoveType.KingsideCastle,
        };

        BitMove killer = new()
        {
            From = 1,
            To = 2,
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
        };

        BitMove[,] killers = new BitMove[1, 2];
        killers[0, 0] = killer;

        Span<BitMove> moves = [special, killer, promotion];
        _ordering.OrderMoves(board, depth: 0, killers, new int[100, 100], moves, 3);

        moves[0].Should().BeEquivalentTo(promotion);
        moves[1].Should().BeEquivalentTo(special);
        moves[2].Should().BeEquivalentTo(killer);
    }

    [Fact]
    public void OrderMoves_prioritizes_killer_over_quiet()
    {
        BitBoard board = BitBoard.FromPieces([]);

        BitMove killer = new()
        {
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
            From = 3,
            To = 4,
        };

        BitMove quiet = new()
        {
            Piece = new BitPiece { Type = PieceType.Bishop, Color = BitPieceColor.White },
            From = 5,
            To = 6,
        };

        BitMove[,] killers = new BitMove[1, 2];
        killers[0, 0] = killer;

        Span<BitMove> moves = [quiet, killer];
        _ordering.OrderMoves(board, depth: 0, killers, new int[100, 100], moves, 2);

        moves[0].Should().BeEquivalentTo(killer);
        moves[1].Should().BeEquivalentTo(quiet);
    }

    [Fact]
    public void OrderMoves_prioritizes_quiet_moves_by_history_heuristic_10x10()
    {
        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: true);

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
        _ordering.OrderMoves(board, depth: 0, new BitMove[1, 2], history, moves, 2);

        moves[0].Should().BeEquivalentTo(moveHigh);
        moves[1].Should().BeEquivalentTo(moveLow);
    }

    [Fact]
    public void OrderMoves_history_is_lower_than_promotions_and_killers()
    {
        BitBoard board = BitBoard.FromPieces([], isWhiteToMove: true);

        BitMove quiet = new()
        {
            From = 10,
            To = 11,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
        };
        BitMove killer = new()
        {
            From = 12,
            To = 13,
            Piece = new BitPiece { Type = PieceType.Rook, Color = BitPieceColor.White },
        };
        BitMove promotion = new()
        {
            From = 14,
            To = 15,
            Piece = new BitPiece { Type = PieceType.Pawn, Color = BitPieceColor.White },
            PromotesTo = PieceType.Queen,
        };

        int[,] history = new int[100, 100];
        history[quiet.From, quiet.To] = 1000;

        BitMove[,] killers = new BitMove[1, 2];
        killers[0, 0] = killer;

        Span<BitMove> moves = [quiet, killer, promotion];
        _ordering.OrderMoves(board, depth: 0, killers, history, moves, 3);

        moves[0].Should().BeEquivalentTo(promotion);
        moves[1].Should().BeEquivalentTo(killer);
        moves[2].Should().BeEquivalentTo(quiet);
    }
}
