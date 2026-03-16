using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.Bots.Grains;
using AnarchyChess.Api.Bots.Models;
using AnarchyChess.Api.Bots.Services;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.ErrorHandling.Infrastructure;
using AnarchyChess.Api.Game.Models;
using AnarchyChess.Api.GameSnapshot.Models;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Profile.Entities;
using AnarchyChess.Api.Shared.Services;
using AnarchyChess.EngineShared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Bots.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class BotController(
    IGrainFactory grains,
    IBotService botService,
    UserManager<AuthedUser> userManager,
    IAuthService authService,
    IRandomCodeGenerator randomCodeGenerator
) : Controller
{
    private readonly IGrainFactory _grains = grains;
    private readonly IBotService _botService = botService;
    private readonly UserManager<AuthedUser> _userManager = userManager;
    private readonly IAuthService _authService = authService;
    private readonly IRandomCodeGenerator _randomCodeGenerator = randomCodeGenerator;

    [HttpGet("{gameToken}")]
    [ProducesResponseType<BotGameState>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameState>> GetBotGame(string gameToken)
    {
        var botGrain = _grains.GetGrain<IBotGrain>(gameToken);
        var result = await botGrain.GetStateAsync();
        return result.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpPost("start")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [Authorize(AuthPolicies.ActiveSession)]
    public async Task<ActionResult<string>> StartBotGame(
        GameColor myColor,
        BotType botType,
        CancellationToken token
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
        {
            return userIdResult.Errors.ToActionResult();
        }
        var userId = userIdResult.Value;
        var user = await _userManager.FindByIdAsync(userId);
        GamePlayer player = new(
            userIdResult.Value,
            myColor,
            UserName: user?.UserName ?? "Guest",
            CountryCode: user?.CountryCode ?? "XX",
            Rating: null
        );

        GameToken gameToken = _randomCodeGenerator.Generate(16);
        var botGrain = _grains.GetGrain<IBotGrain>(gameToken);
        await botGrain.StartGameAsync(player, botType, token);

        return Ok(gameToken.ToString());
    }

    [HttpGet("health")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CheckBotHealth(CancellationToken token)
    {
        bool result = await _botService.CheckHealthAsync(token);
        return Ok(result);
    }
}
