using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Virentum.Api.Contracts.Requests;
using Virentum.Api.Contracts.Responses;
using Virentum.Api.Services.Inspection;

namespace Virentum.Api.Controllers;

/// <summary>
/// Inspection endpoints. The controller is intentionally thin: it binds and
/// validates the request, identifies the caller, delegates to the business
/// service, and returns the DTO. It contains no fruit logic and no branching on
/// fruit type — that lives behind the processor factory.
/// </summary>
[ApiController]
[Route("api/inspection")]
[Authorize]
public sealed class InspectionController : ControllerBase
{
    private readonly IInspectionService _inspectionService;

    public InspectionController(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    /// <summary>
    /// Analyses an uploaded produce image and returns its commercial assessment.
    /// </summary>
    [HttpPost("scan")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(InspectionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<InspectionResponse>> Scan(
        [FromForm] ScanRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _inspectionService.ScanAsync(request, CurrentStoreId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// The store's most recent inspections, newest first.
    /// </summary>
    /// <param name="limit">How many rows to return. Between 1 and 100.</param>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<InspectionHistoryItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<InspectionHistoryItem>>> GetHistory(
        [FromQuery] [Range(1, 100)] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _inspectionService.GetHistoryAsync(CurrentStoreId, limit, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// The store's inspection activity over a rolling window.
    /// </summary>
    /// <param name="days">Length of the window in days. Between 1 and 90.</param>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(InspectionSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<InspectionSummaryResponse>> GetSummary(
        [FromQuery] [Range(1, 90)] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var result = await _inspectionService.GetSummaryAsync(CurrentStoreId, days, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// The authenticated store, read from the token. JwtBearer is configured
    /// with MapInboundClaims disabled, so the original "sub" claim survives; the
    /// NameIdentifier lookup covers a mapped token.
    /// </summary>
    private string CurrentStoreId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
        ?? "unknown";
}
