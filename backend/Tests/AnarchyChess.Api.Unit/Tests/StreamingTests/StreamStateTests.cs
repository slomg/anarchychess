using AnarchyChess.Api.Streaming;
using AwesomeAssertions;
using Orleans.Providers.Streams.Common;

namespace AnarchyChess.Api.Unit.Tests.StreamingTests;

public class StreamStateTests
{
    [Fact]
    public void TryUpdateSequenceToken_updates_the_token_when_the_new_token_is_newer()
    {
        EventSequenceToken oldToken = new(0, 1);
        EventSequenceToken newToken = new(0, 2);
        StreamState state = new();
        state.TryUpdateSequenceToken(oldToken);

        var result = state.TryUpdateSequenceToken(newToken);

        result.Should().BeTrue();
        state.SequenceToken.Should().Be(newToken);
    }

    [Fact]
    public void TryUpdateSequenceToken_does_not_update_the_token_when_the_new_token_is_older()
    {
        EventSequenceToken newToken = new(0, 2);
        StreamState state = new();
        state.TryUpdateSequenceToken(newToken);

        EventSequenceToken oldToken = new(0, 1);

        var result = state.TryUpdateSequenceToken(oldToken);

        result.Should().BeFalse();
        state.SequenceToken.Should().Be(newToken);
    }

    [Fact]
    public void TryUpdateSequenceToken_does_not_update_the_token_when_the_new_token_is_the_same()
    {
        EventSequenceToken token = new(0, 1);
        StreamState state = new();
        state.TryUpdateSequenceToken(token);

        var result = state.TryUpdateSequenceToken(token);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryUpdateSequenceToken_returns_false_and_does_not_update_when_the_new_token_is_null()
    {
        StreamState state = new();
        EventSequenceToken token = new();
        state.TryUpdateSequenceToken(token);

        var result = state.TryUpdateSequenceToken(null);

        result.Should().BeFalse();
        state.SequenceToken.Should().Be(token);
    }

    [Fact]
    public void TryUpdateSequenceToken_returns_false_when_both_current_and_new_tokens_are_null()
    {
        StreamState state = new();

        var result = state.TryUpdateSequenceToken(null);

        result.Should().BeFalse();
        state.SequenceToken.Should().BeNull();
    }
}
