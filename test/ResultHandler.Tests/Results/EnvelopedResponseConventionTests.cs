using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Base;
using Xunit;

namespace ResultHandler.Tests.Results;

public class EnvelopedResponseConventionTests
{
    [Fact]
    public async Task DataEndpoint_DeclaresDataEnvelopeWith200()
    {
        await using var app = CreateApp();
        _ = app.MapGet("/things/{id:int}", (int id) => Microsoft.AspNetCore.Http.Results.Ok()).ProducesEnveloped<ThingResponse>();

        var metadata = Assert.Single(ResponseMetadata(app, "/things/{id:int}"));

        Assert.Equal(StatusCodes.Status200OK, metadata.StatusCode);
        Assert.Equal(typeof(OperationDataResult<ThingResponse>), metadata.Type);
        Assert.Equal(["application/json"], metadata.ContentTypes);
    }

    [Fact]
    public async Task CreateEndpoint_DeclaresTheGivenStatus()
    {
        await using var app = CreateApp();
        _ = app.MapPost("/things", () => Microsoft.AspNetCore.Http.Results.Ok()).ProducesEnveloped<Guid>(StatusCodes.Status201Created);

        var metadata = Assert.Single(ResponseMetadata(app, "/things"));

        Assert.Equal(StatusCodes.Status201Created, metadata.StatusCode);
        Assert.Equal(typeof(OperationDataResult<Guid>), metadata.Type);
    }

    [Fact]
    public async Task CommandEndpoint_DeclaresEnvelopeWithoutData()
    {
        await using var app = CreateApp();
        _ = app.MapPut("/things/{id:int}", (int id) => Microsoft.AspNetCore.Http.Results.Ok()).ProducesEnveloped();

        var metadata = Assert.Single(ResponseMetadata(app, "/things/{id:int}"));

        Assert.Equal(StatusCodes.Status200OK, metadata.StatusCode);
        Assert.Equal(typeof(OperationResult), metadata.Type);
    }

    [Fact]
    public async Task CombinesWithResultProblems()
    {
        await using var app = CreateApp();
        _ = app.MapGroup("/api").ProducesResultProblems().MapGet("/things", () => Microsoft.AspNetCore.Http.Results.Ok()).ProducesEnveloped<ThingResponse>();

        Assert.Equal([200, 500], [.. ResponseMetadata(app, "/api/things").Select(m => m.StatusCode).Order()]);
    }

    private static WebApplication CreateApp() => WebApplication.CreateSlimBuilder().Build();

    private static IEnumerable<IProducesResponseTypeMetadata> ResponseMetadata(IEndpointRouteBuilder app, string route)
        => app.DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Single(endpoint => endpoint.RoutePattern.RawText == route)
            .Metadata
            .OfType<IProducesResponseTypeMetadata>();

    public sealed record ThingResponse(int Id, string Name);
}
