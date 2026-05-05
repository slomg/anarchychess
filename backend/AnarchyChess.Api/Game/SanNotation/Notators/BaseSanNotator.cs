using System.Text;
using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameLogic.Models;
using AnarchyChess.EngineShared;

namespace AnarchyChess.Api.Game.SanNotation.Notators;

public abstract class BaseSanNotator(IPieceLetterMap pieceLetterMap) : ISanNotator
{
    private readonly IPieceLetterMap _pieceLetterMap = pieceLetterMap;

    public abstract SpecialMoveType HandlesMoveType { get; }

    public abstract void Notate(Move move, IEnumerable<Move> legalMoves, StringBuilder sb);

    protected static List<Move> FindMovesAtSameDestination(
        Move move,
        IEnumerable<Move> legalMoves
    ) =>
        // moves where the same piece type moved to the same destination
        [
            .. legalMoves.Where(x =>
                x.To == move.To
                && x.Piece.Type == move.Piece.Type
                && x.From != move.From
                && x.SpecialMoveType == move.SpecialMoveType
            ),
        ];

    protected static char FileLetter(int x) => (char)('a' + x);

    protected string PieceChar(PieceType piece) =>
        GameLogicConstants.PawnLikePieces.Contains(piece)
            ? ""
            : char.ToUpper(_pieceLetterMap.GetLetter(piece)).ToString();

    protected static (bool isRankAmbiguous, bool isFileAmbiguous) DisambiguatePosition(
        Move move,
        IEnumerable<Move> legalMoves,
        StringBuilder sb
    )
    {
        var movesWithSameDestination = FindMovesAtSameDestination(move, legalMoves);
        return DisambiguatePosition(move.From, ambiguousMoves: movesWithSameDestination, sb);
    }

    protected static (bool isRankAmbiguous, bool isFileAmbiguous) DisambiguatePosition(
        AlgebraicPoint position,
        List<Move> ambiguousMoves,
        StringBuilder sb
    )
    {
        var isRankAmbiguous = ambiguousMoves.Any(x => x.From.Y == position.Y);
        var isFileAmbiguous = ambiguousMoves.Any(x => x.From.X == position.X);

        if (isRankAmbiguous)
        {
            sb.Append(FileLetter(position.X));
        }
        if (isFileAmbiguous)
        {
            sb.Append(position.Y + 1);
        }

        return (isRankAmbiguous, isFileAmbiguous);
    }

    protected static void NotateDestination(Move move, StringBuilder sb)
    {
        var isCapture = move.Captures.Any(x => x.Position == move.To);
        if (isCapture)
            sb.Append('x');
        sb.Append(move.To.AsAlgebraic());
    }

    protected static void NotateIntermediateSquares(Move move, StringBuilder sb)
    {
        foreach (var square in move.IntermediateSquares)
        {
            sb.Append('~');
            sb.Append(square.Position.AsAlgebraic());
        }
    }

    protected static void NotateSideCaptures(Move move, StringBuilder sb)
    {
        // captures that are not the destination
        var sideCaptures = move.Captures.Where(x => x.Position != move.To);
        foreach (var capture in sideCaptures)
        {
            sb.Append('x');
            sb.Append(capture.Position.AsAlgebraic());
        }
    }
}
