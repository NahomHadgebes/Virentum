using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Virentum.Api.Domain.Enums;
using Virentum.Api.Exceptions;
using Virentum.Api.Middleware;
using Xunit;

namespace Virentum.Api.Tests.Middleware;

/// <summary>
/// This is the RFC 7807 contract the SPA depends on. The frontend's
/// problemDetails tests assert the same shapes from the receiving end, so the
/// two suites meet in the middle: title, detail, status and a traceId that is
/// always present.
/// </summary>
public sealed class GlobalExceptionHandlerTests
{
    private const string TraceId = "00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01";

    // Environments.Production and .Development are static readonly, not const,
    // so they cannot be default parameter values. The names are the contract.
    private const string Production = "Production";
    private const string Development = "Development";

    private readonly CapturingProblemDetailsService _problemDetails = new();

    private static DefaultHttpContext Context(string path = "/api/inspection/scan")
    {
        var context = new DefaultHttpContext { TraceIdentifier = TraceId };
        context.Request.Method = "POST";
        context.Request.Path = path;
        return context;
    }

    private GlobalExceptionHandler CreateHandler(string environment = Production) =>
        new(
            _problemDetails,
            new StubHostEnvironment(environment),
            NullLogger<GlobalExceptionHandler>.Instance);

    private async Task<ProblemDetails> HandleAsync(
        Exception exception,
        HttpContext? context = null,
        string environment = Production)
    {
        var httpContext = context ?? Context();
        var handled = await CreateHandler(environment).TryHandleAsync(
            httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        return Assert.IsType<ProblemDetails>(_problemDetails.Written);
    }

    [Fact]
    public async Task Maps_an_authentication_failure_to_401()
    {
        var written = await HandleAsync(new AuthenticationFailedException());

        Assert.Equal(401, written.Status);
        Assert.Equal("Authentication failed", written.Title);
        Assert.Equal("Invalid store id or password.", written.Detail);
    }

    [Fact]
    public async Task Maps_an_invalid_inspection_request_to_400()
    {
        var written = await HandleAsync(
            new InvalidInspectionRequestException("An image file is required."));

        Assert.Equal(400, written.Status);
        Assert.Equal("Invalid inspection request", written.Title);
        Assert.Equal("An image file is required.", written.Detail);
    }

    [Fact]
    public async Task Maps_an_unsupported_fruit_to_422()
    {
        var written = await HandleAsync(new UnsupportedFruitException(SupportedFruit.Avocado));

        Assert.Equal(422, written.Status);
        Assert.Equal("Unsupported fruit", written.Title);
    }

    [Fact]
    public async Task Maps_a_vision_failure_to_502()
    {
        var written = await HandleAsync(new VisionAnalysisException("Provider unreachable."));

        Assert.Equal(502, written.Status);
        Assert.Equal("Vision analysis unavailable", written.Title);
    }

    [Fact]
    public async Task Sets_the_status_code_on_the_response_itself()
    {
        var context = Context();

        await HandleAsync(new UnsupportedFruitException(SupportedFruit.Banana), context);

        Assert.Equal(422, context.Response.StatusCode);
    }

    /// <summary>
    /// The client shows this id to the operator so a support engineer can find
    /// the matching log line. It must never be missing.
    /// </summary>
    [Fact]
    public async Task Carries_the_trace_id_for_a_domain_failure()
    {
        var written = await HandleAsync(new AuthenticationFailedException());

        Assert.True(written.Extensions.TryGetValue("traceId", out var traceId));
        Assert.Equal(TraceId, traceId);
    }

    [Fact]
    public async Task Carries_the_trace_id_for_an_unexpected_failure_too()
    {
        var written = await HandleAsync(new InvalidOperationException("connection string is null"));

        Assert.True(written.Extensions.TryGetValue("traceId", out var traceId));
        Assert.Equal(TraceId, traceId);
    }

    [Fact]
    public async Task Records_the_request_path_as_the_problem_instance()
    {
        var written = await HandleAsync(
            new AuthenticationFailedException(), Context("/api/auth/login"));

        Assert.Equal("/api/auth/login", written.Instance);
    }

    /// <summary>
    /// An unexpected exception must become opaque. Leaking the message would
    /// expose internals such as connection strings.
    /// </summary>
    [Fact]
    public async Task Never_leaks_the_message_of_an_unexpected_exception()
    {
        var written = await HandleAsync(new InvalidOperationException("connection string is null"));

        Assert.Equal(500, written.Status);
        Assert.Equal("An unexpected error occurred", written.Title);
        Assert.DoesNotContain("connection string", written.Detail ?? string.Empty, StringComparison.Ordinal);
        Assert.Equal("The request could not be processed. Please try again later.", written.Detail);
    }

    [Fact]
    public async Task Points_a_developer_at_the_logs_in_development()
    {
        var written = await HandleAsync(
            new InvalidOperationException("connection string is null"),
            environment: Development);

        Assert.Contains("server logs", written.Detail ?? string.Empty, StringComparison.Ordinal);
        Assert.DoesNotContain("connection string", written.Detail ?? string.Empty, StringComparison.Ordinal);
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetails? Written { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Written = context.ProblemDetails;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Written = context.ProblemDetails;
            return ValueTask.FromResult(true);
        }
    }

    private sealed class StubHostEnvironment : IHostEnvironment
    {
        public StubHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "Virentum.Api";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
