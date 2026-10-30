using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.AspNetCore.OpenApi;
using Xunit;

namespace ResultHandler.Tests.Results;

public class ResultProblemsConventionTests
{
    [Fact]
    public async Task AnonymousGet_DocumentsOnly500()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapGet("/things", () => "ok");

        Assert.Equal([500], ProblemStatuses(app, "/api/things"));
    }

    [Fact]
    public async Task AuthorizedEndpoint_Documents401And403()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapGet("/secure", () => "ok").RequireAuthorization();

        Assert.Equal([401, 403, 500], ProblemStatuses(app, "/api/secure"));
    }

    [Fact]
    public async Task AllowAnonymousInsideAuthorizedGroup_SkipsAuthStatuses()
    {
        await using var app = CreateApp();
        var group = app.MapGroup("/api").RequireAuthorization().ProducesResultProblems();
        _ = group.MapGet("/public", () => "ok").AllowAnonymous();

        Assert.Equal([500], ProblemStatuses(app, "/api/public"));
    }

    [Fact]
    public async Task EndpointReadingBody_Documents400And422()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapPost("/things", (CreateThingRequest request) => request.Name);

        Assert.Equal([400, 422, 500], ProblemStatuses(app, "/api/things"));
    }

    [Fact]
    public async Task ValidationStatuses_UseValidationSchema()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapPost("/things", (CreateThingRequest request) => request.Name);

        var metadata = ResponseMetadata(app, "/api/things").ToList();

        Assert.Equal(typeof(ResultValidationProblemDetails), Assert.Single(metadata, m => m.StatusCode == 400).Type);
        Assert.Equal(typeof(ResultValidationProblemDetails), Assert.Single(metadata, m => m.StatusCode == 422).Type);
        Assert.Equal(typeof(ResultProblemDetails), Assert.Single(metadata, m => m.StatusCode == 500).Type);
    }

    [Fact]
    public async Task ProducesResultProblem_DocumentsEndpointSpecificStatuses()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapGet("/things/{id}", (int id) => id)
            .ProducesResultProblem(StatusCodes.Status404NotFound, StatusCodes.Status409Conflict);

        Assert.Equal([404, 409, 500], ProblemStatuses(app, "/api/things/{id}"));
    }

    [Fact]
    public async Task ProducesResultProblem_LeavesDeclaredStatusAlone()
    {
        await using var app = CreateApp();
        _ = app.MapGet("/things/{id}", (int id) => id)
            .ProducesProblem(StatusCodes.Status404NotFound, "application/custom+json")
            .ProducesResultProblem(StatusCodes.Status404NotFound);

        var only = Assert.Single(ResponseMetadata(app, "/things/{id}"), m => m.StatusCode == 404);
        Assert.Contains("application/custom+json", only.ContentTypes);
    }

    [Fact]
    public async Task StatusAlreadyDeclared_IsNotDuplicated()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapGet("/declared", () => "ok")
            .ProducesProblem(StatusCodes.Status500InternalServerError, "application/custom+json");

        var declared500 = ResponseMetadata(app, "/api/declared").Where(m => m.StatusCode == 500).ToList();

        var only = Assert.Single(declared500);
        Assert.Contains("application/custom+json", only.ContentTypes);
    }

    [Fact]
    public async Task ProblemMetadata_UsesResultProblemDetailsAndProblemJson()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapGet("/things", () => "ok");

        var metadata = Assert.Single(ResponseMetadata(app, "/api/things"), m => m.StatusCode == 500);

        Assert.Equal(typeof(ResultProblemDetails), metadata.Type);
        Assert.Equal(["application/problem+json"], metadata.ContentTypes);
    }

    private static WebApplication CreateApp()
    {
        var builder = WebApplication.CreateSlimBuilder();
        _ = builder.Services.AddAuthorization();
        return builder.Build();
    }

    private static IEnumerable<IProducesResponseTypeMetadata> ResponseMetadata(IEndpointRouteBuilder app, string route)
        => app.DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.RoutePattern.RawText == route)
            .Metadata
            .OfType<IProducesResponseTypeMetadata>();

    private static int[] ProblemStatuses(IEndpointRouteBuilder app, string route)
        => [.. ResponseMetadata(app, route)
            .Where(m => m.Type is not null && m.Type.IsAssignableTo(typeof(ProblemDetails)))
            .Select(m => m.StatusCode)
            .Order()];

    public sealed record CreateThingRequest(string Name);
}
