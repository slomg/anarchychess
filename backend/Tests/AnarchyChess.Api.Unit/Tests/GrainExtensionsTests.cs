using AnarchyChess.Api.Infrastructure.Extensions;
using NSubstitute;
using Orleans.Providers.Streams.Common;
using Orleans.Streams;

namespace AnarchyChess.Api.Unit.Tests;

public class GrainExtensionsTests
{
    private readonly Func<int, StreamSequenceToken, Task> _callback = (_, _) => Task.CompletedTask;
    private readonly EventSequenceToken _sequenceToken = new(1, 2);

    [Fact]
    public async Task SubscribeOrResumeAsync_subscribes_when_there_are_no_existing_handles()
    {
        var streamMock = Substitute.For<IAsyncStream<int>>();
        streamMock.GetAllSubscriptionHandles().Returns([]);

        await streamMock.SubscribeOrResumeAsync(_callback, _sequenceToken);

        await streamMock.Received(1).SubscribeAsync(Arg.Any<IAsyncObserver<int>>(), _sequenceToken);
    }

    [Fact]
    public async Task SubscribeOrResumeAsync_resumes_existing_handles()
    {
        var handleMock1 = Substitute.For<StreamSubscriptionHandle<int>>();
        var handleMock2 = Substitute.For<StreamSubscriptionHandle<int>>();
        var handleMock3 = Substitute.For<StreamSubscriptionHandle<int>>();

        var streamMock = Substitute.For<IAsyncStream<int>>();
        streamMock.GetAllSubscriptionHandles().Returns([handleMock1, handleMock2, handleMock3]);

        await streamMock.SubscribeOrResumeAsync(_callback, _sequenceToken);

        await handleMock1.Received(1).ResumeAsync(Arg.Any<IAsyncObserver<int>>(), _sequenceToken);
        await handleMock2.Received(1).UnsubscribeAsync();
        await handleMock3.Received(1).UnsubscribeAsync();
        await streamMock.DidNotReceiveWithAnyArgs().SubscribeAsync(default!, default);
    }
}
