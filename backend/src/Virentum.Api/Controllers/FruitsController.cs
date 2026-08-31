using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Virentum.Api.Contracts.Responses;
using Virentum.Api.Services.Catalog;

namespace Virentum.Api.Controllers;

/// <summary>Reference data about the produce the platform can inspect.</summary>
[ApiController]
[Route("api/fruits")]
[Authorize]
public sealed class FruitsController : ControllerBase
{
    private readonly IFruitCatalogService _catalog;

    public FruitsController(IFruitCatalogService catalog)
    {
        _catalog = catalog;
    }

    /// <summary>
    /// Every supported fruit with the ripeness bands it is judged against.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FruitProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public ActionResult<IReadOnlyList<FruitProfileResponse>> GetAll() => Ok(_catalog.GetProfiles());
}
