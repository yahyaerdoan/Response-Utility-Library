using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Base;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Implementations.Error;
using Xunit;

namespace ResultHandler.Tests.Results;

public class ProblemDetailsWritingTests
{
    [Fact]
    public async Task ToProblemResult_WritesProblemJsonWithInstanceAndTraceId_WithoutProblemDetailsService()
    {
        var httpContext = CreateHttpContext(services => { });

        await Result.NotFound("Missing.").ToProblemResult().ExecuteAsync(httpContext);

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal(404, httpContext.Response.StatusCode);
        Assert.StartsWith("application/problem+json", httpContext.Response.ContentType, StringComparison.Ordinal);
        Assert.Equal("/api/things", body.RootElement.GetProperty("instance").GetString());
        Assert.Equal("trace-1", body.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task ToProblemResult_GoesThroughCustomizeProblemDetails()
    {
        var httpContext = CreateHttpContext(services => services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context => context.ProblemDetails.Extensions["tenant"] = "acme"));

        await Result.Conflict("Slug taken.").WithCode("Tenant.SlugTaken").ToProblemResult().ExecuteAsync(httpContext);

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal(409, httpContext.Response.StatusCode);
        Assert.Equal("acme", body.RootElement.GetProperty("tenant").GetString());
        Assert.Equal("Tenant.SlugTaken", body.RootElement.GetProperty("code").GetString());
        Assert.Equal("trace-1", body.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task ToProblemResult_ValidationFailure_WritesMicrosoftShapedErrors()
    {
        var httpContext = CreateHttpContext(services => services.AddProblemDetails());
        var result = new ErrorResult("Validation Failed", ResultStatus.UnprocessableContent, new Dictionary<string, IReadOnlyList<string>>
        {
            ["Title"] = ["Title is required."],
        });

        await result.ToProblemResult().ExecuteAsync(httpContext);

        using var body = await ReadBodyAsync(httpContext);
        var errors = body.RootElement.GetProperty("errors");
        Assert.Equal(JsonValueKind.Object, errors.ValueKind);
        Assert.Equal("Title is required.", errors.GetProperty("Title")[0].GetString());
        Assert.False(body.RootElement.TryGetProperty("fieldErrors", out _));
    }

    [Fact]
    public async Task ToActionResult_ValidationFailure_KeepsErrorsWhenMvcAndProblemDetailsAreRegistered()
    {
        var httpContext = CreateHttpContext(services => services.AddControllers().Services.AddResultProblemDetails());
        httpContext.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(new ApiControllerAttribute()), "api-controller"));
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var result = new ErrorResult("Validation Failed", ResultStatus.UnprocessableContent, new Dictionary<string, IReadOnlyList<string>>
        {
            ["Title"] = ["Title is required."],
        });

        await result.ToActionResult().ExecuteResultAsync(actionContext);

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal(422, httpContext.Response.StatusCode);
        Assert.Equal("Title is required.", body.RootElement.GetProperty("errors").GetProperty("Title")[0].GetString());
        Assert.Equal("trace-1", body.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task ToActionResult_GoesThroughCustomizeProblemDetails()
    {
        var httpContext = CreateHttpContext(services => services.AddControllers().Services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context => context.ProblemDetails.Extensions["tenant"] = "acme"));
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        await Result.Forbidden().ToActionResult().ExecuteResultAsync(actionContext);

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal(403, httpContext.Response.StatusCode);
        Assert.Equal("acme", body.RootElement.GetProperty("tenant").GetString());
        Assert.Equal("/api/things", body.RootElement.GetProperty("instance").GetString());
        Assert.Equal("trace-1", body.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task AddResultProblemDetails_FillsTypeTitleInstanceAndTraceId_ForFrameworkProblems()
    {
        var httpContext = CreateHttpContext(services => services.AddResultProblemDetails());
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var service = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        await service.WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = { Status = 429 } });

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal("https://tools.ietf.org/html/rfc6585#section-4", body.RootElement.GetProperty("type").GetString());
        Assert.Equal("Too Many Requests", body.RootElement.GetProperty("title").GetString());
        Assert.Equal("/api/things", body.RootElement.GetProperty("instance").GetString());
        Assert.Equal("trace-1", body.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task AddResultProblemDetails_RunsTheCallersCustomizationLast()
    {
        var httpContext = CreateHttpContext(services => services.AddResultProblemDetails(options =>
            options.CustomizeProblemDetails = context => context.ProblemDetails.Extensions["traceId"] = "replaced"));
        var service = httpContext.RequestServices.GetRequiredService<IProblemDetailsService>();

        await service.WriteAsync(new ProblemDetailsContext { HttpContext = httpContext, ProblemDetails = { Status = 404 } });

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal("replaced", body.RootElement.GetProperty("traceId").GetString());
    }

    [Fact]
    public async Task ToEnvelopedResult_WritesOnlyTheEnvelopeMembers()
    {
        var httpContext = CreateHttpContext(services => { });

        await Result.Created<string>("R-1").WithCode("Receipt.Created").ToEnvelopedResult().ExecuteAsync(httpContext);

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal(["resultData", "isSuccessful", "statusCode", "statusMessage", "code"], body.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Number, body.RootElement.GetProperty("statusCode").ValueKind);
        Assert.Equal(201, body.RootElement.GetProperty("statusCode").GetInt32());
    }

    [Fact]
    public async Task ToEnvelopedResult_WithoutData_HasNoResultData()
    {
        var httpContext = CreateHttpContext(services => { });

        await Result.Success().ToEnvelopedResult().ExecuteAsync(httpContext);

        using var body = await ReadBodyAsync(httpContext);
        Assert.Equal(["isSuccessful", "statusCode", "statusMessage"], body.RootElement.EnumerateObject().Select(property => property.Name));
    }

    private static DefaultHttpContext CreateHttpContext(Action<IServiceCollection> configure)
    {
        var services = new ServiceCollection().AddLogging();
        configure(services);

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
            TraceIdentifier = "trace-1",
        };
        httpContext.Request.Path = "/api/things";
        httpContext.Response.Body = new MemoryStream();
        return httpContext;
    }

    private static async Task<JsonDocument> ReadBodyAsync(HttpContext httpContext)
    {
        httpContext.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(httpContext.Response.Body, cancellationToken: httpContext.RequestAborted);
    }
}
