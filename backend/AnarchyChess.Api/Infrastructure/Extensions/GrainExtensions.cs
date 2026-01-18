using Orleans.Streams;

namespace AnarchyChess.Api.Infrastructure.Extensions;

public static class GrainExtensions
{
    public static TGrainInterface AsSafeReference<TGrainInterface>(this IAddressable grain)
    {
        try
        {
            return grain.AsReference<TGrainInterface>();
        }
        catch (ArgumentException ex)
            when (ex.Message.Contains("Passing a half baked grain as an argument"))
        {
            return (TGrainInterface)grain;
        }
    }

    public static async Task SubscribeOrResumeAsync<T>(
        this IAsyncStream<T> stream,
        Func<T, StreamSequenceToken, Task> callback,
        StreamSequenceToken? sequenceToken = null
    )
    {
        var existingHandles = await stream.GetAllSubscriptionHandles();
        if (existingHandles.Count == 0)
        {
            await stream.SubscribeAsync(callback, sequenceToken);
            return;
        }

        await existingHandles[0].ResumeAsync(callback, sequenceToken);
        for (int i = 1; i < existingHandles.Count; i++)
        {
            await existingHandles[i].UnsubscribeAsync();
        }
    }
}
