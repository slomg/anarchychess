using AnarchyChess.Api.Shared.Services;

namespace AnarchyChess.Api.Bots.Services;

public interface IBotDecisionServiceFactory
{
    IBotDecisionService Create(BotBehaviorProfile behaviorProfile);
}

public class BotDecisionServiceFactory(
    IBotService botService,
    IRandomProvider randomProvider,
    IBotHeuristics botHeuristics,
    ILogger<BotDecisionService> logger
) : IBotDecisionServiceFactory
{
    private readonly IBotService _botService = botService;
    private readonly IRandomProvider _randomProvider = randomProvider;
    private readonly IBotHeuristics _botHeuristics = botHeuristics;
    private readonly ILogger<BotDecisionService> _logger = logger;

    public IBotDecisionService Create(BotBehaviorProfile behaviorProfile) =>
        new BotDecisionService(
            _botService,
            _randomProvider,
            _botHeuristics,
            _logger,
            behaviorProfile
        );
}
