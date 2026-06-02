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
        var storeId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
            ?? "unknown";

        var result = await _inspectionService.ScanAsync(request, storeId, cancellationToken);
        return Ok(result);
    }
}
