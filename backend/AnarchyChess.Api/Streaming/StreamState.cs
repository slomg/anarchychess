using AnarchyChess.Api.Streaming.Extensions;
using Orleans.Providers.Streams.Common;
using Orleans.Streams;

namespace AnarchyChess.Api.Streaming;

[GenerateSerializer]
[Alias("AnarchyChess.Api.Streaming.StreamState")]
public class StreamState
{
    [Id(0)]
    public EventSequenceToken? SequenceToken { get; private set; }

    public bool TryUpdateSequenceToken(StreamSequenceToken? sequenceToken)
    {
        var eventHubToken = (EventSequenceToken?)sequenceToken;
        if (!eventHubToken.IsNewerThan(SequenceToken))
            return false;

        SequenceToken = eventHubToken;
        return true;
    }
}
