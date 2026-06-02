using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Virentum.Api.Exceptions;

namespace Virentum.Api.Middleware;

/// <summary>
/// Centralised exception handling. Implemented as an <see cref="IExceptionHandler"/>
/// so it plugs into the framework's exception-handling middleware and guarantees
/// that no raw exception ever reaches the client.
///
/// - Expected <see cref="DomainException"/>s map to their declared status/title.
/// - Everything else becomes an opaque 500 — the message and stack trace are
///   logged with structured context but never serialised to the response.
/// All responses follow RFC 7807 (Problem Details), carrying the trace id so a
/// support engineer can correlate a client report with the server logs.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        IHostEnvironment environment,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _environment = environment;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;

        var (statusCode, title, detail) = exception switch
        {
            DomainException domain => (domain.StatusCode, domain.Title, domain.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", FallbackDetail()),
        };

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path} (traceId {TraceId})",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Handled domain exception {ExceptionType} for {Method} {Path} (traceId {TraceId})",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
        };
        problemDetails.Extensions["traceId"] = traceId;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }

    private string FallbackDetail() =>
        _environment.IsDevelopment()
            ? "See server logs for the full exception detail (development environment)."
            : "The request could not be processed. Please try again later.";
}
