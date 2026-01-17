using Orleans.Streams;

namespace AnarchyChess.Api.Streaming.Extensions;

public static class StreamSequenceTokenExtensions
{
    public static bool IsNewerThan(this StreamSequenceToken? me, StreamSequenceToken? other)
    {
        if (me is not null && other is null)
            return true;

        if (me is not null)
            return me.CompareTo(other) > 0;

        return false;
    }
}
