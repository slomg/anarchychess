using AnarchyChess.Ai.Extensions;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Ai.Tests.Tests.ExtensionTests;

public class BitPieceTypeExtensionsTests
{
    [Fact]
    public void IsWhite_returns_true_for_white_pieces_and_false_for_others()
    {
        foreach (BitPieceType piece in Enum.GetValues<BitPieceType>())
        {
            if (piece > BitPieceType.WHITE_START_MARKER && piece < BitPieceType.WHITE_END_MARKER)
            {
                piece.IsWhite().Should().BeTrue();
            }
            else
            {
                piece.IsWhite().Should().BeFalse();
            }
        }
    }

    [Fact]
    public void IsBlack_returns_true_for_black_pieces_and_false_for_others()
    {
        foreach (BitPieceType piece in Enum.GetValues<BitPieceType>())
        {
            if (piece > BitPieceType.BLACK_START_MARKER && piece < BitPieceType.BLACK_END_MARKER)
            {
                piece.IsBlack().Should().BeTrue();
            }
            else
            {
                piece.IsBlack().Should().BeFalse();
            }
        }
    }

    [Fact]
    public void IsNeutral_returns_true_for_neutral_pieces_and_false_for_others()
    {
        foreach (BitPieceType piece in Enum.GetValues<BitPieceType>())
        {
            if (
                piece > BitPieceType.NEUTRAL_START_MARKER
                && piece < BitPieceType.NEUTRAL_END_MARKER
            )
            {
                piece.IsNeutral().Should().BeTrue();
            }
            else
            {
                piece.IsNeutral().Should().BeFalse();
            }
        }
    }

    [Fact]
    public void Color_returns_white_black_or_null_correctly()
    {
        foreach (BitPieceType piece in Enum.GetValues<BitPieceType>())
        {
            if (piece.IsWhite())
            {
                piece.Color().Should().Be(GameColor.White);
            }
            else if (piece.IsBlack())
            {
                piece.Color().Should().Be(GameColor.Black);
            }
            else
            {
                piece.Color().Should().BeNull();
            }
        }
    }
}
