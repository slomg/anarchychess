namespace AnarchyChess.Api.TestInfrastructure.Utils;

public static class Wait
{
    public static Task UntilAsync(Func<bool> condition, int timeout = 1000, int interval = 10) =>
        UntilAsync(async () => condition(), timeout, interval);

    public static Task UntilAsync(Action condition, int timeout = 1000, int interval = 10) =>
        UntilAsync(async () => condition(), timeout, interval);

    public static async Task UntilAsync(
        Func<Task<bool>> condition,
        int timeout = 1000,
        int interval = 10
    )
    {
        var elapsed = 0;
        while (elapsed < timeout)
        {
            bool result = await condition();
            if (result)
            {
                break;
            }

            await Task.Delay(interval);
            elapsed += interval;
        }

        if (!await condition())
        {
            throw new TimeoutException("Condition was not met in time");
        }
    }

    public static async Task UntilAsync(Func<Task> condition, int timeout = 1000, int interval = 10)
    {
        var elapsed = 0;
        while (elapsed < timeout)
        {
            try
            {
                await condition();
                break;
            }
            catch { }

            await Task.Delay(interval);
            elapsed += interval;
        }

        try
        {
            await condition();
        }
        catch (Exception ex)
        {
            throw new TimeoutException("Condition was not met in time", ex);
        }
    }
}
