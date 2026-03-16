namespace AnarchyChess.Api.Shared.Services;

public interface IDelayProvider
{
    Task DelayAsync(int millisecondsDelay);
}

public class DelayProvider : IDelayProvider
{
    public Task DelayAsync(int millisecondsDelay) => Task.Delay(millisecondsDelay);
}
