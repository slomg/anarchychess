using AnarchyChess.Api.Auth.Services;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.ErrorHandling.Infrastructure;
using AnarchyChess.Api.Infrastructure;
using AnarchyChess.Api.Vote.DTOs;
using AnarchyChess.Api.Vote.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AnarchyChess.Api.Vote.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VoteController(IVoteService voteService, IAuthService authService) : ControllerBase
{
    private readonly IVoteService _voteService = voteService;
    private readonly IAuthService _authService = authService;

    [HttpGet("next")]
    [ProducesResponseType<PendingUserVoteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status429TooManyRequests)]
    [Authorize(AuthPolicies.ActiveSession)]
    public async Task<ActionResult<PendingUserVoteDto>> GetNextVotePair(CancellationToken token)
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
        {
            return userIdResult.Errors.ToActionResult();
        }

        var result = await _voteService.SelectNextPairAsync(
            userIdResult.Value,
            ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            token
        );
        return result.Match(Ok, errors => errors.ToActionResult());
    }

    [HttpPost("complete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    [Authorize(AuthPolicies.ActiveSession)]
    [EnableRateLimiting(VoteConstants.VoteRateLimiter)]
    public async Task<ActionResult> CompleteVote(
        [FromQuery] string optionKey,
        CancellationToken token
    )
    {
        var userIdResult = _authService.GetUserId(User);
        if (userIdResult.IsError)
        {
            return userIdResult.Errors.ToActionResult();
        }

        var result = await _voteService.CompleteVoteAsync(
            userIdResult.Value,
            ip: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "",
            voteOptionKey: optionKey,
            token
        );
        return result.Match(value => NoContent(), errors => errors.ToActionResult());
    }
}
