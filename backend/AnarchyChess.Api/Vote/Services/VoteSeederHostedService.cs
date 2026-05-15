namespace AnarchyChess.Api.Vote.Services;

public class VoteSeederHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task StartAsync(CancellationToken token)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IVoteSeeder>();
        await seeder.SeedAsync(token);
    }

    public Task StopAsync(CancellationToken token) => Task.CompletedTask;
}
