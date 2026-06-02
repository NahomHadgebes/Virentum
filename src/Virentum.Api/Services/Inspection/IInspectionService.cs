using Virentum.Api.Contracts.Requests;
using Virentum.Api.Contracts.Responses;

namespace Virentum.Api.Services.Inspection;

/// <summary>
/// Business entry point for the inspection workflow. Controllers depend only on
/// this interface and stay free of orchestration logic.
/// </summary>
public interface IInspectionService
{
    /// <summary>
    /// Runs the full pipeline — validate, analyse, assess, persist — and returns
    /// the client-facing result DTO.
    /// </summary>
    /// <param name="request">The validated edge request.</param>
    /// <param name="storeId">The authenticated store/operator id.</param>
    Task<InspectionResponse> ScanAsync(
        ScanRequest request,
        string storeId,
        CancellationToken cancellationToken = default);
}
