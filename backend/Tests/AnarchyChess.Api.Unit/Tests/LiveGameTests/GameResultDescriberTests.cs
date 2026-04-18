using AnarchyChess.Api.Game.Services;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.EngineShared;
using AwesomeAssertions;

namespace AnarchyChess.Api.Unit.Tests.LiveGameTests;

public class GameResultDescriberTests
{
    private readonly GameResultDescriber _describer = new();

    [Theory]
    [InlineData(GameColor.White, GameResult.BlackWin, "White Captured Their Own King")]
    [InlineData(GameColor.Black, GameResult.WhiteWin, "Black Captured Their Own King")]
    public void KingSelfCapture_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.KingSelfCapture(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.WhiteWin, "White Captured the King")]
    [InlineData(GameColor.Black, GameResult.BlackWin, "Black Captured the King")]
    public void KingCaptured_returns_the_correct_status(
        GameColor winner,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.KingCaptured(winner);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.Aborted, "Game Aborted by White")]
    [InlineData(GameColor.Black, GameResult.Aborted, "Game Aborted by Black")]
    public void Aborted_returns_the_correct_status(
        GameColor abortedBy,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.Aborted(abortedBy);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.BlackWin, "Black Won by Resignation")]
    [InlineData(GameColor.Black, GameResult.WhiteWin, "White Won by Resignation")]
    public void Resignation_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.Resignation(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.BlackWin, "White Abandoned the Game")]
    [InlineData(GameColor.Black, GameResult.WhiteWin, "Black Abandoned the Game")]
    public void Abanoned_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.Abandoned(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(GameColor.White, GameResult.BlackWin, "White's King Got Bored and Left")]
    [InlineData(GameColor.Black, GameResult.WhiteWin, "Black's King Got Bored and Left")]
    public void Overtime_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.Overtime(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(
        GameColor.White,
        GameResult.BlackWin,
        "Bot tried to play an illegal move. This should NEVER happen. Please report this on the discord"
    )]
    [InlineData(
        GameColor.Black,
        GameResult.WhiteWin,
        "Bot tried to play an illegal move. This should NEVER happen. Please report this on the discord"
    )]
    public void BotIllegalMove_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.BotIllegalMove(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(
        GameColor.White,
        GameResult.BlackWin,
        "You were playing so bad the bot got bored and went offline"
    )]
    [InlineData(
        GameColor.Black,
        GameResult.WhiteWin,
        "You were playing so bad the bot got bored and went offline"
    )]
    public void BotOffline_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.BotOffline(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Theory]
    [InlineData(
        GameColor.White,
        GameResult.BlackWin,
        "The bot failed to make a move. This should NEVER happen. Please report this on the discord"
    )]
    [InlineData(
        GameColor.Black,
        GameResult.WhiteWin,
        "The bot failed to make a move. This should NEVER happen. Please report this on the discord"
    )]
    public void BotFailure_returns_the_correct_status(
        GameColor loser,
        GameResult expectedResult,
        string expectedDescription
    )
    {
        var result = _describer.BotFailure(loser);
        result.Should().Be(new GameEndStatus(expectedResult, expectedDescription));
    }

    [Fact]
    public void DrawByAgreement_returns_the_correct_status()
    {
        var result = _describer.DrawByAgreement();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by Agreement"));
    }

    [Fact]
    public void ThreeFold_returns_the_correct_status()
    {
        var result = _describer.ThreeFold();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by 3-Fold Repetition"));
    }

    [Fact]
    public void FiftyMoves_returns_the_correct_status()
    {
        var result = _describer.FiftyMoves();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by 50 Moves Rule"));
    }

    [Fact]
    public void KingTouch_returns_the_correct_status()
    {
        var result = _describer.KingTouch();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by King Touch"));
    }

    [Fact]
    public void MutualKingCapture_returns_the_correct_status()
    {
        var result = _describer.MutualKingCapture();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by Mutual King Capture"));
    }

    [Fact]
    public void Stalemate_returns_the_correct_status()
    {
        var result = _describer.Stalemate();
        result.Should().Be(new GameEndStatus(GameResult.Draw, "Draw by Stalemate"));
    }
}
