using AnarchyChess.Api.Game.Services;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class PieceLetterMapTests : BaseUnitTest
{
    private readonly PieceLetterMap _pieceLetterMap = new();

    public static TheoryData<PieceType, char> PieceToLetterTestData() =>
        new()
        {
            { PieceType.King, 'k' },
            { PieceType.Queen, 'q' },
            { PieceType.Pawn, 'p' },
            { PieceType.UnderagePawn, 'd' },
            { PieceType.SterilePawn, 's' },
            { PieceType.Rook, 'r' },
            { PieceType.Bishop, 'b' },
            { PieceType.Horsey, 'h' },
            { PieceType.Knook, 'n' },
            { PieceType.Antiqueen, 'a' },
            { PieceType.Checker, 'c' },
            { PieceType.TraitorRook, '+' },
        };

    public static TheoryData<char, PieceType> LetterToPieceTestData() =>
        new()
        {
            { 'k', PieceType.King },
            { 'q', PieceType.Queen },
            { 'p', PieceType.Pawn },
            { 'd', PieceType.UnderagePawn },
            { 's', PieceType.SterilePawn },
            { 'r', PieceType.Rook },
            { 'b', PieceType.Bishop },
            { 'h', PieceType.Horsey },
            { 'n', PieceType.Knook },
            { 'a', PieceType.Antiqueen },
            { 'c', PieceType.Checker },
            { '+', PieceType.TraitorRook },
        };

    [Theory]
    [MemberData(nameof(PieceToLetterTestData))]
    public void GetLetter_returns_the_correct_letter_for_PieceType(
        PieceType piece,
        char expectedLetter
    )
    {
        var result = _pieceLetterMap.GetLetter(piece);

        result.Should().Be(expectedLetter);
    }

    [Fact]
    public void PieceToLetterTestData_includes_all_piece_types()
    {
        var testedPieces = PieceToLetterTestData().Select(x => x.Data.Item1).ToHashSet();
        var allPieces = Enum.GetValues<PieceType>().ToHashSet();

        allPieces
            .Should()
            .BeSubsetOf(
                testedPieces,
                because: $"All PieceType enum values must be covered by the test data. Missing: {string.Join(", ", allPieces.Except(testedPieces))}"
            );
    }

    [Fact]
    public void GetLetter_returns_a_question_mark_for_unknown_pieces()
    {
        // some invalid PieceType
        var unknownPiece = (PieceType)99;

        var result = _pieceLetterMap.GetLetter(unknownPiece);

        result.Should().Be('?');
    }

    [Theory]
    [MemberData(nameof(LetterToPieceTestData))]
    public void GetPiece_returns_the_correct_piece_for_letter(char letter, PieceType expectedPiece)
    {
        var result = _pieceLetterMap.GetPiece(letter);

        result.Should().Be(expectedPiece);
    }

    [Fact]
    public void GetPiece_returns_null_for_unknown_letters()
    {
        var result = _pieceLetterMap.GetPiece('1');
        result.Should().BeNull();
    }

    [Fact]
    public void LetterToPieceTestData_includes_all_piece_letters()
    {
        var testedLetters = LetterToPieceTestData().Select(x => x.Data.Item1).ToHashSet();
        var allLetters = Enum.GetValues<PieceType>().Select(_pieceLetterMap.GetLetter).ToHashSet();

        allLetters
            .Should()
            .BeSubsetOf(
                testedLetters,
                because: $"All letters corresponding to PieceType enum values must be covered by the test data. Missing: {string.Join(", ", allLetters.Except(testedLetters))}"
            );
    }
}
