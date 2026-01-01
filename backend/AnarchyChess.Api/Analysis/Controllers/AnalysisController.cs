using AnarchyChess.Api.Analysis.Models;
using AnarchyChess.Api.Analysis.Services;
using AnarchyChess.Api.ErrorHandling.Extensions;
using AnarchyChess.Api.ErrorHandling.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace AnarchyChess.Api.Analysis.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalysisController(IPositionAnalysis positionAnalysis) : Controller
{
    private readonly IPositionAnalysis _positionAnalysis = positionAnalysis;

    [HttpGet("initial")]
    [ProducesResponseType<RootAnalysisPosition>(StatusCodes.Status200OK)]
    public ActionResult<RootAnalysisPosition> GetInitialAnalysisPosition()
    {
        var position = _positionAnalysis.GetInitialPosition();
        return Ok(position);
    }

    [HttpPost("next")]
    [ProducesResponseType<AnalysisPosition>(StatusCodes.Status200OK)]
    [ProducesResponseType<ApiProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<AnalysisPosition> GetNextAnalysisPosition(AnalysisMove move)
    {
        var result = _positionAnalysis.GetNextLegalMoves(move);
        return result.Match(Ok, errors => errors.ToActionResult());
    }
}
